namespace SlackBot.Models
{
    public class SlackFile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Mimetype { get; set; }
        public string Filetype { get; set; }
        public int Size { get; set; }
        public string Url_private { get; set; }
        public string Permalink { get; set; }
    }
}
