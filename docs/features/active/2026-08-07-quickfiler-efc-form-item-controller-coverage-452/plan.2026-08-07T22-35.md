# quickfiler-efc-form-item-controller-coverage — Plan

- **Issue:** #452
- **Parent epic:** #136 `quickfiler-per-file-coverage` (child F9, wave 1, band C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T22-35
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** `full-feature` — acceptance criteria are authoritative in **both** `spec.md` and
  `user-story.md` (AC1-AC11, identical text). `issue.md` is context only.

## Required References

- Policy order (`policy-compliance-order`): `CLAUDE.md` → `.claude/rules/general-code-change.md` →
  `.claude/rules/general-unit-test.md` → `.claude/rules/csharp.md`
- Requirements: `<FEATURE>/spec.md` (DEC-1..DEC-5, C1..C10, CCN-1..CCN-5, AC1-AC11, Definition of
  Done), `<FEATURE>/user-story.md` (AC1-AC11, identical)
- Research (four per-file artifacts): `<FEATURE>/research/EfcItemController.research.md`,
  `<FEATURE>/research/EfcFormController.research.md`, `<FEATURE>/research/EfcViewer.research.md`,
  `<FEATURE>/research/EfcViewer.Designer-and-measurement.research.md`
- Epic: `docs/features/epics/quickfiler-per-file-coverage/epic.md`
- Structure model: `docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437/plan.2026-08-07T20-42.md`

**All work must comply with these policies; do not duplicate their content here.**

## Path Conventions

- `<FEATURE>` = `docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452`
- `<PROD>` = `QuickFiler/Controllers`
- `<VIEW>` = `QuickFiler/Viewers`
- `<IFACE>` = `QuickFiler/Interfaces`
- `<TEST>` = `QuickFiler.Test/Controllers`
- `<TESTV>` = `QuickFiler.Test/Viewers`
- `<LEDGER>` = `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`
- Evidence roots (non-overridable, per `evidence-and-timestamp-conventions`):
  `<FEATURE>/evidence/baseline/`, `<FEATURE>/evidence/qa-gates/`,
  `<FEATURE>/evidence/regression-testing/`, `<FEATURE>/evidence/other/`,
  `<FEATURE>/evidence/issue-updates/`. `artifacts/baselines/`, `artifacts/baseline/`,
  `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/coverage/`, `artifacts/evidence/` are rejected
  and fail preflight.
- Every command-bearing task records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in its
  named artifact. Baseline and final-QC coverage artifacts record **numeric** per-file line-rate and
  branch-rate; placeholders such as `UNVERIFIED` are invalid.
- Timestamps use `yyyy-MM-ddTHH-mm`.

## Toolchain Commands (DEC-3 corrected — bind these exact forms)

- **Format (mutating):** `dotnet tool run csharpier format .`
- **Format (non-mutating check):** `dotnet tool run csharpier check .`
- `CLAUDE.md` §C#1/§CUT3 state `csharpier .`; that is csharpier v0 syntax and fails against the
  pinned 1.2.6 (`dotnet-tools.json:5-11`, `rollForward: false`). `.vscode/tasks.json:53-66` is
  authoritative for the command **form** only; the `CLAUDE.md` toolchain **order** is unchanged.
- **Analyze:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- **Type-check:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- **Test with coverage:** `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <out.cobertura.xml>`
- **Scoped iteration run (no coverage):** `& (& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe") QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings /TestCaseFilter:"FullyQualifiedName~Efc"`
- `msbuild` and `vstest.console.exe` resolve via `vswhere`, **not** `PATH`. `dotnet-coverage` must be
  on `PATH`. `.dotnet-sdk/` is absent from this worktree; `global.json:2-11` pins SDK 8.0.205 and
  directs to `./scripts/vscode/Install-RepoDotNetSdk.ps1`. The tool manifest is `dotnet-tools.json`
  at the **repository root**, not `.config/dotnet-tools.json`.

## Standing Constraints (apply to every task in this plan)

- **Do NOT fix any promoted latent defect:** #459, #460, #461, #463, #464, #465, #466, #467
  (DEC-4). Characterization tests pin CURRENT behavior. Where an existing test pins current
  behavior, preserve that assertion verbatim.
- **Do NOT fix issue #439.** No test may assert that a multi-segment lineage appears. The eventual
  #439 fix point is `EfcFormController.cs:840-842`, which this plan relocates into the
  `BreadcrumbRouterFactory` default body; that relocation must be stated in the PR body.
- **In-scope hygiene** (deletion only, in files F9 already rewrites): the commented-out dead blocks
  at `EfcItemController.cs:452-533` and `:115-134`; `EfcFormController.cs:605-623`, `:147-148`,
  `:304-305`, `:311-312`, `:317-318`, `:323-324`, `:583-586`, `:735`, `:737`, `:827`, `:1002-1006`;
  `EfcViewer.cs:107-155`; and unused `using` directives at `EfcFormController.cs:4,7,8,10,20` and
  `EfcViewer.cs:3,4,6,7,8,9,15` (each verified with IDE0005 before removal).
- **Do NOT edit sibling-owned files:** `EfcHomeControllerDependencies.cs`,
  `EfcHomeControllerDependencyFactories.cs` (F8); `EfcThemeHelper.cs`, `EfcViewerQueue.cs` (F4);
  `BreadcrumbBridgeRouter.cs` (F12); `BreadcrumbOutboundQueue.cs` (F2); `WebView2BreadcrumbHost.cs`,
  `IBreadcrumbWebHost.cs`, `WebView2CoreInitializer.cs` (F13); `EfcDataModel.cs` (F5);
  `IItemViewer.cs`, `ItemViewer.cs` (F14); `KeyboardHandler.cs` (F3);
  `UtilitiesCS/Properties/AssemblyInfo.cs`; `coverage.config`;
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`; any shared `*.props`/`*.targets`;
  `epic.md`. Also do not touch `QuickFiler/Viewers/EfcViewer3.cs` or `EfcViewer3.Designer.cs`
  (absent from the csproj compile set — C5).
- **Popup prohibition — hard safety rule.** A test must never reach
  `EfcFormController.EditFiltersMenuItem_Click` (`:561-566`) with a real controller; `filters.Show()`
  opens a window. The S5 `ShowManageFiltersAction` seam is overridden in every test that can reach
  it, and `MessageBoxShowAction` in every test that can reach `:472-474`, `:710`, or `:756`.
- **Never invoke these defaults:** `EfcViewerQueue.Dequeue`, `EfcDataModel.CreateAsync`, the default
  `BreadcrumbHostFactory` body, `FileIO2.WriteTextFile`.
- **Delegate-identity assertion rule.** Classify each production default before asserting on it. A
  **named-method-group** default (for example `EfcThemeHelper.SetupThemes`) may be asserted with
  `.Method.Name`. A **lambda-valued** default (for example `f => Task.Run(f)`,
  `text => MessageBox.Show(text)`, the breadcrumb factories) has a compiler-generated method name and
  MUST be asserted with `NotBeNull()` plus `NotBeSameAs(sentinel)` — never `.Method.Name`.
- **Parallelization hazard.** `scripts/vscode/TaskMaster.cli.runsettings:4-7` sets
  `<Scope>ClassLevel</Scope>` with `<Workers>0</Workers>`, so test **classes** run in parallel. Any
  test class mutating a process-global static (`QuickFiler.Properties.Settings.Default`,
  `UiThread.Dispatcher`, any `Production*` delegate static) MUST carry `[DoNotParallelize]` and a
  `[TestCleanup]` restoring state. Precedent:
  `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11` — verified at planning time that
  `:11` is the `[DoNotParallelize]` attribute and `:12` is the `[TestClass]` attribute that follows it.
- **Determinism.** No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, unseeded randomness, real
  wall-clock waits, temporary files, external services, live Outlook store, shown forms, popups,
  message pumps, or `DoEvents`. Suspension points use `TaskCompletionSource` only. `UiSyncContext`
  is a plain `new SynchronizationContext()`. Under a ratified Approach A, `Thread.Join()` with no
  timeout is a synchronous handoff, not a wall-clock wait, and all constructed viewers are disposed
  in a `finally`.
- **Timer safety.** `EfcItemController.ToggleExpansion(ToggleState)` (`:862-905`) creates a real
  `System.Threading.Timer` with a 4,000 ms due time when the item is unread. Tests assert the field is
  non-null and dispose it; they never wait.
- **No `LiveOutlook` category.** `Invoke-MSTestWithCoverage.ps1:76` applies
  `/TestCaseFilter:TestCategory!=LiveOutlook` to every coverage run, so a `LiveOutlook` test
  contributes nothing to measured coverage. F9 marks none of its tests that way. `/InIsolation` is
  mandatory.
- **MSTest + Moq + FluentAssertions, Arrange-Act-Assert**, per `CLAUDE.md` §CUT1/§CUT2.
- **500-line ceiling applies to production AND test files.** Split test files with a `.Part2.cs`
  suffix if needed (precedent `QfcStreamingDequeueConfidenceGateTests.Part2.cs`).
  `EfcViewer.Designer.cs` is exempt as generated code (AC4, `epic.md:254-255`).
- **csproj mechanics.** `QuickFiler.csproj` and `QuickFiler.Test.csproj` are legacy non-SDK,
  CRLF-terminated, explicit `<Compile Include>` with no globbing, ordered **append-within-cluster**
  (NOT alphabetical). Use the `Edit` tool with an `old_string` copied verbatim from one or two
  adjacent existing lines, or `perl -0777` with explicit `\r\n`. **Never `sed -i`.** No property,
  reference, or ordering change; no formatter over `.csproj`. Insert new `Controllers\Efc*` entries as
  one contiguous block immediately after `QuickFiler.csproj:301`, strictly below F8's
  `EfcHomeController*` region at `:295-300`.
- **New-file obligations.** Every new production file takes the **>= 90%** line bar (`epic.md:583-585`),
  gets a `<Compile Include>` entry, and gets a `<LEDGER>` row appended **in the same change**
  (`epic.md:579-582`). Files in F1's `interface-only / not-measured` bucket report `N/A`, never `0%`,
  receive no `[ExcludeFromCodeCoverage]`, and are not subject to a percentage floor. Shape-assertion
  tests written purely to manufacture coverage for such a file are prohibited.
- **`[ExcludeFromCodeCoverage]` on any `EfcFormController.*.cs` or `EfcItemController.*.cs` partial is
  a Blocking finding** (`epic.md:220-225`). The split must not concentrate uncovered lines into a
  partial that is then exempted. Exactly one new file is proposed for `ratified-exempt`:
  `<VIEW>/EfcItemControlSurface.cs`.
- **Intra-F9 sequencing.** `EfcFormController` gains `IEfcExpansionStyleHost` and `IEfcViewerCommands`
  on its declaration in Phase 1, and `EfcViewer` implements `IEfcFormViewer` in Phase 2, before the
  Phase 3 and Phase 4 seam tasks — otherwise intermediate commits do not build.
- **net481 language level.** No `init` setters, no `record`, no `record struct` (no `IsExternalInit`
  polyfill; CS0518). `EfcItemViewerLayoutSnapshot` and `EfcUserSettings` are plain `readonly struct`
  types with positional constructors. Precedent: `ResourceTimingRow`.
- **Local test runs.** When running from the main checkout rather than this worktree, filter
  `\.claude\` paths out of any recursive `*.Test.dll` search so stale agent-worktree builds are not
  picked up.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Policy Reads, Blocking Gates, and Baseline Capture

Tasks P0-T4 through P0-T9 and P0-T14 are **halt gates**. On any failure, write the named artifact with
`EXIT_CODE: 1`, mark the plan BLOCKED, report blocked to the caller, and STOP. Do not improvise a
substitute harness, do not self-grant or self-revoke an exemption, and do not proceed to Phase 1.

- [ ] [P0-T1] Read the four policy documents in required order and record the read
  - Order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`
  - Acceptance: `<FEATURE>/evidence/baseline/phase0-instructions-read.md` exists containing `Timestamp:`, `Policy Order:`, and the explicit four-file list
- [ ] [P0-T2] Bootstrap the .NET SDK, the local tool manifest, and the NuGet package graph
  - Commands, in order, from the repository root: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Install-RepoDotNetSdk.ps1`; then `dotnet tool restore` (the manifest is `dotnet-tools.json` at root, not `.config/dotnet-tools.json`); then `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`
  - The third command is mandatory and is not optional on a fresh worktree. `packages/` is gitignored and absent from a newly created worktree; `QuickFiler.Test.csproj:439-447` declares `EnsureNuGetPackageBuildImports` with hard `<Error>` elements that fire in `PrepareForBuild`, and `msbuild /t:Build` does not restore `packages.config` projects. Without it every later `msbuild` and `vstest` task in this plan fails before compiling. `Invoke-Restore.ps1:36` runs `msbuild /t:Restore /p:RestorePackagesConfig=true`, which is the repo-standard form.
  - Acceptance: `<FEATURE>/evidence/baseline/toolchain-bootstrap.md` records `Timestamp:`, all three `Command:` values, `EXIT_CODE: 0` for each, and `Output Summary:` with the resolved SDK version (expect 8.0.205), the csharpier version (expect 1.2.6), and confirmation that `packages/` exists and contains `MSTest.TestFramework.4.3.3` and `System.ValueTuple.4.6.2`
- [ ] [P0-T3] Resolve and record the absolute paths of `msbuild`, `vstest.console.exe`, and `dotnet-coverage`
  - Commands: `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"`; `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`; `dotnet-coverage --version`
  - Rationale: neither msbuild nor vstest is on `PATH`; `Invoke-MSTestWithCoverage.ps1:292-294` throws if `dotnet-coverage` is absent
  - Acceptance: `<FEATURE>/evidence/baseline/tool-resolution.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` and the three resolved absolute paths, referenced by every later command task
- [ ] [P0-T4] HALT GATE G1-G3 — verify F1's ledger exists, covers all four F9 files, and states explicit attribute dispositions
  - G1: `<LEDGER>` exists and is non-empty; record its path and the commit sha that introduced it. G2: it carries a row for each of `QuickFiler/Controllers/EfcFormController.cs`, `QuickFiler/Controllers/EfcItemController.cs`, `QuickFiler/Viewers/EfcViewer.cs`, `QuickFiler/Viewers/EfcViewer.Designer.cs`, transcribed verbatim with line citations. G3: it states, for each of the three files carrying `[ExcludeFromCodeCoverage]` (verified at `EfcFormController.cs:27`, `EfcItemController.cs:25`, `EfcViewer.cs:20`), whether F9 removes the attribute or the exemption is ratified
  - Note: verified at planning time that `docs/features/epics/quickfiler-per-file-coverage/` contains only `epic.md` and that no `coverage-ledger.md` and no per-file harness exists anywhere in the repository. This gate is real and currently unmet.
  - Acceptance: `<FEATURE>/evidence/baseline/f1-ledger-gate-g1-g3.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, the four transcribed rows, and the three attribute dispositions
- [ ] [P0-T5] HALT GATE G4 — verify the ledger states the `EfcViewer.Designer.cs` bucket **and** the enforcement mechanism
  - The Designer file carries no attribute of its own; its current exclusion is a side effect of the type-level attribute on the `EfcViewer` partial at `EfcViewer.cs:20`. The ledger must state both the bucket and how that bucket is enforced once the type-level attribute is removed. This is the condition most likely to be missing from F1's first draft (DEC-5).
  - Acceptance: `<FEATURE>/evidence/baseline/f1-ledger-gate-g4.md` records `Timestamp:`, `EXIT_CODE:`, `Output Summary:`, the bucket token verbatim, and the stated enforcement mechanism
- [ ] [P0-T6] HALT GATE G5 — verify the ledger states classification **rules**, not only rows
  - Required by `epic.md:576-578`, because F9 creates production files during execution that post-date the ledger
  - Acceptance: `<FEATURE>/evidence/baseline/f1-ledger-gate-g5.md` records `Timestamp:`, `EXIT_CODE:`, `Output Summary:`, and the transcribed classification rules
- [ ] [P0-T7] HALT GATE G6 — verify F1's per-file coverage harness exists at its documented path and runs to completion
  - Command: F1's documented harness invocation against the committed Cobertura XML `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
  - Acceptance: `<FEATURE>/evidence/baseline/f1-harness-gate-g6.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and the harness's repo-relative script path
- [ ] [P0-T8] HALT GATE G7 — record F1's harness contract with four literal answers, and reject the issue-#441 defect axis (DEC-2)
  - Required answers: `AGGREGATION_BASIS: filename`; `LINE_SELECTION_AXIS:` must be the direct-child `class/lines/line` axis — the descendant `.//lines/line` axis is the #441 defect and is rejected; `DENOMINATOR_BASIS: line-node-count` (not `@line-rate`); `ZERO_OVER_ZERO_REPORTING: N/A` (not `0%`)
  - If the harness reads `class/@line-rate`, `class/@branch-rate`, `coverage/@lines-valid`, `coverage/@line-rate`, or uses the descendant axis, F9 must NOT consume its numbers: raise a defect against F1 using the exact text at `<FEATURE>/research/EfcViewer.Designer-and-measurement.research.md` §2.4, record dissent, and treat the gate as failed. Do not fabricate a local workaround producing a second inconsistent number.
  - Acceptance: `<FEATURE>/evidence/baseline/f1-harness-gate-g7.md` records `Timestamp:`, `EXIT_CODE:`, `Output Summary:`, the four literal answers, and (if applicable) the defect text and its issue number
- [ ] [P0-T9] HALT GATE G8 — verify the harness emits both a line rate and a branch rate per file
  - `epic.md:189-192` and `:500-502` make these independent gates; F8 found `EfcHomeController.Timing.cs` at 100% line / 66.67% branch, verifiable at `coverage-final.cobertura.xml:946`
  - Acceptance: `<FEATURE>/evidence/baseline/f1-harness-gate-g8.md` records `Timestamp:`, `EXIT_CODE:`, `Output Summary:`, and a sample two-rate row from the harness output
- [ ] [P0-T10] Independently recompute one known file's rates by hand arithmetic and confirm the harness agrees
  - Target: `QuickFiler\Controllers\FilerQueue.cs` in `coverage-final.cobertura.xml:18365-18480`. Expected hand-computed truth from the direct-child class-level `<lines>` (`:18412-18479`): 49 distinct lines, 18 with `hits > 0`, line rate 18/49 = 0.367347; branch 5/10 = 0.500000. The recorded `line-rate="0.405797"` / `branch-rate="0.428571"` are the #441-inflated values (28/69 and 6/14) and must NOT be reproduced by the harness.
  - Acceptance: `<FEATURE>/evidence/baseline/harness-arithmetic-crosscheck.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, both hand-computed rates, the harness's rates, and an explicit AGREE/DISAGREE verdict. A DISAGREE verdict escalates to the P0-T8 defect path and blocks the plan.
- [ ] [P0-T11] Record F1's literal bucket token for the third bucket and flag any divergence
  - `epic.md:520` says `interface-only / not-measured`; sibling plans have used `no-coverable-lines`, `interface-only`, and `no-executable-code` (`docs/features/active/2026-08-07-quickfiler-helper-classes-coverage-434/plan.2026-08-07T20-41.md:577,580,583`). F9 uses F1's literal token in every ledger row it appends.
  - Acceptance: `<FEATURE>/evidence/other/ledger-bucket-token.md` records `Timestamp:`, the three literal bucket tokens F1 uses, and whether the sibling divergence was reconciled by F1
- [ ] [P0-T12] Request the DEC-5 ledger-semantics clarification from F1 and record the answer
  - Ask F1 to either (a) state that `ratified-exempt` means "exempt from the per-file gate", explicitly decoupled from "carries `[ExcludeFromCodeCoverage]`", or (b) add a `generated / measured-not-gated` bucket. `EfcViewer.Designer.cs` has a ~0.50 branch rate by construction — `Dispose(bool)`'s `if (disposing && (components != null))` at `EfcViewer.Designer.cs:20` can only be exercised one way because `components` is initialized to `null` at `:12` and never reassigned. Classifying it `testable` makes AC2 unsatisfiable by construction. The same reasoning applies to the other seven `*.Designer.cs` files in the epic (F14, F15).
  - Acceptance: `<FEATURE>/evidence/other/dec5-ledger-clarification-request.md` records `Timestamp:`, the request text as sent, and F1's answer (or `PENDING` with the date requested)
- [ ] [P0-T13] Run the Approach A headless-construction spike
  - Construct one `new EfcViewer()` on a dedicated STA thread inside `try`/`finally`, never shown, disposed in the `finally`, following `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:16-53`. Assert no throw. Hazards in descending order: (1) `FolderListBox` WebView2 `BeginInit()`/`EndInit()` at `EfcViewer.Designer.cs:882,891` possibly triggering implicit CoreWebView2 initialization — unproven in this repository, the top spike item; (2) the nested `QuickFiler.ItemViewer` at `:4205-4216`; (3) five `SVGControl.ButtonSVG` with `SvgResource.Data` byte arrays at `:36-40,49-54,253-867`; (4) `ComponentResourceManager` loading `Viewers\EfcViewer.resx`; (5) `SetCompatibleTextRenderingDefault` ordering, already handled by `QuickFiler.Test/SetupAssemblyInitializer.cs:14-20`
  - The spike code is scratch: it must not be committed as a test file unless it becomes task P2-T33's `A1`
  - Acceptance: `<FEATURE>/evidence/other/approach-a-construction-spike.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, and a literal `SPIKE_RESULT: PASS` or `SPIKE_RESULT: FAIL` with the failing hazard named
- [ ] [P0-T14] HALT GATE DEC-1 — obtain maintainer ratification of the `EfcViewer` Form-construction approach
  - Present both branches with their evidence. **Approach A:** one real `EfcViewer` constructed on a dedicated STA thread, never shown, disposed in a `finally`, in `<TESTV>/EfcViewer.StaTests.cs`; projected ~100% line / ~100% branch on `EfcViewer.cs` and ~99% line on the Designer file, adding roughly 2,000 covered lines and materially helping AC9; no generated code edited. **Approach B:** no Form construction anywhere, `GetUninitializedObject` plus the S2 `ProcessCmdKeyBase` adapter seam, plus method-level `[ExcludeFromCodeCoverage]` on the Designer's `InitializeComponent` and `Dispose(bool)` (`EfcViewer.Designer.cs:18-25`); projected ~82% line / 100% branch on `EfcViewer.cs`, Designer file out of the denominator, ~2,000 lines forfeited, generated code edited (a durability defect — Visual Studio regenerates `InitializeComponent` and silently drops the attribute) with zero repo precedent (a grep for `ExcludeFromCodeCoverage` across `**/*.Designer.cs` returns no matches).
  - The conflict: `docs/features/epics/winforms-testability-refactor/epic.md:74` condition (d) reads "`Form`-derived types remain prohibited in tests even when unshown". The contrary evidence: `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:31`, `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:49,137,205,323`, `UtilitiesCS.Test/ReusableTypeClasses/ConfigViewer_Tests.cs:53,101,158`, `UtilitiesCS.Test/EmailIntelligence/FolderSelector_Tests.cs:44,68,93`, and `QuickFiler.Test/SetupAssemblyInitializer.cs:14-20`. The distinction the repository enforces is shown-versus-unshown, not `Form`-versus-`Control`.
  - If the maintainer does not ratify a branch, write the artifact with `EXIT_CODE: 1`, mark the plan BLOCKED, and STOP. Do not begin Phase 2 work on `EfcViewer.cs`.
  - Acceptance: `<FEATURE>/evidence/other/dec1-ratification.md` records `Timestamp:`, the spike result from task P0-T13, both options as presented, the maintainer's decision, and a literal `RATIFIED_APPROACH: A` or `RATIFIED_APPROACH: B`
- [ ] [P0-T15] Record the phase-scoped consequences of the ratified branch
  - Under `RATIFIED_APPROACH: A`, tasks P2-T31 through P2-T43 and P5-T2 are IN and tasks P2-T13, P2-T44 through P2-T47, and P5-T3 are OUT. Under `RATIFIED_APPROACH: B`, the reverse. Every other task in Phase 2 and Phase 5 is unconditional. Both branches share the S1 `IEfcViewerCommands` seam (tasks P1-T1 and P2-T3), the `IEfcFormViewer` implementation, and the N1-N15 normal-test list, so a reversal costs one phase and not a re-plan.
  - Acceptance: `<FEATURE>/evidence/other/dec1-task-scoping.md` lists every Phase 2 and Phase 5 task ID as IN or OUT under the ratified branch
