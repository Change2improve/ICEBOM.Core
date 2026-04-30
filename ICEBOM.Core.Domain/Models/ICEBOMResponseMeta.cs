namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMResponseMeta
    {
        public string Schema { get; set; } = "icebom.response.v1";
        public string RequestExportId { get; set; } = string.Empty;
        public DateTime ProcessDate { get; set; } = DateTime.Now;
    }
}
