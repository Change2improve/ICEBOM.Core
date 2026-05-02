namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMResponseMeta
    {
        public string Schema { get; set; } = "icebom.response.v1";

        public string CoreVersion { get; set; } = string.Empty;
        public string RequestExportId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerConfigVersion { get; set; } = string.Empty;
        public string CustomerConfigPath { get; set; } = string.Empty;

        public bool CreateMissingProducts { get; set; }
        public bool UpdateExistingProducts { get; set; }
        public bool CreateMissingBoms { get; set; }
        public bool UpdateExistingBoms { get; set; }
        public bool AllowProductVariants { get; set; }

        public DateTime ProcessDate { get; set; } = DateTime.Now;

        public string ExecutionId { get; set; } = string.Empty;
    }
}
