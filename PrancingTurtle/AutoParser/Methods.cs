using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Database.Models;
using Database.QueryModels.Parser;
using Database.Repositories.Interfaces;
using Logging;

namespace AutoParser
{
    public static class Methods
    {
        private static ILogger _logger;
        private static IAutoParserRepository _autoParserRepository;
        private static Regex _isInt = new Regex("^[0-9]+$");

        private static void DebugLine(string message)
        {
            Console.WriteLine(message);
            if (_logger != null)
            {
                _logger.Debug(message);
            }
        }
        private static void InfoLine(string message)
        {
            Console.WriteLine(message);
            _logger.Info(message);
        }

        static long GetSessionTotalPlayedTime(List<LogEncounter> encounters)
        {
            TimeSpan totalPlayTime = new TimeSpan();
            TimeSpan totalGapTime = new TimeSpan();
            for (var i = 0; i < encounters.Count; i++)
            {
                var uniquePlayers = encounters[0].PlayersSeen.Count;
                totalPlayTime =
                    totalPlayTime.Add(new TimeSpan(0, 0, (int)encounters[i].Length.TotalSeconds * uniquePlayers));

                if (i == encounters.Count - 1) continue; // Last one, so don't count gaps
                TimeSpan gap = encounters[i + 1].Date - encounters[i].Date.Add(encounters[i].Length);
                totalGapTime = totalGapTime.Add(new TimeSpan(gap.Ticks * uniquePlayers));
            }
            TimeSpan totalSessionPlayTime = totalPlayTime.Add(totalGapTime);
            return totalSessionPlayTime.Ticks;
        }

        static Dictionary<Character, List<string>> GetNpcCastsSeen(this LogEncounter encounter)
        {
            var npcCasts = new Dictionary<Character, List<string>>();

            var castList = encounter.Events
                .Where(e => e.ActionType == ActionType.CastStart && e.AttackerType == CharacterType.Npc)
                .GroupBy(e => e.AttackerId);

            foreach (var npcCastList in castList)
            {
                if (!npcCasts.Any(n => n.Key.Id == npcCastList.Key))
                {
                    npcCasts.Add(new Character()
                    {
                        Id = npcCastList.Key,
                        Name = npcCastList.First().AttackerName,
                        Type = CharacterType.Npc
                    }, npcCastList.Select(c => c.AbilityName).Distinct().OrderBy(a => a).ToList());
                }
            }

            return npcCasts;
        }
        static Dictionary<Character, List<EncounterDebuffAction>> DebuffsOnAllCharacters(this LogEncounter encounter, List<ShortAbility> shortAbilities)
        {
            var returnValue = new Dictionary<Character, List<EncounterDebuffAction>>();

            var debuffActions = new List<EncounterDebuffAction>();
            var encDebuffAction = new EncounterDebuffAction();

            foreach (var debuffActionList in encounter.Events
                .Where(e => (e.ActionType == ActionType.DebuffOrDotAfflicted || e.ActionType == ActionType.DebuffOrDotDissipated) && !string.IsNullOrEmpty(e.AbilityName))
                    .GroupBy(x => new { x.AttackerId, x.TargetId, x.TargetName, x.AbilityId, x.AbilityName }))
            {
                debuffActions = new List<EncounterDebuffAction>();

                bool debuffUp = false;
                int lastSecondWentUp = 0;
                int lastSecondDown = 0;
                int totalSecondsUp = 0;
                #region Each event for this debuff
                foreach (var debuffEvent in debuffActionList.OrderBy(e => e.SecondsElapsed))
                {
                    switch (debuffEvent.ActionType)
                    {
                        case ActionType.DebuffOrDotAfflicted:
                            if (lastSecondWentUp == 0) // Going up for the first time
                            {
                                lastSecondWentUp = debuffEvent.SecondsElapsed;
                                encDebuffAction = new EncounterDebuffAction()
                                {
                                    AbilityId = shortAbilities.First(a => a.AbilityId == debuffEvent.AbilityId).Id,
                                    DebuffName = debuffActionList.Key.AbilityName,
                                    EncounterId = encounter.IdForSession,
                                    SecondDebuffWentUp = debuffEvent.SecondsElapsed,
                                    SourceId = debuffEvent.AttackerId,
                                    SourceName = debuffEvent.AttackerName,
                                    SourceType = debuffEvent.AttackerType.ToString(),
                                    TargetId = debuffEvent.TargetId,
                                    TargetName = debuffEvent.TargetName,
                                    TargetType = debuffEvent.TargetType.ToString()
                                };
                            }
                            else if (lastSecondDown > 0)
                            {
                                // Ignore if it's gone down and up in the same second
                                if (lastSecondDown != debuffEvent.SecondsElapsed && !debuffUp)
                                {
                                    // Calculate the time it was up for before it faded, add to the total and then reset here
                                    totalSecondsUp += lastSecondDown - lastSecondWentUp;
                                    lastSecondWentUp = debuffEvent.SecondsElapsed;
                                    encDebuffAction.SecondDebuffWentDown = lastSecondDown;
                                    // Add this debuff event to the list (if it was up for at least one second) and then reset it
                                    if (encDebuffAction.SecondDebuffWentDown > encDebuffAction.SecondDebuffWentUp)
                                    {
                                        // Make sure we haven't added it once already
                                        //if (!debuffActions.Any(a => a.AbilityId == encDebuffAction.AbilityId &&
                                        //                            a.SecondDebuffWentUp ==
                                        //                            encDebuffAction.SecondDebuffWentUp &&
                                        //                            a.SecondDebuffWentDown ==
                                        //                            encDebuffAction.SecondDebuffWentDown &&
                                        //                            a.TargetId == encDebuffAction.TargetId))
                                        //{
                                        debuffActions.Add(new EncounterDebuffAction(encDebuffAction));
                                        //}
                                    }
                                    encDebuffAction = new EncounterDebuffAction()
                                    {
                                        AbilityId = shortAbilities.First(a => a.AbilityId == debuffEvent.AbilityId).Id,
                                        DebuffName = debuffActionList.Key.AbilityName,
                                        EncounterId = encounter.IdForSession,
                                        SecondDebuffWentUp = debuffEvent.SecondsElapsed,
                                        SourceId = debuffEvent.AttackerId,
                                        SourceName = debuffEvent.AttackerName,
                                        SourceType = debuffEvent.AttackerType.ToString(),
                                        TargetId = debuffEvent.TargetId,
                                        TargetName = debuffEvent.TargetName,
                                        TargetType = debuffEvent.TargetType.ToString()
                                    };
                                }
                            }
                            debuffUp = true;
                            break;
                        case ActionType.DebuffOrDotDissipated:
                            debuffUp = false;
                            lastSecondDown = debuffEvent.SecondsElapsed;
                            break;
                    }
                }
                #endregion

                if (debuffUp)
                {
                    //totalSecondsUp += (int)encounter.Length.TotalSeconds - lastSecondWentUp;
                    encDebuffAction.SecondDebuffWentDown = (int)encounter.Length.TotalSeconds;
                    debuffActions.Add(encDebuffAction);
                }
                else
                {
                    //totalSecondsUp = lastSecondDown - lastSecondWentUp;
                    encDebuffAction.SecondDebuffWentDown = lastSecondDown;
                    // If something dropped in the first second of combat and didn't go up again, then it will
                    // effectively be null, so skip it here
                    if (encDebuffAction.AbilityId != 0)
                    {
                        debuffActions.Add(new EncounterDebuffAction(encDebuffAction));
                    }
                }

                var actionsForThisCharacter = returnValue.FirstOrDefault(c => c.Key.Id == debuffActionList.Key.TargetId).Value;
                if (actionsForThisCharacter == null)
                {
                    returnValue.Add(
                        new Character()
                        {
                            Id = debuffActionList.Key.TargetId,
                            Name = debuffActionList.Key.TargetName,
                            Type = debuffActionList.First().TargetType
                        }, new List<EncounterDebuffAction>(debuffActions.Distinct()));
                }
                else
                {
                    actionsForThisCharacter.AddRange(new List<EncounterDebuffAction>(debuffActions));
                }
            }
            return returnValue;
        }
        static Dictionary<Character, List<EncounterBuffAction>> BuffsOnAllCharacters(this LogEncounter encounter, List<ShortAbility> shortAbilities)
        {
            var returnValue = new Dictionary<Character, List<EncounterBuffAction>>();
            var buffActions = new List<EncounterBuffAction>();
            var encBuffAction = new EncounterBuffAction();

            foreach (var buffActionList in encounter.Events
                .Where(e => (e.ActionType == ActionType.BuffGain || e.ActionType == ActionType.BuffFade) && !string.IsNullOrEmpty(e.AbilityName))
                    .GroupBy(x => new { x.AttackerId, x.TargetId, x.TargetName, x.AbilityId, x.AbilityName }))
            {
                buffActions = new List<EncounterBuffAction>();

                bool buffUp = false;
                int lastSecondWentUp = 0;
                int lastSecondDown = 0;
                int totalSecondsUp = 0;
                #region Each event for this buff
                foreach (var buffEvent in buffActionList.OrderBy(e => e.SecondsElapsed))
                {
                    switch (buffEvent.ActionType)
                    {
                        case ActionType.BuffGain:
                            if (lastSecondWentUp == 0) // Going up for the first time
                            {
                                lastSecondWentUp = buffEvent.SecondsElapsed;
                                encBuffAction = new EncounterBuffAction()
                                {
                                    AbilityId = shortAbilities.First(a => a.AbilityId == buffEvent.AbilityId).Id,
                                    BuffName = buffActionList.Key.AbilityName,
                                    EncounterId = encounter.IdForSession,
                                    SecondBuffWentUp = buffEvent.SecondsElapsed,
                                    SourceId = buffEvent.AttackerId,
                                    SourceName = buffEvent.AttackerName,
                                    SourceType = buffEvent.AttackerType.ToString(),
                                    TargetId = buffEvent.TargetId,
                                    TargetName = buffEvent.TargetName,
                                    TargetType = buffEvent.TargetType.ToString()
                                };
                            }
                            else if (lastSecondDown > 0)
                            {
                                // Ignore if it's gone down and up in the same second
                                if (lastSecondDown != buffEvent.SecondsElapsed && !buffUp)
                                {
                                    // Calculate the time it was up for before it faded, add to the total and then reset here
                                    totalSecondsUp += lastSecondDown - lastSecondWentUp;
                                    lastSecondWentUp = buffEvent.SecondsElapsed;
                                    encBuffAction.SecondBuffWentDown = lastSecondDown;
                                    // Add this buff event to the list (if it was up for at least one second) and then reset it
                                    if (encBuffAction.SecondBuffWentDown != encBuffAction.SecondBuffWentUp)
                                    {
                                        buffActions.Add(new EncounterBuffAction(encBuffAction));
                                    }
                                    encBuffAction = new EncounterBuffAction()
                                    {
                                        AbilityId = shortAbilities.First(a => a.AbilityId == buffEvent.AbilityId).Id,
                                        BuffName = buffActionList.Key.AbilityName,
                                        EncounterId = encounter.IdForSession,
                                        SecondBuffWentUp = buffEvent.SecondsElapsed,
                                        SourceId = buffEvent.AttackerId,
                                        SourceName = buffEvent.AttackerName,
                                        SourceType = buffEvent.AttackerType.ToString(),
                                        TargetId = buffEvent.TargetId,
                                        TargetName = buffEvent.TargetName,
                                        TargetType = buffEvent.TargetType.ToString()
                                    };
                                }
                            }
                            buffUp = true;
                            break;
                        case ActionType.BuffFade:
                            buffUp = false;
                            lastSecondDown = buffEvent.SecondsElapsed;
                            break;
                    }
                }
                #endregion

                if (buffUp)
                {
                    // Buff was still up when the encounter finished
                    //totalSecondsUp += (int)encounter.Length.TotalSeconds - lastSecondWentUp;
                    encBuffAction.SecondBuffWentDown = (int)encounter.Length.TotalSeconds;
                    // Should we be inserting this here? added 27/3/15
                    if (encBuffAction.AbilityId != 0)
                    {
                        buffActions.Add(encBuffAction);
                    }
                }
                else
                {
                    //totalSecondsUp += lastSecondDown - lastSecondWentUp;
                    encBuffAction.SecondBuffWentDown = lastSecondDown;
                    // If something dropped in the first second of combat and didn't go up again, then it will
                    // effectively be null, so skip it here
                    if (encBuffAction.AbilityId != 0)
                    {
                        buffActions.Add(new EncounterBuffAction(encBuffAction));
                    }
                }

                var actionsForThisCharacter = returnValue.FirstOrDefault(c => c.Key.Id == buffActionList.Key.TargetId).Value;
                if (actionsForThisCharacter == null)
                {
                    returnValue.Add(
                        new Character()
                        {
                            Id = buffActionList.Key.TargetId,
                            Name = buffActionList.Key.TargetName,
                            Type = buffActionList.First().TargetType
                        }, new List<EncounterBuffAction>(buffActions));
                }
                else
                {
                    actionsForThisCharacter.AddRange(new List<EncounterBuffAction>(buffActions));
                }
            }
            return returnValue;
        }

        static int InsertEncounterToDb(this LogEncounter encounter, int bossFightId, int uploaderId, int guildId, bool expandedLog = false)
        {
            int encounterId = _autoParserRepository.GetEncounterId(uploaderId, encounter.Date, bossFightId, encounter.Length);

            // Used for looping when something goes wrong during an INSERT
            var failCount = 0;

            if (encounterId == 0)
            {
                // Encounter doesn't exist yet
                var newEncounter = new Encounter()
                {
                    BossFightId = bossFightId,
                    SuccessfulKill = encounter.EncounterSuccess,
                    ValidForRanking = encounter.ValidForRanking,
                    Date = encounter.Date,
                    Duration = encounter.Length,
                    UploaderId = uploaderId,
                    GuildId = guildId,
                    IsPublic = encounter.IsPublic,
                    EncounterDifficultyId = encounter.DifficultyId,
                    ToBeDeleted = false,
                    Removed = false
                };

                while (true)
                {
                    if (failCount == 3)
                    {
                        return -1;
                    }

                    var result = _autoParserRepository.AddEncounter(newEncounter);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("There was an error while adding this encounter! {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        encounterId = Convert.ToInt32(result.Message);
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error adding this encounter, but it has been successfully added now.");
                        }
                        failCount = 0;
                        break;
                    }
                }
            }

            var playerList = _autoParserRepository.GetPlayersById(encounter.PlayersSeen.Select(p => p.Id)).ToList();
            var shortAbilities = _autoParserRepository.GetShortAbilities(encounter.AbilitiesSeen.Select(a => a.AbilityId)).ToList();

            // List of records to add
            var damageDone = new List<DamageDone>();
            var healingDone = new List<HealingDone>();
            var shieldingDone = new List<ShieldingDone>();
            var encounterDeaths = new List<EncounterDeath>();

