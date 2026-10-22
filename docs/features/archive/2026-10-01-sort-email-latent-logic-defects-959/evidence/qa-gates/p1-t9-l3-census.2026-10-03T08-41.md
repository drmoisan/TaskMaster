# P1-T9 L3 Phase-One Fix Census

Timestamp: 2026-10-03T08-41
Command: Edit E-A-L3 on UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs (adds `_attachmentsAltName = YesNoToAllResponse.Empty;` to Cleanup_Files); then CMD-CENSUS with PATHS-A and TOKENS-A (single path; TOTAL lines recorded)
EXIT_CODE: 0 (CMD-CENSUS process exit code)
Output Summary: every TOKENS-A total equals the P1 column; the alternate-name reset now occurs three times (field initializer, SaveCaseAsync release, Cleanup_Files reset).

- TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 9
- TOKEN caseYesNoToAllResponse.NoToAll: @ TOTAL = 1
- TOKEN caseYesNoToAllResponse.No: @ TOTAL = 1
- TOKEN caseYesNoToAllResponse.Yes: @ TOTAL = 1
- TOKEN caseYesNoToAllResponse.YesToAll: @ TOTAL = 1
- TOKEN |YesNoToAllResponse. @ TOTAL = 0
- TOKEN HasFlag @ TOTAL = 0
- TOKEN _attachmentsAltName=YesNoToAllResponse.Empty; @ TOTAL = 3
- TOKEN YesNoToAllResponse_ @ TOTAL = 4
- TOKEN YesNoToAll.ShowDialog( @ TOTAL = 6
- TOKEN new(YesNoToAll.ShowDialog) @ TOTAL = 0
- TOKEN AllPromptSessions @ TOTAL = 0
- TOKEN RedirectSaveFolder( @ TOTAL = 0
- TOKEN FolderPathSave=destinationPath; @ TOTAL = 1
- TOKEN FilePathHelperSaveAlt.FolderPath=destinationPath; @ TOTAL = 0
- TOKEN IsPicture @ TOTAL = 1
- TOKEN _responseSaveFile @ TOTAL = 2
- TOKEN Func<Attachment,string,Task<bool>>trySave @ TOTAL = 0
- TOKEN Func<string,bool>fileExists @ TOTAL = 0
- TOKEN File.Exists @ TOTAL = 2
- TOKEN TrySaveAttachmentAsync @ TOTAL = 3
- TOKEN SaveCaseAsync( @ TOTAL = 3
- TOKEN usingSystem.Diagnostics; @ TOTAL = 1
- TOKEN usingDeedle; @ TOTAL = 1
- TOKEN usingSDILReader; @ TOTAL = 1
- TOKEN usingOutlook= @ TOTAL = 1
- TOKEN usingUtilitiesCS; @ TOTAL = 1
- TOKEN #nullableenable @ TOTAL = 1
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 344
- SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 2A145BF2BD43921CF6F9B624501523C0C5AFC9D155BB743DA2487315966BBC79

Acceptance check: every TOKENS-A total equals the P1 column (_attachmentsAltName=YesNoToAllResponse.Empty; 3, [ExcludeFromCodeCoverage] 9, the four stacked labels 1 each, YesNoToAllResponse_ 4, YesNoToAll.ShowDialog( 6). Holds.
