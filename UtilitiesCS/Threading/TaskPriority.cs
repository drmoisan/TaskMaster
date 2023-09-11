using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.Threading
{
    public static class TaskPriority
    {
        async public static Task Run(Action action, PriorityScheduler scheduler)
        {
            await Task.Factory.StartNew(action, default, TaskCreationOptions.None, scheduler);
        }

        async public static Task Run(PriorityScheduler scheduler, Action action)
        {
            await Task.Factory.StartNew(action, default, TaskCreationOptions.None, scheduler);
        }
    }

    public static class TaskPriority<T>
    {
        async public static Task<T> Run(Func<T> func, PriorityScheduler scheduler)
        {
            return await Task<T>.Factory.StartNew(func, default, TaskCreationOptions.None, scheduler);
        }

        async public static Task<T> Run(PriorityScheduler scheduler, Func<T> func)
        {
            return await Task<T>.Factory.StartNew(func, default, TaskCreationOptions.None, scheduler);
        }
    }
}
