# quickfiler-test-form1-live-form (Spec)

- **Issue:** #491
- **Parent (optional):** epic `quickfiler-suite-determinism-foundation`
- **Owner:** drmoisan
- **Last Updated:** 2026-08-22T13-13
- **Status:** Partially delivered — 7 of 11 acceptance criteria met; 4 remaining, blocked on a pre-existing out-of-scope Form-derived type (`QfcFormViewerDerived`) discovered during execution (see `evidence/other/ac-status-summary.2026-08-22T13-13.md`)
- **Version:** 1.1

## Context

`QuickFiler.Test/Form1.cs` and `QuickFiler.Test/Form1.Designer.cs` declare
`public partial class Form1 : System.Windows.Forms.Form`, compiled directly into the
`QuickFiler.Test` unit-test assembly. `Form1.Designer.cs:32-34` constructs three
`QuickFiler.ItemViewer` instances inside `InitializeComponent`. Research confirms the type is
never instantiated by any test today, so no runtime failure currently occurs. The defect is
latent: `.claude/rules/general-unit-test.md` and this epic's determinism goal require that no
unit-test run construct a live WinForms window, and this type is one `new Form1()` call away from
breaching that rule while contributing no test value. It is dead weight that must be removed
before it can be misused.

This child is issue #491 of the `quickfiler-suite-determinism-foundation` epic, scoped in the
epic manifest to "live form in the test project... `Form1.cs` and its designer" only.

## Repro & Evidence

There is no runtime repro. `Form1` is never constructed by any existing test, `[TestClass]`, or
reflection-based discovery path — confirmed by a repository-wide word-boundary search for `Form1`
(82 files) and a targeted search for `Assembly.GetTypes`, `Activator.CreateInstance`, and
`GetType(` inside `QuickFiler.Test/` (4 call sites, none referencing `Form1`). No policy violation
is observable today by running the existing suite.

The defect is a latent policy violation and unused production surface inside the test assembly,
not an active runtime failure. Per the repository's Bugfix Workflow, the "repro" for this class of
defect is the regression guard test specified below: it must be **red** before removal (because
`Form1.Designer.cs:3` currently declares the only `Form`-derived type compiled into
`QuickFiler.Test`) and **green** after removal (because no `Form`-derived type remains in the
assembly). The guard's pre-change failure is the closest analogue to a repro this defect admits.

- Steps to reproduce: none — no test currently exercises `Form1`.
- Expected vs actual behavior: expected — no `Form`-derived type is compiled into a unit-test
  assembly; actual — one is (`QuickFiler.Test.Form1`), unused.
- Logs/screenshots/error snippets: none applicable.
- Frequency / determinism: not applicable; this is a static compile-time defect, not an
  intermittent runtime one.

## Scope & Non-Goals

- In scope:
  - Delete `QuickFiler.Test/Form1.cs`, `QuickFiler.Test/Form1.Designer.cs`, and
    `QuickFiler.Test/Form1.resx`.
  - Remove the corresponding `<Compile Include>` and `<EmbeddedResource Include>` entries from
    `QuickFiler.Test/QuickFiler.Test.csproj`, confined to the two owned regions described under
    Proposed Fix.
  - Add the assembly-level regression guard test specified under Test Strategy.
- Out of scope / non-goals:
  - **Item 2 of the potential document is explicitly deferred**, not addressed by this child. See
    the dedicated subsection below.
  - `QuickFiler/Viewers/Form1.cs` and `QuickFiler/Viewers/Form1.Designer.cs` — a different,
    unrelated production type in namespace `QuickFiler.Viewers`. It is a four-line
    constructor-only partial class with no further members, is not test code, and is not touched
    by this change.
  - `UtilitiesCS.Test/Form1.cs` and `SVGControl.Test/Form1.cs` — the same defect shape in two
    unrelated test assemblies. Separate defects, not addressed here.
  - `QuickFiler.Test/QuickFiler.Test.csproj.bak` — not referenced by `TaskMaster.sln` (the only
    solution reference to a `QuickFiler.Test.csproj`-named file is the live `.csproj` at
    `TaskMaster.sln:25`), not compiled by any toolchain command, and not part of the owned region.
    Left untouched.
  - Any part of `QuickFiler.Test.csproj` outside the two owned regions (see Proposed Fix). Three
    sibling epic children (`#511`/`#571`, `#445`, `#449`) work concurrently against the same file;
    touching any other region risks a fan-in conflict.
  - Any change to `System.Drawing`, `System.Drawing.Design`, or `System.Windows.Forms`
    `<Reference>` entries in `QuickFiler.Test.csproj`. These are retained (see Proposed Fix).
