# Policy Compliance Audit — quickfiler-test-uithread-dispatcher (#493)

- **Feature folder:** `docs/features/active/quickfiler-test-uithread-dispatcher-493`
- **Branch under review:** `bug/quickfiler-test-uithread-dispatcher-493` (HEAD `98113b09`)
- **Base for diff:** `125c36b0669d9dd6095f156901bba138e2272f56` — tip of `epic/quickfiler-bug-family-integration`. Merge-base of HEAD and this SHA re-verified by the reviewer as `125c36b0` itself, so the two-dot diff attributes no sibling-feature changes to this branch.
- **Work mode:** `full-bug` (from `issue.md` marker) — `spec.md` is the sole acceptance-criteria source.
- **Reviewer timestamp:** 2026-08-27T15-07
- **Template note:** The `policy-audit-template-usage` skill requires resolving the template through the MCP tool `mcp__drm-copilot__resolve_policy_audit_template_asset`; no MCP tools are exposed in this review session. Per the skill's fallback provision, this artifact preserves the canonical major headings and documents the missing template resolution here rather than blocking the review.

## Executive Summary

Verdict: **PASS — 0 Blocking findings, 5 Non-blocking findings.**

The branch is a test-infrastructure-only bug fix. The reviewer independently confirmed via `git diff --name-status <base>..HEAD` that exactly five build-relevant paths changed — four `QuickFiler.Test/Controllers/*.cs` files and `QuickFiler.Test/QuickFiler.Test.csproj` (two `<Compile Include>` entries) — and that zero production source, project, props, targets, solution, or packages.config files changed. All other changed paths are Markdown (feature evidence, spec/plan checkbox flips, one promoted potential-bug record, and executor agent-memory notes).

The fix replaces an unsynchronized, never-restoring reflection mutation of the process-wide static `UtilitiesCS.Threading.UiThread._dispatcher` with a single-owner fixture (`UiThreadDispatcherFixture`) implementing a two-lock protocol (`TransactionGate` → `FieldLock`, never the reverse) and `IDisposable` restore scopes with `ReferenceEquals` compare-then-write semantics. Six new regression tests (R1–R6) all pass; the full `QuickFiler.Test` suite is 1072/1072 passed against a 1066/1066 baseline (+6 = exactly R1–R6).

## Rejected Scope Narrowing

None detected. The caller's instructions mandated the full branch-vs-integration-base diff, and the reviewer independently confirmed the supplied base SHA equals `git merge-base HEAD 125c36b0`. No instruction attempted to narrow language coverage, file scope, or toolchain checks. The instruction to diff against the epic integration branch rather than `main` is correct base resolution for an epic child (three sibling features' changes would otherwise be misattributed), not scope narrowing.

## Evidence Location Compliance

