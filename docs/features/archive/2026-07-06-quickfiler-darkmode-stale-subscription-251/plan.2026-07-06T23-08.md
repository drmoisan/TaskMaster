# quickfiler-darkmode-stale-subscription - Minor-Audit Plan

- **Issue:** #251
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/251
- **Requirements Source:** `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md`
- **Plan Path:** `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/plan.2026-07-06T23-08.md`
- **Feature Folder:** `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251`
- **Work Mode:** minor-audit
- **Language:** C#
- **Last Updated:** 2026-07-07T00-15
- **Status:** Execution complete — AC1-AC7 verified and checked off; AC8/P2-T7 deferred pending PR creation

## Requirements Boundary

This minor-audit plan uses only `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md` as the requirements source. Acceptance criteria are limited to the checkbox items (AC1-AC8) under that file's explicit `## Acceptance Criteria` section (confirmed present). `spec.md` and `user-story.md` are not required and are confirmed absent from the feature folder; their absence is not a blocker in minor-audit mode.

Implementation is constrained to a lifecycle unsubscribe-and-guard fix for the stale `PropertyChanged` subscription bug in `QfcCollectionController`. Expected touched files are limited to:

- `QuickFiler/Controllers/QfcCollectionController.cs` (sole production file — AC6)
- `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` (new regression test file)
- `QuickFiler.Test/QuickFiler.Test.csproj` (explicit `<Compile Include>` wiring for the new test file — this project uses `packages.config` with explicit, non-glob `<Compile Include>` items)
- `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md` (AC checkbox status updates only)

All evidence must be written under `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/<kind>/`.

## Confirmed Facts (from source inspection, recorded for Phase 0 investigation task)

