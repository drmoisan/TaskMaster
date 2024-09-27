using Newtonsoft.Json;
using System;

namespace UtilitiesCS.ReusableTypeClasses
{
    public interface INewSmartSerializableConfig
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

        void ActivateLocalDisk();
        void ActivateMostRecent();
        void ActivateNetDisk();
        void ResetLazy();
        void ResetLazy(Lazy<JsonSerializerSettings> localJsonSettings, Lazy<JsonSerializerSettings> netJsonSettings, Lazy<JsonSerializerSettings> jsonSettings);
    }
}