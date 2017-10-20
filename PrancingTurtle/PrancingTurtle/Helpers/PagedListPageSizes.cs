using System.Collections.Generic;

namespace PrancingTurtle.Helpers
{
    public static class PagedListPageSizes
    {
        public static List<int> GetPageSizes()
        {
            return new List<int>() { 5, 10, 25, 50, 100, 250, 500, 1000, 5000 };
        }
    }
}