using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patch_Management
{
    public class InstallResult
    {
        public bool rebootRequired;
        public bool errorsOccured; 

        public InstallResult(bool reboot, bool errors)
        {
            rebootRequired = reboot;
            errorsOccured = errors;
        }
    }
}