- All executor evidence lives under `docs/features/active/quickfiler-test-uithread-dispatcher-493/evidence/<kind>/` — the canonical `<FEATURE>/evidence/<kind>/` location. Verified by enumerating the branch diff: zero changed paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`.
- A recursive scan of the feature `evidence/` tree found **zero non-Markdown files** — no retained `.ps1`, `.py`, or other scripts that could distort extension-based language detection.
- `validate_evidence_locations.py` does not exist in this repository; the scan above was performed manually and is recorded here in its place.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required — no caller instruction specified a non-canonical evidence path.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| Independence / Environment Stability (the defect under repair) | PASS | Every mutation of `UiThread._dispatcher` in the owned files now routes through `UiThreadDispatcherFixture`; reviewer grep confirms the only `typeof(UiThread)` reflection swap in owned files is `UiThreadDispatcherFixture.cs:135`. Restore is conditional and idempotent (`EnsureScope.Dispose`, `UiThreadDispatcherTransaction.Dispose`). |
| Isolation | PASS | R1–R6 each target one contract clause of the fixture/transaction; failures identify the faulty behavior by name. |
| Fast execution | PASS | R1–R6 measured at 1–7 ms each (`evidence/qa-gates/quickfiler-test-run.2026-08-27T11-19.md`). |
| Determinism | PASS | Reviewer grep of the four owned files: zero occurrences of `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Stopwatch`, `Environment.TickCount`, `SpinWait`, or timed `.Wait(n)`. Coordination uses `ManualResetEventSlim`, awaited `Task` completion, and `SemaphoreSlim.WaitAsync()` released by the holder's `Dispose`, never by elapsed time. The six `[Timeout(GateTimeoutMs)]` attributes convert a genuine deadlock into a bounded failure; no code path uses the timeout as a synchronization mechanism. Corroborated by `evidence/qa-gates/determinism-audit.2026-08-27T11-39.md` (20/20 token-path combinations clean). |
| Readability / documented intent | PASS | Every test carries an XML doc comment naming its scenario (R1–R6) and Arrange–Act–Assert section comments. |
| No external dependencies / no temporary files | PASS | No file, network, or process dependency in the four owned files; zero matches for `Path.GetTempFileName` / `Path.GetTempPath` / `Path.GetRandomFileName`. |
| Scenario completeness | PASS | Positive (R1, R2), idempotence/negative (R3, R5, R6 fail-fast), concurrency (R4), state transitions (install/restore) all covered. |
| Test file location | PASS (repo convention) | Tests live in the `QuickFiler.Test` MSTest project mirroring `QuickFiler`, which is this repository's established layout. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| Bugfix workflow — failing regression first | PASS | The defect is a `void`-signature helper, so no red test run can exist pre-fix. The plan captured the honest form: pre-change source excerpt (`evidence/regression-testing/fail-before-exception.2026-08-27T10-27.md`) plus a compile-level red demonstration — three distinct `CS0029` errors when R1–R3 compile against the `void` signature (`evidence/regression-testing/fail-before-compile.2026-08-27T10-44.md`, `EXIT_CODE: 1` expected). |
| Minimal, targeted fix | PASS | Five build-relevant files, all in the test project; both modified files shrank (−49 and −25 lines). |
| File size limit (500 lines) | PASS | Reviewer-measured: `TestSupport.cs` 440, `InitializationTests.Part2.cs` 393, `UiThreadDispatcherFixture.cs` 278, `UiThreadDispatcherFixtureTests.cs` 346. Sibling-owned `FocusAndThemeTests.cs` remains 497 (unmodified). |
| Simplicity / separation of concerns | PASS | One fixture owns the static's mutation; the two-lock design is documented in-code with rationale; no I/O in the fixture. |
| Error handling — fail fast | PASS | `Install` called twice throws `InvalidOperationException` (R6); `ResolveDispatcherField` asserts the backing field exists at initialization. |
| Comments explain why | PASS | Lock-ordering rationale, the deliberate `EnsureDispatcher`-off-the-gate decision, and the parked-dispatcher lifetime are all documented at the point of use. |
| No policy documents modified | PASS | No changed path under `.claude/rules/` or `.github/instructions/`. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| CSharpier formatting | PASS (evidence-verified) | `evidence/qa-gates/csharpier-check.2026-08-27T11-10.md`: `dotnet tool run csharpier check .` exit 0, 1542 files. The reviewer cannot rerun builds in this session; the committed gate artifacts are the verification basis. |
| Analyzer gate (`/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) | PASS (evidence-verified) | `evidence/qa-gates/msbuild-analyzers.2026-08-27T11-13.md`: exit 0, 5 warnings / 0 errors — identical counts to the Phase 0 baseline. The 5 warnings are pre-existing `System.Reactive.PackagesConfigCheck.targets` packages.config notices. |
| Nullable gate (`/t:Rebuild`, `TreatWarningsAsErrors=true`, without `/p:Nullable=enable`) | PASS (evidence-verified) | `evidence/qa-gates/msbuild-nullable.2026-08-27T11-16.md`: exit 0, 5 warnings / 0 errors. The command matches CI (`ci.yml`) and correctly omits `/p:Nullable=enable`. |
| Naming / XML docs | PASS | PascalCase types/members, camelCase locals; public-surface members of the fixture and transaction carry XML documentation. |
| No new dependencies | PASS | No packages.config or reference changes; the csproj diff is two `<Compile Include>` entries only. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| MSTest framework only | PASS | `[TestClass]`/`[TestMethod]`/`[Timeout]` from `Microsoft.VisualStudio.TestTools.UnitTesting`; no xUnit/NUnit references introduced. |
| Moq for mocking | PASS | No new mocks were needed; the pre-existing `Mock<IWebViewCoreInitializer>` usage in the pump harness is unchanged. |
| FluentAssertions | PASS | All new assertions use FluentAssertions with `because:` rationales producing actionable failure messages. |
| Toolchain command selection (CUT3) | PASS (evidence-verified) | The four commands in `evidence/qa-gates/` match CLAUDE.md § CUT3, including `vstest.console.exe ... /EnableCodeCoverage /InIsolation`. |

