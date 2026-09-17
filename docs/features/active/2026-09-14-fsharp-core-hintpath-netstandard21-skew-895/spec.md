# fsharp-core-hintpath-netstandard21-skew (Spec)

- **Issue:** #895
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-16T23-27
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug (this file is the only acceptance-criteria source; there is no user-story.md by design)

> Formatting rule for this document — do not "fix" it. Backtick-delimited repository paths appear only in
> the `## Write Set` section and in the "Files/modules to change" subsection, and the two lists are
> identical. A downstream tool harvests backticked path tokens to derive the change footprint, so every
> other path in this document (comparison files, out-of-scope files, evidence folders, line citations) is
> written as plain prose on purpose. Code identifiers such as class, method and XML element names are
> not paths and may keep their spans.

## Context
The six FSharp.Core HintPath entries in the solution are split between the lib/netstandard2.0 and
lib/netstandard2.1 flavours of the FSharp.Core 11.0.100 package. The netstandard2.1 flavour references
`netstandard, Version=2.1.0.0`, an identity that does not exist for .NET Framework on any machine, so that
copy is unloadable wherever it is deployed. Which flavour lands in any given output directory is
last-writer-wins build-order nondeterminism, so a project can flip between working and broken across a
rebuild with no source change.

Environment:
- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8.1 (net481)
- Python version: not applicable
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`
- Data source or fixture: the restored package folder packages/FSharp.Core.11.0.100, which ships both
  lib/netstandard2.0 and lib/netstandard2.1

Impact / Severity: High (Blocker, Medium and Low were considered and not selected).

The add-in project file (TaskMaster.csproj, lines 501 and 517) references QuickFiler and ToDoModel, both
on netstandard2.1, alongside four netstandard2.0 projects. The add-in output directory currently receives
the netstandard2.0 copy, which is why the reported production failure does not reproduce there today.
That is build-order luck, not correctness: the add-in can flip back into the failing state on any rebuild
without a source change. The originally reported failure in issue #879 is evidence that it has already
been in that state once.

## Repro & Evidence
Steps to Reproduce:
1. Rebuild the solution.
2. Create a child AppDomain whose ApplicationBase is QuickFiler.Test/bin/Debug and whose
   ConfigurationFile is QuickFiler.Test.dll.config.
3. From inside that domain, load Deedle.dll from the same directory and invoke
   `Deedle.Frame.FromRecords<T>(IEnumerable<T>)` over a one-element sequence of any record type.
4. Repeat with ApplicationBase set to TaskMaster.Test/bin/Debug.

Expected:
Both directories deploy the same, loadable FSharp.Core, so the Deedle call behaves identically in each and
no output directory depends on build ordering for correctness.

Actual:
Rooted at QuickFiler.Test/bin/Debug the call raises the production chain; rooted at
TaskMaster.Test/bin/Debug it returns a Frame normally.

```
TypeInitializationException [Deedle.Reflection]
 ---> TypeInitializationException [<StartupCode$Deedle>.$FrameUtils]
 ---> FileNotFoundException [netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51]
