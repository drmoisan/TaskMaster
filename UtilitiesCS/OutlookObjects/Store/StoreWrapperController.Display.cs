#nullable enable
using System;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Display partial of <see cref="StoreWrapperController"/> (issue #797, D4 file-size relief).
    /// The rendering members — <see cref="PopulateWithCurrent"/>,
    /// <see cref="BindExcludeStoreCheckbox"/> and <see cref="GetRelativeFsPath"/> — were relocated
    /// verbatim out of <c>StoreWrapperController.cs</c>, which stood at 478 lines against the
    /// 500-line cap while four acceptance criteria landed in it. Follows the documented
    /// <c>AppOlObjects.JunkFolders.cs</c> partial precedent.
    /// </summary>
    public partial class StoreWrapperController
    {
        internal void PopulateWithCurrent()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => PopulateWithCurrent());
                return;
            }

            // Mirror the current store into the controller before rendering labels.
            // why: issue #797 AC8. The store selection is produced by a list search that returns
            // null when no store matches, and these four dereferences were unguarded while the very
            // next block already used the null-conditional form, so a null selection threw before
            // any placeholder could render. The four are now null-conditional, matching that block.
            ArchiveOutlook = Current?.ArchiveRoot;
            ArchiveFS = Current?.ArchiveFsRoot;
            JunkEmail = Current?.JunkCertain;
            JunkPotential = Current?.JunkPotential;

            // why: issue #797 AC6. The SMTP lookup runs once per store initialisation and is never
            // retried, and a successful result is not persisted, so one transient COM failure at
            // startup left the label showing a generic placeholder for the rest of the session.
            // Retry here, but only when the address is null, which bounds the added UI-thread
            // latency to the single lookup startup already performs.
            // why: issue #812, rescoped by issue #823. PopulateWithCurrent runs on every store
            // re-selection, not only on dialog open, and a failed lookup leaves the address null,
            // so the #797 guard alone re-attempted the COM lookup on every pass. The set below
            // bounds the retry to at most one attempt per controller instance per store, keyed by
            // reference identity, so a controller shown several failing stores still attempts each
            // of them once. That equals one attempt per store per dialog open only because
            // RibbonController.FolderStoresSettings constructs a fresh controller per open; it is
            // not a property of PopulateWithCurrent itself.
            // why: the Add precedes the lookup so that an exception escaping
            // RefreshUserEmailAddress still consumes that store's single attempt; recording it
            // afterwards would leave a throwing store retried on every pass.
            // Every dereference on this path is null-conditional, so a null current store cannot
            // throw here.
            if (
                Current is not null
                && Current.UserEmailAddress is null
                && !_userEmailRetryAttemptedStores.Contains(Current)
            )
            {
                _userEmailRetryAttemptedStores.Add(Current);
                Current.RefreshUserEmailAddress();
            }

            // Populate Form
            Viewer.Inbox.Text = TrimStorePrefix(Current?.Inbox?.FolderPath) ?? "Error Loading";
            Viewer.RootFolder.Text =
                TrimStorePrefix(Current?.RootFolder?.FolderPath) ?? "Error Loading";
            Viewer.UserEmail.Text = Current?.UserEmailAddress ?? BuildUserEmailUnavailableText();
            Viewer.ArchiveOutlook.Text = ArchiveOutlook?.RelativePath ?? "Please select an archive";
            Viewer.ArchiveFS.Text = GetRelativeFsPath();
            //if (Current.ArchiveFsRoot is not null && !Current.ArchiveFsRoot.FolderPath.IsNullOrEmpty())
            //{
            //    var (specialFolder, relativePath) = FsConverter(Current.ArchiveFsRoot.FolderPath);
            //    if (specialFolder.IsNullOrEmpty() & relativePath.IsNullOrEmpty())
            //    {
            //        Viewer.ArchiveFS.Text = "Please select an archive";
            //    }
            //    else
            //    {
            //        Viewer.ArchiveFS.Text = $"{string.Join(" -> ", [specialFolder,relativePath]).Trim()}";
            //    }
            //}
            Viewer.JunkEmail.Text = JunkEmail?.RelativePath ?? "Please select a folder";
            Viewer.JunkPotential.Text = JunkPotential?.RelativePath ?? "Please select a folder";
            BindExcludeStoreCheckbox();
        }

        /// <summary>
        /// Builds the text shown in place of the mailbox SMTP address when no source yielded one
        /// (issue #797, AC6). The message is specific to this failure and, when a reason was
        /// captured, names it, replacing the generic placeholder the label shared with the Inbox and
        /// Root Folder labels.
        /// </summary>
        private string BuildUserEmailUnavailableText()
        {
            var reason = Current?.LastSmtpLookupError;
            if (string.IsNullOrEmpty(reason))
            {
                return "Email address unavailable";
            }

            return $"Email address unavailable: {reason}";
        }

        /// <summary>
        /// Binds the <c>ExcludeStore</c> checkbox to the current store's membership in
        /// <c>Model.ExcludedStoreIds</c> (issue #328, OrdinalIgnoreCase). When the current store's
        /// StoreID is unreadable the checkbox is disabled and cleared (fail-safe per AC10) so it can
        /// neither mislead the user nor mutate the exclusion set.
        /// </summary>
        internal void BindExcludeStoreCheckbox()
        {
            // Defensive: a viewer that does not expose the checkbox (e.g., a partial test double)
            // has nothing to bind. Production viewers always supply it.
            var excludeStore = Viewer?.ExcludeStore;
            if (excludeStore is null)
            {
                return;
            }

            var storeId = Current?.StoreId;
            if (string.IsNullOrWhiteSpace(storeId))
            {
                excludeStore.Enabled = false;
                excludeStore.Checked = false;
                return;
            }

            excludeStore.Enabled = true;
            excludeStore.Checked =
                Model?.ExcludedStoreIds?.Any(id =>
                    string.Equals(id, storeId, StringComparison.OrdinalIgnoreCase)
                )
                ?? false;
        }

        internal string GetRelativeFsPath()
        {
            // why: issue #797 AC8. This dereference of the current store was unguarded and threw
            // when no store matched the selection, on a path the populate method calls, so the
            // placeholder could never render. Null-conditional here returns the same placeholder
            // the method already returns for an unset archive root.
            if (
                Current?.ArchiveFsRoot is not null
                && !Current.ArchiveFsRoot.FolderPath.IsNullOrEmpty()
            )
            {
                var (specialFolder, relativePath) = FsConverter(Current.ArchiveFsRoot.FolderPath);

                // why: issue #797. Readability and consistency only. Both operands call a
                // null-tolerant string extension and neither has a side effect, so replacing the
                // single ampersand with the short-circuit form is behaviourally inert. It does not
                // repair a fault.
                if (specialFolder.IsNullOrEmpty() && relativePath.IsNullOrEmpty())
                {
                    return "Please select an archive";
                }
                else
                {
                    return $"{string.Join(" -> ", [specialFolder, relativePath]).Trim()}";
                }
            }
            return "Please select an archive";
        }

        /// <summary>
        /// Removes the leading store-prefix backslash pair from an Outlook folder path so the
        /// settings dialog can render the path without it (issue #797, AC7). Every other input,
        /// including a null reference and the empty string, is returned unchanged.
        /// </summary>
        /// <param name="folderPath">The folder path to trim. May be null.</param>
        /// <returns>The path without its leading store prefix, or the input unchanged.</returns>
        internal static string? TrimStorePrefix(string? folderPath)
        {
            // why: issue #797 AC7. MAPIFolder.FolderPath is Outlook's native form and carries a
            // leading pair of backslash characters naming the store, which is read straight into
            // the label. This is a display-only trim, so every other input is returned unchanged,
            // including a null reference and the empty string. Declared internal rather than
            // private so the pure-function cases are reachable from UtilitiesCS.Test, to which this
            // assembly already grants InternalsVisibleTo; internal does not widen the public
            // surface of the controller.
            if (folderPath is null || !folderPath.StartsWith(@"\\", StringComparison.Ordinal))
            {
                return folderPath;
            }

            return folderPath.Substring(2);
        }
    }
}
