# 2026-08-08-quickfiler-form-viewers-bayesian-coverage — Spec

- **Issue:** #496
- **Parent (optional):** Epic #136 (`quickfiler-per-file-coverage`), child F15
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08
- **Status:** Draft
- **Version:** 0.2

## Overview

F15 owns the smallest, most designer-heavy slice of epic #136: five hand-written production files
in `QuickFiler/Controllers/` and `QuickFiler/Viewers/`, plus seven generated `*.Designer.cs` /
`Properties/` files that round out `QuickFiler.csproj`'s compiled surface with no owner otherwise
assigned. The feature raises every `testable` file in this set to the epic's per-file floor
(>= 80% line, >= 75% branch), resolves the disposition of the two existing
`[ExcludeFromCodeCoverage]` attributes in this set against F1's ledger rules, and classifies the
seven generated files per epic Ruling DEC-5 — all without changing any observable QuickFiler
behavior and without editing sibling F6's frozen `IQfcFormViewer` contract.

- Target users/personas and primary use cases: the epic/maintainer persona (see
  `user-story.md`) who needs QuickFiler's viewer and Bayesian-performance surface safe for
  autonomous agentic maintenance; no end-user-facing behavior changes.
- Success metrics or expected impact: every `testable` F15 file clears 80% line / 75% branch (or
  reports branch as N/A when `branches-valid = 0`); both `[ExcludeFromCodeCoverage]` attributes in
  this set are removed with the underlying code covered; the seven generated files are correctly
  classified and contribute to, without individually gating, repository-wide totals.

## Behavior

This is an internal testability change, not a user-facing feature; "behavior" below means what
"done" looks like per file rather than an end-to-end user flow.

- `Controllers/BayesianPerformanceController.cs` (156 lines, baseline 66.0% line / 57.1% branch,
  no attribute today): add two `internal` seam members following the existing in-repo precedent at
  `EfcHomeController.cs:294` (`internal Action<EfcViewer> ViewerShowAction`) rather than inventing a
  new seam shape —
  - `internal Action<BayesianPerformanceViewer> ViewerShowAction { get; set; } = viewer => viewer.Show();`,
    invoked in place of the current `Viewer.Show()` call inside `InvestigatePerformance()`.
  - `internal Func<BayesianPerformanceController, BayesianPerformanceViewer> ViewerFactory { get; set; } = c => new BayesianPerformanceViewer(c).Init();`,
    invoked in place of the current `Viewer = new BayesianPerformanceViewer(this).Init();` assignment.
  These two seams let tests drive `InvestigatePerformance()` to completion without a live/visible
  Form. Add coverage for: both `??=` branches (`Serialization`, `Errors`), the false branch of
  `ClassSelector_SelectedIndexChanged`'s `ActiveError is not null` guard, the false branch of
  `OlvVerboseDetails_SelectionChanged`'s outer selection guard, and `ReSortItem()`'s
  `item is not null` branches (the `item is null` false-branch is achievable without a new interop
  mock; the `item is not null` true-branch's `EfcHomeController` construction is F8-owned and out of
  F15's file set — if it proves unmockable from F15's side, document the residual gap against F1's
  ledger rather than editing an F8 file). Do not await or otherwise change the discarded
  `ProgressPackage.InitializeAsync(...)` call at line 58 (see Constraints & Risks).
- `Viewers/BayesianPerformanceViewer.cs` (67 lines, a `Form`, baseline 54.3% line / 12.5% branch, no
  attribute today): no production seam needed — the existing `virtual`/`internal set` `Controller`
  property and the existing `internal` visibility of `GroupKeyGetter` already suffice. Add tests for
  `GroupKeyGetter`'s `catch` path (call with a non-`KeyValuePair` argument), each of the four private
  `Controller?.Xxx()` event-forwarding handlers (both the `Controller != null` side, reached by
  raising the underlying WinForms control event, and the `Controller == null` side, reached via the
  parameterless constructor), all inside the existing `RunWithViewer` STA harness (or a sibling
  parameterless-constructor overload in the same `TestSupport.cs` file, same STA/dispose shape).
