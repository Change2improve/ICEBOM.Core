using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.Domain.Normalizers
{
    public class ICEBOMUnitNormalizer
    {
        private readonly ICEBOMUnitConfig _config;

        public ICEBOMUnitNormalizer(ICEBOMUnitConfig config)
        {
            _config = config;
        }

        public string Normalize(string? unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
                return _config.KnownUnits.FirstOrDefault() ?? "Ud";

            var value = unit.Trim().ToLowerInvariant();

            if (_config.Aliases.TryGetValue(value, out var normalized))
                return normalized;

            return unit;
        }

        public bool IsKnown(string? unit)
        {
            var normalized = Normalize(unit);
            return _config.KnownUnits.Contains(normalized);
        }
    }
}