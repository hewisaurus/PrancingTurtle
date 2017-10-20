using System.Collections.Generic;

namespace PrancingTurtle.Helpers
{
    public class BooleanSearchFilter
    {
        public bool? Filter { get; set; }
        public string Name { get; set; }

        public BooleanSearchFilter()
        {

        }

        public BooleanSearchFilter(string name, bool? filter)
        {
            Name = name;
            Filter = filter;
        }
    }

    public static class BooleanSearchFilters
    {
        public static List<BooleanSearchFilter> GetFilters(bool useFriendlyNames)
        {
            List<BooleanSearchFilter> filters = new List<BooleanSearchFilter>();
            if (useFriendlyNames)
            {
                filters.Add(new BooleanSearchFilter("Yes or No", null));
                filters.Add(new BooleanSearchFilter("Yes", true));
                filters.Add(new BooleanSearchFilter("No", false));
            }
            else
            {
                filters.Add(new BooleanSearchFilter("", null));
                filters.Add(new BooleanSearchFilter("True", true));
                filters.Add(new BooleanSearchFilter("False", false));
            }
            return filters;
        }
    }
}