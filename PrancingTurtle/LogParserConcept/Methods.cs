using Common;
using LogParserConcept.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LogParserConcept.Models.ParsedStats;
using Newtonsoft.Json;

namespace LogParserConcept
{
    public static class Methods
    {
        const string ParentPath = @"F:\PrancingTurtle\Output";
        /// <summary>
        /// The files to be generated within each encounter
        /// </summary>
        static Dictionary<EncounterContainerType, string> encounterContainers = new Dictionary<EncounterContainerType, string>
        {
            { EncounterContainerType.Damage, "damage.txt"},
            { EncounterContainerType.Heal, "heal.txt"},
            { EncounterContainerType.Shield, "shield.txt"},
            { EncounterContainerType.Buff, "buff.txt"},
            { EncounterContainerType.Debuff, "debuff.txt"},
            { EncounterContainerType.Death, "death.txt"}
        };

        private static Regex _isInt = new Regex("^[0-9]+$");

        /// <summary>
        /// The following list is used for specific fights that require a custom downtime length in order to avoid splitting parses
        /// </summary>
        private static List<DowntimeOverride> DowntimeOverrides = new List<DowntimeOverride>()
        {
            new DowntimeOverride{NpcName = "Prince Hylas", DowntimeSeconds = 50},
            new DowntimeOverride{NpcName = "Aqualix", DowntimeSeconds = 30},
            new DowntimeOverride{NpcName = "Denizar", DowntimeSeconds = 30}
        };

        /// <summary>
        /// The default period of time in seconds after the last
        /// damage event before declaring an encounter ended
        /// </summary>
        private const int DefaultEncounterDowntime = 15;

        public static async Task ParseAsync(SessionLogInfo logInfo, string logFile)
        {
            // Check the filesystem first
            var sessionPath = Path.Combine(ParentPath, logInfo.SessionId.ToString());
            if (CheckFolderExists(sessionPath) == false)
            {
                Console.WriteLine("Can't continue as something went wrong while checking that the session folder exists.");
                Console.ReadLine();
                Environment.Exit(1);
            }

            Console.WriteLine("Folder structure checked OK.");

            Console.WriteLine($"Beginning to parse {logFile}");
            await using var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true);
            using var sr = new StreamReader(fs);
            #region Calculate when the session starts in local and UTC time
            // Session date is stored in UTC already, so instead of subtracting offset for UTC, add offset for local
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(logInfo.UploaderTimezone);
            DateTime localSessionDate = logInfo.SessionDate.Add(tzi.GetUtcOffset(logInfo.SessionDate));
            Console.WriteLine($"Session date (local time): {localSessionDate}, UTC time: {logInfo.SessionDate}");
            #endregion
            var logType = LogType.Unknown;

            // Main loop
            var line = "";
            var lineNumber = 1;
            var downtimeSeconds = DefaultEncounterDowntime;
            int encounterNumber = 0;
            bool inCombat = false;
            var currentCombatStarted = new DateTime();
            var currentCombatLastDamage = new DateTime();
            double daysToAdd = 0;
            var lastTimeStamp = new DateTime();
            //var calculatedTimestamp = new DateTime();
            var encounterLength = new TimeSpan(0, 0, 0, 0);
            var unwrittenLines = new List<string>();

            // Per encounter counters
            long totalDamage = 0;
            long totalHealing = 0;
            long totalShielding = 0;
            int damageEvents = 0;
            int healingEvents = 0;
            int shieldingEvents = 0;
            int buffEvents = 0;
            Dictionary<string, int> playerDeaths = new Dictionary<string, int>();
            Dictionary<string, int> npcDeaths = new Dictionary<string, int>();
            Dictionary<string, long> npcDamageTaken = new Dictionary<string, long>();

            // Globals
            var players = new HashSet<string>();
            var npcs = new HashSet<string>();
            var pets = new HashSet<string>();
            var abilities = new HashSet<string>();
            Dictionary<string, int> globalPlayerDeaths = new Dictionary<string, int>();
            Dictionary<string, int> globalNpcDeaths = new Dictionary<string, int>();
            Dictionary<string, long> globalNpcDamageTaken = new Dictionary<string, long>();
            long globalTotalDamage = 0;
            long globalTotalHealing = 0;
            long globalTotalShielding = 0;
            int globalDamageEvents = 0;
            int globalHealingEvents = 0;
            int globalShieldingEvents = 0;
            int globalBuffEvents = 0;
            var totalEncounterDuration = new TimeSpan();

