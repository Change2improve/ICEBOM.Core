namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMUnitConfig
    {
        public List<string> KnownUnits { get; set; } = new()
        {
            "Ud",
            "m",
            "mm",
            "kg"
        };

        public Dictionary<string, string> Aliases { get; set; } = new()
        {
            { "ud", "Ud" },
            { "uds", "Ud" },
            { "unidad", "Ud" },
            { "unidades", "Ud" },
            { "unit", "Ud" },
            { "units", "Ud" },
            { "metro", "m" },
            { "metros", "m" },
            { "milimetro", "mm" },
            { "milímetro", "mm" },
            { "milimetros", "mm" },
            { "milímetros", "mm" },
            { "kilogramo", "kg" },
            { "kilogramos", "kg" }
        };
    }
}
