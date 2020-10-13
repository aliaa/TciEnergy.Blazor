
using System.Collections.Generic;

namespace TciEnergy.Blazor.Shared.Models
{
    public class SelectedExcelColumns
    {
        public string FileName { get; set; }
        public Dictionary<string, int> SelectedColumns { get; set; }
        public bool OverwriteExistingBills { get; set; }
    }
}
