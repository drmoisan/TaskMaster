#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net.Repository.Hierarchy;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Describes whether the store-wrapper model is ready for
    /// <see cref="StoreWrapperController.Launch"/> to open the settings dialog.
    /// </summary>
    internal enum StoreLaunchReadinessState
    {
        Ready,
        ModelUnavailable,
        StoresUnavailable,
    }

    /// <summary>
    /// Result of <see cref="StoreWrapperController.EvaluateLaunchReadiness"/>: the readiness
    /// state plus, when ready, the model and store display names needed to populate the
    /// settings dialog.
    /// </summary>
    internal readonly struct StoreLaunchReadiness
    {
        private StoreLaunchReadiness(
            StoreLaunchReadinessState state,
            StoresWrapper model,
            IList<string?> displayNames
        )
        {
            State = state;
            Model = model;
            DisplayNames = displayNames;
        }

        internal StoreLaunchReadinessState State { get; }

        internal StoresWrapper Model { get; }

        internal IList<string?> DisplayNames { get; }

        internal static StoreLaunchReadiness NotReady(StoreLaunchReadinessState state)
        {
            // why: this project has no #nullable annotation context, so Model/DisplayNames
            // are declared non-nullable; the "not ready" sentinel legitimately has neither.
            // Suppress narrowly rather than adding '?' annotations, which would produce new
            // CS8632 warnings on this file during normal (non-forced-nullable) builds.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return new(state, null, null);
#pragma warning restore CS8625
        }

        internal static StoreLaunchReadiness Ready(
            StoresWrapper model,
            IList<string?> displayNames
        ) => new(StoreLaunchReadinessState.Ready, model, displayNames);
    }

    public class StoreWrapperController
    {
        internal static bool RunFolderSelectionDialog(Func<bool> selector)
        {
            return selector?.Invoke() ?? false;
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public StoreWrapperController(IApplicationGlobals globals)
        {
            Globals = globals;
        }

        internal IApplicationGlobals Globals { get; set; }

        public IStoreWrapperViewer Viewer { get; internal set; } = null!;

        public StoresWrapper Model { get; internal set; } = null!;

        public StoreWrapper Current { get; internal set; } = null!;

        internal FolderMinimalWrapper? ArchiveOutlook { get; set; }
        internal FilePathHelper? ArchiveFS { get; set; }
        internal FolderMinimalWrapper? JunkEmail { get; set; }
        internal FolderMinimalWrapper? JunkPotential { get; set; }
        internal Func<string, (string, string)> FsConverter { get; set; } = null!;

        /// <summary>
        /// Determines whether the store-wrapper model has finished loading and is safe to
        /// bind into the settings dialog. Addresses issue #240: <c>Globals.Ol.StoresWrapper</c>
        /// is populated asynchronously during startup and can be null (load not yet complete),
        /// or non-null with a transiently null <c>Stores</c> list (post-deserialize, before the
        /// async rewire populates it). Callers must not dereference the model until this
        /// reports <see cref="StoreLaunchReadinessState.Ready"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="StoreLaunchReadiness"/> describing the readiness state and, when ready,
        /// the model and the display names of every store it contains.
        /// </returns>
        internal StoreLaunchReadiness EvaluateLaunchReadiness()
        {
            return StoreLaunchReadinessEvaluator.Evaluate(Globals);
        }

        #region Events

        [ExcludeFromCodeCoverage]
        public void Launch()
        {
            var readiness = EvaluateLaunchReadiness();
            if (readiness.State != StoreLaunchReadinessState.Ready)
            {
                MyBox.ShowDialog(
                    "Store settings are not available yet. Please try again after startup completes.",
                    "Store Settings Unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            FsConverter = new FilePathHelperConverter(Globals.FS).GetSerializablePath;
            Model = readiness.Model;
            Viewer = new StoreWrapperViewer(this);
            Viewer.DisplayName.DataSource = readiness.DisplayNames;

            Viewer.ShowDialog();
        }

        public void ButtonOk_Click()
        {
            if (AnyChanges())
            {
                SaveChanges();
            }
            Viewer.Close();
        }

        public void ButtonCancel_Click()
        {
            Viewer.Close();
        }

        public void DisplayName_SelectedValueChanged(object? sender, EventArgs e)
        {
            if (AnyChanges())
            {
                var response = MyBox.ShowDialog(
                    "Save changes?",
                    "Save Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (response == DialogResult.Yes)
                {
                    SaveChanges();
                }
            }
            var displayName = Viewer.DisplayName.SelectedValue?.ToString();
            Current = Model.Stores!.Find(store => store.DisplayName == displayName);
            PopulateWithCurrent();
        }

        /// <summary>
        /// Thin event hook for the <c>ExcludeStore</c> checkbox (issue #328). The exclusion set is
        /// mutated and persisted in the save path (<see cref="AnyChanges"/>/<see cref="SaveChanges"/>)
        /// based on the checkbox's current state, so no business logic lives in this forwarder.
        /// </summary>
        public void ExcludeStore_CheckedChanged(object? sender, EventArgs e) { }

        public void ArchiveFS_Click()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => ArchiveFS_Click());
                return;
            }
            var folderPath = SelectFsFolder();
            if (folderPath.IsNullOrEmpty())
            {
                return;
            }
            else
            {
                ArchiveFS!.FolderPath = folderPath!;
                Viewer.ArchiveFS.Text = GetRelativeFsPath();
            }
        }

        public void ArchiveOutlook_Click()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => ArchiveOutlook_Click());
                return;
            }
            ArchiveOutlook = SelectFolder();
            Viewer.ArchiveOutlook.Text = ArchiveOutlook?.RelativePath;
        }

        public void JunkEmail_Click()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => JunkEmail_Click());
                return;
            }
            JunkEmail = SelectFolder();
            Viewer.JunkEmail.Text = JunkEmail?.RelativePath;
        }

        public void JunkPotential_Click()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => JunkPotential_Click());
                return;
            }
            JunkPotential = SelectFolder();
            Viewer.JunkPotential.Text = JunkPotential?.RelativePath;
        }

        #endregion Events

        #region Methods

        internal bool AnyChanges()
        {
            return !PairwiseEquals(ArchiveOutlook, Current?.ArchiveRoot)
                || !PairwiseEquals(JunkEmail, Current?.JunkCertain)
                || !PairwiseEquals(JunkPotential, Current?.JunkPotential)
                || !PairwiseEquals(ArchiveFS, Current?.ArchiveFsRoot)
                || ExcludeStoreSelectionChanged();
        }

        /// <summary>
        /// Reports whether the <c>ExcludeStore</c> checkbox state differs from the current store's
        /// membership in <c>Model.ExcludedStoreIds</c> (issue #328). Returns false when the StoreID is
        /// unreadable, so an unreadable store can never register as a pending change (fail-safe).
        /// </summary>
        internal bool ExcludeStoreSelectionChanged()
        {
            var storeId = Current?.StoreId;
            if (string.IsNullOrWhiteSpace(storeId))
            {
                return false;
            }

            var currentlyExcluded =
                Model?.ExcludedStoreIds?.Any(id =>
                    string.Equals(id, storeId, StringComparison.OrdinalIgnoreCase)
                )
                ?? false;
            return currentlyExcluded != Viewer.ExcludeStore.Checked;
        }

        internal bool PairwiseEquals<T>(T a, T b)
        {
            if (a is null && b is null)
            {
                return true;
            }
            if (a is null || b is null)
            {
                return false;
            }
            return a.Equals(b);
        }

        internal void PopulateWithCurrent()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => PopulateWithCurrent());
                return;
            }

            // Mirror the current store into the controller before rendering labels.
            ArchiveOutlook = Current.ArchiveRoot;
            ArchiveFS = Current.ArchiveFsRoot;
            JunkEmail = Current.JunkCertain;
            JunkPotential = Current.JunkPotential;

            // Populate Form
            Viewer.Inbox.Text = Current?.Inbox?.FolderPath ?? "Error Loading";
            Viewer.RootFolder.Text = Current?.RootFolder?.FolderPath ?? "Error Loading";
            Viewer.UserEmail.Text = Current?.UserEmailAddress ?? "Error Loading";
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

        internal void SaveChanges()
        {
            Current.ArchiveRoot = ArchiveOutlook;
            Current.JunkCertain = JunkEmail;
            Current.JunkPotential = JunkPotential;
            Current.ArchiveFsRoot = ArchiveFS;
            PersistJunkFolderSelections();
            ApplyExcludeStoreSelection();
            Model.Serialize();
        }

        /// <summary>
        /// Applies the <c>ExcludeStore</c> checkbox state to <c>Model.ExcludedStoreIds</c> (issue #328):
        /// adds the current store's StoreID when checked and removes it when unchecked, guarded by an
        /// OrdinalIgnoreCase membership check for idempotency (no duplicate add, no remove when absent).
        /// A store with an unreadable StoreID is never mutated (fail-safe per AC10).
        /// </summary>
        internal void ApplyExcludeStoreSelection()
        {
            var storeId = Current?.StoreId;
            if (string.IsNullOrWhiteSpace(storeId))
            {
                return;
            }

            Model.ExcludedStoreIds ??= new List<string>();
            var existing = Model.ExcludedStoreIds.FirstOrDefault(id =>
                string.Equals(id, storeId, StringComparison.OrdinalIgnoreCase)
            );

            if (Viewer.ExcludeStore.Checked)
            {
                if (existing is null)
                {
                    Model.ExcludedStoreIds.Add(storeId!);
                }
            }
            else if (existing is not null)
            {
                Model.ExcludedStoreIds.Remove(existing);
            }
        }

        internal void PersistJunkFolderSelections()
        {
            var olObjects = Globals?.Ol;
            if (olObjects is null)
            {
                return;
            }

            var applyMethod = olObjects
                .GetType()
                .GetMethod(
                    "ApplyJunkFolderSelections",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    [typeof(string), typeof(string)],
                    null
                );

            if (applyMethod is null)
            {
                logger.Warn(
                    "Unable to persist junk-folder selections because the Outlook globals implementation does not expose ApplyJunkFolderSelections."
                );
                return;
            }

            applyMethod.Invoke(olObjects, [JunkEmail?.RelativePath, JunkPotential?.RelativePath]);
        }

        internal virtual FolderMinimalWrapper? SelectFolder()
        {
            try
            {
                var folder = Globals.Ol.NamespaceMAPI.PickFolder();
                if (folder is null)
                {
                    return null;
                }
                return new FolderMinimalWrapper(folder, Current.RootFolder!);
            }
            catch (Exception e)
            {
                logger.Error($"Error selecting folder. {e.Message}", e);
                return null;
            }
        }

        [ExcludeFromCodeCoverage]
        internal string? SelectFsFolder()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select a folder";
                folderBrowserDialog.ShowNewFolderButton = true;
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the path of the selected folder
                    return folderBrowserDialog.SelectedPath;
                }
            }
            return null;
        }

        internal string GetRelativeFsPath()
        {
            if (
                Current.ArchiveFsRoot is not null
                && !Current.ArchiveFsRoot.FolderPath.IsNullOrEmpty()
            )
            {
                var (specialFolder, relativePath) = FsConverter(Current.ArchiveFsRoot.FolderPath);
                if (specialFolder.IsNullOrEmpty() & relativePath.IsNullOrEmpty())
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

        #endregion Methods
    }
}
