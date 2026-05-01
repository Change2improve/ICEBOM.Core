using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICEBOM.Core.Domain.Enums;

namespace ICEBOM.Core.App.ViewModels
{
    public class BomResultView
    {
        public string BomId { get; set; } = string.Empty;
        public string ProductReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ErrorsText { get; set; } = string.Empty;
        public string WarningsText { get; set; } = string.Empty;

        public int OdooBomId { get; set; }
    }
}
