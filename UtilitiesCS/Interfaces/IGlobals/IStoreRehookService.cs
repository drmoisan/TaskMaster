using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS
{
    /// <summary>
    /// Collaborator invoked by <see cref="IStoreDisableService.ReenableAsync"/> after disabled state
    /// is cleared, to re-add the Store and re-register its event handlers. This interface is the sole
    /// F1&#8596;F3 boundary (issue #261, epic #260): F1 defines the seam so <c>ReenableAsync</c> can
    /// invoke a collaborator without taking any forward dependency on an F3 type. Wave 0 ships the
    /// no-op default (<see cref="NoOpStoreRehookService"/>); F3 (#263) supplies the real
    /// implementation via a small, in-scope edit that constructs the service with the real
    /// collaborator.
    /// </summary>
    public interface IStoreRehookService
    {
        /// <summary>
        /// Re-adds the store and re-registers its event handlers. Awaited by <c>ReenableAsync</c>
        /// after disabled state has been cleared.
        /// </summary>
        /// <param name="identity">The identity of the store to rehook.</param>
        /// <returns>A task that completes when the rehook finishes.</returns>
        Task RehookAsync(StoreIdentity identity);
    }

    /// <summary>
    /// Wave-0 default rehook collaborator: performs no rehook and completes immediately. Enables F1
    /// to ship without depending on F3. F3 replaces it with a real implementation.
    /// </summary>
    internal sealed class NoOpStoreRehookService : IStoreRehookService
    {
        /// <summary>Performs no rehook. Returns a completed task.</summary>
        /// <param name="identity">The identity of the store to rehook (ignored by the no-op).</param>
        /// <returns>A completed task.</returns>
        public Task RehookAsync(StoreIdentity identity) => Task.CompletedTask;
    }
}
