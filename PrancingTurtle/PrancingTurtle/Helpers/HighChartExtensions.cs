using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Database.QueryModels;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Ability = PrancingTurtle.Models.Ability;

namespace PrancingTurtle.Helpers
{
    public static class HighChartExtensions
    {
        public static Color GetColor(string name)
        {
            Color returnValue;

            switch (name)
            {
                    // Classes
                case "Rogue":
                    returnValue = ColorTranslator.FromHtml("#e5ca26");
                    break;
                case "Warrior":
                    returnValue = ColorTranslator.FromHtml("#be393a");
                    break;
                case "Mage":
                    returnValue = ColorTranslator.FromHtml("#9d6fba");
                    break;
                case "Cleric":
                    returnValue = ColorTranslator.FromHtml("#77ef00");
                    break;
                case "Primalist":
                    returnValue = ColorTranslator.FromHtml("#368dee");
                    break;
                    // Planes
                case "Air":
                    returnValue = ColorTranslator.FromHtml("#68a9f3");
                    break;
                case "Fire":
                    returnValue = ColorTranslator.FromHtml("#fc4e2a");
                    break;
                case "Death":
                    returnValue = ColorTranslator.FromHtml("#624171");
                    break;
                case "Life":
                    returnValue = ColorTranslator.FromHtml("#4da050");
                    break;
                case "Earth":
                    returnValue = ColorTranslator.FromHtml("#b9b960");
                    break;
                case "Water":
                    returnValue = ColorTranslator.FromHtml("#425e83");
                    break;
                case "Physical":
                    returnValue = ColorTranslator.FromHtml("#584f4e");
                    break;
                default:
                    returnValue = Color.Black;
                    break;
            }
            return returnValue;
        }

        public static GlobalOptions Options
        {
            get { return GetGlobalOptions(); }
        }

        private static GlobalOptions GetGlobalOptions()
        {
            return new GlobalOptions
            {
                Colors = ChartColors.ColorArray()
            };
        }

        /// <summary>
        /// Return a series from a list of damage by plane in a format that we can use in a pie chart
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Series ToPieChartSeries(this List<DetailDamageByPlane> data)
        {
            var returnValue = new Series { Type = ChartTypes.Pie };
            var damageTypeValues = data.GroupBy(a => a.DamageType)
                .ToDictionary(plane => plane.Key, plane => plane.Sum(a => a.TotalDamage));

            returnValue.Data = new Data(damageTypeValues.Select(item =>
                new DotNet.Highcharts.Options.Point()
                {
                    Name = item.Key, 
                    Y = item.Value, 
                    Color = GetColor(item.Key), 
                    Sliced = false,
                }).Cast<object>().ToArray());

            return returnValue;
        }

        /// <summary>
        /// Return a series from a list of damage by class in a format that we can use in a pie chart
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Series ToPieChartSeries(this List<DetailDamageByClass> data)
        {
            var returnValue = new Series { Type = ChartTypes.Pie };
            var classValues = data.GroupBy(a => a.Class)
                .ToDictionary(cl => cl.Key, cl => cl.Sum(a => a.TotalDamage));

            returnValue.Data = new Data(classValues.Select(item =>
                new DotNet.Highcharts.Options.Point()
                {
                    Name = item.Key,
                    Y = item.Value,
                    Color = GetColor(item.Key),
                    Sliced = false,
                }).Cast<object>().ToArray());

            return returnValue;
        }

        public static object[] ToPieChartSeries(this List<Ability> data, bool isHealType = false)
        {
            object[] returnValue = isHealType
                ? data.Select(item => new object[] { item.Name.Replace("'", "\\\'"), item.TotalEffectiveHealing }).Cast<object>().ToArray()
                : data.Select(item => new object[] { item.Name.Replace("'", "\\\'"), item.TotalDamage }).Cast<object>().ToArray();
            return returnValue;
        }

        public static Series ToFullPieChartSeries(this Dictionary<string, long> data)
        {
            var returnValue = new Series { Type = ChartTypes.Pie };

            returnValue.Data = new Data(data.Where(d => d.Value > 0).Select(item =>
                new DotNet.Highcharts.Options.Point() { Name = item.Key, Y = item.Value }).Cast<object>().ToArray());

            return returnValue;
        }

        /// <summary>
        /// This returns the full Series object with the right colours for the types
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Series ToFullPieChartSeries(this List<Ability> data)
        {
            var returnValue = new Series { Type = ChartTypes.Pie };
            var damageTypeValues = data.GroupBy(a => a.DamageType)
                .ToDictionary(ability => ability.Key, ability => ability.Sum(a => a.TotalDamage));

            returnValue.Data = new Data(damageTypeValues.Select(item =>
                new DotNet.Highcharts.Options.Point() { Name = item.Key, Y = item.Value, Color = GetColor(item.Key), Sliced = false }).Cast<object>().ToArray());

            return returnValue;
        }

        public static Series HealingToFullPieChartSeries(this List<Ability> data)
        {
            var returnValue = new Series { Type = ChartTypes.Pie };

            var effectiveVsOverheal = new Dictionary<string, long>
            {
                {"Healing", data.Sum(d => d.TotalEffectiveHealing)},
                {"Overhealing", data.Sum(d => d.TotalHealing)}
            };

            returnValue.Data = new Data(effectiveVsOverheal.Select(item =>
                new DotNet.Highcharts.Options.Point() { Name = item.Key, Y = item.Value }).Cast<object>().ToArray());

            return returnValue;
        }

    }
}