using System.Text.Json;

using ICEBOM.Core.Domain.Constants;
using ICEBOM.Core.Domain.Models.Odoo;
using ICEBOM.Core.Domain.Odoo.JsonRpc;

namespace ICEBOM.Core.Domain.Odoo.Auth
{
    public class OdooAuthenticator
    {
        public async Task<OdooLoginResult> AuthenticateAsync(
            OdooConnectionConfig config,
            CancellationToken cancellationToken = default)
        {
            if (config == null)
                return Fail("Configuración de conexión Odoo no informada.");

            if (string.IsNullOrWhiteSpace(config.Url))
                return Fail("URL de Odoo no informada.");

            if (string.IsNullOrWhiteSpace(config.Database))
                return Fail("Base de datos de Odoo no informada.");

            if (string.IsNullOrWhiteSpace(config.Username))
                return Fail("Usuario de Odoo no informado.");

            if (string.IsNullOrWhiteSpace(config.Password))
                return Fail("Password de Odoo no informado.");

            try
            {
                var client = new OdooJsonRpcClient(config.Url);

                var parameters = new Dictionary<string, object>
                {
                    ["db"] = config.Database,
                    ["login"] = config.Username,
                    ["password"] = config.Password
                };

                var result = await client.ExecuteAsync(
                    OdooJsonRpcRoutes.Authenticate,
                    parameters,
                    cancellationToken);

                if (result == null || result.Value.ValueKind == JsonValueKind.False)
                    return Fail("No se pudo autenticar contra Odoo.");

                if (result.Value.ValueKind != JsonValueKind.Object)
                    return Fail("La respuesta de autenticación de Odoo no tiene el formato esperado.");

                var root = result.Value;

                var userId = GetInt(root, "uid");

                if (userId <= 0)
                    return Fail("Credenciales incorrectas o usuario no válido.");

                var session = new OdooSession
                {
                    Url = config.Url,
                    Database = config.Database,
                    UserId = userId,
                    Username = config.Username,
                    Password = config.Password
                };

                return new OdooLoginResult
                {
                    Success = true,
                    UserMessage = "Autenticación correcta.",
                    TechnicalMessage = string.Empty,

                    UserId = userId,
                    PartnerId = GetInt(root, "partner_id"),
                    CompanyId = GetInt(root, "company_id"),

                    UserName = GetString(root, "name"),
                    Database = GetString(root, "db"),
                    SessionId = GetString(root, "session_id"),
                    ServerVersion = GetString(root, "server_version"),

                    Session = session
                };
            }
            catch (Exception ex)
            {
                return Fail("Error autenticando contra Odoo.", ex.Message);
            }
        }

        private static OdooLoginResult Fail(string userMessage, string technicalMessage = "")
        {
            return new OdooLoginResult
            {
                Success = false,
                UserMessage = userMessage,
                TechnicalMessage = technicalMessage,
                UserId = 0,
                PartnerId = 0,
                CompanyId = 0,
                UserName = string.Empty,
                Database = string.Empty,
                SessionId = string.Empty,
                ServerVersion = string.Empty,
                Session = null
            };
        }

        private static int GetInt(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var element))
                return 0;

            if (element.ValueKind == JsonValueKind.Number &&
                element.TryGetInt32(out var value))
                return value;

            return 0;
        }

        private static string GetString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var element))
                return string.Empty;

            return element.ValueKind == JsonValueKind.String
                ? element.GetString() ?? string.Empty
                : string.Empty;
        }
    }
}