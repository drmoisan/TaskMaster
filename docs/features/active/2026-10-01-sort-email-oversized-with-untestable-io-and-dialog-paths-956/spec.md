# 2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths (Spec)

- **Issue:** #956
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-01T07-30
- **Status:** Ready for planning
- **Version:** 0.2
- **Work Mode:** full-bug. This file is the sole acceptance-criteria source; no user-story.md is produced.
- **Research (authoritative, sections 2 to 12):** `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/research/2026-10-01T07-00-sort-email-oversized-with-untestable-io-and-dialog-paths-research.md` (cited below as "research §N").

> Formatting note for later editors: backticked repository paths in this document are the change footprint read by downstream tooling. Every file the fix creates or modifies is backticked (see `## Write Set`). Files that are cited but not modified (callers, the existing test class, the runsettings file, the #945 plan) are deliberately written as plain prose. Do not add backticks to them.

## Context
The SortEmail source file (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs at merge-base) is 1,454 lines, nearly three times the 500-line limit in CLAUDE.md §4 (research §1.1, derivation N1 in research §12). It also mixes sorting logic with direct file-system and WinForms dialog calls that tests cannot reach. #945 added one injected seam for directory creation, and its review reported three residual follow-ups, recorded here together because they share one cause and one file:

1. The file exceeds the size limit.
2. `TrySaveAttachmentAsync` carries `[ExcludeFromCodeCoverage]` because it calls `YesNoToAll.ShowDialog`, and its `UnauthorizedAccessException` branches are untested.
3. Other helpers in the class call `System.IO.File` and `System.IO.Directory` directly.

Environment:
- OS/version: n/a
- Language/runtime: C#, .NET Framework 4.8.1 (UtilitiesCS.csproj, legacy non-SDK project; research §3)
- Command/flags used: line count and review of the SortEmail source after the #945 PR
- Data source or fixture: n/a

Impact / Severity: Medium (as recorded in issue.md).


## Repro & Evidence
Steps to Reproduce:
1. Count the lines in the SortEmail source file at merge-base: 1,454 (research §12 N1).
2. Read `TrySaveAttachmentAsync` (merge-base lines 893 to 984). Both overloads carry `[ExcludeFromCodeCoverage]` (lines 893 and 912); the core calls `YesNoToAll.ShowDialog(message)` at line 936 and reads and writes the static field `_removeReadOnly` at lines 932 to 971. Neither existing try-save test (SortEmail_Tests lines 246 and 273) reaches the `UnauthorizedAccessException` handler.
3. Search the class for direct `System.IO.File`, `System.IO.Directory`, `DirectoryInfo`, `FileInfo` and `FileIO2.WriteTextFile` calls: 14 sites (research §6, derivation N5).

Expected:
- Each file is under 500 lines, with cohesive partial classes or extracted types.
- UI prompts and file-system access sit behind injectable seams, so the coverage exclusion can be removed and the error branches tested.

Actual:
- The file is over the limit.
- The dialog branch forces a coverage exclusion.
- Other helpers still touch the real file system (research §6 inventory).

Logs / Screenshots:
- No logs or screenshots attached; the defect is structural.
- Snippet: #945 executor and review follow-ups (#945 review, code-review.2026-09-30T13-00.md line 60).


## Scope & Non-Goals
- In scope:
  - Split the SortEmail class into six files of one `public static partial class SortEmail` (orchestrator decision D1).
  - Put the read-only-removal prompt in `TrySaveAttachmentAsync` behind a per-call `YesNoToAllPromptSession`, and put the read-only attribute write behind a per-call `Action<string> clearReadOnly` (decision D2).
  - Add regression tests T1 to T11 and session tests S1 to S7 (decision D3).
  - Register the new files in both project files (decision D4).
  - Record the file-system inventory with a seam or a justification for every site (decision D5).
- Out of scope / non-goals:
  - Latent defects L1 to L4 and follow-ups F1 to F3 of research §9 (decision D6). They are listed under Rollout & Follow-up for promotion and are not fixed here. In particular, the unbounded `YesToAll` retry (L2) is preserved exactly and no test exercises it.
  - Any change to the `YesNoToAll.ShowDialog` sites outside the try-save path (merge-base lines 710, 733, 773, 796, 853, 1263, 1280). These remain direct calls; F1 covers them.
  - Removing `[ExcludeFromCodeCoverage]` from already-tested members (F3).
  - Changes to any caller. The callers in TaskMaster/Ribbon/RibbonController.cs, TaskMaster/AppGlobals/AppOlObjects.cs, QuickFiler/Controllers/EfcDataModel.cs and UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs are unchanged (research §9).
  - Changes to the existing test class UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs. It is part of the specification and must stay unmodified and green.
- Explicitly excluded systems, integrations, or datasets: live Outlook, the real file system, and modal WinForms dialogs are not exercised by any test.

The paths in this section are deliberately unbackticked because the fix does not modify them.

## Root Cause Analysis
Legacy static-class design (`SortEmail` is a non-partial `static class`, merge-base line 23) with I/O and UI inline. Responsibility groups that grew independently (MailItem sort, MailItemHelper sort, attachment saving, try-save, the dead legacy routine, undo and move logging; research §1.2) accumulated in one file. Inside the try-save core, the prompt is a hard call to `YesNoToAll.ShowDialog` and the sticky answer is the static field `_removeReadOnly`. A test cannot answer the prompt, and it cannot set or observe the sticky state without racing other test classes: `UtilitiesCS.Test` runs class-level parallel (Workers 0, Scope ClassLevel in the CLI runsettings), and `Cleanup_Files_DoesNotThrow` already writes the same field concurrently (research §4.3). Per-call delegate seams, as used in #945, are safe under parallel tests. Settable static seams are not.


## Proposed Fix

### Design summary (what changes where):

**Decision D1 — partial-class split (research §2).** Line 23 becomes `public static partial class SortEmail`. Members move verbatim into six files under UtilitiesCS/EmailIntelligence/EmailParsingSorting/ per the research §2.2 file map:

| File | Members (merge-base line ranges) |
|---|---|
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (retained) | `logger` 27-29; `InitializeSortToExisting` 31-40; `SortAsync` (MailItemHelper) 112-179; `UpdatePredictiveEngineAsync` 181-208; `ProcessMailItemAsync` 210-301; `ResolvePaths(Folder, ...)` 1059-1103 |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` | `SortAsync` (Explorer) 42-74; `SortAsync` (IList of MailItem, 7 args) 76-110; `SortAsync` (IList of MailItem, 9 args) 303-453; `Sort` 455-553; `ResolvePaths(IList of MailItem, ...)` 1018-1057 |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` | prompt-state fields 624-627 (`_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite`); `Cleanup_Files` 555-561; `GetAttachmentsInfo` 636-659; `GetAttachmentsInfoAsync` 661-699; `SaveAttachment` 701-760; `SaveAttachmentAsync` 762-823; `SaveAttachmentAsync(dest)` 825-837; `SaveCaseAsync` 839-886; `SaveCase` 986-1005; `IsPicture` 1007-1016; `SaveMessageAsMsgAsync` 1105-1114; `SaveMessageAsMSG` 1116-1123 |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` | `TrySaveAttachmentAsync` overloads 888-984 plus the D2 seam (new core, adapter, `RemoveReadOnlyPrompt` field) |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` | `MAX_PATH` 630; `SaveAttachmentsOld` 1125-1336 |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` | `UndoAsync` 563-618; `PushToUndoStack` 1341-1351; `CaptureMoveDetails` 1353-1366; `SanitizeArrayLineTSV` 1368-1384; `StripTabsCrLf` 1386-1396; `WriteCSV_StartNewFileIfDoesNotExist` 1398-1429; `SanitizeArray` 1431-1452 |

Rules for every partial file: line 1 is `#nullable enable` (the moved members use nullable annotations; without the directive each annotation raises CS8632, which the type-check gate promotes to an error); the full using block of merge-base lines 2 to 19 is copied verbatim; the three class-level `#region` pairs (25/620, 622/632, 634/1338) are dropped because a region cannot span files; method-internal regions and `#pragma warning disable/restore CS0618` pairs move with their methods unchanged. No behavior change, no call-site change, no member signature removed or changed.

**Decision D2 — prompt seam, design A (research §4.5).**

- New type `UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs`: `internal sealed class YesNoToAllPromptSession` in namespace `UtilitiesCS`, `#nullable enable`, with:
  - `internal YesNoToAllPromptSession(Func<string, YesNoToAllResponse> showDialog)`, throwing `ArgumentNullException` on null.
  - `internal YesNoToAllResponse Response { get; private set; }`, initially `Empty`.
  - `internal YesNoToAllResponse Ask(string message)`: when `Response` is `Empty`, sets `Response = showDialog(message)`; returns `Response`.
  - `internal void ReleaseSingleAnswer()`: when `Response` is `Yes` or `No`, sets it to `Empty`; `YesToAll` and `NoToAll` are kept.
  - `internal void Reset()`: sets `Response` to `Empty`.
- In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs`:
  1. Two-argument overload `TrySaveAttachmentAsync(this Attachment, string filePathSave)`: signature unchanged; keeps `[ExcludeFromCodeCoverage]` and the #945 lambda `path => System.IO.Directory.CreateDirectory(path)`. Justification comment in code: its only behavior is wiring the real directory-creation default, and calling it from a test would create a real directory (UT4).
  2. Three-argument overload `TrySaveAttachmentAsync(this Attachment, string filePathSave, Action<string> createDirectory)`: signature unchanged; **`[ExcludeFromCodeCoverage]` removed**; becomes a one-statement forward to the core with `ClearReadOnlyAttributeOnDisk` and `RemoveReadOnlyPrompt`. The two existing #945 tests cover it.
  3. New core `internal static async Task<bool> TrySaveAttachmentAsync(this Attachment attachment, string filePathSave, Action<string> createDirectory, Action<string> clearReadOnly, YesNoToAllPromptSession removeReadOnlyPrompt)`, with **no** `[ExcludeFromCodeCoverage]`. Its body is the merge-base body of lines 919 to 983 with three substitutions: `_removeReadOnly` reads become `removeReadOnlyPrompt.Response`, the conditional `YesNoToAll.ShowDialog(message)` assignment becomes `removeReadOnlyPrompt.Ask(message)`, the two reset sites (958, 971) become `removeReadOnlyPrompt.ReleaseSingleAnswer()`, and the `DirectoryInfo` attribute write becomes `clearReadOnly(directory)` inside the existing inner `try`. `var directory = Path.GetDirectoryName(filePathSave);` is computed **before** the inner `try`, as merge-base line 944 does, so the exception boundary is unchanged. The recursive retry passes all three seams.
  4. `private static readonly YesNoToAllPromptSession RemoveReadOnlyPrompt = new(YesNoToAll.ShowDialog);` replaces the `_removeReadOnly` field (merge-base line 628). It is the production sticky state, not a seam: it cannot be replaced and no test passes it.
  5. `[ExcludeFromCodeCoverage] private static void ClearReadOnlyAttributeOnDisk(string directoryPath)` holds the two original statements (`new DirectoryInfo(directoryPath)` and `di.Attributes &= ~FileAttributes.ReadOnly`). Justification comment in code: a file-system adapter whose execution requires the real file system (UT4). Passed as a method group, so no closure is introduced.
- `Cleanup_Files` calls `RemoveReadOnlyPrompt.Reset()` in place of merge-base line 560. Its other three statements are unchanged (L3 is not fixed).
- No settable static seam is introduced anywhere.

Rejected alternatives (research §4.4): B keeps the static field and therefore the race; C (`ref` state) is illegal in `async` methods (CS1988); D (`AsyncLocal`) loses stickiness across consecutive awaited calls and changes production behavior; E (`StrongBox` plus a separate `Func`) splits one concept into two coupled parameters; F (settable static `Func`) is excluded by the issue and the parallel-run rule; G (interface seam) is recorded as a deliberate deviation from interface-first, to stay consistent with the #945 delegate seam already pinned by two tests.

**Decision D5 — file-system inventory (research §6, derivation N5).** Each direct file-system call site at merge-base receives a seam or a recorded justification:

| Merge-base line | Call | Enclosing member | Destination file after split | Disposition |
|---|---|---|---|---|
| 245 | `File.Delete` (in `Task.Run`) | `ProcessMailItemAsync` | SortEmail.cs (retained) | J1 |
| 369 | `File.Delete` | `SortAsync` (MailItem, 9 args) | SortEmail.MailItemSort.cs | J1 |
| 515 | `File.Delete` | `Sort` | SortEmail.MailItemSort.cs | J1 |
| 704 | `File.Exists` | `SaveAttachment` | SortEmail.AttachmentSaving.cs | J2 |
| 767 | `File.Exists` | `SaveAttachmentAsync(this AttachmentHelper)` | SortEmail.AttachmentSaving.cs | J3 |
| 902 | `System.IO.Directory.CreateDirectory` | `TrySaveAttachmentAsync` (2 args) | SortEmail.TrySaveAttachment.cs | Already seamed by #945; production default kept in the excluded wrapper (D2 item 1) |
| 944/947 | `new DirectoryInfo`, `Attributes` write | `TrySaveAttachmentAsync` core | SortEmail.TrySaveAttachment.cs | **Seam**: `clearReadOnly`; production default `ClearReadOnlyAttributeOnDisk` (D2 item 5) |
| 1212, 1214, 1216, 1218, 1227 | `File.Exists` / `File.Delete` | `SaveAttachmentsOld` | SortEmail.LegacyAttachmentSaving.cs | J4 |
| 1406 | `File.Exists` | `WriteCSV_StartNewFileIfDoesNotExist` | SortEmail.UndoAndMoveLog.cs | J5 |
| 1425 | `FileIO2.WriteTextFile` | `WriteCSV_StartNewFileIfDoesNotExist` | SortEmail.UndoAndMoveLog.cs | J5 |

COM-interface writes (`Attachment.SaveAsFile`, `MailItem.SaveAs`) are already mockable through Moq and need no seam. `Path.*` calls are pure string operations.

Justifications:

- **J1.** The enclosing method is a pre-existing `[ExcludeFromCodeCoverage]` Outlook-interop orchestration path (`MailItem.Move`, `Folder`, `Explorer.Selection`) with no injectable COM seam. The file delete is one statement inside it, and seaming it would not make the method testable. This is recorded as a **pre-existing, unchanged exclusion**. It is explicitly **not** a new claim under CLAUDE.md UT2 exemption (c), which names `TaskVisualization`, `QuickFiler`, `TaskMaster`, `ToDoModel` and `Tags` and does not list `UtilitiesCS`. The maintainer may need to ratify this wording (see Risks).
- **J2.** As J1: a pre-existing, unchanged exclusion and not a new UT2 (c) claim. `SaveAttachment` is reachable only from the synchronous `Sort`, which has no compiled external caller.
- **J3.** Live path (EmailFiler line 445 to merge-base 825-837 to here). The method also holds two `YesNoToAll.ShowDialog` sites and the static `_picturesOverwrite` / `_attachmentsOverwrite` state, so a `File.Exists` seam alone would not make it testable. Extending the prompt-session design to those sites is follow-up F1.
- **J4.** Dead code with zero callers (definition only). A follow-up (F2) deletes it rather than seaming unreachable code.
- **J5.** Called once at add-in start (AppOlObjects line 301) to seed the moved-mails CSV. The `File.Exists` branch is effectively unreachable in production and defective (L4). A seam would only cover a defective branch, so the justification is recorded and L4 is promoted.

**Decision D7 — coverage (research §10).** Changed lines must not lose coverage. `YesNoToAllPromptSession` and the new five-argument core must each reach at least 90% line coverage. Because the SortEmail source is split, the per-file comparison aggregates over every filename matching EmailParsingSorting\SortEmail*.cs in the post-change Cobertura projection against SortEmail.cs in the baseline projection; the expected direction of the uncovered-line count is downward, because the try-save core becomes measured and covered.

### Invariant established by the fix

For every call of the five-argument core, the outcome is determined solely by the supplied `createDirectory`, `clearReadOnly`, prompt session state and answer, and `SaveAsFile` outcome, exactly as merge-base lines 919 to 983 determine it from the static field and the real dialog: the prompt is asked only when the session state is `Empty`; the read-only attribute is cleared only after `Yes` or `YesToAll`; a retry happens only after a clear that did not throw; `No`, `NoToAll` or a throwing clear returns `false`; a Cancel (`Empty` answer) rethrows the original `UnauthorizedAccessException`; any other exception propagates unchanged; and only single answers (`Yes`, `No`) are released after use. No test can reach `YesNoToAll.ShowDialog`, the real file system, or any static field of `SortEmail`.

### Trace of one accepted value (production path, no guard between accept and handler)

1. **Accept point.** `EmailFiler.SaveAttachmentAsync` (EmailFiler line 445) calls the `SaveAttachmentAsync(this AttachmentHelper, string destinationPath)` extension (merge-base 825-837), which calls `SaveAttachmentAsync(this AttachmentHelper)`, which reaches the two-argument `TrySaveAttachmentAsync` at merge-base line 819. No guard on this path inspects the directory's attributes or the user's access.
2. **Forwarding.** Two-argument wrapper to the three-argument overload (real `Directory.CreateDirectory`) to the five-argument core with `ClearReadOnlyAttributeOnDisk` and the production `RemoveReadOnlyPrompt`.
3. **Throw point.** `createDirectory(Path.GetDirectoryName(filePathSave))` succeeds; `attachment.SaveAsFile(filePathSave)` throws `UnauthorizedAccessException` because the target folder is read-only.
4. **Handler.** `RemoveReadOnlyPrompt.Response` is `Empty`, so `Ask` shows the modal dialog through `YesNoToAll.ShowDialog`; the user answers `YesToAll`. `directory` is computed before the inner `try`; `clearReadOnly(directory)` runs inside it and succeeds; the `finally` calls `ReleaseSingleAnswer()`, which keeps `YesToAll`; the core retries and `SaveAsFile` succeeds; the call returns `true`.
5. **Stickiness.** The next attachment in the same filing operation that hits a read-only folder reuses `YesToAll` without a dialog, as merge-base line 932 does today.
6. **Release.** After the filing operation, QuickFiler's EfcDataModel line 309 calls `Cleanup_Files`, which calls `RemoveReadOnlyPrompt.Reset()`, as merge-base line 560 does today.

Test T4 reproduces steps 3 to 5 with a fresh session and recording delegates. Why both halves are needed: the session type alone would still leave the core calling the real dialog and the real attribute write, so the handler stays untestable; the delegate parameters alone would still leave sticky state in a static field that parallel test classes race on.

**Must not be widened.** The outer `catch (System.Exception) { throw; }` and the inner `try` boundary around the attribute clear stay exactly as at merge-base. No new `catch` is added, the `createDirectory` call stays outside the `UnauthorizedAccessException` handler (pinned by the existing test `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave`), and no retry bound is added (L2 is out of scope).

### Boundaries and invariants to preserve:
- Type identity, namespace, accessibility and every member signature of `SortEmail`, including the private members reflected on by the existing tests (`SanitizeArrayLineTSV`, `SanitizeArray`).
- Extension-method resolution for `SaveAttachmentAsync` and `TrySaveAttachmentAsync`.
- Production sticky `YesToAll` / `NoToAll` semantics and the `Cleanup_Files` reset point (research §4.5 behavior-equivalence notes).
- The `DirectoryInfo` construction's exception boundary (path computed before the inner `try`).
- Static initializers remain independent of one another, so the unspecified cross-partial initialization order cannot change behavior (research §2.2).

### Dependencies or blocked work:
- None blocking. Builds on the #945 seam (merged). Two maintainer decisions are recorded in Risks and do not block execution.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
See `## Write Set`.

#### Functions/classes/CLI commands impacted:
- `SortEmail` (now partial): member placement only, plus the D2 changes to `TrySaveAttachmentAsync` and `Cleanup_Files`, the `_removeReadOnly` field replaced by `RemoveReadOnlyPrompt`, and the new `ClearReadOnlyAttributeOnDisk`.
- `YesNoToAllPromptSession` (new).
- No CLI commands.

#### Data flow and validation changes:
- Sticky prompt state moves from a static enum field into a `YesNoToAllPromptSession` instance held by a `static readonly` field in production and created per test.
- The session constructor validates its delegate (fail fast). The core does not add a null guard on its delegates, consistent with the #945 review's acceptance of no guard on `createDirectory` for an internal method.

#### Error handling and logging updates:
- None. `Debug.WriteLine` calls in the handler move unchanged. No new catch, no changed exception type.

#### Rollback/feature-flag considerations (if applicable):
- No flag. Rollback is a revert of the PR; no persisted data or configuration changes.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- Five-argument core: inputs `Attachment`, `string filePathSave`, `Action<string> createDirectory`, `Action<string> clearReadOnly`, `YesNoToAllPromptSession removeReadOnlyPrompt`; output `Task<bool>` (true on save, false on declined or failed clear); exceptions as stated in the invariant.
- The prompt message text is unchanged: "The folder {dir} is read-only. Do you want to remove the readonly attribute?"

#### Required configuration keys and defaults:
- None.

#### Backward-compatibility expectations:
- No public or internal signature removed or changed. Additions only: the internal five-argument overload, the internal `YesNoToAllPromptSession` type, and two private members. The private field `_removeReadOnly` is removed; Grep at merge-base shows no reference to it outside the SortEmail source file.

#### Performance constraints (latency/throughput/memory):
- No measurable change: one additional delegate invocation per attribute clear, and one object allocated once per process.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): `InternalsVisibleTo("UtilitiesCS.Test")` and `DynamicProxyGenAssembly2` (UtilitiesCS/Properties/AssemblyInfo.cs lines 18 to 19) make the internal core and session type reachable from tests and Moq. Moq 4.21.0 supports `SetupSequence(...).Throws(...).Pass()` on a void member (in-repo precedent OutlookItemFlaggableTests lines 203 to 206).
- Constraints (budget, performance, compatibility): .NET Framework 4.8.1, C# language version 12 in UtilitiesCS; legacy non-SDK project files need explicit Compile Include entries, or the new files silently do not compile. Test files live in the UtilitiesCS.Test project per CLAUDE.md, which outranks the top-level tests folder layout in .claude/rules/general-unit-test.md for C#. Tests run in parallel and must not be serialized.
- External dependencies (services, libraries, releases): none new. MSTest, Moq and FluentAssertions are already referenced.

