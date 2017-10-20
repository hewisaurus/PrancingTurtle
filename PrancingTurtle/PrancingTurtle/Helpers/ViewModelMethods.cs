using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrancingTurtle.Helpers
{
    public class ViewModelMethods
    {
        public static string SortDirection(string sortText, string sortOrder)
        {
            string sortDirection = sortText;

            if (sortOrder == sortDirection)
            {
                sortDirection = string.Format("{0}_Desc", sortDirection);
            }

            return sortDirection;
        }
    }
}