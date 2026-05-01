namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooProductInfo
    {
        public int Id { get; set; }

        public int TemplateId { get; set; }

        public string Reference { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int UnitId { get; set; }

        public bool Exists => Id > 0;
    }
}