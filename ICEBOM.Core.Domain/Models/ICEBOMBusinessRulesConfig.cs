namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMBusinessRulesConfig
    {
        public bool CommercialCannotHaveBom { get; set; } = true;
        public bool ManufacturedMustHaveBom { get; set; } = true;
        public bool IgnoreChildrenWithBomIsWarning { get; set; } = true;
        public bool ExportErpFalseWithBomIsError { get; set; } = true;
        public bool ExportErpFalseUsedInBomIsError { get; set; } = true;
    }
}