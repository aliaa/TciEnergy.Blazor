
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.Models
{
    public class ElecSubscriberSummary
    {
        public ObjectId Id { get; set; }

        [Display(Name = "نام مشترک")]
        public string Name { get; set; }

        [Display(Name = "شماره اشتراک")]
        public int SubsNum { get; set; }

        [Display(Name = "جمع")]
        public long TotalSum { get; set; }
    }
}
