using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UtilitiesCS.Extensions.Lazy;

namespace UtilitiesCS.EmailIntelligence.EmailParsing
{
    [Serializable]
    public class AttachmentSerializable : IAttachment
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public AttachmentSerializable() { }
        
        public AttachmentSerializable(Attachment a) 
        {             
            // Serialized Properties
            BlockLevel = a.BlockLevel;
            Class = a.Class;
            DisplayName = a.DisplayName;
            FileName = a.FileName;
            Index = a.Index;
            PathName = a.PathName;
            Position = a.Position;
            Size = a.Size;
            Type = a.Type;

            // Non-Serialized Properties
            Application = a.Application;
            Parent = a.Parent;
            PropertyAccessor = a.PropertyAccessor;
            Session = a.Session;

            // Custom Properties
            _a = a;
            _data = new Lazy<byte[]>(() => GetBytes(_a));
            _isImage = new Lazy<bool>(IsAnImage);
        }
        private Attachment _a;

        #endregion Constructors

        #region Serialized Custom Properties

        public string FileExtension { get; set; }
        public string FilenameSeed { get; set; }

        private Lazy<bool> _isImage;
        public bool IsImage 
        {
            get 
            { 
                if (_isImage is null) 
                {
                    _isImage = new Lazy<bool>(IsAnImage);
                    var caller = new System.Diagnostics.StackTrace().GetMyMethodNames().FirstOrDefault();
                    logger.Warn($"{caller} called {nameof(AttachmentSerializable)}.{nameof(IsImage)}.Get " +
                        $"before underlying {_isImage.GetType()} was set");
                }
                return _isImage.Value;
            } 
            set => _isImage = value.ToLazyValue(); 
        }

        private Lazy<byte[]> _data;
        public byte[] AttachmentData { get => _data?.Value; set => _data = value?.ToLazy(); }

        #endregion Serialized Custom Properties

        #region Serialized Standard Attachment Properties

        public OlAttachmentBlockLevel BlockLevel { get; set; }
        public OlObjectClass Class { get; set; }
        public string DisplayName { get; set; }
        public string FileName { get; set; }
        public int Index { get; set; }
        public string PathName { get; set; }
        public int Position { get; set; }
        public int Size { get; set; }
        public OlAttachmentType Type { get; set; }

        #endregion Serialized Standard Attachment Properties

        #region Helper Methods

        internal byte[] GetBytes(Attachment attachment)
        {
            const string PR_ATTACH_DATA_BIN = "http://schemas.microsoft.com/mapi/proptag/0x37010102";
            byte[] bytes = null;
            try
            {
                bytes = attachment.PropertyAccessor.GetProperty(PR_ATTACH_DATA_BIN);
            }
            catch (System.Exception) { }
            
            return bytes;
        }

        internal MemoryStream GetStream(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return stream;
        }

        private List<string> _imageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp"];
        public bool IsAnImage() => _imageExtensions.Contains(FileExtension ?? "");

        internal virtual (string FileNameSeed, string FileExtension) ParseFileName(string fileName)
        {
            var fileNameSeed = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            if (fileNameSeed.Length == 0)
            {
                fileNameSeed = extension;
                extension = "";
            }
            return (fileNameSeed, extension);
        }

        #endregion Helper Methods

        #region Non-Serialized Attachment Properties For Mocking

        [JsonIgnore]
        [field: NonSerialized]
        public Application Application { get; set; }

        [JsonIgnore]
        [field: NonSerialized]
        public object Parent { get; }

        [JsonIgnore]
        [field: NonSerialized]
        public PropertyAccessor PropertyAccessor { get; }

        [JsonIgnore]
        [field: NonSerialized]
        public NameSpace Session { get; set; }

        #endregion Non-Serialized Attachment Properties
    }
}