            // Testing
            Stopwatch encWriter = new Stopwatch();

            while ((line = sr.ReadLine()) != null)
            {
                // If we don't know what type of log this is yet, then figure that out now
                if (logType == LogType.Unknown)
                {
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

                    // If we haven't figured out the log type by this point, skip processing this line and check the next one
                    if (logType == LogType.Unknown)
                    {
                        continue;
                    }
                }

                var entry = await ParseLine(line, logType);
                if (entry.ValidEntry == false)
                {
                    if (!line.Contains("Combat Begin") && !line.Contains("Combat End"))
                    {
                        Console.WriteLine($"Line {lineNumber} is not valid ({entry.InvalidReason})");
                        Console.WriteLine(line);
                        Console.WriteLine();
                    }
                    // Skip the rest of this loop, but increment the line number before we do
                    lineNumber++;
                    continue;
                }

                if (!inCombat)
                {
                    // Not in combat yet. Check to see whether this particular log entry should 'start' combat
                    if (entry.ShouldStartCombat())
                    {
                        // Blank line just to be nice
                        Console.WriteLine();

                        // Begin combat.
                        encounterNumber++;
                        encounterLength = new TimeSpan(0, 0, 0, 0);
                        Console.WriteLine($"Encounter {encounterNumber} started at {entry.ParsedTimeStamp.AddDays(daysToAdd)}");
                        // Default the encounter time unless it needs to be overridden
                        downtimeSeconds = entry.GetDowntimeValueForEncounter();
                        if (downtimeSeconds != DefaultEncounterDowntime)
                        {
                            Console.WriteLine($"Detected an overridden encounter downtime. The value is now {downtimeSeconds}");
                        }
                        inCombat = true;
                        unwrittenLines = new List<string>();

                        // Set the variables that we'll use to determine when the encounter should end
                        currentCombatStarted = entry.ParsedTimeStamp.AddDays(daysToAdd);
                        lastTimeStamp = entry.ParsedTimeStamp.AddDays(daysToAdd);
                        currentCombatLastDamage = entry.ParsedTimeStamp.AddDays(daysToAdd);

                        // Update the time elapsed for this event
                        entry.SetTimeElapsed(currentCombatStarted, true);

                        // Create the files that we'll use for this encounter
                        var encounterContainersCreated = CreateEncounterContainers(sessionPath, encounterNumber);
                        if (!encounterContainersCreated)
                        {
                            Console.WriteLine($"Unable to create containers for encounter {encounterNumber}.");
                        }

                        // Reset counters
                        totalDamage = 0;
                        totalHealing = 0;
                        totalShielding = 0;
                        damageEvents = 0;
                        healingEvents = 0;
                        shieldingEvents = 0;
                        buffEvents = 0;
                        playerDeaths = new Dictionary<string, int>();
                        npcDeaths = new Dictionary<string, int>();
                        npcDamageTaken = new Dictionary<string, long>();

                        #region Single line append
                        encWriter.Reset();
                        encWriter.Start();
                        // NB: This is only used to append lines one at a time.
                        // Add this entry to the file that it belongs to
                        await AppendLine(sessionPath, encounterNumber, encounterContainers[entry.ContainerType], line);
                        // Testing: Write to the encounter file
                        await AppendLine_SingleFileForEncounter(sessionPath, encounterNumber, line);

                        //Console.WriteLine($"{entry.SecondsElapsed}: {line}");

                        #endregion
                    }
                }
                else
                {
                    var secondDifference =
                        (int)(entry.ParsedTimeStamp.AddDays(daysToAdd) - lastTimeStamp).TotalSeconds;
                    if (secondDifference == 0 || secondDifference > 0)
                    {
                        // Timestamp hasn't changed, or it's later in the same day
                        entry.CalculatedTimeStamp = entry.ParsedTimeStamp.AddDays(daysToAdd);
                    }
                    else
                    {
                        // We have just rolled over midnight 
                        daysToAdd++;
                        entry.CalculatedTimeStamp = entry.ParsedTimeStamp.AddDays(daysToAdd);
                        //currentCombatLastDamage = calculatedTimestamp;
                    }

                    // Update the time elapsed for this event
                    entry.SetTimeElapsed(currentCombatStarted);

                    if ((entry.CalculatedTimeStamp - currentCombatLastDamage).TotalSeconds > downtimeSeconds)
                    {
                        encWriter.Stop();
                        inCombat = false;
                        encounterLength = currentCombatLastDamage - currentCombatStarted;
                        // Update global encounter totals
                        totalEncounterDuration += encounterLength;
                        Console.WriteLine($"Combat for encounter {encounterNumber} ended at {entry.ParsedTimeStamp.AddDays(daysToAdd)} ({entry.SecondsElapsed} seconds elapsed). Time elapsed for reading and writing: {encWriter.Elapsed}");
                        Console.WriteLine($"The last damage was detected at {currentCombatLastDamage}");
                        var encInfo = new List<string>
                        {
                            $"Encounter {encounterNumber}",
                            $"Started: {currentCombatStarted}",
                            $"Ended: {currentCombatLastDamage}",
                            $"Duration: {encounterLength}",
                            "===================",
                            $"Total damage done: {totalDamage}. Events: {damageEvents}",
                            $"Total healing done: {totalHealing}. Events: {healingEvents}",
                            $"Total shielding done: {totalShielding}. Events: {shieldingEvents}",
                            "-------------------",
                            $"Deaths: {playerDeaths.Sum(kvp => kvp.Value) + npcDeaths.Sum(kvp => kvp.Value)}",
                            "-------------------",
                        };

                        encInfo.AddRange(playerDeaths.OrderByDescending(kvp => kvp.Value).Select(death => $"{death.Key}: {death.Value}"));
                        encInfo.Add("-------------------");
                        encInfo.AddRange(npcDeaths.OrderByDescending(kvp => kvp.Value).Select(death => $"{death.Key}: {death.Value}"));

                        var encStats = new EncounterStats(
                            encounterNumber, encounterLength, totalDamage, damageEvents, totalHealing, healingEvents,
                            totalShielding, shieldingEvents, playerDeaths, npcDeaths, npcDamageTaken);

                        // Old pre-JSON encounter stats
                        //await WriteEncounterInfo(sessionPath, encounterNumber, encInfo);
                        // JSON
                        //await WriteEncounterStats(sessionPath, encounterNumber, encStats);

                        // Remove the encounter folder if it's not long enough to warrant saving
                        encounterLength = currentCombatLastDamage - currentCombatStarted;
                        if (encounterLength.TotalSeconds < 5)
                        {
                            // Remove the encounter (not long enough)
                            var encRemoved = RemoveEncounterFolder(sessionPath, encounterNumber);
                            if (encRemoved)
                            {
                                Console.WriteLine($"Encounter {encounterNumber} removed (<5s)");
                            }
                        }

                        // Update the session text file
                        var globalInfo = new List<string>
                        {
                            $"Total encounters: {encounterNumber}",
                            $"Total encounter time: <Not calculated>",
                            "===================",
                            $"Total damage done: {globalTotalDamage}. Events: {globalDamageEvents}",
                            $"Total healing done: {globalTotalHealing}. Events: {globalHealingEvents}",
                            $"Total shielding done: {globalTotalShielding}. Events: {globalShieldingEvents}",
                            "-------------------",
                            $"Deaths: {globalPlayerDeaths.Sum(kvp => kvp.Value) + globalNpcDeaths.Sum(kvp => kvp.Value)}",
                            "-------------------",
                        };

                        globalInfo.AddRange(globalPlayerDeaths.OrderByDescending(kvp => kvp.Value).Select(death => $"{death.Key}: {death.Value}"));
                        globalInfo.Add("-------------------");
                        globalInfo.AddRange(globalNpcDeaths.OrderByDescending(kvp => kvp.Value).Select(death => $"{death.Key}: {death.Value}"));

                        // Old pre-JSON encounter stats
                        //await WriteSessionInfo(sessionPath, globalInfo);
                        // JSON
                        //await WriteSessionStats(sessionPath, new SessionStats
                        //(encounterNumber, totalEncounterDuration, globalTotalDamage, globalDamageEvents,
                        //    globalTotalHealing, globalHealingEvents, globalTotalShielding, globalShieldingEvents,
                        //    globalPlayerDeaths, globalNpcDeaths, globalNpcDamageTaken));
                    }
                    // Still in combat but not outside our downtime period. Write the event if we need to
                    else if (!entry.IgnoreThisEvent)
                    {
                        // Update the last combat timestamp if it has changed
                        if (entry.CalculatedTimeStamp != currentCombatLastDamage)
                        {
                            if (entry.TargetType == CharacterType.Npc && entry.IsDamageType)
                            {
                                currentCombatLastDamage = entry.ParsedTimeStamp.AddDays(daysToAdd);
                            }
                        }

                        // Write damage/death records immediately, but collect all other logged entries and write them if we find another damage
                        // record within the permitted downtime. If nothing happens for X seconds, then discard unwritten lines.
                        switch (entry.ContainerType)
                        {
                            case EncounterContainerType.Damage:
                            case EncounterContainerType.Death:
                                // Write any previously unwritten lines to the log
                                if (unwrittenLines.Any())
                                {
                                    await AppendLine_SingleFileForEncounter(sessionPath, encounterNumber, unwrittenLines);
                                    unwrittenLines = new List<string>();
                                }
                                await AppendLine_SingleFileForEncounter(sessionPath, encounterNumber, line);
                                break;
                            case EncounterContainerType.NotLogged:
                            case EncounterContainerType.Unknown:
                                // Do nothing with these
                                break;
                            default:
                                // Not a death or damage record so add this to the list of unwritten lines
                                unwrittenLines.Add(line);
                                break;
                        }

                        // This switch works well, but still logs entries after it needs to (when combat is finished)
                        //switch (entry.ContainerType)
                        //{
                        //    case EncounterContainerType.Unknown:
                        //        Console.WriteLine($"  Unknown container type. Line: {line}");
                        //        break;
                        //    case EncounterContainerType.NotLogged:
                        //        break;
                        //    default:
                        //        //Console.WriteLine($"{entry.SecondsElapsed}: {line}");
                        //        await AppendLine(sessionPath, encounterNumber, encounterContainers[entry.ContainerType], line);
                        //        await AppendLine_SingleFileForEncounter(sessionPath, encounterNumber, line);
                        //        switch (entry.AttackerType)
                        //        {
                        //            case CharacterType.Player:
                        //                players.Add(entry.AttackerName);
                        //                break;
                        //            case CharacterType.Npc:
                        //                npcs.Add(entry.AttackerName);
                        //                break;
                        //            case CharacterType.Pet:
                        //                pets.Add(entry.AttackerName);
                        //                break;
                        //        }
                        //        switch (entry.TargetType)
                        //        {
                        //            case CharacterType.Player:
                        //                players.Add(entry.TargetName);
                        //                break;
                        //            case CharacterType.Npc:
                        //                npcs.Add(entry.TargetName);
                        //                break;
                        //            case CharacterType.Pet:
                        //                pets.Add(entry.TargetName);
                        //                break;
                        //        }

                        //        abilities.Add(entry.AbilityName);
                        //        break;
                        //}

                        // Testing / encounter stats

                        //// Switch the container type again to add to the correct counter
                        //switch (entry.ContainerType)
                        //{
                        //    case EncounterContainerType.Buff:
                        //        buffEvents += 1;
                        //        globalBuffEvents += 1;
                        //        break;
                        //    case EncounterContainerType.Damage:
                        //        damageEvents += 1;
                        //        totalDamage += entry.TotalDamage;
                        //        globalDamageEvents += 1;
                        //        globalTotalDamage += entry.TotalDamage;
                        //        if (entry.TargetType == CharacterType.Npc && entry.TargetTakingDamage)
                        //        {
                        //            npcDamageTaken.AddDamageTaken(entry.TargetName, entry.ActionValue);
                        //            globalNpcDamageTaken.AddDamageTaken(entry.TargetName, entry.ActionValue);
                        //        }
                        //        break;
                        //    case EncounterContainerType.Heal:
                        //        healingEvents += 1;
                        //        totalHealing += entry.ActionValue;
                        //        globalHealingEvents += 1;
                        //        globalTotalHealing += entry.ActionValue;
                        //        break;
                        //    case EncounterContainerType.Shield:
                        //        shieldingEvents += 1;
                        //        totalShielding += entry.ActionValue;
                        //        globalShieldingEvents += 1;
                        //        globalTotalShielding += entry.ActionValue;
                        //        break;
                        //    case EncounterContainerType.Death:
                        //        switch (entry.TargetType)
                        //        {
                        //            case CharacterType.Player:
                        //                playerDeaths.AddDeath(entry.TargetName);
                        //                globalPlayerDeaths.AddDeath(entry.TargetName);
                        //                break;
                        //            case CharacterType.Npc:
                        //                npcDeaths.AddDeath(entry.TargetName);
                        //                globalNpcDeaths.AddDeath(entry.TargetName);
                        //                break;
                        //        }
                        //        break;
                        //}
                    }
                }
                
                lineNumber++;
            }
        }

