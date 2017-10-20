using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace PrancingTurtle.Helpers
{
    public class PlaceholderAttribute : DisplayNameAttribute
    {
        private string Text { get; set; }

        public PlaceholderAttribute(string text)
        {
            Text = text;
        }

        public override string DisplayName
        {
            get { return Text; }
        }
    }
}