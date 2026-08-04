#nullable enable
using System;
using System.Collections.Generic;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>Immutable selector option exposed across the router boundary.</summary>
    public sealed class BreadcrumbSelectorOptionState
    {
        internal BreadcrumbSelectorOptionState(string identity, bool isSelectable)
        {
            Identity = identity;
            IsSelectable = isSelectable;
        }

        public string Identity { get; }
        public bool IsSelectable { get; }
    }

    /// <summary>Immutable committed, pending, and option state captured under the router lock.</summary>
    public sealed class BreadcrumbSelectorState
    {
        internal BreadcrumbSelectorState(
            bool isOpen,
            string? committedIdentity,
            string? pendingIdentity,
            IReadOnlyList<BreadcrumbSelectorOptionState> options
        )
        {
            IsOpen = isOpen;
            CommittedIdentity = committedIdentity;
            PendingIdentity = pendingIdentity;
            var copy = new BreadcrumbSelectorOptionState[options.Count];
            for (int index = 0; index < options.Count; index++)
            {
                copy[index] = options[index];
            }
            Options = Array.AsReadOnly(copy);
        }

        public bool IsOpen { get; }
        public string? CommittedIdentity { get; }
        public string? PendingIdentity { get; }
        public IReadOnlyList<BreadcrumbSelectorOptionState> Options { get; }
    }

    /// <summary>Immutable outcome of one router-owned selector transition.</summary>
    public sealed class BreadcrumbSelectionTransition
    {
        internal BreadcrumbSelectionTransition(
            bool handled,
            bool selectionChanged,
            bool openStateChanged,
            string? renderJson,
            BreadcrumbSelectorState selectorState
        )
        {
            Handled = handled;
            SelectionChanged = selectionChanged;
            OpenStateChanged = openStateChanged;
            RenderJson = renderJson;
            SelectorState = selectorState;
        }

        public bool Handled { get; }
        public bool SelectionChanged { get; }
        public bool OpenStateChanged { get; }
        public string? RenderJson { get; }
        public BreadcrumbSelectorState SelectorState { get; }
    }

    [Flags]
    internal enum BreadcrumbSelectionEffects
    {
        None = 0,
        Handled = 1,
        SelectionChanged = 2,
        OpenStateChanged = 4,
        RenderRequired = 8,
    }

    /// <summary>
    /// Host-neutral selector session over stable row identities. Closed navigation commits directly;
    /// open navigation changes only pending state until commit or cancellation.
    /// </summary>
    internal sealed class BreadcrumbSelectionSession
    {
        private readonly BreadcrumbStateModel _model;

        /// <summary>Creates a session bound to the current breadcrumb model.</summary>
        public BreadcrumbSelectionSession(BreadcrumbStateModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            CommittedIdentity = _model.SelectedRow?.Identity;
        }

        /// <summary>The stable identity currently committed in the model.</summary>
        public string? CommittedIdentity { get; private set; }

        /// <summary>The committed identity captured when the current open session began.</summary>
        public string? OriginalIdentity { get; private set; }

        /// <summary>The stable identity active in the open selector.</summary>
        public string? PendingIdentity { get; private set; }

        /// <summary>True while an open selection session is active.</summary>
        public bool IsOpen { get; private set; }

        /// <summary>Synchronizes committed identity after an external model selection.</summary>
        public void SynchronizeCommittedSelection()
        {
            if (!IsOpen)
            {
                CommittedIdentity = _model.SelectedRow?.Identity;
            }
        }

        /// <summary>Reconciles session identities against an atomically replaced row snapshot.</summary>
        public void ReconcileRowsReplaced()
        {
            int retainedIndex = _model.SelectedIndex;
            int committedIndex = IndexOfSelectable(CommittedIdentity);
            if (
                committedIndex < 0
                && retainedIndex >= 0
                && retainedIndex < _model.Rows.Count
                && _model.Rows[retainedIndex].IsSelectable
            )
            {
                committedIndex = retainedIndex;
            }
            _model.SelectRow(committedIndex);
            CommittedIdentity = committedIndex < 0 ? null : _model.Rows[committedIndex].Identity;

            if (!IsOpen)
            {
                return;
            }
            if (IndexOfSelectable(OriginalIdentity) < 0)
            {
                OriginalIdentity = CommittedIdentity;
            }
            if (IndexOfSelectable(PendingIdentity) < 0)
            {
                PendingIdentity = CommittedIdentity;
            }
        }

        /// <summary>Captures immutable selector state from the current model.</summary>
        public BreadcrumbSelectorState Snapshot()
        {
            var options = new BreadcrumbSelectorOptionState[_model.Rows.Count];
            for (int index = 0; index < options.Length; index++)
            {
                BreadcrumbStateRow row = _model.Rows[index];
                options[index] = new BreadcrumbSelectorOptionState(row.Identity, row.IsSelectable);
            }
            return new BreadcrumbSelectorState(IsOpen, CommittedIdentity, PendingIdentity, options);
        }

        public BreadcrumbSelectionEffects ClearSelector()
        {
            bool wasOpen = IsOpen;
            Cancel();
            _model.Clear();
            SynchronizeCommittedSelection();
            return BreadcrumbSelectionEffects.Handled
                | BreadcrumbSelectionEffects.RenderRequired
                | (
                    wasOpen
                        ? BreadcrumbSelectionEffects.OpenStateChanged
                        : BreadcrumbSelectionEffects.None
                );
        }

        public BreadcrumbSelectionEffects SelectRow(int index)
        {
            _model.SelectRow(index);
            SynchronizeCommittedSelection();
            return BreadcrumbSelectionEffects.Handled
                | BreadcrumbSelectionEffects.SelectionChanged
                | BreadcrumbSelectionEffects.RenderRequired;
        }

        public BreadcrumbSelectionEffects SelectItem(string item)
        {
            if (!BreadcrumbSelectionMap.TrySelectItem(_model, item))
            {
                return BreadcrumbSelectionEffects.None;
            }
            SynchronizeCommittedSelection();
            return BreadcrumbSelectionEffects.Handled
                | BreadcrumbSelectionEffects.SelectionChanged
                | BreadcrumbSelectionEffects.RenderRequired;
        }

        public BreadcrumbSelectionEffects OpenSelector()
        {
            return Open()
                ? BreadcrumbSelectionEffects.Handled | BreadcrumbSelectionEffects.OpenStateChanged
                : BreadcrumbSelectionEffects.None;
        }

        public BreadcrumbSelectionEffects MoveSelector(bool previous)
        {
            bool wasOpen = IsOpen;
            bool moved = previous ? MovePrevious() : MoveNext();
            if (!moved)
            {
                return BreadcrumbSelectionEffects.None;
            }
            return BreadcrumbSelectionEffects.Handled
                | (
                    wasOpen
                        ? BreadcrumbSelectionEffects.None
                        : BreadcrumbSelectionEffects.SelectionChanged
                            | BreadcrumbSelectionEffects.RenderRequired
                );
        }

        public BreadcrumbSelectionEffects CommitSelector()
        {
            if (!IsOpen)
            {
                return BreadcrumbSelectionEffects.None;
            }
            bool changed = CommitPending();
            return BreadcrumbSelectionEffects.Handled
                | BreadcrumbSelectionEffects.OpenStateChanged
                | BreadcrumbSelectionEffects.RenderRequired
                | (
                    changed
                        ? BreadcrumbSelectionEffects.SelectionChanged
                        : BreadcrumbSelectionEffects.None
                );
        }

        public BreadcrumbSelectionEffects ActivateSelector(string identity)
        {
            bool wasOpen = IsOpen;
            bool changed = Activate(identity);
            bool closed = wasOpen && !IsOpen;
            if (!changed && !closed)
            {
                return BreadcrumbSelectionEffects.None;
            }
            return BreadcrumbSelectionEffects.Handled
                | BreadcrumbSelectionEffects.RenderRequired
                | (
                    changed
                        ? BreadcrumbSelectionEffects.SelectionChanged
                        : BreadcrumbSelectionEffects.None
                )
                | (
                    closed
                        ? BreadcrumbSelectionEffects.OpenStateChanged
                        : BreadcrumbSelectionEffects.None
                );
        }

        /// <summary>Commits an expanded subfolder and ends the current selector session.</summary>
        /// <param name="rowIdentity">The unique stable identity of the containing row.</param>
        /// <param name="subfolderIndex">The zero-based expanded subfolder index.</param>
        /// <returns>
        /// The complete explicit-commit effects, or <see cref="BreadcrumbSelectionEffects.None"/>
        /// without mutation when the session, row, or index is invalid.
        /// </returns>
        public BreadcrumbSelectionEffects ActivateSubfolder(string rowIdentity, int subfolderIndex)
        {
            if (!IsOpen || subfolderIndex < 0)
            {
                return BreadcrumbSelectionEffects.None;
            }

            int rowIndex = IndexOfSelectable(rowIdentity);
            if (rowIndex < 0)
            {
                return BreadcrumbSelectionEffects.None;
            }

            BreadcrumbStateRow row = _model.Rows[rowIndex];
            if (!row.IsSuggestion || !row.LeafExpanded || subfolderIndex >= row.Subfolders.Count)
            {
                return BreadcrumbSelectionEffects.None;
            }

            _model.SelectRow(rowIndex);
            _model.SelectSubfolder(subfolderIndex);
            CommittedIdentity = row.Identity;
            EndOpenSession();
            return BreadcrumbSelectionEffects.Handled
                | BreadcrumbSelectionEffects.SelectionChanged
                | BreadcrumbSelectionEffects.OpenStateChanged
                | BreadcrumbSelectionEffects.RenderRequired;
        }

        public BreadcrumbSelectionEffects CancelSelector()
        {
            return Cancel()
                ? BreadcrumbSelectionEffects.Handled
                    | BreadcrumbSelectionEffects.OpenStateChanged
                    | BreadcrumbSelectionEffects.RenderRequired
                : BreadcrumbSelectionEffects.None;
        }

        /// <summary>Snapshots committed selection and starts an open session.</summary>
        public bool Open()
        {
            if (IsOpen || FirstSelectableIndex() < 0)
            {
                return false;
            }

            SynchronizeCommittedSelection();
            OriginalIdentity = CommittedIdentity;
            PendingIdentity = CommittedIdentity;
            IsOpen = true;
            return true;
        }

        /// <summary>Moves to the previous selectable row without wrapping.</summary>
        public bool MovePrevious() => Move(-1);

        /// <summary>Moves to the next selectable row without wrapping.</summary>
        public bool MoveNext() => Move(1);

        /// <summary>Commits the pending row and closes the open session.</summary>
        public bool CommitPending()
        {
            if (!IsOpen)
            {
                return false;
            }

            string? pending = PendingIdentity;
            bool changed = false;
            int index = IndexOfSelectable(pending);
            if (index >= 0)
            {
                changed = !string.Equals(
                    CommittedIdentity,
                    _model.Rows[index].Identity,
                    StringComparison.Ordinal
                );
                _model.SelectRow(index);
                CommittedIdentity = _model.Rows[index].Identity;
            }
            EndOpenSession();
            return changed;
        }

        /// <summary>Activates a stable selectable identity, committing it immediately.</summary>
        public bool Activate(string identity)
        {
            int index = IndexOfSelectable(identity);
            if (index < 0)
            {
                return false;
            }

            if (IsOpen)
            {
                PendingIdentity = _model.Rows[index].Identity;
                return CommitPending();
            }

            bool changed = !string.Equals(
                CommittedIdentity,
                _model.Rows[index].Identity,
                StringComparison.Ordinal
            );
            _model.SelectRow(index);
            CommittedIdentity = _model.Rows[index].Identity;
            return changed;
        }

        /// <summary>Restores the opening selection and closes without committing pending state.</summary>
        public bool Cancel()
        {
            if (!IsOpen)
            {
                return false;
            }

            int originalIndex = IndexOfSelectable(OriginalIdentity);
            if (originalIndex >= 0)
            {
                _model.SelectRow(originalIndex);
                CommittedIdentity = _model.Rows[originalIndex].Identity;
            }
            else if (OriginalIdentity == null)
            {
                _model.SelectRow(-1);
                CommittedIdentity = null;
            }
            EndOpenSession();
            return true;
        }

        private bool Move(int step)
        {
            string? identity = IsOpen ? PendingIdentity : CommittedIdentity;
            int current = IndexOfSelectable(identity);
            int candidate;
            if (current < 0)
            {
                candidate = step > 0 ? FirstSelectableIndex() : LastSelectableIndex();
            }
            else
            {
                candidate = NextSelectableIndex(current, step);
            }
            if (candidate < 0)
            {
                return false;
            }

            string candidateIdentity = _model.Rows[candidate].Identity;
            if (IsOpen)
            {
                PendingIdentity = candidateIdentity;
            }
            else
            {
                _model.SelectRow(candidate);
                CommittedIdentity = candidateIdentity;
            }
            return true;
        }

        private int FirstSelectableIndex() => NextSelectableIndex(-1, 1);

        private int LastSelectableIndex() => NextSelectableIndex(_model.Rows.Count, -1);

        private int NextSelectableIndex(int start, int step)
        {
            for (int index = start + step; index >= 0 && index < _model.Rows.Count; index += step)
            {
                if (_model.Rows[index].IsSelectable)
                {
                    return index;
                }
            }
            return -1;
        }

        private int IndexOfSelectable(string? identity)
        {
            if (string.IsNullOrWhiteSpace(identity))
            {
                return -1;
            }
            for (int index = 0; index < _model.Rows.Count; index++)
            {
                var row = _model.Rows[index];
                if (
                    row.IsSelectable
                    && string.Equals(row.Identity, identity, StringComparison.Ordinal)
                )
                {
                    return index;
                }
            }
            return -1;
        }

        private void EndOpenSession()
        {
            IsOpen = false;
            OriginalIdentity = null;
            PendingIdentity = null;
        }
    }
}
