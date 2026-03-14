using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ObserverHelper_Tests
    {
        [TestMethod]
        public void Subscribe_AndOnNext_InvokeActionAndExposeName()
        {
            // Arrange
            var provider = new TestObservable<int>();
            var received = new List<int>();
            var observer = new ObserverHelper<int>("numbers", received.Add);

            // Act
            observer.Subscribe(provider);
            provider.Publish(42);

            // Assert
            observer.Name.Should().Be("numbers");
            received.Should().Equal(42);
            provider.ObserverCount.Should().Be(1);
        }

        [TestMethod]
        public void Subscribe_WithNullProvider_DoesNotThrowOrSubscribe()
        {
            // Arrange
            var received = new List<int>();
            var observer = new ObserverHelper<int>("numbers", received.Add);

            // Act
            Action act = () => observer.Subscribe(null);

            // Assert
            act.Should().NotThrow();
            received.Should().BeEmpty();
        }

        [TestMethod]
        public void Unsubscribe_RemovesObserverFromProvider()
        {
            // Arrange
            var provider = new TestObservable<int>();
            var received = new List<int>();
            var observer = new ObserverHelper<int>("numbers", received.Add);
            observer.Subscribe(provider);

            // Act
            observer.Unsubscribe();
            provider.Publish(7);

            // Assert
            provider.ObserverCount.Should().Be(0);
            received.Should().BeEmpty();
        }

        [TestMethod]
        public void OnCompleted_UnsubscribesObserver()
        {
            // Arrange
            var provider = new TestObservable<int>();
            var received = new List<int>();
            var observer = new ObserverHelper<int>("numbers", received.Add);
            observer.Subscribe(provider);

            // Act
            observer.OnCompleted();
            provider.Publish(9);

            // Assert
            provider.ObserverCount.Should().Be(0);
            received.Should().BeEmpty();
        }

        [TestMethod]
        public void MultipleObservers_AllReceivePublishedValue()
        {
            // Arrange
            var provider = new TestObservable<string>();
            var firstReceived = new List<string>();
            var secondReceived = new List<string>();
            var first = new ObserverHelper<string>("first", firstReceived.Add);
            var second = new ObserverHelper<string>("second", secondReceived.Add);
            first.Subscribe(provider);
            second.Subscribe(provider);

            // Act
            provider.Publish("payload");

            // Assert
            firstReceived.Should().Equal("payload");
            secondReceived.Should().Equal("payload");
            provider.ObserverCount.Should().Be(2);
        }

        [TestMethod]
        public void OnError_ThrowsNotImplementedException()
        {
            // Arrange
            var observer = new ObserverHelper<int>("numbers", _ => { });
            Action act = () => observer.OnError(new InvalidOperationException("boom"));

            // Assert
            act.Should().Throw<NotImplementedException>();
        }

        private sealed class TestObservable<T> : IObservable<T>
        {
            private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

            public int ObserverCount => _observers.Count;

            public IDisposable Subscribe(IObserver<T> observer)
            {
                _observers.Add(observer);
                return new Subscription(_observers, observer);
            }

            public void Publish(T value)
            {
                foreach (var observer in _observers.ToArray())
                {
                    observer.OnNext(value);
                }
            }

            private sealed class Subscription : IDisposable
            {
                private readonly List<IObserver<T>> _observers;
                private readonly IObserver<T> _observer;

                public Subscription(List<IObserver<T>> observers, IObserver<T> observer)
                {
                    _observers = observers;
                    _observer = observer;
                }

                public void Dispose()
                {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}
