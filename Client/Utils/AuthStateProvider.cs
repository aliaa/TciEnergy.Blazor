using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TciEnergy.Blazor.Shared;
using TciEnergy.Blazor.Shared.Models;
using TciEnergy.Blazor.Shared.ViewModels;

namespace TciEnergy.Blazor.Client
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private HttpClientX httpClient;
        private ILocalStorageService storage;

        public AuthStateProvider(HttpClientX httpClient, ILocalStorageService storage) : base()
        {
            this.httpClient = httpClient;
            this.storage = storage;
        }

        public async Task<BaseAuthUser> GetUser()
        {
            return await storage.GetItemAsync<BaseAuthUser>("user");
        }

        private async Task<ClaimsPrincipal> GetClaims()
        {
            ClaimsIdentity identity;
            var user = await GetUser();
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Username),
                    new Claim(ClaimTypes.Name, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName)
                };
                var perms = new StringBuilder();
                if(user.IsAdmin)
                {
                    foreach (string p in Enum.GetNames(typeof(Permission)))
                        perms.Append(p).Append(",");
                    claims.Add(new Claim("IsAdmin", "true"));
                }
                else
                {
                    foreach (Permission p in user.Permissions)
                        perms.Append(p).Append(",");
                }
                claims.Add(new Claim(nameof(Permission), perms.ToString()));
                identity = new ClaimsIdentity(claims, "Cookies");
            }
            else
                identity = new ClaimsIdentity();

            return new ClaimsPrincipal(identity);
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return new AuthenticationState(await GetClaims());
        }

        public async Task<bool> Login(LoginVM m)
        {
            try
            {
                var user = await httpClient.PostAsJsonAsync<LoginVM, BaseAuthUser>("Account/Login", m);
                if (user == null)
                    return false;
                await storage.SetItemAsync("user", user);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task Logout()
        {
            await httpClient.GetAsync("Account/Logout");
            await storage.RemoveItemAsync("user");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
