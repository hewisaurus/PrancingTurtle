using System;
using System.Linq;
using Dapper;
using Database.Models;
using Database.QueryModels.Misc;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class SearchRepository : DapperRepositoryBase, ISearchRepository
    {
        private readonly ILogger _logger;

        public SearchRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }


        public SearchResult Search(string searchTerm, string username, bool showAll = false)
        {
            var returnValue = new SearchResult();

            // Strip invalid characters from the search terms
            searchTerm = searchTerm.Trim().Replace("%", "").Replace("@", "");

            string dbSearchTerm = string.Format("%{0}%", searchTerm);

            try
            {
                using (var connection = OpenConnection())
                {
                    returnValue.Players = connection.Query<PlayerSearchResult>
                        (MySQL.Player.SearchForPlayer, new { @searchTerm = dbSearchTerm }).ToList();

                    if (showAll)
                    {
                        returnValue.Guilds = connection.Query<Guild, Shard, Guild>
                        (MySQL.Guild.SearchForAllGuilds,
                            (g, s) =>
                            {
                                g.Shard = s;
                                return g;
                            }, new { @searchTerm = dbSearchTerm }).ToList();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(username))
                        {
                            returnValue.Guilds = connection.Query<Guild, Shard, Guild>
                                (MySQL.Guild.SearchForGuildNoAuth,
                            (g, s) =>
                            {
                                g.Shard = s;
                                return g;
                            }, new { @searchTerm = dbSearchTerm }).ToList();
                        }
                        else
                        {
                            returnValue.Guilds = connection.Query<Guild, Shard, Guild>
                                (MySQL.Guild.SearchForGuildWithAuth,
                            (g, s) =>
                            {
                                g.Shard = s;
                                return g;
                            }, new { @searchTerm = dbSearchTerm, @email = username }).ToList();
                        }
                    }


                    returnValue.Encounters = connection.Query<BossFight, Instance, BossFight>
                        (MySQL.BossFight.SearchForEncounter,
                            (bf, i) =>
                            {
                                bf.Instance = i;
                                return bf;
                            }, new { @searchTerm = dbSearchTerm }).ToList();

                }
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Error while searching the database for {0}: {1}", searchTerm, ex.Message));
            }

            return returnValue;
        }
    }
}
