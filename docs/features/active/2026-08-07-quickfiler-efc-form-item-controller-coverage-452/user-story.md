# `quickfiler-efc-form-item-controller-coverage` — User Story

- **Issue:** #452
- **Parent epic:** #136 `quickfiler-per-file-coverage` (child F9, wave 1, band C3)
- **Owner:** drmoisan
- **Status:** Draft
- **Last Updated:** 2026-08-07
- **Work Mode:** `full-feature` — this file is a co-authoritative acceptance-criteria source
  alongside `spec.md`

## Story Statement

- **As a QuickFiler maintainer**, I want the two EFC controllers that today carry
  `[ExcludeFromCodeCoverage]` to be split into cohesive partials, given injectable seams, and brought
  above the per-file coverage floors, **so that** the 2,256 lines of logic that run every time a user
  files an email are protected by tests instead of hidden from measurement.
- **As a QuickFiler maintainer**, I want the exemption attributes removed rather than re-justified,
  **so that** the coverage report stops reporting silence as success — an absent file is not a
  covered file.
- **As a maintainer who delegates work to coding agents**, I want the coverage numbers F9 reports to
  be derived from a rule that is provably immune to open issue #441, **so that** the evidence an
  agent commits means what it says rather than what a corrupted attribute claims.
- **As a maintainer**, I want the decision about constructing an `EfcViewer` in a test made
  explicitly by me and recorded, **so that** a ~2,000-line swing in the repository coverage
  denominator is a ratified choice rather than an incidental consequence of deleting one attribute.
- **As the person who will eventually fix the breadcrumb lineage bug**, I want F9's tests to pin
  today's behavior and F9's PR to say where the fix point moved, **so that** the refactor does not
  silently invalidate issue #439's reproduction steps.

## Problem / Why

Epic #136 requires every testable production file compiled by `QuickFiler/QuickFiler.csproj` to
reach at least 80% line coverage, measured per file rather than per assembly. Child F9 owns the EFC
form/item controller cluster and the EFC viewer — four files:

| File | Lines (verified) | `[ExcludeFromCodeCoverage]` |
| --- | --- | --- |
| `QuickFiler/Controllers/EfcItemController.cs` | 1,170 | Yes, `EfcItemController.cs:25` |
| `QuickFiler/Controllers/EfcFormController.cs` | 1,086 | Yes, `EfcFormController.cs:27` |
| `QuickFiler/Viewers/EfcViewer.cs` | 162 | Yes, `EfcViewer.cs:20` |
| `QuickFiler/Viewers/EfcViewer.Designer.cs` | 4,277 | No attribute of its own |

Three of the four are removed from instrumentation entirely, so they do not appear in the committed
Cobertura report at all. `EfcHomeController.cs` does appear in the same report
(`coverage-final.cobertura.xml:9`), which proves the assembly and the folder were instrumented. The
absence is caused by the attribute, not by a tooling gap: **these files are unmeasured, not
covered.**

For `EfcItemController.cs` the situation is unambiguous. A repository-wide grep for the identifier
returns only `QuickFiler.csproj:301`, the type's own declarations, the two `new EfcItemController(...)`
sites in `EfcFormController.cs:69` and `:87`, and documentation. There is no test anywhere that
references it. Coverage is genuinely zero.

This makes F9 the heaviest child in the epic (`epic.md:386-391`). It is the only one that must
simultaneously remove three exemption attributes, split two files that each breach the 500-line
limit, and take a position on a 4,277-line generated file that enters the coverage denominator as a
side effect of one of those removals.

## Value Framing — why this is not a percentage exercise

**Removing an attribute changes what the number means.** Today the QuickFiler coverage figure is
computed over a denominator that silently omits 2,418 lines of controller logic. Raising that figure
by covering already-visible files would be easier and less valuable than what F9 does: putting the
hidden lines back into the denominator and then covering them. The epic states the principle at
`epic.md:220-225` — the `CLAUDE.md` §UT2 qualifier "without an injectable seam" is a live obligation,
not a standing permission, and `[ExcludeFromCodeCoverage]` on a testable seam is a Blocking finding.
Per-member research found that **zero methods on either controller are irreducible-remainder
candidates**. Every one is reachable after seam extraction. The attributes are therefore
unjustifiable, not merely inconvenient.

