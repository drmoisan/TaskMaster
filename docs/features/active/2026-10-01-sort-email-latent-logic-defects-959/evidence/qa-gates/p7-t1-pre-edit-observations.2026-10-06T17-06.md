# P7-T1 Pre-Edit Observations (CR-1 and CR-3)

Timestamp: 2026-10-06T17-06
Command: (1) CMD-LINE-CONDITION over coverage\final-959.cobertura.xml (the Phase 6 document left on disk by P6-T7 and P6-T8), run as pwsh -NoProfile -Command with Set-Location to the item worktree; (2) Grep tool over UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs (TAS): `^\s*\[TestMethod\]` (count mode), `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly`, `Be\(YesNoToAllResponse\.NoToAll\)`, `RR\. Scenario`, `^`; (3) Grep tool over UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs (TSC): `^using System;`, `^`, `Func<|Action|DateTime|Exception|Guid|Math\.|Console|Environment|StringComparison|Array\.|Convert\.|Tuple|Nullable|Lazy<|IDisposable|EventArgs|TimeSpan|Enum\.`; (4) Read of UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs (A) lines 143 to 145
EXIT_CODE: 0 (the CMD-LINE-CONDITION payload's process exit code, the only pwsh invocation)
ITERATION: 1
Output Summary: the Phase 6 document reads the false-before state of CR-1 (line 143 condition 50% (1/2), SaveAttachment branch-rate 0.75); the ternary still sits at A lines 143 to 145; TAS is as cited (437 lines, 11 test methods, unique E-TAS-SS4 anchors, SS4 absent); TSC is as cited (343 lines, `using System;` once at line 1) and names no System-namespace type, so P7-T3 takes its edit branch.

## CMD-LINE-CONDITION (Phase 6 document)

```
A-CLASS-NODES: 1
A-LINE-143-COUNT: 1
A-LINE-143-BRANCH: True
A-LINE-143-CONDITION: 50% (1/2)
A-SAVEATTACHMENT-BRANCH-RATE: 0.75
```

## Grep-tool and Read observations

- TAS-TESTMETHOD-LINES: 11
- TAS-SS4-NAME-LINES: 0
- TAS-NOTOALL-ASSERT-LINES: 1 (line 296: `attachments.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);`)
- TAS-RR-SUMMARY-LINES: 1 (line 300: `/// RR. Scenario: a helper built under an origin folder is redirected to a destination`)
- TAS-LINES: 437
- TSC-USING-SYSTEM-LINES: 1 (line 1: `using System;`)
- TSC-LINES: 343
- TSC-SYSTEM-IDENTIFIER-LINES: 0 (no matching line)
- A-LINE-143-TEXT: var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage
- A-LINE-144-TEXT: ? picturesOverwritePrompt
- A-LINE-145-TEXT: : attachmentsOverwritePrompt;

## Acceptance (P7-T1, all six required)

1. A-CLASS-NODES: 1, A-LINE-143-COUNT: 1 and A-LINE-143-BRANCH: True: met.
2. A-LINE-143-CONDITION: 50% (1/2) with A-SAVEATTACHMENT-BRANCH-RATE: 0.75 (the false-before state of CR-1): met.
3. A-LINE-143-TEXT, A-LINE-144-TEXT and A-LINE-145-TEXT equal the cited ternary text: met.
4. TAS-TESTMETHOD-LINES: 11, TAS-SS4-NAME-LINES: 0, TAS-NOTOALL-ASSERT-LINES: 1, TAS-RR-SUMMARY-LINES: 1 and TAS-LINES: 437: met.
5. TSC-USING-SYSTEM-LINES: 1 and TSC-LINES: 343: met.
6. TSC-SYSTEM-IDENTIFIER-LINES recorded (0; P7-T3 takes its edit branch): met.
