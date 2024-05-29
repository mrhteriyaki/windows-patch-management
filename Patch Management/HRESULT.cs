using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patch_Management
{
    public class HRESULT
    {

        public static string GetDescription(int HResult)
        {
            string HST = HResult.ToString("X");

            if (HST.Equals("0"))
            {
                return "Operation successful";
            }
            else if (HST.Equals("80004004"))
            {
                return "Operation aborted";
            }
            else if (HST.Equals("80070005"))
            {
                return "General access denied error";
            }
            else if (HST.Equals("80004005"))
            {
                return "Unspecified failure";
            }
            else if (HST.Equals("80070006"))
            {
                return "Handle that is not valid";
            }
            else if (HST.Equals("80070057"))
            {
                return "One or more arguments are not valid";
            }
            else if (HST.Equals("80004002"))
            {
                return "No such interface supported";
            }
            else if (HST.Equals("80004001"))
            {
                return "Not implemented";
            }
            else if (HST.Equals("8007000E"))
            {
                return "Failed to allocate necessary memory";
            }
            else if (HST.Equals("80004003"))
            {
                return "Pointer that is not valid";
            }
            else if (HST.Equals("8000FFFF"))
            {
                return "Unexpected failure";
            }
            return "";
        }
    }
}
