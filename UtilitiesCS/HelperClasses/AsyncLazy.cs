using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    /// <summary>
    /// Provides support for asynchronous lazy initialization. This type is fully threadsafe.
    /// https://blog.stephencleary.com/2012/08/asynchronous-lazy-initialization.html
    /// </summary>
    /// <typeparam name="T">The type of object that is being asynchronously initialized.</typeparam>
    public sealed class AsyncLazy<T>
    {
        /// <summary>
        /// The underlying lazy task.
        /// </summary>
        private readonly Lazy<Task<T>> instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The delegate that is invoked on a background thread to produce the value when it is needed.</param>
        public AsyncLazy(Func<T> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The delegate that is invoked on a background thread to produce the value when it is needed.</param>
        /// <param name="cancel">The cancellation token.</param>
        public AsyncLazy(Func<T> factory, CancellationToken cancel)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory, cancel));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The asynchronous delegate that is invoked on a background thread to produce the value when it is needed.</param>
        public AsyncLazy(Func<Task<T>> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The asynchronous delegate that is invoked on a background thread to produce the value when it is needed.</param>
        /// <param name="cancel">The cancellation token.</param>
        public AsyncLazy(Func<Task<T>> factory, CancellationToken cancel)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory, cancel));
        }

        /// <summary>
        /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncLazy&lt;T&gt;"/> to be await'ed.
        /// </summary>
        public TaskAwaiter<T> GetAwaiter()
        {
            return instance.Value.GetAwaiter();
        }

        /// <summary>
        /// Starts the asynchronous initialization, if it has not already started.
        /// </summary>
        public void Start()
        {
            _ = instance.Value;
        }
    }

    internal class AsyncLazyPropertyCachedValues 
    {
        // In this case, you only want the asynchronous operation executed once: the first time it’s requested.
        // After the operation completes, the result of the operation should be cached and returned immediately.
        public AsyncLazyPropertyCachedValues()
        {
            MyProperty = new AsyncLazy<int>(async () =>
            {
                await Task.Delay(100);
                return 13;
            });
        }

        public AsyncLazy<int> MyProperty { get; private set; }
    }
    
    internal class AsyncLazyUsage
    {
        // The idea is to have a lazy-initialized task, which represents the initialization of the resource.
        // The factory delegate passed to the constructor can be either synchronous (Func<T>) or asynchronous
        // (Func<Task<T>>); either way, it will be run on a thread pool thread.It will not be executed more than once,
        // even when multiple threads attempt to start it simultaneously (this is guaranteed by the Lazy type).
        //
        // There are two “triggers” which can start the initialization: awaiting an AsyncLazy<T> instance or
        // explicitly calling Start.When the factory delegate completes, the value is available, and any methods
        // awaiting the AsyncLazy<T> instance receive the value.
        //
        // It takes a few minutes to wrap your head around the theory, but it’s really easy in practice:
        private static readonly AsyncLazy<MyResource> myResource = new AsyncLazy<MyResource>(
        () => new MyResource()
        // or:
        // async () => { var ret = new MyResource(); await ret.InitAsync(); return ret; }
        );

        public async Task UseResource()
        {
            MyResource resource = await myResource;
            //...
        }
        
        public class MyResource { }
    }

    internal class DataBoundValues : INotifyPropertyChanged 
    {
        // Data binding requires immediate (synchronous) results, and it can only deal with a limited set of types.
        // Data binding will not give awaitable types any special treatment, so the type of an “asynchronous property”
        // used for data binding must be the type of the result of the asynchronous operation
        // (e.g., int instead of Task<int>).
        //
        // For this to work, the data-bound value must be initially set to some default or “unknown” value,
        // and the type can implement INotifyPropertyChanged to let the data binding know when the asynchronous value
        // has been determined.

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private int? myProperty;
        public int? MyProperty
        {
            get { return myProperty; }
            private set
            {
                myProperty = value;
                OnPropertyChanged();
            }
        }

        public async Task InitializeAsync()
        {
            await Task.Delay(100);
            MyProperty = 13;
        }

    }

}
