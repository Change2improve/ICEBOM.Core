using ICEBOM.Core.App.ViewModels;
using ICEBOM.Core.Domain.Models;

namespace ICEBOM.Core.App.Services
{
    public class ResultViewBuilder
    {
        public List<ComponentResultView> BuildComponents(ICEBOMResponse response)
        {
            return response.Components.Select(c => new ComponentResultView
            {
                InternalId = c.InternalId,
                Reference = c.Reference,
                Status = c.Status,
                Action = c.Action.ToString(),
                ErrorsText = string.Join(" | ", c.Errors.Select(e => e.Message)),
                WarningsText = string.Join(" | ", c.Warnings.Select(w => w.Message))
            }).ToList();
        }

        public List<BomResultView> BuildBoms(ICEBOMResponse response)
        {
            return response.Boms.Select(b => new BomResultView
            {
                BomId = b.BomId,
                ProductReference = b.ProductReference,
                Status = b.Status,
                Action = b.Action.ToString(),
                ErrorsText = string.Join(" | ", b.Errors.Select(e => e.Message)),
                WarningsText = string.Join(" | ", b.Warnings.Select(w => w.Message))
            }).ToList();
        }

        public List<BomLineResultView> BuildBomLines(ICEBOMResponse response)
        {
            return response.Boms
                .SelectMany(b => b.Lines.Select(line => new BomLineResultView
                {
                    BomId = b.BomId,
                    ComponentInternalId = line.ComponentInternalId,
                    ComponentReference = line.ComponentReference,
                    Quantity = line.Quantity,
                    OriginalUnit = line.OriginalUnit,
                    NormalizedUnit = line.NormalizedUnit,
                    Status = line.Status,
                    ErrorsText = string.Join(" | ", line.Errors.Select(e => e.Message)),
                    WarningsText = string.Join(" | ", line.Warnings.Select(w => w.Message))
                }))
                .ToList();
        }

        public List<MessageView> BuildMessages(ICEBOMResponse response)
        {
            var messages = new List<MessageView>();

            foreach (var c in response.Components)
            {
                messages.AddRange(c.Errors.Select(e => new MessageView
                {
                    Type = "Error",
                    Code = e.Code,
                    Message = e.Message,
                    Source = $"Component: {c.InternalId}"
                }));

                messages.AddRange(c.Warnings.Select(w => new MessageView
                {
                    Type = "Warning",
                    Code = w.Code,
                    Message = w.Message,
                    Source = $"Component: {c.InternalId}"
                }));
            }

            foreach (var b in response.Boms)
            {
                messages.AddRange(b.Errors.Select(e => new MessageView
                {
                    Type = "Error",
                    Code = e.Code,
                    Message = e.Message,
                    Source = $"BOM: {b.BomId}"
                }));

                messages.AddRange(b.Warnings.Select(w => new MessageView
                {
                    Type = "Warning",
                    Code = w.Code,
                    Message = w.Message,
                    Source = $"BOM: {b.BomId}"
                }));

                foreach (var line in b.Lines)
                {
                    messages.AddRange(line.Errors.Select(e => new MessageView
                    {
                        Type = "Error",
                        Code = e.Code,
                        Message = e.Message,
                        Source = $"BOM Line: {b.BomId} / {line.ComponentInternalId}"
                    }));

                    messages.AddRange(line.Warnings.Select(w => new MessageView
                    {
                        Type = "Warning",
                        Code = w.Code,
                        Message = w.Message,
                        Source = $"BOM Line: {b.BomId} / {line.ComponentInternalId}"
                    }));
                }
            }

            return messages;
        }
    }
}