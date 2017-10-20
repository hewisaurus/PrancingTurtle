using System;
using System.Collections.Generic;
using Database.Models;

namespace PrancingTurtle.Models.ViewModels.SiteAdmin
{
    public class SAUsersVM
    {
        public IEnumerable<AuthUser> Users { get; set; }
        public TimeSpan TimeZoneOffset { get; set; }
    }
}