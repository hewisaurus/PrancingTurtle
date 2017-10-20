using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Database.MySQL;

namespace PrancingTurtle.Helpers.DataTables
{
    public class CustomDataTableBuilder
    {
        public CustomDataTableParam Parameters { get; set; }
        public Dictionary<string, object> Filters { get; set; }
        public string OrderBy { get; set; }

        public CustomDataTableBuilder()
        {

        }

        public CustomDataTableBuilder(CustomDataTableParam parameters)
        {
            Parameters = parameters;

            Filters = new Dictionary<string, object>();
        }

        public void BuildFilterAndOrder(string controller, string action)
        {
            switch (controller)
            {
                #region BossFight
                case "BOSSFIGHT":
                    switch (action)
                    {
                        #region LOADBOSSFIGHTS
                        case "LOADBOSSFIGHTS":
                            Filters = new Dictionary<string, object>();
                            // Filters
                            if (!string.IsNullOrEmpty(Parameters.SearchValue))
                            {
                                Filters.Add($"{AliasTs.Instance.Alias}.Name", Parameters.SearchValue);
                                Filters.Add($"{AliasTs.BossFight.Alias}.Name", Parameters.SearchValue);
                            }
                            // Order - set default
                            OrderBy = $"{AliasTs.BossFight.Alias}.Name ASC";

                            switch (Parameters.SortColumn)
                            {
                                case "Instance":
                                    OrderBy = $"{AliasTs.Instance.Alias}.Name {Parameters.SortColumnDirection}";
                                    break;
                                case "FightName":
                                    OrderBy = $"{AliasTs.BossFight.Alias}.Name {Parameters.SortColumnDirection}";
                                    break;
                                //case "CreatedBy":
                                //    OrderBy = $"{AliasTs.Authentication.Alias}.Firstname {Parameters.SortColumnDirection}, {AliasTs.Authentication.Alias}.Lastname {Parameters.SortColumnDirection}";
                                //    break;
                                //case "ManufacturerName":
                                //    OrderBy = $"{AliasTs.Manufacturer.Alias}.Name {Parameters.SortColumnDirection}";
                                //    break;
                                //case "Created":
                                //    OrderBy = $"{AliasTs.Preset.Alias}.Created {Parameters.SortColumnDirection}";
                                //    break;
                                //case "PresetDescription":
                                //    OrderBy = $"{AliasTs.Preset.Alias}.ShortDescription {Parameters.SortColumnDirection}";
                                //    break;
                                //case "Downloads":
                                //    OrderBy = $"DownloadCount {Parameters.SortColumnDirection}";
                                //    break;
                            }
                            break;
                        #endregion
                    }
                    break;
                #endregion
            }
        }
    }
}