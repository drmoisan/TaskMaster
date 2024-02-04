using Microsoft.Office.Interop.Outlook;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using System;
using Newtonsoft.Json;
using UtilitiesCS.Extensions.Lazy;
using System.Diagnostics;

namespace UtilitiesCS
{
    public class OlFolderInfo : INotifyPropertyChanged
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public OlFolderInfo() { }

        [JsonConstructor]
        public OlFolderInfo(bool selected, int itemCount, long folderSize, string name, string relativePath) 
        { 
            Selected = selected;
            ItemCount = itemCount;
            FolderSize = folderSize;
            Name = name;
            RelativePath = relativePath;
            SubscribeToPropertyChanged(PropertyEnum.All);
        }

        public OlFolderInfo(MAPIFolder olFolder, MAPIFolder olRoot)
        {
            _olFolder = olFolder;
            _olRoot = olRoot;
            ResetLazy();
            SubscribeToPropertyChanged(PropertyEnum.All);
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

        private Lazy<long> _lazyFolderSize;
        public long FolderSize { get => _lazyFolderSize.Value; set => _lazyFolderSize = value.ToLazyValue(); }
        private long LoadFolderSize()
        {
            return OlFolder.Items.Cast<dynamic>().Aggregate(0L, (acc, item) =>
            {
                try { return acc + (item?.Size ?? 0); }
                catch (OverflowException e)
                {
                    string message = $"OverflowException encountered while aggregating " +
                    $"item sizes in {OlFolder.FolderPath}.\n{e.Message}\n";

                    message += $"Accumulator prior to overflow: {acc:N0}\n";

                    try
                    {
                        message += $"Current Value to add: {item.Size}";
                    }
                    catch (System.Exception ie)
                    {
                        message += $"Unable to get size of item to add (see inner exception): \n{ie.Message}\n";
                    }

                    logger.Error(message, e);
                    return acc;
                }
            });
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
                logger.Debug($"{nameof(OlRoot)} or {nameof(OlFolder)} is null. Unable to load {nameof(RelativePath)}.\n" +
                    $"Call hierarchy {new StackTrace().GetMyTraceString()}");
                return null;
            }
            else if (OlFolder.FolderPath == OlRoot.FolderPath)
            {
                logger.Debug($"{nameof(OlFolder.FolderPath)} is the same as {nameof(OlRoot.FolderPath)}. " +
                    $"Returning full path.\nCall hierarchy {new StackTrace().GetMyTraceString()}");
                return OlFolder.FolderPath;
            }
            else if (!OlFolder.FolderPath.Contains(OlRoot.FolderPath))
            {
                logger.Debug($"{nameof(OlFolder.FolderPath)} does not contain {nameof(OlRoot.FolderPath)}. " +
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
        }

        #endregion Lazy Properties

        #region INotifyPropertyChanged

        [Flags]
        public enum PropertyEnum
        {
            OlRoot = 1,
            OlFolder = 2,
            ItemCount = 4,
            FolderSize = 8,
            Name = 16,
            RelativePath = 32,
            All = OlRoot | OlFolder | ItemCount | FolderSize | Name | RelativePath
        }

        [JsonIgnore]
        public PropertyEnum SubscriptionStatus { get; private set; }

        public void SubscribeToPropertyChanged(PropertyEnum properties)
        {
            if (properties.HasFlag(PropertyEnum.OlRoot))
            {
                PropertyChanged -= PropertyChanged_OlRoot;
                PropertyChanged += PropertyChanged_OlRoot;
                SubscriptionStatus |= PropertyEnum.OlRoot;
            }
            if (properties.HasFlag(PropertyEnum.OlFolder))
            {
                PropertyChanged -= PropertyChanged_OlFolder;
                PropertyChanged += PropertyChanged_OlFolder;
                SubscriptionStatus |= PropertyEnum.OlFolder;
            }
            if (properties.HasFlag(PropertyEnum.ItemCount))
            {
                PropertyChanged -= PropertyChanged_ItemCount;
                PropertyChanged += PropertyChanged_ItemCount;
                SubscriptionStatus |= PropertyEnum.ItemCount;
            }
            if (properties.HasFlag(PropertyEnum.FolderSize))
            {
                PropertyChanged -= PropertyChanged_FolderSize;
                PropertyChanged += PropertyChanged_FolderSize; 
                SubscriptionStatus |= PropertyEnum.FolderSize;
            }
            if (properties.HasFlag(PropertyEnum.Name))
            {
                PropertyChanged -= PropertyChanged_Name;
                PropertyChanged += PropertyChanged_Name;
                SubscriptionStatus |= PropertyEnum.Name;
            }
            if (properties.HasFlag(PropertyEnum.RelativePath))
            {
                PropertyChanged -= PropertyChanged_RelativePath;
                PropertyChanged += PropertyChanged_RelativePath;
                SubscriptionStatus |= PropertyEnum.RelativePath;
            }
        }

        public void UnSubscribeToPropertyChanged(PropertyEnum properties)
        {
            if (properties.HasFlag(PropertyEnum.OlRoot))
            {
                PropertyChanged -= PropertyChanged_OlRoot;
                SubscriptionStatus &= ~PropertyEnum.OlRoot;
            }
            if (properties.HasFlag(PropertyEnum.OlFolder))
            {
                PropertyChanged -= PropertyChanged_OlFolder;
                SubscriptionStatus &= ~PropertyEnum.OlFolder;
            }
            if (properties.HasFlag(PropertyEnum.ItemCount))
            {
                PropertyChanged -= PropertyChanged_ItemCount;
                SubscriptionStatus &= ~PropertyEnum.ItemCount;
            }
            if (properties.HasFlag(PropertyEnum.FolderSize))
            {
                PropertyChanged -= PropertyChanged_FolderSize;
                SubscriptionStatus &= ~PropertyEnum.FolderSize;
            }
            if (properties.HasFlag(PropertyEnum.Name))
            {
                PropertyChanged -= PropertyChanged_Name;
                SubscriptionStatus &= ~PropertyEnum.Name;
            }
            if (properties.HasFlag(PropertyEnum.RelativePath))
            {
                PropertyChanged -= PropertyChanged_RelativePath;
                SubscriptionStatus &= ~PropertyEnum.RelativePath;
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