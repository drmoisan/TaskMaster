#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Yields folder tree work back to the UI dispatcher without requiring tests to run WPF.
    /// Unit tests must use fake dispatcher yield implementations.
    /// </summary>
    public interface IDispatcherYield
    {
        Task YieldAsync(CancellationToken cancellationToken);
    }
}
