using System;
using Common;
using LogParserConcept.Json;
using Newtonsoft.Json;

namespace LogParserConcept.Models
{
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LogEntry
    {
        public LogEntry()
        {
            TargetType = CharacterType.Unknown;
            AttackerType = CharacterType.Unknown;
            IgnoreThisEvent = false;
            ValidEntry = false;
        }

        
        public bool ValidEntry { get; set; }
        public string InvalidReason { get; set; }
        [JsonProperty("action")]
        public ActionType ActionType { get; set; }
        [JsonProperty("value")]
        public long ActionValue { get; set; }
        
        [JsonProperty("attId")]
        public string AttackerId { get; set; }
        [JsonProperty("attName")]
        public string AttackerName { get; set; }
        [JsonProperty("attPetOwnerId")]
        public string AttackerPetOwnerId { get; set; }
        [JsonProperty("attType")]
        public CharacterType AttackerType { get; set; }
        
        [JsonProperty("absorbed")]
        public long? AbsorbedAmount { get; set; }
        [JsonProperty("amtBlocked")]
        public long? BlockedAmount { get; set; }
        [JsonProperty("amtDeflected")]
        public long? DeflectAmount { get; set; }
        [JsonProperty("amtIgnored")]
        public long? IgnoredAmount { get; set; }
        [JsonProperty("amtIntercepted")]
        public long? InterceptAmount { get; set; }
        [JsonProperty("amtOverheal")]
        public long? OverhealAmount { get; set; }
        [JsonProperty("amtOverkill")]
        public long? OverKillAmount { get; set; }

        public string Message { get; set; }
        [JsonProperty("dodged")]
        public bool Dodged { get; set; }

        public long SpecialValue { get; set; }
        [JsonProperty("abilityId")]
        public long AbilityId { get; set; }
        [JsonProperty("abilityName")]
        public string AbilityName { get; set; }
        [JsonProperty("abilityType")]
        public string AbilityDamageType { get; set; }
        
        public bool IgnoreThisEvent { get; set; }
        
        [JsonProperty("tgtId")]
        public string TargetId { get; set; }
        [JsonProperty("tgtName")]
        public string TargetName { get; set; }
        [JsonProperty("tgtPetOwnerId")]
        public string TargetPetOwnerId { get; set; }
        [JsonProperty("tgtType")]
        public CharacterType TargetType { get; set; }

        public DateTime ParsedTimeStamp { get; set; }
        public DateTime CalculatedTimeStamp { get; set; }
        /// <summary>
        /// The number of seconds that have elapsed since combat started
        /// </summary>
        [JsonProperty("elapsed")]
        public int SecondsElapsed { get; set; }
        
        [JsonProperty("totalDamage")]
        public long TotalDamage
        {
            get
            {
                if (!IsDamageType) return 0;
                // DO NOT include intercepted values in the total
                //return ActionValue + AbsorbedAmount + BlockedAmount + DeflectAmount + IgnoredAmount + InterceptAmount;
                var amtBlocked = BlockedAmount ?? 0;
                var amtAbsorbed = AbsorbedAmount ?? 0;
                var amtDeflected = DeflectAmount ?? 0;
                var amtIgnored = IgnoredAmount ?? 0;
                return ActionValue + amtAbsorbed + amtBlocked + amtDeflected + amtIgnored;
            }
        }

        public bool HasSpecial =>
            AbsorbedAmount > 0 ||
            BlockedAmount > 0 ||
            DeflectAmount > 0 ||
            IgnoredAmount > 0 ||
            InterceptAmount > 0 ||
            OverKillAmount > 0;

        public bool TargetTakingDamage =>
            (ActionType == ActionType.DotDamageNonCrit ||
             ActionType == ActionType.NormalDamageNonCrit ||
             ActionType == ActionType.DamageCrit ||
             ActionType == ActionType.Block) &&
            ActionValue > 0;

        public bool IsDamageType
        {
            get
            {
                // Include dodge here!
                if (AttackerType == CharacterType.Npc &&
                    (TargetType == CharacterType.Pet || TargetType == CharacterType.Player) &&
                    ActionType == ActionType.Dodge)
                {
                    return true;
                }

                return ActionType == ActionType.DotDamageNonCrit ||
                        ActionType == ActionType.NormalDamageNonCrit ||
                        ActionType == ActionType.DamageCrit ||
                        ActionType == ActionType.Block ||
                        ActionType == ActionType.Miss ||
                        ActionType == ActionType.Resist ||
                        ActionType == ActionType.SelfDamage;
            }
        }

        public bool IsHealingType =>
            ActionType == ActionType.HealCrit ||
            ActionType == ActionType.HealNonCrit;

        public bool IsShieldType =>
            ActionType == ActionType.Absorb ||
            ActionType == ActionType.AbsorbCrit;

        public bool IsPlayerDeathToAnNpc =>
            AttackerType == CharacterType.Npc &&
            TargetType == CharacterType.Player &&
            OverKillAmount > 0;

        /// <summary>
        /// This assumes that the NPC ID (Attacker or target) has already been identified
        /// </summary>
        public bool IsActiveNpc =>
            (AttackerType == CharacterType.Npc || TargetType == CharacterType.Npc) &&
            (IsDamageType || IsHealingType || IsShieldType);

        /// <summary>
        /// This assumes that the Player ID (Attacker or target) has already been identified
        /// </summary>
        public bool IsActivePlayer
        {
            get
            {
                if (AttackerType == CharacterType.Player)
                {
                    return IsDamageType || IsHealingType || IsShieldType;
                }
                return false;
            }
        }

        public bool IsDeathType => OverKillAmount > 0;

        public EncounterContainerType ContainerType
        {
            get
            {
                if (IsDamageType) return EncounterContainerType.Damage;
                if (IsHealingType) return EncounterContainerType.Heal;
                if (IsShieldType) return EncounterContainerType.Shield;

                switch (ActionType)
                {
                    case ActionType.DebuffOrDotAfflicted:
                    case ActionType.DebuffOrDotDissipated:
                        return EncounterContainerType.Debuff;
                    case ActionType.BuffGain:
                    case ActionType.BuffFade:
                        return EncounterContainerType.Buff;
                    case ActionType.CastStart:
                    case ActionType.Interrupted:
                    case ActionType.ResourceGain:
                    case ActionType.Immune:
                    case ActionType.Dodge:
                        return EncounterContainerType.NotLogged;
                    case ActionType.TargetSlain:
                    case ActionType.Died:
                        return EncounterContainerType.Death;
                }

                return EncounterContainerType.Unknown;
            }
        }
    }
}
