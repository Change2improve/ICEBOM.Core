namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooBomInfo
    {
        public int Id { get; set; }
        public string ProductReference { get; set; } = string.Empty;
        public bool Exists => Id > 0;
    }
}