using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups
{
    internal class ClassifierGroupUtilities(IApplicationGlobals globals)
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IApplicationGlobals _globals = globals;
        internal IApplicationGlobals Globals => _globals;

        #region Testing Sizing and Serialization Methods

        internal virtual T Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian"); ;
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            if (File.Exists(disk.FilePath))
            {
                var item = JsonConvert.DeserializeObject<T>(
                    File.ReadAllText(disk.FilePath), jsonSettings);
                return item;
            }
            else { return default(T); }
        }

        internal async virtual Task<T> DeserializeAsync<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian"); ;
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            if (File.Exists(disk.FilePath))
            {
                string fileText = null;
                using (var reader = File.OpenText(disk.FilePath))
                {
                    fileText = await reader.ReadToEndAsync();
                }

                var item = JsonConvert.DeserializeObject<T>(fileText, jsonSettings);
                return item;
            }
            else { return default(T); }
        }

        internal virtual void SerializeAndSave<T>(T obj, string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serializer = JsonSerializer.Create(jsonSettings);
            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian");
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            SerializeAndSave(obj, serializer, disk);
        }

        internal virtual void SerializeAndSave<T>(T obj, JsonSerializer serializer, FilePathHelper disk)
        {
            Directory.CreateDirectory(disk.FolderPath);
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, obj);
                disk.FileName = null;
            }
        }

        internal virtual void SerializeFsSave<T>(T obj, string objName, JsonSerializer serializer, FilePathHelper disk)
        {
            disk.FileName = $"{objName}_Example.json";
            Directory.CreateDirectory(disk.FolderPath);
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, obj);
                sw.Close();
                disk.FileName = null;
            }
        }

        internal virtual void LogSizeComparison(string m1, long s1, string m2, long s2, string objectName)
        {
            var jagged = new string[][]
            {
                [m1, $"{s1:N0}"],
                [m2, $"{s2:N0}"],
            };

            var text = jagged.ToFormattedText(
                ["Method", "Size"],
                [Enums.Justification.Left, Enums.Justification.Right],
                $"{objectName} Size");

            //logger.Debug($"Object size calculations:\n{text}");
        }

        public virtual void SerializeActiveItem()
        {
            var (mailItem, s1) = TryLoadObjectAndGetMemorySize(() => _globals.Ol.App.ActiveExplorer().Selection[1]);
            var s2 = 0; //ObjectSize(mailItem);

            LogSizeComparison("GC Allocation", s1, "Serialization", s2, "MailItem");

            if (mailItem is not null) { SerializeMailInfo(mailItem); }

        }

        internal virtual void SerializeMailInfo(MailItem mailItem)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serializer = JsonSerializer.Create(jsonSettings);

            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian"); ;

            SerializeFsSave(mailItem, "MailItem", serializer, disk);


            var (mailInfo, sizeMailInfo1) = TryLoadObjectAndGetMemorySize(() =>
                new MailItemHelper(mailItem, _globals).LoadAll(_globals, _globals.Ol.ArchiveRoot, true));
            var sizeMailInfo2 = 0; // ObjectSize(mailInfo);
            LogSizeComparison("GC Allocation", sizeMailInfo1, "Serialization", sizeMailInfo2, "MailItemInfo");
            SerializeFsSave(mailInfo, "MailItemInfo", serializer, disk);



            var (minedInfo, sizeMinedInfo1) = TryLoadObjectAndGetMemorySize(() =>
                new MinedMailInfo(mailInfo));
            var sizeMinedInfo2 = 0; // ObjectSize(minedInfo);
            LogSizeComparison("GC Allocation", sizeMinedInfo1, "Serialization", sizeMinedInfo2, "MinedMailInfo");
            SerializeFsSave(minedInfo, "MinedMailInfo", serializer, disk);

        }

        internal virtual (T Object, long Size) TryLoadObjectAndGetMemorySize<T>(Func<T> loader, int copiesToLoad = 1)
        {
            loader.ThrowIfNull();
            if (copiesToLoad < 1) { throw new ArgumentOutOfRangeException(nameof(copiesToLoad), $"{nameof(copiesToLoad)} must be greater than 0"); }
            var start = GC.GetTotalMemory(true);
            long end = 0;

            T obj = loader();

            if (copiesToLoad > 1)
            {
                GCHandle[] objects = new GCHandle[copiesToLoad];
                try
                {
                    for (int i = 1; i < copiesToLoad; i++)
                    {
                        obj = loader();
                        var handle = GCHandle.Alloc(obj);
                        objects[i] = handle;
                    }
                    end = GC.GetTotalMemory(true);

                }
                catch (System.Exception e)
                {
                    logger.Error($"Error loading object of type {typeof(T).Name}\n{e.Message}", e);
                    return (default, 0);
                }
                finally
                {
                    for (int i = 1; i < copiesToLoad; i++)
                    {
                        if (objects[i].IsAllocated) { objects[i].Free(); }
                    }
                }
            }
            var size = (end - start) / copiesToLoad;

            return (obj, size);
        }

        internal virtual JsonSerializer GetSerializer()
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var serializer = JsonSerializer.Create(jsonSettings);
            return serializer;
        }

        public virtual void SerializeChunk(MinedMailInfo[] chunk, JsonSerializer serializer, FilePathHelper disk, int i)
        {
            disk.FileName = $"MinedMailInfo_{i:000}.json";
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, chunk);
                sw.Close();
                disk.FileName = null;
            }
            disk.FileName = null;
        }

        public async virtual Task<bool> ValidateJson<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            try
            {
                T obj = await DeserializeAsync<T>(fileNameSeed, fileNameSuffix);
                if (obj != null)
                    return true;
                else
                    return false;
            }
            catch (System.Exception e)
            {
                if (fileNameSuffix.IsNullOrEmpty())
                    logger.Error($"Error deserializing {typeof(T).Name}.json. \n{e.Message}", e);
                else
                    logger.Error($"Error deserializing {typeof(T).Name}_{fileNameSuffix}.json. \n{e.Message}", e);
                return false;
            }

        }






        #endregion Testing Sizing and Serialization Methods

        #region Helper Methods

        private string GetProgressMessage(int complete, int count, Stopwatch sw)
        {
            double seconds = complete > 0 ? sw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            var remainingSeconds = remaining * seconds;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg = $"Completed {complete} of {count} ({seconds:N2} spm) " +
                $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }

        /// <summary>
        /// If Outlook is not in offline mode, save the state and toggle it to offline mode
        /// </summary>
        /// <param name="offline"></param>
        /// <returns></returns>
        private async Task<bool> ToggleOfflineMode(bool offline)
        {
            if (!offline)
            {
                var commandBars = _globals.Ol.App.ActiveExplorer().CommandBars;
                if (!offline) { commandBars.ExecuteMso("ToggleOnline"); }
                await Task.Delay(5);
            }
            return offline;
        }

        #endregion Helper Methods

    }
}
