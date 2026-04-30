using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Serialization;

namespace ICEBOM.Core.App.Services
{
    public class JsonRequestReader
    {
        public ICEBOMRequest Read(string filePath)
        {
            var json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new SafeEnumConverter<ICEBOMFunctionalTypeEnum>(ICEBOMFunctionalTypeEnum.Unknown));

            var request = JsonSerializer.Deserialize<ICEBOMRequest>(json, options);

            if (request == null)
                throw new InvalidOperationException("No se pudo deserializar el archivo JSON.");

            return request;
        }
    }
}