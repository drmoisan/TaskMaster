# quickfiler-keyboard-actions-coverage — Plan

- **Issue:** #430
- **Parent epic issue:** [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- **Epic:** `quickfiler-per-file-coverage` (child F3, wave 1)
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Branch:** `feature/quickfiler-keyboard-actions-coverage`
- **Depends on:** F1 `quickfiler-coverage-denominator-and-exemption-ledger` (wave 0, merged before this plan executes)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T20-41
- **Status:** Revised for preflight (R-1 through R-6 applied)
- **Version:** 1.1
- **Work Mode:** full-feature (AC sources: `spec.md` **and** `user-story.md`, 14 criteria each, 1:1 by number)

## Required References

- Requirements (authoritative): `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/spec.md` and `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/user-story.md`
- Context only: `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md`. Where `issue.md` conflicts with `spec.md`, `spec.md` wins (see its Correction Log).
- Research (11 artifacts): `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/research/01-KeyboardHandler.md` through `research/11-IItemControler.md`
- Epic contract: `docs/features/epics/quickfiler-per-file-coverage/epic.md`
- Policy, in the order required by `policy-compliance-order`: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`

**All work must comply with those policies; this plan does not duplicate their content.**

## Decisions Record (binding on the executor)

- **D-A — R2 disposition: OPTION A, a separate `QuickFiler/Interfaces/MyBoxDialogPrompt.cs` file.** Rationale: the single forwarding statement is uncoverable either way, and Option B (an in-file `private static readonly Func<...>` default inside `KeyboardHandler.cs`) would place that uncoverable line inside the 456-line file that AC1 and AC2 actually gate at `>= 80%`. Isolating it in a one-statement adapter keeps `KeyboardHandler.cs`'s denominator clean and matches the in-cluster `IMailItemActions` / `MailItemActionsAdapter` precedent named in `research/01-KeyboardHandler.md` §5. The `>= 90%` new-code floor on `MyBoxDialogPrompt.cs` is discharged by a ledger-ratification addendum request raised to the epic orchestrator (P1-T2), not self-granted. If that request is declined, the recorded fallback is Option B and it is executed as a follow-up, not mid-plan.
- **D-B — K5 is IN SCOPE.** The two `EnsureSyncContext` helpers are extracted. They collapse seven separately-uncovered branch pairs into two and remove 14 duplicated lines, which is what keeps the projected file size at ~456 lines.
- **D-C — No contingency split of `KeyboardHandler.cs`.** The split at line 262 is applied only if the measured post-refactor line count exceeds 500 (P1-T19 measures it).
- **D-D — `QuickFiler/QuickFiler.csproj` is edited exactly once in the base path**, to add `<Compile Include>` entries for the two F3-authored new production files, adjacent to the existing `Interfaces\` block at lines 358-368. **Authority: the amended AC9 in `spec.md` and `user-story.md`, which explicitly permits exactly two `<Compile Include>`-only `.csproj` edits — new test files in `QuickFiler.Test/QuickFiler.Test.csproj` and the two new F3-authored production files in `QuickFiler/QuickFiler.csproj`. This is therefore AUTHORIZED, not an open question.** Both edits must remain minimal hunks placed adjacent to the existing `<Compile Include>` block. This is mechanically required by the legacy non-SDK project (no globbing) and registers only F3-authored files. It is not a shared build property file and is not a sibling-owned file. If the P1-T19 contingency split triggers, one further `<Compile Include>`-only hunk is added for `KeyboardHandler.FolderRouting.cs`, adjacent to the existing `Controllers\` block. AC9 bounds this edit to two files, not two hunks.
- **D-E — `QuickFiler.Test/QuickFiler.Test.csproj` is edited three times in the base path (P1-T5, P1-T21, P3-T1), plus one further `<Compile Include>`-only hunk if the P6-T17 contingency split triggers**, with the base-path edits made as three contiguous grouped hunks, each appended adjacent to the existing `Controllers\Ka*/Kbd*` block at lines 92-96, to keep the merge hunk shared with F9/F10/F11 small and mechanically resolvable. The contingency hunk for `KaStringAsyncBoundaryTests.cs` is appended to that same block. AC9 bounds this edit to two `.csproj` files, not to a fixed hunk count.
- **D-F — Characterization, not fixes.** L1-L6 are out of scope and tracked as issues #444 and #445. Every characterization test pins **current** behavior and carries an XML comment naming it a characterization test and citing its issue number.
- **D-G — No STA test file anywhere in this child.** No `*.StaTests.cs` is created.
- **D-H — AC11 is a negative gate.** No `TimeProvider`, `FakeTimeProvider`, fake-timer facility, or injected clock is introduced in any production or test file.
- **D-I — Evidence locations.** Baseline evidence: `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/evidence/baseline/`. Final QC and per-file coverage verification: `.../evidence/qa-gates/`. Decisions, dispositions, and referrals: `.../evidence/other/`. No evidence artifact may be written outside the feature folder's `evidence/` tree; the forbidden alternatives are enumerated in `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` § Non-Overridable Authority and are not repeated here.
- **D-J — Command environment.** `csharpier`, `msbuild`, `vstest.console.exe`, and `dotnet-coverage` are not on `PATH` in this environment and are invoked via `pwsh` with explicit paths. `vstest.console.exe` is resolved with `vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. `/InIsolation` is mandatory for the Moq-based assemblies. `/TestCaseFilter` clauses are joined with `|`, never `OR`. `csharpier` is pinned at 1.2.6 by `dotnet-tools.json`, whose subcommands are `format | check | pipe-files | server`; `csharpier .` is v0 syntax and fails. When enumerating test assemblies recursively, filter out any candidate assembly whose path lies under a per-agent Git worktree directory (the `worktrees` subtree of the repository's `.claude` folder); stale agent-worktree builds otherwise produce spurious assembly-initializer failures.
- **D-K — Repo-wide coverage is record-and-report, not a blocking gate** (AC14). The blocking change-scoped gates are AC1 (per-file `>= 80%` for `testable` files), AC2, and the `>= 90%` new-code floor on the new files.

#### Canonical command forms referenced by tasks

- **FORMAT**: `dotnet tool run csharpier format .`
- **FORMAT-CHECK**: `dotnet tool run csharpier check .`
- **ANALYZE**: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- **TYPECHECK**: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- **SCOPED-TEST `<Filter>`**: `& (vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe) QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~<Filter>"`
- **FULL-COVERAGE `<out>`**: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <out>`
- **PERFILE `<in>` `<out>`**: F1's per-file coverage harness (path recorded by P0-T6), reading the Cobertura XML `<in>` and emitting the per-file table `<out>`.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Compliance, F1 Ledger Consumption, and Baseline Capture

- [ ] [P0-T1] Read the four policy documents in the required order — `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md` — and record the read.
  - Acceptance: `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/evidence/baseline/phase0-instructions-read.<TS>.md` exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of the four files read in that order.
- [ ] [P0-T2] Read `spec.md`, `user-story.md`, and `issue.md` from the feature folder and record the 14-criterion AC set plus the precedence rule that `spec.md` wins over `issue.md`.
  - Acceptance: `.../evidence/baseline/requirements-read.<TS>.md` lists AC1-AC14 from both `spec.md` and `user-story.md`, confirms they are 1:1 by number, and states `PRECEDENCE: spec.md over issue.md`.
- [ ] [P0-T3] Read all 11 research artifacts `research/01-KeyboardHandler.md` through `research/11-IItemControler.md` and reconcile the proposed-case counts against this plan.
  - Acceptance: `.../evidence/baseline/research-read.<TS>.md` records per-artifact case counts 73/0/8/13/13/14/15/0/0/8/0 and states the reconciled plan total of 144 test-authoring tasks across Phases 1-7.
- [ ] [P0-T4] Verify that F1's ledger `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` exists on the integration branch; halt and report BLOCKED if it is absent.
  - Acceptance: `.../evidence/baseline/f1-ledger-presence.<TS>.md` records `LEDGER_PRESENT: true` with the file path and its commit sha, or `LEDGER_PRESENT: false` together with a BLOCKED report and no further phase started.
- [ ] [P0-T5] Record F1's ledger classification, verbatim and with its line citation, for each of the 11 in-scope files.
  - Acceptance: `.../evidence/baseline/f1-ledger-classifications.<TS>.md` contains one row per file for `QuickFiler/Controllers/KeyboardHandler.cs`, `KbdActions.cs`, `KaChar.cs`, `KaKey.cs`, `KaStringAsync.cs`, `QfcFormKeyHandler.cs`, `QuickFiler/Interfaces/IKbdAction.cs`, `IQfcKeyboardHandler.cs`, `IMailItemActions.cs`, `MailItemActionsAdapter.cs`, `IItemControler.cs`, each with the ledger classification and ledger line number; and it records the AC13 escalation rule verbatim — if `KeyboardHandler.cs` is classified `ratified-exempt` in whole, or any of the four interface-only files is classified `testable` with an `>= 80%` target, escalate to the epic orchestrator rather than fabricating tests or self-granting an exemption.
- [ ] [P0-T6] Record F1's per-file coverage harness contract: its script path, its invocation form, whether it aggregates Cobertura entries by the `<class>` element's `filename` attribute or by class, whether it normalizes relative versus absolute `filename` forms, and how it reports a 0/0 file.
  - Acceptance: `.../evidence/baseline/f1-harness-contract.<TS>.md` states the harness path, the exact invocation, `AGGREGATION_BASIS: filename` or `AGGREGATION_BASIS: class`, `PATH_NORMALIZATION: yes|no`, and `ZERO_OVER_ZERO_REPORTING: N/A|0%`; and it states explicitly whether `KaChar.cs` and `KaKey.cs` (two classes each) report as one figure or two. This artifact satisfies the AC12 disclosure obligation.
- [ ] [P0-T7] Record the disposition of F1 dependency requirements D1-D4 from `spec.md` (the `interface-only` third category, `N/A` rather than `0%` for 0/0, `filename`-keyed attribution, and path normalization).
  - Acceptance: `.../evidence/other/f1-dependencies-d1-d4.<TS>.md` states for each of D1, D2, D3, D4 whether F1 satisfied it, and where not satisfied, records the defect report raised to F1 or the epic orchestrator with the exact text.
- [ ] [P0-T8] Bootstrap the C# toolchain: restore the repo .NET SDK if `.dotnet-sdk/` is absent, run `dotnet tool restore` to materialize csharpier 1.2.6, and confirm `dotnet-coverage` and the VS Test Platform components resolve.
  - Acceptance: `.../evidence/baseline/toolchain-bootstrap.<TS>.md` records `Timestamp:`, `Command:` for each bootstrap step, `EXIT_CODE:` per step, and an `Output Summary:` confirming csharpier, msbuild, vstest.console.exe, and dotnet-coverage each resolve to an explicit path.
- [ ] [P0-T9] Execute FORMAT-CHECK to capture the baseline formatting state.
  - Acceptance: `.../evidence/baseline/format-baseline.<TS>.md` records `Timestamp:`, `Command: dotnet tool run csharpier check .`, `EXIT_CODE:`, and `Output Summary:` with the count of files reported as unformatted.
- [ ] [P0-T10] Execute ANALYZE to capture the baseline analyzer state.
  - Acceptance: `.../evidence/baseline/analyzer-baseline.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with warning and error counts.
- [ ] [P0-T11] Execute TYPECHECK to capture the baseline nullable state.
  - Acceptance: `.../evidence/baseline/typecheck-baseline.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with warning and error counts.
- [ ] [P0-T12] Execute FULL-COVERAGE writing to `.../evidence/baseline/coverage-baseline.cobertura.xml` to capture the baseline test result and repository-wide coverage.
  - Acceptance: `.../evidence/baseline/test-coverage-baseline.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` containing the numeric passed/failed/skipped test counts and the numeric repository-wide `line-rate` and `branch-rate` read from the root `<coverage>` element of the emitted Cobertura XML; the XML itself is committed at the stated path.
- [ ] [P0-T13] Execute PERFILE against `.../evidence/baseline/coverage-baseline.cobertura.xml` to capture the baseline per-file line and branch coverage for all 11 in-scope files.
  - Acceptance: `.../evidence/baseline/perfile-coverage-baseline.<TS>.md` records `Timestamp:`, `Command:` (the PERFILE invocation against `.../evidence/baseline/coverage-baseline.cobertura.xml`), `EXIT_CODE:`, and an `Output Summary:` containing one numeric row per in-scope file, with `N/A` (not `0%`) for every file F1's ledger classifies `interface-only`, and stating that `KeyboardHandler.cs` is absent from the denominator because `[ExcludeFromCodeCoverage]` is still present at line 22.
- [ ] [P0-T14] Record the baseline `git rev-parse HEAD` sha and confirm `git status --porcelain` is empty at the start of work.
  - Acceptance: `.../evidence/baseline/baseline-tree-state.<TS>.md` records `Timestamp:`, `Command:` for both `git rev-parse HEAD` and `git status --porcelain`, `EXIT_CODE:` for each, and an `Output Summary:` containing the sha and `PORCELAIN_CLEAN: true`. The sha is recorded for later diff scoping only; no later task asserts that HEAD still equals it.
- [ ] [P0-T15] Record the mapping from latent defects L1-L6 to their promoted GitHub issue numbers (#444 and #445) so every characterization test can cite the correct issue.
  - Acceptance: `.../evidence/other/latent-defect-issue-map.<TS>.md` maps L1 to #444; L2, L3, L4, L5 to #445; and records L6 as referred to F16 with no code change in this child.

### Phase 1 — KeyboardHandler.cs Seams, De-Exemption, and Coverage

- [ ] [P1-T1] Record the R2 disposition decision (Decisions Record D-A) with its rationale before any seam code is written.
  - Acceptance: `.../evidence/other/r2-disposition.<TS>.md` states `DECISION: OPTION A — separate QuickFiler/Interfaces/MyBoxDialogPrompt.cs`, the rationale that Option B would place the uncoverable statement inside the file that AC1 and AC2 gate, and the recorded fallback condition.
- [ ] [P1-T2] Record the ledger-ratification addendum request for `QuickFiler/Interfaces/MyBoxDialogPrompt.cs` addressed to the epic orchestrator, citing `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy "thinnest possible wiring in the host-bound entry point".
  - Acceptance: `.../evidence/other/mybox-adapter-ledger-request.<TS>.md` contains the exact request text, the target ledger path `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`, and a statement that this child does not self-grant the exemption.
- [ ] [P1-T3] Create `QuickFiler.Test/Controllers/KeyboardHandler.TestSupport.cs` containing an internal static support class exposing `SyncContextScope : IDisposable` (snapshots and restores `SynchronizationContext.Current`) and `InlineSynchronizationContext` (whose `Post` invokes the callback synchronously), following `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:353-378`.
  - Acceptance: the file exists, contains both types, contains no `[TestMethod]`, and contains no `Thread.Sleep`, `Task.Delay`, `.Wait()`, `.Result`, or wall-clock read.
- [ ] [P1-T4] Create `QuickFiler.Test/Controllers/KeyboardHandler.ConstructionTests.cs` containing only the R1 exploratory probe `Constructor_WithEfcViewer_RegistersItselfWithViewer`, which opens a `SyncContextScope`, constructs `new EfcViewer()` headlessly, constructs `new KeyboardHandler(viewer, parentMock.Object)`, and asserts `viewer.KeyboardHandler.Should().BeSameAs(handler)`.
  - Acceptance: the file exists with a single `[TestClass]` and a single `[TestMethod]`, uses MSTest, Moq, and FluentAssertions in Arrange-Act-Assert form, and constructs no `Form` other than `EfcViewer` itself.
- [ ] [P1-T5] Update `QuickFiler.Test/QuickFiler.Test.csproj` to add `<Compile Include="Controllers\KeyboardHandler.TestSupport.cs" />` and `<Compile Include="Controllers\KeyboardHandler.ConstructionTests.cs" />` as one contiguous hunk appended immediately after the existing entry at line 96.
  - Acceptance: both entries are present in a single adjacent block, and ANALYZE exits 0.
- [ ] [P1-T6] Execute SCOPED-TEST with filter `KeyboardHandler.ConstructionTests` to run the R1 exploratory probe and record its outcome.
  - Acceptance: `.../evidence/other/r1-efcviewer-probe.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` stating either `EFCVIEWER_CONSTRUCTS_HEADLESSLY: true` with the passing test name, or `EFCVIEWER_CONSTRUCTS_HEADLESSLY: false` with the verbatim failure message and stack frame.
- [ ] [P1-T7] Record the R1 disposition branch selected by the probe outcome from P1-T6.
  - Acceptance: `.../evidence/other/r1-disposition.<TS>.md` states either `R1_COVERED: the EfcViewer constructor overload (KeyboardHandler.cs:35-39) is exercised by Constructor_WithEfcViewer_RegistersItselfWithViewer and no ledger entry is requested`, or `R1_LEDGER_REQUEST: ratification requested for KeyboardHandler.cs lines 35-39 (5 lines, ~1.2%)` with the reason that the parameter type is a concrete `Form`-derived, already-exempt, F9-owned viewer and the only non-breaking alternative would edit `QuickFiler/Viewers/EfcViewer.cs`; and in that branch the probe test is converted to `[Ignore]` with an XML comment citing the request. In neither branch does this child self-grant an exemption.
- [ ] [P1-T8] Create `QuickFiler/Interfaces/IQfcDialogPrompt.cs` declaring exactly one member, `DialogResult ShowActionDialog(string message, string title, BoxIcon icon, Dictionary<string, Action> actions);`, with XML documentation.
  - Acceptance: the file exists, declares one interface with one member and no other type, and contains no executable statement.
- [ ] [P1-T9] Create `QuickFiler/Interfaces/MyBoxDialogPrompt.cs` declaring `sealed class MyBoxDialogPrompt : IQfcDialogPrompt` whose single expression-bodied member forwards 1:1 to `MyBox.ShowDialog`, with no branching, no state, and no constructor.
  - Acceptance: the file exists, contains exactly one type with exactly one member, contains no `if`, no field, and no explicit constructor, and carries an XML comment recording that the single forwarding statement is the R2 ledger-ratification subject.
- [ ] [P1-T10] Update `QuickFiler/QuickFiler.csproj` to add `<Compile Include="Interfaces\IQfcDialogPrompt.cs" />` and `<Compile Include="Interfaces\MyBoxDialogPrompt.cs" />` as one contiguous hunk adjacent to the existing `Interfaces\MailItemActionsAdapter.cs` entry at line 368.
  - Acceptance: both entries are present in a single adjacent block and ANALYZE exits 0.
- [ ] [P1-T11] Implement seam K3 in `QuickFiler/Controllers/KeyboardHandler.cs`: add the three private readonly fields `_prompt`, `_uiDispatcher`, `_isDroppedDown`; add a private core constructor taking `(IFilerHomeController parent, IQfcDialogPrompt prompt, IUiDispatcher uiDispatcher, Func<ComboBox, bool> isDroppedDown)` that resolves each null argument to `new MyBoxDialogPrompt()`, `new WpfUiDispatcher()`, and `cb => cb.DroppedDown`; and add the three optional trailing parameters defaulted to `null` to both existing public constructors, which now delegate to the core.
  - Acceptance: both public constructors retain their existing first two parameters in the same order and types; the three added parameters are trailing and default to `null`; ANALYZE exits 0; and `QuickFiler/Controllers/QfcHomeController.cs` and `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs` are unmodified.
- [ ] [P1-T12] Implement seam K1 in `QuickFiler/Controllers/KeyboardHandler.cs`: replace the `MyBox.ShowDialog(...)` calls at lines 304-309 and 350-355 with `_prompt.ShowActionDialog(...)`, preserving the exact message, title, icon, and dictionary arguments.
  - Acceptance: no `MyBox.` reference remains anywhere in `KeyboardHandler.cs`; both call sites pass the same four arguments as before; ANALYZE exits 0.
- [ ] [P1-T13] Implement seam K2 in `QuickFiler/Controllers/KeyboardHandler.cs`: replace `UiThread.Dispatcher.Invoke(...)` at lines 362 and 370 and `UiThread.Dispatcher.InvokeAsync(...)` at line 401 with the injected `UtilitiesCS.Threading.IUiDispatcher`, creating no new interface.
  - Acceptance: no `UiThread.Dispatcher` reference remains in `KeyboardHandler.cs`; no new file is added under `UtilitiesCS/`; ANALYZE exits 0.
- [ ] [P1-T14] Implement seam K4 in `QuickFiler/Controllers/KeyboardHandler.cs`: replace the `if (cb.DroppedDown)` read at line 278 with `_isDroppedDown(cb)`.
  - Acceptance: no direct `.DroppedDown` getter read remains in `CboFolders_KeyDownAsync`; the production default `cb => cb.DroppedDown` is unchanged; the `CboFolders_KeyDownAsync(object, KeyEventArgs)` signature is byte-identical.
- [ ] [P1-T15] Implement seam K5 in `QuickFiler/Controllers/KeyboardHandler.cs`: extract private `EnsureUiSyncContext()` and `EnsureWinFormsSyncContext()` helpers and replace all seven duplicated `SynchronizationContext` guard blocks (lines 106-107, 135-136, 152-153, 240-241 for the parent variant; 268-271, 319-322, 393-396 for the WinForms variant).
  - Acceptance: exactly two helpers exist; zero inline `SynchronizationContext.SetSynchronizationContext` call sites remain outside them; observable behavior at each of the seven sites is unchanged; ANALYZE exits 0.
- [ ] [P1-T16] Remove the `[ExcludeFromCodeCoverage]` attribute from `QuickFiler/Controllers/KeyboardHandler.cs` line 22.
  - Acceptance: no `ExcludeFromCodeCoverage` attribute remains anywhere in the file, and no such attribute is added to any other in-scope file.
- [ ] [P1-T17] Remove the three unused `using` directives from `QuickFiler/Controllers/KeyboardHandler.cs` — `System.Web.UI.WebControls` (line 12), `System.Windows.Input` (line 14), and `Microsoft.Office.Interop.Outlook` (line 15).
  - Acceptance: none of the three directives remains; ANALYZE exits 0, confirming no member referenced any Outlook Interop type.
- [ ] [P1-T18] Verify the AC3 additive cross-child contract: `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` is byte-identical to its state at the P0-T14 baseline sha, and both two-argument construction sites `QuickFiler/Controllers/QfcHomeController.cs:184-189` and `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:141-147` are unmodified and compile.
  - Acceptance: `.../evidence/qa-gates/ac3-additive-contract.<TS>.md` records `Timestamp:`, the `git diff --stat <baseline-sha> -- QuickFiler/Interfaces/IQfcKeyboardHandler.cs QuickFiler/Controllers/QfcHomeController.cs QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs` command, `EXIT_CODE:`, and an `Output Summary:` showing an empty diff for all three files, plus the ANALYZE exit code.
- [ ] [P1-T19] Verify AC4 for `QuickFiler/Controllers/KeyboardHandler.cs` by measuring its line count after K1-K5.
  - Acceptance: `.../evidence/qa-gates/keyboardhandler-filesize.<TS>.md` records `Timestamp:`, `Command:` (the line-count command against `QuickFiler/Controllers/KeyboardHandler.cs`), `EXIT_CODE:`, and an `Output Summary:` containing the numeric line count and `UNDER_500: true`. If the count exceeds 500, the documented contingency split at line 262 into `KeyboardHandler.cs` and `KeyboardHandler.FolderRouting.cs` is applied in this task, `partial` is added to the class declaration, and a `<Compile Include>`-only entry is added to `QuickFiler/QuickFiler.csproj` per Decisions Record D-D.
- [ ] [P1-T20] Extend `QuickFiler.Test/Controllers/KeyboardHandler.TestSupport.cs` with `BuildHandler(...)`, returning a constructed `KeyboardHandler` together with its `Mock<IQfcFormViewer>`, `Mock<IFilerHomeController>`, `Mock<IFilerFormController>`, `Mock<IQfcDialogPrompt>`, and `Mock<IUiDispatcher>`, where the `IUiDispatcher` double records but does not execute the supplied `Action`.
  - Acceptance: the helper compiles, exposes all five mocks to callers, and the dispatcher double is asserted in a following test to record without executing.
- [ ] [P1-T21] Create the nine remaining `KeyboardHandler` test files as `[TestClass]` scaffolds — `KeyboardHandler.PropertiesTests.cs`, `KeyboardHandler.PreviewKeyDownTests.cs`, `KeyboardHandler.KeyDownSyncTests.cs`, `KeyboardHandler.KeyDownTaskTests.cs`, `KeyboardHandler.AsyncVoidTests.cs`, `KeyboardHandler.ToggleTests.cs`, `KeyboardHandler.ComboBoxRoutingTests.cs`, `KeyboardHandler.BreadcrumbFallThroughTests.cs`, `KeyboardHandler.GetItemViewerTests.cs`, all under `QuickFiler.Test/Controllers/` — and register all nine in `QuickFiler.Test/QuickFiler.Test.csproj` in one contiguous hunk immediately after the P1-T5 block.
  - Acceptance: all nine files exist with one `[TestClass]` each, all nine `<Compile Include>` entries appear in a single adjacent block, and ANALYZE exits 0.
- [ ] [P1-T22] Implement `Constructor_WithFormViewer_RegistersItselfWithViewer` in `QuickFiler.Test/Controllers/KeyboardHandler.ConstructionTests.cs`, asserting `viewer.Verify(v => v.SetKeyboardHandler(It.IsAny<IQfcKeyboardHandler>()), Times.Once())` and that the captured argument is the same instance as the handler.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T23] Implement `Constructor_WithFormViewer_DefaultsAllSixActionCollectionsToEmptyNotNull` in `KeyboardHandler.ConstructionTests.cs`, asserting each of the six `KbdActions<>` properties is non-null with empty `Keys`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T24] Implement `Constructor_WithFormViewer_DefaultsKbdActiveToFalse` in `KeyboardHandler.ConstructionTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T25] Implement `Constructor_WithFormViewer_UsesSuppliedDialogPromptOverProductionDefault` in `KeyboardHandler.ConstructionTests.cs`, constructing with an explicit `Mock<IQfcDialogPrompt>`, driving `BreadcrumbArrowFallThrough(viewer, Right)`, and asserting the supplied prompt received the call.
  - Acceptance: the named test method exists and passes under SCOPED-TEST, proving the optional K3 parameter is wired rather than ignored.
- [ ] [P1-T26] Implement `Constructor_WithNullFormViewer_ThrowsNullReferenceException` in `KeyboardHandler.ConstructionTests.cs` as a characterization test carrying an XML comment stating that the current unguarded behavior is pinned, not endorsed, and that constructor guards are rejected as a behavior change.
  - Acceptance: the named test method exists, carries the characterization XML comment, and passes under SCOPED-TEST.
- [ ] [P1-T27] Implement `CharActions_SetThenGet_RoundTripsSameInstance` in `QuickFiler.Test/Controllers/KeyboardHandler.PropertiesTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T28] Implement `CharActionsAsync_SetThenGet_RoundTripsSameInstance` in `KeyboardHandler.PropertiesTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T29] Implement `KeyActions_SetThenGet_RoundTripsSameInstance` in `KeyboardHandler.PropertiesTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T30] Implement `KeyActionsAsync_SetThenGet_RoundTripsSameInstance` in `KeyboardHandler.PropertiesTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T31] Implement `AlwaysOnKeyActionsAsync_SetThenGet_RoundTripsSameInstance` in `KeyboardHandler.PropertiesTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T32] Implement `StringActionsAsync_SetThenGet_RoundTripsSameInstance` in `KeyboardHandler.PropertiesTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T33] Implement `KbdActive_SetTrue_GetReturnsTrue` in `KeyboardHandler.PropertiesTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T34] Implement `ClearFilter_AfterPartialFilterAccumulation_DiscardsPendingPrefix` in `KeyboardHandler.PropertiesTests.cs`, driving `KeyDownTaskAsync` with `'a'`, calling `ClearFilter()`, driving `'b'`, and asserting the `"ab"` action was not invoked.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T35] Implement `PreviewKeyDown_KbdInactive_LeavesIsInputKeyFalse` in `QuickFiler.Test/Controllers/KeyboardHandler.PreviewKeyDownTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T36] Implement `PreviewKeyDown_KbdActiveAndKeyRegistered_SetsIsInputKeyTrue` in `KeyboardHandler.PreviewKeyDownTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T37] Implement `PreviewKeyDown_KbdActiveAndKeyNotRegistered_LeavesIsInputKeyFalse` in `KeyboardHandler.PreviewKeyDownTests.cs`, registering `Keys.Up` and pressing `Keys.Down`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T38] Implement `PreviewKeyDown_KeyActionsNull_LeavesIsInputKeyFalse` in `KeyboardHandler.PreviewKeyDownTests.cs`, pinning the null guard at line 98.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T39] Implement `PreviewKeyDownAsync_WithNullAmbientContext_InstallsParentSyncContext` in `KeyboardHandler.PreviewKeyDownTests.cs`, using `SyncContextScope` to null `SynchronizationContext.Current` and restore it in `Dispose`.
  - Acceptance: the named test method exists, uses `SyncContextScope`, and passes under SCOPED-TEST.
