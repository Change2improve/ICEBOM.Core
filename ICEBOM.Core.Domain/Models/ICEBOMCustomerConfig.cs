namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMCustomerConfig
    {
        public ICEBOMFakeOdooConfig FakeOdoo { get; set; } = new();
        public string CustomerName { get; set; } = string.Empty;
        public string ConfigVersion { get; set; } = "1.0.0";

        public ICEBOMCustomerSyncPolicy SyncPolicy { get; set; } = new();
        public ICEBOMCustomerDefaults Defaults { get; set; } = new();
        public ICEBOMUnitConfig Units { get; set; } = new();

        public ICEBOMBusinessRulesConfig BusinessRules { get; set; } = new();

        public ICEBOMFunctionalTypeConfig FunctionalTypes { get; set; } = new();

        public ICEBOMExecutionConfig Execution { get; set; } = new();
    }
}