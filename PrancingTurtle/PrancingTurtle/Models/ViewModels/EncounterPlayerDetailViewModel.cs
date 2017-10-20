using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Database.Models;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using PrancingTurtle.Helpers;

namespace PrancingTurtle.Models.ViewModels
{
    public class EncounterPlayerDetailViewModel
    {
        public int EncounterId { get; set; }
        public Player Player { get; set; }
        public string EncounterName { get; set; }
        public string InstanceName { get; set; }
        public TimeSpan EncounterLength { get; set; }
        public DateTime EncounterStart { get; set; }
        public bool EncounterSuccess { get; set; }
        public AbilityBreakdown DamageOutgoingAbilityBreakdown { get; set; }
        public AbilityBreakdown DamageIncomingAbilityBreakdown { get; set; }
        public AbilityBreakdown HealingOutgoingAbilityBreakdown { get; set; }
        public AbilityBreakdown HealingOutgoingTypeBreakdown { get; set; }
        public AbilityBreakdown HealingIncomingAbilityBreakdown { get; set; }
        /// <summary>
        /// We will have to remove this at some point and use CombatEntry
        /// </summary>
        public List<DamageDone> DamageRecords { get; set; }
        public List<HealingDone> HealingRecords { get; set; }
        public Highcharts DamageOutgoingPerSecondChart { get; set; }
        public Highcharts DamageOutgoingAbilityBreakdownChart { get; set; }
        public Highcharts DamageOutgoingTypeBreakdownChart { get; set; }
        public Highcharts DamageIncomingAbilityBreakdownChart { get; set; }
        public Highcharts DamageIncomingTypeBreakdownChart { get; set; }
        public Highcharts HealingOutgoingPerSecondChart { get; set; }
        public Highcharts HealingOutgoingAbilityBreakdownChart { get; set; }
        public Highcharts HealingOutgoingTypeBreakdownChart { get; set; }
        public Highcharts HealingIncomingAbilityBreakdownChart { get; set; }
        public Highcharts HealingIncomingSourceBreakdownChart { get; set; }

        public EncounterPlayerDetailViewModel()
        {
            DamageRecords = new List<DamageDone>();
            HealingRecords = new List<HealingDone>();
            DamageOutgoingAbilityBreakdown = new AbilityBreakdown();
            DamageIncomingAbilityBreakdown = new AbilityBreakdown();
            HealingOutgoingAbilityBreakdown = new AbilityBreakdown();
            HealingOutgoingTypeBreakdown = new AbilityBreakdown();
            HealingIncomingAbilityBreakdown = new AbilityBreakdown();
        }