- `Viewers/ToolStripMenuItemCb.cs` (87 lines, a `ToolStripMenuItem`-derived `Component`, not a
  `Form`; baseline 61.5% line / 50.0% branch, no attribute today, no existing test file): no seam
  needed and no STA construction needed (it is not `Form`-derived and does not require a message
  loop or window handle — confirmed against this repo's precedent of constructing bare WinForms
  controls in the default apartment). Add a new test file covering all four branch points: the
  constructor's `if (Checked)` branch, the `Checked` setter's `if (value)` branch and its
  `CheckedChanged?.Invoke` subscribed/unsubscribed sides, `ToolStripMenuItemCb_Click`'s toggle
  behavior (via `PerformClick()` once `CheckOnClick = true`), and the `CheckOnClick` setter's branch
  plus its subscribe/unsubscribe/re-subscribe state transition. Tests must read/write `Checked`
  through a `ToolStripMenuItemCb`-typed reference, never a `ToolStripMenuItem`-typed one, because
  `Checked` uses `new` (hiding, not overriding) with an independent backing field.
- `Viewers/QfcFormViewer.cs` (262 lines, a `Form` implementing `IQfcFormViewer`, currently carries a
  type-level `[ExcludeFromCodeCoverage]` at line 17 suppressing both itself and
  `QfcFormViewer.Designer.cs`): remove the attribute and cover the file. No new interface seam (the
  frozen `IQfcFormViewer` already exposes everything needed), no injectable delegate, no adapter —
  every branch (`ProcessCmdKey`'s keyboard-handler guard, `ItemViewerTemplateMargin`'s
  null-coalescing operator, `CaptureTlpCellStates`'s null guard) is pure logic over already-
  constructed control references. Construct one unshown `QfcFormViewer` on a dedicated STA thread in
  a new `QuickFiler.Test/Viewers/QfcFormViewer.StaTests.cs`, reusing the `RunWithViewer` harness
  shape from `BayesianPerformanceController.TestSupport.cs` verbatim (STA thread, `finally` dispose,
  `SynchronizationContext` save/restore, `ExceptionDispatchInfo` marshalling), and exercise
  `ProcessCmdKey` via a minimal test-only subclass (`protected override` members are not reachable
  through `InternalsVisibleTo` alone). Never call `.Show()`/`.ShowDialog()`.
- `Viewers/QfcItemViewerExpanded.cs` (63 lines, a `UserControl`, currently carries a type-level
  `[ExcludeFromCodeCoverage]` at line 18 suppressing both itself and its 942-line
  `QfcItemViewerExpanded.Designer.cs`; zero production consumers — confirmed dead but compiled
  code): remove the attribute and cover the file via construct-and-inspect tests in a new
  `QuickFiler.Test/Viewers/QfcItemViewerExpanded.StaTests.cs`, reusing the same STA/dispose harness
  shape (its Designer partial constructs a `WebView2` WinForms control and a
  `CoreWebView2CreationProperties` value, but performs no `EnsureCoreWebView2Async`/
  `CoreWebView2Environment` call, so construction alone does not touch the Evergreen runtime and the
  fourth epic exemption ground — prohibited-to-execute adapter — does not apply). Assert
  `TipsLabels` contains the nine expected Designer-declared label references and that `Controller`'s
  get/set round-trips. This file has zero branch points; report branch coverage as N/A.
- Seven generated files classified per epic Ruling DEC-5 (see Constraints & Risks for the
  per-file table): `QfcFormViewer.Designer.cs`, `QfcItemViewerExpanded.Designer.cs`,
  `BayesianPerformanceViewer.Designer.cs`, `ToolStripMenuItemCb.Designer.cs`,
  `Properties/Resources.Designer.cs`, `Properties/Settings.Designer.cs` are `measured-not-gated`;
  `Properties/AssemblyInfo.cs` is `interface-only / not-measured` (zero coverable lines — it
  declares no class, no method, only assembly-level attributes) rather than `measured-not-gated`.
  No new tests are written to manufacture coverage for any of the seven; their coverage gains are a
  byproduct of covering the five hand-written files above (partial-type propagation once the two
  `[ExcludeFromCodeCoverage]` attributes are removed, and incidental resource access elsewhere in
  the assembly).
- Main user flow (happy path): not applicable — this feature changes internal testability only.
- Alternate/edge flows: not applicable.
- Error handling and recovery behavior: unchanged; no production error-handling logic is modified,
  only exercised by new tests (e.g. `GroupKeyGetter`'s `catch` path, `ReSortItem`'s null guard).

## Inputs / Outputs

- Inputs: none (no CLI flags, files, or env vars are introduced).
- Outputs: coverage evidence artifacts under `<FEATURE>/evidence/qa-gates/`, measured with F1's
  per-file harness once it exists in this checkout (F1 has not executed yet as of this branch — this
  feature treats the epic manifest's classification RULES, not an as-yet-nonexistent ledger file, as
  authoritative for classifying its own files, per the epic's "Directives for F1's Ledger and
  Harness" § dynamic denominator).
- Config keys and defaults: none.
- Versioning or backward-compatibility constraints: `IQfcFormViewer` is frozen — no member may be
  added, removed, or renamed (see Constraints & Risks).

## API / CLI Surface

None. No public API, CLI command, or frozen interface member changes. The two new `internal` seam
members added to `BayesianPerformanceController` (`ViewerShowAction`, `ViewerFactory`) are test-only
seams reachable via `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, already in force; they are
not part of any public or interface-declared surface.

- Example invocations with expected outputs (concise): not applicable.
- Contracts and validation rules: `IQfcFormViewer` (`QuickFiler/Interfaces/IQfcFormViewer.cs`, 51
  lines) is frozen per sibling F6's ratified spec — no member addition, removal, or rename. Every
  member `QfcFormViewer.cs` needs to test is already implemented and already public (or `protected`,
  reachable via a test-only subclass), so no interface edit is required to satisfy this feature.

## Data & State

None. No data transformation, persistence, caching, or migration is introduced. The only state
touched is in-memory WinForms control state constructed and disposed within a single test method.

- Data transformations and invariants: none new.
- Caching or persistence details: none.
- Migration or backfill requirements (if any): none.

## Constraints & Risks

- **`IQfcFormViewer` is frozen.** Sibling F6's ratified spec declares no member may be added to,
  removed from, or renamed on `IQfcFormViewer`. F6 needs no edit to `QfcFormViewer.cs` and states
  explicitly that removing `QfcFormViewer.cs`'s `[ExcludeFromCodeCoverage]` attribute and covering
  the file is F15's obligation, not F6's. Research confirms this is achievable with zero interface
  changes — every member under test is already implemented and already reachable.
- **`BayesianPerformanceController.cs:58` unobserved-task latent defect.**
  `(new ProgressPackage()).InitializeAsync(...)` returns a `Task` that is never awaited and never
  read; any exception during `ProgressPackage` initialization is silently swallowed. Fixing this is
  out of scope under the epic's no-behavior-change NFR (see Out of Scope). A test that covers
  `InvestigatePerformance()` must assert the current (silently-discarded) behavior, not "fix" it by
  awaiting the task. This defect must be promoted via the MCP promotion lifecycle at execution time
  — recorded here as an explicit note, not silently dropped.
- **`QfcItemViewerExpanded` is dead but compiled code.** Zero production consumers exist anywhere in
  the repository (confirmed by repo-wide search for construction, subclassing, and type-name
  reference). Deletion is out of scope for this child (see Out of Scope) because it would shrink the
  epic's dynamic `<Compile Include>` denominator mid-wave — a decision the epic routes to the
  capstone (F16) for the structurally analogous dead-interface case F6 found, not to the owning
  child. Cover it via construct-and-inspect tests instead.
- **Two Forms require DEC-1 STA treatment.** `QfcFormViewer.cs` and `BayesianPerformanceViewer.cs`
  (plus the newly-exposed `QfcItemViewerExpanded.cs`, a `UserControl` covered under the epic's
  Shared Design §3 STA clause for in-memory never-shown controls) can only have their
  `InitializeComponent()` exercised via epic Ruling DEC-1's ratified unshown-construction pattern:
  construct on a dedicated STA thread, never call `.Show()`/`.ShowDialog()` or pump a message loop,
  dispose in `finally`, save/restore `SynchronizationContext`, marshal exceptions with
  `ExceptionDispatchInfo`. Reuse `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs`'s
  `RunWithViewer` harness verbatim rather than hand-rolling a new STA harness. New STA-bound test
  files: `QfcFormViewer.StaTests.cs`, `QfcItemViewerExpanded.StaTests.cs` (both new); no new
  dedicated STA file is required for `BayesianPerformanceViewer.cs`'s additional tests since they
  reuse the existing merged harness.
- **Removing the two `[ExcludeFromCodeCoverage]` attributes is an intended, not incidental,
  side effect on the paired `.Designer.cs` partials.** A type-level attribute on a partial type
  propagates to every partial of that type. Removing the attributes from `QfcFormViewer.cs:17` and
  `QfcItemViewerExpanded.cs:18` newly exposes `QfcFormViewer.Designer.cs` (257 lines) and
  `QfcItemViewerExpanded.Designer.cs` (942 lines) to instrumentation. This is the wanted DEC-1/DEC-5
  interaction the epic anticipated — do not add a narrower member-level exemption to either `.cs`
  file merely to keep the Designer partials looking untouched; doing so would forfeit real,
  freely-obtained coverage and would risk the epic's `#457` lambda-suppression trap if a member-level
  attribute were placed carelessly.
- **The seven generated/`Properties/` files carry a per-file disposition, not a single blanket
  rule:**

  | File | Disposition | Branch surface |
  | --- | --- | --- |
  | `Viewers/QfcFormViewer.Designer.cs` | `measured-not-gated` | 1 branch (`Dispose(bool)` guard) |
  | `Viewers/QfcItemViewerExpanded.Designer.cs` | `measured-not-gated` | 1 branch (`Dispose(bool)` guard) |
  | `Viewers/BayesianPerformanceViewer.Designer.cs` | `measured-not-gated` | 1 branch (`Dispose(bool)` guard) |
  | `Viewers/ToolStripMenuItemCb.Designer.cs` | `measured-not-gated` | 1 branch (`Dispose(bool)` guard) |
  | `Properties/Resources.Designer.cs` | `measured-not-gated` | >= 1 branch (`ResourceManager` lazy-init) |
  | `Properties/Settings.Designer.cs` | `measured-not-gated` | 0 branches — report branch as N/A; line coverage remains a real, un-gated percentage |
  | `Properties/AssemblyInfo.cs` | `interface-only / not-measured` | 0 coverable lines and 0 branches — report N/A, no Cobertura `<class>` element expected |

  No shape-assertion test is written to manufacture coverage for any of these seven files.
- **No thin-forwarder adapter is currently identified as needed for F15.** Research found the
  correct seam level for `BayesianPerformanceController.cs` is a plain injectable delegate
  (`Action`/`Func` property), not an adapter type, so the epic's `#457` lambda-suppression trap
  (adapter types must be `sealed`, non-`partial`, type-level `[ExcludeFromCodeCoverage]`) does not
  presently apply. If execution discovers an adapter is needed after all, it must follow that shape.
- **Shared, non-globbing `.csproj` files.** `QuickFiler/QuickFiler.csproj` and
  `QuickFiler.Test/QuickFiler.Test.csproj` are both non-SDK projects with explicit
  `<Compile Include>` entries and no globbing. Any new test file (the two new `*.StaTests.cs` files)
  needs an explicit entry in `QuickFiler.Test.csproj`; `[assembly: InternalsVisibleTo("QuickFiler.Test")]`
  is already in force, so `internal` seam members are directly reachable. Edit only entries this
  feature owns, keep hunks minimal and adjacent, and preserve CRLF line endings.
- **`csharpier` requires the `format` subcommand.** The pinned 1.2.6 CLI requires
  `dotnet tool run csharpier format .`; the bare `csharpier .` form in `CLAUDE.md` §C#1/§CUT3 is
  stale for this pinned version. `CLAUDE.md` is not amended; this feature records the working
  command in its own evidence.
- **Repository-wide coverage comparison must be a self-consistent before/after pair** measured on
  this branch with the same command and post-processing, never against an imported figure from
  another branch or tool.
- Limits (latency/throughput/memory) and acceptable trade-offs: not applicable — no runtime
  behavior changes.
- Security/privacy considerations: none.
- Operational/rollout risks and mitigations: low risk — internal test-only changes plus two
  attribute removals with full coverage backing; mitigated by the full C# toolchain gate and by
  measuring repository-wide coverage before and after on this branch.

## Out of Scope

- Deleting `QfcItemViewerExpanded.cs`/`.Designer.cs` (dead code). Cover it instead; record a
  recommendation that its removal be promoted as a separate follow-up GitHub issue, analogous to how
  F6 routed its own dead-interface finding (`IQfcFormController.cs`) to the capstone rather than
  fixing it inline.
- Fixing the unobserved/unawaited `ProgressPackage.InitializeAsync(...)` call at
  `BayesianPerformanceController.cs:58`. Promote via the MCP promotion lifecycle at execution time
  instead of fixing it, per the epic's no-behavior-change NFR.
- Any growth of `IQfcFormViewer` or any other interface's member set.
- Building the `#230` WinForms message-pump test seam (`Application.Run()` background thread) — that
  work is tracked and deferred epic-wide, not part of this child.
- Editing `EfcHomeController.cs` (F8-owned) even though its `ViewerShowAction` pattern is the
  precedent this feature follows; if `ReSortItem()`'s `item is not null` branch proves unreachable
  without an F8 edit, document the residual gap against F1's ledger instead.
- Editing `UtilitiesCS/Properties/AssemblyInfo.cs` to widen `InternalsVisibleTo`.
- Any change to repository-wide coverage thresholds themselves.

## Acceptance Criteria

- [ ] `Controllers/BayesianPerformanceController.cs` reaches >= 80% line and >= 75% branch coverage,
      using the `ViewerShowAction`/`ViewerFactory` injectable-delegate seams (following the
      `EfcHomeController.cs:294` precedent), with the unobserved-task defect at line 58 left
      unmodified and promoted via the MCP lifecycle rather than fixed.
- [ ] `Viewers/BayesianPerformanceViewer.cs` reaches >= 80% line and >= 75% branch coverage,
      including `GroupKeyGetter`'s `catch` path and both the `Controller != null` and
      `Controller == null` sides of all four private event-forwarding handlers.
- [ ] `Viewers/ToolStripMenuItemCb.cs` reaches >= 80% line and >= 75% branch coverage across all
      four identified branch points (constructor, `Checked` setter, `CheckOnClick` setter,
      click-toggle forwarding), with a new test file since none exists today.
- [ ] The `[ExcludeFromCodeCoverage]` attribute on `Viewers/QfcFormViewer.cs:17` is removed and the
      file reaches >= 80% line and >= 75% branch coverage via the DEC-1 unshown-STA-construction
      pattern, without adding, removing, or renaming any member of `IQfcFormViewer`.
- [ ] The `[ExcludeFromCodeCoverage]` attribute on `Viewers/QfcItemViewerExpanded.cs:18` is removed
      and the file reaches >= 80% line coverage via construct-and-inspect STA tests; its branch
      coverage is reported N/A (zero branch points), never 0%, and never fails the gate.
- [ ] The two `[ExcludeFromCodeCoverage]` attributes in F15's set are both removed with the
      underlying code covered — neither is retained as a ledger-ratified exemption, per research
      confirming none of F1's three exemption grounds nor the epic's fourth ground applies to either
      file.
- [ ] All six branch-bearing generated files (`QfcFormViewer.Designer.cs`,
      `QfcItemViewerExpanded.Designer.cs`, `BayesianPerformanceViewer.Designer.cs`,
      `ToolStripMenuItemCb.Designer.cs`, `Resources.Designer.cs`, `Settings.Designer.cs`) are
      classified `measured-not-gated` per epic Ruling DEC-5: measured, counted toward totals, not
      individually gated, carrying no `[ExcludeFromCodeCoverage]` attribute; no shape-assertion test
      is written to manufacture coverage for any of them.
- [ ] `Properties/AssemblyInfo.cs` is classified `interface-only / not-measured` (zero coverable
      lines, zero branches) rather than `measured-not-gated`, and is reported N/A rather than as a
      percentage.
- [ ] Any file with `branches-valid = 0` (`QfcItemViewerExpanded.cs`, `Settings.Designer.cs`,
      `AssemblyInfo.cs`) reports branch coverage as N/A, never 0%, and never fails the gate.
- [ ] Repository-wide coverage is retained or improved, measured as a self-consistent before/after
      pair on this branch using the identical command and post-processing, cited with both figures
      in the evidence artifact.
- [ ] No production file in F15's set exceeds 500 lines (the two `.Designer.cs` files exempt as
      generated code); any wholly new file created by this feature reaches >= 90% line coverage.
- [ ] Full C# toolchain (csharpier via `dotnet tool run csharpier format .`, .NET analyzers,
      nullable/`TreatWarningsAsErrors`, MSTest with coverage) passes in a single clean pass.
