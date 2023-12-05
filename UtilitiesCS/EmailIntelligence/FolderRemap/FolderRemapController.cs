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
            InitMappings();            
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
        }

        private IApplicationGlobals _globals;
        private FolderRemapViewer _viewer;

        private FolderRemapTree _folderRemapTree;
        public FolderRemapTree RemapTree { get => _folderRemapTree; }

        private List<OlFolderRemap> _mappings2;
        public List<OlFolderRemap> Mappings2 { get => _mappings2; private set => _mappings2 = value; }
        
        private ConcurrentObservableDictionary<string, string> _mappings;
        internal ConcurrentObservableDictionary<string, string> Mappings { get => _mappings; }

        #region Internal Helper Methods

        internal void InitMappings()
        {            
            var mappings = _globals.TD.FolderRemap.ToArray();
            _mappings = new ConcurrentObservableDictionary<string, string>(mappings.ToDictionary());
        }

        internal void SyncTreeToMappings()
        {
            var treeMappings = RemapTree.Roots
                .SelectMany(x => x
                .FlattenIf(mapping => mapping.MappedTo is not null));

            Mappings2 = treeMappings.ToList();
            // remove any keys that are no longer remapped
            Mappings.Keys
                .Where(key => !treeMappings
                .Any(mapping => mapping.RelativePath == key))
                .ForEach(x => Mappings.Remove(x));

            // add or update remapped keys
            treeMappings.ForEach(mapping =>
            {
                if (!Mappings.TryAdd(mapping.RelativePath, mapping.MappedTo.RelativePath))
                    Mappings[mapping.RelativePath] = mapping.MappedTo.RelativePath;
            });
        }

        internal void SyncGlobalMap()
        {
            // remove any keys that are no longer remapped
            _globals.TD.FolderRemap.Keys
                .Where(key => !Mappings.ContainsKey(key))
                .ForEach(x => _globals.TD.FolderRemap.Remove(x));

            // add or update remapped keys
            Mappings.ForEach(mapping =>
            {
                if (!_globals.TD.FolderRemap.TryAdd(mapping.Key, mapping.Value))
                    _globals.TD.FolderRemap[mapping.Key] = mapping.Value;
            });

            // Save the result
            _globals.TD.FolderRemap.Serialize();
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
                    MoveObjectsToChildren(
                        e.ListView as TreeListView,
                        e.SourceListView as TreeListView,
                        (TreeNode<OlFolderRemap>)e.TargetModel,
                        e.SourceModels);
                    break;
                default:
                    return;
            }

            e.RefreshObjects();
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
                    node.Traverse(x => x.Value.MappedTo = null);
                    //node.Value.Selected = false;
                    return CheckState.Unchecked;
                }
            };
            return putter;
        } 

        #endregion Event Handlers


    }
}
