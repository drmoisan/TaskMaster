using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS
{
    public class FolderWrapper : INotifyPropertyChanged, IFolderWrapper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected FolderWrapper() { }

        [JsonConstructor]
        public FolderWrapper(bool selected, int itemCount, long folderSize, string name, string relativePath)
        {
            Selected = selected;
            ItemCount = itemCount;
            FolderSize = folderSize;
            Name = name;
            RelativePath = relativePath;
            SubscribeToPropertyChanged(IFolderWrapper.PropertyEnum.All);
        }

        public FolderWrapper(MAPIFolder olFolder, MAPIFolder olRoot)
        {
            _olFolder = olFolder;
            _olRoot = olRoot;
            ResetLazy();
            SubscribeToPropertyChanged(IFolderWrapper.PropertyEnum.All);
        }

        private MAPIFolder _olRoot;
        [JsonIgnore]
        public MAPIFolder OlRoot
        {
            get => _olRoot;
            set
            {
                _olRoot = value;
                NotifyPropertyChanged();
            }
        }

        private MAPIFolder _olFolder;
        [JsonIgnore]
        public MAPIFolder OlFolder
        {
            get => _olFolder;
            set
            {
                _olFolder = value;
                NotifyPropertyChanged();
            }
        }

        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                NotifyPropertyChanged();
            }
        }

        #region Lazy Properties

        private Lazy<int> _lazyItemCount;
        public int ItemCount { get => _lazyItemCount.Value; set => _lazyItemCount = value.ToLazyValue(); }

        private Lazy<int> _lazyItemCountSubFolders;
        public int ItemCountSubFolders { get => _lazyItemCountSubFolders.Value; set => _lazyItemCountSubFolders = value.ToLazyValue(); }
        internal int LoadItemCountSubFolders()
        {
            return ItemCount + SumItemCountRecursively(OlFolder);
        }
        internal int SumItemCountRecursively(MAPIFolder folder) 
        {
            return folder.Folders?.Cast<MAPIFolder>().Sum(f => f.Items.Count + SumItemCountRecursively(f)) ?? 0;
        }

        private Lazy<long> _lazyFolderSize;
        public long FolderSize { get => _lazyFolderSize.Value; set => _lazyFolderSize = value.ToLazyValue(); }
        private long LoadFolderSize()
        {
            var items = OlFolder.Items;
            long totalSize = 0L;
            foreach (var objItem in items) 
            {
                try
                {                    
                    var olItem = new OutlookItem(objItem);
                    if (olItem.IsValid()) { totalSize += olItem.Size; }
                    else if (HasProperty(objItem, "Size")) // Fallback for items that don't implement IOutlookItem
                    {
                        totalSize += (long)objItem.GetType().GetProperty("Size").GetValue(objItem, null);
                    }
                }
                catch (System.Exception e)
                {
                    logger.Error(e.Message, e);
                }
                finally
                {                    
                    Marshal.ReleaseComObject(objItem);
                }
            }
            return totalSize;
        }

        private bool HasProperty(object obj, string propertyName)
        {
            if (obj == null) return false;
            return obj.GetType().GetProperty(propertyName) != null;
        }
        //private long LoadFolderSize()
        //{
        //    return OlFolder.Items.Cast<dynamic>().Aggregate(0L, (acc, item) =>
        //    {
        //        try { return acc + (item?.Size ?? 0); }
        //        catch (OverflowException e)
        //        {
        //            string message = $"OverflowException encountered while aggregating " +
        //            $"item sizes in {OlFolder.FolderPath}.\n{e.Message}\n";

        //            message += $"Accumulator prior to overflow: {acc:N0}\n";

        //            try
        //            {
        //                message += $"Current Value to add: {item.Size}";
        //            }
        //            catch (System.Exception ie)
        //            {
        //                message += $"Unable to get size of item to add (see inner exception): \n{ie.Message}\n";
        //            }

        //            logger.Error(message, e);
        //            return acc;
        //        }
        //    });
        //}

        private Lazy<string> _lazyName;
        public string Name { get => _lazyName.Value; private set => _lazyName = value.ToLazy(); }
        internal virtual string LoadName() => OlFolder?.Name;

        private Lazy<string> _lazyRelativePath;
        [JsonProperty]
        public string RelativePath { get => _lazyRelativePath.Value; set => _lazyRelativePath = value.ToLazy(); }
        internal virtual string LoadRelativePath()
        {
            if (OlRoot is null || OlFolder is null)
            {
                logger.Warn($"{nameof(OlRoot)} or {nameof(OlFolder)} is null. Unable to load {nameof(RelativePath)}.\n" +
                    $"Call hierarchy {new StackTrace().GetMyTraceString()}");
                return null;
            }
            else if (OlFolder.FolderPath == OlRoot.FolderPath)
            {
                logger.Warn($"{nameof(OlFolder.FolderPath)} is the same as {nameof(OlRoot.FolderPath)}. " +
                    $"Returning full path.\nCall hierarchy {new StackTrace().GetMyTraceString()}");
                return OlFolder.FolderPath;
            }
            else if (!OlFolder.FolderPath.Contains(OlRoot.FolderPath))
            {
                logger.Warn($"{nameof(OlFolder.FolderPath)} does not contain {nameof(OlRoot.FolderPath)}. " +
                    $"Returning full path.\nCall hierarchy {new StackTrace().GetMyTraceString()}");
                return OlFolder.FolderPath;
            }
            else
            {
                return OlFolder.FolderPath.Replace(OlRoot.FolderPath + "\\", "");
            }
        }

        public async Task LoadLazyAsync()
        {
            await Task.Run(() =>
            {
                _ = Name;
                _ = RelativePath;
                _ = FolderSize;
                _ = ItemCount;
            });
        }

        public void ResetLazy()
        {
            _lazyFolderSize = new Lazy<long>(LoadFolderSize);
            _lazyItemCount = new Lazy<int>(() => OlFolder.Items.Count);
            _lazyName = new Lazy<string>(LoadName);
            _lazyRelativePath = new Lazy<string>(LoadRelativePath);
            _lazyItemCountSubFolders = new Lazy<int>(LoadItemCountSubFolders);
        }

        #endregion Lazy Properties

        #region INotifyPropertyChanged

        
        [JsonIgnore]
        public IFolderWrapper.PropertyEnum SubscriptionStatus { get; private set; }

        public void SubscribeToPropertyChanged(IFolderWrapper.PropertyEnum properties)
        {
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.OlRoot))
            {
                PropertyChanged -= PropertyChanged_OlRoot;
                PropertyChanged += PropertyChanged_OlRoot;
                SubscriptionStatus |= IFolderWrapper.PropertyEnum.OlRoot;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.OlFolder))
            {
                PropertyChanged -= PropertyChanged_OlFolder;
                PropertyChanged += PropertyChanged_OlFolder;
                SubscriptionStatus |= IFolderWrapper.PropertyEnum.OlFolder;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.ItemCount))
            {
                PropertyChanged -= PropertyChanged_ItemCount;
                PropertyChanged += PropertyChanged_ItemCount;
                SubscriptionStatus |= IFolderWrapper.PropertyEnum.ItemCount;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.FolderSize))
            {
                PropertyChanged -= PropertyChanged_FolderSize;
                PropertyChanged += PropertyChanged_FolderSize;
                SubscriptionStatus |= IFolderWrapper.PropertyEnum.FolderSize;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.Name))
            {
                PropertyChanged -= PropertyChanged_Name;
                PropertyChanged += PropertyChanged_Name;
                SubscriptionStatus |= IFolderWrapper.PropertyEnum.Name;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.RelativePath))
            {
                PropertyChanged -= PropertyChanged_RelativePath;
                PropertyChanged += PropertyChanged_RelativePath;
                SubscriptionStatus |= IFolderWrapper.PropertyEnum.RelativePath;
            }
        }

        public void UnSubscribeToPropertyChanged(IFolderWrapper.PropertyEnum properties)
        {
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.OlRoot))
            {
                PropertyChanged -= PropertyChanged_OlRoot;
                SubscriptionStatus &= ~IFolderWrapper.PropertyEnum.OlRoot;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.OlFolder))
            {
                PropertyChanged -= PropertyChanged_OlFolder;
                SubscriptionStatus &= ~IFolderWrapper.PropertyEnum.OlFolder;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.ItemCount))
            {
                PropertyChanged -= PropertyChanged_ItemCount;
                SubscriptionStatus &= ~IFolderWrapper.PropertyEnum.ItemCount;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.FolderSize))
            {
                PropertyChanged -= PropertyChanged_FolderSize;
                SubscriptionStatus &= ~IFolderWrapper.PropertyEnum.FolderSize;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.Name))
            {
                PropertyChanged -= PropertyChanged_Name;
                SubscriptionStatus &= ~IFolderWrapper.PropertyEnum.Name;
            }
            if (properties.HasFlag(IFolderWrapper.PropertyEnum.RelativePath))
            {
                PropertyChanged -= PropertyChanged_RelativePath;
                SubscriptionStatus &= ~IFolderWrapper.PropertyEnum.RelativePath;
            }
        }

        private void PropertyChanged_OlFolder(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OlFolder))
            {
                ResetLazy();
            }
        }

        private void PropertyChanged_OlRoot(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OlRoot))
            {
                _lazyRelativePath = new Lazy<string>(LoadRelativePath);
            }
        }

        private void PropertyChanged_ItemCount(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemCount)) { }
        }

        private void PropertyChanged_FolderSize(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FolderSize)) { }
        }

        private void PropertyChanged_Name(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Name)) { }
        }

        private void PropertyChanged_RelativePath(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RelativePath)) { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

    }
}