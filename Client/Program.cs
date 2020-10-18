using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using TciEnergy.Blazor.Shared;
using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Blazored.Toast;
using Microsoft.JSInterop;

namespace TciEnergy.Blazor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddBlazoredToast();
            builder.Services.AddBlazoredModal();

            var address = new Uri(new Uri(builder.HostEnvironment.BaseAddress), "/api/");
            builder.Services.AddTransient(sp => new HttpClientX(sp.GetService<NavigationManager>(), sp.GetService<IJSRuntime>())
            { BaseAddress = address });
            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore(options =>
            {
                foreach (string perm in Enum.GetNames(typeof(Permission)))
                    options.AddPolicy(perm, policy => policy.RequireAssertion(context =>
                    {
                        var permClaim = context.User.FindFirst(nameof(Permission));
                        return permClaim != null && permClaim.Value.Contains(perm);
                    }));
                options.AddPolicy("Admin", policy => policy.RequireClaim("IsAdmin", "true"));
            });

            await builder.Build().RunAsync();
        }
    }
}
