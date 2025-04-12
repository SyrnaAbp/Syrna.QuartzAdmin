using Microsoft.Extensions.DependencyInjection;

namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public interface IJobRegistrator
    {
        IServiceCollection Services { get; }
    }
}