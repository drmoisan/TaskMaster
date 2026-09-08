# Policy Audit — issue #809, `uithread-init-contract-residuals-784-787-788`

- Artifact timestamp: 2026-09-08T01-35 (local, EDT / UTC-4)
- Component: `UtilitiesCS/Threading` (UiThread initialization contract and awaiter predicate)
- Feature folder: `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809`
- Work mode: `full-bug` (marker read from `issue.md:12`) — `spec.md` is the sole acceptance-criteria source
- Base ref: `04a54e681bd21e841e124c016df30672ee701b75`; branch head `ef431e6a` (9 commits)
- Reviewer verification mode: read-only. No source file, plan, policy document, or orchestration state was modified.

## Template provenance

The `policy-audit-template-usage` skill requires the template to be resolved through
`mcp__drm-copilot__resolve_policy_audit_template_asset`. No MCP tool is exposed to this reviewer
session, so the template could not be resolved and `mcp__drm-copilot__validate_orchestration_artifacts`
could not be run. This artifact is hand-authored and preserves all twelve canonical major headings
required by that skill. The artifact is not marked BLOCKED, because every substantive section below
is backed by direct verification against the tree and the committed evidence.

## Rejected Scope Narrowing

None detected. The caller supplied a diff restricted to `UtilitiesCS/`, `UtilitiesCS.Test/` and
`QuickFiler.Test/`, but also supplied the anchored changed-path set and stated that exactly twelve
paths changed outside the feature folder. The restriction therefore describes the full branch diff
rather than narrowing it, and the audit below covers the whole set. The caller's instruction to treat
`spec.md` rather than `issue.md` as the AC source is the correct `full-bug` behaviour under
`acceptance-criteria-tracking`, not a narrowing.

Recorded constraint on independent verification: the caller prohibited git invocation in this
session. The twelve-path changed set and the commit list are therefore taken from the caller's
pre-computed patch and path list rather than recomputed by the reviewer. Every claim that could be
checked against the working tree, the committed evidence artifacts, and the Cobertura documents was
checked directly and is reported as verified below.

## Evidence Location Compliance

- Canonical scheme required by `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` is
  `<FEATURE>/evidence/<kind>/`. The delivery wrote 44 artifacts under `evidence/baseline/`,
  `evidence/qa-gates/`, `evidence/regression-testing/` and `evidence/other/`. All four are canonical
  sub-paths. **PASS**.
- Forbidden `artifacts/` sub-paths (`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`,
  `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`,
  `artifacts/post-change/`): none exists in the worktree. `artifacts/` contains only `orchestration/`
  and pre-existing `pr_body_*` files, none of which this branch added. **PASS**.