- [ ] [P1-T40] Implement `PreviewKeyDownAsync_WithExistingAmbientContext_DoesNotReadParentContext` in `KeyboardHandler.PreviewKeyDownTests.cs`, using a `MockBehavior.Strict` parent with no `UiSyncContext` setup to pin the short-circuit at line 106.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T41] Implement `PreviewKeyDownAsync_KbdActiveAndAsyncKeyRegistered_SetsIsInputKeyTrue` in `KeyboardHandler.PreviewKeyDownTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T42] Implement `PreviewKeyDownAsync_KbdInactive_LeavesIsInputKeyFalse` in `KeyboardHandler.PreviewKeyDownTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T43] Implement `PreviewKeyDownAsync_KeyActionsAsyncNull_LeavesIsInputKeyFalse` in `KeyboardHandler.PreviewKeyDownTests.cs`, pinning the invalid-state guard at line 108.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T44] Implement `KeyDown_KbdInactive_InvokesNoAction` in `QuickFiler.Test/Controllers/KeyboardHandler.KeyDownSyncTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T45] Implement `KeyDown_RegisteredKeyAction_SuppressesKeyPressAndInvokesWithKeyCode` in `KeyboardHandler.KeyDownSyncTests.cs`, using a real `KaKey("src", Keys.Delete, ...)`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T46] Implement `KeyDown_RegisteredCharAction_SuppressesKeyPressAndInvokesWithChar` in `KeyboardHandler.KeyDownSyncTests.cs`, registering the uppercase char that `(char)e.KeyValue` yields for `Keys.R`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T47] Implement `KeyDown_KeyActionAndCharActionBothRegistered_PrefersKeyAction` in `KeyboardHandler.KeyDownSyncTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T48] Implement `KeyDown_NoMatchingAction_LeavesEventUnhandled` in `KeyboardHandler.KeyDownSyncTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T49] Implement `KeyDown_KeyActionsNullAndCharActionRegistered_FallsThroughToCharAction` in `KeyboardHandler.KeyDownSyncTests.cs`, pinning the `else if` at line 124.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T50] Implement `KeyDownTaskAsync_AlwaysOnKeyRegistered_InvokesEvenWhenKbdInactive` in `QuickFiler.Test/Controllers/KeyboardHandler.KeyDownTaskTests.cs` as an `async Task` test.
  - Acceptance: the named test method exists and passes under SCOPED-TEST with no `Task.Delay`, `Thread.Sleep`, `.Wait()`, or `.Result`.
- [ ] [P1-T51] Implement `KeyDownTaskAsync_KbdInactive_DoesNotInvokeKeyActionsAsync` in `KeyboardHandler.KeyDownTaskTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T52] Implement `KeyDownTaskAsync_KbdActiveAndKeyAsyncRegistered_SuppressesAndAwaitsAction` in `KeyboardHandler.KeyDownTaskTests.cs`, using `Task.CompletedTask`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T53] Implement `KeyDownTaskAsync_KbdActiveAndCharAsyncRegistered_SuppressesAndAwaitsAction` in `KeyboardHandler.KeyDownTaskTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T54] Implement `KeyDownTaskAsync_KeyAsyncAndCharAsyncBothRegistered_PrefersKeyAsync` in `KeyboardHandler.KeyDownTaskTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T55] Implement `KeyDownTaskAsync_AlwaysOnAndKeyAsyncBothRegistered_InvokesBothInOrder` in `KeyboardHandler.KeyDownTaskTests.cs`, asserting the invocation-order list equals `["alwaysOn","key"]`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T56] Implement `KeyDownTaskAsync_FirstFilterCharacter_ActivatesAllStringActions` in `KeyboardHandler.KeyDownTaskTests.cs`, pinning lines 186-187.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T57] Implement `KeyDownTaskAsync_StringFilterUniqueMatch_InvokesActionAndResetsFilter` in `KeyboardHandler.KeyDownTaskTests.cs`, pinning lines 191-196.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T58] Implement `KeyDownTaskAsync_StringFilterAmbiguousPrefix_RetainsFilterWithoutInvoking` in `KeyboardHandler.KeyDownTaskTests.cs`, registering keys `"ab"` and `"ac"` and pinning the implicit greater-than-one fall-through at line 191.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T59] Implement `KeyDownTaskAsync_StringFilterUnmatchedCharacter_RollsBackFilterLength` in `KeyboardHandler.KeyDownTaskTests.cs`, pinning the rollback at line 200.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T60] Implement `KeyDownTaskAsync_StringActionsAsyncNull_LeavesEventUnhandled` in `KeyboardHandler.KeyDownTaskTests.cs`, pinning the invalid-state guard at line 178.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T61] Implement `KeyDownTaskAsync_UppercaseKeyValue_IsLowercasedBeforeFilterMatch` in `KeyboardHandler.KeyDownTaskTests.cs`, pinning the `char.ToLower` boundary at line 180.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T62] Implement `KeyDownTaskAsync_ContainsKeyThenFilterKeys_InvokesKeyEqualsTwicePerAction` in `KeyboardHandler.KeyDownTaskTests.cs` as a characterization test with a counting `Update` delegate, carrying an XML comment naming it a characterization test and citing issue #445.
  - Acceptance: the named test method exists, carries the characterization XML comment with the issue number, and passes under SCOPED-TEST.
