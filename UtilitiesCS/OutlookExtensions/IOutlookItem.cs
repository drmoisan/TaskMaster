using Microsoft.Office.Interop.Outlook;
using System;

namespace UtilitiesCS
{
    public interface IOutlookItem
    {
        Actions Actions { get; }
        Application Application { get; }
        Attachments Attachments { get; }
        string BillingInformation { get; set; }
        string Body { get; set; }
        string Categories { get; set; }
        OlObjectClass Class { get; }
        string Companies { get; set; }
        string ConversationIndex { get; }
        string ConversationTopic { get; }
        DateTime CreationTime { get; }
        OlDownloadState DownloadState { get; }
        string EntryID { get; }
        FormDescription FormDescription { get; }
        Inspector GetInspector { get; }
        OlImportance Importance { get; set; }
        object InnerObject { get; }
        bool IsConflict { get; }
        ItemProperties ItemProperties { get; }
        DateTime LastModificationTime { get; }
        Links Links { get; }
        OlRemoteStatus MarkForDownload { get; set; }
        string MessageClass { get; set; }
        string Mileage { get; set; }
        bool NoAging { get; set; }
        long OutlookInternalVersion { get; }
        string OutlookVersion { get; }
        Folder Parent { get; }
        PropertyAccessor PropertyAccessor { get; }
        bool Saved { get; }
        OlSensitivity Sensitivity { get; set; }
        NameSpace Session { get; }
        long Size { get; }
        string Subject { get; set; }
        bool UnRead { get; set; }
        UserProperties UserProperties { get; }

        void Close(OlInspectorClose SaveMode);
        object Copy();
        void Display();
        object Move(Folder DestinationFolder);
        void PrintOut();
        void Save();
        void SaveAs(string path, OlSaveAsType type);
        void ShowCategoriesDialog();
    }
}