- `validate_evidence_locations.py` is not present in this repository, so the scripted scan could not
  be run; the equivalent check was performed by directory enumeration. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED`
  condition arose.

## Executive Summary

The delivery fixes three defects in `UtilitiesCS/Threading/UiThread.cs` (#787 apartment precondition,
#788 latch-before-`Initialize()`, #784 reference-equality awaiter predicate), adds one narrow internal
interface, two test-only seams, and 17 tests. All four toolchain gates are green in a single final
pass, the full nine-assembly suite is 7137/7137, and the coverage of the file in scope rises from
76.83% to 96.03% line.

Every headline figure the executor reported was re-derived by this reviewer from the raw Cobertura
documents rather than accepted from the artifacts. All of them reproduce exactly.

**Blocking findings: 0.**

Two acceptance criteria are graded PARTIAL rather than PASS, both for evidence-labelling or
literal-wording reasons rather than for defects in the delivered code:

- AC5: the `[P0-T15]` apartment label `MTA_INITIALIZE_OUTCOME` rests on an inference that this same
  delivery later falsified by direct measurement. The run almost certainly executed STA, in which case
  no MTA measurement was taken and the stated refutation of the #782 narrative is unsupported. The
  AC2 design does not depend on the result, so this is an evidence defect, not a code defect.
- AC6: coverage was collected with `dotnet-coverage collect` rather than the `/EnableCodeCoverage`
  collector the criterion names, and the raw Cobertura document is git-ignored rather than stored under
  `evidence/qa-gates/`. Every measurable clause of AC6 is met and independently verified.

## 1. General Unit Test Policy Compliance

| Requirement (`.claude/rules/general-unit-test.md`) | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every new test wraps its Act in `UiThreadStateScope`, which snapshots all eleven `UiThread` statics plus the factory and restores them on disposal, including captured nulls (`UiThreadStateScope.cs:320-335`). |
| Isolation | PASS | Each method targets one behaviour; the three classes that mutate `UiThread` statics carry `[DoNotParallelize]`, as do the three pre-existing writer classes the delivery amended. |
| Fast execution | PASS | `p4-t4-utilitiescs-tests.md` and `p4-t3-quickfiler-tests.md` record sub-second durations for all named rows. |
| Determinism | PASS | No `Thread.Sleep`, no `Task.Delay`, no wall-clock assertion. The anti-retry-storm assertion is a factory invocation count (`UiThreadInitContract_Tests.cs:750`). `FakeTimeProvider` is injected where a clock is needed and is never advanced. |
| Readability | PASS | Arrange/Act/Assert comments on every added method; names state the scenario and expected outcome. |
| No external dependencies | PASS | Zero `Microsoft.Office.Interop` references in the three touched test files; no network, DB, or process. |
| No temporary files | PASS | No file creation in any added test. |
| Test file location mirrors source | PASS | `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` mirrors `UtilitiesCS/Threading/`; the helper lives under `UtilitiesCS.Test/TestHelpers/`. |
| Coverage exclusion policy — no production path excluded | PASS | `coverage.config` excludes only third-party module paths (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest). The derived `coverage/809-effective-coverage.config` appends exactly one entry, `.*\.Test\.dll$`, which matches test assemblies only. No first-party production path is excluded, and no `[ExcludeFromCodeCoverage]` was added by this delivery. |
| Scenario completeness | PASS | Positive (STA accept), negative (MTA reject), boundary (`_uiThreadId == -1` sentinel, null ambient), error handling (throwing capture source), concurrency (two racing STA callers), state transition (failed-then-successful retry). |

Determinism infrastructure note: `SharedStaDispatcherHost` and `StaDispatcherHost` both signal readiness
through an `AutoResetEvent` and shut down through `BeginInvokeShutdown` + `Join`, so no test leaves a
live dispatcher on a pooled worker. This is the hazard #782 finding C10 removed, and the delivery does
not reintroduce it.

## 2. General Code Change Policy Compliance

| Requirement (`.claude/rules/general-code-change.md`) | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | The predicate is a flat sequence of guarded returns; the retry fix is a `lock` plus a `bool`. `IUiCaptureSource` declares exactly the nine members `Initialize()` consumes and nothing more. |
| Reusability | PASS | The reflection over `UiThread` private statics is centralised in one helper so each field name appears once per assembly (`UiThreadStateScope.cs:162-173`). |
| Extensibility / no public break | PASS | `UiThread`'s public surface is unchanged. The two new members are `internal`. `SyncContextForm` gains an interface and no member. |
| Separation of concerns | PASS | The interface is what removes the WinForms dependency from the initialization path under test. |
| Mandatory toolchain loop | PASS | Format, lint, type-check, test completed in one clean pass (`p5-t1` through `p5-t5`, closure at `p5-t7`). Three earlier restarts are recorded and attributed to their own phases. |
| 500-line file limit | FAIL (non-blocking) | `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` measures 1067 lines at head (reviewer-verified with `awk 'END{print NR}'`), against a 1066-line baseline. See Finding F4. |
| Error handling — fail fast | PASS | The new precondition throws `InvalidOperationException` with a named prefix and the observed apartment. No exception is swallowed; a throwing `Initialize()` still propagates. |
| Logging | PASS | No ad-hoc console output added. The decision not to add a logger is stated in `spec.md` and `UiThread` has none today. |
| Naming | PASS | `PascalCase` types/members, `camelCase` locals, `_camelCase` private statics, matching the file's existing style. |
| Dependencies | PASS | No package added, removed, or upgraded. |
| I/O boundaries | PASS | The only I/O-ish dependency (a WinForms form) is now behind `IUiCaptureSource`. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement (`CLAUDE.md` C#1–C#7) | Verdict | Evidence |
|---|---|---|
| CSharpier formatting, pinned version, via `dotnet tool run` | PASS | `p5-t1-format.md` records byte-identical before/after `git status --porcelain --untracked-files=all` images (both empty) and `Formatted 1611 files`; `p5-t2-format-check.md` records `dotnet tool run csharpier check .` exit 0 at `Checked 1611 files`. The 1611 reconciles to the recorded `BASELINE_CHECKED_FILES: 1608` plus the three files this delivery creates — the reviewer confirmed exactly three new files in the changed-path set. |
| .NET analyzers via `/t:Rebuild` | PASS | `p5-t3-analyzer-build.md`: exit 0, `0 Warning(s)`, `0 Error(s)`, 18 projects, equal to the recorded baseline project count. `/t:Rebuild` used, with the correct rationale recorded (a warm `/t:Build` skips `CoreCompile` and runs no analyzer). |
| Nullable type-check via `/t:Rebuild /p:TreatWarningsAsErrors=true`, without `/p:Nullable=enable` | PASS | `p5-t4-nullable-build.md`: exit 0, `0 Warning(s)`, `0 Error(s)`. Both new/changed production files carry `#nullable enable` at line 1 (verified: `UiThread.cs:1`, `IUiCaptureSource.cs:1`), so they are inside the per-file opt-in the gate enforces. |
| Null-safety by default | PASS | `_syncContextForm` is `IUiCaptureSource?`; `ambient` is `SynchronizationContext?`; the `-1` sentinel is checked explicitly. |
| net48 constraints (no `init`, `record`, `record struct`) | PASS | No new value type; the new type is an interface. |
| Legacy `packages.config` `<Compile Include>` requirement | PASS | `UtilitiesCS.csproj` gains `Threading\IUiCaptureSource.cs`; `UtilitiesCS.Test.csproj` gains both new test files. The discovery-side confirmation required by `spec.md` is present: the discovered total rose by exactly 17. |
| Suppressions | PASS | No suppression, no `#pragma warning disable`, no `[ExcludeFromCodeCoverage]` added. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement (`CLAUDE.md` CUT1–CUT3) | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`, `[TestMethod]`, `[STATestClass]`, `[STATestMethod]`, `[DoNotParallelize]` only. |
| Moq for mocking | PASS | The one mock in the changed `QuickFiler.Test` method is `Mock<IQfcFormViewer>`. The `UtilitiesCS.Test` doubles are hand-written fakes, which is appropriate for a type that must control apartment and dispatcher identity. |
| FluentAssertions preferred | PASS | Every new assertion uses FluentAssertions. The two pre-existing `Assert.IsTrue` calls in the amended `QuickFiler.Test` method were carried unchanged, which is the correct minimal-diff choice. |
| No `Form`-derived type in `UtilitiesCS.Test` | PASS | `FakeUiCaptureSource` implements `IUiCaptureSource` without deriving from `Form`; `NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` reports `Passed` in the final full-suite TRX. |

## 5. Test Coverage Detail

All figures in this section were recomputed by the reviewer directly from
`coverage/809-p5-final.cobertura.xml` and `coverage/809-p0-baseline.cobertura.xml` using a class-level
`lines/line` selection with de-duplication by line number. They are not quoted from the executor's
artifacts.

### Coverage floor precedence (resolved explicitly)

`.claude/skills/policy-compliance-order/SKILL.md` places `CLAUDE.md` first and `.claude/rules/general-unit-test.md`
third. `CLAUDE.md` UT2 states a repository-wide floor of >= 80% line with >= 90% for newly added
modules, classes and methods. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`
state a uniform >= 85% line and >= 75% branch. The two are unreconciled. **This audit applies the
`CLAUDE.md` floors (80% line, 90% new code) as governing**, which is also what AC6 names, and reports
the `.claude/rules` figures alongside so the divergence is visible rather than resolved by preference.

