using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Fields
{
    public static class MAPIFields
    {
        const string PROPTAG_SPECIFIER = "http://schemas.microsoft.com/mapi/proptag/";

        // PropTag Types
        const string PT_BINARY = "0102";
        const string PT_LONG = "0003";
        const string PT_TSTRING = "001f"; /* Null-terminated 16-bit (2-byte) character string. 
                                           * Properties with this type have the property type 
                                           * reset to PT_UNICODE when compiling with the UNICODE 
                                           * symbol and to PT_STRING8 when not compiling with the 
                                           * UNICODE symbol. This property type is the same as the 
                                           * OLE type VT_LPSTR for resulting PT_STRING8 properties 
                                           * and VT_LPWSTR for PT_UNICODE properties */
        const string PT_STRING8 = "001e"; /* Null-terminated 8-bit (1-byte) character string. 
                                           * This property type is the same as the OLE type VT_LPSTR */

        const string PR_RECEIVED_BY_NAME = "0x0040"; //PidTagReceivedByName
        const string PR_STORE_ENTRYID = "0x0FFB"; //Message store PID + PT_BINARY
        const string PR_STORE_RECORD_KEY = "0x0FFA"; //
        const string PR_CONVERSATION_TOPIC = "0x0070"; // Normalized Conversation Subject for message group

        const string PR_PARENT_DISPLAY = "0x0e05"; //Message parent folder
        const string PR_DEPTH = "0x3005"; /* Represents the relative level of indentation, 
                                           * or depth, of an object in a hierarchical table
                                           * Data type is PT_LONG */
        const string PR_CONVERSATION_INDEX = "0x0071"; /* PT_BINARY ScCreateConversationIndex 
                                                        * implements the index as a header block 
                                                        * that is 22 bytes in length, followed 
                                                        * by zero or more child blocks each 
                                                        * 5 bytes in length */

        const string PR_CONVERSATION_KEY = "0x000B"; // PT_BINARY
        const string PR_CONVERSATION_ID = "0x3013"; // PT_BINARY

        const string PR_MESSAGE_RECIPIENTS = "0x0e12";
        const string PR_SENDER_NAME = "0x0C1A"; // PT_TSTRING
        const string PR_SENDER_SMTP_ADDRESS = "0x5D01"; // PT_TSTRING
        const string PR_SENDER_ADDRTYPE = "0x0C1E"; // PT_TSTRING

        public struct Schemas
        {
            public static string ConversationTopic { get; private set; } = PROPTAG_SPECIFIER + PR_CONVERSATION_TOPIC + PT_TSTRING;
            public static string FolderName { get; private set; } = PROPTAG_SPECIFIER + PR_PARENT_DISPLAY + PT_TSTRING;
            public static string MessageStore { get; private set; } = PROPTAG_SPECIFIER + PR_STORE_ENTRYID + PT_BINARY;
            public static string ConversationDepth { get; private set; } = PROPTAG_SPECIFIER + PR_DEPTH + PT_LONG;
            public static string ConversationIndex { get; private set; } = PROPTAG_SPECIFIER + PR_CONVERSATION_INDEX + PT_BINARY;
            public static string CustomPrefix { get; private set; } = "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/";
            public static string Triage { get; private set; } = "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage";
            public static string ToDoID { get; private set; } = "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/ToDoID";
            public static string ConversationId { get; private set; } = PROPTAG_SPECIFIER + PR_CONVERSATION_ID + PT_BINARY;
            public static string SenderName { get; private set; } = PROPTAG_SPECIFIER + PR_SENDER_NAME + PT_TSTRING;
            public static string SenderSmtpAddress { get; private set; } = PROPTAG_SPECIFIER + PR_SENDER_SMTP_ADDRESS + PT_TSTRING;
            public static string SenderAddrType { get; private set; } = PROPTAG_SPECIFIER + PR_SENDER_ADDRTYPE + PT_TSTRING;
            public static string ReceivedByName { get; private set; } = "http://schemas.microsoft.com/mapi/proptag/0x0040001E";
            public static string MessageRecipients { get; private set; } = "http://schemas.microsoft.com/mapi/proptag/0x0E12000D";
        }

        public static ImmutableDictionary<string, string> SchemaToField { get; private set; } = new Dictionary<string, string>()
        {
            {Schemas.FolderName, "Folder Name" },
            {Schemas.MessageStore, "Store"},
            {Schemas.ConversationDepth, "ConvDepth" },
            {Schemas.ConversationIndex, "ConversationIndex" },
            {Schemas.ConversationTopic, "ConversationTopic" },
            {Schemas.ConversationId, "ConversationId" },
            {Schemas.ToDoID, "ToDoID" },
            {Schemas.Triage, "Triage" },
            {Schemas.SenderName, "SenderName" },
            {Schemas.SenderSmtpAddress, "SenderSmtpAddress" },
            {Schemas.SenderAddrType, "SenderAddrType" },
            {Schemas.ReceivedByName, "ReceivedByName" },
            {Schemas.MessageRecipients, "MessageRecipients" }
        }.ToImmutableDictionary();


        public static ImmutableDictionary<string, string> FieldToSchema = new Dictionary<string, string>()
        {
            {"Folder Name", Schemas.FolderName },
            {"Store", Schemas.MessageStore},
            {"ConvDepth", Schemas.ConversationDepth },
            {"ConversationIndex", Schemas.ConversationIndex },
            {"ConversationTopic", Schemas.ConversationTopic },
            {"ConversationId", Schemas.ConversationId },
            {"ToDoID", Schemas.ToDoID },
            {"Triage", Schemas.Triage },
            {"SenderName", Schemas.SenderName },
            {"SenderSmtpAddress", Schemas.SenderSmtpAddress },
            {"SenderAddrType", Schemas.SenderAddrType },
            {"ReceivedByName", Schemas.ReceivedByName },
            {"MessageRecipients", Schemas.MessageRecipients }
        }.ToImmutableDictionary();

        public static ImmutableList<string> BinaryToStringFields =
        [
            "ConversationIndex",
            "ConversationId",
            "Store"//,
            //"ReceivedByName"
        ];

        public static ImmutableList<string> ObjectFields =
        [
            "MessageRecipients"
        ];
    }
}
