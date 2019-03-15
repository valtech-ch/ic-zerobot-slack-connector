using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlackBot.BotHelpers
{
    public class WebClient
    {
        public Task<string> GetResponse(string address, RequestMethod requestType, params string[] values)
        {
            Dictionary<string, string> bodyValues = new Dictionary<string, string>();
            if (values != null && values.Length > 1)
            {
                string key = string.Empty;
                for (int index = 0; index < values.Length; ++index)
                {
                    if (index % 2 == 0)
                        key = values[index];
                    else
                        bodyValues.Add(key, values[index]);
                }
            }
            return this.GetResponse(address, requestType, bodyValues);
        }

        public async Task<string> GetResponse(string address, RequestMethod requestType, Dictionary<string, string> bodyValues = null)
        {
            FormUrlEncodedContent content = (FormUrlEncodedContent)null;
            if (bodyValues != null)
                content = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)bodyValues);
            string str;
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = System.TimeSpan.FromSeconds(5);

                HttpResponseMessage response = null;
                if (requestType == RequestMethod.Post)
                    response = await httpClient.PostAsync(address, (HttpContent)content);
                else
                    response = await httpClient.GetAsync(address);

                str = await response.Content.ReadAsStringAsync();
            }
            return str;
        }

        public enum RequestMethod
        {
            Get,
            Post,
        }
    }
}