using LogicBuilder.Attributes;

namespace Enrollment.Bsl.Flow.Interfaces
{
    public interface ICustomActions
    {
        [AlsoKnownAs("WriteToLog")]
        void WriteToLog(string message);
    }
}
