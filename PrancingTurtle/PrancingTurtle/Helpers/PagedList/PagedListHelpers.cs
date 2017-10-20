using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrancingTurtle.Helpers.PagedList
{
    public static class PagedListHelpers
    {
        public static List<int> GetPageSizes()
        {
            return new List<int>() { 5, 10, 25, 50, 100, 250, 500, 1000, 5000 };
        }

        public static SelectList PageSizeList(int currentPageSize)
        {
            return new SelectList(PageSizes, currentPageSize);
        }

        public static List<int> PageSizes
        {
            get
            {
                return new List<int>() { 5, 10, 25, 50, 100, 250, 500, 1000, 5000 };
            }
        }
    }
}