### Per-language rows

- C# line coverage, file in scope `UtilitiesCS/Threading/UiThread.cs`: 121 of 126 executable lines, 96.03% — PASS against the governing 80% floor and PASS against the 85% figure as well. Reviewer-recomputed; matches `FINAL_UITHREAD_LINE_PCT: 96.03` exactly.
- C# line coverage, baseline for the same file: 63 of 82, 76.83% — reviewer-recomputed from the baseline Cobertura; matches `BASELINE_UITHREAD_LINE_PCT: 76.83` exactly. The delivery raises it by 19.20 points, so the no-regression obligation is met with margin, not tolerance.
- C# changed-line coverage across the three production Write Set files: 46 of 48 executable added lines, 95.83% — PASS against the 90% new-code floor.
- C# newly added members, worst row `SynchronizationContextAwaiter.IsCompleted` at 18 of 20 executable lines, 90.00% — PASS at exactly the 90% new-code floor, reviewer-recomputed over source lines 155-190. Every other new member measures 100%.
- C# repo-wide first-party line coverage, nine assemblies: 56248 of 66471, 84.62% — PASS against the governing `CLAUDE.md` 80% floor; that same figure would be a FAIL against the 85% line floor in `.claude/rules/quality-tiers.md`, which the precedence order does not make governing. The executor reported 84.62%; the reviewer's independent de-duplicated computation returns 84.62%.
- C# repo-wide first-party branch coverage: 13062 of 16956, 77.03% (reviewer-computed, de-duplicated) — PASS against the 75% branch floor. The executor reported 79.38% using an all-descendant selection that double-counts method-level line rows; both figures clear the floor, so the verdict is unaffected. See Finding F7.
- C# canonical coverage artifact `artifacts/csharp/coverage.xml`: FAIL — the canonical path does not exist in this worktree. Disposition is non-blocking: the equivalent evidence, two full Cobertura documents produced by the same Microsoft coverage engine, is present at `coverage/809-p0-baseline.cobertura.xml` and `coverage/809-p5-final.cobertura.xml`, and every figure in this section was independently derived from them by the reviewer. See Finding F6.
- PowerShell coverage: PASS — zero `.ps1` and `.psm1` files changed on this branch across the twelve-path changed set, so no PowerShell coverage obligation arises and no FAIL condition was found. Pester measures command and line coverage only, so no branch figure applies to it in any case.
- Python coverage: PASS — zero `.py` files changed on this branch, so no Python coverage obligation arises and no FAIL condition was found.
- TypeScript coverage: PASS — zero `.ts` and `.tsx` files changed on this branch, so no TypeScript coverage obligation arises and no FAIL condition was found.

