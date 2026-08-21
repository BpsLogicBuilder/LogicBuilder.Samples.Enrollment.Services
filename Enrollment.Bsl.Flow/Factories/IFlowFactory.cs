using Enrollment.Bsl.Flow.Interfaces;
using LogicBuilder.RulesDirector;

namespace Enrollment.Bsl.Flow.Factories
{
    public interface IFlowFactory
    {
        DirectorBase GetDirector(IFlowManager flowManager);
        IFlowActivity GetFlowActivity(IFlowManager flowManager);
    }
}
