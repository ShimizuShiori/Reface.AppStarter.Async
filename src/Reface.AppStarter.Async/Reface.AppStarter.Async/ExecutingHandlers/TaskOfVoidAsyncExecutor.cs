using Reface.AppStarter.Attributes;
using Reface.AppStarter.Proxy;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async.ExecutingHandlers
{
    /// <summary>
    /// 返回值是 <see cref="Task"/> 时的异步执行器
    /// </summary>
    [Component]
    public class TaskOfVoidAsyncExecutor : IAsyncExecutor
    {
        public bool CanHandle(MethodInfo method)
        {
            return method.ReturnType == typeof(Task);
        }

        public void Handle(ExecutingInfo executingInfo)
        {
            Task task = (Task)executingInfo.InvokeOriginalMethod();
            task.Start();
        }
    }
}
