using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TciCommon.Server;
using TciEnergy.Blazor.Shared.ViewModels;
using MongoDB.Driver;
using TciEnergy.Blazor.Shared.Models;
using System.Linq;
using Omu.ValueInjecter;
using TciCommon.Models;
using System.Reflection;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/Reports/Changes")]
    [ApiController]
    public class ChangesReportController : BaseController
    {

        public ChangesReportController(ProvinceDBs dbs) : base(dbs) { }

        record BillValue(string Id, int SubsNum, string CityId, float Value);

        [HttpPost]
        public async Task<ActionResult<IEnumerable<ChangesReportResponse>>> Changes(ChangesReportRequest req)
        {
            var field = req.Field;
            Dictionary<int, BillValue> lastValues, prevValues;

            lastValues = (await GetBillValues(field, req.Year, req.Period)).ToDictionary(b => b.SubsNum);

            if (req.CompareWith == ChangesReportRequest.CompareWithEnum.Previous)
            {
                var prevPeriod = req.YearPeriod.Previous;
                prevValues = (await GetBillValues(field, prevPeriod.Year, prevPeriod.Period)).ToDictionary(b => b.SubsNum);
            }
            else if (req.CompareWith == ChangesReportRequest.CompareWithEnum.PrevoisYear)
            {
                prevValues = (await GetBillValues(field, req.Year - 1, req.Period)).ToDictionary(b => b.SubsNum);
            }
            else if(req.CompareWith == ChangesReportRequest.CompareWithEnum.Avg3Previous)
            {
                var values = new List<BillValue>();
                var prevPeriod = req.YearPeriod;
                for (int i = 0; i < 3; i++)
                {
                    prevPeriod = prevPeriod.Previous;
                    values.AddRange(await GetBillValues(field, prevPeriod.Year, prevPeriod.Period));
                }
                prevValues = values.GroupBy(b => b.SubsNum)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.First() with { Value = g.Average(b => b.Value) })
                    .ToDictionary(b => b.SubsNum);
            }
            else
                throw new NotImplementedException();

            var result = new List<ChangesReportResponse>(lastValues.Count);
            var cities = (await db.Find<City>(_ => true).ToListAsync()).ToDictionary(c => c.Id, c => c.Name);
            var subs = (await db.Find<Subscriber>(_ => true).ToListAsync()).ToDictionary(s => s.ElecSub.ElecSubsNum, s => s.Name);
            foreach (var lv in lastValues.Where(lv => prevValues.ContainsKey(lv.Key)).Select(kv => kv.Value))
            {
                var resItem = Mapper.Map<ChangesReportResponse>(lv);
                resItem.LastValue = lv.Value;
                resItem.PreviousValue = prevValues[lv.SubsNum].Value;
                if (Math.Abs(resItem.ChangesPercent) < req.MinChangePercent)
                    continue;
                if (cities.TryGetValue(lv.CityId, out string cityName))
                    resItem.CityName = cityName;
                if (subs.TryGetValue(lv.SubsNum, out string subsName))
                    resItem.SubscriberName = subsName;
                result.Add(resItem);
            }

            result.Sort((ChangesReportResponse a, ChangesReportResponse b) => (int)(Math.Abs(a.ChangesPercent) - Math.Abs(b.ChangesPercent)));
            result.Reverse();
            return result;
        }

        private async Task<IEnumerable<BillValue>> GetBillValues(PropertyInfo field, int year, int period)
        {
            return (await db.Find<ElecBill>(b => b.Year == year && b.Period == period)
                .ToCursorAsync()).ToEnumerable()
                .Select(b => new BillValue(b.Id, b.SubsNum, b.CityId, ConvertBoxedToFloat(field.PropertyType, field.GetValue(b))));
        }

        private static float ConvertBoxedToFloat(Type type, object obj)
        {
            if (type == typeof(int))
                return (int)obj;
            if (type == typeof(float))
                return (float)obj;
            if (type == typeof(double))
                return (float)(double)obj;
            throw new ArgumentException();
        }
    }
}
