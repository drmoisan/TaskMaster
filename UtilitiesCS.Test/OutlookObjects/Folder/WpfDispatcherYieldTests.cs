using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class WpfDispatcherYieldTests
    {
        [TestMethod]
        public async Task YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield()
        {
            var dispatcherYield = new WpfDispatcherYield();
            using (var source = new CancellationTokenSource())
            {
                source.Cancel();

                await dispatcherYield
                    .Invoking(item => item.YieldAsync(source.Token))
                    .Should()
                    .ThrowAsync<OperationCanceledException>();
            }
        }

        [TestMethod]
        public async Task YieldAsync_WithoutDispatcher_RemainsStrict()
        {
            var dispatcherYield = new WpfDispatcherYield();

            await dispatcherYield
                .Invoking(item => item.YieldAsync(CancellationToken.None))
                .Should()
                .ThrowAsync<InvalidOperationException>();
        }
    }
}
