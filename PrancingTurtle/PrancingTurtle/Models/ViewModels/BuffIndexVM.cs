using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Database.Models;
using PagedList;
using PrancingTurtle.Helpers;

namespace PrancingTurtle.Models.ViewModels
{
    public class BuffIndexVm
    {
        // Properties to hold search parameters
        public string SearchName { get; set; }
        public int? SearchBuffGroupId { get; set; }
        public bool? SearchIsPrimary { get; set; }
        // Paging stuff
        public int Page { get; set; }
        public int PageSize { get; set; }
        private string sortOrder { get; set; }
        public string SortOrder
        {
            get { return string.IsNullOrEmpty(sortOrder) ? "BuffGroup" : sortOrder; }
            set { sortOrder = value; }
        }
        // Sorting Parameters
        public string SortParamName { get; set; }
        public string SortParamBuffGroup { get; set; }
        public string SortParamIsPrimary { get; set; }
        // Sorting CSS Classes
        public string SortParamNameClass { get; set; }
        public string SortParamBuffGroupClass { get; set; }
        public string SortParamIsPrimaryClass { get; set; }

        public string DatabaseSortBy
        {
            get { return GetDatabaseSorting(); }
        }

        // Paged Items
        public IPagedList<Database.Models.Buff> PagedItems { get; set; }
        //Lists
        public List<BuffGroup> BuffGroups { get; set; }
        public List<BooleanSearchFilter> BooleanFilters { get { return BooleanSearchFilters.GetFilters(false); } }
        public SelectList PageSizes { get { return GetPageSizeList(); } }

        private SelectList GetPageSizeList()
        {
            return new SelectList(PagedListPageSizes.GetPageSizes(), PageSize);
        }

        public BuffIndexVm()
        {
            PagedItems = null;
        }

        public BuffIndexVm(string sortOrder, string searchName, int? searchBuffGroupId, bool? searchIsPrimary, int pageSize = 10, int pageNumber = 1)
        {
            PageSize = pageSize;
            Page = pageNumber;

            if (!string.IsNullOrEmpty(sortOrder)) SortOrder = sortOrder;
            if (!string.IsNullOrEmpty(searchName)) SearchName = searchName;
            if (searchBuffGroupId != null) SearchBuffGroupId = searchBuffGroupId;
            if (searchIsPrimary != null) SearchIsPrimary = searchIsPrimary;

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

            return "BG.Name ASC";
        }

        public void SetSortParameters()
        {
            SortParamName = "Name";
            SortParamBuffGroup = "BuffGroup";
            SortParamIsPrimary = "IsPrimary";

            switch (SortOrder)
            {
                case "Name":
                    SortParamName = "Name_Desc";
                    break;
                case "BuffGroup":
                    SortParamBuffGroup = "BuffGroup_Desc";
                    break;
                case "IsPrimary":
                    SortParamIsPrimary = "IsPrimary_Desc";
                    break;
            }

            SetSortClasses();
        }
        public void SetSortClasses()
        {
            SortParamNameClass = DefaultSortingCssClasses.Default;
            SortParamBuffGroupClass = DefaultSortingCssClasses.Default;
            SortParamIsPrimaryClass = DefaultSortingCssClasses.Default;

            if (SortOrder.Contains("Name"))
            {
                SortParamNameClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
                    ? DefaultSortingCssClasses.Descending
                    : DefaultSortingCssClasses.Ascending;
                return;
            }
            if (SortOrder.Contains("BuffGroup"))
            {
                SortParamBuffGroupClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
                    ? DefaultSortingCssClasses.Descending
                    : DefaultSortingCssClasses.Ascending;
                return;
            }
            if (SortOrder.Contains("IsPrimary"))
            {
                SortParamIsPrimaryClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
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
                    case "BuffGroup":
                        return "BG.Name";
                    case "Name":
                        return "B.Name";
                    case "IsPrimary":
                        return "B.IsPrimary";
                    default:
                        return null;
                }
            }
            return null;
        }
    }
}