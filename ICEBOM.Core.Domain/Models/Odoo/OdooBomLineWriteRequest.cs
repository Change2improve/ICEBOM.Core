namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooBomLineWriteRequest
    {
        public int BomId { get; set; }

        public string ComponentReference { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public int UnitId { get; set; }

        public decimal Quantity { get; set; }

        public int Sequence { get; set; } = 10;
    }
}