- [ ] [P1-T63] Implement `KeyDownTaskAsync_WithNullAmbientContext_InstallsParentSyncContext` in `KeyboardHandler.KeyDownTaskTests.cs`, using `SyncContextScope` and pinning lines 152-153.
  - Acceptance: the named test method exists, uses `SyncContextScope`, and passes under SCOPED-TEST.
- [ ] [P1-T64] Implement `KeyboardHandler_KeyDownAsync_DelegatesToKeyDownTaskAsync` in `QuickFiler.Test/Controllers/KeyboardHandler.AsyncVoidTests.cs` under an `InlineSynchronizationContext` installed through `SyncContextScope`, with every awaited task already completed.
  - Acceptance: the named test method exists, uses `InlineSynchronizationContext`, contains no `Thread.Sleep`, `Task.Delay`, `.Wait()`, `.Result`, or wall-clock wait, and passes under SCOPED-TEST.
- [ ] [P1-T65] Implement `KeyboardHandler_KeyDownAsync_ActionThrows_SwallowsExceptionAndDoesNotPropagate` in `KeyboardHandler.AsyncVoidTests.cs`, pinning the `catch` at lines 141-147 and asserting a subsequent successful dispatch still works.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T66] Implement `KeyboardHandler_KeyDownAsync_WithNullAmbientContext_InstallsParentSyncContext` in `KeyboardHandler.AsyncVoidTests.cs`, pinning lines 135-136.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T67] Implement `ToggleKeyboardDialogAsyncEventOverload_MarksEventHandledAndTogglesState` in `KeyboardHandler.AsyncVoidTests.cs`, pinning lines 238-245.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T68] Implement `ToggleKeyboardDialog_WhenInactive_CallsToggleOnNavigationAndActivates` in `QuickFiler.Test/Controllers/KeyboardHandler.ToggleTests.cs`, asserting `ToggleOnNavigation(false)` once.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T69] Implement `ToggleKeyboardDialog_WhenActive_CallsToggleOffNavigationAndDeactivates` in `KeyboardHandler.ToggleTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T70] Implement `ToggleKeyboardDialog_EventOverload_MarksEventHandled` in `KeyboardHandler.ToggleTests.cs`, pinning lines 219-223.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T71] Implement `ToggleKeyboardDialogAsync_WhenInactive_AwaitsToggleOnNavigationAsync` in `KeyboardHandler.ToggleTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T72] Implement `ToggleKeyboardDialogAsync_WhenActive_AwaitsToggleOffNavigationAsync` in `KeyboardHandler.ToggleTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T73] Implement `ToggleKeyboardDialog_CalledTwice_ReturnsToOriginalState` in `KeyboardHandler.ToggleTests.cs` as the state-transition completeness case.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T74] Implement `CboFolders_KeyDownAsync_NonComboBoxSender_ReturnsWithoutRouting` in `QuickFiler.Test/Controllers/KeyboardHandler.ComboBoxRoutingTests.cs` with a `MockBehavior.Strict` `IUiDispatcher` and `VerifyNoOtherCalls()`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T75] Implement `CboFolders_KeyDownAsync_ClosedComboBox_RoutesToDdClosedPath` in `KeyboardHandler.ComboBoxRoutingTests.cs` with a handle-free `new ComboBox()`, asserting `InvokeAsync(It.IsAny<Action>())` once.
  - Acceptance: the named test method exists, creates no window handle, and passes under SCOPED-TEST.
- [ ] [P1-T76] Implement `CboFolders_KeyDownAsync_DroppedDownComboBox_RoutesToDdOpenPath` in `KeyboardHandler.ComboBoxRoutingTests.cs`, injecting `isDroppedDown: _ => true` through the K4 seam.
  - Acceptance: the named test method exists, injects the K4 predicate, and passes under SCOPED-TEST.
- [ ] [P1-T77] Implement `DdOpen_KeyDownAsync_Up_LeavesEventUnhandled` in `KeyboardHandler.ComboBoxRoutingTests.cs`, pinning the `Keys.Up` arm at line 333.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T78] Implement `DdOpen_KeyDownAsync_Down_LeavesEventUnhandled` in `KeyboardHandler.ComboBoxRoutingTests.cs`, pinning the `Keys.Down` arm at line 333.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T79] Implement `DdOpen_KeyDownAsync_Right_ShowsPopOutDialogWithAncestorControllerActions` in `KeyboardHandler.ComboBoxRoutingTests.cs`, using a headless `new ItemViewer()` constructed inside the `InlineSynchronizationContext` scope from P1-T3 (its constructor calls `TaskScheduler.FromCurrentSynchronizationContext()` and therefore requires a non-null ambient `SynchronizationContext`, per the precedent at `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:353-364`), a `Mock<IItemControler>` supplying `RightKeyActions`, and `viewer.Controls.Add(combo)`, asserting the K1 prompt received the exact message, title, icon, and dictionary instance.
  - Acceptance: the named test method exists, shows no dialog, and passes under SCOPED-TEST.
- [ ] [P1-T80] Implement `DdOpen_KeyDownAsync_Left_ClosesDropDownThroughDispatcher` in `KeyboardHandler.ComboBoxRoutingTests.cs`, verifying `Invoke(It.IsAny<Action>())` once without executing the action.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T81] Implement `DdOpen_KeyDownAsync_Return_ClosesDropDownThroughDispatcher` in `KeyboardHandler.ComboBoxRoutingTests.cs`, pinning the `Keys.Return` arm at line 367.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T82] Implement `DdOpen_KeyDownAsync_Escape_ClosesDropDownThroughDispatcher` in `KeyboardHandler.ComboBoxRoutingTests.cs`, pinning the `Keys.Escape` arm at line 367.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T83] Implement `DdOpen_KeyDownAsync_UnrecognizedKey_FallsThroughToKeyDownTask` in `KeyboardHandler.ComboBoxRoutingTests.cs`, pinning the `default` arm at lines 382-387.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T84] Implement `DdOpen_KeyDownAsync_WithNullAmbientContext_InstallsWindowsFormsSyncContext` in `KeyboardHandler.ComboBoxRoutingTests.cs`, asserting the installed context is a `WindowsFormsSynchronizationContext` and restoring the ambient context in a disposable scope.
  - Acceptance: the named test method exists, uses `SyncContextScope`, leaks no ambient context, and passes under SCOPED-TEST.
- [ ] [P1-T85] Implement `DdClosed_KeyDownAsync_Right_OpensDropDownThroughDispatcherAsync` in `KeyboardHandler.ComboBoxRoutingTests.cs`, pinning lines 399-405.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T86] Implement `DdClosed_KeyDownAsync_UnrecognizedKey_FallsThroughToKeyDownTask` in `KeyboardHandler.ComboBoxRoutingTests.cs`, pinning the `default` arm at lines 406-410.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T87] Implement `DdClosed_KeyDownAsync_WithNullAmbientContext_InstallsWindowsFormsSyncContext` in `KeyboardHandler.ComboBoxRoutingTests.cs`, pinning lines 393-396 and restoring the ambient context in a disposable scope.
  - Acceptance: the named test method exists, uses `SyncContextScope`, leaks no ambient context, and passes under SCOPED-TEST.
- [ ] [P1-T88] Implement `BreadcrumbArrowFallThrough_NullViewer_ThrowsArgumentNullExceptionNamingViewer` in `QuickFiler.Test/Controllers/KeyboardHandler.BreadcrumbFallThroughTests.cs`, pinning lines 297-300.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T89] Implement `BreadcrumbArrowFallThrough_Right_ShowsPopOutDialogWithControllerRightKeyActions` in `KeyboardHandler.BreadcrumbFallThroughTests.cs`, using a headless `new ItemViewer()` constructed inside the `InlineSynchronizationContext` scope from P1-T3 (its constructor calls `TaskScheduler.FromCurrentSynchronizationContext()` and therefore requires a non-null ambient `SynchronizationContext`, per the precedent at `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:353-364`) and a `Mock<IItemControler>`, asserting the K1 prompt received the exact dictionary instance, message, title, and icon.
  - Acceptance: the named test method exists, shows no dialog, and passes under SCOPED-TEST.
- [ ] [P1-T90] Implement `BreadcrumbArrowFallThrough_Left_SetsFolderDroppedDownFalseWithoutDialog` in `KeyboardHandler.BreadcrumbFallThroughTests.cs`, asserting no throw and `prompt.VerifyNoOtherCalls()`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T91] Implement `GetItemViewer_ControlIsItemViewer_ReturnsSameInstance` in `QuickFiler.Test/Controllers/KeyboardHandler.GetItemViewerTests.cs`, pinning lines 249-252.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T92] Implement `GetItemViewer_NestedChild_WalksParentChainToItemViewer` in `KeyboardHandler.GetItemViewerTests.cs` using a headless `new ItemViewer()` constructed inside the `InlineSynchronizationContext` scope from P1-T3 (its constructor calls `TaskScheduler.FromCurrentSynchronizationContext()` and therefore requires a non-null ambient `SynchronizationContext`, per the precedent at `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:353-364`), then `itemViewer.Controls.Add(panel)` and `panel.Controls.Add(label)`, pinning the recursion at lines 253-256.
  - Acceptance: the named test method exists, creates no window handle, and passes under SCOPED-TEST.
- [ ] [P1-T93] Implement `GetItemViewer_NoItemViewerAncestor_ReturnsNull` in `KeyboardHandler.GetItemViewerTests.cs` with an orphan `new Panel()`, pinning lines 257-260.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P1-T94] Execute SCOPED-TEST with filter `KeyboardHandler` across all ten `KeyboardHandler.*Tests.cs` classes and record the result.
  - Acceptance: `.../evidence/qa-gates/keyboardhandler-tests.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the numeric passed count matching 73 authored cases (72 if the R1 probe was converted to `[Ignore]` under P1-T7) and zero failures.
