using System;
using System.Collections.Generic;

using ICEBOM.Core.Domain.Enums;

namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMResponse
    {
        public ICEBOMResponseMeta Meta { get; set; } = new();

        public ICEBOMResponseSummary Summary { get; set; } = new();

        public List<ICEBOMComponentResult> Components { get; set; } = new();
        public List<ICEBOMBomResult> Boms { get; set; } = new();
    }
}