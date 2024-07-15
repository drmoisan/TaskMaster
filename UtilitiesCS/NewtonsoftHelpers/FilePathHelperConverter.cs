using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class FilePathHelperConverter : JsonConverter<FilePathHelper>
    {
        public FilePathHelperConverter(IFileSystemFolderPaths fileSystemFolders)
        {
            FileSystemFolders = fileSystemFolders;
        }

        protected IFileSystemFolderPaths _fileSystemFolders;
        internal virtual IFileSystemFolderPaths FileSystemFolders { get => _fileSystemFolders; set => _fileSystemFolders = value; }

        public override FilePathHelper ReadJson(JsonReader reader, Type objectType, FilePathHelper existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        internal (string Name, string RelativePath) GetSerializablePath(string folderPath)
        {
            string name = "";
            string relativePath = "";
            KeyValuePair<string, string> match = default;

            var matchingFolders = FileSystemFolders.SpecialFolders
                .Where(x => folderPath.Contains(x.Value));

            if (matchingFolders.Count() == 0) 
            { 
                match = new KeyValuePair<string,string>("Not Found", ""); 
            }
            else if (matchingFolders.Count() == 1) 
            {
                match = matchingFolders.First();
            }
            else if (matchingFolders.Count() > 1)
            {
                var max = matchingFolders.Max(x => x.Value.Length);
                match = matchingFolders.First(x => x.Value.Length == max);
            }
                
            name = match.Key;
            relativePath = match.Value.Length > 0 ? folderPath.Replace(match.Value, ""): folderPath;
                        
            return (name, relativePath);
        }

        public override void WriteJson(JsonWriter writer, FilePathHelper value, JsonSerializer serializer)
        {
            var (name, relativePath) = GetSerializablePath(value.FolderPath);

            writer.WriteStartObject();

            writer.WritePropertyName("FileName");
            writer.WriteValue(value.FileName);

            writer.WritePropertyName("RelativePath");
            writer.WriteValue(relativePath);

            writer.WritePropertyName("SpecialFolderName");
            writer.WriteValue(name);

            writer.WriteEndObject();
        }
    }
}