- [ ] [P0-T16] Capture the baseline formatter state
  - Command: `dotnet tool run csharpier check .`
  - Acceptance: `<FEATURE>/evidence/baseline/csharpier-baseline.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with the count of files that would be reformatted
- [ ] [P0-T17] Capture the baseline analyzer build state
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: `<FEATURE>/evidence/baseline/msbuild-analyzers-baseline.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with warning and error counts
- [ ] [P0-T18] Capture the baseline nullable/type-check build state
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: `<FEATURE>/evidence/baseline/msbuild-nullable-baseline.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with the warning-as-error count, which is the budget task P3-T30 must not exceed
- [ ] [P0-T19] Capture the coverage-enabled baseline test run and the repository-wide baseline line rate
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-efc-form-item-controller-coverage-452\evidence\baseline\coverage-baseline.cobertura.xml`, then F1's per-file harness (path from task P0-T7) over that output
  - The repository-wide figure MUST be derived by the DEC-2 rule so the AC9 comparison is like-for-like. Do not carry forward the uncorrected 70.19% merge-base figure at `epic.md:479-481`.
  - Acceptance: `<FEATURE>/evidence/baseline/coverage-baseline.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with total pass/fail counts, the **numeric** repository-wide `LINE_COVERED / LINE_VALID` and computed rate, and the per-file rows for the four F9 files (expected `ABSENT` / `N/A`). No placeholders.
- [ ] [P0-T20] Record the negative-evidence finding that F9's four files are ABSENT, not covered
  - Enumerate every `filename` attribute in `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml` (70 distinct QuickFiler files) and confirm none of `QuickFiler\Controllers\EfcFormController.cs`, `QuickFiler\Controllers\EfcItemController.cs`, `QuickFiler\Viewers\EfcViewer.cs`, `QuickFiler\Viewers\EfcViewer.Designer.cs` appears. Positive control: `EfcHomeController.cs` appears at `:9`, proving the assembly and the `Controllers\` folder were instrumented. Note when counting: `TaskMaster\AppGlobals\AppQuickFilerSettings.cs` contains the substring "QuickFiler" but is not a QuickFiler-project file and is excluded from the 70.
  - Also record that the epic's baseline table at `epic.md:160-161` is unreliable per DEC-2 and that F9 cites no starting number for its four files
  - Acceptance: `<FEATURE>/evidence/baseline/absence-is-not-coverage.md` records `Timestamp:`, `SearchScope:`, `SearchPatterns:`, `SearchResult:`, `EXIT_CODE:`, and `Output Summary:` with the explicit statement "an absent file is not a covered file"
- [ ] [P0-T21] Record the file-size baseline for every in-scope production and test file
  - Files: `<PROD>/EfcItemController.cs` (expect 1,170), `<PROD>/EfcFormController.cs` (expect 1,086), `<VIEW>/EfcViewer.cs` (expect 162), `<VIEW>/EfcViewer.Designer.cs` (expect 4,277 — note `issue.md:31` and `epic.md:114,389` say 4,276; C10), and `<TEST>/EfcFormControllerTests.cs` (expect 55)
  - Acceptance: `<FEATURE>/evidence/baseline/file-size-baseline.md` lists each path with its measured line count and remaining headroom against 500, with `EfcViewer.Designer.cs` marked EXEMPT (generated)
- [ ] [P0-T22] Deliver the two correction notes to the epic orchestrator
  - Note 1 (C-DEC-2): `epic.md:161` lists `Controllers/FilerQueue.cs` at "69 lines, 40.6%"; the true class-level figures are 49 lines, 36.7% line and 50.0% branch. Note 2 (C6): `epic.md:223` and `:324` still say "33 existing `[ExcludeFromCodeCoverage]` attributes" after the marker-accuracy note at `:121-130` corrected the figure to 21 compiled files carrying a real attribute. F9 does not edit `epic.md`.
  - Acceptance: `<FEATURE>/evidence/other/epic-correction-notes.md` records `Timestamp:`, both note texts as delivered, and the delivery channel
- [ ] [P0-T23] Transcribe the verbatim-migration contract for the one existing EFC test
  - Read `<TEST>/EfcFormControllerTests.cs` (55 lines) and transcribe both `CreateMinimalController()` (`:18-28`, reflection-invoking the private no-arg constructor at `EfcFormController.cs:79`) and `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel` (`:34-53`, the issue-#145 regression test pinning the `_formViewer == null` early return at `EfcFormController.cs:1029-1031`) into the artifact byte-for-byte
  - This test is part of the spec (`CLAUDE.md` §7.3) and MUST be migrated verbatim by task P3-T139
  - Acceptance: `<FEATURE>/evidence/baseline/existing-test-migration-contract.md` contains both transcriptions and the statement that no assertion text may change
- [ ] [P0-T24] Record the standing no-fix register for latent defects
  - #459, #460, #461, #463, #464, #465, #466, #467 (DEC-4) and open issue #439. Record, for each, the members F9 touches that lie on its path, so an execution-time reviewer can confirm characterization rather than correction.
  - Acceptance: `<FEATURE>/evidence/other/latent-defect-no-fix-register.md` records `Timestamp:`, the nine issue numbers, and the touched-member map

### Phase 1 — Shared Seam Contracts and Host-Neutral Modules

This phase creates the cross-file contracts that Phases 2, 3, and 4 consume, so no later phase leaves
a seam declared in one partial and consumed in another across a phase boundary.

- [ ] [P1-T1] Create `<IFACE>/IEfcViewerCommands.cs`
  - `public interface IEfcViewerCommands { void EditFiltersMenuItem_Click(object sender, EventArgs e); }`. Carries no WinForms type beyond `EventArgs`, per the host-neutrality Non-Goal.
  - Acceptance: the file exists, is under 500 lines, and compiles
- [ ] [P1-T2] Create `<IFACE>/IEfcExpansionStyleHost.cs`
  - `internal interface IEfcExpansionStyleHost { void ToggleExpansionStyle(UtilitiesCS.Enums.ToggleState desiredState); }` — one member, matching the existing `EfcFormController.cs:1056` signature exactly
  - Acceptance: the file exists and compiles
- [ ] [P1-T3] Create `<IFACE>/IEfcFormViewer.cs`
  - `public interface IEfcFormViewer : UtilitiesCS.Interfaces.IWinForm.IForm`. Declare only the intent members listed in `<FEATURE>/research/EfcFormController.research.md` §4 S1: `UiSyncContext`, `KeyboardHandler`, `TipsLabels`, `ItemViewer`, `ItemTableLayout`, `SearchTextControl`, `FolderListControl`, `SearchTextValue`, `SearchTextChanged`, `SearchTextKeyDown`, `FocusFolderList()`, `OkButtonText`, `NewFolderButtonText`, the five click events (`OkClicked`, `CancelClicked`, `RefreshClicked`, `NewFolderClicked`, `DeleteClicked`), the four `*Checked` property/`*Changed` event pairs, `EditFiltersClicked`, `ShowMoveOptionsMenu()`, `WireKeyHandlers(PreviewKeyDownEventHandler, KeyEventHandler)`, `GetChildControlsExcept(IList<Control>)`, `CaptureItemViewerLayout()`, `ApplyItemViewerLayout(float)`, `SetItemViewerRowHeight(int, float)`, `SetMinimumAndSize(Size, Size)`, `BreadcrumbWebView`
  - **Do NOT redeclare** `Handle`, `Dispose()`, `Close()`, `Hide()`, `Select()`, `MinimumSize`, `Size`, `Text`, `WindowState`, `Invoke`, `BeginInvoke` — all come free from `IForm`/`IControl`
  - Acceptance: the file exists, compiles, and contains no member already supplied by `IForm`/`IControl`
- [ ] [P1-T4] Create `<PROD>/EfcFormLayoutMath.cs`
  - Contains `internal readonly struct EfcItemViewerLayoutSnapshot` (positional ctor: `TlpExpandedHeight`, `ItemViewerHeight`, `ItemViewerMinHeight`, `ItemViewerTlpRow`, `FirstFiveRowHeights`, `BodyRowHeight`), `internal readonly struct EfcUserSettings` (positional ctor: `SaveAttachments`, `SaveEmail`, `SavePictures`, `MoveConversation`), and `internal static class EfcFormLayoutMath` with `ComputeTlpHeights`, `ComputeBodyRowHeight`, `ComputeMinimumFormSize`, `ExpandForToggle`, `CollapseForToggle` per research §4 S8
  - net481: plain `readonly struct` with positional constructors — no `init`, no `record`, no `record struct`
  - Acceptance: the file exists, is under 500 lines, compiles, and contains no WinForms control access
- [ ] [P1-T5] Wire the four new production files into `QuickFiler/QuickFiler.csproj`
  - `<Compile Include="Controllers\EfcFormLayoutMath.cs" />` as a self-closing entry immediately after `QuickFiler.csproj:301`; the three `Interfaces\` entries adjacent to the existing `Interfaces\` block. CRLF preserved via the `Edit` tool; no `sed -i`, no reordering, no property or reference change.
  - Acceptance: all four entries exist, `git diff --stat QuickFiler/QuickFiler.csproj` shows only added lines, and the file's line endings are unchanged
- [ ] [P1-T6] Append `<LEDGER>` rows for the four new production files in the same change as task P1-T5
  - `EfcFormLayoutMath.cs`: bucket `testable`, line target `>= 90%` (target 100%), branch target `>= 75%`, owner F9 (#452), attribute `none`. The three interface files: F1's literal third-bucket token (from task P0-T11), reported `N/A`, no percentage floor, attribute `none`.
  - Acceptance: four rows exist in `<LEDGER>` using F1's literal bucket tokens and column layout
- [ ] [P1-T7] Add `IEfcExpansionStyleHost` and `IEfcViewerCommands` to the `EfcFormController` base list
  - `<PROD>/EfcFormController.cs:28` becomes `internal class EfcFormController : IFilerFormController, IEfcExpansionStyleHost, IEfcViewerCommands`. No new member is required: `ToggleExpansionStyle(Enums.ToggleState)` exists at `:1056` and `EditFiltersMenuItem_Click(object, EventArgs)` exists at `:561` and is already `public`.
  - Acceptance: the declaration compiles with zero added members and zero behavior change
- [ ] [P1-T8] Create `<TEST>/EfcFormLayoutMathTests.cs` shell
  - `[TestClass]`, no static mutation, therefore no `[DoNotParallelize]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P1-T9] Wire `<TEST>/EfcFormLayoutMathTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcFormLayoutMathTests.cs" />` entry exists with CRLF preserved
