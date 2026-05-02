namespace ICEBOM.Core.Domain.Odoo.Auth
{
    public class OdooSession
    {
        public bool IsConnected { get; set; }

        public string Url { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public int UserId { get; set; }
        public int PartnerId { get; set; }
        public int CompanyId { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string ServerVersion { get; set; } = string.Empty;
    }
}