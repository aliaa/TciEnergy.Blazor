using System.ComponentModel.DataAnnotations;
using TciEnergy.Blazor.Shared.Models;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class ChangesReportRequest
    {
        public enum CompareWithEnum
        {
            [Display(Name = "دوره قبلی")]
            Previous,

            [Display(Name = "میانگین 3 دوره قبل")]
            Avg3Previous,

            [Display(Name = "دوره مشابه سال قبل")]
            PrevoisYear,
        }

        [Display(Name = "نام فیلد قبض")]
        public string FieldName { get; set; }
        
        [Display(Name = "مقایسه با")]
        public CompareWithEnum CompareWith { get; set; }

        [Display(Name = "حداقل درصد تغییر")]
        [Range(1, 50)]
        public int MinChangePercent { get; set; }

        public static readonly string[] FieldNames = new string[] 
        {
            nameof(ElecBill.PowerCoeficient),
            nameof(ElecBill.Maximeter),
            nameof(ElecBill.ContractedDemand),
            nameof(ElecBill.CalculatedDemand),
            nameof(ElecBill.ConsumedDemand),
            nameof(ElecBill.LowConsumption),
            nameof(ElecBill.MediumConsumption),
            nameof(ElecBill.HighConsumption),
            nameof(ElecBill.ReactiveConsumption),
            nameof(ElecBill.DailyActiveConsumption),
            nameof(ElecBill.LowConsumptionPrice),
            nameof(ElecBill.MediumConsumptionPrice),
            nameof(ElecBill.HighConsumptionPrice),
            nameof(ElecBill.ActiveConsumptionPrice),
            nameof(ElecBill.PowerPrice),
            nameof(ElecBill.TollPrice),
            nameof(ElecBill.ElecTollPrice),
            nameof(ElecBill.SubscriptionPrice),
            nameof(ElecBill.TaxPrices),
            nameof(ElecBill.SeasonPeakPrice),
            nameof(ElecBill.PowerViolationPrice)
        };
    }
}
