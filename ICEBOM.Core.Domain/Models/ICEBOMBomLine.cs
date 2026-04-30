namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMBomLine
    {
        public string ComponentReference { get; set; } = string.Empty;
        public string ComponentInternalId { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = "Ud";
    }
}
