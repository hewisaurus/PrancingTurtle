using System;
using Common;

namespace AutoParser
{
    public class CombatEntry
    {
        // Methods
        public CombatEntry()
        {
            this.TargetType = CharacterType.Unknown;
            this.AttackerType = CharacterType.Unknown;
            IgnoreThisEvent = false;
        }

        // Properties
        public long AbsorbedAmount { get; set; }
        public ActionType ActionType { get; set; }
        public long ActionValue { get; set; }

        public string AttackerId { get; set; }
        public string AttackerName { get; set; }
        public string AttackerPetOwnerId { get; set; }
        public CharacterType AttackerType { get; set; }

        public long BlockedAmount { get; set; }
        public long DeflectAmount { get; set; }
        public long IgnoredAmount { get; set; }
        public long InterceptAmount { get; set; }
        public string Message { get; set; }
        public long OverhealAmount { get; set; }
        public long OverKillAmount { get; set; }
        public bool Dodged { get; set; }
        public long SpecialValue { get; set; }
        public long AbilityId { get; set; }
        public string AbilityName { get; set; }
        public string AbilityDamageType { get; set; }
        public bool IgnoreThisEvent { get; set; }

        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public string TargetPetOwnerId { get; set; }
        public CharacterType TargetType { get; set; }

        public DateTime ParsedTimeStamp { get; set; }
        /// <summary>
        /// The number of seconds that have elapsed since combat started
        /// </summary>
        public int SecondsElapsed { get; set; }

        public long TotalDamage
        {
            get
            {
                if (!IsDamageType) return 0;
                // DO NOT include intercepted values in the total
                //return ActionValue + AbsorbedAmount + BlockedAmount + DeflectAmount + IgnoredAmount + InterceptAmount;
                return ActionValue + AbsorbedAmount + BlockedAmount + DeflectAmount + IgnoredAmount;
            }
        }

        public bool HasSpecial
        {
            get
            {
                return (AbsorbedAmount > 0) ||
                       (BlockedAmount > 0) ||
                       (DeflectAmount > 0) ||
                       (IgnoredAmount > 0) ||
                       (InterceptAmount > 0) ||
                       (OverKillAmount > 0);
            }
        }

        public bool TargetTakingDamage
        {
            get
            {
                return (ActionType == ActionType.DotDamageNonCrit ||
                        ActionType == ActionType.NormalDamageNonCrit ||
                        ActionType == ActionType.DamageCrit ||
                        ActionType == ActionType.Block) &&
                        ActionValue > 0;
            }
        }

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

        public bool IsHealingType
        {
            get
            {
                return ActionType == ActionType.HealCrit ||
                       ActionType == ActionType.HealNonCrit;
            }
        }

        public bool IsShieldType
        {
            get
            {
                return ActionType == ActionType.Absorb ||
                       ActionType == ActionType.AbsorbCrit;
            }
        }

        public bool IsPlayerDeathToAnNpc
        {
            get
            {
                return AttackerType == CharacterType.Npc &&
                       TargetType == CharacterType.Player &&
                       OverKillAmount > 0;
            }
        }

        /// <summary>
        /// This assumes that the NPC ID (Attacker or target) has already been identified
        /// </summary>
        public bool IsActiveNpc
        {
            get
            {
                return (AttackerType == CharacterType.Npc || TargetType == CharacterType.Npc) &&
                       (IsDamageType || IsHealingType || IsShieldType);
            }
        }
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

        public bool IsDeathType
        {
            get { return OverKillAmount > 0; }
        }
    }
}
