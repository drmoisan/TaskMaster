# quickfiler-itemviewer-coverage — Atomic Implementation Plan

- **Issue:** #456
- **Parent epic:** `quickfiler-per-file-coverage`, issue #136 (child F14, wave 1)
- **Depends on:** F1 `quickfiler-coverage-ledger`, issue #432 (wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T22-38
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature
- **Feature folder:** `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456`

## Authoritative Sources

`spec.md` is the authoritative requirements document for this plan, including its twelve documented
deviations D1-D12 and its twelve acceptance criteria AC1-AC12. `user-story.md` mirrors the acceptance
criteria and is tracked independently. The ten artifacts under `research/` supply the per-file test-case
inventories this plan consumes verbatim; case IDs below are the research artifacts' own IDs and are not
re-derived here.

Policy stack, applied in this order: `CLAUDE.md`, `.claude/rules/general-code-change.md`,
`.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/quality-tiers.md`.

## Evidence Location Invariant (non-overridable)

Every evidence artifact produced by this plan resolves under
`docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/evidence/<kind>/`, where `<kind>` is
one of `baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/`. Paths under
`artifacts/` are forbidden. `<ts>` denotes the ISO-8601 `yyyy-MM-ddTHH-mm` timestamp captured at the
moment the task runs. Every command-bearing evidence artifact carries `Timestamp:`, `Command:`,
`EXIT_CODE:`, and `Output Summary:`.

## Upstream Dependency Handling — F1 (#432)

F1's feature folder, its per-file coverage harness, and
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` do **not** exist on this branch at
planning time. That is by design: F1 is wave 0 and merges to the epic integration branch before any
wave-1 child executes. The Phase 0 halt gate `[P0-T6]` is therefore an **execution-time** verification,
not a preflight-evaluable precondition; its acceptance is the production of the gate artifact recording
the verification outcome. Genuine absence at execution time is an epic-orchestrator sequencing failure
and is raised then, by halting.

## Measurement-Driven Pruning Rule (referenced by every test-case task)

Research inventories are upper bounds. `[P1-T13]` measures the seven newly-visible files. A test-case
task is satisfied in exactly one of two ways, and the phase evidence artifact records which:

1. The named test exists in the named file, is green, and covers the named production lines/branch
   outcomes; or
2. `[P1-T13]`'s measured per-file data shows **every** line and branch outcome in that case's target
   list is already covered, in which case the task records
   `PRUNED-AS-COVERED: <case id> — <measured citation>` in
   `evidence/qa-gates/pruning.<phase>.<ts>.md` and no test is written.

Option 2 is not available for any case whose target list contains a line or outcome the measurement
shows uncovered, and is never available for a case that pins a documented contract named in an
acceptance criterion (AC11 cases in particular).

## Standing Constraints (bind every task in this plan)

- **No `*.StaTests.cs` file may be created in `QuickFiler.Test`, and no `[STATestClass]` /
  `[STATestMethod]` attribute may be introduced** (spec D5, Non-Goals).
- **Every test that constructs a real `ItemViewer` or `ItemViewerExpanded` installs a
  `SynchronizationContext` before construction and restores the previous one in `finally`/
  `[TestCleanup]`.** `ItemViewer.cs:27` and `ItemViewerExpanded.cs:22` call
  `TaskScheduler.FromCurrentSynchronizationContext()`, which throws `InvalidOperationException` when
  `SynchronizationContext.Current` is null. Pattern:
  `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:336-338`.
- **Do not retype the Designer-backed fields/properties** `L0vhBreadcrumb_WebView2`, `TopicThread`,
  `SentDate`, `L0v2h2_WebView2`, `MoveOptionsMenu`. Seams are added as sibling overloads or injectable
  delegates only.
- **A seam default must never be written as a property/field initializer that references an instance
  member** — that is `error CS0236`. Use either a `private static` method-group default or an explicit
  setter method assigning a backing field.
- **Do not fix issue #438** (`quickfiler-search-keystroke-focus-steal`). Cases asserting the current
  `SetFolderDroppedDown(true)` -> `FocusBreadcrumb()` behavior carry an in-code comment citing #438 and
  stating the assertion pins current behavior. The same annotation discipline applies to #440 for the
  arrow-direction ternary and to `ToolStripMenuItemCb.cs:35-49` (#486) for the menu cases.
- **Do not re-exempt individual members** after the type attribute is removed (issue #457: method-level
  exclusion does not suppress compiler-hoisted lambdas). Do not add `<Sources><Exclude>` anywhere.
- **Frozen surfaces — no edit:** any `QfcItemController.*` file (F10), any breadcrumb bridge/messenger
  file (F12), any breadcrumb drop-down / WebView2 host file (F13), `ToolStripMenuItemCb.cs` (F15),
  `QfcHomeController.cs` (F7), `QuickFiler/Viewers/IItemViewer.cs`, both `*.Designer.cs` files,
  `coverage.config`, `TaskMaster.runsettings`, `scripts/vscode/Invoke-MSTestWithCoverage*.ps1`, and
  `UtilitiesCS/Properties/AssemblyInfo.cs`.
- **The only shared production file this feature edits is `QuickFiler/QuickFiler.csproj`**, and only to
  add one `<Compile Include>` entry. CRLF preserved, minimal adjacent hunk, no property or reference
  changes, no reordering. The same CRLF rule applies to every `QuickFiler.Test/QuickFiler.Test.csproj`
  edit.
- **Tests:** MSTest `[TestClass]`/`[TestMethod]`, Moq (Loose unless a case states Strict),
  FluentAssertions, Arrange-Act-Assert. No temporary files, no external services, no live `Form`, no
  popup, no `Thread.Sleep`/`Task.Delay`/wall-clock wait. `FakeTimeProvider`
  (`Microsoft.Extensions.TimeProvider.Testing`) is available and is the repo standard if a clock is ever
  needed; no file in scope reads a clock, so none is expected.
- **Coverage gates are independent:** `>= 80%` line **and** `>= 75%` branch per `testable` file;
  `>= 90%` line for `ControlColumnTrimmer.cs` as newly created production code. Figures come from F1's
  recomputed per-file numbers derived from deduplicated `<line>` children. A `<class>` `line-rate`
  attribute is never the acceptance figure; any `line-rate` quoted anywhere carries an explicit
  "#441 — unreliable" annotation. A file with zero `<condition>` children is reported branch **N/A**,
  never 0%.
- **No production file may exceed 500 lines.** Generated `*.Designer.cs` files are exempt as generated
  code and are recorded as such.

## Test File Inventory (each requires a `<Compile Include>` entry in `QuickFiler.Test/QuickFiler.Test.csproj`)

All under `QuickFiler.Test/Viewers/`:

| File | Created in | Cases |
| --- | --- | --- |
| `ItemViewerConstructionTests.cs` | Phase 1 | IV-1..IV-4, IV-11, IV-12, IV-17, IV-18 |
| `ItemViewerDisposalTests.cs` | Phase 1 | designer D1, D2 (Phase 1); D3 (Phase 11) |
| `ControlColumnTrimmerTests.cs` | Phase 2 | T10..T17 |
| `ItemViewerSurfaceTests.cs` | Phase 3 | IV-5..IV-10, IV-13..IV-16, IV-19 |
| `ItemViewerBreadcrumbWiringTests.cs` | Phase 4 | C1..C25 |
| `ItemViewerBreadcrumbWiringTestsPart2.cs` | Phase 4 | C26..C49 |
| `ItemViewerCommandsForwardingTests.cs` | Phase 5 | C1..C17 |
| `ItemViewerDisplayStateForwardingTests.cs` | Phase 6 | D1..D16 |
| `ItemViewerFolderSearchForwardingTests.cs` | Phase 7 | F1..F14 |
| `ItemViewerFolderSearchForwardingTestsPart2.cs` | Phase 7 | F15..F25 |
| `ItemViewerWebViewThreadTests.cs` | Phase 8 | C1..C8 |
| `ItemViewerExpandedTests.cs` | Phase 9 | T1, T2, T3, T6, T8, T9 |
| `ItemViewerExpandedMenuTests.cs` | Phase 9 | T4, T5, T7 |
| `ItemViewerExpandedDisposalTests.cs` | Phase 12 | ED1, ED2 |

Each new test file declares its own `private static T CreateUninitialized<T>()` helper over
`FormatterServices.GetUninitializedObject` (pattern:
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:331-335`) rather than reaching into another test
class. No test file may exceed 500 lines; the Part2 splits above are pre-planned for that reason.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Policy Reads, Upstream Gate, and Baseline Capture

- [ ] [P0-T1] Bootstrap the C# toolchain by running `pwsh -NoProfile -File scripts\vscode\Install-RepoDotNetSdk.ps1`,
      then `dotnet tool restore`, then `dotnet tool install --global dotnet-coverage` (or confirm it resolves).
      Acceptance: `evidence/baseline/toolchain-bootstrap.<ts>.md` records all three commands with
      `EXIT_CODE: 0`, plus resolving `dotnet tool run csharpier --version` and `dotnet-coverage --version`
      outputs.
- [ ] [P0-T2] Read, in policy order, `CLAUDE.md`, `.claude/rules/general-code-change.md`,
      `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/quality-tiers.md`
      from **this** worktree. Acceptance: `evidence/baseline/phase0-instructions-read.md` contains
      `Timestamp:`, `Policy Order:`, and the explicit list of files read with their worktree-relative paths.
- [ ] [P0-T3] Read `spec.md`, `user-story.md`, and `issue.md` in the feature folder and record the twelve
      deviations D1-D12 and the twelve acceptance criteria AC1-AC12 by identifier. Acceptance:
      `evidence/baseline/phase0-feature-documents-read.<ts>.md` lists D1-D12 with a one-line restatement
      each and AC1-AC12 with their target files.
- [ ] [P0-T4] Read all ten artifacts under `research/` and build a case index mapping each research case ID
      to its target production file and lines. Acceptance:
      `evidence/baseline/phase0-research-index.<ts>.md` lists every case ID used in Phases 2-12 with its
      source artifact filename.
- [ ] [P0-T5] Read `docs/features/epics/quickfiler-per-file-coverage/epic.md` sections "Shared Design" 1-6,
      "Coverage-Target Reconciliation", "Directives for F1's Ledger and Harness", "Mid-Wave File Creation
      and the Ledger Denominator", "Cross-Child Constraints Discovered During Preparation", and "### F14".
      Acceptance: `evidence/baseline/phase0-epic-read.<ts>.md` records the six section headings and the
      per-file gate figures 80/75/90 quoted from the reconciliation table.
