using System.Text;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;

namespace ICSlackBot.API.Controller
{
    public class TestController : ApiController
    {
        [HttpGet]
        [ActionName("status")]
        public string GetStatus()
        {
            return "OK";
        }
    }
}
