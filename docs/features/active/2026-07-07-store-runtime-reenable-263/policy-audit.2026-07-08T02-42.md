# Policy Compliance Audit — F3 store-runtime-reenable (#263)

- Feature: F3 store-runtime-reenable (issue #263, epic #260)
- Branch: feature/store-runtime-reenable-263
- Review commit: ee46eb5d (HEAD)
- Base (diff): 1724f8d0 (epic/store-lockup-resilience-integration tip)
- Diff command: `git diff 1724f8d0..HEAD`
- Work Mode: full-feature (AC source: spec.md AC1–AC11 + user-story.md)
- Timestamp: 2026-07-08T02-42

## Scope and Baseline

The audited scope is the full branch diff `1724f8d0..HEAD` (45 files: 24 production/test C#,
3 `.csproj`, 18 evidence/docs). This is the F3 change set commit ee46eb5d against its parent.
No caller narrowing was applied; the full diff is covered.

### Rejected Scope Narrowing

None. The caller prompt supplied the correct full-branch diff base (1724f8d0) and did not attempt
to narrow scope to a plan/task/phase subset or to mark any language as out of scope. No narrowing
to reject.

## Executive Summary

The F3 change set complies with the applicable CLAUDE.md and `.claude/rules` policies. All four
toolchain stages relevant to C# (CSharpier format, .NET analyzers, nullable/TreatWarningsAsErrors,
MSTest) passed per the committed QA-gate evidence, corroborated by direct inspection of the diff.
Coverage meets the repository's C# floor. No Blocking findings were identified. Two non-blocking
code-quality observations are recorded (documentation defect; residual decision logic inside
coverage-excluded COM members).

Overall policy verdict: PASS.

## 1. Toolchain Compliance (CLAUDE.md C#, general-code-change.md)

| Stage | Verdict | Evidence |
|---|---|---|
| Formatting (CSharpier) | PASS | `evidence/qa-gates/qa-01-format.md`: `csharpier check .` clean, 1294 files, exit 0. |
| Linting / .NET analyzers | PASS | `evidence/qa-gates/qa-02-analyzers.md`: 0 errors; 73 warnings vs 72 baseline; the +1 is a pre-existing CS0618 in unmodified `ProcessMailItemAsync`, none originate in F3 files. |
| Type / nullable (TWAE) | PASS | `evidence/qa-gates/qa-03-nullable.md`: 0 warnings, 0 errors under `Nullable=enable /TreatWarningsAsErrors=true`. New nullable-annotated files carry explicit `#nullable enable`. |
| Unit tests (MSTest) | PASS | `evidence/regression-testing/startup-regression.md`: 4430/4430 green (non-instrumented run). |

### 1.1 Test framework and library policy (CUT1/CUT2)

PASS. All new/modified tests use MSTest (`[TestClass]`/`[TestMethod]`), Moq for mocking, and
FluentAssertions for assertions. No xUnit/NUnit leakage, no `[Fact]`/`[Theory]` (verified by diff
grep). Files: `StoreRehookCoordinatorTests.cs`, `AppEventsStoreRehookTests.cs`,
`OutlookFolderNotificationSinkTests.cs`, `StoresWrapperRehookTests.cs`, `OutlookReadinessGateTests.cs`,
`StoresWrapperTests.cs`.

### 1.2 Unit-test determinism / external-dependency policy (UT4, general-unit-test.md)

PASS. Diff-wide grep for `Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Path.GetTempFile`,
`GetTempPath`, `StreamWriter`, `Directory.CreateDirectory` found no matches in added lines. The
coordinator's inter-attempt delay is injected and supplied as `_ => Task.CompletedTask` in tests, so
the bounded retry resolves without real time passing. COM boundaries are proxied with
`Mock<Outlook.Store>`, `Mock<Outlook.Items>`, `Mock<Outlook.NameSpace>` (Strict where behavior is
asserted), consistent with the established repo pattern. No live Outlook, no temp files, no real
timers.

### 1.2.1 Coverage (C# / .NET)

