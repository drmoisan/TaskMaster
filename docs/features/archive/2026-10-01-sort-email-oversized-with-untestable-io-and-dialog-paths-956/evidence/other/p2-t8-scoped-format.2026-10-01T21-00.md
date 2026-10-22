# P2-T8 Scoped CSharpier format of the six SortEmail files

Timestamp: 2026-10-01T21-00
Command: CMD-SCOPED-FORMAT with PATHS-SIX and TASKID p2-t8: dotnet tool run csharpier format <six files>; then dotnet tool run csharpier check <six files> (logs coverage\logs\p2-t8.csharpier-format.log and coverage\logs\p2-t8.csharpier-check.log, git-ignored)
EXIT_CODE: 0
Output Summary:
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = E13B67953EE5C77B9D6A3907B549DD038D647D5A4434F01C321CDCF810F543D2
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 713CF2898ED9EF45DE1D13EA5E91B13559A564BCB82A4DF64072CB468F19BDCA
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 093946126FF75C3272C0A6B6ED23F9DF40CB903E6C7F834B4EE297E6E9CB1515
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = DC337FBF2C3BAF8752EA7368293D7C4AC3EEF80DA66BB178836553D8EA900D0C
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs = 35F4EB66BDBA680AE7BFC89F32A911A4D0B261EEA7E1DB27E8DFACD66F536639
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 4328E2439EC11F375EC8EAF8FDBF6408CF13F37A81706F0FA244E91267A3D557
Formatter summary line (observation, not gated): Formatted 6 files in 5256ms.
FORMAT_EXIT_CODE: 0
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = D81EF7DA1573DAC2F6D6392F7BB07D46FCC930D83B53832291D54A48659BBA61
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 94F7FADEF4160906B86F566F83011E5313E22F308BBA37103D21F2F48A3CCF8C
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 03928C9F5C2102BEEADBCC703FB66D84C19EDB013CFE459A5B6C1C42714050B6
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 23050A30E8F66A7B18E73CEB9774D7969781EFA4AA89D6CD0E8AE976A0BCFEA1
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs = 11AB72C2F5F2C5BBA3D4E128ED9FDEC602109FD9634056AC482DBB670541CAF0
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = E67C57F42FC7CFB8E72CCFE5BBE3E896A46628123DA6F3214612BA4D86F3B634
Check summary line: Checked 6 files in 1959ms.
CHECK_EXIT_CODE: 0
Observation: all six hashes changed; the plan predicts this (the Write tool writes LF and CSharpier normalizes line endings per .editorconfig). P2-T9 re-proves the whitespace-insensitive content.
BOM observation after formatting: SortEmail.cs first three bytes `EF BB BF` (the byte-order mark restored in P2-T6 survived the formatter).
Acceptance: FORMAT_EXIT_CODE 0 and CHECK_EXIT_CODE 0 (both hold).