- Explicitly excluded systems, integrations, or datasets: none — this is a self-contained test
  assembly composition change with no data, config, or external-integration surface.

### Item 2 is out of scope for this child

The potential document's Item 2 (the three test-only `internal` members
`AttachBreadcrumbMessengerWhenReadyAsync`, `AttachBreadcrumbMessenger`, and `BreadcrumbOpenTask` in
the production file `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`) is deliberately deferred, for
three reasons, each verified directly rather than inferred:

1. **File-set boundary.** The epic manifest's Scope section (`epic.md:68-69`) scopes #491
   explicitly to "live form in the test project... `Form1.cs` and its designer." Item 2 touches a
   wholly different production file with no textual or structural overlap with `Form1`.
2. **Epic-level non-goal.** The epic's Non-Goals section reserves the `IItemViewer` UI-thread seam
   consolidation (#489) — which rewrites `IItemViewer`, `ItemViewer.cs`, and
   `ItemViewer.WebViewThread.cs` — for a later epic's ItemViewer child. Item 2's members live in a
   sibling partial-class file of the same `ItemViewer` type family
   (`ItemViewer.Breadcrumb.cs`), so deciding whether to promote them into the production call path
   is a design decision about `ItemViewer`'s breadcrumb-attach contract, squarely the kind of
   decision the epic reserves for that later child.
3. **Explicit write prohibition.** The epic's Hard Constraint 1 forbids any child of this epic from
   editing `.claude/**`, and its Recorded Preconditions bar writing under
   `docs/features/potential/**`. Both of Item 2's candidate dispositions — promoting the members
   into the production attach path, or documenting them as intentional test seams — require either
   a code change to a file this child does not own, or a documentation update that would need to
   live under a restricted location to record the seam status formally.

Re-verification during research (2026-08-21) confirmed the three members are still present, still
`internal`, and still production-callerless: a repository-wide search for
`AttachBreadcrumbMessengerWhenReadyAsync` and `AttachBreadcrumbMessenger` inside `QuickFiler/`
(production code only) returns exactly one file, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
itself (the declaration site). Nothing is lost by deferring: the members and their five dependent
test files (`BreadcrumbCollapsedSurfaceReadinessTests.cs`, `BreadcrumbSubfolderActivationTests.cs`,
`BreadcrumbSelectorOpenRetryTests.cs`, `BreadcrumbCoordinatorLifecycleTests.cs`,
`BreadcrumbDropDownIntegrationTests.cs`) continue to function unchanged whether or not this child
runs. The orchestrator is responsible for reporting Item 2 upward for scheduling in the later
ItemViewer-owning epic; this spec records the deferral so it is not silently dropped.

## Root Cause Analysis

- Current hypothesis or confirmed root cause: `QuickFiler.Test/Form1.cs` and
  `QuickFiler.Test/Form1.Designer.cs` were added to the test assembly as a manual/visual harness —
  `Form1.cs:22-34`'s `LoadControlGroup` method exists solely to demonstrate manually adding
  `ItemViewer` controls to a `TableLayoutPanel` at design time, not to run as an automated test.
  No production or test caller ever needed it to compile into `QuickFiler.Test`; it was never
  removed after ceasing to serve that manual purpose.
- Signals/evidence supporting it: zero references to `Form1` outside its own three files and the
  four `QuickFiler.Test.csproj` entries; zero reflection-based discovery paths reach it; the
  `.resx` carries zero `<data>` elements (pure WinForms-designer boilerplate with nothing to load).
- Affected components/modules (paths, services, pipelines):
  - `QuickFiler.Test/Form1.cs`
  - `QuickFiler.Test/Form1.Designer.cs`
  - `QuickFiler.Test/Form1.resx`
  - `QuickFiler.Test/QuickFiler.Test.csproj` (compile/resource entries only)

## Proposed Fix

### Design summary (what changes where)

Delete the three dead files and their `QuickFiler.Test.csproj` entries. Add one new MSTest guard
that asserts, by reflection over type metadata only, that the executing test assembly contains no
`System.Windows.Forms.Form`-derived type. This converts a currently-unenforced policy expectation
into a permanent, automatically-checked invariant, and prevents the class of regression this issue
reports from recurring.

### Boundaries and invariants to preserve

- `QuickFiler/Viewers/Form1.cs` (the unrelated production type in `QuickFiler.Viewers`) is not
  touched.
- The three `<Reference Include="System.Drawing" />`,
  `<Reference Include="System.Drawing.Design" />`, and
  `<Reference Include="System.Windows.Forms" />` entries in `QuickFiler.Test.csproj` are retained.
  46 other files in `QuickFiler.Test` (spanning `Viewers/`, `Controllers/`, `TestSupport/`, and
  `Helper Classes/`, including `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`) depend on
  `System.Windows.Forms` via `using` directives, so these references are load-bearing far beyond
  `Form1` and must not be removed.
- `QuickFiler.Test/QuickFiler.Test.csproj.bak` is not edited.

### Dependencies or blocked work

None. The epic's dependency graph for wave 0 is empty; #491 has no `depends_on` edge to any
sibling child, and its owned csproj regions do not overlap any sibling child's region.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

- Delete: `QuickFiler.Test/Form1.cs`, `QuickFiler.Test/Form1.Designer.cs`,
  `QuickFiler.Test/Form1.resx`.
- Add: a new MSTest guard test file, recommended path
  `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` (mirroring the existing repository style of
  placing test files at the `<Project>.Test/` root, alongside
  `QuickFiler.Test/QfcViewer_Test.cs` and `QuickFiler.Test/SetupAssemblyInitializer.cs`).
- Edit: `QuickFiler.Test/QuickFiler.Test.csproj`, confined to two owned regions.

#### CSPROJ region ownership (re-derive before editing)

This child owns exactly two regions of `QuickFiler.Test/QuickFiler.Test.csproj`, and the executor
must re-derive the current line numbers from the working tree before editing rather than trusting
any number recorded here or in the epic manifest:

- **Lines 161-166** — the `Form1.cs` and `Form1.Designer.cs` `<Compile Include>` blocks, closing
  tags included. (The epic manifest cites "161-165"; the closing tag at 166 is part of the region
  and must be included in the edit.)
- **Lines 179-183** — the entire `<ItemGroup>` whose sole child is the `Form1.resx`
  `<EmbeddedResource>` element. (The epic manifest cites "180-181"; lines 179 and 183 are the
  `<ItemGroup>` open/close tags wrapping that single child, and removing the child without removing
  the now-empty wrapper leaves dead structure with no purpose.)

Because a new test file requires its own `<Compile Include>` entry, and adding that entry anywhere
outside these two regions would collide with a sibling child's concurrent edit, the new entry must
be placed **inside** the owned region: lines 161-166 are replaced by a single new
`<Compile Include="NoLiveFormInTestAssemblyTests.cs" />` entry, so the net edit to the csproj stays
wholly within the two owned regions. Lines 179-183 are deleted in full (the `<ItemGroup>` becomes
empty and is removed, not merely its child element).

No other part of `QuickFiler.Test.csproj` is touched. In particular, the `Controllers` item group
that sibling child #449 appends to (ending at line 178 per research) is left untouched, and the
three `System.Windows.Forms`-family `<Reference>` entries are left untouched.

#### Functions/classes/CLI commands impacted

- Removed: `QuickFiler.Test.Form1` (partial class, two files).
- Added: one new `[TestClass]` in `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` containing one
  `[TestMethod]` implementing the regression guard (see Test Strategy).
- No production code, CLI command, or public API is touched.

#### Data flow and validation changes

None. This is a test-assembly composition change; no runtime data flow is affected.

#### Error handling and logging updates

None applicable — no error-handling or logging code is touched by this change.

#### Rollback/feature-flag considerations (if applicable)

No feature flag is needed. Rollback is a plain revert of the commit; the deleted files' history
remains available in git if a manual visual harness is ever wanted outside the unit-test assembly.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

Not applicable — no interface or contract is added or changed.

#### Required configuration keys and defaults

None. `coverage.config` requires no change: it excludes only third-party module paths by regex
(`Deedle`, `FSharp`, `Castle\.Core`, `FluentAssertions`, `Moq`, `Microsoft\.Testing`, `MSTest`) and
carries no `QuickFiler.Test` or first-party exclusion entry, so `QuickFiler.Test.dll` remains fully
instrumented after the change with no configuration edit required.

#### Backward-compatibility expectations

No public API is removed or changed; `Form1` was never a public contract consumed outside its own
files. No backward-compatibility break is introduced.

#### Performance constraints (latency/throughput/memory)

Not applicable — no measurable performance surface is affected.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): the worktree state matches the research document's
  2026-08-21 re-derivation; the executor must re-confirm all cited line numbers and search results
  before editing, per the epic's "Known-Stale Potential-Document References" warning.
