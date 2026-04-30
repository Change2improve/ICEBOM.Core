using ICEBOM.Core.Domain.Enums;

namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMComponentClassification
    {
        public ICEBOMFunctionalTypeEnum FunctionalType { get; set; } = ICEBOMFunctionalTypeEnum.Unknown;
        public bool IsCommercial { get; set; }
        public bool IsStorable { get; set; } = true;
        public bool IsSellable { get; set; }
        public bool IsSparePart { get; set; }
        public bool IsMaintenance { get; set; }
    }
}
