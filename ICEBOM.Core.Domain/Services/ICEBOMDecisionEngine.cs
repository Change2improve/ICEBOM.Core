using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Repositories;
using ICEBOM.Core.Domain.Normalizers;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMDecisionEngine
    {
        private readonly IOdooRepository _odooRepository;

        private readonly ICEBOMTraceService _traceService;

        private readonly ICEBOMUnitNormalizer _unitNormalizer;

        public ICEBOMDecisionEngine(IOdooRepository odooRepository, ICEBOMTraceService traceService, ICEBOMUnitNormalizer unitNormalizer)
        {
            _odooRepository = odooRepository;
            _traceService = traceService;
            _unitNormalizer = unitNormalizer;
        }

        public void DecideComponent(ICEBOMComponent component, ICEBOMComponentResult result, ICEBOMSettingsSnapshot settings)
        {
            if (result.Errors.Count > 0)
            {
                result.Status = "blocked";
                result.Action = ICEBOMActionEnum.Blocked;

                _traceService.Add(
                    "DecisionComponent",
                    "Component",
                    component.Reference,
                    "El componente tiene errores de validación → Blocked.");

                return;
            }

            if (!component.Control.ExportErp)
            {
                result.Status = "ready";
                result.Action = ICEBOMActionEnum.Skip;

                _traceService.Add(
                    "DecisionComponent",
                    "Component",
                    component.Reference,
                    "ExportErp=false → Skip.");

                return;
            }

            var product = _odooRepository.GetProduct(component.Reference);
            result.OdooProductId = product.Id;
            result.OdooProductName = product.Name;

            var category = _odooRepository.GetCategory(component.Classification.Category);
            result.OdooCategoryId = category.Id;
            result.Category = category.Name;

            var normalizedUnit = _unitNormalizer.Normalize(component.Classification.Unit);
            var unit = _odooRepository.GetUnit(normalizedUnit);
            result.OdooUnitId = unit.Id;

            if (string.IsNullOrWhiteSpace(component.Classification.Unit))
            {
                _traceService.Add(
                    "ResolveProductUnit",
                    "Component",
                    component.Reference,
                    $"El componente no tiene unidad base definida. Se usará '{normalizedUnit}'.");
            }
            else if (unit.Exists)
            {
                _traceService.Add(
                    "ResolveProductUnit",
                    "Component",
                    component.Reference,
                    $"Unidad base '{unit.Name}' encontrada en Odoo simulado. Id={unit.Id}.");
            }
            else
            {
                _traceService.Add(
                    "ResolveProductUnit",
                    "Component",
                    component.Reference,
                    $"Unidad base '{unit.Name}' no encontrada en Odoo simulado.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedUnit) && !unit.Exists)
            {
                result.Warnings.Add(new ICEBOMWarning
                {
                    Code = "ODOO_PRODUCT_UNIT_NOT_FOUND",
                    Message = $"La unidad base '{normalizedUnit}' no existe en Odoo simulado."
                });
            }

            if (string.IsNullOrWhiteSpace(component.Classification.Category))
            {
                _traceService.Add(
                    "ResolveCategory",
                    "Component",
                    component.Reference,
                    "El componente no tiene categoría definida.");
            }
            else if (category.Exists)
            {
                _traceService.Add(
                    "ResolveCategory",
                    "Component",
                    component.Reference,
                    $"Categoría '{category.Name}' encontrada en Odoo simulado. Id={category.Id}.");
            }
            else
            {
                _traceService.Add(
                    "ResolveCategory",
                    "Component",
                    component.Reference,
                    $"Categoría '{category.Name}' no encontrada en Odoo simulado.");
            }

            if (!string.IsNullOrWhiteSpace(component.Classification.Category) && !category.Exists)
            {
                result.Warnings.Add(new ICEBOMWarning
                {
                    Code = "ODOO_CATEGORY_NOT_FOUND",
                    Message = $"La categoría '{component.Classification.Category}' no existe en Odoo simulado."
                });
            }

            if (product.Exists && settings.UpdateExistingProducts)
            {
                var updatedProduct = _odooRepository.UpdateProduct(component.Reference);
                result.OdooProductId = updatedProduct.Id;
                result.OdooProductName = updatedProduct.Name;
                result.Action = ICEBOMActionEnum.Update;

                _traceService.Add(
                    "DecisionComponent",
                    "Component",
                    component.Reference,
                    $"Producto existe en Odoo (Id={product.Id}) y la configuración permite actualizar → Update.");
            }
            else if (product.Exists && !settings.UpdateExistingProducts)
            {
                result.Action = ICEBOMActionEnum.Skip;

                _traceService.Add(
                    "DecisionComponent",
                    "Component",
                    component.Reference,
                    $"Producto existe en Odoo (Id={product.Id}) pero la configuración no permite actualizar → Skip.");
            }
            else if (!product.Exists && settings.CreateMissingProducts)
            {
                var createdProduct = _odooRepository.CreateProduct(component.Reference);
                result.OdooProductId = createdProduct.Id;
                result.OdooProductName = createdProduct.Name;
                result.Action = ICEBOMActionEnum.Create;

                _traceService.Add(
                    "DecisionComponent",
                    "Component",
                    component.Reference,
                    "Producto no existe en Odoo y la configuración permite crearlo → Create.");
            }
            else
            {
                result.Action = ICEBOMActionEnum.Skip;

                _traceService.Add(
                    "DecisionComponent",
                    "Component",
                    component.Reference,
                    "Producto no existe en Odoo y la configuración no permite crearlo → Skip.");
            }

            result.Status = result.Warnings.Count > 0 ? "warning" : "ready";
        }

        public void DecideBom(ICEBOMRequest request, ICEBOMBom bom, ICEBOMBomResult result, ICEBOMSettingsSnapshot settings)
        {
            if (result.Errors.Count > 0)
            {
                result.Status = "blocked";
                result.Action = ICEBOMActionEnum.Blocked;

                _traceService.Add(
                    "DecisionBom",
                    "BOM",
                    bom.BomId,
                    "La BOM tiene errores de validación → Blocked.");

                return;
            }

            var parentComponent = request.Components
                .FirstOrDefault(c => c.InternalId == bom.SourceComponentId);

            if (parentComponent?.Control.IgnoreChildren == true)
            {
                result.Status = "ready";
                result.Action = ICEBOMActionEnum.Skip;

                _traceService.Add(
                    "DecisionBom",
                    "BOM",
                    bom.BomId,
                    "El componente padre tiene IgnoreChildren=true → Skip.");

                return;
            }

            var odooBom = _odooRepository.GetBom(bom.ProductReference);
            result.OdooBomId = odooBom.Id;

            if (odooBom.Exists && settings.UpdateExistingBoms)
            {
                var updatedBom = _odooRepository.UpdateBom(bom.ProductReference);
                result.OdooBomId = updatedBom.Id;
                result.Action = ICEBOMActionEnum.Update;

                _traceService.Add(
                    "DecisionBom",
                    "BOM",
                    bom.BomId,
                    $"La BOM existe en Odoo (Id={odooBom.Id}) y la configuración permite actualizar → Update.");
            }
            else if (odooBom.Exists && !settings.UpdateExistingBoms)
            {
                result.Action = ICEBOMActionEnum.Skip;

                _traceService.Add(
                    "DecisionBom",
                    "BOM",
                    bom.BomId,
                    $"La BOM existe en Odoo (Id={odooBom.Id}) pero la configuración no permite actualizar → Skip.");
            }
            else if (!odooBom.Exists && settings.CreateMissingBoms)
            {
                var createdBom = _odooRepository.CreateBom(bom.ProductReference);
                result.OdooBomId = createdBom.Id;
                result.Action = ICEBOMActionEnum.Create;

                _traceService.Add(
                    "DecisionBom",
                    "BOM",
                    bom.BomId,
                    "La BOM no existe en Odoo y la configuración permite crearla → Create.");
            }
            else
            {
                result.Action = ICEBOMActionEnum.Skip;

                _traceService.Add(
                    "DecisionBom",
                    "BOM",
                    bom.BomId,
                    "La BOM no existe en Odoo y la configuración no permite crearla → Skip.");
            }

            result.Status = "ready";
        }
    }
}