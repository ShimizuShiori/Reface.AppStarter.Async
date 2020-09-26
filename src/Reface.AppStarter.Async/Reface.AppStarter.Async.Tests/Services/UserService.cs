using Reface.AppStarter.Attributes;

namespace Reface.AppStarter.Async.Tests.Services
{
    public interface IUserService
    {
        string GetAdminName();
    }

    [Component]
    public class UserService : IUserService
    {
        public string GetAdminName()
        {
            return "ADMIN";
        }
    }
}
