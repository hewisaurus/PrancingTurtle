using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList.Mvc;

namespace PrancingTurtle.Helpers.PagedList
{
    public static class StandardRenderOptions
    {
        public static PagedListRenderOptions Standard
        {
            get
            {
                return new PagedListRenderOptions()
                {
                    MaximumPageNumbersToDisplay = 6,
                    ContainerDivClasses = new List<string>() { "floatright" },
                    DisplayItemSliceAndTotal = false,
                    ItemSliceAndTotalFormat = "Showing records {0}-{1} of {2}",
                    DisplayLinkToFirstPage = PagedListDisplayMode.Never,
                    DisplayLinkToLastPage = PagedListDisplayMode.Never,
                    DisplayEllipsesWhenNotShowingAllPageNumbers = false,
                    DisplayPageCountAndCurrentLocation = false,
                    PageCountAndCurrentLocationFormat = "Page {0} of {1}",
                    UlElementClasses = new List<string>() { "pagination pagination-blue" },
                    DisplayLinkToNextPage = PagedListDisplayMode.Always,
                    DisplayLinkToPreviousPage = PagedListDisplayMode.Always
                };

            }
        }

        public static PagedListRenderOptions LeftSide
        {
            get
            {
                return new PagedListRenderOptions()
                {
                    MaximumPageNumbersToDisplay = 0,
                    ContainerDivClasses = new List<string>() { "floatright" },
                    DisplayItemSliceAndTotal = true,
                    ItemSliceAndTotalFormat = "Showing records {0}-{1} of {2}",
                    DisplayLinkToFirstPage = PagedListDisplayMode.Never,
                    DisplayLinkToLastPage = PagedListDisplayMode.Never,
                    DisplayEllipsesWhenNotShowingAllPageNumbers = false,
                    DisplayPageCountAndCurrentLocation = true,
                    PageCountAndCurrentLocationFormat = "Page {0} of {1}",
                    UlElementClasses = new List<string>() { "pagination pagination-noborder" },
                    DisplayLinkToNextPage = PagedListDisplayMode.Never,
                    DisplayLinkToPreviousPage = PagedListDisplayMode.Never
                };

            }
        }
    }
}