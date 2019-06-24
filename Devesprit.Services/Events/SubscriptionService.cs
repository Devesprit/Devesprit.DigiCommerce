using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using Devesprit.Data.Events;

namespace Devesprit.Services.Events
{
    public partial class SubscriptionService : ISubscriptionService
    {
        public virtual IList<IConsumer<T>> GetSubscriptions<T>() where T: IEvent
        {
            return AutofacDependencyResolver.Current.GetServices<IConsumer<T>>().ToList();
        }
    }
}
