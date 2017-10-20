namespace Database.MySQL
{
    public static class EncounterPlayerStatistics
    {
        public const string UpdateSingleTargetDps = "UPDATE EncounterPlayerStatistics SET SingleTargetDPS = @dps WHERE EncounterId = @encounterId AND PlayerId = @playerId";
        public const string UpdateBurstStatistics =
            "UPDATE EncounterPlayerStatistics SET " +
            "BurstDamage1sValue = @burstDamage1sValue, " +
            "BurstDamage1sSecond = @burstDamage1sSecond, " +
            "BurstDamage5sValue = @burstDamage5sValue, " +
            "BurstDamage5sPerSecond = @burstDamage5sPerSecond, " +
            "BurstDamage5sStart = @burstDamage5sStart, " +
            "BurstDamage5sEnd = @burstDamage5sEnd, " +
            "BurstDamage15sValue = @burstDamage15sValue, " +
            "BurstDamage15sPerSecond = @burstDamage15sPerSecond, " +
            "BurstDamage15sStart = @burstDamage15sStart, " +
            "BurstDamage15sEnd = @burstDamage15sEnd, " +
            "BurstHealing1sValue = @burstHealing1sValue, " +
            "BurstHealing1sSecond = @burstHealing1sSecond, " +
            "BurstHealing5sValue = @burstHealing5sValue, " +
            "BurstHealing5sPerSecond = @burstHealing5sPerSecond, " +
            "BurstHealing5sStart = @burstHealing5sStart, " +
            "BurstHealing5sEnd = @burstHealing5sEnd, " +
            "BurstHealing15sValue = @burstHealing15sValue, " +
            "BurstHealing15sPerSecond = @burstHealing15sPerSecond, " +
            "BurstHealing15sStart = @burstHealing15sStart, " +
            "BurstHealing15sEnd = @burstHealing15sEnd, " +
            "BurstShielding1sValue = @burstShielding1sValue, " +
            "BurstShielding1sSecond = @burstShielding1sSecond, " +
            "BurstShielding5sValue = @burstShielding5sValue, " +
            "BurstShielding5sPerSecond = @burstShielding5sPerSecond, " +
            "BurstShielding5sStart = @burstShielding5sStart, " +
            "BurstShielding5sEnd = @burstShielding5sEnd, " +
            "BurstShielding15sValue = @burstShielding15sValue, " +
            "BurstShielding15sPerSecond = @burstShielding15sPerSecond, " +
            "BurstShielding15sStart = @burstShielding15sStart, " +
            "BurstShielding15sEnd = @burstShielding15sEnd " +
            "WHERE EncounterId = @encounterId AND PlayerId = @playerId";

        /// <summary>
        /// This query is used to set single target dps to -1 if the player had no ST dps on a matching target
        /// </summary>
        public static string NullifySingleTargetDps
        {
            get { return "UPDATE EncounterPlayerStatistics SET SingleTargetDPS = -1 WHERE EncounterId = @encounterId AND SingleTargetDPS = 0"; }
        }
    }
}
