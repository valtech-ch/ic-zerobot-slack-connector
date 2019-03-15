using ICSlackBot.Logic.Responders;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace ICSlackBot.Tests
{
    [TestFixture]
    public class WeatherResponderTest
    {
        [Test]
        public void TestWeatherRegexOK()
        {
            string inputCorrect = "jarvis weather abc";
            Assert.IsTrue(Regex.IsMatch(inputCorrect, WeatherResponder.WEATHER_REGEX, RegexOptions.IgnoreCase));
        }

        [Test]
        public void TestWeatherRegexNOK()
        {
            string invalidInput = "jarvis weather 1abc";
           Assert.IsFalse(Regex.IsMatch(invalidInput, WeatherResponder.WEATHER_REGEX, RegexOptions.IgnoreCase));
        }
    }
}
