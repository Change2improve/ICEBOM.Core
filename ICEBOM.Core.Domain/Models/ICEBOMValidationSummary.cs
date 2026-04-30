namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMValidationSummary
    {
        public string Status { get; set; } = "ready";
        public List<ICEBOMError> Errors { get; set; } = new();
        public List<ICEBOMWarning> Warnings { get; set; } = new();
    }
}
