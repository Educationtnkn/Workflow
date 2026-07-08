using AppFW.Service.Interfaces.DTO;

namespace AppFW.Utilities.RuleEngine
{
    public interface IRuleInputBuilder
    {
        RuleInput Build(EntityContext context);
    }
}
