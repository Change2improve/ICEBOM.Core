namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMComponentControl
    {
        public bool ExportErp { get; set; } = true;
        public bool IgnoreChildren { get; set; }
        public bool? UpdateExistingProduct { get; set; }
    }
}
