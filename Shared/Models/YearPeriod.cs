namespace TciEnergy.Blazor.Shared.Models
{
    public struct YearPeriod
    {
        public int Year { get; set; }
        public int Period { get; set; }

        public override string ToString()
        {
            return "سال " + Year + " دوره " + Period;
        }

        public YearPeriod Previous
        {
            get
            {
                int _year = Year, _period = Period;
                if(_period > 1)
                    _period--;
                else
                {
                    _period = 6;
                    _year--;
                }
                return new YearPeriod { Year = _year, Period = _period };
            }
        }

        public YearPeriod Next
        {
            get
            {
                int _year = Year, _period = Period;
                if (_period < 6)
                    _period++;
                else
                {
                    _period = 1;
                    _year++;
                }
                return new YearPeriod { Year = _year, Period = _period };
            }
        }
    }
}
