using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;

namespace UtilitiesCS.EmailIntelligence
{
    public interface IAttachmentInfo
    {
        string FileExtension { get; set; }
        string FilenameSeed { get; set; }
        bool IsImage { get; }
        int Size { get; set; }
        OlAttachmentType OlAttachmentType { get; set; }
    }
}