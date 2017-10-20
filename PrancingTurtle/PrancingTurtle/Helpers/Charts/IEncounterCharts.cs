using System.Collections.Generic;
using System.Threading.Tasks;
using Database.QueryModels;
using DotNet.Highcharts;
using PrancingTurtle.Models.ViewModels.Encounter;
using Database.Models;
using DotNet.Highcharts.Options;

namespace PrancingTurtle.Helpers.Charts
{
    public interface IEncounterCharts
    {
        PlayerSomethingDone PlayerDamageDone(Encounter encounter);
        PlayerSomethingTaken PlayerDamageTaken(Encounter encounter);
        PlayerSomethingDone PlayerHealingDone(Encounter encounter);
        PlayerSomethingTaken PlayerHealingTaken(Encounter encounter);
        PlayerSomethingDone PlayerShieldingDone(Encounter encounter);
        PlayerSomethingTaken PlayerShieldingTaken(Encounter encounter);

        NpcSomethingDone NpcDamageDone(Encounter encounter);
        NpcSomethingTaken NpcDamageTaken(Encounter encounter);
        NpcSomethingDone NpcHealingDone(Encounter encounter);
        NpcSomethingTaken NpcHealingTaken(Encounter encounter);
        NpcSomethingDone NpcShieldingDone(Encounter encounter);
        NpcSomethingTaken NpcShieldingTaken(Encounter encounter);

        Highcharts PlayerNpcSomethingDoneOrTaken(int encounterId, string[] xAxisArray, string xAxisTitle, 
            object[] yAxis1Array, string yAxis2Title, object[] averageArray, object[] movingAverageArray = null, bool outgoing = true, bool isPlayer = true,
            string actionType = "DPS", List<CharacterBuffAction> buffTimers = null, List<object> playerDeathList = null,
            List<object> npcDeathList = null);

        Highcharts OverviewChart(string graphTitle, string graphSubtitle, List<CharacterInteractionPerSecond> records,
            int totalSeconds, bool isHealingGraph, bool groupByAbility, bool outgoing);

        Highcharts PieChart(string chartName, string graphTitle, string graphSubtitle, Series chartSeries);
        
    }
}