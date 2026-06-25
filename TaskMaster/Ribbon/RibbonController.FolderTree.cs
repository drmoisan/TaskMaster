using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using TaskTree;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.OlFolderTools.FilterOlFolders;
using UtilitiesCS.OutlookObjects.Folder;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster
{
    public partial class RibbonController
    {
        internal void GetFolderInfo()
        {
            _ = GetFolderInfoAsync();
        }

        internal async Task GetFolderInfoAsync()
        {
            var currentFolder = Globals.Ol.App.ActiveExplorer().CurrentFolder;
            if (currentFolder is not null)
            {
                var snapshot = await GetFolderTreeSnapshotAsync(currentFolder)
                    .ConfigureAwait(false);
                FolderTreeCompatibilityView folderTreeView = new(
                    snapshot,
                    new(Array.Empty<string>())
                );
                var folderViewer = new FolderInfoViewer();
                folderViewer.SetFolderTreeView(folderTreeView);
                folderViewer.Show();
            }
        }

        internal void CompareFolders()
        {
            _ = CompareFoldersAsync();
        }

        internal async Task CompareFoldersAsync()
        {
            var folder1 = PromptUserToSelectFolder();
            if (folder1 is null)
                return;
            var folder2 = PromptUserToSelectFolder();
            if (folder2 is null)
                return;

            var snapshot1 = await GetFolderTreeSnapshotAsync(folder1).ConfigureAwait(false);
            var snapshot2 = await GetFolderTreeSnapshotAsync(folder2).ConfigureAwait(false);
            var (
                identicalNodes,
                identicalContents,
                sameUniqueName,
                onlyCurrentNodes,
                onlyOtherNodes
            ) = CompareFolderSnapshots(snapshot1, folder1, snapshot2, folder2);
            var identicalNodesStats = GetStats(identicalNodes);
            var identicalContentsStats = GetStats(identicalContents);
            var sameUniqueNameStats = GetStats(sameUniqueName);
            var onlyCurrentStats = GetStats(onlyCurrentNodes);
            var onlyOtherStats = GetStats(onlyOtherNodes);

            logger.Info(
                $"\nFolder Comparison Output for {folder1.Name} and {folder2.Name}"
                    + $"\nIdentical Nodes: {identicalNodes.Count:N0} Folder Size: {identicalNodesStats.size}  Item Count: {identicalNodesStats.count:N0}"
                    + $"\nIdentical Contents: {identicalContents.Count:N0}  Folder Size: {identicalContentsStats.size}  Item Count: {identicalContentsStats.count:N0}"
                    + $"\nSame Unique Name: {sameUniqueName.Count:N0}  Folder Size: {sameUniqueNameStats.size}  Item Count: {sameUniqueNameStats.count:N0}"
                    + $"\nOnly In Folder 1 ({folder1.Name}): {onlyCurrentNodes.Count:N0} Folder Size: {onlyCurrentStats.size}  Item Count: {onlyCurrentStats.count:N0}"
                    + $"\nOnly In Folder 2 ({folder2.Name}): {onlyOtherNodes.Count:N0} Folder Size: {onlyOtherStats.size}  Item Count: {onlyOtherStats.count:N0}"
            );

            if (onlyCurrentStats.count > 0)
            {
                logger.Info(
                    $"Folders only in Folder 1 ({folder1.Name}): \n{string.Join("\n", onlyCurrentNodes.Select(x => x.Value.RelativePath))}"
                );
            }
            if (onlyOtherStats.count > 0)
            {
                logger.Info(
                    $"Folders only in Folder 2 ({folder2.Name}): \n{string.Join("\n", onlyOtherNodes.Select(x => x.Value.RelativePath))}"
                );
            }

            if (onlyCurrentStats.count > 0 && onlyOtherStats.count > 0)
            {
                var response = MessageBox.Show(
                    $"Compare items in unique folders?",
                    "Question",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (response == DialogResult.Yes) { }
            }
        }

        internal void CompareItems() { }

        private async Task<FolderTreeSnapshot> GetFolderTreeSnapshotAsync(MAPIFolder folder)
        {
            var request = string.IsNullOrWhiteSpace(folder?.StoreID)
                ? FolderTreeRequest.AllStores(allowStaleSnapshot: true)
                : FolderTreeRequest.ForStore(folder.StoreID, allowStaleSnapshot: true);
            return await GetFolderTreeSnapshotAsync(request).ConfigureAwait(false);
        }

        internal async Task<FolderTreeSnapshot> GetFolderTreeSnapshotAsync(
            FolderTreeRequest request
        )
        {
            return await FolderTreeService
                .GetSnapshotAsync(request, CancellationToken.None)
                .ConfigureAwait(false);
        }

        private static (
            List<TreeNode<FolderWrapper>> nodes,
            List<TreeNode<FolderWrapper>> contents,
            List<TreeNode<FolderWrapper>> sameName,
            List<TreeNode<FolderWrapper>> currentOnly,
            List<TreeNode<FolderWrapper>> otherOnly
        ) CompareFolderSnapshots(
            FolderTreeSnapshot current,
            MAPIFolder currentFolder,
            FolderTreeSnapshot other,
            MAPIFolder otherFolder
        )
        {
            using var currentView = CreateViewForFolder(current, currentFolder);
            using var otherView = CreateViewForFolder(other, otherFolder);
            var currentNodes = currentView.Roots.SelectMany(root => root.FlattenNodes()).ToList();
            var otherNodes = otherView.Roots.SelectMany(root => root.FlattenNodes()).ToList();
            return CompareFolderNodes(currentNodes, otherNodes);
        }

        private static FolderTreeCompatibilityView CreateViewForFolder(
            FolderTreeSnapshot snapshot,
            MAPIFolder folder
        )
        {
            var rootNode = snapshot.FindByPath(folder.StoreID, folder.FolderPath);
            var scopedSnapshot = rootNode is null
                ? snapshot
                : FolderTreeSnapshotQueries.CreateSubtreeSnapshot(snapshot, rootNode);
            return new(scopedSnapshot, new(Array.Empty<string>()));
        }

        private static (
            List<TreeNode<FolderWrapper>> nodes,
            List<TreeNode<FolderWrapper>> contents,
            List<TreeNode<FolderWrapper>> sameName,
            List<TreeNode<FolderWrapper>> currentOnly,
            List<TreeNode<FolderWrapper>> otherOnly
        ) CompareFolderNodes(
            List<TreeNode<FolderWrapper>> currentNodes,
            List<TreeNode<FolderWrapper>> otherNodes
        )
        {
            var compareNodes = new FolderWrapperNodeComparer();
            var (nodes, onlyCurrentNodes, onlyOtherNodes) = CompareFolderNodeMembers(
                currentNodes,
                otherNodes,
                compareNodes
            );
            var compareContents = new FolderWrapperNodeContentsComparer();
            var (contents, onlyCurrentContents, onlyOtherContents) = CompareFolderNodeMembers(
                onlyCurrentNodes,
                onlyOtherNodes,
                compareContents
            );
            var compareNames = new FolderWrapperNameAndParentNameComparer();
            var currentContentsSplit = onlyCurrentContents.Split(compareNames);
            var otherContentsSplit = onlyOtherContents.Split(compareNames);
            var uniqueNameMatch = new List<TreeNode<FolderWrapper>>();
            if (currentContentsSplit.Unique.Count > 0 && otherContentsSplit.Unique.Count > 0)
            {
                uniqueNameMatch = currentContentsSplit
                    .Unique.Intersect(otherContentsSplit.Unique, compareNames)
                    .ToList();
                if (uniqueNameMatch.Count > 0)
                {
                    onlyCurrentContents = onlyCurrentContents
                        .Except(uniqueNameMatch, compareNames)
                        .ToList();
                    onlyOtherContents = onlyOtherContents
                        .Except(uniqueNameMatch, compareNames)
                        .ToList();
                }
            }

            return (nodes, contents, uniqueNameMatch, onlyCurrentContents, onlyOtherContents);
        }

        private static (
            List<TreeNode<FolderWrapper>> same,
            List<TreeNode<FolderWrapper>> onlyCurrent,
            List<TreeNode<FolderWrapper>> onlyOther
        ) CompareFolderNodeMembers(
            List<TreeNode<FolderWrapper>> current,
            List<TreeNode<FolderWrapper>> other,
            IEqualityComparer<TreeNode<FolderWrapper>> comparer
        )
        {
            var same = current.Intersect(other, comparer).ToList();
            var onlyCurrent = current.Except(other, comparer).ToList();
            var onlyOther = other.Except(current, comparer).ToList();
            return (same, onlyCurrent, onlyOther);
        }

        internal (string size, int count) GetStats(List<TreeNode<FolderWrapper>> nodes)
        {
            if (nodes is null || nodes.Count == 0)
                return ("0", 0);
            var sizeL = nodes.Sum(x => x.Value.FolderSize);
            var size = FormatFileSize(sizeL);
            var count = nodes.Sum(x => x.Value.ItemCount);
            return (size, count);
        }

        public static string FormatFileSize(long sizeInBytes)
        {
            string[] sizes = { "bytes", "KB", "MB", "GB", "TB" };
            double len = sizeInBytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.0} {sizes[order]} ({sizeInBytes:N0})";
        }

        internal Outlook.Folder PromptUserToSelectFolder()
        {
            // Ensure this runs on the UI thread
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );

            var outlookApp = Globals?.Ol?.App;
            if (outlookApp == null)
            {
                MessageBox.Show("Outlook application is not available.");
                return null;
            }

            Outlook.Folder selectedFolder = null;
            try
            {
                var ns = outlookApp.GetNamespace("MAPI");
                var folder = ns.PickFolder();
                selectedFolder = folder as Outlook.Folder;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error selecting folder: {ex.Message}");
            }

            return selectedFolder;
        }
    }
}
