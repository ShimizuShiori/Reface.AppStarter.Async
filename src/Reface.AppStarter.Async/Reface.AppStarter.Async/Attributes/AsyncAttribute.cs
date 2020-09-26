using Reface.AppStarter.Async.ExecutingHandlers;
using Reface.AppStarter.Proxy;
using System;
using System.Collections.Generic;

namespace Reface.AppStarter.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AsyncAttribute : ProxyAttribute
    {
        public IEnumerable<IAsyncExecutor> ExecutingHandlers { get; set; }

        public override void OnExecuted(ExecutedInfo executedInfo)
        {
        }

        public override void OnExecuteError(ExecuteErrorInfo executeErrorInfo)
        {
        }

        public override void OnExecuting(ExecutingInfo executingInfo)
        {
            foreach (var handler in this.ExecutingHandlers)
            {
                if (!handler.CanHandle(executingInfo.Method)) continue;
                handler.Handle(executingInfo);
                return;
            }
        }
    }
}