- Constraints (budget, performance, compatibility):
  - No `.claude/**` file may be edited (epic Hard Constraint 1); a rule file cited here is the
    policy the fix is measured against, not an edit target.
  - `vstest` invocations must carry `/InIsolation` and
    `/TestCaseFilter:"TestCategory!=LiveOutlook"` (epic Hard Constraint 2); omitting `/InIsolation`
    produces roughly 1,695 phantom failures from a Moq `TypeInitializationException` that must not
    be mistaken for a real regression.
  - Recursive `*.Test.dll` discovery must exclude `\.claude\worktrees\` paths to avoid loading
    stale agent-worktree builds.
  - `msbuild` analyzer and nullable gates must use `/t:Rebuild`, never `/t:Build` (a warm
    `/t:Build` skips `CoreCompile` and the gate cannot fail), and must never pass
    `/p:Nullable=enable` (no project in this repository opts in solution-wide, and forcing it
    produces hundreds of unrelated errors).
  - CRLF line endings in `QuickFiler.Test.csproj` must be preserved; edits should be confined to
    minimal adjacent hunks within the two owned regions.
- External dependencies (services, libraries, releases): none beyond the existing MSTest, Moq, and
  FluentAssertions packages already referenced by `QuickFiler.Test.csproj`.

## Data / API / Config Impact

- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): none. `coverage.config` is unchanged
  (see Technical Specifications above).

## Test Strategy

### Regression guard — the load-bearing design decision

Because `Form1` is never instantiated, there is no runtime repro to reproduce with a failing test.
The regression test required by the Bugfix Workflow is instead a deterministic, assembly-level
structural guard:

- A single MSTest `[TestClass]`, recommended file
  `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs`, asserting via FluentAssertions that
  `Assembly.GetExecutingAssembly().GetTypes()` contains **no** type assignable to
  `System.Windows.Forms.Form`.
- The check must be scoped to the **executing** (`QuickFiler.Test`) assembly only, never to a
  referenced assembly. `QuickFiler/Viewers/Form1.cs` is a legitimate production `Form`-derived type
  in the referenced `QuickFiler` assembly, and the guard must not flag it.
- The test must **not** construct any form, control, or `BackgroundWorker`. Reflection over
  `Type` metadata (`GetTypes()`, `IsAssignableFrom`/`IsSubclassOf`) is metadata-only and requires no
  instantiation; instantiating any WinForms type inside a unit test is exactly the failure mode
  this issue exists to prevent.
- Verified pre-change state: `Form1.Designer.cs:3` declares the only `Form`-derived type in
  `QuickFiler.Test` today, so this guard is **red** before the fix (fails, because `Form1` is
  found) and **green** after (passes, because no `Form`-derived type remains).
- Frameworks: MSTest attributes (`[TestClass]`, `[TestMethod]`), FluentAssertions for the
  assertion. Moq is not needed — the guard has no dependency to mock.

### Unit tests (MSTest) for the fixed behavior and boundaries

- New: the guard test above (positive case — after the fix, the assertion passes).
- Existing: no existing `QuickFiler.Test` test constructs, references, or depends on `Form1`, so no
  existing test requires modification as a direct consequence of this change.

### Edge cases and negative scenarios (invalid inputs, missing data, boundary values)

- The guard must not produce a false pass by scoping to the wrong assembly (e.g., accidentally
  scanning the referenced `QuickFiler` assembly, which legitimately contains `Form`-derived types).
  This is covered by asserting against `Assembly.GetExecutingAssembly()` specifically.
- The guard must not produce a false pass by matching only the exact type `Form1` rather than any
  `Form`-derived type; the assertion checks assignability to `System.Windows.Forms.Form`, not a
  named-type comparison, so it also catches any future reintroduction of a differently-named live
  form.

### Error handling and logging verification

Not applicable — no error-handling or logging path is introduced or changed.

### Coverage impact and targets for changed lines/modules

`QuickFiler.Test.Form1` contributes 187 always-uncovered coverable lines (157 from
`Form1.Designer.cs`, 30 from `Form1.cs`), all recorded at `hits="0"` in the baseline Cobertura
evidence. That 187-line figure is real, but it describes a **raw, unfiltered `dotnet-coverage`
denominator only** — the count of coverable lines `dotnet-coverage` reports before any
repository-specific post-processing. For the harness this spec mandates
(`scripts\vscode\Invoke-MSTestWithCoverage.ps1`), the expected effect of removing `Form1` on the
measured figure is **no change**: an expected delta of exactly 0 on both `lines-valid` and
`lines-covered`.

This was verified by reading the harness implementation directly, at two independent points where
`QuickFiler.Test.dll` — the assembly `Form1` is compiled into — is excluded from measurement:

1. **Instrumentation exclusion.** `scripts/vscode/Invoke-MSTestWithCoverage.ps1:99` sets
   `$testAssemblyPattern = '.*\.Test\.dll$'` and appends it to the derived settings file's
   `ModulePaths/Exclude` list, so `QuickFiler.Test.dll` is never instrumented by `dotnet-coverage`
   in the first place.
2. **Allowlist exclusion.** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:39-41` skips
   every project whose resolved assembly name ends in `.Test` when building the Koverage project
   allowlist. The in-file comment at `:20-23` states the intent directly: test projects are
   excluded so that `ConvertTo-KoverageCoberturaXml` strips their `<package>` elements from **both**
   the numerator (`lines-covered`) and the denominator (`lines-valid`). That strip is performed at
   `:417-421`, and the root `<coverage>` element's `line-rate`, `lines-covered`, and `lines-valid`
   attributes are recomputed from the surviving packages at `:442-445`.

