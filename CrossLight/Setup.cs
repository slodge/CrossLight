using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore.Droid.Interfaces;
using Cirrious.CrossCore.Exceptions;
using Cirrious.CrossCore.Interfaces.Core;
using Cirrious.CrossCore.Interfaces.IoC;
using Cirrious.CrossCore.Interfaces.Platform.Diagnostics;
using Cirrious.CrossCore.Interfaces.Plugins;
using Cirrious.CrossCore.Plugins;
using Cirrious.MvvmCross.Binding.Droid;

namespace CrossLight
{
    public class MvxSimpleIoCContainer
    : IMvxIoCProvider
    {
        public static readonly IMvxIoCProvider Instance = new MvxSimpleIoCContainer(); 

        private readonly Dictionary<Type, IResolver> _resolvers = new Dictionary<Type, IResolver>();

        private interface IResolver
        {
            object Resolve();
            bool Supports(ResolveOptions options);
        }

        private class ConstructingResolver : IResolver
        {
            private readonly Type _type;
            private readonly MvxSimpleIoCContainer _parent;

            public ConstructingResolver(Type type, MvxSimpleIoCContainer parent)
            {
                _type = type;
                _parent = parent;
            }

            #region Implementation of IResolver

            public object Resolve()
            {
                return _parent.IoCConstruct(_type);
            }

            public bool Supports(ResolveOptions options)
            {
                return options != ResolveOptions.SingletonOnly;
            }

            #endregion
        }

        private class SingletonResolver : IResolver
        {
            private readonly object _theObject;

            public SingletonResolver(object theObject)
            {
                _theObject = theObject;
            }

            #region Implementation of IResolver

            public object Resolve()
            {
                return _theObject;
            }

            public bool Supports(ResolveOptions options)
            {
                return options != ResolveOptions.CreateOnly;
            }

            #endregion
        }

        private class ConstructingSingletonResolver : IResolver
        {
            private readonly Func<object> _theConstructor;
            private object _theObject;

            public ConstructingSingletonResolver(Func<object> theConstructor)
            {
                _theConstructor = theConstructor;
            }

            #region Implementation of IResolver

            public object Resolve()
            {
                if (_theObject != null)
                    return _theObject;

                lock (_theConstructor)
                {
                    if (_theObject == null)
                        _theObject = _theConstructor();
                }

                return _theObject;
            }

            public bool Supports(ResolveOptions options)
            {
                return options != ResolveOptions.CreateOnly;
            }

            #endregion
        }

        public bool CanResolve<T>()
            where T : class
        {
            return CanResolve(typeof(T));
        }

        public bool CanResolve(Type t)
        {
            lock (this)
            {
                return _resolvers.ContainsKey(t);
            }
        }

        public bool TryResolve<T>(out T resolved)
            where T : class
        {
            object item;
            var toReturn = TryResolve(typeof(T), out item);
            resolved = (T)item;
            return toReturn;
        }

        public bool TryResolve(Type type, out object resolved)
        {
            lock (this)
            {
                return InternalTryResolve(type, out resolved);
            }
        }

        public T Resolve<T>()
            where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type t)
        {
            lock (this)
            {
                object resolved;
                if (!InternalTryResolve(t, out resolved))
                {
                    throw new MvxException("Failed to resolve type {0}", t.FullName);
                }
                return resolved;
            }
        }

        public T GetSingleton<T>()
            where T : class
        {
            return (T)GetSingleton(typeof(T));
        }

        public object GetSingleton(Type t)
        {
            lock (this)
            {
                object resolved;
                if (!InternalTryResolve(t, ResolveOptions.SingletonOnly, out resolved))
                {
                    throw new MvxException("Failed to resolve type {0}", t.FullName);
                }
                return resolved;
            }
        }

        public T Create<T>()
            where T : class
        {
            return (T)Create(typeof(T));
        }

        public object Create(Type t)
        {
            lock (this)
            {
                object resolved;
                if (!InternalTryResolve(t, ResolveOptions.CreateOnly, out resolved))
                {
                    throw new MvxException("Failed to resolve type {0}", t.FullName);
                }
                return resolved;
            }
        }

        public void RegisterType<TInterface, TToConstruct>()
            where TInterface : class
            where TToConstruct : class, TInterface
        {
            RegisterType(typeof(TInterface), typeof(TToConstruct));
        }

        public void RegisterType(Type tInterface, Type tConstruct)
        {
            lock (this)
            {
                _resolvers[tInterface] = new ConstructingResolver(tConstruct, this);
            }
        }

