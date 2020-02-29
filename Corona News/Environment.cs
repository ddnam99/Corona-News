using Newtonsoft.Json;

namespace Corona_News
{
    class Environment
    {
        private static ConfigurationWebhook configWebhook = new ConfigurationWebhook();

        public static ConfigurationWebhook GetConfigurationWebhook() => configWebhook;

        [JsonIgnore]
        internal static ConfigurationWebhook ConfigWebhook
        {
            get
            {
                Helper.LoadConfigWebhook();

                return configWebhook;
            }
            set => configWebhook = value;
        }
    }

    class ConfigurationWebhook
    {
        [JsonProperty("Webhook")]
        private string webhook;
        [JsonProperty("Webhook Logs")]
        private string webhookLogs;
        [JsonProperty("Webhook Debug")]
        private string webhookDebug;

        [JsonIgnore]
        public string Webhook
        {
            get
            {
#if DEBUG
                return webhookDebug;
#else
                return webhook;
#endif
            }
            set => webhook = value;
        }
        [JsonIgnore]
        public string WebhookLogs
        {
            get
            {
#if DEBUG
                return webhookDebug;
#else
                return webhookLogs;
#endif
            }
            set => webhookLogs = value;
        }
        [JsonIgnore]
        public string WebhookDebug { get => webhookDebug; set => webhookDebug = value; }

        public override bool Equals(object obj)
        {
            return obj is ConfigurationWebhook webhook &&
                   Webhook == webhook.Webhook &&
                   WebhookLogs == webhook.WebhookLogs &&
                   WebhookDebug == webhook.WebhookDebug;
        }
    }
}