        private static void AddDamageTaken(this Dictionary<string, long> dict, string name, long value)
        {
            if (dict.ContainsKey(name))
            {
                dict[name] = dict[name] + value;
            }
            else
            {
                dict.Add(name, value);
            }
        }

        private static void AddDeath(this Dictionary<string, int> dict, string name)
        {
            if (dict.ContainsKey(name))
            {
                dict[name] = dict[name] + 1;
            }
            else
            {
                dict.Add(name, 1);
            }
        }

        public static async Task<LogEntry> ParseLine(string logLine, LogType logType)
        {
            var entry = new LogEntry();

            #region Validity
            // Check whether this line is valid first
            if (logLine.IndexOf('(') == -1 || logLine.IndexOf(')') == -1 || logLine.Length <= 22)
            {
                // Line length is invalid, or no usable data was found
                entry.InvalidReason = "Line length is invalid, or no usable data was found";
                return entry;
            }

            // Bracket count
            if (CountOccurrences(logLine, '(') != CountOccurrences(logLine, ')'))
            {
                entry.InvalidReason = "Bracket counts are not equal";
                return entry;
            }
            
            // Date parsing
            // Standard logs have a 9 character date (actually, no date but let's ignore that fact)
            // Expanded logs are in the format mm/dd/yyyy hh:mm:ss:mms: (assume mms means milliseconds)
            var dateLength = logType == LogType.Standard ? 9 : 24;

            var entryDate = new DateTime();
            if (logType == LogType.Expanded)
            {
                // When /combatlogexpanded was added, the millisecond value wasn't forced to 3 digits.
                // It is now, but we have to handle a 2 digit ms value if we want to be able to use old logs.
                
                // 12/02/2016 08:19:51:798:
                // 12/02/2016 08:19:52:02:
                // 12/02/2016 08:21:35:00:
                var month = int.Parse(logLine.Substring(0, 2));
                var day = int.Parse(logLine.Substring(3, 2));
                var year = int.Parse(logLine.Substring(6, 4));
                var hour = int.Parse(logLine.Substring(11, 2));
                var minute = int.Parse(logLine.Substring(14, 2));
                var second = int.Parse(logLine.Substring(17, 2));
                // Figure out the millisecond value
                var ms = 0;
                var msValue = logLine.Substring(20, 3);
                if (msValue.Contains(":"))
                {
                    ms = int.Parse(msValue.Substring(0, 2));
                    dateLength = 23;
                }
                else
                {
                    ms = int.Parse(msValue);
                }

                entryDate = new DateTime(year, month, day, hour, minute, second, ms);
            }
            else if (!DateTime.TryParse(logLine.Substring(0, 8), out entryDate))
            {
                entry.InvalidReason = "Date couldn't be parsed";
                return entry;
            }

            entry.ParsedTimeStamp = entryDate;

            // Ensure the log line doesn't contain data from two events, like this:
            // 22:10:29: ( 7 , T=P#R=C#169025725536183234 , T=P#R=C#169025725536183234 , T=X#R=X#0 , T=X#R=X#0 , Geryonn , Geryonn , 0 , 1660365089 , Virulent Poison , 0 ) Geryon22:11:14: ( 27 , T=P#R=R#169025725533810537 , T=P#R=R#169025725533810537 , T=X#R=X#0 , T=X#R=X#0 , Killings , Killings , 25 , 939734518 , Archaic Tablet , 0 ) Killings's Archaic Tablet gives Killings 25 Mana.
            // If it does, skip them both. Unfortunate, but shit happens.

            // Hash count - should be 8.
            if (CountOccurrences(logLine, '#') != 8)
            {
                entry.InvalidReason = "Hash count is not 8";
                return entry;
            }

            #region Determine what part of this line is the 'log data'
            // Count how many open and close brackets we have on this line
            var openBrackets = new List<int>();
            var closeBrackets = new List<int>();

            // We've set "dateLength" based on the log type
            var lineNoTimestamp = logLine.Substring(dateLength).Trim();

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

            var logData = lineNoTimestamp.Substring(logDataStartIndex + 1, logDataEndIndex - logDataStartIndex - 1).Trim().ReplaceInvalidAbilityNames();
            var logDetail = logData.Split(',');

            // There should be 11 elements in the log detail array
            if (logDetail.Length != 11)
            {
                entry.InvalidReason = "Log detail length is invalid ( != 11 )";
                return entry;
            }
            #endregion

            entry.ValidEntry = true;
            #endregion
            #region Data

            #region Process the 'data' part of the line first
            int actionTypeId = int.Parse(logDetail[0].Trim());
            entry.ActionType = _isInt.IsMatch(((ActionType)actionTypeId).ToString()) ? ActionType.Unknown : ((ActionType)actionTypeId);

            #region Attacking Character
            entry.AttackerType = GetCharacterType(logDetail[1].Trim(), out var attId);
            entry.AttackerId = attId;
            #endregion
            #region Target Character
            entry.TargetType = GetCharacterType(logDetail[2].Trim(), out var tarId);
            entry.TargetId = tarId;
            #endregion

            GetCharacterType(logDetail[3].Trim(), out var attPetOwnerId);
            GetCharacterType(logDetail[4].Trim(), out var tarPetOwnerId);
            if (attPetOwnerId != "0")
            {
                entry.AttackerPetOwnerId = attPetOwnerId;
            }
            if (tarPetOwnerId != "0")
            {
                entry.TargetPetOwnerId = tarPetOwnerId;
            }
            entry.AttackerName = logDetail[5].Trim();
            entry.TargetName = logDetail[6].Trim();
            entry.ActionValue = long.Parse(logDetail[7].Trim());
            entry.AbilityId = long.Parse(logDetail[8].Trim());
            entry.AbilityName = logDetail[9].Trim();
            entry.SpecialValue = long.Parse(logDetail[10].Trim());

            #endregion
            #region Process the 'message' part of the line, that incidentally has the damage type as well as additional info
            entry.Message = lineNoTimestamp.Remove(logDataStartIndex, logDataEndIndex - logDataStartIndex + 1).Trim();
            // Check if the message contains special event info, like absorbs, intercepts, etc
            entry.ProcessSpecial();
            // Get damage type if we can (damaging abilities only)
            if (!string.IsNullOrEmpty(entry.TargetName) &&
                (entry.ActionType == ActionType.DamageCrit ||
                 entry.ActionType == ActionType.NormalDamageNonCrit ||
                 entry.ActionType == ActionType.DotDamageNonCrit ||
                 entry.ActionType == ActionType.Block))
            {
                entry.ProcessAbilityDamageType();
            }
            #endregion
            // Determine if we want to ignore this line
            if (entry.IsDeathType && entry.OverKillAmount == entry.TotalDamage)
            {
                entry.IgnoreThisEvent = true;
            }
            #endregion

            return entry;
        }

