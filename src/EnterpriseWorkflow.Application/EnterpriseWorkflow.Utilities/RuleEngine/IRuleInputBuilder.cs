using EnterpriseWorkflow.Elsa.RuleEngineAdapter;
namespace AppFW.Utilities.RuleEngine
{
    public interface IRuleInputBuilder
    {
        RuleInput Build(EntityContext context);
    }
}
