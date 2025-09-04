using Microsoft.AspNetCore.Mvc;


namespace PatchReporting.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ClientReportingController : Controller
    {

        [HttpGet]
        public string Get([FromBody] string clientReportjson)
        {
            //Get pending client commands.
            return "test: ";//clientid.ToString();
        }
    }
}
