using System;

namespace WebApi.Services
{
    public interface ILog
    {
        void Debug(string message, object obj = default);
        void Info(string message, object obj = default);
        void Warning(string message, object obj = default);
        void Error(string message, Exception e = default, object obj = default);
        void Fatal(string message, Exception e = default, object obj = default);
    }
}
