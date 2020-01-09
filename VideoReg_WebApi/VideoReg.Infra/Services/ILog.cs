using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Infra.Services
{
    public interface ILog
    {
        void Debug(string message, object obj = default);
        void Info(string message, object obj = default);
        void Warning(string message, object obj = default);
        void Error(string message, Exception e = default, object obj = default);
    }
}
