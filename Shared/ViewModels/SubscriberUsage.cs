using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class SubscriberUsage
    {
        public string Id { get; set; }

        [Display(Name = "نام مشترک")]
        public string Name { get; set; }

        [Display(Name = "شماره اشتراک")]
        public int SubsNum { get; set; }

        [Display(Name = "جمع")]
        public long Sum { get; set; }
    }
}