- [ ] [P1-T10] Add `ComputeTlpHeights_ReturnsExpandedCollapsedAndDifference` (research 6.9 #117, P)
  - Table-driven; assert the returned `(expanded, collapsed, diff)` triple against hand-computed values from `EfcFormController.cs:169-172`
- [ ] [P1-T11] Add `ComputeTlpHeights_WhenItemViewerAtMinimum_ProducesZeroDifference` (#118, E)
  - `itemViewerHeight == itemViewerMinHeight`; assert `diff == 0`
- [ ] [P1-T12] Add `ComputeBodyRowHeight_SubtractsSumOfFirstFiveRowsAndAddsBodyRow` (#119, P)
  - Assert against a hand-computed sum over a five-element `float[]`
- [ ] [P1-T13] Add `ComputeBodyRowHeight_WhenRowHeightsEmpty_ReturnsCollapsedHeight` (#120, E)
  - Empty `IReadOnlyList<float>`; assert the result equals the collapsed height
- [ ] [P1-T14] Add `ComputeBodyRowHeight_WhenSumExceedsCollapsed_ReturnsNegative` (#121, E)
  - Assert a negative result is returned rather than clamped — pins current arithmetic
- [ ] [P1-T15] Add `ComputeMinimumFormSize_ScalesExplorerSizeBySeventyFivePercent` (#122, P)
  - Assert the 0.75 factor from `EfcFormController.cs:182-185` against hand-computed integers; state the rounding mode used
- [ ] [P1-T16] Add `ComputeMinimumFormSize_WhenExplorerSizeZero_ReturnsZero` (#123, E)
  - `Size.Empty` input; assert `Size.Empty` output
- [ ] [P1-T17] Add `ExpandForToggle_AddsDifferenceToBothMinimumAndSize` (#124, P)
  - Assert both returned `Size` values increased by `diff` in the height component only
- [ ] [P1-T18] Add `CollapseForToggle_SubtractsDifferenceFromBothMinimumAndSize` (#125, P)
  - Mirror of task P1-T17
- [ ] [P1-T19] Add `ExpandThenCollapseForToggle_IsIdentity` (#126, S)
  - Round-trip assertion proving the two operations are exact inverses for the same `diff`
- [ ] [P1-T20] Add `EfcItemViewerLayoutSnapshot_PositionalConstructor_RoundTripsEveryComponent` (P)
  - Construct with six distinct values and assert each property returns its own value — closes the struct's lines in `EfcFormLayoutMath.cs` toward the >= 90% new-file floor
- [ ] [P1-T21] Add `EfcUserSettings_PositionalConstructor_RoundTripsFourFlags` (P)
  - Construct with `true, false, true, false` and assert each flag independently
- [ ] [P1-T22] Verify Phase 1 compiles, runs green, and respects the file-size ceiling
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` then the scoped vstest run with `/TestCaseFilter:"FullyQualifiedName~EfcFormLayoutMath"`
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase1-scoped-run.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with pass/fail counts and the line count of every file created or touched in this phase (all < 500)

### Phase 2 — EfcViewer.cs Seam, Attribute Removal, and Coverage

Tasks P2-T31 through P2-T43 execute only under `RATIFIED_APPROACH: A`; tasks P2-T13 and P2-T44
through P2-T47 execute only under `RATIFIED_APPROACH: B`. The branch is fixed by task P0-T14 and
scoped by task P0-T15. All other tasks in this phase are unconditional.

Branch-outcome enumeration for the file's single branching line `EfcViewer.cs:96`
(`(_keyboardHandler is not null) && (keyData.HasFlag(Keys.Alt))`, two conditions / four outcomes):
condition 1 true and condition 2 true are closed by N13/N15; condition 1 false is closed by A4 (or its
Approach B equivalent); condition 2 false is closed by A5 (or its Approach B equivalent). All four
outcomes must be closed for AC2.

- [ ] [P2-T1] Remove `[ExcludeFromCodeCoverage]` from `<VIEW>/EfcViewer.cs:20` (AC3)
  - This simultaneously un-suppresses `<VIEW>/EfcViewer.Designer.cs` (4,277 lines), because the attribute at `:20` targets the partial **type** declared at `:21`, and C# merges attributes across all partial declarations onto the single emitted type. Phase 5 handles the Designer disposition.
  - Acceptance: no `ExcludeFromCodeCoverage` occurrence remains in `EfcViewer.cs` and the solution builds
- [ ] [P2-T2] Add the `IEfcFormViewer` `ItemViewer` and `ItemTableLayout` properties to `<VIEW>/EfcViewer.cs`
  - `ItemViewer` → the designer field at `EfcViewer.Designer.cs:4262`; `ItemTableLayout` → `L0vh_TLP` at `:4261`
  - Acceptance: both properties exist as 1:1 forwards and the solution builds
- [ ] [P2-T3] Retype the `_formController` field and the `SetController` parameter to `IEfcViewerCommands` (S1)
  - `<VIEW>/EfcViewer.cs:48` `private EfcFormController _formController;` → `private IEfcViewerCommands _formController;`; `:50-53` `SetController(EfcFormController)` → `SetController(IEfcViewerCommands)`
  - Call-site impact is zero: `SetController` has no callers anywhere in the compiled tree (`QfcFormController.cs:44` and `EfcViewer3.cs:39` are a different viewer type and a non-compiled file respectively)
  - Justification independent of DEC-1: the real `EfcFormController.EditFiltersMenuItem_Click` (`:561-566`) constructs and `Show()`s a `TaskVisualization.ManageFilters` window, so invoking `EfcViewer.EditFiltersMenuItem_Click` with the concrete type in a test is a direct AC6 violation
  - Acceptance: the field and parameter types are `IEfcViewerCommands`, and no `EfcFormController` type reference remains in `EfcViewer.cs`
- [ ] [P2-T4] Delete the commented-out dead block at `<VIEW>/EfcViewer.cs:107-155`
  - 49 lines, of which `:121-137` and `:139-155` are byte-identical duplicates of each other. Deletion only; no behavior change.
  - Acceptance: the block is gone and the file's line count drops by 49 from the task P0-T21 baseline
- [ ] [P2-T5] Remove the unused `using` directives in `<VIEW>/EfcViewer.cs`
  - Candidates `:3,4,6,7,8,9,15`; verify each with IDE0005 before removing. Do NOT remove the `log4net.ILog log` field at `:32-34` — it is a member covered by promoted defect #466 and out of scope for F9.
  - Acceptance: the analyzer build reports zero IDE0005 diagnostics for this file and no member was deleted
- [ ] [P2-T6] Add the `IEfcFormViewer` control-accessor members to `<VIEW>/EfcViewer.cs`
  - `SearchTextControl` → `SearchText` (`EfcViewer.Designer.cs:4246`), `FolderListControl` → `FolderListBox` (`:4250`), `SearchTextValue` → `SearchText.Text`, `FocusFolderList()` → `FolderListBox.Select()`, `OkButtonText` set → `Ok.Text`, `NewFolderButtonText` set → `NewFolder.Text`, `ShowMoveOptionsMenu()` → `MoveOptionsMenu.ShowDropDown()`. Each is a 1:1 forward with no logic.
  - Acceptance: all seven members exist as 1:1 forwards and contain no branching
- [ ] [P2-T7] Add the `IEfcFormViewer` intent events to `<VIEW>/EfcViewer.cs`
  - `SearchTextChanged` → `SearchText.TextChanged`, `SearchTextKeyDown` → `SearchText.KeyDown`, `OkClicked`/`CancelClicked`/`RefreshClicked`/`NewFolderClicked`/`DeleteClicked` → `Ok`/`Cancel`/`RefreshPredicted`/`NewFolder`/`BtnDelItem` `.Click`, `EditFiltersClicked` → `EditFiltersMenuItem.Click`, each as an `add`/`remove` accessor pair forwarding to the designer control's event
  - Acceptance: all eight events exist as forwarding accessor pairs
- [ ] [P2-T8] Add the four `IEfcFormViewer` menu-checked property/event pairs to `<VIEW>/EfcViewer.cs`
  - `SaveAttachmentsChecked`/`SaveAttachmentsChanged`, `SaveEmailChecked`/`SaveEmailChanged`, `SavePicturesChecked`/`SavePicturesChanged`, `MoveConversationChecked`/`MoveConversationChanged`, each forwarding to the corresponding `*MenuItem.Checked` and `.CheckedChanged`
  - Acceptance: all four pairs exist as 1:1 forwards
- [ ] [P2-T9] Add the `IEfcFormViewer` layout intent members to `<VIEW>/EfcViewer.cs`
  - `CaptureItemViewerLayout()` returns an `EfcItemViewerLayoutSnapshot` built from `L0vh_TLP.RowStyles[1].Height`, `ItemViewer.Height`, `ItemViewer.MinimumSize.Height`, `L0vh_TLP.GetPositionFromControl(ItemViewer).Row`, and the first five `ItemViewer.L0vh_Tlp.RowStyles` heights; `ApplyItemViewerLayout(float)` writes the body row height; `SetItemViewerRowHeight(int, float)` writes one row style height; `SetMinimumAndSize(Size, Size)` writes `MinimumSize` and `Size`. All arithmetic stays in `EfcFormLayoutMath`; these members read and write only.
  - Rationale for placing the layout reads here rather than retyping `_itemViewer` as `IItemViewer`: `IItemViewer` (`QuickFiler/Viewers/IItemViewer.cs:15-132`) declares `Height` (`:128`) but not `MinimumSize` and not `L0vh_Tlp` (CCN-5). No F14 edit.
  - Acceptance: the four members exist, contain no arithmetic, and compile
- [ ] [P2-T10] Add the `IEfcFormViewer` bulk-wiring members to `<VIEW>/EfcViewer.cs`
  - `WireKeyHandlers(PreviewKeyDownEventHandler, KeyEventHandler)` encapsulating the `ForAllControls(...)` call currently at `EfcFormController.cs:375-386`; `GetChildControlsExcept(IList<Control> except)` encapsulating the `GetAllChildren(except:)` extension currently at `EfcFormController.cs:215` (`UtilitiesCS/Extensions/WinFormsExtensions.cs:160`)
  - Acceptance: both members exist and the extension-method call sites move out of the controller
- [ ] [P2-T11] Add `IEfcFormViewer` to the `EfcViewer` base list
  - `<VIEW>/EfcViewer.cs:21` becomes `public partial class EfcViewer : Form, IEfcFormViewer`. Every intent member is already present from tasks P2-T2 and P2-T6 through P2-T10, so this task compiles standalone and leaves no non-building intermediate state. `Handle`, `Dispose()`, `Close()`, `Hide()`, `Select()`, `MinimumSize`, `Size`, `Text`, `WindowState`, `Invoke`, and `BeginInvoke` are supplied by `Form`'s inherited public members as implicit implementations of `IForm`/`IControl`.
  - Acceptance: the declaration compiles with zero further members added, and the solution builds
- [ ] [P2-T12] Verify `<VIEW>/EfcViewer.cs` compiles, implements `IEfcFormViewer` completely, and stays under 500 lines
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase2-viewer-build.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and the file's line count (projected ~330, must be < 500) alongside its task P0-T21 baseline of 162
- [ ] [P2-T13] [Approach B only] Add the `ProcessCmdKeyBase` virtual seam to `<VIEW>/EfcViewer.cs` (S2)
  - `protected virtual bool ProcessCmdKeyBase(ref Message msg, Keys keyData) => base.ProcessCmdKey(ref msg, keyData);` with a comment stating it exists so a test double can substitute the `Form` base implementation, which cannot run on an instance allocated without a constructor. `ProcessCmdKey`'s final line becomes `return ProcessCmdKeyBase(ref msg, keyData);`.
  - Cost: one permanently uncovered production line. Benefit: the two false-branch outcomes at `:96` become reachable, taking branch coverage from ~50% to 100%.
  - Acceptance: the seam exists, `ProcessCmdKey` routes through it, and the solution builds
- [ ] [P2-T14] Create `<TESTV>/EfcViewerTests.cs` shell
  - Plain `[TestClass]`, no static mutation, therefore no `[DoNotParallelize]`. Include the shared arrange helper `private static EfcViewer NewHeadless() => (EfcViewer)FormatterServices.GetUninitializedObject(typeof(EfcViewer));`. Instances are never disposed — there is no initialized base state to dispose, the same caveat `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:26-28` documents. `QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`, so `internal` members need no reflection; only `private` members do.
  - Acceptance: the shell compiles with zero test methods and constructs no `Form`
- [ ] [P2-T15] Wire `<TESTV>/EfcViewerTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Viewers\EfcViewerTests.cs" />` entry exists with CRLF preserved
- [ ] [P2-T16] Add `UiSyncContext_ReturnsCapturedContextInstance` (N1, P)
  - Reflection-assign `_context` on a headless instance; assert `BeSameAs` the assigned instance
- [ ] [P2-T17] Add `UiScheduler_ReturnsCapturedSchedulerInstance` (N2, P)
  - Reflection-assign `_uiScheduler`; assert `BeSameAs`
- [ ] [P2-T18] Add `SetController_StoresSuppliedCommandsInstance` (N3, S)
  - `Mock<IEfcViewerCommands>`; call `SetController`; reflection-read `_formController`; assert `BeSameAs`
- [ ] [P2-T19] Add `SetController_WithNull_ClearsStoredCommands` (N4, N)
  - Set a non-null commands instance first, then `SetController(null)`; assert `_formController` is null
- [ ] [P2-T20] Add `EditFiltersMenuItem_Click_ForwardsSenderAndArgsToController` (N5, P)
  - `Mock<IEfcViewerCommands>`; invoke the private handler by reflection with a distinct sender and `EventArgs`; assert `Verify(c => c.EditFiltersMenuItem_Click(sender, e), Times.Once)` on the exact instances. Never uses a real `EfcFormController` — that would open a window.
- [ ] [P2-T21] Add `EditFiltersMenuItem_Click_WhenControllerNeverSet_Throws` (N6, Err — characterization of #466)
  - Leave `_formController` null; assert the reflection invoke surfaces a `NullReferenceException` (unwrap `TargetInvocationException.InnerException`). Pins today's defect; does not fix it.
- [ ] [P2-T22] Add `SetKeyboardHandler_ThenKeyboardHandler_ReturnsSameInstance` (N7, P)
  - `Mock<IQfcKeyboardHandler>`; round-trip through `SetKeyboardHandler` and the `KeyboardHandler` getter
- [ ] [P2-T23] Add `SetKeyboardHandler_WithNull_ClearsHandler` (N8, N)
  - Set then clear; assert the getter returns null
- [ ] [P2-T24] Add `TipsLabels_BeforeInitialization_ReturnsNull` (N9, E)
  - Fresh headless instance; assert `TipsLabels` is null before `InitTipsLabelsList` runs
- [ ] [P2-T25] Add `InitTipsLabelsList_PopulatesNineLabelsInDesignerOrder` (N10, P)
  - Reflection-assign the nine designer `Label` fields to nine distinct `GetUninitializedObject(typeof(Label))` sentinels; invoke the private method by reflection; assert `TipsLabels` equals exactly `[LblAcSearch, LblAcFolderList, LblAcTrash, LblAcEmail, LblAcFilters, LblAcOk, LblAcCancel, LblAcRefresh, LblAcNewFolder]` by reference and in that order
- [ ] [P2-T26] Add `InitTipsLabelsList_WhenInvokedTwice_ReplacesPreviousList` (N11, S)
  - Invoke twice; assert the second list is a new instance and still contains the nine sentinels in order
- [ ] [P2-T27] Add `BreadcrumbWebView_ReturnsDesignerFolderListBoxInstance` (N12, P — #439 characterization)
  - Reflection-assign `FolderListBox` to a `GetUninitializedObject(typeof(WebView2))` sentinel; assert `BreadcrumbWebView` is reference-equal to it. Documents that this member performs no lineage or segment transformation. **Do not assert that a multi-segment lineage appears.**
- [ ] [P2-T28] Add `ProcessCmdKey_WithHandlerAndAltModifier_InvokesToggleAndReturnsTrue` (N13, P)
  - `Mock<IQfcKeyboardHandler>`, `var msg = new Message { HWnd = IntPtr.Zero }`, `keyData = Keys.Alt | Keys.F`. Reach the method via `typeof(EfcViewer).GetMethod("ProcessCmdKey", BindingFlags.NonPublic | BindingFlags.Instance)` and `Invoke` with a boxed args array (the `ref Message` parameter requires the by-ref invoke form). Assert `true` and `Verify(h => h.ToggleKeyboardDialogAsync(It.IsAny<object>(), It.Is<KeyEventArgs>(a => a.KeyData == (Keys.Alt | Keys.F))), Times.Once)`. Closes condition-1-true and condition-2-true.
- [ ] [P2-T29] Add `ProcessCmdKey_WithZeroWindowHandle_PassesNullSenderToHandler` (N14, E)
  - Same arrangement with `HWnd = IntPtr.Zero`; assert the captured sender is null, characterizing that `Control.FromHandle(IntPtr.Zero)` yields null
- [ ] [P2-T30] Add `ProcessCmdKey_WithAltOnlyKeyData_StillInvokesHandler` (N15, E)
  - `keyData == Keys.Alt` exactly; assert the handler is invoked and `true` is returned. Boundary case for `HasFlag`.
- [ ] [P2-T31] [Approach A only] Create `<TESTV>/EfcViewer.StaTests.cs` shell
  - `[STATestClass]` (from `Microsoft.VisualStudio.TestTools.UnitTesting`, shipped in MSTest.TestFramework 4.3.3 pinned at `QuickFiler.Test/packages.config:119` — no new package) plus a shared `RunWithViewer(Action<EfcViewer>)` helper copied in shape from `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:16-53`: `new Thread(...)`, `SetApartmentState(ApartmentState.STA)`, `Start()`, `Join()` with no timeout; inside the thread capture and replace `SynchronizationContext.Current` with `new SynchronizationContext()`, construct the viewer, run the action; `finally` disposes the viewer and restores the previous context; marshal exceptions back with `ExceptionDispatchInfo.Capture(...).Throw()`. No `Show()`, no `ShowDialog()`, no `DoEvents`, no timer, no sleep, no message pump.
  - This is the first `*.StaTests.cs` file in `QuickFiler.Test` (C3). At most one `EfcViewer` is constructed per test and it is always disposed in the `finally`.
  - Acceptance: the shell compiles with zero test methods and carries `[STATestClass]`
- [ ] [P2-T32] [Approach A only] Wire `<TESTV>/EfcViewer.StaTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Viewers\EfcViewer.StaTests.cs" />` entry exists with CRLF preserved
- [ ] [P2-T33] [Approach A only] Add `Constructor_OnStaThread_CapturesSynchronizationContextAndScheduler` (A1, P)
  - Assert `UiSyncContext` and `UiScheduler` are both non-null after construction. XML doc comment states why no seam applies: a constructor cannot be executed without constructing the object, and `TaskScheduler.FromCurrentSynchronizationContext()` throws unless a `SynchronizationContext` is installed (AC7).
- [ ] [P2-T34] [Approach A only] Add `Constructor_OnStaThread_PopulatesNineNonNullTipsLabels` (A2, P)
  - Assert `TipsLabels` has exactly nine non-null entries. XML doc comment: only a real construction proves the designer fields are non-null at constructor exit.
- [ ] [P2-T35] [Approach A only] Add `Constructor_OnStaThread_BreadcrumbWebViewIsTheDesignerFolderListBox` (A3, P — #439 characterization)
  - Assert `BreadcrumbWebView` is reference-equal to the designer `FolderListBox` field read by reflection. **Do not assert a multi-segment lineage.**
- [ ] [P2-T36] [Approach A only] Add `ProcessCmdKey_WithNoKeyboardHandler_DefersToBaseAndReturnsFalse` (A4, N)
  - `_keyboardHandler` left null; assert `false` and that no handler call occurred. Closes condition-1-false and line `:104`. XML doc comment: `base.ProcessCmdKey` dereferences `Control.Properties`, allocated only by `Control`'s constructor.
- [ ] [P2-T37] [Approach A only] Add `ProcessCmdKey_WithHandlerButNoAltModifier_DoesNotInvokeHandlerAndReturnsFalse` (A5, N)
  - `Mock<IQfcKeyboardHandler>` set, `keyData = Keys.F`; assert `false`, `Verify(..., Times.Never)`. Closes condition-2-false. Same XML doc rationale.
- [ ] [P2-T38] [Approach A only] Add `Dispose_AfterConstruction_DoesNotThrow` (A6, Err)
  - Construct, dispose inside the STA worker, assert no throw. Exercises the generated `Dispose(bool)` true path at `EfcViewer.Designer.cs:18-25`.
- [ ] [P2-T39] [Approach A only] Add `IntentAccessors_OnConstructedViewer_ReturnTheDesignerControlInstances` (P)
  - On the constructed viewer assert `SearchTextControl`, `FolderListControl`, `ItemViewer`, `ItemTableLayout`, and `BreadcrumbWebView` are each reference-equal to their designer fields, and that `SearchTextValue` equals `SearchText.Text`. Covers the task P2-T2 and P2-T6 forwards.
- [ ] [P2-T40] [Approach A only] Add `IntentEvents_OnConstructedViewer_SubscribeAndUnsubscribeWithoutThrowing` (S)
  - Subscribe then unsubscribe a no-op handler on each of the eight events from task P2-T7; assert no throw and that the designer control's invocation list returns to its prior length. Covers both accessor arms of each event.
- [ ] [P2-T41] [Approach A only] Add `MenuCheckedIntentProperties_OnConstructedViewer_RoundTripEachOfTheFourFlags` (S)
  - Set each of the four `*Checked` properties to `true` then `false`, asserting the underlying `*MenuItem.Checked` follows and that the corresponding `*Changed` event fires. Covers the task P2-T8 forwards.
- [ ] [P2-T42] [Approach A only] Add `LayoutIntentMembers_OnConstructedViewer_CaptureAndApplyWithoutThrowing` (P)
  - Call `CaptureItemViewerLayout()` and assert the returned snapshot's six components match direct designer reads; then call `ApplyItemViewerLayout`, `SetItemViewerRowHeight`, and `SetMinimumAndSize` and assert the designer state changed accordingly. Covers the task P2-T9 forwards.
- [ ] [P2-T43] [Approach A only] Add `WireKeyHandlersAndGetChildControlsExcept_OnConstructedViewer_ReachEveryEligibleChild` (P)
  - Call `WireKeyHandlers` with recording handlers and assert at least one child received both subscriptions; call `GetChildControlsExcept` with a one-element exclusion list and assert the excluded control is absent from the result. Covers the task P2-T10 forwards.
- [ ] [P2-T44] [Approach B only] Add the `EfcViewerProcessCmdKeyDouble` test double to `<TESTV>/EfcViewerTests.cs`
  - `private sealed class EfcViewerProcessCmdKeyDouble : EfcViewer` overriding `ProcessCmdKeyBase` to return a test-controlled value and record its arguments; instances are obtained with `FormatterServices.GetUninitializedObject`, never constructed
  - Acceptance: the double compiles and constructs no `Form`
- [ ] [P2-T45] [Approach B only] Add `ProcessCmdKey_WithNoKeyboardHandler_DefersToSeamAndReturnsSeamResult` (A4 equivalent, N)
  - Uninitialized double, `_keyboardHandler` null, seam returns `false`; assert `false` and that the seam was invoked exactly once. Closes condition-1-false and the `:104` route.
- [ ] [P2-T46] [Approach B only] Add `ProcessCmdKey_WithHandlerButNoAltModifier_DefersToSeamAndDoesNotInvokeHandler` (A5 equivalent, N)
  - `Mock<IQfcKeyboardHandler>` set, `keyData = Keys.F`, seam returns `false`; assert `false`, seam invoked once, handler `Times.Never`. Closes condition-2-false.
- [ ] [P2-T47] [Approach B only] Resolve the `IEfcFormViewer` forward-member coverage exposure and halt if the floor cannot be met
  - Under Approach B no constructed viewer exists, so every setter-shaped forward added by tasks P2-T2 and P2-T6 through P2-T10 that writes a designer control property is unreachable on an uninitialized instance. Cover every getter-shaped forward with reflection-assigned `GetUninitializedObject` sentinels in `<TESTV>/EfcViewerTests.cs`, then measure `EfcViewer.cs` with F1's harness.
  - If the measured line rate is below 0.80, do NOT add `[ExcludeFromCodeCoverage]` and do NOT modify `coverage.config`. Record the shortfall, escalate to the maintainer as a DEC-1 reconsideration, and halt.
  - Acceptance: `<FEATURE>/evidence/qa-gates/approach-b-forward-member-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, the per-member reachability table, and the measured line rate with a PASS/HALT verdict
- [ ] [P2-T48] Verify Phase 2 compiles, runs green, and respects the file-size ceiling
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` then the scoped vstest run with `/TestCaseFilter:"FullyQualifiedName~EfcViewer"`
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase2-scoped-run.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with pass/fail counts and the line counts of `<VIEW>/EfcViewer.cs`, `<TESTV>/EfcViewerTests.cs`, and (Approach A) `<TESTV>/EfcViewer.StaTests.cs`, all < 500
- [ ] [P2-T49] Measure `EfcViewer.cs` per-file line and branch coverage and confirm the AC1/AC2 floors
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-efc-form-item-controller-coverage-452\evidence\qa-gates\coverage-phase2.cobertura.xml`, then F1's per-file harness over that output
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase2-efcviewer-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and **numeric** `LINE_COVERED / LINE_VALID` (rate >= 0.80) and `BRANCH_COVERED / BRANCH_VALID` (rate >= 0.75) for `QuickFiler\Viewers\EfcViewer.cs`, plus the `DERIVATION:` and `ISSUE_441_DISCLOSURE:` statements

### Phase 3 — EfcFormController.cs Partial Split, Seam Extraction, and Coverage

Per-file coverage is measured per Cobertura `filename`, and each partial emits its own `filename`.
**Every one of the eight partials must independently clear 80% line and 75% branch.**
`[ExcludeFromCodeCoverage]` on any `EfcFormController.*.cs` partial is a Blocking finding. No member
of this file is an irreducible-remainder candidate.

- [ ] [P3-T1] Remove `[ExcludeFromCodeCoverage]` from `<PROD>/EfcFormController.cs:27` (AC3)
  - Acceptance: no `ExcludeFromCodeCoverage` occurrence remains in the file and the solution builds
- [ ] [P3-T2] Delete the commented-out dead code in `<PROD>/EfcFormController.cs`
  - The 19-line superseded `GetKbdActions` block at `:605-623` plus the fragments at `:147-148`, `:304-305`, `:311-312`, `:317-318`, `:323-324`, `:583-586`, `:735`, `:737`, `:827`, `:1002-1006`. Deletion only.
  - Acceptance: the file's line count drops by at least 19 from the task P0-T21 baseline of 1,086 and no executable statement changed
- [ ] [P3-T3] Remove the unused `using` directives in `<PROD>/EfcFormController.cs`
  - Candidates `:4` (`System.Diagnostics`), `:7` (`System.Drawing.Drawing2D`), `:8` (`System.IO`), `:10` (`System.Text`), `:20` (`ToDoModel`). `:19` (`TaskVisualization`) is used by `ManageFilters` at `:563` until task P3-T20 moves it into the S5 seam default. Verify each with IDE0005 before removing.
  - Acceptance: the analyzer build reports zero IDE0005 diagnostics for this file
- [ ] [P3-T4] Create `<PROD>/EfcFormController.Properties.cs` by moving the property members
  - Moves `ActiveTheme` (254-264), `LoadTheme` (266-271), `DarkMode` (273-285), `FormHandle` (287), `SelectedFolder` (289-295), the four settings properties (297-343), `Token` (345-350), `IsValidSelection` (1040-1052). Near-pure move; no logic change. Projected ~155 lines.
  - Acceptance: the new partial exists, the moved members no longer appear in `EfcFormController.cs`, and the file is < 500 lines
- [ ] [P3-T5] Create `<PROD>/EfcFormController.Setup.cs` by moving the setup members
  - Moves `CaptureConfigureItemViewer` (166-187), `Cleanup` (189-196), `ConfigureFind` (198-206), `ResolveControlGroups` (208-235), `SetupThemes` (237-248), `LoadUserSettings` (1009-1022), `ToggleExpansionStyle` (1056-1084). Projected ~165 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P3-T6] Create `<PROD>/EfcFormController.EventHandlers.cs` by moving the handler members
  - Moves `RegisterAlwaysOnAsyncKeyActions` (356-368), `WireEventHandlers` (370-402), `SearchText_DownArrow` (404-413), the five `Button*_Click` (415-534), the four `*_CheckedChanged` (536-554), `SearchText_TextChanged` (556-559), `EditFiltersMenuItem_Click` (561-566), `DarkMode_Changed` (679-696). Projected ~265 lines — the largest partial.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P3-T7] Create `<PROD>/EfcFormController.KeyboardActions.cs` by moving the keyboard members
  - Moves `CharacterAsyncActions`/`GetAsyncCharacterActions` (568-603), `CharacterActions`/`GetKbdActions` (625-677), `KbdExecuteAsync` ×2 (812-822), `JumpToAsync` (824-829), `ShowMenu` (915), `ToggleCheckboxAsync` (917-921), the four `ToggleOn/OffNavigation` members (923-955). Projected ~195 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P3-T8] Create `<PROD>/EfcFormController.Actions.cs` by moving the major-action members
  - Moves `ActionOkAsync` (702-731), `ActionCancelAsync` (733-740), `ActionDeleteAsync` (742-750), `CreateFolderAsync` (752-795), `RefreshSuggestionsAsync` (797-806), `PopulateFolderCombobox` (1024-1038). Projected ~165 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P3-T9] Create `<PROD>/EfcFormController.Breadcrumb.cs` by moving the breadcrumb members
  - Moves `ConfigureBreadcrumbControl` (834-854), `InitializeBreadcrumbHostAsync` (858-868), `BindFolderRows` (873-883), `BindBreadcrumbRowsAsync` (886-903). Projected ~135 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P3-T10] Create `<PROD>/EfcFormController.Tips.cs` by moving the tips and window-state members
  - Moves `ToggleTips(bool)` (957-970), `ToggleTips(bool, ToggleState)` (972-989), `ToggleTipsAsync` (991-1007), `MaximizeFormViewer` (905-908), `MinimizeFormViewer` (910-913). Projected ~100 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P3-T11] Reduce `<PROD>/EfcFormController.cs` to the primary partial
  - Retains the class declaration (with `IFilerFormController`, `IEfcExpansionStyleHost`, `IEfcViewerCommands` from task P1-T7), `logger` (125-127), all fields (123-162), both public constructors (32-77), the private no-arg constructor (79), and `Initialize`/`InitializeWithoutData`/`InitializeDataFields` (81-119). Projected ~200 lines.
  - Acceptance: the file is < 500 lines and contains no member moved by tasks P3-T4 through P3-T10
- [ ] [P3-T12] Wire the seven new `EfcFormController.*.cs` partials into `QuickFiler/QuickFiler.csproj`
  - Seven self-closing `<Compile Include="Controllers\EfcFormController.<Aspect>.cs" />` entries as one contiguous block immediately after `QuickFiler.csproj:301`, strictly below F8's `EfcHomeController*` region at `:295-300`. Controllers partials are not form-derived, so no `<SubType>` and no `<DependentUpon>` child. CRLF preserved via the `Edit` tool; no `sed -i`, no reordering.
  - Acceptance: seven entries exist, `git diff` shows only added lines in the csproj, and line endings are unchanged
- [ ] [P3-T13] Append `<LEDGER>` rows for the seven new partials in the same change as task P3-T12
  - Bucket `testable`, line target `>= 90%` (new production files per `epic.md:583-585`), branch target `>= 75%`, owner F9 (#452), attribute `none`, rationale "new production file extracted from `EfcFormController.cs` under the 500-line rule"
  - Acceptance: seven rows exist in `<LEDGER>` using F1's literal bucket token from task P0-T11
- [ ] [P3-T14] Verify the split compiles as a pure move and every partial is under 500 lines
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` then the scoped vstest run with `/TestCaseFilter:"FullyQualifiedName~EfcFormController"`
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase3-split-verification.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, the line count of all eight partials (all < 500), and confirmation that the existing `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel` test still passes
- [ ] [P3-T15] Retype the `_formViewer` field to `IEfcFormViewer` (S1)
  - `private EfcViewer _formViewer;` → `private IEfcFormViewer _formViewer;`. **Both public constructors keep their concrete `EfcViewer` parameter type** with an implicit upcast at field assignment, so `EfcHomeControllerDependencies.FormControllerWithDataFactoryDelegate` and `FormControllerWithoutDataFactoryDelegate` (`EfcHomeControllerDependencies.cs:15-32`) are untouched (CCN-1). `KeyboardHandler(EfcViewer viewer, IFilerHomeController parent)` (`KeyboardHandler.cs:35`) is likewise untouched (C9).
  - Acceptance: the field is `IEfcFormViewer`, both public constructor signatures are byte-identical to before, and the solution builds
- [ ] [P3-T16] Add the two `internal` constructor overloads taking `IEfcFormViewer`
  - One mirroring the 7-parameter data-bearing constructor and one seam-only overload that takes no `EfcHomeController`. Explicit overloads, **never** optional parameters on an existing signature (AC10).
  - Acceptance: both overloads exist, both existing public constructors are unchanged, and the solution builds
- [ ] [P3-T17] Add the S2 `EfcHomeController` injectable-delegate seams
  - `internal Func<Task> ExecuteMovesAction` (fall-through `_homeController.ExecuteMovesAsync()`, site 718), `internal Func<string, Task> OpenOlFolderAction` (`OpenOlFolderAsync`, 722), `internal Func<string, Task> OpenFsFolderAction` (`OpenFsFolderAsync`, 478 and 760), `internal IQfcKeyboardHandler KeyboardHandlerOverride` (`_homeController.KeyboardHandler`, sites 379, 382, 814, 820, 826, 926, 935, 943, 951). All default to `null` and use the already-merged in-family `X is null ? concrete : X` idiom (`EfcHomeController.ExecuteMoves.cs:86-109`). No F8 edit.
  - Acceptance: the four seams exist, each consumed via the single-branch idiom at every listed call site, and the solution builds
- [ ] [P3-T18] Add the S3 `EfcDataModel` injectable-delegate seams
  - `FindMatchesAction` (558, 801), `RefreshSuggestionsAction` (799), `InitFolderHandlerAction` (1033), `FolderArrayAccessor` (1037), `SuggestionScoresAccessor` (890-892), `CreateFolderAsyncAction` (492-497), `CreateFolderAction` (771-775), `MoveToFolderAction` (502-509, 780-789). `MoveToFolderAction` takes `object` for the folder parameter so no test fabricates a `MAPIFolder`; the default fall-through performs the `MAPIFolder` cast mirroring the existing cast at `:498`. Zero edits to F5's `EfcDataModel.cs` (CCN-5).
  - Acceptance: the eight seams exist, each consumed via the `X is null ? concrete : X` idiom, and the solution builds
- [ ] [P3-T19] Add the S4 breadcrumb construction factory seams and record the #439 fix-point relocation
  - `internal Func<IEfcFormViewer, IApplicationGlobals, WebView2BreadcrumbHost> BreadcrumbHostFactory` and `internal Func<IApplicationGlobals, IBreadcrumbWebHost, BreadcrumbBridgeRouter> BreadcrumbRouterFactory`, whose defaults reproduce `EfcFormController.cs:836-849` exactly. The wiring (event hookups 850-851, `ApplyTheme` 852, fire-and-forget init 853) stays in the controller. The factory return type is the concrete `WebView2BreadcrumbHost` because `CoreInitialized` is declared on the concrete class (`WebView2BreadcrumbHost.cs:63`) and not on `IBreadcrumbWebHost` (CCN-4). Zero F12 and F13 edits.
  - The `new OutlookFolderHierarchyProvider(_globals.Ol.FolderTreeService)` construction formerly at `:840-842` — the #439 fix point — moves into `BreadcrumbRouterFactory`'s default body. Record this relocation for the PR body.
  - Acceptance: both seams exist, the default bodies are behavior-identical to `:836-849`, and `<FEATURE>/evidence/other/issue-439-fix-point-relocation.md` records `Timestamp:` and the old and new locations
- [ ] [P3-T20] Add the S5 dialog seams
  - `internal Action<string> MessageBoxShowAction { get; set; } = text => MessageBox.Show(text);` replacing the calls at `:472-474`, `:710`, `:756`; `internal Action<IApplicationGlobals> ShowManageFiltersAction` whose default is `g => { var f = new ManageFilters(); f.LoadFilters(g); f.Show(); }` replacing `:563-565`. A **local** delegate seam is required because `UtilitiesCS/Properties/AssemblyInfo.cs` grants no `InternalsVisibleTo` to `QuickFiler.Test`, so `MyBox.DialogInvoker` is unreachable (C2). Precedent: `EfcHomeController.cs:299-305`.
  - Both defaults are lambda-valued: assert identity with `NotBeNull()` plus `NotBeSameAs(sentinel)`, never `.Method.Name`.
  - Acceptance: both seams exist, no direct `MessageBox.Show` or `new ManageFilters()` call remains outside a default body, and the solution builds
- [ ] [P3-T21] Add the S6 user-settings reader seam
  - `internal Func<EfcUserSettings> UserSettingsReader { get; set; }` whose default reads the `QuickFiler.Properties.Settings.Default` static singleton, replacing the four reads at `:1009-1022`. `EfcUserSettings` is the `readonly struct` created by task P1-T4.
  - Acceptance: the seam exists and `LoadUserSettings` no longer reads `Settings.Default` outside the default body
- [ ] [P3-T22] Add the S7 item-controller delegate seams
  - `internal Action<bool, Enums.ToggleState> ItemToggleNavigationAction` (sites 929, 945) and `internal Func<Enums.ToggleState, Task> ItemToggleNavigationAsyncAction` (sites 938, 954), each with the `_itemController` fall-through. Delegates rather than a shared interface, so the `EfcFormController` and `EfcItemController` phases stay independent.
  - Acceptance: both seams exist and both call-site pairs route through them
- [ ] [P3-T23] Rewrite `CaptureConfigureItemViewer` over `IEfcFormViewer` intent members and `EfcFormLayoutMath`
  - `<PROD>/EfcFormController.Setup.cs`: replace the direct reads at `:168-176` with `_formViewer.CaptureItemViewerLayout()` plus `_globals.Ol.GetExplorerScreenSize()`, compute with `EfcFormLayoutMath.ComputeTlpHeights` / `ComputeBodyRowHeight` / `ComputeMinimumFormSize`, and write back through `ApplyItemViewerLayout` and `SetMinimumAndSize`. Arithmetic must be byte-identical to today's.
  - Acceptance: no `RowStyles`, `GetPositionFromControl`, `MinimumSize`, or `Size` access remains in this method, and the member is ~10 lines of orchestration
- [ ] [P3-T24] Rewrite `ToggleExpansionStyle` over `IEfcFormViewer` intent members and `EfcFormLayoutMath`
  - `<PROD>/EfcFormController.Setup.cs`: `:1056-1084` routes through `SetItemViewerRowHeight`, `EfcFormLayoutMath.ExpandForToggle` / `CollapseForToggle`, and `SetMinimumAndSize`; `WindowState` comes from `IForm`
  - Acceptance: no raw TLP or `Size` arithmetic remains in the member and behavior is unchanged
- [ ] [P3-T25] Rewrite `ResolveControlGroups` over `IEfcFormViewer.GetChildControlsExcept` and `TipsLabels`
  - `<PROD>/EfcFormController.Setup.cs`: `:208-235` replaces the `GetAllChildren(except:)` extension call at `:215` with the intent member; the type-partitioning at `:217-234` stays in the controller so the branching remains testable
  - Acceptance: no `WinFormsExtensions` call remains in the member and the partitioning logic is unchanged
- [ ] [P3-T26] Rewrite `WireEventHandlers` over the `IEfcFormViewer` intent events and `WireKeyHandlers`
  - `<PROD>/EfcFormController.EventHandlers.cs`: `:370-402` replaces the `ForAllControls` bulk wiring at `:375-386` with `_formViewer.WireKeyHandlers(...)` and the 13 discrete `+=` subscriptions with the intent events; `_globals.Ol.PropertyChanged += DarkMode_Changed` at `:401` is unchanged
  - Acceptance: no designer control field is referenced in the member and the subscription count is unchanged
- [ ] [P3-T27] Rewrite `LoadUserSettings` over `UserSettingsReader` and the intent checked-properties
  - `<PROD>/EfcFormController.Setup.cs`: `:1009-1022` reads one `EfcUserSettings` from the seam and writes the four `*Checked` intent properties
  - Acceptance: no `Settings.Default` read and no `ToolStripMenuItem` reference remains in the member
- [ ] [P3-T28] Rewrite the residual direct `_formViewer` control reads over the intent members
  - Remaining sites: `SearchText.Text` at `:558` and `:801` → `SearchTextValue`; `FolderListBox.Select()` at `:410` → `FocusFolderList()`; `Ok.Text`/`NewFolder.Text` at `:203-204` → `OkButtonText`/`NewFolderButtonText`; `MoveOptionsMenu.ShowDropDown()` at `:599`, `:673`, `:915` → `ShowMoveOptionsMenu()`; `BreadcrumbWebView` at `:837` consumed only by the S4 factory; `_formViewer.ItemViewer`/`L0vh_TLP` at `:49-50`, `:68`, `:76` → `ItemViewer`/`ItemTableLayout`; `TipsLabels` at `:210`, `:229`, `:240`
  - **Do not** retype `_itemViewer` as `IItemViewer` — `IItemViewer` declares neither `MinimumSize` nor `L0vh_Tlp` (CCN-5)
  - Acceptance: a grep for designer control field names across all eight partials returns zero matches, and the solution builds
- [ ] [P3-T29] Verify no partial carries `[ExcludeFromCodeCoverage]` and all eight stay under 500 lines
  - Command: grep `ExcludeFromCodeCoverage` across `QuickFiler/Controllers/EfcFormController*.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase3-attribute-and-size-check.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, zero attribute hits, and the eight line counts
- [ ] [P3-T30] Run the nullable-gate cleanup pass over the eight partials
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - `EfcFormController.cs` has no `#nullable enable` directive today while `BreadcrumbBridgeRouter.cs:1` and `BreadcrumbOutboundQueue.cs:1` do; splitting into eight partials multiplies the surface exposed to this gate. **Do not add `#nullable enable` to the new partials** — fix the root null-state issues instead.
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase3-nullable-cleanup.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and a warning-as-error count no higher than the task P0-T18 baseline
- [ ] [P3-T31] Create `<TEST>/EfcFormController.TestSupport.cs`
  - `internal static class` (no `[TestClass]`) providing `CreateController(...)` over the internal `IEfcFormViewer` constructor overloads, `GetPrivateField<T>`/`SetPrivateField<T>` (shape copied from `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:33-53`), `CreateThemeMap()` (copied from `:60-73`), `CreateMockViewer()`, and `CreateRealRouter(out Mock<IBreadcrumbWebHost>, out Mock<IFolderHierarchyProvider>)` constructing a **real** `BreadcrumbBridgeRouter` over mocks plus real `BreadcrumbMessageCodec`, `BreadcrumbHtmlRenderer`, and `BreadcrumbOutboundQueue` (no WebView2, no WinForms, no COM — CCN-2). **Retain `CreateMinimalController()` from `<TEST>/EfcFormControllerTests.cs:18-28` unchanged.**
  - Acceptance: the file exists, is < 500 lines, contains no `[TestClass]`, and constructs no form
- [ ] [P3-T32] Wire `<TEST>/EfcFormController.TestSupport.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcFormController.TestSupport.cs" />` entry exists with CRLF preserved
- [ ] [P3-T33] Create `<TEST>/EfcFormControllerConstructionTests.cs` shell
  - `[TestClass]`; no process-global static mutation, therefore no `[DoNotParallelize]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P3-T34] Wire `<TEST>/EfcFormControllerConstructionTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P3-T35] Create `<TEST>/EfcFormControllerPropertiesTests.cs` shell
  - `[TestClass]`; no static mutation
  - Acceptance: the shell compiles with zero test methods
- [ ] [P3-T36] Wire `<TEST>/EfcFormControllerPropertiesTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P3-T37] Create `<TEST>/EfcFormControllerSetupTests.cs` shell
  - `[TestClass]` with `[DoNotParallelize]` and a restoring `[TestCleanup]`, because task P3-T90 exercises the `UserSettingsReader` default which reads the `QuickFiler.Properties.Settings.Default` process-global singleton
  - Acceptance: the shell compiles with zero test methods and carries both attributes
- [ ] [P3-T38] Wire `<TEST>/EfcFormControllerSetupTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P3-T39] Create `<TEST>/EfcFormControllerEventHandlerTests.cs` shell
  - `[TestClass]`; every test in this file that can reach a dialog assigns `MessageBoxShowAction` and `ShowManageFiltersAction` recorders in its Arrange block
  - Acceptance: the shell compiles with zero test methods
- [ ] [P3-T40] Wire `<TEST>/EfcFormControllerEventHandlerTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P3-T41] Create `<TEST>/EfcFormControllerKeyboardTests.cs` shell
  - `[TestClass]`; no static mutation
  - Acceptance: the shell compiles with zero test methods
- [ ] [P3-T42] Wire `<TEST>/EfcFormControllerKeyboardTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P3-T43] Create `<TEST>/EfcFormControllerActionsTests.cs` shell
  - `[TestClass]`; every test assigns a recording `MessageBoxShowAction` in Arrange
  - Acceptance: the shell compiles with zero test methods
- [ ] [P3-T44] Wire `<TEST>/EfcFormControllerActionsTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P3-T45] Create `<TEST>/EfcFormControllerBreadcrumbTests.cs` shell
  - `[TestClass]`; every test sets `BreadcrumbHostFactory` and `BreadcrumbRouterFactory` so the default bodies (which construct a live WebView2 host) are never invoked
  - Acceptance: the shell compiles with zero test methods
- [ ] [P3-T46] Wire `<TEST>/EfcFormControllerBreadcrumbTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P3-T47] Create `<TEST>/EfcFormControllerTipsTests.cs` shell
  - `[TestClass]`; no static mutation
  - Acceptance: the shell compiles with zero test methods
- [ ] [P3-T48] Wire `<TEST>/EfcFormControllerTipsTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P3-T49] Add `Ctor_WithDataModel_AssignsAllInjectedCollaborators` to `<TEST>/EfcFormControllerConstructionTests.cs` (#1, P)
  - Internal `IEfcFormViewer` overload; assert `_globals`, `_dataModel`, `_formViewer`, `_homeController`, `_parentCleanup`, `_initType`, `Token` all match the injected instances by reflection read-back
- [ ] [P3-T50] Add `Ctor_WithDataModel_CapturesItemViewerAndItemTableLayoutFromViewer` (#2, P)
  - `Mock<IEfcFormViewer>` returns distinct `ItemViewer` and `ItemTableLayout` sentinels; assert `_itemViewer` and `_itemTlp` are those instances (`:49-50`)
- [ ] [P3-T51] Add `Ctor_WithoutDataModel_LeavesDataModelNull` (#3, E)
  - The no-data internal overload; assert `_dataModel` is null and an `EfcItemController` was constructed (`:53-77`)
- [ ] [P3-T52] Add `PrivateParameterlessCtor_ProducesInstanceWithAllFieldsNull` (#4, E)
  - `EfcFormControllerTestSupport.CreateMinimalController()`; assert every reflected instance field is null (`:79`)
- [ ] [P3-T53] Add `Initialize_InvokesSetupSequenceInOrder` (#5, S)
  - Call-recording seams; assert the six setup members were invoked in source order (`:81-99`)
- [ ] [P3-T54] Add `Initialize_ReturnsSameInstance` (#6, P)
  - Assert `BeSameAs(controller)` (`:98`)
- [ ] [P3-T55] Add `Initialize_TriggersFolderComboboxPopulation` (#7, P)
  - Assert `InitFolderHandlerAction` and `FolderArrayAccessor` were both invoked, proving the fire-and-forget `_ = PopulateFolderCombobox()` at `:97` ran; observe with a `TaskCompletionSource` completed from inside the seam, never a delay
- [ ] [P3-T56] Add `InitializeWithoutData_DoesNotPopulateFolderCombobox` (#8, E)
  - Assert `InitFolderHandlerAction` was never invoked (`:101-111`)
- [ ] [P3-T57] Add `InitializeWithoutData_DelegatesToItemControllerInitializeWithoutData` (#9, P)
  - Assert the item-controller seam recorded the call (`:107`)
- [ ] [P3-T58] Add `InitializeDataFields_AssignsDataModelAndRepopulates` (#10, S)
  - Assert `_dataModel` is the supplied model and the folder-combobox path ran (`:113-119`). The `InitializeDataFields(EfcDataModel)` signature is an F8 contract and must not change.
- [ ] [P3-T59] Add `InitializeDataFields_ReturnsSameInstance` (#11, P)
  - Assert `BeSameAs(controller)` (`:118`)
- [ ] [P3-T60] Add `ActiveTheme_WhenThemesNull_ThrowsArgumentNullException` to `<TEST>/EfcFormControllerPropertiesTests.cs` (#12, X)
  - `_themes` null; `Initializer.GetOrLoad` with `strict: true` throws from `Initializer.cs:310-321`. Characterizes #464; does not fix it.
- [ ] [P3-T61] Add `ActiveTheme_WhenUnset_LoadsFromLoadTheme` (#13, P)
  - Reflection-set `_themes` with the `CreateThemeMap()` map; assert the getter returns the `LoadTheme()` result (`:257`)
- [ ] [P3-T62] Add `ActiveTheme_WhenAlreadySet_ReturnsCachedValueWithoutReloading` (#14, E)
  - Pre-set `_activeTheme`; assert no theme lookup occurred
- [ ] [P3-T63] Add `ActiveTheme_Set_AppliesThemeAsynchronously` (#15, S)
  - Assert the stored value changed and `_themes[value].SetTheme(async: true)` was reached (`:259-263`); use a `Theme` with an empty `ControlGroups` dictionary so `SetTheme` is a deterministic no-op
- [ ] [P3-T64] Add `LoadTheme_WhenDarkModeTrue_ReturnsDarkNormalAndApplies` (#16, P)
  - `Mock<IOlObjects>.DarkMode` true; assert `"DarkNormal"` (`:266-271`)
- [ ] [P3-T65] Add `LoadTheme_WhenDarkModeFalse_ReturnsLightNormalAndApplies` (#17, P)
  - Assert `"LightNormal"`
- [ ] [P3-T66] Add `DarkMode_Get_ReadsFromOlObjects` (#18, P)
  - `Mock<IApplicationGlobals>` → `Mock<IOlObjects>`; assert the getter returns the mocked value (`:276-283`)
- [ ] [P3-T67] Add `DarkMode_Get_WhenGlobalsNull_ThrowsNullReference` (#19, X)
  - Pins current behavior: `_globals.Ol` is evaluated eagerly as a `params object[]` element, so the getter NREs rather than returning the default. Characterizes #464; **do not add a guard.**
- [ ] [P3-T68] Add `DarkMode_Set_WritesThroughToOlObjects` (#20, S)
  - Assert `VerifySet(o => o.DarkMode = true, Times.Once)` (`:284`)
- [ ] [P3-T69] Add `FormHandle_ReturnsViewerHandle` (#21, P)
  - `Mock<IEfcFormViewer>` returns a sentinel `IntPtr`; assert equality (`:287`)
- [ ] [P3-T70] Add `SelectedFolder_WhenRouterNull_ReturnsNull` (#22, N)
  - `_router` null; assert null from the `?.` arm (`:294`)
- [ ] [P3-T71] Add `SelectedFolder_AfterRouterSelectsSuggestionRow_ReturnsFullPath` (#23, P)
  - Real router over mocks: `await router.BindRowsAsync(new[]{"Alpha"}, scores, token)` then `router.SelectFirstRow()`. `BreadcrumbRowBuilder.Classify` makes any non-`"===="`-prefixed, non-`"Trash to Delete"` string a `Suggestion` deterministically. **#439 characterization:** the relative stem passes through verbatim; assert no multi-segment lineage.
- [ ] [P3-T72] Add `SaveAttachments_SaveEmail_SavePictures_MoveConversation_RoundTrip` (#24, P)
  - Four independent round-trips over the backing-field properties (`:297-343`)
- [ ] [P3-T73] Add `Token_RoundTrips` (#25, P)
  - Assert getter returns the set `CancellationToken` (`:345-350`)
- [ ] [P3-T74] Add `IsValidSelection_WhenSelectedFolderNull_ReturnsFalse` (#26, N)
  - `_router` null → `SelectedFolder` null; assert `false` (`:1046`)
- [ ] [P3-T75] Add `IsValidSelection_WhenSelectedFolderEmptyOrShorterThanThree_ReturnsFalse` (#27, E)
  - Table-driven over `""`, `"a"`, `"ab"`; assert `false` for each (`:1047-1048`)
- [ ] [P3-T76] Add `IsValidSelection_WhenSelectedFolderStartsWithTripleEquals_ReturnsFalse` (#28, E)
  - `"===Banner"`; assert `false`. Pins the three-`=` test at `:1049` against the four-`=` tests at `:708` and `BreadcrumbRowBuilder.cs:19` — characterizes #465; do not reconcile them.
- [ ] [P3-T77] Add `IsValidSelection_WhenSelectedFolderIsRealPath_ReturnsTrue` (#29, P)
  - A plain multi-segment path; assert `true` (`:1045-1050`)
- [ ] [P3-T78] Add `CaptureConfigureItemViewer_ComputesExpandedCollapsedAndDiffHeights` to `<TEST>/EfcFormControllerSetupTests.cs` (#30, P)
  - `Mock<IEfcFormViewer>.CaptureItemViewerLayout()` returns a fixed snapshot; assert `_tlpHeightExpanded`, `_tlpHeightCollapsed`, `_tlpHeightDiff` match hand-computed values
- [ ] [P3-T79] Add `CaptureConfigureItemViewer_SetsFormMinimumToSeventyFivePercentOfExplorerScreen` (#31, P)
  - `Mock<IOlObjects>.GetExplorerScreenSize()` returns a fixed `Size`; assert `SetMinimumAndSize` received the 0.75-scaled values (`:182-186`)
- [ ] [P3-T80] Add `CaptureConfigureItemViewer_TogglesExpansionStyleOff` (#32, S)
  - Assert the expansion-style path ran with `ToggleState.Off` (`:174`)
- [ ] [P3-T81] Add `Cleanup_UnsubscribesDarkModeChangedFromOlObjects` (#33, S)
  - Assert `VerifyRemove` on `PropertyChanged` (`:191`)
- [ ] [P3-T82] Add `Cleanup_NullsGlobalsViewerAndDataModel` (#34, S)
  - Reflection read-back asserting all three fields are null (`:192-194`)
- [ ] [P3-T83] Add `Cleanup_InvokesParentCleanup` (#35, P)
  - Recording `Action`; assert invoked once (`:195`)
- [ ] [P3-T84] Add `ConfigureFind_WhenInitTypeHasFind_RewritesTitleAndTwoButtonCaptions` (#36, P)
  - Assert `Text`, `OkButtonText`, `NewFolderButtonText` were all set (`:200-205`)
- [ ] [P3-T85] Add `ConfigureFind_WhenInitTypeIsSort_LeavesCaptionsUntouched` (#37, N)
  - Assert no setter was invoked, closing the `HasFlag` false arm (`:200`)
- [ ] [P3-T86] Add `ResolveControlGroups_PartitionsButtonsCheckboxesHighlightedAndDefault` (#38, P)
  - `GetChildControlsExcept` returns handle-less `new Button()`, `new CheckBox()`, `new Label()` instances; assert each lands in the right list (`:217-234`)
- [ ] [P3-T87] Add `ResolveControlGroups_TogglesEveryTipsDetailOff` (#39, S)
  - `Mock<IQfcTipsDetails>` list via reflection; assert each received `Toggle(false)` (`:213`)
- [ ] [P3-T88] Add `ResolveControlGroups_ExcludesItemViewerFromChildEnumeration` (#40, E)
  - Assert the exclusion list passed to `GetChildControlsExcept` contains the item viewer (`:215`)
- [ ] [P3-T89] Add `SetupThemes_PopulatesThemeMapAndSetsActiveTheme` (#41, P)
  - `TipsLabels` returns an empty `List<Label>`; `EfcThemeHelper.SetupFormThemes` is `public static` and tolerates empty lists. Assert `_themes` is non-empty and `_activeTheme` was set (`:239-247`). F9 does not edit `EfcThemeHelper.cs` (CCN-3).
- [ ] [P3-T90] Add `LoadUserSettings_CopiesFourSettingsIntoFieldsAndMenuChecks` (#42, P)
  - Inject an `EfcUserSettings` through `UserSettingsReader`; assert the four fields and the four `*Checked` intent properties. Class is `[DoNotParallelize]` because a companion assertion exercises the singleton-reading default.
- [ ] [P3-T91] Add `ToggleExpansionStyle_OnAndOff_AdjustRowHeightAndFormSizeSymmetrically` (#43, S)
  - Drive both states; assert `SetItemViewerRowHeight` and `SetMinimumAndSize` received the `ExpandForToggle`/`CollapseForToggle` values and that On-then-Off restores the original sizes
- [ ] [P3-T92] Add `RegisterAlwaysOnAsyncKeyActions_RegistersReturnKeyBoundToActionOk` to `<TEST>/EfcFormControllerEventHandlerTests.cs` (#44, P)
  - `Mock<IEfcFormViewer>.KeyboardHandler` returns `Mock<IQfcKeyboardHandler>`; assert the `Keys.Return` entry was written to `AlwaysOnKeyActionsAsync` (`:356-368`). Note this is the only site reading `_formViewer.KeyboardHandler`; every other keyboard access uses `_homeController.KeyboardHandler`.
- [ ] [P3-T93] Add `WireEventHandlers_SubscribesAllThirteenViewerEvents` (#45, P)
  - Assert each of the 13 intent events received exactly one subscription (`:375-400`)
- [ ] [P3-T94] Add `WireEventHandlers_SubscribesDarkModeChangedToOlPropertyChanged` (#46, P)
  - Assert `VerifyAdd` on `IOlObjects.PropertyChanged` (`:401`)
- [ ] [P3-T95] Add `WireEventHandlers_ConfiguresBreadcrumbControl` (#47, S)
  - Assert both breadcrumb factories were invoked (`:393`); the default bodies must never run
- [ ] [P3-T96] Add `SearchTextDownArrow_WhenKeyIsDown_FocusesFolderListAndSelectsFirstRow` (#48, P)
  - `new KeyEventArgs(Keys.Down)`; assert `FocusFolderList()` and `router.SelectFirstRow()` (`:406-412`)
- [ ] [P3-T97] Add `SearchTextDownArrow_WhenKeyIsNotDown_DoesNothing` (#49, N)
  - `Keys.Up`; assert neither call occurred, closing the `:406` false arm
- [ ] [P3-T98] Add `SearchTextDownArrow_WhenRouterNull_DoesNotThrow` (#50, E)
  - `_router` null; assert `FocusFolderList()` still ran and no throw, closing the `?.` arm at `:411`
- [ ] [P3-T99] Add `ButtonOkClick_InvokesActionOkAsync` (#51, P)
  - `async void` handler observed with a `TaskCompletionSource` completed from inside the injected seam; never a delay. Assign a recording `MessageBoxShowAction`.
- [ ] [P3-T100] Add `ButtonOkClick_WhenActionThrows_LogsAndRethrows` (#52, X)
  - Seam returns a pre-faulted `Task`; assert the exception surfaces. Pins the `logger.Error(...); throw;` shape at `:440-444` — characterizes #464; **do not remove the rethrow.**
- [ ] [P3-T101] Add `ButtonCancelClick_InvokesActionCancelAsync` (#53, P)
  - Same `TaskCompletionSource` observation shape (`:415-429`)
- [ ] [P3-T102] Add `ButtonRefreshClick_InvokesRefreshSuggestionsAsync` (#54, P)
  - Same shape (`:447-461`)
- [ ] [P3-T103] Add `ButtonDeleteClick_InvokesActionDeleteAsync` (#55, P)
  - Same shape (`:523-534`). Pins that this handler omits the synchronization-context bootstrap its four siblings perform — characterizes #465; do not add it.
- [ ] [P3-T104] Add `ButtonCreateClick_WhenSelectionInvalid_ShowsMessageAndDoesNotCreate` (#56, N)
  - Recording `MessageBoxShowAction`; assert one message and no create-folder seam call (`:470-475`)
- [ ] [P3-T105] Add `ButtonCreateClick_WhenFindMode_OpensFileSystemFolderThenClosesAndCleansUp` (#57, P)
  - Assert `OpenFsFolderAction`, `Close()`, and the parent cleanup all ran (`:476-482`)
- [ ] [P3-T106] Add `ButtonCreateClick_WhenOneDriveMissing_ReturnsWithoutCreating` (#58, N)
  - Assert no create-folder seam call (`:485-489`)
- [ ] [P3-T107] Add `ButtonCreateClick_WhenFolderCreated_MovesThenClosesAndCleansUp` (#59, P)
  - `CreateFolderAsyncAction` returns a non-null sentinel `object`; assert `MoveToFolderAction` ran with it and the form was disposed (`:500-513`)
- [ ] [P3-T108] Add `ButtonCreateClick_WhenFolderCreationReturnsNull_LeavesFormOpen` (#60, E)
  - Seam returns null; assert no move and no dispose (`:500`)
- [ ] [P3-T109] Add `MenuCheckedChangedHandlers_MirrorMenuStateIntoProperties` (#61, P)
  - Four assertions, one per handler at `:536-554`, each driving its intent `*Changed` event and asserting the matching property
- [ ] [P3-T110] Add `SearchTextChanged_BindsFindMatchesResultToBreadcrumb` (#62, P)
  - `FindMatchesAction` returns relative folder stems; assert the router received them **verbatim** (`:556-559`). **#439 characterization — do not assert a multi-segment lineage.**
- [ ] [P3-T111] Add `EditFiltersMenuItemClick_OpensManageFiltersWithGlobals` (#63, P)
  - Recording `ShowManageFiltersAction`; assert it received the exact `_globals` instance. The default body must never run — it would open a window (`:561-566`).
- [ ] [P3-T112] Add `DarkModeChanged_WhenPropertyNameMatches_SwapsThemeAndRethemesBreadcrumb` (#64, S)
  - `PropertyChangedEventArgs("DarkMode")`; assert `ActiveTheme` swapped and `router.ApplyTheme(bool)` ran (`:679-695`)
- [ ] [P3-T113] Add `DarkModeChanged_WhenPropertyNameDiffers_DoesNothing` (#65, N)
  - A different property name; assert no change, closing the `:681` false arm
- [ ] [P3-T114] Add `GetAsyncCharacterActions_RegistersSevenControllerKeys` to `<TEST>/EfcFormControllerKeyboardTests.cs` (#66, P)
  - Assert the key set and handler count only; **do not invoke** the captured lambdas (`:572-603`)
- [ ] [P3-T115] Add `CharacterAsyncActions_IsMemoizedAcrossReads` (#67, E)
  - Read twice; assert `BeSameAs` (`:569-570`)
- [ ] [P3-T116] Add `GetKbdActions_RegistersEightControllerKeys` (#68, P)
  - Assert the key set only (`:629-677`)
- [ ] [P3-T117] Add `CharacterActions_IsMemoizedAcrossReads` (#69, E)
  - Read twice; assert `BeSameAs` (`:626-627`)
- [ ] [P3-T118] Add `KbdExecuteAsync_Func_TogglesKeyboardDialogThenAwaitsAction` (#70, S)
  - `KeyboardHandlerOverride` returns `Mock<IQfcKeyboardHandler>`; assert ordering via a call recorder (`:812-816`)
- [ ] [P3-T119] Add `KbdExecuteAsync_Action_TogglesKeyboardDialogThenInvokesAction` (#71, S)
  - Same shape for the `Action` overload (`:818-822`)
- [ ] [P3-T120] Add `JumpToAsync_TogglesKeyboardDialogThenFocusesControl` (#72, S)
  - Handle-less `new Button()`; `Focus()` returns `false` without throwing on an unparented control (`:824-829`)
- [ ] [P3-T121] Add `ToggleOffNavigation_RemovesControllerKeysTogglesTipsOffAndItemNavOff` (#73, S)
  - Real `KbdActions<>` via `KeyboardHandlerOverride`; assert key removal, tips toggle, and `ItemToggleNavigationAction` (`:923-930`)
- [ ] [P3-T122] Add `ToggleOffNavigationAsync_RemovesAsyncKeysAndAwaitsBothToggles` (#74, S)
  - Same with the async seams (`:932-939`)
- [ ] [P3-T123] Add `ToggleOnNavigation_AddsControllerKeysTogglesTipsOnAndItemNavOn` (#75, S)
  - Mirror of task P3-T121 (`:941-946`)
- [ ] [P3-T124] Add `ToggleOnNavigationAsync_AddsAsyncKeysAndAwaitsBothToggles` (#76, S)
  - Mirror of task P3-T122 (`:948-955`)
- [ ] [P3-T125] Add `ActionOkAsync_WhenSelectedFolderNull_ShowsMessageAndReturns` to `<TEST>/EfcFormControllerActionsTests.cs` (#77, N)
  - Recording `MessageBoxShowAction`; assert one message and no move (`:707-712`)
- [ ] [P3-T126] Add `ActionOkAsync_WhenSelectedFolderIsBanner_ShowsMessageAndReturns` (#78, E)
  - Selection starting with `"===="`; assert the same outcome, closing the `StartsWith` true arm at `:708`
- [ ] [P3-T127] Add `ActionOkAsync_WhenSortMode_ExecutesMovesThenDisposesAndCleansUp` (#79, P)
  - Assert `ExecuteMovesAction`, `Hide()`, `Dispose()`, and cleanup in order (`:716-729`)
- [ ] [P3-T128] Add `ActionOkAsync_WhenFindMode_OpensOutlookFolderThenDisposesAndCleansUp` (#80, P)
  - Assert `OpenOlFolderAction` ran with the selected path (`:720-723`)
- [ ] [P3-T129] Add `ActionOkAsync_WhenNeitherSortNorFind_ThrowsNotImplementedException` (#81, X)
  - Assert `NotImplementedException` (`:726`)
- [ ] [P3-T130] Add `ActionCancelAsync_ClosesViewerAndCleansUp` (#82, P)
  - `UiSyncContext` is a plain `new SynchronizationContext()`, proven compatible with `UiThread.SynchronizationContextAwaiter` by `UtilitiesCS.Test/Threading/UiThread_Tests.cs:25,40` (`:733-740`)
- [ ] [P3-T131] Add `ActionDeleteAsync_PrependsTrashRowAndRebinds` (#83, S)
  - Assert `"Trash to Delete"` at index 0 of the rebound array (`:742-750`)
- [ ] [P3-T132] Add `ActionDeleteAsync_WhenNoRowsPresented_BindsTrashRowOnly` (#84, E)
  - Empty `_folderRows`; assert a single-element bind. Invoking twice accumulates a duplicate trash row — characterizes #465; **do not add a dedupe guard.**
- [ ] [P3-T133] Add `CreateFolderAsync_WhenSelectionInvalid_ShowsMessage` (#85, N)
  - Recording `MessageBoxShowAction` (`:754-757`)
- [ ] [P3-T134] Add `CreateFolderAsync_WhenFindMode_OpensFileSystemFolder` (#86, P)
  - Assert `OpenFsFolderAction` (`:758-761`)
- [ ] [P3-T135] Add `CreateFolderAsync_WhenOneDriveMissing_ReturnsAfterHidingForm` (#87, N)
  - Assert `Hide()` ran and no create call (`:764-769`)
- [ ] [P3-T136] Add `CreateFolderAsync_WhenFolderCreated_MovesDisposesAndCleansUp` (#88, P)
  - `CreateFolderAction` returns a sentinel; assert `MoveToFolderAction`, `Dispose()`, cleanup (`:778-793`)
- [ ] [P3-T137] Add `CreateFolderAsync_WhenFolderCreationReturnsNull_DoesNotDispose` (#89, E)
  - Seam returns null; assert no dispose (`:778`)
- [ ] [P3-T138] Add `RefreshSuggestionsAsync_RefreshesThenFindsMatchesThenBinds` (#90, S)
  - Assert `RefreshSuggestionsAction` then `FindMatchesAction` then the bind, in order (`:797-806`). Pins that `_formViewer.SearchText.Text` is read from inside the `Task.Run` lambda — characterizes #465; **do not move the read.**
- [ ] [P3-T139] Migrate `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel` **verbatim** from `<TEST>/EfcFormControllerTests.cs:34-53` into `<TEST>/EfcFormControllerActionsTests.cs` (#91)
  - Migrate `CreateMinimalController()` (`:18-28`) with it, unchanged. No assertion text may change. This is the issue-#145 regression test pinning the `_formViewer == null` early return at `EfcFormController.cs:1029-1031` and is part of the spec (`CLAUDE.md` §7.3).
  - Acceptance: the test passes in its new home, its text matches the task P0-T23 transcription byte-for-byte, and `<TEST>/EfcFormControllerTests.cs` is removed from the csproj only if it is left empty
- [ ] [P3-T140] Add `PopulateFolderCombobox_InitializesFolderHandlerThenBindsFolderArray` (#92, P)
  - `FolderArrayAccessor` returns relative folder stems; assert they reach the router **verbatim** (`:1033-1037`). **#439 characterization — do not assert a multi-segment lineage.**
- [ ] [P3-T141] Add `ConfigureBreadcrumbControl_CreatesHostAndRouterThroughFactories` to `<TEST>/EfcFormControllerBreadcrumbTests.cs` (#93, P)
  - Assert both factories were invoked exactly once with the expected arguments (`:836-849`)
- [ ] [P3-T142] Add `ConfigureBreadcrumbControl_WiresCoreInitializedToRouterNotification` (#94, S)
  - Raise `CoreInitialized` on a stub host; assert `router.NotifyCoreInitialized()` (`:850`)
- [ ] [P3-T143] Add `ConfigureBreadcrumbControl_WiresFocusSearchRequestedToSearchTextSelect` (#95, S)
  - Raise `FocusSearchRequested` on the real router; assert the search-text control was selected (`:851`)
- [ ] [P3-T144] Add `ConfigureBreadcrumbControl_AppliesCurrentThemeToRouter` (#96, P)
  - Assert `router.ApplyTheme(DarkMode)` (`:852`)
- [ ] [P3-T145] Add `InitializeBreadcrumbHostAsync_WhenHostThrows_LogsAndDoesNotPropagate` (#97, X)
  - Factory returns null so the boundary catches the `NullReferenceException`; assert no throw escapes (`:864-867`)
- [ ] [P3-T146] Add `BindFolderRows_WhenViewerNull_ReturnsWithoutBinding` (#98, N)
  - `_formViewer` null; assert no router call (`:875-879`)
- [ ] [P3-T147] Add `BindFolderRows_WhenRouterNull_ReturnsWithoutBinding` (#99, N)
  - `_router` null; assert no throw (`:876`)
- [ ] [P3-T148] Add `BindFolderRows_WhenRowsNull_StoresEmptyArray` (#100, E)
  - Assert `_folderRows` is a zero-length array (`:881`)
- [ ] [P3-T149] Add `BindFolderRows_StoresPresentedRowsForLaterTrashPrepend` (#101, S)
  - Assert `_folderRows` holds the bound rows (`:881`)
- [ ] [P3-T150] Add `BindBreadcrumbRowsAsync_JoinsSuggestionScoresIntoRouterBind` (#102, P)
  - `SuggestionScoresAccessor` returns a fixed `FolderScore[]`; assert the router received rows and scores unchanged (`:890-893`). **#439 characterization:** a row whose chain lookup yields `null` still binds.
- [ ] [P3-T151] Add `BindBreadcrumbRowsAsync_WhenSuggestionsUnavailable_PassesEmptyScoreArray` (#103, E)
  - Accessor returns the empty array; assert the router received a zero-length score set (`:891-892`)
- [ ] [P3-T152] Add `BindBreadcrumbRowsAsync_WhenCanceled_LogsDebugAndSwallows` (#104, X)
  - Router bind returns a task faulted with `OperationCanceledException`; assert no throw escapes (`:895-898`)
- [ ] [P3-T153] Add `BindBreadcrumbRowsAsync_WhenRouterThrows_LogsErrorAndSwallows` (#105, X)
  - Router bind returns a pre-faulted task with a general exception; assert no throw escapes (`:899-902`)
- [ ] [P3-T154] Add `ToggleTips_WhenAsync_DispatchesEachTipThroughBeginInvoke` to `<TEST>/EfcFormControllerTipsTests.cs` (#106, S)
  - `Mock<IEfcFormViewer>.BeginInvoke(Delegate)` captures the delegate and the test invokes it; assert each `IQfcTipsDetails.Toggle` ran (`:961-964`)
- [ ] [P3-T155] Add `ToggleTips_WhenSynchronous_DispatchesEachTipThroughInvoke` (#107, S)
  - Same shape over `Invoke(Delegate)` (`:966-968`)
- [ ] [P3-T156] Add `ToggleTipsWithState_WhenAsync_DispatchesDesiredStateThroughBeginInvoke` (#108, S)
  - Assert the desired `ToggleState` reached each tip (`:976-981`)
- [ ] [P3-T157] Add `ToggleTipsWithState_WhenSynchronous_DispatchesDesiredStateThroughInvoke` (#109, S)
  - Same over `Invoke` (`:983-987`)
- [ ] [P3-T158] Add `ToggleTipsAsync_TogglesEveryTipToDesiredStateWithSharedColumn` (#110, P)
  - `Mock<IQfcTipsDetails>.ToggleAsync` returns `Task.CompletedTask`; assert all were awaited (`:996-1000`)
- [ ] [P3-T159] Add `ToggleTipsAsync_WhenTokenAlreadyCanceled_ThrowsOperationCanceled` (#111, X)
  - Pre-canceled `CancellationToken`; assert the throw happens before any tip toggles (`:993`)
- [ ] [P3-T160] Add `ToggleTipsAsync_WithEmptyTipsList_CompletesWithoutThrowing` (#112, E)
  - Empty `_listTipsDetails`; assert completion (`:996-1000`)
- [ ] [P3-T161] Add `MaximizeFormViewer_SetsWindowStateMaximized` (#113, P)
  - Assert `VerifySet(v => v.WindowState = FormWindowState.Maximized)` (`:905-908`)
- [ ] [P3-T162] Add `MinimizeFormViewer_SetsWindowStateMinimized` (#114, P)
  - Mirror of task P3-T161 (`:910-913`)
- [ ] [P3-T163] Add `ShowMenu_ShowsMoveOptionsDropDown` (#115, P)
  - Assert `ShowMoveOptionsMenu()` was invoked (`:915`)
- [ ] [P3-T164] Add `ToggleCheckboxAsync_TogglesKeyboardDialogThenInvertsCheckedState` (#116, S)
  - Handle-less `new CheckBox()`; assert `Checked` inverted after the keyboard-dialog toggle (`:917-921`)
- [ ] [P3-T165] Verify Phase 3 runs green and every production and test file stays under 500 lines
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` then the scoped vstest run with `/TestCaseFilter:"FullyQualifiedName~EfcFormController"`. If any test file exceeds 500 lines, split it with a `.Part2.cs` suffix (precedent `QfcStreamingDequeueConfidenceGateTests.Part2.cs`), add the `<Compile Include>` entry, and rerun.
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase3-scoped-run.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with pass/fail counts and the line count of all eight production partials and all nine test files (all < 500)
- [ ] [P3-T166] Measure per-file coverage for all eight `EfcFormController` partials and confirm the AC1/AC2 floors
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-efc-form-item-controller-coverage-452\evidence\qa-gates\coverage-phase3.cobertura.xml`, then F1's per-file harness over that output
  - The union-by-`filename`, max-hits-per-line rule is load-bearing here: five `async void` button handlers at `:415`, `:431`, `:447`, `:463`, `:523` have their entire bodies in compiler-generated state-machine classes sharing the partial's `filename`. A harness reporting only the first `<class>` would materially understate the numbers.
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase3-formcontroller-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and **numeric** `LINE_COVERED / LINE_VALID` (rate >= 0.90 for the seven new partials, >= 0.80 for the retained primary) and `BRANCH_COVERED / BRANCH_VALID` (rate >= 0.75) per partial, plus the `DERIVATION:` and `ISSUE_441_DISCLOSURE:` statements

### Phase 4 — EfcItemController.cs Partial Split, Seam Extraction, and Coverage

Every one of the eight partials must independently clear 80% line and 75% branch; the seven new ones
take the 90% new-file bar. **Zero methods on `EfcItemController` itself are irreducible-remainder
candidates**, and zero STA-bound tests are required for this file. The only exemption candidate is the
new adapter `<VIEW>/EfcItemControlSurface.cs`. The plan must not drift into exempting
`InitializeWebViewAsync`, `WireEvents`, `ResolveControlGroups`, `AdjustViewerForEfc`,
`ToggleExpansionOn/Off`, or `RegisterActions`.

- [ ] [P4-T1] Remove `[ExcludeFromCodeCoverage]` from `<PROD>/EfcItemController.cs:25` (AC3)
  - Acceptance: no `ExcludeFromCodeCoverage` occurrence remains in the file and the solution builds
- [ ] [P4-T2] Delete the commented-out dead code in `<PROD>/EfcItemController.cs`
  - The 82-line `ConversationInfo`/`ConversationItems`/`DfConversation` block at `:452-533` and the block inside `Initialize` at `:115-134`. Deletion only; no behavior change.
  - Acceptance: the file's line count drops by at least 100 from the task P0-T21 baseline of 1,170 and no executable statement changed
- [ ] [P4-T3] Delete the uncalled `InitializeWebView()` method at `<PROD>/EfcItemController.cs:174-205`
  - Zero callers repository-wide (verified by grep across `QuickFiler/` and `QuickFiler.Test/`). A no-behavior-change removal of an uncalled `internal` method, kept as its own task so the deletion is reviewable. Alternative considered and rejected: seam it through `IWebViewCoreInitializer` (~10 lines). **Do not exempt it.**
  - Consequence: research test case #146 is dropped from the inventory (154 cases remain for this file)
  - Acceptance: the method is gone, no call site broke, and the solution builds
- [ ] [P4-T4] Create `<IFACE>/IEfcItemDataSource.cs` (S3)
  - `internal interface IEfcItemDataSource` with exactly three members — `UtilitiesCS.MailItemHelper MailInfo { get; }`, `QuickFiler.Helper_Classes.IConversationResolver ConversationResolver { get; }`, `Microsoft.Office.Interop.Outlook.MailItem Mail { get; }` — the complete consumed surface (`MailInfo` at `:282`; `ConversationResolver` at `:311,314,315,666,667,1103`; `Mail.UnRead` at `:99,146`)
  - Acceptance: the file exists and compiles
- [ ] [P4-T5] Create `<IFACE>/IEfcItemControlSurface.cs` (S9)
  - The ~14 members enumerated in `<FEATURE>/research/EfcItemController.research.md` §3 S9. Deliberately shaped so **all arithmetic and branching stay in the controller** and only primitive property reads/writes move behind the adapter: `ItemNumberWidth`, `OpenActionWidth`, `GetOpenActionColumnIndex()`, `ReduceColumnWidth(int, float)`, `GetAllChildren()`, `ItemNumberControl`, `DefaultColorControls`, `MailControls`, `ForEachKeyboardControl(Action<Control>)`, `SetBodyToggleColumnWidths(float, float)`, `ConversationVisible`, `BodyWebViewVisible`, `BodyWebView`, `ConversationColumns`
  - Acceptance: the file exists, compiles, and declares no member carrying arithmetic or a branch
- [ ] [P4-T6] Create `<PROD>/EfcDataModelSource.cs` (S3 adapter)
  - `internal sealed class EfcDataModelSource : IEfcItemDataSource` — a three-property pass-through over an `EfcDataModel`, with an `ArgumentNullException` guard on the constructor. Zero edits to F5's `EfcDataModel.cs` (CCN-5). This adapter **is** testable and receives no exemption.
  - Acceptance: the file exists, is < 500 lines, and compiles
- [ ] [P4-T7] Create `<VIEW>/ItemViewerUiDispatcher.cs` (S7 adapter)
  - `internal sealed class ItemViewerUiDispatcher : UtilitiesCS.Threading.IUiDispatcher` holding an `IItemViewer` and forwarding each member to `viewer.UiDispatcher`, with an `ArgumentNullException` guard. Required because `System.Windows.Threading.Dispatcher` is sealed and the existing `WpfUiDispatcher(Dispatcher)` constructor is `internal` to `UtilitiesCS` (C2, `UtilitiesCS/Threading/WpfUiDispatcher.cs:30`).
  - Acceptance: the file exists, is < 500 lines, and compiles
- [ ] [P4-T8] Create `<VIEW>/EfcItemControlSurface.cs` (S9 adapter) with a ratified-exempt rationale
  - `internal sealed class EfcItemControlSurface : IEfcItemControlSurface` implementing every member as a one-line forward to the concrete `ItemViewer`. **This is the single new file proposed for `ratified-exempt`.** Rationale to record in the file header and the ledger row: every member is a one-line forward to a member of the concrete `ItemViewer`, which is itself `[ExcludeFromCodeCoverage]` (`ItemViewer.cs:20`, F14-owned); exercising even one forward requires constructing a real `ItemViewer`, whose constructor runs `InitializeComponent()` over a 6,224-line Designer that instantiates a WebView2 control (`ItemViewerExpanded.Designer.cs:44`), pulling in the WebView2 native loader — an external-process dependency prohibited by `.claude/rules/general-unit-test.md` § External Dependencies independently of the STA question. The adapter contains no branching, no arithmetic, and no state. In-repo precedent: `WebView2CoreInitializer.cs:15`.
  - The exemption must be **F1-ratified**; if F1's ledger does not ratify it, halt rather than self-granting.
  - Acceptance: the file exists, is < 500 lines, contains no branch and no arithmetic, and carries the rationale comment
- [ ] [P4-T9] Create `<PROD>/EfcItemControllerDependencies.cs` (S10 bundle)
  - Mirrors the **shape**, not the file, of F8's `EfcHomeControllerDependencies`: a constructor taking every seam as an optional argument with a `?? production-default` fallback, exposed as get-only properties. Includes the S4 `EfcThemeFactory` delegate declaration mirroring the 10-parameter signature at `QuickFiler/Helper Classes/EfcThemeHelper.cs:16-27` with the production default `EfcThemeHelper.SetupThemes` supplied as a **method group**.
  - **Do not edit** `EfcHomeControllerDependencies.cs` or `EfcHomeControllerDependencyFactories.cs`; neither mentions `EfcItemController` (CCN-1)
  - Acceptance: the file exists, is < 500 lines, no seam property can be left null after construction, and the solution builds
- [ ] [P4-T10] Add a `private EfcItemController() { }` no-arg constructor to `<PROD>/EfcItemController.cs`
  - Matches the already-merged `EfcFormController.cs:79` precedent so a reflection factory in the test-support file can build a fully-null instance. Chosen over the F10 `HarnessController` subclass pattern as the closer and cheaper precedent.
  - Acceptance: the constructor exists, is `private`, and the solution builds
- [ ] [P4-T11] Wire the six new production files into `QuickFiler/QuickFiler.csproj`
  - `Controllers\EfcDataModelSource.cs` and `Controllers\EfcItemControllerDependencies.cs` as self-closing entries in the contiguous block after `:301`; `Viewers\ItemViewerUiDispatcher.cs` and `Viewers\EfcItemControlSurface.cs` adjacent to the existing `Viewers\` block (plain self-closing — they are not form-derived); `Interfaces\IEfcItemDataSource.cs` and `Interfaces\IEfcItemControlSurface.cs` adjacent to the existing `Interfaces\` block. CRLF preserved via the `Edit` tool; no `sed -i`, no reordering.
  - Acceptance: six entries exist, `git diff` shows only added lines, and line endings are unchanged
- [ ] [P4-T12] Append `<LEDGER>` rows for the six new production files in the same change as task P4-T11
  - `EfcDataModelSource.cs`, `EfcItemControllerDependencies.cs`, `ItemViewerUiDispatcher.cs`: bucket `testable`, `>= 90%` line, `>= 75%` branch, attribute `none`. `EfcItemControlSurface.cs`: bucket `ratified-exempt` with the task P4-T8 rationale. `IEfcItemDataSource.cs`, `IEfcItemControlSurface.cs`: F1's literal third-bucket token, reported `N/A`, no floor, attribute `none`.
  - Acceptance: six rows exist in `<LEDGER>` using F1's literal bucket tokens
- [ ] [P4-T13] Create `<PROD>/EfcItemController.Properties.cs` by moving the exposed-properties region
  - Moves `:386-638` minus `LoadTheme` (which goes to the Theme partial), with the 82-line dead block already deleted by task P4-T2. Projected ~275 lines — the largest partial.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P4-T14] Create `<PROD>/EfcItemController.ViewerSetup.cs` by moving the viewer-setup members
  - Moves `AdjustViewerForEfc` (242-253), `ResolveControlGroups` (326-352), `PopulateControls` (280-301), `PopulateConversation` (303-324), `SetTopicThread` (354-359). Projected ~125 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P4-T15] Create `<PROD>/EfcItemController.WebView.cs` by moving the WebView members
  - Moves `InitializeWebViewAsync` (207-240), `WebView2Control_CoreWebView2InitializationCompleted` (770-799), `HtmlDarkConverter` (1098-1108). `InitializeWebView` is already deleted by task P4-T3. Projected ~135 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P4-T16] Create `<PROD>/EfcItemController.EventWiring.cs` by moving the wiring and keyboard members
  - Moves `WireEvents` (642-678), `RegisterActions` (680-692), `RegisterAsyncFocusActions` (694-719), `UnregisterAsyncFocusActions` (721-730), `UnregisterActions` (732-735), `KbdExecuteAsync` (1144-1148), `JumpToAsync` (1150-1155), `RightKeyActions` (1157-1166). Projected ~155 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P4-T17] Create `<PROD>/EfcItemController.EventHandlers.cs` by moving the handler members
  - Moves `ConversationResolverPropertyChanged` (741-755), `TopicThread_ItemSelectionChanged` (757-768), `DarkMode_Changed` (801-815), `Button_MouseEnter` (817-820), `Button_MouseLeave` (822-832). Projected ~95 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P4-T18] Create `<PROD>/EfcItemController.Navigation.cs` by moving the whole UI-Navigation region
  - Moves `:836-1077` in full. Projected ~270 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P4-T19] Create `<PROD>/EfcItemController.Theme.cs` by moving the theme members
  - Moves `LoadTheme` (404-409), `SetThemeDark` (1083-1096), `SetThemeLight` (1110-1123), `ApplyReadEmailFormat` (1125-1129), `SetOlvTheme` (1131-1138). Projected ~95 lines.
  - Acceptance: the new partial exists, is < 500 lines, and the moved members are gone from the primary
- [ ] [P4-T20] Reduce `<PROD>/EfcItemController.cs` to the primary partial
  - Retains the class declaration, `logger` (167-169), the private fields (363-384) plus the new seam fields, the three public constructors (30-74), the private no-arg constructor from task P4-T10, `InitializeWithoutData` (76-88), `InitializeDataFields` (90-112), `Initialize` (114-165), and `Cleanup` (255-278). Projected ~250 lines.
  - Acceptance: the file is < 500 lines and contains no member moved by tasks P4-T13 through P4-T19
- [ ] [P4-T21] Wire the seven new `EfcItemController.*.cs` partials into `QuickFiler/QuickFiler.csproj`
  - Seven self-closing entries appended to the contiguous `Controllers\Efc*` block after `:301`, no `<SubType>` and no `<DependentUpon>` child. CRLF preserved.
  - Acceptance: seven entries exist and `git diff` shows only added lines
- [ ] [P4-T22] Append `<LEDGER>` rows for the seven new partials in the same change as task P4-T21
  - Bucket `testable`, `>= 90%` line, `>= 75%` branch, owner F9 (#452), attribute `none`
  - Acceptance: seven rows exist using F1's literal bucket token
- [ ] [P4-T23] Verify the split compiles as a pure move and every partial is under 500 lines
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase4-split-verification.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and the line count of all eight partials (all < 500)
- [ ] [P4-T24] Retype `_itemViewer` from the concrete `ItemViewer` to `IItemViewer` (S1)
  - Field, the three constructor parameters, and the `ResolveControlGroups(ItemViewer)` parameter all become `IItemViewer`. The 18 concrete member accesses are verified 1:1 forwards to existing `IItemViewer` intent members (`ItemViewer.DisplayState.cs:13-71`, `ItemViewer.Commands.cs:67-101`, `ItemViewer.WebViewThread.cs:15-32`). The two `new EfcItemController(...)` sites at `EfcFormController.cs:69,87` compile unchanged because the concrete type implements the interface (`ItemViewer.cs:21`). In-repo precedent: `QfcItemController.cs:51`. **Zero F14 edits; do not remove `ItemViewer.cs:20`'s attribute.**
  - Acceptance: no `ItemViewer` concrete type reference remains on the controller side except inside `EfcItemControlSurface`, and the solution builds
- [ ] [P4-T25] Retype `_parent` from `EfcFormController` to `IEfcExpansionStyleHost` (S2)
  - Field, the three constructor parameters, and the `Parent` property return type (`:566-570`). `Parent` has zero consumers repo-wide, so the retype is safe. `EfcFormController` already carries the interface from task P1-T7 and needs no new member.
  - Acceptance: the only member of the parent reached is `ToggleExpansionStyle(state)` (sites `:864`, `:909`), and the solution builds
- [ ] [P4-T26] Retype `_dataModel` to `IEfcItemDataSource` and add the explicit test entry points (S3)
  - Field becomes `private IEfcItemDataSource _data;`. `InitializeDataFields(EfcDataModel)` (`:90`) keeps its signature and wraps internally (`_data = new EfcDataModelSource(dataModel)`), preserving the `EfcFormController.cs:116` call site. Add one **new internal overload** `InitializeDataFields(IEfcItemDataSource)` and one **new internal constructor** taking `IEfcItemDataSource`. Explicit overloads only — never optional parameters on an existing signature (AC10).
  - Acceptance: the existing public entry points are byte-identical, both new overloads exist, and the solution builds
- [ ] [P4-T27] Adopt the S4 `EfcThemeFactory` delegate seam and record the CCN-3 cross-child note
  - Replace the direct `EfcThemeHelper.SetupThemes(...)` calls at `:97` and `:144` with the injected delegate from `EfcItemControllerDependencies`. The production default is a **method group**, which does not tolerate optional parameters: if F4 ever changes `EfcThemeHelper.SetupThemes`'s signature it must add an overload. F9 does not edit `EfcThemeHelper.cs`.
  - Acceptance: no direct static call remains outside the default, and `<FEATURE>/evidence/other/ccn3-theme-method-group-contract.md` records `Timestamp:` and the contract text delivered to F4
- [ ] [P4-T28] Adopt the existing `IWebViewCoreInitializer` seam in `InitializeWebViewAsync` (S5)
  - `QuickFiler/Viewers/IWebViewCoreInitializer.cs:13-29` already abstracts `CreateEnvironmentAsync(cacheFolder, options)` and `EnsureCoreWebView2Async(control, environment)`. Production default `new QuickFiler.Viewers.WebView2CoreInitializer()`. Precedent: `QfcItemController.cs:67` + `QfcItemController.Initialization.cs:381`. F13 owns those files; F9 consumes and does not edit them.
  - **Do not fix** the U+2013 EN DASH in the incognito argument at `:217` — characterizes #463
  - Acceptance: both calls route through the seam and the solution builds
- [ ] [P4-T29] Replace the static `UiThread.Dispatcher` calls with an injected `IUiDispatcher` (S6)
  - Sites `ToggleSaveAttachments` (`:1065`) and `ToggleSaveCopyOfMail` (`:1074`). Production default `new UtilitiesCS.Threading.WpfUiDispatcher()` — the public parameterless constructor, which forwards to the same static `UiThread.Dispatcher` and is therefore behavior-identical. Precedent: `QfcItemController.cs:66`.
  - Acceptance: no `UiThread.Dispatcher` reference remains outside the default, and the solution builds
- [ ] [P4-T30] Replace the `_itemViewer.UiDispatcher` calls with the local `ItemViewerUiDispatcher` (S7)
  - Sites `:913` and `:922` in `ToggleExpansionAsync(ToggleState)`. Field defaulted to `new ItemViewerUiDispatcher(_itemViewer)`. **Do not reuse the S6 dispatcher here** — `UiThread.Dispatcher` and `IItemViewer.UiDispatcher` are not provably the same instance, and substituting one for the other would be a behavior change under the no-behavior-change NFR.
  - Acceptance: the two sites route through the S7 field, the S6 field is untouched, and the solution builds
- [ ] [P4-T31] Replace the fire-and-forget `Task.Run(...)` calls with an injectable background-start delegate (S8)
  - `private Func<Func<Task>, Task> _backgroundRunner;` with production default `f => Task.Run(f)`, replacing `:110` and `:164`. The default is **lambda-valued**: assert identity with `NotBeNull()` plus `NotBeSameAs(sentinel)`, never `.Method.Name`.
  - Acceptance: no bare `Task.Run` remains in `Initialize` or `InitializeDataFields`, and the solution builds
- [ ] [P4-T32] Adopt the `IEfcItemControlSurface` seam for the residual raw-control access (S9)
  - Rewrite `AdjustViewerForEfc` (`:242-253`), `ResolveControlGroups` (`:326-352`), `WireEvents` (`:642-678`), `ToggleExpansionOn`/`ToggleExpansionOff` (`:931-956`), the column-width writes in `ToggleExpansion(ToggleState)` (`:862-905`), and the control/column reads in `InitializeWebViewAsync` over the interface. **The width arithmetic and every branch stay in the controller**; only property reads/writes move into the adapter. Production default `new EfcItemControlSurface((ItemViewer)_itemViewer)` when the injected viewer is a concrete `ItemViewer`.
  - This is strictly better than the F10 precedent, which left `((ItemViewer)_itemViewer)` casts in place and kept method-level `[ExcludeFromCodeCoverage]` on `QfcItemController.ViewerSetup.cs:38` and `:132`. Exempting any of these members here would be a Blocking finding.
  - Acceptance: no `((ItemViewer)_itemViewer)` cast remains in any partial, and the solution builds
- [ ] [P4-T33] Add the `EfcItemControllerDependencies` constructor overload (S10)
  - One new internal constructor accepting an `EfcItemControllerDependencies`, applying production defaults when it is null. Explicit overload, never an optional parameter.
  - Acceptance: the overload exists, all three existing public constructors are unchanged, and the solution builds
- [ ] [P4-T34] Extract `OnWebViewInitialized(bool isSuccess, Exception initializationException)` and reduce the event handler to a forwarding shim
  - `CoreWebView2InitializationCompletedEventArgs` has no public constructor. Extract the body of `WebView2Control_CoreWebView2InitializationCompleted` (`:770-799`) into `internal void OnWebViewInitialized(bool, Exception)` in `<PROD>/EfcItemController.WebView.cs` and leave a two-line forward. Chosen over `GetUninitializedObject` plus reflection into a third-party SDK type; matches "leave only the thinnest possible wiring in the host-bound entry point" (`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy). Cost: ~2 uncovered lines instead of a method-level exemption.
  - **Do not change** the `throw (e.InitializationException)` rethrow — characterizes #464
  - Acceptance: the extracted method exists, the shim is two lines, no method-level `[ExcludeFromCodeCoverage]` is added, and behavior is unchanged
- [ ] [P4-T35] Verify no partial carries `[ExcludeFromCodeCoverage]`, all partials are under 500 lines, and the nullable gate is clean
  - Commands: grep `ExcludeFromCodeCoverage` across `QuickFiler/Controllers/EfcItemController*.cs`, then `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase4-attribute-size-nullable.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, zero attribute hits, the eight line counts, and a warning-as-error count no higher than the task P0-T18 baseline
- [ ] [P4-T36] Create `<TEST>/EfcItemController.TestSupport.cs`
  - `internal static class` (no `[TestClass]`) providing an `EfcItemController` reflection factory over the private no-arg constructor from task P4-T10, EFC-specific builders (probe dependency bundle, `IEfcItemDataSource` fake, `Mock<IEfcItemControlSurface>` factory, `MailItemHelper` object-initializer builder, `Theme` with an **empty** `ControlGroups` dictionary so `SetTheme` is a deterministic no-op). **Reference, do not duplicate,** the existing generic helpers in `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (`SetField`/`GetField`/`InvokeNonPublic` `:37-80`, `BuildSyncDispatcher` `:102-137`, `BuildColorTheme`/`BuildThemeDictionary` `:166-192`, `EnsureUiThreadDispatcher` `:238-249`, `StartRunningDispatcher`/`ShutdownDispatcher` `:297-326`) — they are `internal static` members of `QfcItemControllerTestSupport` in the same test assembly.
  - Acceptance: the file exists, is < 500 lines, contains no `[TestClass]`, and duplicates no `QfcItemControllerTestSupport` helper
- [ ] [P4-T37] Wire `<TEST>/EfcItemController.TestSupport.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T38] Create `<TEST>/EfcItemController.ConstructionTests.cs` shell
  - `[TestClass]`; no process-global static mutation
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T39] Wire `<TEST>/EfcItemController.ConstructionTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T40] Create `<TEST>/EfcItemController.ViewerSetupTests.cs` shell
  - `[TestClass]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T41] Wire `<TEST>/EfcItemController.ViewerSetupTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T42] Create `<TEST>/EfcItemController.PropertiesTests.cs` shell
  - `[TestClass]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T43] Wire `<TEST>/EfcItemController.PropertiesTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T44] Create `<TEST>/EfcItemController.EventWiringTests.cs` shell
  - `[TestClass]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T45] Wire `<TEST>/EfcItemController.EventWiringTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T46] Create `<TEST>/EfcItemController.EventHandlersTests.cs` shell
  - `[TestClass]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T47] Wire `<TEST>/EfcItemController.EventHandlersTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T48] Create `<TEST>/EfcItemController.NavigationTests.cs` shell
  - `[TestClass]`. This file carries 37 cases; if it approaches 500 lines, split into `EfcItemController.NavigationTests.Part2.cs` and register it.
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T49] Wire `<TEST>/EfcItemController.NavigationTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T50] Create `<TEST>/EfcItemController.ThemeTests.cs` shell
  - `[TestClass]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T51] Wire `<TEST>/EfcItemController.ThemeTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T52] Create `<TEST>/EfcItemController.WebViewTests.cs` shell
  - `[TestClass]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T53] Wire `<TEST>/EfcItemController.WebViewTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T54] Create `<TEST>/EfcDataModelSourceTests.cs` shell
  - `[TestClass]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T55] Wire `<TEST>/EfcDataModelSourceTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T56] Create `<TEST>/ItemViewerUiDispatcherTests.cs` shell
  - `[TestClass]` with `[DoNotParallelize]` and a `[TestCleanup]` calling `QfcItemControllerTestSupport.ShutdownDispatcher()`, because these tests start and stop a WPF `Dispatcher` on a dedicated STA thread via the existing `StartRunningDispatcher()` helper (`QfcItemController.TestSupport.cs:297-326`)
  - **AC7 note:** this is dispatcher infrastructure, not a WinForms control instantiation, so it does not invoke the epic's STA last-resort clause and does not require a `*.StaTests.cs` file. In-repo precedent for testing an `IUiDispatcher` adapter this way in a plain `[TestClass]`: `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`. No `*.StaTests.cs` file exists for `EfcItemController`.
  - Acceptance: the shell compiles with zero test methods and carries both attributes
- [ ] [P4-T57] Wire `<TEST>/ItemViewerUiDispatcherTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T58] Create `<TEST>/EfcItemControllerDependenciesTests.cs` shell
  - `[TestClass]`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T59] Wire `<TEST>/EfcItemControllerDependenciesTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the `<Compile Include>` entry exists with CRLF preserved
- [ ] [P4-T60] Add `Constructor_WithFiveArguments_CapturesKeyboardHandlerAndExplorerControllerFromHomeController` to `<TEST>/EfcItemController.ConstructionTests.cs` (research #1, P)
  - `Mock<IFilerHomeController>`; assert both captured fields by reflection read-back (`:59-74`)
- [ ] [P4-T61] Add `Constructor_WithFiveArguments_AssignsGlobalsViewerParentAndToken` (#2, P)
  - Assert the four fields match the injected instances
- [ ] [P4-T62] Add `Constructor_WithDataModel_InvokesInitializeOnceWithAsyncTrue` (#3, P)
  - Dependency bundle with mocked theme factory, background runner, and control surface; assert `Initialize` ran once with `async: true` (`:30-42`)
- [ ] [P4-T63] Add `Constructor_WithExplicitAsyncFlagFalse_PassesFlagThroughToInitialize` (#4, P)
  - The `(..., dataModel, bool async, token)` overload at `:44-57`; note it has zero production call sites (characterizes #466)
- [ ] [P4-T64] Add `Initialize_BuildsThemesThroughInjectedFactoryAndSetsActiveTheme` (#5, P)
  - Assert the `EfcThemeFactory` seam was invoked once and `_themes`/`_activeTheme` were set (`:114-165`)
- [ ] [P4-T65] Add `Initialize_TogglesEveryTipsDetailOffAndSharesItemPositionColumn` (#6, P)
  - `Mock<IQfcTipsDetails>` list injected by reflection; assert each was toggled (`:158-161`)
- [ ] [P4-T66] Add `Initialize_StartsWebViewInitializationThroughTheBackgroundRunnerSeam` (#7, P)
  - Recording background runner that captures the delegate without running it; assert capture (`:164`)
- [ ] [P4-T67] Add `InitializeWithoutData_AdjustsViewerAndResolvesControlGroupsWithoutTouchingThemes` (#8, P)
  - `Mock<IEfcItemControlSurface>` plus injected tips lists; assert the theme factory was never invoked (`:76-88`)
- [ ] [P4-T68] Add `InitializeWithoutData_ReturnsSameControllerInstance` (#9, P)
  - Assert `BeSameAs` (`:87`)
- [ ] [P4-T69] Add `InitializeDataFields_WithNullConversationResolver_SkipsConversationPopulationAndStillWiresEvents` (#10, E)
  - `IEfcItemDataSource` fake with a null resolver; assert wiring still ran (`:90-112`)
- [ ] [P4-T70] Add `InitializeDataFields_ReturnsSameControllerInstance` (#11, P)
  - Assert `BeSameAs` (`:111`)
- [ ] [P4-T71] Add `InitializeDataFields_IsAltProbe_ReturnsFalseWhenMailIsNull` (#12, E)
  - Capture the `Func<bool>` passed to the theme factory and invoke it with `Mail` null; assert `false` (`:99`)
- [ ] [P4-T72] Add `Cleanup_UnsubscribesDarkModeChangedFromGlobalsOlPropertyChanged` (#13, S)
  - Assert `VerifyRemove` on `IOlObjects.PropertyChanged` (`:262`)
- [ ] [P4-T73] Add `Cleanup_UnsubscribesMouseEnterAndMouseLeaveFromEveryButton` (#14, S)
  - Handle-less `new Button()` list injected into `_buttons`; assert the invocation lists shrank (`:257-261`)
- [ ] [P4-T74] Add `Cleanup_NullsGlobalsViewerParentDataModelAndThemeState` (#15, S)
  - Reflection read-back over the nulled fields (`:262-277`). Pins that `_timer` is set to null **without being disposed** and that `_buttons` is never nulled — characterizes #460; **do not fix.**
- [ ] [P4-T75] Add `Cleanup_WhenButtonsWereNeverResolved_ThrowsNullReference` (#16, Err — characterizes #460)
  - Controller built through the 5-argument constructor with no subsequent initialize; assert `NullReferenceException` at `:257`. Pins today's behavior.
- [ ] [P4-T76] Add `AdjustViewerForEfc_RemovesColumnsRightOfConversationCountLabel` to `<TEST>/EfcItemController.ViewerSetupTests.cs` (#17, P)
  - `Mock<IItemViewer>.RemoveControlsColsRightOf`; assert one call (`:247`)
- [ ] [P4-T77] Add `AdjustViewerForEfc_ReducesOpenActionColumnByItemNumberMinusOpenWidth` (#18, P)
  - Surface mock returns widths and column index; assert `ReduceColumnWidth` received the hand-computed delta (`:250-252`)
- [ ] [P4-T78] Add `AdjustViewerForEfc_WhenOpenActionIsWiderThanItemNumber_AppliesNegativeDelta` (#19, E)
  - Assert the negative delta is passed through unclamped
- [ ] [P4-T79] Add `ResolveControlGroups_BuildsOneTipsDetailPerLeftTipsLabel` (#20, P)
  - `Mock<IItemViewer>.LeftTipsLabels` returns three handle-less `Label`s; assert three tips details (`:330-332`)
- [ ] [P4-T80] Add `ResolveControlGroups_SetsItemPositionTipsToTheItemNumberLabel` (#21, P)
  - Assert the item-position tips reference the surface's `ItemNumberControl` (`:334`)
- [ ] [P4-T81] Add `ResolveControlGroups_CollectsOnlyTableLayoutPanelsFromChildren` (#22, P)
  - Mixed handle-less control list; assert only `TableLayoutPanel` instances land in `_tableLayoutPanels` (`:336-339`)
- [ ] [P4-T82] Add `ResolveControlGroups_CollectsOnlyButtonsFromChildren` (#23, P)
  - Assert only `Button` instances land in `_buttons` (`:341`)
- [ ] [P4-T83] Add `ResolveControlGroups_WithNoChildren_ProducesEmptyPanelAndButtonCollections` (#24, E)
  - Empty `GetAllChildren()`; assert both collections are empty and non-null (`:336-341`)
- [ ] [P4-T84] Add `PopulateControls_WithNullMailInfo_ReturnsWithoutWritingAnyViewerText` (#25, N)
  - Assert no `IItemViewer` setter was invoked (`:282-286`)
- [ ] [P4-T85] Add `PopulateControls_WritesSenderSubjectBodyTriageSentOnAndActionable` (#26, P)
  - `MailItemHelper` built by object initializer; assert the six intent setters (`:287-292`)
- [ ] [P4-T86] Add `PopulateControls_WhenTaskFlagIsSet_SetsFlagTaskDialogResultToOk` (#27, S)
  - Assert `FlagTaskDialogResult == DialogResult.OK` (`:293-296`)
- [ ] [P4-T87] Add `PopulateControls_WhenTaskFlagIsNotSet_SetsFlagTaskDialogResultToCancel` (#28, S)
  - Closes the false arm (`:297-300`)
- [ ] [P4-T88] Add `PopulateConversation_WithNullConversationResolver_ReturnsWithoutWritingCount` (#29, N)
  - Assert no count write (`:310-313`)
- [ ] [P4-T89] Add `PopulateConversation_AssignsSetTopicThreadAsTheResolverUpdateUiCallback` (#30, P)
  - `Mock<IConversationResolver>`; assert `UpdateUI` was assigned (`:314`)
- [ ] [P4-T90] Add `PopulateConversation_WritesSameFolderCountToTheConversationCountLabel` (#31, P)
  - `Count = new Pair<int>(3, 5)`; assert `ConversationCountText` received `"3"` (`:315-316`)
- [ ] [P4-T91] Add `PopulateConversation_WhenSameFolderCountIsZero_SetsConversationCountBackColorRed` (#32, E)
  - Assert the red back-colour write (`:317-320`)
- [ ] [P4-T92] Add `PopulateConversation_WhenSameFolderCountIsPositive_LeavesBackColorUnchanged` (#33, P)
  - Closes the false arm (`:317-320`)
- [ ] [P4-T93] Add `SetTopicThread_SetsConversationItemsThenSortsBySentDateDescending` (#34, P)
  - Ordered `Mock<IItemViewer>` verification of `SetConversationItems` then `SortConversationByDate` (`:354-359`)
- [ ] [P4-T94] Add `ActiveTheme_WhenUnset_LoadsFromThemesAndCaches` to `<TEST>/EfcItemController.PropertiesTests.cs` (#35, P)
  - Injected `_themes`; assert the value is cached on the second read (`:393-396`)
- [ ] [P4-T95] Add `ActiveTheme_WhenThemesIsNull_ThrowsArgumentNullExceptionFromStrictDependencyCheck` (#36, Err)
  - `Initializer.DependenciesNotNull` throws when `strict: true` and a dependency is null (`Initializer.cs:290-324`); assert `ArgumentNullException` (`:395`)
- [ ] [P4-T96] Add `ActiveTheme_Setter_StoresValueAndAppliesThatThemeAsynchronously` (#37, S)
  - `Theme` with an empty `ControlGroups` dictionary so `SetTheme(true)` is a deterministic no-op (`:396-401`)
- [ ] [P4-T97] Add `ActiveTheme_Setter_WithUnknownKey_ThrowsKeyNotFound` (#38, Err)
  - Assert `KeyNotFoundException` (`:400`)
- [ ] [P4-T98] Add `LoadTheme_WhenDarkModeIsTrue_SelectsDarkNormalAndAppliesIt` (#39, P)
  - Assert `"DarkNormal"` (`:404-409`)
- [ ] [P4-T99] Add `LoadTheme_WhenDarkModeIsFalse_SelectsLightNormalAndAppliesIt` (#40, P)
  - Assert `"LightNormal"`
- [ ] [P4-T100] Add `DarkMode_ReadsFromOutlookObjectsWhenDependenciesArePresent` (#41, P)
  - `Mock<IOlObjects>.DarkMode`; assert pass-through (`:441-448`)
- [ ] [P4-T101] Add `DarkMode_WhenGlobalsOlIsNull_ReturnsFalseWithoutThrowing` (#42, N)
  - `_globals.Ol` null but `_globals` non-null; assert `false` from the `strict: false` path. **Do not** null `_globals` itself — that path NREs and is characterized by #464, not fixed here.
- [ ] [P4-T102] Add `DarkMode_Setter_WritesThroughToOutlookObjects` (#43, S)
  - Assert `VerifySet` (`:449`)
- [ ] [P4-T103] Add `ItemNumber_Setter_WritesTheNumberToTheViewerItemNumberText` (#44, S)
  - Assert `ItemNumberText` write (`:576-580`)
- [ ] [P4-T104] Add `ItemIndex_IsItemNumberMinusOne_AndSetterStoresValuePlusOne` (#45, E)
  - Round-trip both directions (`:582-586`)
- [ ] [P4-T105] Add `SentDate_FormatsItemInfoSentDateAsMonthDayYear` (#46, P)
  - Fixed `DateTime` on a `MailItemHelper`; assert the `"MM/dd/yyyy"` string. Never read wall-clock time.
- [ ] [P4-T106] Add `SentTime_FormatsItemInfoSentDateAsTwentyFourHourClock` (#47, P)
  - Same fixed `DateTime`; assert the `"HH:mm"` string (`:605-608`)
- [ ] [P4-T107] Add `Sender_And_To_ReadFromItemInfoRatherThanTheViewer` (#48, P)
  - Assert both read `_itemInfo`, with the viewer mock strict on those members (`:595-598`, `:621-624`)
- [ ] [P4-T108] Add `Subject_ReadsFromTheViewerSubjectTextRatherThanItemInfo` (#49, E)
  - Documents the asymmetry with `Sender`/`To` — characterizes #466's D-6 (`:610-613`)
- [ ] [P4-T109] Add `SelectedFolder_DelegatesToViewerGetSelectedFolder` (#50, P)
  - Assert the mock's return value flows through (`:588-593`)
- [ ] [P4-T110] Add `Height_DelegatesToViewerHeight` (#51, P)
  - Assert pass-through (`:535-538`)
- [ ] [P4-T111] Add `ScalarProperties_RoundTrip_ConvOriginIdCounterEnterCounterComboRightIsChildIsActiveUiSuppressEventsToken` (#52, P)
  - Independent round-trip assertions over `:417-436`, `:546-558`, `:615-619`, `:631-636`
- [ ] [P4-T112] Add `RightKeyActions_ReturnsASingleCancelEntryWhoseActionIsANoOp` (#53, P)
  - Assert one entry keyed `"&Cancel"` and that invoking its action does nothing observable (`:1157-1166`)
- [ ] [P4-T113] Add `WireEvents_SubscribesPreviewKeyDownAndKeyDownOnEveryEligibleControl` to `<TEST>/EfcItemController.EventWiringTests.cs` (#54, P)
  - Surface mock's `ForEachKeyboardControl` invoked with handle-less controls; assert both subscriptions per control (`:645-662`)
- [ ] [P4-T114] Add `WireEvents_SubscribesToWebViewInitializationCompleted` (#55, P)
  - Assert `VerifyAdd` on `IItemViewer.WebViewInitializationCompleted` (`:664-665`)
- [ ] [P4-T115] Add `WireEvents_WithNullConversationResolver_SkipsResolverSubscriptionAndStillWiresTheRest` (#56, N)
  - Closes the null arm at `:666-669`
- [ ] [P4-T116] Add `WireEvents_SubscribesToConversationItemSelectionChangedAndGlobalsPropertyChanged` (#57, P)
  - Assert both `VerifyAdd` calls (`:670-672`)
- [ ] [P4-T117] Add `WireEvents_SubscribesMouseEnterAndMouseLeaveOnEveryButton` (#58, P)
  - Handle-less `new Button()` list; assert invocation-list growth (`:673-677`)
- [ ] [P4-T118] Add `RegisterActions_WithOverwriteDuplicatesFalse_FiltersOutKeysAlreadyRegistered` (#59, P)
  - Real `KbdActions<char, KaChar, Action<char>>` (public parameterless ctor, `KbdActions.cs:21`); assert the filter behaviour at `:685-690`
- [ ] [P4-T119] Add `RegisterActions_WithOverwriteDuplicatesTrue_DoesNotFilter` (#60, P)
  - Closes the other arm (`:685-690`)
- [ ] [P4-T120] Add `RegisterActions_WithAnUnregisteredKey_SilentlyDropsTheAction` (#61, Err — characterizes #459)
  - The `KbdActions<>` indexer setter does `Find(key)` and assigns only when non-null (`KbdActions.cs:38-47`), so a missing key is a no-op. Pin today's silent drop at `:691`; **do not fix.**
- [ ] [P4-T121] Add `RegisterAsyncFocusActions_WhenCollapsed_RegistersOnlyOpenAndExpand` (#62, S)
  - `_expanded` false via reflection; assert only `'O'` and `'E'` (`:696-705`)
- [ ] [P4-T122] Add `RegisterAsyncFocusActions_WhenExpanded_AlsoRegistersBodyAndDetailJumps` (#63, S)
  - `_expanded` true; assert `'B'` and `'D'` added (`:706-718`)
- [ ] [P4-T123] Add `RegisterAsyncFocusActions_OpenActionInvokesExplorerControllerOpenQfItem` (#64, P)
  - `Mock<IQfcExplorerController>`; invoke the registered action and assert `OpenQFItem` (`:696-700`)
- [ ] [P4-T124] Add `RegisterAsyncFocusActions_ExpandActionRoutesThroughKbdExecuteAsync` (#65, P)
  - Assert the keyboard-dialog toggle preceded the expansion call (`:701-705`)
- [ ] [P4-T125] Add `UnregisterAsyncFocusActions_WhenCollapsed_RemovesOnlyOpenAndExpand` (#66, S)
  - Symmetric with task P4-T121 (`:723-729`)
- [ ] [P4-T126] Add `UnregisterAsyncFocusActions_WhenExpanded_AlsoRemovesBodyAndDetailJumps` (#67, S)
  - Symmetric with task P4-T122 (`:726-729`)
- [ ] [P4-T127] Add `UnregisterActions_RemovesEveryRequestedKeyFromTheItemSource` (#68, P)
  - Assert each requested key is gone (`:732-735`)
- [ ] [P4-T128] Add `UnregisterActions_WithAnEmptyKeyList_IsANoOp` (#69, E)
  - Assert the action set is unchanged (`:734`)
- [ ] [P4-T129] Add `KbdExecuteAsync_TogglesTheKeyboardDialogBeforeInvokingTheAction` (#70, S)
  - `Mock<IQfcKeyboardHandler>` with a call recorder; assert ordering (`:1144-1148`)
- [ ] [P4-T130] Add `KbdExecuteAsync_WhenTheActionThrows_PropagatesAfterTheDialogToggle` (#71, Err)
  - Pre-faulted `Task`; assert the throw surfaces and the toggle already ran (`:1147`)
- [ ] [P4-T131] Add `JumpToAsync_TogglesTheKeyboardDialogThenFocusesTheTargetControl` (#72, P)
  - Handle-less `new Button()`; `Focus()` returns `false` without throwing (`:1150-1155`)
- [ ] [P4-T132] Add `ConversationResolverPropertyChanged_ForExpandedProperty_ReplacesAndSortsTheConversationItems` to `<TEST>/EfcItemController.EventHandlersTests.cs` (#73, P)
  - Invoke directly with `new PropertyChangedEventArgs("Expanded")`; assert `SetConversationItems` then `SortConversationByDate` (`:746-754`)
- [ ] [P4-T133] Add `ConversationResolverPropertyChanged_ForAnyOtherProperty_LeavesTheConversationUntouched` (#74, N)
  - Closes the guard's false arm (`:746`)
- [ ] [P4-T134] Add `ConversationResolverPropertyChanged_IsNeverTriggeredByTheResolversOwnNotifications` (#75, Err — characterizes #461)
  - Raise each name the resolver actually emits (`"ConversationInfo"`, `"ConversationItems"`, `"Df"`, `"UpdateUI"`) and assert the body never runs, pinning that the handler is dead in production. **Do not fix the guard.**
- [ ] [P4-T135] Add `TopicThreadItemSelectionChanged_WithASelectedHelper_NavigatesToItsHtml` (#76, P)
  - `Mock<IItemViewer>.GetSelectedConversationItems()` returns one `MailItemHelper`; assert `NavigateToString` (`:762-767`)
- [ ] [P4-T136] Add `TopicThreadItemSelectionChanged_WithNoSelection_DoesNotNavigate` (#77, N)
  - Null selection; assert no navigation (`:763`)
- [ ] [P4-T137] Add `TopicThreadItemSelectionChanged_WithAnEmptySelection_DoesNotNavigate` (#78, E)
  - Empty selection list; assert no navigation (`:763`)
- [ ] [P4-T138] Add `OnWebViewInitialized_WhenInitializationFailed_RethrowsTheInitializationException` (#79, Err)
  - Call the task P4-T34 extracted method with `isSuccess: false`; assert the supplied exception is rethrown. Pins the stack-trace-resetting rethrow at `:777` — characterizes #464.
- [ ] [P4-T139] Add `OnWebViewInitialized_WhenSuccessful_MarksTheWebViewerInitialized` (#80, S)
  - Reflection read-back of `_isWebViewerInitialized` (`:779`)
- [ ] [P4-T140] Add `OnWebViewInitialized_WithNullItemInfo_ReturnsBeforeNavigating` (#81, N)
  - Assert no navigation (`:781-784`)
- [ ] [P4-T141] Add `OnWebViewInitialized_InDarkMode_NavigatesToDarkToggledHtml` (#82, P)
  - Assert `NavigateToString` received the dark-toggled HTML (`:785-790`)
- [ ] [P4-T142] Add `OnWebViewInitialized_InLightMode_NavigatesToLightToggledHtml` (#83, P)
  - Closes the other arm (`:791-796`)
- [ ] [P4-T143] Add `OnWebViewInitialized_HidesTheBodyWebViewAfterNavigating` (#84, S)
  - Assert `BodyWebViewVisible` set to `false` on the surface mock (`:798`)
- [ ] [P4-T144] Add `DarkModeChanged_ForDarkModeProperty_WhenDarkModeIsOn_SelectsDarkNormal` (#85, S)
  - `Mock<IOlObjects>.Raise` with `"DarkMode"`; assert the theme swap (`:803-809`)
- [ ] [P4-T145] Add `DarkModeChanged_ForDarkModeProperty_WhenDarkModeIsOff_SelectsLightNormal` (#86, S)
  - Closes the other arm (`:810-813`)
- [ ] [P4-T146] Add `DarkModeChanged_ForAnyOtherProperty_LeavesTheActiveThemeUnchanged` (#87, N)
  - Closes the guard's false arm (`:803`)
- [ ] [P4-T147] Add `ButtonMouseEnter_AppliesTheActiveThemeMouseOverColor` (#88, P)
  - Handle-less `new Button()` as sender plus an injected theme map; assert the back-colour write (`:817-820`)
- [ ] [P4-T148] Add `ButtonMouseLeave_WhenDialogResultIsOk_AppliesTheClickedColor` (#89, S)
  - Assert the clicked colour (`:824-827`)
- [ ] [P4-T149] Add `ButtonMouseLeave_WhenDialogResultIsNotOk_AppliesTheDefaultBackColor` (#90, S)
  - Closes the other arm (`:828-831`)
- [ ] [P4-T150] Add `ToggleExpansion_WhenCollapsed_RequestsExpansionOn` to `<TEST>/EfcItemController.NavigationTests.cs` (#91, S)
  - `_expanded` false via reflection; assert the `On` overload's effects (`:838-847`)
- [ ] [P4-T151] Add `ToggleExpansion_WhenExpanded_RequestsExpansionOff` (#92, S)
  - Closes the other arm (`:838-847`)
- [ ] [P4-T152] Add `ToggleExpansionAsync_WhenCollapsed_RequestsExpansionOn` (#93, S)
  - Same shape on the async overload (`:850-859`)
- [ ] [P4-T153] Add `ToggleExpansionAsync_WhenExpanded_RequestsExpansionOff` (#94, S)
  - Closes the other arm (`:850-859`)
- [ ] [P4-T154] Add `ToggleExpansionOn_NotifiesTheParentExpansionStyleHost` (#95, P)
  - `Mock<IEfcExpansionStyleHost>`; assert `ToggleExpansionStyle` (`:864`)
- [ ] [P4-T155] Add `ToggleExpansionOn_SetsBodyToggleColumnsToZeroAndOneHundredAndShowsBothPanes` (#96, S)
  - Surface mock; assert the two column widths and both visibility writes (`:867-871`)
- [ ] [P4-T156] Add `ToggleExpansionOn_WithUnreadItem_ArmsTheReadFormatTimer` (#97, S)
  - Assert `_timer` is non-null, then **dispose it in the test**. Never wait on the 4,000 ms due time (`:873-877`).
- [ ] [P4-T157] Add `ToggleExpansionOn_WithReadItem_DoesNotArmTheTimer` (#98, E)
  - Assert `_timer` remains null (`:873`)
- [ ] [P4-T158] Add `ToggleExpansionOn_WithNullItemInfo_DoesNotArmTheTimer` (#99, N)
  - Closes the null arm (`:873`)
- [ ] [P4-T159] Add `ToggleExpansionOn_RegistersBodyAndDetailJumpKeys` (#100, S)
  - Real `KbdActions<>`; assert `'B'` and `'D'` present (`:879-888`)
- [ ] [P4-T160] Add `ToggleExpansionOn_CalledTwice_ThrowsBecauseTheJumpKeysAreAlreadyRegistered` (#101, Err — characterizes #459)
  - `KbdActions<>.Add` throws `ArgumentException` on a duplicate `(sourceId, key)` pair (`KbdActions.cs:92-98`); pin the throw. **Do not fix.**
- [ ] [P4-T161] Add `ToggleExpansionOff_SetsBodyToggleColumnsToOneHundredAndZeroAndHidesBothPanes` (#102, S)
  - Mirror of task P4-T155 (`:892-897`)
- [ ] [P4-T162] Add `ToggleExpansionOff_DisposesAnArmedReadFormatTimer` (#103, S)
  - Pre-arm `_timer` via reflection; assert it is disposed (`:898-901`)
- [ ] [P4-T163] Add `ToggleExpansionOff_WithNoArmedTimer_IsANoOp` (#104, E)
  - Closes the null arm (`:898`)
- [ ] [P4-T164] Add `ToggleExpansionOff_RemovesBodyAndDetailJumpKeys` (#105, S)
  - Assert `'B'` and `'D'` removed (`:902-903`)
- [ ] [P4-T165] Add `ToggleExpansionAsyncOn_DispatchesToggleExpansionOnThroughTheViewerDispatcher` (#106, P)
  - `Mock<IUiDispatcher>` executing the action synchronously (precedent `QfcItemControllerTestSupport.BuildSyncDispatcher`, `:102-137`); assert dispatch (`:911-919`)
- [ ] [P4-T166] Add `ToggleExpansionAsyncOff_DispatchesToggleExpansionOffThroughTheViewerDispatcher` (#107, P)
  - Mirror (`:920-928`)
- [ ] [P4-T167] Add `ToggleExpansionOnPrivate_DoesNotRegisterJumpKeys_UnlikeTheSynchronousPath` (#108, Err — characterizes #459)
  - Invoke the private `ToggleExpansionOn()` by reflection; assert `'B'`/`'D'` are **not** registered, pinning the sync/async asymmetry (`:944-956`). **Do not fix.**
- [ ] [P4-T168] Add `ToggleExpansionOffPrivate_DoesNotRemoveJumpKeys_UnlikeTheSynchronousPath` (#109, Err — characterizes #459)
  - Mirror (`:931-942`)
- [ ] [P4-T169] Add `ToggleNavigation_WhenActive_DeactivatesAndUnregistersFocusActions` (#110, S)
  - Assert `_activeUI` flipped and the focus actions were removed (`:969-973`)
- [ ] [P4-T170] Add `ToggleNavigation_WhenInactive_ActivatesAndRegistersFocusActions` (#111, S)
  - Closes the other arm (`:974-978`)
- [ ] [P4-T171] Add `ToggleNavigationWithState_Off_WhenActive_Deactivates` (#112, S)
  - The overload consumed by `EfcFormController.cs:929,945` (`:984-988`)
- [ ] [P4-T172] Add `ToggleNavigationWithState_On_WhenInactive_Activates` (#113, S)
  - Closes the other arm (`:989-993`)
- [ ] [P4-T173] Add `ToggleNavigationWithState_Off_WhenAlreadyInactive_IsANoOp` (#114, E)
  - Assert no state change (`:984-993`)
- [ ] [P4-T174] Add `ToggleNavigationWithState_On_WhenAlreadyActive_IsANoOp` (#115, E)
  - Assert no state change (`:984-993`)
- [ ] [P4-T175] Add `ToggleNavigationAsync_Off_WhenActive_AwaitsTipsThenDeactivates` (#116, S)
  - The overload consumed by `EfcFormController.cs:938,954` (`:998-1003`)
- [ ] [P4-T176] Add `ToggleNavigationAsync_On_WhenInactive_AwaitsTipsThenActivates` (#117, S)
  - Closes the other arm (`:998-1008`)
- [ ] [P4-T177] Add `ToggleNavigationAsync_WhenStateAlreadyMatches_LeavesActiveUiUnchanged` (#118, E)
  - Assert no state change (`:999-1008`)
- [ ] [P4-T178] Add `ToggleTips_Asynchronous_PostsEachToggleThroughBeginInvoke` (#119, P)
  - `Mock<IItemViewer>.BeginInvoke` captures the delegate and the test invokes it (`:1015-1018`)
- [ ] [P4-T179] Add `ToggleTips_Synchronous_PostsEachToggleThroughInvoke` (#120, P)
  - Same over `Invoke` (`:1019-1022`)
- [ ] [P4-T180] Add `ToggleTipsWithState_Asynchronous_PostsDesiredStateThroughBeginInvoke` (#121, P)
  - Assert the desired state reached each tip (`:1030-1035`)
- [ ] [P4-T181] Add `ToggleTipsWithState_Synchronous_PostsDesiredStateThroughInvoke` (#122, P)
  - Same over `Invoke` (`:1036-1041`)
- [ ] [P4-T182] Add `ToggleTipsAsync_AwaitsEveryTipToggleConcurrently` (#123, P)
  - `Mock<IQfcTipsDetails>.ToggleAsync` returns `Task.CompletedTask`; assert all awaited (`:1050-1054`)
- [ ] [P4-T183] Add `ToggleTipsAsync_WithACancelledToken_ThrowsBeforeTogglingAnyTip` (#124, Err)
  - Pre-canceled token; assert the throw precedes any toggle (`:1047`)
- [ ] [P4-T184] Add `ToggleTipsAsync_WithAnEmptyTipsList_CompletesWithoutToggling` (#125, E)
  - Assert completion (`:1050-1054`)
- [ ] [P4-T185] Add `ToggleSaveAttachments_InvertsTheAttachmentsCheckStateThroughTheDispatcher` (#126, S)
  - `Mock<IUiDispatcher>` (S6) plus `Mock<IItemViewer>.AttachmentsChecked` (`:1065-1069`)
- [ ] [P4-T186] Add `ToggleSaveCopyOfMail_InvertsTheEmailCopyCheckStateThroughTheDispatcher` (#127, S)
  - Mirror over `EmailCopyChecked` (`:1074-1076`)
- [ ] [P4-T187] Add `SetThemeDark_WhenActiveThemeIsNull_SelectsDarkNormal` to `<TEST>/EfcItemController.ThemeTests.cs` (#128, E)
  - Closes the null arm of the two-way branch (`:1085-1089`)
- [ ] [P4-T188] Add `SetThemeDark_WhenActiveThemeIsANormalVariant_SelectsDarkNormal` (#129, S)
  - Closes the `Contains("Normal")` true arm (`:1085-1089`)
- [ ] [P4-T189] Add `SetThemeDark_WhenActiveThemeIsAnActiveVariant_SelectsDarkActive` (#130, S)
  - Closes the false arm (`:1090-1094`)
- [ ] [P4-T190] Add `SetThemeDark_SetsTheDarkModeBackingFieldWithoutWritingToOutlook` (#131, S)
  - Strict `Mock<IOlObjects>` with no `DarkMode` setter expectation; assert no write (`:1095`)
- [ ] [P4-T191] Add `SetThemeLight_WhenActiveThemeIsNull_SelectsLightNormal` (#132, E)
  - Mirror of task P4-T187 (`:1112-1116`)
- [ ] [P4-T192] Add `SetThemeLight_WhenActiveThemeIsAnActiveVariant_SelectsLightActive` (#133, S)
  - Mirror of task P4-T189 (`:1117-1121`)
- [ ] [P4-T193] Add `SetThemeLight_ClearsTheDarkModeBackingFieldWithoutWritingToOutlook` (#134, S)
  - Mirror of task P4-T190 (`:1122`)
- [ ] [P4-T194] Add `HtmlDarkConverter_BeforeWebViewInitialization_DoesNothing` (#135, N)
  - `_isWebViewerInitialized` false via reflection; assert no navigation (`:1100`)
- [ ] [P4-T195] Add `HtmlDarkConverter_AfterInitialization_NavigatesToTheToggledItemHtml` (#136, P)
  - Real `MailItemHelper`; assert `NavigateToString` (`:1102`)
- [ ] [P4-T196] Add `HtmlDarkConverter_AfterInitialization_TogglesEveryExpandedConversationItem` (#137, P)
  - `Mock<IConversationResolver>` with two expanded items; assert both were toggled (`:1103-1105`)
- [ ] [P4-T197] Add `ApplyReadEmailFormat_MarksTheItemReadAndSavesTheUnderlyingMailItem` (#138, S)
  - `MailItemHelper { Item = Mock<MailItem>.Object }` — `Item` is `virtual` (`MailItemHelper.Properties.cs:92`); assert `UnRead = false` and `Save()` (`:1127`)
- [ ] [P4-T198] Add `ApplyReadEmailFormat_AppliesTheMailRelatedControlGroupOfTheActiveTheme` (#139, P)
  - Build the `"MailRelated"` group with the **object-setter** constructor (`ThemeControlGroup.cs:102-114`) so `_controls` stays null and `ApplyTheme(bool)` (`:212-229`) takes the `else` branch, bypassing the static `UiThread.Dispatcher` entirely (`:1128`)
- [ ] [P4-T199] Add `SetOlvTheme_AppliesAHeaderFormatStyleWithTheGivenForeAndBackColorsToEveryColumn` (#140, P)
  - Real `OLVColumn` instances — plain objects, no handle required (`:1131-1138`)
- [ ] [P4-T200] Add `SetOlvTheme_WithNoColumns_IsANoOp` (#141, E)
  - Empty column list; assert no throw and no write (`:1137`)
- [ ] [P4-T201] Add `InitializeWebViewAsync_CreatesTheEnvironmentInTheLocalAppDataWebViewCacheFolder` to `<TEST>/EfcItemController.WebViewTests.cs` (#142, P)
  - `Mock<IWebViewCoreInitializer>`; assert the cache-folder argument (`:210-213`). **Do not correct** the U+2013 EN DASH incognito argument — characterizes #463.
- [ ] [P4-T202] Add `InitializeWebViewAsync_SwitchesToTheViewerSynchronizationContextBeforeCreatingTheEnvironment` (#143, S)
  - `Mock<IItemViewer>.UiSyncContext` returns `new SynchronizationContext()`; assert ordering (`:220-223`)
- [ ] [P4-T203] Add `InitializeWebViewAsync_EnsuresCoreWebViewOnTheBodyControlWithTheCreatedEnvironment` (#144, P)
  - Surface mock's `BodyWebView` returns null; assert `EnsureCoreWebView2Async` received it with the created environment (`:232-239`)
- [ ] [P4-T204] Add `InitializeWebViewAsync_WhenEnvironmentCreationFails_PropagatesTheFault` (#145, Err)
  - Pre-faulted `Task` from the initializer mock; assert the fault surfaces (`:223-239`)
- [ ] [P4-T205] Add `EfcDataModelSource_ExposesMailInfoConversationResolverAndMailFromTheUnderlyingModel` to `<TEST>/EfcDataModelSourceTests.cs` (#147, P)
  - Construct with `new EfcDataModel(globalsMock, mail: null, new CancellationTokenSource(), CancellationToken.None)` — `TryGetFirstInSelection` (`EfcDataModel.cs:234-252`) swallows the exception from a null `Ol.App`, so `Mail` and `ConversationResolver` are null and no COM object is touched. One test covers all three properties. **No exemption.**
- [ ] [P4-T206] Add `EfcDataModelSource_WithANullModel_ThrowsArgumentNullException` (#148, Err)
  - Assert `ArgumentNullException` from the constructor guard
- [ ] [P4-T207] Add `ItemViewerUiDispatcher_InvokeRunsTheActionOnTheViewerDispatcherThread` to `<TEST>/ItemViewerUiDispatcherTests.cs` (#149, P)
  - `QfcItemControllerTestSupport.StartRunningDispatcher()` plus a `Mock<IItemViewer>` returning that dispatcher; assert the action ran on the dispatcher's thread. `[TestCleanup]` calls `ShutdownDispatcher()`.
- [ ] [P4-T208] Add `ItemViewerUiDispatcher_InvokeAsyncCompletesAfterTheActionRuns` (#150, P)
  - Assert the returned operation completes and the action's side effect is observable. No `Task.Delay`.
- [ ] [P4-T209] Add `ItemViewerUiDispatcher_BeginInvokeDoesNotBlockTheCaller` (#151, P)
  - Use a `TaskCompletionSource` gate rather than any wall-clock wait to prove the caller returned before the action completed
- [ ] [P4-T210] Add `ItemViewerUiDispatcher_GenericInvokeAsyncReturnsTheFunctionResult` (#152, P)
  - Assert the returned value equals the function's result
- [ ] [P4-T211] Add `ItemViewerUiDispatcher_WithANullViewer_ThrowsArgumentNullException` (#153, Err)
  - Assert `ArgumentNullException` from the constructor guard
- [ ] [P4-T212] Add `EfcItemControllerDependencies_WithNoArguments_SuppliesEveryProductionDefault` to `<TEST>/EfcItemControllerDependenciesTests.cs` (#154, P)
  - Assert every seam property is non-null. Classify each default before asserting: `EfcThemeFactory` is a **named method group** (`EfcThemeHelper.SetupThemes`) and may be asserted with `.Method.Name`; the background runner (`f => Task.Run(f)`) and every other lambda-valued default are asserted with `NotBeNull()` only. **Never invoke a default.**
- [ ] [P4-T213] Add `EfcItemControllerDependencies_WithSuppliedSeams_PrefersTheSuppliedInstanceOverTheDefault` (#155, P)
  - Supply a sentinel for each seam; assert `BeSameAs(sentinel)` for interface-typed seams and `NotBeSameAs(productionDefault)` for lambda-valued delegate seams
- [ ] [P4-T214] Verify Phase 4 runs green and every production and test file stays under 500 lines
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` then the scoped vstest run with `/TestCaseFilter:"FullyQualifiedName~EfcItem|FullyQualifiedName~EfcDataModelSource|FullyQualifiedName~ItemViewerUiDispatcher"`. Split any test file exceeding 500 lines with a `.Part2.cs` suffix, register it, and rerun.
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase4-scoped-run.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with pass/fail counts and the line count of all eight production partials, the six new seam files, and every test file (all < 500)
- [ ] [P4-T215] Measure per-file coverage for all eight `EfcItemController` partials and the new seam files
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-efc-form-item-controller-coverage-452\evidence\qa-gates\coverage-phase4.cobertura.xml`, then F1's per-file harness over that output
  - Compiler-generated `<>c`, `<>c__DisplayClassN_M`, and `<M>d__N` classes share the declaring partial's `filename`; the union-by-`filename`, max-hits-per-line rule is load-bearing here
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase4-itemcontroller-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and **numeric** `LINE_COVERED / LINE_VALID` (>= 0.90 for the seven new partials and the three `testable` seam files, >= 0.80 for the retained primary) and `BRANCH_COVERED / BRANCH_VALID` (>= 0.75) per file, with `EfcItemControlSurface.cs` reported against its F1-ratified exemption rather than a floor, plus the `DERIVATION:` and `ISSUE_441_DISCLOSURE:` statements

### Phase 5 — EfcViewer.Designer.cs Disposition

`EfcViewer.Designer.cs` (4,277 lines; `issue.md:31` and `epic.md:114,389` say 4,276 — C10) carries no
`[ExcludeFromCodeCoverage]` of its own and is currently uninstrumented **solely** because of the
type-level attribute on the `EfcViewer` partial removed by task P2-T1. It is exempt from the 500-line
rule as generated code (`epic.md:254-255`, AC4) and is not split.

- [ ] [P5-T1] Verify the Designer file carries no attribute of its own and was not split
  - Command: grep `ExcludeFromCodeCoverage` across `QuickFiler/Viewers/EfcViewer.Designer.cs` and confirm the single `<Compile Include="Viewers\EfcViewer.Designer.cs">` entry with its `<DependentUpon>EfcViewer.cs</DependentUpon>` child at `QuickFiler.csproj:389-391` is unchanged
  - Acceptance: `<FEATURE>/evidence/qa-gates/designer-disposition.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, the grep result, and the unchanged csproj entry
