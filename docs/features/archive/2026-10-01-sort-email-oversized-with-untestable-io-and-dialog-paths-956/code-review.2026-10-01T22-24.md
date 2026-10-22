# Code Review — Issue #956: SortEmail oversized with untestable I/O and dialog paths

- Feature folder: `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/` (`FEATURE/`)
- Branch `bug/sort-email-oversized-with-untestable-io-and-dialog-paths-956`, head `6278c6316`, merge base `f5b46df63` (origin/main)
- Review timestamp: 2026-10-01T22-24
- Companion artifacts: `policy-audit.2026-10-01T22-24.md`, `feature-audit.2026-10-01T22-24.md`
- Method: full read of the seven production files, the two new test files, the two `.csproj` deltas and the git-ignored merge-base backup `coverage/control-956/SortEmail.mergebase.bak`; Grep-based inventories for file-system calls, `YesNoToAll.ShowDialog`, `[ExcludeFromCodeCoverage]`, `_removeReadOnly`, `DoNotParallelize` and banned test APIs; direct read of the `<class>` nodes in the git-ignored post-processed Cobertura documents.

## Executive Summary

Verdict: APPROVE (PASS). Blocking findings: 0. Non-blocking findings: 6 (CR-1 to CR-6). Follow-ups for the coordinator: 6 (F-1 to F-6), none requiring a code change before merge.

The change does what the spec says and little else. The 1454-line static class becomes six partial files (277, 388, 342, 172, 240, 195 lines) by verbatim member moves that the executor's census proves segment-by-segment and that spot reads confirm (header, `Cleanup_Files`, the try-save overloads, the undo and move-log tail). The only behavioral edit is the D2 seam: the read-only prompt moves from a static enum field and a hard `YesNoToAll.ShowDialog` call to a per-call `YesNoToAllPromptSession` plus an `Action<string> clearReadOnly` delegate, with the production defaults held in one `static readonly` field and one two-statement adapter. The resulting core is 63/66 lines and 13/14 branches covered, the only misses being the unmeasured wrapper lambda and two closing braces that follow `throw;`. Eighteen new MSTest/Moq/FluentAssertions tests exercise every branch of the core and every member of the session type without touching disk, dialog or static state, and the pre-existing test class is byte-identical and green.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Non-blocking | UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs | lines 120-133 and 166-170 | CR-1. The `DirectoryInfo` construction has moved inside the inner `try`: merge-base line 944 built `new DirectoryInfo(Path.GetDirectoryName(filePathSave))` before the `try` and only the attribute write sat inside it; now `Path.GetDirectoryName` is outside and `new DirectoryInfo(directoryPath)` runs inside via `ClearReadOnlyAttributeOnDisk`. A constructor exception would be caught by `catch (System.Exception inner)` and return `false` instead of propagating. | Reconcile the spec: either amend the "Boundaries and invariants to preserve" bullet to state that only the `GetDirectoryName` computation stays outside the `try` (matching D2 items 3 and 5, which the code follows), or, if the merge-base boundary is wanted, construct the `DirectoryInfo` in the core and pass it to the adapter. | The divergence is between two parts of the spec, not between code and spec; AC6's literal text holds. In production the path reaching this point has already passed `Directory.CreateDirectory(path)` and `Path.GetDirectoryName` in the first `try`, which throw for the same argument classes (`ArgumentNullException`, `ArgumentException`, `PathTooLongException`), so a `DirectoryInfo` constructor failure here is not reachable in practice. | Spec "Boundaries and invariants to preserve", bullet 4; D2 items 3 and 5; `SortEmail.mergebase.bak` lines 944-948 |
| Non-blocking | UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs | type declaration, line 13 | CR-2. The seam is a delegate parameter plus a sealed concrete class; `.claude/rules/csharp.md` lists an interface seam as the first preference. | Keep as delivered. When follow-up F1 (spec) extends the session to the overwrite and alternate-name prompts, introduce a narrow interface then, when a second consumer exists. | The spec records alternative G as a deliberate deviation to stay consistent with the #945 `Action<string> createDirectory` seam already pinned by two tests; a single sealed type with four members is simpler than an interface plus implementation for one call path. | Spec "Rejected alternatives (research §4.4)" |
| Non-blocking (pre-existing, moved) | UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs | lines 103, 127, 147, 156-159 | CR-3. `Debug.WriteLine` carries the diagnostics although the class holds a log4net `logger`; the outer `catch (System.Exception) { throw; }` is a no-op rethrow that preserves the stack but adds nothing. | Outside this change (AC6 forbids altering the catch structure). Follow-up F-3: replace `Debug.WriteLine` with `logger.Debug`/`logger.Warn` and drop the no-op catch in a separate change with its own tests. | Merge-base lines 927, 951, 968, 980-983 carry the same statements; the test T10 pins that a non-UAE exception propagates unchanged, so removing the no-op catch later is safe. | `SortEmail.mergebase.bak` 919-984 |
| Non-blocking | all six SortEmail partials | lines 2-19 | CR-4. The full 18-directive using block is replicated into every partial; a given partial uses a subset (TrySaveAttachment.cs uses neither `Deedle`, `SDILReader`, `System.Text.RegularExpressions`, `System.Windows.Forms` nor the three `UtilitiesCS.*` sub-namespaces). | Follow-up F-4: trim per-file usings once IDE0005 is enforced or in the F2 dead-code removal; keep as-is now because the spec mandates the verbatim block for the move proof. | Unused usings are not diagnostics at error level here (analyzer rebuild 0 errors) and the verbatim block is what makes the census `HEADER-PREFIX = True` for every file. | P4-T10 census |
| Non-blocking (accepted exception) | UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs | lines 32-40, 162-166 | CR-5. Two `[ExcludeFromCodeCoverage]` attributes remain in the try-save path: the two-argument wrapper (one statement: the real `Directory.CreateDirectory` default) and `ClearReadOnlyAttributeOnDisk` (two statements writing a real directory attribute). | Accept. Both carry in-code UT4 justifications; the spec's Risk (b) records the maintainer decision that this satisfies "remove the exclusion from `TrySaveAttachmentAsync`". | The excluded executable mass fell from the whole 65-line handler to three statements whose only effect is real file-system I/O that a unit test must not perform. No configuration `exclude` entry was added. | AC4 text; spec Risks (b) |
| Non-blocking (pre-existing) | UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs | line 153 | CR-6. The moved `await attachments.ForEachAsync(async x => await x.SaveAttachmentAsync());` lambda is the file's only measured line and is uncovered (closure class `<<SortAsync>b__25_3>d`, `line-rate="0"`). | None for this change; it is merge-base line 361 moved verbatim inside an `[ExcludeFromCodeCoverage]` Outlook-interop method. Will disappear with F2 only if the enclosing method is retired. | Not a regression: the same line was the single uncovered SortEmail line at baseline (class node `line-rate="0.96"`, 24/25). | Cobertura final line 198380; baseline 59557 |

