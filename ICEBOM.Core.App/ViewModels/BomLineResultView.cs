namespace ICEBOM.Core.App.ViewModels
{
    public class BomLineResultView
    {
        public string BomId { get; set; } = string.Empty;
        public string ComponentInternalId { get; set; } = string.Empty;
        public string ComponentReference { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string OriginalUnit { get; set; } = string.Empty;
        public string NormalizedUnit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ErrorsText { get; set; } = string.Empty;
        public string WarningsText { get; set; } = string.Empty;

        public int OdooUnitId { get; set; }
    }
}