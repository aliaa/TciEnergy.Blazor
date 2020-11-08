using System.Collections.Generic;
using System.Linq;
using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TciCommon.Models;
using TciCommon.Server;
using TciEnergy.Blazor.Shared.Models;
using TciEnergy.Blazor.Shared.ViewModels;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PlaceController : BaseController
    {

        public PlaceController(ProvinceDBs dbs) : base(dbs) { }

        public ActionResult<List<TextValue>> ProvinceList()
        {
            return dbs.CommonDb.Find<Province>(p => p.Applications.Contains("Energy")).SortBy(p => p.Name)
                .ToEnumerable().Select(p => new TextValue { Text = p.Name, Value = p.Prefix }).ToList();
        }

        [Authorize]
        public ActionResult<City> MainCity() => GetMainCity(db);

        public static City GetMainCity(IReadOnlyDbContext db)
        {
            var mainCitySettings = db.FindFirst<Settings>(s => s.Key == "MainCity");
            return db.FindById<City>(mainCitySettings.Value);
        }

        [Authorize]
        public ActionResult<List<City>> CitiesList()
        {
            return Cities.ToList();
        }

    }
}
