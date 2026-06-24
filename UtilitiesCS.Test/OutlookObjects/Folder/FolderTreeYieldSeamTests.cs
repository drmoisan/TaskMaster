using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeYieldSeamTests
    {
        [TestMethod]
        public void FakeDeadlineClock_AdvanceControlsYieldDecision()
        {
            var clock = new FakeDeadlineClock();

            clock.ShouldYield().Should().BeFalse();
            clock.AdvanceToYield();
            clock.ShouldYield().Should().BeTrue();
            clock.Reset();

            clock.ShouldYield().Should().BeFalse();
            clock.CheckCount.Should().Be(3);
            clock.ResetCount.Should().Be(1);
        }

        [TestMethod]
        public async Task FakeDispatcherYield_RecordsYieldCount()
        {
            var dispatcherYield = new FakeDispatcherYield();

            await dispatcherYield.YieldAsync(CancellationToken.None);
            await dispatcherYield.YieldAsync(CancellationToken.None);

            dispatcherYield.YieldCount.Should().Be(2);
        }

        [TestMethod]
        public async Task FakeDispatcherYield_CanceledTokenThrows()
        {
            var dispatcherYield = new FakeDispatcherYield();
            using (var source = new CancellationTokenSource())
            {
                source.Cancel();

                await dispatcherYield
                    .Invoking(item => item.YieldAsync(source.Token))
                    .Should()
                    .ThrowAsync<OperationCanceledException>();
            }
        }
    }
}
