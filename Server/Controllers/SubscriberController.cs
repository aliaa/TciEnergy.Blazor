using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Omu.ValueInjecter;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TciCommon.Server;
using TciEnergy.Blazor.Shared.Models;
using TciEnergy.Blazor.Shared.ViewModels;

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

        public ActionResult<Subscriber> Item(string id)
        {
            return db.FindById<Subscriber>(id);
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
                filter = fb.Eq(s => s.City, city);

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

        public async Task<IActionResult> ExcelFile(string city)
        {
            var list = GetList(city);
            var table = tableFactory.Create(list, excludeColumns: new string[] { nameof(ClientSubscriber.Id) });
            return await CreateExcelFile(table, "Subscribers", "Subscribers.xlsx");
        }

        //[Authorize(nameof(Permission.ChangeSubscribers))] //TODO
        [HttpPost]
        public IActionResult Save(Subscriber item)
        {
            try
            {
                db.Save(item);
                return Ok("اطلاعات مشترک با موفقیت ذخیره شد!");
            }
            catch
            {
                return BadRequest("خطا: شماره اشتراک یا نام مشترک قبلا موجود میباشد!");
            }
        }
    }
}