- C#/.NET coverage verdict: PASS.
- Baseline: 61.94% overall 2-assembly line-rate (P0-T13, `evidence/baseline/test-coverage-baseline.md`).
- Post-change: 62.12% overall 2-assembly line-rate; first-party production testable-denominator
  83.23% (87889 / 105600) across all 7 `*.Test.dll` (ci.yml style).
- Change: +0.18 pp overall; no previously-covered production line lost coverage.
- New/changed-code coverage: 99.6% (approx. 252/253 lines of F3 decision logic;
  StoreRehookCoordinator 99.2%, StoreRehookResult 100%, StoresWrapper.AddOrRestoreStore 100%,
  OutlookReadinessGate.IsReady(Store) 100%, AppEvents SubscribeInboxForStore/IsInboxHooked 100%,
  sink AddStoreSubscriptions/RemoveStore/IsStoreHooked 100%).
- Disposition: PASS. Repository first-party production testable-denominator line coverage 83.23%
  clears the CLAUDE.md C# floor of 80% on the testable denominator (production-only first-party code
  after the ratified COM/VSTO/WinForms exemption; General Unit Test Policy §UT2). New-code coverage
  99.6% clears the 90% new-code obligation. No regression on changed lines.
- Evidence: `evidence/qa-gates/qa-04-test-coverage.md`, `evidence/qa-gates/qa-05-coverage-delta.md`.

