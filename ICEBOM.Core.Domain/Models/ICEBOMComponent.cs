namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMComponent
    {
        public string InternalId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICEBOMComponentSource Source { get; set; } = new();
        public ICEBOMComponentClassification Classification { get; set; } = new();
        public ICEBOMComponentControl Control { get; set; } = new();
        public ICEBOMComponentSync Sync { get; set; } = new();
    }
}