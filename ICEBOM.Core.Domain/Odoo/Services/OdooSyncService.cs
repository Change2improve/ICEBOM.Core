using ICEBOM.Core.Domain.Constants;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Models.Odoo;
using ICEBOM.Core.Domain.Repositories;
using ICEBOM.Core.Domain.Services;

namespace ICEBOM.Core.Domain.Odoo.Services
{
    public class OdooSyncService
    {
        private readonly IOdooRepositoryAsync _odooRepository;
        private readonly OdooExecutionOptions _executionOptions;
        private readonly ICEBOMTraceService _traceService;

        public OdooSyncService(IOdooRepositoryAsync odooRepository, OdooExecutionOptions executionOptions, ICEBOMTraceService traceService)
        {
            _odooRepository = odooRepository;
            _executionOptions = executionOptions;
            _traceService = traceService;
        }

        public async Task<OdooProductInfo> CreateProductAsync(OdooProductWriteRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reference))
                return new OdooProductInfo();

            if (_executionOptions.DryRun)
            {
                _traceService.Add(
                    "ExecuteCreateProductDryRun",
                    "Product",
                    request.Reference,
                    $"DRY-RUN: se habría creado el producto '{request.Reference}'.");

                return new OdooProductInfo
                {
                    Reference = request.Reference,
                    Name = request.Name,
                    CategoryId = request.CategoryId,
                    UnitId = request.UnitId
                };
            }

            _traceService.Add(
                "ExecuteCreateProduct",
                "Product",
                request.Reference,
                $"Creando producto '{request.Reference}' en Odoo.");

            var created = await _odooRepository.CreateProductAsync(request, cancellationToken);

            _traceService.Add(
                "ExecuteCreateProduct",
                "Product",
                request.Reference,
                created.Exists
                    ? $"Producto '{request.Reference}' creado en Odoo. Id={created.Id}."
                    : $"No se pudo confirmar la creación del producto '{request.Reference}'.");