- `IOlObjects` (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs`) extends `INotifyPropertyChanged` and declares `bool DarkMode { get; set; }`. It is directly mockable with `Mock<IOlObjects>`, and `PropertyChanged` can be raised in tests via `mockOl.Raise(o => o.PropertyChanged += null, mockOl.Object, new PropertyChangedEventArgs("DarkMode"))`.
- `IApplicationGlobals.Ol` (`UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs`) is `IOlObjects Ol { get; }` — a get-only property, mockable via `mockGlobals.SetupGet(g => g.Ol).Returns(mockOl.Object)`.
- The `QfcCollectionController` constructor (`QuickFiler/Controllers/QfcCollectionController.cs:29-52`) dereferences, in order: `viewerInstance.L1v0L2L3v_TableLayout`, `viewerInstance.L1v0L2_PanelMain`, `AppGlobals.Ol.DarkMode` (via `SetupLightDark`), and `homeController.KeyboardHandler`. `IQfcFormViewer.L1v0L2L3v_TableLayout` and `.L1v0L2_PanelMain` (`QuickFiler/Interfaces/IQfcFormViewer.cs`) return `TableLayoutPanel`/`Panel`; a loose `Mock<IQfcFormViewer>` returns `null` for these by default, and the constructor only assigns them to fields without further dereference, so no real WinForms control construction is required.
- `TlpCellStates` (`QuickFiler/Helper Classes/TlpCellSnapShot.cs`) is a concrete `Dictionary<string, TlpCellSnapShotList>` subclass with no virtual seam; it is a pure data container and should be constructed directly as `new TlpCellStates()` rather than mocked.
- `_itemGroups` (private `List<QfcItemGroup>`) is `null` immediately after construction (only assigned in a separate initialization path, not the constructor), so `RemoveControls()`/`RemoveControlsAsync()` (called from `Cleanup()`/`CleanupAsync()`) take their `if (_itemGroups is not null)` early-exit and require no `TableLayoutPanel` interaction during the regression test.
- `QfcFormController.SetupDisposal.cs:208-213` already implements the correct pattern for a sibling class: `if (_globals?.Ol is not null) { _globals.Ol.PropertyChanged -= DarkMode_CheckedChanged; }` before nulling `_globals`. This is the reference pattern for the `QfcCollectionController` fix.
- The whole `QfcCollectionController` class carries `[ExcludeFromCodeCoverage]` (`QuickFiler/Controllers/QfcCollectionController.cs:20`), matching the CLAUDE.md COM/VSTO/WinForms coverage exemption for Outlook Interop event handler classes. Changed lines inside this class are therefore already excluded from the coverage denominator; the coverage gate for this change is repository-wide no-regression, not a new >=90% requirement on the changed lines themselves.
- No other production caller references `QfcCollectionController.DarkMode_CheckedChanged`. `QfcFormController.EventHandlers.cs:22` declares an unrelated, separately-scoped `DarkMode_CheckedChanged` method on a different class (`QfcFormController`) that already unsubscribes correctly and is out of scope for this fix.

---

### Phase 0 — Policy and Baseline Evidence

- [x] [P0-T1] Record policy-read evidence for issue #251 before implementation begins.
  - Files read (in order): `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: Evidence file exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read above, in order.

- [x] [P0-T2] Verify the minor-audit requirements boundary for issue #251.
  - Files: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md` (and confirm absence of `spec.md`, `user-story.md` in the same folder)
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/minor-audit-scope.2026-07-06T23-08.md`
  - Acceptance: Evidence confirms `issue.md` contains `- Work Mode: minor-audit`, contains an explicit `## Acceptance Criteria` section listing AC1-AC8, treats only that section as the AC source, and confirms `spec.md` and `user-story.md` are absent from the feature folder.

- [x] [P0-T3] Record investigation evidence confirming the constructor/interface facts needed to design the regression test, per the issue's investigation directive.
  - Files: `QuickFiler/Controllers/QfcCollectionController.cs`, `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs`, `UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs`, `QuickFiler/Interfaces/IQfcFormViewer.cs`, `QuickFiler/Interfaces/IFilerHomeController.cs`, `QuickFiler/Helper Classes/TlpCellSnapShot.cs`, `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/investigation-notes.2026-07-06T23-08.md`
  - Acceptance: Evidence records, with file:line citations: (a) `IOlObjects` extends `INotifyPropertyChanged` and its `DarkMode` property shape; (b) the full list of 8 constructor parameters and which are mockable interfaces vs. concrete/enum/struct types constructed directly; (c) confirmation that a loose `Mock<IQfcFormViewer>` avoids real WinForms control construction; (d) confirmation that no other production caller depends on `QfcCollectionController.DarkMode_CheckedChanged`; (e) the `QfcFormController.SetupDisposal.cs` reference unsubscribe pattern to mirror.

- [x] [P0-T4] Run the baseline C# formatting command.
  - Files: `QuickFiler/Controllers/QfcCollectionController.cs`
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/csharpier-baseline.2026-07-06T23-08.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:` stating whether any files were changed.

- [x] [P0-T5] Run the baseline C# analyzer build command.
  - Files: `TaskMaster.sln`, `QuickFiler/Controllers/QfcCollectionController.cs`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/csharp-analyzers-baseline.2026-07-06T23-08.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count or primary diagnostic.

- [x] [P0-T6] Run the baseline C# nullable build command.
  - Files: `TaskMaster.sln`, `QuickFiler/Controllers/QfcCollectionController.cs`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/csharp-nullable-baseline.2026-07-06T23-08.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count or primary diagnostic.

- [x] [P0-T7] Run the baseline MSTest coverage command for the full `QuickFiler.Test` suite.
  - Files: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-06T23-08.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with total tests, pass/fail counts, and the numeric baseline coverage headline percentage.

---

### Phase 1 — Constrained Implementation (Red → Green)

- [x] [P1-T1] Delegate constrained C# implementation to the small-path implementation engineer for issue #251.
  - Files: `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: The implementation handoff references issue #251, the feature folder, the requirements source, the `csharp.md` policy rule, and the constraint that production changes are limited to `QuickFiler/Controllers/QfcCollectionController.cs` only.

- [x] [P1-T2] [expect-fail] Add regression test `Cleanup_ThenDarkModePropertyChanged_DoesNotThrow` to `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`, run it against the pre-fix code, and confirm it fails.
  - Precondition: Phase 0 complete.
  - Test scenario: Construct `QfcCollectionController` via its real constructor with `Mock<IApplicationGlobals>` (whose `Ol` returns a `Mock<IOlObjects>` exposing a real raisable `PropertyChanged` event and `DarkMode` returning `true`), `Mock<IQfcFormViewer>`, `QfEnums.InitTypeEnum.Sort`, `Mock<IFilerHomeController>` (whose `KeyboardHandler` returns a mocked `IQfcKeyboardHandler`), `Mock<IFilerFormController>`, a real `CancellationTokenSource`/`CancellationToken`, and a real `new TlpCellStates()`. Call `Cleanup()`, then raise `PropertyChanged` on the mock `IOlObjects` with `PropertyChangedEventArgs("DarkMode")`, and assert no exception is thrown.
  - Acceptance: Test added with `[TestMethod]`, `[TestClass]` fixture, MSTest + Moq + FluentAssertions per `csharp.md`. Evidence artifact `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/regression-testing/fail-before-quickfiler-darkmode-stale-subscription.2026-07-06T23-08.md` records `Timestamp:`, `Command:` (the targeted `vstest.console.exe` filter run), `EXIT_CODE:` (non-zero / failing), and `Output Summary:` confirming the pre-fix `NullReferenceException` is reproduced. Satisfies AC1 (fail-before half).

- [x] [P1-T3] [expect-fail] Add sibling regression test `CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow` to `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` (identical arrangement, calling `await CleanupAsync()` instead of `Cleanup()`), run it against the pre-fix code, and confirm it fails.
  - Precondition: P1-T2 complete.
  - Acceptance: Test added with `[TestMethod]` and marked `async Task`. The same fail-before evidence artifact from P1-T2 additionally records this test's pre-fix failure (or a second dated entry), confirming reproduction of the NRE via the async cleanup path.

- [x] [P1-T4] Wire `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`.
  - Files: `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: An explicit `<Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />` item is added to the existing `<ItemGroup>` alongside the other `Controllers\*.cs` entries; the project loads and builds with the new file included.

- [x] [P1-T5] Fix `Cleanup()` in `QuickFiler/Controllers/QfcCollectionController.cs` to unsubscribe `DarkMode_CheckedChanged` before nulling `_globals`.
  - Precondition: P1-T2 through P1-T4 complete and confirmed failing.
  - Fix: Immediately before the existing `_globals = null;` assignment in `Cleanup()` (currently around line 2162-2170), add a null-conditional unsubscribe mirroring `QfcFormController.SetupDisposal.cs:210-213`: `if (_globals?.Ol is not null) { _globals.Ol.PropertyChanged -= DarkMode_CheckedChanged; }`.
  - Acceptance: `Cleanup()` unsubscribes `DarkMode_CheckedChanged` from `_globals.Ol.PropertyChanged`, guarded with null-conditional, before any field is nulled. No other statements in `Cleanup()` are modified. Satisfies AC2.

- [x] [P1-T6] Fix `CleanupAsync()` in `QuickFiler/Controllers/QfcCollectionController.cs` to unsubscribe `DarkMode_CheckedChanged` before nulling `_globals`.
  - Precondition: P1-T5 complete.
  - Fix: Apply the identical null-conditional unsubscribe statement immediately before the existing `_globals = null;` assignment in `CleanupAsync()` (currently around line 2152-2160).
  - Acceptance: `CleanupAsync()` unsubscribes `DarkMode_CheckedChanged` from `_globals.Ol.PropertyChanged`, guarded with null-conditional, before any field is nulled. No other statements in `CleanupAsync()` are modified. Satisfies AC3.

- [x] [P1-T7] Add a defensive cleaned-up guard and sender-sourced dark-mode read to `DarkMode_CheckedChanged` in `QuickFiler/Controllers/QfcCollectionController.cs`.
  - Precondition: P1-T6 complete.
  - Fix: At the top of `DarkMode_CheckedChanged(object sender, EventArgs e)` (currently lines 2118-2130), add an early return when the controller has been cleaned up (e.g., `if (_formViewer is null) { return; }`), and change the dark-mode read to prefer `sender as IOlObjects` over `_globals.Ol`, performing no theme change when `sender` is not `IOlObjects` and `_globals` is unavailable.
  - Acceptance: `DarkMode_CheckedChanged` returns without throwing and without calling `SetDarkMode`/`SetLightMode` when invoked on a cleaned-up controller; the method reads dark-mode state from `sender` cast to `IOlObjects` when available. Satisfies AC4.

- [x] [P1-T8] Record implementation-scope evidence confirming only `QuickFiler/Controllers/QfcCollectionController.cs` was changed among production files.
  - Files: `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/regression-testing/implementation-scope.2026-07-06T23-08.md`
  - Acceptance: Evidence lists every changed file (via `git diff --stat`) and confirms the only production file changed is `QuickFiler/Controllers/QfcCollectionController.cs`, satisfying AC6.

- [x] [P1-T9] Run the targeted issue #251 regression tests with coverage and confirm both pass post-fix, with no `SetDarkMode`/`SetLightMode` invocation observed.
  - Files: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`, `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerDarkModeTests"`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/regression-testing/targeted-vstest-coverage.2026-07-06T23-08.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming both `Cleanup_ThenDarkModePropertyChanged_DoesNotThrow` and `CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow` pass, and that the test asserts (via a mocked `IQfcItemController` injected into `_itemGroups`, verified with `Mock.Verify`) that `SetThemeDark`/`SetThemeLight` are never invoked after cleanup. Satisfies AC1 (pass-after half) and AC5.

---

### Phase 2 — Final C# QA Loop

- [x] [P2-T1] Run the final C# formatting command.
  - Files: `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/csharpier-final.2026-07-06T23-08.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:`; if this command changes files, restart Phase 2 from P2-T1 after preserving the evidence.

- [x] [P2-T2] Run the final C# analyzer build command.
  - Files: `TaskMaster.sln`, `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/csharp-analyzers-final.2026-07-06T23-08.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:`; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T3] Run the final C# nullable build command.
  - Files: `TaskMaster.sln`, `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/csharp-nullable-final.2026-07-06T23-08.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:`; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T4] Run the final full-suite MSTest coverage command.
  - Files: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T23-08.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with total tests, pass/fail counts, and the numeric post-change coverage headline percentage; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T5] Record C# coverage comparison evidence for issue #251.
  - Files: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-06T23-08.md`, `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/regression-testing/targeted-vstest-coverage.2026-07-06T23-08.md`, `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T23-08.md`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/csharp-coverage-comparison.2026-07-06T23-08.md`
  - Acceptance: Evidence records baseline coverage, targeted-test coverage, post-change coverage, and confirms no repository-wide regression; explicitly notes that `QfcCollectionController` carries `[ExcludeFromCodeCoverage]` so the changed lines are outside the coverage denominator, and that this is a pre-existing, documented exemption rather than a new suppression introduced by this change. Satisfies AC7 (coverage portion).

