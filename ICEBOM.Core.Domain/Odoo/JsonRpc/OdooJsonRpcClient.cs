using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

using ICEBOM.Core.Domain.Constants;

namespace ICEBOM.Core.Domain.Odoo.JsonRpc
{
    public class OdooJsonRpcClient
    {
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookies = new();

        public string BaseUrl { get; }

        public OdooJsonRpcClient(string baseUrl)
        {
            BaseUrl = NormalizeBaseUrl(baseUrl);

            var handler = new HttpClientHandler
            {
                CookieContainer = _cookies,
                UseCookies = true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<JsonElement?> ExecuteAsync(string route, object parameters, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(route);

            var payload = new
            {
                jsonrpc = OdooJsonRpcFields.JsonRpcVersion,
                method = "call",
                @params = parameters,
                id = Guid.NewGuid().ToString()
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new OdooJsonRpcException((int)response.StatusCode, responseText);

            using var document = JsonDocument.Parse(responseText);
            var root = document.RootElement.Clone();

            if (root.TryGetProperty(OdooJsonRpcFields.Response.Error, out var error) &&
                error.ValueKind != JsonValueKind.Null)
            {
                var message = ExtractOdooErrorMessage(error);

                var code = error.TryGetProperty("code", out var codeElement) &&
                           codeElement.TryGetInt32(out var parsedCode)
                    ? parsedCode
                    : -1;

                throw new OdooJsonRpcException(code, message);
            }

            if (!root.TryGetProperty(OdooJsonRpcFields.Response.Result, out var result))
                return null;

            return result.Clone();
        }

        private static string ExtractOdooErrorMessage(JsonElement error)
        {
            if (error.TryGetProperty("data", out var data) &&
                data.ValueKind == JsonValueKind.Object &&
                data.TryGetProperty("message", out var dataMessage) &&
                dataMessage.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(dataMessage.GetString()))
            {
                return dataMessage.GetString()!;
            }

            if (error.TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(message.GetString()))
            {
                return message.GetString()!;
            }

            return "Error JSON-RPC de Odoo";
        }

        public async Task<JsonElement?> ExecuteKwAsync(string database, int userId, string password, string model, string method,
            object[]? args = null, Dictionary<string, object>? kwargs = null, CancellationToken cancellationToken = default)
        {
            var executeArgs = new object[]
            {
                database,
                userId,
                password,
                model,
                method,
                args ?? Array.Empty<object>(),
                kwargs ?? new Dictionary<string, object>()
            };

            var parameters = new Dictionary<string, object>
            {
                [OdooJsonRpcParameterFields.Service] = "object",
                [OdooJsonRpcParameterFields.Method] = "execute_kw",
                [OdooJsonRpcParameterFields.Args] = executeArgs
            };

            return await ExecuteAsync(
                OdooJsonRpcRoutes.JsonRpc,
                parameters,
                cancellationToken);
        }

        private string BuildUrl(string route)
        {
            return $"{BaseUrl.TrimEnd('/')}/{route.TrimStart('/')}";
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            return (baseUrl ?? string.Empty).Trim().TrimEnd('/');
        }
    }
}