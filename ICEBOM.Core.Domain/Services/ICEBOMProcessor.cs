using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Models.Odoo;
using ICEBOM.Core.Domain.Normalizers;
using ICEBOM.Core.Domain.Odoo.JsonRpc;
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

            ValidateBeforeExecution(request, response);

            if (HasBlockingErrors(response))
            {
                BlockOdooExecution(response);
                CalculateSummary(response);
                response.Trace.AddRange(_traceService.Entries);
                return response;
            }

            if (HasAsyncOdooSync)
            {
                await SyncWithOdooAsync(request, response, cancellationToken);
            }

            CalculateSummary(response);

            response.Trace.AddRange(_traceService.Entries);

            return response;
        }

        private void ValidateBeforeExecution(ICEBOMRequest request, ICEBOMResponse response)
        {
            ValidateRequiredReferenceForProductSync(response);
            ValidateRequiredUnitForProductCreate(response);
            ValidateRequiredParentProductForBomSync(request, response);
            ValidateRequiredLinesForBomSync(response);
        }

        private void ValidateRequiredReferenceForProductSync(ICEBOMResponse response)
        {
            foreach (var component in response.Components)
            {
                if (component.Action != ICEBOMActionEnum.Create &&
                    component.Action != ICEBOMActionEnum.Update)
                    continue;

                if (!string.IsNullOrWhiteSpace(component.Reference))
                    continue;

                component.Status = "blocked";
                component.Action = ICEBOMActionEnum.Blocked;

                component.Errors.Add(new ICEBOMError
                {
                    Code = "PRODUCT_REFERENCE_REQUIRED_FOR_SYNC",
                    Message = "La referencia del producto es obligatoria para sincronizar con Odoo."
                });

                _traceService.Add(
                    "PreExecuteValidation",
                    "Component",
                    component.Reference,
                    "Un producto se iba a sincronizar, pero no tiene referencia válida → Blocked.");
            }
        }

        private void ValidateRequiredUnitForProductCreate(ICEBOMResponse response)
        {
            foreach (var component in response.Components)
            {
                if (component.Action != ICEBOMActionEnum.Create)
                    continue;

                if (component.OdooUnitId > 0)
                    continue;

                component.Status = "blocked";
                component.Action = ICEBOMActionEnum.Blocked;

                component.Errors.Add(new ICEBOMError
                {
                    Code = "PRODUCT_UNIT_NOT_RESOLVED",
                    Message = $"No se pudo resolver la unidad del producto '{component.Reference}'. La unidad es obligatoria para crear productos en Odoo."
                });

                _traceService.Add(
                    "PreExecuteValidation",
                    "Component",
                    component.Reference,
                    $"El producto '{component.Reference}' se iba a crear, pero no tiene OdooUnitId válido → Blocked.");
            }
        }

        private void ValidateRequiredParentProductForBomSync(ICEBOMRequest request, ICEBOMResponse response)
        {
            foreach (var bom in response.Boms)
            {
                if (bom.Action != ICEBOMActionEnum.Create &&
                    bom.Action != ICEBOMActionEnum.Update)
                    continue;

                var parentComponent = response.Components
                    .FirstOrDefault(c =>
                        string.Equals(c.Reference, bom.ProductReference, StringComparison.OrdinalIgnoreCase));

                if (parentComponent == null)
                {
                    BlockBomParentNotResolved(bom);
                    continue;
                }

                if (parentComponent.Action == ICEBOMActionEnum.Blocked ||
                    parentComponent.Errors.Any())
                {
                    BlockBomParentNotResolved(bom);
                    continue;
                }

                var parentWillExist =
                    parentComponent.OdooProductId > 0 ||
                    parentComponent.Action == ICEBOMActionEnum.Create ||
                    parentComponent.Action == ICEBOMActionEnum.Update ||
                    parentComponent.Action == ICEBOMActionEnum.Skip;

                if (parentWillExist)
                    continue;

                BlockBomParentNotResolved(bom);
            }
        }

        private void BlockBomParentNotResolved(ICEBOMBomResult bom)
        {
            bom.Status = "blocked";
            bom.Action = ICEBOMActionEnum.Blocked;

            bom.Errors.Add(new ICEBOMError
            {
                Code = "BOM_PARENT_PRODUCT_NOT_RESOLVED",
                Message = $"No se pudo resolver el producto padre de la BOM '{bom.BomId}'. La BOM no se puede sincronizar con Odoo."
            });

            _traceService.Add(
                "PreExecuteValidation",
                "BOM",
                bom.BomId,
                $"La BOM '{bom.BomId}' se iba a sincronizar, pero no tiene producto padre válido → Blocked.");
        }

        private void ValidateRequiredLinesForBomSync(ICEBOMResponse response)
        {
            foreach (var bom in response.Boms)
            {
                if (bom.Action != ICEBOMActionEnum.Create &&
                    bom.Action != ICEBOMActionEnum.Update)
                    continue;

                var hasValidLines = bom.Lines != null &&
                                    bom.Lines.Any(line =>
                                        line.Status != "blocked" &&
                                        !string.IsNullOrWhiteSpace(line.ComponentReference) &&
                                        line.Quantity > 0);

                if (hasValidLines)
                    continue;

                bom.Status = "blocked";
                bom.Action = ICEBOMActionEnum.Blocked;

                bom.Errors.Add(new ICEBOMError
                {
                    Code = "BOM_HAS_NO_VALID_LINES",
                    Message = $"La BOM '{bom.BomId}' no tiene líneas válidas para sincronizar con Odoo."
                });

                _traceService.Add(
                    "PreExecuteValidation",
                    "BOM",
                    bom.BomId,
                    $"La BOM '{bom.BomId}' se iba a sincronizar, pero no tiene líneas válidas → Blocked.");
            }
        }

        private static bool HasBlockingErrors(ICEBOMResponse response)
        {
            if (response == null)
                return true;

            var componentErrors = response.Components
                .Any(c => c.Errors != null && c.Errors.Any());

            var bomErrors = response.Boms
                .Any(b => b.Errors != null && b.Errors.Any());

            var bomLineErrors = response.Boms
                .Any(b => b.Lines != null &&
                          b.Lines.Any(l => l.Errors != null && l.Errors.Any()));

            return componentErrors || bomErrors || bomLineErrors;
        }

        private void BlockOdooExecution(ICEBOMResponse response)
        {
            _traceService.Add(
                "ExecuteBlocked",
                "System",
                string.Empty,
                "La ejecución contra Odoo se ha bloqueado porque existen errores de validación.");

            foreach (var component in response.Components)
            {
                if (component.Errors != null && component.Errors.Any())
                {
                    component.Status = "blocked";
                    component.Action = ICEBOMActionEnum.Blocked;
                }
            }

            foreach (var bom in response.Boms)
            {
                if (bom.Errors != null && bom.Errors.Any())
                {
                    bom.Status = "blocked";
                    bom.Action = ICEBOMActionEnum.Blocked;
                }

                foreach (var line in bom.Lines)
                {
                    if (line.Errors != null && line.Errors.Any())
                    {
                        line.Status = "blocked";
                    }
                }
            }
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

        private async Task SyncWithOdooAsync(ICEBOMRequest request, ICEBOMResponse response, CancellationToken cancellationToken)
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

                try
                {
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
                catch (OdooJsonRpcException ex)
                {
                    AddOdooExecutionError(
                        result,
                        "ODOO_PRODUCT_SYNC_ERROR",
                        $"Error al sincronizar el producto '{component.Reference}' con Odoo: {ex.Message}",
                        component.Reference);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    AddOdooExecutionError(
                        result,
                        "PRODUCT_SYNC_ERROR",
                        $"Error inesperado al sincronizar el producto '{component.Reference}': {ex.Message}",
                        component.Reference);
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

                try
                {
                    if (!productsByReference.TryGetValue(bom.ProductReference, out var parentProduct))
                    {
                        parentProduct = await _odooRepositoryAsync.GetProductAsync(
                            bom.ProductReference,
                            cancellationToken);

                        if (parentProduct.Exists)
                            productsByReference[bom.ProductReference] = parentProduct;
                    }

                    if (!parentProduct.Exists)
                    {
                        result.Status = "blocked";
                        result.Action = ICEBOMActionEnum.Blocked;

                        result.Errors.Add(new ICEBOMError
                        {
                            Code = "BOM_PARENT_PRODUCT_NOT_FOUND",
                            Message = $"No se encontró en Odoo el producto padre '{bom.ProductReference}' para sincronizar la BOM."
                        });

                        _traceService.Add(
                            "ExecuteError",
                            "BOM",
                            bom.BomId,
                            $"No se encontró el producto padre '{bom.ProductReference}' para sincronizar la BOM '{bom.BomId}'.");

                        continue;
                    }

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
                catch (OdooJsonRpcException ex)
                {
                    AddOdooExecutionError(
                        result,
                        "ODOO_BOM_SYNC_ERROR",
                        $"Error al sincronizar la BOM '{bom.BomId}' con Odoo: {ex.Message}",
                        bom.BomId);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    AddOdooExecutionError(
                        result,
                        "BOM_SYNC_ERROR",
                        $"Error inesperado al sincronizar la BOM '{bom.BomId}': {ex.Message}",
                        bom.BomId);
                }
            }
        }

        private void AddOdooExecutionError(ICEBOMComponentResult result, string code, string message, string reference)
        {
            result.Status = "blocked";
            result.Action = ICEBOMActionEnum.Blocked;

            result.Errors.Add(new ICEBOMError
            {
                Code = code,
                Message = message
            });

            _traceService.Add(
                "ExecuteError",
                "Product",
                reference,
                message);
        }

        private void AddOdooExecutionError(ICEBOMBomResult result, string code, string message, string reference)
        {
            result.Status = "blocked";
            result.Action = ICEBOMActionEnum.Blocked;

            result.Errors.Add(new ICEBOMError
            {
                Code = code,
                Message = message
            });

            _traceService.Add(
                "ExecuteError",
                "BOM",
                reference,
                message);
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