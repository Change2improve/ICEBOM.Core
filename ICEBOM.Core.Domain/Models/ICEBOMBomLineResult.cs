namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMBomLineResult
    {
        public string ComponentInternalId { get; set; } = string.Empty;
        public string ComponentReference { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string OriginalUnit { get; set; } = string.Empty;
        public string NormalizedUnit { get; set; } = string.Empty;
        public string Status { get; set; } = "ready";

        public List<ICEBOMError> Errors { get; set; } = new();
        public List<ICEBOMWarning> Warnings { get; set; } = new();
    }
}