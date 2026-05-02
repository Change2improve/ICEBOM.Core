namespace ICEBOM.Core.Domain.Constants
{
    public static class OdooProductConstants
    {
        public const string Product = "product.product";
        public const string ProductTemplate = "product.template";
        public const string Category = "product.category";
        public const string UnitOfMeasure = "uom.uom";

        public static class Fields
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string DisplayName = "display_name";
            public const string DefaultCode = "default_code";
            public const string ProductTemplateId = "product_tmpl_id";
            public const string CategoryId = "categ_id";
            public const string UnitId = "uom_id";
            public const string PurchaseUnitId = "uom_po_id";
            public const string Type = "type";
            public const string SaleOk = "sale_ok";
            public const string PurchaseOk = "purchase_ok";
            public const string IsStorable = "is_storable";
            public const string Active = "active";
        }

        public static class Types
        {
            public const string Consumable = "consu";
            public const string Service = "service";
            public const string Combo = "combo";
        }

        public static class CategoryFields
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string DisplayName = "display_name";
            public const string CompleteName = "complete_name";
        }

        public static class UnitFields
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string DisplayName = "display_name";
            public const string Active = "active";
            public const string Factor = "factor";
            public const string RelativeFactor = "relative_factor";
            public const string RelativeUnitId = "relative_uom_id";
        }
    }
}