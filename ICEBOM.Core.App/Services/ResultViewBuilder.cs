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

        public List<ConfigItemView> BuildConfigItems(ICEBOMCustomerConfig config)
        {
            var items = new List<ConfigItemView>
            {
                new() { Section = "General", Key = "CustomerName", Value = config.CustomerName },
                new() { Section = "General", Key = "ConfigVersion", Value = config.ConfigVersion },

                new() { Section = "SyncPolicy", Key = "CreateMissingProducts", Value = config.SyncPolicy.CreateMissingProducts.ToString() },
                new() { Section = "SyncPolicy", Key = "UpdateExistingProducts", Value = config.SyncPolicy.UpdateExistingProducts.ToString() },
                new() { Section = "SyncPolicy", Key = "CreateMissingBoms", Value = config.SyncPolicy.CreateMissingBoms.ToString() },
                new() { Section = "SyncPolicy", Key = "UpdateExistingBoms", Value = config.SyncPolicy.UpdateExistingBoms.ToString() },
                new() { Section = "SyncPolicy", Key = "AllowProductVariants", Value = config.SyncPolicy.AllowProductVariants.ToString() },

                new() { Section = "Defaults", Key = "DefaultUnit", Value = config.Defaults.DefaultUnit },
                new() { Section = "Defaults", Key = "DefaultFunctionalType", Value = config.Defaults.DefaultFunctionalType },
            };

            foreach (var unit in config.Units.KnownUnits)
            {
                items.Add(new ConfigItemView
                {
                    Section = "Units.KnownUnits",
                    Key = "Unit",
                    Value = unit
                });
            }

            foreach (var alias in config.Units.Aliases)
            {
                items.Add(new ConfigItemView
                {
                    Section = "Units.Aliases",
                    Key = alias.Key,
                    Value = alias.Value
                });
            }

            foreach (var product in config.FakeOdoo.ExistingProducts)
            {
                items.Add(new ConfigItemView
                {
                    Section = "FakeOdoo.Products",
                    Key = "ExistingProduct",
                    Value = product
                });
            }

            foreach (var bom in config.FakeOdoo.ExistingBoms)
            {
                items.Add(new ConfigItemView
                {
                    Section = "FakeOdoo.BOMs",
                    Key = "ExistingBom",
                    Value = bom
                });
            }

            return items;
        }

        public List<TraceEntryView> BuildTrace(ICEBOMResponse response)
        {
            return response.Trace.Select(t => new TraceEntryView
            {
                Step = t.Step,
                Entity = t.Entity,
                Reference = t.Reference,
                Message = t.Message
            }).ToList();
        }
    }
}