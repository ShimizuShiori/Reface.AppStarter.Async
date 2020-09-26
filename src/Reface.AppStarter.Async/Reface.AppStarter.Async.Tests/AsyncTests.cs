using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reface.AppStarter.Async.Tests.AppModules;
using Reface.AppStarter.Async.Tests.Services;
using Reface.AppStarter.Attributes;
using Reface.AppStarter.UnitTests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async.Tests
{
    [TestClass]
    public class AsyncTests : TestClassBase<TestAppModule>
    {
        [AutoCreate]
        public ITestService Service { get; set; }

        private int GetTid()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        [TestMethod]
        public void NoReturnValue()
        {
            int threadId1 = -1, threadId2 = -1;
            threadId1 = Thread.CurrentThread.ManagedThreadId;
            this.Service.Triggered += (sender, e) =>
             {
                 threadId2 = Thread.CurrentThread.ManagedThreadId;
             };
            this.Service.NoReturnValue();
            Task.Delay(10);
            Assert.AreNotEqual(threadId1, threadId2);
        }

        [TestMethod]
        public void ReturnTaskOfVoid()
        {
            int threadId1 = -1, threadId2 = -1;
            threadId1 = Thread.CurrentThread.ManagedThreadId;
            this.Service.Triggered += (sender, e) =>
            {
                threadId2 = Thread.CurrentThread.ManagedThreadId;
            };
            this.Service.ReturnTaskOfVoid();
            Task.Delay(10);
            Assert.AreNotEqual(threadId1, threadId2);
        }

        [TestMethod]
        public void ReturnTaskOfInt()
        {
            int threadId1 = -1, threadId2 = -1;
            threadId1 = Thread.CurrentThread.ManagedThreadId;
            this.Service.Triggered += (sender, e) =>
            {
                threadId2 = Thread.CurrentThread.ManagedThreadId;
            };
            Task<int> task = this.Service.ReturnTaskOfInt(1, 1);

            Assert.AreEqual(2, task.Result);

            Assert.AreNotEqual(threadId1, threadId2);
        }

        [TestMethod]
        public void ReturnLazyOfInt()
        {
            int threadId1 = -1, threadId2 = -1;
            threadId1 = Thread.CurrentThread.ManagedThreadId;
            this.Service.Triggered += (sender, e) =>
            {
                threadId2 = Thread.CurrentThread.ManagedThreadId;
            };
            Lazy<int> result = this.Service.ReturnLazyOfInt(1);

            Thread.Sleep(100);
            Console.WriteLine("{0}\tStart Assert Value", Thread.CurrentThread.ManagedThreadId);
            Assert.AreEqual(2, result.Value);

            Assert.AreNotEqual(threadId1, threadId2);
        }

        [TestMethod]
        public void ReturnInt()
        {
            int threadId1 = -1, threadId2 = -1;
            threadId1 = Thread.CurrentThread.ManagedThreadId;
            this.Service.Triggered += (sender, e) =>
            {
                threadId2 = Thread.CurrentThread.ManagedThreadId;
            };
            int result = this.Service.ReturnInt(1);

            Assert.AreEqual(11, result);

            Assert.AreEqual(threadId1, threadId2);
        }

        [TestMethod]
        public void ReturValueFromOtherServices()
        {
            Lazy<string> lazyOfStr = this.Service.GetUserNameById(1);
            string result = lazyOfStr.Value;
            Assert.AreEqual("ADMIN", result);
        }
    }
}
