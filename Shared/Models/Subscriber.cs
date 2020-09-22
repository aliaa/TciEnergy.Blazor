using EasyMongoNet;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.Models
{
    [CollectionIndex("{\"" + nameof(ElecSub) + "." + nameof(ElectricitySubscriber.ElecSubsNum) + "\" :1}", Sparse = true, Unique = true)]
    [CollectionIndex(new string[] { nameof(Name) }, Unique = true)]
    [CollectionIndex(new string[] { nameof(City) })]
    [CollectionSave(WriteLog = true)]
    public class Subscriber : MongoEntity
    {
        public enum ConsumerTypeEnum
        {
            DLC,
            [Display(Name = "کافو نوری")]
            OpticKafu,
            WLL,
            BTS,
            [Display(Name = "سوئیچ پرظرفیت (بالای 5)")]
            HighCapacitySwitch,
            [Display(Name = "سوئیچ کم ظرفیت (زیر 5)")]
            LowCapacitySwitch,
            [Display(Name = "دیتا")]
            Data,
            [Display(Name = "اداری")]
            Office,
            [Display(Name = "سایر")]
            Other,
        }

        [Display(Name = "نام مشترک")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "شهر")]
        [Required]
        public ObjectId City { get; set; }

        [Display(Name = "کد ساختمان")]
        public int BuildingCode { get; set; }

        [Display(Name = "کد دفتری")]
        public int RegistryCode { get; set; }

        [Display(Name = "تلفن")]
        public string Phone { get; set; }

        [Display(Name = "تعداد شماره های منصوبه")]
        public int CommCenterPhoneCapacity { get; set; }

        [Display(Name = "نوع مصرف کننده")]
        public ConsumerTypeEnum ConsumerType { get; set; }

        public ElectricitySubscriber ElecSub { get; set; } = new ElectricitySubscriber();

        //public static Subscriber FindByElecSubsNum(IReadOnlyDbContext db, int subsNum)
        //{
        //    return db.FindFirst<Subscriber>(s => s.ElecSub.ElecSubsNum == subsNum);
        //}
    }

    public class ElectricitySubscriber
    {
        public enum ElecPhaseEnum
        {
            [Display(Name = "تک فاز")]
            Single,
            [Display(Name = "سه فاز")]
            Three,
        }

        public enum TariffTypeEnum
        {
            [Display(Name = "مصارف خانگی")]
            Home,
            [Display(Name = "مصارف آموزشی")]
            Education,
            [Display(Name = "سایر مصارف")]
            Other,
        }

        [Display(Name = "شماره اشتراک برق")]
        public int ElecSubsNum { get; set; }

        [Display(Name = "قدرت قراردادی")]
        public int ContractedElecPower { get; set; }

        [Display(Name = "شماره بدنه کنتور")]
        public long ElecCounterBodyNum { get; set; }

        [Display(Name = "شماره پرونده")]
        public int DossierNum { get; set; }

        [Display(Name = "تعداد فاز")]
        public ElecPhaseEnum PhaseType { get; set; }

        [Display(Name = "نوع تعرفه")]
        public TariffTypeEnum TariffType { get; set; }

        [Display(Name = "شماره تنه")]
        public long BodyNumber { get; set; }

        [Display(Name = "ظرفیت ترانس")]
        public int TransCapacity { get; set; }

        [Display(Name = "ضریب کنتور")]
        public int CounterCoefficient { get; set; }
    }
}
