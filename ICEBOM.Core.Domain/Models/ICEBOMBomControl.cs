namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMBomControl
    {
        public bool CreateBom { get; set; } = true;
        public bool? UpdateExistingBom { get; set; }
    }
}
