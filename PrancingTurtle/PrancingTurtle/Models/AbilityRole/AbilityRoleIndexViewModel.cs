using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Database.MySQL;
using PagedList;
using PrancingTurtle.Helpers;

namespace PrancingTurtle.Models.AbilityRole
{
    public class AbilityRoleIndexViewModel
    {
        // Globals
        public string ControllerName = "AbilityRole";
        public string ActionName = "Index";

        // Properties to hold search parameters
        [Placeholder("Filter by ability id")]
        public string SearchAbilityLogId { get; set; }
        [Placeholder("Filter by ability name")]
        public string SearchAbilityName { get; set; }
        [Placeholder("Filter by soul")]
        public string SearchSoul { get; set; }
        public int? SearchClassId { get; set; }
        public int? SearchRoleId { get; set; }

        // Link Parameters
        public object PagerLinkParams(int page)
        {
            return new
            {
                so = SortOrder,
                sai = SearchAbilityLogId,
                san = SearchAbilityName,
                ss = SearchSoul,
                scid = SearchClassId,
                srid = SearchRoleId,
                ps = PageSize,
                pn = page
            };

        }
        public object AbilityIdLinkParams
        {
            get
            {
                return new
                {
                    so = SortParamAbilityLogId,
                    sai = SearchAbilityLogId,
                    san = SearchAbilityName,
                    ss = SearchSoul,
                    scid = SearchClassId,
                    srid = SearchRoleId,
                    ps = PageSize,
                    pn = Page
                };
            }
        }
        public object AbilityNameLinkParams
        {
            get
            {
                return new
                {
                    so = SortParamAbilityName,
                    sai = SearchAbilityLogId,
                    san = SearchAbilityName,
                    ss = SearchSoul,
                    scid = SearchClassId,
                    srid = SearchRoleId,
                    ps = PageSize,
                    pn = Page
                };
            }
        }
        public object ClassIdLinkParams
        {
            get
            {
                return new
                {
                    so = SortParamClassId,
                    sai = SearchAbilityLogId,
                    san = SearchAbilityName,
                    ss = SearchSoul,
                    scid = SearchClassId,
                    srid = SearchRoleId,
                    ps = PageSize,
                    pn = Page
                };
            }
        }
        public object RoleIdLinkParams
        {
            get
            {
                return new
                {
                    so = SortParamRoleId,
                    sai = SearchAbilityLogId,
                    san = SearchAbilityName,
                    ss = SearchSoul,
                    scid = SearchClassId,
                    srid = SearchRoleId,
                    ps = PageSize,
                    pn = Page
                };
            }
        }
        public object SoulLinkParams
        {
            get
            {
                return new
                {
                    so = SortParamSoul,
                    sai = SearchAbilityLogId,
                    san = SearchAbilityName,
                    ss = SearchSoul,
                    scid = SearchClassId,
                    srid = SearchRoleId,
                    ps = PageSize,
                    pn = Page
                };
            }
        }

        // Paging stuff
        public int Page { get; set; }
        public int PageSize { get; set; }
        private string sortOrder { get; set; }

        public string SortOrder
        {
            get { return string.IsNullOrEmpty(sortOrder) ? "Soul" : sortOrder; }
            set { sortOrder = value; }
        }

        // Sorting Parameters
        public string SortParamAbilityLogId { get; set; }
        public string SortParamAbilityName { get; set; }
        public string SortParamSoul { get; set; }
        public string SortParamClassId { get; set; }
        public string SortParamRoleId { get; set; }

        // Sorting CSS Classes
        public string SortParamAbilityLogIdClass { get; set; }
        public string SortParamAbilityNameClass { get; set; }
        public string SortParamSoulClass { get; set; }
        public string SortParamClassIdClass { get; set; }
        public string SortParamRoleIdClass { get; set; }

        public string DatabaseSortBy
        {
            get { return GetDatabaseSorting(); }
        }