- [ ] [P5-T2] [Approach A only] Verify the Designer file was not edited and record its measured rates
  - Command: `git diff --name-only origin/epic/quickfiler-per-file-coverage-integration...HEAD -- QuickFiler/Viewers/EfcViewer.Designer.cs` (expect empty), then obtain the Designer row by re-running F1's per-file harness (path from task P0-T7) over `<FEATURE>/evidence/qa-gates/coverage-phase4.cobertura.xml` scoped to that one additional file. Task P4-T215's enumerated file list covers only the `EfcItemController` partials and the new seam files, so it produces no Designer row even though its Cobertura XML contains the Designer file. This row is an interim reading; task P6-T2 over the task P6-T1 XML is the definitive measurement.
  - Expected shape from the committed report's comparable designers: ~99% line (`ItemViewerExpanded.Designer.cs` at `line-rate="0.9950980392156863"`, `coverage-final.cobertura.xml:4112`; `BayesianPerformanceViewer.Designer.cs` at `0.9914285714285714`, `:5683` — both 16-digit and therefore unmerged and trustworthy under DEC-2) and ~50% branch by construction
  - Acceptance: `<FEATURE>/evidence/qa-gates/designer-approach-a-measurement.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, an empty diff, and the file's **numeric** `LINE_COVERED / LINE_VALID` and `BRANCH_COVERED / BRANCH_VALID`
- [ ] [P5-T3] [Approach B only] Add method-level `[ExcludeFromCodeCoverage]` to the Designer's two generated methods and record the durability defect
  - Targets: `InitializeComponent` and `Dispose(bool)` at `QuickFiler/Viewers/EfcViewer.Designer.cs:18-25`. This is the only Approach B path that keeps the Designer file out of the denominator.
  - Record explicitly: Visual Studio regenerates `InitializeComponent` and silently drops the attribute (a durability defect), and there is zero repo precedent — a grep for `ExcludeFromCodeCoverage` across `**/*.Designer.cs` returns no matches
  - **`coverage.config` must not be edited.** A `.*\.Designer\.cs` exclusion there is a repo-root shared file (guaranteed cross-child conflict), would remove already-covered designer lines repository-wide and thus *lower* coverage, and is a Blocking finding under `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy.
  - Acceptance: `<FEATURE>/evidence/qa-gates/designer-approach-b-attribute.md` records `Timestamp:`, `EXIT_CODE:`, `Output Summary:`, the two attributed members, the durability-defect statement, and confirmation that `coverage.config` is unchanged
- [ ] [P5-T4] Append or confirm the `<LEDGER>` row for `EfcViewer.Designer.cs` using F1's bucket token and enforcement mechanism
  - Use the bucket and mechanism recorded by tasks P0-T5, P0-T11, and P0-T12. Under the DEC-5 semantics the file is **measured, counted toward repository-wide coverage, but not gated on the per-file 80/75 floors**; its ~0.50 branch rate is a construction artifact of `Dispose(bool)` (`components` initialized to `null` at `:12`, never reassigned), not a test gap (AC2).
  - If F1 has not answered the DEC-5 clarification, record `PENDING` and escalate rather than inventing a bucket
  - Acceptance: the `<LEDGER>` row exists with F1's literal token, or the artifact records the escalation
- [ ] [P5-T5] Record the net repository-wide line delta contributed by `EfcViewer.Designer.cs` entering the denominator (AC9 input)
  - Compute `LINE_COVERED` and `LINE_VALID` for the Designer file by the DEC-2 rule from the interim row task P5-T2 obtained by re-running F1's harness over `<FEATURE>/evidence/qa-gates/coverage-phase4.cobertura.xml` for that one file (task P4-T215's own enumerated file list does not include the Designer file), and state the resulting change to the repository-wide numerator and denominator. Task P6-T2 is the definitive measurement; if its Designer row differs, task P7-T5 supersedes this interim figure in the AC9 comparison. Under Approach A this is expected to be strongly positive (roughly 2,000 covered lines); under Approach B it is expected to be zero because the file stays out of the denominator.
  - If the delta is negative, state the specific mitigation applied. Do not add an exclusion.
  - Acceptance: `<FEATURE>/evidence/qa-gates/designer-line-delta.md` records `Timestamp:`, `EXIT_CODE:`, `Output Summary:`, the numeric delta, and (if negative) the mitigation

### Phase 6 — Per-File Coverage Verification

- [ ] [P6-T1] Produce the authoritative post-change Cobertura report
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-efc-form-item-controller-coverage-452\evidence\qa-gates\coverage-final.cobertura.xml`
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-final-run.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with total pass/fail counts, and the XML exists at the stated path
- [ ] [P6-T2] Build the per-file coverage table for every in-scope and newly created file
  - Run F1's per-file harness (path from task P0-T7) over the task P6-T1 XML. Files: the eight `EfcFormController*.cs` partials, the eight `EfcItemController*.cs` partials, `EfcViewer.cs`, `EfcViewer.Designer.cs`, `EfcFormLayoutMath.cs`, `EfcDataModelSource.cs`, `EfcItemControllerDependencies.cs`, `ItemViewerUiDispatcher.cs`, `EfcItemControlSurface.cs`, and the five interface files.
  - Each row carries `LINE_COVERED / LINE_VALID`, the computed line rate, `BRANCH_COVERED / BRANCH_VALID`, the computed branch rate, and the ledger bucket. A `0/0` file reports `N/A`, never `0%`.
  - The artifact must also carry: `DERIVATION: class/lines/line direct-child axis, grouped by class/@filename, deduped by @number with max(@hits)`, with an explicit statement that `@line-rate` was not read; `ISSUE_441_DISCLOSURE:` stating that the root `coverage/@lines-valid` and every `class/@line-rate` in the committed XML are inflated by open issue #441 and were not used; the merged-class branch-condition best-of limitation (`Merge-CoberturaClassesByFilename:240-261` picks the candidate line with the larger `Total` rather than unioning `<conditions>`); and the source XML path with the branch and commit it was produced on
  - Acceptance: `<FEATURE>/evidence/qa-gates/per-file-coverage-table.md` contains the full table with **numeric** values and all four disclosure statements. No placeholders.
- [ ] [P6-T3] Verify the AC1 line-coverage floors per file
  - Assert `>= 0.80` for `EfcViewer.cs` and the two retained primary partials, and `>= 0.90` for every F9-created production file. Interface-only files are reported `N/A` and are not subject to a floor. `EfcViewer.Designer.cs` is reported but not gated (DEC-5).
  - Acceptance: `<FEATURE>/evidence/qa-gates/ac1-line-floor-verification.md` records the per-file verdict table and an overall PASS/FAIL
- [ ] [P6-T4] Verify the AC2 branch-coverage floors per file
  - Assert `>= 0.75` for every `testable` file in scope, reported as an independent gate. `EfcViewer.Designer.cs` is excluded from this gate per DEC-5, subject to F1's ledger clarification.
  - Acceptance: `<FEATURE>/evidence/qa-gates/ac2-branch-floor-verification.md` records the per-file verdict table and an overall PASS/FAIL
- [ ] [P6-T5] Verify the AC5 triple for every newly created production file
  - For each new file assert: a `<Compile Include>` entry exists in `QuickFiler/QuickFiler.csproj`; a `<LEDGER>` row was appended in the same change; and the measured line rate is `>= 0.90` (or `N/A` for the `interface-only / not-measured` bucket, with no percentage floor and no `[ExcludeFromCodeCoverage]`). Confirm no shape-assertion test was written purely to manufacture coverage for an interface-only file.
  - Acceptance: `<FEATURE>/evidence/qa-gates/ac5-new-file-verification.md` records the three-column verdict table per file
- [ ] [P6-T6] Verify the AC4 file-size ceiling for every F9-touched and F9-created file
  - Compare against `<FEATURE>/evidence/baseline/file-size-baseline.md`. `EfcViewer.Designer.cs` is exempt as generated code.
  - Acceptance: `<FEATURE>/evidence/qa-gates/file-size-verification.md` lists each path with its final line count and a PASS/FAIL against 500

### Phase 7 — Final QC Loop, Repository-Wide Comparison, and Acceptance Check-Off

The four command steps P7-T1 through P7-T4 are **unconditional** and run in the mandated order. If
any step fails or changes files, fix and **restart from task P7-T1**; the recorded artifacts must come
from a single clean pass. `EXIT_CODE: SKIPPED` is not a valid outcome for any task in this phase.

- [ ] [P7-T1] Run the formatter and record the result
  - Commands: `dotnet tool run csharpier format .` then `dotnet tool run csharpier check QuickFiler QuickFiler.Test`
  - Do not use `csharpier .` (v0 syntax) and do not use `pipe-files` (stdout-only, non-enforcing). Do not run any formatter over a `.csproj`.
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-csharpier.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and whether `format` modified any file (if it did, restart at task P7-T1)
- [ ] [P7-T2] Run the analyzer build and record the result
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-msbuild-analyzers.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with warning and error counts compared against task P0-T17
- [ ] [P7-T3] Run the nullable/type-check build and record the result
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-msbuild-nullable.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` compared against task P0-T18
- [ ] [P7-T4] Run the coverage-enabled test suite and record numeric coverage
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-efc-form-item-controller-coverage-452\evidence\qa-gates\coverage-final.cobertura.xml`, then F1's per-file harness over that output
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with total pass/fail counts, the **numeric** repository-wide `LINE_COVERED / LINE_VALID` and computed rate, and the per-file numeric line and branch rates for every file in the task P6-T2 list. No placeholders.
- [ ] [P7-T5] Compute the repository-wide before/after coverage comparison (AC9)
  - Before: the task P0-T19 figure. After: the task P7-T4 figure. **Both derived by the DEC-2 rule so the comparison is like-for-like** — do not compare against the uncorrected 70.19% merge-base figure at `epic.md:479-481`. State the net line delta contributed by `EfcViewer.Designer.cs` entering the denominator (from task P5-T5) and, if the overall delta is negative, the specific mitigation applied.
  - Acceptance: `<FEATURE>/evidence/qa-gates/repo-wide-coverage-comparison.md` records `Timestamp:`, both numerators and denominators, both computed rates, the delta, the Designer contribution, and a RETAINED-OR-IMPROVED verdict
- [ ] [P7-T6] Verify the AC3 attribute dispositions in the final diff
  - Assert `[ExcludeFromCodeCoverage]` is absent from `EfcItemController.cs`, `EfcFormController.cs`, and `EfcViewer.cs`; absent from every `EfcFormController.*.cs` and `EfcItemController.*.cs` partial; absent from every F9-created file except `<VIEW>/EfcItemControlSurface.cs`; and that `ItemViewer.cs:20`'s F14-owned attribute is untouched
  - Command: `git diff origin/epic/quickfiler-per-file-coverage-integration...HEAD -- "*.cs" | Select-String ExcludeFromCodeCoverage`
  - Acceptance: `<FEATURE>/evidence/qa-gates/ac3-attribute-verification.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, and the verdict, plus a pointer to the task P0-T14 ratification record
- [ ] [P7-T7] Audit the test-safety, determinism, and convention invariants by inspection (AC6)
  - Confirm: MSTest, Moq, and FluentAssertions throughout with Arrange-Act-Assert; no temporary file, external service, live Outlook store, shown form, popup, message pump, or `DoEvents`; no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, or unseeded randomness; every `async void` handler observed with a `TaskCompletionSource`; every test that can reach `EditFiltersMenuItem_Click` or a `MessageBox.Show` site assigns the S5 seam; `EfcViewerQueue.Dequeue`, `EfcDataModel.CreateAsync`, the default `BreadcrumbHostFactory` body, and `FileIO2.WriteTextFile` are never invoked; every test class mutating a process-global static carries `[DoNotParallelize]` plus a restoring `[TestCleanup]`; no test is marked `LiveOutlook`; no `.Method.Name` assertion is made against a lambda-valued delegate
  - Acceptance: `<FEATURE>/evidence/qa-gates/test-safety-audit.md` lists each check, the search performed, and the result
- [ ] [P7-T8] Verify AC7 STA confinement and produce the test-file inventory
  - Under Approach A: exactly one `*.StaTests.cs` file exists (`<TESTV>/EfcViewer.StaTests.cs`), it is `[STATestClass]`, it constructs at most one never-shown `EfcViewer` per test disposed in a `finally`, and every test in it carries an XML doc comment stating why no seam could isolate the logic. Under Approach B: zero `*.StaTests.cs` files exist. In both cases: **no `*.StaTests.cs` file exists for `EfcItemController` or `EfcFormController`**, and `<TEST>/ItemViewerUiDispatcherTests.cs` is a plain `[TestClass]` using dispatcher infrastructure rather than the STA last-resort clause.
  - Command: glob `QuickFiler.Test/**/*.StaTests.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/ac7-sta-confinement.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, the full test-file inventory, and a pointer to the task P0-T14 ratification record
