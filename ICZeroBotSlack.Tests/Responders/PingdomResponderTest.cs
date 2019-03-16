using ICSlackBot.Logic.Controllers;
using ICSlackBot.Logic.Responders;
using NUnit.Framework;
using SlackBot.Models;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ICSlackBot.Tests.Responders
{
    [TestFixture]
    public class PingdomResponderTest
    {
        /// <summary>
        /// Pingdoms the can respond test.
        /// </summary>
        [Test]
        public void PingdomCanRespondTest()
        {
            PingdomResponder ping = new PingdomResponder();

            ResponseContext context = new ResponseContext()
            {
                Message = new SlackMessage()
                {
                    User = new SlackUser()
                    {
                        Name = "pingdom"
                    },
                    Text = "COS:Blocker:Clariant Prod Delivery High Incident down is down (Incident #2140)"
                }
            };

            Assert.IsTrue(ping.CanRespond(context));
        }


        /// <summary>
        /// Pingdoms the get response test.
        /// </summary>
        [Test]
        public void PingdomGetResponseTest()
        {
            List<SlackAttachment> attach = new List<SlackAttachment>();
            attach.Add(new SlackAttachment()
            {
                Title = "ICR:Bug:Critical:Test Jira Ticket. Virtual System is down (#9999)",
                Text = "ICR:Bug:Critical:Test Jira Ticket. Virtual System is down (#9999)"
            });

            ResponseContext context = new ResponseContext()
            {
                Message = new SlackMessage()
                {
                    Attachments = attach
                }
            };

            string loc = Assembly.GetExecutingAssembly().Location;
            string location = Path.GetDirectoryName(loc);
            ConfigController cfg = new ConfigController(location);

            if (cfg.LoadConfig())
            {
                SlackBotController sbc = new SlackBotController(cfg);
                PingdomResponder ping = new PingdomResponder(sbc);
                BotMessage bm = ping.GetResponse(context);
                Assert.True(bm.Text.Contains("Jira Ticket"));
            }
        }
    }
}
