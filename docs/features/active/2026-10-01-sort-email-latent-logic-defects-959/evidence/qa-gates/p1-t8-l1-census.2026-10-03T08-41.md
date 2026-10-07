# P1-T8 L1 Fix Census

Timestamp: 2026-10-03T08-41
Command: Edit E-A-L1 on UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs (four stacked single-value labels; the attribute above SaveCase removed); then CMD-CENSUS with PATHS-A and TOKENS-A (single path; TOTAL lines recorded)
EXIT_CODE: 0 (CMD-CENSUS process exit code)
Output Summary: the four stacked labels are present once each, no bitwise-or label remains, no HasFlag, nine exclusion attributes remain; every other token is at its BASE value.

- TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 9
- TOKEN caseYesNoToAllResponse.NoToAll: @ TOTAL = 1
- TOKEN caseYesNoToAllResponse.No: @ TOTAL = 1
- TOKEN caseYesNoToAllResponse.Yes: @ TOTAL = 1
- TOKEN caseYesNoToAllResponse.YesToAll: @ TOTAL = 1
- TOKEN |YesNoToAllResponse. @ TOTAL = 0
- TOKEN HasFlag @ TOTAL = 0
- TOKEN _attachmentsAltName=YesNoToAllResponse.Empty; @ TOTAL = 2
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
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 343
- SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = FE7317B56B8FFC0AA52CCF246352BE1425C6E4397FED5E637B862DDE748ECFDC

Acceptance check: the four labels 1 each, the pipe token 0 and HasFlag 0; [ExcludeFromCodeCoverage] 9; every other TOKENS-A total equals BASE (_attachmentsAltName=YesNoToAllResponse.Empty; 2). All three hold.
