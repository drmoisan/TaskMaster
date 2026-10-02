# Policy Audit — Issue #956: SortEmail oversized with untestable I/O and dialog paths

- Feature folder: `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/` (cited below as `FEATURE/`)
- Branch: `bug/sort-email-oversized-with-untestable-io-and-dialog-paths-956`
- Head: `6278c631609ef8e2dcc549e8312ee8c9fc5f902e` (committed 2026-10-01 22:09:59 -0400)
- Base: `origin/main` at merge base `f5b46df637de81a0f4a856152095544f859718cc` (an ancestor of head via the merge commit `a0e5383cf`, so the two-dot and three-dot diffs are identical)
- Work mode: `full-bug` (issue.md marker); AC source: `FEATURE/spec.md` only
- Review timestamp: 2026-10-01T22-24 (local, UTC-4)
- Reviewer tooling: Read, Grep and Glob against the item worktree, plus the two permitted Bash forms `git diff` and `git log` run against the shared object store. No PoshQC, MCP, msbuild or vstest invocation was made by the reviewer; every gate result below is evidence-attested from the committed projections and, where stated, independently re-read from the git-ignored post-processed Cobertura documents left in the worktree.

## Template Resolution Deviation

The MCP tools `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` are not in this session's tool set. This artifact is hand-authored and preserves the twelve canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` section 5, in order, with no template instruction block. The artifact is not marked BLOCKED: every section below is evidence-complete.

## Rejected Scope Narrowing

No scope narrowing was detected. Two caller statements were examined because they touch gate scope, and both were accepted on their merits rather than as instructions:

1. Caller text: "(This item writes no PowerShell file, so no PowerShell gate applies.)" — Verified against the full branch diff (`git diff --stat f5b46df6...6278c6316`, 82 paths): zero `.ps1`/`.psm1` files changed. The statement is a fact about the diff, and the PowerShell row below is recorded as a zero-file language accordingly.
2. Caller text: "Do not treat these three ruled exemptions as findings; do treat any other uncovered changed line, any coverage exclusion entry, or any policy violation as a finding." — This is the coordinator ruling on the AC15 measurement, quoted verbatim in `FEATURE/evidence/qa-gates/coverage-comparison.md` under `## Coordinator ruling (AC15 option (a))`. The reviewer evaluated the three lines independently (section 5): line 49 is the unchanged #945 wrapper lambda that was unmeasured at baseline because both merge-base overloads were excluded; lines 154 and 155 are closing braces after `throw;` that no execution path can reach, and AC6 requires the rethrows unchanged. The reviewer concurs that they are not changed-line regressions. The full-branch audit scope was not narrowed by either statement.

## Evidence Location Compliance

The 82-path branch diff list was scanned for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` and `artifacts/coverage/`: zero occurrences. Every evidence file in the diff lives under `FEATURE/evidence/<kind>/` (`baseline/`, `regression-testing/`, `qa-gates/`, `other/`). No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` event occurred: the caller supplied canonical paths only. `validate_evidence_locations.py` was not executed by the reviewer (no shell beyond the two permitted git forms); the path-list scan above is the substitute and is complete for the diff. Verdict: PASS.

## Executive Summary

| Item | Result |
| --- | --- |
| Overall compliance verdict | PASS |
| Blocking findings | 0 |
| Non-blocking findings | 9 (CR-1 to CR-6 code review; G-1 to G-3 evidence gaps) |
| Languages with changed files | C# only (7 production `.cs`, 2 test `.cs`, 2 `.csproj`) |
| Toolchain pass | One pass in CLAUDE.md order, every step exit 0, CoreCompile ran on both rebuilds (`FEATURE/evidence/qa-gates/toolchain-pass.md`) |
| Repository tests | 7336 -> 7354 passed, 0 failed (full suite with coverage, 9 assemblies) |
| First-party C# coverage | 85.33% -> 85.36% lines, 79.71% -> 79.75% branches; floors met under both CLAUDE.md UT2 (80/75) and `.claude/rules` (85/75) |
| New-code coverage | 96.36% lines over the measured lines of the added production files (106/110); `YesNoToAllPromptSession` 100%, five-argument core 96.23% |
| Changed-line regression | None. SortEmail aggregate uncovered lines 1 -> 4 raw, 0 adjusted under the ratified three-exemption rule; negative control FAILs as required |
| Acceptance criteria | 17/17 PASS (see `feature-audit.2026-10-01T22-24.md`) |
| Remediation required | No |

C# coverage verdict: PASS (first-party lines 85.36% and branches 79.75% meet both floor sets; no changed-line regression under the ratified AC15 rule; every new type and method at or above 90%).

## 1. General Unit Test Policy Compliance

Policy sources: CLAUDE.md General Unit Test Policy (UT1 to UT5) and `.claude/rules/general-unit-test.md`.

### 1.1 Core principles (UT1)

