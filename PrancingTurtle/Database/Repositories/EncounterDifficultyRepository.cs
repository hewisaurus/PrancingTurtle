using System.Linq;
using Dapper;
using Database.Models;
using Database.Repositories.Interfaces;

namespace Database.Repositories
{
    public class EncounterDifficultyRepository : DapperRepositoryBase, IEncounterDifficultyRepository
    {
        public EncounterDifficultyRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {

        }

        public EncounterDifficulty Get(int id)
        {
            string timeElapsed;
            return
                Query(q => q.Query<EncounterDifficulty>(MySQL.EncounterDifficulty.GetById, new {id}), out timeElapsed)
                    .SingleOrDefault();
        }

        public EncounterDifficulty GetDefaultDifficulty()
        {
            string timeElapsed;
            return Query(q => q.Query<EncounterDifficulty>(MySQL.EncounterDifficulty.Default), out timeElapsed).SingleOrDefault();
        }

        public int GetDefaultDifficultyId()
        {
            string timeElapsed;
            var defaultDifficulty = Query(q => q.Query<EncounterDifficulty>(MySQL.EncounterDifficulty.Default), out timeElapsed).SingleOrDefault();
            if (defaultDifficulty == null) return 0;
            return defaultDifficulty.Id;
        }
    }
}
