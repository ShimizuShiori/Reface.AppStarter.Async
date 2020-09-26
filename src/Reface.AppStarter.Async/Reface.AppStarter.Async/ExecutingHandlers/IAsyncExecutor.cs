using Reface.AppStarter.Proxy;
using System.Reflection;

namespace Reface.AppStarter.Async.ExecutingHandlers
{
    /// <summary>
    /// 异常执行器
    /// </summary>
    public interface IAsyncExecutor
    {
        bool CanHandle(MethodInfo method);

        void Handle(ExecutingInfo executingInfo);
    }
}
