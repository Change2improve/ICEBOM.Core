namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMComponentClassification
    {
        public string FunctionalType { get; set; } = string.Empty;
        public bool IsCommercial { get; set; }
        public bool IsStorable { get; set; } = true;
        public bool IsSellable { get; set; }
        public bool IsSparePart { get; set; }
        public bool IsMaintenance { get; set; }
    }
}
