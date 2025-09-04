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
        public Dictionary<string,string> resultList = [];

        public InstallResult(bool reboot, bool errors)
        {
            rebootRequired = reboot;
            errorsOccured = errors;
        }
        public InstallResult()
        { }

        public void AddResult(string Title, string ResultCode)
        {
            resultList.Add(Title, ResultCode);
        }
    }

}