- [ ] [P1-T95] Execute FULL-COVERAGE to `.../evidence/qa-gates/coverage-phase1.cobertura.xml` and then PERFILE to measure `QuickFiler/Controllers/KeyboardHandler.cs` after de-exemption.
  - Acceptance: `.../evidence/qa-gates/perfile-coverage-keyboardhandler.<TS>.md` records `Timestamp:`, both `Command:` lines, `EXIT_CODE:`, and an `Output Summary:` containing the numeric line-coverage and branch-coverage percentages for `KeyboardHandler.cs` and an explicit `>= 80%` pass or fail determination.
- [ ] [P1-T96] Verify AC8 for `QuickFiler/Controllers/KeyboardHandler.cs`: no `ArgumentNullException` guard was added to either public constructor, no `ConfigureAwait` was added, and no `async void` member signature was changed.
  - Acceptance: `.../evidence/qa-gates/keyboardhandler-no-behavior-change.<TS>.md` records `Timestamp:`, `Command:` (the `git diff <baseline-sha> -- QuickFiler/Controllers/KeyboardHandler.cs` command), `EXIT_CODE:`, and an `Output Summary:` confirming zero added guard clauses, zero `ConfigureAwait` occurrences, and unchanged `async void` signatures for `KeyboardHandler_KeyDownAsync`, `ToggleKeyboardDialogAsync(object, KeyEventArgs)`, and `CboFolders_KeyDownAsync`.
- [ ] [P1-T97] Record the Phase 1 evidence index, including the unreachable-branch note for `KeyboardHandler.cs` line 189 as a report-only observation with no contrived test.
  - Acceptance: `.../evidence/qa-gates/phase1-summary.<TS>.md` links every Phase 1 evidence artifact by path and records the line-189 unreachable-branch note verbatim.

### Phase 2 — QfcFormKeyHandler.cs Boundary Hardening

- [ ] [P2-T1] Implement `IsAltKeyCommand_WithMenuKeyCode_ReturnsFalse` appended to `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`, asserting `false` because `Keys.Menu` is the ALT key code `0x12`, not the `Keys.Alt` modifier flag `0x40000`.
  - Acceptance: the named test method exists with a FluentAssertions `because` string and passes under SCOPED-TEST.