        public void RegisterSingleton<TInterface>(TInterface theObject)
            where TInterface : class
        {
            RegisterSingleton(typeof(TInterface), theObject);
        }

        public void RegisterSingleton(Type tInterface, object theObject)
        {
            lock (this)
            {
                _resolvers[tInterface] = new SingletonResolver(theObject);
            }
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> theConstructor)
            where TInterface : class
        {
            RegisterSingleton(typeof(TInterface), theConstructor);
        }

        public void RegisterSingleton(Type tInterface, Func<object> theConstructor)
        {
            lock (this)
            {
                _resolvers[tInterface] = new ConstructingSingletonResolver(theConstructor);
            }
        }

        public T IoCConstruct<T>()
            where T : class
        {
            return (T)IoCConstruct(typeof(T));
        }

        public object IoCConstruct(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            var firstConstructor = constructors.FirstOrDefault();

            if (firstConstructor == null)
                throw new MvxException("Failed to find constructor for type {0}", type.FullName);

            var parameters = GetIoCParameterValues(type, firstConstructor);
            var toReturn = firstConstructor.Invoke(parameters.ToArray());

            // decide not to do property injection for now
            //InjectProperties(type, toReturn);

            return toReturn;
        }

#warning Remove InjectProperties then
        /*
        private void InjectProperties(Type type, object toReturn)
        {
            var injectableProperties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(p => p.PropertyType.IsInterface)
                .Where(p => p.CanWrite);

            foreach (var injectableProperty in injectableProperties)
            {
                object propertyValue;
                if (TryResolve(injectableProperty.PropertyType, out propertyValue))
                {
                    injectableProperty.SetValue(toReturn, propertyValue, null);
                }
            }
        }
        */

        private enum ResolveOptions
        {
            All,
            CreateOnly,
            SingletonOnly
        }

        private bool InternalTryResolve(Type type, out object resolved)
        {
            return InternalTryResolve(type, ResolveOptions.All, out resolved);
        }

        private bool InternalTryResolve(Type type, ResolveOptions resolveOptions, out object resolved)
        {
            IResolver resolver;
            if (!_resolvers.TryGetValue(type, out resolver))
            {
                resolved = CreateDefault(type);
                return false;
            }

            if (!resolver.Supports(resolveOptions))
            {
                resolved = CreateDefault(type);
                return false;
            }

            var raw = resolver.Resolve();
            if (!(type.IsInstanceOfType(raw)))
            {
                throw new MvxException("Resolver returned object type {0} which does not support interface {1}",
                                       raw.GetType().FullName, type.FullName);
            }

            resolved = raw;
            return true;
        }

        private static object CreateDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private List<object> GetIoCParameterValues(Type type, ConstructorInfo firstConstructor)
        {
            var parameters = new List<object>();
            foreach (var parameterInfo in firstConstructor.GetParameters())
            {
                object parameterValue;
                if (!TryResolve(parameterInfo.ParameterType, out parameterValue))
                {
                    throw new MvxException(
                        "Failed to resolve parameter for parameter {0} of type {1} when creating {2}",
                        parameterInfo.Name,
                        parameterInfo.ParameterType.Name,
                        type.FullName);
                }

                parameters.Add(parameterValue);
            }
            return parameters;
        }
    }

    public class MvxDebugTrace : IMvxTrace
    {
        #region IMvxTrace Members

        public void Trace(MvxTraceLevel level, string tag, string message)
        {
            Log.Info(tag, message);
            Debug.WriteLine(tag + ":" + level + ":" + message);
        }

        public void Trace(MvxTraceLevel level, string tag, string message, params object[] args)
        {
            try
            {
                Log.Info(tag, message, args);
                Debug.WriteLine(string.Format(tag + ":" + level + ":" + message, args));
            }
            catch (FormatException)
            {
                Trace(MvxTraceLevel.Error, tag, "Exception during trace");
                Trace(level, tag, message);
            }
        }

        #endregion
    }

    public interface ITopActivity
        : IMvxAndroidCurrentTopActivity
        , IMvxMainThreadDispatcher
    {
        new Activity Activity { get; set; }        
    }

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

    public class MainThreadDispatcherProvider
        : IMvxMainThreadDispatcherProvider
    {
        public IMvxMainThreadDispatcher Dispatcher 
        { 
            get { return Mvx.Resolve<ITopActivity>(); }
        }
    }

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
            get { return GetType().Namespace; }
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
    public class Setup
    {
        public static readonly Setup Instance = new Setup();

        private bool _init;

        public void EnsureInitialized(Context applicationContext)
        {
            if (_init)
                return;

            _init = true;
            var ioc = MvxSimpleIoCContainer.Instance;

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