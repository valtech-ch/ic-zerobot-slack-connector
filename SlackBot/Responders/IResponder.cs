using SlackBot.Models;

namespace SlackBot.Responders
{
    public interface IResponder
    {
        bool CanRespond(ResponseContext context);
        BotMessage GetResponse(ResponseContext context);
        string GetCommandDescription();
    }
}