        static void ProcessAbilityDamageType(this LogEntry entry)
        {
            var msg = entry.Message.ToUpper();
            // Physical Damage
            if (msg.Contains("PHYSICAL DAMAGE") || msg.Contains("PHYSISCH-SCHADEN") || msg.Contains("dégâts de Physiques"))
            {
                entry.AbilityDamageType = "Physical";
                return;
            }
            // Air damage
            if (msg.Contains("AIR DAMAGE") || msg.Contains("LUFT-SCHADEN") || msg.Contains("dégâts de Air"))
            {
                entry.AbilityDamageType = "Air";
                return;
            }
            // Water damage
            if (msg.Contains("WATER DAMAGE") || msg.Contains("WASSER-SCHADEN") || msg.Contains("dégâts de Eau"))
            {
                entry.AbilityDamageType = "Water";
                return;
            }
            // Earth damage
            if (msg.Contains("EARTH DAMAGE") || msg.Contains("ERDE-SCHADEN") || msg.Contains("dégâts de Terre"))
            {
                entry.AbilityDamageType = "Earth";
                return;
            }
            // Fire damage
            if (msg.Contains("FIRE DAMAGE") || msg.Contains("FEUER-SCHADEN") || msg.Contains("dégâts de Feu"))
            {
                entry.AbilityDamageType = "Fire";
                return;
            }
            // Life damage
            if (msg.Contains("LIFE DAMAGE") || msg.Contains("LEBEN-SCHADEN") || msg.Contains("dégâts de Vie"))
            {
                entry.AbilityDamageType = "Life";
                return;
            }
            // Death damage
            if (msg.Contains("DEATH DAMAGE") || msg.Contains("TOD-SCHADEN") || msg.Contains("dégâts de Mort"))
            {
                entry.AbilityDamageType = "Death";
                return;
            }
            // Ethereal damage
            if (msg.Contains("ETHEREAL DAMAGE") || msg.Contains("ÄTHERISCH-SCHADEN") || msg.Contains("dégâts de éthéré"))
            {
                entry.AbilityDamageType = "Ethereal";
                return;
            }
        }

