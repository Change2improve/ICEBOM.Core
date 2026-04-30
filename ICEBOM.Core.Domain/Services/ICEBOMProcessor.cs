using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Repositories;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMProcessor
    {
        private readonly FakeOdooRepository _odooRepository;

        public ICEBOMProcessor()
        {
            _odooRepository = new FakeOdooRepository();
        }

        public ICEBOMResponse Process(ICEBOMRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var response = new ICEBOMResponse
            {
                Meta =
                {
                    RequestExportId = request.Meta.ExportId,
                    ProcessDate = DateTime.Now,
                    CoreVersion = typeof(ICEBOMProcessor).Assembly.GetName().Version?.ToString() ?? "unknown"
                },
                Summary =
                {
                    TotalComponents = request.Components.Count,
                    TotalBoms = request.Boms.Count
                }
            };

            foreach (var component in request.Components)
                response.Components.Add(ValidateComponent(component, request.SettingsSnapshot));

            foreach (var bom in request.Boms)
                response.Boms.Add(ValidateBom(bom, request.SettingsSnapshot));

            ValidateBusinessRules(request, response);
            ValidateBomLines(request, response);

            CalculateSummary(response);

            return response;
        }

        private ICEBOMComponentResult ValidateComponent(ICEBOMComponent component, ICEBOMSettingsSnapshot settings)
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
                var existsInOdoo = _odooRepository.ProductExists(component.Reference);

                if (existsInOdoo && settings.UpdateExistingProducts)
                {
                    result.Status = "ready";
                    result.Action = ICEBOMActionEnum.Update;
                }
                else if (existsInOdoo && !settings.UpdateExistingProducts)
                {
                    result.Status = "ready";
                    result.Action = ICEBOMActionEnum.Skip;
                }
                else if (!existsInOdoo && settings.CreateMissingProducts)
                {
                    result.Status = "ready";
                    result.Action = ICEBOMActionEnum.Create;
                }
                else
                {
                    result.Status = "ready";
                    result.Action = ICEBOMActionEnum.Skip;
                }
            }

            return result;
        }

        private ICEBOMBomResult ValidateBom(ICEBOMBom bom, ICEBOMSettingsSnapshot settings)
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
                var existsInOdoo = _odooRepository.BomExists(bom.ProductReference);

                if (existsInOdoo && settings.UpdateExistingBoms)
                {
                    result.Status = "ready";
                    result.Action = ICEBOMActionEnum.Update;
                }
                else if (existsInOdoo && !settings.UpdateExistingBoms)
                {
                    result.Status = "ready";
                    result.Action = ICEBOMActionEnum.Skip;
                }
                else if (!existsInOdoo && settings.CreateMissingBoms)
                {
                    result.Status = "ready";
                    result.Action = ICEBOMActionEnum.Create;
                }
                else
                {
                    result.Status = "ready";
                    result.Action = ICEBOMActionEnum.Skip;
                }
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

        private void ValidateBusinessRules(ICEBOMRequest request, ICEBOMResponse response)
        {
            var bomProducts = request.Boms
                .Select(b => b.ProductReference)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToHashSet();

            foreach (var comp in response.Components)
            {
                var requestComp = request.Components
                    .FirstOrDefault(c => c.Reference == comp.Reference);

                if (requestComp == null)
                    continue;

                var functionalType = requestComp.Classification.FunctionalType;

                var hasBom = bomProducts.Contains(comp.Reference);

                // 🔴 Regla 1: Commercial no puede tener BOM
                if (functionalType == "commercial" && hasBom)
                {
                    comp.Errors.Add(CreateError(
                        "INVALID_COMMERCIAL_WITH_BOM",
                        $"El componente '{comp.Reference}' es comercial pero tiene BOM."));
                }

                // 🔴 Regla 2: Manufactured debe tener BOM
                if (functionalType == "manufactured" && !hasBom)
                {
                    comp.Errors.Add(CreateError(
                        "MANUFACTURED_WITHOUT_BOM",
                        $"El componente '{comp.Reference}' es fabricado pero no tiene BOM."));
                }
            }
        }

        private void ValidateBomLines(
    ICEBOMRequest request,
    ICEBOMResponse response)
        {
            var componentIds = request.Components
                .Select(c => c.InternalId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();

            var componentReferences = request.Components
                .Select(c => c.Reference)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToHashSet();

            foreach (var bomResult in response.Boms)
            {
                var requestBom = request.Boms
                    .FirstOrDefault(b => b.BomId == bomResult.BomId);

                if (requestBom == null || requestBom.Lines == null)
                    continue;

                foreach (var line in requestBom.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line.ComponentInternalId))
                    {
                        bomResult.Errors.Add(CreateError(
                            "BOM_LINE_MISSING_COMPONENT_INTERNAL_ID",
                            $"La BOM '{requestBom.BomId}' tiene una línea sin identificador interno de componente."));
                    }

                    if (string.IsNullOrWhiteSpace(line.ComponentReference))
                    {
                        bomResult.Errors.Add(CreateError(
                            "BOM_LINE_MISSING_COMPONENT_REFERENCE",
                            $"La BOM '{requestBom.BomId}' tiene una línea sin referencia de componente."));
                    }

                    if (!string.IsNullOrWhiteSpace(line.ComponentInternalId) &&
                        !componentIds.Contains(line.ComponentInternalId))
                    {
                        bomResult.Errors.Add(CreateError(
                            "BOM_LINE_COMPONENT_ID_NOT_FOUND",
                            $"La BOM '{requestBom.BomId}' contiene el componente '{line.ComponentInternalId}', pero no existe en el catálogo."));
                    }

                    if (!string.IsNullOrWhiteSpace(line.ComponentReference) &&
                        !componentReferences.Contains(line.ComponentReference))
                    {
                        bomResult.Errors.Add(CreateError(
                            "BOM_LINE_COMPONENT_REFERENCE_NOT_FOUND",
                            $"La BOM '{requestBom.BomId}' contiene la referencia '{line.ComponentReference}', pero no existe en el catálogo."));
                    }

                    if (line.Quantity <= 0)
                    {
                        bomResult.Errors.Add(CreateError(
                            "BOM_LINE_INVALID_QUANTITY",
                            $"La BOM '{requestBom.BomId}' contiene una línea con cantidad inválida: {line.Quantity}."));
                    }

                    if (string.IsNullOrWhiteSpace(line.Unit))
                    {
                        bomResult.Warnings.Add(new ICEBOMWarning
                        {
                            Code = "BOM_LINE_MISSING_UNIT",
                            Message = $"La BOM '{requestBom.BomId}' contiene una línea sin unidad. Se usará la unidad por defecto."
                        });
                    }
                }

                if (bomResult.Errors.Count > 0)
                {
                    bomResult.Status = "blocked";
                    bomResult.Action = ICEBOMActionEnum.Blocked;
                }
                else if (bomResult.Warnings.Count > 0 && bomResult.Status == "ready")
                {
                    bomResult.Status = "warning";
                }
            }
        }
    }
}