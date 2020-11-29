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
using System.Text;
using TciEnergy.Blazor.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Omu.ValueInjecter;
using Microsoft.AspNetCore.Authorization;
using TciCommon.Server;
using System;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : BaseController
    {
        public AccountController(ProvinceDBs dbs) : base(dbs) { }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BaseAuthUser>> Login(LoginVM model)
        {
            if (model == null)
                return Unauthorized();
            var user = dbs[model.Province].CheckAuthentication(model.Username, model.Password);
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

                IEnumerable<Permission> permissions;
                if (user.IsAdmin)
                    permissions = Enum.GetValues<Permission>();
                else
                    permissions = user.Permissions;
                var perms = new StringBuilder();
                foreach (var perm in permissions)
                    perms.Append(perm).Append(',');
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
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            var user = db.FindFirst<AuthUserX>(u => u.Id == UserId);
            if (user != null)
            {
                if (AuthUserDBExtention.GetHash(model.CurrentPassword) == user.HashedPassword)
                {
                    if (model.NewPassword == model.RepeatNewPassword)
                    {
                        user.Password = model.NewPassword;
                        db.Save(user);
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
        public IActionResult Add(NewUserVM user)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (db.Any<AuthUserX>(u => u.Username == user.Username))
                return BadRequest(new Dictionary<string, List<string>> { { nameof(NewUserVM.Username), new List<string> { "نام کاربری قبلا موجود است!" } } });
            var authUser = Mapper.Map<AuthUserX>(user);
            db.Save(authUser);
            return Ok();
        }

        [Authorize(nameof(Permission.ManageUsers))]
        public ActionResult<List<ClientAuthUser>> List()
        {
            return db.Find<AuthUserX>(u => u.Disabled != true).SortBy(u => u.LastName).ThenBy(u => u.FirstName)
                .Project(Builders<AuthUserX>.Projection.Exclude(u => u.HashedPassword)).As<AuthUserX>()
                .ToEnumerable().Select(u => Mapper.Map<ClientAuthUser>(u)).ToList();
        }

        [Authorize(nameof(Permission.ManageUsers))]
        [HttpPost]
        public IActionResult Save(ClientAuthUser user)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات کاربری نامعتبر است!");
            var existing = db.FindById<AuthUserX>(user.Id);
            if (existing == null)
                return BadRequest("کاربر یافت نشد!");
            existing.InjectFrom(user);
            db.Save(existing);
            return Ok();
        }
    }
}
