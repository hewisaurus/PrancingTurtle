using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;
using Database.QueryModels;
using Common;

namespace PrancingTurtle.Models.ViewModels.Encounter
{
    public class OutgoingDamageBreakdownVM
    {
        public List<EncounterPlayerDamageDoneDetail> Data { get; set; }
        public Player Player { get; set; }
        public Database.Models.Encounter Encounter { get; set; }
        public TimeSpan BuildTime { get; set; }

        // Crit rate
        public int TotalCrits { get; set; }
        public int TotalHits { get; set; }
        public int TotalSwings
        {
            get { return TotalCrits + TotalHits; }
        }
        // Biggest / average hit
        public long TopBiggestHit { get; set; }
        public long TopAverageHit { get; set; }

        public string TargetName
        {
            get
            {
                if (Data == null) return "";
                if (Data.Count == 0) return "";
                var record = Data.First(r => r.DamageType != "NA");
                switch (record.TargetType)
                {
                    case CharacterType.Npc:
                        return record.TargetNpcName;
                    case CharacterType.Pet:
                        return record.TargetPetName;
                    case CharacterType.Player:
                        return record.TargetPlayerName;
                }
                return "";
            }
        }
    }
}