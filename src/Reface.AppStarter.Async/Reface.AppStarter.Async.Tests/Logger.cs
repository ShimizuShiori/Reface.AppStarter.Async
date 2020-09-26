using Reface.AppStarter.Attributes;
using System;
using System.Diagnostics;
using System.Threading;

namespace Reface.AppStarter.Async.Tests
{
    public interface ILogger
    {
        void Log(string message);
    }

    [Component]
    public class Logger : ILogger
    {
        public void Log(string message)
        {
            StackFrame st = new StackFrame(1);
            Console.WriteLine("[{0}] - [{1}] : {2}", Thread.CurrentThread.ManagedThreadId,
                st.GetMethod().Name,
                message);
        }
    }
}
