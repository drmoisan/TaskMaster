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

        public OlFolderInfo() { ResetLazy(); }

        public OlFolderInfo(MAPIFolder olFolder, MAPIFolder olRoot)
        {
            _olFolder = olFolder;
            _olRoot = olRoot;
            ResetLazy();
        }

        [JsonIgnore]
        public MAPIFolder OlRoot 
        { 
            get => _olRoot;
            set 
            { 
                _olRoot = value;
                _lazyRelativePath = new Lazy<string>(LoadRelativePath);
            } 
        }
        private MAPIFolder _olRoot;

        private MAPIFolder _olFolder;
        [JsonIgnore]
        public MAPIFolder OlFolder
        {
            get => _olFolder;
            set
            {
                _olFolder = value;
                ResetLazy();
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
            Name = await Task.Run(LoadName);
            RelativePath = await Task.Run(LoadRelativePath);
            FolderSize = await Task.Run(LoadFolderSize);
            ItemCount = await Task.Run(() => OlFolder.Items.Count);
        }

        public void ResetLazy()
        {
            //_lazyFolderSize = new AsyncLazy<long>(async () => await Task.Run(() => LoadFolderSize()));
            //_lazyItemCount = new AsyncLazy<int>(async () => await Task.Run(() => OlFolder.Items.Count));
            _lazyFolderSize = new Lazy<long>(LoadFolderSize);
            _lazyItemCount = new Lazy<int>(() => OlFolder.Items.Count);
            _lazyName = new Lazy<string>(LoadName);
            _lazyRelativePath = new Lazy<string>(LoadRelativePath);
        }

        #endregion Lazy Properties

        #region AsyncLazy Properties

        //public AsyncLazy<int> ItemCount
        //{
        //    get => _lazyItemCount;
        //    protected set
        //    {
        //        _lazyItemCount = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        //private AsyncLazy<int> _lazyItemCount;

        //public AsyncLazy<long> FolderSize
        //{
        //    get => _lazyFolderSize;
        //    protected set
        //    {
        //        _lazyFolderSize = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        //private AsyncLazy<long> _lazyFolderSize;
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

        //public async Task ForceLazyToComplete()
        //{
        //    _ = await FolderSize;
        //    _ = await ItemCount;
        //}

        //public async Task<(long ItemSize, int ItemCount)> GetLazyValues()
        //{
        //    var itemSize = await FolderSize;
        //    var itemCount = await ItemCount;
        //    return (itemSize, itemCount);
        //}

        //public void ResetLazy()
        //{
        //    FolderSize = new AsyncLazy<long>(async () => await Task.Run(() => LoadFolderSize()));
        //    ItemCount = new AsyncLazy<int>(async () => await Task.Run(() => OlFolder.Items.Count));
        //}

        #endregion AsyncLazy Properties

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

    }
}