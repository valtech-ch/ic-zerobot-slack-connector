using Newtonsoft.Json;
namespace SlackBot.Models
{
    public class SlackAttachmentField
    {
        [JsonProperty(PropertyName = "short")]
        public bool IsShort { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}