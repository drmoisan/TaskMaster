using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class ThreadSafeSingleShotGuard_Tests
    {
        [TestMethod]
        public void CheckAndSetFirstCall_ShouldReturnTrueThenFalse()
        {
            // Arrange
            var guard = new ThreadSafeSingleShotGuard();

            // Act
            bool first = guard.CheckAndSetFirstCall;
            bool second = guard.CheckAndSetFirstCall;

            // Assert
            first.Should().BeTrue();
            second.Should().BeFalse();
        }

        [TestMethod]
        public void CheckAndSetFirstCall_ShouldAllowOnlyOneConcurrentWinner()
        {
            // Arrange
            var guard = new ThreadSafeSingleShotGuard();
            var start = new ManualResetEventSlim(false);
            var tasks = Enumerable
                .Range(0, 16)
                .Select(_ =>
                    Task.Run(() =>
                    {
                        start.Wait();
                        return guard.CheckAndSetFirstCall;
                    })
                )
                .ToArray();

            // Act
            start.Set();
            Task.WaitAll(tasks);
            int winners = tasks.Count(task => task.Result);

            // Assert
            winners.Should().Be(1);
            tasks.Count(task => !task.Result).Should().Be(15);
        }
    }
}
