
using Quartz;

namespace Syrna.QuartzAdmin.ExecutionHistory
{
    public static class Extensions
    {
        public static void SetExecutionHistoryStore(this SchedulerContext context, IExecutionHistoryStore store)
        {
            context.Put(typeof(IExecutionHistoryStore).FullName, store);
        }

        public static IExecutionHistoryStore GetExecutionHistoryStore(this SchedulerContext context)
        {
            return (IExecutionHistoryStore)context.Get(typeof(IExecutionHistoryStore).FullName);
        }
    }
}
