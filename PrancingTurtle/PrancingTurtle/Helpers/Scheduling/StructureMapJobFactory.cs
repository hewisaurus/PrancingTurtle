using System;
using Quartz;
using Quartz.Spi;
using StructureMap;

namespace PrancingTurtle.Helpers.Scheduling
{
    public class StructureMapJobFactory : IJobFactory
    {
        private readonly IContainer _container;

        public StructureMapJobFactory(IContainer container)
        {
            _container = container;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                return (IJob)_container.GetInstance(bundle.JobDetail.JobType);
            }
            catch (Exception e)
            {
                var se = new SchedulerException("Problem instantiating class", e);
                throw se;
            }
        }

        public void ReturnJob(IJob job)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var disposable = job as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}