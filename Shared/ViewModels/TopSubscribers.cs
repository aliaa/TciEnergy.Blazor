using System.Collections.Generic;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class TopSubscribers
    {
        public List<SubscriberUsage> List { get; set; } = new List<SubscriberUsage>();
        public int Year { get; set; }
        public int Period { get; set; }
    }
}
