using System;
using System.Web.Http.SelfHost;
using System.Web.Http;
using log4net;
using System.Net.Http.Formatting;

namespace ICSlackBot.API
{
    public class ApiBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private HttpSelfHostServer _server = null;
        
        public ApiBase(Uri baseAdr)
        {
            try
            {
                // Set up server configuration 
                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAdr);

                config.Formatters.Clear();
                config.Formatters.Add(new JsonMediaTypeFormatter());

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { action = "get", id = RouteParameter.Optional }                   
                );

                // Create server 
                _server = new HttpSelfHostServer(config);

                // Start listening 
                _server.OpenAsync().Wait();
                _logger.Debug("Listening on " + baseAdr);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not start server: {0}", e.GetBaseException().Message);
            }
        }

        public void Stop()
        {
            if (_server != null)
            {
                // Stop listening 
                _server.CloseAsync().Wait();
            }
        }
    }
}
