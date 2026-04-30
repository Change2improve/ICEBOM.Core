using System.Collections.Generic;

namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMBom
    {
        public string BomId { get; set; } = string.Empty;
        public string ProductReference { get; set; } = string.Empty;
        public string SourceComponentId { get; set; } = string.Empty;

        public ICEBOMBomControl Control { get; set; } = new();

        public List<ICEBOMBomLine> Lines { get; set; } = new();
    }
}