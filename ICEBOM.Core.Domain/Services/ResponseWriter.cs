using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.App.Services
{
    public class ResponseWriter
    {
        public string Write(string requestFilePath, ICEBOMResponse response)
        {
            var responsePath = Path.Combine(
                Path.GetDirectoryName(requestFilePath)!,
                "ICEBOM_Response.json");

            var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            });

            File.WriteAllText(responsePath, responseJson);

            return responsePath;
        }
    }
}