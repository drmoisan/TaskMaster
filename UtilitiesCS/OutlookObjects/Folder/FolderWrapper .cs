using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;
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
            ItemHelpers = new AsyncLazy<IItemInfo[]>(async () => await Task.Run(() => LoadItemHelpers()));
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

        #region Folder Comparison

        //private Lazy<IItemInfo[]> _lazyItemHelpers;
        //public IItemInfo[] ItemHelpers { get => _lazyItemHelpers.Value; set => _lazyItemHelpers = value?.ToLazy(); }
        public AsyncLazy<IItemInfo[]> ItemHelpers { get; set; } 
        public IApplicationGlobals Globals { get; set; }


        internal IItemInfo[] LoadItemHelpers()
        {
            if (Globals is null) { throw new ArgumentNullException("Globals"); }
            List<IItemInfo> helpers = [];
            var items = OlFolder.Items;            
            foreach (var objItem in items)
            {
                try
                {
                    if (objItem is MailItem mailItem)
                    {
                        helpers.Add(new MailItemHelper(mailItem, Globals).ToMatchableObject());
                    }
                    else if (objItem is MeetingItem meetingItem)
                    {
                        helpers.Add(new MeetingItemHelper(meetingItem, Globals).ToMatchableObject());
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
            Marshal.ReleaseComObject(items);
            return helpers.ToArray();
        }

        public async Task<double> CalculateItemMatchPercentageAsync(FolderWrapper other, IApplicationGlobals globals, CancellationToken cancel)
        {
            Globals = globals;
            return await CalculateItemMatchPercentageAsync(other, cancel).ConfigureAwait(false);
        }

        public async Task<double> CalculateItemMatchPercentageAsync(FolderWrapper other, CancellationToken cancel)
        {
            if (Globals is null) { throw new ArgumentNullException("Globals"); }
            if (other.Globals is null) { other.Globals = Globals; }
            if (ItemCount == 0 || other.ItemCount == 0) { return 0; }
            
            var (matching, currentOnly, otherOnly) = await CompareItemsAsync(other, cancel).ConfigureAwait(false);
            return CalculateItemMatchPercentage(matching, currentOnly, otherOnly);
        }

        public double CalculateItemMatchPercentage(IItemInfo[] matching, IItemInfo[] currentOnly, IItemInfo[] otherOnly)
        {
            if (matching.IsNullOrEmpty()) { return 0; }
            double matchPercentage = (matching.Length * 2) / (double)(matching.Length * 2 + currentOnly.Length + otherOnly.Length);
            return matchPercentage;
        }

        public async Task<(IItemInfo[] matching, IItemInfo[] currentOnly, IItemInfo[] otherOnly)> CompareItemsAsync(FolderWrapper other, IApplicationGlobals globals, CancellationToken cancel)
        {
            Globals = globals;
            return await CompareItemsAsync(other, cancel).ConfigureAwait(false);
        }

        public async Task<(IItemInfo[] matching, IItemInfo[] currentOnly, IItemInfo[] otherOnly)> CompareItemsAsync(FolderWrapper other, CancellationToken cancel)
        {
            if (Globals is null) { throw new ArgumentNullException("Globals"); }
            if (other.Globals is null) { other.Globals = Globals; }
            var currentHelpers = await ItemHelpers;
            var otherHelpers = await other.ItemHelpers;
            if (currentHelpers.IsNullOrEmpty() || otherHelpers.IsNullOrEmpty()) 
            { 
                return ([], currentHelpers, otherHelpers); 
            }
            else
            {
                int i = 0;
                int j = 0;
                var matching2 = new List<IItemInfo>();

                for (i = 0; i < currentHelpers.Length; i++)
                {
                    for (j = 0; j < otherHelpers.Length; j++)
                    {
                        if(currentHelpers[i].Equals(otherHelpers[j]))
                        {
                            matching2.Add(currentHelpers[i]);
                            break; // Found a match, no need to check further for this item
                        }
                    }
                }

                var matching = currentHelpers.Intersect(otherHelpers).ToArray();
                var currentOnly = currentHelpers.Except(otherHelpers).ToArray();
                var otherOnly = otherHelpers.Except(currentHelpers).ToArray();
                return (matching, currentOnly, otherOnly); 
            }
        }



        #endregion Folder Comparison

    }
}