namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public interface IDataMapValueResolver
    {
        string? Resolve(DataMapValue? dmv);
    }
}