Policy-precedence note: CLAUDE.md (authority #1 under policy-compliance-order) defines the C# floor
as 80% on the testable denominator with the ratified COM/VSTO/WinForms `[ExcludeFromCodeCoverage]`
exemption (tracked in `feature/csharp-coverage-uplift`). The 83.23% figure is the raw first-party
production denominator BEFORE applying those exemptions and already clears 80%; applying the
exemptions would only raise it. This is the governing threshold for this C#/VSTO codebase.

The coverage artifact `artifacts/csharp/coverage.xml` was not (re)generated for this review;
verification relied on the committed numeric Cobertura measurements recorded in the QA-gate evidence,
per the evidence-verification model.

## 2. Design Principles (general-code-change.md §1–2)

PASS. The design follows the spec's Approach B: one shared per-store primitive per subsystem, reused
by both the startup loop body and the runtime rehook path (simplicity, reusability, separation of
concerns). `StoreRehookCoordinator` depends only on injected narrow delegates/interfaces (store
lookup, `AddOrRestoreStore` gateway, inbox-subscribe seam, `IOutlookFolderNotificationSink`,
`IOutlookFolderTreeService`, `IOutlookReadinessGate`), keeping pure decision logic separable from COM
I/O. `StoreRehookResult`/`StoreRehookOutcome` model the outcome as a strongly-typed record + enum.

## 3. Error Handling and Logging (general-code-change.md §3, C#4)

PASS. `RehookStoreCoreAsync` wraps its COM-crossing body in a single boundary catch that maps any
exception to `PermanentError` and never lets it escape (AC7). Transient HRESULTs are classified via
the shared `OutlookReadinessGate.IsTransientError`/public HRESULT constants rather than duplicated
literals. Logging uses the repository log4net pattern (`LogManager.GetLogger`) at appropriate levels
(Debug for success/already-hooked, Warn for store-not-found, Error for transient-timeout and
permanent-error with HRESULT via `DescribeHResult`). No ad-hoc console output introduced.

## 4. Module and File Structure (general-code-change.md §4, C#5)

PASS. Every new and modified production file is <= 500 lines (`evidence/other/file-size-check.md`,
re-verified by line count): largest is `OutlookFolderNotificationSink.cs` at 498. New per-store
primitives were placed in partial-class files (`AppEvents.StoreRehook.cs`,
`AppOlObjects.StoreRehook.cs`, `ApplicationGlobals.StoreRehook.cs`) to keep the existing files within
the ceiling, mirroring the established `AppEvents.ReadinessHookup.cs` / `AppOlObjects.StoreLoading.cs`
split precedent. Public surface expansion is intentional and minimal (see §6).

## 5. Public API / Compatibility (general-code-change.md, C#3)

PASS. Two members were widened `internal -> public` for cross-assembly access from the
`TaskMaster`-assembly coordinator: `StoresWrapper.AddOrRestoreStore` and
`OutlookFolderNotificationSink.IsStoreHooked`. Confirmed `UtilitiesCS` grants no
`InternalsVisibleTo("TaskMaster")` (only doc comments note its absence; no assembly attribute), so
the widening is the minimal necessary surface. The COM-free test seam `AddStoreSubscriptions` remains
`internal` (correctly not widened). `IOutlookReadinessGate.IsReady(Store)` and
`IOutlookFolderNotificationSink.AddStore/RemoveStore` are additive interface members; the existing
parameterless `IsReady()` and `Start()/Dispose()` semantics are unchanged.

## 6. `[ExcludeFromCodeCoverage]` Compliance (UT2, quality-tiers COM/VSTO exemption)

PASS with a non-blocking observation. The exclusions added in this change set annotate COM-bound
composition-root / wrapper members whose testable decision logic has been extracted into
non-excluded, tested seams:

- `ApplicationGlobals.StoreRehook.cs`: `BuildStoreRehookCoordinator`, `ResolveLiveStore`,
  `SubscribeStoreInbox`, `FolderNotificationSink` getter — all read live `NamespaceMAPI.Stores` /
  call `store.GetDefaultFolder` / `store.DisplayName` directly with no seam below COM (composition
  root). The coordinator's decision logic they feed is tested at 99% via mocked seams.
- `AppOlObjects.StoreRehook.cs`: `ResolveInboxForStore`, `FolderNotificationSink` getter — the
  delegates invoked all cross the live COM boundary; the pure per-store attribution logic is tested
  separately via `EmitPerStoreInboxAttribution`, mirroring the pre-existing untested `LoadInboxes`
  body it was extracted from.
- `OutlookFolderNotificationSink.cs`: COM constructor, `Start`, `Dispose`, `AddStore(Outlook.Store)`,
  `AddAllStores`, `BuildStoreFolderSubscriptions` — the COM-free registration seam
  `AddStoreSubscriptions` and the pure predicates (`IsStoreHooked`, `RemoveStore`, `SubscriptionCount`)
  are NOT excluded and are fully tested.

No exclusion hides novel testable decision logic that lacks coverage elsewhere. The 80%
testable-denominator floor is met (83.23%) without relying on these exclusions, so they are not
load-bearing for the floor. See code-review CR-2 for the residual observation on the HRESULT-branch
inside the excluded `ResolveInboxForStore`.

## 7. Evidence Location Compliance

PASS. All F3 evidence artifacts are written under the canonical
`docs/features/active/2026-07-07-store-runtime-reenable-263/evidence/<kind>/` tree
(baseline, qa-gates, regression-testing, issue-updates, other). No files in the branch diff are
written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or `artifacts/evidence/`.
No non-canonical evidence path detected; no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` required.

## Verdict

Policy compliance: PASS. Blocking findings: 0.

## Appendix A — Commands and Sources

- `git diff 1724f8d0..HEAD` (full branch diff; primary evidence).
- `git diff 1724f8d0..HEAD --stat -- "*StoreDisableService.cs" "*IStoreRehookService.cs" "*IApplicationGlobals.cs" "*StoreIdentity.cs"` — empty (F1 files unchanged).
- File line counts via `awk 'END{print NR}'` over all changed production/test files.
- Banned-pattern grep over added diff lines (Thread.Sleep / Task.Delay / DateTime.Now / temp-file APIs) — none.
- QA-gate evidence: `evidence/qa-gates/qa-01..05`, `evidence/regression-testing/startup-regression.md`,
  `evidence/other/no-f1-compile-dependency.md`, `evidence/other/file-size-check.md`.
- PR context artifacts (`artifacts/pr_context.summary.txt` / `.appendix.txt`): not present in this
  worktree; scope derived directly from the caller-supplied diff base and the committed evidence,
  which are the authoritative legitimate scope sources.
