namespace SlackBot.Models
{
    public class SlackUser
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }

        public string FormattedUserID
        {
            get
            {
                if (!string.IsNullOrEmpty(ID)) {
                    return "<@" + ID + ">";
                }
                return string.Empty;
            }
        }

        public bool IsSlackbot
        {
            get { return ID == "USLACKBOT"; }
        }

        public bool IsBot { get; set; }

        public SlackUser()
        {
            ID = string.Empty;
            Name = string.Empty;
            ImageUrl = string.Empty;
            IsBot = false;
        }
    }
}