#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UtilitiesCS.EmailIntelligence.Bayesian.Performance
{
    public class BayesianSerializationHelper(IApplicationGlobals globals)
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private IApplicationGlobals _globals = globals;
        internal IApplicationGlobals Globals => _globals;

        #region Serialization

        public virtual T? Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));

            if (_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                var disk = new FilePathHelper { FolderPath = Path.Combine(folderRoot, "Bayesian") };
                var fileName = fileNameSuffix.IsNullOrEmpty()
                    ? $"{fileNameSeed}.json"
                    : $"{fileNameSeed}_{fileNameSuffix}.json";
                disk.FileName = fileName;
                if (FileExists(disk.FilePath))
                {
                    var item = JsonConvert.DeserializeObject<T>(
                        ReadAllText(disk.FilePath),
                        jsonSettings
                    );
                    return item;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                return default(T);
            }
        }

        public virtual async Task<T?> DeserializeAsync<T>(
            string fileNameSeed,
            string fileNameSuffix = ""
        )
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));

            if (_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                var disk = new FilePathHelper();
                disk.FolderPath = Path.Combine(folderRoot, "Bayesian");
                var fileName = fileNameSuffix.IsNullOrEmpty()
                    ? $"{fileNameSeed}.json"
                    : $"{fileNameSeed}_{fileNameSuffix}.json";
                disk.FileName = fileName;
                if (FileExists(disk.FilePath))
                {
                    var fileText = await ReadAllTextAsync(disk.FilePath);
                    var item = JsonConvert.DeserializeObject<T>(fileText, jsonSettings);
                    return item;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                return default(T);
            }
        }

        public virtual async Task<T?> DeserializeAsync<T>(
            ProgressTrackerPane progress,
            string fileNameSeed,
            string fileNameSuffix = "",
            string fileExtension = ".json"
        )
        {
            JsonSerializerSettings jsonSettings = GetJsonSettings();
            FilePathHelper disk = GetDisk(fileNameSeed, fileNameSuffix, fileExtension);

            T? item = default;

            if (FileExists(disk.FilePath))
            {
                var fileText = await ReadTextWithProgressAsync(
                    disk,
                    progress,
                    $"Reading {disk.FileName} Async: "
                );

                try
                {
                    item = JsonConvert.DeserializeObject<T>(fileText, jsonSettings);
                }
                catch (Exception e)
                {
                    logger.Error(
                        $"Error deserializing {disk.FileName}\n{e.Message}\nStack Trace:\n{e.StackTrace}",
                        e
                    );
                }
            }
            return item;
        }

        protected virtual bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        [ExcludeFromCodeCoverage]
        protected virtual string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        [ExcludeFromCodeCoverage]
        protected virtual async Task<string> ReadAllTextAsync(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                return await reader.ReadToEndAsync();
            }
        }

        [ExcludeFromCodeCoverage]
        protected virtual Task<string> ReadTextWithProgressAsync(
            FilePathHelper disk,
            ProgressTrackerPane progress,
            string messagePrefix
        )
        {
            return disk.ReadTextWithProgressAsync(progress, messagePrefix);
        }

        protected FilePathHelper GetDisk(
            string fileNameSeed,
            string fileNameSuffix,
            string extension
        )
        {
            if (_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                var disk = new FilePathHelper();
                disk.FolderPath = Path.Combine(folderRoot, "Bayesian");
                var fileName = fileNameSuffix.IsNullOrEmpty()
                    ? $"{fileNameSeed}{extension}"
                    : $"{fileNameSeed}_{fileNameSuffix}{extension}";
                disk.FileName = fileName;
                return disk;
            }
            else
            {
                // Pre-existing behavior: returns null when AppData is unavailable. null! keeps the
                // non-null return type consumed by callers that dereference disk without a guard.
                return null!;
            }
        }

        protected JsonSerializerSettings GetJsonSettings()
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));
            return jsonSettings;
        }

        public virtual async Task SaveTextsAsync(
            IEnumerable<string> texts,
            string fileNameSeed,
            string fileNameSuffix = "",
            string fileExtension = ".txt"
        )
        {
            if (_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                var disk = new FilePathHelper();
                disk.FolderPath = Path.Combine(folderRoot, "Bayesian");
                var fileName = fileNameSuffix.IsNullOrEmpty()
                    ? $"{fileNameSeed}{fileExtension}"
                    : $"{fileNameSeed}_{fileNameSuffix}{fileExtension}";

                disk.FileName = fileName;
                if (FileExists(disk.FilePath))
                {
                    DeleteFile(disk.FilePath);
                }
                await WriteTextsAsync(disk.FilePath, texts);
            }
        }

        public virtual async Task SaveCsvAsync(
            string[][] jagged,
            string fileNameSeed,
            string fileNameSuffix = ""
        )
        {
            var texts = jagged.Select(x => x.StringJoin(",")).ToArray();
            await SaveTextsAsync(texts, fileNameSeed, fileNameSuffix, ".csv");
        }

        public virtual void SerializeAndSave<T>(
            T obj,
            string fileNameSeed,
            string fileNameSuffix = ""
        )
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));

            var serializer = JsonSerializer.Create(jsonSettings);
            if (_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                var disk = new FilePathHelper();
                disk.FolderPath = Path.Combine(folderRoot, "Bayesian");
                var fileName = fileNameSuffix.IsNullOrEmpty()
                    ? $"{fileNameSeed}.json"
                    : $"{fileNameSeed}_{fileNameSuffix}.json";
                disk.FileName = fileName;
                SerializeAndSave(obj, serializer, disk);
            }
        }

        public virtual async Task SerializeAndSaveAsync<T>(
            T obj,
            ProgressTrackerPane progress,
            string fileNameSeed,
            string fileNameSuffix = "",
            string fileExtension = ".json",
            string progressPrefix = "",
            CancellationToken cancel = default
        )
        {
            var jsonSettings = GetJsonSettings();
            var serializer = JsonSerializer.Create(jsonSettings);
            var disk = GetDisk(fileNameSeed, fileNameSuffix, fileExtension);

            await SerializeWithProgressAsync(
                serializer,
                obj,
                disk,
                progress,
                cancel,
                progressPrefix
            );
        }

        [ExcludeFromCodeCoverage]
        protected virtual Task SerializeWithProgressAsync<T>(
            JsonSerializer serializer,
            T obj,
            FilePathHelper disk,
            ProgressTrackerPane progress,
            CancellationToken cancel,
            string progressPrefix
        )
        {
            return serializer.SerializeWithProgressAsync(
                obj,
                disk,
                progress,
                cancel,
                progressPrefix
            );
        }

        public virtual async Task WriteTextsAsync(string filePath, IEnumerable<string> texts)
        {
            using (var sourceStream = CreateTextWriteStream(filePath))
            {
                await texts
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async text =>
                    {
                        byte[] encodedText = Encoding.Unicode.GetBytes(text + Environment.NewLine);
                        await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                    });
            }
            ;
        }

        [ExcludeFromCodeCoverage]
        protected virtual Stream CreateTextWriteStream(string filePath)
        {
            return new FileStream(
                filePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true
            );
        }

        [ExcludeFromCodeCoverage]
        protected internal virtual void SerializeAndSave<T>(
            T obj,
            JsonSerializer serializer,
            FilePathHelper disk
        )
        {
            using (StreamWriter sw = File.CreateText(disk.FilePath))
            {
                serializer.Serialize(sw, obj);
                disk.FileName = null;
            }
        }

        #endregion Serialization
    }
}
