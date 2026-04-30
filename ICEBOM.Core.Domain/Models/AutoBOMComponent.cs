namespace ICEBOM.Core.Domain.Models
{
    public class AutoBOMComponent
    {
        public string InternalId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public AutoBOMComponentSource Source { get; set; } = new();
        public AutoBOMComponentClassification Classification { get; set; } = new();
        public AutoBOMComponentControl Control { get; set; } = new();
        public AutoBOMComponentSync Sync { get; set; } = new();
    }

    public class AutoBOMComponentSource
    {
        public string Cad { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public bool ReadOnly { get; set; }
    }

    public class AutoBOMComponentClassification
    {
        public string FunctionalType { get; set; } = string.Empty;
        public bool IsCommercial { get; set; }
        public bool IsStorable { get; set; } = true;
        public bool IsSellable { get; set; }
        public bool IsSparePart { get; set; }
        public bool IsMaintenance { get; set; }
    }

    public class AutoBOMComponentControl
    {
        public bool ExportErp { get; set; } = true;
        public bool IgnoreChildren { get; set; }
        public bool? UpdateExistingProduct { get; set; }
    }

    public class AutoBOMComponentSync
    {
        public string Status { get; set; } = "ready";
        public List<AutoBOMError> Errors { get; set; } = new();
        public List<AutoBOMWarning> Warnings { get; set; } = new();
    }
}