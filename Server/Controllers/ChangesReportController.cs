using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TciEnergy.Blazor.Shared.ViewModels;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/Reports/Changes")]
    [ApiController]
    public class ChangesReportController : ControllerBase
    {

        [HttpPost]
        public async Task<ActionResult<List<ChangesReportResponse>>> Changes(ChangesReportRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
