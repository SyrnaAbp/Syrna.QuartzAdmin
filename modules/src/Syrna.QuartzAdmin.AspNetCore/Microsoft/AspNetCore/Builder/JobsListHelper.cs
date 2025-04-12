using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public static class JobsListHelper
    {
        private static List<Type> _quartzJobs = null;
        public static List<Type> GetQuartzAdminJobs(List<Assembly> lists = null)
        {
            if (_quartzJobs == null)
            {
                try
                {
                    var types1 = from t in Assembly.GetEntryAssembly().GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(QuartzTriggerAttribute), true) select t;
                    var types = from t in Assembly.GetCallingAssembly().GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(QuartzTriggerAttribute), true) select t;
                    _quartzJobs = [.. types, .. types1];
                    if (_quartzJobs == null || _quartzJobs.Count == 0)
                    {
                        var types2 = AppDomain.CurrentDomain.GetAssemblies().ToList()
                            .SelectMany(a => a.GetTypes())
                            .Where(t => t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(QuartzTriggerAttribute), true));
                        _quartzJobs = types2.ToList();
                    }

                    lists?.ForEach(asm =>
                    {
                        var typeasm = from t in asm.GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(QuartzTriggerAttribute), true) select t;
                        _quartzJobs.AddRange(typeasm);
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception("Can't  find  type with  IJob and have  QuartzTriggerAttribute", ex);
                }
            }
            return _quartzJobs;
        }
    }
}
