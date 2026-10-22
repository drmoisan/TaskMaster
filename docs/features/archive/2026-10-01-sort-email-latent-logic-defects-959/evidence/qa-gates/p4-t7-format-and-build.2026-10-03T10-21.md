# P4-T7 Scoped Format and Test Build: BUILD RED, CS1769 (stop record)

Timestamp: 2026-10-03T10-21
Command: CMD-SCOPED-FORMAT (PATHS-A, PATHS-TSC, PATHS-TAS, PATHS-TST1; TASKID p4-t7); then CMD-BUILD-TEST (PROJECT UtilitiesCS.Test, TASKID p4-t7): msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU, resolved through vswhere, plus /nodeReuse:false, plus a normal-verbosity file logger
EXIT_CODE: 1 (scoped to the CMD-BUILD-TEST invocation, the printed MSBUILD_EXIT_CODE)
Output Summary: The format step passed (FORMAT_EXIT_CODE 0, CHECK_EXIT_CODE 0, all four files rewritten). The test build failed with 16 errors, all CS1769, all in SortEmail_SaveCase_Tests.cs and SortEmail_AttachmentSaving_Tests.cs; the UtilitiesCS production project compiled. P4-T7 acceptance not met. Stop label: P4-T7 BUILD RED (CS1769), planner amendment required.

## CMD-SCOPED-FORMAT output

```
BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = F28804C09C43D35D62458F014C4407B590855852B35B7C3A4BB711353DB386C9
BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = EEE8DC3FBB85F854D760B67200780C4236068CAAA9566F18CC17B254E69016E8
BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = FE92FDCB45DEA937269759387F66106A59F8AD15B488FBFF90F6E02C07877100
BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 752DDF312C4DACBF0626D62F0B50460FEBD06B6EB12CF5DB1F98F1F2E9912028
Formatted 4 files in 3658ms.
FORMAT_EXIT_CODE: 0
AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 5447FCB4DBE1A74F6BE83BF9628752D9F5BDE03567B75A7F6FFAAA425940B3A0
AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 373ABA5C1B277BB18E23C4AD82487EDFD1D7BC33DE2CB528DDE294DCAC21EEB4
AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = F8D30D3B6C22C1AC087650D72E467651BB91C100D3296FDF4BCD823F2C72582F
AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 900B58CF0CB9C457EBAF62375D6087452844D880EBF395A1EC259544FA158A7F
Checked 4 files in 1522ms.
CHECK_EXIT_CODE: 0
```

## CMD-BUILD-TEST labelled output

```
    0 Warning(s)
    16 Error(s)
MSBUILD_EXIT_CODE: 1
CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 0
ERROR_LINES: 32
ERROR_LINES_TEST_FILES: 32
ERROR_LINES_OTHER_FILES: 0
MISSING_SAVEATTACHMENTASYNC_6: 0
MISSING_SAVECASEASYNC_6: 0
MISSING_SAVEATTACHMENT_4: 0
MISSING_REDIRECTSAVEFOLDER: 0
ERROR_CODES: CS1769
DLL_ADVANCED: False
```

ERROR_LINES is 32 for 16 compiler errors because the file logger records each error twice (once in the target output, once in the build summary).

## Error locations (file, line and column; message with the host path removed)

Message: `error CS1769: Type 'Func<Attachment, string, Task<bool>>' from assembly 'UtilitiesCS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' cannot be used across assembly boundaries because it has a generic type argument that is an embedded interop type.`

- EmailIntelligence\SortEmail_SaveCase_Tests.cs: (126,19), (155,19), (184,19), (192,19), (220,19), (228,19), (257,19), (265,19), (292,19) (nine call sites of the six-argument SortEmail.SaveCaseAsync)
- EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs: (45,19), (78,19), (109,19), (138,19), (167,19), (175,19), (204,19) (seven call sites of the seamed SortEmail.SaveAttachmentAsync)

## Cause (observed, not repaired)

UtilitiesCS/UtilitiesCS.csproj line 223 sets `<EmbedInteropTypes>True</EmbedInteropTypes>` on the Microsoft.Office.Interop.Outlook reference, so `Attachment` is an embedded interop type in UtilitiesCS. The seam parameter `Func<Attachment, string, Task<bool>> trySave` that Listing L-A-FINAL introduces (SortEmail.AttachmentSaving.cs lines 187 and 245 after the P4-T7 format, on the six-argument SaveAttachmentAsync core and the six-argument SaveCaseAsync) therefore cannot be bound from another assembly (UtilitiesCS.Test), and the compiler rejects every test call site with CS1769. The plan's only named P4-T7 stop is CS0121 (method-group ambiguity), which was not observed. Repairing the defect requires changing the listings L-A-FINAL, L-TSC-FINAL and L-TAS-FINAL (for example, a non-generic delegate type declared in UtilitiesCS in place of the generic Func over an embedded interop type), and that change is a planner amendment. Under the stop discipline the executor does not edit a listing, a test or a gate.

## Acceptance (P4-T7, all three required)

1. FORMAT_EXIT_CODE 0 and CHECK_EXIT_CODE 0: met.
2. MSBUILD_EXIT_CODE 0 and ERROR_LINES 0: not met (1 and 32).
3. DLL_ADVANCED True: not met (False).

P4-T7 is not checked off. The compile-red span P4-T1 to P4-T7 remains open (COMPILE-RED SPAN OPEN: last completed task P4-T6; files that do not compile: UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs and UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs).
