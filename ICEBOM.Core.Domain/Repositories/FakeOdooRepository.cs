using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Models.Odoo;

namespace ICEBOM.Core.Domain.Repositories
{
    public class FakeOdooRepository : IOdooRepository
    {
        private readonly HashSet<string> _existingProducts;
        private readonly HashSet<string> _existingBoms;

        private readonly HashSet<string> _existingCategories;

        private readonly HashSet<string> _existingUnits;

        public FakeOdooRepository(ICEBOMFakeOdooConfig config)
        {
            _existingProducts = config.ExistingProducts.ToHashSet();
            _existingBoms = config.ExistingBoms.ToHashSet();

            _existingCategories = config.ExistingCategories.ToHashSet();

            _existingUnits = config.ExistingUnits.ToHashSet();
        }

        public OdooProductInfo GetProduct(string reference)
        {
            if (_existingProducts.Contains(reference))
            {
                return new OdooProductInfo
                {
                    Id = GenerateFakeId(reference),
                    Reference = reference,
                    Name = $"Producto {reference}"
                };
            }

            return new OdooProductInfo
            {
                Id = 0,
                Reference = reference
            };
        }

        public OdooBomInfo GetBom(string productReference)
        {
            if (_existingBoms.Contains(productReference))
            {
                return new OdooBomInfo
                {
                    Id = GenerateFakeId($"BOM-{productReference}"),
                    ProductReference = productReference
                };
            }

            return new OdooBomInfo
            {
                Id = 0,
                ProductReference = productReference
            };
        }

        public OdooProductInfo CreateProduct(string reference)
        {
            var id = GenerateFakeId(reference);

            _existingProducts.Add(reference);

            return new OdooProductInfo
            {
                Id = id,
                Reference = reference,
                Name = $"Producto {reference}"
            };
        }

        public OdooProductInfo UpdateProduct(string reference)
        {
            var existing = GetProduct(reference);

            if (existing.Exists)
                return existing;

            return CreateProduct(reference);
        }

        public OdooBomInfo CreateBom(string productReference)
        {
            var id = GenerateFakeId($"BOM-{productReference}");

            _existingBoms.Add(productReference);

            return new OdooBomInfo
            {
                Id = id,
                ProductReference = productReference
            };
        }

        public OdooBomInfo UpdateBom(string productReference)
        {
            var existing = GetBom(productReference);

            if (existing.Exists)
                return existing;

            return CreateBom(productReference);
        }

        private static int GenerateFakeId(string value)
        {
            return Math.Abs(value.GetHashCode());
        }

        public OdooUnitInfo GetUnit(string unitName)
        {
            if (string.IsNullOrWhiteSpace(unitName))
            {
                return new OdooUnitInfo
                {
                    Id = 0,
                    Name = string.Empty
                };
            }

            var normalized = unitName.Trim();

            if (!_existingUnits.Contains(normalized))
            {
                return new OdooUnitInfo
                {
                    Id = 0,
                    Name = normalized
                };
            }

            return new OdooUnitInfo
            {
                Id = GenerateFakeId($"UOM-{normalized}"),
                Name = normalized
            };
        }

        public OdooCategoryInfo GetCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return new OdooCategoryInfo
                {
                    Id = 0,
                    Name = string.Empty
                };
            }

            var normalized = categoryName.Trim();

            if (!_existingCategories.Contains(normalized))
            {
                return new OdooCategoryInfo
                {
                    Id = 0,
                    Name = normalized
                };
            }

            return new OdooCategoryInfo
            {
                Id = GenerateFakeId($"CATEGORY-{normalized}"),
                Name = normalized
            };
        }
    }
}