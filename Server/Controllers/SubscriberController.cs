using AliaaCommon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using Omu.ValueInjecter;
using System.Collections.Generic;
using System.IO;
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
        private readonly DataTableFactory tableFactory;

        public SubscriberController(ProvinceDBs dbs, DataTableFactory tableFactory) : base(dbs)
        {
            this.tableFactory = tableFactory;
        }

        private List<ClientSubscriber> GetList(string city)
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

        public ActionResult<List<ClientSubscriber>> List(string city) => GetList(city);

        public IActionResult ExcelFile(string city)
        {
            var list = GetList(city);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            byte[] file;
            using (var memStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memStream))
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Subscribers");
                    var table = tableFactory.Create(list, excludeColumns: new string[] { nameof(ClientSubscriber.Id) });
                    sheet.Cells["A1"].LoadFromDataTable(table, true);
                    package.Save();
                }
                file = memStream.ToArray();
            }
            return File(file, "application/octet-stream", "Subscribers.xlsx");
        }
    }
}
