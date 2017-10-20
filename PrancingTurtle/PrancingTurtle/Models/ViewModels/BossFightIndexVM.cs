using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Database.Models;
using PagedList;
using PrancingTurtle.Helpers;

namespace PrancingTurtle.Models.ViewModels
{
    public class BossFightIndexVm
    {
        // Properties to hold search parameters
        public string SearchName { get; set; }
        public int? SearchInstanceId { get; set; }
        public string SearchDpsCheck { get; set; }
        public bool? SearchRequiresSpecial { get; set; }
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
        public string SortParamInstance { get; set; }
        public string SortParamDpsCheck { get; set; }
        public string SortParamRequiresSpecial { get; set; }
        // Sorting CSS Classes
        public string SortParamNameClass { get; set; }
        public string SortParamInstanceClass { get; set; }
        public string SortParamDpsCheckClass { get; set; }
        public string SortParamRequiresSpecialClass { get; set; }

        public string DatabaseSortBy
        {
            get { return GetDatabaseSorting(); }
        }


        // Paged Items
        public IPagedList<Database.Models.BossFight> PagedItems { get; set; }
        public List<Instance> Instances { get; set; } 
        //Lists
        public SelectList PageSizes { get { return GetPageSizeList(); } }
        public List<BooleanSearchFilter> BooleanFilters { get { return BooleanSearchFilters.GetFilters(false); } }

        private SelectList GetPageSizeList()
        {
            return new SelectList(PagedListPageSizes.GetPageSizes(), PageSize);
        }

        public BossFightIndexVm()
        {
            Instances = new List<Instance>();
            PagedItems = null;
        }

        public BossFightIndexVm(string sortOrder, string searchName, string searchDpsCheck, int? searchInstanceId, bool? searchRequiresSpecial, int pageSize = 25, int pageNumber = 1)
        {
            PageSize = pageSize;
            Page = pageNumber;

            if (!string.IsNullOrEmpty(sortOrder)) SortOrder = sortOrder;
            if (!string.IsNullOrEmpty(searchName)) SearchName = searchName;
            if (!string.IsNullOrEmpty(searchDpsCheck)) SearchDpsCheck = searchDpsCheck;
            if (searchInstanceId != null) SearchInstanceId = searchInstanceId;
            if (searchRequiresSpecial != null) SearchRequiresSpecial = searchRequiresSpecial;

            Instances = new List<Instance>();
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

            return "I.Name, BF.Name ASC";
        }

        public void SetSortParameters()
        {
            SortParamName = "Name";
            SortParamDpsCheck = "Dps";
            SortParamInstance = "Instance";
            SortParamRequiresSpecial = "Special";

            switch (SortOrder)
            {
                case "Name":
                    SortParamName = "Name_Desc";
                    break;
                case "Dps":
                    SortParamDpsCheck = "Dps_Desc";
                    break;
                case "Instance":
                    SortParamInstance = "Instance_Desc";
                    break;
                case "Special":
                    SortParamRequiresSpecial = "Special_Desc";
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
            if (SortOrder.Contains("Dps"))
            {
                SortParamDpsCheckClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
                    ? DefaultSortingCssClasses.Descending
                    : DefaultSortingCssClasses.Ascending;
                return;
            }
            if (SortOrder.Contains("Instance"))
            {
                SortParamInstanceClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
                    ? DefaultSortingCssClasses.Descending
                    : DefaultSortingCssClasses.Ascending;
                return;
            }
            if (SortOrder.Contains("Special"))
            {
                SortParamRequiresSpecialClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
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
                        return "BF.Name";
                    case "Dps":
                        return "BF.DpsCheck";
                    case "Instance":
                        return "BF.InstanceId";
                    case "Special":
                        return "BF.RequiresSpecialProcessing";
                    default:
                        return null;
                }
            }
            return null;
        }
    }
}