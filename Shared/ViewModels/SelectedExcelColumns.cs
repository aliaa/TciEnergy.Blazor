using System.Collections.Generic;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class SelectedExcelColumns
    {
        public string FileName { get; set; }
        public Dictionary<int, string> SelectedColumns { get; set; }
        public bool OverwriteExistingBills { get; set; }
    }
}
