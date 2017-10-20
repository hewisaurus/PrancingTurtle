using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Database.Repositories;
using Database.Repositories.Interfaces;
using StructureMap;

namespace PrancingTurtle.DependencyResolution.Registries
{
    public class AuthenticationRegistry : Registry
    {
        public AuthenticationRegistry()
        {
            For<IAuthenticationRepository>().Use<AuthenticationRepository>();
        }
    }
}