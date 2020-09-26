using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reface.AppStarter.Async.Tests.AppModules;
using Reface.AppStarter.Attributes;
using Reface.AppStarter.UnitTests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async.Tests
{
    [TestClass]
    public class TaskTests : TestClassBase<TestAppModule>
    {
        [AutoCreate]
        public ILogger Logger { get; set; }

        [TestMethod]
        public void MyTestMethod()
        {
            Lazy<int> lazy = new Lazy<int>(() =>
            {
                this.Logger.Log("lazy will return 100");
                return 100;
            });

            Lazy<int> lazy2 = new Lazy<int>(() =>
            {
                Task<int> task = new Task<int>(obj =>
                {
                    this.Logger.Log("task doing");
                    Lazy<int> realObj = (Lazy<int>)obj;
                    int _value = realObj.Value;
                    this.Logger.Log("task done");
                    return _value;
                }, lazy);
                task.Start();
                this.Logger.Log("lazy2 wil return value by task");
                return task.Result;
            });

            Thread.Sleep(1000);

            int value = lazy2.Value;
            this.Logger.Log("get value from lazy2");
            Assert.AreEqual(100, value);
        }
    }
}
