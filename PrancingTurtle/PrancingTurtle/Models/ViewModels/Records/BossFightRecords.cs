using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;
using Database.QueryModels.Misc;
using DotNet.Highcharts;

namespace PrancingTurtle.Models.ViewModels.Records
{
    public class BossFightRecords
    {
        public Database.Models.BossFight BossFight { get; set; }
        public List<Database.Models.Encounter> FastestKills { get; set; }
        public Database.Models.Encounter SingleFastestKill { get; set; }
        public List<RankPlayerGuild> TopDps { get; set; }
        public RankPlayerGuild SingleTopDps { get; set; }
        public List<RankPlayerGuild> TopHps { get; set; }
        public RankPlayerGuild SingleTopHps { get; set; }
        public List<RankPlayerGuild> TopAps { get; set; }
        public RankPlayerGuild SingleTopAps { get; set; }
        public List<RankPlayerGuild> TopSingleTargetDps { get; set; }
        public string TimeZoneId { get; set; }
        public bool LoadImage { get; set; }

        public Highcharts GuildEncounterKillTimers { get; set; }
        public Highcharts KillStatistics { get; set; }

        public Highcharts ClericDpsChartTest { get; set; }

        public EncounterDifficulty Difficulty { get; set; }

        public TimeSpan TimeZoneOffset
        {
            get { return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId).GetUtcOffset(DateTime.UtcNow); }
        }

        /// <summary>
        /// Returns the list of fastest kills for all regions
        /// </summary>
        public List<Database.Models.Encounter> AllRegionsFastestKills
        {
            get
            {
                if (FastestKills == null || !FastestKills.Any())
                {
                    return new List<Database.Models.Encounter>();
                }

                return FastestKills.Take(10).ToList();
            }
        }

        /// <summary>
        /// Returns the list of fastest kills for the EU region
        /// </summary>
        public List<Database.Models.Encounter> EuFastestKills
        {
            get
            {
                if (FastestKills == null || !FastestKills.Any())
                {
                    return new List<Database.Models.Encounter>();
                }

                return FastestKills.Where(e => e.UploadGuild.Shard.Region == "EU").Take(10).ToList();
            }
        }

        /// <summary>
        /// Returns the list of fastest kills for the NA region
        /// </summary>
        public List<Database.Models.Encounter> NaFastestKills
        {
            get
            {
                if (FastestKills == null || !FastestKills.Any())
                {
                    return new List<Database.Models.Encounter>();
                }

                return FastestKills.Where(e => e.UploadGuild.Shard.Region == "US").Take(10).ToList();
            }
        }

        public List<RankPlayerGuild> Top10SingleTargetDps
        {
            get
            {
                if (TopSingleTargetDps == null || !TopSingleTargetDps.Any())
                {
                    return new List<RankPlayerGuild>();
                }

                return TopSingleTargetDps.Take(10).ToList();
            }
        }

        public List<RankPlayerGuild> Top10Dps
        {
            get
            {
                if (TopDps == null || !TopDps.Any())
                {
                    return new List<RankPlayerGuild>();
                }

                return TopDps.Take(10).ToList();
            }
        }

        public List<RankPlayerGuild> Top10Hps
        {
            get
            {
                if (TopHps == null || !TopHps.Any())
                {
                    return new List<RankPlayerGuild>();
                }

                return TopHps.Take(10).ToList();
            }
        }

        public List<RankPlayerGuild> Top10Aps
        {
            get
            {
                if (TopAps == null || !TopAps.Any())
                {
                    return new List<RankPlayerGuild>();
                }

                return TopAps.Take(10).ToList();
            }
        }

        public List<RankPlayerGuild> Top10WarriorDps { get; set; }
        public List<RankPlayerGuild> Top10RogueDps { get; set; }
        public List<RankPlayerGuild> Top10ClericDps { get; set; }
        public List<RankPlayerGuild> Top10MageDps { get; set; }
        public List<RankPlayerGuild> Top10PrimalistDps { get; set; }
        public List<RankPlayerGuild> Top10WarriorHps { get; set; }
        public List<RankPlayerGuild> Top10RogueHps { get; set; }
        public List<RankPlayerGuild> Top10ClericHps { get; set; }
        public List<RankPlayerGuild> Top10MageHps { get; set; }
        public List<RankPlayerGuild> Top10PrimalistHps { get; set; }
        public List<RankPlayerGuild> Top10WarriorAps { get; set; }
        public List<RankPlayerGuild> Top10RogueAps { get; set; }
        public List<RankPlayerGuild> Top10ClericAps { get; set; }
        public List<RankPlayerGuild> Top10MageAps { get; set; }
        public List<RankPlayerGuild> Top10PrimalistAps { get; set; } 



