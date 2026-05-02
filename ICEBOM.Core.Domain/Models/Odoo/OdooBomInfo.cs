namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooBomInfo
    {
        public int Id { get; set; }

        public string ProductReference { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public int ProductTemplateId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal ProductQuantity { get; set; }

        public int UnitId { get; set; }

        public string UnitName { get; set; } = string.Empty;

        public bool Exists => Id > 0;
    }
}