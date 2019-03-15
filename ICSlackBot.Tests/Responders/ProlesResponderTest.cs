using ICSlackBot.Logic.Responders;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace ICSlackBot.Tests
{
    [TestFixture]
    public class ProlesResponderTest
    {
        [Test]
        public void TestProlesRegexOk()
        {
            const string inputCorrect = "jarvis budget slg15-07";
            Assert.IsTrue(Regex.IsMatch(inputCorrect, ProlesResponder.PROLES_TERM, RegexOptions.IgnoreCase));
        }

        [Test]
        public void TestProlesRegexNok()
        {
            const string invalidInput = "jarvis budget ";
            Assert.IsFalse(Regex.IsMatch(invalidInput, CookResponder.CookRegEx, RegexOptions.IgnoreCase));
        }
    }
}
