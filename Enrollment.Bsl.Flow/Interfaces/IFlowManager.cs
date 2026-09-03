using LogicBuilder.App.Bsl.Utils.Interfaces;
using LogicBuilder.RulesDirector;
using System;

namespace Enrollment.Bsl.Flow.Interfaces
{
    public interface IFlowManager
    {
        DirectorBase Director { get; }
        IFlowActivity FlowActivity { get; }
        IFlowDataCache FlowDataCache { get; }
        Progress Progress { get; }
        IRulesCache RulesCache { get; }
        IServiceProvider ServiceProvider { get; }

        void Start(string module);
        void SetCurrentBusinessBackupData();
        void FlowComplete();
        void Terminate();
    }
}
