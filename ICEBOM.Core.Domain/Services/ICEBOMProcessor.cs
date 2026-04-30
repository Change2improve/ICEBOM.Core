using System;

using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMProcessor
    {
        public ICEBOMResponse Process(ICEBOMRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var response = new ICEBOMResponse
            {
                Meta =
                {
                    RequestExportId = request.Meta.ExportId,
                    ProcessDate = DateTime.Now
                },
                Summary =
                {
                    TotalComponents = request.Components.Count,
                    TotalBoms = request.Boms.Count
                }
            };

            foreach (var component in request.Components)
                response.Components.Add(ValidateComponent(component));

            foreach (var bom in request.Boms)
                response.Boms.Add(ValidateBom(bom));

            CalculateSummary(response);

            return response;
        }

        private static ICEBOMComponentResult ValidateComponent(ICEBOMComponent component)
        {
            var result = new ICEBOMComponentResult
            {
                InternalId = component.InternalId,
                Reference = component.Reference,
                Action = ICEBOMActionEnum.ValidationOnly
            };

            if (string.IsNullOrWhiteSpace(component.InternalId))
                result.Errors.Add(CreateError("COMPONENT_MISSING_INTERNAL_ID", "El componente no tiene identificador interno."));

            if (string.IsNullOrWhiteSpace(component.Reference))
                result.Errors.Add(CreateError("COMPONENT_MISSING_REFERENCE", $"El componente '{component.InternalId}' no tiene referencia."));

            if (string.IsNullOrWhiteSpace(component.Name))
                result.Errors.Add(CreateError("COMPONENT_MISSING_NAME", $"El componente '{component.InternalId}' no tiene nombre."));

            if (string.IsNullOrWhiteSpace(component.Classification.FunctionalType))
                result.Errors.Add(CreateError("COMPONENT_MISSING_FUNCTIONAL_TYPE", $"El componente '{component.InternalId}' no tiene tipo funcional."));

            if (result.Errors.Count > 0)
            {
                result.Status = "blocked";
                result.Action = ICEBOMActionEnum.Blocked;
            }
            else
            {
                result.Status = "ready";
                result.Action = ICEBOMActionEnum.Create;
            }

            return result;
        }

        private static ICEBOMBomResult ValidateBom(ICEBOMBom bom)
        {
            var result = new ICEBOMBomResult
            {
                BomId = bom.BomId,
                ProductReference = bom.ProductReference,
                Action = ICEBOMActionEnum.ValidationOnly
            };

            if (string.IsNullOrWhiteSpace(bom.BomId))
                result.Errors.Add(CreateError("BOM_MISSING_ID", "La lista de materiales no tiene identificador."));

            if (string.IsNullOrWhiteSpace(bom.ProductReference))
                result.Errors.Add(CreateError("BOM_MISSING_PRODUCT_REFERENCE", $"La BOM '{bom.BomId}' no tiene referencia de producto padre."));

            if (string.IsNullOrWhiteSpace(bom.SourceComponentId))
                result.Errors.Add(CreateError("BOM_MISSING_SOURCE_COMPONENT_ID", $"La BOM '{bom.BomId}' no tiene componente origen."));

            if (bom.Lines == null || bom.Lines.Count == 0)
                result.Errors.Add(CreateError("BOM_MISSING_LINES", $"La BOM '{bom.BomId}' no tiene líneas."));

            if (result.Errors.Count > 0)
            {
                result.Status = "blocked";
                result.Action = ICEBOMActionEnum.Blocked;
            }
            else
            {
                result.Status = "ready";
                result.Action = ICEBOMActionEnum.Create;
            }

            return result;
        }

        private static ICEBOMError CreateError(string code, string message)
        {
            return new ICEBOMError
            {
                Code = code,
                Message = message
            };
        }

        private static void CalculateSummary(ICEBOMResponse response)
        {
            response.Summary.ErrorsCount = 0;
            response.Summary.WarningsCount = 0;

            foreach (var component in response.Components)
            {
                response.Summary.ErrorsCount += component.Errors.Count;
                response.Summary.WarningsCount += component.Warnings.Count;
            }

            foreach (var bom in response.Boms)
            {
                response.Summary.ErrorsCount += bom.Errors.Count;
                response.Summary.WarningsCount += bom.Warnings.Count;
            }

            response.Summary.Status = response.Summary.ErrorsCount > 0
                ? "blocked"
                : response.Summary.WarningsCount > 0
                    ? "warning"
                    : "ready";
        }
    }
}