## Data / API / Config Impact
- User-facing or API changes: none. The read-only prompt appears under the same conditions with the same text.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): new Compile Include entries only (decision D4):
  - In `UtilitiesCS/UtilitiesCS.csproj`, directly after the SortEmail.cs entry (line 817), in this order: SortEmail.AttachmentSaving.cs, SortEmail.LegacyAttachmentSaving.cs, SortEmail.MailItemSort.cs, SortEmail.TrySaveAttachment.cs, SortEmail.UndoAndMoveLog.cs, each prefixed with the EmailIntelligence\EmailParsingSorting\ folder; and Dialogs\YesNoToAllPromptSession.cs after the Dialogs\YesNoToAll.cs entry (line 574).
  - In `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs after line 98, and Dialogs\YesNoToAllPromptSession_Tests.cs after line 442.
  - Form: four-space indent, backslash separators, self-closing, no DependentUpon. Project files are excluded from CSharpier and are edited by hand.

## Test Strategy
Seeded from issue (all three are addressed by D1, D2/D3 and D5 respectively; the authoritative criteria are in `## Acceptance Criteria`):

1. Split the SortEmail source by responsibility into partial files or types under 500 lines each, with no behavior change and the existing tests green.
2. Put `YesNoToAll.ShowDialog` behind an injected prompt delegate or interface, remove `[ExcludeFromCodeCoverage]` from `TrySaveAttachmentAsync`, and add tests for the `UnauthorizedAccessException` branches.
3. Inventory the remaining direct `Directory` and `File` calls in SortEmail helpers, and give each one a seam or a recorded justification.

