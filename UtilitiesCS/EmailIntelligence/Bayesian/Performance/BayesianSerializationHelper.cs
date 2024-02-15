using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.EmailIntelligence.Bayesian.Performance
{
    internal class BayesianSerializationHelper(IApplicationGlobals globals)
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private IApplicationGlobals _globals = globals;
        internal IApplicationGlobals Globals => _globals;

        #region Serialization

        public virtual T Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));

            var disk = new FilePathHelper { FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian") };
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

        public async virtual Task<T> DeserializeAsync<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));

            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian");
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

        public virtual async Task SaveTextsAsync(IEnumerable<string> texts, string fileNameSeed, string fileNameSuffix = "", string fileExtension = ".txt")
        {
            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian");
            var fileName = fileNameSuffix.IsNullOrEmpty() ?
                $"{fileNameSeed}{fileExtension}" :
                $"{fileNameSeed}_{fileNameSuffix}{fileExtension}";

            disk.FileName = fileName;
            if (File.Exists(disk.FilePath)) { File.Delete(disk.FilePath); }
            await WriteTextsAsync(disk.FilePath, texts);
        }

        public virtual async Task SaveCsvAsync(string[][] jagged, string fileNameSeed, string fileNameSuffix = "")
        {
            var texts = jagged.Select(x => x.StringJoin(",")).ToArray();
            await SaveTextsAsync(texts, fileNameSeed, fileNameSuffix, ".csv");
        }

        public virtual void SerializeAndSave<T>(T obj, string fileNameSeed, string fileNameSuffix = "")
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            jsonSettings.Converters.Add(new AppGlobalsConverter(Globals));

            var serializer = JsonSerializer.Create(jsonSettings);
            var disk = new FilePathHelper();
            disk.FolderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian");
            var fileName = fileNameSuffix.IsNullOrEmpty() ? $"{fileNameSeed}.json" : $"{fileNameSeed}_{fileNameSuffix}.json";
            disk.FileName = fileName;
            SerializeAndSave(obj, serializer, disk);
        }

        public virtual async Task WriteTextsAsync(string filePath, IEnumerable<string> texts)
        {

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await texts.ToAsyncEnumerable().ForEachAwaitAsync(async text =>
                {
                    byte[] encodedText = Encoding.Unicode.GetBytes(text + Environment.NewLine);
                    await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                });
            };
        }

        internal virtual void SerializeAndSave<T>(T obj, JsonSerializer serializer, FilePathHelper disk)
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
