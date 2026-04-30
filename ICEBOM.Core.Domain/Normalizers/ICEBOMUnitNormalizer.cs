namespace ICEBOM.Core.Domain.Normalizers
{
    public static class ICEBOMUnitNormalizer
    {
        public static string Normalize(string? unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
                return "Ud";

            var value = unit.Trim();

            return value.ToLowerInvariant() switch
            {
                "ud" => "Ud",
                "uds" => "Ud",
                "unidad" => "Ud",
                "unidades" => "Ud",
                "unit" => "Ud",
                "units" => "Ud",

                "m" => "m",
                "metro" => "m",
                "metros" => "m",

                "mm" => "mm",
                "milimetro" => "mm",
                "milímetro" => "mm",
                "milimetros" => "mm",
                "milímetros" => "mm",

                "kg" => "kg",
                "kilogramo" => "kg",
                "kilogramos" => "kg",

                _ => value
            };
        }

        public static bool IsKnown(string? unit)
        {
            var normalized = Normalize(unit);

            return normalized is "Ud" or "m" or "mm" or "kg";
        }
    }
}