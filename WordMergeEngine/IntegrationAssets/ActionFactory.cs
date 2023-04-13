
namespace WordMergeEngine.IntegrationAssets
{
    public class ActionFactory
    {
        public IOperation GetInstance(string action)
        {
            var operation = AppDomain.CurrentDomain.GetAssemblies()
                                                   .SelectMany(a => a.GetTypes())
                                                   .Where(t => t.GetInterfaces().Contains(typeof(IOperation)))
                                                   .Select(t => Activator.CreateInstance(t) as IOperation)
                                                   .FirstOrDefault(t => t.GetContentType().ToLower() == action.ToLower());
            if (operation == null)
                throw new Exception($"Неверно задано действие: {action}, обратитесь к администратору");

            return operation;
        }
    }
}
