using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TciEnergy.Blazor.Shared.Models;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class ChangesReportResponse : ClientElecBill
    {
        [Display(Name = "درصد تغییرات")]
        public float ChangesPercent { get; set; }
    }
}