- Regression tests to add or update (decision D3; research §5.2):
  - New `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs`, class `SortEmail_TrySaveAttachment_Tests`, namespace `UtilitiesCS.Test.EmailIntelligence`. Every test calls the five-argument core with a recording `createDirectory`, a recording `clearReadOnly`, a fresh `YesNoToAllPromptSession` whose delegate records messages and returns scripted answers, and a `MockBehavior.Loose` `Mock<Attachment>` whose `SaveAsFile` is scripted with `SetupSequence`. Paths are rooted in-memory literals distinct from #945's (for example C:\Sortemail956Sandbox\attachments\saved.txt); nothing touches the disk.

    | Test | Scenario | Asserts | Branches |
    |---|---|---|---|
    | T1 | save succeeds first time | true; prompt not called; `clearReadOnly` not called; `Response == Empty` | B0 |
    | T2 | UAE, `Yes`, retry succeeds | true; prompt once, message contains the directory and "is read-only"; `clearReadOnly` once with the directory; `SaveAsFile` twice; `Response == Empty` | B1, B3, B5 |
    | T3 | as T2 with `YesToAll` | true; `Response == YesToAll` | B1, B3, B6 |
    | T4 | sticky `YesToAll`: two calls on one session, each first save throws UAE | prompt exactly once across both; both true | B2, B3 |
    | T5 | UAE, `No` | false; `clearReadOnly` not called; `SaveAsFile` once; `Response == Empty` | B1, B7 reset arm |
    | T6 | UAE, `NoToAll`, then a second UAE call | both false; prompt once; `Response == NoToAll` | B1, B2, B7 sticky arm |
    | T7 | UAE, `Empty` (Cancel) | throws `UnauthorizedAccessException`; `clearReadOnly` not called; `Response == Empty` | B1, B8 |
    | T8 | UAE, `Yes`, `clearReadOnly` throws | false; `SaveAsFile` once; `Response == Empty` | B4, B5 |
    | T9 | UAE, `YesToAll`, `clearReadOnly` throws | false; `Response == YesToAll` | B4, B6 |
    | T10 | `SaveAsFile` throws a non-UAE exception | propagates; prompt not called | B9 |
    | T11 | UAE, `Yes`, retry throws UAE, second answer `No` | false; prompt twice; `clearReadOnly` once; `SaveAsFile` twice | B3, B5, B1, B7 |

    No test combines `YesToAll` with a retry that keeps failing; that path recurses without bound (L2).
    Size rule (orchestrator decision, 2026-10-01): T1 to T11 live in this one file only; no contingency split file is created. The research estimate is 330 to 380 lines (research §8), and the file must be under 500 lines after CSharpier. Exceeding that limit is a stop condition reported to the orchestrator, not a licence for the executor to create additional files.
  - New `UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs`, namespace `UtilitiesCS.Test.Dialogs`: S1 null constructor argument throws `ArgumentNullException`; S2 `Ask` from `Empty` invokes the delegate and stores its answer; S3 `Ask` from a non-`Empty` state returns it without invoking; S4 `ReleaseSingleAnswer` clears `Yes` and `No`; S5 `ReleaseSingleAnswer` keeps `YesToAll` and `NoToAll` (a `DataRow` set is acceptable); S6 `Reset` clears a sticky value; S7 `Ask` whose delegate returns `Empty` leaves `Response == Empty` and asks again on the next call.
  - Existing UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs: unchanged; all of its test methods must stay green (research §8 lists them).
