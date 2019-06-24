using System.Collections.Generic;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Hangfire.Storage;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IBackgroundJobModelFactory
    {
        BackgroundJobModel PrepareBackgroundJobModel(RecurringJobDto job, HashSet<string> pausedJobs);
    }
}