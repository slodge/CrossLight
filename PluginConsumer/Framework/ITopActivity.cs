using Android.App;
using Cirrious.CrossCore.Droid.Interfaces;

namespace PluginConsumer.Framework
{
    public interface ITopActivity
        : IMvxAndroidCurrentTopActivity
    {
        new Activity Activity { get; set; }        
    }
}