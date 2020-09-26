using Reface.AppStarter.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Reface.AppStarter.Async.Tests.Services
{
    public interface ITestService
    {
        event EventHandler Triggered;

        void NoReturnValue();

        Task ReturnTaskOfVoid();

        Task<int> ReturnTaskOfInt(int i, int j);

        Lazy<int> ReturnLazyOfInt(int i);

        int ReturnInt(int i);

        Lazy<string> GetUserNameById(int id);
    }

    [Component]
    public class TestService : ITestService
    {
        public event EventHandler Triggered;
        private readonly IUserService userService;

        public TestService(IUserService userService)
        {
            this.userService = userService;
        }

        private void OnTriggered()
        {
            this.Triggered?.Invoke(this, EventArgs.Empty);
        }

        [Async]
        public void NoReturnValue()
        {
            OnTriggered();
        }

        [Async]
        public Task ReturnTaskOfVoid()
        {
            return new Task(() =>
            {
                OnTriggered();
            });
        }

        [Async]
        public Task<int> ReturnTaskOfInt(int i, int j)
        {
            return new Task<int>(() =>
            {
                return i + j;
            });
        }

        [Async]
        public Lazy<int> ReturnLazyOfInt(int i)
        {
            return new Lazy<int>(() =>
            {
                Thread.Sleep(10);
                Console.WriteLine("{0}\tResult Ready", Thread.CurrentThread.ManagedThreadId);
                return i * 2;
            });
        }

        public Lazy<int> Test(Lazy<int> args)
        {
            Func<object> funcObj = () => args.Value;
            Func<int> func = new Func<int>(() => args.Value);

            Task<int> task = new Task<int>(() =>
            {
                return args.Value;
            });
            task.Start();
            return new Lazy<int>(() => task.Result);
        }

        [Async]
        public int ReturnInt(int i)
        {
            this.OnTriggered();
            return i + 10;
        }

        [Async]
        public Lazy<string> GetUserNameById(int id)
        {
            Thread.Sleep(100);
            return new Lazy<string>(() => this.userService.GetAdminName());
        }
    }
}
