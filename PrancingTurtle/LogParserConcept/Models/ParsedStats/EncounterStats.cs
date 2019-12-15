using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogParserConcept.Models.ParsedStats
{
    public class EncounterStats
    {
        [JsonProperty("encounterNumber")]
        public int EncounterNumber { get; set; }
        [JsonProperty("duration")]
        public TimeSpan Duration { get; set; }
        [JsonProperty("damage")]
        public DamageDoneStats DamageStats { get; set; }
        [JsonProperty("healing")]
        public HealingDoneStats HealingStats { get; set; }
        [JsonProperty("shieldingdamage")]
        public ShieldingDoneStats ShieldingStats { get; set; }
        [JsonProperty("deaths")]
        public DeathStats Deaths { get; set; }
        [JsonProperty("npcTopDmgTakenName")]
        public string TopDamageTakenNpcName { get; set; }
        [JsonProperty("npcTopDmgTakenTotal")]
        public long TopDamageTakenNpcTotal { get; set; }

        public EncounterStats()
        {

        }

        public EncounterStats(
            int encounterNumber, TimeSpan duration,
            long damageTotal, int damageEvents,
            long healingTotal, int healingEvents,
            long shieldingTotal, int shieldingEvents,
            Dictionary<string, int> playerDeaths,
            Dictionary<string, int> npcDeaths,
            Dictionary<string, long> npcDamageTaken)
        {
            EncounterNumber = encounterNumber;
            Duration = duration;
            DamageStats = new DamageDoneStats { Events = damageEvents, Total = damageTotal };
            HealingStats = new HealingDoneStats { Events = healingEvents, Total = healingTotal };
            ShieldingStats = new ShieldingDoneStats { Events = shieldingEvents, Total = shieldingTotal };

            Deaths = new DeathStats
            {
                NpcDeaths = new List<CharacterDeath>(),
                PlayerDeaths = new List<CharacterDeath>()
            };
            foreach (var (k, v) in playerDeaths.OrderByDescending(e => e.Value))
            {
                Deaths.PlayerDeaths.Add(new CharacterDeath
                {
                    Name = k,
                    Deaths = v
                });
            }
            foreach (var (k, v) in npcDeaths.OrderByDescending(e => e.Value))
            {
                Deaths.NpcDeaths.Add(new CharacterDeath
                {
                    Name = k,
                    Deaths = v
                });
            }

            if (npcDamageTaken.Any())
            {
                var topNpc = npcDamageTaken.OrderByDescending(e => e.Value).First();
                TopDamageTakenNpcTotal = topNpc.Value;
                TopDamageTakenNpcName = topNpc.Key;
            }
            
        }
    }
}
