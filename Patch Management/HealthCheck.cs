using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patch_Management
{
    public class HealthCheck
    {


        static readonly string[] errorCodes = {
            "80070032", //Feature update failure
            "80248014" //Download Error - possible TLS issue, check CA updates and HTTP TCP 80 access. HTTPS://slscr.update.microsoft.com
        }; 


        //Generate Log.



    }
}
