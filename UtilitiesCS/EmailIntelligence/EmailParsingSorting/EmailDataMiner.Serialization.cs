#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public partial class EmailDataMiner
    {
        #region Testing Sizing and Serialization Methods

        internal virtual T? Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            if (!_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                logger.Debug(
                    $"{nameof(EmailDataMiner)}.{nameof(Deserialize)} aborting due to lack of AppData Special Folder"
                );
                return default(T);
            }

            return DeserializeFromFolder<T>(
                Path.Combine(folderRoot, "Bayesian"),
                fileNameSeed,
                fileNameSuffix,
                path => File.Exists(path),
                path => File.ReadAllText(path)
            );
        }

        internal static T? DeserializeFromFolder<T>(
            string folderPath,
            string fileNameSeed,
            string fileNameSuffix,
            Func<string, bool> fileExists,
            Func<string, string> readAllText
        )
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
            var disk = new FilePathHelper { FolderPath = folderPath };
            disk.FileName = fileNameSuffix.IsNullOrEmpty()
                ? $"{fileNameSeed}.json"
                : $"{fileNameSeed}_{fileNameSuffix}.json";
            if (fileExists(disk.FilePath))
            {
                var item = JsonConvert.DeserializeObject<T>(
                    readAllText(disk.FilePath),
                    jsonSettings
                );
                return item;
            }

            return default(T);
        }

        internal static async Task<T?> DeserializeAsync<T>(
            string folderPath,
            string fileNameSeed,
            string fileNameSuffix = ""
        )
        {
            return await DeserializeAsync<T>(
                folderPath,
                fileNameSeed,
                fileNameSuffix,
                path => File.Exists(path),
                ReadAllTextAsync
            );
        }

        internal static async Task<T?> DeserializeAsync<T>(
            string folderPath,
            string fileNameSeed,
            string fileNameSuffix,
            Func<string, bool> fileExists,
            Func<string, Task<string>> readAllTextAsync
        )
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
            var disk = new FilePathHelper { FolderPath = folderPath };
            disk.FileName = fileNameSuffix.IsNullOrEmpty()
                ? $"{fileNameSeed}.json"
                : $"{fileNameSeed}_{fileNameSuffix}.json";
            if (fileExists(disk.FilePath))
            {
                var fileText = await readAllTextAsync(disk.FilePath);
                var item = JsonConvert.DeserializeObject<T>(fileText, jsonSettings);
                return item;
            }

            return default(T);
        }

        private static async Task<string> ReadAllTextAsync(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                return await reader.ReadToEndAsync();
            }
        }

        [ExcludeFromCodeCoverage]
        internal virtual void SerializeAndSave<T>(
            T obj,
            string fileNameSeed,
            string fileNameSuffix = ""
        )
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
            var serializer = JsonSerializer.Create(jsonSettings);
            if (!_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                logger.Debug(
                    $"{nameof(EmailDataMiner)}.{nameof(SerializeAndSave)} is aborting due to lack of AppData special folder"
                );
                return;
            }
            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(folderRoot, "Bayesian");
            var fileName = fileNameSuffix.IsNullOrEmpty()
                ? $"{fileNameSeed}.json"
                : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            SerializeAndSave(obj, serializer, disk);
        }

        internal virtual void SerializeAndSave<T>(
            T obj,
            JsonSerializer serializer,
            FilePathHelper disk
        )
        {
            SerializeAndSave(
                obj,
                serializer,
                disk,
                path => Directory.CreateDirectory(path),
                path => File.CreateText(path)
            );
        }

        internal static void SerializeAndSave<T>(
            T obj,
            JsonSerializer serializer,
            FilePathHelper disk,
            Action<string> createDirectory,
            Func<string, TextWriter> createTextWriter
        )
        {
            createDirectory(disk.FolderPath);
            using (var writer = createTextWriter(disk.FilePath))
            {
                serializer.Serialize(writer, obj);
                disk.FileName = null;
            }
        }

        [ExcludeFromCodeCoverage]
        internal virtual void SerializeFsSave<T>(
            T obj,
            string objName,
            JsonSerializer serializer,
            FilePathHelper disk
        )
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

        internal virtual void LogSizeComparison(
            string m1,
            long s1,
            string m2,
            long s2,
            string objectName
        )
        {
            var jagged = new string[][] { [m1, $"{s1:N0}"], [m2, $"{s2:N0}"] };

            var text = jagged.ToFormattedText(
                ["Method", "Size"],
                [Enums.Justification.Left, Enums.Justification.Right],
                $"{objectName} Size"
            );

            //logger.Debug($"Object size calculations:\n{text}");
        }

        public virtual void SerializeActiveItem()
        {
            var (mailItem, s1) = TryLoadObjectAndGetMemorySize(() =>
                _globals.Ol.App.ActiveExplorer().Selection[1]
            );
            var s2 = 0; //ObjectSize(mailItem);

            LogSizeComparison("GC Allocation", s1, "Serialization", s2, "MailItem");

            if (mailItem is not null)
            {
                SerializeMailInfo(mailItem);
            }
        }

        [ExcludeFromCodeCoverage]
        internal virtual void SerializeMailInfo(MailItem mailItem)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
            var serializer = JsonSerializer.Create(jsonSettings);

            var disk = new FilePathHelper();
            if (!_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                logger.Debug(
                    $"{nameof(EmailDataMiner)}.{nameof(SerializeMailInfo)} aborted due to lack of AppData special folder"
                );
                return;
            }

            disk.FolderPath = Path.Combine(folderRoot, "Bayesian");
            ;
            SerializeFsSave(mailItem, "MailItem", serializer, disk);

            var (mailInfo, sizeMailInfo1) = TryLoadObjectAndGetMemorySize(() =>
                new MailItemHelper(mailItem, _globals).LoadAll(
                    _globals,
                    _globals.Ol.ArchiveRoot,
                    true
                )
            );
            var sizeMailInfo2 = 0; // ObjectSize(mailInfo);
            LogSizeComparison(
                "GC Allocation",
                sizeMailInfo1,
                "Serialization",
                sizeMailInfo2,
                "MailItemInfo"
            );
            SerializeFsSave(mailInfo, "MailItemInfo", serializer, disk);

            var (minedInfo, sizeMinedInfo1) = TryLoadObjectAndGetMemorySize(() =>
                new MinedMailInfo(mailInfo!)
            );
            var sizeMinedInfo2 = 0; // ObjectSize(minedInfo);
            LogSizeComparison(
                "GC Allocation",
                sizeMinedInfo1,
                "Serialization",
                sizeMinedInfo2,
                "MinedMailInfo"
            );
            SerializeFsSave(minedInfo, "MinedMailInfo", serializer, disk);
        }

        internal virtual (T? Object, long Size) TryLoadObjectAndGetMemorySize<T>(
            Func<T> loader,
            int copiesToLoad = 1
        )
        {
            loader.ThrowIfNull();
            if (copiesToLoad < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(copiesToLoad),
                    $"{nameof(copiesToLoad)} must be greater than 0"
                );
            }
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
                        if (objects[i].IsAllocated)
                        {
                            objects[i].Free();
                        }
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
                Formatting = Formatting.Indented,
            };
            var serializer = JsonSerializer.Create(jsonSettings);
            return serializer;
        }

        [ExcludeFromCodeCoverage]
        public virtual void SerializeChunk(
            MinedMailInfo[] chunk,
            JsonSerializer serializer,
            FilePathHelper disk,
            int i
        )
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

        public virtual async Task<bool> ValidateJson<T>(
            string fileNameSeed,
            string fileNameSuffix = ""
        )
        {
            try
            {
                if (!_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
                {
                    return false;
                }
                var folderPath = Path.Combine(folderRoot, "Bayesian");
                T? obj = await DeserializeForValidation<T>(
                    folderPath,
                    fileNameSeed,
                    fileNameSuffix
                );
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
                    logger.Error(
                        $"Error deserializing {typeof(T).Name}_{fileNameSuffix}.json. \n{e.Message}",
                        e
                    );
                return false;
            }
        }

        internal virtual Task<T?> DeserializeForValidation<T>(
            string folderPath,
            string fileNameSeed,
            string fileNameSuffix = ""
        )
        {
            return DeserializeAsync<T>(folderPath, fileNameSeed, fileNameSuffix);
        }

        #endregion Testing Sizing and Serialization Methods
    }
}
