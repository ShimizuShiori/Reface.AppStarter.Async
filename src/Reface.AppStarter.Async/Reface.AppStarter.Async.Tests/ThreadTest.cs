using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async.Tests
{

    [TestClass]
    public class ThreadTest
    {
        [TestMethod]
        public void T1()
        {
            // 某个任务的结果
            int resultOfSomeTask = 0;
            Thread someTask = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1000);
                resultOfSomeTask = 100;
            }));
            someTask.Start();

            DoSomeThing(2); // 做一些别的事情
            DoSomeThing(3); // 做一些别的事情
            DoSomeThing(1); // 做一些别的事情

            // 产生与 while(true) 同样的效果
            // 当 someTask 完成后，才会继续进行
            someTask.Join();

            Assert.AreEqual(100, resultOfSomeTask);
        }

        /// <summary>
        /// 这是一个描述了一个使用整形并返回整形的委托类型
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public delegate int TaskGetIntByInt(int i);

        [TestMethod]
        public void T2()
        {
            TaskGetIntByInt delayPlus = new TaskGetIntByInt(i =>
            {
                Thread.Sleep(1000);
                return i * i;
            });

            DoSomeThing(1);
            DoSomeThing(2);
            DoSomeThing(3);

            AsyncCallback callback = new AsyncCallback(ar =>
            {
                string state = ar.AsyncState as string;
                Assert.AreEqual("Hello", state);
            });

            IAsyncResult r = delayPlus.BeginInvoke(10, callback, "HelloWorld");
            int result = delayPlus.EndInvoke(r);

            Assert.AreEqual(100, result);
        }

        [TestMethod]
        public void T3()
        {
            TaskGetIntByInt someTask = new TaskGetIntByInt(i =>
            {
                Thread.Sleep(3000);
                return i * i;
            });

            DoSomeThing(1);
            DoSomeThing(2);
            DoSomeThing(3);

            AsyncCallback callback = new AsyncCallback(_ar =>
            {
                TaskGetIntByInt task = (TaskGetIntByInt)_ar.AsyncState;
                int result = task.EndInvoke(_ar);
                Assert.AreEqual(100, result);
                Console.WriteLine("CALLBACK END");
            });

            IAsyncResult ar = someTask.BeginInvoke(10, callback, someTask);

        }

        [TestMethod]
        public void T4()
        {
            WaitCallback waitCallback = new WaitCallback(obj =>
            {
                Thread.Sleep(100);
                int wc, cc;
                ThreadPool.GetAvailableThreads(out wc, out cc);
                int wc2, cc2;
                ThreadPool.GetMaxThreads(out wc2, out cc2);
                Console.WriteLine("[{0}] - {1} - {2}/{4} - {3}/{5}", Thread.CurrentThread.ManagedThreadId, "Task End", wc, cc, wc2, cc2);
            });
            Console.WriteLine("SetMinThreads : {0}", ThreadPool.SetMinThreads(5, 2));
            Console.WriteLine("SetMaxThreads : {0}", ThreadPool.SetMaxThreads(10, 4));



            for (int i = 0; i < 20; i++)
            {
                ThreadPool.QueueUserWorkItem(waitCallback);
            }


            Thread.Sleep(100 * 30);
            {

                int wc, cc;
                ThreadPool.GetAvailableThreads(out wc, out cc);
                int wc2, cc2;
                ThreadPool.GetMaxThreads(out wc2, out cc2);
                Console.WriteLine("[{0}] - {1} - {2}/{4} - {3}/{5}", Thread.CurrentThread.ManagedThreadId, "T4 End", wc, cc, wc2, cc2);
            }
        }

        [TestMethod]
        public void T5()
        {
            var task = AsyncPower(new Arg(10));

            var task3 = task.ContinueWith(t =>
              {

                  Console.WriteLine("[{0}] - {1}", Thread.CurrentThread.ManagedThreadId, "4");
              });

            var task2 = task.ContinueWith(t =>
            {
                Console.WriteLine("[{0}] - {1}", Thread.CurrentThread.ManagedThreadId, "1");
                return Task.Run(() =>
                {
                    Console.WriteLine("[{0}] - {1}", Thread.CurrentThread.ManagedThreadId, "3");
                });
            });

            var task4 = task2.ContinueWith(t =>
            {
                Console.WriteLine("[{0}] - {1}", Thread.CurrentThread.ManagedThreadId, "2");
            });


            task2.Wait();
            task3.Wait();
            task4.Wait();

        }

        [TestMethod]
        public async Task T6Async()
        {
            using (var arg = new Arg(10))
            {
                Task<int> task = AsyncPower(arg);
                task.ContinueWith(t =>
                {
                    Console.WriteLine(t.Result);
                });
            }
        }

        private Task<int> AsyncPower(Arg args)
        {
            return Task<int>.Run(() =>
            {
                Thread.Sleep(1000);
                return args.Value * args.Value;
            });
        }

        private void DoSomeThing(int i)
        {
            Thread.Sleep(100);
        }

        public class Arg : IDisposable
        {
            public int Value { get; private set; }

            public Arg(int value)
            {
                Value = value;
            }

            public void Dispose()
            {
                Console.WriteLine("Con.Dispose");
            }
        }
    }
}