## 5. Test Coverage Detail

This change adds **zero production lines**; all 624 added lines are in the `QuickFiler.Test` assembly, which the coverage pipeline correctly excludes from the instrumented denominator (test-file exclusion is required by policy).

| Language | Row |
| --- | --- |
| C# | C# coverage: **FAIL** — canonical artifact `artifacts/csharp/coverage.xml` is absent, and coverage verification is mandatory for every language with changed files; verdict recorded per the artifact-absence rule. Disposition: **Non-blocking** (NB-2, § 8) — the committed numeric coverage evidence fully substitutes: raw whole-repo Cobertura `line-rate 0.19049434489769984`, `branch-rate 0.16177560720359307`, `lines-valid 78690` are **byte-identical** to the Phase 0 baseline triple (delta 0.00 percentage points), and the recomputed filtered first-party figure is 22.8059%. No changed line lost coverage because no production line changed. |
| C# (repo floor context) | The sub-85% repo-wide C# line coverage figure and sub-75% branch figure are pre-existing whole-repo denominator properties (unfiltered, vendor/COM/VSTO-inflated) that this test-only change neither caused, moved, nor can remediate; the branch's coverage delta vs baseline is exactly zero. FAIL on the row above is therefore procedural (artifact absence + pre-existing floor shortfall), not a defect of this change. |
| TypeScript | TypeScript coverage: **PASS** — zero TypeScript files exist in the branch diff (verified via `git diff --numstat`; no `.ts`/`.tsx` paths), so no TypeScript coverage obligation attaches to this branch. |
| Python | Python coverage: **PASS** — zero Python files exist in the branch diff (no `.py` paths), so no Python coverage obligation attaches to this branch. |
| PowerShell | PowerShell coverage: **PASS** — zero PowerShell files exist in the branch diff (no `.ps1`/`.psm1` paths), so no PowerShell coverage obligation attaches to this branch. |

Per-file coverage for the four changed files: all four are test files, excluded from the coverage denominator by policy (UT2: coverage tooling excludes test files so metrics reflect application code). New-code (90%) and modified-file (80%) floors apply to production code; no production file was added or modified.

Note on the coverage wrapper exit code: `Invoke-MSTestWithCoverage.ps1` exited 1 solely because `Assert-CoberturaLineCoverageThreshold` threw on the 80% floor against the recomputed 22.8059% first-party rate. All 1072 tests passed in that run. This is a pre-existing repository condition, not a regression introduced by this branch.

Deliberate omission, recorded: no `artifacts/csharp/coverage.xml` was generated by this review. The reviewer's delegation explicitly prohibited emitting a coverage XML artifact for this test-only change; coverage is recorded numerically above and in `evidence/qa-gates/quickfiler-test-coverage.2026-08-27T11-23.md` / `evidence/baseline/quickfiler-test-coverage-baseline.2026-08-27T10-25.md`.

## 6. Test Execution Metrics

