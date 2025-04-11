using System;
namespace Syrna.QuartzAdmin.Jobs.Abstractions.Resolvers
{
    public interface IResolver
    {
        string Resolve(string varBlock);
    }
}

