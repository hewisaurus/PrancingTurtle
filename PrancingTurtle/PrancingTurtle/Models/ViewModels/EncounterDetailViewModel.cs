using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels
{
    public class EncounterDetailViewModel
    {
        public Highcharts RaidDps { get; set; }
        public Highcharts RaidHps { get; set; }
        public Highcharts RaidAps { get; set; }
        public Highcharts PlayerIDps { get; set; }
        public Highcharts PlayerADps { get; set; }
        public Highcharts PlayerHps { get; set; }
        public Highcharts PlayerAps { get; set; }
        public Highcharts PlayerDeaths { get; set; }

        public int EncounterId { get; set; }
        public string EncounterName { get; set; }
        public string InstanceName { get; set; }
        public TimeSpan EncounterLength { get; set; }
        public DateTime EncounterStart { get; set; }
        public bool EncounterSuccess { get; set; }

        public List<TopDamageDone> TopPlayerDamageDoneRecords { get; set; }
        public List<TopDamageDone> TopNpcDamageDoneRecords { get; set; }
        public List<DamageDone> DamageRecords { get; set; }
        public List<EncounterDeath> DeathRecords { get; set; }
        public AbilityBreakdown PlayerAbilityBreakdown { get; set; }
        public AbilityBreakdown NpcAbilityBreakdown { get; set; }
        // Need to decide if we actually want this chart
        //public Highcharts AbilityBreakdownChart { get; set; }

        public Dictionary<Player, long> AverageDps { get; set; }
        public Dictionary<Player, long> AverageHps { get; set; }
        public Dictionary<Player, long> AverageAps { get; set; }

        public List<Player> Players { get; set; }

        public EncounterDetailViewModel()
        {
            AverageDps = new Dictionary<Player, long>();
            AverageHps = new Dictionary<Player, long>();
            AverageAps = new Dictionary<Player, long>();
            Players = new List<Player>();
            //PlayerDeaths = new List<EncounterDeath>();

            DeathRecords = new List<EncounterDeath>();
            DamageRecords = new List<DamageDone>();
            PlayerAbilityBreakdown = new AbilityBreakdown();
            NpcAbilityBreakdown = new AbilityBreakdown();
            TopPlayerDamageDoneRecords = new List<TopDamageDone>();
            TopNpcDamageDoneRecords = new List<TopDamageDone>();
        }

        public void UpdateDetailedStats()
        {
            if (!DamageRecords.Any()) return;
            #region Loop through abilities for players / pets
            foreach (var dmgGroup in DamageRecords.Where(e => e.SourcePlayer != null || e.SourcePetName != null).GroupBy(d => d.Ability.Name))
            {
                bool isPetAbility = !string.IsNullOrEmpty(dmgGroup.First().SourcePetName);
                long critDamage = 0;
                long hitDamage = 0;

                int crits = dmgGroup.Count(d => d.CriticalHit);
                int swings = dmgGroup.Count();
                int hits = swings - crits;
                decimal critrate = crits > 0 ? (((decimal)crits / (decimal)swings) * 100) : 0;

                Ability ability = new Ability
                {
                    Name = isPetAbility ? string.Format("{0} (Pet)", dmgGroup.Key) : dmgGroup.Key,
                    DamageType = dmgGroup.First().Ability.DamageType,
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
                    if (dmg.SourcePlayerId != null && dmg.TargetPlayerId != null)
                    {
                        // Ignore this - player damage to other players.
                        //TODO: catch this and add to a player damage to other players group
                        continue;
                    }
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
                        if (ability.Statistics.MaxHit == 0 || dmg.TotalDamage > ability.Statistics.MinHit)
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



                PlayerAbilityBreakdown.Abilities.Add(ability);
            }
            #endregion
            #region Loop through NPC Abilities
            foreach (var dmgGroup in DamageRecords.Where(e => e.SourceNpcName != null).GroupBy(d => d.Ability.Name))
            {
                long critDamage = 0;
                long hitDamage = 0;

                int crits = dmgGroup.Count(d => d.CriticalHit);
                int swings = dmgGroup.Count();
                int hits = swings - crits;
                decimal critrate = crits > 0 ? (((decimal)crits / (decimal)swings) * 100) : 0;

                Ability ability = new Ability
                {
                    Name = dmgGroup.Key,
                    DamageType = dmgGroup.First().Ability.DamageType,
                    Statistics = new AbilityStatistics
                    {
                        Swings = swings,
                        Hits = hits,
                        Crits = crits,
                        CritRate = critrate
                    }
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
                        if (ability.Statistics.MaxHit == 0 || dmg.TotalDamage > ability.Statistics.MinHit)
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
                ability.Source = dmgGroup.First().SourceNpcName;
                NpcAbilityBreakdown.Abilities.Add(ability);
            }
            #endregion
            #region Player and NPC ability breakdown percentages
            long totalDamage = PlayerAbilityBreakdown.Abilities.Sum(a => a.TotalDamage);
            foreach (var ability in PlayerAbilityBreakdown.Abilities)
            {
                ability.BreakdownPercentage = ability.TotalDamage > 0 ? ((decimal)ability.TotalDamage / (decimal)totalDamage) * 100 : 0;
            }

            long totalNpcDamage = NpcAbilityBreakdown.Abilities.Sum(a => a.TotalDamage);
            foreach (var ability in NpcAbilityBreakdown.Abilities)
            {
                ability.BreakdownPercentage = ability.TotalDamage > 0 ? ((decimal)ability.TotalDamage / (decimal)totalNpcDamage) * 100 : 0;
            }
            #endregion
            TopPlayerDamageDoneRecords = new List<TopDamageDone>();
            foreach (var dmg in DamageRecords.Where(d => d.SourcePlayerId != null).OrderByDescending(d => d.TotalDamage).Take(5))
            {
                TopPlayerDamageDoneRecords.Add(new TopDamageDone()
                {
                    AbilityName = dmg.Ability.Name,
                    AttackerName = dmg.SourcePlayer.Name,
                    DamageType = dmg.Ability.DamageType,
                    TargetName = dmg.TargetNpcName,
                    IconPath = dmg.Ability.Icon,
                    Value = dmg.TotalDamage
                });
            }

            TopNpcDamageDoneRecords = new List<TopDamageDone>();
            foreach (var dmg in DamageRecords.Where(d => d.SourcePlayerId == null && d.TargetPlayerId != null).OrderByDescending(d => d.EffectiveDamage).Take(5))
            {
                TopNpcDamageDoneRecords.Add(new TopDamageDone()
                {
                    AbilityName = dmg.Ability.Name,
                    AttackerName = dmg.SourceNpcName,
                    DamageType = dmg.Ability.DamageType,
                    TargetName = dmg.TargetPlayer.Name,
                    IconPath = dmg.Ability.Icon,
                    Value = dmg.EffectiveDamage
                });
            }
        }
    }
}