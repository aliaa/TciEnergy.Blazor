using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using TciEnergy.Blazor.Shared.Models;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class ClientElecBill : ElecBill
    {
        [Display(Name = "نام مشترک")]
        public string SubscriberName { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SubscriberId { get; set; }

        [Display(Name = "شهر")]
        public string CityName { get; set; }
    }
}
