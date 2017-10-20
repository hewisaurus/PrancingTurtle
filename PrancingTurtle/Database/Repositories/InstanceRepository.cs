using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class InstanceRepository : DapperRepositoryBase, IInstanceRepository
    {
        private readonly ILogger _logger;

        public InstanceRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        public Instance Get(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<Instance>(MySQL.Instance.Get, new { id }), out timeElapsed).SingleOrDefault();
        }

        // async methods
        public async Task<List<Instance>> GetAllAsync()
        {
            var result = await QueryAsync(q => q.QueryAsync<Instance>(MySQL.Instance.GetAll));
            return result.ToList();
        }

        public async Task<Instance> GetAsync(int id)
        {
            var result = await QueryAsync(q => q.QueryAsync<Instance>(MySQL.Instance.Get, new { id }));
            return result.SingleOrDefault();
        }

        public async Task<ReturnValue> Create(Instance newInstance)
        {
            ReturnValue rv = new ReturnValue();
            try
            {
                var result = await ExecuteAsync(q => q.ExecuteAsync(MySQL.Instance.Create,
                    new
                    {
                        @name = newInstance.Name,
                        @maxRaidSize = newInstance.MaxRaidSize,
                        @visible = newInstance.Visible,
                        @includeInProgression = newInstance.IncludeInProgression,
                        @includeInLists = newInstance.IncludeInLists,
                        @shortName = newInstance.ShortName
                    }));
                rv.Success = true;
                return rv;
            }
            catch (Exception e)
            {
                // return error message
                rv.Message = e.InnerException.Message;
                return rv;
            }
        }

        public async Task<ReturnValue> Update(Instance updateInstance)
        {
            ReturnValue rv = new ReturnValue();
            try
            {
                var result = await ExecuteAsync(q => q.ExecuteAsync(MySQL.Instance.Update,
                    new
                    {
                        id = updateInstance.Id,
                        name = updateInstance.Name,
                        maxRaidSize = updateInstance.MaxRaidSize,
                        visible = updateInstance.Visible,
                        includeInProgression = updateInstance.IncludeInProgression,
                        includeInLists = updateInstance.IncludeInLists,
                        shortName = updateInstance.ShortName
                    }));
                rv.Success = true;
                return rv;
            }
            catch (Exception e)
            {
                rv.Message = e.InnerException.Message;
                return rv;
            }
        }

        public async Task<ReturnValue> Delete(int id)
        {
            ReturnValue rv = new ReturnValue();
            try
            {
                var result = await ExecuteAsync(q => q.ExecuteAsync(MySQL.Instance.Delete,
                    new { @id = id}));
                rv.Success = true;
                return rv;
            }
            catch (Exception e)
            {
                rv.Message = e.InnerException.Message;
                return rv;
            }
        }
    }
}
