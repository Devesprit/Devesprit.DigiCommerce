using System;
using System.Linq;
using System.Web;
using Devesprit.Core.Plugin;
using Devesprit.Data.Events;
using Elmah;

namespace Devesprit.Services.Events
{
    public partial class EventPublisher : IEventPublisher
    {
        private readonly ISubscriptionService _subscriptionService;

        public EventPublisher(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        protected virtual void PublishToConsumer<T>(IConsumer<T> x, T eventMessage) where T : IEvent
        {
            //Ignore not installed plugins
            var plugin = FindPlugin(x.GetType());
            if (plugin != null && !plugin.Installed)
                return;

            try
            {
                x.HandleEvent(eventMessage);
            }
            catch (Exception exc)
            {
                //log error
                var logger = Elmah.ErrorLog.GetDefault(HttpContext.Current);
                //we put in to nested try-catch to prevent possible cyclic (if some error occurs)
                try
                {
                    logger.Log(new Error(exc));
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }

        protected virtual PluginDescriptor FindPlugin(Type providerType)
        {
            if (providerType == null)
                throw new ArgumentNullException(nameof(providerType));

            if (PluginManager.ReferencedPlugins == null)
                return null;

            foreach (var plugin in PluginManager.ReferencedPlugins)
            {
                if (plugin.ReferencedAssembly == null)
                    continue;

                if (plugin.ReferencedAssembly.FullName == providerType.Assembly.FullName)
                    return plugin;
            }

            return null;
        }

        public virtual void Publish<T>(T eventMessage) where T : IEvent
        {
            if (HttpContext.Current == null)
            {
                return;
            }
            var subscriptions = _subscriptionService.GetSubscriptions<T>();
            subscriptions.ToList().ForEach(x => PublishToConsumer(x, eventMessage));
        }
    }
}
