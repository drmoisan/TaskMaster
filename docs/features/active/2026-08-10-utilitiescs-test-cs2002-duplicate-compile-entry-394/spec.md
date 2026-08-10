# 2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394 (Spec)

- **Issue:** #394
- **Parent (optional):** none (epic: `build-ci-coverage-gate-fidelity`, standalone child, wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T14-20
- **Status:** Draft
- **Version:** 0.2

## Context
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` lists the source file
  `OutlookObjects\Folder\PercentageFormatterTests.cs` twice in the same `<Compile>` `<ItemGroup>`
  (line 304 and line 356, verified by direct read at base commit `edf3d34c`). Every build of the
  test project — locally and on `windows-latest` CI — emits compiler warning CS2002 ("Source file
  ... specified multiple times") for that file.
- Observed environment(s): local developer workstations (Windows) and the `windows-latest` CI
  runner; reproduces under `msbuild TaskMaster.sln` and under a direct
  `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj` build.
- Customer impact and severity: no external/customer impact. This is an internal build-hygiene
  defect affecting developers and CI consumers of build output. Severity is **Low**: it is a
  warning-noise defect, not a build failure, and does not affect compiled output or test behavior.
  It affects every build of the affected project (100% of builds), so it is "always" reproducing,
  not intermittent.
- First observed date and version(s) impacted: the duplicate predates merge-base `003c5715` and is
  confirmed present at epic base `edf3d34c`. It was first logged as a promoted potential entry on
  2026-07-20 (`docs/features/potential/promoted/2026-07-20-utilitiescs-test-cs2002-duplicate-compile-entry.md`)
  and independently re-discovered and promoted again on 2026-08-08
  (`docs/features/potential/promoted/2026-08-08-utilitiescs-test-duplicate-percentageformattertests-compile-entry.md`,
  now GitHub issue #510 — see Rollout & Follow-up).

## Repro & Evidence
- Steps to reproduce:
  1. From the repository root, restore packages once per fresh worktree: `nuget restore` (the
     project uses `packages.config`; restore is solution-scoped even for a single-project build
     because relative `..\packages\...` HintPaths are shared).
  2. Force a genuine recompile of the test project:
     `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU`
     (note: `AnyCPU`, no space, for a direct single-project build; the solution-level command uses
     `"/p:Platform=Any CPU"`, with a space — the two spellings are not interchangeable and mixing
     them produces a `_CheckForInvalidConfigurationAndPlatform` error, not a CS2002-relevant
     result).
  3. Observe the `csc` warning output for `UtilitiesCS.Test.csproj`.
- Expected vs actual behavior: expected — the build completes with no CS2002 for
  `PercentageFormatterTests.cs`; each source file appears exactly once in the `<Compile>` item
  group. Actual —
  `CSC : warning CS2002: Source file '...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times [UtilitiesCS.Test.csproj]`
  is emitted on every build that actually runs `CoreCompile` for this project.
- Logs/screenshots/error snippets: prior dated evidence in this repository already captures this
  exact command family against this exact duplicate:
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-msbuild-analyzers.md`
  (2026-08-08T16-08, cold `/t:Build`, CS2002 present) and
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/rebuild-warnings-as-errors.2026-08-08T17-45.md`
  (2026-08-08T17-45, full `/t:Rebuild` with `/p:TreatWarningsAsErrors=true`, exit code 0, CS2002
  reported as a warning). This feature must still capture its own fresh fail-before/post-fix
  evidence per the Acceptance Criteria; the cited files are corroborating precedent, not a
  substitute.
- Frequency / determinism: **always**, on any build where `CoreCompile` actually executes for
  `UtilitiesCS.Test.csproj` (a cold build, or any build after a change to that project). It is
  **not** deterministic across a naive `/t:Build`-vs-`/t:Build` comparison: MSBuild's incremental
  up-to-date check can skip `CoreCompile` on a second `/t:Build` against an unchanged tree, which
  silently omits CS2002 from the output with no relationship to whether the duplicate item is still
  present. This is why the fail-before/post-fix evidence pair for this feature must use `/t:Rebuild`
  (see Root Cause Analysis and Acceptance Criteria).

## Scope & Non-Goals
- In scope:
  - Delete the second (line 356) `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />`
    item from `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, retaining the first (line 304) unchanged.
  - Capture fail-before and post-fix build evidence for CS2002 using a `/t:Rebuild` command.
  - Capture before/after `PercentageFormatterTests` test counts via vstest to confirm the file
    remains compiled and tested.
  - Sweep the remainder of the project file (`Compile`, `EmbeddedResource`, `None`, `Reference`,
    `ProjectReference`, `BootstrapperPackage`, `Analyzer`, `AdditionalFiles`, `packages.config`
    entries) for any other duplicate `Include` values and report findings.
  - Feature-folder documentation and evidence artifacts under this feature's own folder.
- Out of scope / non-goals:
  - The `System.Linq` `Reference` item's duplicated `<Private>True</Private>` child metadata
    element (lines 842-846). This is a duplicated child element inside a single, non-duplicated
    `Reference` item — not a duplicate `Include` value — and is functionally harmless (both values
    are identical, so there is no resolution ambiguity). Fixing it is explicitly out of scope: the
    issue's own scope constraint is "remove the duplicate item and nothing else," and fixing an
    unrelated anomaly in the same change would violate that constraint. If desired, it should be
    raised as a separate, low-priority potential entry.
  - Any change to `CLAUDE.md`, `.claude/rules/**`, or `scripts/**`. Those surfaces belong to
    sibling epic features (`csharp-toolchain-gate-fidelity-512`,
    `coverage-threshold-policy-reconciliation-494`); editing them here would create a fan-in
    conflict on the epic integration branch.
  - Reformatting, reordering, or line-ending normalization of
    `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. The file is confirmed CRLF on all 972 lines; the
    fix is a single-line deletion performed as a targeted string edit, not a full-file rewrite.
  - Re-evaluating or re-promoting CS2002's severity based on sibling feature
    `csharp-toolchain-gate-fidelity-512`. That feature changes only how the toolchain gate is
    *documented*, not any diagnostic-severity configuration; see Root Cause Analysis for the
    evidence that CS2002 is not, and will not become, promoted to an error by that work.
  - A repository-level automated check that fails when any `.csproj` lists the same `Include`
    value twice (suggested informally in issue #510's notes). This would require editing files
    under `scripts/`, which this feature is prohibited from touching; it is recorded as a follow-up
    idea in Rollout & Follow-up, not as in-scope work.
- Explicitly excluded systems, integrations, or datasets: not applicable — this is a single-file,
  build-configuration-only change with no runtime data path.

## Root Cause Analysis
- Confirmed root cause: `UtilitiesCS.Test/UtilitiesCS.Test.csproj` contains two textually identical,
  bare, un-conditioned `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />`
  items at line 304 and line 356, both inside the single `<Compile>` `<ItemGroup>` that spans lines
  72-529. This corrects the originating potential entry's line numbers (288/338, as of an earlier
  commit) and its "two separate `<ItemGroup>` sections" hypothesis — both occurrences sit in the
  same `<ItemGroup>`. The likely origin is a merge artifact: two independently appended blocks of
  `OutlookObjects\Folder\*Tests.cs` entries each carrying the same filename.
- Signals/evidence supporting it:
  - Direct file reads at base commit `edf3d34c` confirmed both occurrences, their exact line
    numbers, and their identical, attribute-free text.
  - A full manual sweep of all 452 `Compile` `Include` values, plus every other item type in the
    file (`EmbeddedResource`, `None`, `Reference` (~114), `ProjectReference`, `BootstrapperPackage`,
    `Analyzer`, `AdditionalFiles`) and every `packages.config` `<package>` entry (~99), found
    exactly one duplicate `Include` value in the entire file: this one. See the Duplicate Sweep
    Result table below.
  - `PercentageFormatterTests.cs` contains exactly 7 `[TestMethod]` members and no
    `[DataTestMethod]`/`[DataRow]`, so the duplicate `<Compile>` item does not change the number of
    discoverable tests; it only causes the file to be passed to `csc.exe` twice on the command
    line.
  - C#/`csc.exe` resolves types and members independently of source-file list order; removing
    either of the two identical items produces a bit-identical compiled assembly. This is a general
    property of the compiler, not something requiring further verification for this fix.
  - Empirical evidence (`docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-msbuild-analyzers.md`
    and `.../evidence/qa-gates/phase2-final-msbuild-analyzers.md`) demonstrates that a repeat
    `/t:Build` against an already-built, unchanged `UtilitiesCS.Test` project skips `CoreCompile`
    and does not re-emit CS2002 — i.e., `/t:Build` is a vacuous fail-before/post-fix comparison for
    this defect. `/t:Rebuild` forces `CoreCompile` unconditionally and reliably surfaces (or
    confirms the absence of) CS2002 regardless of prior build state. CI's own `TreatWarningsAsErrors`
    step already uses `/t:Rebuild` for exactly this reason, per its own inline comment
    (`.github/workflows/ci.yml` lines 103-116).
  - **Correction to the issue's risk framing.** `issue.md`'s Impact/Severity section states the
    duplicate "would break the build if warning-promotion rules changed," citing sibling feature
    `csharp-toolchain-gate-fidelity-512` as a live consideration. This claim is not supported by the
    evidence gathered for this feature. A dated, direct empirical run of CI's exact command
    (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`,
    captured 2026-08-08 in
    `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/rebuild-warnings-as-errors.2026-08-08T17-45.md`)
    completed with exit code 0 while CS2002 was present and explicitly reported as a warning, not
    an error. A repository-wide grep for `NoWarn`, `WarningsNotAsErrors`, `TreatWarningsAsErrors`,
    and `2002` across every `*.csproj` and `.editorconfig`, and a check for `Directory.Build.props`
    at the repository root, found no suppression mechanism responsible for this — CS2002 is simply
    not the kind of diagnostic that `/warnaserror` promotes (it is emitted by the compiler's
    command-line/source-file-list processing ahead of the `Compilation` object's diagnostic
    filtering, so it is structurally outside the `TreatWarningsAsErrors` path). `512`'s scope is
    limited to correcting which command and target the toolchain documentation specifies; it does
    not add, remove, or change any `NoWarn`/`WarningsNotAsErrors`/`TreatWarningsAsErrors` value.
    Therefore `512` landing would not change this outcome. The defensible justification for this fix
    is warning-signal hygiene — CS2002 noise makes it harder to notice genuine new warnings in build
    output — not a claim that the duplicate could someday fail the build. Severity remains **Low**.
- Affected components/modules (paths, services, pipelines):
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (the project file itself; no `.cs` source, no runtime
  code, and no CI workflow file is affected).

### Duplicate Sweep Result

| Item type | Count | Duplicates found |
|---|---|---|
| `Compile` | 452 | 1 — `OutlookObjects\Folder\PercentageFormatterTests.cs` (lines 304, 356) — the defect this feature fixes |
| `EmbeddedResource` | 1 | none |
| `None` | 7 (across three `<ItemGroup>`s) | none |
| `Reference` | ~114 | none (every `Include` assembly-name token is distinct) |
| `ProjectReference` | 2 | none |
| `BootstrapperPackage` | 2 | none |
| `Analyzer` | 9 | none |
| `AdditionalFiles` | 1 | none |
| `PackageReference` | 0 | not applicable — legacy `packages.config`-style project, `PackageReference` is not used |
| `packages.config` `<package>` | ~99 | none |

One non-duplicate-`Include` anomaly was found and is recorded as out of scope above: the
`System.Linq` `Reference` item (lines 842-846) contains a duplicated `<Private>True</Private>`
child element within a single, non-repeated `Reference` item.

## Proposed Fix

### Design summary (what changes where):
Delete the single line at line 356 of `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
(`<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />`), which is the second of
the two identical items. Retain line 304 unchanged. No other line in the file changes.

### Boundaries and invariants to preserve:
- Exactly one `<Compile Include>` item for `PercentageFormatterTests.cs` must remain after the
  change.
- File encoding (CRLF line endings, no BOM detected within the limits of available tooling) must
  be preserved. The change must be a targeted single-line deletion, not a full-file read/rewrite,
  so that any encoding property not conclusively determined by this feature's research is not put
  at risk.
- Surrounding lines (line 303 `FolderSuggestionTreeStateTests.cs` and line 305
  `FolderProbabilityAdapterTests.cs` around the retained item; lines 349-357
  `FolderConverterTests.cs` through `FolderNodeViewModelTests.cs` around the deleted item) must be
  otherwise undisturbed.
- Compiled output must be unchanged: removing either duplicate item cannot alter the compiled
  assembly, because `csc.exe` resolves types/members independently of source-file list order.

### Dependencies or blocked work:
None. This feature is standalone within the epic (wave 0, `depends_on: []`), shares no file surface
with any sibling feature, and has no upstream or downstream dependency in
`docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — delete line 356 only.

#### Functions/classes/CLI commands impacted:
Not applicable — this is a build-configuration item-list edit. No C# class, method, or CLI command
changes. `PercentageFormatterTests.cs` itself is not modified.

#### Data flow and validation changes:
Not applicable — no data flow exists for this change; it affects only which source files MSBuild
passes to the C# compiler.

#### Error handling and logging updates:
Not applicable — no runtime error handling or logging is affected. The observable effect is the
absence of a compiler warning line in build output.

#### Rollback/feature-flag considerations (if applicable):
Not applicable — a single-line deletion in a project file requires no feature flag. Rollback is a
single-commit revert (`git revert`) restoring the duplicate line.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
Not applicable — no public interface, API, or data format changes. The only "output" affected is
MSBuild/`csc.exe` diagnostic text (the CS2002 line disappears from build output).

#### Required configuration keys and defaults:
Not applicable — no configuration keys are introduced, removed, or changed.

#### Backward-compatibility expectations:
No breaking change. The compiled `UtilitiesCS.Test.dll` is expected to be behaviorally identical
before and after (same 7 discoverable `PercentageFormatterTests` tests, same production code
compiled). No consumer of `UtilitiesCS.Test.csproj` depends on the file being listed twice.

#### Performance constraints (latency/throughput/memory):
Not applicable — no measurable performance effect. Removing one duplicate compile-unit reference
from 452 total items has no observable effect on build time within normal variance.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - A fresh worktree has no `packages/` directory populated and no `bin/`/`obj/` output for
    `UtilitiesCS.Test`, so `nuget restore` must run before any build attempt in that worktree.
  - `msbuild.exe` and `vstest.console.exe` are not on `PATH` in the build/execution environment;
    they must be located via `vswhere.exe` or the confirmed fallback paths recorded in the research
    artifact (`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
    and `...\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`).
- Constraints (budget, performance, compatibility):
  - Scope is limited to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`; no other production or
    governance file may be touched (see Scope & Non-Goals).
  - A direct single-project MSBuild invocation must use `/p:Platform=AnyCPU` (no space); a
    solution-level invocation must use `"/p:Platform=Any CPU"` (with a space). The two spellings
    are not interchangeable, and using the wrong one for the target produces a
    `_CheckForInvalidConfigurationAndPlatform` build-configuration error rather than useful CS2002
    evidence.
  - The fail-before and post-fix build evidence must use `/t:Rebuild`, not `/t:Build`. A `/t:Build`
    capture is not acceptable evidence for this feature's acceptance criteria, because it can
    silently skip `CoreCompile` and produce a false "no CS2002" reading unrelated to the fix.
- External dependencies (services, libraries, releases): none beyond the repository's existing
  MSBuild/NuGet/vstest toolchain. No new package or library is introduced.

## Data / API / Config Impact
- User-facing or API changes: none.
- Data or migration considerations: not applicable — no data or schema is affected.
- Logging/telemetry updates (if any): not applicable — the only observable change is the absence of
  a build-time compiler warning line; no application logging or telemetry is touched.
- Compatibility notes (CLI flags, config schemas, versioning): not applicable — no CLI flag, config
  schema, or versioned contract changes. The `.csproj` schema itself is unchanged; only one item
  entry is removed.

## Test Strategy
- Regression tests to add or update: **none, by design.** This defect has no runtime behavior to
  regression-test with a unit test — the duplicate `<Compile>` item is a build-configuration
  artifact, not a code path. Inventing a C# unit test to "cover" a project-file line-count would
  test nothing meaningful and would not fail before the fix or pass after it in any way tied to the
  actual defect. The regression evidence for this fix is a **build-output assertion**: a `/t:Rebuild`
  capture showing CS2002 present before the change and absent after, for the same command against
  the same project. This is the correct and sufficient regression check for a build-configuration
  defect of this kind.
- Unit tests (pytest) for the fixed behavior and boundaries: not applicable — this repository's
  test stack for this project is MSTest, not pytest, and per the point above no new unit test is
  warranted for this defect.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): not
  applicable in the traditional sense (there is no runtime input to this change). The relevant
  edge case already checked is the sweep for other duplicate `Include` values across every item
  type in the project file (Compile, EmbeddedResource, None, Reference, ProjectReference,
  BootstrapperPackage, Analyzer, AdditionalFiles, packages.config) — see the Duplicate Sweep Result
  table in Root Cause Analysis. No further duplicates were found; none require fixing beyond the
  one item in scope.
- Error handling and logging verification: not applicable — no error handling or logging code is
  changed.
- Coverage impact and targets for changed lines/modules: not applicable. The change is a one-line
  deletion in a non-executable `.csproj` XML file; it has no line/branch coverage measurement
  surface and does not appear in any Cobertura coverage report. `PercentageFormatterTests.cs`
  itself is not modified, so its own test coverage is unaffected.
- Toolchain commands to run (format → lint → type-check → test): CSharpier formatting and .NET
  analyzer/nullable checks are not meaningful for a `.csproj` XML edit (CSharpier formats `*.cs`
  files only). The applicable verification commands for this change are:
  1. `nuget restore` (once, if `packages\` is not already populated in the working tree).
  2. Fail-before capture:
     `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU`
     — run before the edit; expect CS2002 for `PercentageFormatterTests.cs` in the output.
  3. Apply the one-line deletion.
  4. Post-fix capture: the same command again; expect no CS2002 for that file.
  5. Test-count verification, run once against the pre-fix rebuilt assembly and once against the
     post-fix rebuilt assembly:
     `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~PercentageFormatterTests"`
     — expect "Total tests: 7" both times, all passing.
  6. As a compatibility check (not a substitute for steps 2-5), CI's own solution-level command may
     optionally be run: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
     (expect exit code 0 both before and after, with CS2002 present before and absent after).
- Manual validation steps (if required): review the diff to `UtilitiesCS.Test.csproj` to confirm it
  is exactly one deleted line with no whitespace, ordering, or line-ending change elsewhere in the
  file.

## Acceptance Criteria
- [ ] Exactly one `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />` item
      remains in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` after the change (the item previously at
      line 356 is removed; the item previously at line 304 is retained unchanged).
- [ ] Fail-before evidence captures the CS2002 warning for `PercentageFormatterTests.cs`, using a
      `/t:Rebuild` command (`msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU`
      or the equivalent solution-level `/t:Rebuild` command). An artifact captured from a `/t:Build`
      invocation does not satisfy this criterion, because `/t:Build` can skip `CoreCompile` on an
      already-built tree and silently omit CS2002 for reasons unrelated to the fix. Evidence is
      recorded under `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/evidence/baseline/`.
- [ ] Post-change build of the same project, using the same `/t:Rebuild` command, emits no CS2002
      for `PercentageFormatterTests.cs`. Evidence is recorded under
      `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/evidence/qa-gates/`.
- [ ] `PercentageFormatterTests` test count is unchanged at 7, verified via vstest
      (`/TestCaseFilter:"FullyQualifiedName~PercentageFormatterTests"`), with the before and after
      counts recorded numerically in
      `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/evidence/regression-testing/`.
- [ ] The duplicate sweep across every item type in `UtilitiesCS.Test.csproj` (Compile,
      EmbeddedResource, None, Reference, ProjectReference, BootstrapperPackage, Analyzer,
      AdditionalFiles, packages.config) is recorded, with findings reported. Beyond the one
      `Compile` duplicate this feature fixes, no other duplicate `Include` value exists; the
      unrelated `System.Linq` duplicated-`<Private>` anomaly is reported but explicitly not fixed
      (see Scope & Non-Goals).
- [ ] The diff touches only `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (a single-line deletion) plus
      this feature folder's own documentation and evidence files, with no reformatting, reordering,
      or line-ending change anywhere in the `.csproj`.
- [ ] Full toolchain pass completed for the applicable stages: CSharpier formatting and
      analyzer/nullable-flow checks are not applicable to a `.csproj`-only change (documented above
      as "Not applicable"); the applicable stage is the build/test verification in Test Strategy
      steps 2-5, all of which pass.
- [ ] Docs/config references updated to match the new behavior: this `spec.md` and the mirrored
      acceptance-criteria list in `issue.md` are consistent with each other and with the delivered
      change.

## Risks & Mitigations
- Technical or operational risks:
  - **Wrong build command masks the defect.** Using `/t:Build` instead of `/t:Rebuild` for the
    fail-before or post-fix capture can silently omit CS2002 from output for reasons unrelated to
    the fix, producing false confidence. Mitigation: this spec mandates `/t:Rebuild` for both
    captures (see Acceptance Criteria).
  - **Platform-spelling error breaks the build capture.** A direct single-project build requires
    `/p:Platform=AnyCPU` (no space); using the solution-level `"/p:Platform=Any CPU"` spelling
    against the single `.csproj` fails `_CheckForInvalidConfigurationAndPlatform` and produces an
    unrelated tooling error rather than CS2002 evidence. Mitigation: the exact command and spelling
    are specified in Repro & Evidence and Test Strategy; the executor should not substitute the
    solution-level spelling for a project-level invocation.
  - **Cold worktree lacks restored packages.** A fresh worktree has no `packages/` directory; the
    first build attempt without `nuget restore` fails with NuGet-missing-package errors unrelated
    to this defect. Mitigation: `nuget restore` is listed as a hard prerequisite step.
  - **Overstating severity.** The issue's original framing ("would break the build if
    warning-promotion rules changed") is not supported by direct evidence (see Root Cause Analysis)
    and could lead to over-scoping this as an urgent fix. Mitigation: this spec documents the
    verified evidence and keeps the stated severity at Low, with warning-signal hygiene as the
    actual justification.
- Mitigations and rollbacks: rollback is a single-commit `git revert` restoring the deleted line.
  No feature flag or staged rollout is applicable to a single-line project-file change.

## Rollout & Follow-up
- Release/rollout steps: standard PR merge to the epic integration branch
  (`epic/build-ci-coverage-gate-fidelity-integration`) via branch
  `bug/utilitiescs-test-cs2002-duplicate-compile-entry-394`. No deployment, migration, or
  feature-flag rollout is required.
- Post-fix monitoring or clean-up tasks:
  - **Close issue #510 alongside #394.** `docs/features/potential/promoted/2026-08-08-utilitiescs-test-duplicate-percentageformattertests-compile-entry.md`
    documents the identical defect (same lines 304/356, same root cause, confirmed to predate
    merge-base `003c5715`) and was promoted to GitHub issue #510
    (`https://github.com/drmoisan/TaskMaster/issues/510`). This feature's fix resolves both issues;
    the PR that lands this change should reference and close both #394 and #510 to avoid a
    duplicate future fix attempt.
  - **Follow-up idea, out of scope here:** a repository-level automated check (referenced informally
    in issue #510's notes) that fails when any `.csproj` lists the same `Include` value twice would
    prevent recurrence. Implementing it would require editing files under `scripts/`, which this
    feature is prohibited from touching; it should be filed as a separate potential entry if the
    team wants to pursue it.
  - **Unrelated anomaly for future consideration:** the `System.Linq` `Reference` item's duplicated
    `<Private>True</Private>` child element (out of scope here; see Scope & Non-Goals) may be worth
    a separate, low-priority potential entry.
- Links: issue #394 (`https://github.com/drmoisan/TaskMaster/issues/394`), issue #510
  (`https://github.com/drmoisan/TaskMaster/issues/510`), epic
  `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`, research artifact
  `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/research/2026-08-10T14-15-cs2002-duplicate-compile-entry.md`.
