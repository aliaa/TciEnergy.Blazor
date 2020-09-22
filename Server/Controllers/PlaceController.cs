using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TciCommon.Models;
using TciCommon.ServerUtils;
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
            return db.Find<Province>(_ => true).SortBy(p => p.Name)
                .ToEnumerable().Select(p => new TextValue { Text = p.Name, Value = p.Prefix }).ToList();
        }
    }
}
