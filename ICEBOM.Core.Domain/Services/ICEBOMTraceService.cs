using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMTraceService
    {
        private readonly List<ICEBOMTraceEntry> _entries = new();

        public IReadOnlyList<ICEBOMTraceEntry> Entries => _entries;

        public void Add(string step, string entity, string reference, string message)
        {
            _entries.Add(new ICEBOMTraceEntry
            {
                Step = step,
                Entity = entity,
                Reference = reference,
                Message = message
            });
        }
    }
}