        public Dictionary<string, object> Filters
        {
            get
            {
                var filters = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(SearchAbilityLogId))
                {
                    filters.Add(GetDatabaseColumnName("AbilityId"), SearchAbilityLogId);
                }
                if (!string.IsNullOrEmpty(SearchAbilityName))
                {
                    filters.Add(GetDatabaseColumnName("Name"), SearchAbilityName);
                }
                if (!string.IsNullOrEmpty(SearchSoul))
                {
                    filters.Add(GetDatabaseColumnName("Soul"), SearchSoul);
                }
                if (SearchClassId != null)
                {
                    filters.Add(GetDatabaseColumnName("Class"), SearchClassId);
                }
                if (SearchRoleId != null)
                {
                    filters.Add(GetDatabaseColumnName("Role"), SearchRoleId);
                }
                return filters;
            }
        }

        // Paged Items
        public IPagedList<Database.Models.AbilityRole> PagedItems { get; set; }
        public List<Database.Models.RoleIcon> Roles { get; set; }
        public List<Database.Models.PlayerClass> Classes { get; set; }

        public AbilityRoleIndexViewModel()
        {
            Roles = new List<Database.Models.RoleIcon>();
            Classes = new List<Database.Models.PlayerClass>();
        }
        public AbilityRoleIndexViewModel(string sortOrder, string abilityName, string abilityId, string soul, int? classId, int? roleId, int pageSize = 10, int pageNumber = 1)
        {
            PageSize = pageSize;
            Page = pageNumber;
            if (!string.IsNullOrEmpty(sortOrder)) SortOrder = sortOrder;
            if (!string.IsNullOrEmpty(abilityName)) SearchAbilityName = abilityName;
            if (!string.IsNullOrEmpty(abilityId)) SearchAbilityLogId = abilityId;
            if (!string.IsNullOrEmpty(soul)) SearchSoul = soul;
            if (classId != null) SearchClassId = classId;
            if (roleId != null) SearchRoleId = roleId;

            PagedItems = null;
            Roles = new List<Database.Models.RoleIcon>();
            Classes = new List<Database.Models.PlayerClass>();

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

            return "Soul ASC";
        }

        public void SetSortParameters()
        {
            SortParamAbilityName = ViewModelMethods.SortDirection("Name", SortOrder);
            SortParamAbilityNameClass = DefaultSortingCssClasses.CheckSortIcons(SortOrder, "Name");

            SortParamAbilityLogId = ViewModelMethods.SortDirection("AbilityId", SortOrder);
            SortParamAbilityLogIdClass = DefaultSortingCssClasses.CheckSortIcons(SortOrder, "AbilityId");

            SortParamSoul = ViewModelMethods.SortDirection("Soul", SortOrder);
            SortParamSoulClass = DefaultSortingCssClasses.CheckSortIcons(SortOrder, "Soul");

            SortParamClassId = ViewModelMethods.SortDirection("Class", SortOrder);
            SortParamClassIdClass = DefaultSortingCssClasses.CheckSortIcons(SortOrder, "Class");

            SortParamRoleId = ViewModelMethods.SortDirection("Role", SortOrder);
            SortParamRoleIdClass = DefaultSortingCssClasses.CheckSortIcons(SortOrder, "Role");
        }

        public string GetDatabaseColumnName(string columnName)
        {
            if (!string.IsNullOrEmpty(columnName))
            {
                switch (columnName)
                {
                    case "AbilityId":
                        return string.Format("{0}.AbilityLogId", AliasTs.AbilityRole.Alias);
                    case "Soul":
                        return string.Format("{0}.Soul", AliasTs.AbilityRole.Alias);
                    case "Class":
                        return string.Format("{0}.Id", AliasTs.PlayerClass.Alias);
                    case "Role":
                        return string.Format("{0}.Id", AliasTs.RoleIcon.Alias);
                    case "Name":
                    default:
                        return string.Format("{0}.AbilityName", AliasTs.AbilityRole.Alias);
                }
            }
            return null;
        }
    }
}