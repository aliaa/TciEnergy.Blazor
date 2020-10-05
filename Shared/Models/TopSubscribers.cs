using System.Collections.Generic;

namespace TciEnergy.Blazor.Shared.Models
{
    public class TopSubscribers
    {
        public List<ElecSubscriberSummary> List { get; set; } = new List<ElecSubscriberSummary>();
        public int Year { get; set; }
        public int Period { get; set; }
    }
}