```

Logs / Screenshots:
- Attached minimal logs or screenshot: yes (the measured HintPath split below, re-derived against the
  current tree by the research artifact; line numbers are current as of 2026-09-16).

Measured HintPath split, by file and line (HintPath element line; the Reference element is the line above):

| Project file | Line | Flavour | netstandard reference | Touched by this fix |
|---|---|---|---|---|
| QuickFiler/QuickFiler.csproj | 52 | lib\netstandard2.1 | 2.1.0.0 | yes |
| QuickFiler.Test/QuickFiler.Test.csproj | 259 | lib\netstandard2.1 | 2.1.0.0 | yes |
| ToDoModel/ToDoModel.csproj | 42 | lib\netstandard2.1 | 2.1.0.0 | yes |
| UtilitiesCS/UtilitiesCS.csproj | 70 | lib\netstandard2.0 | 2.0.0.0 | no (already correct) |
| UtilitiesCS.Test/UtilitiesCS.Test.csproj | 599 | lib\netstandard2.0 | 2.0.0.0 | no (already correct) |
| ToDoModel.Test/ToDoModel.Test.csproj | 96 | lib\netstandard2.0 | 2.0.0.0 | no (already correct) |

The issue table cited UtilitiesCS.Test line 598; that is the Reference element line. The HintPath element is
line 599. Same block, no content drift.

netstandard 2.0.0.0 resolves from the GAC; netstandard 2.1.0.0 does not exist for .NET Framework.
Deedle.dll 3.0.0.0 references netstandard 2.0.0.0 and FSharp.Core 4.5.0.0, so Deedle is never the
source of the 2.1.0.0 request. FSharp.Core is.

Additional verified facts from the research artifact (research/2026-09-16T23-30-fsharp-core-hintpath-research.md
in this feature folder):
- There is no PackageReference form of FSharp.Core anywhere; the solution is packages.config-only. The
  other twelve of the eighteen project files contain no FSharp.Core text at all.
- No props, targets, Content or None item, and no package build asset can copy an FSharp.Core.dll into any
  output directory; the six HintPaths are the only selection mechanism. The package on disk contains only
  lib/netstandard2.0 and lib/netstandard2.1 binaries.
- Fifteen Debug|Any CPU output directories receive a copy of FSharp.Core.dll, directly or transitively via
  ProjectReference: QuickFiler, QuickFiler.Test, ToDoModel, UtilitiesCS, UtilitiesCS.Test, ToDoModel.Test,
  Tags, Tags.Test, VBFunctions.Test, TaskTree, TaskTree.Test, TaskVisualization, TaskVisualization.Test,
  TaskMaster, TaskMaster.Test. SVGControl, SVGControl.Test and VBFunctions receive none.
- On the unfixed tree QuickFiler, QuickFiler.Test and ToDoModel deploy the netstandard2.1 flavour
  deterministically (their own HintPath is a primary reference and wins). TaskTree, TaskVisualization,
  TaskTree.Test, TaskVisualization.Test, TaskMaster and TaskMaster.Test have mixed-flavour parents and
  their deployed flavour is unspecified by any source file; those six are the ones that can flip.
- All fifteen app.config FSharp.Core binding redirects are on the assembly-version axis (0.0.0.0-11.0.0.0
  to 11.0.0.0), identical for both flavours. No config file names a flavour or a lib path.

## Write Set
Every file this fix creates or modifies, as repository-relative paths (one per line). Nothing else is
written. Research and evidence artifacts under this feature folder are excluded from the write set by
convention.

- `QuickFiler/QuickFiler.csproj`
- `QuickFiler.Test/QuickFiler.Test.csproj`
- `ToDoModel/ToDoModel.csproj`
- `TaskMaster.Test/TaskMaster.Test.csproj`
- `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs`
- `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs`
- `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`

## Scope & Non-Goals
- In scope:
  - Three project-file edits, each changing exactly one HintPath value from the netstandard2.1 folder to
    the netstandard2.0 folder of the same FSharp.Core 11.0.100 package: QuickFiler (line 52),
    QuickFiler.Test (line 259), ToDoModel (line 42). No other line in those files changes.
  - Two new MSTest test files under the TaskMaster.Test Bootstrap folder: a Shape-A static source
    assertion (FSharpCoreHintPathAlignmentTests) and a Shape-B post-build deployed-binary assertion
    (FSharpCoreDeployedIdentityTests).
  - One edit to the TaskMaster.Test project file adding two explicit Compile Include items for the new
    files (that project lists sources explicitly; there is no wildcard, so an unregistered test file
    silently does not exist).
  - One comment-only edit to NetstandardBindChildDomainTests.cs correcting the `<remarks>` block on
    `ProbeApplicationBase` (lines 363-371 on the current tree), which states as fact that the
    QuickFiler.Test output directory deploys the flavour referencing netstandard 2.1.0.0. That statement
    is true today and false after this fix. No test logic, assertion, attribute or constant changes.
- Out of scope / non-goals (paths in this list are deliberately not backticked; they are not write
  claims):
  - Latent defect 1, packages.config omission in ToDoModel.Test: ToDoModel.Test/packages.config carries
    no FSharp.Core and no Deedle entry although ToDoModel.Test/ToDoModel.Test.csproj (lines 92-96)
    carries HintPaths for both. The HintPaths resolve today only because five sibling projects restore
    the same package folders; on the next FSharp.Core or Deedle bump the sync script skips them (it only
    rewrites HintPaths whose package id is in the project's own packages.config) and the project would
    fail with CS0006. This is a separate correctness gap; not fixed here.
  - Latent defect 2, inverted TFM preference in scripts/vscode/Sync-PackageReferences.ps1 (lines 13-19):
    the fallback list ranks netstandard2.1 ahead of netstandard2.0, which is inverted for .NET Framework
    targets (net481 implements .NET Standard 2.0 at most). This script is the only tool in the repository
    that can write a netstandard2.1 HintPath into a project file and is the plausible origin of this skew.
    Reordering it would require new Pester coverage under the Pester gate and has no in-scope trigger once
    the six HintPaths resolve; not fixed here.
  - Issue #898 (Meziantou analyzer HintPath skew): same class of file, different package, explicitly a
    separate run. This fix does not reformat, reorder or renumber any other item in the three project
    files it touches.
  - Any app.config change. All FSharp.Core binding redirects are on the assembly-version axis and are
    unaffected by flavour. The #879 netstandard redirect in TaskMaster/app.config (lines 72-75) is
    independent of this fix and must not be removed.
  - Any packages.config change (all six FSharp.Core entries that exist already pin 11.0.100).
  - Any behavioural change to NetstandardBindChildDomainTests.cs, ChildDomainBindProbe.cs,
    AddInEagerInstallShapeTests.cs or UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs (the #879 remedy).
    Their catches, handlers and assertions are not widened, narrowed or removed.
  - Any change to the other three FSharp.Core HintPaths (UtilitiesCS, UtilitiesCS.Test, ToDoModel.Test),
    which are already correct.
  - Any Release, x86 or x64 output directory (the mandated build command produces Debug|Any CPU only;
    Shape A covers those configurations at the source level because it reads project files).
- Explicitly excluded systems, integrations, or datasets: Outlook runtime, the VSTO add-in lifecycle,
  the GAC contents of any machine, and the CI workflow files (CI already runs nuget restore then msbuild
  and needs no change).

## Root Cause Analysis
A NuGet package shipping both lib/netstandard2.0 and lib/netstandard2.1 produces per-project HintPath
skew in a legacy packages.config solution. When projects with different flavours feed the same output
directory, the copy that wins is MSB3277-class last-writer-wins.

Why MSBuild does not report it: all six Reference elements carry the identical identity
`FSharp.Core, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a`. The flavour difference
is invisible at the identity level, so ResolveAssemblyReferences sees no conflict, and the two binaries
differ only in which netstandard version they themselves reference.

Found while investigating issue #879. Two successive regression-probe designs for that issue returned green
against an unfixed tree solely because the probe's child domain was rooted at TaskMaster.Test/bin/Debug,
where nothing requests netstandard 2.1.0.0. The probe designs were correct; the directory was wrong.

Probable tool-level origin (hypothesis consistent with script logic; commit history was not consulted):
the sync script's inverted TFM preference list would produce exactly the observed three/three split when a
project's previous HintPath folder disappears in a package bump while projects already on netstandard2.0
keep resolving. See Non-Goals, latent defect 2.

## Proposed Fix

### Invariant established by this fix
Every FSharp.Core HintPath in every project file in the solution selects the lib\netstandard2.0 flavour,
and consequently every FSharp.Core.dll deployed to any Debug|Any CPU output directory of TaskMaster.sln
references netstandard 2.0.0.0 and no deployed copy references netstandard 2.1.0.0; the choice of flavour
no longer depends on build order anywhere.

### Trace of one accepted value (why the fix is load-bearing)
1. Accept point: ResolveAssemblyReferences accepts the QuickFiler HintPath (line 52) because the file
   exists and its identity matches the Reference Include. It validates identity only; it does not validate
   the target framework the binary was built for or the netstandard version the binary references.
2. Throw point: at first touch of a Deedle type in a process rooted at a directory holding that copy, the
   CLR binder resolves FSharp.Core's own reference to netstandard 2.1.0.0, finds no such identity on
   .NET Framework, and raises FileNotFoundException inside the `$FrameUtils` static constructor, surfaced
   as the TypeInitializationException chain in Repro & Evidence.
3. Current absorption point: nothing catches it by design. In the add-in it reaches the ribbon handler as
   a production failure (issue #879). In the test host it is not observed at all today only because the
   TaskMaster.Test output directory happens to receive the netstandard2.0 copy; build order, not code, is
   absorbing the defect, which is why the earlier #879 probes were green on an unfixed tree.
4. Where the fix moves detection: from runtime to source and build time. Shape A fails at the project file
   (no build needed) whenever any HintPath selects a flavour other than netstandard2.0 or the count is not
   six. Shape B fails at the deployed binary in each of the fifteen directories whenever any copy
   references 2.1.0.0, and its positive control proves the detector can see a 2.1.0.0 reference at all.
   Neither half suffices alone: Shape A cannot see what the build deployed (it cannot cover the six
   flip-capable directories), and Shape B cannot run without a built tree and covers one configuration.

Inverse constraint: no consumer-side catch is widened to "be safe". The #879 handler in
UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs and its negative control
`NegativeControl_WithoutInstall_Netstandard21Throws` (which binds the netstandard 2.1.0.0 display name
directly, not through FSharp.Core) are untouched and must still pass, including the negative control
failing without the installer. No netstandard binding redirect is added to any test host config.

### Design summary (what changes where):
Two-part fix.

1. Align the three currently-netstandard2.1 HintPaths on netstandard2.0, the only flavour loadable on
   .NET Framework. Each edit replaces one path segment inside the existing HintPath element and touches
   no other character in the file:

   ```
   before: ..\packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll
   after:  ..\packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll
   ```

   Applied at QuickFiler line 52, QuickFiler.Test line 259 and ToDoModel line 42.

2. Add two regression tests under the TaskMaster.Test Bootstrap folder (MSTest + FluentAssertions; Moq is
   not needed because there are no seams), registered by two explicit Compile Include items in the
   TaskMaster.Test project file:
   - Shape A, FSharpCoreHintPathAlignmentTests (static, no build, no restore required): discovers every
     project file under the repository root, collects every HintPath whose value ends in FSharp.Core.dll,
     and asserts (a) exactly six exist and (b) each ends in lib\netstandard2.0\FSharp.Core.dll. Both
     assertions are required: agreement-only would pass a tree edited to netstandard2.1 everywhere;
     flavour-only would pass vacuously if a HintPath were deleted.
   - Shape B, FSharpCoreDeployedIdentityTests (post-build): for each of the fifteen directories in a fixed
     literal list, opens the deployed FSharp.Core.dll with `PEReader`, reads
     `MetadataReader.AssemblyReferences`, and asserts the reference named `netstandard` has version
     2.0.0.0 and that no reference has version 2.1.0.0. A positive control reads the package's own
     lib/netstandard2.1 binary and asserts the detector reports 2.1.0.0, so a detector that finds no
     netstandard reference cannot pass every directory.

### Boundaries and invariants to preserve:
- The three edited project files change on exactly one line each; every other Reference, Compile,
  ProjectReference and property item keeps its text, order and position (issue #898 will touch these
  files separately).
- The three already-correct HintPaths are byte-identical before and after.
- No app.config, packages.config, props, targets, runsettings or coverage.config file changes.
- The #879 harness keeps its behaviour; only a `<remarks>` block changes.
- The new tests are read-only over files, hold no shared mutable state, use no clock, RNG, process,
  AppDomain or temporary file, and carry no `[DoNotParallelize]`; they run under the standard
  Workers=0 / ClassLevel runsettings unchanged.

### Dependencies or blocked work:
- Shape B depends on a completed real solution build (the mandated msbuild /t:Rebuild command) and on
  packages/ having been restored (for the positive control). Both are preconditions the test fails loudly
  on (InvalidOperationException naming the missing path), never skips.
- Issue #879 is closed on GitHub and its remedy is merged into this tree. Its active feature folder
  (`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/`, containing
  a fully checked plan) still exists on disk and has not yet been archived — correction of an earlier,
  inaccurate claim in this document that no such folder exists. The comment-only edit in this spec targets
  `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`, which is ordinary shared repository test
  code, not a file inside #879's own `docs/features/active/...` folder; nothing in this fix writes into
  #879's canonical feature-folder state, so the comment-only edit belongs to this issue.
- Issue #898 must not run concurrently against the same three project files; if it does, the merge must
  union item lists rather than resolve by ordering.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `QuickFiler/QuickFiler.csproj` — HintPath value edit only (line 52).
- `QuickFiler.Test/QuickFiler.Test.csproj` — HintPath value edit only (line 259).
- `ToDoModel/ToDoModel.csproj` — HintPath value edit only (line 42).
- `TaskMaster.Test/TaskMaster.Test.csproj` — add two Compile Include items beside the existing three
  Bootstrap entries (lines 324-326 on the current tree).
- `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` — new file, Shape A.
- `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` — new file, Shape B.
- `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` — comment-only correction of the
  `<remarks>` block on `ProbeApplicationBase` (lines 363-371). The assertion message at lines 206-207
  ("the flavour of FSharp.Core deployed beside Deedle is what determines whether the identity under test
  is requested at all") is flavour-neutral, remains accurate after the fix, and is not changed.

#### Functions/classes/CLI commands impacted:
- New test class `FSharpCoreHintPathAlignmentTests` with two tests:
  `SolutionHasExactlySixFSharpCoreHintPaths` and `EveryFSharpCoreHintPath_SelectsNetstandard20`.
- New test class `FSharpCoreDeployedIdentityTests` with a `[DataTestMethod]` carrying one `[DataRow]` per
  enumerated directory (fifteen literal rows, so a failure names the directory) named
  `DeployedFSharpCore_ReferencesNetstandard20`, plus the positive control
  `Detector_OnPackageNetstandard21Binary_Reports21`.
- Repository-root discovery in both classes walks up from `AppDomain.CurrentDomain.BaseDirectory` to the
  first directory containing TaskMaster.sln (existing precedent in RibbonControllerTests and
  SortEmail_Tests). Shape A's project-file enumeration must skip directories named .git, .claude,
  packages, bin, obj and node_modules; the .claude exclusion is load-bearing because agent worktrees are
  nested under .claude/worktrees and contain full copies of every project file, which would inflate the
  count past six.
- No production class, method or CLI command changes.

#### Data flow and validation changes:
- None at runtime. The change alters which binary is copied at build time; the assembly identity,
  binding redirects and public surface are identical between flavours.

#### Error handling and logging updates:
- None in production code. In the new tests: a missing output directory, a missing FSharp.Core.dll, or an
  unrestored package folder throws InvalidOperationException with the full path (fail loud, never skip,
  matching the #879 harness convention); a flavour mismatch is a FluentAssertions failure whose message
  includes the directory or project file and the observed version.

#### Rollback/feature-flag considerations (if applicable):
- Rollback is reverting the three one-line HintPath edits; the tests would then fail again, which is the
  intended signal. No feature flag applies.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- Shape A input: every project file under the repository root (after the exclusions above), parsed with
  `XDocument` (precedent: AddInEagerInstallShapeTests). Output: a list of (file, HintPath value) pairs;
  assertion targets are count == 6 and each value ends with lib\netstandard2.0\FSharp.Core.dll
  (ordinal, case-insensitive comparison on the path segment).
- Shape B input: the fifteen literal directory names, each resolved to repoRoot/name/bin/Debug/FSharp.Core.dll.
  Reader: `PEReader` over a read-only `FileStream`, `GetMetadataReader()`, iterate `AssemblyReferences`,
  `GetAssemblyReference(handle)`, compare `GetString(Name)` to "netstandard" and read `Version`. No
  assembly is loaded into any AppDomain, which is why fifteen same-identity files from different paths
  can be inspected in one process.
- Positive control input: the lib/netstandard2.1 binary under the package folder derived from any one of
  the six discovered HintPaths (replace the netstandard2.0 segment; do not hard-code the package version).

#### Required configuration keys and defaults:
- None. System.Reflection.Metadata 10.0.0.12 is already referenced by the TaskMaster.Test project file
  (line 241) with System.Collections.Immutable in its packages.config; no dependency edit is needed.

#### Backward-compatibility expectations:
- Full. Both flavours expose assembly version 11.0.0.0 and the same public API; consumers compiled against
  either are binary-compatible with the netstandard2.0 copy.

#### Performance constraints (latency/throughput/memory):
- Shape A reads eighteen small XML files; Shape B reads fifteen PE headers plus one control. Expected
  runtime well under one second combined; no constraint is at risk.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): the solution builds with the mandated msbuild command on a
  machine where FSharp.Core 11.0.0.0 is not in the GAC (if it were, copy-local would be false and no
  directory would receive a copy; not the case on the machines that produced the evidence). packages/ is
  restored before Shape B runs. Tests run against the Debug|Any CPU output tree.
- Constraints (budget, performance, compatibility): project files are excluded from CSharpier by
  the ignore file, so the format step will not touch the HintPath edits; the C# toolchain loop still
  applies because the repository's C# rule scopes project files. New test files must stay under 500 lines
  each. Tests must run in parallel under Workers=0 / ClassLevel with no serialisation attributes.
- External dependencies (services, libraries, releases): FSharp.Core 11.0.100 package (unchanged),
  System.Reflection.Metadata 10.0.0.12 (already referenced), MSTest, FluentAssertions.

## Data / API / Config Impact
- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): no config schema, CLI flag or version
  changes; app.config binding redirects are unchanged and remain correct for the netstandard2.0 flavour.

## Test Strategy
Seeded from issue (all three seeded areas are addressed below): unit coverage areas, integration scenario
to retest, manual verification notes.

Align all six HintPath entries on lib/netstandard2.0, which is the only flavour loadable on .NET
Framework. The issue's suggested build-time guard is realised as the Shape-B test so the skew cannot
silently return. Retest by rebuilding and re-running the child domain probe rooted at
QuickFiler.Test/bin/Debug; it must stop raising the chain. Manual verification is a fresh Outlook session
exercising the QuickFiler ribbon path that originally failed.

This is complementary to issue #879, not a replacement for it. That issue's remedy makes the add-in robust
to whichever flavour is deployed; this one removes the unloadable flavour so the question stops arising.
Expected side effect to record, not fix: `AfterInstall_DeedleTypeInitializerSucceeds` in the #879 harness
will pass with or without the #879 installer once QuickFiler.Test deploys the 2.0 flavour; its
discriminating power moves to the display-name tests, whose negative control is unaffected.

- Regression tests to add or update:
  - Shape A: FSharpCoreHintPathAlignmentTests (two tests named above). Observed failing on the unfixed
    tree with count 6 and three members on netstandard2.1; requires no build and no restore, so it is the
    first gate run in every loop.
  - Shape B: FSharpCoreDeployedIdentityTests (fifteen data rows plus the positive control). Observed
    failing on the unfixed tree in at least the QuickFiler, QuickFiler.Test and ToDoModel rows
    (deterministic), possibly more depending on build order. Runs only after a whole-solution
    /t:Rebuild, never after a partial build; the build log used for the expect-fail run and the
    pass-after run must show no "Skipping target CoreCompile" line for any project, otherwise the build
    was not real and the run is vacuous.
  - Update: the `<remarks>` block on `ProbeApplicationBase` in NetstandardBindChildDomainTests.cs
    (comment only).
- Unit tests for the fixed behavior and boundaries (MSTest, FluentAssertions; no Moq): count of six;
  flavour of each; per-directory netstandard version for all fifteen; positive control on the 2.1 binary.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): missing directory
  or file throws (broken precondition, not a pass); unrestored packages/ makes the control throw; a
  seventh HintPath or a HintPath under a renamed package folder fails Shape A; a tree with all six on
  netstandard2.1 fails Shape A on flavour and Shape B on every row.
- Error handling and logging verification: assertion messages name the offending file or directory and
  the observed version; thrown preconditions name the full path.
- Coverage impact and targets for changed lines/modules: no production .cs file changes, so the changed-
  line coverage delta is nil; new files are test code outside the denominator. Baseline and post-change
  repository-wide coverage artifacts and their comparison are still required by the plan template and must
  be stored under this feature folder's evidence/baseline and evidence/qa-gates directories; the known
  run-to-run spread of repository-wide coverage must not be attributed to this change.
- Toolchain commands to run (format -> lint -> type-check -> test), verbatim from CLAUDE.md, restarted
  from step 1 on any failure or file change:
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` with the standard runsettings
     (Workers=0, ClassLevel) unchanged.