Because `Form1`'s 187 lines live in `QuickFiler.Test.dll`, they are outside this harness's
denominator **both before and after** the change: `QuickFiler.Test` is never instrumented (point 1)
and, even if it were, its packages would be stripped from the Koverage-filtered totals before those
totals are written back to the output file (point 2). The 187-line reduction is a real property of
a raw `dotnet-coverage collect` capture; it is not a property of the filtered figure this spec's
acceptance criteria are measured against.

- Baseline capture (before any file is deleted):
  ```
  pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 `
    -SearchRoot . `
    -Configuration Debug `
    -CoverageOutput 'docs\features\active\2026-08-07-quickfiler-test-form1-live-form-491\evidence\baseline\coverage-baseline.cobertura.xml'
  ```
- Post-change capture (after the fix, same harness):
  ```
  pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 `
    -SearchRoot . `
    -Configuration Debug `
    -CoverageOutput 'docs\features\active\2026-08-07-quickfiler-test-form1-live-form-491\evidence\qa-gates\coverage-postchange.cobertura.xml'
  ```
- Both parameter names (`-SearchRoot`, `-Configuration`, `-CoverageOutput`) are confirmed from the
  script's `param()` block.
- **Two-denominator hazard.** `Invoke-MSTestWithCoverage.ps1` post-processes the raw Cobertura
  output for Koverage compatibility and strips `<package>` elements for third-party assemblies not
  part of the solution, so it emits a filtered, first-party-only figure as its final output. The
  baseline and post-change figures must both come from this same script's output; a raw
  `dotnet-coverage collect` figure must never be substituted for either side of the comparison, and
  a filtered figure must never be compared against an unfiltered one.
  - **Sequencing detail (verified by reading the script).** `Invoke-MSTestWithCoverage.ps1` writes
    the **raw** Cobertura capture to the path named by `-CoverageOutput` first, then calls
    `Assert-CoberturaLineCoverageThreshold` on the filtered content at `:341`, and only overwrites
    that same file with the **filtered** content at `:343` if the assertion at `:341` does not
    throw. `Assert-CoberturaLineCoverageThreshold` (`Invoke-MSTestWithCoverage.Helpers.ps1:487-490`)
    throws when computed line coverage is below 80%. If it throws, execution stops before `:343`
    runs, and the file left on disk at `-CoverageOutput` is the **raw, unfiltered** capture — not
    the filtered figure a reader might expect. A capture must therefore be checked for a zero exit
    code from `Invoke-MSTestWithCoverage.ps1` before either side of the baseline/post-change
    comparison is read from disk; a nonzero exit invalidates that side of the comparison regardless
    of what the file on disk appears to contain.
