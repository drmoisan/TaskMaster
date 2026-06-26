using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Windows_Forms;

namespace TaskMaster
{
    /// <summary>
    /// Junk-folder partial of <see cref="AppOlObjects"/> (Issue #207, AC8 file-size relief). This
    /// cohesive region — the junk-potential / junk-certain backing fields, properties, settings
    /// read/write helpers, apply/refresh, and lazy loaders — was extracted from
    /// <c>AppOlObjects.cs</c> to bring that file under the 500-line cap. Behavior is unchanged
    /// (move-only).
    /// </summary>
    public partial class AppOlObjects
    {
        private Folder _junkPotential;
        public Folder JunkPotential => Initializer.GetOrLoad(ref _junkPotential, LoadJunkPotential);

        internal static string ReadJunkCertainSetting() =>
            Properties.Settings.Default.OlJunkCertain;

        internal static void WriteJunkCertainSetting(string relativePath) =>
            Properties.Settings.Default.OlJunkCertain = relativePath;

        internal static string ReadJunkPotentialSetting() =>
            Properties.Settings.Default.JunkPotential;

        internal static void WriteJunkPotentialSetting(string relativePath) =>
            Properties.Settings.Default.JunkPotential = relativePath;

        internal void ApplyJunkFolderSelections(
            string junkCertainRelativePath,
            string junkPotentialRelativePath
        )
        {
            WriteJunkCertainSetting(junkCertainRelativePath);
            WriteJunkPotentialSetting(junkPotentialRelativePath);
            Properties.Settings.Default.Save();
            RefreshJunkFolderSelections();
        }

        internal void RefreshJunkFolderSelections()
        {
            _junkCertain = null;
            _junkPotential = null;
            _ = JunkCertain;
            _ = JunkPotential;
        }

        internal Folder LoadJunkPotential()
        {
            var folderPath = ReadJunkPotentialSetting();
            if (folderPath.IsNullOrEmpty())
            {
                return null;
            }

            // Issue #211 (AC10): resolve the configured path by DIRECT navigation over the live
            // store root instead of `new FolderTree(Root)`, which recursively enumerated the entire
            // default-store hierarchy on the STA before searching (~50s cold-start stall). The
            // adapter exposes only one folder level at a time, so resolution touches only the
            // folders along the path plus the first-segment breadth-first frontier. The matching
            // semantics are identical to the prior FindSequentialNode comparator.
            var node = JunkFolderPathNavigator.ResolvePath(new OutlookFolderNode(Root), folderPath);
            var folder = (node as OutlookFolderNode)?.OlFolder as Folder;
            if (folder is null)
            {
                MyBox.ShowDialog(
                    "Junk Potential Folder not found. Please select it manually.",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );
                folder = NamespaceMAPI.PickFolder() as Folder;
                if (folder is null)
                {
                    return null;
                }
                var wrapper = new FolderWrapper(folder, Root);
                WriteJunkPotentialSetting(wrapper.RelativePath);
                Properties.Settings.Default.Save();
            }
            return folder;
        }

        private Folder _junkCertain;

        //public Folder JunkCertain
        //{
        //    get
        //    {
        //        if (_junkCertain is null)
        //        {
        //            _junkCertain = (Folder)App.Session.DefaultStore.GetDefaultFolder(OlDefaultFolders.olFolderJunk);
        //        }
        //        return _junkCertain;
        //    }
        //}
        public Folder JunkCertain => Initializer.GetOrLoad(ref _junkCertain, LoadJunkCertain);

        internal Folder LoadJunkCertain()
        {
            var folderPath = ReadJunkCertainSetting();
            if (folderPath.IsNullOrEmpty())
            {
                return null;
            }

            // Issue #211 (AC10): resolve the configured path by DIRECT navigation over the live
            // store root instead of `new FolderTree(Root)`, which recursively enumerated the entire
            // default-store hierarchy on the STA before searching (~50s cold-start stall). The
            // adapter exposes only one folder level at a time, so resolution touches only the
            // folders along the path plus the first-segment breadth-first frontier. The matching
            // semantics are identical to the prior FindSequentialNode comparator.
            var node = JunkFolderPathNavigator.ResolvePath(new OutlookFolderNode(Root), folderPath);
            var folder = (node as OutlookFolderNode)?.OlFolder as Folder;
            if (folder is null)
            {
                MyBox.ShowDialog(
                    "Junk Folder not found. Please select it manually.",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );
                folder = NamespaceMAPI.PickFolder() as Folder;
                if (folder is null)
                {
                    return null;
                }
                var wrapper = new FolderWrapper(folder, Root);
                WriteJunkCertainSetting(wrapper.RelativePath);
                Properties.Settings.Default.Save();
            }
            return folder;
        }

        /// <summary>
        /// Thin COM adapter wrapping a live <see cref="MAPIFolder"/> as an <see cref="IFolderNode"/>
        /// for <see cref="JunkFolderPathNavigator"/> (issue #211, AC10). <see cref="Name"/> reads
        /// <c>MAPIFolder.Name</c>; <see cref="ChildFolders"/> lazily enumerates ONLY this folder's
        /// direct <c>Folders</c> on first access — no recursion and no eager full-tree walk — so the
        /// navigator touches only the folders along the resolution path plus the first-segment
        /// breadth-first frontier. Decorated <see cref="ExcludeFromCodeCoverageAttribute"/> because
        /// it is a direct COM wrapper with no testable logic; the navigation logic it feeds is fully
        /// covered by JunkFolderPathNavigatorTests against the pure helper.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private sealed class OutlookFolderNode : IFolderNode
        {
            private readonly MAPIFolder _olFolder;
            private IReadOnlyList<IFolderNode> _childFolders;

            public OutlookFolderNode(MAPIFolder olFolder) => _olFolder = olFolder;

            public MAPIFolder OlFolder => _olFolder;

            public string Name => _olFolder?.Name;

            public IReadOnlyList<IFolderNode> ChildFolders
            {
                get
                {
                    if (_childFolders is null)
                    {
                        var children = new List<IFolderNode>();
                        var subFolders = _olFolder?.Folders;
                        if (subFolders is not null)
                        {
                            foreach (MAPIFolder child in subFolders)
                            {
                                children.Add(new OutlookFolderNode(child));
                            }
                        }
                        _childFolders = children;
                    }
                    return _childFolders;
                }
            }
        }
    }
}
