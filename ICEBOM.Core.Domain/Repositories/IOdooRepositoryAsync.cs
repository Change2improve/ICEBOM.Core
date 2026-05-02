using ICEBOM.Core.Domain.Models.Odoo;

namespace ICEBOM.Core.Domain.Repositories
{
    public interface IOdooRepositoryAsync
    {
        Task<OdooProductInfo> GetProductAsync(string reference, CancellationToken cancellationToken = default);

        Task<OdooBomInfo> GetBomAsync(string productReference, CancellationToken cancellationToken = default);

        Task<OdooUnitInfo> GetUnitAsync(string unitName, CancellationToken cancellationToken = default);

        Task<OdooCategoryInfo> GetCategoryAsync(string categoryName, CancellationToken cancellationToken = default);

        Task<OdooProductInfo> CreateProductAsync(string reference, CancellationToken cancellationToken = default);

        Task<OdooProductInfo> CreateProductAsync(OdooProductWriteRequest request, CancellationToken cancellationToken = default);

        Task<OdooProductInfo> UpdateProductAsync(string reference, CancellationToken cancellationToken = default);

        Task<OdooProductInfo> UpdateProductAsync(OdooProductWriteRequest request, CancellationToken cancellationToken = default);

        Task<OdooBomInfo> CreateBomAsync(string productReference, CancellationToken cancellationToken = default);

        Task<OdooBomInfo> CreateBomAsync(OdooBomWriteRequest request, CancellationToken cancellationToken = default);

        Task<OdooBomInfo> UpdateBomAsync(string productReference, CancellationToken cancellationToken = default);

        Task<OdooBomInfo> UpdateBomAsync(OdooBomWriteRequest request, CancellationToken cancellationToken = default);

        Task<int> DeleteBomLinesAsync(int bomId, CancellationToken cancellationToken = default);

        Task<int> CreateBomLineAsync(OdooBomLineWriteRequest request, CancellationToken cancellationToken = default);

        Task<int> ReplaceBomLinesAsync(int bomId, IReadOnlyCollection<OdooBomLineWriteRequest> lines, CancellationToken cancellationToken = default);

        Task<int> SyncBomLinesAsync(string productReference, IReadOnlyCollection<OdooBomLineWriteRequest> lines, CancellationToken cancellationToken = default);
    }
}