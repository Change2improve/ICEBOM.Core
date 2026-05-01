using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.Domain.Normalizers
{
    public class ICEBOMFunctionalTypeNormalizer
    {
        private readonly ICEBOMFunctionalTypeConfig _config;

        public ICEBOMFunctionalTypeNormalizer(ICEBOMFunctionalTypeConfig config)
        {
            _config = config;
        }

        public ICEBOMFunctionalTypeEnum Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ICEBOMFunctionalTypeEnum.Unknown;

            var key = value.Trim().ToLowerInvariant();

            if (_config.Aliases.TryGetValue(key, out var normalizedValue))
            {
                if (Enum.TryParse<ICEBOMFunctionalTypeEnum>(
                    normalizedValue,
                    ignoreCase: true,
                    out var aliasResult))
                {
                    return aliasResult;
                }
            }

            if (Enum.TryParse<ICEBOMFunctionalTypeEnum>(
                value,
                ignoreCase: true,
                out var directResult))
            {
                return directResult;
            }

            return ICEBOMFunctionalTypeEnum.Unknown;
        }
    }
}