- [ ] No observable QuickFiler behavior changes — no Form is shown, no message loop is pumped, and
      the latent unawaited-task defect is left byte-for-byte unmodified.
- [ ] The `BayesianPerformanceController.cs:58` unobserved-task defect and the
      `QfcItemViewerExpanded` dead-code finding are each promoted via the MCP promotion lifecycle as
      GitHub issues before execution completes, not left as prose in this feature folder.

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add two `internal` delegate properties to
  `BayesianPerformanceController`; add four new/expanded test files
  (`BayesianPerformanceControllerTests.cs` additions, a new `ToolStripMenuItemCb` test file, a new
  `QfcFormViewer.StaTests.cs`, a new `QfcItemViewerExpanded.StaTests.cs`, plus additions to whatever
  file already covers `BayesianPerformanceViewer.cs`); remove two `[ExcludeFromCodeCoverage]`
  attributes; add a minimal test-only subclass for `QfcFormViewer.ProcessCmdKey`; append
  `<Compile Include>` entries to `QuickFiler.Test.csproj` for new test files only.
- New classes/functions/commands to add or update: `BayesianPerformanceController.ViewerShowAction`,
  `BayesianPerformanceController.ViewerFactory` (both `internal`); a private test-only
  `TestableQfcFormViewer` subclass inside the new STA test file.
- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable — internal test
  coverage change merged via the epic's standard integration-branch fan-in.

## Definition of Done

- [ ] Acceptance criteria documented above and mapped to tests
- [ ] Behavior matches acceptance criteria; no observable QuickFiler flow changed
- [ ] Tests added for all five hand-written files' identified branch/scenario gaps
- [ ] Edge cases and error-handling paths covered (`GroupKeyGetter` catch path, null-guard branches,
      `ReSortItem` null item)
- [ ] Docs updated (this spec, `user-story.md`, and any promoted-issue links)
- [ ] Both latent findings promoted via the MCP promotion lifecycle
- [ ] Toolchain pass completed (csharpier format → analyzers → nullable → MSTest with coverage)
