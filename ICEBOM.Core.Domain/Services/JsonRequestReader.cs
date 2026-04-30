using System.IO;
using System.Text.Json;

using ICEBOM.Core.Domain.Models;

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

            var request = JsonSerializer.Deserialize<ICEBOMRequest>(json, options);

            if (request == null)
                throw new InvalidOperationException("No se pudo deserializar el archivo JSON.");

            return request;
        }
    }
}