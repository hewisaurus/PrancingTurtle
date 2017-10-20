using System;

namespace PrancingTurtle.Helpers
{
    public static class DefaultSortingCssClasses
    {
        public static string Default = "fa fa-sort";
        public static string Ascending = "fa fa-sort-asc";
        public static string Descending = "fa fa-sort-desc";

        public static string CheckSortIcons(string sortOrder, string toCheckFor)
        {
            var sortClass = Default;

            if (sortOrder.Contains(toCheckFor))
            {
                sortClass = sortOrder.IndexOf("_", StringComparison.Ordinal) > 0 ? Descending : Ascending;
            }

            return sortClass;
        }
    }
}