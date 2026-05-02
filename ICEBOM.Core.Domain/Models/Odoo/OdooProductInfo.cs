namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooProductInfo
    {
        public int Id { get; set; }

        public int ProductTemplateId { get; set; }

        public string Reference { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int UnitId { get; set; }

        public string UnitName { get; set; } = string.Empty;

        public bool Exists => Id > 0;
    }
}