- Manual validation steps (if required): fresh Outlook session exercising the QuickFiler ribbon path from
  issue #879 after the rebuild; record the outcome under this feature folder's evidence/other directory.
- Expect-fail evidence location: the unfixed-tree runs of Shape A and Shape B are captured under this
  feature folder's evidence/regression-testing directory with the exact command, EXIT_CODE and an output
  summary, before the project-file edit is applied. Diff bases for any footprint or containment check are
  anchored to origin/main after a fetch, never to local main.

## Acceptance Criteria
Each criterion is independently verifiable. Criteria 1, 2 and 5 detect the defect and must be run and
observed FAILING on the unfixed tree, with that failing run captured as evidence, before the fix is
applied; criteria 3 and 4 are containment and non-regression gates whose pass condition is stated
exactly so it cannot be satisfied vacuously.

- [x] AC1. Exactly six FSharp.Core HintPath entries exist across all project files in the solution
      (enumerated from the repository root, excluding .git, .claude, packages, bin, obj and node_modules
      directories), and every one of the six ends in lib\netstandard2.0\FSharp.Core.dll. Verified by the
      new Shape-A tests `SolutionHasExactlySixFSharpCoreHintPaths` and
      `EveryFSharpCoreHintPath_SelectsNetstandard20` in FSharpCoreHintPathAlignmentTests. Pre-fix
      observation required: run on the unfixed tree and observe the flavour test FAILING with three of six
      members on netstandard2.1 (QuickFiler line 52, QuickFiler.Test line 259, ToDoModel line 42) while
      the count test passes with six; capture the run under evidence/regression-testing.
