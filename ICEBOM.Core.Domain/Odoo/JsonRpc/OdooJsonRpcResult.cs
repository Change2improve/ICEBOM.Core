using System.Text.Json;

namespace ICEBOM.Core.Domain.Odoo.JsonRpc
{
    public class OdooJsonRpcResult
    {
        public bool Success => ErrorMessage == string.Empty;

        public string RawResponse { get; set; } = string.Empty;
        public JsonElement? Result { get; set; }

        public int? ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}