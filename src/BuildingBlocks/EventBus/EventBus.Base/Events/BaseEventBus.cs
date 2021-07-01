using EventBus.Base.Abstraction;
using EventBus.Base.SubManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly IEventBusSubscriptionManager SubsManager;

        private EvenBusConfig evenBusConfig;

        protected BaseEventBus(EvenBusConfig congfig, IServiceProvider serviceProvider)
        {
            evenBusConfig = congfig;
            ServiceProvider = serviceProvider;
            SubsManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
        }

        public virtual string ProcessEventName(string eventName)
        {
            if (evenBusConfig.DeleteEventPrefix)
            {
                eventName = eventName.TrimStart(evenBusConfig.EventNamePrefix.ToArray());
            }

            if (evenBusConfig.DeleteEventSuffix)
            {
                eventName = eventName.TrimEnd(evenBusConfig.EventNameSuffix.ToArray());
            }

            return eventName;
        }

        public virtual string GetSubName(string eventName)
        {
            return $"{evenBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }

        public virtual void Dispose()
        {
            evenBusConfig = null;
        }

        public async Task<bool> ProcessEvent(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);

            var processed = false;

            if (SubsManager.HasSubscriptionForEvent(eventName))
            {
                var subscriptions = SubsManager.GetHandlersForEvent(eventName);

                using (var scope = ServiceProvider.CreateScope()) //Microsoft.Extensions.DependencyInjection.Abstractions
                {
                    foreach (var subscription in subscriptions)
                    {
                        var handler = ServiceProvider.GetService(subscription.HandleType);
                        if (handler == null) continue;

                        var eventType = SubsManager.GetEventTypeByName($"{evenBusConfig.EventNamePrefix}{eventName}{evenBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        //if(integrationEvent is IntegrationEvent)
                        //{
                        //    evenBusConfig.CorrelationIsSettter?.Invoke((integrationEvent as IntegrationEvent)).CorrelationId);
                        //}

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }

                processed = true;
            }

            return processed;
        }

        public abstract void Publish(IntegrationEvent @event);

        public abstract void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;

        public abstract void UnSubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
        
    }
}
