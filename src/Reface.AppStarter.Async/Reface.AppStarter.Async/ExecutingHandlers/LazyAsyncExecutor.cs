using Reface.AppStarter.Attributes;
using Reface.AppStarter.Proxy;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async.ExecutingHandlers
{
    [Component]
    public class LazyAsyncExecutor : IAsyncExecutor
    {
        private static readonly Type TYPE_LAZY_ANY = typeof(Lazy<>);

        public bool CanHandle(MethodInfo method)
        {
            Type returnType = method.ReturnType;
            return returnType.IsGenericType
                && returnType.GetGenericTypeDefinition() == TYPE_LAZY_ANY;
        }

        public void Handle(ExecutingInfo executingInfo)
        {
            Type returnType = executingInfo.Method.ReturnType.GetGenericArguments()[0];
            object rawResult = executingInfo.InvokeOriginalMethod(false);
            Delegate getFromLazy = BuildGetValueFromLazyFunc(returnType);
            Type taskType = typeof(Task<>).MakeGenericType(returnType);
            object task = Activator.CreateInstance(taskType, new object[]{
                getFromLazy,
                rawResult
            });
            Delegate getResultFromTask = BuildGetResultFromTask(returnType);
            Type lazyType = typeof(LazyProxy<>).MakeGenericType(returnType);
            object result = Activator.CreateInstance(lazyType, new object[] {
                task,
                getResultFromTask
            });
            executingInfo.Return(result);
        }


        private Delegate BuildGetValueFromLazyFunc(Type resultType)
        {
            Type lazyType = typeof(Lazy<>).MakeGenericType(resultType);
            DynamicMethod dm = new DynamicMethod("BuildGetValueFromLazyFunc", resultType, new Type[]
            {
                typeof(object)
            });
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, lazyType);
            il.Emit(OpCodes.Callvirt, lazyType.GetProperty("Value").GetMethod);
            il.Emit(OpCodes.Ret);
            return dm.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(object), resultType));
        }

        private Delegate BuildGetResultFromTask(Type returnType)
        {
            DynamicMethod dm = new DynamicMethod("BuildGetResultFromTask", returnType, new Type[]
            {
                typeof(object)
            });
            Type taskType = typeof(Task<>).MakeGenericType(returnType);
            var il = dm.GetILGenerator();

            //var loc1 = il.DeclareLocal(typeof(int));
            var loc2 = il.DeclareLocal(taskType);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, taskType);
            il.Emit(OpCodes.Stloc, loc2);
            il.Emit(OpCodes.Ldloc, loc2);
            il.Emit(OpCodes.Callvirt, taskType.GetMethod("Start", new Type[] { }));
            //il.Emit(OpCodes.Ldc_I4_2);
            //il.Emit(OpCodes.Stloc, loc1);
            //il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldloc, loc2);
            il.Emit(OpCodes.Callvirt, taskType.GetProperty("Result").GetMethod);
            il.Emit(OpCodes.Ret);
            Type funcType = typeof(Func<,>)
                .MakeGenericType(typeof(object), returnType);
            return dm.CreateDelegate(funcType);
        }
    }
}
