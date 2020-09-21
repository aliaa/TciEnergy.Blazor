using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TciCommon.Models;
using TciEnergy.Blazor.Shared.ViewModels;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PlaceController : BaseController
    {
        private readonly IMongoCollection<Province> provinceCol;

        public PlaceController(IMongoCollection<Province> provinceCol)
        {
            this.provinceCol = provinceCol;
        }

        public async Task<ActionResult<List<TextValue>>> ProvinceList()
        {
            return (await provinceCol.Find(_ => true).SortBy(p => p.Name).ToCursorAsync())
                .ToEnumerable().Select(p => new TextValue { Text = p.Name, Value = p.Prefix }).ToList();
        }
    }
}
