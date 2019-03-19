using System.Web.Http;

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
