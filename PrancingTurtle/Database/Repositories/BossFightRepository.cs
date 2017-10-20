using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Dapper;
using Database.MySQL;
using Database.Repositories.Interfaces;
using Logging;
using BossFight = Database.Models.BossFight;
using BossFightDifficulty = Database.Models.BossFightDifficulty;
using EncounterDifficulty = Database.Models.EncounterDifficulty;
using Instance = Database.Models.Instance;

namespace Database.Repositories
{
    public class BossFightRepository : DapperRepositoryBase, IBossFightRepository
    {
        private readonly ILogger _logger;

        public BossFightRepository(IConnectionFactory connectionFactory, ILogger logger) : base(connectionFactory)
        {
            _logger = logger;
        }

        private EncounterDifficulty GetDefaultDifficulty()
        {
            string timeElapsed;
            var defaultDifficulty = Query(q => q.Query<EncounterDifficulty>(MySQL.EncounterDifficulty.Default), out timeElapsed).SingleOrDefault();
            return defaultDifficulty;
        }

        public async Task<List<BossFight>> GetAllAsync()
        {
            var result = await QueryAsync(q => q.QueryAsync<BossFight, Instance, BossFight>(MySQL.BossFight.GetAll,
                (bf, ins) =>
                {
                    bf.Instance = ins;
                    return bf;
                }));
            return result.ToList();
        }

        public async Task<BossFight> GetAsync(int id)
        {
            var result = await QueryAsync(q => q.QueryAsync<BossFight, Instance, BossFight>(MySQL.BossFight.Get,
                (bf, ins) =>
                {
                    bf.Instance = ins;
                    return bf;
                }, new { id }));
            return result.SingleOrDefault();
        }

        public async Task<ReturnValue> Create(BossFight model)
        {
            ReturnValue rv = new ReturnValue();
            try
            {
                var result = await ExecuteAsync(q => q.ExecuteAsync(MySQL.BossFight.Create,
                    new
                    {
                        name = model.Name,
                        instanceId = model.InstanceId,
                        dpsCheck = model.DpsCheck,
                        requiresSpecialProcessing = model.RequiresSpecialProcessing,
                        priorityIfDuplicate = model.PriorityIfDuplicate
                    }));
                rv.Success = true;
                return rv;
            }
            catch (Exception e)
            {
                // return error message
                rv.Message = e.Message;
                return rv;
            }
        }

        public async Task<ReturnValue> Update(BossFight model)
        {
            ReturnValue rv = new ReturnValue();
            try
            {
                var result = await ExecuteAsync(q => q.ExecuteAsync(MySQL.BossFight.Update,
                    new
                    {
                        id = model.Id,
                        name = model.Name,
                        instanceId = model.InstanceId,
                        dpsCheck = model.DpsCheck,
                        requiresSpecialProcessing = model.RequiresSpecialProcessing,
                        priorityIfDuplicate = model.PriorityIfDuplicate
                    }));
                rv.Success = true;
                return rv;
            }
            catch (Exception e)
            {
                rv.Message = e.Message;
                return rv;
            }
        }

        public async Task<ReturnValue> Delete(int id)
        {
            ReturnValue rv = new ReturnValue();
            try
            {
                var result = await ExecuteAsync(q => q.ExecuteAsync(MySQL.BossFight.Delete,
                    new { id }));
                rv.Success = true;
                return rv;
            }
            catch (Exception e)
            {
                rv.Message = e.Message;
                return rv;
            }
        }

        public async Task<PagedData<BossFight>> GetPagedDataAsync(Dictionary<string, object> filters, string orderBy, int offset, int pageSize, bool useOr = false)
        {
            // Create the list to return
            var returnValue = new PagedData<BossFight>();
            // Initialise templates
            SqlBuilder.Template selectTemplate;
            SqlBuilder.Template countTemplate;

            // Set the queries that we need to use
            var select = MySQL.BossFight.PagedQuery.SelectAllFrom(AliasTs.BossFight);
            var count = MySQL.BossFight.PagedQuery.CountAllFrom(AliasTs.BossFight);
            // Create the builder itself
            SqlBuilderRepository.CreateBuilder(filters, orderBy, offset, pageSize, select, count, out selectTemplate, out countTemplate, useOr);

            var countTask = QueryAsync(q => q.QueryAsync<long>(countTemplate.RawSql, countTemplate.Parameters));
            var recordTask = QueryAsync(q => q.QueryAsync(selectTemplate.RawSql, BossFight.Map, selectTemplate.Parameters));

            // Wait for both tasks to finish
            await Task.WhenAll(countTask, recordTask);
            // Get the results from both tasks
            var total = countTask.Result.SingleOrDefault();
            var records = recordTask.Result;

            // Mysql counts with bigint (long) so convert it to a 32-bit integer here
            returnValue.TotalRecords = Convert.ToInt32(total);
            // Get the page of data to send back to the controller
            returnValue.Data = records;

            return returnValue;
        }

        public BossFight Get(int id)
        {
            string timeElapsed;
            return Query(q => q.Query<BossFight, Instance, BossFight>
                (MySQL.BossFight.Get, (bf, i) =>
                {
                    bf.Instance = i;
                    return bf;
                }, new { id }), out timeElapsed).SingleOrDefault();
        }

        public List<BossFight> GetAll(bool filterProgressionInstances)
        {
            string timeElapsed;
            return Query(q => q.Query<BossFight, Instance, BossFight>
                (filterProgressionInstances
            ? MySQL.BossFight.GetAllProgressionInstances
            : MySQL.BossFight.GetAll
            , (bf, i) =>
                {
                    bf.Instance = i;
                    return bf;
                }), out timeElapsed).ToList();
        }
        

        /// <summary>
        /// Now that we've moved all difficult records to the one table, this single query should return them all
        /// </summary>
        /// <returns></returns>
        public List<BossFightDifficulty> GetBossFightsAndDifficultySettings()
        {
            string timeElapsed;
            return
                Query(q => q.Query<BossFightDifficulty, BossFight, EncounterDifficulty, Instance, BossFightDifficulty>
                    (MySQL.BossFight.GetFullBossFightInfo,
                        (bfd, bf, ed, ins) =>
                        {
                            bf.Instance = ins;
                            bfd.BossFight = bf;
                            bfd.EncounterDifficulty = ed;
                            return bfd;
                        }), out timeElapsed).ToList();
        }

        public bool DifficultyRecordsExist(int bossFightId)
        {
            string timeElapsed;
            return Query(q => q.Query<long>(MySQL.BossFightDifficulty.DifficultyRecordsExist, new { bossFightId }), out timeElapsed).SingleOrDefault() > 0;
        }

        public List<BossFightDifficulty> GetDifficultySettings(int bossFightId)
        {
            string timeElapsed;
            return Query(q => q.Query<BossFightDifficulty, EncounterDifficulty, BossFightDifficulty>
                (MySQL.BossFightDifficulty.GetAll,
                    (bfd, ed) =>
                    {
                        bfd.EncounterDifficulty = ed;
                        return bfd;
                    }, new { bossFightId }), out timeElapsed).ToList();
        }

        
    }
}
