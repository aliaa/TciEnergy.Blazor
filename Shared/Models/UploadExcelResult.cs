using System.Collections.Generic;

namespace TciEnergy.Blazor.Shared.Models
{
    public class UploadExcelResult
    {
        public class Header
        {
            public int ColumnIndex { get; set; }
            public string Text { get; set; }
            public string BestSimilarField { get; set; }
            public float SimilarityRate { get; set; }
        }

        public List<Header> Headers { get; set; }
        public string FileName { get; set; }
    }
}
