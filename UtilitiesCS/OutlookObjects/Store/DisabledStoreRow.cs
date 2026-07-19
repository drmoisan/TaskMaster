#nullable enable
namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Pure row view-model projected from F1's <see cref="DisabledStoreEntry"/> for the
    /// disabled-stores list (issue #265). Carries no WinForms, I/O, or Outlook dependency;
    /// the grid binds to a list of these and click handling resolves a row from the
    /// controller's own list.
    /// </summary>
    public class DisabledStoreRow
    {
        /// <summary>The F1 store identity used to call <c>ReenableAsync</c>.</summary>
        public StoreIdentity Identity { get; set; }

        /// <summary>The display text for the store (from the entry's identity).</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Human-readable scope text ("Session Only" or "Future Sessions").</summary>
        public string ScopeLabel { get; set; } = string.Empty;

        /// <summary>
        /// True when the store is disabled for future sessions; drives the visual distinction
        /// rendered by the Designer/cell-formatting layer.
        /// </summary>
        public bool IsFutureSession { get; set; }
    }
}
