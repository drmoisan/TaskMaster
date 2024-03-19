using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class ObserverHelper<T> : IObserver<T>
    {
        private IDisposable _unsubscriber;
        private string _instanceName;
        private Action<T> _action;

        public ObserverHelper(string instanceName, Action<T> action)
        {
            _instanceName = instanceName;
            _action = action;
        }

        public string Name => _instanceName;

        public virtual void Subscribe(IObservable<T> provider)
        {
            if (provider != null)
                _unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe() => _unsubscriber.Dispose();

        public void OnCompleted() => this.Unsubscribe();

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(T value)
        {
            _action(value);
        }
    }
}
