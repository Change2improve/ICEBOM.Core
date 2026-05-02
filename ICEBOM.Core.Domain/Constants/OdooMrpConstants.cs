namespace ICEBOM.Core.Domain.Constants
{
    public static class OdooMrpConstants
    {
        public const string Bom = "mrp.bom";
        public const string BomLine = "mrp.bom.line";

        public static class BomFields
        {
            public const string Id = "id";
            public const string ProductId = "product_id";
            public const string ProductTemplateId = "product_tmpl_id";
            public const string ProductQuantity = "product_qty";
            public const string ProductUnitId = "product_uom_id";
            public const string Code = "code";
            public const string Type = "type";
            public const string Active = "active";
            public const string BomLineIds = "bom_line_ids";
        }

        public static class BomLineFields
        {
            public const string BomId = "bom_id";
            public const string ProductId = "product_id";
            public const string ProductQuantity = "product_qty";
            public const string ProductUnitId = "product_uom_id";
            public const string Sequence = "sequence";
        }

        public static class BomTypes
        {
            public const string Normal = "normal";
            public const string Phantom = "phantom";
        }
    }
}