- [ ] [P2-T2] Implement `IsAltKeyCommand_WithMenuKeyCodePlusAltModifier_ReturnsTrue` in `QfcFormKeyHandlerTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P2-T3] Implement `IsAltKeyCommand_WithControlAndShiftModifiers_ReturnsFalse` in `QfcFormKeyHandlerTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P2-T4] Implement `IsAltKeyCommand_WithControlPlusAltModifiers_ReturnsTrue` in `QfcFormKeyHandlerTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P2-T5] Implement `IsAltKeyCommand_WithAllModifiersAndLetterKey_ReturnsTrue` in `QfcFormKeyHandlerTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P2-T6] Implement `IsAltKeyCommand_WithShiftModifierOnly_ReturnsFalse` in `QfcFormKeyHandlerTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P2-T7] Implement `IsAltKeyCommand_WithArrowKeysUsedByFormNavigation_ReturnsFalse` in `QfcFormKeyHandlerTests.cs` as a `[DataTestMethod]` with `[DataRow]` entries for `Keys.Up`, `Keys.Down`, `Keys.Left`, and `Keys.Right`.
  - Acceptance: the named test method exists with all four data rows and passes under SCOPED-TEST.
- [ ] [P2-T8] Implement `IsAltKeyCommand_WithKeyCodeValueMask_IsUnaffectedByKeyCodeBits` in `QfcFormKeyHandlerTests.cs`, passing `(Keys)0x0000FFFF`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P2-T9] Execute SCOPED-TEST with filter `QfcFormKeyHandlerTests` and record the result.
  - Acceptance: `.../evidence/qa-gates/qfcformkeyhandler-tests.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with 12 passing tests (4 pre-existing plus 8 new) and zero failures.
- [ ] [P2-T10] Record the AC6 structural-inapplicability note for `QuickFiler/Controllers/QfcFormKeyHandler.cs` and confirm the production file is byte-identical to the P0-T14 baseline sha.
  - Acceptance: `.../evidence/qa-gates/qfcformkeyhandler-scenario-completeness.<TS>.md` records `Timestamp:`, `Command:` (the `git diff <baseline-sha> -- QuickFiler/Controllers/QfcFormKeyHandler.cs` command), `EXIT_CODE:`, and an `Output Summary:` stating that null-input and error-handling scenarios are structurally inapplicable because `IsAltKeyCommand` takes a non-nullable `Keys` value type and `Enum.HasFlag` cannot throw, and showing an empty diff for the production file. This determination is carried forward verbatim into the P12-T10 AC6 matrix.

### Phase 3 — KbdActions.cs Construction and Edge Coverage

- [ ] [P3-T1] Create `QuickFiler.Test/Controllers/KbdActionsConstructionAndEdgeTests.cs` as a `[TestClass]` scaffold and add `<Compile Include="Controllers\KbdActionsConstructionAndEdgeTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` immediately adjacent to the existing entry at line 93, in one contiguous hunk.
  - Acceptance: the file exists with one `[TestClass]`, the single `<Compile Include>` entry is adjacent to line 93, and ANALYZE exits 0.
- [ ] [P3-T2] Implement `Ctor_FromEnumerable_CopiesAllElementsInOrder` in `KbdActionsConstructionAndEdgeTests.cs`, covering the currently unexecuted `IEnumerable` constructor at `KbdActions.cs:26-29`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P3-T3] Implement `Ctor_FromEnumerable_SnapshotsSource_LaterMutationDoesNotAffectRegistry` in `KbdActionsConstructionAndEdgeTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P3-T4] Implement `Ctor_FromEnumerable_WithNullList_ThrowsArgumentNullException` in `KbdActionsConstructionAndEdgeTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P3-T5] Implement `Ctor_FromEnumerable_AcceptsDuplicateSourceAndKey_UnlikeAdd` in `KbdActionsConstructionAndEdgeTests.cs` as a characterization test for latent defect L1, seeding two `KaKey("Collection", Keys.Down, ...)` entries and asserting construction does not throw while `Find(Keys.Down)` throws `InvalidOperationException`.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test for current behavior and citing issue #444, and passes under SCOPED-TEST.
- [ ] [P3-T6] Implement `Indexer_Set_WhenKeyNotRegistered_IsSilentNoOp` in `KbdActionsConstructionAndEdgeTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P3-T7] Implement `Indexer_Get_WhenKeyNotRegistered_ThrowsNullReferenceException` in `KbdActionsConstructionAndEdgeTests.cs`.
  - Acceptance: the named test method exists with a FluentAssertions `because` string and passes under SCOPED-TEST.
- [ ] [P3-T8] Implement `NonGenericEnumerator_YieldsTheSameInstancesAsGenericEnumerator` in `KbdActionsConstructionAndEdgeTests.cs`, covering the currently unexecuted explicit `IEnumerable.GetEnumerator` at `KbdActions.cs:139`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P3-T9] Implement `Remove_WhenKeyPresentUnderDifferentSourceId_ReturnsFalseAndRetainsEntry` in `KbdActionsConstructionAndEdgeTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P3-T10] Implement `FilterKeys_WhenNoElementMatches_ReturnsEmptyArrayNotNull` in `KbdActionsConstructionAndEdgeTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P3-T11] Implement `AddInstance_WhenInstanceIsNull_AgainstPopulatedRegistry_ThrowsNullReferenceException` in `KbdActionsConstructionAndEdgeTests.cs` as a characterization test noting the missing `ArgumentNullException` guard.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test and citing issue #444, and passes under SCOPED-TEST.
- [ ] [P3-T12] Implement `CharInstantiation_MatchingAndStorageIdentityAgree_WhenKeyEqualsIsPlainEquality` in `KbdActionsConstructionAndEdgeTests.cs`, exercising the `KbdActions<char, KaChar, Action<char>>` closed generic.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P3-T13] Implement `KeyAsyncInstantiation_AddFindRemoveRoundTrip_PreservesAwaitableDelegate` in `KbdActionsConstructionAndEdgeTests.cs` as an `async Task` test using `Task.CompletedTask`.
  - Acceptance: the named test method exists, contains no `Task.Delay` or `Thread.Sleep`, and passes under SCOPED-TEST.
- [ ] [P3-T14] Implement `CharAsyncInstantiation_AddThroughNewConstraint_ConstructsElementAndStoresDelegate` in `KbdActionsConstructionAndEdgeTests.cs`, exercising the `new()` constraint at `KbdActions.cs:99` against `KaCharAsync`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P3-T15] Execute SCOPED-TEST with filter `FullyQualifiedName~KbdActions`, which matches `KbdActionsConstructionAndEdgeTests`, `KbdActionsTests`, and `KbdActionsRemainingBranchesTests` unambiguously with a single explicit property operand, and record the result.
  - Acceptance: `.../evidence/qa-gates/kbdactions-tests.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the numeric passed count, zero failures, and confirmation that all three `KbdActions*` test classes were selected by the filter.
- [ ] [P3-T16] Verify `QuickFiler/Controllers/KbdActions.cs` is byte-identical to the P0-T14 baseline sha.
  - Acceptance: `.../evidence/qa-gates/kbdactions-no-production-change.<TS>.md` records `Timestamp:`, the `git diff <baseline-sha> -- QuickFiler/Controllers/KbdActions.cs` command, `EXIT_CODE:`, and an empty-diff `Output Summary:`. The unreachable-branch note for `KeyboardHandler.cs:189` is recorded once, by P1-T97, and is not repeated here.

### Phase 4 — KaChar.cs Orphan-Member, Boundary, and Error Coverage

- [ ] [P4-T1] Implement `KaChar_DelegateType_ReturnsActionOfKeys_CharacterizingKnownMismatch` appended to `QuickFiler.Test/Controllers/KaCharTests.cs` as a characterization test for latent defect L2.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test for current behavior and citing issue #445, and passes under SCOPED-TEST.
- [ ] [P4-T2] Implement `KaChar_Update_DefaultsToNullAndRoundTripsAssignedAction` in `KaCharTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P4-T3] Implement `KaChar_Update_InvokesAssignedActionWithSuppliedString` in `KaCharTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P4-T4] Implement `KaChar_SourceIdKeyAndDelegateSetters_ReplaceConstructedValues` in `KaCharTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P4-T5] Implement `KaChar_KeyEquals_AtCharMaxValueBoundary_MatchesAndRejects` in `KaCharTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P4-T6] Implement `KaChar_Delegate_WhenActionThrows_PropagatesToCaller` in `KaCharTests.cs`.
  - Acceptance: the named test method exists with a FluentAssertions `because` string and passes under SCOPED-TEST.
- [ ] [P4-T7] Implement `KaChar_Delegate_WhenNull_InvocationThrowsNullReferenceException` in `KaCharTests.cs` as a characterization test.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test and citing issue #445, and passes under SCOPED-TEST.
- [ ] [P4-T8] Implement `KaCharAsync_ParameterlessConstructor_LeavesNullDelegateAndDefaultKey` in `KaCharTests.cs`, covering the constructor reached through the `new()` constraint at `KbdActions.cs:99`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P4-T9] Implement `KaCharAsync_Update_DefaultsToNullAndRoundTripsAssignedAction` in `KaCharTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P4-T10] Implement `KaCharAsync_SourceIdKeyAndDelegateSetters_ReplaceConstructedValues` in `KaCharTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P4-T11] Implement `KaCharAsync_KeyEquals_AtDefaultAndMaxCharBoundaries_MatchesAndRejects` in `KaCharTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P4-T12] Implement `KaCharAsync_Delegate_WhenFunctionReturnsFaultedTask_AwaitObservesTheFault` in `KaCharTests.cs` as an `async Task` test using `Task.FromException`.
  - Acceptance: the named test method exists, contains no `Task.Delay` or `Thread.Sleep`, and passes under SCOPED-TEST.
- [ ] [P4-T13] Implement `KaCharAsync_Delegate_WhenFunctionThrowsSynchronously_ThrowsBeforeTaskIsReturned` in `KaCharTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P4-T14] Execute SCOPED-TEST with filter `KaCharTests` and record the result.
  - Acceptance: `.../evidence/qa-gates/kachar-tests.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the numeric passed count and zero failures.
- [ ] [P4-T15] Verify `QuickFiler.Test/Controllers/KaCharTests.cs` remains at or below 500 lines and that `QuickFiler/Controllers/KaChar.cs` is byte-identical to the P0-T14 baseline sha.
  - Acceptance: `.../evidence/qa-gates/kachar-filesize-and-no-production-change.<TS>.md` records `Timestamp:`, `Command:` for both the line-count command and the `git diff <baseline-sha> -- QuickFiler/Controllers/KaChar.cs` command, `EXIT_CODE:` for each, and an `Output Summary:` containing the numeric test-file line count with `UNDER_500: true` and an empty diff for the production file.
- [ ] [P4-T16] Record the `KaChar.cs` coverage-attribution note stating whether the harness reports `KaChar` and `KaCharAsync` as one file figure or two class figures, using the basis established in P0-T6.
  - Acceptance: `.../evidence/qa-gates/kachar-attribution.<TS>.md` states the aggregation basis and which of the two reporting shapes applies.

### Phase 5 — KaKey.cs Orphan-Member, Flags-Contract, and Error Coverage

- [ ] [P5-T1] Implement `KaKey_DelegateType_ReturnsActionOfKeys_MatchingItsDeclaredDelegate` appended to `QuickFiler.Test/Controllers/KaKeyTests.cs`.
  - Acceptance: the named test method exists with a FluentAssertions `because` string and passes under SCOPED-TEST.
- [ ] [P5-T2] Implement `KaKey_Update_DefaultsToNullAndRoundTripsAssignedAction` in `KaKeyTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P5-T3] Implement `KaKey_Update_InvokesAssignedActionWithSuppliedString` in `KaKeyTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P5-T4] Implement `KaKey_SourceIdKeyAndDelegateSetters_ReplaceConstructedValues` in `KaKeyTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P5-T5] Implement `KaKey_KeyEquals_WithModifierCombinedKey_DoesNotMatchBareKeyCode` in `KaKeyTests.cs`, worded as contract documentation rather than as a defect regression test.
  - Acceptance: the named test method exists with a FluentAssertions `because` string citing the `e.KeyCode` lookup path and passes under SCOPED-TEST.