## Detailed Review

### 1. Partial-class split (D1)

- Declaration: every partial reads `public static partial class SortEmail` in `namespace UtilitiesCS` (read directly); merge-base line 23 was `public static class SortEmail`.
- Header rule: line 1 `#nullable enable`, lines 2-19 the merge-base using block byte-for-byte, in all six files (compared against `SortEmail.mergebase.bak` lines 1-19).
- Member placement matches the spec D1 file map exactly (verified by reading each file against the map): retained file holds `logger`, `InitializeSortToExisting`, the `MailItemHelper` `SortAsync`, `UpdatePredictiveEngineAsync`, `ProcessMailItemAsync`, `ResolvePaths(Folder, ...)`; MailItemSort holds the Explorer, 7-argument and 9-argument `SortAsync`, `Sort`, `ResolvePaths(IList<MailItem>, ...)`; AttachmentSaving holds the four prompt-state fields, `Cleanup_Files`, `GetAttachmentsInfo[Async]`, `SaveAttachment`, both `SaveAttachmentAsync`, `SaveCaseAsync`, `SaveCase`, `IsPicture`, `SaveMessageAsMsgAsync`, `SaveMessageAsMSG`; TrySaveAttachment holds the three overloads, the session field and the adapter; LegacyAttachmentSaving holds `MAX_PATH` and `SaveAttachmentsOld` (method-internal `#region` blocks intact); UndoAndMoveLog holds `UndoAsync`, `PushToUndoStack`, `CaptureMoveDetails`, `SanitizeArrayLineTSV`, `StripTabsCrLf`, `WriteCSV_StartNewFileIfDoesNotExist`, `SanitizeArray`.
- Verbatim moves: the executor's partition census (36 segments, `PARTITION-EXACT: True`) and move census (`FILE-EXACT = True` for the five fully-moved files, each segment found exactly once in its destination, S10 and S13 and S23 legitimately absent because they were replaced or removed) are consistent with the reviewer's spot reads of the tail (merge-base 1341-1452 equals UndoAndMoveLog 82-193), the field block (624-630 split across AttachmentSaving 25-28 and LegacyAttachmentSaving 25) and the undo routine (563-618 equals UndoAndMoveLog 25-80).
- `[ExcludeFromCodeCoverage]` count: 28 at merge-base, 28 after (4 + 5 + 10 + 2 + 1 + 6), with the three-argument overload's attribute removed and the adapter's added. The two `#pragma warning disable/restore CS0618` pairs travelled with their methods.
- Cross-partial references (`logger` used from MailItemSort line 182; `RemoveReadOnlyPrompt.Reset()` from AttachmentSaving line 35; `MAX_PATH` from the legacy routine) compile because partials share one type; static initializers are independent so the unspecified cross-file initialization order has no observable effect.
- Dropped class-level `#region` pairs are the only non-member text removed; a region cannot span files.

