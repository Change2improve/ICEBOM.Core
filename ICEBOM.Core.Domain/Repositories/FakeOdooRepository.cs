using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.Domain.Repositories
{
    public class FakeOdooRepository
    {
        private readonly HashSet<string> _existingProducts;
        private readonly HashSet<string> _existingBoms;

        public FakeOdooRepository(ICEBOMFakeOdooConfig config)
        {
            _existingProducts = config.ExistingProducts.ToHashSet();
            _existingBoms = config.ExistingBoms.ToHashSet();
        }

        public bool ProductExists(string reference)
        {
            return _existingProducts.Contains(reference);
        }

        public bool BomExists(string productReference)
        {
            return _existingBoms.Contains(productReference);
        }
    }
}