        static void ProcessSpecial(this LogEntry entry)
        {
            int messageOpenBracket = entry.Message.IndexOf('(');
            int messageCloseBracket = entry.Message.IndexOf(')');
            if (messageOpenBracket >= 0 && messageCloseBracket >= 0)
            {
                string specialData = entry.Message.Substring(messageOpenBracket + 1,
                    messageCloseBracket - messageOpenBracket - 1)
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
                        return;
                    }
                    for (int i = 1; i <= (strArray.Length / 2); i++)
                    {
                        // In French combat logs, we might see 'Attaque auto. (à distance)', ranged auto attack.
                        // Use TryParse for the special, so that it doesn't break if it finds these lines
                        if (!Int64.TryParse(strArray[(i - 1) * 2].Trim(), out var num2)) continue;
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
        }

        /// <summary>
        /// This is the method that's called to remove commas from any abilities that contain them.
        /// We shouldn't have to do this but someone decided that commas in names wouldn't cause issues with CSV-formatted data. Nice.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string ReplaceInvalidAbilityNames(this string input)
        {
            input = input
                .Replace("Saute, cours, vole !", "Juke and Run")
                .Replace("Blessing of Mobility, ", "Blessing of Mobility and ");

            return input;
        }

        static CharacterType GetCharacterType(string characterIdEntry, out string characterId)
        {
            characterId = "0";
            CharacterType returnValue = CharacterType.Unknown;

            // We expect the whole field here, e.g. T=P#R=R#240379631105751000
            try
            {
                string[] data = characterIdEntry.Split('#');
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

        public static bool CreateEncounterContainers(string parentPath, int encounterNumber)
        {
            var encounterPath = Path.Combine(parentPath, encounterNumber.ToString());
            try
            {
                // Encounter folder
                if (!Directory.Exists(encounterPath))
                {
                    Directory.CreateDirectory(encounterPath);
                }

                foreach (var container in encounterContainers)
                {
                    var thisPath = Path.Combine(encounterPath, container.Value);
                    var fs = File.Create(thisPath);
                    fs.Close();
                }

                // Double-check that they all exist
                foreach (var container in encounterContainers)
                {
                    var thisPath = Path.Combine(encounterPath, container.Value);
                    if (!File.Exists(thisPath))
                    {
                        Console.WriteLine($"Unable to find {thisPath}");
                        return false;
                    }
                }

                // All is good
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Checks whether the specified folder exists. If it doesn't, it will be created.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns>True if the folder already existed or was successfully created, otherwise False.</returns>
        public static bool CheckFolderExists(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    return true;
                }

                // Create the directory for this session
                Directory.CreateDirectory(folderPath);

                // Check that it actually exists now and didn't silently fail
                return Directory.Exists(folderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private static int CountOccurrences(string input, char searchFor)
        {
            int count = 0;
            char[] inputChars = input.ToCharArray();
            int length = inputChars.Length;
            for (int n = 0; n < length; n++)
            {
                if (inputChars[n] == searchFor)
                    count++;
            }

            return count;
        }

        static bool ShouldStartCombat(this LogEntry entry)
        {
            switch (entry.AttackerType)
            {
                // Return true if a player is getting hit by an NPC, or an NPC is getting hit by a player.
                case CharacterType.Npc when entry.TargetType == CharacterType.Player && entry.IsDamageType:
                case CharacterType.Player when entry.TargetType == CharacterType.Npc && entry.IsDamageType:
                    return true;
                default:
                    return false;
            }
        }

        static void SetTimeElapsed(this LogEntry entry, DateTime encounterStart, bool isStartOfEncounter = false)
        {
            entry.SecondsElapsed = isStartOfEncounter ? 0 : entry.SecondsElapsed = (int)(entry.CalculatedTimeStamp - encounterStart).TotalSeconds;
        }

        static int GetDowntimeValueForEncounter(this LogEntry entry)
        {
            foreach (var downtimeOverride in DowntimeOverrides)
            {
                if (entry.TargetType == CharacterType.Npc && entry.TargetName == downtimeOverride.NpcName)
                {
                    return downtimeOverride.DowntimeSeconds;
                }
                if (entry.AttackerType == CharacterType.Npc && entry.AttackerName == downtimeOverride.NpcName)
                {
                    return downtimeOverride.DowntimeSeconds;
                }
            }

            return DefaultEncounterDowntime;
        }

        /// <summary>
        /// This is only used to append a single line at a time.
        /// Works, but is incredibly slow if you forget to disable Windows Defender or any other A/V.
        /// On the upside, uses next to no memory to function well.
        /// </summary>
        /// <param name="sessionPath"></param>
        /// <param name="encounterNumber"></param>
        /// <param name="filename"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        static async Task AppendLine(string sessionPath, int encounterNumber, string filename, string line)
        {
            try
            {
                var filePath = Path.Combine(sessionPath, encounterNumber.ToString(), filename);
                await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to {filename}: {ex.Message}");
            }
        }

        static async Task AppendLine_SingleFileForEncounter(string sessionPath, int encounterNumber, string line)
        {
            try
            {
                var filePath = Path.Combine(sessionPath, encounterNumber.ToString(), $"encounter{encounterNumber}.txt");
                await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to encounter{encounterNumber}.txt: {ex.Message}");
            }
        }

        static async Task AppendLine_SingleFileForEncounter(string sessionPath, int encounterNumber, List<string> lines)
        {
            try
            {
                var filePath = Path.Combine(sessionPath, encounterNumber.ToString(), $"encounter{encounterNumber}.txt");
                await File.AppendAllLinesAsync(filePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to encounter{encounterNumber}.txt: {ex.Message}");
            }
        }

        /// <summary>
        /// This was built for testing purposes but isn't currently used.
        /// As it stands right now, ~4797kb of damage records gives a ~5396kb damage JSON document.
        /// </summary>
        /// <param name="sessionPath"></param>
        /// <param name="encounterNumber"></param>
        /// <param name="filename"></param>
        /// <param name="entries"></param>
        /// <returns></returns>
        static async Task WriteJson(string sessionPath, int encounterNumber, string filename, List<LogEntry> entries)
        {
            try
            {
                var filePath = Path.Combine(sessionPath, encounterNumber.ToString(), filename);
                var json = JsonConvert.SerializeObject(entries);
                await File.WriteAllTextAsync(filePath, json + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to {filename}: {ex.Message}");
            }
        }

        static async Task WriteSessionInfo(string sessionPath, List<string> lines)
        {
            try
            {
                var filePath = Path.Combine(sessionPath, "session.txt");
                await using StreamWriter sw = new StreamWriter(filePath);
                foreach (var line in lines)
                {
                    await sw.WriteLineAsync(line);
                }

                await sw.FlushAsync();
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to session info: {ex.Message}");
            }
        }

        static async Task WriteSessionStats(string sessionPath, SessionStats stats)
        {
            try
            {
                await File.WriteAllTextAsync(
                    Path.Combine(sessionPath, "session.txt"),
                    JsonConvert.SerializeObject(stats));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to session info: {ex.Message}");
            }
        }

        static async Task WriteEncounterInfo(string sessionPath, int encounterNumber, List<string> lines)
        {
            try
            {
                var filePath = Path.Combine(sessionPath, $"{encounterNumber}.txt");
                await using StreamWriter sw = new StreamWriter(filePath);
                foreach (var line in lines)
                {
                    await sw.WriteLineAsync(line);
                }

                await sw.FlushAsync();
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to encounter {encounterNumber} info: {ex.Message}");
            }
        }

        static async Task WriteEncounterStats(string sessionPath, int encounterNumber, EncounterStats stats)
        {
            try
            {
                await File.WriteAllTextAsync(
                    Path.Combine(sessionPath, $"{encounterNumber}.txt"),
                    JsonConvert.SerializeObject(stats));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to encounter {encounterNumber} stats: {ex.Message}");
            }
        }

        static bool RemoveEncounterFolder(string sessionPath, int encounterNumber)
        {
            try
            {
                var encounterPath = Path.Combine(sessionPath, encounterNumber.ToString());
                if (Directory.Exists(encounterPath))
                {
                    Directory.Delete(encounterPath, true);
                }

                // Check again to ensure it's gone
                return !Directory.Exists(encounterPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing encounter {encounterNumber}: {ex.Message}");
                return false;
            }
        }
    }
}
