using Reface.AppStarter.AppModules;

namespace Reface.AppStarter.Async.Tests.AppModules
{
    [ComponentScanAppModule]
    [AsyncAppModule]
    [ProxyAppModule]
    public class TestAppModule : AppModule
    {
    }
}
