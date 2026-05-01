using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Normalizers;

namespace ICEBOM.Core.Domain.Services
{
    public static class ICEBOMSettingsMapper
    {
        public static void ApplyCustomerConfig(ICEBOMRequest request, ICEBOMCustomerConfig config)
        {
            request.SettingsSnapshot.CreateMissingProducts = config.SyncPolicy.CreateMissingProducts;
            request.SettingsSnapshot.UpdateExistingProducts = config.SyncPolicy.UpdateExistingProducts;
            request.SettingsSnapshot.CreateMissingBoms = config.SyncPolicy.CreateMissingBoms;
            request.SettingsSnapshot.UpdateExistingBoms = config.SyncPolicy.UpdateExistingBoms;
            request.SettingsSnapshot.AllowProductVariants = config.SyncPolicy.AllowProductVariants;
        }

        public static void ApplyFunctionalTypeAliases(ICEBOMRequest request, ICEBOMCustomerConfig config)
        {
            var normalizer = new ICEBOMFunctionalTypeNormalizer(config.FunctionalTypes);

            foreach (var component in request.Components)
            {
                if (component.Classification.FunctionalType != ICEBOMFunctionalTypeEnum.Unknown)
                    continue;

                component.Classification.FunctionalType =
                    normalizer.Normalize(component.Classification.FunctionalTypeRaw);
            }
        }

        public static void ApplyDefaultFunctionalType(ICEBOMRequest request, ICEBOMCustomerConfig config)
        {
            if (!Enum.TryParse<ICEBOMFunctionalTypeEnum>(
                    config.Defaults.DefaultFunctionalType,
                    ignoreCase: true,
                    out var defaultType))
            {
                defaultType = ICEBOMFunctionalTypeEnum.Unknown;
            }

            foreach (var component in request.Components)
            {
                if (component.Classification.FunctionalType == ICEBOMFunctionalTypeEnum.Unknown)
                {
                    component.Classification.FunctionalType = defaultType;
                }
            }
        }
    }
}