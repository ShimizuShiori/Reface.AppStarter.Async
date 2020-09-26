using Reface.AppStarter.Attributes;
using Reface.AppStarter.Proxy;
using System.Reflection;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async.ExecutingHandlers
{
    /// <summary>
    /// 无返回值的异步执行器
    /// </summary>
    [Component]
    public class VoidAsyncExecutor : IAsyncExecutor
    {
        public bool CanHandle(MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public void Handle(ExecutingInfo executingInfo)
        {
            Task task = new Task(() => executingInfo.InvokeOriginalMethod());
            task.Start();
            executingInfo.Return(null);
        }
    }
}
