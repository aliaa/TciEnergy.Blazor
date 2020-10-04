using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.Models
{
    public class ClientElecBill : ElecBill
    {
        [Display(Name = "نام مشترک")]
        public string SubscriberName { get; set; }

        public ObjectId SubscriberId { get; set; }

        [Display(Name = "شهر")]
        public string CityName { get; set; }
    }
}
