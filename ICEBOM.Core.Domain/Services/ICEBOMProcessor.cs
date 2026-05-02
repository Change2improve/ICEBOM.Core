using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Models.Odoo;
using ICEBOM.Core.Domain.Normalizers;
using ICEBOM.Core.Domain.Odoo.Services;
using ICEBOM.Core.Domain.Repositories;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMProcessor
    {
        private readonly IOdooRepository _odooRepository;

        private readonly ICEBOMValidator _validator;

        private readonly ICEBOMDecisionEngine _decisionEngine;

        private readonly ICEBOMTraceService _traceService;

        private readonly IOdooRepositoryAsync? _odooRepositoryAsync;

        private readonly OdooSyncService? _odooSyncService;

        private readonly ICEBOMDecisionEngineAsync? _decisionEngineAsync;

        private bool HasAsyncOdooSync => _odooRepositoryAsync != null && _odooSyncService != null;

        public ICEBOMProcessor(IOdooRepository odooRepository, ICEBOMUnitNormalizer unitNormalizer, ICEBOMBusinessRulesConfig businessRules, ICEBOMUnsupportedFeaturesConfig unsupportedFeatures, ICEBOMTraceService traceService)
        {
            _odooRepository = odooRepository;
            _traceService = traceService ?? new ICEBOMTraceService();

            _validator = new ICEBOMValidator(
                _odooRepository,
                unitNormalizer,
                businessRules,
                unsupportedFeatures,
                _traceService);

            _decisionEngine = new ICEBOMDecisionEngine(
                _odooRepository,
                _traceService,
                unitNormalizer);
        }

        public ICEBOMProcessor(IOdooRepository odooRepository, ICEBOMUnitNormalizer unitNormalizer, ICEBOMBusinessRulesConfig businessRules, ICEBOMUnsupportedFeaturesConfig unsupportedFeatures) 
            : this(odooRepository, unitNormalizer, businessRules, unsupportedFeatures, new ICEBOMTraceService())
        {
        }

        public ICEBOMProcessor(IOdooRepository odooRepository, ICEBOMUnitNormalizer unitNormalizer, ICEBOMBusinessRulesConfig businessRules, ICEBOMUnsupportedFeaturesConfig unsupportedFeatures,
            IOdooRepositoryAsync odooRepositoryAsync, OdooSyncService odooSyncService, ICEBOMTraceService traceService) : this(odooRepository, unitNormalizer, businessRules, unsupportedFeatures, traceService)
        {
            _odooRepositoryAsync = odooRepositoryAsync ?? throw new ArgumentNullException(nameof(odooRepositoryAsync));
            _odooSyncService = odooSyncService ?? throw new ArgumentNullException(nameof(odooSyncService));

            _decisionEngineAsync = new ICEBOMDecisionEngineAsync(odooRepositoryAsync, traceService, unitNormalizer);
        }

        public ICEBOMResponse Process(ICEBOMRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var response = CreateBaseResponse(request);

            _traceService.Add("Start", "System", "", "Inicio del procesamiento");

            foreach (var component in request.Components)
                response.Components.Add(_validator.ValidateComponent(component, request.SettingsSnapshot));

            foreach (var bom in request.Boms)
                response.Boms.Add(_validator.ValidateBom(bom, request.SettingsSnapshot));

            _validator.ValidateBusinessRules(request, response);
            _validator.ValidateBomLines(request, response);

            for (int i = 0; i < request.Components.Count; i++)
            {
                _decisionEngine.DecideComponent(
                    request.Components[i],
                    response.Components[i],
                    request.SettingsSnapshot);
            }

            for (int i = 0; i < request.Boms.Count; i++)
            {
                _decisionEngine.DecideBom(request, request.Boms[i], response.Boms[i], request.SettingsSnapshot);
            }

            CalculateSummary(response);

            response.Trace.AddRange(_traceService.Entries);

            return response;
        }

        public async Task<ICEBOMResponse> ProcessAsync(ICEBOMRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var response = CreateBaseResponse(request);

            _traceService.Add("Start", "System", "", "Inicio del procesamiento async");

            foreach (var component in request.Components)
                response.Components.Add(_validator.ValidateComponent(component, request.SettingsSnapshot));

            foreach (var bom in request.Boms)
                response.Boms.Add(_validator.ValidateBom(bom, request.SettingsSnapshot));

            _validator.ValidateBusinessRules(request, response);
            _validator.ValidateBomLines(request, response);

            for (int i = 0; i < request.Components.Count; i++)
            {
                if (_decisionEngineAsync != null)
                {
                    await _decisionEngineAsync.DecideComponentAsync(
                        request.Components[i],
                        response.Components[i],
                        request.SettingsSnapshot,
                        cancellationToken);
                }
                else
                {
                    _decisionEngine.DecideComponent(
                        request.Components[i],
                        response.Components[i],
                        request.SettingsSnapshot);
                }
            }

            for (int i = 0; i < request.Boms.Count; i++)
            {
                if (_decisionEngineAsync != null)
                {
                    await _decisionEngineAsync.DecideBomAsync(
                        request,
                        request.Boms[i],
                        response.Boms[i],
                        request.SettingsSnapshot,
                        cancellationToken);
                }
                else
                {
                    _decisionEngine.DecideBom(
                        request,
                        request.Boms[i],
                        response.Boms[i],
                        request.SettingsSnapshot);
                }
            }

            if (HasAsyncOdooSync)
            {
                await SyncWithOdooAsync(request, response, cancellationToken);
            }

            CalculateSummary(response);

            response.Trace.AddRange(_traceService.Entries);

            return response;
        }

        private static void CalculateSummary(ICEBOMResponse response)
        {
            response.Summary.ErrorsCount = 0;
            response.Summary.WarningsCount = 0;

            foreach (var component in response.Components)
            {
                response.Summary.ErrorsCount += component.Errors.Count;
                response.Summary.WarningsCount += component.Warnings.Count;
            }

            foreach (var bom in response.Boms)
            {
                response.Summary.ErrorsCount += bom.Errors.Count;
                response.Summary.WarningsCount += bom.Warnings.Count;
            }

            response.Summary.Status = response.Summary.ErrorsCount > 0
                ? "blocked"
                : response.Summary.WarningsCount > 0
                    ? "warning"
                    : "ready";
        }

        private static ICEBOMResponse CreateBaseResponse(ICEBOMRequest request)
        {
            return new ICEBOMResponse
            {
                Meta =
                {
                    ExecutionId = Guid.NewGuid().ToString(),
                    RequestExportId = request.Meta.ExportId,
                    ProcessDate = DateTime.Now,
                    CoreVersion = typeof(ICEBOMProcessor)
                    .Assembly
                    .GetName()
                    .Version?
                    .ToString() ?? "unknown"
                },
                Summary =
                {
                    TotalComponents = request.Components.Count,
                    TotalBoms = request.Boms.Count
                }
            };
        }

        private async Task SyncWithOdooAsync(
    ICEBOMRequest request,
    ICEBOMResponse response,
    CancellationToken cancellationToken)
        {
            if (_odooRepositoryAsync == null || _odooSyncService == null)
                return;

            var productsByReference = new Dictionary<string, OdooProductInfo>(StringComparer.OrdinalIgnoreCase);

            // 1) Ejecutar acciones de productos
            for (int i = 0; i < request.Components.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var component = request.Components[i];
                var result = response.Components[i];

                if (result.Errors.Count > 0 || string.IsNullOrWhiteSpace(component.Reference))
                    continue;

                OdooProductInfo? product = null;

                if (result.Action == ICEBOMActionEnum.Create)
                {
                    var writeRequest = _odooSyncService.BuildProductWriteRequest(component, result);
                    product = await _odooSyncService.CreateProductAsync(writeRequest, cancellationToken);
                }
                else if (result.Action == ICEBOMActionEnum.Update)
                {
                    var writeRequest = _odooSyncService.BuildProductWriteRequest(component, result);
                    product = await _odooSyncService.UpdateProductAsync(writeRequest, cancellationToken);
                }
                else
                {
                    product = await _odooRepositoryAsync.GetProductAsync(component.Reference, cancellationToken);
                }

                if (product != null && product.Exists)
                {
                    ApplyProductResult(result, product);
                    productsByReference[component.Reference] = product;
                }
            }

            // 2) Ejecutar acciones de BOMs
            for (int i = 0; i < request.Boms.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bom = request.Boms[i];
                var result = response.Boms[i];

                if (result.Errors.Count > 0 || string.IsNullOrWhiteSpace(bom.ProductReference))
                    continue;

                if (!productsByReference.TryGetValue(bom.ProductReference, out var parentProduct))
                {
                    parentProduct = await _odooRepositoryAsync.GetProductAsync(
                        bom.ProductReference,
                        cancellationToken);

                    if (parentProduct.Exists)
                        productsByReference[bom.ProductReference] = parentProduct;
                }

                if (!parentProduct.Exists)
                    continue;

                if (result.Action == ICEBOMActionEnum.Create)
                {
                    var writeRequest = _odooSyncService.BuildBomWriteRequest(bom, result, parentProduct);
                    var odooBom = await _odooSyncService.CreateBomAsync(writeRequest, cancellationToken);

                    ApplyBomResult(result, odooBom);
                }
                else if (result.Action == ICEBOMActionEnum.Update)
                {
                    var writeRequest = _odooSyncService.BuildBomWriteRequest(bom, result, parentProduct);
                    var odooBom = await _odooSyncService.UpdateBomAsync(writeRequest, cancellationToken);

                    ApplyBomResult(result, odooBom);
                }
                else
                {
                    continue;
                }

                var lineRequests = _odooSyncService.BuildBomLineWriteRequests(
                    result,
                    productsByReference);

                await _odooSyncService.ReplaceBomLinesAsync(
                    bom.ProductReference,
                    lineRequests,
                    cancellationToken);
            }
        }

        private static void ApplyProductResult(ICEBOMComponentResult result, OdooProductInfo product)
        {
            if (product == null)
                return;

            result.OdooProductId = product.Id;
            result.OdooProductName = product.Name;
            result.OdooCategoryId = product.CategoryId;
            result.Category = product.CategoryName;
            result.OdooUnitId = product.UnitId;
        }

        private static void ApplyBomResult(ICEBOMBomResult result, OdooBomInfo bom)
        {
            if (bom == null)
                return;

            result.OdooBomId = bom.Id;
        }
    }
}