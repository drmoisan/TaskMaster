# [P6-T13] Closure summary and residual risks

Timestamp: 2026-09-08T03-16

## 1. The six acceptance criteria, their check-off state and their evidence

Work mode is `full-bug`, so `spec.md` is the sole acceptance-criteria source. All six are checked off.

| AC | State | Evidence |
|---|---|---|
| AC1 | `- [x]` | `evidence/regression-testing/p3-t6-pass-after.md` for the apartment-rejection tests; `evidence/qa-gates/p4-t3-quickfiler-tests.md` for the reconciled MTA caller; implementation in [P3-T1] and [P4-T1] |
| AC2 | `- [x]` | `evidence/regression-testing/p3-t6-pass-after.md` for the retry test; `evidence/other/p6-t4-ac2-regression-reconciliation.md` for the regression-scenario clause; implementation in [P3-T2] |
| AC3 | `- [x]` | `evidence/regression-testing/p3-t6-pass-after.md` for the seven awaiter cases; `evidence/qa-gates/p4-t3-quickfiler-tests.md` for the `EfcFormControllerTests` and `WinFormsPumpHostTests` rows; implementation in [P3-T4] |
| AC4 | `- [x]` | `evidence/qa-gates/p5-t5-tests-coverage.md` for the seventeen added tests in the discovered total; `evidence/qa-gates/p4-t4-utilitiescs-tests.md` for their outcomes; fake-dispatcher seam in [P1-T4], [P1-T6] and [P1-T7] |
| AC5 | `- [x]` | `evidence/other/p0-t15-mta-synccontextform-measurement.md`; `evidence/other/p6-t4-ac2-regression-reconciliation.md`; the three repetition artifacts `evidence/qa-gates/p5-t6-tryaddvalues-rep1.md`, `...-rep2.md`, `...-rep3.md` |
| AC6 | `- [x]` | `evidence/qa-gates/p5-t5-tests-coverage.md`; `evidence/qa-gates/p6-t1-uithread-file-coverage.md`; `evidence/qa-gates/p6-t2-changed-line-coverage.md`; `evidence/qa-gates/p6-t3-aggregate-coverage.md` |

AC1 through AC4 are additionally mirrored into the `## Acceptance Criteria` section of `issue.md` by [P6-T11]. `issue.md` carries no AC5 or AC6, so no line was added to it.

### Two facts the AC4 citation must record

- **No test added by this delivery requires a live Outlook process.** The three files this delivery creates or extends in `UtilitiesCS.Test` reference no `Microsoft.Office.Interop` type and carry no `TestCategory` attribute, so none is excluded by the `TestCategory!=LiveOutlook` filter and none needs a host. Verified by search over `UiThreadInitContract_Tests.cs`, `UiThread_Tests.cs` and `UiThreadStateScope.cs`: 0 matches for `Microsoft.Office.Interop` and 0 for `TestCategory`.
- **Every new type in `UtilitiesCS.Test` was verified non-`Form`-derived.** `UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` reports `Passed` in the [P5-T5] full-suite TRX, duration `00:00:00.0121231`. The new types are `FakeUiCaptureSource`, `ApartmentThreadRunner`, `SharedStaDispatcherHost`, the two contract test classes, the private nested `StaDispatcherHost` in `SynchronizationContextAwaiter_Tests`, and `UiThreadStateScope`.

### The AC6 collector substitution

AC6-COLLECTOR-SUBSTITUTION: coverage was collected by `dotnet-coverage collect ... -- vstest.console.exe ...` rather than by `vstest.console.exe ... /EnableCodeCoverage`, which is what the literal wording of AC6 names. `scripts/vscode/TaskMaster.cli.runsettings` carries no data collector, and `scripts/vscode/Invoke-MSTestWithCoverage.ps1:19-26` records that the omission is deliberate because the outer `dotnet-coverage` instrumentation and the built-in Code Coverage collector conflict. The substitution is stated rather than silently adopted.

## 2. Residual ordering risk at production await sites

Research R6 enumerated eleven production await sites at which the AC3 predicate change can alter execution ordering, and found **no existing test asserts ordering at any of them**. The suite therefore cannot detect an ordering regression at these sites. This is recorded as **residual**, not as covered.

The four highest-consequence sites, named individually:

- `QuickFiler/Controllers/EfcFormController.cs:877` — `ActionCancelAsync`; `Close()` then `Cleanup()` would run before already-queued UI work rather than after it.
- `QuickFiler/Controllers/QfcCollectionController.cs:782` — `RemoveControlsAsync`; a `TlpLayout` toggle and a row removal would run before queued layout work.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:67` — a `TaskScheduler.FromCurrentSynchronizationContext()` site; the resulting scheduler would target the persistent WinForms context instead of the dispatcher context.
- `QuickFiler/Controllers/EfcItemController.cs:201` — the second `TaskScheduler.FromCurrentSynchronizationContext()` site, same shape.

The predicate's `ambient is null` early return is the guard that keeps the two `TaskScheduler` sites from throwing `InvalidOperationException`, and `SynchronizationContextAwaiter_Tests.IsCompleted_WhenAmbientContextIsNullAndCapturedContextIsNotNull_ReturnsFalse` pins it.

## 3. Production sites that gain a possible new throw

Four sites, each unreachable in production after `TaskMaster/ThisAddIn.cs:35` runs on the Outlook STA:

- `TaskMaster/AppGlobals/AppOlObjects.cs:367` — reachable off the UI thread by construction, because the enclosing branch at `:364` is entered only when the caller is off it. The new throw is strictly better than today's behaviour there, which constructs a `SyncContextForm` on the worker and performs the COM read on the wrong apartment, the exact failure the comment at `:361-363` says it is preventing.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:179` — reachable wherever the predictor runs; no existing test hits it with a null backing field.
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapViewer.cs:40` and `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersViewer.cs:79` — only for a hypothetical off-UI-thread caller of `SetController`. Both in-repo drivers are `[STATestClass]` and both still pass.
- `UtilitiesCS/Threading/ThreadMonitor.cs:143` — **recorded for completeness rather than as a regression risk.** It reads `UiThread.UiSyncContext` inside `PingAndAwaitDiagnosticWindow()`, declared at `UtilitiesCS/Threading/ThreadMonitor.cs:138` and carrying `[ExcludeFromCodeCoverage]` at `:137`. No test in this repository reaches that member; its only in-repository call site is `UtilitiesCS/Threading/ThreadMonitor.cs:109`. Furthermore `ThreadMonitor` is constructed only inside `Initialize()`, after `_uiSyncContext` has been assigned, so the field is never null on that path and the getter never calls `Init()`.

## 4. Manual live-host verification, reported separately

Not an acceptance criterion and not performed by this delivery: QuickFiler launch, item load, and breadcrumb open on a live Outlook host, confirming no change in observable UI behaviour and no new keyboard-focus regressions after #677 and #796. This is the residual that the automated suite cannot close, per section 2.

## 5. Follow-up candidates, not in this delivery

- Routing `QfcHomeController` through `IUiDispatcher`. The seam exists at `UtilitiesCS/Threading/IUiDispatcher.cs` and `UtilitiesCS/Threading/WpfUiDispatcher.cs` and `QfcItemController` already consumes it, but `QuickFiler/Controllers/QfcHomeController.cs:360` is not routed through it. Research R5 records this as larger than the adopted option for the same benefit.
- Reconciling the 80% versus 85% coverage-floor divergence between `CLAUDE.md` and `.claude/rules/general-unit-test.md`. `CLAUDE.md` governs under the precedence order in `.claude/skills/policy-compliance-order/SKILL.md`, and the divergence was recorded rather than resolved.
- Closing #784, #787 and #788 with a pointer to #809.
- Correcting the GitHub issue body for #809, which still carries the superseded bare-owning-thread-identity sentence that `issue.md:59` has already corrected locally. That sentence is unsafe: a continuation resumed after `ConfigureAwait(false)` can land on a recycled thread-pool thread whose managed id equals the owner's, and `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:263-272` records the opposite rule.
- The pre-existing 500-line overrun in `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`, which `evidence/qa-gates/p4-t5-line-counts.md` records under `PRE_EXISTING_FILES_OVER_500:` at a baseline of 1066 lines. This delivery increases it by exactly the one `[DoNotParallelize]` attribute line [P2-T7] adds, to 1067.
- The two-line coverage residual at `UtilitiesCS/Threading/UiThread.cs:177-178`, the body of the `ReferenceEquals(_context, _uiSyncContext)` clause of the new predicate, which no case in this delivery reaches. Closing it needs one further awaiter test that installs `_uiSyncContext` and awaits that same instance from the owning thread while a different context is ambient. `evidence/qa-gates/p6-t1-uithread-file-coverage.md` and `evidence/qa-gates/p6-t2-changed-line-coverage.md` both record it; the `IsCompleted` member still meets the 90% floor exactly at 90.00%.
- The three-line coverage residual at `UtilitiesCS/Threading/UiThread.cs:38-40`, the body of the `onLockupDetected` guard. No test in this delivery passes a non-null `onLockupDetected` on a path that reaches the assignment, because the one test that supplies a callback supplies it to assert that a rejected `Init()` does not perform the assignment.

## 6. Environmental finding recorded for future planners

Research R4 concluded that a plain `[TestMethod]` runs MTA in this repository, and `UtilitiesCS.Test/test.runsettings` does record that global STA execution is intentionally disabled. **That premise does not hold for every scheduling arrangement.** Plain `[TestMethod]` cases were measured running on an **STA** thread; `evidence/regression-testing/p2-t10-fail-before.md` records the measurement verbatim and the correction it forced.

**The operational rule below is the load-bearing part of this section and it is confirmed. A test that needs a caller of a known apartment must create a dedicated thread and set the apartment explicitly, rather than relying on the ambient worker.**

**Correction (2026-09-08, orchestrator, after feature review).** An earlier revision of this section attributed the STA observation to a `[TestClass] [DoNotParallelize]` class sharing the serial execution bucket with an `[STATestClass] [DoNotParallelize]` class. **That stated cause is not established and should not be relied on.** A simpler explanation covers the same observation and two further tree-verified facts:

- `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18` carries the only assembly-level `[assembly: Parallelize(...)]` in this repository. No other test assembly has one, so an assembly invoked without a `/Settings:` runsettings does not parallelize at all.
- No `.runsettings` anywhere in the repository sets `ExecutionThreadApartmentState`.

Tests dispatched to the MSTest parallel worker pool run on thread-pool threads and are MTA. Tests that run on the main test-execution thread — the `[DoNotParallelize]` serial bucket, or every test in an assembly where parallelization is off — inherit that thread's apartment, and the vstest execution thread on .NET Framework is STA unless `ExecutionThreadApartmentState` overrides it. Bucket-sharing with an `[STATestClass]` is not required for the effect.

A consequence for this delivery is recorded in the correction sections of `p0-t15-mta-synccontextform-measurement.md` and `p6-t4-ac2-regression-reconciliation.md`: the `[P0-T15]` probe most likely ran STA, so it took no MTA measurement, and the status of the #782 mechanism narrative reverts to UNKNOWN. Acceptance criterion AC5 is unchecked in `spec.md` for that reason.