| Principle | Verdict | Evidence |
| --- | --- | --- |
| Independence | PASS | Each of the 18 new tests constructs its own `Seams`/`YesNoToAllPromptSession` and `Mock<Attachment>`; no shared fixture, no static state of `SortEmail` read or written (Grep of both test files for static member access of `SortEmail`: none other than the extension-method call under test). Runs green under the class-level parallel CLI runsettings (hash `98EF03A8...` equal to P0-T4) with no `DoNotParallelize` (Grep: 0). |
| Isolation | PASS | T1 to T11 target the five-argument `TrySaveAttachmentAsync` core only; S1 to S7 target one member of `YesNoToAllPromptSession` each. |
| Fast execution | PASS | Scoped runs: 26 tests and 7 tests completed in single vstest invocations (`FEATURE/evidence/regression-testing/test-run-final.md`, `p4-t6-session-run.2026-10-01T21-22.md`). No waits, sleeps or timers (Grep for `Thread.Sleep|Task.Delay|DateTime.Now`: 0). |
| Determinism | PASS | Scripted `Queue<YesNoToAllResponse>` answers and `SetupSequence` on `SaveAsFile`; an unscripted prompt throws from the empty queue. Same 26/26 and 7/7 outcomes at P3-T8/P3-T9 and P4-T5/P4-T6. |
| Readability | PASS | Every test carries a `/// <summary>` with scenario and expected outcome and a `T<n>`/`S<n>` tag matching the spec table; Arrange/Act/Assert comments present in all 18. |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `FEATURE/evidence/baseline/coverage-baseline.md` (JaCoCo package projection plus first-party summary of the git-ignored `coverage/baseline-956.cobertura.xml`)
- C# post-change coverage artifact: `FEATURE/evidence/qa-gates/coverage-post-change.md` (JaCoCo package projection plus first-party summary of the git-ignored `coverage/final-956.cobertura.xml`, root element re-read by the reviewer: `lines-covered="56202" lines-valid="65845" branches-covered="13618" branches-valid="17076" timestamp="1790904220"`)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.33% lines (56113/65760) / 79.71% branches (13594/17054) -> Post-change: 85.36% lines (56202/65845) / 79.75% branches (13618/17076). Change: +0.03% lines (+89 covered, +85 valid) / +0.04% branches (+24 covered, +22 valid). New/changed-code coverage: 96.36%. Disposition: PASS. Evidence: FEATURE/evidence/baseline/coverage-baseline.md; FEATURE/evidence/qa-gates/coverage-post-change.md; FEATURE/evidence/qa-gates/coverage-comparison.md; reviewer re-read of the `<class>` nodes in the git-ignored coverage/final-956.cobertura.xml and coverage/baseline-956.cobertura.xml.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
| --- | --- | --- | --- | --- | --- | --- |
| C# | 11 (7 production .cs: 1 modified, 6 added; 2 test .cs added; 2 .csproj modified) | 7354 (full suite, 9 assemblies; 18 added) | 7354 passed, 0 failed | 85.33% lines / 79.71% branches | 85.36% lines / 79.75% branches | 96.36% lines (106/110 measured lines of the added production files) |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