- [ ] [P5-T6] Implement `KaKey_KeyEquals_WithExplicitNoneAndUndefinedValue_BehavesAsPlainEquality` in `KaKeyTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P5-T7] Implement `KaKey_Delegate_WhenActionThrows_PropagatesToCaller` in `KaKeyTests.cs`.
  - Acceptance: the named test method exists with a FluentAssertions `because` string and passes under SCOPED-TEST.
- [ ] [P5-T8] Implement `KaKey_Delegate_WhenNull_InvocationThrowsNullReferenceException` in `KaKeyTests.cs` as a characterization test.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test and citing issue #445, and passes under SCOPED-TEST.
- [ ] [P5-T9] Implement `KaKeyAsync_ParameterlessConstructor_LeavesNullDelegateAndNoneKey` in `KaKeyTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P5-T10] Implement `KaKeyAsync_Update_DefaultsToNullAndRoundTripsAssignedAction` in `KaKeyTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P5-T11] Implement `KaKeyAsync_SourceIdKeyAndDelegateSetters_ReplaceConstructedValues` in `KaKeyTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P5-T12] Implement `KaKeyAsync_KeyEquals_WithModifierCombinedKey_DoesNotMatchBareKeyCode` in `KaKeyTests.cs`, worded as contract documentation.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P5-T13] Implement `KaKeyAsync_Delegate_WhenFunctionReturnsFaultedTask_AwaitObservesTheFault` in `KaKeyTests.cs` as an `async Task` test using `Task.FromException`.
  - Acceptance: the named test method exists, contains no `Task.Delay` or `Thread.Sleep`, and passes under SCOPED-TEST.
- [ ] [P5-T14] Implement `KaKeyAsync_Delegate_WhenFunctionThrowsSynchronously_ThrowsBeforeTaskIsReturned` in `KaKeyTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P5-T15] Execute SCOPED-TEST with filter `KaKeyTests` and record the result.
  - Acceptance: `.../evidence/qa-gates/kakey-tests.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the numeric passed count and zero failures.
- [ ] [P5-T16] Verify `QuickFiler.Test/Controllers/KaKeyTests.cs` remains at or below 500 lines and that `QuickFiler/Controllers/KaKey.cs` is byte-identical to the P0-T14 baseline sha.
  - Acceptance: `.../evidence/qa-gates/kakey-filesize-and-no-production-change.<TS>.md` records `Timestamp:`, `Command:` for both the line-count command and the `git diff <baseline-sha> -- QuickFiler/Controllers/KaKey.cs` command, `EXIT_CODE:` for each, and an `Output Summary:` containing the numeric test-file line count with `UNDER_500: true` and an empty diff for the production file.
- [ ] [P5-T17] Record the `KaKey.cs` coverage-attribution note stating whether the harness reports `KaKey` and `KaKeyAsync` as one file figure or two class figures.
  - Acceptance: `.../evidence/qa-gates/kakey-attribution.<TS>.md` states the aggregation basis and which reporting shape applies.

### Phase 6 — KaStringAsync.cs Branch, Boundary, and Input-Validation Coverage

- [ ] [P6-T1] Implement `KeyEquals_WithEmptyString_WhileActivatedWithUpdate_ThrowsArgumentOutOfRangeException` appended to `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` as a characterization test for latent defect L4.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test for current behavior, citing issue #445 and recording that the path is unreachable through `KeyboardHandler`, and passes under SCOPED-TEST.
- [ ] [P6-T2] Implement `KeyEquals_WithEmptyString_WhileNotActivated_ReturnsTrueWithoutThrowing` in `KaStringAsyncTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P6-T3] Implement `KeyEquals_WithNull_ThrowsArgumentNullException` in `KaStringAsyncTests.cs` as a characterization test.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test and citing issue #445, and passes under SCOPED-TEST.
- [ ] [P6-T4] Implement `KeyEquals_ContainsMatchWhileActivatedWithNullUpdate_ReturnsTrueWithoutThrowing` in `KaStringAsyncTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P6-T5] Implement `KeyEquals_SingleCharNonMatchWhileNotActivated_DoesNotInvokeToggleControl` in `KaStringAsyncTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P6-T6] Implement `KeyEquals_SingleCharNonMatchWhileActivatedWithNullToggle_DoesNotThrow` in `KaStringAsyncTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P6-T7] Implement `KeyEquals_MultiCharNonMatchWhileNotActivated_InvokesUpdateButNotToggleControl` in `KaStringAsyncTests.cs` as the characterization test for latent defect L5, the only case that separates the line-61 and line-72 gates.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test, flagging the divergence as intent-unclear rather than a confirmed bug, and citing issue #445, and passes under SCOPED-TEST.
- [ ] [P6-T8] Implement `KeyEquals_OnNonMatchBranches_ResetsActivatedToFalse` in `KaStringAsyncTests.cs`, covering the single-char and multi-char non-match branches.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P6-T9] Implement `KeyEquals_WithExactKeyMatch_InvokesUpdateWithLastCharacter` in `KaStringAsyncTests.cs`.
  - Acceptance: the named test method exists with a FluentAssertions `because` string and passes under SCOPED-TEST.
- [ ] [P6-T10] Implement `KeyEquals_WithSingleCharSubstringNotAtStart_ReportsCharacterByProbeLengthNotMatchPosition` in `KaStringAsyncTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P6-T11] Implement `KeyEquals_IsOrdinalAndCaseSensitive_UppercaseProbeDoesNotMatchLowercasedKey` in `KaStringAsyncTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P6-T12] Implement `KeySetter_WithNull_ThrowsNullReferenceException` in `KaStringAsyncTests.cs` as a characterization test.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test and citing issue #445, and passes under SCOPED-TEST.
- [ ] [P6-T13] Implement `Constructor_WithNullKey_ThrowsNullReferenceException` in `KaStringAsyncTests.cs` as a characterization test distinct from the setter case.
  - Acceptance: the named test method exists, carries an XML comment naming it a characterization test and citing issue #445, and passes under SCOPED-TEST.
- [ ] [P6-T14] Implement `Setters_AfterConstruction_ReplaceSourceIdKeyDelegateUpdateAndToggleControl` in `KaStringAsyncTests.cs`, asserting the `Key` setter re-normalizes to lower case.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P6-T15] Implement `Delegate_WhenFunctionReturnsFaultedTask_AwaitObservesTheFault` in `KaStringAsyncTests.cs` as an `async Task` test using `Task.FromException`.
  - Acceptance: the named test method exists, contains no `Task.Delay` or `Thread.Sleep`, and passes under SCOPED-TEST.
- [ ] [P6-T16] Execute SCOPED-TEST with filter `KaStringAsyncTests` and record the result.
  - Acceptance: `.../evidence/qa-gates/kastringasync-tests.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the numeric passed count and zero failures.
- [ ] [P6-T17] Verify `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` remains at or below 500 lines, splitting the boundary and error cases into `QuickFiler.Test/Controllers/KaStringAsyncBoundaryTests.cs` with a `<Compile Include>` entry adjacent to line 96 only if the measured count exceeds 500.
  - Acceptance: `.../evidence/qa-gates/kastringasync-filesize.<TS>.md` records `Timestamp:`, `Command:` (the line-count command against `QuickFiler.Test/Controllers/KaStringAsyncTests.cs`), `EXIT_CODE:`, and an `Output Summary:` containing the numeric line count and `UNDER_500: true`, and stating whether the split was applied. If the split is applied, its `<Compile Include>`-only hunk is the contingency hunk permitted by Decisions Record D-E.
- [ ] [P6-T18] Verify AC11 for this file: `QuickFiler/Controllers/KaStringAsync.cs` is byte-identical to the P0-T14 baseline sha, and neither it nor `KaStringAsyncTests.cs` contains `async`, `await`, `Task.Delay`, `Thread.Sleep`, any timer type, `DateTime`, `DateTimeOffset`, `TimeProvider`, `FakeTimeProvider`, or `Stopwatch` in production code, with no remediation performed on the pre-existing suite.
  - Acceptance: `.../evidence/qa-gates/kastringasync-ac11.<TS>.md` records `Timestamp:`, `Command:` for the grep command and the `git diff <baseline-sha> -- QuickFiler/Controllers/KaStringAsync.cs` command, `EXIT_CODE:` for each, and a zero-match `Output Summary:` for the production file that also shows an empty diff for it and states that Correction C1 is applied and the pre-existing suite required no remediation.
- [ ] [P6-T19] Record the Phase 6 expectation note stating that `KaStringAsync.cs` line coverage is not expected to move because every executable line was already reached, and that the value of this phase is branch coverage and scenario completeness.
  - Acceptance: `.../evidence/qa-gates/kastringasync-coverage-expectation.<TS>.md` states the expectation with its `research/07-KaStringAsync.md` §3 citation.

### Phase 7 — MailItemActionsAdapter.cs Guard and Hardening

- [ ] [P7-T1] Implement the `ArgumentNullException` constructor guard in `QuickFiler/Interfaces/MailItemActionsAdapter.cs` replacing the bare `_mail = mail;` at line 19, together with its test `Constructor_WithNullMailItem_ThrowsArgumentNullException` appended to `QuickFiler.Test/Controllers/MailItemActionsAdapterTests.cs`, as one indivisible task.
  - Acceptance: the guard throws `ArgumentNullException` with `ParamName == "mail"`; the named test method exists and asserts that `ParamName`; the public constructor shape `MailItemActionsAdapter(MailItem)` is byte-identical; and SCOPED-TEST for `MailItemActionsAdapterTests` exits 0. Neither the guard nor the test is committed without the other.
- [ ] [P7-T2] Implement `Construction_YieldsAnIMailItemActions` in `MailItemActionsAdapterTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P7-T3] Implement `Reply_WhenUnderlyingMailItemThrows_PropagatesException` in `MailItemActionsAdapterTests.cs` using a throwing `Mock<MailItem>` setup.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P7-T4] Implement `UnRead_Get_ReturnsFalse_WhenUnderlyingMailItemIsRead` in `MailItemActionsAdapterTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P7-T5] Implement `UnRead_Set_True_ForwardsToUnderlyingMailItem` in `MailItemActionsAdapterTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P7-T6] Implement `EntryID_WhenUnderlyingMailItemReturnsNull_ReturnsNull` in `MailItemActionsAdapterTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P7-T7] Implement `Reply_WhenUnderlyingMailItemReturnsNull_ReturnsNull` in `MailItemActionsAdapterTests.cs`.
  - Acceptance: the named test method exists and passes under SCOPED-TEST.
- [ ] [P7-T8] Implement `Display_InvokesUnderlyingMailItemNonModally` in `MailItemActionsAdapterTests.cs` with an argument-capturing callback, asserting `captured.Should().NotBe(true)` and not tightening the assertion speculatively.
  - Acceptance: the named test method exists, asserts inequality to boxed `true` rather than equality to a sentinel, and passes under SCOPED-TEST.
- [ ] [P7-T9] Execute SCOPED-TEST with filter `MailItemActionsAdapterTests` and record the result.
  - Acceptance: `.../evidence/qa-gates/mailitemactionsadapter-tests.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with 15 passing tests (7 pre-existing plus 8 new) and zero failures.
