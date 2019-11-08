using System;
using System.Diagnostics;
using PrancingTurtle.DependencyResolution;
using PrancingTurtle.Helpers.Scheduling.Jobs;
using Quartz;
using Quartz.Spi;
using StructureMap;

namespace PrancingTurtle.Helpers.Scheduling
{
    public class ScheduledTasks
    {
        public static void Start()
        {
            Debug.WriteLine("Scheduled task start has been called.");

            var container = new Container(new DefaultRegistry());
            IJobFactory jobFactory = new StructureMapJobFactory(container);
            IScheduler scheduler = container.GetInstance<IScheduler>();
            scheduler.JobFactory = jobFactory;
            scheduler.Start();

            // Jobs attached under here

            // Update player names
            var random = new Random();
            var updatePlayerNameInterval = random.Next(120, 180);

            IJobDetail jPlayerNameUpdate = JobBuilder.Create<UpdatePlayerNames>().WithIdentity("UpdatePlayerNames","SilentUpdate").Build();
            ITrigger tPlayerNameUpdate =
                TriggerBuilder.Create()
                    .WithIdentity("UpdatePlayerNames","SilentUpdate")
                    .StartNow()
                    .WithSimpleSchedule(s => s.WithIntervalInMinutes(updatePlayerNameInterval).RepeatForever())
                    .Build();
            scheduler.ScheduleJob(jPlayerNameUpdate, tPlayerNameUpdate);

            // Update player names
            var zeroLengthEncInterval = random.Next(80, 120);

            IJobDetail jZeroLengthEncounterUpdate = JobBuilder.Create<ZeroDurationEncounters>().WithIdentity("ZeroDurationEncounters", "SilentUpdate").Build();
            ITrigger tZeroLengthEncounterUpdate =
                TriggerBuilder.Create()
                    .WithIdentity("ZeroDurationEncounters", "SilentUpdate")
                    .StartNow()
                    .WithSimpleSchedule(s => s.WithIntervalInMinutes(zeroLengthEncInterval).RepeatForever())
                    .Build();
            scheduler.ScheduleJob(jZeroLengthEncounterUpdate, tZeroLengthEncounterUpdate);

            // Remove orphaned player records
            var removeOrphanedPlayerRecordsInterval = random.Next(180, 240);

            IJobDetail jOrphanedPlayerRecords = JobBuilder.Create<RemoveOrphanedPlayerRecords>()
                .WithIdentity("OrphanedPlayerRecords", "SilentUpdate").Build();
            ITrigger tOrphanedPlayerRecords =
                TriggerBuilder.Create()
                    .WithIdentity("OrphanedPlayerRecords", "SilentUpdate")
                    .StartNow()
                    .WithSimpleSchedule(s => s.WithIntervalInMinutes(removeOrphanedPlayerRecordsInterval).RepeatForever())
                    .Build();
            scheduler.ScheduleJob(jOrphanedPlayerRecords, tOrphanedPlayerRecords);

            // Update EncounterNpcs
            //var updateEncounterNpcsInterval = random.Next(50, 70);

            //IJobDetail jupdateEncounterNpcs = JobBuilder.Create<AddMissingEncounterNpcs>()
            //    .WithIdentity("EncounterNpcRecords", "SilentUpdate").Build();
            //ITrigger tupdateEncounterNpcs =
            //    TriggerBuilder.Create()
            //        .WithIdentity("EncounterNpcRecords", "SilentUpdate")
            //        .StartNow()
            //        .WithSimpleSchedule(s => s.WithIntervalInMinutes(updateEncounterNpcsInterval).RepeatForever())
            //        .Build();
            //scheduler.ScheduleJob(jupdateEncounterNpcs, tupdateEncounterNpcs);

            // Update EncounterPlayerRoles
            var updateEncounterPlayersInterval = random.Next(10,20);

            IJobDetail jUpdateEncounterPlayers = JobBuilder.Create<AddMissingEncounterPlayerRoles>()
                .WithIdentity("EncounterPlayerRoleRecords", "SilentUpdate").Build();
            ITrigger tUpdateEncounterPlayers =
                TriggerBuilder.Create()
                    .WithIdentity("EncounterPlayerRoleRecords", "SilentUpdate")
                    .StartNow()
                    .WithSimpleSchedule(s => s.WithIntervalInMinutes(updateEncounterPlayersInterval).RepeatForever())
                    .Build();
            scheduler.ScheduleJob(jUpdateEncounterPlayers, tUpdateEncounterPlayers);

            // Update EncounterPlayerStatistics
            var updateEncounterPlayerStatisticsInterval = random.Next(10, 20);

            IJobDetail jUpdateEncounterPlayerStatistics = JobBuilder.Create<EncounterPlayerStatistics>()
                .WithIdentity("EncounterPlayerStatistics", "SilentUpdate").Build();
            ITrigger tUpdateEncounterPlayerStatistics =
                TriggerBuilder.Create()
                    .WithIdentity("EncounterPlayerStatistics", "SilentUpdate")
                    .StartNow()
                    .WithSimpleSchedule(s => s.WithIntervalInMinutes(updateEncounterPlayerStatisticsInterval).RepeatForever())
                    .Build();
            scheduler.ScheduleJob(jUpdateEncounterPlayerStatistics, tUpdateEncounterPlayerStatistics);
            // DEBUG
            Debug.WriteLine("Encounter player stats schedule has been called.");

            // Remove old wipes
            var markOldWipesForDeletionInterval = random.Next(10, 20);

            IJobDetail jmarkOldWipesForDeletion = JobBuilder.Create<MarkOldWipesForDeletion>()
                .WithIdentity("MarkOldWipesForDeletion", "SilentUpdate").Build();
            ITrigger tmarkOldWipesForDeletion =
                TriggerBuilder.Create()
                    .WithIdentity("MarkOldWipesForDeletion", "SilentUpdate")
                    .StartNow()
                    .WithSimpleSchedule(s => s.WithIntervalInMinutes(markOldWipesForDeletionInterval).RepeatForever())
                    .Build();
            scheduler.ScheduleJob(jmarkOldWipesForDeletion, tmarkOldWipesForDeletion);
        }
    }
}