using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LogParserConcept.Models
{
    public enum ActionType
    {
        Unknown = 0,
        CastStart = 1,
        Interrupted = 2,
        NormalDamageNonCrit = 3,
        DotDamageNonCrit = 4,
        HealNonCrit = 5,
        BuffGain = 6,
        BuffFade = 7,
        DebuffOrDotAfflicted = 8,
        DebuffOrDotDissipated = 9,
        Miss = 10,
        TargetSlain = 11,
        Died = 12,
        SelfDamage = 14,
        Dodge = 15,
        Parry = 16,
        Resist = 19,
        DamageCrit = 23,
        ResourceGain = 27,
        Immune = 26,
        HealCrit = 28,
        Block = 29,
        Absorb = 32,
        AbsorbCrit = 33
    }
}