- [x] AC2. After a whole-solution rebuild with the mandated msbuild /t:Rebuild command whose log contains
      no "Skipping target CoreCompile" line, for each of the fifteen enumerated Debug|Any CPU output
      directories (QuickFiler, QuickFiler.Test, ToDoModel, UtilitiesCS, UtilitiesCS.Test, ToDoModel.Test,
      Tags, Tags.Test, VBFunctions.Test, TaskTree, TaskTree.Test, TaskVisualization,
      TaskVisualization.Test, TaskMaster, TaskMaster.Test) the deployed FSharp.Core.dll's own
      assembly-reference table names netstandard at version 2.0.0.0 and contains no reference at version
      2.1.0.0, read via System.Reflection.Metadata without loading the assembly; and the positive control
      over the package's lib/netstandard2.1 binary reports 2.1.0.0. Verified by the new Shape-B tests
      `DeployedFSharpCore_ReferencesNetstandard20` (one data row per directory) and
      `Detector_OnPackageNetstandard21Binary_Reports21` in FSharpCoreDeployedIdentityTests. Pre-fix
      observation required: run on the unfixed, freshly rebuilt tree and observe the QuickFiler,
      QuickFiler.Test and ToDoModel rows FAILING (any additional failing rows are recorded as observed
      build-order outcomes) while the positive control passes; capture the run under
      evidence/regression-testing.
