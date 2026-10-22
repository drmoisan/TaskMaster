# Feature Audit — Issue #956: SortEmail oversized with untestable I/O and dialog paths

- Feature folder: `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/` (`FEATURE/`)
- Branch `bug/sort-email-oversized-with-untestable-io-and-dialog-paths-956`, head `6278c6316`
- Review timestamp: 2026-10-01T22-24
- Companion artifacts: `policy-audit.2026-10-01T22-24.md`, `code-review.2026-10-01T22-24.md`

## Scope and Baseline

- Work mode: `full-bug` (`- Work Mode: full-bug` in `FEATURE/issue.md`). Per the acceptance-criteria-tracking skill the AC source is `FEATURE/spec.md` only; no `user-story.md` exists.
- Base: `origin/main` at merge base `f5b46df637de81a0f4a856152095544f859718cc`. The branch merged origin/main at `a0e5383cf` and the merge base is an ancestor of head, so the two-dot and three-dot diffs coincide. 14 commits on the branch; the fix commit is `03efa278c`, the split is `29a3e7332`, the tests are `a9a0f9bdd`; everything after `ef790798d` (the toolchain-pass commit) is documents and evidence only (`git diff --stat ef790798d..6278c6316`: 11 Markdown files).
- Baseline facts verified: merge-base `SortEmail.cs` is 1454 lines, `public static class SortEmail`, both `TrySaveAttachmentAsync` overloads `[ExcludeFromCodeCoverage]`, `_removeReadOnly` static field at line 628 (read from `coverage/control-956/SortEmail.mergebase.bak`, the git-ignored byte copy whose SHA-256 `195BABDB...` the P1-T6 record ties to the merge base). Baseline tests 7336/7336; baseline first-party coverage 85.33% lines, 79.71% branches; baseline SortEmail class 24/25 lines.
- Branch diff: 82 paths. Code: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (modified, 1179 lines removed net), five new partials, `UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs` (new), `UtilitiesCS/UtilitiesCS.csproj` (+6), `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` (new, 375), `UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs` (new, 180), `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (+2). The remainder is `FEATURE/**`, one `docs/features/potential/promoted/` record and seven `.claude/agent-memory/**` files.
- Evidence corpus: `FEATURE/evidence/baseline/` (13 files), `regression-testing/` (10), `qa-gates/` (13), `other/` (25); plan `FEATURE/plan.2026-10-01T06-34.md` revision 1.2 with all 75 tasks checked and a Revision Log recording the AC15 amendment.
- Reviewer verification channel: Read/Grep/Glob of the worktree, `git diff`/`git log` against the shared object store, and direct reads of the git-ignored Cobertura documents `coverage/baseline-956.cobertura.xml` and `coverage/final-956.cobertura.xml`. No build, test or coverage run was executed by the reviewer; the PR CI run remains the repository-wide gate.

## Acceptance Criteria Inventory

Source: `FEATURE/spec.md`, section `## Acceptance Criteria`, lines 257 to 273. 17 checkbox items, all `- [x]` at head (Grep `^- \[x\] AC([1-9]|1[0-7])\. `: 17; `^- \[ \] AC`: 0). AC15 was amended on 2026-10-01 under the coordinator ruling AC15 option (a); the amendment is recorded in the plan Revision Log and quoted verbatim in `FEATURE/evidence/qa-gates/coverage-comparison.md`.

| AC | Short title | Executor state |
| --- | --- | --- |
| AC1 | Six partial files, each under 500 lines, `#nullable enable` plus the merge-base using block, D1 placement | [x] |
| AC2 | Member signature parity with the three named exceptions and three named additions; nothing outside the Write Set | [x] |
| AC3 | `YesNoToAllPromptSession` contract | [x] |
| AC4 | Attribute placement: none on core and three-argument overload; retained on wrapper and adapter with justifications; others moved unchanged | [x] |
| AC5 | No `ShowDialog`/`DirectoryInfo`/`FileAttributes` in the try-save path except the adapter and the initializer; `Cleanup_Files` resets; no settable static seam | [x] |
| AC6 | Core matches the invariant and trace; boundaries unchanged; no new catch; no retry bound | [x] |
| AC7 | Project-file entries in the stated form | [x] |
| AC8 | T1 to T11 exist, assert the table outcomes, pass | [x] |
| AC9 | S1 to S7 exist, assert the stated outcomes, pass | [x] |
| AC10 | Test hygiene: frameworks, isolation, no static access, no `DoNotParallelize`, no file system, no L2 retry, parallel green, sandbox absent | [x] |
| AC11 | `SortEmail_Tests.cs` byte-identical and green | [x] |
| AC12 | Fail-before recorded (compile-red) | [x] |
| AC13 | File-system inventory complete; J1/J2 wording | [x] |
| AC14 | Full toolchain one pass, CoreCompile ran, recorded | [x] |
| AC15 | Changed-line coverage not reduced under the three-exemption rule; new code >= 90% | [x] |
| AC16 | No raw coverage or test-platform document in the diff | [x] |
| AC17 | L1 to L4 and F1 to F3 left unfixed and listed | [x] |

## Acceptance Criteria Evaluation

| AC | Status | Evidence and reviewer verification |
| --- | --- | --- |
| AC1 | PASS | All six files read: each begins `#nullable enable` then the 18 merge-base using directives (compared with `SortEmail.mergebase.bak` 1-19) and declares `public static partial class SortEmail`. Line counts 277 / 388 / 342 / 172 / 240 / 195 (P4-T10 `LINES`; reads agree). Member placement matches the D1 map file by file (see code-review section 1). CSharpier check exit 0 after format changed nothing. |
| AC2 | PASS | Census: 36 merge-base segments, `PARTITION-EXACT: True`, each found exactly once in its destination (`FILE-EXACT = True` for the five fully-moved files); S10 (`Cleanup_Files`) replaced by S10P, S13 (`_removeReadOnly`) removed, S23 (three-argument overload) replaced. Grep for `_removeReadOnly` in `*.cs`: 0. Three-argument overload: same name, parameters, return type `Task<bool>`, `internal static`, attribute removed. Additions limited to the five-argument core, `RemoveReadOnlyPrompt`, `ClearReadOnlyAttributeOnDisk`. `OUTSIDE-WRITE-SET: 0` (P4-T13); reviewer diff list agrees. Attribute count 28 -> 28. |
| AC3 | PASS | `YesNoToAllPromptSession.cs` read: `internal sealed class` in `namespace UtilitiesCS`, `#nullable enable`; constructor `internal YesNoToAllPromptSession(Func<string, YesNoToAllResponse>)` with `?? throw new ArgumentNullException(nameof(showDialog))`; `internal YesNoToAllResponse Response { get; private set; }` (default `Empty`); `Ask` asks only when `Empty`; `ReleaseSingleAnswer` clears `Yes`/`No` only; `Reset` clears all. S1 asserts the exception and parameter name; 7/7 pass. |
| AC4 | PASS | Core (line 87) and three-argument overload (line 60) carry no attribute. Wrapper (line 40) and adapter (line 165) carry `[ExcludeFromCodeCoverage]` with the UT4 justification comments at lines 32-34 and 162-164. All other 26 attributes moved with their members (census). |
| AC5 | PASS | Grep `YesNoToAll.ShowDialog` in TrySaveAttachment.cs: line 29 only (the initializer). `DirectoryInfo`/`FileAttributes`: lines 168-169 only (inside the adapter). `Cleanup_Files` line 35: `RemoveReadOnlyPrompt.Reset();`. The field is `private static readonly`; `Response` has a private setter; no settable static member introduced. |
| AC6 | PASS | Core read against merge-base 919-983: `var directory = Path.GetDirectoryName(filePathSave);` at line 120 precedes the inner `try` at 121; `clearReadOnly(directory)` at 123 is inside it; `catch (System.Exception inner) { Debug.WriteLine; return false; }` and `finally { ReleaseSingleAnswer(); }` preserved; outer `catch (System.UnauthorizedAccessException e)` and `catch (System.Exception) { throw; }` unchanged; final `else { throw; }` unchanged; recursive retry without bound. No new `catch`. T1 to T11 reproduce the six-step trace (T4 is steps 3 to 5). Reviewer note (code-review CR-1, non-blocking): the `DirectoryInfo` construction itself now executes inside the inner `try` through the adapter, as D2 items 3 and 5 prescribe; the AC6 text as written is satisfied. |
| AC7 | PASS | `UtilitiesCS.csproj` lines 575 and 819-823; `UtilitiesCS.Test.csproj` lines 99 and 444; four-space indent, backslash separators, self-closing, placed after the sibling entries the spec names, spec order preserved. Numstat `6 0` and `2 0`. |
| AC8 | PASS | Eleven `[TestMethod]`s T1 to T11 read; each assertion set matches or strengthens the Test Strategy row (T2 asserts the full prompt text; T4 asserts four saves). P4-T5: 26/26 with `NAME-SET-MATCH: True`; P4-T11 TRX summary `Total 26, executed 26, passed 26, failed 0`. |
| AC9 | PASS | Seven `[TestMethod]`s S1 to S7 read, each asserting the stated outcome (S5 as two sessions rather than a `DataRow`, which the spec permits). P4-T6 and P4-T11: `Total 7, executed 7, passed 7, failed 0`. |
| AC10 | PASS | Only MSTest, Moq and FluentAssertions namespaces imported. Each test creates its own `Seams`/session and mock. Grep for `DoNotParallelize`, `File.`, `Directory.`, `Thread.Sleep`, `Task.Delay`, `DateTime.Now` in both files: 0. No test configures a `YesToAll` answer with a retry that keeps failing. Runsettings hash equals P0-T4 (class-level parallel); `SANDBOX-956-EXISTS-BEFORE/AFTER: False` on every scoped run. |
| AC11 | PASS | P4-T14: SHA-256 `791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E` equals P0-T4; `git diff --exit-code` 0; porcelain 0. All 15 names Passed at P4-T5 (and at P0-T10). The file is absent from the branch diff list. |
| AC12 | PASS | `FEATURE/evidence/regression-testing/fail-before-exception.2026-10-01T20-50.md`: `msbuild UtilitiesCS.Test.csproj /t:Build` against merge-base production exits 1 with `CS0246` in the two new test files only (`ERROR_LINES_OTHER_FILES: 0`), production diff exit 0, `DLL_ADVANCED: False`. Markdown projection, no raw log committed. |
| AC13 | PASS | Reviewer Grep of `File\.|Directory\.|DirectoryInfo|FileInfo|FileIO2` across the six partials: 15 sites = SortEmail.cs 173 (J1); MailItemSort 161, 307 (J1); AttachmentSaving 106 (J2), 169 (J3); TrySaveAttachment 49 (#945 wrapper), 168 (seam adapter); LegacyAttachmentSaving 114, 116, 118, 120, 129 (J4); UndoAndMoveLog 147, 166 (J5). One-to-one with the D5 table (14 merge-base rows, the `DirectoryInfo` row now at the adapter). No new direct call. Spec lines 126-127 word J1 and J2 as "pre-existing, unchanged exclusion" making "no new claim under CLAUDE.md UT2 exemption (c)". |
| AC14 | PASS | `FEATURE/evidence/qa-gates/toolchain-pass.md`: format, check, analyzer rebuild, nullable rebuild, scoped tests, tests with coverage, every `EXIT_CODE: 0`; both rebuilds `SKIP_CORECOMPILE_LINES: 0` with `UCS_CSC_OUT_LINES: 2` and `UCS_TEST_CSC_OUT_LINES: 2`; `LOOP-RESTARTS: 0`. No code file changed after the pass. |
| AC15 | PASS | `FEATURE/evidence/qa-gates/coverage-comparison.md`: aggregate uncovered 1 -> 4; exemptions by construct landed on lines 49, 154, 155 (`EXEMPT-LINE-COUNT: 3`, `EXEMPT-UNCOVERED: 3`); adjusted delta 0 (bound 0), raw delta 3 (bound 3); negative control `CONTROL-VERDICT: FAIL`, `CONTROL-RAW-VERDICT: FAIL`; `CORE-PERCENT: 96.23`, `SESSION-PERCENT: 100`. Reviewer re-read of the Cobertura class nodes: TrySaveAttachment.cs `line-rate="0.954545"` with `hits="0"` at exactly 49, 154, 155; YesNoToAllPromptSession.cs `line-rate="1" branch-rate="1"`; MailItemSort.cs closure 0/1 at line 153 (the baseline's single uncovered line, moved); every other mapped SortEmail line `hits="1"`. The ruling and its rationale are quoted verbatim in the evidence; the reviewer concurs that the three lines are not changed-line regressions. |
| AC16 | PASS | Diff path list: no `.trx`, `.cobertura.xml`, `.jacoco.xml`, `.coverage` or binary; `RAW-DOC-PATHS: 0`, `RAW-DOCUMENT-FILES: 0`. Committed evidence is the JaCoCo package projections with the one-line first-party summaries (`coverage-baseline.md`, `coverage-post-change.md`) and TRX-derived summaries (`test-results-summary.md`, `test-run-final.md`, `coverage-post-change.md` SUMMARY block). |
| AC17 | PASS | Spec lines 290-296 list L1 to L4 and F1 to F3. Code confirms each remains: `SaveCase` cases `(NoToAll | No)` and `(Yes | YesToAll)` at AttachmentSaving 300 and 303 (L1); unbounded recursive retry in the core (L2); `Cleanup_Files` does not reset `_attachmentsAltName` (L3); `File.Exists(Path.Combine(strFileName, strFileLocation))` with the inverted branch at UndoAndMoveLog 147 (L4); six direct `ShowDialog` sites outside the try-save path (F1); `SaveAttachmentsOld` and `IsPicture` present (F2); attributes on `SanitizeArrayLineTSV`, `SanitizeArray`, `SaveMessageAsMsgAsync`, `SaveMessageAsMSG` present (F3). |

Blocking findings from this evaluation: none. No AC was evaluated PARTIAL, FAIL or UNVERIFIED.

## Acceptance Criteria Check-off

All 17 items were already `- [x]` in `FEATURE/spec.md` when the review began (checked by the executor at P4-T16 to P4-T32, inventory at P4-T33). The reviewer verified each against code and evidence and evaluated all 17 as PASS; no item was newly checked off and no item was unchecked. `FEATURE/spec.md` was not modified by this review.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md`
- Total AC items: 17
- Checked off (delivered): 17
- Remaining (unchecked): 0
- Items remaining: none

## Baseline Comparison

| Measure | Merge base | Head | Change |
| --- | --- | --- | --- |
| `SortEmail` source files | 1 (1454 lines) | 6 (277, 388, 342, 172, 240, 195) | 500-line breach resolved |
| `TrySaveAttachmentAsync` measured by coverage | no (both overloads excluded) | yes (core 51/53, forward 9/9) | handler now tested |
| Static prompt state | `_removeReadOnly` enum field | `static readonly YesNoToAllPromptSession` | same stickiness, injectable per call |
| Direct `YesNoToAll.ShowDialog` in try-save path | 1 call | 0 calls (1 delegate reference in the initializer) | seamed |
| Direct `DirectoryInfo` attribute write in try-save path | inline | `ClearReadOnlyAttributeOnDisk` adapter | seamed |
| Tests in `UtilitiesCS.Test` touching this area | 15 | 33 | +18 |
| Repository tests | 7336 | 7354 | +18, 0 failures |
| First-party coverage | 85.33% lines / 79.71% branches | 85.36% / 79.75% | +0.03 / +0.04 |
| SortEmail aggregate uncovered lines | 1 | 4 raw (0 adjusted) | within the ratified bounds |

## Gaps, Assumptions and Follow-ups

- G-1 (non-blocking): the canonical `artifacts/csharp/coverage.xml` is absent from the worktree; the committed projections and the git-ignored post-processed Cobertura documents were the evidence. Reviewer re-read the class nodes directly.
- G-2 (non-blocking): PR context artifacts are absent in the worktree and stale in the session checkout; scope was derived from `git diff --stat` and `git log`.
- G-3 (non-blocking, pre-existing): `quality-tiers.yml` is absent, so tier-dependent gates are not evaluable.
- Assumption: the caller's statement that the UTF-8 BOM at the start of the retained `SortEmail.cs` is intentional was accepted; CSharpier check passed on the file.
- Follow-ups F-1 to F-6 are listed in `code-review.2026-10-01T22-24.md`; the spec's own L1 to L4 and F1 to F3 are owed to the coordinator for promotion (AC17).

## Summary

All 17 acceptance criteria PASS against code and evidence. The SortEmail class is split into six cohesive partial files under 500 lines by verbatim moves; the read-only prompt and the attribute write sit behind per-call seams with the production defaults held in a `static readonly` session and a two-statement adapter; the formerly excluded handler is now measured at 96.23% with every reachable branch covered; eighteen new tests pass under parallel settings without touching disk or dialogs; the existing test class is byte-identical and green; the full toolchain passed in one pass with CoreCompile proven; first-party coverage rose on both axes; and the diff carries no raw tool document. Zero blocking findings; six non-blocking code-review findings and three evidence gaps are recorded with follow-ups.

Verdict: PASS.