- [ ] [P7-T9] Verify AC10 no-behavior-change and the scope boundary
  - Command: `git diff --name-only origin/epic/quickfiler-per-file-coverage-integration...HEAD`
  - Assert no entry for `EfcHomeControllerDependencies.cs`, `EfcHomeControllerDependencyFactories.cs`, `EfcDataModel.cs`, `BreadcrumbBridgeRouter.cs`, `BreadcrumbOutboundQueue.cs`, `WebView2BreadcrumbHost.cs`, `IBreadcrumbWebHost.cs`, `WebView2CoreInitializer.cs`, `IItemViewer.cs`, `ItemViewer.cs`, `EfcViewerQueue.cs`, `EfcThemeHelper.cs`, `KeyboardHandler.cs`, `UtilitiesCS/Properties/AssemblyInfo.cs`, `coverage.config`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, `epic.md`, `EfcViewer3.cs`, `EfcViewer3.Designer.cs`, or any `*.props`/`*.targets`. Also assert both `EfcFormController` public constructor signatures and `Initialize()`/`InitializeWithoutData()`/`InitializeDataFields(EfcDataModel)` are unchanged, and that every new test entry point is an explicit overload rather than an optional parameter.
  - Acceptance: `<FEATURE>/evidence/qa-gates/scope-boundary-verification.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, the full changed-file list, and the verdict
- [ ] [P7-T10] Verify AC10 #439 characterization and record the fix-point relocation for the PR body
  - Confirm that no test asserts a multi-segment lineage, that the characterization tests for `PopulateFolderCombobox`, `SearchText_TextChanged`, `RefreshSuggestionsAsync`, `ActionDeleteAsync`, `BindFolderRows`, `BindBreadcrumbRowsAsync`, `ConfigureBreadcrumbControl`, `SelectedFolder`, and `IsValidSelection` assert that relative-stem rows pass through verbatim and that a row whose chain lookup yields `null` still binds, and that issue #439 is not fixed
  - Acceptance: `<FEATURE>/evidence/qa-gates/ac10-issue-439-characterization.md` records `Timestamp:`, the named characterization tests, the search performed for lineage assertions (result: none), and the statement for the PR body that the #439 fix point moved from `EfcFormController.cs:840-842` to `BreadcrumbRouterFactory`'s default body
- [ ] [P7-T11] Verify AC11 latent-defect promotion and record any execution-phase discoveries
  - Confirm none of #459, #460, #461, #463, #464, #465, #466, #467 is fixed, and that each characterization test pins current behavior. Promote any defect newly discovered during execution through the MCP promotion lifecycle before F9 completes and record its issue number.
  - Acceptance: `<FEATURE>/evidence/other/execution-phase-promotions.md` records `Timestamp:`, the eight pre-existing issue numbers with their characterizing tests, and one issue number and URL per newly discovered defect (or `none`)
- [ ] [P7-T12] Check off AC1-AC11 in `<FEATURE>/spec.md`
  - Per the `acceptance-criteria-tracking` skill: mark each criterion only when its cited evidence artifact exists and supports it; leave unmet criteria unchecked with a stated reason
  - Acceptance: all eleven checkboxes in `spec.md` carry a verdict and each cites its evidence artifact path
- [ ] [P7-T13] Check off AC1-AC11 in `<FEATURE>/user-story.md` in step with `spec.md`
  - Acceptance: the two files' AC states are identical; any divergence is a failure
- [ ] [P7-T14] Check off the `## Definition of Done` list in `<FEATURE>/spec.md`
  - Eleven items, each with its evidence artifact: AC verification in both files, DEC-1 ratification plus spike, halt gates G1-G8, both controllers split with every partial under 500 and above the floors, three attributes removed with one ratified exemption, the verbatim test migration, ledger rows appended in the same change, the F1-harness per-file evidence with both disclosure statements, the two epic-orchestrator correction notes, the DEC-5 clarification, and the full toolchain pass
  - Acceptance: every Definition of Done checkbox carries a verdict citing its evidence artifact path