### Residual uncovered lines in the file under change

Reviewer-verified uncovered set at head: `38, 39, 40, 177, 178`. Nothing else.

| Lines | Construct | Assessment |
|---|---|---|
| 38-40 | Body of `if (onLockupDetected is not null)` in `Init()` | The one test that supplies a callback supplies it to prove a rejected `Init()` performs no assignment, so the precondition throws first. Three lines, no branch left unevaluated at the condition itself (line 37 is covered). Non-blocking. |
| 177-178 | Body of `ReferenceEquals(_context, _uiSyncContext)` in the new predicate | The condition at line 176 is covered and evaluated false; only the true-arm body is unreached. This is the delivery's highest-risk branch and its least-covered one. See Finding F2 and the Q4 adjudication in the code review. Non-blocking. |

## 6. Test Execution Metrics

| Metric | Value | Verification |
|---|---|---|
| Full nine-assembly suite | 7137 total / 7137 passed / 0 failed, `SKIPPED_DERIVED: 0` | `p5-t5-tests-coverage.md`, TRX `p5t5.trx` counters quoted with `LastWriteTimeUtc` |
| Baseline total | 7120 | `p0-t12-vstest.md` token `BASELINE_TOTAL_TESTS:` |
| Delta | +17, matching the 17 methods added (4 + 6 + 7) | Reviewer counted the added `[TestMethod]`/`[STATestMethod]` declarations in the patch: 4 in `UiThreadInitApartmentContract_Tests`, 6 in `UiThreadInitRetryContract_Tests`, 7 in `SynchronizationContextAwaiter_Tests`. Exact. |
| Targeted `UtilitiesCS.Test` run | 82/82 passed, two passes | `p4-t4-utilitiescs-tests.md` |
| Targeted `QuickFiler.Test` run | 70/70 passed, two passes | `p4-t3-quickfiler-tests.md` |
| Fail-before | 22 discovered / 16 passed / 6 failed, exit 1, `[expect-fail]` | `p2-t10-fail-before.md`, re-measured pass of record |
| Pass-after | 22/22, exit 0 | `p3-t6-pass-after.md` |
| #780 flake control | 3 dedicated repetitions plus the full-suite row, all `Passed` | `p5-t6-tryaddvalues-rep1/2/3.md`, `REPETITIONS_PASSED: 3` |
| Toolchain clean pass | `TOOLCHAIN_LOOP_CLEAN_PASS: 1` | `p5-t7-loop-closure.md` |

