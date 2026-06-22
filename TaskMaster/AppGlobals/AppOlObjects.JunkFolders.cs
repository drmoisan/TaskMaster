using System.Collections.Generic;
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
            var root = new FolderTree(Root).Roots.FirstOrDefault();
            var folderPath = ReadJunkPotentialSetting();
            if (folderPath.IsNullOrEmpty())
            {
                return null;
            }
            var sequence = new Queue<string>(folderPath.Split('\\'));

            var node = root.FindSequentialNode((current, other) => current.Name == other, sequence);
            var folder = node?.Value?.OlFolder as Folder;
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
            var root = new FolderTree(Root).Roots.FirstOrDefault();
            var folderPath = ReadJunkCertainSetting();
            if (folderPath.IsNullOrEmpty())
            {
                return null;
            }
            var sequence = new Queue<string>(folderPath.Split('\\'));

            var node = root.FindSequentialNode((current, other) => current.Name == other, sequence);
            var folder = node?.Value?.OlFolder as Folder;
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
    }
}
