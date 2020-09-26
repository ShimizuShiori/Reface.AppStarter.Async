using Reface.AppStarter.Attributes;
using Reface.AppStarter.Proxy;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async.ExecutingHandlers
{
    [Component]
    public class TaskOfTypeAsyncExecutor : IAsyncExecutor
    {
        private readonly static Type TYPE_TASK_OF_ANY = typeof(Task<>);

        public bool CanHandle(MethodInfo method)
        {
            Type returnType = method.ReturnType;
            if (!returnType.IsGenericType) return false;
            return returnType.GetGenericTypeDefinition() == TYPE_TASK_OF_ANY;
        }

        public void Handle(ExecutingInfo executingInfo)
        {
            Task task = (Task)executingInfo.InvokeOriginalMethod();
            task.Start();
        }
    }
}