- [ ] [P0-T6] **HALT GATE (F1, issue #432).** Verify all three conditions on this branch: (a) F1's per-file
      coverage harness is present and runnable, with its exact invocation recorded; (b)
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` exists and states the
      classification **rules** (not merely rows) for the three buckets `testable`, `ratified-exempt`,
      `interface-only / not-measured`;
      (c) the harness reports a zero-`<condition>` file as branch **N/A** and a compiled file with no
      `<class>` element as line **N/A**, each demonstrated by running the harness against the committed
      indicative Cobertura report at
      `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
      and reading the resulting rows for `QuickFiler\Viewers\IItemViewer.cs` (no `<class>` element — must
      read line `N/A`, never a synthesised 0%) and for a zero-`<condition>` file present in that report
      (must read branch `N/A`, never 0%), with the concrete file names recorded.
      Acceptance: `evidence/baseline/f1-harness-gate.<ts>.md` records the
      harness invocation, the three verdicts, and the concrete files used for (c). **If any of (a), (b),
      or (c) fails, HALT the plan, mark this task blocked, and escalate to F1/#432; do not run any
      measurement task and do not proceed to Phase 1.**
- [ ] [P0-T7] Capture the formatting baseline with `dotnet tool run csharpier check .`. Acceptance:
      `evidence/baseline/csharpier-check.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`,
      `Output Summary:` naming any pre-existing unformatted files.
- [ ] [P0-T8] Capture the analyzer baseline with
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
      Acceptance: `evidence/baseline/msbuild-analyzers.<ts>.md` with the four schema fields and the
      warning/error counts.
- [ ] [P0-T9] Capture the nullable/type-check baseline with
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
      Acceptance: `evidence/baseline/msbuild-nullable.<ts>.md` with the four schema fields and the
      warning/error counts.
- [ ] [P0-T10] Capture the coverage-mode test baseline with
      `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-itemviewer-coverage-456\evidence\baseline\coverage-baseline.cobertura.xml`,
      first confirming no nested `.claude\worktrees\**\bin\Debug` output is inside the search root.
      Acceptance: `evidence/baseline/coverage-run.<ts>.md` with the four schema fields plus **numeric**
      repository-wide line and branch headline values read from the emitted Cobertura root `<coverage>`
      element, and the test pass/fail counts.
- [ ] [P0-T11] Run F1's harness (invocation from `[P0-T6]`) against
      `evidence/baseline/coverage-baseline.cobertura.xml` and record the per-file line and branch figure
      for all ten in-scope production files. Acceptance:
      `evidence/baseline/per-file-baseline.<ts>.md` carries one row per file with recomputed line rate,
      branch rate or `N/A`, and an explicit `ABSENT (suppressed by ItemViewer.cs:20)` marker for each file
      that emits no `<class>` element.
- [ ] [P0-T12] Record the AC8 "before" repository-wide figure and the tree state. Acceptance:
      `evidence/baseline/repo-wide-before.<ts>.md` records the numeric repository-wide line and branch
      rates from `[P0-T10]`, the current `git rev-parse HEAD`, and `git status --porcelain` output
      demonstrating a clean tree; the recorded SHA is context, not a gate.
- [ ] [P0-T13] Record the physical line count of each of the ten in-scope production files plus
      `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`'s current shape. Acceptance:
      `evidence/baseline/file-sizes-before.<ts>.md` lists file, line count, and 500-line-rule applicability
      (both `*.Designer.cs` files marked exempt as generated code).

### Phase 1 — Attribute Removal and First Measurement (T0/T0b)

Per spec deviation D2 this phase must land as one change: removing `[ExcludeFromCodeCoverage]` un-hides
seven files at once, and the designer's present coverage is incidental and sibling-owned. The F14-owned
construction fixture and the two `ItemViewer.Designer.cs` `Dispose` tests are authored here, not later.

- [ ] [P1-T1] Remove the `[ExcludeFromCodeCoverage]` attribute at `QuickFiler/Viewers/ItemViewer.cs:20`.
      Acceptance: the attribute is absent; `grep -n ExcludeFromCodeCoverage QuickFiler/Viewers/ItemViewer*.cs`
      returns only comment lines; no attribute is added to `ItemViewer.Designer.cs`, to any other partial,
      or to any individual member.
- [ ] [P1-T2] Remove `using System.Diagnostics.CodeAnalysis;` at `QuickFiler/Viewers/ItemViewer.cs:5` after
      verifying no other symbol in that file resolves through it. Acceptance: the directive is gone and the
      analyzer build in `[P1-T11]` is clean; no other `using` directive in the file is touched.
- [ ] [P1-T3] Rewrite the stale exemption comment at `QuickFiler/Viewers/ItemViewer.Commands.cs:10` so it no
      longer asserts that the type is `[ExcludeFromCodeCoverage]`. Acceptance: the comment states that the
      partial is measured and carries no exemption; the string `[ExcludeFromCodeCoverage]` no longer appears
      as an assertion of current state in that file.
- [ ] [P1-T4] Rewrite the stale exemption comment at `QuickFiler/Viewers/ItemViewer.DisplayState.cs:9-10`,
      **retaining** the CS0579 note (moved if the comment is rewritten) because it documents why per-partial
      exemption is impossible. Acceptance: the exemption assertion is gone and the CS0579 rationale is still
      present in the file.
- [ ] [P1-T5] Rewrite the stale exemption comment at `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:17`.
      Acceptance: the exemption assertion is gone; the surrounding #351 design-intent comment at `:9-16` is
      unchanged.
- [ ] [P1-T6] Rewrite the header comment block at `QuickFiler/Viewers/ItemViewer.WebViewThread.cs:8-12`.
      Acceptance: the exemption assertion is gone; the Seam D / Cluster 2d intent and the `SentDate`
      encapsulation note are preserved.
- [ ] [P1-T7] Create `QuickFiler.Test/Viewers/ItemViewerConstructionTests.cs` with a `[TestInitialize]` that
      saves `SynchronizationContext.Current` and installs `new SynchronizationContext()`, a `[TestCleanup]`
      that disposes the viewer and restores the previous context, and case **IV-1**
      `Constructor_CapturesAmbientSyncContextSchedulerAndDispatcher` asserting `UiSyncContext`,
      `UiScheduler`, and `UiDispatcher` are non-null after `new QuickFiler.ItemViewer()`. Acceptance: the
      file exists, is a plain `[TestClass]` with no STA attribute, and IV-1 is green.
- [ ] [P1-T8] Create `QuickFiler.Test/Viewers/ItemViewerDisposalTests.cs` with a test-local
      `private sealed class DisposeProbe : QuickFiler.ItemViewer { internal void DisposeUnmanagedOnly() => base.Dispose(false); }`
      and designer case **D1** `Dispose_WhenDisposingIsFalse_SkipsComponentDisposalAndCallsBase`, covering the
      jump-0-false outcome of `ItemViewer.Designer.cs:18`. Acceptance: the file exists, the context fixture
      from `[P1-T7]` is replicated, and D1 is green with no reflection used to reach `Dispose(bool)`.
- [ ] [P1-T9] Add designer case **D2** `Dispose_WhenDisposingIsTrue_EvaluatesComponentGuardAndCallsBase` to
      `QuickFiler.Test/Viewers/ItemViewerDisposalTests.cs`, calling public `Dispose()` on a constructed
      viewer to pin the jump-0-true / jump-1-false outcomes. Acceptance: D2 is green and its in-code comment
      records that it pins an outcome previously supplied only by an unidentified cross-test disposal
      cascade.
- [ ] [P1-T10] Add `<Compile Include="Viewers\ItemViewerConstructionTests.cs" />` and
      `<Compile Include="Viewers\ItemViewerDisposalTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
      inside the existing `Viewers\` block (adjacent to `:80`). Acceptance: both entries present, CRLF
      preserved, `git diff --stat` shows exactly two added lines in that file and no reordering.
- [ ] [P1-T11] Run the analyzer build and then the nullable build with the commands from `[P0-T8]` and
      `[P0-T9]`. Acceptance: `evidence/qa-gates/phase1-build.<ts>.md` records both commands with
      `EXIT_CODE: 0` and no new warnings relative to `[P0-T8]`/`[P0-T9]`.
- [ ] [P1-T12] Re-run the coverage suite with the `[P0-T10]` command, writing to
      `evidence/baseline/coverage-after-t0.cobertura.xml`. Acceptance:
      `evidence/baseline/coverage-after-t0-run.<ts>.md` carries the four schema fields plus numeric
      repository-wide line and branch values.
- [ ] [P1-T13] Run F1's harness against `evidence/baseline/coverage-after-t0.cobertura.xml` and record the
      **measured** per-file line and branch rate for all seven newly-visible files (`ItemViewer.cs`,
      `.Breadcrumb.cs`, `.Commands.cs`, `.DisplayState.cs`, `.FolderSearch.cs`, `.WebViewThread.cs`,
      `.Designer.cs`) plus `ItemViewerExpanded.cs`, `ItemViewerExpanded.Designer.cs`, and the `N/A` row for
      `IItemViewer.cs`. Acceptance: `evidence/baseline/measured-per-file-after-t0.<ts>.md` carries one row
      per file with a recomputed line rate, a branch rate or `N/A`, and the per-line uncovered list for the
      six hand-written partials; no `<class>` `line-rate` attribute is quoted without a
      "#441 — unreliable" annotation.
- [ ] [P1-T14] Derive the pruning ledger from `[P1-T13]` by evaluating the Measurement-Driven Pruning Rule
      against every case ID in Phases 2-12. Acceptance:
      `evidence/qa-gates/pruning.phase1.<ts>.md` lists every case ID with verdict `AUTHOR` or
      `PRUNED-AS-COVERED` plus the measured citation supporting each `PRUNED-AS-COVERED`.

### Phase 2 — ControlColumnTrimmer Seam Extraction (S1)

`ItemViewer.cs:77-95`, `:97-107`, `:137-164` and `ItemViewerExpanded.cs:69-87`, `:89-99`, `:129-156` are
verbatim duplicates. Extract once; point both at it. This is the feature's only new production file.
Verified: no consumer reads a pre-transform value between the rewired call sites — the only intervening
member is `InitControlGroups`, whose sole reference is the commented-out line at `ItemViewer.cs:132` /
`ItemViewerExpanded.cs:124`.

- [ ] [P2-T1] Create `QuickFiler/Viewers/ControlColumnTrimmer.cs` declaring
      `internal static class ControlColumnTrimmer` with
      `internal static void RemoveColumnsRightOf(Control root, Control furthestRight, Control columnSpanTarget)`,
      `internal static void RemoveControlsRightOf(Control root, Control furthestRight)`, and
      `internal static List<Control> ControlsRightOf(Control root, Control furthestRight)`, bodies copied
      verbatim from `ItemViewer.cs:79-94`, `:99-106`, `:139-163` with `this` replaced by `root` and
      `L0v2h2_WebView2` replaced by `columnSpanTarget`. Acceptance: the file compiles, is under 500 lines,
      carries XML doc comments on all three members, and contains no `ItemViewer` reference and no
      `[ExcludeFromCodeCoverage]`.
- [ ] [P2-T2] Add `<Compile Include="Viewers\ControlColumnTrimmer.cs" />` to
      `QuickFiler/QuickFiler.csproj` inside the `Viewers\` block adjacent to `:392`. Acceptance: exactly one
      added line, CRLF preserved, no property change, no reference change, no reordering of unrelated
      entries; `git diff QuickFiler/QuickFiler.csproj` shows a single-line hunk.
- [ ] [P2-T3] Replace `QuickFiler/Viewers/ItemViewer.cs:77-95` with
      `public void RemoveControlsColsRightOf(Control furthestRight) => ControlColumnTrimmer.RemoveColumnsRightOf(this, furthestRight, L0v2h2_WebView2);`
      and delete the now-unreferenced private `RemoveControlsRightOf` (`:97-107`) and `ControlsRightOf`
      (`:137-164`). Acceptance: the public signature is byte-identical to its previous declaration, the two
      private members are gone, and no other member of the file references them.
- [ ] [P2-T4] Remove the `using System.Linq;` and `using System.Drawing;` directives from
      `QuickFiler/Viewers/ItemViewer.cs` (verified unused after `[P2-T3]`: `Point`, `Size`, `Any`, `First`,
      `Where`, `Select` occurred only in the extracted bodies). Acceptance: both directives are gone, the
      analyzer build in `[P2-T21]` is clean, and no pre-existing unused directive (`System.Data`,
      `System.Text`) is touched.
- [ ] [P2-T5] Update the stale comment at `QuickFiler/Viewers/ItemViewer.cs:132` so it no longer names a
      method that this file no longer declares. Acceptance: the comment references
      `ControlColumnTrimmer.ControlsRightOf` or is removed; no executable line changes.
- [ ] [P2-T6] Replace `QuickFiler/Viewers/ItemViewerExpanded.cs:69-87` with the delegating one-liner and
      delete the private `RemoveControlsRightOf` (`:89-99`) and `ControlsRightOf` (`:129-156`). Acceptance:
      as `[P2-T3]`, for `ItemViewerExpanded`.
- [ ] [P2-T7] Remove the `using System.Linq;` and `using System.Drawing;` directives from
      `QuickFiler/Viewers/ItemViewerExpanded.cs`. Acceptance: as `[P2-T4]`, for `ItemViewerExpanded`.
- [ ] [P2-T8] Update the stale comment at `QuickFiler/Viewers/ItemViewerExpanded.cs:124`. Acceptance: as
      `[P2-T5]`.
- [ ] [P2-T9] Verify the extraction changed no contract: `QuickFiler/Viewers/IItemViewer.cs:131`
      (`void RemoveControlsColsRightOf(Control furthestRight);`) and
      `QuickFiler/Controllers/EfcItemController.cs:247` are untouched. Acceptance:
      `git diff --name-only` contains neither path, and
      `evidence/qa-gates/s1-contract-unchanged.<ts>.md` records both file paths with their unchanged line
      text.
- [ ] [P2-T10] Append a ledger row for `QuickFiler/Viewers/ControlColumnTrimmer.cs` to
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` in the same change as
      `[P2-T2]`, bucket `testable`, target `>= 90% line`, per the epic's Mid-Wave File Creation rules 3
      and 4. Acceptance: the row exists, names the file, bucket, target, and the creating child F14/#456.
- [ ] [P2-T11] Create `QuickFiler.Test/Viewers/ControlColumnTrimmerTests.cs` as a plain `[TestClass]` with
      `Panel`/`Label`/`TableLayoutPanel` fixtures whose `Location` and `Size` are set explicitly so no layout
      pass is required, all controls disposed in `finally`, and add
      `<Compile Include="Viewers\ControlColumnTrimmerTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`.
      Acceptance: the file and the csproj entry exist, CRLF preserved, no STA attribute anywhere.
- [ ] [P2-T12] Add case **T10** `RemoveColumnsRightOf_WhenParentIsTableLayoutPanel_TrimsTrailingColumns`
      covering the `:71`-true and `:77`-true outcomes with `furthestRight` in a non-last TLP column.
      Acceptance: green; asserts `ColumnCount` decreased by the expected amount.
- [ ] [P2-T13] Add case **T11** `RemoveColumnsRightOf_WhenTargetIsInLastColumn_LeavesColumnsIntact` covering
      the `:77`-false outcome. Acceptance: green; asserts `ColumnCount` unchanged.
- [ ] [P2-T14] Add case **T12** `RemoveColumnsRightOf_WhenParentIsNotTableLayoutPanel_FallsBackToControlRemoval`
      covering the `:71`-false outcome with a `Panel`-parented control. Acceptance: green; asserts the
      fallback path ran.
- [ ] [P2-T15] Add case **T13** `RemoveControlsRightOf_WhenControlsExistToTheRight_RemovesAndDisposesThem`
      covering the `:92`-true outcome. Acceptance: green; asserts removal from `Parent.Controls` and
      `IsDisposed == true`.
- [ ] [P2-T16] Add case **T14** `RemoveControlsRightOf_WhenNothingIsToTheRight_MakesNoChange` covering the
      `:92`-false outcome. Acceptance: green; asserts the control collection is unchanged.
- [ ] [P2-T17] Add case **T15** `ControlsRightOf_WhenAnchorIsInTree_UsesItsWalkedLocationAsLimit` covering the
      `:143`-true and `:152`-true outcomes with a nested child whose walked location differs from
      `Location`. Acceptance: green; asserts the returned set.
- [ ] [P2-T18] Add case **T16** `ControlsRightOf_WhenAnchorIsOutsideTree_FallsBackToAnchorOwnLocation`
      covering the `:143`-false outcome. Acceptance: green; anchor not parented into the walked root.
- [ ] [P2-T19] Add case **T17** `ControlsRightOf_WhenAllControlsAreLeftOfLimit_ReturnsEmpty` covering the
      `:152`-false outcome. Acceptance: green; asserts an empty result.
- [ ] [P2-T20] Verify file sizes after extraction. Acceptance:
      `evidence/qa-gates/file-sizes-after-s1.<ts>.md` records `ItemViewer.cs` and `ItemViewerExpanded.cs`
      line counts, both strictly below 500 and both strictly below their `[P0-T13]` counts, plus
      `ControlColumnTrimmer.cs` below 500.
- [ ] [P2-T21] Run the analyzer build and the nullable build. Acceptance:
      `evidence/qa-gates/phase2-build.<ts>.md` records both with `EXIT_CODE: 0` and no new warnings.
- [ ] [P2-T22] Measure `QuickFiler/Viewers/ControlColumnTrimmer.cs` with F1's harness. Acceptance:
      `evidence/qa-gates/coverage.controlcolumntrimmer.<ts>.md` records the four schema fields plus a
      recomputed line rate `>= 90%` and a branch rate `>= 75%`, both taken from deduplicated `<line>` /
      `<condition>` data, never from a `<class>` `line-rate` attribute.

### Phase 3 — ItemViewer.cs

Post-S1 shape: ~383 lines, ~156 coverable lines, one branch point at `:179` in a member with no reachable
caller (deviation D11). Three members are unreachable from any call site or designer wiring:
`MenuItem_CheckedChanged(object, EventArgs)` at `:171`, `MenuItem_CheckedChanged(ToolStripMenuItem)` at
`:177` (called only from `:171`, and therefore transitively unreachable), and
`MoveOptionsMenu_Click(object, EventArgs)` at `:205`. Option B is adopted — all three are **kept** and made
reachable to tests through seam S2b, because AC9 confines production edits to seam addition, visibility
widening, extraction of verbatim duplicates, comment correction, and attribute removal; member deletion is
outside that enumeration. Seam S2b is exactly the visibility widening performed by `[P3-T1]` (`:177`) and
`[P3-T2]` (`:171` and `:205`); no other member is widened and no reflection is used.

- [ ] [P3-T1] Widen `QuickFiler/Viewers/ItemViewer.cs`'s `MenuItem_CheckedChanged(ToolStripMenuItem)`
      (`:177`) from `private` to `internal static`. Acceptance: the call at `:174` still compiles unchanged,
      the method reads no instance state, and the analyzer build is clean.
- [ ] [P3-T2] Widen `QuickFiler/Viewers/ItemViewer.cs`'s `MenuItem_CheckedChanged(object, EventArgs)`
      (`:171`) from `private` to `internal`, keeping it an instance method, and widen
      `MoveOptionsMenu_Click(object, EventArgs)` (`:205`) from `private` to `internal`, also keeping it
      an instance method. Both are unreferenced and undesigner-wired (verified: zero call sites in the
      solution), so widening is behavior-neutral and falls under AC9's "visibility widening". Acceptance:
      both signatures and bodies are otherwise unchanged; both remain `EventHandler`-compatible instance
      methods; no designer wiring is added or removed; the analyzer build is clean.
- [ ] [P3-T3] Create `QuickFiler.Test/Viewers/ItemViewerSurfaceTests.cs` using the `U` fixture
      (`CreateUninitialized<ItemViewer>()`, no constructor, therefore no `SynchronizationContext`
      requirement) and add its `<Compile Include>` entry to `QuickFiler.Test/QuickFiler.Test.csproj`.
      Acceptance: file and entry exist, CRLF preserved, plain `[TestClass]`.
- [ ] [P3-T4] Add case **IV-2** `Constructor_WithNoAmbientSynchronizationContext_Throws` to
      `ItemViewerConstructionTests.cs`, clearing `SynchronizationContext.Current` before construction.
      Acceptance: green; asserts `InvalidOperationException` from `ItemViewer.cs:27` and restores the
      previous context in `finally`.
- [ ] [P3-T5] Add case **IV-3** `Constructor_PopulatesTipsLabelsInDeclaredOrder` to
      `ItemViewerConstructionTests.cs`, covering `:110-128`, `:135`, and the `TipsLabels` getter.
      Acceptance: green; asserts the eleven designer `Label` instances in declared order by reference.
- [ ] [P3-T6] Add case **IV-4** `Constructor_PopulatesLeftAndExpandedTipsLabels` to
      `ItemViewerConstructionTests.cs`, covering `:130`, `:134`, and the two getters. Acceptance: green.
- [ ] [P3-T7] Add case **IV-5** `ControllerProperty_RoundTripsAssignedValue` to `ItemViewerSurfaceTests.cs`,
      covering `:55-56` with a `Mock<IItemControler>`. Acceptance: green; getter returns the assigned mock.
- [ ] [P3-T8] Add case **IV-6** `LabelProperties_RoundTripAssignedControls` to `ItemViewerSurfaceTests.cs`,
      covering the `Label` accessor pairs in `:209-278`, `:304-308`, `:329-333`, `:339-343`, `:349-353`,
      `:364-378`, `:424-428`. Acceptance: green; each assigned `Label` is returned by reference.
- [ ] [P3-T9] Add case **IV-7** `TextAndListProperties_RoundTripAssignedControls` covering `:279-303` and
      `:389-393`. Acceptance: green.
- [ ] [P3-T10] Add case **IV-8** `WebViewAndLayoutProperties_RoundTripAssignedControls` covering `:309-328`
      using `CreateUninitialized<WebView2>()` for the WebView2 property. Acceptance: green; the declared
      property types are not changed by this test or any other task.
- [ ] [P3-T11] Add case **IV-9** `ButtonSvgProperties_RoundTripAssignedControls` covering `:334-338`,
      `:344-348`, `:354-363`, `:379-388` with `SVGControl.ButtonSVG` instances. Acceptance: green.
- [ ] [P3-T12] Add case **IV-10** `MenuProperties_RoundTripAssignedControls` covering `:394-423` with a
      `MenuStrip`-free `ToolStripMenuItem`/`ToolStripMenuItemCb` set. Acceptance: green; no popup is shown.
- [ ] [P3-T13] Add case **IV-11** `MenuItems_FirstAccess_LoadsFiveMenuComponentsInDeclaredOrder` to
      `ItemViewerConstructionTests.cs`, covering `:189` and `:193-202`. Acceptance: green; asserts five
      components in declared order.
- [ ] [P3-T14] Add case **IV-12** `MenuItems_SecondAccess_ReturnsCachedInstance` to
      `ItemViewerConstructionTests.cs`, covering the `Initializer.GetOrLoad` cached path at `:189`.
      Acceptance: green; `BeSameAs` on the two reads.
- [ ] [P3-T15] Add case **IV-13** `MenuItemCheckedChanged_WhenChecked_AppliesCheckedImage` to
      `ItemViewerSurfaceTests.cs`, invoking the S2b `internal static` overload with
      `new ToolStripMenuItem { Checked = true }` to take the `:179`-true outcome. Acceptance: green;
      asserts `Image` equals `Properties.Resources.CheckBoxChecked`. **This case is load-bearing for the
      branch gate on this file.** In-code comment cites `ToolStripMenuItemCb.cs:35-49` and issue #486.
- [ ] [P3-T16] Add case **IV-14** `MenuItemCheckedChanged_WhenUnchecked_ClearsImage` covering the
      `:179`-false outcome and `:185`. Acceptance: green; asserts `Image` is null.
- [ ] [P3-T17] Add case **IV-15**
      `MenuItemCheckedChangedHandler_WhenSenderIsMenuItem_DelegatesToTypedOverload` covering `:173-174`.
      Acceptance: green.
- [ ] [P3-T18] Add case **IV-16**
      `MenuItemCheckedChangedHandler_WhenSenderIsNotMenuItem_ThrowsInvalidCast` covering the `:173` cast
      failure path. Acceptance: green; asserts `InvalidCastException` with an in-code comment recording
      that guarding the cast is out of scope under the no-behavior-change NFR.
- [ ] [P3-T19] Add case **IV-17** `WebViewParentChanged_WhenReparented_InvokesWiredHandler` to
      `ItemViewerConstructionTests.cs`, covering `:167-169` by re-parenting `L0v2h2_WebView2` into a local
      `Panel`. Acceptance: green; the designer wiring at `ItemViewer.Designer.cs:256` is neither added nor
      removed.
- [ ] [P3-T20] Add case **IV-18** `RemoveControlsColsRightOf_DelegatesToTrimmerWithWebViewSpanTarget` to
      `ItemViewerConstructionTests.cs`, covering the S1 delegation line. Acceptance: green; asserts the
      observable `TableLayoutPanel` mutation rather than mocking the static trimmer.
- [ ] [P3-T21] Add case **IV-19** `MoveOptionsMenuClick_DoesNothing` to `ItemViewerSurfaceTests.cs`,
      covering `:205` by invoking the `internal` `MoveOptionsMenu_Click(null, EventArgs.Empty)` widened
      in `[P3-T2]` on the `U` fixture. Acceptance: green; asserts the invocation does not throw and
      mutates no state; no reflection is used and no designer wiring is added.
- [ ] [P3-T22] Verify `QuickFiler/Viewers/ItemViewer.cs` line count. Acceptance:
      `evidence/qa-gates/file-size.itemviewer.<ts>.md` records a count strictly below 500 and strictly below
      the 432 recorded in `[P0-T13]`.
- [ ] [P3-T23] Measure `QuickFiler/Viewers/ItemViewer.cs` with F1's harness. Acceptance:
      `evidence/qa-gates/coverage.itemviewer.<ts>.md` carries the four schema fields plus a recomputed line
      rate `>= 80%` and branch rate `>= 75%` (or `N/A` if the harness reports zero `<condition>` children),
      with no `<class>` `line-rate` attribute quoted un-annotated.

### Phase 4 — ItemViewer.Breadcrumb.cs

Branch-dense: ~26 decision points. The branch gate binds, not the line gate. Seams S3a, S3b, and S4 all
edit this file at non-overlapping locations and are merged into this single phase per spec. Case C22
(the two geometry closure bodies at `:172-175`) is **not authored** — covering it would require the
first `*.StaTests.cs` file in `QuickFiler.Test`, which spec D5 prohibits.

- [ ] [P4-T1] Widen `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:25` from
      `internal BreadcrumbBridgeCoordinator BreadcrumbCoordinator { get; private set; }` to
      `{ get; internal set; }` (seam S4). Acceptance: exactly one token changed; the production write at
      `:59` still compiles; the property remains `internal`, so no public surface changes.
- [ ] [P4-T2] Add an `internal` sibling overload
      `CreateCollapsedBreadcrumbCandidate(Func<CoreWebView2> readCore)` holding the body of `:80-98` with
      `:82` replaced by `CoreWebView2 core = readCore();`, and reduce the existing private zero-argument
      member to a one-line delegation passing `() => _l0vhBreadcrumb_WebView2.CoreWebView2` (seam S3b).
      Acceptance: the zero-argument member's signature and visibility are unchanged; the default is passed
      as a method argument, **not** as a field or property initializer (a field initializer capturing
      `_l0vhBreadcrumb_WebView2` is `error CS0236`); `AttachBreadcrumbWebViewAsync()` at `:63` still
      compiles unchanged.
- [ ] [P4-T3] Add an `internal` five-argument sibling overload
      `ConfigureBreadcrumbDropDown(CoreWebView2Environment environment, IWebViewCoreInitializer initializer, Func<CoreWebView2Environment, IWebViewCoreInitializer, Action, Action, BreadcrumbPopupUiOperations, IBreadcrumbDropDownHost> hostFactory, Func<Rectangle> anchorBounds, Func<Rectangle> workingArea)`
      holding the idempotence guard `:147-153`, the `EnsureBreadcrumbLifecycle` call `:155-157`, the
      `hostFactory(...)` invocation supplying `FocusBreadcrumbCore` and
      `() => BreadcrumbCoordinator?.CancelSelector()`, and the tail call to the existing three-argument
      overload (seam S3a). Acceptance: the overload compiles and is `internal`.
- [ ] [P4-T4] Add `private IBreadcrumbDropDownHost CreateDefaultBreadcrumbDropDownHost(CoreWebView2Environment environment, IWebViewCoreInitializer initializer, Action focusAnchor, Action cancelSelector, BreadcrumbPopupUiOperations operations)`
      holding `:158-168` verbatim, including the self-referential
      `BreadcrumbDropDownHost host = null;` local and the `() => host.ControlHost?.Control.Focus()` closure
      (which requires the concrete type and cannot be hoisted). Acceptance: the closure is unchanged and
      `host` remains declared as the concrete `BreadcrumbDropDownHost`.
- [ ] [P4-T5] Rewrite the two-argument production wrapper
      `ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)` (`:142-177`) to
      delegate to `[P4-T3]`'s overload, passing `CreateDefaultBreadcrumbDropDownHost` and the two existing
      geometry lambdas from `:171-175` verbatim. Acceptance: the wrapper's signature and visibility are
      unchanged, no call site moves, and `git diff` shows no change to
      `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`.
- [ ] [P4-T6] Create `QuickFiler.Test/Viewers/ItemViewerBreadcrumbWiringTests.cs` (fixture `V` = full
      `new QuickFiler.ItemViewer()` inside a `SynchronizationContext` scope disposed in `finally`; fixture
      `U` = `CreateUninitialized<ItemViewer>()`) and add its `<Compile Include>` entry. Acceptance: file and
      entry exist, plain `[TestClass]`, CRLF preserved.
- [ ] [P4-T7] Create `QuickFiler.Test/Viewers/ItemViewerBreadcrumbWiringTestsPart2.cs` as a second, distinct
      plain `[TestClass]` sharing the same fixture shape, and add its `<Compile Include>` entry.
      Acceptance: file and entry exist; the class name is distinct from `[P4-T6]`'s.
- [ ] [P4-T8] Add case **C1** `L0vhBreadcrumb_WebView2_RoundTripsTheDesignerField` (`:21-22`, fixture U).
      Acceptance: green; uses the property setter and does not retype the property.
- [ ] [P4-T9] Add case **C2** `BreadcrumbDropDownHost_BeforeInitialize_IsNull` (`:26-27` null arm, U).
      Acceptance: green.
- [ ] [P4-T10] Add case **C3** `BreadcrumbOpenTask_BeforeInitialize_ReturnsCompletedFalse` (`:29-30` `??`
      right arm, U). Acceptance: green; the returned task completes synchronously with `false`.
- [ ] [P4-T11] Add case **C4** `BreadcrumbOpenTask_AfterInitialize_ReturnsCoordinatorTask` (`:29-30` `??`
      left arm, V, seam `:40-43`). Acceptance: green; readiness completes synchronously on the test thread.
- [ ] [P4-T12] Add case **C5**
      `InitializeBreadcrumbPipeline_WithInjectedOperations_CreatesCoordinatorAndBridge` (`:40-44`, `:50-60`,
      V, `Mock<IFolderHierarchyProvider>`). Acceptance: green; `BreadcrumbCoordinator` is non-null after the
      call.
- [ ] [P4-T13] Add case **C6** `InitializeBreadcrumbPipeline_SecondCall_IsNoOpAndKeepsFirstCoordinator`
      (`:45-48` true arm, V, two distinct provider mocks). Acceptance: green; `BreadcrumbCoordinator` is
      reference-identical before and after the second call.
- [ ] [P4-T14] Add case **C7** `InitializeBreadcrumbPipeline_SingleArgOverload_CapturesAmbientContext`
      (`:37-38`, V under an ambient `SynchronizationContext`). Acceptance: green.
- [ ] [P4-T15] Add case **C8**
      `AttachBreadcrumbWebViewAsync_BeforeInitialize_ReturnsFalseWithoutInvokingFactory` (`:69-72` true arm,
      U, seam `:65-67`). Acceptance: green; the factory delegate records zero invocations.
- [ ] [P4-T16] Add case **C9** `AttachBreadcrumbWebViewAsync_AfterInitialize_DelegatesToCollapsedAttachment`
      (`:69` false arm, `:74`, V). Acceptance: green; readiness completes synchronously.
- [ ] [P4-T17] Add case **C10**
      `CreateCollapsedBreadcrumbCandidate_WithInjectedCoreReader_BuildsMessengerAndReadiness` (`:82-97`, V,
      seam S3b, `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` stand-in). Acceptance:
      green; no real browser process is started.
- [ ] [P4-T18] Add case **C11** `AttachBreadcrumbMessengerWhenReadyAsync_NullMessenger_Throws` (`:106-108`,
      U). Acceptance: green; `ArgumentNullException` with parameter name `messenger`.
- [ ] [P4-T19] Add case **C12** `AttachBreadcrumbMessengerWhenReadyAsync_NullReadiness_Throws` (`:109-112`,
      U). Acceptance: green; `ArgumentNullException` with parameter name `readiness`.
- [ ] [P4-T20] Add case **C13**
      `AttachBreadcrumbMessengerWhenReadyAsync_BeforeInitialize_ThrowsInvalidOperation` (`:113-118` true
      arm, U). Acceptance: green.
- [ ] [P4-T21] Add case **C14**
      `AttachBreadcrumbMessengerWhenReadyAsync_AfterInitialize_DelegatesToCoordinator` (`:113` false arm,
      `:120-123`, V). Acceptance: green.
- [ ] [P4-T22] Add case **C15** `AttachBreadcrumbMessenger_NullMessenger_Throws` (`:128-131`, U).
      Acceptance: green.
- [ ] [P4-T23] Add case **C16** `AttachBreadcrumbMessenger_BeforeInitialize_ThrowsInvalidOperation`
      (`:132-137` true arm, U). Acceptance: green.
- [ ] [P4-T24] Add case **C17** `AttachBreadcrumbMessenger_AfterInitialize_AttachesToHub` (`:132` false arm,
      `:139`, V). Acceptance: green.
- [ ] [P4-T25] Add case **C18** `ConfigureBreadcrumbDropDown_SameEnvironmentTwice_ReusesHost` (`:147-153`
      true arm, V, seam S3a). Acceptance: green; the injected factory returns a real
      `new BreadcrumbDropDownHost(anchor, environment, initializer, "html", noOp, noOp, noOp)` over the same
      `FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment))` instance the test passes,
      and the factory records exactly one invocation across the two calls.
- [ ] [P4-T26] Add case **C19** `ConfigureBreadcrumbDropDown_DifferentEnvironment_ConstructsNewHost`
      (`:147-149` false arm plus the five-argument overload body, V, seam S3a, two distinct uninitialised
      environments). Acceptance: green; the factory records two invocations.
- [ ] [P4-T27] Record case **C20** (`:164`, the `() => host.ControlHost?.Control.Focus()` closure body) as a
      **named residual**. Acceptance: `evidence/qa-gates/residuals.breadcrumb.<ts>.md` records that the
      closure requires the concrete `BreadcrumbDropDownHost` (`ControlHost` is not on
      `IBreadcrumbDropDownHost`, verified at `IBreadcrumbDropDownHost.cs:19`) and therefore cannot be
      reached through the injected-factory seam; no test and no STA file is created for it.
- [ ] [P4-T28] Add case **C21** `ConfigureBreadcrumbDropDown_CancelDelegate_CancelsSelector` (`:166` closure
      body, V, seam S3a). Acceptance: green; the injected factory captures the `cancelSelector` `Action`
      and invoking it reaches `BreadcrumbCoordinator.CancelSelector()`.
- [ ] [P4-T29] Record case **C22** (`:172-175`, the two geometry closure bodies) as a **named residual**.
      Acceptance: `evidence/qa-gates/residuals.breadcrumb.<ts>.md` records that
      `Control.RectangleToScreen` and `Screen.FromControl` require a created window handle and that spec D5
      prohibits creating the first `*.StaTests.cs` file in `QuickFiler.Test`; a repository-wide search
      confirms zero `*.StaTests.cs` files and zero `[STATestClass]` attributes in `QuickFiler.Test`.
- [ ] [P4-T30] Add case **C23** `ConfigureBreadcrumbDropDown_InjectedOverload_NullHost_Throws` (`:185-188`,
      U, seam `:179-183`). Acceptance: green.
- [ ] [P4-T31] Add case **C24** `ConfigureBreadcrumbDropDown_InjectedOverload_NullAnchorBounds_Throws`
      (`:189` `??` throw arm, U). Acceptance: green.
- [ ] [P4-T32] Add case **C25** `ConfigureBreadcrumbDropDown_InjectedOverload_NullWorkingArea_Throws`
      (`:190` `??` throw arm, U). Acceptance: green.
- [ ] [P4-T33] Add case **C26** `ConfigureBreadcrumbDropDown_InjectedOverload_ConfiguresLifecycleHost`
      (`:191-194`, V, `Mock<IBreadcrumbDropDownHost>`) to
      `ItemViewerBreadcrumbWiringTestsPart2.cs`. Acceptance: green.
- [ ] [P4-T34] Add case **C27** `SetBreadcrumbTheme_BeforeInitialize_IsNoOp` (`:197-198` null arm, U).
      Acceptance: green.
- [ ] [P4-T35] Add case **C28** `SetBreadcrumbTheme_AfterInitialize_ForwardsToCoordinatorAndHost` (`:197-198`
      non-null arm, V). Acceptance: green; `Mock<IBreadcrumbDropDownHost>` verifies `SetTheme`.
- [ ] [P4-T36] Add case **C29** `FocusBreadcrumb_BeforeInitialize_CallsCoreDirectly` (`:202-205` true arm,
      V). Acceptance: green.
- [ ] [P4-T37] Add case **C30** `FocusBreadcrumb_AfterInitialize_RoutesThroughCoordinator` (`:202` false arm,
      `:208`, V). Acceptance: green; dispatch runs synchronously inline under the ambient context.
- [ ] [P4-T38] Add case **C31** `FocusBreadcrumbCore_WhenViewerDisposed_DoesNotTouchControl` (`:213-214`
      short-circuit arm, V then disposed). Acceptance: green.
- [ ] [P4-T39] Add case **C32** `FocusBreadcrumbCore_WhenWebViewNull_DoesNotThrow` (`:215` null arm, U with
      `L0vhBreadcrumb_WebView2 = null`). Acceptance: green.
- [ ] [P4-T40] Add case **C33** `FocusBreadcrumbCore_WhenWebViewDisposed_DoesNotThrow` (`:216` true arm, U
      with a disposed `WebView2`). Acceptance: green.
- [ ] [P4-T41] Add case **C34** `FocusBreadcrumbCore_WhenLive_FocusesWebView` (`:213-219` all-false path, V).
      Acceptance: green; asserts no throw, not that focus was actually taken.
- [ ] [P4-T42] Add case **C35**
      `SetBreadcrumbDropDownState_BeforeInitializeAndDroppedDown_FocusesBreadcrumb` (`:225`, `:227-229` both
      true, V). Acceptance: green; **in-code comment cites issue #438** and states the assertion pins
      current behavior (AC11).
- [ ] [P4-T43] Add case **C36** `SetBreadcrumbDropDownState_BeforeInitializeAndClosed_IsNoOp` (`:227` false
      arm, `:231`, U). Acceptance: green; **in-code comment cites issue #438** as the paired closed-state
      case (AC11).
- [ ] [P4-T44] Add case **C37** `SetBreadcrumbDropDownState_AfterInitialize_ForwardsToCoordinator` (`:225`
      false arm, `:234`, V). Acceptance: green.
- [ ] [P4-T45] Add case **C38** `ResetBreadcrumb_BeforeInitialize_IsNoOp` (`:237` null arm, U). Acceptance:
      green.
- [ ] [P4-T46] Add case **C39** `ResetBreadcrumb_AfterInitialize_ResetsCoordinator` (`:237` non-null arm, V).
      Acceptance: green.
- [ ] [P4-T47] Add case **C40** `OnBreadcrumbSelectionChanged_RaisesFolderSelectionChanged` (`:239-240` both
      arms, V). Acceptance: green; covers the no-subscriber and one-subscriber cases.
- [ ] [P4-T48] Add case **C41** `OnBreadcrumbFolderArrowKeyDown_Right_RaisesKeysRight` (`:242-248` ternary
      true, V). Acceptance: green; **in-code comment cites issue #440** and states the mapping is pinned as
      currently implemented.
- [ ] [P4-T49] Add case **C42** `OnBreadcrumbFolderArrowKeyDown_Left_RaisesKeysLeft` (`:246` ternary false,
      V). Acceptance: green; **in-code comment cites issue #440**.
- [ ] [P4-T50] Add case **C43** `OnBreadcrumbFolderArrowKeyDown_NoSubscriber_DoesNotThrow` (`:243`
      null-conditional arm, V). Acceptance: green.
- [ ] [P4-T51] Add case **C44** `OnBreadcrumbUnhandledArrow_RaisesEventToSubscriber` (`:250-251` both arms,
      V). Acceptance: green; **in-code comment cites issue #440**.
- [ ] [P4-T52] Add case **C45** `EnsureBreadcrumbLifecycle_SecondCall_ReturnsSameCoordinator` (`:257-260`
      true arm, V, reached through the two distinct entry points `:50` and `:155`). Acceptance: green.
- [ ] [P4-T53] Add case **C46** `EnsureBreadcrumbResourceOwnership_SecondCall_DoesNotAddSecondComponent`
      (`:281-284` true arm, V). Acceptance: green.
- [ ] [P4-T54] Add case **C47** `EnsureBreadcrumbResourceOwnership_WhenComponentsNull_CreatesContainer`
      (`:286` `??=` null arm, U with the `components` field set to null by reflection). Acceptance: green;
      the reflection targets the **declared** `components` field, never a compiler-generated backing-field
      name.
- [ ] [P4-T55] Add case **C48** `DisposeBreadcrumbResources_OnViewerDispose_ClearsCoordinatorAndBridge`
      (`:291-296`, V then `Dispose()`). Acceptance: green.
- [ ] [P4-T56] Add case **C49** `DisposeBreadcrumbResources_BeforeInitialize_IsNoOp` (`:293` null arm, V
      disposed without initialise). Acceptance: green.
- [ ] [P4-T57] Verify `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` line count and both test files' line
      counts. Acceptance: `evidence/qa-gates/file-size.breadcrumb.<ts>.md` records the production file
      strictly below 500 and each of the two test files strictly below 500.
- [ ] [P4-T58] Measure `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` with F1's harness. Acceptance:
      `evidence/qa-gates/coverage.itemviewer-breadcrumb.<ts>.md` carries the four schema fields plus a
      recomputed line rate `>= 80%` and branch rate `>= 75%`, and names the C20 and C22 residuals with their
      uncovered line counts.

### Phase 5 — ItemViewer.Commands.cs

109 physical lines, 32 coverable lines, **zero branch points and zero lambdas** (verified by full read).
The branch gate is vacuous for this file and must be reported `N/A`, never 0% (spec D9). No seam is
introduced and no production line changes in this phase.

- [ ] [P5-T1] Create `QuickFiler.Test/Viewers/ItemViewerCommandsForwardingTests.cs` with the `U` fixture and
      a reflection helper over the protected `Control.OnClick(EventArgs)` (pattern
      `QfcThemeHelperTests.cs:277-285`), and add its `<Compile Include>` entry. Acceptance: file and entry
      exist, CRLF preserved, plain `[TestClass]`.
- [ ] [P5-T2] Add case **C1** `DeleteItemClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick`
      (`:15-16`). Acceptance: green; one invocation after subscribe, still one after unsubscribe.
- [ ] [P5-T3] Add case **C2** `FlagTaskClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick`
      (`:21-22`). Acceptance: as C1.
- [ ] [P5-T4] Add case **C3** `PopOutClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick`
      (`:27-28`). Acceptance: as C1.
- [ ] [P5-T5] Add case **C4** `ReplyClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick` (`:33-34`).
      Acceptance: as C1.
- [ ] [P5-T6] Add case **C5** `ReplyAllClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick`
      (`:39-40`). Acceptance: as C1.
- [ ] [P5-T7] Add case **C6** `ForwardClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick`
      (`:45-46`). Acceptance: as C1.
- [ ] [P5-T8] Add case **C7** `ConversationModeChanged_AddThenRemove_TracksMenuItemCheckedChanged`
      (`:51-52`). Acceptance: green; **in-code comment names `ToolStripMenuItemCb.cs:35-49` and issue #486**
      because the case depends on the setter raising `CheckedChanged` on every assignment.
- [ ] [P5-T9] Add case **C8** `ConversationModeChecked_RoundTripsMenuItemCheckedState` (`:57-58`).
      Acceptance: green; default false, true after set, false after reset.
- [ ] [P5-T10] Add case **C9** `EmailCopyChanged_AddThenRemove_TracksMenuItemCheckedChanged` (`:63-64`).
      Acceptance: as C7, with the same #486 comment.
- [ ] [P5-T11] Add case **C10** `EmailCopyChecked_RoundTripsMenuItemCheckedState` (`:69-70`). Acceptance: as
      C8.
- [ ] [P5-T12] Add case **C11** `AttachmentsChanged_AddThenRemove_TracksMenuItemCheckedChanged` (`:75-76`).
      Acceptance: as C7, with the same #486 comment.
- [ ] [P5-T13] Add case **C12** `AttachmentsChecked_RoundTripsMenuItemCheckedState` (`:81-82`). Acceptance:
      as C8.
- [ ] [P5-T14] Add case **C13** `PicturesChanged_AddThenRemove_TracksMenuItemCheckedChanged` (`:87-88`).
      Acceptance: green; in-code comment records that this event has no production subscriber today (LD-1)
      so the test pins the viewer contract only, plus the #486 comment.
- [ ] [P5-T15] Add case **C14** `PicturesChecked_RoundTripsMenuItemCheckedState` (`:93-94`). Acceptance: as
      C8.
- [ ] [P5-T16] Add case **C15** `FlagTaskDialogResult_RoundTripsButtonDialogResult` (`:99-100`). Acceptance:
      green; asserts default `DialogResult.None`, then `OK`, then `Cancel` — the exact values production
      writes.
- [ ] [P5-T17] Add case **C16** `FlagTaskBackColor_RoundTripsButtonBackColor` (`:105-106`). Acceptance:
      green; sets and reads `Color.Red`. Does **not** assert the default getter value and does **not** use
      `Color.Transparent`.
- [ ] [P5-T18] Add case **C17** `CommandMembers_OnViewerWithUnassignedControls_ThrowNullReference` as the
      single representative negative-flow case. Acceptance: green; in-code comment states that this file has
      no null guards by design and that adding them is out of scope under the no-behavior-change NFR, and
      that no further per-member `NullReferenceException` cases are authored.
- [ ] [P5-T19] Measure `QuickFiler/Viewers/ItemViewer.Commands.cs` with F1's harness. Acceptance:
      `evidence/qa-gates/coverage.itemviewer-commands.<ts>.md` carries the four schema fields plus a
      recomputed line rate `>= 80%` and branch reported as **`N/A` (zero `<condition>` children)** — a `0%`
      branch reading for this file is a harness defect to be raised against F1/#432, not a gate failure.

### Phase 6 — ItemViewer.DisplayState.cs

81 physical lines, 23 coverable lines, **zero fields, zero branch points, zero lambdas**. Deviation D8:
this file is not a state machine; the "state transitions" obligation resolves to round-trip and
normalization coverage. No seam is introduced and no production line changes in this phase.

- [ ] [P6-T1] Create `QuickFiler.Test/Viewers/ItemViewerDisplayStateForwardingTests.cs` with the `U` fixture
      and a reflection helper over the protected `Control.OnDoubleClick(EventArgs)`, and add its
      `<Compile Include>` entry. Acceptance: file and entry exist, CRLF preserved, plain `[TestClass]`.
- [ ] [P6-T2] Add case **D1** `SenderText_RoundTripsSenderLabelText` (`:15-16`). Acceptance: green.
- [ ] [P6-T3] Add case **D2** `SubjectText_RoundTripsSubjectLabelText` (`:21-22`). Acceptance: green.
- [ ] [P6-T4] Add case **D3** `BodyText_RoundTripsBodyTextBoxText` (`:27-28`). Acceptance: green.
- [ ] [P6-T5] Add case **D4** `TriageText_RoundTripsTriageLabelText` (`:33-34`). Acceptance: green.
- [ ] [P6-T6] Add case **D5** `SentOnText_RoundTripsSentOnLabelText` (`:39-40`). Acceptance: green.
- [ ] [P6-T7] Add case **D6** `ActionableText_RoundTripsActionableLabelText` (`:45-46`). Acceptance: green.
- [ ] [P6-T8] Add case **D7** `ItemNumberText_RoundTripsItemNumberLabelText` (`:51-52`). Acceptance: green;
      uses the two formats production writes (`"7"` and `"07"`).
- [ ] [P6-T9] Add case **D8** `FolderText_RoundTripsFolderLabelText` (`:57-58`). Acceptance: green.
- [ ] [P6-T10] Add case **D9** `ConversationCountText_RoundTripsConversationCountLabelText` (`:63-64`).
      Acceptance: green.
- [ ] [P6-T11] Add case **D10** `TextProjections_AssignedNull_ReadBackAsEmptyString` on the representative
      member `SenderText`. Acceptance: green; in-code comment records that `Control.Text` performs the
      normalization and this file adds none. Exactly one such case is authored, not nine.
- [ ] [P6-T12] Add case **D11** `TextProjections_InitialState_AreEmptyStringNotNull` on `SenderText`.
      Acceptance: green; observes the framework behavior rather than assuming it.
- [ ] [P6-T13] Add case **D12** `ConversationCountBackColor_RoundTripsLabelBackColor` (`:69-70`) assigning
      `Color.Red`, the exact value production assigns. Acceptance: green.
- [ ] [P6-T14] Add case **D13** `ConversationCountBackColor_AssignedColorEmpty_ResetsToInheritedBackColor`.
      Acceptance: green; asserts the getter returns the label's effective default rather than `Color.Empty`;
      does not assert on `Color.Transparent`.
- [ ] [P6-T15] Add case **D14**
      `BodyDoubleClick_AddThenRemove_SubscribesAndUnsubscribesTextBoxDoubleClick` (`:75-76`). Acceptance:
      green; one invocation after subscribe, still one after unsubscribe.
- [ ] [P6-T16] Add case **D15** `FocusSubject_OnHeadlessViewer_DoesNotThrowAndLeavesSubjectUnfocused`
      (`:79`). Acceptance: green; asserts no throw and `LblSubject.Focused == false`. Does **not** assert
      that focus was taken.
- [ ] [P6-T17] Add case **D16** `DisplayStateMembers_OnViewerWithUnassignedControls_ThrowNullReference` as
      the single representative negative-flow case. Acceptance: green; in-code comment states that no
      further per-member throw cases are authored and why.
- [ ] [P6-T18] Measure `QuickFiler/Viewers/ItemViewer.DisplayState.cs` with F1's harness. Acceptance:
      `evidence/qa-gates/coverage.itemviewer-displaystate.<ts>.md` carries the four schema fields plus a
      recomputed line rate `>= 80%` and branch reported as **`N/A` (zero `<condition>` children)**.

### Phase 7 — ItemViewer.FolderSearch.cs

74 physical lines, ~21 coverable lines, ~10 condition points. This is the only small partial with real
branch logic. Seam S4 (`[P4-T1]`) is a precondition for every non-null-arm case. `FocusSearch()` (`:72`)
is deliberately left uncovered and recorded as the file's named residual: `Control.Invoke` requires a
created window handle, and forcing one is the only thing that would push this file toward STA.

- [ ] [P7-T1] Create `QuickFiler.Test/Viewers/ItemViewerFolderSearchForwardingTests.cs` with fixtures **B**
      (bare `CreateUninitialized<ItemViewer>()`), **C** (B plus
      `viewer.BreadcrumbCoordinator = new BreadcrumbBridgeCoordinator(mockMessenger.Object, mockProvider.Object, BreadcrumbUiDispatcher.CreateForCurrentThreadTests())`
      through seam S4), and **S** (B plus `viewer.TxtboxSearch = new TextBox()`), and add its
      `<Compile Include>` entry. Acceptance: file and entry exist; both mocks are **Loose**, not Strict,
      because the coordinator's constructor subscribes to `IWebViewMessenger.MessageReceived` and builds a
      router from the provider.
- [ ] [P7-T2] Create `QuickFiler.Test/Viewers/ItemViewerFolderSearchForwardingTestsPart2.cs` as a second,
      distinct plain `[TestClass]` sharing the same fixture shape, and add its `<Compile Include>` entry.
      Acceptance: file and entry exist; the class name is distinct from `[P7-T1]`'s.
- [ ] [P7-T3] Add case **F1** `SetFolderItems_BeforePipeline_IsNoOp` (`:20` `?.` null arm, B). Acceptance:
      green.
- [ ] [P7-T4] Add case **F2** `SetFolderItems_WithCoordinator_AppendsItemsToPage` (`:20` non-null arm, C).
      Acceptance: green; asserts via `viewer.GetFolderItems()`; does not assert coordinator-internal
      ordering.
- [ ] [P7-T5] Add case **F3** `SetFolderSuggestions_BeforePipeline_IsNoOp` (`:22` null arm, B). Acceptance:
      green.
- [ ] [P7-T6] Add case **F4** `SetFolderSuggestions_WithCoordinator_PublishesRows` (`:22` non-null arm, C,
      `List<FolderRow>`). Acceptance: green.
- [ ] [P7-T7] Add case **F5** `GetSelectedFolder_BeforePipeline_ReturnsNull` (`:25` null arm, B).
      Acceptance: green; pins the documented "legacy empty-combo value" contract at `:15-16`.
- [ ] [P7-T8] Add case **F6** `GetSelectedFolder_WithCoordinator_ReturnsRouterSelection` (`:25` non-null
      arm, C). Acceptance: green.
- [ ] [P7-T9] Add case **F7** `SetFolderSelectedIndex_BeforePipeline_IsNoOp` (`:27` null arm, B).
      Acceptance: green.
- [ ] [P7-T10] Add case **F8** `SetFolderSelectedIndex_WithCoordinator_SelectsRow` (`:27` non-null arm, C).
      Acceptance: green.
- [ ] [P7-T11] Add case **F9** `SetFolderSelectedItem_BeforePipeline_IsNoOp` (`:29` null arm, B).
      Acceptance: green.
- [ ] [P7-T12] Add case **F10** `SetFolderSelectedItem_WithCoordinator_SelectsMatchingItem` (`:29` non-null
      arm, C). Acceptance: green.
- [ ] [P7-T13] Add case **F11**
      `SetFolderDroppedDown_True_BeforePipeline_RoutesToBreadcrumbFocusFallback` (`:31`, B). Acceptance:
      green; **in-code comment cites issue #438** and states explicitly that the assertion pins *current*
      behavior so a future fix produces a legible red test (AC11).
- [ ] [P7-T14] Add case **F12** `SetFolderDroppedDown_False_BeforePipeline_IsNoOp` (`:31` re-hit, B).
      Acceptance: green; **in-code comment cites issue #438** as the paired closed-state case (AC11).
- [ ] [P7-T15] Add case **F13** `ClearFolderItems_BeforePipeline_IsNoOp` (`:34` null arm, B). Acceptance:
      green.
- [ ] [P7-T16] Add case **F14** `ClearFolderItems_WithCoordinator_EmptiesPage` (`:34` non-null arm, C).
      Acceptance: green; seeds via `SetFolderItems` then asserts `GetFolderItems()` is empty.
- [ ] [P7-T17] Add case **F15** `FocusFolderDropDown_BeforePipeline_DoesNotThrow` (`:36`, B) to
      `ItemViewerFolderSearchForwardingTestsPart2.cs`. Acceptance: green.
- [ ] [P7-T18] Add case **F16** `FolderContains_BeforePipeline_ReturnsFalse` (`:38` `&&` left arm false,
      short-circuit, B). Acceptance: green.
- [ ] [P7-T19] Add case **F17** `FolderContains_WithCoordinatorAndKnownItem_ReturnsTrue` (`:38` both arms
      true, C). Acceptance: green.
- [ ] [P7-T20] Add case **F18** `FolderContains_WithCoordinatorAndUnknownItem_ReturnsFalse` (`:38` left
      true, right false, C). Acceptance: green.
- [ ] [P7-T21] Add case **F19** `GetFolderItems_BeforePipeline_ReturnsEmptyArray` (`:41` `?.` null arm plus
      `??` right arm, B). Acceptance: green; asserts `Array.Empty<string>()` semantics.
- [ ] [P7-T22] Add case **F20** `GetFolderItems_WithCoordinator_ReturnsRouterItems` (`:41` non-null arm plus
      `??` left arm, C). Acceptance: green.
- [ ] [P7-T23] Add case **F21** `FolderSelectionChanged_AddThenRemove_TracksHandlerField` (`:46-47`, B,
      reflection on the private raiser `OnBreadcrumbSelectionChanged`). Acceptance: green; the reflection
      targets the declared method name, not a compiler-generated name.
- [ ] [P7-T24] Add case **F22** `FolderKeyDown_AddThenRemove_TracksHandlerField` (`:54-55`, B, reflection on
      `OnBreadcrumbFolderArrowKeyDown`). Acceptance: green; asserts **only that the handler field is
      invoked**, never the `Keys.Left`/`Keys.Right` mapping, and carries an in-code comment citing issue
      #440 recording why the mapping is not asserted here.
- [ ] [P7-T25] Add case **F23** `SearchText_ReturnsSearchTextBoxText` (`:58`, S). Acceptance: green.
- [ ] [P7-T26] Add case **F24**
      `SearchTextChanged_AddThenRemove_SubscribesAndUnsubscribesTextBoxTextChanged` (`:62-63`, S,
      reflected `OnTextChanged`). Acceptance: green.
- [ ] [P7-T27] Add case **F25** `SearchKeyDown_AddThenRemove_SubscribesAndUnsubscribesTextBoxKeyDown`
      (`:68-69`, S, reflected `OnKeyDown`). Acceptance: green.
- [ ] [P7-T28] Verify no fix for issue #438 was introduced and that the AC11 annotations are present.
      Acceptance: `evidence/qa-gates/issue-438-not-fixed.<ts>.md` records that
      `ItemViewer.FolderSearch.cs:31-32` and `ItemViewer.Breadcrumb.cs:223-235` are behaviorally unchanged
      versus `[P0-T12]`'s recorded HEAD, and lists the four cases (F11, F12, C35, C36) carrying an in-code
      `#438` citation with their file and line.
- [ ] [P7-T29] Measure `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` with F1's harness. Acceptance:
      `evidence/qa-gates/coverage.itemviewer-foldersearch.<ts>.md` carries the four schema fields plus a
      recomputed line rate `>= 80%` and branch rate `>= 75%`, and names `FocusSearch` (`:72` plus its
      lambda) as the file's residual with the reason recorded.

### Phase 8 — ItemViewer.WebViewThread.cs

37 physical lines, 9 coverable lines, **zero branch points**. Two injectable-delegate seams are added
**inside this file** so the added field-initializer lines stay in this file's own denominator. `:35`
(`MoveOptionsMenu.ShowDropDown()`) shows a real popup and cannot be executed in a unit test at all, so
its seam is mandatory; `:15`'s seam is adopted so the 9-line denominator retains margin.

- [ ] [P8-T1] Add seam **W-1** to `QuickFiler/Viewers/ItemViewer.WebViewThread.cs`: a
      `private Action<ToolStripMenuItem> _showMoveOptionsMenu = ShowMoveOptionsMenuCore;` field, a
      `private static void ShowMoveOptionsMenuCore(ToolStripMenuItem menu) => menu.ShowDropDown();`
      default, an `internal void SetMoveOptionsMenuPresenter(Action<ToolStripMenuItem> presenter) => _showMoveOptionsMenu = presenter;`
      setter, and `:35` rewritten to `public void ShowMoveOptionsMenu() => _showMoveOptionsMenu(MoveOptionsMenu);`.
      Acceptance: the default is a **static method group**, never a property initializer referencing an
      instance member (`error CS0236`); the public `ShowMoveOptionsMenu()` signature is unchanged; the
      default path is behaviorally identical to the previous direct call.
- [ ] [P8-T2] Add seam **W-2** to the same file: a
      `private Action<Microsoft.Web.WebView2.WinForms.WebView2, string> _navigateToString = NavigateToStringCore;`
      field, a
      `private static void NavigateToStringCore(Microsoft.Web.WebView2.WinForms.WebView2 webView, string html) => webView.NavigateToString(html);`
      default, an `internal void SetNavigateToStringHandler(Action<Microsoft.Web.WebView2.WinForms.WebView2, string> handler) => _navigateToString = handler;`
      setter, and `:15` rewritten to
      `public void NavigateToString(string html) => _navigateToString(L0v2h2_WebView2, html);`.
      Acceptance: as `[P8-T1]`; the `L0v2h2_WebView2` property is **not** retyped.
- [ ] [P8-T3] Verify the two seams introduced no branch point and no behavior change. Acceptance:
      `evidence/qa-gates/webviewthread-seam-review.<ts>.md` records a search of the file showing no `if`,
      `?:`, `?.`, `??`, `&&`, `||`, loop, or `try`, and confirms the seam fields are declared in this file
      rather than in `ItemViewer.cs`.
- [ ] [P8-T4] Create `QuickFiler.Test/Viewers/ItemViewerWebViewThreadTests.cs` with the `U` fixture, every
      created `Control`/`Component` disposed in `finally`, and add its `<Compile Include>` entry.
      Acceptance: file and entry exist, plain `[TestClass]`, CRLF preserved.
- [ ] [P8-T5] Run spike **S-A**: determine whether `BrightIdeasSoftware.FastObjectListView.SetObjects(IList)`
      and `.Sort(OLVColumn, SortOrder)` execute on a handle-less instance in a plain `[TestMethod]`.
      Acceptance: `evidence/other/spike-fastobjectlistview-handleless.<ts>.md` records `PASS` or `FAIL` with
      the observed exception type if any; the spike code is deleted after the result is recorded.
- [ ] [P8-T6] Add case **C1**
      `WebViewInitializationCompleted_Subscribe_AddsHandlerToDesignerWebView` (`:19`). Acceptance: green;
      asserts the handler is present on the `WebView2` event backing field via reflection on the declared
      field name.
- [ ] [P8-T7] Add case **C2**
      `WebViewInitializationCompleted_Unsubscribe_RemovesHandlerFromDesignerWebView` (`:20`). Acceptance:
      green.
- [ ] [P8-T8] Add case **C3** `ConversationItemSelectionChanged_SubscribeAndUnsubscribe_RoundTripOnTopicThread`
      (`:31-32`). Acceptance: green; raises through the protected `ListView.OnItemSelectionChanged` by
      reflection and asserts one invocation then zero.
- [ ] [P8-T9] Add case **C4** `SetConversationItems_ForwardsTheListToTopicThread` (`:23`). Acceptance:
      green. If `[P8-T5]` recorded `FAIL`, author the case as a negative-path assertion that the forwarding
      line executes and surfaces the control's own exception, with an in-code comment citing the spike
      artifact; do **not** create an STA file and do **not** force a window handle.
- [ ] [P8-T10] Add case **C5** `SortConversationByDate_SortsTopicThreadOnTheSentDateColumn` (`:25`).
      Acceptance: green; asserts `PrimarySortColumn` is the injected `SentDate` instance and
      `PrimarySortOrder` matches the supplied order. Same `[P8-T5]`-`FAIL` fallback as `[P8-T9]`.
- [ ] [P8-T11] Add case **C6** `GetSelectedConversationItems_WithNoSelection_ReturnsEmptyList` (`:27`).
      Acceptance: green; asserts non-null with `Count == 0`.
- [ ] [P8-T12] Add case **C7** `NavigateToString_ForwardsHtmlToTheDesignerWebView` (`:15`, seam W-2).
      Acceptance: green; injects a recording handler through `SetNavigateToStringHandler` and asserts the
      exact html string and the exact `L0v2h2_WebView2` instance are forwarded once.
- [ ] [P8-T13] Add case **C8** `ShowMoveOptionsMenu_ForwardsToTheMoveOptionsMenuPresenter` (`:35`, seam
      W-1). Acceptance: green; injects a recording presenter through `SetMoveOptionsMenuPresenter` and
      asserts it received the exact `MoveOptionsMenu` instance. **No popup is shown.**
- [ ] [P8-T14] Record the two static default bodies (`ShowMoveOptionsMenuCore`, `NavigateToStringCore`) as
      this file's named residuals. Acceptance: `evidence/qa-gates/residuals.webviewthread.<ts>.md` states
      that executing `ShowDropDown()` would show a real popup (prohibited outright) and that
      `NavigateToString` on a live core requires a browser process (an external dependency), and records the
      resulting line arithmetic.
- [ ] [P8-T15] Measure `QuickFiler/Viewers/ItemViewer.WebViewThread.cs` with F1's harness and verify the
      file size. Acceptance: `evidence/qa-gates/coverage.itemviewer-webviewthread.<ts>.md` carries the four
      schema fields plus a recomputed line rate `>= 80%`, branch reported as **`N/A` (zero `<condition>`
      children)**, and a physical line count strictly below 500.

### Phase 9 — ItemViewerExpanded.cs

Post-S1 shape: ~125 lines, ~57 coverable lines, one branch point at `:171`. Seam S2 is **mandatory for
the branch gate**: the `true` arm of `:171` is unreachable through every public path because
`ToolStripMenuItemCb`'s shadowing `Checked` setter never assigns `base.Checked`. Research proposed
`*.StaTests.cs` homes for these cases; spec D5 overrides that — plain `[TestClass]` files only.

- [ ] [P9-T1] Widen `QuickFiler/Viewers/ItemViewerExpanded.cs`'s `MenuItem_CheckedChanged(ToolStripMenuItem)`
      (`:169`) from `private` to `internal static`. Acceptance: the constructor calls at `:24-27` and the
      call at `:166` still compile unchanged.
- [ ] [P9-T2] Widen `QuickFiler/Viewers/ItemViewerExpanded.cs`'s `MenuItem_CheckedChanged(object, EventArgs)`
      (`:163`) from `private` to `internal`, keeping it an instance method because the designer wires it as
      `new System.EventHandler(this.MenuItem_CheckedChanged)`. Acceptance: no edit is made to
      `ItemViewerExpanded.Designer.cs`.
- [ ] [P9-T3] Create `QuickFiler.Test/Viewers/ItemViewerExpandedTests.cs` with a `SynchronizationContext`
      fixture (required: `ItemViewerExpanded.cs:22` calls `TaskScheduler.FromCurrentSynchronizationContext()`)
      and add its `<Compile Include>` entry. Acceptance: file and entry exist, plain `[TestClass]`, no STA
      attribute, viewer disposed and context restored in `[TestCleanup]`.
- [ ] [P9-T4] Create `QuickFiler.Test/Viewers/ItemViewerExpandedMenuTests.cs` as a plain `[TestClass]`
      needing no viewer construction, and add its `<Compile Include>` entry. Acceptance: file and entry
      exist.
- [ ] [P9-T5] Add case **T1** `Constructor_PopulatesTipsLabelCollections` (`:35`, `:41`, `:47`) to
      `ItemViewerExpandedTests.cs`. Acceptance: green.
- [ ] [P9-T6] Add case **T2** `Constructor_CapturesUiSyncContextAndScheduler` (`:60`, `:66`). Acceptance:
      green.
- [ ] [P9-T7] Add case **T3** `ControllerProperty_RoundTripsAssignedValue` (`:53`) with a
      `Mock<IItemControler>`. Acceptance: green.
- [ ] [P9-T8] Add case **T4** `MenuItemCheckedChangedHandler_WhenMenuItemUnchecked_ClearsImage` (`:170`,
      `:171`-false, `:176-179`) to `ItemViewerExpandedMenuTests.cs` via the S2 `internal static` overload.
      Acceptance: green; uses a plain `ToolStripMenuItem`.
- [ ] [P9-T9] Add case **T5** `MenuItemCheckedChangedHandler_WhenMenuItemChecked_AppliesCheckedImage`
      (`:172-174`, the `:171`-true outcome). Acceptance: green; invokes the S2 overload with
      `new ToolStripMenuItem { Checked = true }`. **This case is load-bearing for the branch gate**; an
      in-code comment cites `ToolStripMenuItemCb.cs:32-51` and issue #486 recording why the outcome is
      unreachable through every public path.
- [ ] [P9-T10] Add case **T6**
      `MenuItemCheckedChangedEvent_WhenMenuItemCheckStateChanges_InvokesTypedOverload` (`:164-167`) to
      `ItemViewerExpandedTests.cs` by setting `viewer.ConversationMenuItem.Checked = true`. Acceptance:
      green; in-code comment cites issue #486 and records that the resulting image state reflects the
      current shadowing defect.
- [ ] [P9-T11] Add case **T7** `MenuItemCheckedChangedEvent_WhenSenderIsNotMenuItem_Throws` (`:164-165`) to
      `ItemViewerExpandedMenuTests.cs`. Acceptance: green; asserts `InvalidCastException`.
- [ ] [P9-T12] Add case **T8** `RemoveControlsColsRightOf_DelegatesToTrimmerWithWebViewSpanTarget` (the S1
      delegation line) to `ItemViewerExpandedTests.cs`. Acceptance: green; asserts the observable
      `TableLayoutPanel` mutation.
- [ ] [P9-T13] Add case **T9** `WebViewParentChanged_WhenReparented_RunsHandler` (`:159-161`) by re-parenting
      `L0v2h2_WebView2` into a local `Panel`. Acceptance: green; no designer wiring is added or removed.
- [ ] [P9-T14] Verify `QuickFiler/Viewers/ItemViewerExpanded.cs` line count and that no public member was
      added, removed, retyped, or renamed. Acceptance:
      `evidence/qa-gates/file-size.itemviewerexpanded.<ts>.md` records a count strictly below 500 and
      strictly below the 181 recorded in `[P0-T13]`, plus a public-surface diff showing only the two
      visibility widenings from `[P9-T1]`/`[P9-T2]` and the S1 body change.
- [ ] [P9-T15] Measure `QuickFiler/Viewers/ItemViewerExpanded.cs` with F1's harness. Acceptance:
      `evidence/qa-gates/coverage.itemviewerexpanded.<ts>.md` carries the four schema fields plus a
      recomputed line rate `>= 80%` and branch rate `>= 75%`, with the `[P0-T11]` baseline (37.74% line /
      8.33% branch recomputed) quoted alongside for the delta, and any `line-rate` attribute annotated
      "#441 — unreliable".

### Phase 10 — IItemViewer.cs

Deviation D4: this file has **zero coverable lines**. No tests are written for it, no attribute is added,
and no edit of any kind is made. Shape-assertion or reflection-contract tests written to manufacture
coverage are prohibited.

- [ ] [P10-T1] Verify `QuickFiler/Viewers/IItemViewer.cs` emits no `<class>` element in
      `evidence/baseline/coverage-after-t0.cobertura.xml`, using
      `QuickFiler\Viewers\ItemViewerExpanded.cs` as a same-folder positive control proving the folder was
      instrumented. Acceptance: `evidence/qa-gates/coverage.iitemviewer.<ts>.md` records the negative
      result, the positive control's XML location, and the file's compile entry at
      `QuickFiler/QuickFiler.csproj:392` proving it is not a build exclusion.
- [ ] [P10-T2] Record the `interface-only / not-measured` classification for
      `QuickFiler/Viewers/IItemViewer.cs` and append the corresponding row to
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Acceptance: the ledger row
      exists, its bucket is `interface-only / not-measured` and **not** `ratified-exempt`, and its reported
      figure is **N/A** for both line and branch, never 0%.
- [ ] [P10-T3] Verify no `[ExcludeFromCodeCoverage]` attribute was added to
      `QuickFiler/Viewers/IItemViewer.cs` and that the file received no edit at all. Acceptance:
      `git diff --name-only` does not contain the path, and the file contains zero occurrences of
      `ExcludeFromCodeCoverage`.
- [ ] [P10-T4] Verify zero tests were authored whose subject is `IItemViewer.cs`. Acceptance:
      `evidence/qa-gates/coverage.iitemviewer.<ts>.md` records a search of all fourteen test files created
      by this plan showing no shape-assertion, reflection-contract, or member-declaration test targeting the
      interface, and states that existing `Mock<IItemViewer>` usages elsewhere are consumers, not subjects.

### Phase 11 — ItemViewer.Designer.cs

Deviation D3: `testable`, not exempt-candidate. Exactly one branch point (`:18`,
`if (disposing && (components != null))`, four outcomes). `components` is declared `= null` at `:10` and
never assigned by `InitializeComponent()`, so one arm is dead and branch is capped at 3/4 = exactly 75%
unless the breadcrumb container path runs first. The file receives **no edit**.

- [ ] [P11-T1] Verify designer cases **D1** and **D2** authored in `[P1-T8]`/`[P1-T9]` are present in
      `QuickFiler.Test/Viewers/ItemViewerDisposalTests.cs` and green. Acceptance: both test names appear in
      the run output recorded in `evidence/qa-gates/phase11-tests.<ts>.md` with `Passed`.
- [ ] [P11-T2] Add designer case **D3**
      `Dispose_AfterBreadcrumbResourceOwnershipEstablished_DisposesComponentContainer` to
      `ItemViewerDisposalTests.cs`, driving `ItemViewer.Breadcrumb.cs:279-289` so `components` becomes
      non-null and then disposing the viewer, covering the `:18` jump-1-true outcome and lines `:19-21`.
      Acceptance: green; asserts the `BreadcrumbResourceOwner`'s dispose action ran. D1 and D2 are retained
      regardless so the file passes at 75% even if this path is later restructured.
- [ ] [P11-T3] Verify `QuickFiler/Viewers/ItemViewer.Designer.cs` received no edit. Acceptance:
      `git diff --name-only` does not contain the path, and the file contains zero occurrences of
      `ExcludeFromCodeCoverage`.
- [ ] [P11-T4] Verify no member-level re-exemption was introduced anywhere in the `ItemViewer` family after
      `[P1-T1]`. Acceptance: `evidence/qa-gates/no-reexemption.<ts>.md` records a search over
      `QuickFiler/Viewers/ItemViewer*.cs` and `QuickFiler/Viewers/IItemViewer.cs` returning zero
      `[ExcludeFromCodeCoverage]` attribute applications, and records that issue #457 is the reason
      member-level exclusion is not an option.
- [ ] [P11-T5] Append the ledger row for `QuickFiler/Viewers/ItemViewer.Designer.cs` to
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Acceptance: bucket `testable`,
      500-line rule recorded as **exempt (generated code)**, and the structural caps recorded (line ~99.9%
      after any single construction; branch capped at 3/4 = 75% while `components` is never assigned, with
      the `ItemViewer.Breadcrumb.cs:286` exception noted).
- [ ] [P11-T6] Measure `QuickFiler/Viewers/ItemViewer.Designer.cs` with F1's harness. Acceptance:
      `evidence/qa-gates/coverage.itemviewer-designer.<ts>.md` carries the four schema fields plus a
      recomputed line rate `>= 80%` and branch rate `>= 75%`, with any `line-rate` attribute annotated
      "#441 — unreliable".

### Phase 12 — ItemViewerExpanded.Designer.cs

Same structure: one branch point at `:16`, `components` declared `= null` at `:8` and never assigned,
branch capped at 3/4 = 75%, line already ~98.5-99.5%. The file receives **no edit**. Its current 50%
branch figure depends on an unpinned cross-test disposal cascade; both `Dispose` paths are pinned here.

- [ ] [P12-T1] Create `QuickFiler.Test/Viewers/ItemViewerExpandedDisposalTests.cs` with a
      `SynchronizationContext` fixture and a test-local
      `private sealed class DisposeProbe : QuickFiler.ItemViewerExpanded { internal void DisposeUnmanagedOnly() => base.Dispose(false); }`,
      and add its `<Compile Include>` entry. Acceptance: file and entry exist, plain `[TestClass]`, CRLF
      preserved.
- [ ] [P12-T2] Add case **ED1** `Dispose_WhenDisposingIsFalse_SkipsComponentDisposalAndCallsBase`, covering
      the `:16` jump-0-false outcome and lines `:15`, `:20`, `:21`. Acceptance: green; reaches
      `Dispose(bool)` through the derived probe with no reflection.
- [ ] [P12-T3] Add case **ED2** `Dispose_WhenDisposingIsTrue_EvaluatesComponentGuardAndCallsBase`, calling
      public `Dispose()` to pin the jump-0-true and jump-1-false outcomes. Acceptance: green; in-code
      comment records that this pins an outcome previously supplied only by an unidentified disposal
      cascade in another test.
- [ ] [P12-T4] Verify `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` received no edit. Acceptance:
      `git diff --name-only` does not contain the path and the file contains zero occurrences of
      `ExcludeFromCodeCoverage`.
- [ ] [P12-T5] Append the ledger row for `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs`. Acceptance:
      bucket `testable`, 500-line rule recorded as **exempt (generated code)**, structural caps recorded
      (line ~98.5-99.5%; branch capped at 3/4 = 75%).
- [ ] [P12-T6] Measure `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` with F1's harness. Acceptance:
      `evidence/qa-gates/coverage.itemviewerexpanded-designer.<ts>.md` carries the four schema fields plus a
      recomputed line rate `>= 80%` and a branch rate of exactly `75%` or better, with the 50% baseline from
      `[P0-T11]` quoted for the delta.

### Phase 13 — Cleanup, Ledger, and Cross-Child Closure

- [ ] [P13-T1] Remove the unused `using System.Linq;` at `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:3`.
      Acceptance: the directive is gone; a search of the file confirms no LINQ operator is used
      (`Array.Empty<string>()` at `:42` is `System.Array`); the other four directives are untouched.
- [ ] [P13-T2] Verify the four stale exemption comments corrected in `[P1-T3]`..`[P1-T6]` are still correct
      and that no file in the `ItemViewer` family asserts a coverage exemption. Acceptance:
      `evidence/qa-gates/stale-comments-corrected.<ts>.md` quotes the corrected text at
      `ItemViewer.Commands.cs`, `ItemViewer.DisplayState.cs` (including the retained CS0579 note),
      `ItemViewer.FolderSearch.cs`, and `ItemViewer.WebViewThread.cs`.
- [ ] [P13-T3] Verify `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` carries a row
      for all eleven files: the ten in the spec scope table plus `ControlColumnTrimmer.cs`. Acceptance:
      `evidence/qa-gates/ledger-completeness.<ts>.md` lists each file with its bucket, target, measured
      figure, and any recorded structural cap or residual.
- [ ] [P13-T4] Record the four cross-child freeze requests (to F13, F12, F15, and the F10/F7 advisories) and
      the blocking requirement on F1 (#432) covering the zero-`<condition>` branch N/A rule and the
      recomputed-per-file-rate rule. Acceptance: the notes are appended to the epic's cross-child notes and
      mirrored at `evidence/issue-updates/cross-child-notes.<ts>.md` with `PostedAs:` recorded.
- [ ] [P13-T5] Sweep every latent defect surfaced during execution against the already-promoted set (#486,
      #487, #488, #489, #490, #491, #438, #440, #441, #457, #230) and promote any genuinely new one through
      the MCP promotion lifecycle. Acceptance:
      `evidence/issue-updates/latent-defect-sweep.<ts>.md` lists each observed defect with either its
      existing issue number (referenced, not re-promoted) or the new issue URL created for it.
- [ ] [P13-T6] Verify the change's file scope. Acceptance: `evidence/qa-gates/change-scope.<ts>.md` records
      `git diff --name-only` against the `[P0-T12]` baseline SHA and confirms it contains **no** F13, F12,
      F10, F15, or F7 production file, neither `*.Designer.cs`, not `IItemViewer.cs`, not `coverage.config`,
      not `TaskMaster.runsettings`, not `scripts/vscode/Invoke-MSTestWithCoverage*.ps1`, not
      `UtilitiesCS/Properties/AssemblyInfo.cs`, and no `*.StaTests.cs` file anywhere.

### Phase 14 — Final QC Toolchain Loop and Acceptance-Criteria Closure

Run steps `[P14-T1]` through `[P14-T5]` in this exact order. **If any step fails or changes any file,
restart from `[P14-T1]`.** These tasks are unconditional; `EXIT_CODE: SKIPPED` is not a passing outcome.

- [ ] [P14-T1] Run `dotnet tool run csharpier format .`. Acceptance:
      `evidence/qa-gates/final-csharpier-format.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`,
      `Output Summary:` naming every file the formatter rewrote; if any file was rewritten, restart the loop
      at `[P14-T1]` after this artifact is written.
- [ ] [P14-T2] Run `dotnet tool run csharpier check .`. Acceptance:
      `evidence/qa-gates/final-csharpier-check.<ts>.md` with the four schema fields and `EXIT_CODE: 0`.
- [ ] [P14-T3] Run
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
      Acceptance: `evidence/qa-gates/final-msbuild-analyzers.<ts>.md` with the four schema fields,
      `EXIT_CODE: 0`, and zero errors.
- [ ] [P14-T4] Run
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
      Acceptance: `evidence/qa-gates/final-msbuild-nullable.<ts>.md` with the four schema fields,
      `EXIT_CODE: 0`, and zero errors.
- [ ] [P14-T5] Run the coverage-mode test step with
      `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-itemviewer-coverage-456\evidence\qa-gates\coverage-final.cobertura.xml`,
      which satisfies the `CLAUDE.md` § CUT3 `vstest.console.exe ... /EnableCodeCoverage` requirement by
      wrapping `vstest.console.exe` with `/InIsolation` under `dotnet-coverage collect`. Acceptance:
      `evidence/qa-gates/final-test-coverage.<ts>.md` with the four schema fields, `EXIT_CODE: 0`, zero
      failed tests, and **numeric** post-change repository-wide line and branch values.
- [ ] [P14-T6] Record the clean-pass statement for the loop. Acceptance:
      `evidence/qa-gates/final-toolchain-clean-pass.<ts>.md` names the five commands in order, states that
      all five completed without errors in the **same** pass, and records the number of loop restarts that
      occurred.
- [ ] [P14-T7] Produce the AC8 repository-wide before/after comparison. Acceptance:
      `evidence/qa-gates/repo-wide-comparison.<ts>.md` records the `[P0-T12]` before figures, the
      `[P14-T5]` after figures, and the delta, and states explicitly whether repository-wide line coverage
      was **retained or improved**; a reduction is a gate failure requiring remediation, not a pass.
- [ ] [P14-T8] Produce the consolidated per-file coverage summary for all eleven files. Acceptance:
      `evidence/qa-gates/per-file-final-summary.<ts>.md` lists each file with its bucket, recomputed line
      rate, branch rate or `N/A`, gate verdict against `>= 80%` / `>= 75%` (`>= 90%` line for
      `ControlColumnTrimmer.cs`), and its named residuals; every figure is derived from deduplicated
      `<line>`/`<condition>` data and no `<class>` `line-rate` attribute is quoted un-annotated.
- [ ] [P14-T9] Check off AC1-AC12 in `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/spec.md`
      per `.claude/skills/acceptance-criteria-tracking/SKILL.md`. Acceptance: each of the twelve checkboxes
      is `[x]` with the evidence artifact path recorded for it, or is left `[ ]` with a stated blocker; no
      criterion is checked without an evidence citation.
- [ ] [P14-T10] Check off the mirrored AC1-AC12 in
      `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/user-story.md`. Acceptance: the
      twelve checkbox states match `[P14-T9]` exactly and cite the same evidence paths.
- [ ] [P14-T11] Verify evidence completeness against the Definition of Done in `spec.md`. Acceptance:
      `evidence/qa-gates/definition-of-done.<ts>.md` confirms items 1-7: AC check-off with evidence,
      per-file coverage evidence for all eleven files under `evidence/qa-gates/`, baseline evidence under
      `evidence/baseline/`, the recorded final toolchain pass, ledger rows for all eleven files, the
      cross-child freeze requests and the F1 blocking requirement communicated, and the latent-defect
      promotion sweep completed.
- [ ] [P14-T12] Verify post-format file sizes. After `[P14-T1]`'s formatter pass, record the physical
      line count of all fourteen test files created by this plan under `QuickFiler.Test/Viewers/` and of
      `QuickFiler/Viewers/ControlColumnTrimmer.cs`, `ItemViewer.cs`, `ItemViewer.Breadcrumb.cs`,
      `ItemViewer.Commands.cs`, `ItemViewer.DisplayState.cs`, `ItemViewer.FolderSearch.cs`,
      `ItemViewer.WebViewThread.cs`, and `ItemViewerExpanded.cs`. Acceptance:
      `evidence/qa-gates/final-file-sizes.<ts>.md` lists every file with its post-format count, each
      strictly below 500, with both `*.Designer.cs` files recorded as exempt as generated code. If any
      file is at or above 500, split it into an additional `...Part2.cs`/`...Part3.cs` sibling class,
      add the `<Compile Include>` entry with CRLF preserved, and restart the loop at `[P14-T1]`.

## Coverage Evidence Contract

| Stage | Artifact | Required values |
| --- | --- | --- |
| Baseline, repository-wide | `evidence/baseline/coverage-run.<ts>.md` | numeric line and branch rate |
| Baseline, per file | `evidence/baseline/per-file-baseline.<ts>.md` | per-file recomputed line, branch or `N/A` |
| Post-T0 measurement | `evidence/baseline/measured-per-file-after-t0.<ts>.md` | per-file recomputed line, branch or `N/A`, uncovered line lists |
| Per-file gate | `evidence/qa-gates/coverage.<file>.<ts>.md` | recomputed line `>= 80%`, branch `>= 75%` or `N/A` |
| New module gate | `evidence/qa-gates/coverage.controlcolumntrimmer.<ts>.md` | recomputed line `>= 90%` |
| Final, repository-wide | `evidence/qa-gates/final-test-coverage.<ts>.md` | numeric line and branch rate |
| Delta / no-regression | `evidence/qa-gates/repo-wide-comparison.<ts>.md` | before, after, delta, retained-or-improved verdict |

If any required coverage value is unavailable, the plan outcome is **remediation-required** and must not
be reported as PASS.

## Open Questions / Notes

- `[P0-T6]`'s halt gate is the only place F1's absence can stop this plan. It is evaluated at execution
  time only.
- Deviation D11 is resolved in favour of Option B (keep the three unreferenced private members of
  `ItemViewer.cs` and cover them through seam S2b) because AC9's enumeration of permitted production edits
  does not include member deletion. F14 must not add the missing designer wiring for those members: the
  wired path in `ItemViewerExpanded` is the defective one (issue #486).
- Research proposed `*.StaTests.cs` homes for the `ItemViewerExpanded`, `ControlColumnTrimmer`, and
  breadcrumb-geometry cases. Spec D5 overrides all three. Case C22 and the residuals in `[P4-T27]`,
  `[P4-T29]`, and `[P8-T14]` are the recorded cost of that decision.
- The four stale exemption comments are corrected in Phase 1 rather than Phase 13 because spec AC10 and
  the T0 sequencing rule both require them to land in the same change that removes the attribute. Phase 13
  verifies them.
