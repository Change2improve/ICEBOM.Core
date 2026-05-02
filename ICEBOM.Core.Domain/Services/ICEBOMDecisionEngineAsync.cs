using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Models.Odoo;
using ICEBOM.Core.Domain.Normalizers;
using ICEBOM.Core.Domain.Repositories;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMDecisionEngineAsync
    {
        private readonly IOdooRepositoryAsync _odooRepository;
        private readonly ICEBOMTraceService _traceService;
        private readonly ICEBOMUnitNormalizer _unitNormalizer;

        public ICEBOMDecisionEngineAsync(
            IOdooRepositoryAsync odooRepository,
            ICEBOMTraceService traceService,
            ICEBOMUnitNormalizer unitNormalizer)
        {
            _odooRepository = odooRepository;
            _traceService = traceService;
            _unitNormalizer = unitNormalizer;
        }

        public async Task DecideComponentAsync(
            ICEBOMComponent component,
            ICEBOMComponentResult result,
            ICEBOMSettingsSnapshot settings,
            CancellationToken cancellationToken = default)
        {
            if (component == null || result == null)
                return;

            if (result.Errors.Count > 0)
            {
                result.Action = ICEBOMActionEnum.Blocked;
                result.Status = "blocked";
                return;
            }

            var unitName = _unitNormalizer.Normalize(
                string.IsNullOrWhiteSpace(component.Classification.Unit)
                    ? "Ud"
                    : component.Classification.Unit);

            var unit = await _odooRepository.GetUnitAsync(unitName, cancellationToken);

            result.OdooUnitId = unit.Id;

            var categoryName = component.Classification.Category;

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var category = await _odooRepository.GetCategoryAsync(categoryName, cancellationToken);
                result.OdooCategoryId = category.Id;
                result.Category = category.CompleteName;
            }

            var product = await _odooRepository.GetProductAsync(component.Reference, cancellationToken);

            if (product.Exists)
            {
                result.OdooProductId = product.Id;
                result.OdooProductName = product.Name;

                if (settings.UpdateExistingProducts)
                {
                    result.Action = ICEBOMActionEnum.Update;

                    _traceService.Add(
                        "DecisionComponentReal",
                        "Component",
                        component.Reference,
                        $"Producto existe en Odoo real (Id={product.Id}) y la configuración permite actualizar → Update.");
                }
                else
                {
                    result.Action = ICEBOMActionEnum.None;

                    _traceService.Add(
                        "DecisionComponentReal",
                        "Component",
                        component.Reference,
                        $"Producto existe en Odoo real (Id={product.Id}) pero la configuración no permite actualizar → None.");
                }
            }
            else
            {
                if (settings.CreateMissingProducts)
                {
                    result.Action = ICEBOMActionEnum.Create;

                    _traceService.Add(
                        "DecisionComponentReal",
                        "Component",
                        component.Reference,
                        "Producto no existe en Odoo real y la configuración permite crearlo → Create.");
                }
                else
                {
                    result.Action = ICEBOMActionEnum.Blocked;
                    result.Status = "blocked";

                    result.Errors.Add(new ICEBOMError
                    {
                        Code = "ODOO_PRODUCT_NOT_FOUND",
                        Message = $"El producto '{component.Reference}' no existe en Odoo y no está permitido crearlo."
                    });
                }
            }
        }

        public async Task DecideBomAsync(
            ICEBOMRequest request,
            ICEBOMBom bom,
            ICEBOMBomResult result,
            ICEBOMSettingsSnapshot settings,
            CancellationToken cancellationToken = default)
        {
            if (bom == null || result == null)
                return;

            if (result.Errors.Count > 0)
            {
                result.Action = ICEBOMActionEnum.Blocked;
                result.Status = "blocked";
                return;
            }

            var odooBom = await _odooRepository.GetBomAsync(bom.ProductReference, cancellationToken);

            if (odooBom.Exists)
            {
                result.OdooBomId = odooBom.Id;

                if (settings.UpdateExistingBoms)
                {
                    result.Action = ICEBOMActionEnum.Update;

                    _traceService.Add(
                        "DecisionBomReal",
                        "BOM",
                        bom.ProductReference,
                        $"La BOM existe en Odoo real (Id={odooBom.Id}) y la configuración permite actualizar → Update.");
                }
                else
                {
                    result.Action = ICEBOMActionEnum.None;

                    _traceService.Add(
                        "DecisionBomReal",
                        "BOM",
                        bom.ProductReference,
                        $"La BOM existe en Odoo real (Id={odooBom.Id}) pero la configuración no permite actualizar → None.");
                }
            }
            else
            {
                if (settings.CreateMissingBoms)
                {
                    result.Action = ICEBOMActionEnum.Create;

                    _traceService.Add(
                        "DecisionBomReal",
                        "BOM",
                        bom.ProductReference,
                        "La BOM no existe en Odoo real y la configuración permite crearla → Create.");
                }
                else
                {
                    result.Action = ICEBOMActionEnum.Blocked;
                    result.Status = "blocked";

                    result.Errors.Add(new ICEBOMError
                    {
                        Code = "ODOO_BOM_NOT_FOUND",
                        Message = $"La BOM del producto '{bom.ProductReference}' no existe en Odoo y no está permitido crearla."
                    });
                }
            }
        }
    }
}