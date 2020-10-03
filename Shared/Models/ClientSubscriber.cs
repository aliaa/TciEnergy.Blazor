using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.Models
{
    public class ClientSubscriber
    {
        public ObjectId Id { get; set; }

        [Display(Name = "نام مشترک")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "آدرس")]
        [Required]
        public string Address { get; set; }

        [Display(Name = "شهر")]
        [Required]
        public string City { get; set; }

        [Display(Name = "کد ساختمان")]
        public int BuildingCode { get; set; }

        [Display(Name = "کد دفتری")]
        public int RegistryCode { get; set; }

        [Display(Name = "تلفن")]
        public string Phone { get; set; }

        [Display(Name = "تعداد شماره های منصوبه")]
        public int CommCenterPhoneCapacity { get; set; }

        [Display(Name = "نوع مصرف کننده")]
        public Subscriber.ConsumerTypeEnum ConsumerType { get; set; }

        [Display(Name = "شماره اشتراک برق")]
        public int ElecSubsNum { get; set; }

        [Display(Name = "قدرت قراردادی")]
        public int ContractedElecPower { get; set; }

        [Display(Name = "شماره بدنه کنتور")]
        public long ElecCounterBodyNum { get; set; }

        [Display(Name = "شماره پرونده")]
        public int DossierNum { get; set; }

        [Display(Name = "تعداد فاز")]
        public ElectricitySubscriber.ElecPhaseEnum PhaseType { get; set; }

        [Display(Name = "نوع تعرفه")]
        public ElectricitySubscriber.TariffTypeEnum TariffType { get; set; }

        [Display(Name = "شماره تنه")]
        public long BodyNumber { get; set; }

        [Display(Name = "ظرفیت ترانس")]
        public int TransCapacity { get; set; }

        [Display(Name = "ضریب کنتور")]
        public int CounterCoefficient { get; set; }
    }
}
