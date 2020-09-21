namespace TciEnergy.Blazor.Client.Utils
{
    public class ColorUtils
    {
        public static string GetColorOfPercent(int? p)
        {
            if (p == null)
                return "initial";
            return new HSLColor(p.Value / 100f * 80f, 240f, 180f).ToHexString();
        }
    }
}