### 2. Prompt seam (D2)

`YesNoToAllPromptSession` (70 lines):
- Constructor null-guards the delegate with `?? throw new ArgumentNullException(nameof(showDialog))` (S1 asserts the parameter name).
- `Ask` prompts only while `Response == Empty` and stores the answer; an `Empty` answer (Cancel) leaves the session askable (S7).
- `ReleaseSingleAnswer` clears `Yes`/`No`, keeps `YesToAll`/`NoToAll` (S4, S5); `Reset` clears everything (S6).
- Documented as unsynchronized, single-caller. Production usage is one static instance driven sequentially from `SaveAttachmentAsync` through `ForEachAsync`, which is the same concurrency profile as the static enum field it replaces.

Core `TrySaveAttachmentAsync(Attachment, string, Action<string>, Action<string>, YesNoToAllPromptSession)`:
- Equivalence to merge-base 919-983, checked statement by statement. The three `_removeReadOnly` reads become `removeReadOnlyPrompt.Response`; the conditional `ShowDialog` assignment becomes `Ask(message)` (same message text); the `finally` reset-if-Yes and the reset-if-No become `ReleaseSingleAnswer()`, which is equivalent in both arms because the Yes/YesToAll arm can only hold Yes or YesToAll and the No/NoToAll arm only No or NoToAll, so the extra clause of `ReleaseSingleAnswer` is a no-op at each site; the recursive retry forwards all three seams.
- Exception structure: outer `try` with the same two `catch` clauses; inner `try/catch/finally` around `clearReadOnly(directory)`; `throw;` in the final `else` and in the outer `catch (System.Exception)`. No `catch` added, no retry bound added (L2 preserved). `createDirectory` stays outside the UAE handler, pinned by the pre-existing IOException test.
- CR-1 nuance recorded above: the `DirectoryInfo` construction itself now runs inside the inner `try`.
- The three-argument overload is now a one-statement forward and is no longer `async`; `async` is not part of the signature and the forwarded task carries every exception because the core is `async`, so caller semantics are unchanged.
- `RemoveReadOnlyPrompt` is `private static readonly` with the production `YesNoToAll.ShowDialog` delegate; nothing can replace it and no test references it (AC5). `Cleanup_Files` calls `Reset()` in place of the field assignment; its other three statements are unchanged (L3 preserved by design).
- `ClearReadOnlyAttributeOnDisk` holds the two original statements and is passed as a method group (no closure).

### 3. Project files (D4)

`UtilitiesCS.csproj`: `Dialogs\YesNoToAllPromptSession.cs` at line 575 directly after the `YesNoToAll.cs` entry; the five partials at lines 819-823 directly after `SortEmail.cs` (line 818) in the spec's order (AttachmentSaving, LegacyAttachmentSaving, MailItemSort, TrySaveAttachment, UndoAndMoveLog). `UtilitiesCS.Test.csproj`: `EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs` at line 99 after `SortEmail_Tests.cs`; `Dialogs\YesNoToAllPromptSession_Tests.cs` at line 444 after `YesNoToAll_Tests.cs`. Form matches (four-space indent, backslash separators, self-closing, no `DependentUpon`). Numstat `+6/-0` and `+2/-0`.

### 4. Tests

