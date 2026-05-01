namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMCustomerSyncPolicy
    {
        public bool CreateMissingProducts { get; set; } = true;
        public bool UpdateExistingProducts { get; set; } = false;
        public bool CreateMissingBoms { get; set; } = true;
        public bool UpdateExistingBoms { get; set; } = true;
        public bool AllowProductVariants { get; set; } = false;
    }
}
