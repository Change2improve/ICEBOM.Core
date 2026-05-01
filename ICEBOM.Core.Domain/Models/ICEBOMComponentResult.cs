using ICEBOM.Core.Domain.Enums;

namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMComponentResult
    {
        public string InternalId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;

        public string Status { get; set; } = "ready";

        public ICEBOMActionEnum Action { get; set; }

        public List<ICEBOMError> Errors { get; set; } = new();
        public List<ICEBOMWarning> Warnings { get; set; } = new();

        public int OdooProductId { get; set; }

        public int OdooCategoryId { get; set; }

        public string Category { get; set; } = string.Empty;

        public string OdooProductName { get; set; } = string.Empty;
        public int OdooUnitId { get; set; }
    }
}