`SortEmail_TrySaveAttachment_Tests` (375 lines, 11 tests):
- The private `Seams` recorder owns the `YesNoToAllPromptSession`, records `CreatedDirectories`, `ClearedDirectories`, `PromptMessages`, dequeues scripted answers (an unscripted prompt throws `InvalidOperationException` from the empty queue, which fails the test loudly) and can throw a configured `ClearException`. Each test news its own instance.
- `Mock<Attachment>(MockBehavior.Loose)` with `SetupSequence(x => x.SaveAsFile(path)).Throws(...).Pass()` scripts the save outcomes; `Verify(..., Times.*)` pins call counts.
- Assertions match or exceed the spec table: T2 asserts the full prompt text rather than "contains"; T1 also asserts the single `createDirectory` call; T4 asserts four saves and two clears; T7 and T10 use `ThrowAsync<T>()`.
- Paths are literals under a `Sortemail956Sandbox` folder name distinct from the #945 sandbox; the executor probed both before and after every scoped run (never present).
- Branch census against the core: B0 (T1), B1 ask-when-Empty (T2, T3, T5, T6, T7, T8, T9, T11), B2 skip-ask-when-held (T4, T6 second call), B3 Yes/YesToAll clear-and-retry (T2, T3, T4, T11), B4 clear throws (T8, T9), B5 release Yes (T2, T8, T11), B6 keep YesToAll (T3, T4, T9), B7 No/NoToAll (T5, T6, T11), B8 Cancel rethrow (T7), B9 outer rethrow (T10). The Cobertura `MoveNext` node shows every `condition-coverage` at 100% except line 153 (`50% (1/2)`, the unreachable fall-through after `throw;`).
- The negative control (clear statement removed) fails exactly the six tests that depend on the clear and passes the five that do not, which demonstrates the tests discriminate on the seam rather than on incidental state.

`YesNoToAllPromptSession_Tests` (180 lines, 7 tests): one member per test, `WithParameterName("showDialog")` on S1, delegate call counters for S3 and S6, paired sessions for S4 and S5. Class node 20/20 lines, 8/8 branches.

Policy fit: MSTest attributes, Moq, FluentAssertions only; `/// <summary>` on every test; Arrange/Act/Assert comments; no `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `File.` or `Directory.` (Grep: 0 in both files); file sizes 375 and 180.

### 5. Hygiene and footprint

- `OUTSIDE-WRITE-SET: 0`, `WRITE-SET-MISSING: 0`, `RAW-DOC-PATHS: 0` (P4-T13); the reviewer's 82-path diff list agrees: 11 code files, FEATURE documents and evidence, one `docs/features/potential/promoted/` record, seven agent-memory files.
- No raw trx or Cobertura document committed; the committed evidence is projections and TRX-derived summaries (CLAUDE.md Committed Test Evidence Format).
- Account, machine and worktree tokens: zero matches in FEATURE (P4-T15); the reviewer found none in the code or in these three artifacts.
- `SortEmail.cs` begins with a UTF-8 BOM (intentional per the caller); CSharpier check passed on it.

## Follow-ups (for the coordinator; not filed by the reviewer)

- F-1. Reconcile the spec's "Boundaries and invariants to preserve" bullet about the `DirectoryInfo` construction boundary with D2 items 3 and 5 (CR-1); no code change is required unless the merge-base boundary is wanted.
- F-2. Promote the spec's Rollout list as recorded: L1 (`SaveCase` flag-combination cases never match), L2 (unbounded `YesToAll` retry), L3 (`_attachmentsAltName` never reset), L4 (`WriteCSV_StartNewFileIfDoesNotExist` argument order and inverted condition), F1 (apply the session to the overwrite and alternate-name prompts), F2 (delete `SaveAttachmentsOld` and `IsPicture`), F3 (remove the attribute from the already-tested helpers). All verified still present in code and unmodified.
- F-3. Replace `Debug.WriteLine` with the class logger and remove the no-op outer rethrow in the try-save core (CR-3), with T10 as the pin.
- F-4. Trim unused using directives per partial (CR-4).
- F-5. Canonical C# coverage artifact path convention: the runner writes under `coverage/` and the committed form is the projection; the reviewer procedure names `artifacts/csharp/coverage.xml`. Recurring across reviews; needs one decision.
- F-6. `quality-tiers.yml` is absent from the repository root, so tier-dependent gates cannot be evaluated for any project (pre-existing).

## Verdict

APPROVE. 0 blocking findings, 6 non-blocking findings, 6 follow-ups. No remediation inputs artifact is produced.
