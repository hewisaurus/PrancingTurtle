using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Database.Models;

namespace AutoParser
{
    public class LogEncounter
    {
        public bool UseInSession { get; set; }
        public int IdForSession { get; set; }
        public bool IsPublic { get; set; }

        // The 1-based index of this encounter within the combatlog
        public int CombatLogEncounterNumber { get; set; }

        /// <summary>
        /// The base date value from the CombatLog's creation date
        /// </summary>
        public DateTime BaseDate { get; set; }
        /// <summary>
        /// The Date object that will hold the actual Date and Time that combat started
        /// </summary>
        public DateTime Date { get; set; }
        public TimeSpan Length { get; set; }

        public long LineStart { get; set; }
        public long LineEnd { get; set; }

        public string BossName { get; set; }
        public string DisplayName { get; set; }

        public int DifficultyId { get; set; }
        public string Difficulty { get; set; }

        public string UiDisplayName
        {
            get
            {
                if (!Events.Any()) return null;
                if (Length == new TimeSpan()) return null;
                return string.Format("{0}: {1}{2} - {3} ({4})",
                    CombatLogEncounterNumber, DisplayName,
                    EncounterSuccess ? "" : " Attempt",
                    Length, Date.Add(TimeZoneInfo.Local.BaseUtcOffset));
            }
        }

        public List<CombatEntry> Events { get; set; }
        public List<Character> PlayersSeen { get; set; }
        public List<Character> PetsSeen { get; set; }
        public List<Character> NpcsSeen { get; set; }
        public List<Database.Models.Ability> AbilitiesSeen { get; set; }
        public List<NpcDeath> NpcDeaths { get; set; } 
        /// <summary>
        /// Did the encounter end successfully?
        /// </summary>
        public bool EncounterSuccess { get; set; }
        public bool ValidForRanking { get; set; }

        private void Initialise()
        {
            BossName = "temporaryPlaceholder";

            Events = new List<CombatEntry>();
            PlayersSeen = new List<Character>();
            PetsSeen = new List<Character>();
            NpcsSeen = new List<Character>();
            AbilitiesSeen = new List<Database.Models.Ability>();
            NpcDeaths = new List<NpcDeath>();

            Date = new DateTime();
            UseInSession = false;
            Difficulty = "Normal";

            LineStart = 0;
            LineEnd = 0;
        }

        public LogEncounter()
        {
            Initialise();
            BaseDate = new DateTime();
        }

        public LogEncounter(DateTime combatlogDate)
        {
            Initialise();
            BaseDate = combatlogDate;
        }

        public LogEncounter(DateTime combatlogDate, int encounterNumber)
        {
            Initialise();
            BaseDate = combatlogDate;
            CombatLogEncounterNumber = encounterNumber;
        }

        public LogEncounter(int encounterNumber)
        {
            Initialise();
            BaseDate = new DateTime();
            CombatLogEncounterNumber = encounterNumber;
        }

        public void UpdateDetailedEncounterInfo()
        {
            if (!Events.Any()) return;

            PlayersSeen = new List<Character>();
            NpcsSeen = new List<Character>();
            PetsSeen = new List<Character>();
            AbilitiesSeen = new List<Database.Models.Ability>();

            UpdateEncounterName();

            GetCharacterTypeSeen(CharacterType.Player, PlayersSeen);
            GetCharacterTypeSeen(CharacterType.Npc, NpcsSeen);
            GetCharacterTypeSeen(CharacterType.Pet, PetsSeen);

            // Ignore further processing if we didn't find any NPCs
            if (!NpcsSeen.Any()) return;

            UpdateAbilitiesSeen();

            UpdateSecondsElapsedForEvents();

            #region Was the boss killed?
            EncounterSuccess = Events.Any(e => e.TargetName == BossName && e.OverKillAmount > 0);
            if (!EncounterSuccess && BossName != DisplayName)
            {
                EncounterSuccess = Events.Any(e => e.TargetName == DisplayName && e.OverKillAmount > 0);
                ValidForRanking = EncounterSuccess;
            }
            #endregion

            // Gather the list of NPCs that died
            foreach (
                var npcDeathEvent in
                    Events.Where(
                        e =>
                            e.TargetType == CharacterType.Npc &&
                            (e.ActionType == ActionType.TargetSlain || e.ActionType == ActionType.Died)))
            {
                if (string.IsNullOrEmpty(npcDeathEvent.TargetName.Trim())) continue;

                var thisNpc = NpcDeaths.FirstOrDefault(n => n.Name == npcDeathEvent.TargetName);
                if (thisNpc == null)
                {
                    // Needs to be added
                    NpcDeaths.Add(new NpcDeath(){ Name = npcDeathEvent.TargetName, Deaths = 1 });
                }
                else
                {
                    // Counter needs to be incremented
                    thisNpc.Deaths++;
                }
            }
        }

