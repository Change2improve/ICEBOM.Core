using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Repositories;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMDecisionEngine
    {
        private readonly FakeOdooRepository _odooRepository;

        public ICEBOMDecisionEngine(FakeOdooRepository odooRepository)
        {
            _odooRepository = odooRepository;
        }

        public void DecideComponent(
            ICEBOMComponent component,
            ICEBOMComponentResult result,
            ICEBOMSettingsSnapshot settings)
        {
            if (result.Errors.Count > 0)
            {
                result.Status = "blocked";
                result.Action = ICEBOMActionEnum.Blocked;
                return;
            }

            var exists = _odooRepository.ProductExists(component.Reference);

            if (exists && settings.UpdateExistingProducts)
            {
                result.Action = ICEBOMActionEnum.Update;
            }
            else if (exists && !settings.UpdateExistingProducts)
            {
                result.Action = ICEBOMActionEnum.Skip;
            }
            else if (!exists && settings.CreateMissingProducts)
            {
                result.Action = ICEBOMActionEnum.Create;
            }
            else
            {
                result.Action = ICEBOMActionEnum.Skip;
            }

            result.Status = "ready";
        }

        public void DecideBom(
            ICEBOMBom bom,
            ICEBOMBomResult result,
            ICEBOMSettingsSnapshot settings)
        {
            if (result.Errors.Count > 0)
            {
                result.Status = "blocked";
                result.Action = ICEBOMActionEnum.Blocked;
                return;
            }

            var exists = _odooRepository.BomExists(bom.ProductReference);

            if (exists && settings.UpdateExistingBoms)
            {
                result.Action = ICEBOMActionEnum.Update;
            }
            else if (exists && !settings.UpdateExistingBoms)
            {
                result.Action = ICEBOMActionEnum.Skip;
            }
            else if (!exists && settings.CreateMissingBoms)
            {
                result.Action = ICEBOMActionEnum.Create;
            }
            else
            {
                result.Action = ICEBOMActionEnum.Skip;
            }

            result.Status = "ready";
        }
    }
}