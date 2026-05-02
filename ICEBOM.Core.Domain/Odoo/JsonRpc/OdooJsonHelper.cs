using System.Text.Json;

namespace ICEBOM.Core.Domain.Odoo.JsonRpc
{
    public static class OdooJsonHelper
    {
        public static string GetString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value))
                return string.Empty;

            return value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }

        public static int GetInt(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value))
                return 0;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result))
                return result;

            return 0;
        }

        public static long GetLong(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value))
                return 0;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var result))
                return result;

            return 0;
        }

        public static decimal GetDecimal(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value))
                return 0m;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var result))
                return result;

            return 0m;
        }

        public static long GetMany2OneId(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value))
                return 0;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var id))
                return id;

            if (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() > 0)
            {
                var first = value[0];

                if (first.ValueKind == JsonValueKind.Number && first.TryGetInt64(out var result))
                    return result;
            }

            return 0;
        }

        public static string GetMany2OneName(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value))
                return string.Empty;

            if (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() > 1)
                return value[1].GetString() ?? string.Empty;

            return string.Empty;
        }
    }
}