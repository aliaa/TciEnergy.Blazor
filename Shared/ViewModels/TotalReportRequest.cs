using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class TotalReportRequest
    {
        public enum BasedOnEnum
        {
            [Display(Name = "شهر")]
            City,
            [Display(Name = "دوره")]
            Period,
            [Display(Name = "مشترکین")]
            Subscribers,
        }

        public enum OperationEnum
        {
            [Display(Name = "جمع")]
            Sum,
            [Display(Name = "میانگین")]
            Avg,
        }

        [Display(Name = "شهر")]
        public string City { get; set; } = "all";

        [Display(Name = "دوره")]
        public string Period { get; set; } = "all";

        [Display(Name = "بر اساس")]
        public BasedOnEnum BasedOn { get; set; }

        [Display(Name = "فیلد")]
        public string Field { get; set; }

        [Display(Name = "عملگر")]
        public OperationEnum Operation { get; set; }
    }
}
