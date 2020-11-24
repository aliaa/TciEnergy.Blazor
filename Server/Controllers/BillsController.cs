using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TciCommon.Server;
using TciEnergy.Blazor.Shared.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

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

        public async Task<ActionResult<List<string>>> YearPeriods()
        {
            return (await db.Aggregate<ElecBill>()
                .Group(b => new { b.Year, b.Period }, g => new { g.Key })
                .ReplaceRoot(a => a.Key)
                .SortByDescending(a => a.Year)
                .ThenByDescending(a => a.Period)
                .ToCursorAsync()).ToEnumerable()
                .Select(a => a.Year + "-" + a.Period)
                .ToList();
        }

        class FakeClientElecBill : ClientElecBill
        {
            public Subscriber Subscriber { get; set; }
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
                filters.Add(fb.Eq(b => b.CityId, city));

            var agg = db.Aggregate<ElecBill>().Match(fb.And(filters));
            return await ConvertAggregate(agg);
        }

        private async Task<List<ClientElecBill>> ConvertAggregate(IAggregateFluent<ElecBill> agg)
        {
            var agg2 = agg.Lookup(nameof(Subscriber), nameof(ElecBill.SubsNum), nameof(Subscriber.ElecSub) + "." + nameof(ElectricitySubscriber.ElecSubsNum), nameof(FakeClientElecBill.Subscriber))
                .Unwind(nameof(FakeClientElecBill.Subscriber))
                .AppendStage<FakeClientElecBill>("{$addFields: { " + nameof(ClientElecBill.SubscriberName) + " : \"$" + nameof(FakeClientElecBill.Subscriber) + "." + nameof(Subscriber.Name) + "\" }}")
                .AppendStage<FakeClientElecBill>("{$addFields: { " + nameof(ClientElecBill.SubscriberId) + " : \"$" + nameof(FakeClientElecBill.Subscriber) + "._id" + "\" }}")
                .Project<ClientElecBill>(Builders<FakeClientElecBill>.Projection.Exclude(x => x.Subscriber));

            var list = await agg2.ToListAsync();
            var citiesDic = Cities.ToDictionary(k => k.Id, v => v.Name);
            foreach (var item in list)
            {
                if (citiesDic.TryGetValue(item.CityId, out string cityName))
                    item.CityName = cityName;
            }
            return list;
        }

        public async Task<ActionResult<List<ClientElecBill>>> BySubscriber(string subscriber)
        {
            var subsNum = db.FindById<Subscriber>(subscriber).ElecSub.ElecSubsNum;
            var agg = db.Aggregate<ElecBill>().Match(b => b.SubsNum == subsNum);
            return await ConvertAggregate(agg);
        }

        public async Task<IActionResult> ExcelFile(string city, string yearPeriod)
        {
            var list = (await List(city, yearPeriod)).Value;
            var table = tableFactory.Create(list, excludeColumns: new string[] { nameof(ClientElecBill.Id), nameof(ClientElecBill.CityId) });
            return await CreateExcelFile(table, yearPeriod, "ElecBills.xlsx");
        }

        public IActionResult ChangePayStatus(string id, ElecBill.PayStatusEnum newStatus, long num)
        {
            var bill = db.FindById<ElecBill>(id);
            if (bill == null)
                return BadRequest("id not found");
            if (bill.PayStatus == ElecBill.PayStatusEnum.NotPaid && newStatus != ElecBill.PayStatusEnum.Paid)
                return BadRequest("newStatus != " + ElecBill.PayStatusEnum.Paid);
            if (bill.PayStatus == ElecBill.PayStatusEnum.Paid && newStatus != ElecBill.PayStatusEnum.Documented)
                return BadRequest("newStatus != " + ElecBill.PayStatusEnum.Documented);

            var update = Builders<ElecBill>.Update.Set(b => b.PayStatus, newStatus);
            if (newStatus == ElecBill.PayStatusEnum.Paid)
                update = update.Set(b => b.PaymentNumber, num);
            else if (newStatus == ElecBill.PayStatusEnum.Documented)
                update = update.Set(b => b.DocumentNumber, num);
            db.UpdateOne<ElecBill>(b => b.Id == id, update);
            return Ok();
        }

    }
}
