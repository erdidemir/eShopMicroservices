using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base
{
    public class EvenBusConfig
    {

        public int ConnectionRetryCount { get; set; } = 5;

        public string DefaultTopicName { get; set; } = "eShopEventBus";

        public string EventBusConnectionString { get; set; } = string.Empty;

        public string SubscriberClientAppName { get; set; } = string.Empty;

        public string EventNamePrefix { get; set; } = string.Empty;

        public string EventNameSuffix { get; set; } = "IntegrationEvent";

        public EventBusType EventBusType { get; set; } = EventBusType.RabbitMQ;

        public object Connetion { get; set; }

        public bool DeleteEventPrefix => !String.IsNullOrEmpty(EventNamePrefix);
        public bool DeleteEventSuffix => !String.IsNullOrEmpty(EventNameSuffix);

    }

    public enum EventBusType
    {
        RabbitMQ = 0,
        AzureServiceBus = 1
    }
}
