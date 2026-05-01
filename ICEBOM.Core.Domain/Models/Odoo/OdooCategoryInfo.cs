namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooCategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Exists => Id > 0;
    }
}