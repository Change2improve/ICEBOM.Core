using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Services;

namespace ICEBOM.Core.App.Services
{
    public class CustomerConfigReader
    {
        public ICEBOMCustomerConfig Read(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("No se encontró el archivo de configuración del cliente.", filePath);

            var json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());

            var config = JsonSerializer.Deserialize<ICEBOMCustomerConfig>(json, options);

            if (config == null)
                throw new InvalidOperationException("No se pudo deserializar la configuración del cliente.");

            var validator = new ICEBOMCustomerConfigValidator();
            var errors = validator.Validate(config);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "La configuración del cliente no es válida:\n" +
                    string.Join("\n", errors));
            }


            return config;
        }
    }
}