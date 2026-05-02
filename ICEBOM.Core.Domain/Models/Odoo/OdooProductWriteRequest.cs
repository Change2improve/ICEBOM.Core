namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooProductWriteRequest
    {
        public string Reference { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public int UnitId { get; set; }

        public bool CanBePurchased { get; set; } = true;

        public bool CanBeSold { get; set; } = false;

        public bool IsStorable { get; set; } = true;
    }
}