        private void UpdateAbilitiesSeen()
        {
            // Players first
            foreach (var entry in
                Events.Where(entry =>
                    (entry.AttackerType == CharacterType.Player || entry.AttackerType == CharacterType.Pet) &&
                    entry.ActionType != ActionType.TargetSlain))
            {
                if (!AbilitiesSeen.Any(a => a.AbilityId == entry.AbilityId))
                {
                    var attackerName = entry.AttackerName.Contains("@")
                        ? entry.AttackerName.Substring(0, entry.AttackerName.IndexOf('@'))
                        : entry.AttackerName;

                    AbilitiesSeen.Add(new Database.Models.Ability()
                    {
                        AbilityId = entry.AbilityId,
                        Description = string.Format("First seen by {0}", attackerName),
                        Name = !string.IsNullOrEmpty(entry.AbilityName) ? entry.AbilityName : "Unknown Ability",
                        DamageType = entry.IsDamageType ? entry.AbilityDamageType : null,
                        //AbilityDealsDamage = entry.IsDamageType
                    });
                }
            }
            // NPCs
            foreach (var entry in Events.Where(entry => entry.AttackerType == CharacterType.Npc && entry.ActionType != ActionType.TargetSlain))
            {
                if (!AbilitiesSeen.Any(a => a.AbilityId == entry.AbilityId))
                {
                    AbilitiesSeen.Add(new Database.Models.Ability()
                    {
                        AbilityId = entry.AbilityId,
                        Description = string.Format("First seen by {0}", entry.AttackerName),
                        Name = !string.IsNullOrEmpty(entry.AbilityName) ? entry.AbilityName : "Unknown Ability",
                        DamageType = entry.IsDamageType ? entry.AbilityDamageType : null,
                        //AbilityDealsDamage = entry.IsDamageType
                    });
                }
            }

            // See if we can update any of the abilities that don't have a damage type from the other encounter events
            foreach (var ability in AbilitiesSeen.Where(a => string.IsNullOrEmpty(a.DamageType)))
            {
                var matchingEvent = Events.FirstOrDefault(e => e.AbilityId == ability.AbilityId && !string.IsNullOrEmpty(e.AbilityDamageType));
                if (matchingEvent != null)
                {
                    ability.DamageType = matchingEvent.AbilityDamageType;
                }
            }
        }