- [ ] [P7-T15] Mirror the issue update for #452
  - Acceptance: `<FEATURE>/evidence/issue-updates/issue-452.<timestamp>.md` records `Timestamp:`, the exact text, and `PostedAs:` with the URL, or a `POSTING BLOCKED` header with the reason
- [ ] [P7-T16] Write the final status summary and confirm a clean single-pass toolchain run
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-status.md` records `Timestamp:`, the four command steps of the final pass with their `EXIT_CODE: 0` values, the AC1-AC11 status table, the per-file coverage summary, and an explicit statement that tasks P7-T1 through P7-T4 all passed within one uninterrupted loop

## Test Plan

- **Unit (MSTest + Moq + FluentAssertions, Arrange-Act-Assert):** 308 named test methods under
  Approach A and 299 under Approach B, one per atomic task — 12 in Phase 1 (`EfcFormLayoutMath` and
  the two structs), 26 or 17 in Phase 2 (`EfcViewer`, branch-dependent: 15 unconditional plus 11
  Approach-A or 2 Approach-B), 116 in Phase 3 (`EfcFormController`), and 154 in Phase 4
  (`EfcItemController` plus the new seam artifacts, after dropping research case #146 with the
  `InitializeWebView` deletion).
- **New test files:** `<TEST>/EfcFormLayoutMathTests.cs`, `<TESTV>/EfcViewerTests.cs`,
  `<TESTV>/EfcViewer.StaTests.cs` (Approach A only), `<TEST>/EfcFormController.TestSupport.cs`,
  `<TEST>/EfcFormControllerConstructionTests.cs`, `...PropertiesTests.cs`, `...SetupTests.cs`,
  `...EventHandlerTests.cs`, `...KeyboardTests.cs`, `...ActionsTests.cs`, `...BreadcrumbTests.cs`,
  `...TipsTests.cs`, `<TEST>/EfcItemController.TestSupport.cs`,
  `<TEST>/EfcItemController.ConstructionTests.cs`, `...ViewerSetupTests.cs`, `...PropertiesTests.cs`,
  `...EventWiringTests.cs`, `...EventHandlersTests.cs`, `...NavigationTests.cs`, `...ThemeTests.cs`,
  `...WebViewTests.cs`, `<TEST>/EfcDataModelSourceTests.cs`, `<TEST>/ItemViewerUiDispatcherTests.cs`,
  `<TEST>/EfcItemControllerDependenciesTests.cs` — each requiring an explicit `<Compile Include>`
  entry in `QuickFiler.Test/QuickFiler.Test.csproj`, plus any `.Part2.cs` split file.
- **Migrated test:** `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel`
  and `CreateMinimalController()` from `<TEST>/EfcFormControllerTests.cs`, moved verbatim by task
  P3-T139.
- **Integration / manual:** none. No live Outlook, no shown form, no popup, no external service.
- **Coverage evidence:**
  - Baseline: `<FEATURE>/evidence/baseline/coverage-baseline.md` and
    `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml`
  - Absence record: `<FEATURE>/evidence/baseline/absence-is-not-coverage.md`
  - Per-phase: `<FEATURE>/evidence/qa-gates/phase2-efcviewer-coverage.md`,
    `phase3-formcontroller-coverage.md`, `phase4-itemcontroller-coverage.md`
  - Post-change: `<FEATURE>/evidence/qa-gates/final-coverage.md` and
    `<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml`
  - Comparison: `<FEATURE>/evidence/qa-gates/per-file-coverage-table.md`,
    `ac1-line-floor-verification.md`, `ac2-branch-floor-verification.md`,
    `repo-wide-coverage-comparison.md`

## Open Questions / Notes

- **Upstream gate (blocking).** F1's per-file harness and `<LEDGER>` do not exist on this branch —
  verified at planning time that `docs/features/epics/quickfiler-per-file-coverage/` contains only
  `epic.md` and that no per-file coverage report generator exists anywhere in the repository. Tasks
  P0-T4 through P0-T9 are hard halt gates. This is by design for a wave-1 child, not a planning
  defect: the harness is an execution-time read, not a preflight-evaluable condition.
- **DEC-1 (blocking).** The `EfcViewer` Form-construction approach must be ratified by the maintainer
  at task P0-T14 before any Phase 2 work begins. Both branches share the S1 seam, the
  `IEfcFormViewer` implementation, and the N1-N15 test list, so a reversal costs one phase.
- **DEC-2 measurement rule.** Every acceptance number is computed from the direct-child
  `class/lines/line` axis grouped by `@filename`, deduplicated by `@number` taking `max(@hits)`. F9
  never reads `class/@line-rate`, `class/@branch-rate`, `coverage/@lines-valid`, or
  `coverage/@line-rate`, and never uses the `.//lines/line` descendant axis. Detection tell: a
  16-significant-digit rate was never merged and is trustworthy; a rate with six or fewer decimals has
  been through the defective path.