- [ ] [P7-T10] Verify AC10 and AC9 for this file: the only production diff is the constructor guard, no sibling-owned file was modified, and `QuickFiler/Controllers/QfcItemController.Initialization.cs` is unchanged.
  - Acceptance: `.../evidence/qa-gates/mailitemactionsadapter-ac9-ac10.<TS>.md` records `Timestamp:`, `Command:` (the `git diff --stat <baseline-sha> -- QuickFiler/Interfaces/MailItemActionsAdapter.cs QuickFiler/Controllers/QfcItemController.Initialization.cs` command), `EXIT_CODE:`, and an `Output Summary:` showing the guard-only diff and an empty diff for the F10-owned file, plus the statement that the guard and its test shipped together.
- [ ] [P7-T11] Record the L6 test-layout deviation referral: `QuickFiler.Test/Controllers/MailItemActionsAdapterTests.cs` sits in `Controllers/` while its production file is `QuickFiler/Interfaces/MailItemActionsAdapter.cs`, and the file is deliberately not moved.
  - Acceptance: `.../evidence/other/l6-test-layout-referral.<TS>.md` states the deviation, the reason it is not fixed here (a rename-vs-edit merge conflict in the legacy non-SDK `QuickFiler.Test.csproj` during a 14-child parallel wave for zero coverage benefit), and the referral to F16 to adjudicate project-wide.
- [ ] [P7-T12] Record the Phase 7 branch-coverage note confirming that `MailItemActionsAdapter.cs` remains at 100% branch coverage after the guard, because the guard's both branches are exercised by P7-T1 and the existing seven forwarding tests.
  - Acceptance: `.../evidence/qa-gates/mailitemactionsadapter-branch-note.<TS>.md` states the determination and cites the coverage-on-changed-lines rule in `.claude/rules/csharp.md:41`.

### Phase 8 — IKbdAction.cs Interface-Only Disposition

- [ ] [P8-T1] Verify that `QuickFiler/Interfaces/IKbdAction.cs` emits zero executable IL by exhaustive construct check — no default interface member, no static member, no static constructor, no constant initializer, no attribute constructor, no nested type, no auto-property initializer, no event accessor, and no operator.
  - Acceptance: `.../evidence/qa-gates/ikbdaction-zero-il.<TS>.md` records the file's full member list, states `ZERO_EXECUTABLE_IL: true`, and cites `.claude/rules/general-unit-test.md` § Coverage Requirements interface-only clarification.
- [ ] [P8-T2] Record F1's ledger classification for `QuickFiler/Interfaces/IKbdAction.cs`, cited by ledger line number, and apply the AC13 escalation rule if the classification is `testable` with an `>= 80%` target.
  - Acceptance: `.../evidence/qa-gates/ikbdaction-ledger-citation.<TS>.md` quotes the ledger row verbatim with its line number and records either acceptance or the escalation text sent to the epic orchestrator.
- [ ] [P8-T3] Verify `QuickFiler/Interfaces/IKbdAction.cs` is byte-identical to the P0-T14 baseline sha, confirming that neither the unused `using` directives nor the commented-out members at lines 15-16 were touched.
  - Acceptance: `.../evidence/qa-gates/ikbdaction-unchanged.<TS>.md` records `Timestamp:`, `Command:` (the `git diff <baseline-sha> -- QuickFiler/Interfaces/IKbdAction.cs` command), `EXIT_CODE:`, and an empty-diff `Output Summary:`.
- [ ] [P8-T4] Record the per-file coverage evidence entry for `QuickFiler/Interfaces/IKbdAction.cs` as `N/A` with its stated reason, and record whether the harness emitted a Cobertura `<class>` element for the file or omitted it entirely.
  - Acceptance: `.../evidence/qa-gates/ikbdaction-perfile.<TS>.md` records `PER_FILE_COVERAGE: N/A`, the reason `interface-only — zero executable lines — not in the coverage denominator`, `COBERTURA_CLASS_ELEMENT_EMITTED: true|false`, and confirms zero test cases were written for this file by design.

### Phase 9 — IQfcKeyboardHandler.cs Interface-Only Disposition and Freeze Verification

- [ ] [P9-T1] Verify that `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` emits zero executable IL by the same exhaustive construct check.
  - Acceptance: `.../evidence/qa-gates/iqfckeyboardhandler-zero-il.<TS>.md` records the full member list, states `ZERO_EXECUTABLE_IL: true`, and cites the interface-only clarification.
- [ ] [P9-T2] Record F1's ledger classification for `QuickFiler/Interfaces/IQfcKeyboardHandler.cs`, cited by ledger line number, and apply the AC13 escalation rule if it is classified `testable`.
  - Acceptance: `.../evidence/qa-gates/iqfckeyboardhandler-ledger-citation.<TS>.md` quotes the ledger row verbatim with its line number and records either acceptance or the escalation text.
- [ ] [P9-T3] Verify the AC3 freeze: `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` is byte-identical to the P0-T14 baseline sha, including the unused `using System.Collections.Generic;` at line 2, which is deliberately retained to keep the file out of the epic's conflict surface.
  - Acceptance: `.../evidence/qa-gates/iqfckeyboardhandler-frozen.<TS>.md` records `Timestamp:`, `Command:` (the `git diff <baseline-sha> -- QuickFiler/Interfaces/IQfcKeyboardHandler.cs` command), `EXIT_CODE:`, an empty-diff `Output Summary:`, and the explicit statement that no member was added, removed, renamed, or re-typed.
- [ ] [P9-T4] Record the per-file coverage evidence entry for `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` as `N/A` with its stated reason.
  - Acceptance: `.../evidence/qa-gates/iqfckeyboardhandler-perfile.<TS>.md` records `PER_FILE_COVERAGE: N/A`, the reason, `COBERTURA_CLASS_ELEMENT_EMITTED: true|false`, and confirms zero test cases were written for this file by design.

### Phase 10 — IMailItemActions.cs Interface-Only Disposition

- [ ] [P10-T1] Verify that `QuickFiler/Interfaces/IMailItemActions.cs` emits zero executable IL by the same exhaustive construct check.
  - Acceptance: `.../evidence/qa-gates/imailitemactions-zero-il.<TS>.md` records the full member list, states `ZERO_EXECUTABLE_IL: true`, and cites the interface-only clarification.
- [ ] [P10-T2] Record F1's ledger classification for `QuickFiler/Interfaces/IMailItemActions.cs`, cited by ledger line number, and apply the AC13 escalation rule if it is classified `testable`.
  - Acceptance: `.../evidence/qa-gates/imailitemactions-ledger-citation.<TS>.md` quotes the ledger row verbatim with its line number and records either acceptance or the escalation text.
- [ ] [P10-T3] Verify `QuickFiler/Interfaces/IMailItemActions.cs` is byte-identical to the P0-T14 baseline sha.
  - Acceptance: `.../evidence/qa-gates/imailitemactions-unchanged.<TS>.md` records `Timestamp:`, `Command:` (the `git diff <baseline-sha> -- QuickFiler/Interfaces/IMailItemActions.cs` command), `EXIT_CODE:`, and an empty-diff `Output Summary:`.
- [ ] [P10-T4] Record the per-file coverage evidence entry for `QuickFiler/Interfaces/IMailItemActions.cs` as `N/A` with its stated reason.
  - Acceptance: `.../evidence/qa-gates/imailitemactions-perfile.<TS>.md` records `PER_FILE_COVERAGE: N/A`, the reason, `COBERTURA_CLASS_ELEMENT_EMITTED: true|false`, and confirms zero test cases were written for this file by design.

### Phase 11 — IItemControler.cs Interface-Only Disposition

- [ ] [P11-T1] Verify that `QuickFiler/Interfaces/IItemControler.cs` emits zero executable IL by the same exhaustive construct check.
  - Acceptance: `.../evidence/qa-gates/iitemcontroler-zero-il.<TS>.md` records the full member list, states `ZERO_EXECUTABLE_IL: true`, and cites the interface-only clarification.
- [ ] [P11-T2] Record F1's ledger classification for `QuickFiler/Interfaces/IItemControler.cs`, cited by ledger line number, and apply the AC13 escalation rule if it is classified `testable`.
  - Acceptance: `.../evidence/qa-gates/iitemcontroler-ledger-citation.<TS>.md` quotes the ledger row verbatim with its line number and records either acceptance or the escalation text.
- [ ] [P11-T3] Verify `QuickFiler/Interfaces/IItemControler.cs` is byte-identical to the P0-T14 baseline sha, confirming the misspelled type name, the namespace, the three unused `using` directives, and the redundant `public` at line 13 were all left untouched.
  - Acceptance: `.../evidence/qa-gates/iitemcontroler-byte-identical.<TS>.md` records `Timestamp:`, `Command:` (the `git diff <baseline-sha> -- QuickFiler/Interfaces/IItemControler.cs` command), `EXIT_CODE:`, an empty-diff `Output Summary:`, and the statement that every contemplated change touches an F9-, F10-, or F14-owned file.
- [ ] [P11-T4] Record the per-file coverage evidence entry for `QuickFiler/Interfaces/IItemControler.cs` as `N/A`, and record the D3 harness verification that coverage was attributed by the `<class>` element's `filename` attribute rather than by substring match on the type name.
  - Acceptance: `.../evidence/qa-gates/iitemcontroler-perfile.<TS>.md` records `PER_FILE_COVERAGE: N/A`, the reason, `ATTRIBUTION_BY_FILENAME_ATTRIBUTE: true|false`, and confirms no `ItemViewer.cs` line was mis-attributed to this file.

### Phase 12 — Final QA Loop, Per-File Coverage Verification, and Evidence

- [ ] [P12-T1] Execute FORMAT (`dotnet tool run csharpier format .`) and record the result.
  - Acceptance: `.../evidence/qa-gates/final-format.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with the count of files reformatted. If any file changed, the loop restarts at this task.
- [ ] [P12-T2] Execute FORMAT-CHECK (`dotnet tool run csharpier check .`) and require exit code 0.
  - Acceptance: `.../evidence/qa-gates/final-format-check.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` reporting zero unformatted files.
- [ ] [P12-T3] Execute ANALYZE and require exit code 0.
  - Acceptance: `.../evidence/qa-gates/final-analyzer.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with zero errors and the warning count compared against the P0-T10 baseline.
