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
using Microsoft.AspNetCore.Http;
using System.IO;
using OfficeOpenXml;
using System;

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
                filters.Add(fb.Eq(b => b.CityId, ObjectId.Parse(city)));

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

        public async Task<ActionResult<List<ClientElecBill>>> BySubscriber(ObjectId subscriber)
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

        public IActionResult ChangePayStatus(ObjectId id, ElecBill.PayStatusEnum newStatus)
        {
            var bill = db.FindById<ElecBill>(id);
            if (bill == null)
                return BadRequest("id not found");
            if (bill.PayStatus == ElecBill.PayStatusEnum.NotPaid && newStatus != ElecBill.PayStatusEnum.Paid)
                return BadRequest("newStatus != " + ElecBill.PayStatusEnum.Paid);
            if (bill.PayStatus == ElecBill.PayStatusEnum.Paid && newStatus != ElecBill.PayStatusEnum.Documented)
                return BadRequest("newStatus != " + ElecBill.PayStatusEnum.Documented);
            db.UpdateOne<ElecBill>(b => b.Id == id, Builders<ElecBill>.Update.Set(b => b.PayStatus, newStatus));
            return Ok();
        }

        public async Task<UploadExcelResult> UploadExcel(IFormFile file)
        {
            var fileName = Path.GetTempFileName();
            FileInfo finfo = new FileInfo(fileName);
            finfo.Attributes = FileAttributes.Temporary;
            using var uploadedStream = file.OpenReadStream();
            using var memoryStream = new MemoryStream((int)file.Length);
            await uploadedStream.CopyToAsync(memoryStream);
            uploadedStream.Close();

            memoryStream.Position = 0;
            using var fileStream = finfo.OpenWrite();
            await memoryStream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
            fileStream.Close();

            memoryStream.Position = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var excel = new ExcelPackage(memoryStream);
            var sheet = excel.Workbook.Worksheets[excel.Compatibility.IsWorksheets1Based ? 1 : 0];
            var result = new UploadExcelResult { FileName = finfo.Name };
            result.Headers = Enumerable.Range(1, sheet.Dimension.Columns)
                .Select(col => new { col, text = sheet.GetValue<string>(1, col) })
                .Where(h => !string.IsNullOrEmpty(h.text))
                .Select(h => new UploadExcelResult.Header 
                { 
                    ColumnIndex = h.col, 
                    Text = h.text, 
                    BestSimilarField = BillFields[GetBestSimilarField(BillFields.Keys, h.text)]
                })
                .ToList();
            return result;
        }

        private static readonly Dictionary<string, string> BillFields = typeof(ElecBill).GetProperties()
            .Where(p => p.Name != nameof(ElecBill.Id) && p.Name != nameof(ElecBill.CityId) && p.CanWrite)
            .ToDictionary(k => AliaaCommon.Utils.DisplayName(k), v => v.Name);

        private string GetBestSimilarField(IEnumerable<string> fields, string compareTo)
        {
            float bestSimilarity = 0;
            string best = fields.FirstOrDefault();
            foreach (var f in fields)
            {
                float sim = AliaaCommon.Utils.GetSimilarityRateOfStrings(f, compareTo);
                if(sim > bestSimilarity)
                {
                    bestSimilarity = sim;
                    best = f;
                }
            }
            return best;
        }
    }
}
