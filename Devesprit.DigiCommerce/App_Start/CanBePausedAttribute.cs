using Hangfire.Common;
using Hangfire.Server;

namespace Devesprit.DigiCommerce
{
    public class CanBePausedAttribute : JobFilterAttribute, IServerFilter
    {
        public void OnPerforming(PerformingContext filterContext)
        {
            var values = filterContext.Connection.GetAllItemsFromSet("paused-jobs");
            if (values.Contains(filterContext.BackgroundJob.Job.ToString()))
            {
                filterContext.Canceled = true;
            }
        }

        public void OnPerformed(PerformedContext filterContext)
        {
        }
    }
}