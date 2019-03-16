using ICZeroBotSlack.Logic.Models;
using RestSharp;
using System;

namespace ICZeroBotSlack.Logic.Connectors
{
    public class IcZeroBotConnector
    {
        readonly private string _baseURL = "https://localhost";

        public IcZeroBotConnector(string baseUrl)
        {
            _baseURL = baseUrl;
        }

        public string CreateTask(IcZeroBotTask task)
        {
            var client = new RestClient(_baseURL);
            client.AddDefaultHeader("Content-type", "application/json");

            var request = new RestRequest("tasks", Method.POST);
            request.AddJsonBody(task);

            IRestResponse response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(response.ErrorMessage);
            }

            return "empty";
        }
    }
}
