using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reface.AppStarter.Async.Tests.AppModules;
using Reface.AppStarter.Attributes;
using Reface.AppStarter.UnitTests;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async.Tests
{
    public class MyLazy<T> : Lazy<T>
    {
        static Func<T> realFactory(object context, Func<object, T> factory)
        {
            T value = factory(context);
            return () => value;
        }

        public MyLazy(object context, Func<object, T> factory)
            : base(realFactory(context, factory))
        {

        }
    }

    [TestClass]
    public class EmitTests : TestClassBase<TestAppModule>
    {
        [AutoCreate]
        public ILogger Logger { get; set; }

        private static Type TYPE_INT = typeof(int);

        [TestMethod]
        public void Func()
        {
            Func<int, int, int> func = (i, j) => i + j;
            Assert.AreEqual(2, func(1, 1));
        }

        public int abc(object lazyValue)
        {
            return ((Lazy<int>)lazyValue).Value;
        }

        public Delegate BuildGetValueFromLazyFunc(Type resultType)
        {
            Type lazyType = typeof(Lazy<>).MakeGenericType(resultType);
            DynamicMethod dm = new DynamicMethod("Name", resultType, new Type[]
            {
                typeof(object)
            });
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, lazyType);
            il.Emit(OpCodes.Callvirt, lazyType.GetProperty("Value").GetMethod);
            //il.Emit(OpCodes.Stloc_0);
            //il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            return dm.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(object), resultType));
        }

        private Delegate BuildGetResultFromTask(Type returnType)
        {
            DynamicMethod dm = new DynamicMethod("Name2", returnType, new Type[]
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

        private int GetResult(object obj)
        {
            Task<int> task = (Task<int>)obj;
            task.Start();
            return task.Result;
        }

        [TestMethod]
        public void BuildGetResultFromTaskTest()
        {
            Type intType = typeof(int);
            Delegate del = BuildGetResultFromTask(intType);
            Task<int> task = new Task<int>(() => 1);

            Func<object, int> fun = del as Func<object, int>;
            int i = fun(task);

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void TestGetDel()
        {
            Lazy<int> li = new Lazy<int>(() => 1);
            Type intType = typeof(int);
            Delegate del = BuildGetValueFromLazyFunc(intType);
            Type taskType = typeof(Task<>).MakeGenericType(intType);
            Type lazyInt = typeof(MyLazy<>).MakeGenericType(typeof(int));

            Task<int> task = (Task<int>)Activator.CreateInstance(taskType, del, li);
            task.Start();
            Assert.AreEqual(1, task.Result);

        }

        [TestMethod]
        public void haha()
        {
            var li = new Lazy<int>(() => 1);
            Type type = typeof(Task<int>);
            MethodInfo method = typeof(EmitTests).GetMethod(nameof(abc));
            Type funcType = typeof(Func<,>);
            Type funcIntType = funcType.MakeGenericType(typeof(object), typeof(int));
            var del = method.CreateDelegate(funcIntType, this);
            Task task = (Task)Activator.CreateInstance(type, del, li);
            //Task<int> task2 = new Task<int>(obj => (obj as Lazy<int>).Value, new Lazy<int>(() => 1));
            task.Start();

            object value = task.GetType().GetProperty("Result")
                .GetValue(task, null);

            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void All()
        {
            Lazy<int> li = new Lazy<int>(() =>
            {
                this.Logger.Log("Lazy is getting value");
                Thread.Sleep(10);
                this.Logger.Log("Lazy's value is ok");
                return 100;
            });
            Type returnType = typeof(int);
            Delegate getFromLazy = BuildGetValueFromLazyFunc(returnType);
            Type taskType = typeof(Task<>).MakeGenericType(returnType);
            object task = Activator.CreateInstance(taskType, new object[]{
                getFromLazy,
                li
            });

            Delegate getResultFromTask = BuildGetResultFromTask(returnType);
            Type lazyType = typeof(MyLazy<>).MakeGenericType(returnType);
            object result = Activator.CreateInstance(lazyType, new object[] {
                task,
                getResultFromTask
            });

            Thread.Sleep(10);
            this.Logger.Log("Lazy is ready");
            Lazy<int> lazy = (Lazy<int>)result;
            Thread.Sleep(10);
            this.Logger.Log("will get Lazy's value");
            Thread.Sleep(10);
            int value = lazy.Value;
            this.Logger.Log("Lazy's value getted");
            Assert.AreEqual(100, value);
        }

        public Task<int> CreateTask(object[] args)
        {
            var arg_0 = (Func<object, int>)args[0];
            var arg_1 = (object)args[1];
            return new Task<int>(arg_0, arg_1);
        }

        public Func<Func<object, int>, object, Task<int>> GetCreateTaskFunc()
        {
            DynamicMethod dm = new DynamicMethod("CreateTask", typeof(Task<int>), new Type[]
            {
                typeof(Func<object,int>),
                typeof(object)
            });
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg, 0);
            il.Emit(OpCodes.Ldarg, 1);
            il.Emit(OpCodes.Newobj, typeof(Task<int>).GetConstructor(new Type[] {
                typeof(Func<object,int>),
                typeof(object)
            }));
            il.Emit(OpCodes.Ret);
            return dm.CreateDelegate(typeof(Func<Func<object, int>, object, Task<int>>)) as Func<Func<object, int>, object, Task<int>>;
        }

        [TestMethod]
        public void TestGetCreateTaskFunc()
        {
            var func = GetCreateTaskFunc();
            Task<int> task = func(obj => 1, new object());
            task.Start();
            Assert.AreEqual(1, task.Result);
        }

        private Func<object[], object> CreateFunc(Type returnType, Type[] argsTypes)
        {
            DynamicMethod dm = new DynamicMethod("NewFunc", returnType, new Type[]
{
    typeof(object[])
});
            var il = dm.GetILGenerator();
            var locs = argsTypes.Select(t =>
            {
                return il.DeclareLocal(t);
            }).ToArray();
            for (int i = 0; i < argsTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Castclass, argsTypes[i]);
                il.Emit(OpCodes.Stloc, locs[i]);
            }
            for (int i = 0; i < locs.Length; i++)
            {
                il.Emit(OpCodes.Ldloc, locs[i]);
            }
            il.Emit(OpCodes.Newobj, returnType.GetConstructor(argsTypes));
            il.Emit(OpCodes.Ret);
            return dm.CreateDelegate(typeof(Func<object[], object>)) as Func<object[], object>;
        }

        //[TestMethod]
        //public void CreateFuncTest()
        //{
        //    Func<object[], object> func = CreateFunc(typeof(A), new Type[] 
        //    { 
        //        typeof(int),
        //        typeof(int),
        //        typeof(string)
        //    });
        //    A a = (A)func(new object[] { 1, 2, "str" });
        //}

        public class A
        {
            public A()
            {

            }
            public A(int i, int j, string s)
            {
                Console.WriteLine("{0},{1},{2}", i, j, s);
            }
        }
    }
}
