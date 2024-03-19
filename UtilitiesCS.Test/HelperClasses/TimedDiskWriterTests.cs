using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.Interfaces;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class TimedDiskWriterTests
    {
        private MockRepository mockRepository;
        private Mock<TimeElapsedEventArgs> mockElapsedEventArgs;
        private Mock<ITimerWrapper> mockTimer;
        private Mock<TimedDiskWriter<string>> mockTimedDiskWriter;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockElapsedEventArgs = this.mockRepository.Create<TimeElapsedEventArgs>();
            this.mockElapsedEventArgs.SetupAllProperties();

            this.mockTimer = this.mockRepository.Create<ITimerWrapper>();
            this.mockTimer.SetupAllProperties();
            //this.mockTimer.SetupSet(x => x.Elapsed += It.IsAny<EventHandler<TimeElapsedEventArgs>>());
            this.mockTimer.SetupSet(x => x.AutoReset = It.IsAny<bool>());
            this.mockTimer.SetupSet(x => x.Enabled = It.IsAny<bool>());
            this.mockTimer.Setup(x => x.StartTimer()).Callback(
                () => this.mockTimer.SetupGet(y => y.Enabled).Returns(true));
            this.mockTimer.Setup(x => x.StopTimer()).Callback(
                () => this.mockTimer.SetupGet(y => y.Enabled).Returns(false));

            this.mockTimedDiskWriter = new Mock<TimedDiskWriter<string>> { CallBase = true };
            //this.mockTimedDiskWriter.SetupAllProperties();
            this.mockTimedDiskWriter.Setup(x => x.StartTimer())
                .Callback(() => this.mockTimedDiskWriter.SetupGet(
                    y => y.TimerActive).Returns(true));

            this.mockTimedDiskWriter.Setup(x => x.StopTimer())
                .Callback(() => this.mockTimedDiskWriter.SetupGet(
                    y => y.TimerActive).Returns(false));

            //this.mockTimedDiskWriter.SetupSet(x => x.Timer = It.IsAny<ITimerWrapper>());
        }

        private TimedDiskWriter<string> CreateTimedDiskWriter()
        {
            return new TimedDiskWriter<string>();
        }

        [TestMethod]
        public void Enqueue_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var timedDiskWriter = this.CreateTimedDiskWriter();

            timedDiskWriter.DiskWriter = (x) => { };
            timedDiskWriter.Config.WriteInterval = TimeSpan.FromHours(1);

            string item1 = "Queued String 1";
            string item2 = "Queued String 2";

            var expected = new BlockingCollection<string>(new ConcurrentQueue<string>())
            {
                item1,
                item2
            };

            // Act
            timedDiskWriter.Enqueue(item1);
            timedDiskWriter.Enqueue(item2);
            timedDiskWriter.StopTimer();

            var actual = timedDiskWriter.Queue;

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task EnqueueAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var timedDiskWriter = this.CreateTimedDiskWriter();
            
            timedDiskWriter.DiskWriter = (x) => {  };
            timedDiskWriter.Config.WriteInterval = TimeSpan.FromHours(1);

            string item1 = "Queued String 1";
            string item2 = "Queued String 2";

            var expected = new BlockingCollection<string>(new ConcurrentQueue<string>())
            {
                item1,
                item2
            };
            
            CancellationToken token = default;

            // Act
            await timedDiskWriter.EnqueueAsync(item1, token);
            await timedDiskWriter.EnqueueAsync(item2, token);
            timedDiskWriter.StopTimer();

            var actual = timedDiskWriter.Queue;

            // Assert
            actual.Should().BeEquivalentTo(expected);
            
        }

        [TestMethod]
        public void StartTimer_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var timedDiskWriter = this.CreateTimedDiskWriter();
            timedDiskWriter.DiskWriter = (x) => { };

            // Act
            timedDiskWriter.StartTimer();

            // Assert
            Assert.IsTrue(timedDiskWriter.TimerActive);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartTimer_StateUnderTest_NoDiskWriter()
        {
            // Arrange
            var timedDiskWriter = this.CreateTimedDiskWriter();

            // Act
            timedDiskWriter.StartTimer();

        }

        [TestMethod]
        public void StopTimer_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var timedDiskWriter = this.CreateTimedDiskWriter();
            timedDiskWriter.DiskWriter = (x) => { };

            // Act
            timedDiskWriter.StartTimer();
            timedDiskWriter.StopTimer();

            // Assert
            Assert.IsFalse(timedDiskWriter.TimerActive);
        }

        [TestMethod]
        public void TryStartTimer_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var timedDiskWriter = this.CreateTimedDiskWriter();
            timedDiskWriter.DiskWriter = (x) => { };

            // Act
            var actual = timedDiskWriter.TryStartTimer() && timedDiskWriter.TimerActive;

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TryStartTimer_StateUnderTest_NoDiskWriter()
        {
            // Arrange
            var timedDiskWriter = this.CreateTimedDiskWriter();

            // Act
            var expected = false;
            var actual = timedDiskWriter.TryStartTimer();

            // Assert
            Assert.AreEqual(expected, actual);
        }
       
        [TestMethod]
        public void Configuration_PropertyChanged_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var timedDiskWriter = this.mockTimedDiskWriter.Object; 
            
            object sender = null;
            var e = new PropertyChangedEventArgs("WriteInterval");
            
            // Act
            timedDiskWriter.Configuration_PropertyChanged(sender, e);

            // Assert
            this.mockTimedDiskWriter.Verify(x => x.StopTimer(), Times.Once);
            this.mockTimedDiskWriter.Verify(x => x.StartTimer(), Times.Once);
            Assert.IsTrue(timedDiskWriter.TimerActive);
        }

        [TestMethod]
        public void OnTimedEvent_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var timedDiskWriter = this.CreateTimedDiskWriter();
            
            string item1 = "Queued String 1";
            string item2 = "Queued String 2";
            timedDiskWriter.Queue = 
                new BlockingCollection<string>(
                    new ConcurrentQueue<string>()){ item1, item2 };

            var expected = new List<string> { item1, item2 };
            var actual = new List<string>();
            timedDiskWriter.DiskWriter = (items) => { actual = items.ToList(); };

            object sender = null;
            var e = new TimeElapsedEventArgs();

            //// Act
            timedDiskWriter.OnTimedEvent(sender, e);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void OnTimedEvent_StateUnderTest_4EmptyCallsDoesNotStopTimer()
        {
            // Arrange
            var timedDiskWriter = this.mockTimedDiskWriter.Object;

            timedDiskWriter.Queue =
                new BlockingCollection<string>(
                    new ConcurrentQueue<string>())
                { };

            timedDiskWriter.DiskWriter = (items) => { };

            object sender = null;
            var e = new TimeElapsedEventArgs();

            //// Act
            timedDiskWriter.StartTimer();

            for (int i = 0; i < 4; i++)
            {
                timedDiskWriter.OnTimedEvent(sender, e);
            }

            // Assert
            this.mockTimedDiskWriter.Verify(x => x.StopTimer(), Times.Never);
            Assert.IsTrue(timedDiskWriter.TimerActive);
        }

        [TestMethod]
        public void OnTimedEvent_StateUnderTest_5EmptyCallsStopTimer()
        {
            // Arrange
            var timedDiskWriter = this.mockTimedDiskWriter.Object;

            timedDiskWriter.Queue =
                new BlockingCollection<string>(
                    new ConcurrentQueue<string>()) { };

            timedDiskWriter.DiskWriter = (items) => { };

            object sender = null;
            var e = new TimeElapsedEventArgs();

            //// Act
            for (int i = 0; i < 5; i++)
            {
                timedDiskWriter.OnTimedEvent(sender, e);
            }

            // Assert
            this.mockTimedDiskWriter.Verify(x => x.StopTimer(), Times.Exactly(1));
            Assert.IsFalse(timedDiskWriter.TimerActive);
        }
    }
}
