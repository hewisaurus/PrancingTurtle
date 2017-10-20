using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace PrancingTurtle.Helpers.DataTables
{
    public class CustomDataTableParam
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }

        public string SortColumn { get; set; }
        public string SortColumnDirection { get; set; }
        public string SearchValue { get; set; }

        public int PageSize => Length > 0 ? Length : 5;
        public int Skip => Start > 0 ? Start : 0;

        public CustomDataTableParam()
        {

        }

        public CustomDataTableParam(NameValueCollection collection)
        {
            var drawValue = collection["draw"];
            if (!string.IsNullOrEmpty(drawValue))
            {
                Draw = Int32.Parse(drawValue);
            }
            var startValue = collection["start"];
            if (!string.IsNullOrEmpty(startValue))
            {
                Start = Int32.Parse(startValue);
            }
            var lengthValue = collection["length"];
            if (!string.IsNullOrEmpty(lengthValue))
            {
                Length = Int32.Parse(lengthValue);
            }

            var sortColNumber = collection["order[0][column]"];
            SortColumn = collection[$"columns[{sortColNumber}][name]"];
            SortColumnDirection = collection["order[0][dir]"].ToUpper();
            SearchValue = collection["search[value]"];
        }
    }
}