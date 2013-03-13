using System;
using Android.App;

namespace CrossLight
{
    public class AndroidTopActivity
        : ITopActivity
    {
        public Activity Activity { get; set; }

        public bool RequestMainThreadAction(Action action)
        {
            var activity = Activity;
            if (activity == null)
                return false;

            activity.RunOnUiThread(action);
            return true;
        }
    }
}