- [x] AC3. The HintPath entries in UtilitiesCS/UtilitiesCS.csproj (line 70),
      UtilitiesCS.Test/UtilitiesCS.Test.csproj (line 599) and ToDoModel.Test/ToDoModel.Test.csproj
      (line 96) are byte-identical before and after the fix, and the three edited project files differ
      from origin/main (after a fetch) on exactly one line each, that line being the HintPath value.
      Verified by a git diff against origin/main restricted to those six files: empty for the three
      untouched files, one changed line for each of the three edited files.
- [x] AC4. The full C# toolchain passes in a single final pass with zero regressions after the fix, run in
      the order and with the commands quoted in Test Strategy: CSharpier check clean, analyzer rebuild
      with zero errors, nullable rebuild with zero errors, and vstest with coverage under the standard
      Workers=0 / ClassLevel runsettings with every test passing, including the pre-existing
      NetstandardBindChildDomainTests class and its negative control, and with no `[DoNotParallelize]`
      or serialisation added anywhere. Each gate's command, EXIT_CODE and output summary are recorded
      under evidence/qa-gates.
- [x] AC5. The `<remarks>` block on `ProbeApplicationBase` in NetstandardBindChildDomainTests.cs no
      longer states that the QuickFiler.Test output directory deploys the flavour referencing the
      unsatisfiable netstandard 2.1.0.0 identity; it describes the post-fix state (all deployed copies
      reference netstandard 2.0.0.0, and the directory is retained as the historically failing root with
      the discriminating power now carried by the display-name tests) with no change to any test method,
      assertion, attribute, constant or the assertion message at lines 206-207. Pre-fix observation
      required: grep of the unfixed file for "unsatisfiable" returns the stale sentence at lines 366-367;
      verified after the fix by reading the block and by a diff of that file showing changes only inside
      XML documentation comment lines.

