#nullable enable
using System;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Host-neutral selector session over stable row identities. Closed navigation commits directly;
    /// open navigation changes only pending state until commit or cancellation.
    /// </summary>
    public sealed class BreadcrumbSelectionSession
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
