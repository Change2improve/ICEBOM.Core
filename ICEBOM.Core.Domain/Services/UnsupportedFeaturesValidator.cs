using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.Domain.Services
{
    public class UnsupportedFeaturesValidator
    {
        private readonly ICEBOMUnsupportedFeaturesConfig _config;

        private readonly ICEBOMTraceService _traceService;

        public UnsupportedFeaturesValidator(ICEBOMUnsupportedFeaturesConfig config, ICEBOMTraceService traceService)
        {
            _config = config;
            _traceService = traceService;
        }

        public void ValidateComponent(ICEBOMComponent component, ICEBOMComponentResult result)
        {
            if (_config.WarnSupplierData &&
                (!string.IsNullOrWhiteSpace(component.Sync.Supplier) ||
                 !string.IsNullOrWhiteSpace(component.Sync.SupplierReference)))
            {
                result.Warnings.Add(new ICEBOMWarning
                {
                    Code = "SUPPLIER_NOT_SUPPORTED",
                    Message = $"El componente '{component.Reference}' contiene información de proveedor, pero no está soportado en ICEBOM V1."
                });

                _traceService.Add(
                    "UnsupportedFeature",
                    "Component",
                    component.Reference,
                    "Supplier detectado → no soportado en V1.");
            }

            if (_config.WarnRoutes &&
                !string.IsNullOrWhiteSpace(component.Sync.Route))
            {
                result.Warnings.Add(new ICEBOMWarning
                {
                    Code = "ROUTE_NOT_SUPPORTED",
                    Message = $"El componente '{component.Reference}' contiene ruta de fabricación, pero no está soportada en ICEBOM V1."
                });

                _traceService.Add(
                    "UnsupportedFeature",
                    "Component",
                    component.Reference,
                    $"Route '{component.Sync.Route}' detectada → no soportada en V1.");
            }

            if (_config.WarnManufacturingOperations &&
                !string.IsNullOrWhiteSpace(component.Sync.ManufacturingOperation))
            {
                result.Warnings.Add(new ICEBOMWarning
                {
                    Code = "MANUFACTURING_OPERATION_NOT_SUPPORTED",
                    Message = $"El componente '{component.Reference}' contiene operación de fabricación, pero no está soportada en ICEBOM V1."
                });

                _traceService.Add(
                    "UnsupportedFeature",
                    "Component",
                    component.Reference,
                    $"Operation '{component.Sync.ManufacturingOperation}' detectada → no soportada en V1.");
            }
        }
    }
}