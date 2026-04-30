using System;
using System.Collections.Generic;

namespace ICEBOM.Core.Domain.Models
{
    public class AutoBOMRequest
    {
        public AutoBOMRequestMeta Meta { get; set; } = new();
        public AutoBOMSettingsSnapshot SettingsSnapshot { get; set; } = new();

        public List<AutoBOMComponent> Components { get; set; } = new();
        public List<AutoBOMBom> Boms { get; set; } = new();

        public AutoBOMValidationSummary Validation { get; set; } = new();
    }

    public class AutoBOMRequestMeta
    {
        public string Schema { get; set; } = "icebom.request.v1";
        public string ExportId { get; set; } = string.Empty;
        public DateTime ExportDate { get; set; } = DateTime.Now;
        public AutoBOMSourceInfo Source { get; set; } = new();
    }

    public class AutoBOMSourceInfo
    {
        public string Cad { get; set; } = string.Empty;
        public string Connector { get; set; } = string.Empty;
        public string ConnectorVersion { get; set; } = string.Empty;
    }

    public class AutoBOMSettingsSnapshot
    {
        public bool CreateMissingProducts { get; set; } = true;
        public bool UpdateExistingProducts { get; set; } = false;
        public bool CreateMissingBoms { get; set; } = true;
        public bool UpdateExistingBoms { get; set; } = true;
        public bool AllowProductVariants { get; set; } = false;
    }

    public class AutoBOMValidationSummary
    {
        public string Status { get; set; } = "ready";
        public List<AutoBOMError> Errors { get; set; } = new();
        public List<AutoBOMWarning> Warnings { get; set; } = new();
    }
}