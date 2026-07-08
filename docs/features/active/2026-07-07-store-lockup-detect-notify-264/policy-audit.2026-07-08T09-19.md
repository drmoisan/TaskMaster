# Policy Audit — store-lockup-detect-notify (F4, Issue #264)

- Timestamp: 2026-07-08T09-19
- Reviewer: feature-review
- Branch: `feature/store-lockup-detect-notify-264`
- Base (resolved): `epic/store-lockup-resilience-integration` (epic child -> integration PR)
- Merge-base: `6a525937`
- F4 implementation commit: `e0b58302`
- Work Mode: `full-feature` (AC sources: `spec.md` AC1–AC10 and `user-story.md` 5 ACs)

## Executive Summary

The F4 change set delivers UI-thread lockup detection, per-store attribution, immediate
auto-disable, a modeless three-button notification, and one WARN log line. All changed languages
were reviewed against the full branch diff (`git diff 6a525937 HEAD`). Only C# production and C#
test files carry executable changes; the remaining changes are Markdown evidence/docs and project
manifests. The full C# toolchain was reported green by the orchestrator on the merged state and is
corroborated by the committed QA-gate evidence. No blocking findings.

Overall policy verdict: PASS.

## Scope and Baseline

- Full-branch diff reviewed: `6a525937..HEAD` (F4 implementation isolated at `e0b58302`).
- Changed production C# files: `UtilitiesCS/Threading/{StoreLockupResponder,LockupStallDecider,
  CurrentStoreContext,ThreadMonitor,UiThread}.cs`,
  `UtilitiesCS/OutlookObjects/Store/{StoreLockupAttribution,StoreWrapper,StoresWrapper}.cs`,
  `UtilitiesCS/Dialogs/MyBoxModeless.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`,
  `TaskMaster/ThisAddIn.cs`.
- Changed test C# files: `UtilitiesCS.Test/{Threading,Dialogs,OutlookObjects/Store}/*Tests.cs`,
  `TaskMaster.Test/AppGlobals/AppOlObjectsAttributionContextTests.cs`.
- Manifest changes: `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`, `TaskMaster.Test.csproj`,
  `UtilitiesCS.Test/packages.config` (adds test-only `Microsoft.Extensions.TimeProvider.Testing`
  9.0.0, mirroring existing `QuickFiler.Test` wiring).

## Rejected Scope Narrowing

None. The caller correctly scoped the audit to the full F4 change set against the integration base
branch (a legitimate epic-child -> integration base per `pr-base-branch-merge-base`). No instruction
attempted to narrow scope, mark a language out of scope, or skip a coverage check.

## 1. Language Toolchain and Coverage Verdicts

### 1.1 Changed-language inventory

| Language | Changed files in branch diff | Coverage verdict required |
|---|---|---|
| C# | Yes (10 production, 7 test) | PASS/FAIL required |
| PowerShell | No | N/A (zero changed files) |
| TypeScript | No | N/A (zero changed files) |
| Python | No | N/A (zero changed files) |

### 1.2 C# Toolchain (CLAUDE.md CUT3 order)

| Stage | Command | Result | Evidence |
|---|---|---|---|
| Format | `csharpier .` | EXIT 0 (1312 files, 0 need formatting) | `evidence/qa-gates/qa-01-format.md`; orchestrator report |
| Analyzers | `msbuild ... /p:EnableNETAnalyzers /p:EnforceCodeStyleInBuild` | EXIT 0 (baseline warnings, 0 new) | `evidence/qa-gates/qa-02-analyzers.md` |
| Nullable / TWAE | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors` | EXIT 0 | `evidence/qa-gates/qa-03-nullable.md` |
| Tests | `vstest.console.exe ... /TestCaseFilter:"TestCategory!=LiveOutlook"` | EXIT 0 (4481–4488 passed / 0 failed) | `evidence/qa-gates/qa-04-test-coverage.md`; `qa-07-postmerge-toolchain-verify.md` |

Verdict: PASS.

### 1.2.1 C# Coverage (verified from execution-produced artifacts, not re-run)

Coverage was measured during execution via a Cobertura runsettings vstest run over both test DLLs
and recorded in the canonical evidence location. The prompt's per-language artifact path
`artifacts/csharp/coverage.xml` is absent in the working tree; coverage is instead verified from the
committed QA-gate evidence (`evidence/qa-gates/qa-04-test-coverage.md`,
`qa-05-coverage-delta.md`), which carry concrete per-file and per-package Cobertura figures. This is
the required evidence-verification model. The absence of the loose `artifacts/csharp/coverage.xml`
file is recorded as an observation, not a FAIL, because the coverage numbers are present, specific,
and traceable to a named vstest command.

- New-code coverage (per F4 file, `[ExcludeFromCodeCoverage]`-honoring):
  - Baseline: n/a (all files new to coverage or additively wrapped).
  - Post-change: CurrentStoreContext.cs 92.3%; LockupStallDecider.cs (+LockupAttribution) 100.0%;
    StoreLockupAttribution.cs 100.0%; StoreLockupResponder.cs 96.1%; ThreadMonitor.cs (testable
    seam) 100.0%; MyBoxModeless.cs (host-Show overload exempt) 100.0%.
  - New/changed-code coverage: 97.7% (aggregate 334/342).
  - Change: increase.
  - Disposition: PASS. Every F4 new file is at or above the >= 90% new-code threshold.
  - Evidence: `evidence/qa-gates/qa-04-test-coverage.md` lines 16–30; `qa-05-coverage-delta.md`
    Check (b).
- Repo-wide first-party (testable denominator, CLAUDE.md COM/VSTO/WinForms exemption):
  - Baseline: UtilitiesCS 88.25% (P0-T9 same-methodology).
  - Post-change: UtilitiesCS 90.50% (`[ExcludeFromCodeCoverage]`-honoring) / 88.41% (raw
    same-methodology as baseline).
  - New/changed-code coverage: n/a for this row (see new-code row above).
  - Change: increase (88.25% -> 88.41% raw; 90.50% honoring).
  - Disposition: PASS. UtilitiesCS (the primary first-party testable assembly carrying the bulk of
    F4) is 90.50% >= 80% floor; no regression on any first-party package (overall 56.51% -> 56.69%
    UP; TaskMaster 66.53% -> 66.57% UP).
  - Evidence: `evidence/qa-gates/qa-05-coverage-delta.md` Checks (a) and (c).

Repo-wide raw-root note: the whole-repo Cobertura root reads 60.82% (honoring) / 56.69% (raw). This
figure is not a valid repo-wide gate value because the F4-scoped two-DLL run does not instrument
assemblies outside the change set (QuickFiler, ToDoModel, Tags, TaskVisualization) and includes
vendored packages outside the CLAUDE.md testable denominator. The authoritative repo-wide gate is
the full-suite PR CI run; the governing local first-party measure is UtilitiesCS 90.50%. This is
consistent with the CLAUDE.md testable-denominator exemption.

C# coverage verdict: PASS.

## 2. General Code Change Policy

| Rule | Verdict | Evidence |
|---|---|---|
| Simplicity / separation of concerns | PASS | Pure decider (`LockupStallDecider`), pure formatter (`StoreLockupAttribution`), host-neutral orchestrator (`StoreLockupResponder`), host-bound shell isolated (`ThreadMonitor.Run/Tick`, `[ExcludeFromCodeCoverage]`). |
| Error handling / fail-fast | PASS | `StoreLockupResponder` ctor null-guards `disableService`/`dispatcher` with `ArgumentNullException`; guards enforce no-context and already-disabled early returns. |
| Logging pattern | PASS | Single `[store-lockup]` WARN via injected sink defaulting to log4net `Log.Warn`; no ad-hoc console output on the attribution path. |
| Public API / compatibility | PASS | `UiThread.Init` extended with optional parameters (defaults preserve prior behavior); no breaking signature change. |
| File size <= 500 lines | PASS | All changed production files <= 500 (largest: AppOlObjects.cs 472, StoresWrapper.cs 449, ThreadMonitor.cs 240). AppOlObjects.cs reduced from 525 (pre-F4 over-cap) to 472 via partial split, per spec constraint. |
| Dependencies | PASS | Production adds no new dependency (`System.TimeProvider` already referenced). Test project adds `Microsoft.Extensions.TimeProvider.Testing` 9.0.0, an already-approved in-repo test-only package mirroring `QuickFiler.Test`. |
| I/O boundary isolation | PASS | Attribution/decision/format logic is COM-free and testable without Outlook; COM stays behind `IStoreDisableService` and the `Dispatcher` seam. |

## 3. C# Code Change Policy

| Rule | Verdict | Evidence |
|---|---|---|
| Csharpier formatting | PASS | qa-01 EXIT 0. |
| .NET analyzers / EnforceCodeStyleInBuild | PASS | qa-02 EXIT 0, 0 new warnings. |
| Nullable + TreatWarningsAsErrors | PASS | qa-03 EXIT 0. |
| net48 value-type constraint (no init/record-struct) | PASS | `LockupAttribution` is a plain `readonly struct` with an ordinary constructor and get-only properties (LockupStallDecider.cs:18–40); no `init`/`record`/`record struct` (avoids CS0518). |
| Strong contracts / null-safety | PASS | Interface/delegate seams throughout; explicit types at public boundaries. |
| Cohesive files / internal surface | PASS | `MyBoxModeless` kept a sibling `internal static` helper rather than bloating `MyBox`. |

## 4. General + C# Unit Test Policy

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest + Moq + FluentAssertions | PASS | All F4 test files use `[TestClass]`/`[TestMethod]`, `Mock<T>`, and FluentAssertions. |
| Determinism (injected clock, no real timers) | PASS | `FakeTimeProvider` drives `ThreadMonitorTests`; pass-through `IUiDispatcher` mock; no wall-clock reads. |
| Banned APIs in tests (Thread.Sleep/Task.Delay/DateTime.Now/temp files) | PASS | Banned-API scan of all seven F4 test files returned zero matches. |
| Isolation of process-global static | PASS | `CurrentStoreContextTests` and `ThreadMonitorTests` marked `[DoNotParallelize]`; each scope fully disposed back to null. |
| Scenario completeness (positive/negative/edge/order) | PASS | Order, no-context, already-disabled, BeginInvoke-not-Invoke, single-WARN, three-button routing, threshold boundary (>=), episode reset all covered. |
| Test file location (mirrors source, not colocated) | PASS | Tests under `UtilitiesCS.Test/**` and `TaskMaster.Test/**`, mirroring source trees. |

## 5. Determinism Infrastructure

- Clock: injected `System.TimeProvider` (production `TimeProvider.System`; tests `FakeTimeProvider`).
  Elapsed time computed via `GetUtcNow()`, not direct wall-clock reads on the attribution path.
- No banned timing APIs on the attribution path. Retained `Thread.Sleep` occurrences
  (ThreadMonitor.cs:148, 207) are pre-existing diagnostic-only code inside
  `[ExcludeFromCodeCoverage]` host-bound methods (`PingAndAwaitDiagnosticWindow`, `GetStackTrace`),
  gated behind the small `delayThreshold` and off the auto-disable/notify path. Confirmed
  pre-existing by comparing `e0b58302~1` (three `Thread.Sleep` sites including the removed polling
  loop) with `e0b58302` (two diagnostic sites remain; the polling-loop sleep was replaced by a
  clock-driven `ITimer`). F4 reduced, not added, `Thread.Sleep` usage.

## 6. Evidence Location Compliance

Scanned the branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`: none found. All F4 evidence is written to the
canonical `docs/features/active/2026-07-07-store-lockup-detect-notify-264/evidence/<kind>/` path
(`baseline/`, `qa-gates/`, `issue-updates/`, `other/`). No `EVIDENCE_LOCATION_OVERRIDE_REJECTED`
events. The repo's `validate_evidence_locations.py` script was not present in this worktree; the
manual diff scan is the substitute and returned zero violations.

Verdict: PASS.

## 7. Policy Findings Summary

| Row label | Verdict | Basis |
|---|---|---|
| C# toolchain (format/analyze/nullable/test) | PASS | qa-01..04, qa-07 all EXIT 0 |
| C# coverage new-code | PASS | 97.7% aggregate; every file >= 90% |
| C# coverage repo-wide testable denominator | PASS | UtilitiesCS 90.50% >= 80%; no regression |
| General code change policy | PASS | Section 2 |
| C# code change policy (incl. net48 value types) | PASS | Section 3 |
| Unit test policy (determinism, banned APIs, location) | PASS | Section 4, 5 |
| Evidence location compliance | PASS | Section 6 |
| File size <= 500 lines | PASS | All changed files <= 472 lines |

blocking_count (policy-audit): 0
