using System;
using System.Web.Http.SelfHost;
using System.Web.Http;

namespace ICSlackBot.API
{
    public class ApiBase
    {
        private HttpSelfHostServer _server = null;
        private Data data = null;

        public ApiBase(Uri baseAdr)
        {
            try
            {
                data = new Data();

                // Set up server configuration 
                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAdr);

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { action = "get", id = RouteParameter.Optional }
                );

                config.Filters.Add(new FilterIPAttribute()
                {
                    AllowedSingleIPs = "::1,127.0.0.1",
                    AllowedMaskedIPs = "172.0.0.0;255.0.0.0,192.168.0.0;255.255.0.0"
                });

                // Create server 
                _server = new HttpSelfHostServer(config);

                // Start listening 
                _server.OpenAsync().Wait();
                Console.WriteLine("Listening on " + baseAdr);
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

        public void AddMessage(string user, string message, string channel)
        {
            if (data != null)
            {
                data.Add(message, user, channel);
            }
        }
    }
}
