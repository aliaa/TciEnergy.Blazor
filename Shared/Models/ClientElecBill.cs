using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.Models
{
    public class ClientElecBill : ElecBill
    {
        [Display(Name = "نام مشترک")]
        public string SubscriberName { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string SubscriberId { get; set; }

        [Display(Name = "شهر")]
        public string CityName { get; set; }
    }
}
