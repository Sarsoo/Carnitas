using Carnitas.Job;

namespace Carnitas.Grpc.Mapping;

public static class LogTypeExtensions
{
    extension(LogType logType)
    {
        public OperationLogType ToOperationLogType() => (OperationLogType)logType;
    }
    
    extension(OperationLogType logType)
    {
        public LogType ToLogType() => (LogType)logType;
    }
}