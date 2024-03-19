using BrightIdeasSoftware;
using Swordfish.NET.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS.EmailIntelligence.FolderRemap
{
    public class FolderRemapController
    {
        public FolderRemapController(IApplicationGlobals appGlobals) 
        {
            _globals = appGlobals;
            _folderRemapTree = new FolderRemapTree(_globals.Ol.ArchiveRoot, _globals.TD.FolderRemap);
            Mappings2 = _folderRemapTree.GetRemapList();
            _folderRemapTree.PropertyChanged += OlFolderTree_PropertyChanged;
            _viewer = new FolderRemapViewer();
            _viewer.SetController(this);
            _viewer.TlvOriginal.CheckStateGetter = GetCheckedState;
            _viewer.TlvOriginal.CheckStatePutter = MakeCheckedStatePutter();
            _viewer.OlvMap.CheckStateGetter = GetMappedCheckedState;
            _viewer.OlvMap.CheckStatePutter = PutMappedCheckedState;
            _viewer.Show();
            ExpandTo(1, true);
            _viewer.TlvOriginal.Refresh();
            _viewer.Refresh();
        }

        private IApplicationGlobals _globals;
        private FolderRemapViewer _viewer;

        private FolderRemapTree _folderRemapTree;
        public FolderRemapTree RemapTree { get => _folderRemapTree; }

        private List<OlFolderRemap> _mappings2;
        public List<OlFolderRemap> Mappings2 { get => _mappings2; private set => _mappings2 = value; }
        
        private bool _update;

        #region Internal Helper Methods

        internal void SyncTreeToMappings()
        {
            var treeMappings = RemapTree.Roots
                .SelectMany(x => x
                .FlattenIf(mapping => mapping.MappedTo is not null));

            Mappings2 = treeMappings.ToList();
        }

        internal void SyncGlobalMap()
        {
            // remove any keys that are no longer remapped
            _globals.TD.FolderRemap.Keys
                .Where(key => !Mappings2.Any(x => x.RelativePath == key))
                .ForEach(x => _globals.TD.FolderRemap.Remove(x));

            // add or update remapped keys
            Mappings2.ForEach(mapping =>
            {
                if (!_globals.TD.FolderRemap.TryAdd(mapping.RelativePath, mapping.MappedTo.RelativePath))
                    _globals.TD.FolderRemap[mapping.RelativePath] = mapping.MappedTo.RelativePath;
            });

            // Save the result
            _globals.TD.FolderRemap.Serialize();
        }

        internal void ExpandTo(int level, bool addChecked=false) 
        {
            RemapTree.Roots
                     .SelectMany(root => root
                     .FindAll(node => node.Depth < level))
                     .ForEach(x => _viewer.TlvOriginal.Expand(x));

            if (addChecked)
            {
                RemapTree.Roots
                         .SelectMany(root => root
                         .FindAll(node => node
                            .FindAll(node => node.Value.MappedTo != null)
                            .Any()))
                         .ForEach(x => _viewer.TlvOriginal.Expand(x));
            }
        }

        #endregion Internal Helper Methods

        #region Event Handlers

        internal void Discard() => _viewer.Close();

        internal void Save()
        {
            _viewer.Close();
            SyncGlobalMap();
        }

        public void OlFolderTree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_update) return;
            SyncTreeToMappings();
            _viewer.OlvMap.SetObjects(Mappings2);
        }

        internal void HandleModelCanDrop(object sender, BrightIdeasSoftware.ModelDropEventArgs e)
        {
            e.Handled = true;
            e.Effect = DragDropEffects.None;
            if (e.SourceModels.Contains(e.TargetModel))
                e.InfoMessage = "Cannot drop on self";
            else
            {
                var sourceModels = e.SourceModels.Cast<TreeNode<OlFolderRemap>>();
                var target = e.TargetModel as TreeNode<OlFolderRemap>;
                if (target.Value.MappedTo is not null)
                    e.InfoMessage = $"Target {target.Value.Name} is mapped to another folder {target.Value.MappedTo.Name}";
                //if (sourceModels.Any(x => target.IsAncestor(x)))
                //    e.InfoMessage = "Cannot drop on descendant (think of the temporal paradoxes!)";
                //else
                // allow drop on ancestor to allow collapsing predictive classes into a single class
                e.Effect = DragDropEffects.Move;
            }
        }

        internal void HandleModelDropped(object sender, BrightIdeasSoftware.ModelDropEventArgs e)
        {
            switch (e.DropTargetLocation)
            {
                case DropTargetLocation.Background:
                    
                    break;
                case DropTargetLocation.Item: 
                    _update = true;
                    MoveObjectsToChildren(
                        e.ListView as TreeListView,
                        e.SourceListView as TreeListView,
                        (TreeNode<OlFolderRemap>)e.TargetModel,
                        e.SourceModels);
                    _update = false;
                    break;
                default:
                    return;
            }

            //e.RefreshObjects();
            //_viewer.TlvOriginal.Sort(0);
        }

        private void MoveObjectsToChildren(TreeListView targetTree, TreeListView sourceTree, TreeNode<OlFolderRemap> target, IList toMove)
        {
            var targetObj = target.Value;
            if (targetObj.MappedTo is not null)
                targetObj = targetObj.MappedTo;
            foreach (TreeNode<OlFolderRemap> x in toMove)
            {
                x.Traverse(node => node.Value.MappedTo = targetObj);
            }

            SyncTreeToMappings();
            _viewer.OlvMap.SetObjects(Mappings2);
        }

        internal CheckStateGetterDelegate GetMappedCheckedState = delegate (object rowObject)
        {
            var node = (OlFolderRemap)rowObject;
            if (node.MappedTo is not null) { return CheckState.Checked; }
            else { return CheckState.Unchecked; }
        };

        internal CheckStatePutterDelegate PutMappedCheckedState = delegate (object rowObject, CheckState newValue)
        {
            var node = (OlFolderRemap)rowObject;
            if (newValue == CheckState.Checked)
            {
                return CheckState.Checked;
            }
            else 
            {
                node.MappedTo = null;
                return CheckState.Unchecked;
            }            
        };



        internal CheckStateGetterDelegate GetCheckedState = delegate (object rowObject)
        {
            var node = (TreeNode<OlFolderRemap>)rowObject;
            if (node.Value.MappedTo is not null)
                return CheckState.Checked;
            else if (node.Flatten().Any(x => x.MappedTo is not null))
                return CheckState.Indeterminate;
            else
                return CheckState.Unchecked;

        };

        internal CheckStatePutterDelegate MakeCheckedStatePutter()
        {
            CheckStatePutterDelegate putter = delegate (object rowObject, CheckState newValue)
            {
                var node = (TreeNode<OlFolderRemap>)rowObject;
                if (newValue == CheckState.Checked)
                {
                    var destination = FolderSelector.SelectFolder(RemapTree.Roots);
                    if (destination is null || destination.RelativePath == node.Value.RelativePath)
                    {
                        if (node.Flatten().Any(x => x.MappedTo is not null))
                            return CheckState.Indeterminate;
                        else
                            return CheckState.Unchecked;
                    }

                    if (destination.MappedTo is not null) { destination = destination.MappedTo; }
                    node.Value.MappedTo = destination;
                    
                    return CheckState.Checked;
                }
                
                else
                {
                    node.Value.MappedTo = null;
                    return CheckState.Unchecked;
                }
            };
            return putter;
        } 

        #endregion Event Handlers


    }
}
