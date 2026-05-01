namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMFunctionalTypeConfig
    {
        public Dictionary<string, string> Aliases { get; set; } = new()
        {
            { "manufactured", "Manufactured" },
            { "fabricado", "Manufactured" },
            { "fabricada", "Manufactured" },
            { "commercial", "Commercial" },
            { "comercial", "Commercial" }
        };
    }
}