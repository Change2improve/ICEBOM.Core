namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMFakeOdooConfig
    {
        public List<string> ExistingProducts { get; set; } = new();
        public List<string> ExistingBoms { get; set; } = new();

        public List<string> ExistingCategories { get; set; } = new();

        public List<string> ExistingUnits { get; set; } = new();
    }
}
