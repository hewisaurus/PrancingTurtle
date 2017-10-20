using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.MySQL;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class AbilityRepository : DapperRepositoryBase, IAbilityRepository
    {
        private readonly ILogger _logger;

        public AbilityRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }
        

        public List<AbilityIdName> GetAbilitiesWithNoIcon()
        {
            string timeElapsed;
            var result = Query(q => q.Query<AbilityIdName>(SQL.Ability.AbilitiesMissingIcons), out timeElapsed).ToList();
            return result;
        }

        public ReturnValue RemoveOrphanedAbilities(List<int> abilityIds)
        {
            var returnValue = new ReturnValue();

            try
            {
                _logger.Debug(string.Format("Beginning orphaned ability removal process ({0})", abilityIds.Count));

                string timeElapsed;

                // Temporarily disable FK checks - we HAVE checked this before running this, haven't we?
                _logger.Debug("Disabling FK checks for required tables");
                Execute(e => e.Execute("ALTER TABLE Ability NOCHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE DamageDone NOCHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE HealingDone NOCHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE ShieldingDone NOCHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE EncounterBuffAction NOCHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE EncounterDebuffAction NOCHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE EncounterDeath NOCHECK CONSTRAINT all"), out timeElapsed);
                
                int result = 0;

                while (true)
                {
                    if (abilityIds.Count > 1000)
                    {
                        List<int> removeAbilityIds = abilityIds.Take(1000).ToList();
                        result = Execute(e => e.Execute(SQL.Ability.DeleteOrphanedAbilities, new { @Ids = removeAbilityIds }), out timeElapsed);
                        _logger.Debug(string.Format("Removed {0} orphaned abilities in {1}", result, timeElapsed));
                        abilityIds.RemoveRange(0, 1000);
                    }
                    else
                    {
                        result = Execute(e => e.Execute(SQL.Ability.DeleteOrphanedAbilities, new { @Ids = abilityIds }), out timeElapsed);
                        _logger.Debug(string.Format("Removed {0} orphaned abilities in {1}", result, timeElapsed));
                        break;
                    }
                }

                // Enable FK Checking
                _logger.Debug("Enabling FK checks for required tables");
                Execute(e => e.Execute("ALTER TABLE Ability WITH CHECK CHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE DamageDone WITH CHECK CHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE HealingDone WITH CHECK CHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE ShieldingDone WITH CHECK CHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE EncounterBuffAction WITH CHECK CHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE EncounterDebuffAction WITH CHECK CHECK CONSTRAINT all"), out timeElapsed);
                Execute(e => e.Execute("ALTER TABLE EncounterDeath WITH CHECK CHECK CONSTRAINT all"), out timeElapsed);

                

                _logger.Debug("Finished removing orphaned abilities");
                returnValue.Success = true;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("The following error occurred while trying to remove orphaned abilities: {0}", ex.Message));
            }

            return returnValue;
        }

        public ReturnValue UpdateAbilityIcons(List<AbilityNameIcon> abilities)
        {
            var returnValue = new ReturnValue();

            try
            {
                string timeElapsed;

                foreach (var ability in abilities)
                {
                    var ability1 = ability;
                    Execute(
                        e => e.Execute(SQL.Ability.UpdateAbilityIcon, new {@id = ability1.Id, @icon = ability1.Icon}),
                        out timeElapsed);
                }

                returnValue.Success = true;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("The following error occurred while trying to update ability icons: {0}", ex.Message));
            }

            return returnValue;
        }

        public PagedData<Ability> GetPagedData(Dictionary<string, object> filters, string orderBy, int page, int pageSize)
        {
            // Create the list to return
            var returnValue = new PagedData<Ability>();
            // Initialise templates
            SqlBuilder.Template selectTemplate;
            SqlBuilder.Template countTemplate;
            string timeElapsed;
            // Set the queries that we need to use
            var select = AbilitySql.PagedQuery.SelectAllFrom(AliasTs.Ability.Name, AliasTs.Ability.Alias);
            var count = AbilitySql.PagedQuery.CountAllFrom(AliasTs.Ability.Name, AliasTs.Ability.Alias);

            // Create the builder itself
            SqlBuilderRepository.CreateBuilder(filters, orderBy, page, pageSize, select, count, out selectTemplate, out countTemplate);

            // Count the number of records that were found
            var total = Query(s => s.Query<long>(countTemplate.RawSql, countTemplate.Parameters).Single(), out timeElapsed);
            // Mysql counts with bigint (long) so convert it to a 32-bit integer here
            returnValue.TotalRecords = Convert.ToInt32(total);
            // Get the page of data to send back to the controller
            returnValue.Data = Query(q => q.Query<Ability>(selectTemplate.RawSql, selectTemplate.Parameters), out timeElapsed);

            // Uncomment this if we really want to see how long it's taking, but by default, don't.
            //_logger.Debug(string.Format("GetSecuredPagedData: {0} records returned in {1}", returnValue.Data.Count(), timeElapsed));

            // Done!
            return returnValue;
        }
    }
}
