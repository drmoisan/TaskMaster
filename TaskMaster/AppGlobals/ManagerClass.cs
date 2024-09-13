using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;
using UtilitiesCS;
using System.IO;
using UtilitiesCS.Extensions;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using System.Windows.Input;
using TaskMaster;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;


namespace TaskMaster.AppGlobals
{
    public class ManagerClass: ConcurrentDictionary<string, BayesianClassifierGroup>, INewSmartSerializable<ManagerClass>
    {
        public ManagerClass() { }

        #region commented code
        public async static Task<ManagerClass> LoadAsync(string fileName, string localPath, string networkPath)
        {
            var manager = new ManagerClass();

            //var config = SmartSerializableConfig

            var network = new FilePathHelper(fileName, networkPath);
            var networkDt = File.Exists(network.FilePath) ? File.GetLastWriteTimeUtc(network.FilePath) : default;

            var local = new FilePathHelper(fileName, localPath);
            var localDt = File.Exists(local.FilePath) ? File.GetLastWriteTimeUtc(local.FilePath) : default;

            //var localSettings = manager.GetSettings(false);
            //var networkSettings = manager.GetSettings(true);

            //var manager = GetManager(local, localSettings);
            //manager.NetDisk = network;
            //manager.NetJsonSettings = networkSettings;
            //manager.LocalDisk = local;
            //manager.LocalJsonSettings = localSettings;

            //if (networkDt != default && (localDt == default || networkDt > localDt))
            //{
            //    IdleActionQueue.AddEntry(async () =>
            //        await Task.Run(() =>
            //        {
            //            _manager = GetManager(network, networkSettings);
            //            _manager.NetDisk = network;
            //            _manager.NetJsonSettings = networkSettings;
            //            _manager.LocalDisk = local;
            //            _manager.LocalJsonSettings = localSettings;
            //            _manager.ActivateLocalDisk();
            //            IdleActionQueue.AddEntry(() => _manager.Serialize());
            //        }
            //        ));
            //}
            await Task.CompletedTask; // TODO: remove this line once the method is implemented
            return manager;
        }

        public async static Task<ManagerClass> LoadAsync(SmartSerializableConfigOld config)
        {
            var manager = new ManagerClass();

            

            //var network = new FilePathHelper(fileName, networkPath);
            //var networkDt = File.Exists(network.FilePath) ? File.GetLastWriteTimeUtc(network.FilePath) : default;

            //var local = new FilePathHelper(fileName, localPath);
            //var localDt = File.Exists(local.FilePath) ? File.GetLastWriteTimeUtc(local.FilePath) : default;

            //var localSettings = manager.GetSettings(false);
            //var networkSettings = manager.GetSettings(true);

            //var manager = GetManager(local, localSettings);
            //manager.NetDisk = network;
            //manager.NetJsonSettings = networkSettings;
            //manager.LocalDisk = local;
            //manager.LocalJsonSettings = localSettings;

            //if (networkDt != default && (localDt == default || networkDt > localDt))
            //{
            //    IdleActionQueue.AddEntry(async () =>
            //        await Task.Run(() =>
            //        {
            //            _manager = GetManager(network, networkSettings);
            //            _manager.NetDisk = network;
            //            _manager.NetJsonSettings = networkSettings;
            //            _manager.LocalDisk = local;
            //            _manager.LocalJsonSettings = localSettings;
            //            _manager.ActivateLocalDisk();
            //            IdleActionQueue.AddEntry(() => _manager.Serialize());
            //        }
            //        ));
            //}
            await Task.CompletedTask; // TODO: remove this line once the method is implemented
            return manager;
        }


        //private ScDictionary<string, BayesianClassifierGroup> GetManager(
        //    FilePathHelper disk,
        //    JsonSerializerSettings settings)
        //{
        //    return ScDictionary<string, BayesianClassifierGroup>.Deserialize(
        //        fileName: disk.FileName,
        //        folderPath: disk.FolderPath,
        //        askUserOnError: false,
        //        settings: settings);
        //}
        //private async Task LoadManagerAsync()
        //{
        //    //LoadProgressPane(_tokenSource);
        //    //await Task.Run(
        //    //    () => _manager = LoadManager(),
        //    //    CancelToken);
        //}
        //public void SaveManagerLocal()
        //{
        //    //_manager.ActivateLocalDisk();
        //    //_manager.Serialize();
        //}
        //public void SaveManagerNetwork()
        //{
        //    //_manager.ActivateNetDisk();
        //    //_manager.Serialize();
        //}
        #endregion commented code

        #region ISmartSerializable

        protected NewSmartSerializable<ManagerClass> ism = new();
        public INewSmartSerializableConfig Config { get => ism.Config; set => ism.Config = value; }
        public FilePathHelper Disk { get => ism.Config.Disk; set => ism.Config.Disk = value; }
        public JsonSerializerSettings JsonSettings { get => ism.Config.JsonSettings; set => ism.Config.JsonSettings = value; }
        public FilePathHelper LocalDisk { get => ism.Config.LocalDisk; set => ism.Config.LocalDisk = value; }
        public JsonSerializerSettings LocalJsonSettings { get => ism.Config.LocalJsonSettings; set => ism.Config.LocalJsonSettings = value; }
        public FilePathHelper NetDisk { get => ism.Config.NetDisk; set => ism.Config.NetDisk = value; }
        public JsonSerializerSettings NetJsonSettings { get => ism.Config.NetJsonSettings; set => ism.Config.NetJsonSettings = value; }
        public void ActivateLocalDisk() => ism.Config.ActivateLocalDisk();
        public void ActivateNetDisk() => ism.Config.ActivateNetDisk();
        public void Serialize() => ism.Serialize();
        public void Serialize(string filePath) => ism.Serialize(filePath);
        public void SerializeThreadSafe(string filePath) => ism.SerializeThreadSafe(filePath);
        public ManagerClass Deserialize(string fileName, string folderPath) => ism.Deserialize(fileName, folderPath);
        public ManagerClass Deserialize(string fileName, string folderPath, bool askUserOnError) => ism.Deserialize(fileName, folderPath, askUserOnError);
        public ManagerClass Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) => ism.Deserialize(fileName, folderPath, askUserOnError, settings);
        public ManagerClass Deserialize(NewSmartSerializableLoader config) => ism.Deserialize(config);
        public async Task<ManagerClass> DeserializeAsync(NewSmartSerializableLoader config) => await Task.Run(()=>ism.Deserialize(config));

        public static class Static
        {
            private static NewSmartSerializable<ManagerClass> GetInstance() => new();

            public static ManagerClass Deserialize(string fileName, string folderPath) =>
            GetInstance().Deserialize(fileName, folderPath);
            
            public static ManagerClass Deserialize(string fileName, string folderPath, bool askUserOnError) =>
                GetInstance().Deserialize(fileName, folderPath, askUserOnError);
            
            public static ManagerClass Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) =>
            GetInstance().Deserialize(fileName, folderPath, askUserOnError, settings);

            public static ManagerClass Deserialize(NewSmartSerializableLoader config) =>
                GetInstance().Deserialize(config);

            public static async Task<ManagerClass> DeserializeAsync(NewSmartSerializableLoader config) =>
                await GetInstance().DeserializeAsync(config);

            internal static JsonSerializerSettings GetDefaultSettings() => 
                NewSmartSerializable<ManagerClass>.GetDefaultSettings();
        }

        #endregion ISmartSerializable
    }
}
