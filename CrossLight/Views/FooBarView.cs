using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using Android.OS;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using CrossLight.Annotations;

namespace CrossLight
{
    [Activity(Label = "CrossLight", MainLauncher = true, Icon = "@drawable/icon")]
    public class FooBarView : Activity
    {
        private MvxBindingContext _bindingContext;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Setup.Instance.EnsureInitialized(ApplicationContext);
            _bindingContext = new MvxBindingContext(this, new LayoutInflaterProvider(LayoutInflater), new FooBarViewModel());
            
            var view = _bindingContext.BindingInflate(Resource.Layout.Main, null);
            SetContentView(view);
        }

        protected override void OnDestroy()
        {
            _bindingContext.ClearAllBindings();
            base.OnDestroy();
        }
    }
}

