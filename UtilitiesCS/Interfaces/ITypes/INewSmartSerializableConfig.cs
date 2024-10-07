using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace UtilitiesCS.ReusableTypeClasses
{
    public interface INewSmartSerializableConfig: ICloneable, INotifyPropertyChanged
    {
        bool ClassifierActivated { get; set; }
        FilePathHelper Disk { get; set; }
        JsonSerializerSettings JsonSettings { get; set; }
        DateTime LocalDate { get; }
        FilePathHelper LocalDisk { get; set; }
        JsonSerializerSettings LocalJsonSettings { get; set; }
        FilePathHelper NetDisk { get; set; }
        JsonSerializerSettings NetJsonSettings { get; set; }
        DateTime NetworkDate { get; }
        ActiveDiskEnum ActiveDisk { get; }
        INewSmartSerializableConfig DeepCopy();
        void CopyFrom(INewSmartSerializableConfig config, bool deep);

        void ActivateLocalDisk();
        void ActivateMostRecent();
        void ActivateNetDisk();
        void ResetLazy();
        void ResetLazy(Lazy<JsonSerializerSettings> localJsonSettings, Lazy<JsonSerializerSettings> netJsonSettings, Lazy<JsonSerializerSettings> jsonSettings);

        enum ActiveDiskEnum
        {
            Neither,
            Local,
            Net
        }
    }
}