- **Disclosed, not fixed.** `Merge-CoberturaClassesByFilename:240-261` does not union `<conditions>`
  across a merged group; branch figures on merged files are a best-of, not a true union.
  `Invoke-MSTestWithCoverage.Helpers.ps1` is a shared file outside F9's assignment and must not be
  edited by this child.
- **DEC-5 clarification (non-blocking but required for AC2).** `EfcViewer.Designer.cs` needs a
  semantic F1's three buckets do not express — measured, counted toward repository-wide coverage, but
  not gated on the per-file floors. Requested at task P0-T12; the same reasoning applies to the other
  seven `*.Designer.cs` files in the epic (F14, F15).
- **Accepted residual.** The two-line forwarding shim for
  `WebView2Control_CoreWebView2InitializationCompleted` (task P4-T34) costs ~2 uncovered lines rather
  than a method-level exemption. No `[ExcludeFromCodeCoverage]` is added and `coverage.config` is not
  modified.
- **Single ratified exemption.** `<VIEW>/EfcItemControlSurface.cs` is the only new file proposed for
  `ratified-exempt`. If F1's ledger does not ratify it, halt rather than self-granting.
- **Watch items, not blockers.** F12 changing `BreadcrumbBridgeRouter`'s constructor arity, sealing
  off `SelectFirstRow`, or changing `SelectedFolderPath`'s derivation (`:364-380`) breaks F9's tests
  at fan-in (CCN-2). F4 adding an optional parameter to `EfcThemeHelper.SetupThemes` breaks the S4
  method-group conversion at compile time (CCN-3). F13 promoting `CoreInitialized` onto
  `IBreadcrumbWebHost` would let F9 widen the `BreadcrumbHostFactory` return type afterwards — a
  follow-up, not a prerequisite (CCN-4).
- **Fan-in expectation.** Conflicts on `QuickFiler.csproj` and `QuickFiler.Test.csproj` are expected,
  additive on both sides, and resolved by keeping both sets of entries (`epic.md:613-617`). Not a
  decomposition defect.
- **Latent defects.** None of #459, #460, #461, #463, #464, #465, #466, #467, or #439 is fixed here.
  Where a test pins a defect, the assertion is preserved rather than "corrected".
