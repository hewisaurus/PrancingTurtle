using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Database.Models;
using Database.MySQL;
using PagedList;
using PrancingTurtle.Helpers;

namespace PrancingTurtle.Models.ViewModels
{
    public class AbilityIndexVm
    {
        // Properties to hold search parameters
        public string SearchName { get; set; }
        public string SearchAbilityId { get; set; }
        public int? SearchClassId { get; set; }
        public int? SearchSoulId { get; set; }
        //public string SearchRank { get; set; }
        //public string SearchIcon { get; set; }
        public string SearchSoulPoints { get; set; }
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
        public string SortParamAbility { get; set; }
        public string SortParamClass { get; set; }
        public string SortParamSoul { get; set; }
        //public string SortParamRank { get; set; }
        //public string SortParamIcon { get; set; }
        public string SortParamSoulPoints { get; set; }
        // Sorting CSS Classes
        public string SortParamNameClass { get; set; }
        public string SortParamAbilityClass { get; set; }
        public string SortParamClassClass { get; set; }
        public string SortParamSoulClass { get; set; }
        //public string SortParamRankClass { get; set; }
        //public string SortParamIconClass { get; set; }
        public string SortParamSoulPointsClass { get; set; }

        public string DatabaseSortBy
        {
            get { return GetDatabaseSorting(); }
        }


        // Paged Items
        public IPagedList<Database.Models.Ability> PagedItems { get; set; }
        //Lists
        public List<PlayerClass> Classes { get; set; }
        public List<Soul> Souls { get; set; } 
        public SelectList PageSizes { get { return GetPageSizeList(); } }

        private SelectList GetPageSizeList()
        {
            return new SelectList(PagedListPageSizes.GetPageSizes(), PageSize);
        }

        public AbilityIndexVm()
        {
            PagedItems = null;
        }

        //public AbilityIndexVm(string sortOrder, string searchName, string searchAbilityId, string searchRank, string searchIcon, string searchSoulPoints,
        //    int? searchClassId, int? searchSoulId, int pageSize = 10, int pageNumber = 1)
        public AbilityIndexVm(string sortOrder, string searchName, string searchAbilityId, string searchSoulPoints,
           int? searchClassId, int? searchSoulId, int pageSize = 10, int pageNumber = 1)
        {
            PageSize = pageSize;
            Page = pageNumber;

            if (!string.IsNullOrEmpty(sortOrder)) SortOrder = sortOrder;
            if (!string.IsNullOrEmpty(searchName)) SearchName = searchName;
            if (!string.IsNullOrEmpty(searchAbilityId)) SearchAbilityId = searchAbilityId;
            //if (!string.IsNullOrEmpty(searchRank)) SearchRank = searchRank;
            //if (!string.IsNullOrEmpty(searchIcon)) SearchIcon = searchIcon;
            if (!string.IsNullOrEmpty(searchSoulPoints)) SearchSoulPoints = searchSoulPoints;
            if (searchClassId != null) SearchClassId = searchClassId;
            if (searchSoulId != null) SearchSoulId = searchSoulId;

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

            return "A.Name ASC";
        }

        public void SetSortParameters()
        {
            SortParamName = "Name";
            SortParamAbility = "Ability";
            SortParamClass = "Class";
            SortParamSoul = "Soul";
            //SortParamRank = "Rank";
            //SortParamIcon = "Icon";
            SortParamSoulPoints = "SoulPoints";

            switch (SortOrder)
            {
                case "Name":
                    SortParamName = "Name_Desc";
                    break;
                case "Ability":
                    SortParamAbility = "Ability_Desc";
                    break;
                case "Class":
                    SortParamClass = "Class_Desc";
                    break;
                case "Soul":
                    SortParamSoul = "Soul_Desc";
                    break;
                //case "Rank":
                //    SortParamRank = "Rank_Desc";
                //    break;
                //case "Icon":
                //    SortParamIcon = "Icon_Desc";
                //    break;
                case "SoulPoints":
                    SortParamSoulPoints = "SoulPoints_Desc";
                    break;
            }

            SetSortClasses();
        }
        public void SetSortClasses()
        {
            SortParamNameClass = DefaultSortingCssClasses.Default;


            if (SortOrder.Contains("Ability"))
            {
                SortParamAbilityClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
                    ? DefaultSortingCssClasses.Descending
                    : DefaultSortingCssClasses.Ascending;
                return;
            }
            if (SortOrder.Contains("Class"))
            {
                SortParamClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
                    ? DefaultSortingCssClasses.Descending
                    : DefaultSortingCssClasses.Ascending;
                return;
            }
            if (SortOrder.Contains("Soul"))
            {
                SortParamSoulClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
                    ? DefaultSortingCssClasses.Descending
                    : DefaultSortingCssClasses.Ascending;
                return;
            }
            //if (SortOrder.Contains("Rank"))
            //{
            //    SortParamRankClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
            //        ? DefaultSortingCssClasses.Descending
            //        : DefaultSortingCssClasses.Ascending;
            //    return;
            //}
            //if (SortOrder.Contains("Icon"))
            //{
            //    SortParamIconClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
            //        ? DefaultSortingCssClasses.Descending
            //        : DefaultSortingCssClasses.Ascending;
            //    return;
            //}
            if (SortOrder.Contains("SoulPoints"))
            {
                SortParamSoulPointsClass = SortOrder.IndexOf("_", StringComparison.Ordinal) > 0
                    ? DefaultSortingCssClasses.Descending
                    : DefaultSortingCssClasses.Ascending;
                return;
            }
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
                        return $"{AliasTs.Ability.Alias}.Name";
                    case "Ability":
                        return $"{AliasTs.Ability.Alias}.AbilityId";
                    case "Class":
                        return $"{AliasTs.Soul.Alias}.PlayerClassId";
                    case "Soul":
                        return $"{AliasTs.Ability.Alias}.SoulId";
                    //case "Rank":
                    //    return "A.RankNumber";
                    //case "Icon":
                    //    return "A.Icon";
                    case "SoulPoints":
                        return $"{AliasTs.Ability.Alias}.MinimumPointsInSoul";
                    default:
                        return null;
                }
            }
            return null;
        }
    }
}