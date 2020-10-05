using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TciEnergy.Blazor.Shared.Models;
using TciCommon.ServerUtils;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Linq;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DashboardController : BaseController
    {
        public DashboardController(ProvinceDBs dbs) : base(dbs) { }


        private class BillPeriodGroupping
        {
            public int Year { get; set; }
            public int Period { get; set; }
            public int Count { get; set; }
            public long Sum1 { get; set; }
            public long Sum2 { get; set; }
            public long Sum3 { get; set; }
            public long Sum4 { get; set; }
        }

        private class BillPeriodList : List<BillPeriodGroupping>
        {
            public BillPeriodList() : base() { }
            public BillPeriodList(IEnumerable<BillPeriodGroupping> list) : base(list) { }

            public BillPeriodGroupping Latest
            {
                get
                {
                    BillPeriodGroupping res = null;
                    foreach (var bp in this)
                        if (res == null || bp.Year > res.Year || (bp.Year == res.Year && res.Period < bp.Period))
                            res = bp;
                    return res;
                }
            }

            public BillPeriodGroupping GetPrevious(BillPeriodGroupping current)
            {
                int pyear = current.Year;
                int pperiod = current.Period - 1;
                if (pperiod == 0)
                {
                    pyear--;
                    pperiod = 6;
                }
                return GetByYearPeriod(pyear, pperiod);
            }

            public BillPeriodGroupping GetByYearPeriod(int year, int period)
            {
                foreach (BillPeriodGroupping bp in this)
                    if (bp.Year == year && bp.Period == period)
                        return bp;
                return new BillPeriodGroupping { Year = year, Period = period };
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ElecBillsTotalInformation>>> TotalPrices()
        {
            var result = new List<ElecBillsTotalInformation>();
            var mainCityPeriods = await AggregateOnPeriods(true);
            var othersPeriods = await AggregateOnPeriods(false);
            if (mainCityPeriods.Count == 0 && othersPeriods.Count == 0)
                return result;

            BillPeriodList bigger;
            if (mainCityPeriods.Count == 0)
                bigger = othersPeriods;
            else if (othersPeriods.Count == 0)
                bigger = mainCityPeriods;
            else if (mainCityPeriods[0].Year == othersPeriods[0].Year)
                bigger = mainCityPeriods[0].Period > othersPeriods[0].Period ? mainCityPeriods : othersPeriods;
            else if (mainCityPeriods[0].Year > othersPeriods[0].Year)
                bigger = mainCityPeriods;
            else
                bigger = othersPeriods;

            var current = bigger.Latest;
            for (int i = 0; i < 6; i++)
            {
                result.Add(new ElecBillsTotalInformation
                {
                    Year = current.Year,
                    Period = current.Period,
                    TotalPriceMainCity = mainCityPeriods.GetByYearPeriod(current.Year, current.Period).Sum1,
                    TotalPriceOthers = othersPeriods.GetByYearPeriod(current.Year, current.Period).Sum1,
                    BillsCountMainCity = mainCityPeriods.GetByYearPeriod(current.Year, current.Period).Count,
                    BillsCountOthers = othersPeriods.GetByYearPeriod(current.Year, current.Period).Count
                });
                current = bigger.GetPrevious(current);
            }
            return result;
        }

        private async Task<BillPeriodList> AggregateOnPeriods(bool mainCity)
        {
            var agg = db.Aggregate<ElecBill>();
            if (mainCity)
                agg = agg.Match(b => MainCityElecSubsNum.Contains(b.SubsNum));
            else
                agg = agg.Match(b => !MainCityElecSubsNum.Contains(b.SubsNum));
            var list = await agg.Group(
                    key => new { key.Year, key.Period },
                    g => new { Key = g.Key, Count = g.Count(), TotalPrice = g.Sum(b => b.TotalPrice) })
                .Project(x => new BillPeriodGroupping { Year = x.Key.Year, Period = x.Key.Period, Count = x.Count, Sum1 = x.TotalPrice })
                .SortByDescending(g => g.Year)
                .ThenByDescending(g => g.Period)
                .Limit(6).ToListAsync();
            return new BillPeriodList(list);
        }

        private List<int> _mainCityElecSubsNum = null;
        public List<int> MainCityElecSubsNum
        {
            get
            {
                if (_mainCityElecSubsNum == null)
                    _mainCityElecSubsNum = db.Find<Subscriber>(s => s.ElecSub != null && s.City == MainCityId).Project(s => s.ElecSub.ElecSubsNum).ToList();
                return _mainCityElecSubsNum;
            }
        }


        [HttpGet]
        public async Task<ActionResult<TopSubscribers>> TopUsageSubscribers(bool mainCity, int count = 10)
        {
            FilterDefinition<ElecBill> citiesFilter;
            var fb = Builders<ElecBill>.Filter;
            if (mainCity)
                citiesFilter = fb.In(b => b.SubsNum, MainCityElecSubsNum);
            else
                citiesFilter = fb.Nin(b => b.SubsNum, MainCityElecSubsNum);

            var lastPeriod = db.Find(citiesFilter).SortByDescending(b => b.Year).ThenByDescending(b => b.Period)
                .Project(b => new { b.Year, b.Period }).FirstOrDefault();
            if (lastPeriod == null)
                return new TopSubscribers();

            var list = await db.Aggregate<ElecBill>()
                .Match(citiesFilter)
                .Match(b => b.Year == lastPeriod.Year && b.Period == lastPeriod.Period)
                .Group(key => key.SubsNum, g => new ElecSubscriberSummary { SubsNum = g.Key, TotalSum = g.Sum(b => b.TotalPrice) })
                .SortByDescending(x => x.TotalSum)
                .Limit(count)
                .ToListAsync();

            foreach (var item in list)
            {
                var subscriber = db.FindFirst<Subscriber>(s => s.ElecSub.ElecSubsNum == item.SubsNum);
                if (subscriber != null)
                {
                    item.Name = subscriber.Name;
                    item.Id = subscriber.Id;
                }
            }
            return new TopSubscribers { List = list, Year = lastPeriod.Year, Period = lastPeriod.Period };
        }
    }
}
