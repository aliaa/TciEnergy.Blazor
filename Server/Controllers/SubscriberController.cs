using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Omu.ValueInjecter;
using System.Collections.Generic;
using System.Linq;
using TciCommon.ServerUtils;
using TciEnergy.Blazor.Shared.Models;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class SubscriberController : BaseController
    {
        public SubscriberController(ProvinceDBs dbs) : base(dbs) { }

        public ActionResult<List<ClientSubscriber>> List(string city)
        {
            FilterDefinition<Subscriber> filter;
            var fb = Builders<Subscriber>.Filter;
            if (city == "all")
                filter = fb.Empty;
            else if (city == "others")
                filter = fb.Ne(s => s.City, PlaceController.GetMainCity(db).Id);
            else
                filter = fb.Eq(s => s.City, ObjectId.Parse(city));

            var result = db.Find(filter).ToEnumerable()
                .Select(s => Mapper.Map<ClientSubscriber>(s).InjectFrom(s.ElecSub))
                .Cast<ClientSubscriber>().ToList();

            var citiesDic = Cities.ToDictionary(c => c.Id.ToString(), c => c.Name);
            foreach (var item in result)
            {
                if (citiesDic.ContainsKey(item.City))
                    item.City = citiesDic[item.City];
                else
                    item.City = null;
            }

            return result;
        }
    }
}
