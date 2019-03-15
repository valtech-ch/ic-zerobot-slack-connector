using System;
using System.Collections.Generic;
using SlackBot.Models;

namespace SlackBot.Responders
{
    public class SimpleResponder : IResponder
    {
        public Func<ResponseContext, bool> CanRespondFunction { get; set; }
        public List<Func<ResponseContext, BotMessage>> GetResponseFunctions { get; set; }
        private string Description = null;

        public SimpleResponder()
        {
            GetResponseFunctions = new List<Func<ResponseContext, BotMessage>>();
            Description = string.Empty;
        }

        public bool CanRespond(ResponseContext context)
        {
            return CanRespondFunction(context);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            if (GetResponseFunctions.Count == 0) {
                throw new InvalidOperationException("Attempted to get a response for \"" + context.Message.Text + "\", but no valid responses have been registered.");
            }

            return GetResponseFunctions[new Random().Next(GetResponseFunctions.Count - 1)](context);
        }

        public string GetCommandDescription()
        {
            return Description;
        }

        public void SetCommandDescription(string txt)
        {
            Description = txt;
        }
    }
}