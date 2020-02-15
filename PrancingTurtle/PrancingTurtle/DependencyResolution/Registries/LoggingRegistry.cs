using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Logging;
using StructureMap;

namespace PrancingTurtle.DependencyResolution.Registries
{
    public class LoggingRegistry : Registry
    {
        public LoggingRegistry()
        {
            For<ILogger>().Use<NLogHandler>();
        }
    }
}