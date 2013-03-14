using System.Reflection;
using Android.Content;
using Cirrious.CrossCore.Droid.Interfaces;

namespace NoBinding.Framework
{
    public class AndroidGlobals
        : IMvxAndroidGlobals
    {
        private readonly Context _applicationContext;

        public AndroidGlobals(Context applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public virtual string ExecutableNamespace
        {
            get { return "PluginConsumer"; }
        }

        public virtual Assembly ExecutableAssembly
        {
            get { return GetType().Assembly; }
        }

        public Context ApplicationContext
        {
            get { return _applicationContext; }
        }
    }
}