using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TciCommon.ServerUtils;
using TciEnergy.Blazor.Shared.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using AliaaCommon;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class BillsController : BaseController
    {
        private readonly DataTableFactory tableFactory;

        public BillsController(ProvinceDBs dbs, DataTableFactory tableFactory) : base(dbs)
        {
            this.tableFactory = tableFactory;
        }

        public ActionResult<List<string>> YearPeriods()
        {
            return db.Aggregate<ElecBill>()
                .Group(b => new { b.Year, b.Period }, g => new { g.Key })
                .ReplaceRoot(a => a.Key)
                .SortByDescending(a => a.Year)
                .ThenByDescending(a => a.Period)
                .ToEnumerable()
                .Select(a => a.Year + "-" + a.Period)
                .ToList();
        }

        public async Task<ActionResult<List<ClientElecBill>>> List(string city, string yearPeriod)
        {
            var split = yearPeriod.Split("-");
            int year = int.Parse(split[0]);
            int period = int.Parse(split[1]);

            var fb = Builders<ElecBill>.Filter;
            var filters = new List<FilterDefinition<ElecBill>>
            {
                fb.Eq(b => b.Year, year),
                fb.Eq(b => b.Period, period)
            };
            if (city != "all")
                filters.Add(fb.Eq(b => b.CityId, ObjectId.Parse(city)));

            var list = await db.Aggregate<ElecBill>()
                .Match(fb.And(filters))
                .Lookup(nameof(Subscriber), nameof(ElecBill.SubsNum), nameof(Subscriber.ElecSub) + "." + nameof(ElectricitySubscriber.ElecSubsNum), nameof(ClientElecBill.SubscriberName))
                .Unwind(nameof(ClientElecBill.SubscriberName))
                .AppendStage<ClientElecBill>("{$addFields: { SubscriberName : \"$SubscriberName.Name\" }}")
                .ToListAsync();

            var citiesDic = Cities.ToDictionary(k => k.Id, v => v.Name);
            foreach (var item in list)
            {
                if (citiesDic.TryGetValue(item.CityId, out string cityName))
                    item.CityName = cityName;
            }
            return list;
        }

        public async Task<IActionResult> ExcelFile(string city, string yearPeriod)
        {
            var list = (await List(city, yearPeriod)).Value;
            var table = tableFactory.Create(list, excludeColumns: new string[] { nameof(ClientElecBill.Id), nameof(ClientElecBill.CityId) });
            return await CreateExcelFile(table, yearPeriod, "ElecBills.xlsx");
        }
    }
}
