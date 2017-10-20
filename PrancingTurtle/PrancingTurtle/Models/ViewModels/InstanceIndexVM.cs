using System;
using System.Web.Mvc;
using Database.Models;
using PagedList;
using PrancingTurtle.Helpers;

namespace PrancingTurtle.Models.ViewModels
{
    public class InstanceIndexVm
    {
        // Properties to hold search parameters
        public string SearchName { get; set; }
        // Paging stuff
        public int Page { get; set; }
        public int PageSize { get; set; }
        private string sortOrder { get; set; }
        public string SortOrder
        {
            get { return string.IsNullOrEmpty(sortOrder) ? "Name" : sortOrder; }
            set { sortOrder = value; }
        }
        // Sorting Parameters
        public string SortParamName { get; set; }
        // Sorting CSS Classes
        public string SortParamNameClass { get; set; }

        public string DatabaseSortBy
        {
            get { return GetDatabaseSorting(); }
        }


        // Paged Items
        public IPagedList<Instance> PagedItems { get; set; }
        //Lists
        public SelectList PageSizes { get { return GetPageSizeList(); } }

        private SelectList GetPageSizeList()
        {
            return new SelectList(PagedListPageSizes.GetPageSizes(), PageSize);
        }

        public InstanceIndexVm()
        {
            PagedItems = null;
        }

        public InstanceIndexVm(string sortOrder, string searchName, int pageSize = 25, int pageNumber = 1)
        {
            PageSize = pageSize;
            Page = pageNumber;

            if (!string.IsNullOrEmpty(sortOrder)) SortOrder = sortOrder;
            if (!string.IsNullOrEmpty(searchName)) SearchName = searchName;

            PagedItems = null;

            SetSortParameters();
        }

        private string GetDatabaseSorting()
        {
            int underscoreIndex = SortOrder.IndexOf("_", StringComparison.Ordinal);
            string sortDirection = underscoreIndex > 0 ? "DESC" : "ASC";

            string column = GetDatabaseColumnName(SortOrder.Replace("_Desc", ""));
            if (!string.IsNullOrEmpty(column))
            {
                return string.Format("{0} {1}", column, sortDirection);
            }

            return "Name ASC";
        }

        public void SetSortParameters()
        {
            SortParamName = "Name";

            switch (SortOrder)
            {
                case "Name":
                    SortParamName = "Name_Desc";
                    break;
            }

            SetSortClasses();
        }
        public void SetSortClasses()
        {
            SortParamNameClass = DefaultSortingCssClasses.Default;

            if (SortOrder.Contains("Name"))
            {
                SortParamNameClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
                    ? DefaultSortingCssClasses.Descending
                    : DefaultSortingCssClasses.Ascending;
                return;
            }
        }
        public string GetDatabaseColumnName(string columnName)
        {
            if (!string.IsNullOrEmpty(columnName))
            {
                switch (columnName)
                {
                    case "Name":
                        return "Name";
                    default:
                        return null;
                }
            }
            return null;
        }
    }
}