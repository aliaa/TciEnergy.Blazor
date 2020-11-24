using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class ChangesReportResponse
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Display(Name = "شماره اشتراک")]
        public int SubsNum { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CityId { get; set; }

        [Display(Name = "نام مشترک")]
        public string SubscriberName { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SubscriberId { get; set; }

        [Display(Name = "شهر")]
        public string CityName { get; set; }

        [Display(Name = "آخرین مقدار")]
        public float LastValue { get; set; }

        [Display(Name = "مقدار قبلی")]
        public float PreviousValue { get; set; }

        [Display(Name = "درصد تغییرات")]
        public float ChangesPercent
        {
            get
            {
                if (LastValue == PreviousValue)
                    return 0;
                if (PreviousValue == 0)
                    return 100;
                return ((LastValue - PreviousValue) / PreviousValue) * 100f;
            }
        }
    }
}
