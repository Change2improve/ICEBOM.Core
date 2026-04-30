namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMResponseSummary
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
}
