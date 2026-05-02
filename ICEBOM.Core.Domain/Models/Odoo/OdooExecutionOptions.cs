namespace ICEBOM.Core.Domain.Models.Odoo
{
    public class OdooExecutionOptions
    {
        public bool DryRun { get; set; } = true;

        public bool AllowWrites => !DryRun;

        public string ModeLabel => DryRun ? "DryRun" : "Real";
    }
}