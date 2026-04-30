using ICEBOM.Core.Domain.Enums;

namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMBomResult
    {
        public string BomId { get; set; } = string.Empty;
        public string ProductReference { get; set; } = string.Empty;

        public string Status { get; set; } = "ready";

        public ICEBOMActionEnum Action { get; set; }

        public List<ICEBOMError> Errors { get; set; } = new();
        public List<ICEBOMWarning> Warnings { get; set; } = new();
    }
}
