using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Syrna.QuartzAdmin.Blazor.Components;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using Syrna.QuartzAdmin.Scheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Syrna.QuartzAdmin.Blazor.Services
{
    public class JobUIProvider : IJobUIProvider
    {
        private readonly ISchedulerDefinitionService _schDefSvc;
        private readonly QuartzAdminUIOptions _options;
        private readonly ILogger<JobUIProvider> _logger;

        private Dictionary<string, Type> _availableJobUITypes;

        public JobUIProvider(ILogger<JobUIProvider> logger,
            ISchedulerDefinitionService schDefSvc,
            IOptions<QuartzAdminUIOptions> options)
        {
            _logger = logger;
            _schDefSvc = schDefSvc;
            _options = options.Value;
        }

        public Type GetJobUIType(string jobTypeFullName)
        {
            if (_availableJobUITypes == null)
            {
                LoadAvailableJobUITypes();
            }

            if (jobTypeFullName != null && _availableJobUITypes.ContainsKey(jobTypeFullName))
            {
                return _availableJobUITypes[jobTypeFullName];
            }
            else
            {
                return typeof(DefaultJobUI);
            }
        }

        [MemberNotNull(nameof(_availableJobUITypes))]
        private void LoadAvailableJobUITypes()
        {
            if (_options.AllowedJobAssemblyFiles == null)
            {
                _availableJobUITypes = new();
                return;
            }

            var jobUIMapping = new Dictionary<string, Type>();
            List<Type> jobUITypes = new();

            _logger.LogDebug("Detecting IJobUI implementations...");

            // load BlazorQuartz's IJobUI implementations
            jobUITypes.AddRange(Assembly.GetAssembly(typeof(JobUIProvider))!.GetExportedTypes()
                    .Where(x =>
                            x.IsPublic &&
                            x.IsClass &&
                            !x.IsAbstract &&
                            typeof(IJobUI).IsAssignableFrom(x)));

            int systemJobUICount = jobUITypes.Count;
            _logger.LogInformation("Detected {count} IJobUI implementations under QuartzAdmin assembly", systemJobUICount);

            _logger.LogDebug("Detecting IJobUI implementations specified in AllowedJobAssemblyFiles...");
            var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(JobUIProvider))!.Location) ?? string.Empty;

            foreach (var assemblyStr in _options.AllowedJobAssemblyFiles)
            {
                string assemblyPath = Path.Combine(path, assemblyStr + ".dll");
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    if (assembly == null)
                    {
                        _logger.LogWarning("Cannot load allowed job assembly name '{assembly}'", assemblyStr);
                        continue;
                    }

                    jobUITypes.AddRange(assembly.GetExportedTypes()
                        .Where(x =>
                            x.IsPublic &&
                            x.IsClass &&
                            !x.IsAbstract &&
                            typeof(IJobUI).IsAssignableFrom(x)));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load allowed job assembly filename '{assembly}'", assemblyStr);
                    continue;
                }
            }

            _logger.LogInformation("Detected {count} IJobUI implementations specified in AllowedJobAssemblyFiles. Total: {total}",
                jobUITypes.Count - systemJobUICount, jobUITypes.Count);

            if (!jobUITypes.Any())
            {
                _availableJobUITypes = new();
                return;
            }

            var jobTypes = _schDefSvc.GetJobTypeNames(false).Result;//.Select(j => j.FullName).ToHashSet();
            foreach (var jobUIType in jobUITypes)
            {
                var jobClass = GetJobClass(jobUIType);
                if (jobClass != null && jobTypes.Contains(jobClass))
                {
                    jobUIMapping.Add(jobClass, jobUIType);
                }
            }

            _availableJobUITypes = jobUIMapping;
        }

        private string GetJobClass(Type jobUIType)
        {
            var jobUI = (IJobUI)Activator.CreateInstance(jobUIType);
            if (jobUI != null)
                return jobUI.JobClass;

            _logger.LogWarning("Failed to instantiate job ui type {type}", jobUIType.FullName);
            return null;
        }
    }
}