## 7. Code Quality Checks

| Gate | Result | Command recorded |
|---|---|---|
| Format | exit 0, tree unchanged | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` |
| Lint | exit 0, 0 warnings, 0 errors, 18 projects | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` |
| Type-check | exit 0, 0 warnings, 0 errors | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` |
| Test | exit 0, 7137/7137 | `dotnet-coverage collect ... -- vstest.console.exe ... /InIsolation` — see the AC6 collector deviation in Finding F5 |
| Repository hygiene | `DELIVERY_ADDED_RESULTS_FILE_COUNT: 0` | `p6-t12-untracked-output-check.md`; reviewer confirmed `coverage/` holds only `.gitkeep` as a tracked path and that the two 18 MB Cobertura documents are git-ignored, so no large binary blob enters history |
| Host-token sanitisation | 43 artifacts scanned, 0 hits, plus a second pass over the scanner's own artifact | `p6-t14-artifact-sanitisation.md` |

## 8. Gaps and Exceptions

### F1 — `[P0-T15]` apartment inference is unsound (Medium, non-blocking)

`evidence/other/p0-t15-mta-synccontextform-measurement.md` records `MTA_INITIALIZE_OUTCOME: COMPLETED`
and infers the executing apartment was MTA from research R4's premise that a plain `[TestMethod]` runs
MTA. The same delivery falsified that premise: `p2-t10-fail-before.md` records the verbatim TRX message
`Expected Thread.CurrentThread.GetApartmentState() to be ApartmentState.MTA {value: 1}, but found
ApartmentState.STA {value: 0}.` Full reasoning is in the code review under Q2. The reviewer's
determination is that the `[P0-T15]` run most likely executed **STA**, in which case no MTA measurement
exists and the refutation of the #782 narrative is unsupported. The AC2 design is unaffected. Recorded
against AC5.

### F2 — the highest-risk branch is the least-covered (Low, non-blocking)

`UiThread.cs:177-178` is the true arm of `ReferenceEquals(_context, _uiSyncContext)`, which is exactly
the clause with the stale-thread-id residual described in F3. Adjudicated in the code review under Q4.

### F3 — stale `_uiThreadId` residual in the new predicate (Low, non-blocking)

Adjudicated in the code review under Q1. Unreachable in production; not reached by any test; a
hardening recommendation is recorded.

### F4 — 500-line limit exceeded at head in a touched test file (FAIL-level, non-blocking)

`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` is 1067 lines at head against a
1066-line baseline. The rule in `.claude/rules/general-code-change.md` admits no exception for test
code. The overrun is pre-existing at 566 lines over the limit; this delivery adds exactly one
`[DoNotParallelize]` attribute line, which `spec.md`'s Write Set amendment establishes as necessary for
the shared-static serialization guarantee to hold at all. Splitting a 1066-line unrelated test file is
outside the approved Write Set and would be a large unrelated change. Disposition: non-blocking, with a
follow-up issue recommended.

### F5 — AC6 literal wording deviations (Low, non-blocking)

Two clauses of AC6 are not met literally: the collector (`dotnet-coverage collect` substituted for
`/EnableCodeCoverage`, documented as `AC6-COLLECTOR-SUBSTITUTION`) and the storage location (the raw
Cobertura document is git-ignored under `coverage/` rather than stored under `evidence/qa-gates/`;
what is stored there are three markdown artifacts recording figures derived from it). Adjudicated in
the feature audit.

### F6 — canonical C# coverage artifact absent (procedural FAIL, non-blocking)

Recorded in section 5. Not a code defect.

### F7 — aggregate branch-coverage measurement method (Low, non-blocking)

`p6-t3-aggregate-coverage.md` and `p5-t5-tests-coverage.md` compute branch coverage by summing
`condition-coverage` over an all-descendant `.//line` selection, which counts method-level rows in
addition to class-level rows. The reviewer's de-duplicated computation returns 77.03% where the
artifacts report 79.38%. Line percentage is unaffected (both 84.62%) because the duplication is
close to proportional on lines. Both figures clear the 75% floor, so no verdict changes. The
comparison to baseline remains valid because both sides used the same method.

