namespace ICEBOM.Core.App.ViewModels
{
    public class MessageView
    {
        public string Type { get; set; } = string.Empty; // Error / Warning
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
