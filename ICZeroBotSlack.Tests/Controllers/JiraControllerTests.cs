using ICSlackBot.Logic.Controllers;
using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace ICSlackBot.Tests
{
    [TestFixture]
    public class JiraTests
    {
        
        /// <summary>
        /// Tests if it's possible to create a Jira Ticket by checking the rest api responce.
        /// </summary>
        [Test]
        public void CreateAndCommentJiraTicket()
        {
            string loc = Assembly.GetExecutingAssembly().Location;
            string location = Path.GetDirectoryName(loc);

            ConfigController cfg = new ConfigController(location);

            if (cfg.LoadConfig())
            {
                JiraController jc = jc = new JiraController(
                    cfg.Get("Jira", "User", "Unknow"),
                    cfg.Get("Jira", "Pwd", "???"),
                    cfg.Get("Jira", "Url", "https://localhost/"),
                    cfg.Get("Jira", "Assignee", string.Empty)
                    );
                JiraResponce responce = jc.CreateTicket("ICR", "Critical", "The test title", "The test Body", JiraIssueTypeConst.Bug);
                Assert.IsNotNull(responce.id);
                responce = jc.AddComment(responce.key, "Test add comment to created ticket");
                Assert.IsNotNull(responce.id);
            }
        }
    }
}
