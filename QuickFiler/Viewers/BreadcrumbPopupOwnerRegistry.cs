#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Issue #810 (AC7): the popup-owner registry behind
    /// <c>QfcFormViewer.IsDeactivationSelfInflictedByOwnPopup</c>.
    /// <para>
    /// Each item viewer that can own a breadcrumb popup registers a predicate reporting whether its
    /// own popup is currently open. The form asks this registry whether any of them is, which is the
    /// question the issue #796 (AC2) self-inflicted-deactivation guard turns on. The derivation was
    /// previously inline on the form-derived viewer, which is exempt from coverage measurement at
    /// class level and therefore emits no Cobertura class element at all; holding it here makes it
    /// measurable and testable without a form hierarchy.
    /// </para>
    /// </summary>
    internal sealed class BreadcrumbPopupOwnerRegistry
    {
        private readonly Dictionary<Control, Func<bool>> _owners =
            new Dictionary<Control, Func<bool>>();

        /// <summary>
        /// Records the predicate reporting whether <paramref name="itemViewer"/>'s own breadcrumb
        /// popup is open, replacing any predicate previously recorded for the same control.
        /// </summary>
        /// <param name="itemViewer">
        /// The control that owns the popup. A null value is ignored rather than rejected: the
        /// registration hop runs from a form lookup that can legitimately find no form.
        /// </param>
        /// <param name="popupIsOpen">
        /// The predicate. A null value is ignored on the same reasoning.
        /// </param>
        /// <remarks>
        /// Assigning by key rather than adding is what keeps the registry one entry per owner. An
        /// appending registry would keep consulting a superseded predicate after its owner had
        /// closed its popup, which would report a popup open when none is.
        /// </remarks>
        internal void Register(Control itemViewer, Func<bool> popupIsOpen)
        {
            if (itemViewer == null || popupIsOpen == null)
            {
                return;
            }

            _owners[itemViewer] = popupIsOpen;
        }

        /// <summary>
        /// Whether any registered owner reports its own breadcrumb popup open.
        /// </summary>
        /// <remarks>
        /// With no registration this is false, which is the genuine case: a form that owns no open
        /// popup has not caused its own deactivation, so the issue #677 contract stays in force.
        /// </remarks>
        internal bool AnyOpen => _owners.Values.Any(popupIsOpen => popupIsOpen());
    }
}
