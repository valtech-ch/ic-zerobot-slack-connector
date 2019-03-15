using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml.Linq;

namespace ICSlackBot.API.Controller
{
    public class MessageController : ApiController
    {
        [HttpGet]
        [ActionName("getall")]
        public HttpResponseMessage GetChannelFeed(string id)
        {
            XElement channel = new XElement("channel");

            foreach (SlackMessage sm in new Data().GetAllMessages().Where(sm => string.IsNullOrEmpty(id) || sm.Channel == id).Reverse())
            {
                XElement msg = new XElement("item");

                msg.Add(new XElement("title", string.Format("{0} - {1:f}", sm.User, sm.Received)));
                msg.Add(new XElement("description", sm.Message));
                msg.Add(new XElement("link", string.Format("https://{0}/messages/{1}/", "ic-test.slack.com", sm.Channel)));
                msg.Add(new XElement("author", sm.User));

                channel.Add(msg);
            }

            XDocument root = new XDocument(
                new XElement("rss", new XAttribute("version", "2.0"), channel)
            );

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(root.ToString(),
                Encoding.UTF8, "application/rss+xml")
            };
            return response;
        }
    }
}
