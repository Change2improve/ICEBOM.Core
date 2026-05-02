using ICEBOM.Core.Domain.Odoo.Auth;

namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooLoginResult
    {
        public bool Success { get; set; }

        public string UserMessage { get; set; } = string.Empty;

        public string TechnicalMessage { get; set; } = string.Empty;

        public int UserId { get; set; }

        public int PartnerId { get; set; }

        public int CompanyId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Database { get; set; } = string.Empty;

        public string SessionId { get; set; } = string.Empty;

        public string ServerVersion { get; set; } = string.Empty;

        public OdooSession? Session { get; set; }

        public bool HasError => !Success;
    }
}