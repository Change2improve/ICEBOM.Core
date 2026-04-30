using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Normalizers;
using ICEBOM.Core.Domain.Repositories;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMValidator
    {
        private readonly FakeOdooRepository _odooRepository;

        public ICEBOMValidator(FakeOdooRepository odooRepository)
        {
            _odooRepository = odooRepository;
        }

        public ICEBOMComponentResult ValidateComponent(ICEBOMComponent component, ICEBOMSettingsSnapshot settings)
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

            if (component.Classification.FunctionalType == ICEBOMFunctionalTypeEnum.Unknown)
                result.Errors.Add(CreateError("COMPONENT_MISSING_FUNCTIONAL_TYPE", $"El componente '{component.InternalId}' no tiene tipo funcional."));

            result.Status = result.Errors.Count > 0 ? "blocked" : "ready";
            result.Action = ICEBOMActionEnum.ValidationOnly;

            return result;
        }

        public ICEBOMBomResult ValidateBom(ICEBOMBom bom, ICEBOMSettingsSnapshot settings)
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

            result.Status = result.Errors.Count > 0 ? "blocked" : "ready";
            result.Action = ICEBOMActionEnum.ValidationOnly;

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

        public void ValidateBusinessRules(ICEBOMRequest request, ICEBOMResponse response)
        {
            var bomProducts = request.Boms
                .Select(b => b.ProductReference)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToHashSet();

            var bomLineReferences = request.Boms
                .SelectMany(b => b.Lines)
                .Select(l => l.ComponentReference)
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

                if (functionalType == ICEBOMFunctionalTypeEnum.Commercial && hasBom)
                {
                    comp.Errors.Add(CreateError(
                        "INVALID_COMMERCIAL_WITH_BOM",
                        $"El componente '{comp.Reference}' es comercial pero tiene BOM."));
                }

                if (functionalType == ICEBOMFunctionalTypeEnum.Manufactured && !hasBom)
                {
                    comp.Errors.Add(CreateError(
                        "MANUFACTURED_WITHOUT_BOM",
                        $"El componente '{comp.Reference}' es fabricado pero no tiene BOM."));
                }

                if (requestComp.Control.IgnoreChildren && hasBom)
                {
                    comp.Warnings.Add(new ICEBOMWarning
                    {
                        Code = "IGNORE_CHILDREN_WITH_BOM",
                        Message = $"El componente '{comp.Reference}' tiene IgnorarHijos=true, pero se ha recibido una BOM para él. La BOM será omitida."
                    });
                }

                if (!requestComp.Control.ExportErp && hasBom)
                {
                    comp.Errors.Add(CreateError(
                        "NON_EXPORTABLE_COMPONENT_WITH_BOM",
                        $"El componente '{comp.Reference}' tiene ExportarERP=false, pero se ha recibido una BOM para él."));
                }

                if (!requestComp.Control.ExportErp && bomLineReferences.Contains(comp.Reference))
                {
                    comp.Errors.Add(CreateError(
                        "NON_EXPORTABLE_COMPONENT_USED_IN_BOM",
                        $"El componente '{comp.Reference}' tiene ExportarERP=false, pero se usa como línea de BOM."));
                }
            }
        }

        public void ValidateBomLines(ICEBOMRequest request, ICEBOMResponse response)
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
                    var originalUnit = line.Unit;
                    var normalizedUnit = ICEBOMUnitNormalizer.Normalize(line.Unit);

                    var lineResult = new ICEBOMBomLineResult
                    {
                        ComponentInternalId = line.ComponentInternalId,
                        ComponentReference = line.ComponentReference,
                        Quantity = line.Quantity,
                        OriginalUnit = originalUnit,
                        NormalizedUnit = normalizedUnit
                    };

                    if (string.IsNullOrWhiteSpace(line.Unit))
                    {
                        var warning = new ICEBOMWarning
                        {
                            Code = "BOM_LINE_MISSING_UNIT",
                            Message = $"La BOM '{requestBom.BomId}' contiene una línea sin unidad. Se usará la unidad por defecto 'Ud'."
                        };

                        bomResult.Warnings.Add(warning);
                        lineResult.Warnings.Add(warning);
                    }
                    else if (!ICEBOMUnitNormalizer.IsKnown(line.Unit))
                    {
                        var warning = new ICEBOMWarning
                        {
                            Code = "BOM_LINE_UNKNOWN_UNIT",
                            Message = $"La BOM '{requestBom.BomId}' contiene una unidad no reconocida: '{line.Unit}'."
                        };

                        bomResult.Warnings.Add(warning);
                        lineResult.Warnings.Add(warning);
                    }

                    line.Unit = normalizedUnit;

                    if (string.IsNullOrWhiteSpace(line.ComponentInternalId))
                    {
                        var error = CreateError(
                            "BOM_LINE_MISSING_COMPONENT_INTERNAL_ID",
                            $"La BOM '{requestBom.BomId}' tiene una línea sin identificador interno de componente.");

                        bomResult.Errors.Add(error);
                        lineResult.Errors.Add(error);
                    }

                    if (string.IsNullOrWhiteSpace(line.ComponentReference))
                    {
                        var error = CreateError(
                            "BOM_LINE_MISSING_COMPONENT_REFERENCE",
                            $"La BOM '{requestBom.BomId}' tiene una línea sin referencia de componente.");

                        bomResult.Errors.Add(error);
                        lineResult.Errors.Add(error);
                    }

                    if (!string.IsNullOrWhiteSpace(line.ComponentInternalId) &&
                        !componentIds.Contains(line.ComponentInternalId))
                    {
                        var error = CreateError(
                            "BOM_LINE_COMPONENT_ID_NOT_FOUND",
                            $"La BOM '{requestBom.BomId}' contiene el componente '{line.ComponentInternalId}', pero no existe en el catálogo.");

                        bomResult.Errors.Add(error);
                        lineResult.Errors.Add(error);
                    }

                    if (!string.IsNullOrWhiteSpace(line.ComponentReference) &&
                        !componentReferences.Contains(line.ComponentReference))
                    {
                        var error = CreateError(
                            "BOM_LINE_COMPONENT_REFERENCE_NOT_FOUND",
                            $"La BOM '{requestBom.BomId}' contiene la referencia '{line.ComponentReference}', pero no existe en el catálogo.");

                        bomResult.Errors.Add(error);
                        lineResult.Errors.Add(error);
                    }

                    if (line.Quantity <= 0)
                    {
                        var error = CreateError(
                            "BOM_LINE_INVALID_QUANTITY",
                            $"La BOM '{requestBom.BomId}' contiene una línea con cantidad inválida: {line.Quantity}.");

                        bomResult.Errors.Add(error);
                        lineResult.Errors.Add(error);
                    }

                    lineResult.Status = lineResult.Errors.Count > 0
                        ? "blocked"
                        : lineResult.Warnings.Count > 0
                            ? "warning"
                            : "ready";

                    bomResult.Lines.Add(lineResult);
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
