using Cirrious.CrossCore.Interfaces.Core;
using Cirrious.CrossCore.Interfaces.IoC;

namespace CrossLight
{
    public class MainThreadDispatcherProvider
        : IMvxMainThreadDispatcherProvider
    {
        public IMvxMainThreadDispatcher Dispatcher 
        { 
            get { return Mvx.Resolve<ITopActivity>(); }
        }
    }
}