### F8 — evidence artifact `Timestamp:` headers run ahead of wall clock (Informational)

Declared timestamps are 1.5 to 2 hours later than the artifacts' own file mtimes (for example
`p5-t5-tests-coverage.md` declares `2026-09-08T02-50` with mtime `2026-09-08 01:06:21 -0400`, and
`p6-t13` declares `03-16` with mtime `01:14`). The `LastWriteTimeUtc` values the artifacts quote for
their TRX files do reconcile with the mtimes at UTC-4, so the runs themselves are corroborated and
nothing appears fabricated; only the human-authored header stamps drift. The
`evidence-and-timestamp-conventions` skill fixes the format but not the source clock, so this is an
accuracy observation rather than a rule violation.

### F9 — the published environmental finding names a mechanism the tree does not support (Low, non-blocking)

`p6-t13-closure-summary.md` section 6 tells future planners that a plain `[TestMethod]` runs STA
"when a `[TestClass] [DoNotParallelize]` class shares the serial execution bucket with an
`[STATestClass] [DoNotParallelize]` class". The reviewer's reading of the tree supports a simpler and
broader explanation, given in the code review under Q2. Since that note is written for future
planners, the wrong mechanism can propagate; a correction is recommended.

### F10 — follow-up candidates exist only as prose (Process, non-blocking)

`p6-t13-closure-summary.md` section 5 lists six follow-up candidates, including the two coverage
residuals, the 500-line overrun, and the `IUiDispatcher` routing. Prose in a feature folder does not
survive merge. Recommend promoting the durable ones through the promotion lifecycle into real issues.

### Accepted, pre-existing, or out-of-delivery items (not findings)

- The 80% versus 85% coverage-floor divergence between `CLAUDE.md` and `.claude/rules` is a standing
  governance conflict, recorded in `spec.md` and resolved here by the documented precedence order.
