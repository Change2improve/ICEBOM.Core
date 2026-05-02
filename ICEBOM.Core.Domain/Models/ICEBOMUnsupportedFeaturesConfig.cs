namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMUnsupportedFeaturesConfig
    {
        public bool BlockProductVariants { get; set; } = true;
        public bool WarnSupplierData { get; set; } = true;
        public bool WarnRoutes { get; set; } = true;
        public bool WarnManufacturingOperations { get; set; } = true;
    }
}
