﻿using Android.App;
using Android.OS;
using Cirrious.CrossCore.Interfaces.IoC;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using CrossLight.Framework;

namespace CrossLight.Views
{
    [Activity(Label = "CrossLight", MainLauncher = true, Icon = "@drawable/icon")]
    public class FooBarView : Activity
    {
        private MvxBindingContext _bindingContext;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // setup the application
            Setup.Instance.EnsureInitialized(ApplicationContext);
            Mvx.Resolve<ITopActivity>().Activity = this;

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

