using System;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Services
{
    public interface IJobUIProvider
    {
        Task<Type> GetJobUIType(string jobTypeFullName);
    }
}