using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TciCommon.Models;
using EasyMongoNet.Driver2;
using MongoDB.Driver;
using TciEnergy.Blazor.Shared.Models;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IMongoCollection<City> citiesCol;
        private readonly IMongoCollection<Settings> settingsCol;

        public DashboardController(IMongoCollection<City> citiesCol, IMongoCollection<Settings> settingsCol)
        {
            this.citiesCol = citiesCol;
            this.settingsCol = settingsCol;
        }

        [HttpGet]
        public async Task<ActionResult<City>> MainCity()
        {
            var mainCitySettings = await settingsCol.FindFirstAsync(s => s.Key == "MainCity");
            return await citiesCol.FindByIdAsync(mainCitySettings.Value);
        }

        [HttpGet]
        public async Task<string[,]> TotalElecBillInfoTable()
        {

        }
    }
}
