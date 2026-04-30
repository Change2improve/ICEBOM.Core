using System;
using System.Collections.Generic;

namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMRequest
    {
        public ICEBOMRequestMeta Meta { get; set; } = new();
        public ICEBOMSettingsSnapshot SettingsSnapshot { get; set; } = new();

        public List<ICEBOMComponent> Components { get; set; } = new();
        public List<ICEBOMBom> Boms { get; set; } = new();

        public ICEBOMValidationSummary Validation { get; set; } = new();
    }
}