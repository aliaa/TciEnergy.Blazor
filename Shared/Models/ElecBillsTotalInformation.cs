using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TciEnergy.Blazor.Shared.Models
{
    public class ElecBillsTotalInformation
    {
        public int Year { get; set; }
        public int Period { get; set; }

        public string YearPeriod => Year + " دوره " + Period;

        [Display(Name = "مبلغ قابل پرداخت شهر اصلی")]
        public long TotalPriceMainCity { get; set; }
        
        [Display(Name = "مبلغ قابل پرداخت سایر شهرستانها")]
        public long TotalPriceOthers { get; set; }

        [Display(Name = "جمع مبلغ قابل پرداخت")]
        public long TotalPriceSum => TotalPriceMainCity + TotalPriceOthers;

        [Display(Name = "تعداد قبوض شهر اصلی")]
        public int BillsCountMainCity { get; set; }

        [Display(Name = "تعداد قبوض سایر شهرستانها")]
        public int BillsCountOthers { get; set; }
    }
}
