using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICEBOM.Core.Domain.Enums;

namespace ICEBOM.Core.App.ViewModels
{
    public class ComponentResultView
    {
        public string InternalId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ErrorsText { get; set; } = string.Empty;
        public string WarningsText { get; set; } = string.Empty;
    }
}
