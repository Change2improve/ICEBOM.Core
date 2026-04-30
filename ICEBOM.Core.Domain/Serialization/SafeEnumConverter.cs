using System.Text.Json;
using System.Text.Json.Serialization;

namespace ICEBOM.Core.Domain.Serialization
{
    public class SafeEnumConverter<TEnum> : JsonConverter<TEnum>
        where TEnum : struct, Enum
    {
        private readonly TEnum _defaultValue;

        public SafeEnumConverter(TEnum defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public override TEnum Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();

                if (string.IsNullOrWhiteSpace(value))
                    return _defaultValue;

                if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
                    return result;
            }

            if (reader.TokenType == JsonTokenType.Number &&
                reader.TryGetInt32(out var intValue))
            {
                if (Enum.IsDefined(typeof(TEnum), intValue))
                    return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
            }

            return _defaultValue;
        }

        public override void Write(
            Utf8JsonWriter writer,
            TEnum value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}