        public void GetCharacterTypeSeen(CharacterType type, List<Character> listToUpdate)
        {
            var filteredEvents = Events.Where(
                e => e.AttackerType == type ||
                    e.TargetType == type);
            foreach (var evt in filteredEvents)
            {
                if (evt.AttackerType == type &&
                    !string.IsNullOrEmpty(evt.AttackerName.Trim()))
                {
                    if (!listToUpdate.Any(l => l.Id == evt.AttackerId &&
                                               l.Name == evt.AttackerName))
                    {
                        listToUpdate.Add(new Character()
                        {
                            Id = evt.AttackerId,
                            Name = evt.AttackerName,
                            Type = type
                        });
                    }
                }
                if (evt.TargetType == type &&
                    !string.IsNullOrEmpty(evt.TargetName.Trim()))
                {
                    if (!listToUpdate.Any(l => l.Id == evt.TargetId &&
                                               l.Name == evt.TargetName))
                    {
                        listToUpdate.Add(new Character()
                        {
                            Id = evt.TargetId,
                            Name = evt.TargetName,
                            Type = type
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Update the names of the encounter based on the NPC taking the most damage
        /// </summary>
        private void UpdateEncounterName()
        {
            // Stop if there aren't any events to parse
            if (!Events.Any())
            {
                return;
            }

            var eventsByNpcId = Events
                    .Where(e => e.TargetType == CharacterType.Npc && e.TargetName != "" &&
                                (e.ActionType == ActionType.NormalDamageNonCrit ||
                                 e.ActionType == ActionType.DotDamageNonCrit ||
                                 e.ActionType == ActionType.DamageCrit))
                    .GroupBy(e => e.TargetId).ToList();

            // Stop if no NPCs took damage
            if (!eventsByNpcId.Any())
            {
                return;
            }

            var npcDamage = eventsByNpcId.Select(npcEvents => new CharacterDamage
            {
                Character = new Character
                {
                    Id = npcEvents.Key,
                    Name = npcEvents.First(n => n.TargetName != "").TargetName,
                    Type = CharacterType.Npc
                },
                DamageTaken = npcEvents.Sum(n => n.ActionValue)
            }).ToList();

            BossName = npcDamage.OrderByDescending(n => n.DamageTaken).First().Character.Name;

            // Update DisplayName here if we want, e.g. Grim Brothers in IG will show up as one or the other name
            DisplayName = BossName;
            if (BossName == "Tyshe")
            {
                DisplayName = "Akylios";
            }

            // English - Skelf brothers in The Rhen of Fate
            if (BossName == "Hidrac" || BossName == "Oonta" || BossName == "Weyloz")
            {
                DisplayName = "Skelf Brothers";
            }

            // German - Drekanoth of Fate in the Rhen of Fate
            if (BossName == "Drekanoth des Schicksals")
            {
                DisplayName = "Drekanoth of Fate";
            }

            // French - Drekanoth of Fate in the Rhen of Fate
            if (BossName == "Drekanoth du Destin")
            {
                DisplayName = "Drekanoth of Fate";
            }

            // English - Izkinra in Mount Sharax
            if (BossName == "Warmaster Ilrath" || BossName == "Warmaster Shaddoth")
            {
                DisplayName = "Izkinra";
            }
            // French - Izkinra in Mount Sharax
            if (BossName == "Maître de guerre Shaddoth" || BossName == "Maître de guerre Ilrath")
            {
                DisplayName = "Izkinra";
            }

            // French - The Yrlwalach in Mount Sharax
            if (BossName == "L'Yrlwalach")
            {
                DisplayName = "The Yrlwalach";
            }

            if (BossName == "Fragment of Threngar")
            {
                DisplayName = "Threngar";
            }

            // German - P.U.M.P.K.I.N. in Tyrant's Forge
            if (BossName == "GOLDJUNGE")
            {
                DisplayName = "P.U.M.P.K.I.N.";
            }
            // French - P.U.M.P.K.I.N. in Tyrant's Forge
            if (BossName == "PATATOR")
            {
                DisplayName = "P.U.M.P.K.I.N.";
            }

            if (BossName == "Prince Dollin" || BossName == "Roi runique Molinar")
            {
                DisplayName = "Rune King Molinar";
            }

            // Practice dummies
            if (BossName == "Cible d'entraînement type boss de raid" || BossName == "Schlachtzugboss-Übungsfigur")
            {
                DisplayName = "Raid Boss Practice Dummy";
            }

            if (BossName == "Cible d'entraînement type boss expert" || BossName == "Expertenboss-Übungsfigur")
            {
                DisplayName = "Expert Boss Practice Dummy";
            }

            if (BossName == "Cible d'entraînement type boss expert : niveau 67" || BossName == "Expertenboss-Übungsfigur: Stufe 67")
            {
                DisplayName = "Expert Boss Practice Dummy: Level 67";
            }

            if (BossName == "Cible d'entraînement type boss de donjon" || BossName == "Dungeon-Boss-Übungsfigur")
            {
                DisplayName = "Dungeon Boss Practice Dummy";
            }
            //Dungeon Boss Practice Dummy

            // Akylios - Hammerknell Fortress
            if (BossName == "Jornaru")
            {
                DisplayName = "Akylios";
            }

            // Inwar Darktide - Hammerknell Fortress
            if (BossName == "Tide Warden" || BossName == "Aqualix" || BossName == "Denizar")
            {
                DisplayName = "Inwar Darktide";
            }

            // Gilded Prophecy
            if (BossName == "Anrak l'ignoble" || BossName == "Anrak der Üble")
            {
                DisplayName = "Anrak the Foul";
            }

            // Mind of Madness
            // Pillars of Justice
            if (BossName == "Vis" || BossName == "Misericordia")
            {
                DisplayName = "Pillars of Justice";
            }

            // Tartaric Depths
            // Council of fate
            if (BossName == "Marchioness Boldoch" || BossName == "Count Pleuzhal" || BossName == "Countessa Danazhal" ||
                BossName == "Boldoch's Soul" || BossName == "Pleuzhal's Soul" || BossName == "Danazhal's Soul")
            {
                DisplayName = "Council of Fate";
            }
        }

        private void UpdateSecondsElapsedForEvents()
        {
            // Set the Date for this encounter
            DateTime firstEvent = Events.First().ParsedTimeStamp;
            var encounterTime = firstEvent.TimeOfDay;
            var encounterDate = BaseDate.Date;
            Date = encounterDate.Add(encounterTime);

            double daysToAdd = 0D;

            foreach (var evt in Events)
            {
                var calculated = encounterDate.Add(evt.ParsedTimeStamp.TimeOfDay).AddDays(daysToAdd);
                var secondDifference = (calculated - Date.AddDays(daysToAdd)).TotalSeconds;
                if (secondDifference < 0)
                {
                    // We have just rolled over midnight 
                    daysToAdd++;
                }

                evt.SecondsElapsed = (int)(calculated.AddDays(daysToAdd) - Date).TotalSeconds;
            }
        }
    }
}