**Behavior that no test protects is behavior that regresses silently.** Research surfaced eight
promoted defects in these files, several of which are the kind that only a test finds: a keyboard
registration path that silently drops every action because the `KbdActions<>` indexer setter only
assigns when the element already exists (#459); a `Cleanup()` that nulls an armed
`System.Threading.Timer` without disposing it (#460); an event handler whose `nameof` guard resolves
to a property name the publisher never raises, making it dead in production (#461); an incognito
browser argument written with a U+2013 EN DASH instead of two hyphens (#463). None of these is fixed
by F9 — the epic NFR forbids behavior change — but each is pinned by a characterization test so the
eventual fix has a test to invert.

**Evidence that survives scrutiny.** Open issue #441 is worse than its title. The per-file
`@line-rate` and `@branch-rate` attributes in this repository's Cobertura output are corrupted, not
just the repository total, because `Merge-CoberturaClassesByFilename`
(`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:167-292`) recomputes them from a doubled line
count. This is provable to the digit: `FilerQueue.cs` records `line-rate="0.405797"`, which is
exactly 28/69, while its true class-level rate is 18/49 = 0.367347. The epic's own baseline table at
`epic.md:161` is therefore wrong for that file. F9 derives every number it cites from the direct-child
`class/lines/line` axis instead, and discloses the derivation in the committed artifact. That is what
makes F9's evidence usable by the capstone rather than another inconsistent number.

**Confidence for autonomous maintenance.** Two 1,000-line files with no seams and no tests are files
an agent cannot safely edit. After F9 they are sixteen focused partials, each under 275 lines, each
independently measured, behind interfaces and injectable delegates that a test can substitute. That
is the concrete deliverable behind the epic's business-outcome hypothesis.

## Personas & Scenarios

### Persona — QuickFiler maintainer

- **Who:** the engineer, or a delegated coding agent, responsible for QuickFiler's Outlook add-in
  behavior in a VSTO/WinForms codebase with an active long-term goal of migrating away from VSTO.
- **What they care about:** that a change to the EFC controller family is safe to merge without a
  manual Outlook smoke test, and that the test suite says so quickly and without flakiness.
- **Constraints:** unit tests must never construct a shown form, open a popup, touch a live Outlook
  store, or write to disk; test classes run in parallel
  (`scripts/vscode/TaskMaster.cli.runsettings:4-7`), so process-global statics are a live flakiness
  risk; no production or test file may exceed 500 lines; `UtilitiesCS` grants no `InternalsVisibleTo`
  to `QuickFiler.Test`, so `UtilitiesCS` internals are unreachable and local seams are required.
- **Frustrations:** coverage numbers that do not mean what they say; a CI job that hangs on a modal
  message box; a 1,170-line file with no seam anywhere; research that plans against a stale figure in
  a manifest.
- **Goals:** merge with confidence, and hand this cluster to an agent without hand-holding.

### Scenario — the maintainer decides whether a Form may be constructed

A maintainer is asked to ratify DEC-1 before Phase 1 begins.

1. **Trigger:** F9 must remove `[ExcludeFromCodeCoverage]` from `EfcViewer.cs:20`. That attribute
   sits on the partial **type**, and C# merges attributes across partials onto the single emitted
   type, so removing it also un-suppresses `EfcViewer.Designer.cs` — 4,277 lines. There is no
   type-level way to separate the two partials.
2. **Obstacle:** the inherited precedent
   `docs/features/epics/winforms-testability-refactor/epic.md:74` condition (d) states that
   "`Form`-derived types remain prohibited in tests even when unshown", and `EfcViewer` is a `Form`
   (`EfcViewer.cs:21`). Under a literal reading, no test may construct one, the Designer lands at 0%,
   and repository-wide coverage falls — a direct AC9 failure.
3. **Contrary evidence:** `QuickFiler.Test` already constructs an unshown `Form` on a dedicated STA
   thread and disposes it in a `finally`
   (`QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:16-53`), which is the
   sole reason `BayesianPerformanceViewer.Designer.cs` reports 99.14% line coverage.
   `QuickFiler.Test/SetupAssemblyInitializer.cs:14-20` calls `Application.EnableVisualStyles()` at
   `[AssemblyInitialize]` precisely so real controls can be constructed. `UtilitiesCS.Test` does the
   same for four more `Form`s. The rule the repository actually enforces is shown versus unshown.
4. **Decision:** the maintainer ratifies Approach A (one STA-constructed, never-shown, disposed
   viewer; ~100% on `EfcViewer.cs`, ~99% on the Designer, roughly 2,000 lines added to the covered
   total) or Approach B (no Form construction; ~82% line on `EfcViewer.cs`; method-level attributes
   on generated code that Visual Studio will silently drop on regeneration; ~2,000 lines forfeited).
5. **Expected outcome:** because both branches share the same S1 seam and the same normal-test list,
   a reversal costs one plan phase rather than a re-plan. The choice is recorded under
   `<FEATURE>/evidence/other/` and F9 proceeds.

### Scenario — an agent reports a coverage number that is wrong

An agent finishes the split and reports per-file coverage read from the Cobertura `@line-rate`
attributes.

1. **Trigger:** the obvious way to read per-file coverage out of a Cobertura file is the rate
   attribute on each `<class>`.
2. **Today:** `Merge-CoberturaClassesByFilename` rewrites those attributes for every merged class
   from a doubled denominator. Both controllers will produce many merged classes, because C# emits a
   `<>c` closure class per lambda-bearing type and a `<M>d__N` state machine per `async` method, and
   `EfcFormController.cs` alone has five `async void` button handlers (`:415`, `:431`, `:447`,
   `:463`, `:523`) whose entire bodies live in state-machine classes. The reported numbers are wrong
   in an unpredictable direction — FilerQueue's line rate is overstated while its branch rate is
   understated.
3. **After this child:** the derivation rule is binding and stated in the acceptance criteria. Rates
   come from the direct-child `class/lines/line` axis grouped by `@filename`, deduplicated by
   `@number` taking `max(@hits)`. The evidence artifact carries an explicit `DERIVATION:` line and an
   `ISSUE_441_DISCLOSURE:` line. A reviewer can tell a contaminated value by inspection: a 16-digit
   rate was never merged; a rate with six or fewer decimals went through the defective path.
4. **Expected outcome:** the capstone can reconcile F9's numbers against every sibling's, and the
   correction to `epic.md:161` is reported rather than propagated.

### Scenario — someone later fixes the breadcrumb lineage bug

An engineer picks up issue #439 after F9 has merged.

1. **Trigger:** suggestion rows render a single leaf segment instead of a root-to-leaf lineage.
2. **Obstacle:** the mechanism is a path-namespace mismatch, not a rendering defect.
   `FolderPredictor.AddSuggestions` (`FolderPredictor.cs:804-808`) emits relative folder stems, while
   `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (`OutlookFolderHierarchyProvider.cs:64-68`)
   matches rooted `node.FolderPath`, so the lookup returns `null` and
   `BreadcrumbRowBuilder.BuildRows` falls back to single-segment rendering
   (`BreadcrumbRowBuilder.cs:28-31`). The fix point is the provider construction at
   `EfcFormController.cs:840-842` — which F9's refactor relocates into the default body of the
   `BreadcrumbRouterFactory` seam.
3. **Risk if F9 says nothing:** the engineer follows the issue's reproduction path, finds the
   construction site gone, and either rediscovers the mechanism from scratch or concludes the bug was
   already fixed.
4. **After this child:** F9's characterization tests pin **current** behavior — relative-stem rows
   pass through verbatim, the router receives them unchanged, a row whose chain lookup yields `null`
   still binds — and no test asserts that a multi-segment lineage appears. The PR body names the new
   fix point.
5. **Expected outcome:** the #439 fix is a small, well-located change against a suite that fails
   loudly and correctly when the behavior intentionally changes.

## Acceptance Criteria

These criteria are identical in text to the `## Acceptance Criteria` section of `spec.md`; both files
are authoritative for `full-feature` work mode and must be checked off in step. Numbering matches
`issue.md` AC1-AC11. No criterion has been dropped or renumbered; wording has been refined to be
individually measurable and to name its evidence artifact.

- [ ] **AC1 — Per-file line coverage floor.** Every file classified `testable` in F1's ledger within
      F9's scope — the eight `EfcItemController.*.cs` partials, the eight `EfcFormController.*.cs`
      partials, `EfcViewer.cs`, and the F9-created seam files — measures **>= 80% line coverage**
      (>= 90% for F9-created files per AC5), verified with F1's per-file harness on F9's branch, with
      every rate derived by the DEC-2 rule from the direct-child `class/lines/line` axis grouped by
      `@filename`. Evidence: the per-file coverage table under `<FEATURE>/evidence/qa-gates/`,
      carrying `LINE_COVERED / LINE_VALID` per file, the `DERIVATION:` statement, and the
      `ISSUE_441_DISCLOSURE:` statement.
- [ ] **AC2 — Per-file branch coverage floor.** Every such file also measures **>= 75% branch
      coverage**, reported as an independent gate alongside line coverage in the same artifact, with
      `BRANCH_COVERED / BRANCH_VALID` per file. `EfcViewer.Designer.cs` is excluded from this gate
      per DEC-5 (its ~0.50 branch rate is a construction artifact of `Dispose(bool)`, not a test
      gap), subject to F1's ledger clarification.
- [ ] **AC3 — Exemption attributes removed.** `[ExcludeFromCodeCoverage]` is absent from
      `EfcItemController.cs`, `EfcFormController.cs`, and `EfcViewer.cs` in the final diff, and each
      reaches the AC1/AC2 floors via seam extraction. No `EfcItemController.*.cs` or
      `EfcFormController.*.cs` partial carries the attribute. The single new production file
      proposed for `ratified-exempt` — `Viewers/EfcItemControlSurface.cs` — carries an F1-ratified
      rationale meeting the irreducible-remainder standard. DEC-1 is ratified by the maintainer and
      the ratified branch (A or B) is recorded in `<FEATURE>/evidence/other/` before Phase 1 begins.
      Evidence: the final diff plus the Phase 0 ratification record.
- [ ] **AC4 — File-size compliance.** No production file in F9's scope exceeds **500 lines** after
      refactor, and no F9-authored test file exceeds 500 lines. `EfcViewer.Designer.cs` is exempt as
      generated code (`epic.md:254-255`). Evidence: a line-count table for every F9-touched and
      F9-created file under `<FEATURE>/evidence/qa-gates/`.
- [ ] **AC5 — New files registered, covered, and ledgered.** Every production file newly created by
      F9 (partial splits, seam types, interfaces) reaches **>= 90% line coverage** per the
      `CLAUDE.md` §UT2 new-module rule, has a `<Compile Include>` entry in
      `QuickFiler/QuickFiler.csproj`, and has an appended row in
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` **added in the same
      change** — except files in the `interface-only / not-measured` bucket, which are reported
      `N/A`, never `0%`, receive no `[ExcludeFromCodeCoverage]`, and are not subject to a percentage
      floor. Both csproj edits preserve CRLF and touch no property, reference, or ordering.
      Evidence: the diff plus the per-file coverage table.
- [ ] **AC6 — Test conventions and determinism.** All tests use **MSTest**, **Moq**, and
      **FluentAssertions**, follow Arrange-Act-Assert, and are deterministic and isolated: no
      temporary files, no external services, no live or shown forms, no popups, no message pumps, no
      `DoEvents`, no `Thread.Sleep`/`Task.Delay`, no unseeded randomness, no direct wall-clock reads.
      `async void` handlers are observed with a `TaskCompletionSource`. Every test class that mutates
      a process-global static is `[DoNotParallelize]` with restoring `[TestCleanup]`. No test is
      marked `LiveOutlook`. Evidence: the policy-audit artifact plus a green coverage-enabled test
      run.
- [ ] **AC7 — STA confinement.** Any test relying on the epic's STA last-resort clause is confined to
      `QuickFiler.Test/Viewers/EfcViewer.StaTests.cs`, is marked `[STATestClass]`, constructs at most
      one never-shown `EfcViewer` on a dedicated STA thread disposed in a `finally`, and carries a
      per-test XML doc comment stating why no seam could isolate the logic. **No `*.StaTests.cs` file
      exists for `EfcItemController` or `EfcFormController`** — every member of both is reachable
      through a seam or a handle-less control. Under a ratified Approach B, zero `*.StaTests.cs`
      files exist at all. Evidence: the test-file inventory plus the DEC-1 ratification record.
- [ ] **AC8 — Toolchain green in final form.** The full C# toolchain passes in order in a single
      final pass: `dotnet tool run csharpier format .` (DEC-3) → analyzer msbuild → nullable msbuild
      with `/p:TreatWarningsAsErrors=true` → `vstest.console.exe` with coverage. Evidence: four
      artifacts under `<FEATURE>/evidence/qa-gates/`, each with `Timestamp:`, `Command:`, and
      `EXIT_CODE: 0`.
- [ ] **AC9 — Repository-wide coverage retained or improved.** Repository-wide line coverage is
      retained or improved against the baseline measured on this branch, with **both** figures
      derived by the DEC-2 rule so the comparison is like-for-like. The artifact states the net line
      delta contributed by `EfcViewer.Designer.cs` entering the denominator, and — if the delta is
      negative — the specific mitigation applied. Evidence: a before/after repository-wide comparison
      under `<FEATURE>/evidence/qa-gates/`, with the pre-change baseline captured in Phase 0 under
      `<FEATURE>/evidence/baseline/`.
- [ ] **AC10 — No behavior change.** No observable QuickFiler flow changes. Characterization tests
      pin **current** behavior on the #439 path — `PopulateFolderCombobox`, `SearchText_TextChanged`,
      `RefreshSuggestionsAsync`, `ActionDeleteAsync`, `BindFolderRows`, `BindBreadcrumbRowsAsync`,
      `ConfigureBreadcrumbControl`, `SelectedFolder`, `IsValidSelection` — and assert that
      relative-stem rows pass through verbatim and that a row whose chain lookup yields `null` still
      binds. **No test asserts that a multi-segment lineage appears.** Open defect #439 is not fixed.
      Public constructor signatures consumed by F8 are unchanged; every new test entry point is an
      explicit overload, never an optional parameter. No sibling-owned file is edited. Evidence: the
      final diff plus the named characterization tests.
- [ ] **AC11 — Latent defects promoted, not left as prose.** Every latent defect discovered during
      research or execution is tracked as a GitHub issue via the MCP promotion lifecycle. The eight
      research-discovered defects are already promoted as #459, #460, #461, #463, #464, #465, #466,
      and #467 (DEC-4); **F9 fixes none of them**. Any defect newly discovered during execution is
      promoted before F9 completes, with its issue number recorded here. Evidence: the issue-number
      list in `spec.md` plus the execution-phase promotion record under `<FEATURE>/evidence/other/`.

## Non-Goals

- Raising the observable capability of QuickFiler. This child is an enabler; end-user behavior is
  unchanged.
- Fixing issue #439, and writing any test that asserts the lineage behavior #439 requests. F9's
  refactor relocates the fix point and says so; it does not apply the fix.
- Fixing any of the eight promoted latent defects (#459, #460, #461, #463, #464, #465, #466, #467).
  Several are pinned by characterization tests that assert the current, defective behavior.
- Editing any sibling-owned file: F8's `EfcHomeControllerDependencies.cs` and
  `EfcHomeControllerDependencyFactories.cs`, F5's `EfcDataModel.cs`, F12's
  `BreadcrumbBridgeRouter.cs`, F13's WebView2 host files, F14's `IItemViewer.cs` and `ItemViewer.cs`,
  F4's `EfcViewerQueue.cs` and `EfcThemeHelper.cs`, or F3's `KeyboardHandler.cs`.
- Editing `coverage.config`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, any shared build
  property file, `UtilitiesCS/Properties/AssemblyInfo.cs`, or `epic.md`. Corrections to `epic.md` are
  reported to the epic orchestrator, not applied by F9.
- Widening the `UtilitiesCS` `InternalsVisibleTo` grant. Where a `UtilitiesCS` internal is needed, F9
  builds a local seam in its own assignment.
- Fixing issue #441 in the shared Cobertura post-processor. F9 works around it with a binding
  derivation rule and discloses the limitation.
- Any work on `QuickFiler/Viewers/EfcViewer3.cs` or `EfcViewer3.Designer.cs`. Neither has a
  `<Compile Include>` entry, so both are outside the coverage denominator and outside the epic.
- Changing the repository-wide coverage thresholds themselves.
- Converting QuickFiler away from VSTO/WinForms. Where a seam choice is open, F9 prefers host-neutral
  extraction that a future WebView2/Office.js port can reuse.
