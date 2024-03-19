using Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public interface IAttachment
    {
        Application Application { get; set; }
        byte[] AttachmentData { get; set; }
        OlAttachmentBlockLevel BlockLevel { get; set; }
        OlObjectClass Class { get; set; }
        string DisplayName { get; set; }
        string FileExtension { get; set; }
        string FileName { get; set; }
        string FilenameSeed { get; set; }
        int Index { get; set; }
        bool IsImage { get; }
        object Parent { get; }
        string PathName { get; set; }
        int Position { get; set; }
        PropertyAccessor PropertyAccessor { get; }
        NameSpace Session { get; set; }
        int Size { get; set; }
        OlAttachmentType Type { get; set; }
    }
}