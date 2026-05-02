using ICEBOM.Core.Domain.Enums;

namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class ICEBOMOdooModeConfig
    {
        public ICEBOMOdooModeEnum Mode { get; set; } = ICEBOMOdooModeEnum.Fake;

        public bool DryRun { get; set; } = true;
    }
}
