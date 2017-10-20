using System;
using Common;
using Database.Models;

namespace Database.Repositories.Interfaces
{
    public interface IScheduledTaskRepository
    {
        ScheduledTask Get(string name);

        ReturnValue UpdateTask(int id, DateTime runTime);
    }
}