            return created;
        }

        public async Task<OdooProductInfo> UpdateProductAsync(OdooProductWriteRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reference))
                return new OdooProductInfo();

            if (_executionOptions.DryRun)
            {
                _traceService.Add(
                    "ExecuteUpdateProductDryRun",
                    "Product",
                    request.Reference,
                    $"DRY-RUN: se habría actualizado el producto '{request.Reference}'.");

                return await _odooRepository.GetProductAsync(request.Reference, cancellationToken);
            }

            _traceService.Add(
                "ExecuteUpdateProduct",
                "Product",
                request.Reference,
                $"Actualizando producto '{request.Reference}' en Odoo.");

            var updated = await _odooRepository.UpdateProductAsync(request, cancellationToken);

            _traceService.Add(
                "ExecuteUpdateProduct",
                "Product",
                request.Reference,
                updated.Exists
                    ? $"Producto '{request.Reference}' actualizado en Odoo. Id={updated.Id}."
                    : $"No se pudo confirmar la actualización del producto '{request.Reference}'.");

            return updated;
        }

        public async Task<OdooBomInfo> CreateBomAsync(OdooBomWriteRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProductReference))
                return new OdooBomInfo();

            if (_executionOptions.DryRun)
            {
                _traceService.Add(
                    "ExecuteCreateBomDryRun",
                    "BOM",
                    request.ProductReference,
                    $"DRY-RUN: se habría creado la BOM del producto '{request.ProductReference}'.");

                return new OdooBomInfo
                {
                    ProductReference = request.ProductReference,
                    ProductId = request.ProductId,
                    ProductTemplateId = request.ProductTemplateId,
                    UnitId = request.UnitId,
                    ProductQuantity = request.ProductQuantity,
                    Code = request.Code,
                    Type = request.Type
                };
            }

            _traceService.Add(
                "ExecuteCreateBom",
                "BOM",
                request.ProductReference,
                $"Creando BOM del producto '{request.ProductReference}' en Odoo.");

            var created = await _odooRepository.CreateBomAsync(request, cancellationToken);

            _traceService.Add(
                "ExecuteCreateBom",
                "BOM",
                request.ProductReference,
                created.Exists
                    ? $"BOM del producto '{request.ProductReference}' creada en Odoo. Id={created.Id}."
                    : $"No se pudo confirmar la creación de la BOM del producto '{request.ProductReference}'.");

            return created;
        }

        public async Task<OdooBomInfo> UpdateBomAsync(OdooBomWriteRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProductReference))
                return new OdooBomInfo();

            if (_executionOptions.DryRun)
            {
                _traceService.Add(
                    "ExecuteUpdateBomDryRun",
                    "BOM",
                    request.ProductReference,
                    $"DRY-RUN: se habría actualizado la BOM del producto '{request.ProductReference}'.");

                return await _odooRepository.GetBomAsync(request.ProductReference, cancellationToken);
            }

            _traceService.Add(
                "ExecuteUpdateBom",
                "BOM",
                request.ProductReference,
                $"Actualizando BOM del producto '{request.ProductReference}' en Odoo.");

            var updated = await _odooRepository.UpdateBomAsync(request, cancellationToken);

            _traceService.Add(
                "ExecuteUpdateBom",
                "BOM",
                request.ProductReference,
                updated.Exists
                    ? $"BOM del producto '{request.ProductReference}' actualizada en Odoo. Id={updated.Id}."
                    : $"No se pudo confirmar la actualización de la BOM del producto '{request.ProductReference}'.");

            return updated;
        }

        public async Task<int> ReplaceBomLinesAsync(string productReference, IReadOnlyCollection<OdooBomLineWriteRequest> lines, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(productReference))
                return 0;

            var lineCount = lines?.Count ?? 0;

            if (_executionOptions.DryRun)
            {
                _traceService.Add(
                    "ExecuteReplaceBomLinesDryRun",
                    "BOM",
                    productReference,
                    $"DRY-RUN: se habrían sustituido {lineCount} líneas de la BOM del producto '{productReference}'.");

                return lineCount;
            }

            _traceService.Add(
                "ExecuteReplaceBomLines",
                "BOM",
                productReference,
                $"Sustituyendo {lineCount} líneas de la BOM del producto '{productReference}' en Odoo.");

            var syncedCount = await _odooRepository.SyncBomLinesAsync(
                productReference,
                lines,
                cancellationToken);

            _traceService.Add(
                "ExecuteReplaceBomLines",
                "BOM",
                productReference,
                $"Sustituidas {syncedCount} líneas de la BOM del producto '{productReference}' en Odoo.");

            return syncedCount;
        }

        public OdooProductWriteRequest BuildProductWriteRequest(ICEBOMComponent component, ICEBOMComponentResult result)
        {
            return new OdooProductWriteRequest
            {
                Reference = component.Reference,
                Name = component.Name,
                CategoryId = result.OdooCategoryId,
                UnitId = result.OdooUnitId,
                IsStorable = component.Classification.IsStorable,
                CanBePurchased = true,
                CanBeSold = component.Classification.IsSellable
            };
        }

        public OdooBomWriteRequest BuildBomWriteRequest(ICEBOMBom bom, ICEBOMBomResult result, OdooProductInfo parentProduct)
        {
            return new OdooBomWriteRequest
            {
                ProductReference = bom.ProductReference,
                ProductId = parentProduct.Id,
                ProductTemplateId = parentProduct.ProductTemplateId,
                UnitId = parentProduct.UnitId,
                ProductQuantity = 1m,
                Code = bom.BomId,
                Type = OdooMrpConstants.BomTypes.Normal
            };
        }

        public IReadOnlyCollection<OdooBomLineWriteRequest> BuildBomLineWriteRequests(ICEBOMBomResult bomResult, IReadOnlyDictionary<string, OdooProductInfo> productsByReference)
        {
            var lines = new List<OdooBomLineWriteRequest>();
            var sequence = 10;

            foreach (var line in bomResult.Lines)
            {
                if (!productsByReference.TryGetValue(line.ComponentReference, out var product))
                    continue;

                if (!product.Exists)
                    continue;

                lines.Add(new OdooBomLineWriteRequest
                {
                    ProductId = product.Id,
                    UnitId = product.UnitId,
                    Quantity = line.Quantity <= 0 ? 1m : (decimal)line.Quantity,
                    Sequence = sequence
                });

                sequence += 10;
            }

            return lines;
        }
    }
}