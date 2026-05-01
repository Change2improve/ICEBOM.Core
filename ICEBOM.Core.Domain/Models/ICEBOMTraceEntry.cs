namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMTraceEntry
    {
        public string Step { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
