using System.Collections.Generic;
using CronExpressionDescriptor;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Hangfire.Storage;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class BackgroundJobModelFactory : IBackgroundJobModelFactory
    {
        public virtual BackgroundJobModel PrepareBackgroundJobModel(RecurringJobDto job, HashSet<string> pausedJobs)
        {
            var result = new BackgroundJobModel();
            if (job == null) return result;

            result.Id = job.Id;
            result.Job = job.Job.ToString();
            result.TimeZoneId = job.TimeZoneId;
            result.Cron = job.Cron;
            result.NextExecution = job.NextExecution?.ToLocalTime().ToString("F") ?? "-";
            result.LastExecution = job.LastExecution?.ToLocalTime().ToString("F") ?? "-";
            result.LastExecutionState = job.LastJobState;
            result.LastJobId = job.LastJobId;
            result.CronDesc = ExpressionDescriptor.GetDescription(job.Cron);
            result.Paused = pausedJobs.Contains(job.Id);
            return result;
        }
    }
}