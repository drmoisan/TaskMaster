using System.Diagnostics.CodeAnalysis;
using BrightIdeasSoftware;

namespace TaskTree
{
    /// <summary>
    /// Host adapter that wraps a concrete <see cref="BrightIdeasSoftware.TreeListView"/> and exposes
    /// only the <see cref="ITreeVisual"/> operations the move logic requires. This isolates the
    /// third-party control behind a mockable seam so the host-neutral move logic never references the
    /// concrete control.
    /// </summary>
    /// <remarks>
    /// Exemption site E2. Excluded from coverage under the ratified WinForms host-adapter exemption:
    /// <see cref="AddObject"/>/<see cref="RemoveObject"/> are pure two-line delegations to the wrapped
    /// virtual-mode control, whose members cannot execute deterministically on an unshown, handle-less
    /// control without reintroducing message-pump/live-control reliance. The move logic that consumes
    /// this adapter is fully covered against <see cref="ITreeVisual"/> mocks.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    internal sealed class TreeListViewVisual : ITreeVisual
    {
        private readonly TreeListView _tree;

        /// <summary>Creates an adapter over the supplied tree control.</summary>
        public TreeListViewVisual(TreeListView tree)
        {
            _tree = tree;
        }

        /// <summary>The wrapped control, exposed to support reference comparison of source/target trees.</summary>
        public TreeListView Tree => _tree;

        /// <inheritdoc />
        public void AddObject(object model)
        {
            _tree.AddObject(model);
        }

        /// <inheritdoc />
        public void RemoveObject(object model)
        {
            _tree.RemoveObject(model);
        }
    }
}