| Metric | Baseline (P0-T12) | Final (P3-T5) |
| --- | --- | --- |
| Total tests (`QuickFiler.Test`, `TestCategory!=LiveOutlook`) | 1066 | 1072 |
| Passed | 1066 | 1072 |
| Failed | 0 | 0 |
| Skipped | 0 | 0 |

Delta: +6, exactly the six new regression tests R1–R6 (all named in the passed list with 1–7 ms durations). Both AC-6-named theme tests (`SetThemeDark_FromNormal_SelectsDarkNormalTheme`, `SetThemeLight_FromNormal_SelectsLightNormalTheme`) pass. The parallelized supplementary run (P3-T6) also reports 1072/1072 passed.

## 7. Code Quality Checks

| Check | Result |
| --- | --- |
| Formatting (`dotnet tool run csharpier check .`) | exit 0 (evidence) |
| Analyzers (msbuild Rebuild + analyzers) | exit 0, 5 pre-existing warnings, 0 errors (evidence) |
| Nullable/type-check (msbuild Rebuild + TreatWarningsAsErrors) | exit 0, 5 pre-existing warnings, 0 errors (evidence) |
| Tests (vstest, /InIsolation) | exit 0, 1072/1072 (evidence) |
| Reviewer static checks | file sizes, determinism greps, lock-ordering trace, `async void` scan (zero matches), single-swap-implementation grep — all clean |

## 8. Gaps and Exceptions

All findings are classified explicitly; none is Blocking.