        //public List<RankPlayerGuild> Top10WarriorDps
        //{
        //    get
        //    {
        //        if (TopDps == null || !TopDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopDps.Where(p => p.Class == "Warrior").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10RogueDps
        //{
        //    get
        //    {
        //        if (TopDps == null || !TopDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopDps.Where(p => p.Class == "Rogue").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10ClericDps
        //{
        //    get
        //    {
        //        if (TopDps == null || !TopDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopDps.Where(p => p.Class == "Cleric").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10MageDps
        //{
        //    get
        //    {
        //        if (TopDps == null || !TopDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopDps.Where(p => p.Class == "Mage").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10PrimalistDps
        //{
        //    get
        //    {
        //        if (TopDps == null || !TopDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopDps.Where(p => p.Class == "Primalist").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10WarriorHps
        //{
        //    get
        //    {
        //        if (TopHps == null || !TopHps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopHps.Where(p => p.Class == "Warrior").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10RogueHps
        //{
        //    get
        //    {
        //        if (TopHps == null || !TopHps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopHps.Where(p => p.Class == "Rogue").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10ClericHps
        //{
        //    get
        //    {
        //        if (TopHps == null || !TopHps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopHps.Where(p => p.Class == "Cleric").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10MageHps
        //{
        //    get
        //    {
        //        if (TopHps == null || !TopHps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopHps.Where(p => p.Class == "Mage").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10PrimalistHps
        //{
        //    get
        //    {
        //        if (TopHps == null || !TopHps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopHps.Where(p => p.Class == "Primalist").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10WarriorAps
        //{
        //    get
        //    {
        //        if (TopAps == null || !TopAps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopAps.Where(p => p.Class == "Warrior").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10RogueAps
        //{
        //    get
        //    {
        //        if (TopAps == null || !TopAps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopAps.Where(p => p.Class == "Rogue").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10ClericAps
        //{
        //    get
        //    {
        //        if (TopAps == null || !TopAps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopAps.Where(p => p.Class == "Cleric").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10MageAps
        //{
        //    get
        //    {
        //        if (TopAps == null || !TopAps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopAps.Where(p => p.Class == "Mage").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10PrimalistAps
        //{
        //    get
        //    {
        //        if (TopAps == null || !TopAps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopAps.Where(p => p.Class == "Primalist").Take(10).ToList();
        //    }
        //}

        // TOP 10 SINGLE TARGET
        public List<RankPlayerGuild> Top10SingleTargetWarriorDps { get; set; }
        public List<RankPlayerGuild> Top10SingleTargetRogueDps { get; set; }
        public List<RankPlayerGuild> Top10SingleTargetClericDps { get; set; }
        public List<RankPlayerGuild> Top10SingleTargetMageDps { get; set; }
        public List<RankPlayerGuild> Top10SingleTargetPrimalistDps { get; set; }
        //public List<RankPlayerGuild> Top10SingleTargetWarriorDps
        //{
        //    get
        //    {
        //        if (TopSingleTargetDps == null || !TopSingleTargetDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopSingleTargetDps.Where(p => p.Class == "Warrior").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10SingleTargetRogueDps
        //{
        //    get
        //    {
        //        if (TopSingleTargetDps == null || !TopSingleTargetDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopSingleTargetDps.Where(p => p.Class == "Rogue").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10SingleTargetClericDps
        //{
        //    get
        //    {
        //        if (TopSingleTargetDps == null || !TopSingleTargetDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopSingleTargetDps.Where(p => p.Class == "Cleric").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10SingleTargetMageDps
        //{
        //    get
        //    {
        //        if (TopSingleTargetDps == null || !TopSingleTargetDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopSingleTargetDps.Where(p => p.Class == "Mage").Take(10).ToList();
        //    }
        //}

        //public List<RankPlayerGuild> Top10SingleTargetPrimalistDps
        //{
        //    get
        //    {
        //        if (TopSingleTargetDps == null || !TopSingleTargetDps.Any())
        //        {
        //            return new List<RankPlayerGuild>();
        //        }

        //        return TopSingleTargetDps.Where(p => p.Class == "Primalist").Take(10).ToList();
        //    }
        //}

        public BossFightRecords()
        {
            // Default the timezone to UTC
            TimeZoneId = "UTC";
        }
    }
}