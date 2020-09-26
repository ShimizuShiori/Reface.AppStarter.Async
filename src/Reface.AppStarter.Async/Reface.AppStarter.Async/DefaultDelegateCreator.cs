using Reface.AppStarter.Attributes;
using System;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async
{
    [Component]
    public class DefaultDelegateCreator : IDelegateCreator
    {
        private static readonly ConcurrentDictionary<string, Delegate>
            cachedDeletgate = new ConcurrentDictionary<string, Delegate>();

        public Delegate CreateGetResultFromTask(Type resultType)
        {
            DynamicMethod dm = new DynamicMethod("BuildGetResultFromTask", resultType, new Type[]
              {
                typeof(object)
              });
            Type taskType = typeof(Task<>).MakeGenericType(resultType);
            var il = dm.GetILGenerator();

            var loc2 = il.DeclareLocal(taskType);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, taskType);
            il.Emit(OpCodes.Stloc, loc2);
            il.Emit(OpCodes.Ldloc, loc2);
            il.Emit(OpCodes.Callvirt, taskType.GetMethod("Start", new Type[] { }));
            il.Emit(OpCodes.Ldloc, loc2);
            il.Emit(OpCodes.Callvirt, taskType.GetProperty("Result").GetMethod);
            il.Emit(OpCodes.Ret);
            Type funcType = typeof(Func<,>)
                .MakeGenericType(typeof(object), resultType);
            return dm.CreateDelegate(funcType);
        }

        public Delegate CreateGetValueFromLazy(Type valueType)
        {
            Type lazyType = typeof(Lazy<>).MakeGenericType(valueType);
            DynamicMethod dm = new DynamicMethod("BuildGetValueFromLazyFunc", valueType, new Type[]
            {
                typeof(object)
            });
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, lazyType);
            il.Emit(OpCodes.Callvirt, lazyType.GetProperty("Value").GetMethod);
            il.Emit(OpCodes.Ret);
            return dm.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(object), valueType));
        }
    }
}