- **NB-1 (Non-blocking) — Plan gate P4-T2's literal acceptance condition failed and the task was checked off.** P4-T2 required byte-exact set equality of msbuild-log lines containing `QfcItemController.FocusAndThemeTests.cs`; equality did not hold. The reviewer independently reproduced the executor's explanation from the retained extract files (`TestResults/plan-logs/p0-t10/`, `.../p4-t2/`): after deleting exactly the two added compile-input tokens (`Controllers\QfcItemController.UiThreadDispatcherFixture.cs`, `...FixtureTests.cs`) from the final extracts, both the analyzer-step and nullable-step extracts become byte-identical to their baselines, and the diagnostic-bearing subset is zero on both sides. The gate was structurally unsatisfiable as written (every matching log line is a `csc.exe` invocation enumerating the project's whole source set, which this change necessarily grows). The deviation is fully disclosed in `evidence/qa-gates/unowned-file-diagnostics-comparison.2026-08-27T11-30.md` and `evidence/other/ac-checkoff-ac6.2026-08-27T11-59.md`. Impact: none on any spec AC (see feature-audit § AC-6). Remediation owed: none for this branch; future plans should not gate on raw compiler-invocation text.
- **NB-2 (Non-blocking) — C# coverage row FAIL is procedural.** See § 5. The canonical coverage XML is absent by deliberate, instructed omission, and the repo-wide figure is below the floor for pre-existing reasons unrelated to this branch (zero coverage delta, zero production lines changed). No remediation is required of this feature.
- **NB-3 (Non-blocking) — Residual R-1 is real but tracked.** `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:42-51` still swaps `UiThread._dispatcher` by raw reflection outside both locks. The reviewer verified it restores the prior value in a `finally` (`WpfUiDispatcherTests.cs`, finally block), so it is a latent lost-update/ordering hazard against a concurrent transaction, not a recurrence of the no-restore defect. It is outside this feature's owned set, is spec residual risk R-1, and is tracked as GitHub issue **#648** (verified OPEN via `gh issue view 648`), with promotion evidence at `evidence/issue-updates/issue-r1-followup-completed.2026-08-27T14-53.md`. Cross-assembly mutators in `UtilitiesCS.Test` are residual R-2, likewise out of scope.
- **NB-4 (Non-blocking) — Exception-safety hardening opportunity in the restore paths.** (a) `PumpHarness.Restore()` (`InitializationTests.Part2.cs:313-326`) sets `_restored = true`, then calls `TokenSource.Dispose()` before `_transaction.Dispose()` without try/finally; a hypothetical throw from `TokenSource.Dispose()` would permanently skip both the restore and the gate release with no retry possible. (b) `UiThreadDispatcherTransaction.Dispose()` calls `CompareExchange` before `ReleaseTransactionGate()` without try/finally, so a hypothetical restore throw would leak the gate. `CancellationTokenSource.Dispose()` and `FieldInfo.SetValue` on a resolved static field are non-throwing in practice, and every downstream consumer is `[Timeout]`-bounded, so this is theoretical; a `try/finally` in each would close it. Recommend as follow-up polish, not remediation.
- **NB-5 (Non-blocking) — Deliberate design consequence: an `EnsureScope` disposed while a transaction's value occupies the field skips its restore permanently.** If a transaction installs over the parked seed and the ensure scope is disposed before the transaction restores, the transaction's restore later reinstates the parked dispatcher with no remaining owner — the same steady-state leak the pre-fix helper produced (spec R-3 records that unowned `Ensure` callers still discard their scope, so exposure is unchanged). This is the accepted cost of keeping `EnsureDispatcher` off `TransactionGate` (which is what keeps un-`[Timeout]`-ed callers hang-free); recorded so future readers do not mistake it for an oversight.

The MCP policy-audit template asset could not be resolved in this session (no MCP tools exposed); this artifact preserves the canonical headings per the skill's fallback provision.

## 9. Summary of Changes

- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (new, 278 lines): `UiThreadDispatcherFixture` (FieldLock-atomic `Current`/`Exchange`/`CompareExchange`, gate-free `EnsureDispatcher`, `BeginTransactionAsync`) and `UiThreadDispatcherTransaction` (one-shot `Install`, idempotent restore-before-release `Dispose`).
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (new, 346 lines): regression tests R1–R6, all `[Timeout(60000)]`.
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (440 lines, −49): `EnsureUiThreadDispatcher` now returns `IDisposable`, delegating to the fixture; private parked-dispatcher machinery removed (relocated into the fixture).
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` (393 lines, −25): private `UiThreadDispatcherGate` and `SwapUiThreadDispatcher` removed; `BuildPumpHarnessAsync`/`PumpHarness` now consume the shared transaction, preserving the acquire-at-build-start hold window and restore-before-release ordering.
- `QuickFiler.Test/QuickFiler.Test.csproj`: two `<Compile Include>` entries.
- Documentation: feature evidence tree, spec/plan checkbox flips, promoted record for #648, two executor agent-memory notes.

## 10. Compliance Verdict

**PASS.** Zero Blocking findings. Five Non-blocking findings (NB-1 through NB-5) recorded in § 8, none requiring remediation before merge into the epic integration branch. No `remediation-inputs` artifact is produced because no remediation-required finding exists.

## Appendix A: Test Inventory

New tests (all in `QfcItemController_UiThreadDispatcherFixtureTests`, MSTest, `[Timeout(60000)]`):

| ID | Test | Contract clause |
| --- | --- | --- |
| R1 | `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | Ensure installs only into a null field; #230 clobber precondition unreachable (primary deterministic assertion) |
| R2 | `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | Seed + conditional restore to null |
| R3 | `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | Scope double-dispose neither throws nor re-writes |
| R4 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | Restore strictly precedes gate release (supporting probabilistic assertion) |
| R5 | `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` | No `SemaphoreFullException`; gate stays sound |
| R6 | `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` | One-shot install fails fast |

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier check .` — exit 0 (evidence `qa-gates/csharpier-check.2026-08-27T11-10.md`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — exit 0 (evidence `qa-gates/msbuild-analyzers.2026-08-27T11-13.md`)
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` — exit 0 (evidence `qa-gates/msbuild-nullable.2026-08-27T11-16.md`)
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"` — exit 0, 1072/1072 (evidence `qa-gates/quickfiler-test-run.2026-08-27T11-19.md`)
