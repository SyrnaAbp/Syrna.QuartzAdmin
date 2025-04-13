using System;

namespace Syrna.QuartzAdmin.Blazor.Services
{
    public interface IJobUIProvider
    {
        Type GetJobUIType(string jobTypeFullName);
    }
}