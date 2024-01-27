using Microsoft.Office.Interop.Outlook;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UtilitiesCS.HelperClasses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class OlFolderTree: INotifyPropertyChanged
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public OlFolderTree() { }

        public OlFolderTree(MAPIFolder olRoot)
        {
            var root = RootFromFolder(olRoot);
            _roots = new List<TreeNode<OlFolderInfo>>() { root };
            WireNotifications();
        }

        public OlFolderTree(MAPIFolder olRoot, IList<string> selections)
        {
            var root = RootFromFolder(olRoot);
            root.Traverse(node => node.Selected = selections.Contains(node.RelativePath));
            _roots = new List<TreeNode<OlFolderInfo>>() { root };
            WireNotifications();
        }

        public OlFolderTree(MAPIFolder olRoot, IList<string> selections, ProgressTracker progress)
        {
            int folderCount = 0;
            var root = RootFromFolder(olRoot, progress.SpawnChild(95), ref folderCount);

            if (folderCount > 0)
            { 
                var traverseProgress = progress.SpawnChild(5);
                double increment = 100 / (double)folderCount;
                double rt = 0;

                root.Traverse(node => 
                { 
                    node.Selected = selections.Contains(node.RelativePath);
                    rt += increment;
                    traverseProgress.Report(rt);
                });
            }
            _roots = new List<TreeNode<OlFolderInfo>>() { root };
            WireNotifications();

            progress.Report(100);
        }

        private TreeNode<OlFolderInfo> RootFromFolder(MAPIFolder olRoot)
        {
            var info = new OlFolderInfo(olRoot, olRoot);
            var root = new TreeNode<OlFolderInfo>(info);
            this.InitializeChildren(root, olRoot);
            return root;
        }

        private TreeNode<OlFolderInfo> RootFromFolder(MAPIFolder olRoot, ProgressTracker progress, ref int runningCount)
        {
            var info = new OlFolderInfo(olRoot, olRoot);
            var root = new TreeNode<OlFolderInfo>(info);
            Interlocked.Increment(ref runningCount);

            if (olRoot.Folders.Count > 0)
            {
                this.InitializeChildren(root, olRoot, progress, ref runningCount);
            }
            else
            {
                progress.Report(100);
            }
            return root;
        }

        private List<TreeNode<OlFolderInfo>> _roots;
        public List<TreeNode<OlFolderInfo>> Roots { get => _roots; }

        private void InitializeChildren(TreeNode<OlFolderInfo> node, MAPIFolder olRoot)
        {
            node.Value.OlFolder.Folders.Cast<MAPIFolder>()
                .ForEach(child =>
                {
                    var childNode = node.AddChild(new OlFolderInfo(child, olRoot));
                    if (child.Folders.Count > 0)
                        InitializeChildren(childNode, olRoot);
                });
        }

        private void InitializeChildren(TreeNode<OlFolderInfo> node, MAPIFolder olRoot, ProgressTracker progress, ref int runningTotal)
        {
            var children = node.Value.OlFolder.Folders.Cast<MAPIFolder>().ToArray();
            var count = children.Count();
            
            if (count > 0)
            {
                double increment = 100 / (double)count;
                double rt = 0;
                foreach (var child in children) 
                {
                    var childNode = node.AddChild(new OlFolderInfo(child, olRoot));
                    Interlocked.Increment(ref runningTotal);
                    
                    rt += increment;
                    if (child.Folders.Count > 0)
                    {
                        var childProgress = progress.SpawnChild(increment);
                        InitializeChildren(childNode, olRoot, childProgress, ref runningTotal);
                    }
                    else
                    {
                        progress.Report(rt, $"Building Outlook Folder Tree ({runningTotal} completed)");
                        //progress.Increment(increment);
                    }
                }
                
                progress.Report(100);
            }                
        }

        public void Select(TreeNode<OlFolderInfo> node, bool includeDescendents)
        {
            if (includeDescendents)
            {
                node.Traverse(node => node.Selected = true);
            }
            
        }

        public List<TreeNode<OlFolderInfo>> FilterSelected(bool include)
        {
            var dummyRootOut = new TreeNode<OlFolderInfo>(_roots[0].Value);
            var dummyRootIn = new TreeNode<OlFolderInfo>(_roots[0].Value);
            dummyRootIn.Children = _roots;

            FilterChildren(dummyRootIn, dummyRootOut, include); 
            var selected = dummyRootOut.Children;
            return selected;
        }

        private void FilterChildren(TreeNode<OlFolderInfo> source, TreeNode<OlFolderInfo> destination, bool include)
        {
            foreach (var sourceChild in source.Children)
            {
                if (sourceChild.Value.Selected == include)
                {
                    var destinationChild = destination.AddChild(sourceChild.Value);
                    FilterChildren(sourceChild, destinationChild, include);
                }
                else
                {
                    FilterChildren(sourceChild, destination, include);
                }
            }
        }

        #region INotifyPropertyChanged

        internal void WireNotifications()
        {
            _roots.ForEach(root => root.Traverse(node => node.Value.PropertyChanged += Child_PropertyChanged));
        }
        
        private TimedBatchAction _batchNotifier = new(TimeSpan.FromMilliseconds(50));

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _batchNotifier.RequestAction(() => PropertyChanged?.Invoke(sender, e));   
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged
    }


    
}