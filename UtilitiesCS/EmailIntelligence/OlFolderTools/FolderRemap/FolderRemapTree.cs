using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.EmailIntelligence.FolderRemap
{
    public class FolderRemapTree : INotifyPropertyChanged
    {
        public FolderRemapTree() { }

        public FolderRemapTree(MAPIFolder olRoot)
        {
            var root = RootFromFolder(olRoot);
            _roots = new List<TreeNode<OlFolderRemap>>() { root };
            WireNotifications();
        }

        public FolderRemapTree(MAPIFolder olRoot, IDictionary<string, string> mappings)
        {
            var root = RootFromFolder(olRoot);

            foreach (var mapping in mappings) 
            {
                var fromNode = root.FindAll(x => x.Value.RelativePath == mapping.Key).FirstOrDefault();
                var toNode = root.FindAll(x => x.Value.RelativePath == mapping.Value).FirstOrDefault();
                if (fromNode is not null && toNode is not null)
                    fromNode.Value.MappedTo = toNode.Value;
            }
            _roots = new List<TreeNode<OlFolderRemap>>() { root };
            WireNotifications();
        }

        private TreeNode<OlFolderRemap> RootFromFolder(MAPIFolder olRoot)
        {
            var info = new OlFolderRemap(olRoot, olRoot);
            var root = new TreeNode<OlFolderRemap>(info);
            this.InitializeChildren(root, olRoot);
            return root;
        }

        private List<TreeNode<OlFolderRemap>> _roots;
        public List<TreeNode<OlFolderRemap>> Roots { get => _roots; }

        private void InitializeChildren(TreeNode<OlFolderRemap> node, MAPIFolder olRoot)
        {
            node.Value.OlFolder.Folders.Cast<MAPIFolder>()
                .ForEach(child =>
                {
                    var childNode = node.AddChild(new OlFolderRemap(child, olRoot));
                    if (child.Folders.Count > 0)
                        InitializeChildren(childNode, olRoot);
                });
        }

        public List<OlFolderRemap> GetRemapList()
        {
            return _roots.SelectMany(node => node
                         .FindAll(node => node.Value.MappedTo is not null))
                         //.OrderBy(node => node.Depth)
                         .Select(node => node.Value)
                         .ToList();
        }
        
        public List<TreeNode<OlFolderRemap>> GetInvertedMapTree() 
        {
            var remapList = GetRemapList();
            var remapTree = new List<TreeNode<OlFolderRemap>>();
            foreach (var mapping in remapList)
            {
                TreeNode<OlFolderRemap> mapNode = remapTree.SelectMany(x => x
                    .FindAll(x => x.Value.RelativePath == mapping.MappedTo.RelativePath))
                    .FirstOrDefault();

                if (mapNode == default(TreeNode<OlFolderRemap>))
                {
                    mapNode = new TreeNode<OlFolderRemap>(mapping.MappedTo);
                    remapTree.Add(mapNode);
                }
                mapNode.AddChild(mapping);
            }
            return remapTree;
        }

        public List<TreeNode<OlFolderRemap>> FilterMapped(bool include)
        {
            var dummyRootOut = new TreeNode<OlFolderRemap>(_roots[0].Value);
            var dummyRootIn = new TreeNode<OlFolderRemap>(_roots[0].Value);
            dummyRootIn.Children = _roots;

            FilterChildren(dummyRootIn, dummyRootOut, include);
            var selected = dummyRootOut.Children;
            return selected;
        }

        private void FilterChildren(TreeNode<OlFolderRemap> source, TreeNode<OlFolderRemap> destination, bool include)
        {
            foreach (var sourceChild in source.Children)
            {
                if ((sourceChild.Value.MappedTo is not null) == include)
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


    public class OlFolderRemap : INotifyPropertyChanged
    {
        public OlFolderRemap() { }

        public OlFolderRemap(MAPIFolder olFolder, MAPIFolder olRoot)
        {
            _olFolder = olFolder;
            _olRoot = olRoot;
            _relativePath = olFolder.FolderPath.Replace(olRoot.FolderPath+"\\", "");
            _name = olFolder.Name;
        }

        private MAPIFolder _olRoot;
        public MAPIFolder OlRoot { get => _olRoot; set => _olRoot = value; }

        private MAPIFolder _olFolder;
        public MAPIFolder OlFolder
        {
            get => _olFolder;
            set
            {
                _olFolder = value;
                RelativePath = _olFolder.FolderPath.Replace(_olRoot.FolderPath, "");
                Name = _olFolder.Name;
            }
        }

        private string _name;
        public string Name { get => _name; private set => _name = value; }

        private string _relativePath;
        public string RelativePath { get => _relativePath; private set => _relativePath = value; }

        private OlFolderRemap _mappedTo;
        public OlFolderRemap MappedTo
        {
            get => _mappedTo;
            set
            {
                _mappedTo = value;
                NotifyPropertyChanged();
            }
        }
        //private object _mappedTo;
        //public object MappedTo
        //{
        //    get => _mappedTo;
        //    set
        //    {
        //        _mappedTo = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}