- Unit tests (MSTest + Moq + FluentAssertions) for the fixed behavior and boundaries: as above. Each test creates its own session and mocks; no test reads or writes a static member of `SortEmail`; no `DoNotParallelize`, no serial settings, no retries.
- Edge cases and negative scenarios: Cancel (T7), throwing clear (T8, T9), non-UAE exception (T10), second prompt within one call (T11), null delegate (S1), `Empty` answer re-asking (S7).
- Error handling and logging verification: exception type and propagation asserted in T7 and T10; `false` returns asserted in T5, T6, T8, T9, T11.
- Fail-before: the new tests do not compile against merge-base because the five-argument overload and `YesNoToAllPromptSession` do not exist. The plan records this compile-red run as the fail-before, the same shape as #945 D-3.
- Coverage impact and targets for changed lines/modules (decision D7): baseline and post-change coverage are captured as Markdown projections only (Committed Test Evidence Format in CLAUDE.md; maintainer decision on #671): a JaCoCo package projection and the one-line first-party summary. Fixed filenames:
  - `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/baseline/coverage-baseline.md`
  - `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/qa-gates/coverage-post-change.md`
  - `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/qa-gates/coverage-comparison.md` (UtilitiesCS package line/branch band, aggregated SortEmail uncovered-line delta, and per-type line coverage for `YesNoToAllPromptSession` and the five-argument core)
  - `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/regression-testing/test-results-summary.md` (TRX-derived summary for the scoped runs)
- Toolchain commands to run (format, lint, type-check, test), in CLAUDE.md order:
  1. dotnet tool run csharpier format . (verify: dotnet tool run csharpier check .)
  2. msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  3. msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
  4. The test: MSTest with Coverage (Koverage) task, or scripts/vscode/Invoke-MSTestWithCoverage.ps1, or the #945 DIRECT coverage route (research §10). Scoped vstest filters: FullyQualifiedName~EmailIntelligence.SortEmail_ (old and new SortEmail classes; expected 26 tests, a prediction to be measured) and FullyQualifiedName~Dialogs.YesNoToAllPromptSession_Tests. Because a zero-match filter exits 0, each scoped run must assert the TRX total is non-zero.
  - Each rebuild must show that CoreCompile ran (no "Skipping target CoreCompile" line in the log).
- Manual validation steps (if required): none. Sandbox no-write check, per #945: confirm the C:\Sortemail956Sandbox path does not exist before and after the scoped test run.


## Acceptance Criteria
- [ ] AC1. `SortEmail` is declared `public static partial class SortEmail` and is split across exactly these files, each under five hundred total lines after CSharpier: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` and `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs`. Each starts with `#nullable enable` followed by the full merge-base using block, and members are placed as in the D1 file map.
- [ ] AC2. Every public, internal and private member signature of `SortEmail` at merge-base is present after the change with identical name, parameters, return type, accessibility and attributes, except: the private field `_removeReadOnly` is removed, `[ExcludeFromCodeCoverage]` is removed from the three-argument `TrySaveAttachmentAsync` overload, and the additions are only the five-argument `TrySaveAttachmentAsync` core, the private `RemoveReadOnlyPrompt` field and the private `ClearReadOnlyAttributeOnDisk` method. No file outside the Write Set is modified, so no call site changes.
- [ ] AC3. `UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs` declares `internal sealed class YesNoToAllPromptSession` with the constructor, `Response`, `Ask`, `ReleaseSingleAnswer` and `Reset` contracts stated in D2, and the constructor throws `ArgumentNullException` for a null delegate.
- [ ] AC4. The five-argument `TrySaveAttachmentAsync` core and the three-argument overload carry no `[ExcludeFromCodeCoverage]`; the two-argument wrapper keeps it and `ClearReadOnlyAttributeOnDisk` carries it, each with an in-code comment stating the justification recorded in D2. Every other `[ExcludeFromCodeCoverage]` attribute present at merge-base moves with its member unchanged.
- [ ] AC5. No `YesNoToAll.ShowDialog` call and no `DirectoryInfo` or `FileAttributes` reference remains in the try-save code path other than inside `ClearReadOnlyAttributeOnDisk` and the single production initializer `new(YesNoToAll.ShowDialog)` of `RemoveReadOnlyPrompt`; `Cleanup_Files` calls `RemoveReadOnlyPrompt.Reset()`; no settable static seam exists.
- [ ] AC6. The delivered core matches the invariant and the six-step accepted-value trace in Proposed Fix: `Path.GetDirectoryName` is computed before the inner `try`, `clearReadOnly` runs inside it, the outer rethrow and the inner exception boundary are unchanged, no new `catch` is added, and no retry bound is added.
- [ ] AC7. `UtilitiesCS/UtilitiesCS.csproj` contains Compile Include entries for the five new SortEmail partial files and for Dialogs\YesNoToAllPromptSession.cs, and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` contains Compile Include entries for every new test file, in the form stated under Data / API / Config Impact.
- [ ] AC8. Tests T1 to T11 exist in `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs`, each asserting the outcomes listed for it in the Test Strategy table, and all pass.
- [ ] AC9. Tests S1 to S7 exist in `UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs`, each asserting the outcome listed for it in Test Strategy, and all pass.
- [ ] AC10. The new tests use MSTest, Moq and FluentAssertions only; each creates its own `YesNoToAllPromptSession` and mocks; none reads or writes a static member of `SortEmail`, uses `DoNotParallelize`, touches the file system or creates a temporary file; none exercises the `YesToAll` retry that keeps failing (L2); and they pass under the parallel CLI runsettings without serialization. The sandbox path named in Test Strategy does not exist before or after the run.
- [ ] AC11. The existing test file for `SortEmail_Tests` is byte-identical to merge-base and every test method in it passes after the change.
- [ ] AC12. A fail-before is recorded: the new test files fail to compile against the merge-base production code, captured as a Markdown projection under the feature folder's evidence tree.
- [ ] AC13. Every direct file-system call site across the SortEmail partial files after the change corresponds to a row of the D5 inventory table, with the `DirectoryInfo` construction and attribute write located only in `ClearReadOnlyAttributeOnDisk`; no new direct file-system call is introduced; and J1 and J2 remain worded as pre-existing, unchanged exclusions that make no new claim under CLAUDE.md UT2 exemption (c).
- [ ] AC14. The full toolchain passes in one pass in CLAUDE.md order (CSharpier check, analyzer rebuild, TreatWarningsAsErrors rebuild, MSTest with coverage), each rebuild log showing that CoreCompile ran, with commands and exit codes recorded as Markdown projections under the feature folder's evidence tree.
- [ ] AC15. Coverage for changed lines is not reduced: the coverage comparison projection shows the aggregated uncovered-line count over all SortEmail partial files is not greater than the baseline count for the single SortEmail source file, and `YesNoToAllPromptSession` and the five-argument core each reach at least ninety percent line coverage.
- [ ] AC16. The diff adds no raw coverage collector document and no raw test-platform document (no trx file, no Cobertura document and no binary coverage file anywhere in the diff); committed test evidence is limited to the three forms CLAUDE.md "Committed Test Evidence Format" permits: the package-level JaCoCo projection, the one-line first-party coverage summary, and Markdown test-result summaries derived from the trx document.
- [ ] AC17. Latent defects L1 to L4 and follow-ups F1 to F3 are left unfixed in code and are listed under Rollout & Follow-up for the coordinator to promote.

## Risks & Mitigations
- Technical or operational risks:
  - A verbatim move drops or duplicates a member, or a partial file misses `#nullable enable`. Mitigation: AC1, AC2; the TreatWarningsAsErrors rebuild fails on CS8632 and on duplicate members.
  - A new file is not registered in a legacy project file and silently does not compile, so its tests never run. Mitigation: AC7; scoped runs assert a non-zero TRX total.
  - Behavior drift in the handler (exception boundary moved, release semantics changed). Mitigation: invariant, trace and AC6; T1 to T11 cover every branch B0 to B9.
  - Parallel-test interference through static state. Mitigation: design A; AC10.
  - A warm build skips compilation and the gates pass vacuously. Mitigation: the Rebuild target and the CoreCompile check in AC14.
- Maintainer decisions recorded (not blocking automation; research Automation Feasibility):
  - (a) J1/J2 wording: these are pre-existing, unchanged `[ExcludeFromCodeCoverage]` exclusions in `UtilitiesCS`, which UT2 exemption (c) does not list. This spec makes no new claim under (c); the maintainer may ratify or revise the wording.
  - (b) Keeping `[ExcludeFromCodeCoverage]` on the two-argument wrapper is treated as satisfying "remove the exclusion from `TrySaveAttachmentAsync`", because the wrapper's only behavior is real directory creation and the excluded executable mass falls from the whole try-save body to a one-statement wrapper and a two-statement adapter.
- Mitigations and rollbacks: revert the PR; no data or configuration migration.

## Rollout & Follow-up
- Release/rollout steps: standard PR merge; no deployment step beyond the add-in build.
- Post-fix monitoring or clean-up tasks — items for the coordinator to promote (not fixed here; research §9):
  - L1: `SaveCase` switch cases combine enum flags (`No | NoToAll` is 10, `Yes | YesToAll` is 5) that can never match, so the synchronous path never saves an existing-file attachment.
  - L2: with a sticky `YesToAll`, a `SaveAsFile` that keeps throwing `UnauthorizedAccessException` after a successful attribute clear (for example an ACL denial) recurses without bound; live through EmailFiler.
  - L3: `Cleanup_Files` never resets `_attachmentsAltName`, so an alternate-name "ToAll" answer persists for the add-in lifetime.
  - L4: `WriteCSV_StartNewFileIfDoesNotExist` reverses the `Path.Combine` arguments, inverts its condition relative to its name, and would dereference a null array if the branch ran.
  - F1: apply `YesNoToAllPromptSession` to the overwrite and alternate-name prompts in `SaveAttachmentAsync` / `SaveCaseAsync` (J3).
  - F2: delete the dead `SaveAttachmentsOld` and `IsPicture`.
  - F3: remove `[ExcludeFromCodeCoverage]` from the already-tested `SanitizeArrayLineTSV`, `SanitizeArray`, `SaveMessageAsMsgAsync` and `SaveMessageAsMSG`.
- Links: issue https://github.com/drmoisan/TaskMaster/issues/956; predecessor #945 (plan and review in its feature folder); research document cited at the top of this file.

## Write Set
Files created or modified by this fix (repository-relative):

- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (modified)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` (new)
- `UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs` (new)
- `UtilitiesCS/UtilitiesCS.csproj` (modified)
- `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` (new)
- `UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs` (new)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (modified)
- `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/**` (feature documents and evidence projections)
