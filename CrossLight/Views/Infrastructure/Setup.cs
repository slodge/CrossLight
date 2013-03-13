using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore.Droid.Interfaces;
using Cirrious.CrossCore.Interfaces.Core;
using Cirrious.CrossCore.Interfaces.Platform.Diagnostics;
using Cirrious.CrossCore.Interfaces.Plugins;
using Cirrious.CrossCore.Plugins;
using Cirrious.MvvmCross.Binding.Droid;

namespace CrossLight
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

            ioc.RegisterSingleton<IMvxTrace>(new MvxDebugTrace());
            ioc.RegisterSingleton<IMvxAndroidGlobals>(new AndroidGlobals(applicationContext));

            var topActivity = new AndroidTopActivity();
            ioc.RegisterSingleton<ITopActivity>(topActivity);
            ioc.RegisterSingleton<IMvxAndroidCurrentTopActivity>(topActivity);

            ioc.RegisterSingleton<IMvxPluginManager>(new MvxFileBasedPluginManager("Droid"));
            ioc.RegisterSingleton<IMvxMainThreadDispatcherProvider>(new MainThreadDispatcherProvider());

            var builder = new MvxDroidBindingBuilder(ignored => { }, ignored => { }, ignored => { });
            builder.DoRegistration();
        }
    }
}