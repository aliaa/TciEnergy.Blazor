﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TciCommon.Models;
using TciEnergy.Blazor.Shared.Models;
using TciCommon.ServerUtils;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DashboardController : BaseController
    {
        public DashboardController(ProvinceDBs dbs) : base(dbs) { }

        [HttpGet]
        public ActionResult<City> MainCity()
        {
            var mainCitySettings = db.FindFirst<Settings>(s => s.Key == "MainCity");
            return db.FindById<City>(mainCitySettings.Value);
        }

        
    }
}
