using ICEBOM.Core.Domain.Models.Odoo;

namespace ICEBOM.Core.Domain.Odoo.Auth
{
    public static class OdooSessionFactory
    {
        public static OdooSession Create(OdooConnectionConfig config, int userId)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return new OdooSession
            {
                Url = config.Url,
                Database = config.Database,
                UserId = userId,
                Username = config.Username,
                Password = config.Password
            };
        }
    }
}