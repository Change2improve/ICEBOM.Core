namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMComponentSync
    {
        public string Status { get; set; } = "ready";
        public List<ICEBOMError> Errors { get; set; } = new();
        public List<ICEBOMWarning> Warnings { get; set; } = new();

        public string Supplier { get; set; } = string.Empty;
        public string SupplierReference { get; set; } = string.Empty;

        public string Route { get; set; } = string.Empty;
        public string ManufacturingOperation { get; set; } = string.Empty;
    }
}