## Risks & Mitigations
- Technical or operational risks:
  - Latent defect 1 (ToDoModel.Test packages.config lacks FSharp.Core and Deedle entries) is not addressed
    by this fix. Risk: the next FSharp.Core or Deedle bump leaves ToDoModel.Test with dangling HintPaths
    (CS0006) because the sync script skips packages not listed in the project's own packages.config.
    Mitigation: not in this fix; recommend the orchestrator or reviewer track it as a separate issue.
  - Latent defect 2 (the sync script's TFM fallback ranks netstandard2.1 ahead of netstandard2.0) is not
    addressed by this fix. Risk: a future bump that removes the netstandard2.0 folder from the package
    would let the script reintroduce a netstandard2.1 HintPath. Mitigation: Shape A asserts the flavour,
    not merely agreement, so any such reintroduction fails at test time; recommend a separate issue to
    reorder the list with accompanying Pester coverage.
  - Concurrent edits to the same three project files by issue #898. Mitigation: this fix changes exactly
    one line per file with no reordering; merge by union of items.
  - Shape B run after a partial or warm build passes vacuously. Mitigation: AC2 requires a /t:Rebuild whose
    log contains no skipped CoreCompile target, and the positive control proves the detector functions.
  - Shape A enumeration picking up nested agent worktrees under .claude and inflating the count.
    Mitigation: the directory exclusions in AC1 are mandatory, and the count assertion would expose the
    problem rather than hide it.
- Mitigations and rollbacks: revert the three HintPath lines to roll back; both new tests then fail
  again by design. No data, config or runtime state to unwind.

## Rollout & Follow-up
- Release/rollout steps: merge to main; no deployment step beyond the normal add-in build. CI already
  restores and rebuilds the whole solution before running tests, so Shape B runs there without workflow
  changes.
- Post-fix monitoring or clean-up tasks:
  - Follow-up 1 (separate issue, not opened by this spec): ToDoModel.Test packages.config omission of
    FSharp.Core and Deedle entries.
  - Follow-up 2 (separate issue, not opened by this spec): Sync-PackageReferences.ps1 TFM preference
    order inverted for .NET Framework, with Pester coverage for the change.
  - Adjacent but out of scope: issue #898 (Meziantou analyzer HintPath skew) touches the same project
    files for a different package and runs separately.
  - Record, not fix: the #879 test `AfterInstall_DeedleTypeInitializerSucceeds` no longer discriminates
    once QuickFiler.Test deploys the 2.0 flavour.
- Links: issue #895 (https://github.com/drmoisan/TaskMaster/issues/895); issue #879 (closed, merged;
  origin of the observation); issue #898 (adjacent); research artifact
  research/2026-09-16T23-30-fsharp-core-hintpath-research.md in this feature folder (Numeric Derivation
  Evidence for the counts of six and fifteen is complete there with two independent enumerations each and
  explicit member-set comparison, which is what licenses the numeric assertions in AC1 and AC2).