- [ ] [P12-T4] Execute TYPECHECK and require exit code 0.
  - Acceptance: `.../evidence/qa-gates/final-typecheck.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with zero errors and the warning count compared against the P0-T11 baseline.
- [ ] [P12-T5] Execute FULL-COVERAGE writing to `.../evidence/qa-gates/coverage-final.cobertura.xml` and require exit code 0.
  - Acceptance: `.../evidence/qa-gates/final-test-coverage.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` containing the numeric passed/failed/skipped counts and the numeric repository-wide `line-rate` and `branch-rate`; the XML is committed at the stated path.
- [ ] [P12-T6] Verify AC7 by confirming that P12-T1 through P12-T5 completed as one uninterrupted pass with no step failing and no step changing a file, restarting the loop from P12-T1 if either occurred.
  - Acceptance: `.../evidence/qa-gates/final-toolchain-pass.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` naming the five commands P12-T1 through P12-T5, mapping to the four AC7 toolchain stages, naming for the test stage both the `scripts/vscode/Invoke-MSTestWithCoverage.ps1` wrapper and the underlying coverage-enabled `vstest.console.exe` invocation it drives, recording each of the five exit codes as 0, stating `UNINTERRUPTED_PASS: true`, and recording the number of loop restarts that preceded the clean pass.
- [ ] [P12-T7] Execute PERFILE against `.../evidence/qa-gates/coverage-final.cobertura.xml` to produce the final per-file coverage table for all 11 in-scope files plus the two new production files.
  - Acceptance: `.../evidence/qa-gates/perfile-coverage-final.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` containing a table with one numeric row per file, `N/A` (never `0%`) for every file F1's ledger classifies `interface-only`, an explicit `>= 80%` pass or fail per `testable` file, and the aggregation basis carried forward from P0-T6.
- [ ] [P12-T8] Verify the coverage delta and thresholds by comparing baseline, post-change, and new-code figures.
  - Acceptance: `.../evidence/qa-gates/coverage-delta.<TS>.md` records, as numeric values: baseline per-file coverage from P0-T13, post-change per-file coverage from P12-T7, the delta per file, the new-code coverage for `QuickFiler/Interfaces/MyBoxDialogPrompt.cs` against the `>= 90%` floor, and an explicit statement that no changed line lost coverage. `QuickFiler/Interfaces/IQfcDialogPrompt.cs` is recorded as `N/A` rather than measured against the `>= 90%` floor, because it is interface-only with zero executable lines; the artifact cites F1 dependency D1 (the `interface-only` third category) and the interface-only clarification in `.claude/rules/general-unit-test.md` § Coverage Requirements. Where the `>= 90%` new-code floor is not met by `MyBoxDialogPrompt.cs`, the artifact cites the P1-T2 ledger-ratification request rather than recording a self-granted exemption.
- [ ] [P12-T9] Gate AC1 and AC2 on the P12-T7 per-file table: every file F1's ledger classifies `testable` must measure `>= 80%` line coverage, and `QuickFiler/Controllers/KeyboardHandler.cs` must reach the floor net of any F1-ratified remainder from P1-T7. If any `testable` file is below the floor, add targeted test cases for the specific uncovered lines named in the table and re-run P12-T1 through P12-T7; if the shortfall is irreducible, escalate to the epic orchestrator per AC13 and record the escalation rather than self-granting an exemption.
  - Acceptance: `.../evidence/qa-gates/ac1-ac2-gate.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` naming each `testable` file with its numeric line-coverage figure and `FLOOR_MET: true|false`, states `ALL_TESTABLE_FILES_AT_FLOOR: true`, and records the number of remediation iterations performed. A `false` in any row blocks plan completion.
- [ ] [P12-T10] Verify AC6 by recording a scenario-completeness matrix covering all seven in-scope files with executable behavior — `KeyboardHandler.cs`, `KbdActions.cs`, `KaChar.cs`, `KaKey.cs`, `KaStringAsync.cs`, `QfcFormKeyHandler.cs`, `MailItemActionsAdapter.cs`.
  - Acceptance: `.../evidence/qa-gates/ac6-scenario-completeness.<TS>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` containing one row per file with the four columns positive / invalid-input / boundary / error-handling, each cell naming at least one covering test method by fully qualified name or the words `STRUCTURALLY INAPPLICABLE` plus the structural reason. No cell is left blank and no test is manufactured to fill a structurally inapplicable cell; the `QfcFormKeyHandler` determination is carried forward verbatim from P2-T10.
- [ ] [P12-T11] Record the AC14 repository-wide coverage figures before and after this child's work as a record-and-report obligation.
  - Acceptance: `.../evidence/qa-gates/repo-wide-coverage.<TS>.md` records the baseline `line-rate` and `branch-rate` from P0-T12, the final figures from P12-T5, the numeric deltas, and the explicit statement that the repository-wide floor is not a blocking gate for this child while AC1 and AC2 (gated at P12-T9) and the `>= 90%` new-code floor (gated at P12-T8) are.
- [ ] [P12-T12] Verify AC9 file-boundary isolation by diffing the full changed-file list against the P0-T14 baseline sha.
  - Acceptance: `.../evidence/qa-gates/ac9-file-boundary.<TS>.md` records `Timestamp:`, `Command:` (the `git diff --name-only <baseline-sha>` command), `EXIT_CODE:`, and an `Output Summary:` listing every changed path, together with an explicit confirmation that `coverage.config`, `UtilitiesCS/Properties/AssemblyInfo.cs`, every shared build property file, and every sibling-owned file — specifically `QfcCollectionController.cs`, `QfcItemController.*`, `QfcHomeController.cs`, `EfcHomeControllerDependencyFactories.cs`, `ItemViewer.*`, and `EfcViewer.cs` — are absent from the list, and that the only `.csproj` edits are `QuickFiler/QuickFiler.csproj` (two F3-authored production files, per Decisions Record D-D) and `QuickFiler.Test/QuickFiler.Test.csproj` (new test file entries, per D-E).
- [ ] [P12-T13] Verify AC11 across the whole change set by searching every added or modified production and test file for `TimeProvider`, `FakeTimeProvider`, `System.Timers`, `System.Threading.Timer`, `DispatcherTimer`, and injected-clock constructs.
  - Acceptance: `.../evidence/qa-gates/ac11-no-timer-or-clock.<TS>.md` records `Timestamp:`, `Command:` (the grep command), `EXIT_CODE:`, and a zero-match `Output Summary:`, together with the statement that Correction C1 supersedes the `issue.md` lines 73-74 fake-timer expectation.
- [ ] [P12-T14] Verify AC5 determinism across every new or modified test file by searching for `Thread.Sleep`, `Task.Delay`, `.Wait()`, `.Result`, `DateTime.Now`, `DateTime.UtcNow`, and `Stopwatch`, and by confirming every test that touches `SynchronizationContext.Current` uses the disposable restore scope.
  - Acceptance: `.../evidence/qa-gates/ac5-determinism.<TS>.md` records `Timestamp:`, `Command:` for each grep command, `EXIT_CODE:` for each, a zero-match `Output Summary:` for the banned constructs, and the enumerated list of tests touching `SynchronizationContext.Current` each confirmed to use `SyncContextScope`.
- [ ] [P12-T15] Verify AC8 by confirming every characterization test carries an XML comment naming it as such and citing its promoted issue number.
  - Acceptance: `.../evidence/qa-gates/ac8-characterization-tests.<TS>.md` lists each characterization test by fully qualified name with its cited issue number (#444 or #445), and confirms that no latent defect L1-L6 was fixed and no observable QuickFiler keyboard flow changed.
- [ ] [P12-T16] Verify AC4 by measuring the line count of every production file in the F3 scope, the two new production files, and every new or appended test file — the ten `QuickFiler.Test/Controllers/KeyboardHandler.*Tests.cs` files, `QuickFiler.Test/Controllers/KeyboardHandler.TestSupport.cs`, `QuickFiler.Test/Controllers/KbdActionsConstructionAndEdgeTests.cs`, `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`, and `QuickFiler.Test/Controllers/MailItemActionsAdapterTests.cs`.
  - Acceptance: `.../evidence/qa-gates/ac4-file-sizes.<TS>.md` records `Timestamp:`, `Command:` (the line-count command over the enumerated file list), `EXIT_CODE:`, and an `Output Summary:` containing one numeric line count per file with `UNDER_500: true` asserted for each — production and test files alike, in a single table — and stating whether the `KeyboardHandler.cs` contingency split was applied.
- [ ] [P12-T17] Check off acceptance criteria AC1 through AC14 in both `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/spec.md` and `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/user-story.md`, each with a pointer to the evidence artifact that discharges it.
  - Acceptance: in both files, every criterion whose discharging evidence artifact exists and passes is changed from `- [ ]` to `- [x]` with the evidence path appended; every criterion not discharged remains `- [ ]` and is listed in `.../evidence/qa-gates/ac-status-summary.<TS>.md` with the specific gap. The two files agree criterion for criterion. The AC Status Summary reports `Total AC items: 14`, the checked count, and the unchecked count.
- [ ] [P12-T18] Record the final evidence index and confirm the working tree is clean with all audit-trail evidence committed.
  - Acceptance: `.../evidence/qa-gates/final-summary.<TS>.md` records `Timestamp:`, `Command:` (the `git status --porcelain` command), `EXIT_CODE:`, and an `Output Summary:` that links every evidence artifact produced by Phases 0 through 12 by path — explicitly including `.../evidence/qa-gates/ac1-ac2-gate.<TS>.md` from P12-T9, `.../evidence/qa-gates/ac6-scenario-completeness.<TS>.md` from P12-T10, and `.../evidence/qa-gates/ac-status-summary.<TS>.md` from P12-T17 — and records the F1 dependency dispositions D1-D5, the R1 and R2 dispositions, the L1-L6 issue map, and `PORCELAIN_CLEAN: true`.

## Test Plan

- **Unit:** 144 new MSTest `[TestMethod]` cases across Phases 1-7 — 73 for `KeyboardHandler.cs` (10 new test files plus 1 shared support file), 8 appended to `QfcFormKeyHandlerTests.cs`, 13 in the new `KbdActionsConstructionAndEdgeTests.cs`, 13 appended to `KaCharTests.cs`, 14 appended to `KaKeyTests.cs`, 15 appended to `KaStringAsyncTests.cs`, and 8 appended to `MailItemActionsAdapterTests.cs`. Zero cases for the four interface-only files, by design.
- **Integration:** none. No in-scope file has an external boundary; all tests are unit tests with Moq doubles and headless WinForms argument objects.
- **Manual/CLI:** none. No test constructs a live form, shows a popup, depends on the UI thread, uses a temporary file, or reaches an external service or a live Outlook process.
- **Coverage evidence:**
  - Baseline repository-wide Cobertura: `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/evidence/baseline/coverage-baseline.cobertura.xml`
  - Baseline per-file table: `.../evidence/baseline/perfile-coverage-baseline.<TS>.md`
  - Interim `KeyboardHandler.cs` per-file figure: `.../evidence/qa-gates/perfile-coverage-keyboardhandler.<TS>.md`
  - Final repository-wide Cobertura: `.../evidence/qa-gates/coverage-final.cobertura.xml`
  - Final per-file table: `.../evidence/qa-gates/perfile-coverage-final.<TS>.md`
  - Delta and threshold comparison: `.../evidence/qa-gates/coverage-delta.<TS>.md`
  - AC1/AC2 blocking floor gate: `.../evidence/qa-gates/ac1-ac2-gate.<TS>.md`
  - Repository-wide record-and-report: `.../evidence/qa-gates/repo-wide-coverage.<TS>.md`

## Open Questions / Notes

- **F1 ledger is the sole classification authority.** If F1 classifies `KeyboardHandler.cs` as `ratified-exempt` in whole, or classifies any of the four interface-only files as `testable` with an `>= 80%` target, halt and escalate to the epic orchestrator per AC13. Do not fabricate tests and do not self-grant an exemption.
- **The `>= 90%` new-code floor on `MyBoxDialogPrompt.cs` cannot be met by testing.** Its single statement forwards to the static `MyBox.ShowDialog`, and `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` does not grant `InternalsVisibleTo("QuickFiler.Test")`, so the `MyBox.DialogInvoker` stub is unreachable. The disposition is the P1-T2 ledger-ratification request. Adding `InternalsVisibleTo("QuickFiler.Test")` to `UtilitiesCS` is prohibited by AC9.
- **`KeyboardHandler.cs` line 189 is unreachable** and is recorded as a note, not chased with a contrived test.
- **`ClearFilter()`, `KeyboardHandler_PreviewKeyDown`, and `GetItemViewer` have no callers** anywhere in the repository. They are covered because they remain on the public and `internal` surface, and their removal is proposed as a follow-up issue rather than performed here, since deletion is a public-surface change.
- **`QuickFiler.Test/QuickFiler.Test.csproj` is a known merge hot spot** shared with F9, F10, and F11 in the same wave. The three grouped edits (P1-T5, P1-T21, P3-T1) are all appended adjacent to the existing block at lines 92-96 to keep the conflict hunk small and mechanically resolvable.
- **The three non-compiled viewer files** — `QfcFormViewerExpanded.cs`, `QfcFormViewerDark.cs`, and `EfcViewer3.cs` — appear in no `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj` and are outside the coverage denominator and outside the epic. The null-guard divergence recorded at `research/03-QfcFormKeyHandler.md` §8 R-3 therefore exists only in non-compiled code and requires no action from this child.