            #region Loop time!
            foreach (var groupedCombatEvent in encounter.Events.GroupBy(e => e.SecondsElapsed))
            {
                var i = 1;

                foreach (var groupEvent in groupedCombatEvent)
                {
                    #region Damage
                    if (groupEvent.IsDamageType)
                    {
                        #region Initialise
                        DamageDone newDamageDone = new DamageDone()
                        {
                            EncounterId = encounterId,
                            AbilityId = shortAbilities.First(a => a.AbilityId == groupEvent.AbilityId).Id,
                            OrderWithinSecond = i,
                            SecondsElapsed = groupEvent.SecondsElapsed,
                            CriticalHit = groupEvent.ActionType == ActionType.DamageCrit,
                            Dodged = groupEvent.ActionType == ActionType.Dodge
                        };
                        #endregion
                        #region Attacker
                        switch (groupEvent.AttackerType)
                        {
                            case CharacterType.Npc:
                                newDamageDone.SourceNpcId = groupEvent.AttackerId;
                                newDamageDone.SourceNpcName = groupEvent.AttackerName;
                                break;
                            case CharacterType.Pet:
                                newDamageDone.SourcePetName = groupEvent.AttackerName;
                                newDamageDone.SourcePlayerId = playerList.First(p => p.PlayerId == groupEvent.AttackerPetOwnerId).Id;
                                break;
                            case CharacterType.Player:
                                newDamageDone.SourcePlayerId = playerList.First(p => p.PlayerId == groupEvent.AttackerId).Id;
                                break;
                        }
                        #endregion
                        #region Target
                        switch (groupEvent.TargetType)
                        {
                            case CharacterType.Npc:
                                newDamageDone.TargetNpcId = groupEvent.TargetId;
                                newDamageDone.TargetNpcName = groupEvent.TargetName;
                                break;
                            case CharacterType.Pet:
                                newDamageDone.TargetPetName = groupEvent.TargetName;
                                break;
                            case CharacterType.Player:
                                newDamageDone.TargetPlayerId = playerList.First(p => p.PlayerId == groupEvent.TargetId).Id;
                                break;
                        }
                        #endregion
                        #region Damage Description
                        // Work out effective DPS here based on the special type, if there is any.
                        //
                        // If a player kills an NPC, the effective damage is the spell value minus
                        // the overkill value, and the total damage is the spell value
                        //
                        // If a player hits an NPC with a shield, the effective damage is the spell value
                        // and the total damage is the spell value plus the absorb value
                        #endregion
                        #region Special Values
                        newDamageDone.BlockedAmount = groupEvent.BlockedAmount;
                        newDamageDone.AbsorbedAmount = groupEvent.AbsorbedAmount;
                        newDamageDone.DeflectedAmount = groupEvent.DeflectAmount;
                        newDamageDone.IgnoredAmount = groupEvent.IgnoredAmount;
                        newDamageDone.InterceptedAmount = groupEvent.InterceptAmount;
                        newDamageDone.OverkillAmount = groupEvent.OverKillAmount;
                        newDamageDone.TotalDamage = groupEvent.ActionValue + groupEvent.AbsorbedAmount + groupEvent.BlockedAmount +
                                    groupEvent.DeflectAmount + groupEvent.IgnoredAmount + groupEvent.InterceptAmount;
                        #endregion
                        #region Effective and Total Damage
                        // First, check if there is a shield
                        if (groupEvent.AbsorbedAmount > 0)
                        {
                            // Did the shield get burnt through and the target killed?
                            newDamageDone.EffectiveDamage = groupEvent.OverKillAmount > 0
                                ? groupEvent.ActionValue - groupEvent.OverKillAmount
                                : groupEvent.ActionValue;
                        }
                        else if (groupEvent.OverKillAmount > 0)
                        {
                            newDamageDone.EffectiveDamage = groupEvent.ActionValue - groupEvent.OverKillAmount;
                        }
                        else
                        {
                            newDamageDone.EffectiveDamage = groupEvent.ActionValue;
                        }
                        #endregion
                        damageDone.Add(newDamageDone);
                    }
                    #endregion
                    #region Healing
                    if (groupEvent.IsHealingType)
                    {
                        #region Initialise
                        HealingDone newHealingDone = new HealingDone()
                        {
                            EncounterId = encounterId,
                            AbilityId = shortAbilities.First(a => a.AbilityId == groupEvent.AbilityId).Id,
                            OrderWithinSecond = i,
                            SecondsElapsed = groupEvent.SecondsElapsed,
                            CriticalHit = groupEvent.ActionType == ActionType.HealCrit,
                        };
                        #endregion
                        #region Source
                        switch (groupEvent.AttackerType)
                        {
                            case CharacterType.Npc:
                                newHealingDone.SourceNpcId = groupEvent.AttackerId;
                                newHealingDone.SourceNpcName = groupEvent.AttackerName;
                                break;
                            case CharacterType.Pet:
                                newHealingDone.SourcePetName = groupEvent.AttackerName;
                                newHealingDone.SourcePlayerId = playerList.First(p => p.PlayerId == groupEvent.AttackerPetOwnerId).Id;
                                break;
                            case CharacterType.Player:
                                newHealingDone.SourcePlayerId = playerList.First(p => p.PlayerId == groupEvent.AttackerId).Id;
                                break;
                        }
                        #endregion
                        #region Target
                        switch (groupEvent.TargetType)
                        {
                            case CharacterType.Npc:
                                newHealingDone.TargetNpcId = groupEvent.TargetId;
                                newHealingDone.TargetNpcName = groupEvent.TargetName;
                                break;
                            case CharacterType.Pet:
                                newHealingDone.TargetPetName = groupEvent.TargetName;
                                break;
                            case CharacterType.Player:
                                newHealingDone.TargetPlayerId = playerList.First(p => p.PlayerId == groupEvent.TargetId).Id;
                                break;
                        }
                        #endregion
                        newHealingDone.OverhealAmount = groupEvent.OverhealAmount;
                        #region Effective and Total Healing
                        if (groupEvent.OverhealAmount > 0)
                        {
                            newHealingDone.EffectiveHealing = groupEvent.ActionValue;
                            newHealingDone.TotalHealing = groupEvent.ActionValue + groupEvent.OverhealAmount;
                        }
                        else
                        {
                            newHealingDone.EffectiveHealing = groupEvent.ActionValue;
                            newHealingDone.TotalHealing = groupEvent.ActionValue;
                        }
                        #endregion
                        healingDone.Add(newHealingDone);
                    }
                    #endregion
                    #region Shielding
                    if (groupEvent.IsShieldType)
                    {
                        #region Shielding done by players
                        #region Initialise
                        ShieldingDone newShieldingDone = new ShieldingDone()
                        {
                            EncounterId = encounterId,
                            AbilityId = shortAbilities.First(a => a.AbilityId == groupEvent.AbilityId).Id,
                            OrderWithinSecond = i,
                            SecondsElapsed = groupEvent.SecondsElapsed,
                            CriticalHit = groupEvent.ActionType == ActionType.AbsorbCrit,
                        };
                        #endregion
                        #region Source
                        switch (groupEvent.AttackerType)
                        {
                            case CharacterType.Npc:
                                newShieldingDone.SourceNpcId = groupEvent.AttackerId;
                                newShieldingDone.SourceNpcName = groupEvent.AttackerName;
                                break;
                            case CharacterType.Player:
                                newShieldingDone.SourcePlayerId = playerList.First(p => p.PlayerId == groupEvent.AttackerId).Id;
                                break;
                        }
                        #endregion
                        #region Target
                        switch (groupEvent.TargetType)
                        {
                            case CharacterType.Npc:
                                newShieldingDone.TargetNpcId = groupEvent.TargetId;
                                newShieldingDone.TargetNpcName = groupEvent.TargetName;
                                break;
                            case CharacterType.Pet:
                                newShieldingDone.TargetPetName = groupEvent.TargetName;
                                break;
                            case CharacterType.Player:
                                newShieldingDone.TargetPlayerId = playerList.First(p => p.PlayerId == groupEvent.TargetId).Id;
                                break;
                        }
                        #endregion
                        newShieldingDone.ShieldValue = groupEvent.ActionValue;
                        shieldingDone.Add(newShieldingDone);
                        #endregion
                    }
                    #endregion
                    #region Player Deaths
                    if (groupEvent.IsDeathType)
                    {
                        #region Death Events
                        #region Initialise
                        EncounterDeath newDeath = new EncounterDeath()
                        {
                            EncounterId = encounterId,
                            AbilityId = shortAbilities.First(a => a.AbilityId == groupEvent.AbilityId).Id,
                            OrderWithinSecond = i,
                            SecondsElapsed = groupEvent.SecondsElapsed,
                        };
                        #endregion
                        #region Attacker
                        switch (groupEvent.AttackerType)
                        {
                            case CharacterType.Npc:
                                newDeath.SourceNpcId = groupEvent.AttackerId;
                                newDeath.SourceNpcName = groupEvent.AttackerName;
                                break;
                            case CharacterType.Pet:
                                newDeath.SourcePetName = groupEvent.AttackerName;
                                newDeath.SourcePlayerId = playerList.First(p => p.PlayerId == groupEvent.AttackerPetOwnerId).Id;
                                break;
                            case CharacterType.Player:
                                newDeath.SourcePlayerId = playerList.First(p => p.PlayerId == groupEvent.AttackerId).Id;
                                break;
                        }
                        #endregion
                        #region Target
                        switch (groupEvent.TargetType)
                        {
                            case CharacterType.Npc:
                                newDeath.TargetNpcId = groupEvent.TargetId;
                                newDeath.TargetNpcName = groupEvent.TargetName;
                                break;
                            case CharacterType.Pet:
                                newDeath.TargetPetName = groupEvent.TargetName;
                                break;
                            case CharacterType.Player:
                                newDeath.TargetPlayerId = playerList.First(p => p.PlayerId == groupEvent.TargetId).Id;
                                break;
                        }
                        #endregion

                        newDeath.OverkillValue = groupEvent.OverKillAmount;
                        newDeath.TotalDamage = groupEvent.ActionValue;
                        #endregion

                        encounterDeaths.Add(newDeath);
                    }
                    #endregion

                    i++;
                }
            }

            #endregion

            #region Debuffs
            var debuffActions = new List<EncounterDebuffAction>();
            foreach (var kvp in encounter.DebuffsOnAllCharacters(shortAbilities).OrderBy(d => d.Key.Type).ThenBy(d => d.Key.Name))
            {
                kvp.Value.ForEach(action => action.EncounterId = encounterId);
                debuffActions.AddRange(kvp.Value.OrderBy(v => v.SourceName).ThenBy(v => v.DebuffName).ThenBy(v => v.SecondDebuffWentUp));
            }
            #endregion
            #region Buffs
            var buffActions = new List<EncounterBuffAction>();
            foreach (var kvp in encounter.BuffsOnAllCharacters(shortAbilities).OrderBy(d => d.Key.Type).ThenBy(d => d.Key.Name))
            {
                kvp.Value.ForEach(action => action.EncounterId = encounterId);
                buffActions.AddRange(kvp.Value.OrderBy(v => v.SourceName).ThenBy(v => v.BuffName).ThenBy(v => v.SecondBuffWentUp));
            }
            #endregion

            #region NPC Casts
            var npcCasts = new List<EncounterNpcCast>();
            foreach (var kvp in encounter.GetNpcCastsSeen().OrderBy(c => c.Key.Name))
            {
                foreach (var ability in kvp.Value)
                {
                    if (!npcCasts.Any(c => c.AbilityName == ability && c.NpcName == kvp.Key.Name))
                    {
                        npcCasts.Add(new EncounterNpcCast()
                        {
                            AbilityName = ability,
                            EncounterId = encounterId,
                            NpcId = kvp.Key.Id,
                            NpcName = kvp.Key.Name
                        });
                    }
                }
            }
            #endregion

            #region Save!
            // Get encounter info so we know what we need to add here

            var info = _autoParserRepository.GetInfo(encounterId);

