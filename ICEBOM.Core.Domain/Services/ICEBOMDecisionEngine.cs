using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Repositories;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMDecisionEngine
    {
        private readonly FakeOdooRepository _odooRepository;

        private readonly ICEBOMTraceService _traceService;

        public ICEBOMDecisionEngine(FakeOdooRepository odooRepository, ICEBOMTraceService traceService)
        {
            _odooRepository = odooRepository;
            _traceService = traceService;
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

            var exists = _odooRepository.ProductExists(component.Reference);

            if (exists && settings.UpdateExistingProducts)
            {
                result.Action = ICEBOMActionEnum.Update;

                _traceService.Add(
                    "DecisionComponent",
                    "Component",
                    component.Reference,
                    "Producto existe en Odoo y la configuración permite actualizar → Update.");
            }
            else if (exists && !settings.UpdateExistingProducts)
            {
                result.Action = ICEBOMActionEnum.Skip;

                _traceService.Add(
                    "DecisionComponent",
                    "Component",
                    component.Reference,
                    "Producto existe en Odoo pero la configuración no permite actualizar → Skip.");
            }
            else if (!exists && settings.CreateMissingProducts)
            {
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

            result.Status = "ready";
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

            var exists = _odooRepository.BomExists(bom.ProductReference);

            if (exists && settings.UpdateExistingBoms)
            {
                result.Action = ICEBOMActionEnum.Update;

                _traceService.Add(
                    "DecisionBom",
                    "BOM",
                    bom.BomId,
                    "La BOM existe en Odoo y la configuración permite actualizar → Update.");
            }
            else if (exists && !settings.UpdateExistingBoms)
            {
                result.Action = ICEBOMActionEnum.Skip;

                _traceService.Add(
                    "DecisionBom",
                    "BOM",
                    bom.BomId,
                    "La BOM existe en Odoo pero la configuración no permite actualizar → Skip.");
            }
            else if (!exists && settings.CreateMissingBoms)
            {
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