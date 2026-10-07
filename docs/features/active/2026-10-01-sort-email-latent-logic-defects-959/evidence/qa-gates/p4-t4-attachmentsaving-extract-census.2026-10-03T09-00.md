# P4-T4 AttachmentSaving Extraction State Census

Timestamp: 2026-10-03T09-00
Command: Write tool rewrote UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs from Listing L-A-FINAL (after reading it; four leading spaces stripped) with exactly one listing line omitted, `attachmentHelper.FilePathHelperSaveAlt.FolderPath = destinationPath;` (D6 extraction step); then CMD-CENSUS with PATHS-A and TOKENS-A, and CMD-USINGS
EXIT_CODE: 0 (scoped to the CMD-USINGS payload, the last invocation; process exit code)
Output Summary: the three prompt sessions, the AllPromptSessions property, the foreach cleanup, the two excluded wrappers with their seamed cores, the destination overload calling the one-statement RedirectSaveFolder, the six-argument SaveCaseAsync, the deletion of IsPicture and _responseSaveFile, the A-side exclusion removals and the eight-directive using block are in place; every TOKENS-A total equals the EXTRACT column; the file has 315 lines.

CMD-CENSUS (single path; TOTAL lines):
- TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 3
- TOKEN caseYesNoToAllResponse.NoToAll: @ TOTAL = 1
- TOKEN caseYesNoToAllResponse.No: @ TOTAL = 1
- TOKEN caseYesNoToAllResponse.Yes: @ TOTAL = 1
- TOKEN caseYesNoToAllResponse.YesToAll: @ TOTAL = 1
- TOKEN |YesNoToAllResponse. @ TOTAL = 0
- TOKEN HasFlag @ TOTAL = 0
- TOKEN _attachmentsAltName=YesNoToAllResponse.Empty; @ TOTAL = 0
- TOKEN YesNoToAllResponse_ @ TOTAL = 0
- TOKEN YesNoToAll.ShowDialog( @ TOTAL = 0
- TOKEN new(YesNoToAll.ShowDialog) @ TOTAL = 3
- TOKEN AllPromptSessions @ TOTAL = 2
- TOKEN RedirectSaveFolder( @ TOTAL = 2
- TOKEN FolderPathSave=destinationPath; @ TOTAL = 1
- TOKEN FilePathHelperSaveAlt.FolderPath=destinationPath; @ TOTAL = 0
- TOKEN IsPicture @ TOTAL = 0
- TOKEN _responseSaveFile @ TOTAL = 0
- TOKEN Func<Attachment,string,Task<bool>>trySave @ TOTAL = 2
- TOKEN Func<string,bool>fileExists @ TOTAL = 2
- TOKEN File.Exists @ TOTAL = 2
- TOKEN TrySaveAttachmentAsync @ TOTAL = 1
- TOKEN SaveCaseAsync( @ TOTAL = 2
- TOKEN usingSystem.Diagnostics; @ TOTAL = 0
- TOKEN usingDeedle; @ TOTAL = 0
- TOKEN usingSDILReader; @ TOTAL = 0
- TOKEN usingOutlook= @ TOTAL = 0
- TOKEN usingUtilitiesCS; @ TOTAL = 0
- TOKEN #nullableenable @ TOTAL = 1
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 315
- SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = F28804C09C43D35D62458F014C4407B590855852B35B7C3A4BB711353DB386C9

CMD-USINGS (S and M are changed in Phase 5; the A row is gated here):
- USINGS A count=8 exact=True firstline=True blankafter=True
- USINGS T count=5 exact=True firstline=True blankafter=True
- USINGS U count=10 exact=True firstline=True blankafter=True
- USINGS S count=18 exact=False firstline=True blankafter=True
- USINGS M count=18 exact=False firstline=True blankafter=True
- USINGS-EXACT-FILES: 3

Acceptance check: every TOKENS-A total equals the EXTRACT column (FilePathHelperSaveAlt.FolderPath=destinationPath; 0, FolderPathSave=destinationPath; 1, RedirectSaveFolder( 2, new(YesNoToAll.ShowDialog) 3, AllPromptSessions 2, YesNoToAllResponse_ 0, YesNoToAll.ShowDialog( 0, IsPicture 0, _responseSaveFile 0, [ExcludeFromCodeCoverage] 3, TrySaveAttachmentAsync 1, SaveCaseAsync( 2); USINGS A count=8 exact=True firstline=True blankafter=True; LINES 315 (at most 499). All three hold.
