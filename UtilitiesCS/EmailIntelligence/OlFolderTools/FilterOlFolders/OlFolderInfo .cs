using Microsoft.Office.Interop.Outlook;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace UtilitiesCS
{
    public class OlFolderInfo : INotifyPropertyChanged
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public OlFolderInfo(MAPIFolder olFolder, MAPIFolder olRoot)
        {
            _olFolder = olFolder;
            _olRoot = olRoot;
            _relativePath = olFolder.FolderPath.Replace(olRoot.FolderPath + "\\", "");
            _name = olFolder.Name;
            _lazyFolderSize = new AsyncLazy<long>(async () => await Task.Run(() => LoadFolderSize()));
            _lazyItemCount = new AsyncLazy<int>(async () => await Task.Run(() => OlFolder.Items.Count));
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
                RelativePath = _olFolder.FolderPath.Replace(_olRoot.FolderPath + "\\", "");
                Name = _olFolder.Name;
            }
        }

        private string _name;
        public string Name { get => _name; private set => _name = value; }

        private string _relativePath;
        public string RelativePath { get => _relativePath; private set => _relativePath = value; }

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

        #region AsyncLazy Properties

        public AsyncLazy<int> ItemCount
        {
            get => _lazyItemCount;
            protected set
            {
                _lazyItemCount = value;
                NotifyPropertyChanged();
            }
        }
        private AsyncLazy<int> _lazyItemCount;

        public AsyncLazy<long> FolderSize
        {
            get => _lazyFolderSize;
            protected set
            {
                _lazyFolderSize = value;
                NotifyPropertyChanged();
            }
        }
        private AsyncLazy<long> _lazyFolderSize;
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

        public async Task ForceLazyToComplete()
        {
            _ = await FolderSize;
            _ = await ItemCount;
        }

        public async Task<(long ItemSize, int ItemCount)> GetLazyValues()
        {
            var itemSize = await FolderSize;
            var itemCount = await ItemCount;
            return (itemSize, itemCount);
        }

        public void ResetLazy()
        {
            FolderSize = new AsyncLazy<long>(async () => await Task.Run(() => LoadFolderSize()));
            ItemCount = new AsyncLazy<int>(async () => await Task.Run(() => OlFolder.Items.Count));
        }

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