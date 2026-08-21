using Enrollment.Bsl.Flow.Interfaces;
using Microsoft.Extensions.Logging;

namespace Enrollment.Bsl.Flow
{
    public class CustomActions(ILogger<CustomActions> logger) : ICustomActions
    {
        private readonly ILogger<CustomActions> _logger = logger;

        public void WriteToLog(string message)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Write to log from workflow {Message}", message);
        }
    }
}
