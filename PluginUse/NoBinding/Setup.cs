using Android.Content;
using Cirrious.CrossCore.Droid.Interfaces;
using Cirrious.CrossCore.Interfaces.Core;
using Cirrious.CrossCore.Interfaces.Platform.Diagnostics;
using Cirrious.CrossCore.Interfaces.Plugins;
using Cirrious.CrossCore.IoC;
using Cirrious.CrossCore.Platform.Diagnostics;
using Cirrious.CrossCore.Plugins;
using NoBinding.Framework;

namespace NoBinding
{
    public class Setup
    {
        public static readonly Setup Instance = new Setup();

        private Setup()
        {            
        }

        public void EnsureInitialized(Context applicationContext)
        {
            if (MvxSimpleIoCContainer.Instance != null)
                return;

            var ioc = MvxSimpleIoCContainer.Initialise();

            ioc.RegisterSingleton<IMvxTrace>(new MvxDebugOnlyTrace());
            ioc.RegisterSingleton<IMvxPluginManager>(new MvxFileBasedPluginManager("Droid"));

            var topActivity = new AndroidTopActivity();
            ioc.RegisterSingleton<ITopActivity>(topActivity);
            ioc.RegisterSingleton<IMvxAndroidCurrentTopActivity>(topActivity);
            ioc.RegisterSingleton<IMvxMainThreadDispatcherProvider>(topActivity);
       }
    }
}