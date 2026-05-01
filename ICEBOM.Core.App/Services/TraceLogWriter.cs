using System.IO;

using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.App.Services
{
    public class TraceLogWriter
    {
        public string Write(string requestFilePath, ICEBOMResponse response)
        {
            var logPath = Path.Combine(
                Path.GetDirectoryName(requestFilePath)!,
                "ICEBOM_Trace.log");

            var lines = new List<string>
            {
                $"ICEBOM Trace Log",
                $"RequestExportId: {response.Meta.RequestExportId}",
                $"CoreVersion: {response.Meta.CoreVersion}",
                $"CustomerName: {response.Meta.CustomerName}",
                $"CustomerConfigVersion: {response.Meta.CustomerConfigVersion}",
                $"ProcessDate: {response.Meta.ProcessDate:yyyy-MM-dd HH:mm:ss}",
                "",
                "TRACE",
                "-----"
            };

            foreach (var entry in response.Trace)
            {
                lines.Add($"[{entry.Step}] [{entry.Entity}] [{entry.Reference}] {entry.Message}");
            }

            File.WriteAllLines(logPath, lines);

            return logPath;
        }
    }
}