- The eleven production await sites at which the predicate change can alter ordering have no ordering
  test today. The delivery records this as residual rather than covered, which is the correct
  disposition; live-host verification is explicitly outside the acceptance criteria.

## 9. Summary of Changes

12 files, 1134 insertions, 34 deletions, across 9 commits.

Production (4 files):
- `UtilitiesCS/Threading/UiThread.cs` — STA precondition as the first statement of `Init()`; `lock (InitLock)` plus a success-recorded `_initialized` flag replacing `ThreadSafeSingleShotGuard`; `SyncContextFormFactory` and `ResetForTesting()` seams; the replaced `IsCompleted` predicate; the `NonStaInitMessagePrefix` constant.
- `UtilitiesCS/Threading/IUiCaptureSource.cs` — new internal interface, 9 members, no executable line.
- `UtilitiesCS/Threading/SyncContextForm.cs` — declaration only.
- `UtilitiesCS/UtilitiesCS.csproj` — one `<Compile Include>`.

Test (8 files): two new files (`UiThreadInitContract_Tests.cs`, `UiThreadStateScope.cs`), 243 added
lines in `UiThread_Tests.cs`, the reconciled `QfcHomeControllerRunAsyncTests` method, three
one-attribute additions, and one `<Compile Include>` pair.

## 10. Compliance Verdict

**PASS with 0 blocking findings.**

Ten findings are recorded; all are non-blocking. Two acceptance criteria (AC5, AC6) are graded PARTIAL
for evidence-labelling and literal-wording reasons and are detailed in the feature audit. No
remediation-inputs artifact is produced, because no finding requires code, test, or plan rework before
merge. The recommended actions are: correct the `[P0-T15]` apartment label and the `p6-t13` section 6
mechanism note, and promote the durable follow-ups to issues.

## Appendix A: Test Inventory

Added (17):

`UiThreadInitApartmentContract_Tests` (4): `Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState`,
`Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged`,
`Init_OnStaThread_DoesNotThrowAndPopulatesAllFourCaptureFields`,
`Init_ApartmentBoundaryIsStaEqualityNotMtaInequality_RejectsFromMtaAndAcceptsFromSta`.

`UiThreadInitRetryContract_Tests` (6): `Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields`,
`Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset`,
`AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory`,
`Init_CalledConcurrentlyFromTwoStaThreads_InvokesTheFactoryExactlyOnce`,
`Init_WithMonitorUiThreadEnabled_ConstructsAndRunsTheThreadMonitorWithTheInjectedTimeProvider`,
`UiSyncContext_ReadWithNullBackingFieldFromStaThread_InitializesThroughTheLazyPath`.

`SynchronizationContextAwaiter_Tests` (7): `IsCompleted_WhenAmbientContextIsTheCapturedInstance_ReturnsTrue`,
`IsCompleted_WhenAmbientContextIsNullAndCapturedContextIsNotNull_ReturnsFalse`,
`IsCompleted_WhenUiThreadIdIsTheMinusOneSentinel_ReturnsFalse`,
`IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue`,
`IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse`,
`IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse`,
`IsCompleted_OnDefaultAwaiterOnAContextFreeThread_ReturnsTrue`.

Modified (1): `QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly`,
signature changed to `async Task`, not renamed.

Pinned regression guards re-run and green: both `WinFormsPumpHostTests` marshal tests,
`EfcFormControllerTests.ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows`,
both `UiThread_Dispatcher_Tests` methods, `WpfDispatcherYieldTests`, `FolderPredictorTests`
reflection test, and both `[STATestClass]` viewer tests.

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .` / `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `dotnet-coverage collect --output coverage\809-p5-final.cobertura.xml --output-format cobertura --settings coverage\809-effective-coverage.config -- vstest.console.exe <nine assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation` — substituted for `vstest.console.exe ... /EnableCodeCoverage` per `AC6-COLLECTOR-SUBSTITUTION`; see Finding F5.
