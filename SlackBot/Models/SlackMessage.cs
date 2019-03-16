using System.Collections.Generic;

namespace SlackBot.Models
{
    public class SlackMessage
    {
        public SlackChatHub ChatHub { get; set; }
        public bool MentionsBot { get; set; }
        public string RawData { get; set; }
        public string Text { get; set; }
        public SlackUser User { get; set; }
        public List<SlackAttachment> Attachments { get; set; }
        public List<SlackFile> Files { get; set; }
    }
}