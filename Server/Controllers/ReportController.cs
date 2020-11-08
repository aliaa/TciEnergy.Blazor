using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TciCommon.Models;
using TciCommon.Server;
using TciEnergy.Blazor.Shared.Models;
using TciEnergy.Blazor.Shared.ViewModels;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ReportController : BaseController
    {
        public ReportController(ProvinceDBs dbs) : base(dbs) { }

        private const string _ID = "_id";
        private const string VALUE = "Value";
        private const string YEAR = nameof(ElecBill.Year);
        private const string PERIOD = nameof(ElecBill.Period);
        private const string SUBSNUM = nameof(ElecBill.SubsNum);
        private const string CITYID = nameof(ElecBill.CityId);

        [HttpPost]
        public ActionResult<List<ChartItem>> Total(TotalReportRequest req)
        {
            var opStr = "$" + req.Operation.ToString().ToLower();

            int year = 0, period = 0;
            if(req.Period != "all")
            {
                var split = req.Period.Split('-');
                year = int.Parse(split[0]);
                period = int.Parse(split[1]);
            }

            City city = null;
            if (ObjectId.TryParse(req.City, out ObjectId cityId))
                city = Cities.FirstOrDefault(c => c.Id == cityId);

            var agg = db.Aggregate<ElecBill>();
            if (year != 0)
                agg = agg.Match(b => b.Year == year && b.Period == period);
            if (city != null)
                agg = agg.Match(b => b.CityId == city.Id);

            var report = new List<ChartItem>();

            if (req.BasedOn == TotalReportRequest.BasedOnEnum.Period)
            {
                var grouping = new BsonDocument { { _ID, new BsonDocument { { YEAR, "$" + YEAR }, { PERIOD, "$" + PERIOD } } } };
                grouping.AddRange(new BsonDocument { { VALUE, new BsonDocument { { opStr, "$" + req.Field } } } });

                var agg2 = agg.Group(grouping).Sort(new BsonDocument { { _ID + "." + YEAR, -1 }, { _ID + "." + PERIOD, -1 } });
                foreach (var item in agg2.ToEnumerable())
                    report.Add(new ChartItem { Text = $"سال {item[_ID][YEAR].AsInt32} دوره {item[_ID][PERIOD].AsInt32}", Value = GetValue(item[VALUE]) });
            }
            else if (req.BasedOn == TotalReportRequest.BasedOnEnum.Subscribers)
            {
                var fb = Builders<Subscriber>.Filter;
                var filter = fb.Empty;
                if (city != null)
                    filter = fb.Eq(s => s.City, city.Id);
                var citiesDic = Cities.ToDictionary(c => c.Id, c => c.Name);
                var subscribers = db.Find(filter).Project(s => new { s.City, s.ElecSub.ElecSubsNum, s.Name })
                    .ToEnumerable()
                    .Where(s => citiesDic.ContainsKey(s.City))
                    .ToDictionary(s => s.ElecSubsNum);
                
                var grouping = new BsonDocument { { _ID, "$" + SUBSNUM } };
                grouping.AddRange(new BsonDocument { { VALUE, new BsonDocument { { opStr, "$" + req.Field } } } });
                var agg2 = agg.Group(grouping).Sort(new BsonDocument { { VALUE, -1 } });
                var res = agg2.Limit(100).ToEnumerable();
                foreach (var item in res)
                {
                    int subsNum = item[_ID].AsInt32;
                    if(subscribers.ContainsKey(subsNum))
                        report.Add(new ChartItem { Text = subscribers[subsNum].Name, Value = GetValue(item[VALUE]) } );
                }
            }
            else
            {
                var grouping = new BsonDocument { { _ID, "$" + CITYID } };
                grouping.AddRange(new BsonDocument { { VALUE, new BsonDocument { { opStr, "$" + req.Field } } } });
                var agg2 = agg.Group(grouping).Sort(new BsonDocument { { VALUE, -1 } });
                foreach (var item in agg2.ToEnumerable())
                {
                    ObjectId cid = item[_ID].AsObjectId;
                    City c = db.FindById<City>(cid);
                    if (c != null)
                        report.Add(new ChartItem { Text = c.Name, Value = GetValue(item[VALUE]) } );
                }
            }

            return report;
        }

        public static double GetValue(BsonValue value)
        {
            if (value.IsInt32)
                return value.AsInt32;
            if (value.IsInt64)
                return value.AsInt64;
            if (value.IsDouble)
                return value.AsDouble;
            return double.NaN;
        }
    }
}
