using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace PrancingTurtle.Helpers.BurstCalculation
{
    public static class BurstCalculator
    {
        private static async Task<Tuple<int, long>> OneSecondBurst(List<DamageDone> dmgRecords, List<HealingDone> healRecords, List<ShieldingDone> shieldRecords, string type = "Damage")
        {
            var returnValue = new Tuple<int, long>(0,0);
            
            switch (type)
            {
                case "Healing":
                    if (healRecords.Any())
                    {
                        returnValue = healRecords.GroupBy(d => d.SecondsElapsed)
                            .Select(group => new Tuple<int, long>(group.Key, group.Sum(d => d.EffectiveHealing)))
                            .OrderByDescending(d => d.Item2).First();
                    }
                    break;
                case "Shielding":
                    if (shieldRecords.Any())
                    {
                        returnValue = shieldRecords.GroupBy(d => d.SecondsElapsed)
                            .Select(group => new Tuple<int, long>(group.Key, group.Sum(d => d.ShieldValue)))
                            .OrderByDescending(d => d.Item2).First();
                    }
                    break;
                default:
                    if (dmgRecords.Any())
                    {
                        returnValue = dmgRecords.GroupBy(d => d.SecondsElapsed)
                            .Select(group => new Tuple<int, long>(group.Key, group.Sum(d => d.TotalDamage)))
                            .OrderByDescending(d => d.Item2).First();
                    }
                    break;
            }

            return returnValue;
        }
        private static async Task<Tuple<int, int, long, long>> FiveSecondBurst(List<DamageDone> dmgRecords, List<HealingDone> healRecords, List<ShieldingDone> shieldRecords, string type = "Damage")
        {
            // 5-second burst
            int minFiveSecond = 0;
            int maxFiveSecond = 0;

            switch (type)
            {
                case "Healing":
                    minFiveSecond = healRecords.Min(d => d.SecondsElapsed) + 2;
                    maxFiveSecond = healRecords.Max(d => d.SecondsElapsed) - 2;
                    break;
                case "Shielding":
                    minFiveSecond = shieldRecords.Min(d => d.SecondsElapsed) + 2;
                    maxFiveSecond = shieldRecords.Max(d => d.SecondsElapsed) - 2;
                    break;
                default:
                    minFiveSecond = dmgRecords.Min(d => d.SecondsElapsed) + 2;
                    maxFiveSecond = dmgRecords.Max(d => d.SecondsElapsed) - 2;
                    break;
            }

            var fiveSecondValues = new List<Tuple<int, int, long, long>>();
            // Check that our encounter is more than 5 seconds long
            if ((maxFiveSecond + 2) - (minFiveSecond - 2) > 5)
            {
                for (int s = minFiveSecond; s <= maxFiveSecond; s++)
                {
                    var start = s - 2;
                    var end = s + 2;
                    long total = 0;
                    for (int i = 1; i <= 5; i++)
                    {
                        switch (type)
                        {
                            case "Healing":
                                total +=
                                    healRecords.Where(d => d.SecondsElapsed == s - (3 - i)).Sum(d => d.EffectiveHealing);
                                break;
                            case "Shielding":
                                total +=
                                    shieldRecords.Where(d => d.SecondsElapsed == s - (3 - i)).Sum(d => d.ShieldValue);
                                break;
                            default:
                                total += dmgRecords.Where(d => d.SecondsElapsed == s - (3 - i)).Sum(d => d.TotalDamage);
                                break;
                        }
                    }
                    long totalPerSecond = total/5;
                    fiveSecondValues.Add(new Tuple<int, int, long, long>(start, end, total, totalPerSecond));
                }
            }
            var topFiveSecondBurst = fiveSecondValues.Any()
                ? fiveSecondValues.OrderByDescending(d => d.Item3).First()
                : new Tuple<int, int, long, long>(0, 0, 0, 0);

            return topFiveSecondBurst;
        }
        private static async Task<Tuple<int, int, long, long>> FifteenSecondBurst(List<DamageDone> dmgRecords, List<HealingDone> healRecords, List<ShieldingDone> shieldRecords, string type = "Damage")
        {
            // 5-second burst
            int minFifteenSecond = 0;
            int maxFifteenSecond = 0;

            switch (type)
            {
                case "Healing":
                    minFifteenSecond = healRecords.Min(d => d.SecondsElapsed) + 7;
                    maxFifteenSecond = healRecords.Max(d => d.SecondsElapsed) - 7;
                    break;
                case "Shielding":
                    minFifteenSecond = shieldRecords.Min(d => d.SecondsElapsed) + 7;
                    maxFifteenSecond = shieldRecords.Max(d => d.SecondsElapsed) - 7;
                    break;
                default:
                    minFifteenSecond = dmgRecords.Min(d => d.SecondsElapsed) + 7;
                    maxFifteenSecond = dmgRecords.Max(d => d.SecondsElapsed) - 7;
                    break;
            }

            var fifteenSecondValues = new List<Tuple<int, int, long, long>>();
            // Check that our encounter is more than 5 seconds long
            if ((maxFifteenSecond + 7) - (minFifteenSecond - 7) > 15)
            {
                for (int s = minFifteenSecond; s <= maxFifteenSecond; s++)
                {
                    var start = s - 7;
                    var end = s + 7;
                    long total = 0;
                    for (int i = 1; i <= 15; i++)
                    {
                        switch (type)
                        {
                            case "Healing":
                                total += healRecords.Where(d => d.SecondsElapsed == s - (8 - i)).Sum(d => d.EffectiveHealing);
                                break;
                            case "Shielding":
                                total += shieldRecords.Where(d => d.SecondsElapsed == s - (8 - i)).Sum(d => d.ShieldValue);
                                break;
                            default:
                                total += dmgRecords.Where(d => d.SecondsElapsed == s - (8 - i)).Sum(d => d.TotalDamage);
                                break;
                        }
                    }
                    long totalPerSecond = total / 15;
                    fifteenSecondValues.Add(new Tuple<int, int, long, long>(start, end, total, totalPerSecond));
                }
            }
            var topFifteenSecondBurst = fifteenSecondValues.Any() ? fifteenSecondValues.OrderByDescending(d => d.Item3).First() : new Tuple<int, int, long, long>(0, 0, 0, 0);

            return topFifteenSecondBurst;
        }

        public static async Task<List<EncounterPlayerStatistics>> CalculateBurst(List<DamageDone> dmgRecords, List<HealingDone> healRecords, List<ShieldingDone> shieldRecords, int encounterId, int maxSeconds)
        {
            var returnValue = new List<EncounterPlayerStatistics>();
            #region Damage

            if (dmgRecords != null && dmgRecords.Any())
            {
                foreach (var playerDmgGroup in dmgRecords.Where(d => d.SourcePlayerId != null && d.SecondsElapsed <= maxSeconds).GroupBy(d => d.SourcePlayerId))
                {
                    if (playerDmgGroup.Key == null) continue;

                    var dmgTasks = new List<Task>();

                    // 1-second burst
                    var topOneSecondBurst = OneSecondBurst(playerDmgGroup.ToList(), null, null, "Damage");
                    dmgTasks.Add(topOneSecondBurst);

                    //// 5-second burst
                    var topFiveSecondBurst = FiveSecondBurst(playerDmgGroup.ToList(), null, null, "Damage");
                    dmgTasks.Add(topFiveSecondBurst);

                    //// 15-second burst
                    var topFifteenSecondBurst = FifteenSecondBurst(playerDmgGroup.ToList(), null, null, "Damage");
                    dmgTasks.Add(topFifteenSecondBurst);

                    await Task.WhenAll(dmgTasks);
                    // Add it to the list
                    var thisPlayer = returnValue.FirstOrDefault(p => p.PlayerId == playerDmgGroup.Key);
                    if (thisPlayer == null)
                    {
                        returnValue.Add(new EncounterPlayerStatistics()
                        {
                            EncounterId = encounterId,
                            PlayerId = (int) playerDmgGroup.Key,
                            BurstDamage1sSecond = topOneSecondBurst.Result.Item1,
                            BurstDamage1sValue = topOneSecondBurst.Result.Item2,
                            BurstDamage5sStart = topFiveSecondBurst.Result.Item1,
                            BurstDamage5sEnd = topFiveSecondBurst.Result.Item2,
                            BurstDamage5sValue = topFiveSecondBurst.Result.Item3,
                            BurstDamage5sPerSecond = topFiveSecondBurst.Result.Item4,
                            BurstDamage15sStart = topFifteenSecondBurst.Result.Item1,
                            BurstDamage15sEnd = topFifteenSecondBurst.Result.Item2,
                            BurstDamage15sValue = topFifteenSecondBurst.Result.Item3,
                            BurstDamage15sPerSecond = topFifteenSecondBurst.Result.Item4
                        });
                    }
                    else
                    {
                        thisPlayer.BurstDamage1sSecond = topOneSecondBurst.Result.Item1;
                        thisPlayer.BurstDamage1sValue = topOneSecondBurst.Result.Item2;
                        thisPlayer.BurstDamage5sStart = topFiveSecondBurst.Result.Item1;
                        thisPlayer.BurstDamage5sEnd = topFiveSecondBurst.Result.Item2;
                        thisPlayer.BurstDamage5sValue = topFiveSecondBurst.Result.Item3;
                        thisPlayer.BurstDamage5sPerSecond = topFiveSecondBurst.Result.Item4;
                        thisPlayer.BurstDamage15sStart = topFifteenSecondBurst.Result.Item1;
                        thisPlayer.BurstDamage15sEnd = topFifteenSecondBurst.Result.Item2;
                        thisPlayer.BurstDamage15sValue = topFifteenSecondBurst.Result.Item3;
                        thisPlayer.BurstDamage15sPerSecond = topFifteenSecondBurst.Result.Item4;
                    }
                }
            }

            #endregion
            #region Healing
            if (healRecords != null && healRecords.Any())
            {
                foreach (var playerHealGroup in healRecords.Where(d => d.SourcePlayerId != null && d.SecondsElapsed <= maxSeconds).GroupBy(d => d.SourcePlayerId)){
                    if (playerHealGroup.Key == null) continue;

                    var healTasks = new List<Task>();
                    // 1-second burst
                    var topOneSecondBurst = OneSecondBurst(null, playerHealGroup.ToList(), null, "Healing");
                    healTasks.Add(topOneSecondBurst);

                    //// 5-second burst
                    var topFiveSecondBurst = FiveSecondBurst(null, playerHealGroup.ToList(), null, "Healing");
                    healTasks.Add(topFiveSecondBurst);

                    //// 15-second burst
                    var topFifteenSecondBurst = FifteenSecondBurst(null, playerHealGroup.ToList(), null, "Healing");
                    healTasks.Add(topFifteenSecondBurst);

                    await Task.WhenAll(healTasks);

                    // Add it to the list
                    var thisPlayer = returnValue.FirstOrDefault(p => p.PlayerId == playerHealGroup.Key);
                    if (thisPlayer == null)
                    {
                        returnValue.Add(new EncounterPlayerStatistics()
                        {
                            EncounterId = encounterId,
                            PlayerId = (int) playerHealGroup.Key,
                            BurstHealing1sSecond = topOneSecondBurst.Result.Item1,
                            BurstHealing1sValue = topOneSecondBurst.Result.Item2,
                            BurstHealing5sStart = topFiveSecondBurst.Result.Item1,
                            BurstHealing5sEnd = topFiveSecondBurst.Result.Item2,
                            BurstHealing5sValue = topFiveSecondBurst.Result.Item3,
                            BurstHealing5sPerSecond = topFiveSecondBurst.Result.Item4,
                            BurstHealing15sStart = topFifteenSecondBurst.Result.Item1,
                            BurstHealing15sEnd = topFifteenSecondBurst.Result.Item2,
                            BurstHealing15sValue = topFifteenSecondBurst.Result.Item3,
                            BurstHealing15sPerSecond = topFifteenSecondBurst.Result.Item4
                        });
                    }
                    else
                    {
                        thisPlayer.BurstHealing1sSecond = topOneSecondBurst.Result.Item1;
                        thisPlayer.BurstHealing1sValue = topOneSecondBurst.Result.Item2;
                        thisPlayer.BurstHealing5sStart = topFiveSecondBurst.Result.Item1;
                        thisPlayer.BurstHealing5sEnd = topFiveSecondBurst.Result.Item2;
                        thisPlayer.BurstHealing5sValue = topFiveSecondBurst.Result.Item3;
                        thisPlayer.BurstHealing5sPerSecond = topFiveSecondBurst.Result.Item4;
                        thisPlayer.BurstHealing15sStart = topFifteenSecondBurst.Result.Item1;
                        thisPlayer.BurstHealing15sEnd = topFifteenSecondBurst.Result.Item2;
                        thisPlayer.BurstHealing15sValue = topFifteenSecondBurst.Result.Item3;
                        thisPlayer.BurstHealing15sPerSecond = topFifteenSecondBurst.Result.Item4;
                    }
                }
            }
            #endregion
            #region Shielding
            if (shieldRecords != null && shieldRecords.Any())
            {
                foreach (var playerShieldGroup in shieldRecords.Where(d => d.SourcePlayerId != null && d.SecondsElapsed <= maxSeconds).GroupBy(d => d.SourcePlayerId))
                {
                    if (playerShieldGroup.Key == null) continue;

                    var shieldTasks = new List<Task>();
                    // 1-second burst
                    var topOneSecondBurst = OneSecondBurst(null, null, playerShieldGroup.ToList(), "Shielding");
                    shieldTasks.Add(topOneSecondBurst);

                    //// 5-second burst
                    var topFiveSecondBurst = FiveSecondBurst(null, null, playerShieldGroup.ToList(), "Shielding");
                    shieldTasks.Add(topFiveSecondBurst);

                    //// 15-second burst
                    var topFifteenSecondBurst = FifteenSecondBurst(null, null, playerShieldGroup.ToList(), "Shielding");
                    shieldTasks.Add(topFifteenSecondBurst);

                    await Task.WhenAll(shieldTasks);

                    // Add it to the list
                    var thisPlayer = returnValue.FirstOrDefault(p => p.PlayerId == playerShieldGroup.Key);
                    if (thisPlayer == null)
                    {
                        returnValue.Add(new EncounterPlayerStatistics()
                        {
                            EncounterId = encounterId,
                            PlayerId = (int)playerShieldGroup.Key,
                            BurstShielding1sSecond = topOneSecondBurst.Result.Item1,
                            BurstShielding1sValue = topOneSecondBurst.Result.Item2,
                            BurstShielding5sStart = topFiveSecondBurst.Result.Item1,
                            BurstShielding5sEnd = topFiveSecondBurst.Result.Item2,
                            BurstShielding5sValue = topFiveSecondBurst.Result.Item3,
                            BurstShielding5sPerSecond = topFiveSecondBurst.Result.Item4,
                            BurstShielding15sStart = topFifteenSecondBurst.Result.Item1,
                            BurstShielding15sEnd = topFifteenSecondBurst.Result.Item2,
                            BurstShielding15sValue = topFifteenSecondBurst.Result.Item3,
                            BurstShielding15sPerSecond = topFifteenSecondBurst.Result.Item4
                        });
                    }
                    else
                    {
                        thisPlayer.BurstShielding1sSecond = topOneSecondBurst.Result.Item1;
                        thisPlayer.BurstShielding1sValue = topOneSecondBurst.Result.Item2;
                        thisPlayer.BurstShielding5sStart = topFiveSecondBurst.Result.Item1;
                        thisPlayer.BurstShielding5sEnd = topFiveSecondBurst.Result.Item2;
                        thisPlayer.BurstShielding5sValue = topFiveSecondBurst.Result.Item3;
                        thisPlayer.BurstShielding5sPerSecond = topFiveSecondBurst.Result.Item4;
                        thisPlayer.BurstShielding15sStart = topFifteenSecondBurst.Result.Item1;
                        thisPlayer.BurstShielding15sEnd = topFifteenSecondBurst.Result.Item2;
                        thisPlayer.BurstShielding15sValue = topFifteenSecondBurst.Result.Item3;
                        thisPlayer.BurstShielding15sPerSecond = topFifteenSecondBurst.Result.Item4;
                    }
                }
            }
            #endregion

            return returnValue;
        }
    }
}