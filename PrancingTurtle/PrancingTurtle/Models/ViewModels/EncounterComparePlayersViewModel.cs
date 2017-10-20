using System;
using System.Collections.Generic;
using System.Linq;

namespace PrancingTurtle.Models.ViewModels
{
    public class EncounterComparePlayersViewModel
    {
        public long TopDamage { get; set; }
        public long TopDps { get; set; }
        public List<PlayerComparison> PlayersToCompare { get; set; }
        public PlayerComparison TopPlayer { get; set; }
        public Database.Models.Encounter Encounter { get; set; }
        public Database.Models.Session Session { get; set; }
        public TimeSpan BuildTime { get; set; }
        public string PageTitle { get; set; }
        public string TimeZoneId { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        public List<Ability> AbilityDps { get; set; }
        public List<Ability> AbilityHps { get; set; } 

        public EncounterComparePlayersViewModel()
        {
            PlayersToCompare = new List<PlayerComparison>();
        }

        public void UpdateDetailedStats()
        {
            if (PlayersToCompare.Any())
            {
                TopPlayer = PlayersToCompare.OrderByDescending(p => p.HealingBreakdown.Abilities.Sum(a => a.TotalEffectiveHealing)).First();
            }

            AbilityDps = new List<Ability>();
            AbilityHps = new List<Ability>();

            foreach (var ptc in PlayersToCompare)
            {
                #region Damage
                if (ptc.DamageBreakdown.Abilities.Any())
                {
                    long totalDamageDealt = ptc.DamageBreakdown.Abilities.Sum(a => a.TotalDamage);
                    //ptc.DamageBreakdown.TotalHits = ptc.DamageBreakdown.Abilities.Sum(a => a.Statistics.Hits);
                    //ptc.DamageBreakdown.TotalCrits = ptc.DamageBreakdown.Abilities.Sum(a => a.Statistics.Crits);
                    foreach (var playerAbility in ptc.DamageBreakdown.Abilities)
                    {
                        if (!AbilityDps.Any(a => a.AbilityId == playerAbility.AbilityId))
                        {
                            AbilityDps.Add(playerAbility);
                        }
                        else
                        {
                            var thisAbility = AbilityDps.First(a => a.AbilityId == playerAbility.AbilityId);
                            //long thisAbilityDamage = thisAbility.TotalDamage;

                            //if (playerAbility.TotalDamage > thisAbilityDamage)
                            //{
                            //    thisAbility.TotalDamage = thisAbilityDamage;
                            //    thisAbility.DamagePerSecond = thisAbilityDamage/(long) Encounter.Duration.TotalSeconds;
                            //}
                            if (playerAbility.TotalDamage > thisAbility.HighestTotalDamage)
                            {
                                thisAbility.HighestTotalDamage = playerAbility.TotalDamage;
                            }
                        }
                        playerAbility.BreakdownPercentage = playerAbility.TotalDamage > 0
                            ? ((decimal) playerAbility.TotalDamage/(decimal) totalDamageDealt)*100
                            : 0;
                    }
                }
                #endregion
                #region Healing
                if (ptc.HealingBreakdown.Abilities.Any())
                {
                    // Add the abilities we find to the temp list
                    // Use ability names while comparing healing as it seems some healing abilities
                    // have totally different IDs, once we're done and have sorted the temp list, copy it to the main one.
                    long totalHealingDone = ptc.HealingBreakdown.Abilities.Sum(a => a.TotalEffectiveHealing);
                    foreach (var playerAbility in ptc.HealingBreakdown.Abilities)
                    {
                        var thisHealingAbility =
                            AbilityHps.FirstOrDefault(
                                a => a.Name == playerAbility.Name && a.IconPath == playerAbility.IconPath);

                        if (thisHealingAbility == null)
                        {
                            AbilityHps.Add(playerAbility);
                        }
                        else
                        {
                            thisHealingAbility.TotalEffectiveHealing += playerAbility.TotalEffectiveHealing;
                            playerAbility.BreakdownPercentage = playerAbility.TotalEffectiveHealing > 0
                            ? ((decimal)playerAbility.TotalEffectiveHealing / (decimal)totalHealingDone) * 100
                            : 0;
                        }
                    }
                }
                #endregion
            }
        }
    }
}