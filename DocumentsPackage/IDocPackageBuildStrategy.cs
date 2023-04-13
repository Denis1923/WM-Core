
namespace DocumentsPackage
{
    public interface IDocPackageBuildStrategy
    {
        List<string> GetTemplateList(CallingContext context);
    }
}
