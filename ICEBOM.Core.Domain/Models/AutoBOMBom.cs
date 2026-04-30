using System.Collections.Generic;

namespace ICEBOM.Core.Domain.Models
{
    public class AutoBOMBom
    {
        public string BomId { get; set; } = string.Empty;
        public string ProductReference { get; set; } = string.Empty;
        public string SourceComponentId { get; set; } = string.Empty;

        public AutoBOMBomControl Control { get; set; } = new();

        public List<AutoBOMBomLine> Lines { get; set; } = new();
    }

    public class AutoBOMBomControl
    {
        public bool CreateBom { get; set; } = true;
        public bool? UpdateExistingBom { get; set; }
    }

    public class AutoBOMBomLine
    {
        public string ComponentReference { get; set; } = string.Empty;
        public string ComponentInternalId { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = "Ud";
    }
}