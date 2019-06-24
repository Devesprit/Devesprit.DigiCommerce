using System.Collections.Generic;
using Devesprit.Data.Events;

namespace Devesprit.Services.Events
{
    public partial interface ISubscriptionService
    {
        IList<IConsumer<T>> GetSubscriptions<T>() where T : IEvent;
    }
}
