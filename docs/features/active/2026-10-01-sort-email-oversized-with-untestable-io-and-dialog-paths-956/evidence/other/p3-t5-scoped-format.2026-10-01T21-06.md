# P3-T5 Scoped CSharpier format of the nine changed or new files

Timestamp: 2026-10-01T21-06
Command: CMD-SCOPED-FORMAT with PATHS-NINE and TASKID p3-t5: dotnet tool run csharpier format <nine files>; then dotnet tool run csharpier check <nine files> (logs coverage\logs\p3-t5.csharpier-format.log and coverage\logs\p3-t5.csharpier-check.log, git-ignored; one trailing `Get-Date -Format "yyyy-MM-ddTHH-mm"` statement appended to read the write time)
EXIT_CODE: 0
Output Summary:
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = D81EF7DA1573DAC2F6D6392F7BB07D46FCC930D83B53832291D54A48659BBA61
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 94F7FADEF4160906B86F566F83011E5313E22F308BBA37103D21F2F48A3CCF8C
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = A619C1A7C1B98F50B39DB066CA8C4F081410AFB2CB587AA9894C919F02D8305B
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 0492724AC8BDF089BFC25FF28773E42A7EF894F166648C5FEF4EE136286E6AC3
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs = 11AB72C2F5F2C5BBA3D4E128ED9FDEC602109FD9634056AC482DBB670541CAF0
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = E67C57F42FC7CFB8E72CCFE5BBE3E896A46628123DA6F3214612BA4D86F3B634
BEFORE UtilitiesCS\Dialogs\YesNoToAllPromptSession.cs = 50E104ADB9A3ADD79A87359DEA1FC94A3781DF27284E3B0A13C2E60EB89A84C8
BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = BFACBB41EEEC99935D7FB00B4AC94BFD2C810BAFE4C27F9D4B40DFE85F9B0645
BEFORE UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs = 0A0FBBD8F51CFEE972E60718EA619F9A0FFB1670FA5A76B7635FC74AC4F40C8B
Formatter summary line (observation, not gated): Formatted 9 files in 7133ms.
FORMAT_EXIT_CODE: 0
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = D81EF7DA1573DAC2F6D6392F7BB07D46FCC930D83B53832291D54A48659BBA61
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 94F7FADEF4160906B86F566F83011E5313E22F308BBA37103D21F2F48A3CCF8C
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = A619C1A7C1B98F50B39DB066CA8C4F081410AFB2CB587AA9894C919F02D8305B
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = B1E3570AF37D4EBCB0118C39DE283F485D4AFF6401F04BFDDC2A50CBD798719E
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs = 11AB72C2F5F2C5BBA3D4E128ED9FDEC602109FD9634056AC482DBB670541CAF0
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = E67C57F42FC7CFB8E72CCFE5BBE3E896A46628123DA6F3214612BA4D86F3B634
AFTER UtilitiesCS\Dialogs\YesNoToAllPromptSession.cs = 2F18C839968EB3752FBB4A15289A991109ED9DAD6358CF0D5790FA75D797F565
AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = BFACBB41EEEC99935D7FB00B4AC94BFD2C810BAFE4C27F9D4B40DFE85F9B0645
AFTER UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs = 0A0FBBD8F51CFEE972E60718EA619F9A0FFB1670FA5A76B7635FC74AC4F40C8B
Check summary line: Checked 9 files in 2785ms.
CHECK_EXIT_CODE: 0
Observation: only the two files written by the Write tool in Phase 3 (SortEmail.TrySaveAttachment.cs and YesNoToAllPromptSession.cs) changed hash, consistent with line-ending normalization; the Edit-modified SortEmail.AttachmentSaving.cs and the six other files were unchanged. P3-T6 re-proves the whitespace-insensitive content.
Acceptance: FORMAT_EXIT_CODE 0 and CHECK_EXIT_CODE 0 (both hold).
