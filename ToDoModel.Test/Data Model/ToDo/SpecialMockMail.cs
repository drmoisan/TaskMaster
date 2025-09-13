using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoModel.Test.Data_Model.ToDo
{
    internal class SpecialMockMail:MailItem
    {
        public void Close(OlInspectorClose SaveMode)
        {
            throw new NotImplementedException();
        }

        public object Copy()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Display(object Modal)
        {
            throw new NotImplementedException();
        }

        public object Move(MAPIFolder DestFldr)
        {
            throw new NotImplementedException();
        }

        public void PrintOut()
        {
            throw new NotImplementedException();
        }
                
        public ConcurrentDictionary<string, int> CallDictionary { get; set; } = new ConcurrentDictionary<string, int>();

        public void Save()
        {
            CallDictionary.AddOrUpdate(nameof(Save), 1, (key, oldValue) => oldValue + 1);
        }

        public void SaveAs(string Path, object Type)
        {
            throw new NotImplementedException();
        }

        public void ClearConversationIndex()
        {
            throw new NotImplementedException();
        }

        public MailItem Forward()
        {
            throw new NotImplementedException();
        }

        public MailItem Reply()
        {
            throw new NotImplementedException();
        }

        public MailItem ReplyAll()
        {
            throw new NotImplementedException();
        }

        public void Send()
        {
            throw new NotImplementedException();
        }

        public void ShowCategoriesDialog()
        {
            throw new NotImplementedException();
        }

        public void AddBusinessCard(ContactItem contact)
        {
            throw new NotImplementedException();
        }

        public void MarkAsTask(OlMarkInterval MarkInterval)
        {
            throw new NotImplementedException();
        }

        public void ClearTaskFlag()
        {
            throw new NotImplementedException();
        }

        public Conversation GetConversation()
        {
            throw new NotImplementedException();
        }

        public Application Application { get; set; }

        public OlObjectClass Class { get; set; }

        public NameSpace Session { get; set; }

        public object Parent { get; set; }

        public Actions Actions { get; set; }

        public Attachments Attachments { get; set; }

        public string BillingInformation { get; set; }
        public string Body { get; set; }
        public string Categories { get; set; }
        public string Companies { get; set; }

        public string ConversationIndex { get; set; }

        public string ConversationTopic { get; set; }

        public DateTime CreationTime { get; set; }

        public string EntryID { get; set; }

        public FormDescription FormDescription { get; set; }

        public Inspector GetInspector { get; set; }

        public OlImportance Importance { get; set; }

        public DateTime LastModificationTime { get; set; }

        public object MAPIOBJECT { get; set; }

        public string MessageClass { get; set; }
        public string Mileage { get; set; }
        public bool NoAging { get; set; }

        public int OutlookInternalVersion { get; set; }

        public string OutlookVersion { get; set; }

        public bool Saved { get; set; }

        public OlSensitivity Sensitivity { get; set; }

        public int Size { get; set; }

        public string Subject { get; set; }
        public bool UnRead { get; set; }

        public UserProperties UserProperties { get; set; }

        public bool AlternateRecipientAllowed { get; set; }
        public bool AutoForwarded { get; set; }
        public string BCC { get; set; }
        public string CC { get; set; }
        public DateTime DeferredDeliveryTime { get; set; }
        public bool DeleteAfterSubmit { get; set; }
        public DateTime ExpiryTime { get; set; }
        public DateTime FlagDueBy { get; set; }
        public string FlagRequest { get; set; }
        public OlFlagStatus FlagStatus { get; set; }
        public string HTMLBody { get; set; }
        public bool OriginatorDeliveryReportRequested { get; set; }
        public bool ReadReceiptRequested { get; set; }

        public string ReceivedByEntryID { get; set; }

        public string ReceivedByName { get; set; }

        public string ReceivedOnBehalfOfEntryID { get; set; }

        public string ReceivedOnBehalfOfName { get; set; }

        public DateTime ReceivedTime { get; set; }

        public bool RecipientReassignmentProhibited { get; set; }

        public Recipients Recipients { get; set; }

        public bool ReminderOverrideDefault { get; set; }
        public bool ReminderPlaySound { get; set; }
        public bool ReminderSet { get; set; }
        public string ReminderSoundFile { get; set; }
        public DateTime ReminderTime { get; set; }
        public OlRemoteStatus RemoteStatus { get; set; }

        public string ReplyRecipientNames { get; set; }

        public Recipients ReplyRecipients { get; set; }

        public MAPIFolder SaveSentMessageFolder { get; set; }

        public string SenderName { get; set; }

        public bool Sent { get; set; }

        public DateTime SentOn { get; set; }

        public string SentOnBehalfOfName { get; set; }

        public bool Submitted { get; set; }

        public string To { get; set; }
        public string VotingOptions { get; set; }
        public string VotingResponse { get; set; }

        public Links Links { get; set; }

        public ItemProperties ItemProperties { get; set; }

        public OlBodyFormat BodyFormat { get; set; }

        public OlDownloadState DownloadState { get; set; }

        public int InternetCodepage { get; set; }
        public OlRemoteStatus MarkForDownload { get; set; }

        public bool IsConflict { get; set; }

        public bool IsIPFax { get; set; }
        public OlFlagIcon FlagIcon { get; set; }
        public bool HasCoverSheet { get; set; }

        public bool AutoResolvedWinner { get; set; }

        public Conflicts Conflicts { get; set; }

        public string SenderEmailAddress { get; set; }

        public string SenderEmailType { get; set; }

        public bool EnableSharedAttachments { get; set; }
        public OlPermission Permission { get; set; }
        public OlPermissionService PermissionService { get; set; }

        public PropertyAccessor PropertyAccessor { get; set; }

        public Account SendUsingAccount { get; set; }
        public string TaskSubject { get; set ; }
        public DateTime TaskDueDate { get; set; }
        public DateTime TaskStartDate { get; set; }
        public DateTime TaskCompletedDate { get; set; }
        public DateTime ToDoTaskOrdinal { get; set; }

        public bool IsMarkedAsTask { get; set; }

        public string ConversationID { get; set; }

        public AddressEntry Sender { get; set; }
        public string PermissionTemplateGuid { get; set; }
        public object RTFBody { get; set; }

        public string RetentionPolicyName { get; set; }

        public DateTime RetentionExpirationDate { get; set; }

        public event ItemEvents_10_OpenEventHandler Open { add { } remove { } }
        public event ItemEvents_10_CustomActionEventHandler CustomAction { add { } remove { } }
        public event ItemEvents_10_CustomPropertyChangeEventHandler CustomPropertyChange { add { } remove { } }

        event ItemEvents_10_ForwardEventHandler ItemEvents_10_Event.Forward
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event ItemEvents_10_CloseEventHandler ItemEvents_10_Event.Close
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public event ItemEvents_10_PropertyChangeEventHandler PropertyChange { add { } remove { } }
        public event ItemEvents_10_ReadEventHandler Read { add { } remove { } }

        event ItemEvents_10_ReplyEventHandler ItemEvents_10_Event.Reply
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event ItemEvents_10_ReplyAllEventHandler ItemEvents_10_Event.ReplyAll
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event ItemEvents_10_SendEventHandler ItemEvents_10_Event.Send
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public event ItemEvents_10_WriteEventHandler Write { add { } remove { } }
        public event ItemEvents_10_BeforeCheckNamesEventHandler BeforeCheckNames { add { } remove { } }
        public event ItemEvents_10_AttachmentAddEventHandler AttachmentAdd { add { } remove { } }
        public event ItemEvents_10_AttachmentReadEventHandler AttachmentRead { add { } remove { } }
        public event ItemEvents_10_BeforeAttachmentSaveEventHandler BeforeAttachmentSave { add { } remove { } }
        public event ItemEvents_10_BeforeDeleteEventHandler BeforeDelete { add { } remove { } }
        public event ItemEvents_10_AttachmentRemoveEventHandler AttachmentRemove { add { } remove { } }
        public event ItemEvents_10_BeforeAttachmentAddEventHandler BeforeAttachmentAdd { add { } remove { } }
        public event ItemEvents_10_BeforeAttachmentPreviewEventHandler BeforeAttachmentPreview { add { } remove { } }
        public event ItemEvents_10_BeforeAttachmentReadEventHandler BeforeAttachmentRead { add { } remove { } }
        public event ItemEvents_10_BeforeAttachmentWriteToTempFileEventHandler BeforeAttachmentWriteToTempFile { add { } remove { } }
        public event ItemEvents_10_UnloadEventHandler Unload { add { } remove { } }
        public event ItemEvents_10_BeforeAutoSaveEventHandler BeforeAutoSave { add { } remove { } }
        public event ItemEvents_10_BeforeReadEventHandler BeforeRead { add { } remove { } }
        public event ItemEvents_10_AfterWriteEventHandler AfterWrite { add { } remove { } }
        public event ItemEvents_10_ReadCompleteEventHandler ReadComplete { add { } remove { } }
    }
}
