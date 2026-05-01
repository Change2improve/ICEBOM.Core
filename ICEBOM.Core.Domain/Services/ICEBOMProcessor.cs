using System.ComponentModel.DataAnnotations;

using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Normalizers;
using ICEBOM.Core.Domain.Repositories;

namespace ICEBOM.Core.Domain.Services
{
    public class ICEBOMProcessor
    {
        private readonly FakeOdooRepository _odooRepository;

        private readonly ICEBOMValidator _validator;

        private readonly ICEBOMDecisionEngine _decisionEngine;

        private readonly ICEBOMTraceService _traceService;

        public ICEBOMProcessor(FakeOdooRepository odooRepository, ICEBOMUnitNormalizer unitNormalizer, ICEBOMBusinessRulesConfig businessRules)
        {
            _odooRepository = odooRepository;
            _traceService = new ICEBOMTraceService();

            _validator = new ICEBOMValidator(_odooRepository, unitNormalizer, businessRules, _traceService);
            _decisionEngine = new ICEBOMDecisionEngine(_odooRepository, _traceService);
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
    }
}