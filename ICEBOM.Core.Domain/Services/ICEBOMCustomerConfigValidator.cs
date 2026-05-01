using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMCustomerConfigValidator
    {
        public List<string> Validate(ICEBOMCustomerConfig config)
        {
            var errors = new List<string>();

            if (config == null)
            {
                errors.Add("La configuración del cliente no puede ser nula.");
                return errors;
            }

            if (config.BusinessRules == null)
                errors.Add("Falta la sección businessRules.");

            if (config.FunctionalTypes == null)
            {
                errors.Add("Falta la sección functionalTypes.");
            }
            else if (config.FunctionalTypes.Aliases == null)
            {
                errors.Add("La sección functionalTypes.aliases no puede ser nula.");
            }

            if (string.IsNullOrWhiteSpace(config.CustomerName))
                errors.Add("El nombre del cliente no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(config.ConfigVersion))
                errors.Add("La versión de configuración no puede estar vacía.");

            if (config.SyncPolicy == null)
            {
                errors.Add("Falta la sección syncPolicy.");
            }
            else if (config.SyncPolicy.AllowProductVariants)
            {
                errors.Add("ICEBOM V1 no soporta variantes de producto. El valor syncPolicy.allowProductVariants debe ser false.");
            }

            if (config.Defaults == null)
                errors.Add("Falta la sección defaults.");
            else if (string.IsNullOrWhiteSpace(config.Defaults.DefaultUnit))
                errors.Add("La unidad por defecto no puede estar vacía.");

            if (config.Units == null)
            {
                errors.Add("Falta la sección units.");
            }
            else
            {
                if (config.Units.KnownUnits == null || config.Units.KnownUnits.Count == 0)
                    errors.Add("Debe existir al menos una unidad conocida en units.knownUnits.");

                if (config.Units.Aliases == null)
                    errors.Add("La sección units.aliases no puede ser nula.");
            }

            if (config.FakeOdoo == null)
                errors.Add("Falta la sección fakeOdoo.");

            return errors;
        }
    }
}