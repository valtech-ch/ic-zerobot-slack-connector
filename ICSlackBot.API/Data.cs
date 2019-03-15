using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ICSlackBot.API
{
    /// <summary>
    /// Data Layer for saving messages
    /// </summary>
    public class Data
    {
        static string filePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\msgs.json";
        static List<SlackMessage> _messages = new List<SlackMessage>();

        public Data()
        {
            Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Init()
        {
            _messages = new List<SlackMessage>();

            if (File.Exists(filePath))
            {
                _messages = JsonConvert.DeserializeObject<List<SlackMessage>>(File.ReadAllText(filePath));
            }
        }

        /// <summary>
        /// Saves to file.
        /// </summary>
        private void SaveToFile()
        {
            if (_messages != null)
            {
                var count = _messages.Count - 30;
                if (count > 0)
                {
                    _messages.RemoveRange(0, count);
                }
                File.WriteAllText(filePath, JsonConvert.SerializeObject(_messages));
            }
        }


        /// <summary>
        /// Adds the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="user">The user.</param>
        /// <param name="channel">The channel.</param>
        public void Add(string message, string user, string channel)
        {
            SlackMessage sm = new SlackMessage()
            {
                Message = message,
                User = user,
                Channel = channel.Replace("#", ""),
                Received = DateTime.Now
            };
            if (_messages != null && !_messages.Contains(sm))
            {
                _messages.Add(sm);
            }

            SaveToFile();
        }

        /// <summary>
        /// Gets all messages.
        /// </summary>
        /// <returns></returns>
        public List<SlackMessage> GetAllMessages()
        {
            if (_messages != null)
            {
                return _messages.ToList();
            }

            return new List<SlackMessage>();
        }
    }

    /// <summary>
    /// Slack message model
    /// </summary>
    public class SlackMessage
    {
        public string Message { get; set; }
        public string User { get; set; }
        public string Channel { get; set; }
        public DateTime Received { get; set; }
    }
}
