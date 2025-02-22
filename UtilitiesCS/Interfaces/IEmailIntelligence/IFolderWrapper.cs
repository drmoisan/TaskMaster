using Microsoft.Office.Interop.Outlook;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public interface IFolderWrapper
    {
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

        long FolderSize { get; set; }
        int ItemCount { get; set; }
        int ItemCountSubFolders { get; set; }
        string Name { get; }
        MAPIFolder OlFolder { get; set; }
        MAPIFolder OlRoot { get; set; }
        string RelativePath { get; set; }
        bool Selected { get; set; }
        PropertyEnum SubscriptionStatus { get; }

        event PropertyChangedEventHandler PropertyChanged;

        Task LoadLazyAsync();
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "");
        void ResetLazy();
        void SubscribeToPropertyChanged(PropertyEnum properties);
        void UnSubscribeToPropertyChanged(PropertyEnum properties);
    }


}