        public void UpdateDetailedStats()
        {
            if (!DamageRecords.Any()) return;
            #region Loop through abilities
            #region Damage
            foreach (var dmgGroup in DamageRecords.GroupBy(d => d.Ability.Name))
            {
                long critDamage = 0;
                long hitDamage = 0;

                int crits = dmgGroup.Count(d => d.CriticalHit);
                int swings = dmgGroup.Count();
                int hits = swings - crits;
                decimal critrate = crits > 0 ? (((decimal)crits / (decimal)swings) * 100) : 0;

                Ability ability = new Ability
                {
                    //Name = isPetAbility ? string.Format("{0} (Pet)", dmgGroup.Key) : dmgGroup.Key,
                    Name = dmgGroup.Key,
                    IsPetAbility = !string.IsNullOrEmpty(dmgGroup.First().SourcePetName),
                    DamageType = dmgGroup.First().Ability.DamageType ?? "unknown",
                    Statistics = new AbilityStatistics
                    {
                        Swings = swings,
                        Hits = hits,
                        Crits = crits,
                        CritRate = critrate
                    },
                    IconPath = dmgGroup.First().Ability.Icon
                };

                foreach (var dmg in dmgGroup)
                {
                    if (dmg.CriticalHit)
                    {
                        #region Min Crit
                        if (ability.Statistics.MinCrit == 0 || dmg.TotalDamage < ability.Statistics.MinCrit)
                        {
                            ability.Statistics.MinCrit = dmg.TotalDamage;
                        }
                        #endregion
                        #region Max Crit
                        if (ability.Statistics.MaxCrit == 0 || dmg.TotalDamage > ability.Statistics.MaxCrit)
                        {
                            ability.Statistics.MaxCrit = dmg.TotalDamage;
                        }
                        #endregion

                        critDamage += dmg.TotalDamage;
                    }
                    else
                    {
                        #region Min Hit
                        if (ability.Statistics.MinHit == 0 || dmg.TotalDamage < ability.Statistics.MinHit)
                        {
                            ability.Statistics.MinHit = dmg.TotalDamage;
                        }
                        #endregion
                        #region Max Hit
                        if (ability.Statistics.MaxHit == 0 || dmg.TotalDamage > ability.Statistics.MaxHit)
                        {
                            ability.Statistics.MaxHit = dmg.TotalDamage;
                        }
                        #endregion

                        hitDamage += dmg.TotalDamage;
                    }
                }

                #region AverageHit
                ability.Statistics.AverageHit = hits > 0 ? hitDamage / hits : 0;
                #endregion
                #region AverageCrit
                ability.Statistics.AverageCrit = crits > 0 ? critDamage / crits : 0;
                #endregion

                ability.TotalDamage = critDamage + hitDamage;
                ability.DamagePerSecond = EncounterLength.TotalSeconds > 0 ? ability.TotalDamage / (long)EncounterLength.TotalSeconds : 0;

                if (dmgGroup.First().SourcePlayer == null || dmgGroup.First().SourcePlayerId != Player.Id)
                {
                    // An NPC or other player dealt damage to this player
                    DamageIncomingAbilityBreakdown.Abilities.Add(ability);
                }
                else
                {
                    if (dmgGroup.First().TargetPlayerId == null) // Ignore damage to other players
                    {
                        DamageOutgoingAbilityBreakdown.Abilities.Add(ability);
                    }
                }
            }
            #endregion
            #region Healing
            var healSourceBreakdown = new Dictionary<string, long>();
            foreach (var healGroup in HealingRecords.GroupBy(h => h.Ability.Name))
            {
                long critHeal = 0;
                long hitHeal = 0;
                // Track total healing so the outgoing healing type breakdown is accurate
                long totalCritHeal = 0;
                long totalHitHeal = 0;

                int crits = healGroup.Where(h => h.EffectiveHealing > 0).Count(h => h.CriticalHit);
                int swings = healGroup.Count(h => h.EffectiveHealing > 0);
                int hits = swings - crits;
                decimal critrate = crits > 0 ? (((decimal)crits / (decimal)swings) * 100) : 0;

                Ability ability = new Ability
                {
                    Name = healGroup.Key,
                    IsPetAbility = !string.IsNullOrEmpty(healGroup.First().SourcePetName),
                    Statistics = new AbilityStatistics
                    {
                        Swings = swings,
                        Hits = hits,
                        Crits = crits,
                        CritRate = critrate
                    },
                    IconPath = healGroup.First().Ability.Icon
                };

                foreach (var heal in healGroup)
                {
                    if (heal.SourcePlayerId != null)
                    {
                        //if (heal.SourcePlayerId != Player.Id)
                        //{
                            if (healSourceBreakdown.ContainsKey(heal.SourcePlayer.Name))
                            {
                                healSourceBreakdown[heal.SourcePlayer.Name] += heal.EffectiveHealing;
                            }
                            else
                            {
                                healSourceBreakdown.Add(heal.SourcePlayer.Name, heal.EffectiveHealing);
                            }
                        //}
                    }
                    
                    if (heal.CriticalHit)
                    {
                        #region Min Crit
                        if (ability.Statistics.MinCrit == 0 || heal.EffectiveHealing < ability.Statistics.MinCrit)
                        {
                            if (heal.EffectiveHealing > 0)
                            {
                                ability.Statistics.MinCrit = heal.EffectiveHealing;
                            }
                        }
                        #endregion
                        #region Max Crit
                        if (ability.Statistics.MaxCrit == 0 || heal.EffectiveHealing > ability.Statistics.MaxCrit)
                        {
                            ability.Statistics.MaxCrit = heal.EffectiveHealing;
                        }
                        #endregion

                        critHeal += heal.EffectiveHealing;
                        totalCritHeal += heal.TotalHealing;
                    }
                    else
                    {
                        #region Min Hit
                        if (ability.Statistics.MinHit == 0 || heal.EffectiveHealing < ability.Statistics.MinHit)
                        {
                            if (heal.EffectiveHealing > 0)
                            {
                                ability.Statistics.MinHit = heal.EffectiveHealing;
                            }
                        }
                        #endregion
                        #region Max Hit
                        if (ability.Statistics.MaxHit == 0 || heal.EffectiveHealing > ability.Statistics.MaxHit)
                        {
                            ability.Statistics.MaxHit = heal.EffectiveHealing;
                        }
                        #endregion

                        hitHeal += heal.EffectiveHealing;
                        totalHitHeal += heal.TotalHealing;
                    }
                }

                #region AverageHit
                ability.Statistics.AverageHit = hits > 0 ? hitHeal / hits : 0;
                #endregion
                #region AverageCrit
                ability.Statistics.AverageCrit = crits > 0 ? critHeal / crits : 0;
                #endregion

                ability.TotalEffectiveHealing = critHeal + hitHeal;
                ability.TotalHealing = totalCritHeal + totalHitHeal;
                ability.EffectiveHealingPerSecond = EncounterLength.TotalSeconds > 0 ? ability.TotalEffectiveHealing / (long)EncounterLength.TotalSeconds : 0;
                if (ability.TotalEffectiveHealing > 0)
                {
                    if (healGroup.First().SourcePlayerId == Player.Id)
                    {
                        if (healGroup.First().TargetPlayerId == Player.Id)
                        {
                            // Self Healing
                            HealingIncomingAbilityBreakdown.Abilities.Add(new Ability(ability));
                            HealingOutgoingAbilityBreakdown.Abilities.Add(new Ability(ability));
                            HealingOutgoingTypeBreakdown.Abilities.Add(new Ability(ability));
                        }
                        else
                        {
                            // Healing someone else
                            HealingOutgoingAbilityBreakdown.Abilities.Add(new Ability(ability));
                            HealingOutgoingTypeBreakdown.Abilities.Add(new Ability(ability));
                        }
                    }
                    else if (healGroup.First().TargetPlayerId == Player.Id)
                    {
                        // Being healed by someone else
                        HealingIncomingAbilityBreakdown.Abilities.Add(new Ability(ability));
                    }
                }
            }
            #endregion
            #endregion
            #region Lists
            var secondsElapsed = new List<string>();
            var tDps = new List<object>(); // Total Damage
            var eDps = new List<object>(); // Effective Damage
            var aDps = new List<object>(); // Average DPS

            long totalDamageDone = 0;

            var tHps = new List<object>(); // Total Healing
            var eHps = new List<object>(); // Effective Healing
            var aHps = new List<object>(); // Average HPS

            long totalHealingDone = 0;
            #endregion
            #region Loop through the encounter events by second
            for (int i = 0; i <= EncounterLength.TotalSeconds; i++)
            {
                int secondElapsed = i;

                secondsElapsed.Add(secondElapsed.ToString(CultureInfo.InvariantCulture));
                #region Damage
                var damageThisSecond = DamageRecords
                    .Where(d => d.SecondsElapsed == secondElapsed &&
                                d.SourcePlayer != null).ToList();
                if (damageThisSecond.Any())
                {
                    long damageDealt = damageThisSecond.Sum(d => d.TotalDamage);
                    tDps.Add(damageDealt);
                    eDps.Add(damageThisSecond.Sum(d => d.EffectiveDamage));
                    totalDamageDone += damageDealt;
                }
                else
                {
                    tDps.Add(0);
                    eDps.Add(0);
                }

                if (secondElapsed > 0)
                {
                    aDps.Add((int)(totalDamageDone / secondElapsed));
                }
                else
                {
                    aDps.Add(0);
                }
                #endregion
                #region Healing
                var healingThisSecond = HealingRecords
                    .Where(h => h.SecondsElapsed == secondElapsed &&
                                h.SourcePlayerId == Player.Id).ToList();
                if (healingThisSecond.Any())
                {
                    long healingDone = healingThisSecond.Sum(d => d.EffectiveHealing);
                    tHps.Add(healingThisSecond.Sum(d => d.TotalHealing));
                    eHps.Add(healingDone);
                    totalHealingDone += healingDone;
                }
                else
                {
                    tHps.Add(0);
                    eHps.Add(0);
                }

                if (secondElapsed > 0)
                {
                    aHps.Add((int)(totalHealingDone / secondElapsed));
                }
                else
                {
                    aHps.Add(0);
                }
                #endregion
            }
            #endregion
            #region Create arrays from our lists
            var secondsArray = secondsElapsed.ToArray();

            var tDpsArray = tDps.ToArray();
            var eDpsArray = eDps.ToArray();
            var aDpsArray = aDps.ToArray();

            var tHpsArray = tHps.ToArray();
            var eHpsArray = eHps.ToArray();
            var aHpsArray = aHps.ToArray();
            #endregion
            #region Ability Percentages
            long totalDamageDealt = DamageOutgoingAbilityBreakdown.Abilities.Sum(a => a.TotalDamage);
            long totalDamageTaken = DamageIncomingAbilityBreakdown.Abilities.Sum(a => a.TotalDamage);
            foreach (var ability in DamageOutgoingAbilityBreakdown.Abilities)
            {
                ability.BreakdownPercentage = ability.TotalDamage > 0 ? ((decimal)ability.TotalDamage / (decimal)totalDamageDealt) * 100 : 0;
            }
            foreach (var ability in DamageIncomingAbilityBreakdown.Abilities)
            {
                ability.BreakdownPercentage = ability.TotalDamage > 0 ? ((decimal)ability.TotalDamage / (decimal)totalDamageTaken) * 100 : 0;
            }
            long totalHealingDealt = HealingOutgoingAbilityBreakdown.Abilities.Sum(a => a.TotalEffectiveHealing);
            long totalHealingTaken = HealingIncomingAbilityBreakdown.Abilities.Sum(a => a.TotalEffectiveHealing);
            foreach (var ability in HealingOutgoingAbilityBreakdown.Abilities)
            {
                ability.BreakdownPercentage = ability.TotalEffectiveHealing > 0 ? ((decimal)ability.TotalEffectiveHealing / (decimal)totalHealingDealt) * 100 : 0;
            }
            foreach (var ability in HealingIncomingAbilityBreakdown.Abilities)
            {
                ability.BreakdownPercentage = ability.TotalEffectiveHealing > 0 ? ((decimal)ability.TotalEffectiveHealing / (decimal)totalHealingTaken) * 100 : 0;
            }
            #endregion
            #region Update the charts
            #region DamageOutgoingPerSecond
            DamageOutgoingPerSecondChart = new Highcharts(string.Format("encounter{0}dps", EncounterId))
                    .InitChart(new Chart
                    {
                        DefaultSeriesType = ChartTypes.Line,
                        ZoomType = ZoomTypes.Xy,
                        Height = 300
                    })
                    .SetCredits(new Credits()
                    {
                        Enabled = true,
                        Href = "#",
                        Text = "Hewi@Laethys"
                    })
                    .SetTitle(new Title { Text = "Damage per second" })
                    .SetXAxis(
                        new XAxis
                        {
                            Categories = secondsArray,
                            MinTickInterval = (EncounterLength.TotalSeconds / 10),
                            Title = new XAxisTitle()
                            {
                                Text = "Seconds Elapsed"
                            }
                        })
                    .SetYAxis(new[]
                        {
                            new YAxis
                            {
                                Title = new YAxisTitle {Text = "Damage"},
                                Min = 0,
                                //PlotLines = new YAxisPlotLines[]
                                //{
                                //    plotLine
                                //}, 
                                
                            }
                        })
                    .SetPlotOptions(new PlotOptions
                    {
                        Line = new PlotOptionsLine
                        {
                            EnableMouseTracking = true,
                            Marker = new PlotOptionsLineMarker { Enabled = false }
                        }
                    })
                    .SetSeries(new[]
                        {
                            new Series
                            {
                                Name = "Average Damage Per Second",
                                Data = new Data(aDpsArray)
                            },
                            new Series
                            {
                                Name = "Total Damage",
                                Data = new Data(tDpsArray),
                                PlotOptionsLine = new PlotOptionsLine
                                {
                                    Visible = false
                                }
                            },
                            new Series
                            {
                                Name = "Effective Damage",
                                Data = new Data(eDpsArray)
                            }
                        })
                        .SetExporting(new Exporting { Enabled = false });
            #endregion
            #region HealingOutgoingPerSecond
            HealingOutgoingPerSecondChart = new Highcharts(string.Format("encounter{0}hps", EncounterId))
                    .InitChart(new Chart
                    {
                        DefaultSeriesType = ChartTypes.Line,
                        ZoomType = ZoomTypes.Xy,
                        Height = 300
                    })
                    .SetCredits(new Credits()
                    {
                        Enabled = true,
                        Href = "#",
                        Text = "Hewi@Laethys"
                    })
                    .SetTitle(new Title { Text = "Healing per second" })
                    .SetXAxis(
                        new XAxis
                        {
                            Categories = secondsArray,
                            MinTickInterval = (EncounterLength.TotalSeconds / 10),
                            Title = new XAxisTitle()
                            {
                                Text = "Seconds Elapsed"
                            }
                        })
                    .SetYAxis(new[]
                        {
                            new YAxis
                            {
                                Title = new YAxisTitle {Text = "Healing"},
                                Min = 0,
                            }
                        })
                    .SetPlotOptions(new PlotOptions
                    {
                        Line = new PlotOptionsLine
                        {
                            EnableMouseTracking = true,
                            Marker = new PlotOptionsLineMarker { Enabled = false }
                        }
                    })
                    .SetSeries(new[]
                        {
                            new Series
                            {
                                Name = "Average Healing Per Second",
                                Data = new Data(aHpsArray)
                            },
                            new Series
                            {
                                Name = "Total Healing",
                                Data = new Data(tHpsArray),
                                PlotOptionsLine = new PlotOptionsLine
                                {
                                    Visible = false
                                }
                            },
                            new Series
                            {
                                Name = "Effective Healing",
                                Data = new Data(eHpsArray)
                            }
                        })
                        .SetExporting(new Exporting { Enabled = false });
            #endregion
            #region DamageOutgoingAbilityBreakdownChart
            DamageOutgoingAbilityBreakdownChart =
                new Highcharts(string.Format("encounter{0}damageoutgoingbreakdown", EncounterId))
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Outgoing Damage Breakdown" })
               .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.percentage, 2) +' %'; }" })
               .SetPlotOptions(new PlotOptions
               {
                   Pie = new PlotOptionsPie
                   {
                       AllowPointSelect = true,
                       Cursor = Cursors.Pointer,
                       Size = new PercentageOrPixel(85, true)
                   }
               })
                .SetCredits(new Credits()
                {
                    Enabled = true,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
              .SetSeries(new Series
              {
                  Type = ChartTypes.Pie,
                  Data = new Data(DamageOutgoingAbilityBreakdown.Abilities.OrderBy(a => a.TotalDamage).ToList().ToPieChartSeries())
              });
            #endregion
            #region DamageOutgoingTypeBreakdownChart
            DamageOutgoingTypeBreakdownChart = new Highcharts(string.Format("encounter{0}damageoutgoingtypes", EncounterId))
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Outgoing Damage Types" })
                .SetTooltip(new Tooltip
                {
                    Formatter =
                        "function() { return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.percentage, 2) +' %'; }"
                })
                .SetPlotOptions(new PlotOptions
                {
                    Pie = new PlotOptionsPie
                    {
                        AllowPointSelect = true,
                        Cursor = Cursors.Pointer,
                        Size = new PercentageOrPixel(85, true)
                    }
                })
                .SetCredits(new Credits()
                {
                    Enabled = true,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
                .SetSeries(DamageOutgoingAbilityBreakdown.Abilities.ToList().ToFullPieChartSeries());
            #endregion
            #region DamageIncomingAbilityBreakdownChart
            DamageIncomingAbilityBreakdownChart =
                new Highcharts(string.Format("encounter{0}damageincomingbreakdown", EncounterId))
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Incoming Damage Breakdown" })
               .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.percentage, 2) +' %'; }" })
               .SetPlotOptions(new PlotOptions
               {
                   Pie = new PlotOptionsPie
                   {
                       AllowPointSelect = true,
                       Cursor = Cursors.Pointer,
                       Size = new PercentageOrPixel(85, true)
                   }
               })
                .SetCredits(new Credits()
                {
                    Enabled = true,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
              .SetSeries(new Series
              {
                  Type = ChartTypes.Pie,
                  Data = new Data(DamageIncomingAbilityBreakdown.Abilities.OrderBy(a => a.TotalDamage).ToList().ToPieChartSeries())
              });
            #endregion
            #region DamageIncomingTypeBreakdownChart
            DamageIncomingTypeBreakdownChart = new Highcharts(string.Format("encounter{0}damageincomingtypes", EncounterId))
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Incoming Damage Types" })
                .SetTooltip(new Tooltip
                {
                    Formatter =
                        "function() { return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.percentage, 2) +' %'; }"
                })
                .SetPlotOptions(new PlotOptions
                {
                    Pie = new PlotOptionsPie
                    {
                        AllowPointSelect = true,
                        Cursor = Cursors.Pointer,
                        Size = new PercentageOrPixel(85, true)
                    }
                })
                .SetCredits(new Credits()
                {
                    Enabled = true,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
                .SetSeries(DamageIncomingAbilityBreakdown.Abilities.ToList().ToFullPieChartSeries());
            #endregion
            #region HealingOutgoingAbilityBreakdownChart
            HealingOutgoingAbilityBreakdownChart =
                new Highcharts(string.Format("encounter{0}Healingoutgoingbreakdown", EncounterId))
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Outgoing Healing Breakdown" })
               .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.percentage, 2) +' %'; }" })
               .SetPlotOptions(new PlotOptions
               {
                   Pie = new PlotOptionsPie
                   {
                       AllowPointSelect = true,
                       Cursor = Cursors.Pointer,
                       Size = new PercentageOrPixel(85, true)
                   }
               })
                .SetCredits(new Credits()
                {
                    Enabled = true,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
              .SetSeries(new Series
              {
                  Type = ChartTypes.Pie,
                  Data = new Data(HealingOutgoingAbilityBreakdown.Abilities.OrderBy(a => a.TotalEffectiveHealing).ToList().ToPieChartSeries(true))
              });
            #endregion
            #region HealingOutgoingTypeBreakdownChart
            HealingOutgoingTypeBreakdownChart = new Highcharts(string.Format("encounter{0}Healingoutgoingtypes", EncounterId))
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Outgoing Healing Types" })
                .SetTooltip(new Tooltip
                {
                    Formatter =
                        "function() { return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.percentage, 2) +' %'; }"
                })
                .SetPlotOptions(new PlotOptions
                {
                    Pie = new PlotOptionsPie
                    {
                        AllowPointSelect = true,
                        Cursor = Cursors.Pointer,
                        Size = new PercentageOrPixel(85, true)
                    }
                })
                .SetCredits(new Credits()
                {
                    Enabled = true,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
                .SetSeries(HealingOutgoingTypeBreakdown.Abilities.ToList().HealingToFullPieChartSeries());
            #endregion
            #region HealingIncomingAbilityBreakdownChart
            HealingIncomingAbilityBreakdownChart =
                new Highcharts(string.Format("encounter{0}Healingincomingbreakdown", EncounterId))
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Incoming Healing Breakdown" })
               .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.percentage, 2) +' %'; }" })
               .SetPlotOptions(new PlotOptions
               {
                   Pie = new PlotOptionsPie
                   {
                       AllowPointSelect = true,
                       Cursor = Cursors.Pointer,
                       Size = new PercentageOrPixel(85, true)
                   }
               })
                .SetCredits(new Credits()
                {
                    Enabled = true,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
              .SetSeries(new Series
              {
                  Type = ChartTypes.Pie,
                  Data = new Data(HealingIncomingAbilityBreakdown.Abilities.OrderBy(a => a.TotalEffectiveHealing).ToList().ToPieChartSeries(true))
              });
            #endregion
            #region HealingIncomingSourceBreakdownChart
            HealingIncomingSourceBreakdownChart = new Highcharts(string.Format("encounter{0}Healingsources", EncounterId))
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Incoming Healing Sources" })
                .SetTooltip(new Tooltip
                {
                    Formatter =
                        "function() { return '<b>'+ this.point.name +'</b>: '+ Highcharts.numberFormat(this.percentage, 2) +' %'; }"
                })
                .SetPlotOptions(new PlotOptions
                {
                    Pie = new PlotOptionsPie
                    {
                        AllowPointSelect = true,
                        Cursor = Cursors.Pointer,
                        Size = new PercentageOrPixel(85,true)
                    }
                })
                .SetCredits(new Credits()
                {
                    Enabled = true,
                    Href = "#",
                    Text = "Hewi@Laethys"
                })
                .SetSeries(healSourceBreakdown.ToFullPieChartSeries());
            #endregion
            #endregion
        }
    }

}