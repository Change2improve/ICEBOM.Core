namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooBomWriteRequest
    {
        public string ProductReference { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public int ProductTemplateId { get; set; }

        public int UnitId { get; set; }

        public decimal ProductQuantity { get; set; } = 1m;

        public string Code { get; set; } = string.Empty;

        public string Type { get; set; } = "normal";
    }
}