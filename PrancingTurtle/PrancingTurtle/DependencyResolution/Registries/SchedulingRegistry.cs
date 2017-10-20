using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
using StructureMap;

namespace PrancingTurtle.DependencyResolution.Registries
{
    public class SchedulingRepository : Registry
    {
        public SchedulingRepository()
        {
            For<IScheduler>().Use(ctx => new StdSchedulerFactory().GetScheduler());
        }
    }
}