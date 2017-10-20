using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrancingTurtle.Models.Shared
{
    public class SearchViewModel
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }

        public SearchViewModel()
        {

        }

        public SearchViewModel(string action, string controller)
        {
            ActionName = action;
            ControllerName = controller;
        }
    }
}