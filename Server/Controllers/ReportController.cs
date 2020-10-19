using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TciCommon.ServerUtils;
using TciEnergy.Blazor.Shared.ViewModels;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ReportController : BaseController
    {
        public ReportController(ProvinceDBs dbs) : base(dbs)
        {
        }

        [HttpPost]
        public ActionResult<List<ChartItem>> Total(TotalReportRequest req)
        {
            return new List<ChartItem> { new ChartItem { Text = "123", Value= 10 }, new ChartItem { Text = "456", Value = 8 } };
        }
    }
}
