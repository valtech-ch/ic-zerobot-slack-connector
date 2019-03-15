namespace ICSlackBot.Logic.EventHandlers
{
    public class BotEvents
    {
        public delegate void SlackControllerDisconnectEventHandler();
        public delegate void SlackControllerConnectedEventHandler();
        public delegate void SlackControllerMessageReceivedEventHandler(string from, string messsage, string channel);
    }
}
