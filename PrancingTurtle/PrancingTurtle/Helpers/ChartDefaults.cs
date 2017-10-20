using System.Drawing;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;

namespace PrancingTurtle.Helpers
{
    /// <summary>
    /// Some of the defaults for our HighCharts. This class was built based on a dark-background.
    /// </summary>
    public static class ChartDefaults
    {
        public static Chart ChartInit
        {
            get
            {
                return new Chart
                {
                    DefaultSeriesType = ChartTypes.Area,
                    ZoomType = ZoomTypes.Xy,
                    Height = 500,
                    BackgroundColor = new BackColorOrGradient(new Gradient
                    {
                        LinearGradient = new[] { 0, 0, 0, 400 },
                        Stops = new object[,]
                        {
                            {0, Color.FromArgb(13, 255, 255, 255)},
                            {1, Color.FromArgb(13, 255, 255, 255)}
                        }
                    }),
                    Style = "color: '#fff'"
                };
            }

        }

        public static Credits Credits
        {
            get
            {
                return new Credits()
                {
                    Enabled = false,
                    Href = "#",
                    Text = "Hewi"
                };
            }
        }

        public static string TickPositioner
        {
            get
            {
                return @"function () { var positions = [], tick = Math.floor(this.dataMin), increment = Math.ceil((this.dataMax - this.dataMin) / 15);
                           for (; tick - increment <= this.dataMax; tick += increment) { positions.push(tick); } return positions; }";
            }
        }

        public static PlotOptionsArea AreaPlotOptions
        {
            get
            {
                return new PlotOptionsArea
                {
                    Stacking = Stackings.Normal,
                    LineColor = ColorTranslator.FromHtml("#666666"),
                    LineWidth = 0,
                    Marker = new PlotOptionsAreaMarker
                    {
                        Enabled = false
                    },
                    DataLabels = new PlotOptionsAreaDataLabels()
                    {
                        Color = Color.White
                    }
                };
            }
        }

        public static PlotOptionsSpline SplinePlotOptions
        {
            get
            {
                return new PlotOptionsSpline
                {
                    Stacking = Stackings.Normal,
                    //LineColor = ColorTranslator.FromHtml("#666666"),
                    LineWidth = 0,
                    Marker = new PlotOptionsSplineMarker()
                    {
                        Enabled = false
                    },
                    DataLabels = new PlotOptionsSplineDataLabels()
                    {
                        Color = Color.White
                    }
                };
            }
        }

        public static XAxisTitle XAxisTitle
        {
            get
            {
                return new XAxisTitle()
                {
                    Text = "Seconds Elapsed",
                    Style = "color: '#FFF'",
                };
            }
        }
    }
}