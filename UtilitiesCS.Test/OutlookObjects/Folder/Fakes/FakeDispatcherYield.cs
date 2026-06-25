using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder.Fakes
{
    public sealed class FakeDispatcherYield : IDispatcherYield
    {
        public int YieldCount { get; private set; }

        public Task YieldAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            YieldCount++;
            return Task.CompletedTask;
        }
    }
}
