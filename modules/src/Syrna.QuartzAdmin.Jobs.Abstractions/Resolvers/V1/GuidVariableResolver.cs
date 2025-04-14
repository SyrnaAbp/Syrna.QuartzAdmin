using Syrna.QuartzAdmin.Jobs.Abstractions;
using Syrna.QuartzAdmin.Jobs.Abstractions.Resolvers;
using System;
namespace Syrna.QuartzAdmin.Jobs.Abstractions.Resolvers.V1
{
    internal class GuidVariableResolver : IResolver
    {
        const string Format = $"{{{{{VariableNameContants.Guid}}}}}";

        public GuidVariableResolver()
        {
        }

        public string Resolve(string varBlock)
        {
            if (varBlock != Format)
                throw new FormatException($"Invalid {VariableNameContants.Guid} format. Expected format is {Format}.");
            return Guid.NewGuid().ToString();
        }
    }
}

