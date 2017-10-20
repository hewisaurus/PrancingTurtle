using System.Collections.Generic;
using System.Drawing;

namespace PrancingTurtle.Helpers
{
    public static class ChartColors
    {
        public static string WhiteTextStyle = "color: '#fff'";

        public static Color[] ColorArray()
        {
            var colorList = new List<Color>
            {
                ColorTranslator.FromHtml("#333333"), // 1
                ColorTranslator.FromHtml("#7cb5ec"), // 2
                ColorTranslator.FromHtml("#bc80bd"), // 3
                //ColorTranslator.FromHtml("#80b1d3"), // 4
                ColorTranslator.FromHtml("#fdb462"), // 5
                ColorTranslator.FromHtml("#b3de69"), // 6
                ColorTranslator.FromHtml("#fccde5"), // 7
                ColorTranslator.FromHtml("#d9d9d9"), // 8
                ColorTranslator.FromHtml("#fb8072"), // 9
                ColorTranslator.FromHtml("#ffed6f"), // 10
                ColorTranslator.FromHtml("#bebada"), // 11
                ColorTranslator.FromHtml("#434348"), // 12
                ColorTranslator.FromHtml("#90ed7d"), // 13
                ColorTranslator.FromHtml("#f7a35c"), // 14
                ColorTranslator.FromHtml("#8085e9"), // 15
                ColorTranslator.FromHtml("#f15c80"), // 16
                ColorTranslator.FromHtml("#e4d354"), // 17
                ColorTranslator.FromHtml("#8085e8"), // 18
                ColorTranslator.FromHtml("#8d4653"), // 19
                ColorTranslator.FromHtml("#91e8e1"), // 20
                ColorTranslator.FromHtml("#8dd3c7"), // 21
                ColorTranslator.FromHtml("#0606c5"), // 22
            };

            return colorList.ToArray();
        }

        public static Color[] ColorArrayBlackBg()
        {
            var colorList = new List<Color>
            {
                //ColorTranslator.FromHtml("#7cb5ec"), 
                ColorTranslator.FromHtml("#bc80bd"), 
                ColorTranslator.FromHtml("#fdb462"), 
                ColorTranslator.FromHtml("#b3de69"), 
                ColorTranslator.FromHtml("#fccde5"), 
                ColorTranslator.FromHtml("#d9d9d9"), 
                ColorTranslator.FromHtml("#fb8072"), 
                ColorTranslator.FromHtml("#ffed6f"), 
                ColorTranslator.FromHtml("#bebada"), 
                ColorTranslator.FromHtml("#434348"), 
                ColorTranslator.FromHtml("#90ed7d"), 
                ColorTranslator.FromHtml("#f7a35c"), 
                ColorTranslator.FromHtml("#8085e9"), 
                ColorTranslator.FromHtml("#f15c80"), 
                ColorTranslator.FromHtml("#e4d354"), 
                ColorTranslator.FromHtml("#8085e8"), 
                ColorTranslator.FromHtml("#8d4653"), 
                ColorTranslator.FromHtml("#91e8e1"), 
                ColorTranslator.FromHtml("#8dd3c7"), 
                ColorTranslator.FromHtml("#0606c5"), 
            };

            return colorList.ToArray();
        }
    }
}