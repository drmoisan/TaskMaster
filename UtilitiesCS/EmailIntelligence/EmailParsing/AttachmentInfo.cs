using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;

namespace UtilitiesCS.EmailIntelligence.EmailParsing
{
    [Serializable]
    public class AttachmentInfo : IAttachmentInfo
    {
        public string FileExtension { get; set; }
        public string FilenameSeed { get; set; }
        private List<string> _imageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp"];
        public bool IsImage => _imageExtensions.Contains(FileExtension ?? ""); 
        public int Size { get; set; }
        public OlAttachmentType OlAttachmentType { get; set; }
    }
}
