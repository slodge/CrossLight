using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;

namespace PluginConsumer.Framework
{
    public class LayoutInflaterProvider
        : IMvxLayoutInflater
    {
        public LayoutInflaterProvider(LayoutInflater layoutInflater)
        {
            LayoutInflater = layoutInflater;
        }

        public LayoutInflater LayoutInflater { get; private set; }
    }
}