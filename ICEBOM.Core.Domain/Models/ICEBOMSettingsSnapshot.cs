using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICEBOM.Core.Domain.Models
{
    public class ICEBOMSettingsSnapshot
    {
        public bool CreateMissingProducts { get; set; } = true;
        public bool UpdateExistingProducts { get; set; } = false;
        public bool CreateMissingBoms { get; set; } = true;
        public bool UpdateExistingBoms { get; set; } = true;
        public bool AllowProductVariants { get; set; } = false;
    }
}
