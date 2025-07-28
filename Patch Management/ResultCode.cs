using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patch_Management
{
    public class ResultCode
    {

        public static string GetCodeValue(int ResultCode)
        {
            if (ResultCode == 0)
            {
                return "Not Started (0)";
            }
            else if (ResultCode == 1)
            {
                return "In Progress (1)";
            }
            else if (ResultCode == 2)
            {
                return "Completed (2)";
            }
            else if (ResultCode == 3)
            {
                return "Completed with Error (3)";
            }
            else if (ResultCode == 4)
            {
                return "Failed (4)";
            }
            else if (ResultCode == 5)
            {
                return "Aborted (5)";
            }
            return "Unknown Code: " + ResultCode.ToString();
        }
    }
}
