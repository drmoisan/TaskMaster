# P1-T4 Scoped Format of the Two New Test Files

Timestamp: 2026-10-03T08-38
Command: dotnet tool run csharpier format UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs; then dotnet tool run csharpier check over the same two paths (CMD-SCOPED-FORMAT, TASKID p1-t4)
EXIT_CODE: 0 (scoped to the csharpier check invocation; the printed CHECK_EXIT_CODE)
Output Summary: the formatter rewrote both files (line endings and one signature wrap); the read-only check passed.

- BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 503B2CE8714F7FC57461FFA4D9962A0ABD5BF0066446816ABBB266CF42B789D0
- BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = A2B3B7E1E2DE3A182EDD06FABB600B5B6364FED65E3F407AB2C9C2A76F8740DA
- Formatter summary (observation): Formatted 2 files in 2136ms.
- FORMAT_EXIT_CODE: 0
- AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 630EABD1E515A42402F81C1E23F529F2513678A62E25757870E2B5CED2877103
- AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = AB1FC708A04F39164234EBB01813AFB7998E9E10F582B4FDB7D03A881635783A
- Check summary: Checked 2 files in 721ms.
- CHECK_EXIT_CODE: 0

Acceptance check: FORMAT_EXIT_CODE 0 and CHECK_EXIT_CODE 0. Holds.
