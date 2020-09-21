using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliaaCommon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TciEnergy.Blazor.Server.Models;
using TciEnergy.Blazor.Shared.Models;
using TciEnergy.Blazor.Shared.ViewModels;
using TciEnergy.Blazor.Server.Utils;
using System.Security.Claims;
using TciCommon.Models;
using System.Text;
using TciEnergy.Blazor.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Omu.ValueInjecter;
using Microsoft.AspNetCore.Authorization;
using EasyMongoNet.Driver2;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IMongoCollection<AuthUserX> userCol;
        private readonly IMongoCollection<LoginLog> loginLogCol;

        public AccountController(IMongoCollection<AuthUserX> userCol, IMongoCollection<LoginLog> loginLogCol)
        {
            this.userCol = userCol;
            this.loginLogCol = loginLogCol;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BaseAuthUser>> Login(LoginVM model)
        {
            if (model == null)
                return Unauthorized();
            var user = await userCol.CheckAuthentication(loginLogCol, model.Username, model.Password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, model.Username),
                    new Claim(ClaimTypes.Name, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim(nameof(Province), model.Province)
                };
                if (user.IsAdmin)
                    claims.Add(new Claim("IsAdmin", "true"));

                var perms = new StringBuilder();
                foreach (var perm in user.Permissions)
                    perms.Append(perm).Append(",");
                claims.Add(new Claim(nameof(Permission), perms.ToString()));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                var clientUser = Mapper.Map<ClientAuthUser>(user);
                clientUser.ProvincePrefix = model.Province;
                return clientUser;
            }
            return Unauthorized();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new AuthenticationProperties { IsPersistent = false });
            return Ok();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            var user = await userCol.FindFirstAsync(u => u.Id == UserId.Value);
            if (user != null)
            {
                if (AuthUserDBExtention.GetHash(model.CurrentPassword) == user.HashedPassword)
                {
                    if (model.NewPassword == model.RepeatNewPassword)
                    {
                        user.Password = model.NewPassword;
                        await userCol.InsertOneAsync(user); // TODO save loggs
                        return Ok();
                    }
                    else
                        return BadRequest("رمز جدید و تکرار آن باهم برابر نیستند.");
                }
                else
                    return BadRequest("رمز فعلی اشتباه میباشد.");
            }
            return Unauthorized();
        }

        [Authorize(nameof(Permission.ManageUsers))]
        [HttpPost]
        public async Task<IActionResult> Add(NewUserVM user)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (await userCol.AnyAsync(u => u.Username == user.Username))
                return BadRequest(new Dictionary<string, List<string>> { { nameof(NewUserVM.Username), new List<string> { "نام کاربری قبلا موجود است!" } } });
            var authUser = Mapper.Map<AuthUserX>(user);
            await userCol.InsertOneAsync(authUser);
            return Ok();
        }

        [Authorize(nameof(Permission.ManageUsers))]
        public async Task<ActionResult<List<ClientAuthUser>>> List()
        {
            return (await userCol.Find(_ => true).SortBy(u => u.LastName).ThenBy(u => u.FirstName)
                .Project(Builders<AuthUserX>.Projection.Exclude(u => u.HashedPassword)).As<AuthUserX>()
                .ToCursorAsync()).ToEnumerable().Select(u => Mapper.Map<ClientAuthUser>(u)).ToList();
        }

        [Authorize(nameof(Permission.ManageUsers))]
        [HttpPost]
        public async Task<IActionResult> Save(ClientAuthUser user)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات کاربری نامعتبر است!");
            var existing = await userCol.FindByIdAsync(user.Id);
            if (existing == null)
                return BadRequest("کاربر یافت نشد!");
            existing.InjectFrom(user);
            await userCol.InsertOneAsync(existing); //TODO: write log
            return Ok();
        }
    }
}