- [x] [P2-T6] Update issue #251 acceptance-criteria status after verified completion.
  - Files: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/issue-updates/ac-status.2026-07-06T23-08.md`
  - Acceptance: Only verified acceptance criteria (AC1-AC7) under `## Acceptance Criteria` in `issue.md` are changed from `[ ]` to `[x]`; AC8 remains unchecked pending PR CI. Unchanged text is preserved. Evidence records total AC items, checked items, remaining items, and the verification evidence used for each checked item, per `acceptance-criteria-tracking`.

- [x] [P2-T7] Verify required CI checks pass green on the PR head SHA once the PR is opened for issue #251. (Deferred — no PR exists at plan-execution time; see evidence for explicit deferral reason. Must be re-run to completion once a PR exists, before AC8 is checked off.)
  - Files: PR created from this branch against `main`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/ci-check-verification.<pr-timestamp>.md`
  - Acceptance: Evidence records the PR URL, head SHA, the required check names, and their pass/fail status (`gh pr checks <PR>` or equivalent), confirming all required checks are green. This task is explicitly deferred until a PR exists; if no PR has been opened yet at plan-execution time, record that explicit deferral reason in the evidence artifact rather than a numeric `EXIT_CODE` (the only authorized non-command completion path in this plan). Once a PR exists, this task must be re-run to completion before AC8 is checked off. Satisfies AC8.

- [x] [P2-T8] Record final minor-audit readiness evidence for issue #251.
  - Files: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/plan.2026-07-06T23-08.md`, `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md`, `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/phase0-instructions-read.md`, `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/regression-testing/implementation-scope.2026-07-06T23-08.md`, `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/csharp-coverage-comparison.2026-07-06T23-08.md`
  - Evidence: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/minor-audit-readiness.2026-07-06T23-08.md`
  - Acceptance: Evidence confirms Phase 0 artifacts exist, Phase 1 scope and regression-test evidence exist, Phase 2 C# QA artifacts exist, every command-bearing task has an executed numeric `EXIT_CODE`, AC1-AC7 are checked off in `issue.md`, and AC8/P2-T7 disposition (green or explicitly deferred pending PR) is recorded.
