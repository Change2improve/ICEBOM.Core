using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.Domain.Normalizers
{
    public class ICEBOMUnitNormalizer
    {
        private readonly ICEBOMUnitConfig _config;
        private readonly string _defaultUnit;

        public ICEBOMUnitNormalizer(ICEBOMUnitConfig config, string? defaultUnit = null)
        {
            _config = config ?? new ICEBOMUnitConfig();

            _defaultUnit = string.IsNullOrWhiteSpace(defaultUnit)
                ? "Units"
                : defaultUnit.Trim();
        }

        public string Normalize(string? unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
                return _defaultUnit;

            var trimmed = unit.Trim();
            var key = trimmed.ToLowerInvariant();

            if (_config.Aliases != null &&
                _config.Aliases.TryGetValue(key, out var normalized) &&
                !string.IsNullOrWhiteSpace(normalized))
            {
                return normalized.Trim();
            }

            return trimmed;
        }

        public bool IsKnown(string? unit)
        {
            var normalized = Normalize(unit);

            return _config.KnownUnits != null &&
                   _config.KnownUnits.Any(known =>
                       string.Equals(
                           known?.Trim(),
                           normalized,
                           StringComparison.OrdinalIgnoreCase));
        }
    }
}