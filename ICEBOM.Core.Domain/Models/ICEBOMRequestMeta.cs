namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMRequestMeta
    {
        public string Schema { get; set; } = "icebom.request.v1";
        public string ExportId { get; set; } = string.Empty;
        public DateTime ExportDate { get; set; } = DateTime.Now;
        public ICEBOMSourceInfo Source { get; set; } = new();
    }
}