- Acceptance condition: post-change line coverage (from the filtered Koverage output) is `>=`
  baseline line coverage (from the same filtered output), with both values recorded as actual
  numbers in the evidence artifacts above — not as placeholders or estimates. For this specific
  change, the expected delta is 0 on both `lines-valid` and `lines-covered` (see "Coverage impact
  and targets for changed lines/modules" above); a delta of 0 satisfies this condition, and any
  observed delta must still be recorded as an actual number rather than assumed.

### Toolchain commands to run (format -> lint -> type-check -> test)

Run in this exact order, restarting from the top if any step fails or modifies files:

1. `dotnet tool run csharpier format .` then verify with `dotnet tool run csharpier check .`
   (`.csharpierignore` excludes `*.csproj`, so the csproj edit is invisible to this check; the new
   `.cs` test file is formatted and checked normally).
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
   (no `/p:Nullable=enable`)
4. `vstest.console.exe <QuickFiler.Test assembly path> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`,
   with recursive `*.Test.dll` discovery (if used) excluding `\.claude\worktrees\`.

### Manual validation steps (if required)

None required. The guard test and the coverage comparison are sufficient automated verification;
there is no user-facing surface to validate manually.

## Acceptance Criteria

- [ ] No `System.Windows.Forms.Form`-derived type is compiled into the `QuickFiler.Test` assembly,
      proven by a named MSTest guard test (`NoLiveFormInTestAssemblyTests` or equivalent) that
      reflects over `Assembly.GetExecutingAssembly().GetTypes()` and fails if any such type is
      present.
- [x] `QuickFiler.Test/Form1.cs`, `QuickFiler.Test/Form1.Designer.cs`, and
      `QuickFiler.Test/Form1.resx` are deleted from the working tree.
- [x] The corresponding `<Compile Include>` and `<EmbeddedResource Include>` entries are removed
      from `QuickFiler.Test/QuickFiler.Test.csproj`, with the edit confined to the two owned line
      regions (the re-derived `Form1.cs`/`Form1.Designer.cs` compile block, and the re-derived
      `Form1.resx` `<ItemGroup>`), and with the new guard test's `<Compile Include>` entry placed
      inside the same owned region rather than elsewhere in the file.
- [x] The `<Reference Include="System.Drawing" />`, `<Reference Include="System.Drawing.Design" />`,
      and `<Reference Include="System.Windows.Forms" />` entries remain present and unmodified in
      `QuickFiler.Test.csproj`.
- [x] `dotnet tool run csharpier format .` and `dotnet tool run csharpier check .` both complete
      with no diffs.
- [x] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      completes with zero analyzer errors.
- [x] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      completes with zero errors, and no command in this change ever passes
      `/p:Nullable=enable`.
- [ ] `vstest.console.exe <QuickFiler.Test assembly path> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
      completes with zero failing tests.
- [ ] No pre-existing `QuickFiler.Test` test regresses as a result of this change (test-count and
      pass-count parity with the pre-change run, apart from the one new guard test).
- [ ] Post-change line coverage (captured via `Invoke-MSTestWithCoverage.ps1`) is greater than or
      equal to the baseline line coverage captured via the same script before the change, with both
      values recorded as actual numbers in the evidence artifacts. For this harness the expected
      delta is 0 (see Coverage impact and targets above); the criterion remains satisfied by an
      observed delta of 0 and is not satisfied by an unrecorded or estimated value.
- [x] Item 2 of the potential document (the three `internal` members of
      `ItemViewer.Breadcrumb.cs`) is explicitly recorded as deferred to a later epic's
      ItemViewer-owning child, not silently dropped from tracking.

## Risks & Mitigations

- Technical or operational risks:
  - **Csproj fan-in conflict.** Three sibling epic children (`#511`/`#571`, `#445`, `#449`) edit
    the same `QuickFiler.Test.csproj` concurrently. An edit outside the two owned regions risks
    colliding with a sibling's concurrent change.
    - Mitigation: confine every csproj edit to the two owned regions (161-166, 179-183, re-derived
      at edit time), including the new guard test's compile entry.
  - **Line-number drift.** The epic manifest's cited line numbers (161-165, 180-181) already
    differ slightly from the freshly re-derived numbers (161-166, 179-183) used in this spec.
    - Mitigation: the executor re-derives exact line numbers from the working tree immediately
      before editing, per the epic's "Known-Stale Potential-Document References" warning, and does
      not trust any cited number, including the ones in this spec.
  - **Coverage-figure hazard.** Comparing a raw `dotnet-coverage collect` figure against the
    harness's Koverage-filtered figure would produce a meaningless, non-comparable pair of numbers.
    - Mitigation: both the baseline and post-change captures use the identical
      `Invoke-MSTestWithCoverage.ps1` invocation shape, so both sides are filtered identically.
  - **Phantom vstest failures.** Omitting `/InIsolation` produces roughly 1,695 unrelated phantom
    failures that could be mistaken for a real regression caused by this change.
    - Mitigation: always run `vstest.console.exe` with `/InIsolation` and the documented
      `TestCategory!=LiveOutlook` filter, per epic Hard Constraint 2.
- Mitigations and rollbacks: the change is a pure deletion plus one additive guard test; rollback
  is a plain `git revert` of the commit, with no data migration or feature flag involved.

### Corrected assumption: coverage-delta claim (preflight finding)

The original research for this child computed the 187-line figure from the raw Cobertura
denominator without reading the implementation of
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, and the research document says so
explicitly. That gap produced an incorrect claim, since corrected in the "Coverage impact and
targets" subsection above: the harness excludes `.Test`-suffixed assemblies from both
instrumentation and the Koverage allowlist, so removing `Form1` is expected to leave the measured
`lines-valid` and `lines-covered` totals unchanged, not to raise them.

- Mitigation: coverage claims about this repository's measured figures must be verified against
  the harness's post-processing code (`Invoke-MSTestWithCoverage.ps1` and its `Helpers.ps1`
  companion), not inferred from a raw Cobertura capture or from the coverable-line count alone.
  Any future spec or plan asserting a specific coverage-delta direction must cite the harness
  behavior it relies on, the same way this correction does.

## Rollout & Follow-up

- Release/rollout steps: standard PR merge through the epic's per-child pull-request flow; no
  staged rollout, feature flag, or migration is required.
- Post-fix monitoring or clean-up tasks:
  - The orchestrator reports Item 2 (the three `ItemViewer.Breadcrumb.cs` test-only members)
    upward for scheduling as a separate issue in the later ItemViewer-owning epic.
  - Optional, not required: `QuickFiler.Test/QuickFiler.Test.csproj.bak` also carries stale `Form1`
    entries (at its own line numbers 82-98) and could be deleted for hygiene in a future,
    unrelated change; it carries no build risk either way and is explicitly out of scope here.
- Links: issue #491
  (https://github.com/drmoisan/TaskMaster/issues/491); epic
  `docs/features/epics/quickfiler-suite-determinism-foundation/epic.md`; research
  `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/research/form1-removal-research.2026-08-21T18-15.md`;
  original potential document
  `docs/features/potential/promoted/2026-08-07-quickfiler-test-form1-live-form.md`.
