namespace TciEnergy.Blazor.Shared.Models
{
    public class YearPeriod
    {
        public int Year { get; set; }
        public int Period { get; set; }

        public override string ToString()
        {
            return "سال " + Year + " دوره " + Period;
        }
    }
}
