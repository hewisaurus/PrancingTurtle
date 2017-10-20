using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database.Models;
using Database.Repositories.Interfaces;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Microsoft.AspNet.Identity;
using Common;
using PrancingTurtle.Helpers;
using PrancingTurtle.Helpers.Charts;
using PrancingTurtle.Models.ViewModels.BossFight;
using PrancingTurtle.Models.ViewModels.Records;
using Logging;

namespace PrancingTurtle.Controllers
{
    public class RecordsController : Controller
    {
        private readonly ILogger _logger;
        private readonly IBossFightRepository _bossFightRepository;
        private readonly IEncounterDifficultyRepository _difficultyRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IEncounterRepository _encounterRepository;
        private readonly IRecordsRepository _recordsRepository;
        private readonly IGuildRepository _guildRepository;
        private readonly IAuthenticationRepository _authRepository;
        private readonly IAuthUserCharacterRepository _authUserCharacterRepository;

        private readonly IRecordCharts _recordCharts;

        public RecordsController(IEncounterDifficultyRepository difficultyRepository, ILogger logger,
            ISessionRepository sessionRepository, IBossFightRepository bossFightRepository,
            IEncounterRepository encounterRepository, IRecordsRepository recordsRepository,
            IGuildRepository guildRepository, IRecordCharts recordCharts, 
            IAuthenticationRepository authRepository, IAuthUserCharacterRepository authUserCharacterRepository)
        {
            _difficultyRepository = difficultyRepository;
            _logger = logger;
            _sessionRepository = sessionRepository;
            _bossFightRepository = bossFightRepository;
            _encounterRepository = encounterRepository;
            _recordsRepository = recordsRepository;
            _guildRepository = guildRepository;
            _recordCharts = recordCharts;
            _authRepository = authRepository;
            _authUserCharacterRepository = authUserCharacterRepository;
        }

        // GET: Records
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Guild(int id = -1, int d = -1, int g = -1)
        {
            #region Validation
            if (id == -1)
            {
                return View("InvalidResource", model: "bossfight");
            }

            var difficulty = _difficultyRepository.GetDefaultDifficulty();
            if (difficulty == null)
            {
                return View("InvalidResource", model: "encounter difficulty");
            }

            if (d != -1)
            {
                var overrideDifficulty = _difficultyRepository.Get(d);
                if (overrideDifficulty != null)
                {
                    difficulty = overrideDifficulty;
                }
            }

            var bossFight = _bossFightRepository.Get(id);
            if (bossFight == null)
            {
                return View("InvalidResource", model: "bossfight");
            }

            var guild = _guildRepository.Get(g);
            if (guild == null)
            {
                return View("InvalidResource", model: "guild");
            }

            // Privacy
            if (!Request.IsAuthenticated && guild.HideFromRankings)
            {
                return View("GuildPrivate");
            }
            if (guild.HideFromRankings)
            {
                bool sameGuild = false;
                // Check we have a valid user
                var user = _authRepository.GetUserAccount(User.Identity.GetUserId());
                if (user == null)
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Home");
                }

                // Get the characters for this user
                var userCharacters = _authUserCharacterRepository.GetCharacters(user.Email);
                if (userCharacters.Any(c => c.GuildId == guild.Id) || User.IsInRole(UserGroups.Admin))
                {
                    sameGuild = true;
                }

                if (!sameGuild)
                {
                    return View("GuildPrivate");
                }
            }

            #endregion

            var model = new GuildRecords()
            {
                GuildName = guild.Name,
                // Fight related
                BossFight = bossFight,
                Difficulty = difficulty,
                // General, not guild related
                SingleFastestKill = _encounterRepository.GetFastestKills(id, difficulty.Id).FirstOrDefault(), // Fix this later - use the method that only returns one

                // Player-based
                SingleTopAps = _recordsRepository.GetSingleTopAps(id, difficulty.Id),
                SingleTopDps = _recordsRepository.GetSingleTopDps(id, difficulty.Id),
                SingleTopHps = _recordsRepository.GetSingleTopHps(id, difficulty.Id),
                // Guild-based
                SingleTopApsGuild = _recordsRepository.GetSingleTopApsGuild(id, difficulty.Id),
                SingleTopDpsGuild = _recordsRepository.GetSingleTopDpsGuild(id, difficulty.Id),
                SingleTopHpsGuild = _recordsRepository.GetSingleTopHpsGuild(id, difficulty.Id),
                // This guild-related
                GuildHybridXpsOverTimeChart = _recordCharts.GuildHybridXpsOverTime(id, difficulty.Id, g, guild.Name, bossFight.Name),
                GuildDurationOverTimeChart = _recordCharts.GetEncounterDurationOverTime(id, difficulty.Id, g, guild.Name, bossFight.Name),
                GuildTopDps = _recordsRepository.GetTopGuildDps(id, g, difficulty.Id),
                GuildTopHps = _recordsRepository.GetTopGuildHps(id, g, difficulty.Id),
                GuildTopAps = _recordsRepository.GetTopGuildAps(id, g, difficulty.Id),
                PlayerDpsOverTimeChart = _recordCharts.GuildPlayerXpsOverTime(id, difficulty.Id, guild.Id, guild.Name, bossFight.Name, "DPS", 20),
                PlayerHpsOverTimeChart = _recordCharts.GuildPlayerXpsOverTime(id, difficulty.Id, guild.Id, guild.Name, bossFight.Name, "HPS", 20),
                PlayerApsOverTimeChart = _recordCharts.GuildPlayerXpsOverTime(id, difficulty.Id, guild.Id, guild.Name, bossFight.Name, "APS", 20),
            };

            if (System.IO.File.Exists(Server.MapPath(string.Format("~/Content/images/portrait/{0}.png", bossFight.PortraitFilename))))
            {
                model.LoadImage = true;
            }

            return View(model);
        }

        

        private Highcharts GuildKillTimers(int bossFightId, int difficultyId)
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


        
    }
}