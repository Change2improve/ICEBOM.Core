namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMComponentSource
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
}
