namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMExecutionConfig
    {
        public string Mode { get; set; } = "Production";
        public bool IncludeTrace { get; set; } = false;
    }
}
