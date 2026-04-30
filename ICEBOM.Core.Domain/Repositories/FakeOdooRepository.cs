using System.Collections.Generic;

namespace ICEBOM.Core.Domain.Repositories
{
    public class FakeOdooRepository
    {
        private readonly HashSet<string> _existingProducts = new()
        {
            "P-001",
            "C-001"
        };

        private readonly HashSet<string> _existingBoms = new()
        {
            "P-001"
        };

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