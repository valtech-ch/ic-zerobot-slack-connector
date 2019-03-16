using ICSlackBot.Logic.Responders;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace ICSlackBot.Tests
{
    [TestFixture]
    public class CookResponderTests
    {
        [Test]
        public void TestCookRegexOk()
        {
            const string inputCorrect = "jarvis cook pizza";
            Assert.IsTrue(Regex.IsMatch(inputCorrect, CookResponder.CookRegEx, RegexOptions.IgnoreCase));
        }

        [Test]
        public void TestCookRegexNok()
        {
            const string invalidInput = "jarvis cook ";
            Assert.IsFalse(Regex.IsMatch(invalidInput, CookResponder.CookRegEx, RegexOptions.IgnoreCase));
        }
    }
}
