using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using TciCommon.ServerUtils;
using TciEnergy.Blazor.Shared.Models;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class SubscriberController : BaseController
    {
        private readonly PlaceController placeController;

        public SubscriberController(ProvinceDBs dbs, PlaceController placeController) : base(dbs)
        {
            this.placeController = placeController;
        }

        public ActionResult<List<Subscriber>> List(string city)
        {
            FilterDefinition<Subscriber> filter;
            var fb = Builders<Subscriber>.Filter;
            if (city == "all")
                filter = fb.Empty;
            else if (city == "others")
                filter = fb.Ne(s => s.City, placeController.MainCity().Value.Id);
            else
                filter = fb.Eq(s => s.City, ObjectId.Parse(city));

            return db.Find(filter).ToList();
        }
    }
}
