using System.IO;
using System.Text.Json;

using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.App.Services
{
    public class JsonRequestReader
    {
        public AutoBOMRequest Read(string filePath)
        {
            var json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var request = JsonSerializer.Deserialize<AutoBOMRequest>(json, options);

            if (request == null)
                throw new InvalidOperationException("No se pudo deserializar el archivo JSON.");

            return request;
        }
    }
}