using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace TciEnergy.Blazor.Shared.Models
{
    [CollectionIndex(new string[] { nameof(SubsNum) })]
    public class ElecBill : MongoEntity
    {
        public static readonly PropertyInfo[] ValidImportProperties = typeof(ElecBill).GetProperties()
            .Where(p => p.CanWrite && p.Name != nameof(Id) && p.Name != nameof(CityId) && 
                p.Name != nameof(PayStatus) && p.Name != nameof(DocumentNumber) && p.Name != nameof(PaymentNumber))
            .ToArray();

        private static readonly Type[] OperatableTypes = new Type[] { typeof(int), typeof(long), typeof(float), typeof(double) };
        public static readonly PropertyInfo[] ValidReportProperties = ValidImportProperties
            .Where(p => OperatableTypes.Contains(p.PropertyType))
            .Where(p => p.Name != nameof(SubsNum) && p.Name != nameof(Year) && p.Name != nameof(Period) && p.Name != nameof(BillId))
            .ToArray();

        public enum PayStatusEnum
        {
            [Display(Name = "پرداخت نشده")]
            NotPaid,
            [Display(Name = "پرداخت شده")]
            Paid,
            [Display(Name = "سند زده شده")]
            Documented,
        }

        [Display(Name = "شماره اشتراک")]
        public int SubsNum { get; set; }

        public ObjectId CityId { get; set; }

        [BsonRepresentation(BsonType.String)]
        [Display(Name = "وضعیت پرداخت")]
        public PayStatusEnum PayStatus { get; set; }

        private int _year;
        [Display(Name = "سال")]
        public int Year
        {
            get { return _year; }
            set
            {
                if (value < 100)
                {
                    if (value >= 60)
                        _year = 1300 + value;
                    else
                        _year = 1400 + value;
                }
                else
                    _year = value;
            }
        }

        [Display(Name = "دوره")]
        public int Period { get; set; }

        [Display(Name = "تاریخ قبلی")]
        public DateTime PreviousDate { get; set; }

        [Display(Name = "تاریخ فعلی")]
        public DateTime CurrentDate { get; set; }

        [Display(Name = "تعداد روز")]
        public int DayCount
        {
            get
            {
                return (int)Math.Round((CurrentDate - PreviousDate).TotalDays);
            }
        }

        [Display(Name = "ضریب قدرت")]
        public float PowerCoeficient { get; set; }

        [Display(Name = "ماکسیمتر")]
        public float Maximeter { get; set; }

        [Display(Name = "دیماند قراردادی")]
        public float ContractedDemand { get; set; }

        [Display(Name = "دیماند محاسبه")]
        public float CalculatedDemand { get; set; }

        [Display(Name = "دیماند مصرفی")]
        public float ConsumedDemand { get; set; }

        [Display(Name = "مصرف کم بار")]
        public int LowConsumption { get; set; }

        [Display(Name = "مبلغ کم بار")]
        public int LowConsumptionPrice { get; set; }

        [Display(Name = "مصرف میان بار")]
        public int MediumConsumption { get; set; }

        [Display(Name = "مبلغ میان بار")]
        public int MediumConsumptionPrice { get; set; }

        [Display(Name = "مصرف اوج بار")]
        public int HighConsumption { get; set; }

        [Display(Name = "مبلغ اوج بار")]
        public int HighConsumptionPrice { get; set; }

        [Display(Name = "مصرف راکتیو")]
        public int ReactiveConsumption { get; set; }

        [Display(Name = "بهای مصرف راکتیو")]
        public int ReactiveConsumptionPrice { get; set; }

        [Display(Name = "جمع مصرف اکتیو")]
        public int ActiveConsumptionSum { get; set; }

        [Display(Name = "بهای مصرف اکتیو")]
        public int ActiveConsumptionPrice { get; set; }

        [Display(Name = "بهای قدرت")]
        public int PowerPrice { get; set; }

        [Display(Name = "مبلغ عوارض")]
        public int TollPrice { get; set; }

        [Display(Name = "مبلغ عوارض برق")]
        public int ElecTollPrice { get; set; }

        [Display(Name = "مبلغ آبونمان")]
        public int SubscriptionPrice { get; set; }

        [Display(Name = "مبلغ مالیات")]
        public int TaxPrices { get; set; }

        [Display(Name = "مبلغ پیک فصل")]
        public int SeasonPeakPrice { get; set; }

        [Display(Name = "بهای تجاوز از قدرت")]
        public int PowerViolationPrice { get; set; }

        [Display(Name = "مبلغ دوره")]
        public int PeriodPrice { get; set; }

        [Display(Name = "شناسه قبض")]
        public long BillId { get; set; }

        [Display(Name = "بدهی قبلی")]
        public long PreviousDept { get; set; }

        [Display(Name = "مبلغ قابل پرداخت")]
        public long TotalPrice { get; set; }

        [Display(Name = "شماره رسید پرداخت")]
        public long? PaymentNumber { get; set; }

        [Display(Name = "شماره سند")]
        public long? DocumentNumber { get; set; }
    }
}
