using System;
using System.Diagnostics;
using System.Linq;
using Dapper;
using Database.Models.Misc;
using Database.MySQL;
using Database.Repositories.Interfaces;
using Hangfire;
using Logging;

namespace Database.Repositories
{
    public class RecurringTaskRepo : DapperRepositoryBase, IRecurringTaskRepo
    {
        private readonly ILogger _logger;

        public RecurringTaskRepo(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 0)]
        public void UpdateDailyStats()
        {
            try
            {
                using (var conn = OpenConnection())
                {
                    var records = conn.Query<TableRowCount>(StatisticsSql.RelevantRecords).ToList();

                    var dmgRecords = records.First(r => r.Name == "DamageDone").Rows;
                    var healRecords = records.First(r => r.Name == "HealingDone").Rows;
                    var shieldRecords = records.First(r => r.Name == "ShieldingDone").Rows;
                    conn.Execute(StatisticsSql.Insert,
                        new
                        {
                            date = DateTime.UtcNow,
                            damageRecords = dmgRecords,
                            healingRecords = healRecords,
                            shieldingRecords = shieldRecords
                        });
                    //Debug.WriteLine("{0} damage, {1} healing, {2} shielding {3}", dmgRecords, healRecords, shieldRecords, DateTime.UtcNow);
                    //_logger.Debug($"{dmgRecords} damage, {healRecords} healing, {shieldRecords} shielding {DateTime.UtcNow}");
                }
                _logger.Debug("Daily stats updated");
            }
            catch (Exception ex)
            {
                _logger.Debug($"An error occurred while updating daily stats: {ex.Message}");
            }
        }

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(10)]
        public void DeleteRemovedEncounter()
        {
            try
            {
                using (var conn = OpenConnection())
                {
                    var encountersToRemove = conn.Query<Models.Encounter>(RecurringTasksSql.GetAllCountersRequiringDeletion).ToList();
                    if (!encountersToRemove.Any())
                    {
                        _logger.Info("No encounters to remove (that are marked for deletion)");
                        return;
                    }
                    var encToRemove = encountersToRemove.First();
                    if (encountersToRemove.Count > 1)
                    {
                        var rnd = new Random();
                        rnd = new Random(rnd.Next(Int32.MaxValue));
                        var encIdxToRemove = rnd.Next(encountersToRemove.Count);
                        Debug.WriteLine($"Selected index {encIdxToRemove} to remove");
                        encToRemove = encountersToRemove[encIdxToRemove];
                    }

                    //var encToRemove = conn.Query<Models.Encounter>(RecurringTasksSql.GetNextEncounterToDelete).SingleOrDefault();
                    //if (encToRemove == null)
                    //{
                    //    _logger.Info("No encounters to remove (that are marked for deletion)");
                    //    return;
                    //}
                    //var encTotalCount = conn.Query<long>(RecurringTasksSql.CountEncounterRequiringDeletion).SingleOrDefault();

                    Debug.WriteLine($"Beginning removal of encounter {encToRemove.Id} ({encToRemove.Date}). {encountersToRemove.Count} to remove in total");
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    bool success = false;
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // Remove records from tables
                            //Debug.WriteLine("Deleting records from EncounterOverview");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromOverview, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from EncounterBuffEvent");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromBuffEvent, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from EncounterBuffUptime");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromBuffUptime, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from EncounterBuffAction");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromBuffAction, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from EncounterDebuffAction");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromDebuffAction, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from EncounterNpc");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromNpc, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from EncounterNpcCast");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromNpcCast, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from EncounterDeath");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromDeath, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from DamageDone");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromDamageDone, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from HealingDone");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromHealingDone, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from ShieldingDone");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromShieldingDone, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from EncounterPlayerRole");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromPlayerRole, new { id = encToRemove.Id }, trans, 600);
                            //Debug.WriteLine("Deleting records from EncounterPlayerStatistics");
                            conn.Execute(RecurringTasksSql.DeleteEncounterFromPlayerStatistics, new { id = encToRemove.Id }, trans, 600);
                            // Mark encounter as removed
                            //Debug.WriteLine("Setting removed flag");
                            conn.Execute(RecurringTasksSql.MarkEncounterAsRemoved, new { id = encToRemove.Id }, trans, 600);
                            trans.Commit();
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error: {ex.Message}");
                            Debug.WriteLine("Rolling back transaction...");
                            trans.Rollback();
                            success = false;
                        }
                    }
                    sw.Stop();

                    //Debug.WriteLine("{0} damage, {1} healing, {2} shielding {3}", dmgRecords, healRecords, shieldRecords, DateTime.UtcNow);
                    //_logger.Debug($"{dmgRecords} damage, {healRecords} healing, {shieldRecords} shielding {DateTime.UtcNow}");
                    if (success)
                    {
                        _logger.Debug($"Encounter {encToRemove.Id} ({encToRemove.Date}) removed in {sw.Elapsed}");
                        Debug.WriteLine($"Encounter {encToRemove.Id} ({encToRemove.Date}) removed in {sw.Elapsed}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"An error occurred while deleting a removed encounter: {ex.Message}");
            }
        }
    }
}