            #region Damage
            if (!info.HasDamageRecords)
            {
                while (true)
                {
                    if (failCount >= 3)
                    {
                        _logger.Debug("Something has gone wrong during the insert process, and multiple retries have not fixed the issue :(");
                        break;
                    }

                    Console.WriteLine("--- Saving {0} damage records for {1} ({2})... ", damageDone.Count, encounter.DisplayName, encounterId);
                    var result = _autoParserRepository.AddDamageDone(damageDone);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("Error while saving damage records: {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error adding damage records, but one or more retries have resolved the issue.");
                        }
                        failCount = 0;
                        break;
                    }
                }
            }
            else
            {
                //DebugLine("--- Damage records already exist, skipping...");
                Console.WriteLine("--- Damage records already exist, skipping...");
            }
            #endregion
            #region Healing
            if (!info.HasHealingRecords)
            {
                while (true)
                {
                    if (failCount >= 3)
                    {
                        _logger.Debug("Something has gone wrong during the insert process, and multiple retries have not fixed the issue :(");
                        break;
                    }

                    Console.WriteLine("--- Saving {0} healing records for {1} ({2})... ", healingDone.Count, encounter.DisplayName, encounterId);
                    var result = _autoParserRepository.AddHealingDone(healingDone);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("Error while saving healing records: {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error adding healing records, but one or more retries have resolved the issue.");
                        }
                        failCount = 0;
                        break;
                    }
                }
            }
            else
            {
                //DebugLine("--- Healing records already exist, skipping...");
                Console.WriteLine("--- Healing records already exist, skipping...");
            }
            #endregion
            #region Shielding
            if (!info.HasShieldingRecords)
            {
                while (true)
                {
                    if (failCount >= 3)
                    {
                        _logger.Debug("Something has gone wrong during the insert process, and multiple retries have not fixed the issue :(");
                        break;
                    }

                    Console.WriteLine("--- Saving {0} shielding records for {1} ({2})... ", shieldingDone.Count, encounter.DisplayName, encounterId);
                    var result = _autoParserRepository.AddShieldingDone(shieldingDone);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("Error while saving shielding records: {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error adding shielding records, but one or more retries have resolved the issue.");
                        }
                        failCount = 0;
                        break;
                    }
                }
            }
            else
            {
                //DebugLine("--- Shielding records already exist, skipping...");
                Console.WriteLine("--- Shielding records already exist, skipping...");
            }
            #endregion
            #region Deaths
            if (!info.HasDeathRecords)
            {
                while (true)
                {
                    if (failCount >= 3)
                    {
                        _logger.Debug("Something has gone wrong during the insert process, and multiple retries have not fixed the issue :(");
                        break;
                    }

                    Console.WriteLine("--- Saving {0} death records for {1} ({2})... ", encounterDeaths.Count, encounter.DisplayName, encounterId);
                    var result = _autoParserRepository.AddEncounterDeaths(encounterDeaths);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("Error while saving death records: {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error adding death records, but one or more retries have resolved the issue.");
                        }
                        failCount = 0;
                        break;
                    }
                }
            }
            else
            {
                //DebugLine("--- Death records already exist, skipping...");
                Console.WriteLine("--- Death records already exist, skipping...");
            }
            #endregion
            #region DebuffAction
            if (!info.HasDebuffActionRecords)
            {
                while (true)
                {
                    if (failCount >= 3)
                    {
                        _logger.Debug("Something has gone wrong during the insert process, and multiple retries have not fixed the issue :(");
                        break;
                    }

                    Console.WriteLine("--- Saving {0} debuff action records for {1} ({2})... ", debuffActions.Count, encounter.DisplayName, encounterId);
                    var result = _autoParserRepository.AddDebuffAction(debuffActions);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("Error while saving debuff action records: {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error adding debuff action records, but one or more retries have resolved the issue.");
                        }
                        failCount = 0;
                        break;
                    }
                }
            }
            else
            {
                //DebugLine("--- Debuff action records already exist, skipping...");
                Console.WriteLine("--- Debuff action records already exist, skipping...");
            }
            #endregion
            #region BuffAction
            if (!info.HasBuffActionRecords)
            {
                while (true)
                {
                    if (failCount >= 3)
                    {
                        _logger.Debug("Something has gone wrong during the insert process, and multiple retries have not fixed the issue :(");
                        break;
                    }

                    Console.WriteLine("--- Saving {0} buff action records for {1} ({2})... ", buffActions.Count, encounter.DisplayName, encounterId);
                    var result = _autoParserRepository.AddBuffAction(buffActions);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("Error while saving buff action records: {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error adding buff action records, but one or more retries have resolved the issue.");
                        }
                        failCount = 0;
                        break;
                    }
                }
            }
            else
            {
                //DebugLine("--- Buff action records already exist, skipping...");
                Console.WriteLine("--- Buff action records already exist, skipping...");
            }
            #endregion
            #region NpcCast
            if (!info.HasNpcCastRecords)
            {
                while (true)
                {
                    if (failCount >= 3)
                    {
                        _logger.Debug("Something has gone wrong during the insert process, and multiple retries have not fixed the issue :(");
                        break;
                    }

                    Console.WriteLine("--- Saving {0} NPC cast records for {1} ({2})... ", npcCasts.Count, encounter.DisplayName, encounterId);
                    var result = _autoParserRepository.AddNpcCasts(npcCasts);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("Error while saving NPC cast records: {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error adding NPC cast records, but one or more retries have resolved the issue.");
                        }
                        failCount = 0;
                        break;
                    }
                }
            }
            else
            {
                //DebugLine("--- NPC cast records already exist, skipping...");
                Console.WriteLine("--- NPC cast records already exist, skipping...");
            }
            #endregion
            #endregion

            return encounterId;
        }
        public static long CalculateTotalDamage(BossFight bossFight, List<CombatEntry> records)
        {
            if (bossFight.RequiresSpecialProcessing)
            {
                #region Lord Fionn
                if (bossFight.Name == "Lord Fionn" && bossFight.Instance.Name == "Comet of Ahnket")
                {
                    return records.Where(e => e.IsDamageType).Sum(e => e.ActionValue) - records.Where(e => e.IsHealingType).Sum(e => e.ActionValue);
                }
                #endregion
            }

            return records.Sum(e => e.TotalDamage);
        }
        public static LogEncounter ProcessSpecialEncounter(LogEncounter encounter, BossFight bossFight)
        {
            if (!bossFight.RequiresSpecialProcessing) return encounter;

            #region Krass, in Bindings of Blood: Laethys

            if (encounter.BossName == "Krass" &&
                bossFight.Instance.Name == "Bindings of Blood: Laethys")
            {
                // Krass will start ignoring damage once he has taken the required amount,
                // which isn't specified here in case it changes
                // The required Raid DPS is specified on the encounter itself, so let's see how long
                // it took before he started ignoring damage, and see if the total DPS was over the
                // required threshold.

                bool nailedIt =
                    encounter.Events.Any(
                        e => e.AbilityId == 0 && e.ActionType == ActionType.Died && e.AttackerName == "Krass");

                int krassIgnored = 0;
                const int krassIgnoreLimit = 10;
                int lastIgnoredSecond = 0;
                //var lastIgnoredTimestamp = new DateTime();
                foreach (var krassEvent in encounter.Events.Where(e => e.TargetName == "Krass"))
                {
                    if (krassIgnored == krassIgnoreLimit)
                    {
                        break;
                    }
                    if (krassEvent.IgnoredAmount > 0L)
                    {
                        krassIgnored++;
                        lastIgnoredSecond = krassEvent.SecondsElapsed;
                        //lastIgnoredTimestamp = krassEvent.CalculatedTimeStamp;
                    }
                }

                // Calculate how much damage Krass took
                var krassDmgTaken =
                    encounter.Events.Where(
                    e => e.TargetName == "Krass" &&
                        e.IgnoredAmount == 0 &&
                        e.TargetTakingDamage &&
                        e.SecondsElapsed <= lastIgnoredSecond)
                        .Sum(e => e.ActionValue);

                var krassRaidDps = krassDmgTaken / (long)encounter.Length.TotalSeconds;
                encounter.EncounterSuccess = krassRaidDps >= bossFight.DpsCheck;

                encounter.Events = encounter.Events.Where
                    (e => e.SecondsElapsed <= lastIgnoredSecond).ToList();

                // Finally, update the duration if it has changed
                TimeSpan encDuration = encounter.Events.Last().ParsedTimeStamp -
                                       encounter.Events.First().ParsedTimeStamp;
                if (encounter.Length != encDuration)
                {
                    encounter.Length = encDuration;
                }
            }
            #endregion
            #region Akylios, in Bindings of Blood: Akylios

            if (encounter.BossName == "Akylios" &&
                bossFight.Instance.Name == "Bindings of Blood: Akylios")
            {
                // Check that Tyshe was killed normally, and Akylios was shown as "dying" without overkill.
                bool bobAkyliosAkyDead = encounter.Events.Any(
                    e => e.ActionType == ActionType.Died &&
                        e.AttackerName == "Akylios" &&
                        e.AbilityId == 0);
                bool bobAkyliosTysheDead = encounter.Events.Any(e => e.TargetName == "Tyshe" && e.OverKillAmount > 0);

                encounter.EncounterSuccess = bobAkyliosAkyDead && bobAkyliosTysheDead;
            }
            #endregion
            #region Lord Greenscale, in Bindings of Blood: Greenscale

            if (encounter.BossName == "Lord Greenscale" &&
                bossFight.Instance.Name == "Bindings of Blood: Greenscale")
            {
                encounter.EncounterSuccess = encounter.Events.Any(
                    e => e.ActionType == ActionType.Died &&
                        e.AttackerName == "Greenscale" &&
                        e.AbilityId == 0);
            }

            #endregion
            #region Laethys, in Bindings of Blood: Laethys
            if (encounter.BossName == "Laethys" &&
                bossFight.Instance.Name == "Bindings of Blood: Laethys")
            {
                encounter.EncounterSuccess = encounter.Events.Any(
                    e => e.ActionType == ActionType.Died &&
                        e.AttackerName == "Laethys" &&
                        e.AbilityId == 0);
            }
            #endregion
            #region Bulf, in Mount Sharax
            // After dying 3 times, bulf runs back to the middle of the room at 10-15% and will despawn if hit hard enough.
            // If he dies 3 times, and then disappears without everyone in the raid dying, it's a kill. If he dies 3 times but
            // everyone dies, it's a wipe (not enough DPS)
            if (bossFight.Name == "Bulf" && bossFight.Instance.Name == "Mount Sharax")
            {
                encounter.EncounterSuccess = false;

                var bulfKills =
                    encounter.Events.Where(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Bulf").OrderByDescending(e => e.SecondsElapsed);
                if (bulfKills.Count() == 3) //Ignore further processing if Bulf hasn't died 3 times
                {
                    // Check to see if anyone stayed alive after the 3rd death. If they did, then it's a successful kill.
                    var thirdKill = bulfKills.First();
                    // Loop through each unique player (that dealt damage to the boss) and see if they died after Bulf's 3rd death
                    // Check this because we've seen a case where 3 people were locked out and 2 of them didn't die, but they should not be counted
                    var checkEvents = encounter.Events.Where(e => e.SecondsElapsed > thirdKill.SecondsElapsed).ToList();
                    var validPlayers =
                        checkEvents.Where(
                            e => e.AttackerType == CharacterType.Player && e.TargetType == CharacterType.Npc)
                            .Select(e => e.AttackerId)
                            .Distinct().ToList();

                    foreach (var playerId in validPlayers)
                    {
                        if (!checkEvents.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetId == playerId))
                        {
                            //_logger.Debug("No death found!");
                            encounter.EncounterSuccess = true;
                            break;
                        }
                    }

                    // If it looks like we have a successful encounter, check the damage that bulf took after his 3rd death
                    // against the hitpoint value we have in the database, if any
                    if (encounter.EncounterSuccess && bossFight.Hitpoints > 0)
                    {
                        var damageTaken = checkEvents.Where(e => e.TargetName == "Bulf").Sum(e => e.TotalDamage);
                        if (damageTaken < bossFight.Hitpoints)
                        {
                            encounter.EncounterSuccess = false;
                        }
                    }

                    // If we have got this far and it's still a success, cut the enter to the last time where the boss took damage
                    // so that the parse isn't skewed by other mobs still being up
                    if (encounter.EncounterSuccess)
                    {
                        var secondBulfLastSeen = checkEvents.Where(e => e.TargetName == "Bulf").Max(e => e.SecondsElapsed);

                        encounter.Events = encounter.Events.Where
                        (e => e.SecondsElapsed <= secondBulfLastSeen).ToList();

                        // Finally, update the duration if it has changed
                        TimeSpan encDuration = encounter.Events.Last().ParsedTimeStamp -
                                               encounter.Events.First().ParsedTimeStamp;
                        if (encounter.Length != encDuration)
                        {
                            encounter.Length = encDuration;
                        }
                    }
                }
            }
            #endregion
            #region Jinoscoth, in Mount Sharax
            if (bossFight.Name == "Jinoscoth" && bossFight.Instance.Name == "Mount Sharax")
            {
                // Check if the boss has been killed
                var bossKilledEvent = encounter.Events.FirstOrDefault(
                    e =>
                        e.ActionType == ActionType.TargetSlain &&
                        e.TargetName == "Jinoscoth" &&
                        e.TargetType == CharacterType.Npc);

                if (bossKilledEvent != null)
                {
                    // Remove all events that occurred after the boss was killed
                    encounter.Events = encounter.Events.Where(e => e.SecondsElapsed <= bossKilledEvent.SecondsElapsed).ToList();
                    // Finally, update the duration if it has changed
                    TimeSpan encDuration = encounter.Events.Last().ParsedTimeStamp -
                                           encounter.Events.First().ParsedTimeStamp;
                    if (encounter.Length != encDuration)
                    {
                        encounter.Length = encDuration;
                    }
                }

                //REMOVE THIS - it should be happening in the general parsing area, not special

                //// If it looks like we have a successful encounter, check the damage that the boss took
                //// against the hitpoint value we have in the database, if any
                //if (encounter.EncounterSuccess && bossFight.Hitpoints > 0)
                //{
                //    var damageTaken = encounter.Events.Where(e => e.TargetName == "Jinoscoth" &&
                //        e.TargetType == CharacterType.Npc).Sum(e => e.TotalDamage);
                //    if (damageTaken < bossFight.Hitpoints)
                //    {
                //        encounter.EncounterSuccess = false;
                //    }
                //}
            }
            #endregion
            #region Izkinra, in Mount Sharax

            if (bossFight.Name == "Izkinra" &&
                bossFight.Instance.Name == "Mount Sharax")
            {
                // Izkinra will turn into a statue once she has been killed, but no death record pops up in the logs.
                // Default to a failed encounter unless we find ignored damage!
                encounter.EncounterSuccess = false;

                int izkinraIgnored = 0;
                const int izkinraIgnoreLimit = 6;
                int lastIgnoredSecond = 0;
                //var lastIgnoredTimestamp = new DateTime();
                foreach (var izkinraEvent in encounter.Events.Where(e => e.TargetName == "Izkinra"))
                {
                    if (izkinraIgnored == izkinraIgnoreLimit)
                    {
                        encounter.EncounterSuccess = true;
                        break;
                    }
                    if (izkinraEvent.IgnoredAmount > 0L)
                    {
                        izkinraIgnored++;
                        lastIgnoredSecond = izkinraEvent.SecondsElapsed;
                    }
                }

                if (encounter.EncounterSuccess)
                {
                    encounter.Events = encounter.Events.Where
                        (e => e.SecondsElapsed <= lastIgnoredSecond).ToList();

                    // Update the duration if it has changed
                    TimeSpan encDuration = encounter.Events.Last().ParsedTimeStamp -
                                           encounter.Events.First().ParsedTimeStamp;
                    if (encounter.Length != encDuration)
                    {
                        encounter.Length = encDuration;
                    }

                    // Finally, check that the previous two NPCs were killed and logging wasn't started after they died
                    var ilrathDead =
                        encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Warmaster Ilrath") ||          // English
                        encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Kriegsmeister Ilrath") ||      // German
                        encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Maître de guerre Ilrath");     // French
                    var shaddothDead =
                        encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Warmaster Shaddoth") ||          // English
                        encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Kriegsmeister Shaddoth") ||      // German
                        encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Maître de guerre Shaddoth");     // French
                    encounter.EncounterSuccess = ilrathDead && shaddothDead;
                }
            }
            #endregion

            #region Skelf Brothers, in The Rhen of Fate

            if (bossFight.Name == "Skelf Brothers" && bossFight.Instance.Name == "The Rhen of Fate")
            {
                var oontaDead = encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Oonta");
                var hidracDead = encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Hidrac");
                var weylozDead = encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Weyloz");
                // Success = false, unless all three skelf have died
                encounter.EncounterSuccess = oontaDead && hidracDead && weylozDead;
            }
            #endregion

            #region P.U.M.P.K.I.N, in Tyrant's Forge

            if (bossFight.Name == "P.U.M.P.K.I.N.")
            {
                encounter.EncounterSuccess = encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Lt. Charles");
            }
            #endregion

            #region Inwar Darktide, in Hammerknell Fortress

            if (bossFight.Name == "Inwar Darktide" && bossFight.Instance.Name == "Hammerknell Fortress")
            {
                encounter.EncounterSuccess = false;
                encounter.ValidForRanking = false;
                // Make sure both minis were killed first, as the encounter is not valid unless they died along with inwar in the same encounter
                if (encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Aqualix") &&
                    encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Denizar"))
                {
                    encounter.EncounterSuccess =
                        encounter.Events.Any(
                            e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Inwar Darktide") ||
                        encounter.Events.Any(
                            e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Inwar Noirflux") ||
                        encounter.Events.Any(
                            e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Inwar Dunkelflut");
                    encounter.ValidForRanking = encounter.EncounterSuccess;
                }
            }
            #endregion

            #region Jornaru and Akylios, in Hammerknell Fortress

            if (bossFight.Name == "Akylios" && bossFight.Instance.Name == "Hammerknell Fortress")
            {
                encounter.EncounterSuccess = false;
                encounter.ValidForRanking = false;
                if (encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Jornaru") &&
                    encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Akylios"))
                {
                    encounter.EncounterSuccess = true;
                    encounter.ValidForRanking = encounter.EncounterSuccess;
                }

                var akyDeath =
                   encounter.Events.FirstOrDefault(
                       e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Akylios");
                if (akyDeath != null)
                {
                    encounter.Events = encounter.Events.Where(e => e.SecondsElapsed <= akyDeath.SecondsElapsed).ToList();

                    // Update the duration if it has changed
                    TimeSpan encDuration = encounter.Events.Last().ParsedTimeStamp -
                                           encounter.Events.First().ParsedTimeStamp;
                    if (encounter.Length != encDuration)
                    {
                        encounter.Length = encDuration;
                    }
                }
            }

            #endregion

            #region Anrak the Foul, in Intrepid Gilded Prophecy

            if (bossFight.Name == "Anrak the Foul" && bossFight.Instance.Name == "Intrepid Gilded Prophecy")
            {
                var anrakDeath =
                    encounter.Events.FirstOrDefault(
                        e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Anrak the Foul");// English
                if (anrakDeath == null)
                {
                    anrakDeath =
                    encounter.Events.FirstOrDefault(
                        e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Anrak der Üble");// German
                }
                if (anrakDeath == null)
                {
                    anrakDeath =
                    encounter.Events.FirstOrDefault(
                        e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Anrak l'ignoble");// French
                }
                if (anrakDeath == null)
                {
                    encounter.EncounterSuccess = false;
                    encounter.ValidForRanking = false;
                }
                else
                {
                    encounter.Events = encounter.Events.Where(e => e.SecondsElapsed <= anrakDeath.SecondsElapsed).ToList();

                    // Update the duration if it has changed
                    TimeSpan encDuration = encounter.Events.Last().ParsedTimeStamp -
                                           encounter.Events.First().ParsedTimeStamp;
                    if (encounter.Length != encDuration)
                    {
                        encounter.Length = encDuration;
                    }

                    encounter.EncounterSuccess = true;
                }
            }
            #endregion

            #region The Pillars of Justice, in Mind of Madness

            #endregion

            #region Pagura, in Mind of Madness
            if (bossFight.Name == "Pagura" && bossFight.Instance.Name == "Mind of Madness")
            {
                encounter.EncounterSuccess = false;
                if (encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Brachy") &&
                    encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Crustok") &&
                    encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Pagura"))
                {
                    encounter.EncounterSuccess = true;
                    encounter.ValidForRanking = encounter.EncounterSuccess;
                }
            }
            #endregion

            #region Commander Isiel, in The Bastion of Steel
            if (bossFight.Name == "Commander Isiel" && bossFight.Instance.Name == "The Bastion of Steel")
            {
                encounter.EncounterSuccess = false;
                if (encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Commander Isiel") &&
                    encounter.Events.Any(e => e.ActionType == ActionType.TargetSlain && e.TargetName == "Vindicator MK1"))
                {
                    encounter.EncounterSuccess = true;
                    encounter.ValidForRanking = encounter.EncounterSuccess;
                }
            }
            #endregion

            #region Prince Hylas, in Greenscale's Blight

            var hylasDiedRecord = encounter.Events.FirstOrDefault(e =>
                e.ActionType == ActionType.TargetSlain && e.TargetName == "Prince Hylas");
            if (hylasDiedRecord == null)
            {
                encounter.EncounterSuccess = false;
                Console.WriteLine(" ** No record was found where Hylas was slain!");
            }
            else
            {
                encounter.EncounterSuccess = true;
                encounter.Events = encounter.Events.Where(e => e.SecondsElapsed <= hylasDiedRecord.SecondsElapsed)
                    .ToList();
            }

            #endregion

            return encounter;
        }

        static void UpdateNpcDeaths(List<NpcDeath> npcDeaths)
        {
            if (!npcDeaths.Any()) return;

            var existingNpcDeaths =
                _autoParserRepository.GetExistingNpcDeaths(npcDeaths.Select(n => n.Name).ToList());

            foreach (var npcDeath in npcDeaths)
            {
                var existingDeathRecord = existingNpcDeaths.FirstOrDefault(n => n.Name == npcDeath.Name);
                if (existingDeathRecord == null)
                {
                    _autoParserRepository.AddNpcDeath(new NpcDeath()
                    {
                        Name = npcDeath.Name,
                        Deaths = npcDeath.Deaths
                    });
                }
                else
                {
                    _autoParserRepository.UpdateNpcDeath(new NpcDeath()
                    {
                        Name = npcDeath.Name,
                        Deaths = existingDeathRecord.Deaths + npcDeath.Deaths
                    });
                }
            }
        }

        static void UpdateDbAbilitiesWithNoDamageType(List<Database.Models.Ability> sessionAbilities)
        {
            var failCount = 0;
            while (true)
            {
                if (failCount == 3)
                {
                    break;
                }

                if (!sessionAbilities.Any()) return;

                var abilitiesToLookFor = sessionAbilities.Where(a => !string.IsNullOrEmpty(a.DamageType)).Select(a => a.AbilityId).Distinct().ToList();

                if (!abilitiesToLookFor.Any()) return;

                var dbAbilities = _autoParserRepository.GetAbilityIdsWithoutDamageTypes(abilitiesToLookFor);
                var damageTypes = new Dictionary<long, string>();

                if (dbAbilities != null)
                {
                    foreach (var dbAbility in dbAbilities)
                    {
                        var encAbility = sessionAbilities.First(a => a.AbilityId == dbAbility);
                        if (!damageTypes.ContainsKey(encAbility.AbilityId))
                        {
                            damageTypes.Add(encAbility.AbilityId, encAbility.DamageType);
                        }
                    }
                }

                if (damageTypes.Any())
                {
                    var result = _autoParserRepository.UpdateAbilityDamageTypes(damageTypes);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("There was an error updating the ability types: {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error updating the ability types, but they have been successfully updated now.");
                        }
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        static void UpdateAbilitiesInDb(List<Database.Models.Ability> abilities)
        {
            if (!abilities.Any())
            {
                return;
            }

            var failCount = 0;
            while (true)
            {
                if (failCount == 3)
                {
                    break;
                }


                #region Check what we need to add
                var missingAbilities = _autoParserRepository.GetAbilityIdsNotInDb(abilities.Select(a => a.AbilityId));
                var abilitiesToAdd = missingAbilities.Select(abilityId => abilities.First(a => a.AbilityId == abilityId)).ToList();
                #endregion

                if (abilitiesToAdd.Any())
                {
                    var result = _autoParserRepository.AddAbilities(abilitiesToAdd);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("There was an error updating the ability list: {0}", result.Message));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        if (failCount > 0)
                        {
                            DebugLine("Encountered an earlier error updating the ability list, but it has been successfully updated now.");
                        }
                        break;
                    }
                }
                else
                {
                    break;
                }
            }


        }

        static void UpdatePlayersInDb(List<Character> players, string shardName)
        {
            if (!players.Any())
            {
                return;
            }
            var playersToAdd = new List<Player>();
            //var missingPlayers = _parseRepository.GetPlayerIdsNotInDb(players.Select(p => p.Id));
            var missingPlayers = _autoParserRepository.GetPlayerIdsNotInDb(players.Select(p => p.Id));
            #region Players missing from the database (add them!)
            if (missingPlayers != null)
            {

                foreach (var playerId in missingPlayers)
                {
                    var playerToAdd = players.First(p => p.Id == playerId);
                    string shard = shardName;
                    string name = playerToAdd.Name;
                    if (playerToAdd.Name.Contains("@"))
                    {
                        name = playerToAdd.Name.Substring(0, playerToAdd.Name.IndexOf('@'));
                        shard = playerToAdd.Name.Substring(playerToAdd.Name.IndexOf('@') + 1);
                    }
                    playersToAdd.Add(new Player()
                    {
                        Name = name,
                        PlayerId = playerToAdd.Id,
                        Shard = shard
                    });
                }

                if (playersToAdd.Any())
                {
                    //DebugLine(string.Format("Saving changes to Player list... (Adding {0})", playersToAdd.Count));
                    Console.WriteLine("Saving changes to Player list... (Adding {0})", playersToAdd.Count);
                    var result = _autoParserRepository.AddPlayers(playersToAdd);
                    if (!result.Success)
                    {
                        DebugLine(string.Format("There was an error saving the player list: {0}", result.Message));
                    }
                }
            }
            #endregion
            #region Players requiring an update (shard changes)

            var failCount = 0;
            while (true)
            {
                if (failCount == 3)
                {
                    break;
                }

                var playersToUpdate = new List<Player>();
                var existingPlayers = _autoParserRepository.GetPlayersById(players.Select(p => p.Id)).ToList();
                if (existingPlayers.Any())
                {
                    foreach (var character in players)
                    {
                        var dbPlayer = existingPlayers.First(p => p.PlayerId == character.Id);

                        if (dbPlayer.Name.Contains("@"))
                        {
                            var shard = dbPlayer.Name.Substring(dbPlayer.Name.IndexOf('@') + 1);
                            var name = dbPlayer.Name.Substring(0, dbPlayer.Name.IndexOf('@'));

                            // Need to update this player
                            playersToUpdate.Add(new Player()
                            {
                                Name = name,
                                Shard = shard,
                                PlayerId = character.Id
                            });
                        }
                        else
                        {
                            if (dbPlayer.Name != character.Name || dbPlayer.Name != shardName)
                            {
                                // Need to update this player
                                playersToUpdate.Add(new Player()
                                {
                                    Name = character.Name,
                                    Shard = shardName,
                                    PlayerId = character.Id
                                });
                            }
                        }
                    }
                    if (playersToUpdate.Any())
                    {
                        var result = _autoParserRepository.UpdatePlayers(playersToUpdate);
                        if (!result.Success)
                        {
                            DebugLine(string.Format("There was an error updating the player list: {0}", result.Message));
                            DebugLine("Retrying in 5 seconds...");
                            Thread.Sleep(5000); // Sleep 5 seconds and retry

                            failCount++;
                        }
                        else
                        {
                            if (failCount > 0)
                            {
                                DebugLine("Encountered an earlier error updating players, but they have been successfully updated now.");
                            }
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

            }

            #endregion
        }

        public static bool CombatStarted(CombatEntry entry)
        {
            // Return true if a player is getting hit by an NPC, or an NPC is getting hit by a player.
            if (entry.AttackerType == CharacterType.Npc && entry.TargetType == CharacterType.Player)
            {
                if (entry.IsDamageType)
                {
                    return true;
                }
            }
            else if (entry.AttackerType == CharacterType.Player && entry.TargetType == CharacterType.Npc)
            {
                if (entry.IsDamageType)
                {
                    return true;
                }
            }

            return false;
        }

        static CombatEntry ProcessAbilityDamageType(CombatEntry entry)
        {
            var msg = entry.Message.ToUpper();
            // Physical Damage
            if (msg.Contains("PHYSICAL DAMAGE") || msg.Contains("PHYSISCH-SCHADEN") || msg.Contains("dégâts de Physiques"))
            {
                entry.AbilityDamageType = "Physical";
                return entry;
            }
            // Air damage
            if (msg.Contains("AIR DAMAGE") || msg.Contains("LUFT-SCHADEN") || msg.Contains("dégâts de Air"))
            {
                entry.AbilityDamageType = "Air";
                return entry;
            }
            // Water damage
            if (msg.Contains("WATER DAMAGE") || msg.Contains("WASSER-SCHADEN") || msg.Contains("dégâts de Eau"))
            {
                entry.AbilityDamageType = "Water";
                return entry;
            }
            // Earth damage
            if (msg.Contains("EARTH DAMAGE") || msg.Contains("ERDE-SCHADEN") || msg.Contains("dégâts de Terre"))
            {
                entry.AbilityDamageType = "Earth";
                return entry;
            }
            // Fire damage
            if (msg.Contains("FIRE DAMAGE") || msg.Contains("FEUER-SCHADEN") || msg.Contains("dégâts de Feu"))
            {
                entry.AbilityDamageType = "Fire";
                return entry;
            }
            // Life damage
            if (msg.Contains("LIFE DAMAGE") || msg.Contains("LEBEN-SCHADEN") || msg.Contains("dégâts de Vie"))
            {
                entry.AbilityDamageType = "Life";
                return entry;
            }
            // Death damage
            if (msg.Contains("DEATH DAMAGE") || msg.Contains("TOD-SCHADEN") || msg.Contains("dégâts de Mort"))
            {
                entry.AbilityDamageType = "Death";
                return entry;
            }
            // Ethereal damage
            if (msg.Contains("ETHEREAL DAMAGE") || msg.Contains("ÄTHERISCH-SCHADEN") || msg.Contains("dégâts de éthéré"))
            {
                entry.AbilityDamageType = "Ethereal";
                return entry;
            }

            return entry;
        }

        static CombatEntry ProcessSpecial(CombatEntry entry, string specialData)
        {
            // Replace text strings with our own to allow for ones with spaces and other languages
            // Like overkill in german, which is 'zu viel des Guten'
            specialData = specialData
                .Replace("absorbiert", "ABSORBED").Replace("absorbed", "ABSORBED").Replace("absorbé", "ABSORBED") //Absorption
                .Replace("geblockt", "BLOCKED").Replace("blocked", "BLOCKED").Replace("bloqué", "BLOCKED") // Blocked
                .Replace("überheilen", "OVERHEAL").Replace("overheal", "OVERHEAL").Replace("excès de soins", "OVERHEAL") // Overheal
                .Replace("abgefangen", "INTERCEPTED").Replace("intercepted", "INTERCEPTED").Replace("intercepté", "INTERCEPTED") // Intercepted
                .Replace("ignoriert", "IGNORED").Replace("ignored", "IGNORED").Replace("ignoré", "IGNORED") // Ignored
                .Replace("zu viel des Guten", "OVERKILL").Replace("overkill", "OVERKILL").Replace("surpuissance", "OVERKILL"); //Overkill 

            string[] strArray = specialData.Trim().Split(' ');
            if (strArray.Length != 0)
            {
                if ((strArray.Length % 2) != 0)
                {
                    return entry;
                }
                for (int i = 1; i <= (strArray.Length / 2); i++)
                {
                    // In French combat logs, we might see 'Attaque auto. (à distance)', ranged auto attack.
                    // Use TryParse for the special, so that it doesn't break if it finds these lines
                    long num2 = 0;
                    if (Int64.TryParse(strArray[(i - 1) * 2].Trim(), out num2))
                    {
                        //long num2 = Int64.Parse(strArray[(i - 1) * 2].Trim());
                        string special = strArray[(i * 2) - 1].Trim();
                        switch (special)
                        {
                            case "ABSORBED":
                                entry.AbsorbedAmount = num2;
                                break;
                            case "BLOCKED":
                                entry.BlockedAmount = num2;
                                break;
                            case "OVERHEAL":
                                entry.OverhealAmount = num2;
                                break;
                            case "INTERCEPTED":
                                entry.InterceptAmount = num2;
                                break;
                            case "OVERKILL":
                                entry.OverKillAmount = num2;
                                break;
                            case "IGNORED":
                                entry.IgnoredAmount = num2;
                                break;
                            case "deflected": // should this even appear in lines anymore?
                                entry.DeflectAmount = num2;
                                break;
                            default:
                                Console.WriteLine("Found an unhandled special: {0}", special);
                                Console.WriteLine("Whole line: {0}", specialData);
                                break;
                        }
                    }
                }
            }
            return entry;
        }

        static CharacterType GetCharacterType(string characterIdEntry, out string characterId)
        {
            characterId = "0";
            CharacterType returnValue = CharacterType.Unknown;

            // We expect the whole field here, e.g. T=P#R=R#240379631105751000
            try
            {
                string[] data = characterIdEntry.Trim().Split('#');
                if (data.Length == 3)
                {
                    // This is a player T=P#R=R#240379631105751000
                    // This is a pet    T=N#R=R#9223372045572243803
                    string attType = data[0].Substring(2, 1).ToUpper();
                    if (attType == "C" || attType == "P")
                    {
                        // C = Character who gathered the combatlog
                        // P = Another player in the group / raid
                        // O = Player outside the raid, e.g. someone who has just left the group
                        returnValue = CharacterType.Player;
                    }
                    else if (attType == "N")
                    {
                        // Check the relationship to the character gathering the combatlog
                        string relType = data[1].Substring(2, 1).ToUpper();
                        // G = Pet in raid (e.g. Beacon of Despair)
                        // R = Pet in raid (e.g. Blood Raptor)
                        // O = Outside of raid (NPC)
                        if (relType == "O")
                        {
                            returnValue = CharacterType.Npc;
                        }
                        else
                        {
                            returnValue = CharacterType.Pet;
                        }
                        //returnValue = relType == "O" ? CharacterType.NPC : CharacterType.Pet;
                    }
                    characterId = data[2];
                }
            }
            catch (Exception ex)
            {
                characterId = "0";
            }
            return returnValue;
        }

        static CombatEntry ProcessLogLineEntry(CombatEntry entry, string logData)
        {
            string[] splitData = logData.Split(',');
            int actionTypeId = int.Parse(splitData[0].Trim());
            entry.ActionType = _isInt.IsMatch(((ActionType)actionTypeId).ToString()) ? ActionType.Unknown : ((ActionType)actionTypeId);

            #region Attacking Character
            string attId;
            entry.AttackerType = GetCharacterType(splitData[1].Trim(), out attId);
            entry.AttackerId = attId;
            #endregion
            #region Target Character
            string tarId;
            entry.TargetType = GetCharacterType(splitData[2].Trim(), out tarId);
            entry.TargetId = tarId;
            #endregion
            string attPetOwnerId;
            string tarPetOwnerId;
            GetCharacterType(splitData[3].Trim(), out attPetOwnerId);
            GetCharacterType(splitData[4].Trim(), out tarPetOwnerId);
            if (attPetOwnerId != "0")
            {
                entry.AttackerPetOwnerId = attPetOwnerId;
            }
            if (tarPetOwnerId != "0")
            {
                entry.TargetPetOwnerId = tarPetOwnerId;
            }
            entry.AttackerName = splitData[5].Trim();
            entry.TargetName = splitData[6].Trim();
            entry.ActionValue = long.Parse(splitData[7].Trim());
            entry.AbilityId = long.Parse(splitData[8].Trim());
            entry.AbilityName = splitData[9].Trim();
            entry.SpecialValue = long.Parse(splitData[10].Trim());

            return entry;
        }

        /// <summary>
        /// Process the current line into a CombatEntry
        /// </summary>
        /// <param name="line">The log line</param>
        /// <param name="expandedLog">True if the log type is Expanded, otherwise false</param>
        /// <returns></returns>
        public static CombatEntry ProcessLine(string line, bool expandedLog = false)
        {
            try
            {
                // Catch test criteria
                //if (line.Contains("(à distance)"))

                //( 7 , T=P#R=C#169025725536183234 , T=P#R=C#169025725536183234 , T=X#R=X#0 , T=X#R=X#0 , Geryonn , Geryonn , 0 , 1660365089 , Virulent Poison , 0 ) Geryon22:11:14: ( 27 , T=P#R=R#169025725533810537 , T=P#R=R#169025725533810537 , T=X#R=X#0 , T=X#R=X#0 , Killings , Killings , 25 , 939734518 , Archaic Tablet , 0 ) Killings's Archaic Tablet gives Killings 25 Mana.

                var lineNoTimestamp = "";
                var ce = new CombatEntry();
                //{
                    //ParsedTimeStamp = DateTime.Parse(line.Substring(0, 8))
                //};

                if (expandedLog)
                {
                    // Seems like the ms value isn't forced to 3 digits. If it's zero then it is 2 digits
                    // Assume the date portion length is 24 unless told otherwise
                    var dateLength = 24;
                    // 12/02/2016 08:19:51:798:
                    // 12/02/2016 08:19:52:02:
                    // 12/02/2016 08:21:35:00:

                    // mm/dd/yyyy hh:mm:ss:mms:
                    var month = int.Parse(line.Substring(0, 2));
                    var day = int.Parse(line.Substring(3, 2));
                    var year = int.Parse(line.Substring(6, 4));
                    var hour = int.Parse(line.Substring(11, 2));
                    var minute = int.Parse(line.Substring(14, 2));
                    var second = int.Parse(line.Substring(17, 2));
                    // Figure out the millisecond value
                    var ms = 0;
                    var msValue = line.Substring(20, 3);
                    if (msValue.Contains(":"))
                    {
                        ms = int.Parse(msValue.Substring(0, 2));
                        dateLength = 23;
                    }
                    else
                    {
                        ms = int.Parse(msValue);
                    }

                    ce.ParsedTimeStamp = new DateTime(year, month, day, hour, minute, second, ms);
                    // Remove the date portion from the line
                    lineNoTimestamp = line.Remove(0, dateLength).Trim();
                    if (lineNoTimestamp.StartsWith(":"))
                    {
                        lineNoTimestamp = line.Substring(1);
                    }
                }
                else
                {
                    ce.ParsedTimeStamp = DateTime.Parse(line.Substring(0, 8));
                    // Remove the date portion from the line
                    lineNoTimestamp = line.Remove(0, 9).Trim();
                }

                // Check for commas here!
                lineNoTimestamp = lineNoTimestamp
                    .Replace("Saute, cours, vole !", "Juke and Run")
                    .Replace("Blessing of Mobility, ", "Blessing of Mobility and ");


                #region Determine what part of this line is the 'log data'
                // Count how many open and close brackets we have on this line
                var openBrackets = new List<int>();
                var closeBrackets = new List<int>();

                for (var i = lineNoTimestamp.IndexOf('('); i > -1; i = lineNoTimestamp.IndexOf('(', i + 1))
                {
                    openBrackets.Add(i);
                }
                for (var i = lineNoTimestamp.IndexOf(')'); i > -1; i = lineNoTimestamp.IndexOf(')', i + 1))
                {
                    closeBrackets.Add(i);
                }

                if (openBrackets.Count == 0)
                {
                    return null;
                }

                int logDataStartIndex = openBrackets[0];
                int openBeforeFirstClose = openBrackets.Count(t => t < closeBrackets[0]);
                int logDataEndIndex = closeBrackets[openBeforeFirstClose - 1];

                string logData = lineNoTimestamp.Substring(logDataStartIndex + 1, logDataEndIndex - logDataStartIndex - 1).Trim();
                #endregion

                // Process the actual log data here
                ce = ProcessLogLineEntry(ce, logData);

                ce.Message = lineNoTimestamp.Remove(logDataStartIndex, logDataEndIndex - logDataStartIndex + 1).Trim();

                // Check if the message contains special event info, like absorbs, intercepts, etc
                int messageOpenBracket = ce.Message.IndexOf('(');
                int messageCloseBracket = ce.Message.IndexOf(')');
                if (messageOpenBracket >= 0 && messageCloseBracket >= 0)
                {
                    string special = ce.Message.Substring(messageOpenBracket + 1,
                        messageCloseBracket - messageOpenBracket - 1);

                    // Process special event info
                    ce = ProcessSpecial(ce, special);
                }

                // Get damage type if we can (damaging abilities only)
                if (!string.IsNullOrEmpty(ce.TargetName) &&
                    (ce.ActionType == ActionType.DamageCrit ||
                    ce.ActionType == ActionType.NormalDamageNonCrit ||
                    ce.ActionType == ActionType.DotDamageNonCrit ||
                    ce.ActionType == ActionType.Block))
                {
                    ce = ProcessAbilityDamageType(ce);
                }

                // Determine if we want to ignore this line
                if (ce.IsDeathType && ce.OverKillAmount == ce.TotalDamage)
                {
                    ce.IgnoreThisEvent = true;
                }
                return ce;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while parsing the following line!");
                Console.WriteLine(line);
                Console.WriteLine("ERROR: {0}", ex.Message);
                return null;
            }
        }

        public static bool CanParseLine(string line)
        {
            // On top of making sure that we have brackets within this line (denoting event data), make sure that
            // the number of open brackets is the same as the closes. If not, deem it an invalid line.
            bool validLine = (line.IndexOf('(') >= 0) && (line.IndexOf(')') >= 0) && line.Length > 22;
            if (validLine)
            {
                // Count how many open and close brackets we have on this line
                var openBrackets = new List<int>();
                var closeBrackets = new List<int>();

                for (var i = line.IndexOf('('); i > -1; i = line.IndexOf('(', i + 1))
                {
                    openBrackets.Add(i);
                }
                for (var i = line.IndexOf(')'); i > -1; i = line.IndexOf(')', i + 1))
                {
                    closeBrackets.Add(i);
                }

                if (openBrackets.Count != closeBrackets.Count)
                {
                    validLine = false;
                }
            }

            // If it's valid so far, make sure that we can parse the date out of the front of it
            if (validLine)
            {
                DateTime lineDate;
                validLine = DateTime.TryParse(line.Substring(0, 8), out lineDate);
            }

            // Last check - make sure that the log line doesn't contain data from two lines, like this:
            // 22:10:29: ( 7 , T=P#R=C#169025725536183234 , T=P#R=C#169025725536183234 , T=X#R=X#0 , T=X#R=X#0 , Geryonn , Geryonn , 0 , 1660365089 , Virulent Poison , 0 ) Geryon22:11:14: ( 27 , T=P#R=R#169025725533810537 , T=P#R=R#169025725533810537 , T=X#R=X#0 , T=X#R=X#0 , Killings , Killings , 25 , 939734518 , Archaic Tablet , 0 ) Killings's Archaic Tablet gives Killings 25 Mana.
            // If it does, skip them both. Unfortunate, but shit happens.
            if (validLine)
            {
                var hashCount = line.Count(l => l == '#');
                validLine = hashCount == 8;
            }

            return validLine;
        }

        public static List<LogEncounter> ParseStream(StreamReader sr, DateTime fileDate, out long lineCount, bool fastParse = false)
        {
            int downtimeSeconds = 15;
            var encounters = new List<LogEncounter>();
            int encounterNumber = 1;
            string line = "";
            int lineNumber = 0;
            bool inCombat = false;
            var currentCombatStarted = new DateTime();
            var currentCombatLastDamage = new DateTime();
            double daysToAdd = 0;
            var lastTimeStamp = new DateTime();
            var calculatedTimestamp = new DateTime();

            // Default logType to unknown and set it when we can so we know which loop to use
            var logType = LogType.Unknown;

            #region Parse the file here

            try
            {
                while ((line = sr.ReadLine()) != null)
                {
                    lineNumber++;

                    if (logType == LogType.Unknown)
                    {
                        // Figure out what type of log we're dealing with
                        // /combatlog 01:13:53
                        // /combatlogexpanded 12/02/20
                        if (line.Length > 8)
                        {
                            if (line.Substring(2, 1) == ":" && line.Substring(5, 1) == ":")
                            {
                                logType = LogType.Standard;
                            }
                            else if (line.Substring(2, 1) == "/" && line.Substring(5, 1) == "/")
                            {
                                logType = LogType.Expanded;
                            }
                        }
                    }

                    // No change to the standard logging parse methods
                    switch (logType)
                    {
                        case LogType.Standard:
                            #region Standard logging
                            // The following code was from when we only had /combatlog and no datetime + millisecond accuracy
                            if (CanParseLine(line))
                            {
                                //Console.WriteLine(lineNumber);
                                //if (lineNumber > 68620)
                                //{
                                //    Console.WriteLine(line);
                                //}
                                var entry = ProcessLine(line);
                                //if (entry == null) continue; // Uncomment this when we want it to break on errors so we can see them

                                if (inCombat)
                                {
                                    #region Currently in combat

                                    var secondDifference =
                                        (int)(entry.ParsedTimeStamp.AddDays(daysToAdd) - lastTimeStamp).TotalSeconds;
                                    if (secondDifference == 0 || secondDifference > 0)
                                    {
                                        // Timestamp hasn't changed, or it's later in the same day
                                        calculatedTimestamp = entry.ParsedTimeStamp.AddDays(daysToAdd);
                                    }
                                    else
                                    {
                                        // We have just rolled over midnight 
                                        daysToAdd++;
                                        calculatedTimestamp = entry.ParsedTimeStamp.AddDays(daysToAdd);
                                        //currentCombatLastDamage = calculatedTimestamp;
                                    }

                                    // Exit combat if no NPC has taken damage in the last 10 seconds
                                    // JUNE 8, 2015 - CHANGE THIS TO 15 SECONDS as some bosses have a greater downtime (stored in 'downtimeSeconds')
                                    if ((calculatedTimestamp - currentCombatLastDamage).TotalSeconds > downtimeSeconds)
                                    {
                                        //Console.WriteLine("Determined that encounter {0} has just ended, second is now {1}, last combat was {2}. Total time {3}",
                                        //encounterNumber, calculatedTimestamp, currentCombatLastDamage, (calculatedTimestamp - currentCombatLastDamage).TotalSeconds);
                                        //Thread.Sleep(5000);
                                        inCombat = false;
                                        encounters[encounterNumber - 1].Length = currentCombatLastDamage -
                                                                                 currentCombatStarted;
                                        encounters[encounterNumber - 1].LineEnd = lineNumber;
                                        encounterNumber++;
                                    }
                                    else
                                    {
                                        if (!entry.IgnoreThisEvent)
                                        {
                                            if (!fastParse)
                                            {
                                                encounters[encounterNumber - 1].Events.Add(entry);
                                            }
                                            // Update the last combat timestamp if it has changed
                                            if (calculatedTimestamp != currentCombatLastDamage)
                                            {
                                                if (entry.TargetType == CharacterType.Npc && entry.IsDamageType)
                                                {
                                                    currentCombatLastDamage = entry.ParsedTimeStamp.AddDays(daysToAdd);
                                                }
                                            }
                                        }
                                    }

                                    #endregion
                                }
                                else if (CombatStarted(entry))
                                {
                                    #region Combat Started

                                    Console.WriteLine("Encounter {0} started at {1}", encounterNumber,
                                        entry.ParsedTimeStamp.AddDays(daysToAdd));
                                    downtimeSeconds = 15; // Default this to 15
                                    inCombat = true;
                                    currentCombatStarted = entry.ParsedTimeStamp.AddDays(daysToAdd);
                                    lastTimeStamp = entry.ParsedTimeStamp.AddDays(daysToAdd);
                                    currentCombatLastDamage = entry.ParsedTimeStamp.AddDays(daysToAdd);
                                    encounters.Add(new LogEncounter(fileDate)
                                    {
                                        Events = new List<CombatEntry>() { entry },
                                        LineStart = lineNumber
                                    });

                                    // Check if we need to extend the downtime timer for very specific fights
                                    if ((entry.TargetType == CharacterType.Npc &&
                                         (entry.TargetName == "Aqualix" || entry.TargetName == "Denizar")) ||
                                        (entry.AttackerType == CharacterType.Npc &&
                                         (entry.AttackerName == "Aqualix" || entry.AttackerName == "Denizar")))
                                    {
                                        downtimeSeconds = 30;
                                        Console.WriteLine(
                                            "Inwar Darktide encounter detected, so setting combat downtime value to {0}",
                                            downtimeSeconds);
                                    }

                                    if (entry.TargetType == CharacterType.Npc && entry.TargetName == "Prince Hylas" ||
                                        entry.AttackerType == CharacterType.Npc && entry.AttackerName == "Prince Hylas")
                                    {
                                        downtimeSeconds = 50;
                                        Console.WriteLine(
                                            "Prince Hylas encounter detected, so setting combat downtime value to {0}",
                                            downtimeSeconds);
                                    }
                                    // Do we need this? Adjusted HP instead...
                                    //else if (entry.TargetType == CharacterType.Npc && (entry.TargetName == "Pagura"))
                                    //{
                                    //    downtimeSeconds = 20;
                                    //    Console.WriteLine("Pagura encounter detected, so setting combat downtime value to {0}", downtimeSeconds);
                                    //}

                                    #endregion
                                }
                                //lineNumber++;
                            }
                            #endregion
                            break;
                        case LogType.Expanded:
                            #region Expanded logging
                            var expEntry = ProcessLine(line, true);
                            // If this is null then it didn't process - skip it.
                            if (expEntry == null) continue;
                            if (inCombat)
                            {
                                #region Currently in combat
                                

                                // Exit combat if no NPC has taken damage in the last 10 seconds
                                // JUNE 8, 2015 - CHANGE THIS TO 15 SECONDS as some bosses have a greater downtime (stored in 'downtimeSeconds')
                                if ((calculatedTimestamp - currentCombatLastDamage).TotalSeconds > downtimeSeconds)
                                {
                                    //Console.WriteLine("Determined that encounter {0} has just ended, second is now {1}, last combat was {2}. Total time {3}",
                                    //encounterNumber, calculatedTimestamp, currentCombatLastDamage, (calculatedTimestamp - currentCombatLastDamage).TotalSeconds);
                                    //Thread.Sleep(5000);
                                    inCombat = false;
                                    encounters[encounterNumber - 1].Length = currentCombatLastDamage -
                                                                             currentCombatStarted;
                                    encounters[encounterNumber - 1].LineEnd = lineNumber;
                                    encounterNumber++;
                                }
                                else
                                {
                                    if (!expEntry.IgnoreThisEvent)
                                    {
                                        if (!fastParse)
                                        {
                                            encounters[encounterNumber - 1].Events.Add(expEntry);
                                        }
                                        // Update the last combat timestamp if it has changed
                                        if (calculatedTimestamp != currentCombatLastDamage)
                                        {
                                            if (expEntry.TargetType == CharacterType.Npc && expEntry.IsDamageType)
                                            {
                                                currentCombatLastDamage = expEntry.ParsedTimeStamp;
                                            }
                                        }
                                    }
                                }

                                #endregion
                            }
                            else if (CombatStarted(expEntry))
                            {
                                #region Combat Started

                                Console.WriteLine("Encounter {0} started at {1}", encounterNumber, expEntry.ParsedTimeStamp);
                                downtimeSeconds = 15; // Default this to 15
                                inCombat = true;
                                currentCombatStarted = expEntry.ParsedTimeStamp;
                                lastTimeStamp = expEntry.ParsedTimeStamp;
                                currentCombatLastDamage = expEntry.ParsedTimeStamp;
                                encounters.Add(new LogEncounter(fileDate)
                                {
                                    Events = new List<CombatEntry>() { expEntry },
                                    LineStart = lineNumber
                                });

                                // Check if we need to extend the downtime timer for very specific fights
                                if ((expEntry.TargetType == CharacterType.Npc &&
                                     (expEntry.TargetName == "Aqualix" || expEntry.TargetName == "Denizar")) ||
                                    (expEntry.AttackerType == CharacterType.Npc &&
                                     (expEntry.AttackerName == "Aqualix" || expEntry.AttackerName == "Denizar")))
                                {
                                    downtimeSeconds = 30;
                                    Console.WriteLine(
                                        "Inwar Darktide encounter detected, so setting combat downtime value to {0}",
                                        downtimeSeconds);
                                }
                                #endregion
                            }
                            #endregion
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLine(string.Format("An error occurred! {0}", ex.Message));
            }

            #endregion

            DebugLine(string.Format("Parsed {0} lines", lineNumber));
            lineCount = lineNumber;
            return encounters;
        }

        /// <summary>
        /// This method is similar to ParseStream except that it filters fights based on a passed list of boss names and encounters longer than 10 seconds
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="fileDate"></param>
        /// <param name="lineCount"></param>
        /// <param name="bossNames"></param>
        /// <param name="fastParse"></param>
        /// <returns></returns>
        public static List<LogEncounter> ParseStream_v2(StreamReader sr, DateTime fileDate, out long lineCount, Dictionary<int, string> bossNames, bool fastParse = false)
        {
            int downtimeSeconds = 15;
            var encounters = new List<LogEncounter>();
            var encountersToSave = new List<LogEncounter>();
            int encounterNumber = 1;
            string line = "";
            int lineNumber = 0;
            bool inCombat = false;
            var currentCombatStarted = new DateTime();
            var currentCombatLastDamage = new DateTime();
            double daysToAdd = 0;
            var lastTimeStamp = new DateTime();
            var calculatedTimestamp = new DateTime();

            #region Parse the file here

            try
            {
                while ((line = sr.ReadLine()) != null)
                {
                    lineNumber++;
                    if (CanParseLine(line))
                    {
                        //Console.WriteLine(lineNumber);
                        //if (lineNumber > 68620)
                        //{
                        //    Console.WriteLine(line);
                        //}
                        var entry = ProcessLine(line);
                        //if (entry == null) continue; // Uncomment this when we want it to break on errors so we can see them

                        if (inCombat)
                        {
                            #region Currently in combat

                            var secondDifference = (int)(entry.ParsedTimeStamp.AddDays(daysToAdd) - lastTimeStamp).TotalSeconds;
                            if (secondDifference == 0 || secondDifference > 0)
                            {
                                // Timestamp hasn't changed, or it's later in the same day
                                calculatedTimestamp = entry.ParsedTimeStamp.AddDays(daysToAdd);
                            }
                            else
                            {
                                // We have just rolled over midnight 
                                daysToAdd++;
                                calculatedTimestamp = entry.ParsedTimeStamp.AddDays(daysToAdd);
                                //currentCombatLastDamage = calculatedTimestamp;
                            }

                            // Exit combat if no NPC has taken damage in the last 10 seconds
                            // JUNE 8, 2015 - CHANGE THIS TO 15 SECONDS as some bosses have a greater downtime (stored in 'downtimeSeconds')
                            if ((calculatedTimestamp - currentCombatLastDamage).TotalSeconds > downtimeSeconds)
                            {
                                //Console.WriteLine("Determined that encounter {0} has just ended, second is now {1}, last combat was {2}. Total time {3}",
                                //encounterNumber, calculatedTimestamp, currentCombatLastDamage, (calculatedTimestamp - currentCombatLastDamage).TotalSeconds);
                                //Thread.Sleep(5000);
                                inCombat = false;

                                var thisEncounter = encounters[encounterNumber - 1];
                                thisEncounter.Length = currentCombatLastDamage - currentCombatStarted;
                                thisEncounter.LineEnd = lineNumber;

                                Console.Write(thisEncounter.Length);
                                if (thisEncounter.Length.TotalSeconds <= 10)
                                {
                                    Console.WriteLine(" SKIP | EncounterTooShort");
                                }
                                else
                                {
                                    // Update encounter info here
                                    thisEncounter.UpdateDetailedEncounterInfo();
                                    if (bossNames.ContainsValue(thisEncounter.DisplayName))
                                    {
                                        encountersToSave.Add(thisEncounter);
                                        Console.WriteLine(" SAVE | {0} | {1}", thisEncounter.DisplayName, thisEncounter.EncounterSuccess ? "Kill" : "Wipe");
                                    }
                                    else
                                    {
                                        Console.WriteLine(" SKIP | {0} - NotABoss", thisEncounter.DisplayName);
                                    }
                                }

                                encounterNumber++;
                            }
                            else
                            {
                                if (!entry.IgnoreThisEvent)
                                {
                                    if (!fastParse)
                                    {
                                        encounters[encounterNumber - 1].Events.Add(entry);
                                    }
                                    // Update the last combat timestamp if it has changed
                                    if (calculatedTimestamp != currentCombatLastDamage)
                                    {
                                        if (entry.TargetType == CharacterType.Npc && entry.IsDamageType)
                                        {
                                            currentCombatLastDamage = entry.ParsedTimeStamp.AddDays(daysToAdd);
                                        }
                                    }
                                }
                            }

                            #endregion
                        }
                        else if (CombatStarted(entry))
                        {
                            #region Combat Started

                            //Console.Write("E{0}: {1}", encounterNumber, entry.ParsedTimeStamp.AddDays(daysToAdd));
                            Console.Write("E{0}: ", encounterNumber);
                            downtimeSeconds = 15; // Default this to 15
                            inCombat = true;
                            currentCombatStarted = entry.ParsedTimeStamp.AddDays(daysToAdd);
                            lastTimeStamp = entry.ParsedTimeStamp.AddDays(daysToAdd);
                            currentCombatLastDamage = entry.ParsedTimeStamp.AddDays(daysToAdd);
                            encounters.Add(new LogEncounter(fileDate) { Events = new List<CombatEntry>() { entry }, LineStart = lineNumber });

                            // Check if we need to extend the downtime timer for very specific fights
                            if ((entry.TargetType == CharacterType.Npc && (entry.TargetName == "Aqualix" || entry.TargetName == "Denizar")) ||
                                (entry.AttackerType == CharacterType.Npc && (entry.AttackerName == "Aqualix" || entry.AttackerName == "Denizar")))
                            {
                                downtimeSeconds = 30;
                                //Console.WriteLine("Inwar Darktide encounter detected, so setting combat downtime value to {0}", downtimeSeconds);
                            }

                            #endregion
                        }
                        //lineNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLine(string.Format("An error occurred! {0}", ex.Message));
            }

            #endregion

            DebugLine(string.Format("Parsed {0} lines", lineNumber));
            lineCount = lineNumber;
            encounters.Clear();
            return encountersToSave;
        }

        public static void ParseAndSave(ILogger logger, SessionLogInfo logInfo, string logFile, string smallSeparator, string largeSeparator, IAutoParserRepository repository)
        {
            _logger = logger;
            _autoParserRepository = repository;

            DebugLine(smallSeparator);
            DebugLine(string.Format("Beginning to parse {0}", logFile));

            var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, true);
            var sr = new StreamReader(fs);
            DebugLine(string.Format("Combatlog size: {0}mb", fs.Length / 1024 / 1024));
            long fileLength = fs.Length;

            #region Calculate when the session starts in local and UTC time
            // Session date is stored in UTC already, so instead of subtracting offset for UTC, add offset for local
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(logInfo.UploaderTimezone);
            DateTime localSessionDate = logInfo.SessionDate.Add(tzi.GetUtcOffset(logInfo.SessionDate));
            DebugLine(string.Format("Session date (local time): {0}, UTC time: {1}", localSessionDate, logInfo.SessionDate));
            #endregion
            #region Perform Parse
            Stopwatch sw = new Stopwatch();
            sw.Start();
            long totalLines = 0;
            var encounters = ParseStream(sr, logInfo.SessionDate, out totalLines);
            sr.Close();
            fs.Close();
            sw.Stop();
            _logger.Debug(string.Format("Parsed file in {0}, found {1} encounters", sw.Elapsed, encounters.Count));
            Console.WriteLine("Parsed file in {0}, found {1} encounters", sw.Elapsed, encounters.Count);
            #endregion
            #region First encounter loop - update info
            Stopwatch swFirstLoop = new Stopwatch();
            swFirstLoop.Start();
            var sessionPlayers = new List<Character>();
            var sessionAbilities = new List<Database.Models.Ability>();
            var sessionNpcDeaths = new List<NpcDeath>();

            Parallel.ForEach(encounters, p =>
            {
                p.UpdateDetailedEncounterInfo();
                p.IsPublic = logInfo.PublicSession;

            });
            swFirstLoop.Stop();
            DebugLine(string.Format("Parallel loop processing stage 1 completed in {0}", swFirstLoop.Elapsed));
            // Get the default difficulty ID
            var defaultDifficultyId = _autoParserRepository.GetDefaultDifficultyId();
            swFirstLoop.Reset();
            swFirstLoop.Start();
            foreach (var encounter in encounters)
            {
                foreach (var encounterPlayer in encounter.PlayersSeen.Where(encounterPlayer => !sessionPlayers.Any(p => p.Id == encounterPlayer.Id)))
                {
                    sessionPlayers.Add(encounterPlayer);
                }
                foreach (var encounterAbility in encounter.AbilitiesSeen.Where(encounterAbility => !sessionAbilities.Any(sa => sa.AbilityId == encounterAbility.AbilityId)))
                {
                    sessionAbilities.Add(encounterAbility);
                }

                // Npc Deaths
                foreach (var npcDeath in encounter.NpcDeaths)
                {
                    var sessionNpc = sessionNpcDeaths.FirstOrDefault(n => n.Name == npcDeath.Name);
                    if (sessionNpc == null)
                    {
                        sessionNpcDeaths.Add(npcDeath);
                    }
                    else
                    {
                        sessionNpc.Deaths += npcDeath.Deaths;
                    }
                }
            }
            swFirstLoop.Stop();
            //DebugLine(string.Format("Loop processing stage 2 completed in {0}", swFirstLoop.Elapsed));
            #endregion



            //DebugLine("--- Checking the player list... ");
            Console.WriteLine("--- Checking the player list... ");
            UpdatePlayersInDb(sessionPlayers, logInfo.UploaderShard);

            //DebugLine("--- Checking the ability list... ");
            Console.WriteLine("--- Checking the ability list... ");
            UpdateAbilitiesInDb(sessionAbilities);

            //DebugLine("--- Verifying ability damage types... ");
            Console.WriteLine("--- Verifying ability damage types... ");
            UpdateDbAbilitiesWithNoDamageType(sessionAbilities);

            Console.WriteLine("--- Updating NPC Deaths... ");
            // TEMP
            //Console.WriteLine("===========================");
            //foreach (var death in sessionNpcDeaths.OrderByDescending(n => n.Deaths))
            //{
            //    Console.WriteLine("{0}: {1}", death.Name, death.Deaths);
            //}
            //Console.WriteLine("===========================");
            UpdateNpcDeaths(sessionNpcDeaths);


            var ignoredEncounters = _autoParserRepository.GetIgnoredEncounterNames();

            #region Loop through encounters
            foreach (var encounter in encounters)
            {
                var thisEncounter = encounter;
                thisEncounter.DifficultyId = defaultDifficultyId;

                // Skip this encounter if it doesn't have a name (e.g. players dueling in between boss pulls)
                if (string.IsNullOrEmpty(thisEncounter.DisplayName)) continue;

                // Double check that the encounter duration has been set
                if ((int)encounter.Length.TotalSeconds == 0)
                {
                    var maxSeconds = encounter.Events.Max(e => e.SecondsElapsed);
                    encounter.Length = new TimeSpan(0, 0, 0, maxSeconds);
                }

                // Skip this encounter if it is less than 5 seconds in length
                // JUNE 6th, 2015 - Instead of checking the length, calculate it from the timestamps
                //var firstEvent = thisEncounter.Events.First();
                //var lastEvent = thisEncounter.Events.Last();
                //TimeSpan durationTs = thisEncounter.Events.First().ParsedTimeStamp -
                //                      thisEncounter.Events.Last().ParsedTimeStamp;
                // NOVEMBER 7th 2015 - switching back to duration/length total seconds as rolling over midnight was causing issues

                //if ((thisEncounter.Events.Last().ParsedTimeStamp - thisEncounter.Events.First().ParsedTimeStamp).TotalSeconds <= 5)
                if (thisEncounter.Length.TotalSeconds <= 5)
                {
                    DebugLine(string.Format("==== Skipping encounter: {0} (Less than 5 seconds long) ====", thisEncounter.DisplayName));
                    continue;
                }

                if (ignoredEncounters.Contains(thisEncounter.DisplayName))
                {
                    DebugLine(string.Format("==== Skipping encounter: {0} (It's on the ignore list!) ====", thisEncounter.DisplayName));
                    continue;
                }

                //DebugLine(smallSeparator);
                Console.WriteLine(smallSeparator);

                var bossFights = _autoParserRepository.GetBossFights(thisEncounter.DisplayName).ToList();
                if (!bossFights.Any())
                {
                    string error = string.Format("Boss check #1: No boss exists in the database with the name {0} ({1})", thisEncounter.DisplayName, thisEncounter.Length);
                    DebugLine(error);
                    continue;
                }
                BossFight bossFight = null;
                if (bossFights.Count == 1)
                {
                    bossFight = bossFights.First();
                }
                else
                {
                    bossFight = bossFights.FirstOrDefault(bf => bf.PriorityIfDuplicate);
                    if (bossFight == null)
                    {
                        // None of the boss fights have been marked with priority, so figure it out from the abilities
                        // If there's more than one, then at least one should have a unique ability name to look for so we can properly identify it
                        foreach (var fight in bossFights.Where(bf => !string.IsNullOrEmpty(bf.UniqueAbilityName)))
                        {
                            var uniqueAbilityName = fight.UniqueAbilityName;
                            // If we have put extra abilities in there, split it on the comma and check them all
                            if (uniqueAbilityName.IndexOf(",", StringComparison.Ordinal) > -1)
                            {
                                var uniqueAbilities = fight.UniqueAbilityName.Split(',').ToList();

                                if (thisEncounter.Events.Any(e => e.AttackerType == CharacterType.Npc && uniqueAbilities.Contains(e.AbilityName)))
                                {
                                    bossFight = fight;
                                    break;
                                }
                            }
                            else
                            {
                                if (thisEncounter.Events.Any(e => e.AttackerType == CharacterType.Npc && e.AbilityName == fight.UniqueAbilityName))
                                {
                                    bossFight = fight;
                                    break;
                                }
                            }

                        }
                    }
                }

                if (bossFight == null)
                {
                    var npcAbilities =
                        thisEncounter.Events.Where(
                            entry =>
                                entry.AttackerType == CharacterType.Npc && entry.ActionType != ActionType.TargetSlain)
                            .Select(a => a.AbilityName).Distinct().ToList();
                    string error = string.Format("Boss check #2: Couldn't match {0} ({1}) to any of the {2} bosses that match this name (No matching ability). " +
                                                 "Abilities: {3}.", thisEncounter.DisplayName, thisEncounter.Length, bossFights.Count,
                                                 string.Join(", ", npcAbilities));
                    DebugLine(error);
                    continue;
                }

                // Apply special processing if required
                thisEncounter = ProcessSpecialEncounter(thisEncounter, bossFight);

                // Check hitpoint value to determine if the boss took enough damage
                // Also don't bother checking this if it was not already going to be a successful encounter

                //if (thisEncounter.EncounterSuccess && bossFight.Hitpoints > 0)
                if (thisEncounter.EncounterSuccess)
                {
                    // Check if this encounter / bossfight has difficulty settings
                    if (_autoParserRepository.DifficultyRecordsExist(bossFight.Id))
                    {
                        var difficulties = _autoParserRepository.GetDifficultySettings(bossFight.Id).OrderByDescending(d => d.OverrideHitpoints);
                        // Loop through the difficulties from hardest to easiest to check the damage taken

                        foreach (var difficulty in difficulties)
                        {
                            // Check if there are a range of names we need to check
                            if (difficulty.OverrideHitpointTarget.Contains(","))
                            {
                                var npcsToCheck = difficulty.OverrideHitpointTarget.Split(',');
                                foreach (var npc in npcsToCheck)
                                {
                                    var npc1 = npc;
                                    var eventList = thisEncounter.Events.Where(e => e.TargetName == npc1).ToList();
                                    if (eventList.Any())
                                    {
                                        var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                        // var totalDamage = eventList.Sum(e => e.TotalDamage);
                                        encounter.ValidForRanking = totalDamage >= difficulty.OverrideHitpoints;
                                        Console.WriteLine("> 1 Targets ({0} mode). Damage taken {1} ({2})",
                                            difficulty.EncounterDifficulty.Name, totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                        //Console.WriteLine("More than one target to look for - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                        if (encounter.ValidForRanking)
                                        {
                                            thisEncounter.Difficulty = difficulty.EncounterDifficulty.Name;
                                            thisEncounter.DifficultyId = difficulty.EncounterDifficultyId;
                                            break;
                                        }
                                    }
                                }
                                if (encounter.ValidForRanking)
                                {
                                    // Break out of this foreach loop so we don't try and look for something with a lower difficulty even though this one has been found to be correct
                                    break;
                                }
                            }
                            else
                            {
                                var eventList = thisEncounter.Events.Where(e => e.TargetName == difficulty.OverrideHitpointTarget).ToList();
                                if (eventList.Any())
                                {
                                    var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                    // var totalDamage = eventList.Sum(e => e.TotalDamage);
                                    //Console.WriteLine("{0} damage taken for {1}. Is it enough? {2}", totalDamage, bossFight.HitpointTarget, totalDamage >= bossFight.Hitpoints ? "Yes" : "No");
                                    encounter.ValidForRanking = totalDamage >= difficulty.OverrideHitpoints;
                                    Console.WriteLine("1 Target ({0} mode). Damage taken {1} ({2})",
                                        difficulty.EncounterDifficulty.Name, totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                    //Console.WriteLine("Only one target to look for - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                    if (encounter.ValidForRanking)
                                    {
                                        thisEncounter.Difficulty = difficulty.EncounterDifficulty.Name;
                                        thisEncounter.DifficultyId = difficulty.EncounterDifficultyId;
                                        // Break out of this foreach loop so we don't try and look for something with a lower difficulty even though this one has been found to be correct
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        // No difficulty records exist for this BossFight, so stick to our normal tables for HP and targets
                        //Console.WriteLine("Criteria for this counter to be successful: Minimum damage taken: {0}", bossFight.Hitpoints);
                        //Console.WriteLine("NPC name override: {0}", string.IsNullOrEmpty(bossFight.HitpointTarget) ? "none" : bossFight.HitpointTarget);

                        // Check if we need to filter specific NPCs
                        if (!string.IsNullOrEmpty(bossFight.HitpointTarget))
                        {
                            if (bossFight.HitpointTarget.Contains(","))
                            {
                                // There's an array of two or more NPC names to check. This is usually only to cover the various languages
                                var npcsToCheck = bossFight.HitpointTarget.Split(',');
                                foreach (var npc in npcsToCheck)
                                {
                                    var npc1 = npc;
                                    var eventList = thisEncounter.Events.Where(e => e.TargetName == npc1).ToList();
                                    if (eventList.Any())
                                    {
                                        var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                        // var totalDamage = eventList.Sum(e => e.TotalDamage);
                                        //Console.WriteLine("{0} damage taken for {1}. Is it enough? {2}", totalDamage, npc1, totalDamage >= bossFight.Hitpoints ? "Yes" : "No");
                                        encounter.ValidForRanking = totalDamage >= bossFight.Hitpoints;
                                        Console.WriteLine("More than one target to look for - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                    }
                                }
                            }
                            else
                            {
                                var eventList = thisEncounter.Events.Where(e => e.TargetName == bossFight.HitpointTarget).ToList();
                                if (eventList.Any())
                                {
                                    var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                    // var totalDamage = eventList.Sum(e => e.TotalDamage);
                                    //Console.WriteLine("{0} damage taken for {1}. Is it enough? {2}", totalDamage, bossFight.HitpointTarget, totalDamage >= bossFight.Hitpoints ? "Yes" : "No");
                                    encounter.ValidForRanking = totalDamage >= bossFight.Hitpoints;
                                    Console.WriteLine("Only one target to look for - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                }
                            }
                        }
                        else
                        {
                            // Check for damage taken by the NPC with the same name as the BossFight
                            var eventList = thisEncounter.Events.Where(e => e.TargetName == bossFight.Name).ToList();
                            if (eventList.Any())
                            {
                                var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                // var totalDamage = eventList.Sum(e => e.TotalDamage);
                                //Console.WriteLine("{0} damage taken for {1}. Is it enough? {2}", totalDamage, bossFight.Name, totalDamage >= bossFight.Hitpoints ? "Yes" : "No");
                                encounter.ValidForRanking = totalDamage >= bossFight.Hitpoints;
                                Console.WriteLine("Looking for encounter name - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                            }
                        }
                    }
                }

                InfoLine(string.Format("Saving: {0} ({1}) {2}",
                        thisEncounter.EncounterSuccess
                            ? string.Format("{0} ({1}) {2} mode Kill", thisEncounter.DisplayName, bossFight.Instance.Name, thisEncounter.Difficulty)
                            : string.Format("{0} ({1}) Attempt", thisEncounter.DisplayName, bossFight.Instance.Name),
                        thisEncounter.Length, thisEncounter.Date.Add(tzi.GetUtcOffset(thisEncounter.Date))));

                var thisEncounterId = thisEncounter.InsertEncounterToDb(bossFight.Id, logInfo.UploaderId, logInfo.UploaderGuildId);

                if (thisEncounterId > -1)
                {
                    //DebugLine(string.Format("--- Encounter ID to use for this session: {0}", thisEncounterId));
                    //Console.WriteLine("--- Encounter ID to use for this session: {0}", thisEncounterId);
                    encounter.IdForSession = thisEncounterId;
                    encounter.UseInSession = true;
                }

            }
            #endregion

            var sessionEncounters = encounters.Where(e => e.UseInSession).ToList();
            if (sessionEncounters.Any())
            {
                // Update the session within the db to match what we've parsed
                sessionEncounters = sessionEncounters.OrderBy(e => e.Date).ToList();

                try
                {
                    var totalPlayedTime = GetSessionTotalPlayedTime(sessionEncounters);

                    var failCount = 0;

                    while (true)
                    {
                        if (failCount == 3)
                        {
                            DebugLine("There was an error updating the session log post-parse, and multiple retries failed to fix the issue :(");
                            break;
                        }

                        var result = _autoParserRepository.UpdateSessionLogPostParse(logInfo.SessionLogId, totalPlayedTime, fileLength, totalLines);
                        if (!result.Success)
                        {
                            DebugLine(string.Format("An error occurred while updating the session log post-parse: {0}", result.Message));
                            DebugLine("Retrying in 5 seconds...");
                            Thread.Sleep(5000); // Sleep 5 seconds and retry

                            failCount++;
                        }
                        else
                        {
                            if (failCount > 0)
                            {
                                DebugLine("Encountered an earlier error while updating the session log post-parse, but it has been successfully updated now.");
                            }
                            failCount = 0;
                            break;
                        }
                    }

                    //var result = _autoParserRepository.UpdateSessionLogPostParse(logInfo.SessionLogId, totalPlayedTime, fileLength, totalLines);
                    //DebugLine(result.Success
                    //    ? string.Format("Updated the SessionLog: LogSize {0}, TotalPlayedTime {1}, LogLines {2}", fileLength,
                    //        totalPlayedTime, totalLines)
                    //    : string.Format("An error occurred while updating the session log post-parse: {0}",
                    //        result.Message));
                }
                catch (Exception ex)
                {
                    DebugLine(string.Format("An error occurred while updating the session log post-parse: {0}", ex.Message));
                }

                var existingEncountersForSession = _autoParserRepository.GetEncounterIdsForSession(logInfo.SessionId);

                var newSessionEncounters = new List<SessionEncounter>();

                // Add missing encounters
                // In the event that we only just added the session, obviously we'll be adding all of the encounters here
                // This is really for the cases where we remove an encounter and want to reimport it, or where we are adding
                // additional logs to an existing session.
                foreach (var logEncounter in sessionEncounters)
                {
                    if (!existingEncountersForSession.Contains(logEncounter.IdForSession))
                    {
                        newSessionEncounters.Add(new SessionEncounter()
                        {
                            EncounterId = logEncounter.IdForSession,
                            SessionId = logInfo.SessionId
                        });
                    }
                }

                if (newSessionEncounters.Any())
                {
                    var failCount = 0;

                    while (true)
                    {
                        if (failCount == 3)
                        {
                            DebugLine("There was an error adding the encounters to this session, and multiple retries failed to fix the issue :(");
                            break;
                        }

                        var result = _autoParserRepository.AddSessionEncounters(newSessionEncounters);
                        if (!result.Success)
                        {
                            DebugLine(string.Format("An error occurred while adding the encounters to this session: {0}", result.Message));
                            DebugLine("Retrying in 5 seconds...");
                            Thread.Sleep(5000); // Sleep 5 seconds and retry

                            failCount++;
                        }
                        else
                        {
                            if (failCount > 0)
                            {
                                DebugLine("Encountered an earlier error while adding the encounters to this session, but they have been successfully added now.");
                            }
                            failCount = 0;
                            break;
                        }
                    }
                    //DebugLine(result.Success
                    //    ? string.Format("{0} encounters added to this session", newSessionEncounters.Count)
                    //    : string.Format("There was an error adding the encounters to this session! {0}", result.Message));
                }
                else
                {
                    //DebugLine("All of the encounters for this session already exist in the database!");
                    Console.WriteLine("All of the encounters for this session already exist in the database!");
                }

                try
                {
                    var failCount = 0;

                    while (true)
                    {
                        if (failCount == 3)
                        {
                            DebugLine("There was an error updating the session post-parse, and multiple retries failed to fix the issue :(");
                            break;
                        }

                        var result = _autoParserRepository.UpdateSessionPostParse(logInfo.SessionId);
                        if (!result.Success)
                        {
                            DebugLine(string.Format("An error occurred while updating the session post-parse: {0}", result.Message));
                            DebugLine("Retrying in 5 seconds...");
                            Thread.Sleep(5000); // Sleep 5 seconds and retry

                            failCount++;
                        }
                        else
                        {
                            if (failCount > 0)
                            {
                                DebugLine("Encountered an earlier error while updating the session post-parse, but it has been successfully updated now.");
                            }
                            failCount = 0;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLine(string.Format("An error occurred while updating the session post-parse: {0}", ex.Message));
                }


                //DebugLine(updateSession.Success
                //    ? string.Format("Session {0} updated successfully!", logInfo.SessionId)
                //    : string.Format("An error occurred while updating session {0} info: {1}", logInfo.SessionId,
                //        updateSession.Message));
                //InfoLine(updateSession.Success
                //    ? string.Format("Session {0} updated successfully!", logInfo.SessionId)
                //    : string.Format("An error occurred while updating session {0} info: {1}", logInfo.SessionId,
                //        updateSession.Message));

            }
        }

        /// <summary>
        /// Retrieves the valid list of BossFights from the database. Will retry 3 times if something goes wrong.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<int, string> GetBossFightsFromDatabase()
        {
            int failCount = 0;
            while (true)
            {
                if (failCount == 3)
                {
                    return new Dictionary<int, string>();
                }

                try
                {
                    var validBosses = _autoParserRepository.GetBossFights();
                    if (!validBosses.Any())
                    {
                        DebugLine(string.Format("There was an error while trying to retrieve boss names: No names were found!"));
                        DebugLine("Retrying in 5 seconds...");
                        Thread.Sleep(5000); // Sleep 5 seconds and retry

                        failCount++;
                    }
                    else
                    {
                        return validBosses;
                    }
                }
                catch (Exception ex)
                {
                    DebugLine(string.Format("There was an error while trying to retrieve boss names: {0}", ex.Message));
                    DebugLine("Retrying in 5 seconds...");
                    Thread.Sleep(5000); // Sleep 5 seconds and retry

                    failCount++;
                }
            }
        }

        /// <summary>
        /// This method is a test one to try and trim the memory usage as we go by removing wasted encounters while parsing
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="logInfo"></param>
        /// <param name="logFile"></param>
        /// <param name="smallSeparator"></param>
        /// <param name="largeSeparator"></param>
        /// <param name="repository"></param>
        public static void ParseAndSave_v2(ILogger logger, SessionLogInfo logInfo, string logFile, string smallSeparator, string largeSeparator, IAutoParserRepository repository)
        {
            _logger = logger;
            _autoParserRepository = repository;

            DebugLine(smallSeparator);
            DebugLine(string.Format("Beginning to parse {0}", logFile));

            var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, true);
            var sr = new StreamReader(fs);
            DebugLine(string.Format("Combatlog size: {0}mb", fs.Length / 1024 / 1024));
            long fileLength = fs.Length;

            #region Calculate when the session starts in local and UTC time
            // Session date is stored in UTC already, so instead of subtracting offset for UTC, add offset for local
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(logInfo.UploaderTimezone);
            DateTime localSessionDate = logInfo.SessionDate.Add(tzi.GetUtcOffset(logInfo.SessionDate));
            DebugLine(string.Format("Session date (local time): {0}, UTC time: {1}", localSessionDate, logInfo.SessionDate));
            #endregion
            #region Retrieve valid encounter names
            Console.Write("Retrieving encounter names...");
            var validBosses = GetBossFightsFromDatabase();
            if (!validBosses.Any())
            {
                Console.WriteLine("Error! No encounter names were returned. Exiting in 3 seconds...");
                Thread.Sleep(3000);
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine(" OK ({0})", validBosses.Count);
            }
            #endregion
            #region Perform Parse
            Stopwatch sw = new Stopwatch();
            sw.Start();
            long totalLines = 0;
            var encounters = ParseStream_v2(sr, logInfo.SessionDate, out totalLines, validBosses);
            sr.Close();
            fs.Close();
            sw.Stop();
            _logger.Debug(string.Format("Parsed file in {0}, found {1} encounters", sw.Elapsed, encounters.Count));
            Console.WriteLine("Parsed file in {0}, found {1} encounters", sw.Elapsed, encounters.Count);
            #endregion
            #region First encounter loop - update info
            Stopwatch swFirstLoop = new Stopwatch();
            swFirstLoop.Start();
            var sessionPlayers = new List<Character>();
            var sessionAbilities = new List<Database.Models.Ability>();
            var sessionNpcDeaths = new List<NpcDeath>();

            //Parallel.ForEach(encounters, p =>
            //{
            //    p.UpdateDetailedEncounterInfo();
            //    p.IsPublic = logInfo.PublicSession;

            //});
            //swFirstLoop.Stop();
            //DebugLine(string.Format("Parallel loop processing stage 1 completed in {0}", swFirstLoop.Elapsed));
            // Get the default difficulty ID
            var defaultDifficultyId = _autoParserRepository.GetDefaultDifficultyId();
            swFirstLoop.Reset();
            swFirstLoop.Start();
            foreach (var encounter in encounters)
            {
                foreach (var encounterPlayer in encounter.PlayersSeen.Where(encounterPlayer => !sessionPlayers.Any(p => p.Id == encounterPlayer.Id)))
                {
                    sessionPlayers.Add(encounterPlayer);
                }
                foreach (var encounterAbility in encounter.AbilitiesSeen.Where(encounterAbility => !sessionAbilities.Any(sa => sa.AbilityId == encounterAbility.AbilityId)))
                {
                    sessionAbilities.Add(encounterAbility);
                }

                // Npc Deaths
                foreach (var npcDeath in encounter.NpcDeaths)
                {
                    var sessionNpc = sessionNpcDeaths.FirstOrDefault(n => n.Name == npcDeath.Name);
                    if (sessionNpc == null)
                    {
                        sessionNpcDeaths.Add(npcDeath);
                    }
                    else
                    {
                        sessionNpc.Deaths += npcDeath.Deaths;
                    }
                }
            }
            swFirstLoop.Stop();
            //DebugLine(string.Format("Loop processing stage 2 completed in {0}", swFirstLoop.Elapsed));
            #endregion



            //DebugLine("--- Checking the player list... ");
            Console.WriteLine("--- Checking the player list... ");
            UpdatePlayersInDb(sessionPlayers, logInfo.UploaderShard);

            //DebugLine("--- Checking the ability list... ");
            Console.WriteLine("--- Checking the ability list... ");
            UpdateAbilitiesInDb(sessionAbilities);

            //DebugLine("--- Verifying ability damage types... ");
            Console.WriteLine("--- Verifying ability damage types... ");
            UpdateDbAbilitiesWithNoDamageType(sessionAbilities);

            Console.WriteLine("--- Updating NPC Deaths... ");
            // TEMP
            //Console.WriteLine("===========================");
            //foreach (var death in sessionNpcDeaths.OrderByDescending(n => n.Deaths))
            //{
            //    Console.WriteLine("{0}: {1}", death.Name, death.Deaths);
            //}
            //Console.WriteLine("===========================");
            UpdateNpcDeaths(sessionNpcDeaths);


            var ignoredEncounters = _autoParserRepository.GetIgnoredEncounterNames();

            #region Loop through encounters
            foreach (var encounter in encounters)
            {
                var thisEncounter = encounter;
                thisEncounter.DifficultyId = defaultDifficultyId;

                // Skip this encounter if it doesn't have a name (e.g. players dueling in between boss pulls)
                if (string.IsNullOrEmpty(thisEncounter.DisplayName)) continue;

                // Double check that the encounter duration has been set
                if ((int)encounter.Length.TotalSeconds == 0)
                {
                    var maxSeconds = encounter.Events.Max(e => e.SecondsElapsed);
                    encounter.Length = new TimeSpan(0, 0, 0, maxSeconds);
                }

                // Skip this encounter if it is less than 5 seconds in length
                // JUNE 6th, 2015 - Instead of checking the length, calculate it from the timestamps
                //var firstEvent = thisEncounter.Events.First();
                //var lastEvent = thisEncounter.Events.Last();
                //TimeSpan durationTs = thisEncounter.Events.First().ParsedTimeStamp -
                //                      thisEncounter.Events.Last().ParsedTimeStamp;
                // NOVEMBER 7th 2015 - switching back to duration/length total seconds as rolling over midnight was causing issues

                //if ((thisEncounter.Events.Last().ParsedTimeStamp - thisEncounter.Events.First().ParsedTimeStamp).TotalSeconds <= 5)
                if (thisEncounter.Length.TotalSeconds <= 5)
                {
                    DebugLine(string.Format("==== Skipping encounter: {0} (Less than 5 seconds long) ====", thisEncounter.DisplayName));
                    continue;
                }

                if (ignoredEncounters.Contains(thisEncounter.DisplayName))
                {
                    DebugLine(string.Format("==== Skipping encounter: {0} (It's on the ignore list!) ====", thisEncounter.DisplayName));
                    continue;
                }

                //DebugLine(smallSeparator);
                Console.WriteLine(smallSeparator);

                var bossFights = _autoParserRepository.GetBossFights(thisEncounter.DisplayName).ToList();
                if (!bossFights.Any())
                {
                    string error = string.Format("Boss check #1: No boss exists in the database with the name {0} ({1})", thisEncounter.DisplayName, thisEncounter.Length);
                    DebugLine(error);
                    continue;
                }
                BossFight bossFight = null;
                if (bossFights.Count == 1)
                {
                    bossFight = bossFights.First();
                }
                else
                {
                    bossFight = bossFights.FirstOrDefault(bf => bf.PriorityIfDuplicate);
                    if (bossFight == null)
                    {
                        // None of the boss fights have been marked with priority, so figure it out from the abilities
                        // If there's more than one, then at least one should have a unique ability name to look for so we can properly identify it
                        foreach (var fight in bossFights.Where(bf => !string.IsNullOrEmpty(bf.UniqueAbilityName)))
                        {
                            var uniqueAbilityName = fight.UniqueAbilityName;
                            // If we have put extra abilities in there, split it on the comma and check them all
                            if (uniqueAbilityName.IndexOf(",", StringComparison.Ordinal) > -1)
                            {
                                var uniqueAbilities = fight.UniqueAbilityName.Split(',').ToList();

                                if (thisEncounter.Events.Any(e => e.AttackerType == CharacterType.Npc && uniqueAbilities.Contains(e.AbilityName)))
                                {
                                    bossFight = fight;
                                    break;
                                }
                            }
                            else
                            {
                                if (thisEncounter.Events.Any(e => e.AttackerType == CharacterType.Npc && e.AbilityName == fight.UniqueAbilityName))
                                {
                                    bossFight = fight;
                                    break;
                                }
                            }

                        }
                    }
                }

                if (bossFight == null)
                {
                    var npcAbilities =
                        thisEncounter.Events.Where(
                            entry =>
                                entry.AttackerType == CharacterType.Npc && entry.ActionType != ActionType.TargetSlain)
                            .Select(a => a.AbilityName).Distinct().ToList();
                    string error = string.Format("Boss check #2: Couldn't match {0} ({1}) to any of the {2} bosses that match this name (No matching ability). " +
                                                 "Abilities: {3}.", thisEncounter.DisplayName, thisEncounter.Length, bossFights.Count,
                                                 string.Join(", ", npcAbilities));
                    DebugLine(error);
                    continue;
                }

                // Apply special processing if required
                thisEncounter = ProcessSpecialEncounter(thisEncounter, bossFight);

                // Check hitpoint value to determine if the boss took enough damage
                // Also don't bother checking this if it was not already going to be a successful encounter

                //if (thisEncounter.EncounterSuccess && bossFight.Hitpoints > 0)
                if (thisEncounter.EncounterSuccess)
                {
                    // Check if this encounter / bossfight has difficulty settings
                    if (_autoParserRepository.DifficultyRecordsExist(bossFight.Id))
                    {
                        var difficulties = _autoParserRepository.GetDifficultySettings(bossFight.Id).OrderByDescending(d => d.OverrideHitpoints);
                        // Loop through the difficulties from hardest to easiest to check the damage taken

                        foreach (var difficulty in difficulties)
                        {
                            // Check if there are a range of names we need to check
                            if (difficulty.OverrideHitpointTarget.Contains(","))
                            {
                                var npcsToCheck = difficulty.OverrideHitpointTarget.Split(',');
                                foreach (var npc in npcsToCheck)
                                {
                                    var npc1 = npc;
                                    var eventList = thisEncounter.Events.Where(e => e.TargetName == npc1).ToList();
                                    if (eventList.Any())
                                    {
                                        var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                        // var totalDamage = eventList.Sum(e => e.TotalDamage);
                                        encounter.ValidForRanking = totalDamage >= difficulty.OverrideHitpoints;
                                        Console.WriteLine("> 1 Targets ({0} mode). Damage taken {1} ({2})",
                                            difficulty.EncounterDifficulty.Name, totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                        //Console.WriteLine("More than one target to look for - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                        if (encounter.ValidForRanking)
                                        {
                                            thisEncounter.Difficulty = difficulty.EncounterDifficulty.Name;
                                            thisEncounter.DifficultyId = difficulty.EncounterDifficultyId;
                                            break;
                                        }
                                    }
                                }
                                if (encounter.ValidForRanking)
                                {
                                    // Break out of this foreach loop so we don't try and look for something with a lower difficulty even though this one has been found to be correct
                                    break;
                                }
                            }
                            else
                            {
                                var eventList = thisEncounter.Events.Where(e => e.TargetName == difficulty.OverrideHitpointTarget).ToList();
                                if (eventList.Any())
                                {
                                    var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                    // var totalDamage = eventList.Sum(e => e.TotalDamage);
                                    //Console.WriteLine("{0} damage taken for {1}. Is it enough? {2}", totalDamage, bossFight.HitpointTarget, totalDamage >= bossFight.Hitpoints ? "Yes" : "No");
                                    encounter.ValidForRanking = totalDamage >= difficulty.OverrideHitpoints;
                                    Console.WriteLine("1 Target ({0} mode). Damage taken {1} ({2})",
                                        difficulty.EncounterDifficulty.Name, totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                    //Console.WriteLine("Only one target to look for - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                    if (encounter.ValidForRanking)
                                    {
                                        thisEncounter.Difficulty = difficulty.EncounterDifficulty.Name;
                                        thisEncounter.DifficultyId = difficulty.EncounterDifficultyId;
                                        // Break out of this foreach loop so we don't try and look for something with a lower difficulty even though this one has been found to be correct
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        // No difficulty records exist for this BossFight, so stick to our normal tables for HP and targets
                        //Console.WriteLine("Criteria for this counter to be successful: Minimum damage taken: {0}", bossFight.Hitpoints);
                        //Console.WriteLine("NPC name override: {0}", string.IsNullOrEmpty(bossFight.HitpointTarget) ? "none" : bossFight.HitpointTarget);

                        // Check if we need to filter specific NPCs
                        if (!string.IsNullOrEmpty(bossFight.HitpointTarget))
                        {
                            if (bossFight.HitpointTarget.Contains(","))
                            {
                                // There's an array of two or more NPC names to check. This is usually only to cover the various languages
                                var npcsToCheck = bossFight.HitpointTarget.Split(',');
                                foreach (var npc in npcsToCheck)
                                {
                                    var npc1 = npc;
                                    var eventList = thisEncounter.Events.Where(e => e.TargetName == npc1).ToList();
                                    if (eventList.Any())
                                    {
                                        var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                        //var totalDamage = eventList.Sum(e => e.TotalDamage);
                                        //Console.WriteLine("{0} damage taken for {1}. Is it enough? {2}", totalDamage, npc1, totalDamage >= bossFight.Hitpoints ? "Yes" : "No");
                                        encounter.ValidForRanking = totalDamage >= bossFight.Hitpoints;
                                        Console.WriteLine("More than one target to look for - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                    }
                                }
                            }
                            else
                            {
                                var eventList = thisEncounter.Events.Where(e => e.TargetName == bossFight.HitpointTarget).ToList();
                                if (eventList.Any())
                                {
                                    var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                    //var totalDamage = eventList.Sum(e => e.TotalDamage);
                                    //Console.WriteLine("{0} damage taken for {1}. Is it enough? {2}", totalDamage, bossFight.HitpointTarget, totalDamage >= bossFight.Hitpoints ? "Yes" : "No");
                                    encounter.ValidForRanking = totalDamage >= bossFight.Hitpoints;
                                    Console.WriteLine("Only one target to look for - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                                }
                            }
                        }
                        else
                        {
                            // Check for damage taken by the NPC with the same name as the BossFight
                            var eventList = thisEncounter.Events.Where(e => e.TargetName == bossFight.Name).ToList();
                            if (eventList.Any())
                            {
                                var totalDamage = CalculateTotalDamage(bossFight, eventList);
                                //var totalDamage = eventList.Sum(e => e.TotalDamage);
                                //Console.WriteLine("{0} damage taken for {1}. Is it enough? {2}", totalDamage, bossFight.Name, totalDamage >= bossFight.Hitpoints ? "Yes" : "No");
                                encounter.ValidForRanking = totalDamage >= bossFight.Hitpoints;
                                Console.WriteLine("Looking for encounter name - damage taken {0} - result {1}", totalDamage, encounter.ValidForRanking ? "PASS" : "FAIL");
                            }
                        }
                    }
                }

                InfoLine(string.Format("Saving: {0} ({1}) {2}",
                        thisEncounter.EncounterSuccess
                            ? string.Format("{0} ({1}) {2} mode Kill", thisEncounter.DisplayName, bossFight.Instance.Name, thisEncounter.Difficulty)
                            : string.Format("{0} ({1}) Attempt", thisEncounter.DisplayName, bossFight.Instance.Name),
                        thisEncounter.Length, thisEncounter.Date.Add(tzi.GetUtcOffset(thisEncounter.Date))));

                var thisEncounterId = thisEncounter.InsertEncounterToDb(bossFight.Id, logInfo.UploaderId, logInfo.UploaderGuildId);

                if (thisEncounterId > -1)
                {
                    //DebugLine(string.Format("--- Encounter ID to use for this session: {0}", thisEncounterId));
                    //Console.WriteLine("--- Encounter ID to use for this session: {0}", thisEncounterId);
                    encounter.IdForSession = thisEncounterId;
                    encounter.UseInSession = true;
                }

            }
            #endregion

            var sessionEncounters = encounters.Where(e => e.UseInSession).ToList();
            if (sessionEncounters.Any())
            {
                // Update the session within the db to match what we've parsed
                sessionEncounters = sessionEncounters.OrderBy(e => e.Date).ToList();

                try
                {
                    var totalPlayedTime = GetSessionTotalPlayedTime(sessionEncounters);

                    var failCount = 0;

                    while (true)
                    {
                        if (failCount == 3)
                        {
                            DebugLine("There was an error updating the session log post-parse, and multiple retries failed to fix the issue :(");
                            break;
                        }

                        var result = _autoParserRepository.UpdateSessionLogPostParse(logInfo.SessionLogId, totalPlayedTime, fileLength, totalLines);
                        if (!result.Success)
                        {
                            DebugLine(string.Format("An error occurred while updating the session log post-parse: {0}", result.Message));
                            DebugLine("Retrying in 5 seconds...");
                            Thread.Sleep(5000); // Sleep 5 seconds and retry

                            failCount++;
                        }
                        else
                        {
                            if (failCount > 0)
                            {
                                DebugLine("Encountered an earlier error while updating the session log post-parse, but it has been successfully updated now.");
                            }
                            failCount = 0;
                            break;
                        }
                    }

                    //var result = _autoParserRepository.UpdateSessionLogPostParse(logInfo.SessionLogId, totalPlayedTime, fileLength, totalLines);
                    //DebugLine(result.Success
                    //    ? string.Format("Updated the SessionLog: LogSize {0}, TotalPlayedTime {1}, LogLines {2}", fileLength,
                    //        totalPlayedTime, totalLines)
                    //    : string.Format("An error occurred while updating the session log post-parse: {0}",
                    //        result.Message));
                }
                catch (Exception ex)
                {
                    DebugLine(string.Format("An error occurred while updating the session log post-parse: {0}", ex.Message));
                }

                var existingEncountersForSession = _autoParserRepository.GetEncounterIdsForSession(logInfo.SessionId);

                var newSessionEncounters = new List<SessionEncounter>();

                // Add missing encounters
                // In the event that we only just added the session, obviously we'll be adding all of the encounters here
                // This is really for the cases where we remove an encounter and want to reimport it, or where we are adding
                // additional logs to an existing session.
                foreach (var logEncounter in sessionEncounters)
                {
                    if (!existingEncountersForSession.Contains(logEncounter.IdForSession))
                    {
                        newSessionEncounters.Add(new SessionEncounter()
                        {
                            EncounterId = logEncounter.IdForSession,
                            SessionId = logInfo.SessionId
                        });
                    }
                }

                if (newSessionEncounters.Any())
                {
                    var failCount = 0;

                    while (true)
                    {
                        if (failCount == 3)
                        {
                            DebugLine("There was an error adding the encounters to this session, and multiple retries failed to fix the issue :(");
                            break;
                        }

                        var result = _autoParserRepository.AddSessionEncounters(newSessionEncounters);
                        if (!result.Success)
                        {
                            DebugLine(string.Format("An error occurred while adding the encounters to this session: {0}", result.Message));
                            DebugLine("Retrying in 5 seconds...");
                            Thread.Sleep(5000); // Sleep 5 seconds and retry

                            failCount++;
                        }
                        else
                        {
                            if (failCount > 0)
                            {
                                DebugLine("Encountered an earlier error while adding the encounters to this session, but they have been successfully added now.");
                            }
                            failCount = 0;
                            break;
                        }
                    }
                    //DebugLine(result.Success
                    //    ? string.Format("{0} encounters added to this session", newSessionEncounters.Count)
                    //    : string.Format("There was an error adding the encounters to this session! {0}", result.Message));
                }
                else
                {
                    //DebugLine("All of the encounters for this session already exist in the database!");
                    Console.WriteLine("All of the encounters for this session already exist in the database!");
                }

                try
                {
                    var failCount = 0;

                    while (true)
                    {
                        if (failCount == 3)
                        {
                            DebugLine("There was an error updating the session post-parse, and multiple retries failed to fix the issue :(");
                            break;
                        }

                        var result = _autoParserRepository.UpdateSessionPostParse(logInfo.SessionId);
                        if (!result.Success)
                        {
                            DebugLine(string.Format("An error occurred while updating the session post-parse: {0}", result.Message));
                            DebugLine("Retrying in 5 seconds...");
                            Thread.Sleep(5000); // Sleep 5 seconds and retry

                            failCount++;
                        }
                        else
                        {
                            if (failCount > 0)
                            {
                                DebugLine("Encountered an earlier error while updating the session post-parse, but it has been successfully updated now.");
                            }
                            failCount = 0;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLine(string.Format("An error occurred while updating the session post-parse: {0}", ex.Message));
                }


                //DebugLine(updateSession.Success
                //    ? string.Format("Session {0} updated successfully!", logInfo.SessionId)
                //    : string.Format("An error occurred while updating session {0} info: {1}", logInfo.SessionId,
                //        updateSession.Message));
                //InfoLine(updateSession.Success
                //    ? string.Format("Session {0} updated successfully!", logInfo.SessionId)
                //    : string.Format("An error occurred while updating session {0} info: {1}", logInfo.SessionId,
                //        updateSession.Message));

            }
        }

    }
}
