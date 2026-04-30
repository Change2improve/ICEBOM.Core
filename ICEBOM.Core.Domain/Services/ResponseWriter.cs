using System.IO;
using System.Text.Json;

using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.App.Services
{
    public class ResponseWriter
    {
        public string Write(string requestFilePath, AutoBOMResponse response)
        {
            var responsePath = Path.Combine(
                Path.GetDirectoryName(requestFilePath)!,
                "AutoBOM_Response.json");

            var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(responsePath, responseJson);

            return responsePath;
        }
    }
}