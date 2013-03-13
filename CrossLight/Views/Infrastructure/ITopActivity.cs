using Android.App;
using Cirrious.CrossCore.Droid.Interfaces;
using Cirrious.CrossCore.Interfaces.Core;

namespace CrossLight
{
    public interface ITopActivity
        : IMvxAndroidCurrentTopActivity
          , IMvxMainThreadDispatcher
    {
        new Activity Activity { get; set; }        
    }
}