Artifact state and verdicts (C# only; the other three languages have zero changed files):

| Language | Coverage document used | Verdict | Disposition |
| --- | --- | --- | --- |
| C# | committed JaCoCo projections in FEATURE/evidence plus the git-ignored post-processed Cobertura documents (route DIRECT: outer `dotnet-coverage collect`, inner `vstest.console.exe /InIsolation` over 9 assemblies, post-processed by `ConvertTo-KoverageCoberturaXml`) | PASS | Floors met; no changed-line regression; new code at or above 90% |

C# canonical-path coverage artifact `artifacts/csharp/coverage.xml`: absent from the review worktree (the runner writes under `coverage/`, and CLAUDE.md "Committed Test Evidence Format" forbids committing the raw document), so the canonical path is rated FAIL as an evidence-location convention only, non-blocking, and recorded as gap G-1; the figures in this audit come from the committed projections and the reviewer's direct read of the post-processed Cobertura documents, which are the repository's mandated evidence form. A document at that path in the session checkout dated 2026-09-05 (epoch 1788642282, 84.83% lines) predates this branch and was not used.

### 1.3 Coverage and scenarios (UT2), structure (UT3), external dependencies (UT4), audit (UT5)

| Clause | Verdict | Evidence |
| --- | --- | --- |
| UT2 repository floors (C# lines >= 80%, branches >= 75%; rules: lines >= 85%, branches >= 75%) | PASS | First-party 85.36% lines, 79.75% branches (`LINE-FLOOR: MET`, `BRANCH-FLOOR: MET`); reviewer-confirmed root element values. |
| UT2 new modules/classes/methods >= 90% | PASS | `YesNoToAllPromptSession` class node `line-rate="1" branch-rate="1"` (20/20 lines, 8/8 branches). Five-argument core (`MoveNext` method node) 51/53 lines = 96.23%, 11/12 branches; the two misses are lines 154 and 155 (post-`throw;` braces). Three-argument forward 9/9. |
| UT2 no coverage reduction on changed lines | PASS | SortEmail aggregate uncovered lines: baseline 1 (merge-base L361, the `ForEachAsync` lambda) -> final 4 (the same lambda at MailItemSort L153, plus TrySave L49, L154, L155). Adjusted delta 0 under the three-exemption rule; raw 3 (bound 3). Negative control (lowest covered non-exempt line treated as uncovered) reads FAIL on both bounds. Reviewer re-read the final TrySave `<class>` node: exactly `49`, `154`, `155` carry `hits="0"`; every other mapped line of the six partials carries `hits="1"` except MailItemSort L153 (pre-existing). |
| UT2 exclusion of test files from measurement | PASS | Package projection lists production packages only (`QuickFiler`, `UtilitiesCS`, ...); no `*.Test` package. |
| UT2 scenario completeness | PASS | Positive (T1, T2, T3, S2), negative/invalid (S1 null delegate, T5/T6 declined), edge (T4/T6 sticky ToAll across calls, S7 Empty re-ask, T11 second prompt in one call), error handling (T7 cancel rethrow, T8/T9 throwing clear, T10 non-UAE propagation), state transitions (S4, S5, S6, T2/T3 release vs keep). Concurrency: the session is documented as unsynchronized and single-caller; production usage is sequential (`ForEachAsync`), unchanged from the static field it replaces. |
| UT3 AAA, clear failure messages, intent | PASS | FluentAssertions throughout (`Should().Equal`, `Should().Be`, `ThrowAsync<T>()`, `WithParameterName`); Moq `Verify(..., Times.*)` for call counts. Control run messages (P3-T11) show actionable text, e.g. "Expected seams.ClearedDirectories to be equal to {...}, but found empty collection." |
| UT4 no external dependencies, no temporary files | PASS | Paths are in-memory rooted literals under a `Sortemail956Sandbox` folder name; the injected delegates record rather than act; sandbox existence probed before and after every scoped run (`SANDBOX-956-EXISTS-BEFORE/AFTER: False`, also for the #945 sandbox). No `File.`/`Directory.` call in either new test file (Grep: 0). |
| UT5 policy audit by the author | PASS | `FEATURE/evidence/other/p1-t5-test-census.2026-10-01T20-49.md` and the P4-T10 census record the per-test token checks. |
| Rules: test file location | PASS (repo convention) | CLAUDE.md places C# tests in `UtilitiesCS.Test/` mirroring the production project; `Dialogs/` mirrors `UtilitiesCS/Dialogs/`; the TrySave tests sit beside the pre-existing `SortEmail_Tests.cs` under `EmailIntelligence/`. The spec records that CLAUDE.md outranks the rules' top-level `tests/` layout for C#. |
| Rules: determinism infrastructure (clock, RNG, banned APIs) | PASS | No time or randomness in the changed code; no banned API in tests. |
| Rules: property-based tests (T1/T2 modules) | Cannot be tier-evaluated | `quality-tiers.yml` is absent from the repository root in this worktree (Glob: none), so no tier is recorded for `UtilitiesCS`. Recorded as gap G-3 (pre-existing, outside the diff). |

## 2. General Code Change Policy Compliance

| Clause | Verdict | Evidence |
| --- | --- | --- |
| Bugfix workflow: failing regression test first | PASS | `FEATURE/evidence/regression-testing/fail-before-exception.2026-10-01T20-50.md`: compile-red build of `UtilitiesCS.Test.csproj` against merge-base production (exit 1, `CS0246` x2 in the two new test files, 0 errors elsewhere, production diff exit 0). Runtime equivalent: the P3-T11 negative control (`clearReadOnly(directory);` removed) fails exactly T2, T3, T4, T8, T9, T11 and passes T1, T5, T6, T7, T10 as predicted; P3-T13 restore returns 11/11. |
| Bugfix workflow: minimal targeted fix | PASS | Production change is the partial split (verbatim moves; census `FILE-EXACT = True` for the five fully-moved files, every one of 36 merge-base segments found exactly once) plus the D2 seam in one file and one statement in `Cleanup_Files`. `OUTSIDE-WRITE-SET: 0`, `WRITE-SET-MISSING: 0` (P4-T13). |
| Design principles: simplicity, separation of I/O | PASS | The prompt and the attribute write are isolated behind a 70-line sealed session type and a two-statement adapter; the core no longer references WinForms or `DirectoryInfo`. |
| Classes vs functions | PASS | `YesNoToAllPromptSession` models state plus invariant (ask only while Empty; release only single answers); `ClearReadOnlyAttributeOnDisk` is a small stateless adapter. |
| Interfaces and contracts | PASS with recorded deviation | The seam is a delegate plus sealed class rather than an interface (`.claude/rules/csharp.md` DI Seams order prefers an interface). The spec records alternative G as a deliberate deviation for consistency with the #945 delegate seam pinned by two existing tests. Non-blocking CR-2. |
| Error handling | PASS (pre-existing pattern preserved) | Constructor fails fast (`ArgumentNullException`). The core keeps the merge-base `catch (System.Exception inner) { ...; return false; }` around the attribute clear and the outer `catch (System.Exception) { throw; }`; AC6 forbids changing them. Non-blocking CR-3 records the pre-existing no-op rethrow and `Debug.WriteLine` diagnostics. |
| File size <= 500 lines | PASS | SortEmail.cs 277, MailItemSort 388, AttachmentSaving 342, TrySaveAttachment 172, LegacyAttachmentSaving 240, UndoAndMoveLog 195, YesNoToAllPromptSession 70; tests 375 and 180 (P4-T10 census `LINES` values; file reads agree). The 1454-line merge-base breach is resolved. |
| Naming, docs, comments | PASS | PascalCase/camelCase observed; XML docs on the session type and all three `TrySaveAttachmentAsync` overloads; "why" comments at the two `[ExcludeFromCodeCoverage]` sites and the production session field. |
| Public API stability | PASS | No signature removed or changed; additions are `internal`/`private`. Grep for `_removeReadOnly` across `*.cs`: 0 (removed private field had no external reference). Callers in TaskMaster, QuickFiler and EmailFiler untouched (not in the diff). |
| Dependencies | PASS | None added; `packages.config` unchanged. |
| Mandatory toolchain loop | PASS | One pass: format (0 Write Set files changed), check (`Checked 1636 files`), analyzer rebuild (`SKIP_CORECOMPILE_LINES: 0`, 0 errors), nullable rebuild (same), tests with coverage (7354/7354). `LOOP-RESTARTS: 0`. No `.cs`/`.csproj` changed after the pass (`git diff --stat ef790798d..6278c6316`: 11 docs/evidence files only). |
| Architecture boundaries (`.claude/rules/architecture-boundaries.md`) | PASS (legacy code, no new runtime surface) | The split moves existing Outlook-interop code verbatim; the only new type (`YesNoToAllPromptSession`) references `System` only. No `NetArchTest` project exists in the solution (pre-existing). |

## 3. Language-Specific Code Change Policy Compliance

Policy sources: CLAUDE.md C# Code Change Policy (C#1 to C#7) and `.claude/rules/csharp.md`.

| Clause | Verdict | Evidence |
| --- | --- | --- |
| C#1.1 CSharpier via `dotnet tool run`, format then check | PASS | P4-T1 `dotnet tool run csharpier format .` -> `WRITESET-CHANGED-COUNT: 0`; P4-T2 `dotnet tool run csharpier check .` -> `CHECK_EXIT_CODE: 0`, `Checked 1636 files`. The retained `SortEmail.cs` begins with a UTF-8 BOM; CSharpier check accepted it (caller notes it is intentional). |
| C#1.2 analyzers, `/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild` | PASS | P4-T3: exit 0, `ERRORS: 0`, `WRITESET_DIAGNOSTIC_LINES: 0`, `UCS_CSC_OUT_LINES: 2`, `UCS_TEST_CSC_OUT_LINES: 2`, `SKIP_CORECOMPILE_LINES: 0`. |
| C#1.3 nullable, `/p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`, `/t:Rebuild` | PASS | P4-T4: exit 0, same CoreCompile evidence. Every partial and the session type carry `#nullable enable` on line 1 (read directly). |
| C#2 strong contracts, null safety, composition | PASS | Explicit types at every `internal` boundary; `Response { get; private set; }`; the session is `sealed` and holds one responsibility. |
| C#3 methods small, shallow branching | PASS | Session members are 1 to 6 lines; the core keeps the merge-base shape (AC6). |
| C#4 exceptions at boundaries with context, logging pattern | PASS with pre-existing exception recorded | New code: `ArgumentNullException(nameof(showDialog))`. Moved code keeps `Debug.WriteLine` and the broad catches (CR-3, pre-existing, unchanged by mandate). |
| C#5 one responsibility per file, `internal` surface | PASS | Six cohesive partials by responsibility group; new members `internal`/`private`. |
| C#6 naming, XML docs, "why" comments | PASS | See section 2. |
| C#7 analyzer configuration, no new suppressions | PASS | No `.editorconfig`, `GlobalSuppressions` or `#pragma` added; the two pre-existing `#pragma warning disable/restore CS0618` pairs moved with their methods unchanged. |
| csharp.md DI seam preference order | PASS with recorded deviation | Delegate seam (option 2) chosen over interface (option 1) with written rationale in `FEATURE/spec.md` (rejected alternatives A to G). CR-2. |
| csharp.md prohibited behaviours | PASS | No broad refactor beyond the one class; no weakened assertions (the existing `SortEmail_Tests.cs` is SHA-256-identical to merge-base, `791E2B9E...`); no sleeps or retries; toolchain run before success was reported. |
| Legacy `.csproj` registration | PASS | `UtilitiesCS.csproj` lines 575 and 819 to 823; `UtilitiesCS.Test.csproj` lines 99 and 444; four-space indent, backslash separators, self-closing, placed after the sibling entries the spec names (`+6/-0` and `+2/-0` numstat). |

## 4. Language-Specific Unit Test Policy Compliance

| Clause | Verdict | Evidence |
| --- | --- | --- |
| CUT1 MSTest only | PASS | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`; no xUnit/NUnit reference. |
| CUT2 Moq for mocks | PASS | `new Mock<Attachment>(MockBehavior.Loose)` with `SetupSequence(...).Throws(...).Pass()` and `Verify(..., Times.*)`; the `Seams` recorder is a hand-written fake for delegates, which Moq does not need to wrap. |
| CUT2 FluentAssertions preferred | PASS | Every assertion is FluentAssertions; no MSTest `Assert` call in either new file. |
| CUT3 toolchain command selection | PASS | Steps 1 to 3 as written in CLAUDE.md; step 4 issued as the DIRECT route (outer `dotnet-coverage collect`, inner `vstest.console.exe` with the CLI runsettings, `/InIsolation`, explicit `/ResultsDirectory` and `/Logger:trx;LogFileName=`), which is the route the spec names as an accepted alternative to the VS Code task and which supplies the explicit trx name CLAUDE.md requires for a summary. |
| Determinism (csharp.md) | PASS | No PATH, cwd, network or clock dependence. |
| Parallel execution preserved | PASS | CLI runsettings hash unchanged; 26/26 and 7/7 under class-level parallelism; the production sticky state is a `static readonly` session that no test touches. |

## 5. Test Coverage Detail

Measurement route: `dotnet-coverage collect --output-format cobertura --settings coverage\effective-coverage-956.config -- vstest.console.exe <9 assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook&<four shell-icon and OSBrowser class exclusions, identical at baseline and final>`, post-processed by `ConvertTo-KoverageCoberturaXml`, projected by `ConvertTo-JacocoPackageProjection` with `Assert-JacocoProjectionReconciliation`. Baseline and final runs used the identical command, filter and settings.

Repository and package figures:

| Scope | Baseline | Post-change | Change |
| --- | --- | --- | --- |
| First-party lines | 56113/65760 = 85.33% | 56202/65845 = 85.36% | +89 covered, +85 valid, +0.03% |
| First-party branches | 13594/17054 = 79.71% | 13618/17076 = 79.75% | +24 covered, +22 valid, +0.04% |
| UtilitiesCS package lines | 38821/43423 = 89.40% | 38909/43508 = 89.43% | +88 covered, +85 valid |
| UtilitiesCS package branches | 9410/11271 = 83.49% | 9434/11293 = 83.54% | +24 covered, +22 valid |

Per-file figures for the changed production files, read by the reviewer from the `<class>` nodes of the two git-ignored Cobertura documents (worktree lines 59557 baseline; 53491, 59645, 59669, 59703, 59906, 198380 final):

| File | Baseline | Post-change | Uncovered lines (post) | Disposition |
| --- | --- | --- | --- | --- |
| SortEmail.cs (merge-base, single file) | 24/25 lines, `line-rate="0.96"` | split | L361 `ForEachAsync` lambda | baseline reference |
| SortEmail.cs (retained) | part of the above | 5/5, `line-rate="1"` | none | PASS |
| SortEmail.AttachmentSaving.cs | part of the above | 10/10, `line-rate="1"` | none | PASS |
| SortEmail.TrySaveAttachment.cs | both overloads excluded at merge-base (unmeasured) | 63/66 lines `line-rate="0.954545"`, 13/14 branches `branch-rate="0.928571"` | L49 (excluded wrapper's lambda), L154, L155 (braces after `throw;`) | PASS under the ruled three-exemption rule; the only branch miss is the post-`throw;` jump at L153 (`50% (1/2)`), which is unreachable |
| SortEmail.UndoAndMoveLog.cs | part of the above | 8/8, `line-rate="1"` | none | PASS |
| SortEmail.MailItemSort.cs | part of the above | 0/1 (closure class `<<SortAsync>b__25_3>d`) | L153, the merge-base L361 lambda moved verbatim inside an `[ExcludeFromCodeCoverage]` method | PASS under the spec D7 aggregate rule for the split; pre-existing uncovered line, not a regression (CR-6) |
| SortEmail.LegacyAttachmentSaving.cs | excluded (unmeasured) | no class node (every member excluded) | none measured | PASS (dead code, F2) |
| YesNoToAllPromptSession.cs | new | 20/20 lines, 8/8 branches, `line-rate="1" branch-rate="1"` | none | PASS (>= 90%) |

Aggregate over the added production files: 106/110 measured lines = 96.36%; over the two files that contain new executable code (TrySaveAttachment.cs and YesNoToAllPromptSession.cs): 83/86 lines = 96.51%, 21/22 branches = 95.45%. Five-argument core alone: 51/53 lines = 96.23% (CORE-SPAN 87-160), 11/12 branches.

AC15 rule as ratified (coordinator ruling, option (a)): exemptions identified by file and containing construct, not line number; adjusted delta at most 0 (observed 0), raw delta at most 3 (observed 3); negative control must FAIL (observed `CONTROL-VERDICT: FAIL`, `CONTROL-RAW-VERDICT: FAIL`). Reviewer assessment: the wrapper lambda (L49) was never in the baseline denominator because both merge-base overloads carried the attribute, so its appearance is a newly measured line, not a newly uncovered one; L154 and L155 follow `throw;` statements and are unreachable by construction; removing the attribute from the core brought all three into the denominator together with 63 newly covered lines. No changed line that was covered at baseline is uncovered after the change.

C# coverage verdict: PASS (lines 85.36% and branches 79.75% first-party; new code 96.36%; no changed-line regression).

## 6. Test Execution Metrics

| Run | Command (summary) | Total | Passed | Failed | Evidence |
| --- | --- | --- | --- | --- | --- |
| Baseline scoped SortEmail filter (P0-T10) | `vstest.console.exe UtilitiesCS.Test.dll /Settings:TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_` | 15 | 15 | 0 | `FEATURE/evidence/baseline/test-run-baseline.md` |
| Baseline full suite with coverage (P0-T11) | CMD-COVERAGE-DIRECT, 9 assemblies | 7336 | 7336 | 0 | `FEATURE/evidence/baseline/coverage-baseline.md` |
| Fail-before (P1-T6) | `msbuild UtilitiesCS.Test.csproj /t:Build` against merge-base production | build exit 1 (`CS0246` x2, new test files only) | — | — | `FEATURE/evidence/regression-testing/fail-before-exception.2026-10-01T20-50.md` |
| Post-fix scoped TrySave tests (P3-T8) and session tests (P3-T9) | scoped vstest | 26 and 7 | 26 and 7 | 0 | `p3-t8-scoped-run.2026-10-01T21-11.md`, `p3-t9-session-run.2026-10-01T21-12.md` |
| Negative control, clear removed (P3-T11, expect-fail) | scoped vstest on `SortEmail_TrySaveAttachment_Tests` | 11 | 5 | 6 (T2, T3, T4, T8, T9, T11 exactly as predicted) | `negative-control-clearreadonly-removed.md` |
| Control restored (P3-T13) | scoped vstest | 11 | 11 | 0 | `p3-t13-post-restore-run.2026-10-01T21-15.md` |
| Final scoped SortEmail filter (P4-T5) | as P0-T10 | 26 | 26 | 0 | `FEATURE/evidence/regression-testing/test-run-final.md` (`NAME-SET-MATCH: True` against the 15 existing plus 11 new names) |
| Final scoped session filter (P4-T6) | `/TestCaseFilter:FullyQualifiedName~Dialogs.YesNoToAllPromptSession_Tests` | 7 | 7 | 0 | `p4-t6-session-run.2026-10-01T21-22.md` |
| Final full suite with coverage (P4-T7) | CMD-COVERAGE-DIRECT, 9 assemblies | 7354 | 7354 | 0 (`NEWLY-FAILING: NONE`) | `FEATURE/evidence/qa-gates/coverage-post-change.md` |
| TRX-derived summaries (P4-T11) | `Get-TrxRunSummary`/`Format-TrxRunSummary` | 26 and 7 | 26 and 7 | 0 | `FEATURE/evidence/regression-testing/test-results-summary.md` |

Test count delta: +18 (11 `SortEmail_TrySaveAttachment_Tests` + 7 `YesNoToAllPromptSession_Tests`). The 15 pre-existing `SortEmail_Tests` methods all pass on the unchanged file.

Timestamp consistency check (no shell clock available to the reviewer; commit epochs from `git log`): the fix commit `03efa278c` at epoch 1790903451 decodes to 21:10:51 local and precedes the P3-T7 test-build label 21-10/21-11; the Cobertura root `timestamp="1790904220"` decodes to 21:23:40 local and matches the P4-T7 label 21-24; head `6278c6316` at 22:09:59 local matches the P4-T33 label 22-09. Labels are consistent with the commit clock.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
| --- | --- | --- | --- |
| Confidentiality masking scan | P4-T15 CMD-SWEEP over FEATURE (61 files; account, profile leaf, machine, worktree root, Users-path tokens derived at run time) plus reviewer Grep of the two new test files and the three review artifacts | `ACCOUNT-TOKEN-MATCHES: 0`, `PROFILE-LEAF-MATCHES: 0`, `MACHINE-TOKEN-MATCHES: 0`, `WORKTREE-ROOT-MATCHES: 0`, `USERS-PATH-MATCHES: 0`. The only drive-letter literal in the diff is the in-memory `Sortemail956Sandbox` test path, which is a fixture value, not a host path. | PASS |
| Suppression scan (added lines) | Grep of the seven production files and two test files for `#pragma warning`, `SuppressMessage`, `GlobalSuppressions`, `ExcludeFromCodeCoverage` | No new `#pragma` or `SuppressMessage`; `[ExcludeFromCodeCoverage]` count 28 at merge-base and 28 after (one removed from the three-argument overload, one added on the two-statement adapter), every other attribute moved with its member (census `FILE-EXACT = True`). | PASS (see exception X-1) |
| Workflow change scan | Diff path list | No `.github/workflows/**` change on the branch. | PASS |
| Raw document scan | P4-T13 `RAW-DOC-PATHS: 0`; P4-T15 `RAW-DOCUMENT-FILES: 0`; reviewer scan of the 82-path diff list for `.trx`, `.cobertura.xml`, `.coverage`, `.jacoco.xml` | None in the diff. | PASS |
| Byte identity of the existing test file | P4-T14 `Get-FileHash` + `git diff --exit-code` | `791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E` equals P0-T4; diff exit 0; porcelain 0 lines. | PASS |
| File size scan | P4-T10 `LINES` census; reviewer reads | All nine `.cs` files in the Write Set under 500 lines. | PASS |

## 8. Gaps and Exceptions

Findings (none blocking):

| ID | Severity | Clause | Finding | Disposition | Evidence |
| --- | --- | --- | --- | --- | --- |
| CR-1 | Non-blocking | CLAUDE.md C#4; spec "Boundaries and invariants" | `new DirectoryInfo(directoryPath)` now executes inside the inner `try` (via `ClearReadOnlyAttributeOnDisk`), whereas merge-base line 944 constructed it before the `try`. A constructor exception would now return `false` instead of propagating. Practically unreachable: `Directory.CreateDirectory` and `Path.GetDirectoryName` already accepted the same path in the first `try`. The spec's D2 items 3 and 5 prescribe exactly this placement while its "Boundaries" bullet says the construction boundary is preserved; the AC6 text as written is satisfied. | Accepted; follow-up F-1 to reconcile the spec wording | `SortEmail.TrySaveAttachment.cs` 120-133, 166-170; `coverage/control-956/SortEmail.mergebase.bak` 944-948 |
| CR-2 | Non-blocking | csharp.md DI Seams order | Delegate plus sealed class instead of an interface seam. | Accepted deviation recorded in spec (alternative G) | `FEATURE/spec.md` Proposed Fix, "Rejected alternatives" |
| CR-3 | Non-blocking, pre-existing | CLAUDE.md C#4.1, C#4.2 | Outer `catch (System.Exception) { throw; }` is a no-op rethrow; `Debug.WriteLine` used for diagnostics instead of the log4net `logger` the class already holds. Moved unchanged by AC6 mandate. | Accepted; follow-up F-3 | `SortEmail.TrySaveAttachment.cs` 103, 127, 147, 156-159 |
| CR-4 | Non-blocking | CLAUDE.md §4.3 explicit imports | The full 18-directive using block is replicated into every partial; several directives are unused in a given partial (for example `Deedle`, `SDILReader`, `System.Text.RegularExpressions` in TrySaveAttachment.cs). Spec-mandated verbatim copy; analyzers do not flag IDE0005 at error. | Accepted; follow-up F-4 | Lines 2-19 of each partial |
| CR-5 | Non-blocking | rules Coverage Exclusion Policy; CLAUDE.md UT2 | Two member-level `[ExcludeFromCodeCoverage]` attributes remain in the try-save path (two-argument wrapper; two-statement adapter), each with an in-code UT4 justification. The rules clause targets configuration `exclude` globs on production files; no such entry exists (`coverage.config` unchanged). Spec Risk (b) records the maintainer decision. | Accepted exception X-1 | `SortEmail.TrySaveAttachment.cs` 32-40, 162-166 |
| CR-6 | Non-blocking, pre-existing | UT2 changed-line coverage | `SortEmail.MailItemSort.cs` L153 (`await attachments.ForEachAsync(async x => await x.SaveAttachmentAsync());`) is the only measured line of its file and is uncovered; it is merge-base L361 moved verbatim inside an excluded Outlook-interop method. | Not a regression; governed by the D7 aggregate rule | Cobertura final line 198380; baseline class node 59557 |
| G-1 | Non-blocking | Reviewer coverage-artifact procedure | Canonical `artifacts/csharp/coverage.xml` absent in the worktree; the repository's mandated evidence form (projection plus summary) and the git-ignored post-processed documents were used instead. | Recorded; follow-up F-5 (convention) | Glob of `artifacts/**` in the worktree: none |
| G-2 | Non-blocking | pr-context-artifacts skill | No `artifacts/pr_context.summary.txt`/`.appendix.txt` exists in the worktree, and the session checkout's pair describes another branch (`bug/stale-analyzer-include-paths`, 2026-09-29). The MCP collector is not in this session's tool set and Bash is restricted to `git diff`/`git log`. Scope was derived from `git diff --stat` and `git log` against the shared object store (82 paths, 14 commits including one merge of origin/main). | Recorded assumption | This document, header |
| G-3 | Non-blocking, pre-existing | quality-tiers.md | `quality-tiers.yml` is absent from the repository root, so tier-dependent gates (property-test density, mutation score) cannot be evaluated for `UtilitiesCS`. Outside the diff. | Recorded | Glob: none |

Exceptions accepted:

- X-1: `[ExcludeFromCodeCoverage]` retained on the two-argument `TrySaveAttachmentAsync` wrapper (its only statement is the real `Directory.CreateDirectory` default) and placed on `ClearReadOnlyAttributeOnDisk` (two statements writing a real directory attribute). Both carry in-code UT4 justifications (AC4) and the excluded executable mass fell from the whole 65-line try-save body to three statements. Treated as an accepted member-level exemption, consistent with prior review rulings that the Coverage Exclusion Policy's blocking clause covers configuration `exclude` entries.
- X-2: Three uncovered lines in `SortEmail.TrySaveAttachment.cs` (L49, L154, L155) under the coordinator ruling on AC15, quoted verbatim in `FEATURE/evidence/qa-gates/coverage-comparison.md`; reviewer concurs (section 5).

## 9. Summary of Changes

Production (`UtilitiesCS`):
- `SortEmail.cs` reduced from 1454 to 277 lines and declared `public static partial class SortEmail`; retains `logger`, `InitializeSortToExisting`, `SortAsync(IList<MailItemHelper>, ...)`, `UpdatePredictiveEngineAsync`, `ProcessMailItemAsync`, `ResolvePaths(Folder, ...)`. Class-level `#region` pairs dropped.
- Five new partials by responsibility, verbatim member moves: `SortEmail.MailItemSort.cs` (388), `SortEmail.AttachmentSaving.cs` (342; `Cleanup_Files` now calls `RemoveReadOnlyPrompt.Reset()`), `SortEmail.TrySaveAttachment.cs` (172; D2 seam), `SortEmail.LegacyAttachmentSaving.cs` (240), `SortEmail.UndoAndMoveLog.cs` (195).
- `SortEmail.TrySaveAttachment.cs`: three-argument overload loses `[ExcludeFromCodeCoverage]` and forwards to a new internal five-argument core taking `Action<string> clearReadOnly` and `YesNoToAllPromptSession removeReadOnlyPrompt`; `private static readonly YesNoToAllPromptSession RemoveReadOnlyPrompt = new(YesNoToAll.ShowDialog)` replaces the `_removeReadOnly` static field; `[ExcludeFromCodeCoverage] private static void ClearReadOnlyAttributeOnDisk(string)` holds the `DirectoryInfo` attribute write.
- New `UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs` (70): `internal sealed`, constructor null-guard, `Response`, `Ask`, `ReleaseSingleAnswer`, `Reset`.
- `UtilitiesCS.csproj`: six `Compile Include` entries.

Tests (`UtilitiesCS.Test`):
- `EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` (375): T1 to T11 with a per-test `Seams` recorder.
- `Dialogs/YesNoToAllPromptSession_Tests.cs` (180): S1 to S7.
- `UtilitiesCS.Test.csproj`: two `Compile Include` entries.
- `EmailIntelligence/SortEmail_Tests.cs`: unchanged (SHA-256 identical).

Documents: `FEATURE/` issue, spec (v0.2, AC15 amended under the coordinator ruling), research, plan (revision 1.2, 75/75 tasks checked), 61 evidence files; one `docs/features/potential/promoted/` record; seven `.claude/agent-memory/**` files from the planner, orchestrator, researcher and executor passes (outside the code audit).

## 10. Compliance Verdict

PASS. Zero blocking findings. Nine non-blocking findings (CR-1 to CR-6, G-1 to G-3) and two accepted exceptions (X-1, X-2) are recorded above. No remediation inputs artifact is produced. Follow-ups are listed in `code-review.2026-10-01T22-24.md` for the coordinator; none requires a code change before merge.

C# coverage verdict: PASS.

## Appendix A: Test Inventory

New tests (18), all Passed at P4-T5/P4-T6/P4-T7:

| Class | Test | Spec ID | Scenario |
| --- | --- | --- | --- |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly | T1 | first save succeeds; no prompt, no clear |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer | T2 | UAE, Yes, retry succeeds, answer released |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer | T3 | UAE, YesToAll kept |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt | T4 | sticky YesToAll across two calls, one prompt |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer | T5 | UAE, No, false, released |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt | T6 | sticky NoToAll across two calls |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException | T7 | Cancel (Empty) rethrows |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer | T8 | clear throws after Yes |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer | T9 | clear throws after YesToAll |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt | T10 | non-UAE exception propagates |
| SortEmail_TrySaveAttachment_Tests | TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse | T11 | Yes then retry denied, second answer No |
| YesNoToAllPromptSession_Tests | Constructor_WhenShowDialogIsNull_ThrowsArgumentNullException | S1 | null delegate |
| YesNoToAllPromptSession_Tests | Ask_WhenNoAnswerIsHeld_InvokesPromptAndStoresAnswer | S2 | Ask from Empty |
| YesNoToAllPromptSession_Tests | Ask_WhenAnswerIsHeld_ReturnsItWithoutInvokingPrompt | S3 | Ask while held |
| YesNoToAllPromptSession_Tests | ReleaseSingleAnswer_WhenAnswerIsYesOrNo_ClearsIt | S4 | release Yes/No |
| YesNoToAllPromptSession_Tests | ReleaseSingleAnswer_WhenAnswerIsYesToAllOrNoToAll_KeepsIt | S5 | keep ToAll |
| YesNoToAllPromptSession_Tests | Reset_WhenToAllAnswerIsHeld_ClearsItSoThePromptIsShownAgain | S6 | Reset clears sticky |
| YesNoToAllPromptSession_Tests | Ask_WhenPromptReturnsEmpty_HoldsNoAnswerAndAsksAgain | S7 | Empty answer re-asks |

Pre-existing `SortEmail_Tests` (15, unchanged file, all Passed at P0-T10 and P4-T5): SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath; SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath; TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave; TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile; StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString; StripTabsCrLf_WithPlainText_ReturnsOriginalString; Cleanup_Files_DoesNotThrow; GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments; GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments; SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine; SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows; SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException; SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException; InitializeSortToExisting_AlwaysThrows_NotImplementedException; InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException.

## Appendix B: Toolchain Commands Reference

| Step | Command as run (executor, per evidence) | Exit |
| --- | --- | --- |
| Format | `dotnet tool run csharpier format .` | 0 (`WRITESET-CHANGED-COUNT: 0`) |
| Format check | `dotnet tool run csharpier check .` | 0 (`Checked 1636 files`) |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (CoreCompile ran) |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 (CoreCompile ran) |
| Scoped tests | `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_" "/ResultsDirectory:coverage\test-results\956\p4-t5" "/Logger:trx;LogFileName=p4-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` (and the `Dialogs.YesNoToAllPromptSession_Tests` filter) | 0 |
| Tests with coverage | `dotnet-coverage collect --output coverage\final-956.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-956.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&<exclusions>" "/ResultsDirectory:coverage\test-results\956\final" "/Logger:trx;LogFileName=final-956.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`, then `ConvertTo-KoverageCoberturaXml`, `Assert-CoberturaLineCoverageThreshold`, `Assert-CoberturaBranchCoverageThreshold`, `Get-CoberturaFirstPartyCoverageReport`, `ConvertTo-JacocoPackageProjection` | 0 |
| Reviewer | `git diff --stat f5b46df6... 6278c6316`; `git diff --stat ef790798d... 6278c6316`; `git log --format='%H %ct %s' f5b46df6...6278c6316`; Read/Grep/Glob of the worktree files and the git-ignored Cobertura documents | read-only |
