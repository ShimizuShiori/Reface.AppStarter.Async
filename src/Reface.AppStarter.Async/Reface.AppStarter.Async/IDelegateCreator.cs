using System;

namespace Reface.AppStarter.Async
{
    public interface IDelegateCreator
    {
        Delegate CreateGetValueFromLazy(Type valueType);

        Delegate CreateGetResultFromTask(Type resultType);
    }
}
