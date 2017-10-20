using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Dapper;
using Database.Models;
using Database.MySQL;
using Database.Repositories.Interfaces;
using AbilityRole = Database.Models.AbilityRole;
using RoleIcon = Database.Models.RoleIcon;

namespace Database.Repositories
{
    public class AbilityRoleRepository : DapperRepositoryBase, IAbilityRoleRepository
    {
        public AbilityRoleRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        //public async Task<List<AbilityRole>> GetAllAsync()
        //{
        //    return (await QueryAsync(q => q.QueryAsync<AbilityRole>("", new {}))).ToList();
        //}

        public PagedData<AbilityRole> GetPagedData(Dictionary<string, object> filters, string orderBy, int page, int pageSize)
        {
            // Create the list to return
            var returnValue = new PagedData<AbilityRole>();
            // Initialise templates
            SqlBuilder.Template selectTemplate;
            SqlBuilder.Template countTemplate;
            string timeElapsed;
            // Set the queries that we need to use
            var select = MySQL.AbilityRole.PagedQuery.SelectAllFrom(AliasTs.AbilityRole.Name, AliasTs.AbilityRole.Alias);
            var count = MySQL.AbilityRole.PagedQuery.CountAllFrom(AliasTs.AbilityRole.Name, AliasTs.AbilityRole.Alias);

            // Create the builder itself
            SqlBuilderRepository.CreateBuilder(filters, orderBy, page, pageSize, select, count, out selectTemplate, out countTemplate);

            // Count the number of records that were found
            var total = Query(s => s.Query<long>(countTemplate.RawSql, countTemplate.Parameters).Single(), out timeElapsed);
            // Mysql counts with bigint (long) so convert it to a 32-bit integer here
            returnValue.TotalRecords = Convert.ToInt32(total);
            // Get the page of data to send back to the controller
            returnValue.Data = Query(q => q.Query<AbilityRole, RoleIcon, PlayerClass, AbilityRole>
            (selectTemplate.RawSql, (ar, ri, pc) =>
            {
                ar.Role = ri;
                ar.Class = pc;
                return ar;
            }, selectTemplate.Parameters), out timeElapsed);

            // Uncomment this if we really want to see how long it's taking, but by default, don't.
            //_logger.Debug(string.Format("GetSecuredPagedData: {0} records returned in {1}", returnValue.Data.Count(), timeElapsed));

            // Done!
            return returnValue;
        }
    }
}
