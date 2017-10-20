using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Database.Models;
using Database.Repositories.Interfaces;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;

namespace PrancingTurtle.Helpers.Charts
{
    public class RecordCharts : IRecordCharts
    {
        private readonly IEncounterRepository _encounterRepository;
        private readonly IRecordsRepository _recordsRepository;

        public RecordCharts(IEncounterRepository encounterRepository, IRecordsRepository recordsRepository)
        {
            _encounterRepository = encounterRepository;
            _recordsRepository = recordsRepository;
        }

        [Obsolete("This method is obsolete", true)]
        public Highcharts GuildKillTimers(int bossFightId, int difficultyId)
        {
            const string graphTitle = "Kill times by guild";

            var guildKillTimes = _encounterRepository.GetDateSortedKills(bossFightId, difficultyId);

            #region Build chart series
            // Determine the guild IDs of those we wish to show by default.
            // Default this chart to the 4 guilds with the lowest kill times
            var visibleGuildIds = new List<int>();
            var lowestTimeList = guildKillTimes.OrderBy(s => s.Duration.TotalSeconds);
            foreach (var guildKill in lowestTimeList.TakeWhile(kill => visibleGuildIds.Count != 4))
            {
                if (!visibleGuildIds.Contains((int)guildKill.GuildId))
                {
                    visibleGuildIds.Add((int)guildKill.GuildId);
                }
            }
            var seriesList = new List<Series>();
            var killGroup = guildKillTimes.OrderBy(g => g.UploadGuild.Name).GroupBy(e => new { e.GuildId, e.UploadGuild.Name });
            foreach (var guildKillGroup in killGroup)
            {
                var thisSeries = new Series { Name = guildKillGroup.Key.Name.Replace("'", "\\\'") };

                thisSeries.PlotOptionsSpline = visibleGuildIds.Contains((int)guildKillGroup.Key.GuildId)
                    ? new PlotOptionsSpline() { Visible = true }
                    : new PlotOptionsSpline() { Visible = false };

                thisSeries.Data = new Data(guildKillGroup.Select(guildKill => new object[] { guildKill.Date, guildKill.Duration.TotalSeconds }).ToArray());
                seriesList.Add(thisSeries);
            }

            var seriesArray = seriesList.ToArray();
            #endregion

            var chart =
                new Highcharts(string.Format("bf{0}f{1}gkt", bossFightId, difficultyId))
                .InitChart(new Chart
                {
                    DefaultSeriesType = ChartTypes.Spline,
                    ZoomType = ZoomTypes.Xy,
                    Height = 400,
                    BackgroundColor = new BackColorOrGradient(new Gradient
                    {
                        LinearGradient = new[] { 0, 0, 0, 400 },
                        Stops = new object[,]
                        {{ 0, Color.FromArgb(13, 255, 255, 255) },
                        { 1, Color.FromArgb(13, 255, 255, 255) }}
                    }),
                    Style = ChartColors.WhiteTextStyle
                })
                .SetCredits(ChartDefaults.Credits)
            .SetOptions(new GlobalOptions
            {
                Colors = ChartColors.ColorArray(),
                Global = new Global { UseUTC = false }
            })
            .SetTitle(new Title
            {
                Text = graphTitle,
                Style = ChartColors.WhiteTextStyle
            })
            .SetXAxis(new XAxis
            {
                Type = AxisTypes.Datetime,
                DateTimeLabelFormats = new DateTimeLabel { Month = "%e %b", Year = "%e %b", Day = "%e %b", Week = "%e %b" },
                LineColor = Color.White,
                TickColor = Color.White,
                Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle },


            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Text = "Encounter length (seconds)", Style = ChartColors.WhiteTextStyle },
                //Min = 0
                TickColor = Color.White,
                LineColor = Color.White,
                Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle },

            })
            .SetSeries(seriesArray)
                .SetExporting(new Exporting { Enabled = false })
                .SetLegend(new Legend() { ItemStyle = ChartColors.WhiteTextStyle, ItemHoverStyle = "color: '#bbb'" })
            ;
            return chart;
        }

        public Highcharts GuildHybridXpsOverTime(int bossFightId, int difficultyId, int guildId, string guildName, string bossFightName)
        {
            var graphTitle = string.Format("{0} vs {1}: Raid performance over time", guildName.Replace("'", "\\\'"), bossFightName);
            var stats = _recordsRepository.GetGuildStatsOverTimeHybrid(bossFightId, difficultyId, guildId);
            if (!stats.Any()) return null;

            #region Build chart series

            var dpsSeries = new Series() { Name = "Damage" };
            var dpsDataList = new List<object[]>();
            var hpsSeries = new Series() { Name = "Healing", PlotOptionsSpline = new PlotOptionsSpline() { Visible = false } };
            var hpsDataList = new List<object[]>();
            var apsSeries = new Series() { Name = "Absorption", PlotOptionsSpline = new PlotOptionsSpline() { Visible = false } };
            var apsDataList = new List<object[]>();
            var durationSeries = new Series()
            {
                Name = "Encounter duration",
                YAxis = "1",
                PlotOptionsSpline = new PlotOptionsSpline()
                {
                    Color = Color.FromArgb(255, 124, 181, 236),
                }
            };
            var durationDataList = new List<object[]>();

            foreach (var stat in stats)
            {
                dpsDataList.Add(new object[] { stat.Date, stat.AverageDps });
                hpsDataList.Add(new object[] { stat.Date, stat.AverageHps });
                apsDataList.Add(new object[] { stat.Date, stat.AverageAps });
                durationDataList.Add(new object[] { stat.Date, new DateTime().Add(stat.Duration) });
            }

            dpsSeries.Data = new Data(dpsDataList.ToArray());
            hpsSeries.Data = new Data(hpsDataList.ToArray());
            apsSeries.Data = new Data(apsDataList.ToArray());
            durationSeries.Data = new Data(durationDataList.ToArray());

            var chartSeries = new Series[] { dpsSeries, hpsSeries, apsSeries, durationSeries };
            #endregion

            #region Build Y-Axes

            var yAxes = new List<YAxis>();
            // Standard Y
            yAxes.Add(
                new YAxis
                {
                    Title = new YAxisTitle { Text = "Per second", Style = ChartColors.WhiteTextStyle },
                    TickColor = Color.White,
                    LineColor = Color.White,
                    Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle }
                });
            yAxes.Add(
                new YAxis
                {
                    Title = new YAxisTitle { Text = "Encounter duration", Style = ChartColors.WhiteTextStyle },
                    Type = AxisTypes.Datetime,
                    DateTimeLabelFormats = new DateTimeLabel { Hour = "%Mm %Ss", Minute = "%Mm %Ss", Second = "%Mm %Ss" },
                    TickColor = Color.White,
                    LineColor = Color.White,
                    Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle },
                    Opposite = true,
                });
            #endregion

            var chart = new Highcharts(string.Format("bf{0}d{1}g{2}hybrid", bossFightId, difficultyId, guildId))
            .InitChart(new Chart
            {
                DefaultSeriesType = ChartTypes.Spline,
                ZoomType = ZoomTypes.Xy,
                Height = 400,
                BackgroundColor = new BackColorOrGradient(new Gradient
                {
                    LinearGradient = new[] { 0, 0, 0, 400 },
                    Stops = new object[,]
                        {{ 0, Color.FromArgb(13, 255, 255, 255) },
                        { 1, Color.FromArgb(13, 255, 255, 255) }}
                }),
                Style = ChartColors.WhiteTextStyle
            })
                .SetCredits(ChartDefaults.Credits)
            .SetOptions(new GlobalOptions
            {
                Colors = ChartColors.ColorArrayBlackBg(),
                Global = new Global { UseUTC = false }
            })
            .SetTitle(new Title
            {
                Text = graphTitle,
                Style = ChartColors.WhiteTextStyle
            })
            .SetXAxis(new XAxis
            {
                Type = AxisTypes.Datetime,
                DateTimeLabelFormats = new DateTimeLabel { Month = "%e %b", Year = "%e %b", Day = "%e %b", Week = "%e %b" },
                LineColor = Color.White,
                TickColor = Color.White,
                Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle },
            })
            //.SetYAxis(new YAxis
            //{
            //    Title = new YAxisTitle { Text = "Per second", Style = ChartColors.WhiteTextStyle },
            //    TickColor = Color.White,
            //    LineColor = Color.White,
            //    Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle }
            //})
            .SetYAxis(yAxes.ToArray())

            // Combined tooltip
            // There's an if/else if/else statement in there to format the tooltips differently depending on the series name
            // The DPS chart has a tooltip that will convert the value to thousands / millions / billions etc.
            // Eg 1500000DPS will be 1.50M DPS

            .SetTooltip(new Tooltip()
            {
                Formatter =
            @"function() {
                var text = '';
                if(this.series.name == 'Encounter duration') 
                {
                    text = '<b>' + this.series.name +'</b><br/>' + Highcharts.dateFormat('%Mm %Ss',new Date(this.y));
                } else if (this.series.name == 'Healing')  {
                    text = '<b>' + this.y + '</b> HPS';
                } else if (this.series.name == 'Absorption') {
                    text = '<b>' + this.y + '</b> APS';
                } else {
                    var ret = '',
                    multi,
                    axis = this.series.yAxis,
                    numericSymbols = ['k', 'M', 'B', 'T', 'P', 'E'],
                    i = numericSymbols.length;
                    while (i-- && ret === '') {
                        multi = Math.pow(1000, i);
                        if (axis.tickInterval >= multi && numericSymbols[i] !== null) {
                            ret = '<b>' + Highcharts.numberFormat(this.y / multi / 1000, 2) + numericSymbols[i] + '</b> DPS';
                        }
                }
                return ret;
                }
                return text;
            }"
            })
            // Tooltip for XPS
            //.SetTooltip(new Tooltip() { ValueSuffix = " per second" })

            // Tooltip for Duration
            //.SetTooltip(new Tooltip { Formatter = @"function() { return  '<b>' + this.series.name +'</b><br/>' +
            //       Highcharts.dateFormat('%Mm %Ss',new Date(this.y)); }" })

            .SetSeries(chartSeries)
                .SetExporting(new Exporting { Enabled = false })
                .SetLegend(new Legend() { ItemStyle = ChartColors.WhiteTextStyle, ItemHoverStyle = "color: '#bbb'" })
                ;

            return chart;
        }

        public Highcharts GuildPlayerXpsOverTime(int bossFightId, int difficultyId, int guildId, string guildName, string bossFightName,
            string xpsType, int topX)
        {
            var graphTitle = string.Format("{0} vs {1}: Top {2} {3} over time", guildName.Replace("'", "\\\'"), bossFightName, topX, xpsType);
            var stats = _recordsRepository.GetTopXpsOverTime(bossFightId, difficultyId, guildId, xpsType);
            var yAxisText = "";
            var yAxisPlotLine = new YAxisPlotLines()
            {
                Color = Color.Red,
                Width = 1,
            };
            switch (xpsType)
            {
                case "APS":
                    yAxisText = "Absorption per second";
                    break;
                case "HPS":
                    yAxisText = "Healing per second";
                    break;
                default:
                case "DPS":
                    yAxisText = "Damage per second";
                    break;
            }

            if (!stats.Any()) return null;

            var seriesList = new List<Series>();
            #region Overall single top XPS series
            var topList = new List<EncounterPlayerStatistics>();
            EncounterPlayerStatistics prevStat = null;
            var statGroup = stats.GroupBy(g => g.Encounter.Date.ToShortDateString());
            #region Stat loop
            foreach (var statList in statGroup)
            {
                switch (xpsType)
                {
                    case "HPS":
                        var topHPSStat = statList.OrderByDescending(s => s.HPS).First();
                        if (prevStat == null)
                        {
                            topList.Add(topHPSStat);
                            prevStat = topHPSStat;
                        }
                        else
                        {
                            if (topHPSStat.HPS > prevStat.HPS)
                            {
                                topList.Add(topHPSStat);
                                prevStat = topHPSStat;
                            }
                        }
                        break;
                    case "APS":
                        var topAPSStat = statList.OrderByDescending(s => s.APS).First();
                        if (prevStat == null)
                        {
                            topList.Add(topAPSStat);
                            prevStat = topAPSStat;
                        }
                        else
                        {
                            if (topAPSStat.APS > prevStat.APS)
                            {
                                topList.Add(topAPSStat);
                                prevStat = topAPSStat;
                            }
                        }
                        break;
                    default:
                    case "DPS":
                        var topDPSStat = statList.OrderByDescending(s => s.DPS).First();
                        if (prevStat == null)
                        {
                            topList.Add(topDPSStat);
                            prevStat = topDPSStat;
                        }
                        else
                        {
                            if (topDPSStat.DPS > prevStat.DPS)
                            {
                                topList.Add(topDPSStat);
                                prevStat = topDPSStat;
                            }
                        }
                        break;
                }

            }
            #endregion

            var topSeries = new Series { Name = string.Format("Highest {0}", xpsType), PlotOptionsSpline = new PlotOptionsSpline() { Visible = true } };
            var topOverallList = new List<object[]>();
            foreach (var stat in topList)
            {
                switch (xpsType)
                {
                    case "APS":
                        topOverallList.Add(new object[] { stat.Encounter.Date, stat.APS });
                        break;
                    case "HPS":
                        topOverallList.Add(new object[] { stat.Encounter.Date, stat.HPS });
                        break;
                    default:
                    case "DPS":
                        topOverallList.Add(new object[] { stat.Encounter.Date, stat.DPS });
                        break;
                }
            }
            topSeries.Data = new Data(topOverallList.ToArray());
            seriesList.Add(topSeries);
            #endregion

            #region Top X unique players

            var orderedStats = new List<EncounterPlayerStatistics>();
            var topValue = 0L;
            switch (xpsType)
            {
                case "APS":
                    orderedStats = stats.OrderByDescending(s => s.APS).ToList();
                    topValue = topList.Last().APS;
                    break;
                case "HPS":
                    orderedStats = stats.OrderByDescending(s => s.HPS).ToList();
                    topValue = topList.Last().HPS;
                    break;
                default:
                case "DPS":
                    orderedStats = stats.OrderByDescending(s => s.DPS).ToList();
                    topValue = topList.Last().DPS;
                    break;
            }
            yAxisPlotLine.Value = topValue;
            yAxisPlotLine.Label = new YAxisPlotLinesLabel()
            {
                Text = string.Format("Top {0}: {1}", xpsType, topValue)
            };

            var topXPlayerIds = new List<int>();
            foreach (var orderedStat in orderedStats.TakeWhile(orderedStat => topXPlayerIds.Count != topX))
            {
                if (!topXPlayerIds.Contains(orderedStat.PlayerId))
                {
                    topXPlayerIds.Add(orderedStat.PlayerId);
                }
            }
            var top3VisiblePlayers = new List<int>();
            foreach (var orderedStat in orderedStats.TakeWhile(orderedStat => top3VisiblePlayers.Count != 3))
            {
                if (!top3VisiblePlayers.Contains(orderedStat.PlayerId))
                {
                    top3VisiblePlayers.Add(orderedStat.PlayerId);
                }
            }
            #endregion

            var topXPlayerStatGroup =
                stats.Where(s => topXPlayerIds.Contains(s.PlayerId))
                    .GroupBy(s => new { s.PlayerId, s.Player.Name })
                    .OrderBy(s => s.Key.Name);

            foreach (var playerStats in topXPlayerStatGroup)
            {
                var thisSeries = new Series() { Name = playerStats.Key.Name.Replace("'", "\\\'") };
                thisSeries.PlotOptionsSpline = top3VisiblePlayers.Contains(playerStats.Key.PlayerId)
                    ? new PlotOptionsSpline() { Visible = true }
                    : new PlotOptionsSpline() { Visible = false };
                var dataList = new List<object[]>();
                foreach (var playerRecord in playerStats.OrderBy(s => s.Encounter.Date))
                {
                    switch (xpsType)
                    {
                        case "APS":
                            dataList.Add(new object[] { playerRecord.Encounter.Date, playerRecord.APS });
                            break;
                        case "HPS":
                            dataList.Add(new object[] { playerRecord.Encounter.Date, playerRecord.HPS });
                            break;
                        default:
                        case "DPS":
                            dataList.Add(new object[] { playerRecord.Encounter.Date, playerRecord.DPS });
                            break;
                    }
                }

                thisSeries.Data = new Data(dataList.ToArray());
                seriesList.Add(thisSeries);
            }

            var chart =
                new Highcharts(string.Format("bf{0}d{1}{2}{3}", bossFightId, difficultyId, guildId, xpsType))
                .InitChart(new Chart
                {
                    DefaultSeriesType = ChartTypes.Spline,
                    ZoomType = ZoomTypes.Xy,
                    Height = 400,
                    BackgroundColor = new BackColorOrGradient(new Gradient
                    {
                        LinearGradient = new[] { 0, 0, 0, 400 },
                        Stops = new object[,]
                        {{ 0, Color.FromArgb(13, 255, 255, 255) },
                        { 1, Color.FromArgb(13, 255, 255, 255) }}
                    }),
                    Style = ChartColors.WhiteTextStyle
                })
                .SetCredits(ChartDefaults.Credits)
            .SetOptions(new GlobalOptions
            {
                Colors = ChartColors.ColorArrayBlackBg(),
                Global = new Global { UseUTC = false }
            })
            .SetTitle(new Title
            {
                Text = graphTitle,
                Style = ChartColors.WhiteTextStyle
            })
            .SetXAxis(new XAxis
            {
                Type = AxisTypes.Datetime,
                DateTimeLabelFormats = new DateTimeLabel { Month = "%e %b", Year = "%e %b", Day = "%e %b", Week = "%e %b" },
                LineColor = Color.White,
                TickColor = Color.White,
                Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle },
            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Text = yAxisText, Style = ChartColors.WhiteTextStyle },
                //Min = 0
                TickColor = Color.White,
                LineColor = Color.White,
                Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle },
                PlotLines = new[]
                {
                    yAxisPlotLine
                }
            })
            .SetSeries(seriesList.ToArray())
                .SetExporting(new Exporting { Enabled = false })
                .SetLegend(new Legend() { ItemStyle = ChartColors.WhiteTextStyle, ItemHoverStyle = "color: '#bbb'" })
            ;



            return chart;
        }

        public Highcharts GetEncounterDurationOverTime(int bossFightId, int difficultyId, int guildId, string guildName, string bossFightName)
        {
            var graphTitle = string.Format("{0} vs {1}: Encounter duration over time", guildName.Replace("'", "\\\'"), bossFightName);
            var stats = _recordsRepository.GetEncounterDurationOverTime(bossFightId, difficultyId, guildId);
            if (!stats.Any()) return null;

            #region Build chart series

            //var dpsSeries = new Series() { Name = "Damage" };
            //var dpsDataList = new List<object[]>();
            //var hpsSeries = new Series() { Name = "Healing", PlotOptionsSpline = new PlotOptionsSpline() { Visible = false } };
            //var hpsDataList = new List<object[]>();
            //var apsSeries = new Series() { Name = "Absorption", PlotOptionsSpline = new PlotOptionsSpline() { Visible = false } };
            //var apsDataList = new List<object[]>();
            var durationSeries = new Series() { Name = "Duration" };
            var durationDataList = new List<object[]>();

            foreach (var stat in stats)
            {
                //dpsDataList.Add(new object[] { stat.Date, stat.AverageDps });
                //hpsDataList.Add(new object[] { stat.Date, stat.AverageHps });
                //apsDataList.Add(new object[] { stat.Date, stat.AverageAps });
                durationDataList.Add(new object[] { stat.Date, new DateTime().Add(stat.Duration) });
            }

            //dpsSeries.Data = new Data(dpsDataList.ToArray());
            //hpsSeries.Data = new Data(hpsDataList.ToArray());
            //apsSeries.Data = new Data(apsDataList.ToArray());
            durationSeries.Data = new Data(durationDataList.ToArray());

            //var chartSeries = new Series[] { dpsSeries, hpsSeries, apsSeries };
            var chartSeries = durationSeries;
            #endregion

            var chart = new Highcharts(string.Format("bf{0}d{1}g{2}duration", bossFightId, difficultyId, guildId))
            .InitChart(new Chart
            {
                DefaultSeriesType = ChartTypes.Spline,
                ZoomType = ZoomTypes.Xy,
                Height = 400,
                BackgroundColor = new BackColorOrGradient(new Gradient
                {
                    LinearGradient = new[] { 0, 0, 0, 400 },
                    Stops = new object[,]
                        {{ 0, Color.FromArgb(13, 255, 255, 255) },
                        { 1, Color.FromArgb(13, 255, 255, 255) }}
                }),
                Style = ChartColors.WhiteTextStyle
            })
                .SetCredits(ChartDefaults.Credits)
            .SetOptions(new GlobalOptions
            {
                Colors = ChartColors.ColorArrayBlackBg(),
                Global = new Global { UseUTC = false }
            })
            .SetTitle(new Title
            {
                Text = graphTitle,
                Style = ChartColors.WhiteTextStyle
            })
            .SetXAxis(new XAxis
            {
                Type = AxisTypes.Datetime,
                DateTimeLabelFormats = new DateTimeLabel { Month = "%e %b", Year = "%e %b", Day = "%e %b", Week = "%e %b" },
                LineColor = Color.White,
                TickColor = Color.White,
                Labels = new XAxisLabels { Style = ChartColors.WhiteTextStyle },
            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Text = "Encounter time", Style = ChartColors.WhiteTextStyle },
                Type = AxisTypes.Datetime,
                DateTimeLabelFormats = new DateTimeLabel { Hour = "%Mm %Ss", Minute = "%Mm %Ss", Second = "%Mm %Ss" },
                //Min = 0
                TickColor = Color.White,
                LineColor = Color.White,
                Labels = new YAxisLabels { Style = ChartColors.WhiteTextStyle },
            })
            //.SetTooltip(new Tooltip() { ValueSuffix = " per second" })
            .SetTooltip(new Tooltip { Formatter = @"function() { return  '<b>' + this.series.name +'</b><br/>' +
                    Highcharts.dateFormat('%Mm %Ss',new Date(this.y)); }" })
            .SetSeries(chartSeries)
                .SetExporting(new Exporting { Enabled = false })
                .SetLegend(new Legend() { ItemStyle = ChartColors.WhiteTextStyle, ItemHoverStyle = "color: '#bbb'" })
                ;

            return chart;
        }
    }
}