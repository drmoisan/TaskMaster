# 2026-10-01-sort-email-latent-logic-defects (Spec)

- **Issue:** #959
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-02T05-40
- **Status:** Proposed (awaiting orchestrator acceptance before planning)
- **Version:** 0.2
- **Work mode:** full-bug. This file is the sole acceptance-criteria source for this item; no user-story.md is produced.
- **Research record:** 2026-10-02T05-15 research under this folder's research directory (sections cited below by number).

> Formatting note for later editors: inline code spans around repository paths are reserved for files this fix creates or modifies, plus evidence artifacts under this feature folder. Files that are cited for comparison but are not changed (for example the retained SortEmail.cs partial, SortEmail_Tests.cs, EmailFiler.cs, EfcDataModel.cs, AppOlObjects.cs) are written as plain prose on purpose. Do not add code spans to them.

## Context
The #956 preparation research found four logic defects in the SortEmail static class. #956 split the class into partials and added testability seams; it neither fixed nor depended on these defects. On this branch the defects live in three partial files under UtilitiesCS/EmailIntelligence/EmailParsingSorting (research section 1, aliases A, T and U):

- **L1:** `SaveCase` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` (lines 290 to 309) has two switch labels that bitwise-or enum values, so neither label can ever match a value the enum produces.
- **L2:** the five-argument `TrySaveAttachmentAsync` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` (lines 87 to 160) retries without bound when a "Yes to all" answer is held and the save keeps failing with `UnauthorizedAccessException`. This path is live in production through EmailFiler.
- **L3:** `Cleanup_Files` in the same AttachmentSaving partial (lines 30 to 36) never resets `_attachmentsAltName` (line 27).
- **L4:** `WriteCSV_StartNewFileIfDoesNotExist` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` (lines 139 to 170) passes its `Path.Combine` arguments in reverse order (L4a), writes when the file exists instead of when it is absent (L4b), and never allocates the output array it hands to `SanitizeArray` (L4c).

Environment:
- OS/version: Windows 11 (Outlook VSTO add-in)
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: static reading of the three partial files; no command was run during research
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

L2 can hang a production filing operation (one awaiting state machine per iteration, thread pool kept busy, mail never moved). L1, L3 and L4 are latent: L1's only caller chain has no compiled external entry point; L3 causes a sticky alternate-name answer to persist for the add-in lifetime; L4 makes the moved-mails header seeding a permanent no-op.


## Repro & Evidence
Steps to Reproduce (static; none of the four has been reproduced at runtime, consistent with the issue):
1. Read `SaveCase` at lines 298 to 308 of the AttachmentSaving partial. The labels are `(YesNoToAllResponse.NoToAll | YesNoToAllResponse.No)` and `(YesNoToAllResponse.Yes | YesNoToAllResponse.YesToAll)`. The enum (UtilitiesCS/Dialogs/YesNoToAll.cs lines 14 to 21) declares Empty, Yes, No, YesToAll and NoToAll with the values zero, one, two, four and eight and no `[Flags]` attribute; every value that reaches the parameter is one of those five, so both labels are dead and every call takes `default`.
2. Read the `UnauthorizedAccessException` handler at lines 101 to 155 of the TrySaveAttachment partial and trace the recursion at lines 134 to 140 with `Response == YesToAll`, `clearReadOnly` succeeding and `SaveAsFile` throwing on every call (research section 3.2): line 108 is false after the first call, lines 115 to 118 are true on every call, line 132 keeps `YesToAll`, line 134 recurses with identical arguments. No counter, no changing state, no terminating condition.
3. Read `Cleanup_Files` at lines 30 to 36 of the AttachmentSaving partial: it assigns `Empty` to `_responseSaveFile`, `_attachmentsOverwrite` and `_picturesOverwrite` and calls `RemoveReadOnlyPrompt.Reset()`; `_attachmentsAltName` is absent (research section 11, derivation N3: four fields, three reset).
4. Read `WriteCSV_StartNewFileIfDoesNotExist` at lines 145 to 169 of the UndoAndMoveLog partial: `File.Exists(Path.Combine(strFileName, strFileLocation))` (reversed; the sole caller AppOlObjects.cs lines 301 to 304 passes a file name then a rooted folder, so `Path.Combine` returns the rooted folder and the check tests a directory), the body runs when the check is true (inverted), and `strOutput` is `null` at line 145 and never allocated before `SanitizeArray` writes `strOutput![j]` at line 183.

Expected:
- `SaveCase`: No or NoToAll saves to `filePathSaveAlt`; Yes or YesToAll saves to `filePathSave`; Empty saves nothing.
- `TrySaveAttachmentAsync`: a denial that persists after the read-only attribute has been cleared under a held YesToAll answer ends the retry and surfaces the error to the caller.
- `Cleanup_Files` resets all four prompt-answer fields and the read-only session.
- `WriteCSV_StartNewFileIfDoesNotExist` writes the header to the combination of folder then file name, only when that file is absent, without throwing.

Actual:
- `SaveCase` never saves on either branch.
- `TrySaveAttachmentAsync` loops without bound in the stated state.
- `Cleanup_Files` leaves `_attachmentsAltName` at its last value.
- `WriteCSV_StartNewFileIfDoesNotExist` is a no-op; once L4a and L4b alone were fixed it would throw `NullReferenceException` on the first header row.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: #956 research findings L1 to L4 (feature folder for issue #956 under docs/features/active, research subfolder); this item's research record sections 2.1, 3.1, 4.1 and 5.1 quote the current code with line numbers.


## Scope & Non-Goals
- In scope:
  - L1: the `SaveCase` labels and removal of its `[ExcludeFromCodeCoverage]` (decision D1).
  - L2: a bounded retry in the five-argument try-save path that surfaces a persistent denial by rethrow (decision D2).
  - L3: the missing `_attachmentsAltName` reset in `Cleanup_Files` (decision D3).
  - L4: L4a, L4b and L4c together, through a delegate seam that makes the method testable without the file system (decision D4). L4c is not listed in the issue but is required so the fix does not convert a no-op into a crash at add-in start (research section 5.2).
  - One regression test file per touched partial, two of them new; two project-file registrations; feature-folder evidence (decision D7).
- Out of scope / non-goals (paths deliberately unbackticked):
  - Issue #966 in its entirety: F1 (routing the overwrite and alternate-name prompts through `YesNoToAllPromptSession`), F2 (deleting `SaveAttachmentsOld` and `IsPicture`), F3 (removing `[ExcludeFromCodeCoverage]` from already-tested members, including `SanitizeArray`), replacing the existing `Debug.WriteLine` calls in the try-save handler, removing the outer `catch (System.Exception) { throw; }`, removing unused `using` directives in the partials, and the CR-1 wording correction to the #956 spec.
  - The private duplicate of L4 in ToDoModel/Email Utilities/SortItemsToExistingFolder.cs (lines 285 to 315).
  - The missing try/finally around `Cleanup_Files` in QuickFiler/Controllers/EfcDataModel.cs (lines 308 to 309).
  - The shape of the moved-mails header (fourteen lines, the first empty, one column name per line); this spec preserves the as-coded content.
  - Any change to EmailFiler.cs, EfcDataModel.cs, AppOlObjects.cs, UtilitiesCS/Dialogs/YesNoToAll.cs, UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs, UtilitiesCS/UtilitiesCS.csproj, the retained SortEmail.cs partial, or UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs.
- Explicitly excluded systems, integrations, or datasets: Outlook runtime, the real file system, and the modal dialog; no test touches any of them.

**Scope conflict, flagged for the orchestrator (not resolved in this spec):** issue.md lines 22 to 30 state that #966 is folded into this item by a maintainer directive dated 2026-10-02, with a scope rule that same-file defects must be fixed here rather than listed as follow-ups. Decision D8, which governs this spec, places #966 entirely out of scope, and the research record was prepared on the same basis (its header lists #966 as out of scope). This spec follows D8. If the issue.md directive stands, D7 and D8 must be re-issued and this spec revised before planning; the #966 items are therefore listed under Rollout & Follow-up as "promote or fold in, per that decision" rather than as settled follow-ups.

## Root Cause Analysis
These are legacy code paths that had no test coverage, partly because of `[ExcludeFromCodeCoverage]` attributes and dialog or file-system dependencies. #956 has merged on this branch's history, so the seams it added (the five-argument try-save overload, the `YesNoToAllPromptSession` type, `InternalsVisibleTo` for the test project) are available (research section 1.3).

- **L1.** A bitwise-or of two enum constants is itself a constant of the enum type, so the compiler accepts `NoToAll | No` (value ten) and `Yes | YesToAll` (value five) as case labels. The author intended "either of these two values", which in C# is expressed by stacked labels or a `when` guard (the asynchronous twin `SaveCaseAsync` at lines 251 to 252 and 279 to 280 uses the guard form correctly).
- **L2.** The retry is a recursive call with the same five arguments. The design relies on `ReleaseSingleAnswer` to force a re-prompt after a single Yes, but a YesToAll answer is by definition never released, so nothing in the loop changes between iterations. The only exits are a successful save, a throwing `clearReadOnly`, or a non-access exception. Returning `false` would not help: every production caller discards the boolean (research section 3.3, callers at AttachmentSaving lines 221, 266 and 281 and upstream through EmailFiler and EfcDataModel), so the mail would be moved with its attachment silently unsaved.
- **L3.** `_attachmentsAltName` was added alongside the other three fields but the reset list in `Cleanup_Files` was not extended. Its only reader, `SaveCaseAsync`, prompts only while the field is `Empty`, so a sticky YesToAll or NoToAll answer suppresses the alternate-name prompt for the rest of the add-in lifetime.
- **L4.** Three independent errors in one method mask each other: the reversed `Path.Combine` makes `File.Exists` test a directory path (false), the inverted condition then skips the body, and the body, were it reached, dereferences a null array. The combination has made the method a permanent no-op, which is why the null dereference was never observed.


## Proposed Fix

### Design summary (what changes where):

The orchestrator decisions below are binding on planning and execution. Each is recorded with its rationale.

- **D1 (L1).** Replace the two combined labels in `SaveCase` with four stacked single-value labels: `YesNoToAllResponse.NoToAll` and `YesNoToAllResponse.No` fall through to one `attachment.SaveAsFile(filePathSaveAlt)`; `YesNoToAllResponse.Yes` and `YesNoToAllResponse.YesToAll` fall through to one `attachment.SaveAsFile(filePathSave)`; `Empty` reaches `default` and makes no call. Remove `[ExcludeFromCodeCoverage]` from `SaveCase` only. Do not use `HasFlag`. Rationale: the enum is not a flags enum and no caller passes a combination; `HasFlag` would make `Empty` (zero) match every label; the method has no dialog or file-system call once `SaveAsFile` is mocked, so the exclusion no longer has a justification (research sections 2.3 and 2.4).
- **D2 (L2).** The internal five-argument `TrySaveAttachmentAsync` keeps its exact signature and becomes a forward to a new private static `TrySaveAttachmentCoreAsync` that takes the same five arguments plus `bool isRetryAfterClear`. In the `UnauthorizedAccessException` handler, before the prompt check and before any further clear, when `isRetryAfterClear` is true and the session holds `YesToAll`, the handler logs through the existing `logger` field (`logger.Error`, message and exception) and rethrows the original exception with a bare `throw;`. The recursion passes `isRetryAfterClear: true`. Rationale: rethrow is the only outcome a production caller observes (section 3.3); the same exception already propagates on the Cancel path and the XML contract names rethrow as an outcome; the condition checks the held answer because after a single Yes the answer is released and the retry must re-prompt (test T11); a private core keeps the signature that the eleven existing tests and the three-argument forward pin byte-identical (section 3.4). Existing tests T1 to T11 in `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` and the two try-save tests in SortEmail_Tests.cs must pass unchanged. The regression test T12 uses a `createDirectory` call-count tripwire, never a timeout (section 3.5).
- **D3 (L3).** Add `_attachmentsAltName = YesNoToAllResponse.Empty;` to `Cleanup_Files` beside its siblings. Test by reflection on the four private static fields with a four-row `DataRow` test in the new file `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` (shared with the L1 tests). No production accessor. Rationale: the field's only reader shows a modal dialog and calls the real-directory overload, so behavioral observation needs the F1 seams of #966, which are out of scope; an accessor would add production surface whose only consumer is a test; in-repo precedent exists for reflective access to private static fields (research section 4.4). The UT5 call-out and the order-independence argument are recorded in Test Strategy.
- **D4 (L4).** Add an internal four-argument overload `WriteCSV_StartNewFileIfDoesNotExist(string strFileName, string strFileLocation, Func<string, bool> fileExists, Action<string, string[], string> writeTextFile)`. The public two-argument overload becomes a forward passing `File.Exists` and `FileIO2.WriteTextFile` as method groups and keeps `[ExcludeFromCodeCoverage]` with a justification comment. The core tests `!fileExists(Path.Combine(strFileLocation, strFileName))`, allocates `strOutput` to the first-dimension length of `strAryOutput` before calling `SanitizeArray` (L4c), and writes once through `writeTextFile(strFileName, strOutput, strFileLocation)`. Header content is not redesigned. Rationale: the bugfix workflow requires a failing regression test first, and without a seam the only test would touch the real disk, which UT4 prohibits with no approved exception; the seam is therefore a precondition of the mandated workflow, not a widening (research section 5.5). Method groups rather than lambdas keep closures out of the excluded wrapper (coverage mechanics, section 6.5). Fixing L4a and L4b without L4c would turn the startup no-op in AppOlObjects.LoadEmailMoveWriter into a `NullReferenceException` when the moved-mails file is absent. Red-first for L4 is behavioral: the seam lands first as a behavior-preserving forward of the current body, the tests are observed RED, then the logic fix is applied.
- **D5.** Every regression test is written first and observed failing (tagged `[expect-fail]` in the plan, fail-before evidence under the feature's regression-testing evidence folder), then the minimal fix is applied.
- **D6.** Test policy: MSTest, Moq, FluentAssertions; no temporary files, no real file-system reads or writes, no `[DoNotParallelize]`, no Workers set to one, no retries, no `Thread.Sleep`, `Task.Delay` or timeouts; the tests pass under the repository runsettings file scripts/vscode/TaskMaster.cli.runsettings (Workers zero, ClassLevel scope).
- **D7.** Write set: exactly `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs`, `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs`, the two new test files `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` and `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (two Compile Include entries), plus this feature folder. No caller change, no change to YesNoToAll.cs or YesNoToAllPromptSession.cs, no change to UtilitiesCS.csproj (it already lists all six partials), SortEmail_Tests.cs unchanged.
- **D8.** Out of scope: issue #966 entirely, the ToDoModel duplicate, the EfcDataModel try/finally, and the header shape (listed in Scope & Non-Goals and in Rollout & Follow-up). See the scope-conflict note in Scope & Non-Goals.
- **D9.** Coverage: the CLAUDE.md floors (C# line at least eighty percent, branch at least seventy-five percent, on the testable denominator per UT2); new or changed members (`SaveCase`, `TrySaveAttachmentCoreAsync`, the seamed four-argument `WriteCSV_StartNewFileIfDoesNotExist` core, `Cleanup_Files`) each at least ninety percent line coverage; changed lines must not lose coverage; unreachable closing braces after a rethrow are exemptions only when identified by file and content in the comparison artifact.
- **D10.** Toolchain: exactly the CLAUDE.md C# toolchain (see Test Strategy for the commands). Evidence under this feature folder's evidence subfolders; committed test evidence is only the JaCoCo package-level projection, the one-line first-party summary and TRX-derived summaries (Markdown), never raw XML, TRX or coverage documents.

### Boundaries and invariants to preserve:

- The internal five-argument `TrySaveAttachmentAsync` signature (`this Attachment attachment, string filePathSave, Action<string> createDirectory, Action<string> clearReadOnly, YesNoToAllPromptSession removeReadOnlyPrompt`, returning `Task<bool>`) is byte-identical after the change. The two-argument and three-argument overloads and `ClearReadOnlyAttributeOnDisk` keep their bodies and their `[ExcludeFromCodeCoverage]` attributes.
- `createDirectory` is still called as the first statement of the try block, before every save attempt (pinned by SortEmail_Tests.cs lines 273 to 289).
- Outcome table after the change (research section 7): save succeeds on first or retried attempt returns true; denial with No or NoToAll returns false and releases a single answer; denial with Yes or YesToAll followed by a throwing clear returns false; denial with Yes, successful clear, denied retry re-prompts (T11); denial with YesToAll, successful clear, denied retry now logs and rethrows the original `UnauthorizedAccessException` after exactly one clear, two save attempts and one prompt, leaving YesToAll held until `Cleanup_Files`; Cancel (Empty) rethrows (T7); a non-access exception propagates (T10).
- The public two-argument `WriteCSV_StartNewFileIfDoesNotExist(string, string)` signature is unchanged, so AppOlObjects.cs compiles unchanged.
- `Cleanup_Files` remains `public static void` with no parameters; the four enum fields remain `private static`.
- The header array content and order (`Triage`, `FolderName`, `Sent_On`, `From`, `To`, `CC`, `Subject`, `Body`, `fromDomain`, `Conversation_ID`, `EntryID`, `Attachments`, `FlaggedAsTask`, at indices one to thirteen of a fourteen-row array) are unchanged.
- All three production files carry `#nullable enable`; new code must be null-clean under the nullable gate.

### Dependencies or blocked work:

- #956 is merged into this branch's history: the partial files, the five-argument overload, `YesNoToAllPromptSession` and the `InternalsVisibleTo` entries exist on disk (verified by reading them). Nothing blocks execution.
- Two decisions were required from the orchestrator and are recorded above: accepting the L4 seam (D4) and accepting the reflective static write in the L3 test with its UT5 call-out (D3).

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

| File | Change | Lines now / predicted after CSharpier (research section 6.1) |
|---|---|---|
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` | L1 labels; remove the attribute above `SaveCase`; L3 reset line | 343 / about 346 |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` | five-argument overload becomes a forward; new private `TrySaveAttachmentCoreAsync`; guard, `logger.Error`, `throw;`; XML remark on the new outcome | 173 / about 215 |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` | public overload becomes an excluded forward with a justification comment; new internal four-argument core with L4a, L4b, L4c fixed | 196 / about 235 |
| `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` | `CreateDirectoryLimit` tripwire on the private `Seams` recorder; test T12 | 376 / about 425 |
| `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` | new: three `SaveCase` tests (five rows) and the four-row `Cleanup_Files` test | 0 / about 150 to 200 |
| `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` | new: two `WriteCSV_StartNewFileIfDoesNotExist` tests with recording delegates | 0 / about 120 to 160 |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | two Compile Include items after the existing SortEmail test entries at lines 98 to 99, same form (four-space indent, backslash separators, self-closing) | n/a |

No production project-file change. No caller change.

#### Functions/classes/CLI commands impacted:

- `SortEmail.SaveCase(YesNoToAllResponse, Attachment, string, string)`: labels corrected; coverage exclusion removed.
- `SortEmail.TrySaveAttachmentAsync(this Attachment, string, Action<string>, Action<string>, YesNoToAllPromptSession)`: body becomes a forward.
- `SortEmail.TrySaveAttachmentCoreAsync(Attachment, string, Action<string>, Action<string>, YesNoToAllPromptSession, bool isRetryAfterClear)`: new, private, carries the former body plus the guard.
- `SortEmail.Cleanup_Files()`: one added reset.
- `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(string, string)`: body becomes an excluded forward.
- `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(string, string, Func<string, bool>, Action<string, string[], string>)`: new, internal, carries the fixed body.
- Test classes `SortEmail_TrySaveAttachment_Tests` (extended), `SortEmail_AttachmentSaving_Tests` (new), `SortEmail_UndoAndMoveLog_Tests` (new).

#### Data flow and validation changes:

- L4: the existence check now queries the folder-then-file combination, and the write happens only when that path is absent. The header array is allocated to the row count of the two-dimensional source before `SanitizeArray` fills it. The null-forgiving operator on the write argument may be dropped if nullable flow analysis accepts the assignment; if the by-ref call to `SanitizeArray` (whose parameter is a nullable by-ref array) makes the compiler treat the local as maybe-null again, the operator is retained. The nullable gate decides; neither outcome is an acceptance criterion.
- L2: no change to what is saved or where; only the terminating condition of the retry changes.
- L1, L3: no data-flow change beyond the corrected branch selection and the added reset.

#### Error handling and logging updates:

- L2 adds one `logger.Error` call (the log4net field declared in the retained SortEmail.cs partial, lines 25 to 27, visible to every partial) with a message naming the denied file path, for example "The file {filePathSave} is still denied after the read-only attribute was cleared.", and the caught exception as the second argument, followed by `throw;` so the original exception instance and stack propagate. This follows CLAUDE.md General Code Change Policy section 3 (fail fast, do not silently ignore) and C#4.2 (project logging pattern). The existing `Debug.WriteLine` calls in the handler are left in place (out of scope, #966).
- A short "why" comment above the guard states that the attribute was already cleared once in this call and a YesToAll answer is never asked again, so another clear-and-retry cannot change the outcome.
- No other error-handling change. The outer `catch (System.Exception) { throw; }` stays (out of scope).

#### Rollback/feature-flag considerations (if applicable):

- No feature flag. Rollback is a revert of the fix commits; the public and internal signatures that callers use are unchanged, so a revert has no caller impact.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

```csharp
// SortEmail.TrySaveAttachment.cs
internal static Task<bool> TrySaveAttachmentAsync(
    this Attachment attachment,
    string filePathSave,
    Action<string> createDirectory,
    Action<string> clearReadOnly,
    YesNoToAllPromptSession removeReadOnlyPrompt
)
{
    return TrySaveAttachmentCoreAsync(
        attachment, filePathSave, createDirectory, clearReadOnly, removeReadOnlyPrompt,
        isRetryAfterClear: false
    );
}

private static async Task<bool> TrySaveAttachmentCoreAsync(
    Attachment attachment,
    string filePathSave,
    Action<string> createDirectory,
    Action<string> clearReadOnly,
    YesNoToAllPromptSession removeReadOnlyPrompt,
    bool isRetryAfterClear
)
{
    try { /* createDirectory; await Task.Run(SaveAsFile); return true; unchanged */ }
    catch (System.UnauthorizedAccessException e)
    {
        Debug.WriteLine(e.Message);
        if (isRetryAfterClear && removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll)
        {
            logger.Error($"The file {filePathSave} is still denied after the read-only attribute was cleared.", e);
            throw;
        }
        /* prompt, clear, release: unchanged; the recursion becomes
           return await TrySaveAttachmentCoreAsync(..., isRetryAfterClear: true); */
    }
    catch (System.Exception) { throw; }
}

// SortEmail.UndoAndMoveLog.cs
[ExcludeFromCodeCoverage] // wiring of real file-system defaults only; a test call would touch the disk (UT4)
public static void WriteCSV_StartNewFileIfDoesNotExist(string strFileName, string strFileLocation)
{
    WriteCSV_StartNewFileIfDoesNotExist(strFileName, strFileLocation, File.Exists, FileIO2.WriteTextFile);
}

internal static void WriteCSV_StartNewFileIfDoesNotExist(
    string strFileName,
    string strFileLocation,
    Func<string, bool> fileExists,
    Action<string, string[], string> writeTextFile
)
{
    if (!fileExists(Path.Combine(strFileLocation, strFileName)))
    {
        var strAryOutput = new string[14, 2];
        /* thirteen header assignments unchanged */
        string[]? strOutput = new string[strAryOutput.GetLength(0)];
        SanitizeArray(strAryOutput, ref strOutput);
        writeTextFile(strFileName, strOutput!, strFileLocation);
    }
}

// SortEmail.AttachmentSaving.cs
internal static void SaveCase(YesNoToAllResponse response, Attachment attachment, string filePathSave, string filePathSaveAlt)
{
    switch (response)
    {
        case YesNoToAllResponse.NoToAll:
        case YesNoToAllResponse.No:
            attachment.SaveAsFile(filePathSaveAlt);
            break;
        case YesNoToAllResponse.Yes:
        case YesNoToAllResponse.YesToAll:
            attachment.SaveAsFile(filePathSave);
            break;
        default:
            break;
    }
}

public static void Cleanup_Files()
{
    _responseSaveFile = YesNoToAllResponse.Empty;
    _attachmentsOverwrite = YesNoToAllResponse.Empty;
    _attachmentsAltName = YesNoToAllResponse.Empty;
    _picturesOverwrite = YesNoToAllResponse.Empty;
    RemoveReadOnlyPrompt.Reset();
}
```

`FileIO2.WriteTextFile(string filename, string[] strOutput, string folderpath)` (UtilitiesCS/To Depricate/FileIO2.cs lines 36 to 48) matches `Action<string, string[], string>` and `File.Exists` has a single string overload, so both convert as method groups without a lambda.

#### Required configuration keys and defaults:

- None.

#### Backward-compatibility expectations:

- No public or internal signature used by a caller changes. The only new members are one private method and one internal overload. The behavior change visible to callers is L2's rethrow in a state that previously never returned.

#### Performance constraints (latency/throughput/memory):

- L2 removes an unbounded asynchronous loop; the bounded path performs at most two save attempts and one attribute clear per call. No other performance-relevant change. No measurement is required.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - The Outlook `Attachment` interface is mockable with Moq (demonstrated by the existing tests in both SortEmail test files).
  - `throw;` preserves the exception instance and `await` rethrows the same instance from `Task.Run`, so a `BeSameAs` assertion on the rethrown exception holds (research section 3.5, tagged as inference; the test will confirm it).
  - Moq's `Setup(...).Throws(...)` applies to every call, so a persistent failure does not depend on `SetupSequence` exhaustion semantics, which were not verified (research section 3.6).
- Constraints (budget, performance, compatibility):
  - Every changed or new C# file stays under five hundred lines after CSharpier (the existing SortEmail_Tests.cs is at 458 lines, which is why the L1 and L3 tests go to a new file; research section 6.2).
  - Tests must be parallel-safe under class-level parallelism with Workers zero.
  - No digits appear in acceptance-criteria lines (numeric-derivation validator); all counts in that section are written as words, and figures live in the body sections.
- External dependencies (services, libraries, releases):
  - None added. MSTest, Moq 4.21.0 and FluentAssertions are already referenced by the test project.

## Data / API / Config Impact
- User-facing or API changes: in the YesToAll-held persistent-denial state, a filing operation now fails with the original `UnauthorizedAccessException` (logged at error level) instead of hanging; the mail is not moved. Once L4 is fixed, the moved-mails log receives its header rows on first use when the file is absent (previously never).
- Data or migration considerations: none. Existing moved-mails files are not rewritten (the method writes only when the file is absent).
- Logging/telemetry updates (if any): one new `logger.Error` entry in the try-save handler.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy
Seeded from issue (restated; the authoritative criteria are in Acceptance Criteria):

- For each of L1 to L4, write a failing regression test first, using the #956 prompt and file-system seams, then apply the minimal fix. Mapped to AC2, AC4, AC7 and AC9.
- For L2, stop retrying on a persistent `UnauthorizedAccessException` under a held YesToAll and surface the error by rethrow. Mapped to AC3 and AC4.

- Regression tests to add or update:

| Defect | File | Test | Red mechanism before the fix | Needs a production pre-step |
|---|---|---|---|---|
| L1 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` | `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath` (DataRow No, NoToAll); `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath` (DataRow Yes, YesToAll); `SaveCase_WhenAnswerIsEmpty_DoesNotSave` (control, green in both states) | `Times.Once` on the mocked `SaveAsFile` observes zero calls | no |
| L2 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` | `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear` (T12) | the `CreateDirectoryLimit` tripwire (limit three) throws `InvalidOperationException` on the third attempt, so `ThrowAsync<UnauthorizedAccessException>` fails | no |
| L3 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` | `Cleanup_Files_ResetsEveryPromptAnswerField` (DataRow `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite`) | the `_attachmentsAltName` row reads `YesToAll` back; the other three rows are green controls | no |
| L4 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` | `WriteCSV_StartNewFileIfDoesNotExist_WhenFileIsAbsent_WritesHeaderOnceToCombinedPath` (L4-T1); `WriteCSV_StartNewFileIfDoesNotExist_WhenFileExists_DoesNotWrite` (L4-T2) | L4-T1: queried path is file-then-folder and no write occurs; L4-T2: the branch is entered and `SanitizeArray` throws `NullReferenceException` | yes: the four-argument seam overload lands first as a behavior-preserving forward of the current body, so the red is behavioral rather than a compile failure |

  T12 arrangement (research section 3.5): `Seams(YesToAll) { CreateDirectoryLimit = 3 }`; `attachment.Setup(x => x.SaveAsFile(SandboxFilePath)).Throws(denied)` with a single `denied` instance; assert the awaited call throws and `.Which.Should().BeSameAs(denied)`; `CreatedDirectories` equals the sandbox directory twice; `ClearedDirectories` equals it once; `PromptMessages` equals the expected prompt once; `Session.Response` is `YesToAll`; `SaveAsFile` verified `Times.Exactly(2)`. T3, T4 and T11 remain the controls that the bound does not fire on a successful retry or on a re-prompted single Yes.

  L4 tests: `fileExists` records the queried path and returns a scripted bool; `writeTextFile` records the file name, the lines and the folder. Rooted literal inputs under a sandbox root such as C:\Sortemail959Sandbox\logs with file name MovedMails.txt; nothing touches the disk because both delegates only record. L4-T1 asserts the queried path equals `Path.Combine(folder, fileName)`, one write with that file name and folder, fourteen lines, line index one equal to "Triage" and the last line equal to "FlaggedAsTask".

  New test files that import the Outlook interop namespace must write `System.Action` and `System.Exception` because the interop namespace declares types named `Action` and `Exception` (research section 1.3).

- Unit tests (pytest) for the fixed behavior and boundaries: n/a (C#, MSTest). The three test files above are the unit tests.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): `Empty` answer to `SaveCase` (no call); YesToAll held with a successful retry (T3, T4: bound must not fire); single Yes with a denied retry (T11: re-prompt, bound must not fire); file present (L4-T2: no write); the three control rows of the `Cleanup_Files` test.
- Error handling and logging verification: T12 asserts the rethrown instance is the original exception. The `logger.Error` call is not asserted (log4net static field; no seam is added for it); its presence is verified by reading the diff.
- Coverage impact and targets for changed lines/modules: per D9. The #956 brace exemptions for the try-save file are content-identified and must be re-derived for this item because the handler changes (a second `throw;`, body moved into the private core). Baseline captured before the first production edit; post-change after the final toolchain pass; comparison lists per-member figures for `SaveCase`, `TrySaveAttachmentCoreAsync`, the four-argument CSV core and `Cleanup_Files`.
- Toolchain commands to run (format → lint → type-check → test), exactly as in CLAUDE.md:
  1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. Invoke-MSTestWithCoverage.ps1 under scripts/vscode (the `test: MSTest with Coverage (Koverage)` VS Code task), which writes the fixed-name TRX under coverage\test-results and wraps vstest in dotnet-coverage. Each msbuild log must show no skipped CoreCompile target, so the rebuild is known to have compiled.
  Scoped run during development: filter FullyQualifiedName containing `EmailIntelligence.SortEmail_` (matches four classes; predicted total of thirty-eight tests, to be measured) with the TRX total asserted non-zero, because a zero-match filter exits zero.
- Evidence artifacts (Markdown projections only, fixed digit-free filenames, each with `Timestamp:`, `Command:`, `EXIT_CODE:` and an output summary):
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/coverage-baseline.md` (JaCoCo package-level projection plus the one-line first-party summary, before the first production edit)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/test-run-baseline.md` (TRX-derived summary)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-save-case.md` (L1, two positive tests RED)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-try-save-retry.md` (L2, T12 RED with the sentinel)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-cleanup-files.md` (L3, the `_attachmentsAltName` row RED, three rows green)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-write-csv.md` (L4, both tests RED after the seam commit and before the logic fix)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/pass-after-regression-tests.md` (scoped run, all four classes green, total asserted non-zero)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/toolchain-final-pass.md` (the four commands of the final pass with exit codes and the CoreCompile non-skip check)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-post-change.md` (JaCoCo projection plus one-line summary)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-comparison.md` (baseline versus post-change; per-member figures for the four named members; changed-line check; content-identified brace exemptions, if any)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/negative-controls.md` (each fix reverted one at a time; only its own tests fail)
- Manual validation steps (if required): none. Sandbox check: the root C:\Sortemail959Sandbox (and the #956 root C:\Sortemail956Sandbox used by the existing tests) must not exist before or after the run; record the Test-Path result in the pass-after projection.

**UT5 call-out (required in the change description).** `Cleanup_Files_ResetsEveryPromptAnswerField` writes static state of `SortEmail` through reflection. Order-independence argument (research section 4.4): the only writers of the four fields are `Cleanup_Files` and the dialog-driven members `SaveAttachment`, `SaveAttachmentAsync`, `SaveCaseAsync` and `SaveAttachmentsOld`, none of which any test executes; the only concurrent writer in a test run is `Cleanup_Files_DoesNotThrow` in SortEmail_Tests.cs, which writes `Empty`, the value the assertion expects; before the fix it cannot write `_attachmentsAltName` at all, so it cannot cause a false pass, and after the fix a concurrent reset can only make the assertion true earlier; no test writes a non-`Empty` value except this one, for its own row, immediately before its own `Cleanup_Files` call. The test is order-independent in both states and needs no `[DoNotParallelize]`.


## Acceptance Criteria
- [ ] AC1 (L1 code). In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs`, `SaveCase` contains exactly four single-value case labels, stacked so that `YesNoToAllResponse.NoToAll` and `YesNoToAllResponse.No` lead to one `attachment.SaveAsFile(filePathSaveAlt)` call and `YesNoToAllResponse.Yes` and `YesNoToAllResponse.YesToAll` lead to one `attachment.SaveAsFile(filePathSave)` call, with `default` making no call; no case label in `SaveCase` contains a bitwise-or operator and the method does not call `HasFlag`; the `[ExcludeFromCodeCoverage]` attribute immediately above `SaveCase` is removed and every other `[ExcludeFromCodeCoverage]` attribute in that file is unchanged. Verified by reading the method and by a Grep over the file confirming no pipe character inside a case label.
- [ ] AC2 (L1 tests). The new file `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` contains `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath` (DataRow rows No and NoToAll), `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath` (DataRow rows Yes and YesToAll) and `SaveCase_WhenAnswerIsEmpty_DoesNotSave`; each uses a Moq mock of the Outlook `Attachment` interface with two rooted literal paths and verifies `SaveAsFile` once on the expected path and never on the other; all rows pass after the fix; the two positive tests were observed failing before the fix, recorded in the fail-before projection for `SaveCase` named in Test Strategy with a non-zero EXIT_CODE.
- [ ] AC3 (L2 code). In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs`, the internal five-argument `TrySaveAttachmentAsync` extension keeps its exact parameter list and return type and its body is a single forward to a new `private static` `TrySaveAttachmentCoreAsync` that takes the same five arguments plus a `bool isRetryAfterClear` passed as false; the core carries no `[ExcludeFromCodeCoverage]`; the recursive call inside the core passes `isRetryAfterClear: true`; inside the `UnauthorizedAccessException` handler, before the prompt check and before any `clearReadOnly` call, when `isRetryAfterClear` is true and `removeReadOnlyPrompt.Response` equals `YesNoToAllResponse.YesToAll`, the handler calls `logger.Error` with a message naming `filePathSave` and the caught exception and then executes a bare `throw;`; `createDirectory` remains the first statement of the try block; the two-argument and three-argument overloads and `ClearReadOnlyAttributeOnDisk` keep their bodies and their existing `[ExcludeFromCodeCoverage]` attributes. Verified by reading the diff.
- [ ] AC4 (L2 regression test). `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` gains `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear` and a `CreateDirectoryLimit` property on the private `Seams` recorder whose `CreateDirectory` throws `InvalidOperationException` once the recorded calls reach the limit; the test scripts `SaveAsFile` with `Setup` (not `SetupSequence`) to throw one `UnauthorizedAccessException` instance on every call, asserts that the awaited call throws that same instance, that the created-directory record holds the sandbox directory twice, the cleared-directory record holds it once, the prompt record holds the expected prompt once, the session still holds `YesToAll`, and `SaveAsFile` was called exactly twice; it contains no timeout, `Task.Delay` or `Thread.Sleep`; it was observed failing before the fix with the sentinel `InvalidOperationException`, recorded in the fail-before projection for the try-save retry named in Test Strategy, and passes after the fix.
- [ ] AC5 (L2 unchanged behavior). The eleven pre-existing test methods of `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` are textually unchanged by the diff and pass after the fix, and the two try-save tests in SortEmail_Tests.cs (`TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` and `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave`) pass after the fix; SortEmail_Tests.cs is not listed by a name-only diff against the merge base with main.
- [ ] AC6 (L3 code). `Cleanup_Files` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` contains the statement `_attachmentsAltName = YesNoToAllResponse.Empty;` alongside the three existing resets and the `RemoveReadOnlyPrompt.Reset()` call; the four `YesNoToAllResponse` fields remain `private static`; no accessor, property or method is added to expose them.
- [ ] AC7 (L3 regression test). `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` contains `Cleanup_Files_ResetsEveryPromptAnswerField` with four DataRow rows naming `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName` and `_picturesOverwrite`; each row obtains the field by reflection with non-public static binding, asserts the field is found, sets it to `YesToAll`, calls `SortEmail.Cleanup_Files()` and asserts the field reads `Empty`; the `_attachmentsAltName` row was observed failing before the fix while the other three rows passed, recorded in the fail-before projection for `Cleanup_Files` named in Test Strategy; all four rows pass after the fix; the test carries no `[DoNotParallelize]`; the UT5 call-out with the order-independence argument appears in this spec's Test Strategy and in the pull request change description.
- [ ] AC8 (L4 code). In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs`, an `internal static` four-parameter overload of `WriteCSV_StartNewFileIfDoesNotExist` exists taking the file name, the folder, a file-existence predicate delegate named `fileExists` and a text-write action delegate named `writeTextFile` (exact delegate types as stated in Technical specifications); the public two-parameter overload's body is a single forward passing `File.Exists` and `FileIO2.WriteTextFile` as method groups with no lambda, and it carries `[ExcludeFromCodeCoverage]` preceded by a justification comment; the core evaluates the negation of `fileExists(Path.Combine(strFileLocation, strFileName))`, allocates `strOutput` with one element per row of `strAryOutput` before calling `SanitizeArray`, and calls `writeTextFile(strFileName, strOutput, strFileLocation)` once inside that branch; the thirteen header names and their order are unchanged; `SanitizeArray` and its attribute are unchanged; AppOlObjects.cs is unchanged.
- [ ] AC9 (L4 regression tests). The new file `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` contains `WriteCSV_StartNewFileIfDoesNotExist_WhenFileIsAbsent_WritesHeaderOnceToCombinedPath` (asserts the queried path equals the folder-then-file-name combination, one write with that file name and folder, fourteen lines, the line at index one equal to "Triage" and the last line equal to "FlaggedAsTask") and `WriteCSV_StartNewFileIfDoesNotExist_WhenFileExists_DoesNotWrite` (asserts no write); both use recording delegates only and touch no file system; both were observed failing after the seam commit and before the logic fix (the absent-file test on the reversed queried path and missing write, the file-exists test with `NullReferenceException`), recorded in the fail-before projection for the CSV helper named in Test Strategy; both pass after the fix.
- [ ] AC10 (registration and size). `UtilitiesCS.Test/UtilitiesCS.Test.csproj` gains exactly two Compile Include items, one for each new test file, in the existing form (four-space indent, backslash separators, self-closing), and no other project-file change; after CSharpier, each of the six C# files in the write set (the three production partials, the extended try-save test file and the two new test files) is under five hundred lines (project files and Markdown are outside that limit).
- [ ] AC11 (write set and scope). A name-only diff against the merge base with main lists only the three production partials, the extended try-save test file, the two new test files, the test project file and files under this feature folder; in particular it does not list UtilitiesCS.csproj, YesNoToAll.cs, YesNoToAllPromptSession.cs, SortEmail_Tests.cs, the retained SortEmail.cs partial, EmailFiler.cs, EfcDataModel.cs or AppOlObjects.cs; no change touches the overwrite or alternate-name prompt calls, `SaveAttachmentsOld`, `IsPicture`, the `Debug.WriteLine` calls in the try-save handler, the outer rethrow, or the `using` directives of the partials.
- [ ] AC12 (test policy). The new and changed test code uses only MSTest, Moq and FluentAssertions; a Grep over the three test files in the write set for `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `Timeout`, `Directory.CreateDirectory`, `File.Create`, `File.WriteAll` and `GetTempPath` returns zero hits; the full test run passes under the repository runsettings file (Workers zero, class-level scope) through the Invoke-MSTestWithCoverage route; the sandbox roots named in Test Strategy do not exist before or after the run, with the Test-Path results recorded in the pass-after projection.
- [ ] AC13 (toolchain). The four CLAUDE.md C# toolchain commands (CSharpier format then check; msbuild Rebuild with analyzers and code style enforced; msbuild Rebuild with warnings treated as errors; Invoke-MSTestWithCoverage) pass in one final pass with zero exit codes recorded in the toolchain projection named in Test Strategy, and each of the two msbuild logs is confirmed to contain no skipped CoreCompile target.
- [ ] AC14 (coverage). The baseline projection is captured before the first production edit and the post-change projection after the final toolchain pass, both as JaCoCo package-level projections with the one-line first-party summary; the comparison projection named in Test Strategy shows `SaveCase`, `TrySaveAttachmentCoreAsync`, the four-argument `WriteCSV_StartNewFileIfDoesNotExist` core and `Cleanup_Files` each at or above ninety percent line coverage; no changed line that was covered at baseline is uncovered after the change; the first-party line and branch figures on the testable denominator are recorded against the CLAUDE.md floors (eighty percent line, seventy-five percent branch) and are not lower than the baseline figures; any unreachable closing brace after a `throw;` in the try-save file is exempted only when listed by file and content in the comparison projection.
- [ ] AC15 (evidence format). Every evidence artifact added by the diff is a Markdown projection under this feature folder's evidence subfolders (baseline, regression-testing, qa-gates) carrying `Timestamp:`, `Command:` and `EXIT_CODE:` fields; the diff adds no file with an xml, trx or coverage extension anywhere in the repository; the fail-before projections for L1, L2, L3 and L4 each record a non-zero EXIT_CODE and name the failing test method.

## Risks & Mitigations
- Technical or operational risks:
  - L2 changes a production outcome from "never returns" to "throws". Upstream, EfcDataModel.MoveToFolderAsync has no try/finally, so the rethrow skips `Cleanup_Files` and sticky answers persist to the next operation. This is pre-existing for the Cancel and non-access paths (T7, T10) and is recorded for promotion, not fixed here.
  - The `BeSameAs` assertion in T12 depends on `await` rethrowing the same instance from `Task.Run`; if the runtime wraps it, the assertion is relaxed to type-and-message equality and the deviation is recorded in the fail-before projection.
  - Nullable flow after the by-ref `SanitizeArray` call may still require the null-forgiving operator on the write argument; the nullable gate decides, and either form is acceptable.
  - The reflective static write in the L3 test is the one deliberate deviation from the "no mutable global state" rule; the order-independence argument in Test Strategy is the mitigation and must be reproduced in the change description (UT5).
  - The scope conflict between issue.md and D8 (see Scope & Non-Goals) could invalidate D7 and this spec's write set if the maintainer directive stands.
- Mitigations and rollbacks:
  - Negative controls: revert each fix one at a time and confirm only its own tests fail (recorded in the negative-controls projection).
  - Rollback is a revert of the fix commits; no signature used by a caller changes.
  - The orchestrator confirms D7 and D8 against issue.md before the plan is generated.

## Rollout & Follow-up
- Release/rollout steps: merges with the next add-in build; no configuration or data migration. The PR description states the L2 behavior change (throw instead of hang) and the L4 first-use header write.
- Post-fix monitoring or clean-up tasks (items to promote through the potential-to-issue lifecycle, or to fold into this item if the orchestrator re-issues D8 per the scope-conflict note):
  - Issue #966 items: F1 (prompt session for the overwrite and alternate-name prompts), F2 (delete `SaveAttachmentsOld` and `IsPicture` after confirming no callers), F3 (remove `[ExcludeFromCodeCoverage]` from already-tested members, including `SanitizeArray`, which the L4 tests now exercise), replace the handler's `Debug.WriteLine` calls with the project logger, remove the outer rethrow, remove unused `using` directives, CR-1 wording correction to the #956 spec.
  - ToDoModel/Email Utilities/SortItemsToExistingFolder.cs lines 285 to 315: private duplicate of L4 with the same three defects.
  - QuickFiler/Controllers/EfcDataModel.cs lines 308 to 309: add try/finally so `Cleanup_Files` runs when the filer throws.
  - Moved-mails header shape: one column name per line (fourteen lines, the first empty) in both this copy and the ToDoModel copy; a single tab-separated header line may have been intended. Maintainer decision required.
  - Terminal sink of an exception propagated from EfcFormController, EfcHomeController and EfcDataModel was not traced (research section 3.3); confirm the user-visible handling of the new L2 rethrow.
- Links: issue #959; issue #966 (SortEmail review residuals); issue #956 (structural split and seams) and its feature folder under docs/features/active; this folder's research record (2026-10-02T05-15) and plan scaffold (2026-10-02T05-07).
