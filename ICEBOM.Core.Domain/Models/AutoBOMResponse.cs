using System;
using System.Collections.Generic;

namespace ICEBOM.Core.Domain.Models
{
    public class AutoBOMResponse
    {
        public AutoBOMResponseMeta Meta { get; set; } = new();

        public AutoBOMResponseSummary Summary { get; set; } = new();

        public List<AutoBOMComponentResult> Components { get; set; } = new();
        public List<AutoBOMBomResult> Boms { get; set; } = new();
    }

    public class AutoBOMResponseMeta
    {
        public string Schema { get; set; } = "icebom.response.v1";
        public string RequestExportId { get; set; } = string.Empty;
        public DateTime ProcessDate { get; set; } = DateTime.Now;
    }

    public class AutoBOMResponseSummary
    {
        public string Status { get; set; } = "ready";

        public int TotalComponents { get; set; }
        public int CreatedProducts { get; set; }
        public int UpdatedProducts { get; set; }

        public int TotalBoms { get; set; }
        public int CreatedBoms { get; set; }
        public int UpdatedBoms { get; set; }

        public int ErrorsCount { get; set; }
        public int WarningsCount { get; set; }
    }

    public class AutoBOMComponentResult
    {
        public string InternalId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;

        public string Status { get; set; } = "ready";

        public string Action { get; set; } = string.Empty;

        public List<AutoBOMError> Errors { get; set; } = new();
        public List<AutoBOMWarning> Warnings { get; set; } = new();
    }

    public class AutoBOMBomResult
    {
        public string BomId { get; set; } = string.Empty;
        public string ProductReference { get; set; } = string.Empty;

        public string Status { get; set; } = "ready";

        public string Action { get; set; } = string.Empty;

        public List<AutoBOMError> Errors { get; set; } = new();
        public List<AutoBOMWarning> Warnings { get; set; } = new();
    }
}