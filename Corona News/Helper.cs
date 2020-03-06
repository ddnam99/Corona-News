using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;

namespace Corona_News
{
    class Helper
    {
        public static void LoadConfigWebhook()
        {
            var path = "./data/ConfigWebhook.json";

            if (File.Exists(path))
            {
                var configWebhook = JsonConvert.DeserializeObject<ConfigurationWebhook>(File.ReadAllText(path));
                if (!Environment.GetConfigurationWebhook().Equals(configWebhook))
                {
                    ConsoleLogs("Corona News: Update Webhook Config!", configWebhook.WebhookLogs);
                    Environment.ConfigWebhook = configWebhook;
                }
            }
        }

        public static void WriteEnvironmentToFile() => File.WriteAllText("./data/ConfigWebhook.json", JsonConvert.SerializeObject(Environment.ConfigWebhook, Formatting.Indented));

        public static void ConsoleLogs(string log, string webhook = "")
        {
            Console.WriteLine($"{DateTime.Now.ToString()}> {log}");
#if RELEASE
            if (string.IsNullOrEmpty(webhook)) Message(log, Environment.ConfigWebhook.WebhookLogs);
            else Message(log, webhook);
#endif
        }

        public static void LogError(string log, string webhook = "")
        {
            Console.WriteLine($"{DateTime.Now.ToString()}> {log}");

#if RELEASE
            if (string.IsNullOrEmpty(webhook)) Message($"```{log}```", Environment.ConfigWebhook.WebhookLogs);
            else Message($"```{log}```", webhook);
#endif
        }

        public static string Message(string text, string webhook = "")
        {
            if (string.IsNullOrEmpty(webhook)) webhook = Environment.ConfigWebhook.Webhook;

            var client = new RestClient(webhook);
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\"text\":\"" + text.Replace(@"\", @"\\") + "\"}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            return response.Content;
        }
    }
}