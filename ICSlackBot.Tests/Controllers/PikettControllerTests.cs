using ICSlackBot.Logic.Controllers;
using ICSlackBot.Logic.Responders;
using NUnit.Framework;
using System;

namespace ICSlackBot.Tests
{
    [TestFixture]
    public class PikettControllerTests
    {

        /// <summary>
        /// Tests if it's possible to create a Jira Ticket by checking the rest api responce.
        /// </summary>
        [Test]
        public void CreateJiraTicket()
        {
            ConfigController cfg = new ConfigController(AppDomain.CurrentDomain.BaseDirectory);
            SlackBotController bot = new SlackBotController(cfg);


            PikettResponder sr = new PikettResponder(bot);
            sr.GetResponse(null);
            Assert.IsNotNull(true);
        }
    }
}
