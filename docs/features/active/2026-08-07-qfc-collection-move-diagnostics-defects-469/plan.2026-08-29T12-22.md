# qfc-collection-move-diagnostics-defects (Plan)

- **Issue:** #469
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T08-21
- **Status:** Ready for Codex preflight
- **Version:** 0.6
- **Work Mode:** full-bug (marker source: `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/issue.md`, line 12, `- Work Mode: full-bug`)
- **Requirements and acceptance-criteria source:** `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/spec.md`, section `## Acceptance Criteria` (13 criteria, AC1 through AC13)
- **Verified findings source:** `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/research/2026-08-29T12-31-qfc-collection-move-diagnostics-defects-469.md`

## What this change is, and what it is not

This is a comment-and-documentation-accuracy change. It is not a defect fix.

Three of issue #469's four defects are already remediated and merged on `origin/main` with regression
tests. The fourth defect's only remaining action — removing the `stackMovedItems` parameter from
`MoveEmailsAsync` — is tracked as separate open issue #629 and is out of scope here. The residual
work attributable to #469 is: two stale comments left behind by the defect-2 fix, a defect-numbering
inversion between the published issue text and the shipped source comments, and one resolved
cross-feature note in an unrelated feature's spec.

There is no behavior change, no new test method, no new file, no csproj edit, and no production
logic edit anywhere in this plan.

## Fail-before exception (stated, not skipped)

The repository Bugfix Workflow requires a failing regression test before a fix. That requirement is
inapplicable to this change: comment text and XML documentation have no observable runtime behavior,
so no deterministic red state exists and no test can be authored that fails before the change and
passes after it. Phase 1 records a fail-before exception dossier under
`docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/regression-testing/`
in place of a failing run. No task in this plan is tagged `[expect-fail]`, and none should be.

## Evidence location rule (non-overridable)

All evidence artifacts produced by this plan are written under
`docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/<kind>/` where
`<kind>` is one of `baseline`, `regression-testing`, `qa-gates`, or `other`. No artifact is written
under `artifacts/`. Every command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and
`Output Summary:`.

Every file path recorded inside an evidence artifact is written repository-relative. Where a command
prints an absolute path — MSBuild diagnostics in the non-zero branches of P0-T11, P0-T12, P6-T3 and
P6-T4, and any absolute entry in the unformatted-file enumerations of P0-T10 and P6-T2 — the
executor removes the repository-root prefix from the recorded text before saving the artifact, so no
account name, machine name or drive letter is included in the final progress commit. The rewrite applies to recorded
paths only; counts, exit codes and quoted summary lines are recorded verbatim.

Throughout this plan `FEATURE` is shorthand for
`docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469`.

## Toolchain (exact order; restart from step 1 on any failure or file change)

1. `dotnet tool run csharpier format .` — verify with `dotnet tool run csharpier check .`. Always
   through `dotnet tool run` so the manifest-pinned 1.2.6 is used; never a global install. Run
   `dotnet tool restore` once first (the manifest is `dotnet-tools.json` at the repository root).
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe` over the built test assemblies with code coverage enabled — realised in this
   plan by `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`, which resolves
   `vstest.console.exe` through `vswhere` and collects Cobertura through `dotnet-coverage`.

`/t:Rebuild` is mandatory. A warm `/t:Build` skips `CoreCompile` on every project because MSBuild's
up-to-date check does not invalidate on a command-line `/p:` change, so the analyzer and nullable
gates would exit 0 without running. `/p:Nullable=enable` must NOT be added: no project in this
repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so forcing it
conscripts files that never opted in and produces hundreds of errors. CI omits it deliberately.

## Verified facts this plan gates on

Re-derived against the current tree in this authoring pass. See the self-review enumeration at the
end of this file.

**Stale-comment sites — exactly 2 in compiled source.**

| Site | Current content |
|---|---|
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs:171` | single-line token `one element longer` |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:398` | single-line token `one element longer` |

The alternative phrasing `trailing element is null` wraps across two comment lines in both files and
matches zero single source lines. No gate in this plan asserts that phrase.

**Defect-numbering inversion — exactly 8 sites.** Every site carries the defect number and its
distinguishing text on one physical line, so every gate below is a combined single-line token.

| Site | Line | Currently reads |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2371 | `Issue #469 defect 1: exactly one diagnostics line per cached move group. The array` |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2381 | `Issue #469 defect 2: the null test must dominate every dereference of qf. It` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 275 | `/// Issue #469 defect 1. Regression test proving that the diagnostics array carries exactly` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 306 | `because: "issue #469 defect 1 requires one diagnostics line per cached move "` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 313 | `/// Issue #469 defect 1. Regression test proving the off-by-one is a length defect at every` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 340 | `because: "issue #469 defect 1 requires exactly one diagnostics line per cached "` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 352 | `/// Issue #469 defect 2. Regression test proving that the item-controller null guard runs` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 387 | `because: "issue #469 defect 2 requires the null guard to run before the first "` |

Sites at `:306`, `:340` and `:387` are FluentAssertions `because:` string literals inside executable
statements, not comments. Spec AC7 permits `because:` string edits; this plan does not assert
"comment lines only" anywhere.

The `Issue #469 defect 3` sites (`QfcCollectionController.cs:71`, `:732`, `:2344`;
`QfcCollectionControllerTests.cs:66`; `QfcCollectionControllerDefects468MoveTests.cs:17`, `:29`,
`:57`, `:64`) and the `Issue #469 defect 4` site
(`QfcCollectionControllerDefects468MoveTests.cs:463`) already agree with `issue.md` and are NOT
edited by this plan.

**Why a whole-file token-presence gate would be vacuous.** Both `Issue #469 defect 1` and
`Issue #469 defect 2` already exist in both files today, so a gate asserting that either file merely
contains one of those strings passes before any work is done. Every renumbering gate in Phase 3 is
therefore a combined single-line token with an exact expected match count, and every gate is scoped
to one named file.

**Why no repository-wide zero-hit gate is authored.** The token `one element longer` also occurs in
`docs/features/active/quickfiler-home-controller-metrics-442/spec.md:869` and in this feature's own
`issue.md`, `spec.md` and research document. The string `Issue #469 defect` likewise occurs across
`docs/features/**`. A repository-wide zero-hit gate on either is unsatisfiable by construction.

**Invariants.**

- The `.Where(line => !string.IsNullOrWhiteSpace(line))` filter at
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs:174` must not be deleted. Deleting it fails
  `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` in
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:403`.
- The token `IQfcCollectionController` currently has zero occurrences in
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, so asserting at least one occurrence after
  the rewrite is a false-before / true-after gate.
- `QuickFiler/Controllers/QfcCollectionController.cs` is 2446 lines, already over the 500-line cap,
  under an explicit no-split constraint delegated to open issue #623. This change must be
  net-neutral or net-negative on that file. No split is planned.
- `QuickFiler/Controllers/QfcCollectionController.cs:21` carries `[ExcludeFromCodeCoverage]`. No
  acceptance condition in this plan claims a coverage increase attributable to that class; such a
  condition could not fail. Coverage is captured numerically anyway and the non-attribution is
  stated explicitly in Phase 6.
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` is 497 lines against
  the 500 cap (the spec's figure of 498 is off by one; 497 is the re-derived value). No test method
  is added to it.
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is exactly 499 lines and receives
  nothing.
- `QuickFiler.Test` is a legacy non-SDK project that enumerates sources with explicit
  `Compile Include` items. No new file is created, so no csproj entry is needed and no csproj is
  edited.

**Out of scope, gated in Phase 5.** Removing the `stackMovedItems` parameter (issue #629); any edit
to `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`; re-fixing defects 1 through 3;
`QuickFiler/Legacy/QfcGroupOperationsLegacy.cs` (not in the csproj, not compiled); deleting the
whitespace filter; the `TaskMaster/AppGlobals/AppAutoFileObjects.cs` `Initialized<T>`
non-memoization finding (follow-up candidate only).

**No closing keyword.** No commit produced by this plan may carry a GitHub closing keyword for #469.
Disposition of issue #469 is the maintainer's decision. Phase 7 gates this over
`origin/main..HEAD`.

## Exact replacement text authored by this plan

The executor applies these literals verbatim. They are quoted here so that every literal an
acceptance condition later searches for has a stated origin.

**R1 — replaces `QuickFiler/Controllers/QfcHomeController.Metrics.cs` lines 171 through 173 (3 lines
replaced by 3 lines, 12-space indent):**

```text
            // The call is made through IQfcCollectionController.GetMoveDiagnostics, which carries
            // no XML documentation and therefore no non-null element guarantee, so this filter
            // defends the interface contract rather than a known producer defect.
```

**R2 — replaces `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` lines 398 through 400
(3 lines replaced by 3 lines, 8-space indent; the `/// <summary>` at 397 and `/// </summary>` at 401
are untouched):**

```text
        /// The call is made through the IQfcCollectionController.GetMoveDiagnostics contract,
        /// which carries no XML documentation and no non-null element guarantee. Null and
        /// whitespace-only entries must therefore be dropped before the write.
```

**R3 — the eight renumbering edits.** Each is a single-character substitution on one physical line
that changes only the defect digit. Line length is unchanged, so neither file's line count changes.

| File | Line | Becomes |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2371 | `Issue #469 defect 2: exactly one diagnostics line per cached move group. The array` |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2381 | `Issue #469 defect 1: the null test must dominate every dereference of qf. It` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 275 | `/// Issue #469 defect 2. Regression test proving that the diagnostics array carries exactly` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 306 | `because: "issue #469 defect 2 requires one diagnostics line per cached move "` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 313 | `/// Issue #469 defect 2. Regression test proving the off-by-one is a length defect at every` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 340 | `because: "issue #469 defect 2 requires exactly one diagnostics line per cached "` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 352 | `/// Issue #469 defect 1. Regression test proving that the item-controller null guard runs` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | 387 | `because: "issue #469 defect 1 requires the null guard to run before the first "` |

**R4 — replaces `docs/features/active/quickfiler-home-controller-metrics-442/spec.md` line 869:**

```text
    ### CFN-2 — RESOLVED — `GetMoveDiagnostics` returned an array one element longer than it filled (feature 468)
```

The four leading spaces above are presentation only, to keep a hash-prefixed line out of column 0
inside this file. The literal written into the target file starts at column 0 with `###`.

**R5 — inserted into `docs/features/active/quickfiler-home-controller-metrics-442/spec.md`
immediately after the blank line 870, ahead of the existing `- **Location:**` bullet:**

```text
- **CFN-2 RESOLVED (2026-08-29).** Feature 468 landed the recommended fix:
  `QuickFiler/Controllers/QfcCollectionController.cs` now allocates
  `new string[_itemGroupsToMove.Count]` and assigns every index on both branches of the loop, so
  the trailing-null hazard described in the bullets below no longer exists. It is pinned by
  `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine` and
  `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` in
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`. The bullets below are
  retained as the historical record. The `WriteMetricsAsync` null-and-whitespace filter is retained
  for a different and still-valid reason: `IQfcCollectionController.GetMoveDiagnostics` carries no
  non-null element guarantee.
```

The literal `CFN-2 RESOLVED` has zero occurrences in that file today and the literal `RESOLVED` has
zero occurrences in that file today, so the Phase 4 gate is false-before / true-after.

## Gate vocabulary used below

- **Discriminating gate** — false at branch head, true only after the task that satisfies it. These
  carry the verification weight.
- **Invariant guard** — already true at branch head and required to stay true. These detect scope
  creep and accidental deletion. They are labelled as guards so no reviewer mistakes them for
  discriminating gates.

## Execution checkpoint boundaries

This plan contains 69 atomic tasks. After completing and checking off task 35, `[P4-T2]`, the atomic
executor must stop and return `PROGRESS_COMMIT_REQUIRED: P0-T1..P4-T2`. The orchestrator must stage
only the completed interval's in-scope files, collect canonical commit context through the repository
automation adapter, delegate the commit message to the routed `commit-steward` profile, create the
commit, and record the interval and resulting SHA in `artifacts/orchestration/orchestrator-state.json`
before resuming at `[P4-T3]`. No executor may mutate the worktree while that commit is being prepared.

After `[P7-T17]`, the executor must return `PROGRESS_COMMIT_REQUIRED: P4-T3..P7-T17`. The
orchestrator applies the same commit-context and commit-steward sequence to the final partial interval,
records its SHA, and only then proceeds to review and pull-request steps. These boundaries do not add
or renumber atomic tasks.

---

## Pre-execution base-reconciliation gate

Before `[P0-T1]`, the orchestrator must run `git fetch origin main` and then `git merge --no-edit origin/main` while the worktree has no executor-created tracked changes. Use merge, never rebase. It must record the result in `FEATURE/evidence/baseline/p0-t6-base-reconciliation.2026-08-29T12-22.md` with `Command:`, `EXIT_CODE:`, and `Output Summary:`. The evidence must show that `git rev-parse origin/main` equals `git merge-base origin/main HEAD`, `git ls-files --unmerged` returns zero paths, and `(Get-Content).Count` is 2446 for `QuickFiler/Controllers/QfcCollectionController.cs`, 215 for `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, 497 for `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`, 453 for `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, and 499 for `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`. If the merge conflicts or any required line count differs, return the plan to preflight revision before atomic execution begins. Do not begin `[P0-T1]` until this gate passes.

### Phase 0 — Baseline Capture, Toolchain Bootstrap, and Citation Re-verification

- [x] [P0-T1] Read `AGENTS.md` in full and record the standing-instructions entry in
  `FEATURE/evidence/baseline/phase0-instructions-read.2026-08-29T12-22.md`. Acceptance: the artifact
  exists and its `Policy Order:` field lists `AGENTS.md — standing instructions` first.

- [x] [P0-T2] Re-read the `Agent Code Change Policy` section of `AGENTS.md` and append
  `AGENTS.md — Agent Code Change Policy` as the second entry of the `Policy Order:` field in
  `FEATURE/evidence/baseline/phase0-instructions-read.2026-08-29T12-22.md`. Acceptance: that entry is
  in position 2.

- [x] [P0-T3] Re-read the `General Unit Test Policy` section of `AGENTS.md` and append
  `AGENTS.md — General Unit Test Policy` as the third entry of the `Policy Order:` field in the same
  artifact. Acceptance: that entry is in position 3.

- [x] [P0-T4] Read `.agents/skills/csharp/SKILL.md` in full and append it as the fourth entry of the
  `Policy Order:` field in the same artifact. Acceptance: the artifact lists
  `.agents/skills/csharp/SKILL.md` in position 4.

- [x] [P0-T5] Re-read the `Tone Policy` section of `AGENTS.md`, append
  `AGENTS.md — Tone Policy` as the fifth entry, and finalise
  `FEATURE/evidence/baseline/phase0-instructions-read.2026-08-29T12-22.md` with the fields
  `Timestamp:`, `Policy Order:`, `Distinct Files Read:`, and `Tone Policy Acknowledgement:`.
  Acceptance: the artifact contains all four fields; `Policy Order:` contains exactly five ordered
  entries; `Distinct Files Read:` contains exactly `AGENTS.md` and `.agents/skills/csharp/SKILL.md`;
  and the acknowledgement confirms that professional, factual, neutral wording is required.

- [x] [P0-T6] Verify the pre-execution base-reconciliation evidence. Do not run `git fetch` or `git merge` in this task. Confirm `FEATURE/evidence/baseline/p0-t6-base-reconciliation.2026-08-29T12-22.md` records the required commands, `EXIT_CODE:`, matching `origin/main` and merge-base revisions, zero unmerged paths, and the five required line counts. Acceptance: every recorded condition passes. If the evidence is absent or any condition fails, return the plan to preflight revision before proceeding.

- [x] [P0-T7] Probe the .NET SDK and bootstrap it if the probe fails. Run `dotnet --version`. If it
  exits non-zero (the repository-local `.dotnet-sdk` path named by `global.json` is gitignored and
  is absent from a fresh worktree), run `scripts/vscode/Install-RepoDotNetSdk.ps1` from the
  repository root and re-run `dotnet --version`. Record both invocations in
  `FEATURE/evidence/baseline/p0-t7-dotnet-sdk-probe.2026-08-29T12-22.md`. Acceptance: the artifact
  records a final `dotnet --version` invocation with `EXIT_CODE: 0` and an `Output Summary:` naming
  the resolved SDK version, which must be `8.0.205` or a later 8.0.x feature band per
  `global.json`.

```powershell
dotnet --version
$LASTEXITCODE
```

- [x] [P0-T8] Restore the CSharpier tool manifest. Run `dotnet tool restore` from the repository
  root. Record it in `FEATURE/evidence/baseline/p0-t8-dotnet-tool-restore.2026-08-29T12-22.md`.
  Acceptance: `EXIT_CODE: 0`, and `dotnet tool run csharpier --version` prints `1.2.6` (the version
  pinned by `dotnet-tools.json` at the repository root). Record the printed version verbatim in
  `Output Summary:`.

```powershell
dotnet tool restore
$LASTEXITCODE
dotnet tool run csharpier --version
```

- [x] [P0-T9] Restore NuGet packages for the solution. Run `scripts/vscode/Invoke-Restore.ps1` from
  the repository root. This uses `vswhere`-resolved MSBuild with `/t:Restore` and
  `/p:RestorePackagesConfig=true`, which is required because every project in this solution is a
  legacy `packages.config` project. Record it in
  `FEATURE/evidence/baseline/p0-t9-nuget-restore.2026-08-29T12-22.md`. Acceptance: `EXIT_CODE: 0`
  and the directory `packages` exists at the repository root after the run. Do not use
  `Invoke-VSBuild.ps1` for this: it runs a package-reference synchronisation pass over every csproj
  and can rewrite `HintPath` elements, which would breach the no-csproj-edit constraint.

```powershell
pwsh -NoProfile -File 'scripts\vscode\Invoke-Restore.ps1'
$LASTEXITCODE
Test-Path 'packages'
```

- [x] [P0-T10] Capture the baseline CSharpier state. Run `dotnet tool run csharpier check .` and
  record it in `FEATURE/evidence/baseline/p0-t10-csharpier-check.2026-08-29T12-22.md`.
  Acceptance: the artifact records `Command:`, `EXIT_CODE:` and an `Output Summary:` that states the
  exit code and enumerates every file path the command reported as unformatted, verbatim, together
  with the count of those paths. The branch decision referenced by P6-T1 is made on the exit code,
  not on any output literal: `EXIT_CODE: 0` means the repository is CSharpier-clean and the Phase 6
  mutating `format .` pass is safe repository-wide; a non-zero exit code means pre-existing drift
  exists and P6-T1 scopes its mutating invocation to this plan's own four C# paths so that drift in
  unrelated files is not swept into this change's diff. The enumerated path list is recorded as the
  P6-T2 baseline set.

```powershell
dotnet tool run csharpier check .
$LASTEXITCODE
```

- [x] [P0-T11] Capture the baseline analyzer build. Record it in
  `FEATURE/evidence/baseline/p0-t11-msbuild-analyzers.2026-08-29T12-22.md`. Acceptance: the artifact
  records `Command:`, `EXIT_CODE:`, and an `Output Summary:` quoting the MSBuild summary lines that
  report the error count and the warning count. If `EXIT_CODE:` is non-zero, the artifact must
  enumerate every reported error code and file so that Phase 6 can distinguish a pre-existing
  baseline failure from a regression introduced by this change.

```powershell
$vswhere = Join-Path ([Environment]::GetEnvironmentVariable('ProgramFiles(x86)')) 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
$LASTEXITCODE
```

- [x] [P0-T12] Capture the baseline nullable/type-check build. Record it in
  `FEATURE/evidence/baseline/p0-t12-msbuild-nullable.2026-08-29T12-22.md`. Acceptance: same field
  and enumeration requirements as P0-T11. Do not add `/p:Nullable=enable`; the command below is
  character-for-character the CI command.

```powershell
$vswhere = Join-Path ([Environment]::GetEnvironmentVariable('ProgramFiles(x86)')) 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
$LASTEXITCODE
```

- [x] [P0-T13] Capture the baseline `QuickFiler.Test` passing-test count against the explicitly named
  assembly `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` produced by P0-T12. Record it in
  `FEATURE/evidence/baseline/p0-t13-quickfiler-test-count.2026-08-29T12-22.md`. Acceptance: the
  artifact records `Command:`, `EXIT_CODE:`, and an `Output Summary:` that quotes verbatim the
  vstest summary line reporting the failed, passed, skipped and total counts, and records the
  passed count as `BASELINE_PASSED:` and the total count as `BASELINE_TOTAL:`. Also record
  `BASELINE_TESTMETHOD_MOVETESTS:` (expected 9) and `BASELINE_TESTMETHOD_METRICSTESTS:` (expected
  11) from the two `Select-String` counts below. Spec AC9 and AC13 compare against
  `BASELINE_PASSED:`. `/ResultsDirectory:` is mandatory on every `/Logger:trx` invocation in this
  plan, because `vstest.console.exe` otherwise writes into a `TestResults` folder relative to the
  working directory; each run gets its own task-ID subdirectory so no two runs share a folder. The
  chosen parent `TestResults` is excluded by `.gitignore` line 39, `[Tt]est[Rr]esult*/`, so the TRX
  does not dirty the tree. No acceptance condition in this plan asserts TRX file existence; the
  asserted observation is the vstest summary line recorded in the markdown artifact, so the TRX is
  a convenience record and does not belong under `FEATURE/evidence/`.

```powershell
$vswhere = Join-Path ([Environment]::GetEnvironmentVariable('ProgramFiles(x86)')) 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' '/Settings:scripts\vscode\TaskMaster.cli.runsettings' /InIsolation '/TestCaseFilter:TestCategory!=LiveOutlook' /Logger:trx '/ResultsDirectory:TestResults\p0-t13'
$LASTEXITCODE
@(Select-String -LiteralPath 'QuickFiler.Test\Controllers\QfcCollectionControllerDefects468MoveTests.cs' -SimpleMatch -Pattern '[TestMethod]').Count
@(Select-String -LiteralPath 'QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs' -SimpleMatch -Pattern '[TestMethod]').Count
```

- [x] [P0-T14] Capture the baseline solution-wide coverage figure. Run
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` and record it in
  `FEATURE/evidence/baseline/p0-t14-coverage.2026-08-29T12-22.md`. Acceptance: the artifact records
  `Command:`, `EXIT_CODE:`, and an `Output Summary:` containing `BASELINE_LINE_RATE_PERCENT:` set to
  the numeric line-coverage percentage read from the `line-rate` attribute of the root `coverage`
  element of `coverage/coverage.cobertura.xml`, multiplied by 100 and recorded to four decimal
  places. Note for the executor: `Invoke-MSTestWithCoverage.ps1` calls
  `Assert-CoberturaLineCoverageThreshold`, which throws when the solution-wide line coverage is
  below 80 percent, and it throws before the Koverage post-processing step. If the baseline run
  throws for that reason, record the thrown percentage as `BASELINE_LINE_RATE_PERCENT:`, record
  `BASELINE_THRESHOLD_STATE: below-80-at-baseline`, and continue; that is a pre-existing repository
  condition, not a condition this comment-only change can create or repair, and Phase 6 compares
  against it rather than against an absolute floor.

```powershell
pwsh -NoProfile -File 'scripts\vscode\Invoke-MSTestWithCoverage.ps1' -SearchRoot .
$LASTEXITCODE
```

- [x] [P0-T15] Re-derive every citation this plan depends on against the current tree before any edit
  is made, and record the result in
  `FEATURE/evidence/baseline/p0-t15-citation-reverification.2026-08-29T12-22.md`. Acceptance: the
  artifact records `Command:`, `EXIT_CODE:`, and an `Output Summary:` in which every one of the
  following holds, and the task fails if any single one does not:
  - `git rev-parse origin/main` and `git merge-base origin/main HEAD` still print the same value,
    re-confirming after the Phase 0 baseline captures the ancestry that P0-T6 established, so every
    `git diff origin/main` gate in this plan reports exactly this branch's changes. If they differ,
    `origin/main` advanced during Phase 0; re-run P0-T6 and then re-run P0-T10 through P0-T14 before
    proceeding, because a merge landing after a baseline invalidates that baseline.
  - the count for the single-line token `one element longer` is 1 in
    `QuickFiler/Controllers/QfcHomeController.Metrics.cs` and 1 in
    `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`;
  - the count for `IQfcCollectionController` is 0 in
    `QuickFiler/Controllers/QfcHomeController.Metrics.cs`;
  - the count for `.Where(` is 1 in `QuickFiler/Controllers/QfcHomeController.Metrics.cs`;
  - the eight pre-edit tokens listed below each have count 1 in their named file. They are drawn
    from the `Currently reads` column of the "Defect-numbering inversion — exactly 8 sites" table
    above, not from the R3 table, whose third column states post-edit text and whose tokens all have
    count 0 before Phase 3 runs. In `QuickFiler/Controllers/QfcCollectionController.cs`:
    `Issue #469 defect 1: exactly one diagnostics line` and
    `Issue #469 defect 2: the null test must dominate`. In
    `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`:
    `Issue #469 defect 1. Regression test proving that the diagnostics array`,
    `issue #469 defect 1 requires one diagnostics line per cached move`,
    `Issue #469 defect 1. Regression test proving the off-by-one`,
    `issue #469 defect 1 requires exactly one diagnostics line per cached`,
    `Issue #469 defect 2. Regression test proving that the item-controller null guard`, and
    `issue #469 defect 2 requires the null guard to run before the first`. Each pairs the defect
    number with its distinguishing text on one physical line and is the exact pre-edit counterpart
    of a token P3-T3 or P3-T10 asserts after the edit;
  - `(Get-Content).Count` is 2446 for `QuickFiler/Controllers/QfcCollectionController.cs`, 215 for
    `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, 497 for
    `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`, 453 for
    `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, and 499 for
    `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`;
  - the count for `CFN-2 RESOLVED` is 0 in
    `docs/features/active/quickfiler-home-controller-metrics-442/spec.md`.

```powershell
git rev-parse origin/main
git merge-base origin/main HEAD
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs' -SimpleMatch -Pattern 'one element longer').Count
@(Select-String -LiteralPath 'QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs' -SimpleMatch -Pattern 'one element longer').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs' -SimpleMatch -Pattern 'IQfcCollectionController').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs' -SimpleMatch -Pattern '.Where(').Count
(Get-Content -LiteralPath 'QuickFiler\Controllers\QfcCollectionController.cs').Count
(Get-Content -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs').Count
(Get-Content -LiteralPath 'QuickFiler.Test\Controllers\QfcCollectionControllerDefects468MoveTests.cs').Count
(Get-Content -LiteralPath 'QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs').Count
(Get-Content -LiteralPath 'QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs').Count
@(Select-String -LiteralPath 'docs\features\active\quickfiler-home-controller-metrics-442\spec.md' -SimpleMatch -Pattern 'CFN-2 RESOLVED').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcCollectionController.cs' -SimpleMatch -Pattern 'Issue #469 defect 1: exactly one diagnostics line').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcCollectionController.cs' -SimpleMatch -Pattern 'Issue #469 defect 2: the null test must dominate').Count
$m = 'QuickFiler.Test\Controllers\QfcCollectionControllerDefects468MoveTests.cs'
@(Select-String -LiteralPath $m -SimpleMatch -Pattern 'Issue #469 defect 1. Regression test proving that the diagnostics array').Count
@(Select-String -LiteralPath $m -SimpleMatch -Pattern 'issue #469 defect 1 requires one diagnostics line per cached move').Count
@(Select-String -LiteralPath $m -SimpleMatch -Pattern 'Issue #469 defect 1. Regression test proving the off-by-one').Count
@(Select-String -LiteralPath $m -SimpleMatch -Pattern 'issue #469 defect 1 requires exactly one diagnostics line per cached').Count
@(Select-String -LiteralPath $m -SimpleMatch -Pattern 'Issue #469 defect 2. Regression test proving that the item-controller null guard').Count
@(Select-String -LiteralPath $m -SimpleMatch -Pattern 'issue #469 defect 2 requires the null guard to run before the first').Count
```

---

### Phase 1 — Fail-Before Exception Dossier

- [x] [P1-T1] Write the fail-before exception dossier to
  `FEATURE/evidence/regression-testing/fail-before-exception.2026-08-29T12-22.md`. Acceptance: the
  file exists and contains all of the following fields: `Timestamp:`;
  `WhyFailingRunImpossible:` stating in one to three sentences that comment text and XML
  documentation carry no observable runtime behavior, so no deterministic red state exists and no
  new test can be authored that fails before this change and passes after it; an alternative-proof
  section naming the four existing tests that act as the guard for this change
  (`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`,
  `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine`,
  `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls`,
  `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing`) and stating that
  no test method is added, removed or renamed; and the negative-claim fields `SearchScope:` naming
  `FEATURE/evidence/regression-testing/`, `SearchPatterns:` naming `fail-before-exception.*.md`, and
  `SearchResult:` naming the path of this dossier. No task in this plan carries the `[expect-fail]`
  tag.

---

### Phase 2 — Stale-Comment Correction (spec items A and B)

- [x] [P2-T1] Replace lines 171 through 173 of `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
  with literal R1 exactly as quoted in the "Exact replacement text" section above, preserving the
  12-space indentation. Do not touch line 174. Acceptance: the file contains the single-line token
  `The call is made through IQfcCollectionController.GetMoveDiagnostics, which carries` exactly
  once, and `(Get-Content).Count` for the file is still 215.

- [x] [P2-T2] Verify spec AC1 as a discriminating gate. Acceptance: the count of the single-line
  token `one element longer` in `QuickFiler/Controllers/QfcHomeController.Metrics.cs` is 0. This
  gate is scoped to that one named file; no repository-wide variant is run, because the same token
  legitimately remains in this feature's `issue.md`, `spec.md` and research document and in
  `docs/features/active/quickfiler-home-controller-metrics-442/spec.md`. Record the result in
  `FEATURE/evidence/qa-gates/p2-t2-ac1-metrics-token.2026-08-29T12-22.md`.

```powershell
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs' -SimpleMatch -Pattern 'one element longer').Count
```

- [x] [P2-T3] Verify spec AC2. Acceptance: in
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, the count of `IQfcCollectionController` is
  at least 1 (discriminating: it was 0 at branch head per P0-T15), the count of `.Where(` is exactly
  1 (invariant guard: the filter expression is retained verbatim), and the count of
  `IsNullOrWhiteSpace` is at least 1 (invariant guard). Record the result in
  `FEATURE/evidence/qa-gates/p2-t3-ac2-interface-reason.2026-08-29T12-22.md`.

```powershell
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs' -SimpleMatch -Pattern 'IQfcCollectionController').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs' -SimpleMatch -Pattern '.Where(').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs' -SimpleMatch -Pattern 'IsNullOrWhiteSpace').Count
```

- [x] [P2-T4] Replace lines 398 through 400 of
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` with literal R2 exactly as quoted
  above, preserving the 8-space indentation. Leave line 397 and line 401, which are the XML summary
  opening and closing tag lines, line 402, which is the `[TestMethod]` attribute, and the entire
  method body from line 403 onward, all untouched. Acceptance: the file contains the single-line
  token
  `/// The call is made through the IQfcCollectionController.GetMoveDiagnostics contract,` exactly
  once, and `(Get-Content).Count` for the file is still 453.

- [x] [P2-T5] Verify spec AC3 as a discriminating gate. Acceptance: the count of the single-line
  token `one element longer` in `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` is 0.
  Scoped to that one named file for the reason stated in P2-T2. Record the result in
  `FEATURE/evidence/qa-gates/p2-t5-ac3-metricstests-token.2026-08-29T12-22.md`.

```powershell
@(Select-String -LiteralPath 'QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs' -SimpleMatch -Pattern 'one element longer').Count
```

- [x] [P2-T6] Verify that both Phase 2 edits are exactly three-lines-for-three-lines. Acceptance:
  `git diff origin/main --numstat` reports added count 3 and deleted count 3 for
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, and added count 3 and deleted count 3 for
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`. Record the raw numstat output in
  `FEATURE/evidence/qa-gates/p2-t6-numstat.2026-08-29T12-22.md`. The `origin/main` anchor is valid
  because P0-T15 established that `origin/main` is an ancestor of `HEAD`.

```powershell
git diff origin/main --numstat -- QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs
git status --porcelain -- QuickFiler QuickFiler.Test
```

---

### Phase 3 — Defect-Numbering Correction (spec items C1 and C2)

- [x] [P3-T1] At `QuickFiler/Controllers/QfcCollectionController.cs` line 2371, change the defect
  digit from 1 to 2 so the line reads
  `            // Issue #469 defect 2: exactly one diagnostics line per cached move group. The array`.
  Change nothing else on that line and nothing on lines 2363 through 2365. Acceptance: the file
  contains the single-line token `Issue #469 defect 2: exactly one diagnostics line` exactly once.

- [x] [P3-T2] At `QuickFiler/Controllers/QfcCollectionController.cs` line 2381, change the defect
  digit from 2 to 1 so the line reads
  `                // Issue #469 defect 1: the null test must dominate every dereference of qf. It`.
  Change nothing else on that line and nothing on lines 2373 through 2376. Acceptance: the file
  contains the single-line token `Issue #469 defect 1: the null test must dominate` exactly once.

- [x] [P3-T3] Verify spec AC5 as a set of discriminating gates over
  `QuickFiler/Controllers/QfcCollectionController.cs`. Acceptance, all four of which must hold and
  any one of which fails the task: the count of `Issue #469 defect 2: exactly one diagnostics line`
  is 1; the count of `Issue #469 defect 1: exactly one diagnostics line` is 0; the count of
  `Issue #469 defect 1: the null test must dominate` is 1; the count of
  `Issue #469 defect 2: the null test must dominate` is 0. Every token is a combined single-line
  token pairing the defect number with its distinguishing text, because both bare strings
  `Issue #469 defect 1` and `Issue #469 defect 2` occur in this file at branch head and a
  presence-only gate on either would pass before any work is done. Record the four counts in
  `FEATURE/evidence/qa-gates/p3-t3-ac5-production-renumbering.2026-08-29T12-22.md`.

```powershell
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcCollectionController.cs' -SimpleMatch -Pattern 'Issue #469 defect 2: exactly one diagnostics line').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcCollectionController.cs' -SimpleMatch -Pattern 'Issue #469 defect 1: exactly one diagnostics line').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcCollectionController.cs' -SimpleMatch -Pattern 'Issue #469 defect 1: the null test must dominate').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcCollectionController.cs' -SimpleMatch -Pattern 'Issue #469 defect 2: the null test must dominate').Count
```

- [x] [P3-T4] At `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` line
  275, change the defect digit from 1 to 2. Acceptance: the file contains the single-line token
  `Issue #469 defect 2. Regression test proving that the diagnostics array` exactly once.

- [x] [P3-T5] At `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` line
  306, change the defect digit from 1 to 2 inside the `because:` string literal. This is a string
  literal in an executable statement, not a comment; spec AC7 explicitly permits `because:` string
  edits. Do not alter the continuation lines 307 and 308. Acceptance: the file contains the
  single-line token `issue #469 defect 2 requires one diagnostics line per cached move` exactly
  once.

- [x] [P3-T6] At `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` line
  313, change the defect digit from 1 to 2. Acceptance: the file contains the single-line token
  `Issue #469 defect 2. Regression test proving the off-by-one` exactly once.

- [x] [P3-T7] At `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` line
  340, change the defect digit from 1 to 2 inside the `because:` string literal. Do not alter the
  continuation line 341. Acceptance: the file contains the single-line token
  `issue #469 defect 2 requires exactly one diagnostics line per cached` exactly once.

- [x] [P3-T8] At `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` line
  352, change the defect digit from 2 to 1. Acceptance: the file contains the single-line token
  `Issue #469 defect 1. Regression test proving that the item-controller null guard` exactly once.

- [x] [P3-T9] At `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` line
  387, change the defect digit from 2 to 1 inside the `because:` string literal. Do not alter the
  continuation lines 388 and 389. Acceptance: the file contains the single-line token
  `issue #469 defect 1 requires the null guard to run before the first` exactly once.

- [x] [P3-T10] Verify the static half of spec AC6 as a set of discriminating gates over
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`. Acceptance, all six
  of which must hold and any one of which fails the task: each of the six tokens below has count 1.
  Every token is a combined single-line token for the reason stated in P3-T3. Record the six counts
  in `FEATURE/evidence/qa-gates/p3-t10-ac6-test-renumbering.2026-08-29T12-22.md`.
  The artifact must additionally record the plan's reading of two clauses in spec AC6 whose wording
  does not match the tree. First, AC6 says "the three array-length tests"; two tests carry the
  defect-2 citations, `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine` declared at `:290` and
  `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` declared at `:327`, and the third
  test in the group, `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing`
  declared at `:369`, is the null-guard test and moves to defect 1. Second, AC6 says "the three test
  method bodies are unchanged"; three of the six edited lines (`:306`, `:340`, `:387`) are
  FluentAssertions `because:` string literals inside method bodies, so that clause holds in the sense
  that no executable statement, assertion subject, or control flow changes and only the
  failure-message text does. Spec AC7 explicitly permits `because:` string edits. P7-T6 may check AC6
  off only after this record exists.

```powershell
$f = 'QuickFiler.Test\Controllers\QfcCollectionControllerDefects468MoveTests.cs'
@(Select-String -LiteralPath $f -SimpleMatch -Pattern 'Issue #469 defect 2. Regression test proving that the diagnostics array').Count
@(Select-String -LiteralPath $f -SimpleMatch -Pattern 'issue #469 defect 2 requires one diagnostics line per cached move').Count
@(Select-String -LiteralPath $f -SimpleMatch -Pattern 'Issue #469 defect 2. Regression test proving the off-by-one').Count
@(Select-String -LiteralPath $f -SimpleMatch -Pattern 'issue #469 defect 2 requires exactly one diagnostics line per cached').Count
@(Select-String -LiteralPath $f -SimpleMatch -Pattern 'Issue #469 defect 1. Regression test proving that the item-controller null guard').Count
@(Select-String -LiteralPath $f -SimpleMatch -Pattern 'issue #469 defect 1 requires the null guard to run before the first').Count
```

- [x] [P3-T11] Verify that the eight renumbering edits changed exactly eight lines and added no
  lines. Acceptance: `git diff origin/main --numstat` reports added count 2 and deleted count 2 for
  `QuickFiler/Controllers/QfcCollectionController.cs`, and added count 6 and deleted count 6 for
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`. These figures are
  exact because each of the eight edits is a single-character substitution that preserves line
  length. Record the raw numstat output in
  `FEATURE/evidence/qa-gates/p3-t11-numstat.2026-08-29T12-22.md`.

```powershell
git diff origin/main --numstat -- QuickFiler/Controllers/QfcCollectionController.cs QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs
git status --porcelain -- QuickFiler QuickFiler.Test
```

---

### Phase 4 — Cross-Feature Note Resolution (spec item D)

- [x] [P4-T1] Replace line 869 of
  `docs/features/active/quickfiler-home-controller-metrics-442/spec.md` with literal R4 exactly as
  quoted above. Acceptance: that file contains the single-line token
  `### CFN-2 — RESOLVED —` exactly once.

- [x] [P4-T2] Insert literal R5 exactly as quoted above into
  `docs/features/active/quickfiler-home-controller-metrics-442/spec.md` immediately after the blank
  line 870 and ahead of the existing `- **Location:**` bullet. Do not delete or reword any existing
  bullet in the CFN-2 section; they are retained as the historical record. Acceptance: that file
  contains the single-line token `CFN-2 RESOLVED (2026-08-29).` exactly once. That token is short
  and sits entirely on the first physical line of R5, so it survives any reflow of the bullet's
  continuation lines.

**Progress-commit boundary after task 35.** Stop here after checking off `[P4-T2]` and return
`PROGRESS_COMMIT_REQUIRED: P0-T1..P4-T2`. Do not begin `[P4-T3]` until the orchestrator has recorded
the completed interval's commit SHA in the canonical checkpoint.

- [x] [P4-T3] Verify spec AC11. Acceptance: in
  `docs/features/active/quickfiler-home-controller-metrics-442/spec.md`, the count of the token
  `CFN-2 RESOLVED` is at least 1 (discriminating: it was 0 at branch head per P0-T15) and the count
  of the token `CFN-2` is at least 9 (invariant guard: nine occurrences exist at branch head at
  lines 130, 147, 300, 591, 835, 869, 927, 940 and 953, and none may be deleted). Markdown files are
  exempt from the 500-line cap, so no line-count gate applies to this file. Record the two counts in
  `FEATURE/evidence/qa-gates/p4-t3-ac11-cfn2-resolved.2026-08-29T12-22.md`.

```powershell
$s = 'docs\features\active\quickfiler-home-controller-metrics-442\spec.md'
@(Select-String -LiteralPath $s -SimpleMatch -Pattern 'CFN-2 RESOLVED').Count
@(Select-String -LiteralPath $s -SimpleMatch -Pattern 'CFN-2').Count
```

---

### Phase 5 — Scope-Boundary and Invariant Verification

- [x] [P5-T1] Verify the first half of spec AC12: the forbidden file is absent from the change.
  Acceptance: the output of `git diff origin/main --name-only -- QuickFiler QuickFiler.Test docs`
  contains zero lines equal to `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, and the
  output of the companion `git status --porcelain -- QuickFiler QuickFiler.Test docs` likewise
  contains zero lines naming that path. The porcelain companion is required because a
  `--name-only` diff enumerates tracked changes only and cannot report an untracked addition. Record
  both outputs in
  `FEATURE/evidence/qa-gates/p5-t1-ac12-forbidden-file.2026-08-29T12-22.md`. This is an invariant
  guard against scope creep into issue #629.

```powershell
git diff origin/main --name-only -- QuickFiler QuickFiler.Test docs
git status --porcelain -- QuickFiler QuickFiler.Test docs
```

- [x] [P5-T2] Verify the second half of spec AC12: issue #629 was not absorbed. Acceptance: the count
  of the token `StackMovedItems` in `QuickFiler/Interfaces/IQfcCollectionController.cs` is at least
  2 (it occurs at lines 54 and 63 at branch head).
  Casing note: the issue text and the implementation use the camelCase form `stackMovedItems`, but
  the interface declares the parameter in PascalCase as `StackMovedItems` at `:54` and `:63`, and the
  camelCase form does not occur in that file at all. The gate is run with `-CaseSensitive` and asserts
  the interface's casing, so a case-sensitive gate on the camelCase spelling, which would be
  unsatisfiable against this file, is deliberately not authored.
  This is an invariant guard. Record the count in
  `FEATURE/evidence/qa-gates/p5-t2-ac12-parameter-retained.2026-08-29T12-22.md`.

```powershell
@(Select-String -LiteralPath 'QuickFiler\Interfaces\IQfcCollectionController.cs' -CaseSensitive -SimpleMatch -Pattern 'StackMovedItems').Count
```

- [x] [P5-T3] Verify the whitespace filter was not deleted, statically. Acceptance, both of which
  must hold: in `QuickFiler/Controllers/QfcHomeController.Metrics.cs` the count of the single-line
  token `strOutput.Where(line` is exactly 1 and the count of the single-line token
  `IsNullOrWhiteSpace(line)).ToArray();` is exactly 1. Both tokens are drawn from the single
  physical line 174 and neither contains an angle bracket, so neither can be mistaken for a
  documented command shape. Spec non-goal 5 forbids deleting this filter; the behavioral
  counterpart of this guard is P6-T7. This is an invariant guard. Record both counts in
  `FEATURE/evidence/qa-gates/p5-t3-filter-retained.2026-08-29T12-22.md`.

```powershell
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs' -SimpleMatch -Pattern 'strOutput.Where(line').Count
@(Select-String -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs' -SimpleMatch -Pattern 'IsNullOrWhiteSpace(line)).ToArray();').Count
```

- [x] [P5-T4] Verify spec AC8 and the companion file-size invariants. Acceptance, all five of which
  must hold: `(Get-Content).Count` is at most 2446 for
  `QuickFiler/Controllers/QfcCollectionController.cs` (spec AC8; no split is performed and
  decomposition remains delegated to open issue #623), at most 497 for
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`, at most 215 for
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, at most 453 for
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, and exactly 499 for
  `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, which this change does not touch.
  Use `(Get-Content).Count`, not `Measure-Object -Line`. Record all five figures in
  `FEATURE/evidence/qa-gates/p5-t4-ac8-file-sizes.2026-08-29T12-22.md`.

```powershell
(Get-Content -LiteralPath 'QuickFiler\Controllers\QfcCollectionController.cs').Count
(Get-Content -LiteralPath 'QuickFiler.Test\Controllers\QfcCollectionControllerDefects468MoveTests.cs').Count
(Get-Content -LiteralPath 'QuickFiler\Controllers\QfcHomeController.Metrics.cs').Count
(Get-Content -LiteralPath 'QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs').Count
(Get-Content -LiteralPath 'QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs').Count
```

- [x] [P5-T5] Verify spec AC7 by classifying every changed line in the C# diff. Acceptance: for
  `git diff origin/main -- QuickFiler QuickFiler.Test`, every output line that begins with a single
  `+` or a single `-` and is not a `+++` or `---` file header, after removal of that leading
  character and of leading whitespace, begins with one of exactly three prefixes: `// `, `/// `, or
  `because: `. The executor records the full classified list in
  `FEATURE/evidence/qa-gates/p5-t5-ac7-changed-line-classification.2026-08-29T12-22.md` together
  with the total changed-line count, which must be 28: 3 added and 3 deleted in
  `QfcHomeController.Metrics.cs`, 3 added and 3 deleted in `QfcHomeControllerMetricsTests.cs`, 2
  added and 2 deleted in `QfcCollectionController.cs`, and 6 added and 6 deleted in
  `QfcCollectionControllerDefects468MoveTests.cs` — 14 added and 14 deleted, 28 diff lines in total.
  The task fails if any changed line falls outside the three prefixes or if the per-file added and
  deleted counts differ from those figures. This is the mechanical realisation of "zero executable
  lines change".

```powershell
git diff origin/main -- QuickFiler QuickFiler.Test
git diff origin/main --numstat -- QuickFiler QuickFiler.Test
```

- [x] [P5-T6] Verify the "no test method is added or removed" half of spec AC9, statically.
  Acceptance: the count of `[TestMethod]` is 9 in
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` and 11 in
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, each equal to the corresponding
  `BASELINE_TESTMETHOD_` value recorded by P0-T13. This is an invariant guard. Record both counts in
  `FEATURE/evidence/qa-gates/p5-t6-ac9-testmethod-counts.2026-08-29T12-22.md`.

```powershell
@(Select-String -LiteralPath 'QuickFiler.Test\Controllers\QfcCollectionControllerDefects468MoveTests.cs' -SimpleMatch -Pattern '[TestMethod]').Count
@(Select-String -LiteralPath 'QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs' -SimpleMatch -Pattern '[TestMethod]').Count
```

---

### Phase 6 — Full C# QA Loop

Every command task in this phase is unconditional. `EXIT_CODE: SKIPPED` is not a passing outcome for
any of them. If any task in this phase fails or rewrites a tracked file, restart the phase from
P6-T1.

- [x] [P6-T1] Run the CSharpier formatting pass. If P0-T10 recorded `EXIT_CODE: 0`, run
  `dotnet tool run csharpier format .` at the repository root. If P0-T10 recorded a non-zero exit
  code, the repository carries pre-existing formatting drift that a repo-wide mutating pass would
  sweep into this change's diff and break spec AC7, so instead run
  `dotnet tool run csharpier format` against exactly the four C# paths this plan edits. Either way
  the command runs; only its path argument is conditioned on the recorded baseline. Acceptance: the
  invocation exits 0 and `git diff origin/main --name-only -- QuickFiler QuickFiler.Test` lists no
  path other than the four this plan edits. The exit code alone is not sufficient evidence for this
  write-mode command, because `format` exits 0 both when it rewrites files and when it does not;
  the name-only diff is the required additional observation, and
  `git status --porcelain -- QuickFiler QuickFiler.Test` is recorded alongside it as the companion
  that can report an untracked addition. Record all three outputs in
  `FEATURE/evidence/qa-gates/p6-t1-csharpier-format.2026-08-29T12-22.md`.
  Additionally, `git diff origin/main --numstat -- QuickFiler QuickFiler.Test` after the format pass
  must still report added 3 / deleted 3 for `QuickFiler/Controllers/QfcHomeController.Metrics.cs`,
  added 3 / deleted 3 for `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, added 2 /
  deleted 2 for `QuickFiler/Controllers/QfcCollectionController.cs`, and added 6 / deleted 6 for
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`, unchanged from the
  figures P2-T6, P3-T11 and P5-T5 recorded before this write-mode command ran. Record this numstat
  output in the same artifact. Without it the AC7 evidence cited by P7-T7 predates the formatter and
  is not known to still describe the tree.

```powershell
# Run exactly one of the next two lines, selected by the exit code P0-T10 recorded.
# P0-T10 EXIT_CODE 0: repository-wide pass is safe.
dotnet tool run csharpier format .
# P0-T10 exit code non-zero: scope the mutating pass to this plan's four C# paths.
dotnet tool run csharpier format QuickFiler\Controllers\QfcCollectionController.cs QuickFiler\Controllers\QfcHomeController.Metrics.cs QuickFiler.Test\Controllers\QfcCollectionControllerDefects468MoveTests.cs QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs
$LASTEXITCODE
git diff origin/main --name-only -- QuickFiler QuickFiler.Test
git status --porcelain -- QuickFiler QuickFiler.Test
git diff origin/main --numstat -- QuickFiler QuickFiler.Test
```

- [x] [P6-T2] Run `dotnet tool run csharpier check .` at the repository root. Acceptance: every file
  the output reports as unformatted also appears in the enumeration recorded by P0-T10. If P0-T10
  recorded no unformatted file, this means `EXIT_CODE: 0` and an output carrying zero
  unformatted-file reports. If P0-T10 enumerated unformatted files, the exit code may be non-zero, and
  the acceptance is instead that the set of files reported here is a subset of the P0-T10 enumeration
  and that none of the four C# paths this plan edits appears in it; any file reported here and absent
  from the P0-T10 enumeration is a regression introduced by this change, and the phase restarts from
  P6-T1 after it is fixed. This mirrors the baseline-relative rule used by P6-T3 and P6-T4 and is
  required because P6-T1 deliberately does not repair pre-existing drift in unrelated files. Record
  the exit code, the full reported file list, and the subset verdict in
  `FEATURE/evidence/qa-gates/p6-t2-csharpier-check.2026-08-29T12-22.md`.

```powershell
dotnet tool run csharpier check .
$LASTEXITCODE
```

- [x] [P6-T3] Run the analyzer build. Acceptance: if `EXIT_CODE: 0`, the MSBuild summary reports
  `0 Error(s)`. If `EXIT_CODE:` is non-zero, compare every reported diagnostic against the
  enumeration recorded by P0-T11: a diagnostic present in that enumeration is a pre-existing
  baseline failure and is an accepted baseline-relative outcome; any diagnostic absent from that
  enumeration is a regression introduced by this change and the phase restarts from P6-T1 after it
  is fixed. Record the outcome in
  `FEATURE/evidence/qa-gates/p6-t3-msbuild-analyzers.2026-08-29T12-22.md`.

```powershell
$vswhere = Join-Path ([Environment]::GetEnvironmentVariable('ProgramFiles(x86)')) 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
$LASTEXITCODE
```

- [x] [P6-T4] Run the nullable/type-check build. Acceptance and baseline-comparison rule: identical
  to P6-T3, compared against the P0-T12 enumeration. `/p:Nullable=enable` must not be added. Record
  the outcome in `FEATURE/evidence/qa-gates/p6-t4-msbuild-nullable.2026-08-29T12-22.md`.

```powershell
$vswhere = Join-Path ([Environment]::GetEnvironmentVariable('ProgramFiles(x86)')) 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
$LASTEXITCODE
```

- [x] [P6-T5] Run the solution-wide coverage-enabled test pass, which is the realisation of
  toolchain step 4. Acceptance: the run completes and
  `FEATURE/evidence/qa-gates/p6-t5-coverage.2026-08-29T12-22.md` records `Command:`, `EXIT_CODE:`,
  and an `Output Summary:` containing `POST_LINE_RATE_PERCENT:` set to the numeric line-coverage
  percentage read from the `line-rate` attribute of the root `coverage` element of
  `coverage/coverage.cobertura.xml`, multiplied by 100 and recorded to four decimal places, together
  with `POST_THRESHOLD_STATE:` mirroring the P0-T14 convention. The raw Cobertura file stays under
  `coverage/`, which `.gitignore` line 144 excludes, so it does not dirty the tree.

```powershell
pwsh -NoProfile -File 'scripts\vscode\Invoke-MSTestWithCoverage.ps1' -SearchRoot .
$LASTEXITCODE
```

- [x] [P6-T6] Re-run the scoped `QuickFiler.Test` pass and verify spec AC9. Acceptance: the vstest
  summary reports a passed count equal to `BASELINE_PASSED:` from P0-T13, a total count equal to
  `BASELINE_TOTAL:` from P0-T13, and a failed count of 0. Record the summary line verbatim as
  `POST_PASSED:` and `POST_TOTAL:` in
  `FEATURE/evidence/regression-testing/p6-t6-quickfiler-test-count.2026-08-29T12-22.md`.

```powershell
$vswhere = Join-Path ([Environment]::GetEnvironmentVariable('ProgramFiles(x86)')) 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' '/Settings:scripts\vscode\TaskMaster.cli.runsettings' /InIsolation '/TestCaseFilter:TestCategory!=LiveOutlook' /Logger:trx '/ResultsDirectory:TestResults\p6-t6'
$LASTEXITCODE
```

- [x] [P6-T7] Verify spec AC4 and the behavioral half of spec AC6 by naming the four guard tests
  explicitly. Acceptance: the run reports total 4, passed 4, failed 0, skipped 0 for the four named
  tests `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`,
  `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine`,
  `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` and
  `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing`. Record the summary
  line verbatim in
  `FEATURE/evidence/regression-testing/p6-t7-named-guard-tests.2026-08-29T12-22.md`. Naming the
  tests rather than searching for prose is deliberate: a test node identifier is stable under
  reformatting.

```powershell
$vswhere = Join-Path ([Environment]::GetEnvironmentVariable('ProgramFiles(x86)')) 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
$filter = 'FullyQualifiedName~WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting|FullyQualifiedName~GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine|FullyQualifiedName~GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls|FullyQualifiedName~GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing'
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' '/Settings:scripts\vscode\TaskMaster.cli.runsettings' /InIsolation "/TestCaseFilter:$filter" /Logger:trx '/ResultsDirectory:TestResults\p6-t7'
$LASTEXITCODE
```

- [x] [P6-T8] Record the coverage comparison and the non-attribution statement in
  `FEATURE/evidence/qa-gates/p6-t8-coverage-delta.2026-08-29T12-22.md`. Acceptance: the artifact
  records `BASELINE_LINE_RATE_PERCENT:` from P0-T14, `POST_LINE_RATE_PERCENT:` from P6-T5, their
  arithmetic difference in percentage points as `DELTA_PERCENTAGE_POINTS:`, and a
  `CHANGED_LINE_COVERAGE:` field. The delta must be greater than or equal to minus 0.50 percentage
  points. If it is not, re-run P6-T5 once and recompute against the second reading before declaring
  a regression, because `dotnet-coverage` denominator selection is not deterministic across runs in
  this repository and the recorded difference can exceed the band with no executable line changed.
  Record both readings and the verdict. A difference inside the band is instrumentation and
  scheduling noise and not a regression, since no executable line changed.
  `CHANGED_LINE_COVERAGE:` must be recorded as
  `NOT APPLICABLE — 0 executable lines changed` because P5-T5 established that all 28 diff lines in
  `QuickFiler/` and `QuickFiler.Test/` are comment, XML-doc or `because:` string lines, and a
  changed-line coverage figure over an empty executable changed-line set is undefined rather than
  zero. The artifact must additionally state: `QuickFiler/Controllers/QfcCollectionController.cs`
  carries `[ExcludeFromCodeCoverage]` at line 21, so no coverage figure in this artifact is
  attributable to that class and no coverage-increase claim is made for it anywhere in this plan.

- [x] [P6-T9] Declare the clean toolchain pass. Acceptance: record in
  `FEATURE/evidence/qa-gates/p6-t9-clean-pass.2026-08-29T12-22.md` that P6-T1 through P6-T7 all
  completed in a single uninterrupted sequence with no failure and no file rewrite between them,
  naming each command run and its exit code. "No failure" here means that each task's own acceptance
  condition held, not that every recorded exit code was 0: P6-T2 through P6-T5 are baseline-relative
  and may record a non-zero exit code only when their respective task acceptance condition explicitly
  accepts the baseline state. If any of those tasks failed its acceptance condition or rewrote a tracked file, this task fails and the phase
  restarts from P6-T1.
  The artifact must additionally record the AC10 realisation mapping explicitly, one line per
  toolchain step: step 1 `dotnet tool run csharpier format .` and `check .` by P6-T1 and P6-T2; step
  2 the `EnableNETAnalyzers` and `EnforceCodeStyleInBuild` msbuild by P6-T3; step 3 the
  `TreatWarningsAsErrors` msbuild by P6-T4; step 4 `vstest.console.exe` by P6-T6 and P6-T7 with
  coverage collection realised by `dotnet-coverage` in P6-T5. The artifact must state that no
  invocation in this plan passes the `/EnableCodeCoverage` switch named in spec AC10, because
  `scripts/vscode/TaskMaster.cli.runsettings` declares no coverage `DataCollector` and this
  repository's coverage pipeline is `dotnet-coverage` producing Cobertura, and must state that this
  is a wording divergence between AC10 and the repository's actual step-4 command rather than an
  omitted step.

- [x] [P6-T10] Verify spec AC13. Acceptance:
  `FEATURE/evidence/regression-testing/p6-t10-test-count-comparison.2026-08-29T12-22.md` exists and
  records `BASELINE_PASSED:` (from P0-T13), `POST_PASSED:` (from P6-T6), the two source artifact
  paths, and an explicit equality verdict. Both figures live under
  `FEATURE/evidence/regression-testing/`, which is the canonical location required by the repository
  evidence conventions.

---

### Phase 7 — Acceptance Check-off, Commit, and Traceability

- [x] [P7-T1] Check off AC1 in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/spec.md`
  by changing its list marker from `- [ ] AC1` to `- [x] AC1`. Do not alter the criterion text; the
  criterion line changes only in its checkbox marker. Record the evidence path
  `FEATURE/evidence/qa-gates/p2-t2-ac1-metrics-token.2026-08-29T12-22.md`. Acceptance: exactly one
  line in that file begins with `- [x] AC1 —` (the em dash and its leading space are required:
  without them the string is also a prefix of `- [x] AC10` through `- [x] AC13`) and the cited
  artifact exists on disk.

- [x] [P7-T2] Check off AC2 in the same file, recording in this task's progress output
  `FEATURE/evidence/qa-gates/p2-t3-ac2-interface-reason.2026-08-29T12-22.md`. Acceptance: exactly
  one line begins with `- [x] AC2 —` and the cited artifact exists on disk.

- [x] [P7-T3] Check off AC3, recording in this task's progress output
  `FEATURE/evidence/qa-gates/p2-t5-ac3-metricstests-token.2026-08-29T12-22.md`. Acceptance: exactly
  one line begins with `- [x] AC3 —` and the cited artifact exists on disk.

- [x] [P7-T4] Check off AC4, recording in this task's progress output
  `FEATURE/evidence/regression-testing/p6-t7-named-guard-tests.2026-08-29T12-22.md`. Acceptance:
  exactly one line begins with `- [x] AC4 —` and the cited artifact exists on disk.

- [x] [P7-T5] Check off AC5, recording in this task's progress output
  `FEATURE/evidence/qa-gates/p3-t3-ac5-production-renumbering.2026-08-29T12-22.md`. Acceptance:
  exactly one line begins with `- [x] AC5 —` and the cited artifact exists on disk.

- [x] [P7-T6] Check off AC6, recording in this task's progress output
  `FEATURE/evidence/qa-gates/p3-t10-ac6-test-renumbering.2026-08-29T12-22.md` and
  `FEATURE/evidence/regression-testing/p6-t7-named-guard-tests.2026-08-29T12-22.md`. Acceptance:
  exactly one line begins with `- [x] AC6 —` and both cited artifacts exist on disk.

- [x] [P7-T7] Check off AC7, recording in this task's progress output
  `FEATURE/evidence/qa-gates/p5-t5-ac7-changed-line-classification.2026-08-29T12-22.md`.
  Acceptance: exactly one line begins with `- [x] AC7 —` and the cited artifact exists on disk.

- [x] [P7-T8] Check off AC8, recording in this task's progress output
  `FEATURE/evidence/qa-gates/p5-t4-ac8-file-sizes.2026-08-29T12-22.md`. Acceptance: exactly one line
  begins with `- [x] AC8 —` and the cited artifact exists on disk.

- [x] [P7-T9] Check off AC9, recording in this task's progress output
  `FEATURE/evidence/regression-testing/p6-t6-quickfiler-test-count.2026-08-29T12-22.md` and
  `FEATURE/evidence/qa-gates/p5-t6-ac9-testmethod-counts.2026-08-29T12-22.md`. Acceptance: exactly
  one line begins with `- [x] AC9 —` and both cited artifacts exist on disk.

- [x] [P7-T10] Check off AC10, recording in this task's progress output
  `FEATURE/evidence/qa-gates/p6-t9-clean-pass.2026-08-29T12-22.md`. Acceptance: exactly one line
  begins with `- [x] AC10 —` and the cited artifact exists on disk.

- [x] [P7-T11] Check off AC11, recording in this task's progress output
  `FEATURE/evidence/qa-gates/p4-t3-ac11-cfn2-resolved.2026-08-29T12-22.md`. Acceptance: exactly one
  line begins with `- [x] AC11 —` and the cited artifact exists on disk.

- [x] [P7-T12] Check off AC12, recording in this task's progress output
  `FEATURE/evidence/qa-gates/p5-t1-ac12-forbidden-file.2026-08-29T12-22.md` and
  `FEATURE/evidence/qa-gates/p5-t2-ac12-parameter-retained.2026-08-29T12-22.md`. Acceptance: exactly
  one line begins with `- [x] AC12 —` and both cited artifacts exist on disk.

- [x] [P7-T13] Check off AC13, recording in this task's progress output
  `FEATURE/evidence/regression-testing/p6-t10-test-count-comparison.2026-08-29T12-22.md`.
  Acceptance: exactly one line begins with `- [x] AC13 —` and the cited artifact exists on disk.

- [x] [P7-T14] Prepare the final partial-interval commit candidate without staging or committing it.
  Enumerate the four C# files, the two documentation files
  (`docs/features/active/quickfiler-home-controller-metrics-442/spec.md` and this feature's
  `spec.md`), this plan file, and everything under `FEATURE/evidence/` that remains uncommitted after
  the task-35 boundary. Disposition of issue #469 remains the maintainer's decision, so the eventual
  commit message must contain no GitHub closing keyword. Acceptance: the candidate enumeration is
  recorded in `FEATURE/evidence/other/p7-t14-final-commit-candidate.2026-08-29T12-22.md`; no index
  entry or commit is created by this task; and the recorded candidate contains no out-of-scope path.

- [x] [P7-T15] Verify that no commit on this branch carries a GitHub closing keyword for issue #469.
  Acceptance: over the concatenated commit messages of the range `origin/main..HEAD`, the
  case-insensitive count of each of these nine tokens is 0: `close #469`, `closes #469`,
  `closed #469`, `fix #469`, `fixes #469`, `fixed #469`, `resolve #469`, `resolves #469`,
  `resolved #469`.
  Record every count in
  `FEATURE/evidence/qa-gates/p7-t15-no-closing-keyword.2026-08-29T12-22.md`. This gate is scoped to
  commit messages; the tokens legitimately appear in documentation prose and are not searched for
  there.

```powershell
$log = git log origin/main..HEAD --format=%B | Out-String
@('close #469','closes #469','closed #469','fix #469','fixes #469','fixed #469','resolve #469','resolves #469','resolved #469') | ForEach-Object { $_ + ' => ' + ([regex]::Matches($log, [regex]::Escape($_), 'IgnoreCase').Count) }
```

- [x] [P7-T16] Verify the final change footprint against `origin/main`. Acceptance: `git diff
  origin/main --name-only -- QuickFiler QuickFiler.Test docs` lists exactly these five source and
  document paths and no others, plus this plan file, this feature's `spec.md`, and any number of
  paths under `FEATURE/evidence/`:
  `QuickFiler/Controllers/QfcCollectionController.cs`,
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs`,
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`,
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`,
  `docs/features/active/quickfiler-home-controller-metrics-442/spec.md`.
  This feature's `issue.md` and its `research/` document are additionally expected in the output and
  are excluded from the exact enumeration, because earlier commits on this branch added them and they
  therefore appear in every `origin/main`-anchored diff regardless of this plan's edits. No `.csproj`,
  `.props`, `.targets`, `packages.config`, or coverage-configuration file may appear. The pathspec
  `-- QuickFiler QuickFiler.Test docs` is mandatory because this is the issue-deliverable footprint
  gate. The branch also carries committed preparation history outside these roots; that history is
  reviewed by the later full feature-branch review rather than attributed to this plan. The companion
  `git status --porcelain -- QuickFiler QuickFiler.Test docs` output is recorded in the same artifact,
  because a `--name-only` diff cannot report an untracked addition. Record both in
  `FEATURE/evidence/qa-gates/p7-t16-final-footprint.2026-08-29T12-22.md`.

```powershell
git diff origin/main --name-only -- QuickFiler QuickFiler.Test docs
git status --porcelain -- QuickFiler QuickFiler.Test docs
```

- [x] [P7-T17] Finalise the final partial interval without staging or committing it. Run
  `git status --porcelain -- QuickFiler QuickFiler.Test docs` and the P7-T15 closing-keyword scan.
  Acceptance: every status path belongs to the P7-T16 allowed footprint; the status includes this
  plan file and `FEATURE/evidence/other/p7-t17-finalisation.2026-08-29T12-22.md`; and the P7-T15 scan
  reports 0 for all nine tokens over `origin/main..HEAD`. Record the final state in that artifact,
  check off this task, and return `PROGRESS_COMMIT_REQUIRED: P4-T3..P7-T17`. The orchestrator then
  stages the complete final interval, collects canonical commit context, obtains the routed
  commit-steward message, commits it, records the interval and SHA in the canonical checkpoint, and
  reruns the closing-keyword scan before review.

```powershell
git status --porcelain -- QuickFiler QuickFiler.Test docs
git log origin/main..HEAD --format=%B
```

---

## Acceptance-criteria traceability

Every one of the spec's 13 acceptance criteria maps to at least one task that verifies it.

| Spec AC | Verifying task(s) | Gate kind |
|---|---|---|
| AC1 | P2-T2 | discriminating |
| AC2 | P2-T3 | discriminating (`IQfcCollectionController` count moves 0 to at least 1) plus two invariant guards |
| AC3 | P2-T5 | discriminating |
| AC4 | P6-T7 | named-test pass |
| AC5 | P3-T3 | discriminating, four combined single-line tokens |
| AC6 | P3-T10, P6-T7 | discriminating (six tokens) plus named-test pass |
| AC7 | P5-T5, P3-T11, P2-T6 | changed-line classification plus exact per-file numstat |
| AC8 | P5-T4 | invariant guard on line counts |
| AC9 | P6-T6, P5-T6 | baseline-relative passing count plus `[TestMethod]` count invariance |
| AC10 | P6-T1 through P6-T7, declared by P6-T9 | unconditional toolchain commands in order |
| AC11 | P4-T3 | discriminating (`CFN-2 RESOLVED` count moves 0 to at least 1) plus an invariant guard |
| AC12 | P5-T1, P5-T2 | invariant guards against absorbing issue #629 |
| AC13 | P0-T13, P6-T6, P6-T10 | baseline plus post-change counts recorded under `evidence/regression-testing/` |

Acceptance criteria this change cannot fail are not restated anywhere in this plan. In particular,
no acceptance condition claims a coverage increase attributable to
`QuickFiler/Controllers/QfcCollectionController.cs`, and no repository-wide zero-hit gate is
authored for `one element longer` or `Issue #469 defect`.

## Deliberately not delivered

Issue #469's Expected Behavior item 4 is not delivered by this plan. The only remaining action for
that item is removal of the `stackMovedItems` parameter, which is open issue #629. This plan gates
against absorbing it (P5-T1 and P5-T2) rather than attempting it.

---

## SELF-REVIEW: RE-DERIVED THIS PASS

Adversarial self-review completed in the preflight revision pass that produced version 0.4. Every
citation below was re-derived directly against the current working tree during this pass; none was
carried forward from the delegation prompt, from `spec.md`, from the research document, or from an
earlier round of this plan without independent confirmation. Items 41 through 51 are the citations
that the version 0.3 revision deltas introduced or altered, and item 24 is the citation that
revision corrected.

The version 0.4 pass ran against a tree into which `origin/main` had been merged, so every citation
was re-observed after that merge rather than before it. Items 18, 20, 22, 23, 24, 25, 26, 29, 31 and
52 were re-derived directly in this pass. Item 18 was reclassified, item 31 was corrected for a line
shift the merge introduced, and item 52 is new.

The version 0.5 pass applied two localized text insertions and renumbered nothing. Items 53 and 54
are new and record what that pass measured. Items 10 through 17 were re-measured as counts rather
than as line reads in the same pass, because the version 0.5 delta makes P0-T15 assert those eight
counts explicitly; each was confirmed at count 1 in its named file. No other citation changed.

1. `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/issue.md:12` —
   re-derived: the line reads `- Work Mode: full-bug`. Mode resolves to `full-bug`, so `spec.md` is
   required and `user-story.md` is optional and absent.
2. `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/spec.md:302-316` —
   re-derived: the `## Acceptance Criteria` section holds exactly 13 unchecked criteria, AC1 through
   AC13. All 13 are mapped in the traceability table above.
3. Research document path — re-derived with `rg --files`: the single markdown file under the feature's
   `research/` subdirectory is
   `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/research/2026-08-29T12-31-qfc-collection-move-diagnostics-defects-469.md`.
4. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:171` — re-derived: carries the single-line
   token `one element longer`. Repository search excluding `docs/**` returns exactly two hits for
   that token, this one and item 5.
5. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:398` — re-derived: carries the same
   single-line token, inside the XML doc comment opened at `:397` and closed at `:401`.
6. `trailing element is null` — re-derived: in both files the phrase wraps across two comment lines
   (`Metrics.cs:171-172`, `QfcHomeControllerMetricsTests.cs:398-399`), so it matches zero single
   source lines. No gate in this plan asserts it.
7. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:174` — re-derived: reads
   `var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();`. Exactly one
   `.Where(` occurrence in the file. Not deleted by any task.
8. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:403` — re-derived: the method
   `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` is declared there; `:406` feeds
   `new[] { "line-one", "   ", null, "line-two" }` and `:420-423` asserts only `line-one` and
   `line-two` reach the writer. Deleting the filter fails this test.
9. `IQfcCollectionController` in `QuickFiler/Controllers/QfcHomeController.Metrics.cs` — re-derived:
   zero occurrences. The AC2 gate is therefore false-before / true-after.
10. `QuickFiler/Controllers/QfcCollectionController.cs:2371` — re-derived: reads
    `// Issue #469 defect 1: exactly one diagnostics line per cached move group. The array`.
11. `QuickFiler/Controllers/QfcCollectionController.cs:2381` — re-derived: reads
    `// Issue #469 defect 2: the null test must dominate every dereference of qf. It`.
12. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:275` — re-derived:
    `/// Issue #469 defect 1. Regression test proving that the diagnostics array carries exactly`.
13. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:306` — re-derived:
    `because: "issue #469 defect 1 requires one diagnostics line per cached move "`; a string
    literal, with continuations at `:307-308`.
14. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:313` — re-derived:
    `/// Issue #469 defect 1. Regression test proving the off-by-one is a length defect at every`.
15. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:340` — re-derived:
    `because: "issue #469 defect 1 requires exactly one diagnostics line per cached "`; a string
    literal, with a continuation at `:341`.
16. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:352` — re-derived:
    `/// Issue #469 defect 2. Regression test proving that the item-controller null guard runs`.
    Correction to the research document, which cites `:351` for this site in its section 8 table;
    the actual line is 352, matching the delegation prompt.
17. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:387` — re-derived:
    `because: "issue #469 defect 2 requires the null guard to run before the first "`; a string
    literal, with continuations at `:388-389`.
18. Sibling-region re-check for the renumbering — re-derived: the case-sensitive and
    case-insensitive searches for `Issue #469 defect` outside `docs/**` return 17 hits. Eight are the
    sites above. The remaining nine are `QfcCollectionController.cs:71`, `:732`, `:2344`;
    `QfcCollectionControllerTests.cs:66`; `QfcCollectionControllerDefects468MoveTests.cs:17`, `:29`,
    `:57`, `:64` and `QfcCollectionControllerDefects468MoveTests.cs:463`.
    `QfcCollectionControllerDefects468MoveTests.cs:17` is a class-level summary enumerating
    "issue #469 defects 1," and continuing "2, 3 and 4" on `:18`; it lists all four numbers and is
    therefore invariant under a 1-for-2 swap. The other eight are defect-3 citations except `:463`,
    which is defect 4. All nine already agree with `issue.md` numbering and are untouched by this
    plan. This confirms
    the swap is confined to defects 1 and 2 and does not invalidate a defect-3 or defect-4 citation.
19. Line-length invariance of the eight edits — re-derived by reading each line: every one of the
    eight is a single-character digit substitution, so no line changes length and neither file
    changes line count. This is what makes the exact numstat figures in P3-T11 (2/2 and 6/6)
    derivable rather than guessed.
20. `QuickFiler/Controllers/QfcCollectionController.cs` line count — re-derived by end-of-file read:
    last content line is 2446. The executor uses this current-tree value rather than the older
    preparation citation.
21. `QuickFiler/Controllers/QfcCollectionController.cs:21` — re-derived: carries
    `[ExcludeFromCodeCoverage]` immediately above the class declaration at `:22`. No coverage
    increase is claimed for this class anywhere in the plan.
22. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` line count —
    re-derived by end-of-file read: last content line is 497, not the 498 stated in `spec.md` line
    188 and research section 7. The plan gates at "at most 497" and records the discrepancy; the
    invariant the spec intends (the file must not grow past 500) holds under either figure.
23. `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` line count — re-derived by
    end-of-file read: last content line is 499. Nothing is added to it.
24. `QuickFiler/Controllers/QfcHomeController.Metrics.cs` line count — re-derived: 215 lines.
    Research section 7 recorded this as "232, approximate, unverified"; 215 is the measured value.
25. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` line count — re-derived by
    end-of-file read: last content line is 453.
26. `[TestMethod]` counts — re-derived: 9 in `QfcCollectionControllerDefects468MoveTests.cs` and 11
    in `QfcHomeControllerMetricsTests.cs`. These are the P5-T6 invariance targets.
27. `QuickFiler/Interfaces/IQfcCollectionController.cs` — re-derived: `StackMovedItems` occurs at
    `:54` (an XML `param` name) and `:63` (the `MoveEmailsAsync` parameter declaration), in
    PascalCase. The camelCase form `stackMovedItems` does not occur in that file, which confirms the
    spec AC12 casing note and makes the P5-T2 token satisfiable as written.
28. `QuickFiler/Interfaces/IQfcCollectionController.cs:122-129` — re-derived: `GetMoveDiagnostics` is
    declared across those lines with no XML documentation comment above it, which is the factual
    basis for the R1 and R2 replacement text.
29. `docs/features/active/quickfiler-home-controller-metrics-442/spec.md:869` — re-derived: the
    CFN-2 heading reads
    `### CFN-2 — `GetMoveDiagnostics` returns an array one element longer than it fills (feature 468)`.
    `CFN-2` occurs nine times in that file (lines 130, 147, 300, 591, 835, 869, 927, 940, 953) and
    `RESOLVED` occurs zero times, so `CFN-2 RESOLVED` is false-before / true-after.
30. Repository-wide unsatisfiability check — re-derived: `one element longer` occurs in
    `docs/features/active/quickfiler-home-controller-metrics-442/spec.md:869` and in this feature's
    own documents, and `Issue #469 defect` occurs throughout `docs/features/**`. A repository-wide
    zero-hit gate on either would be unsatisfiable; none is authored.
31. `QuickFiler.Test/QuickFiler.Test.csproj:137` and `:157` — re-derived in this pass: the two edited
    test files already carry `Compile Include` entries, at `:137` for
    `Controllers\QfcCollectionControllerDefects468MoveTests.cs` and `:157` for
    `Controllers\QfcHomeControllerMetricsTests.cs`. Version 0.3 cited `:135` and `:155`, which were
    correct before the base merge; the merge of `origin/main` added
    `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` at `:116`, shifting both
    entries down by one. A later source insertion moved both current entries down once more to lines
    137 and 157. No new file is created by this plan, so no csproj edit is required and none is
    planned.
32. `dotnet-tools.json` at the repository root — re-derived: pins `csharpier` to `1.2.6` with
    `rollForward: false`. There is no `.config/dotnet-tools.json`; the root-level manifest is the
    one `dotnet tool restore` resolves.
33. `global.json` — re-derived: requires SDK `8.0.205` with `rollForward: latestFeature` and search
    paths `.dotnet-sdk` then the host. `.dotnet-sdk` is absent from this worktree, which is why
    P0-T7 probes and conditionally runs `scripts/vscode/Install-RepoDotNetSdk.ps1`.
34. `.gitignore:26`, `:27`, `:39`, `:144` — re-derived by reading the file: `[Bb]in/`, `[Oo]bj/`,
    `[Tt]est[Rr]esult*/` and `coverage/*` are all ignored. A first pass of this self-review searched
    for the literal `TestResults` and wrongly concluded that TRX output was untracked-and-visible;
    the actual entry at `:39` is the bracketed-character-class form `[Tt]est[Rr]esult*/`, which does
    match `TestResults/`. Every vstest run in this plan therefore directs `/ResultsDirectory` to a
    per-task subdirectory of `TestResults`, which is both explicit and ignored, and the raw Cobertura
    file stays under `coverage/`.
35. `.csharpierignore` — re-derived: excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`,
    `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`. Evidence artifacts written by this
    plan are therefore outside the CSharpier check, and `packages.config` is not excluded but is
    also not touched.
36. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:341` and
    `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:459-491` — re-derived:
    `Assert-CoberturaLineCoverageThreshold` throws when solution-wide line coverage is below 80
    percent, and it runs before `Set-Content` writes the post-processed XML. P0-T14 and P6-T5 record
    that behavior so a pre-existing sub-threshold repository state is not misattributed to this
    change.
37. `scripts/vscode/Invoke-Restore.ps1:36` — re-derived: runs `vswhere`-resolved MSBuild with
    `/t:Restore /p:RestorePackagesConfig=true`, which is the correct restore for this
    all-`packages.config` solution, and it does not rewrite any csproj.
38. `packages` directory at the repository root — absent at the Codex conversion baseline. P0-T9
    probes the execution-time state and restores packages when required before the first build.
39. `QuickFiler.Test/bin/Debug/` — absent at the Codex conversion baseline. P0-T13 runs after P0-T11
    and P0-T12 have produced the execution-time assembly; task ordering does not assume the baseline
    remains unchanged.
40. Codex policy surfaces — re-derived from `AGENTS.md`,
    `.agents/skills/policy-compliance-order/SKILL.md`, and `.agents/skills/csharp/SKILL.md`. Phase 0
    records five ordered policy entries backed by the two distinct required files.
41. All five gated file line counts — re-derived in this revision pass by counting every physical
    line of each file: 2446 for `QuickFiler/Controllers/QfcCollectionController.cs`, 215 for
    `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, 497 for
    `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`, 453 for
    `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, 499 for
    `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`. P0-T6, P0-T15, P2-T1, P5-T4 and
    the source-limit notes use the same current-tree values.
42. Changed-line arithmetic in P5-T5 — re-derived from the per-file numstat figures the plan itself
    fixes: 3 + 3 + 2 + 6 = 14 added and 14 deleted, so the diff-line total is 28. Version 0.2 stated
    both 20 and 28 in one sentence; 28 is the derivable value and is now the only figure stated.
    P6-T8 already cited 28 and is therefore consistent without further edit.
43. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:290`, `:327` and
    `:369` — re-derived: the three method declarations in the `GetMoveDiagnostics_With` group are
    `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine`,
    `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` and
    `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing`, in that order.
    Two carry the defect-2 citations and the third is the null-guard test, which is the factual
    basis for the AC6 clause reading recorded by P3-T10.
44. `scripts/vscode/TaskMaster.cli.runsettings` — re-derived: a case-insensitive search for
    `DataCollector` and for `coverage` returns zero matches, so the runsettings file declares no
    coverage data collector and nothing collects coverage implicitly on the vstest runs in P6-T6 and
    P6-T7. This is the factual basis for the AC10 realisation mapping recorded by P6-T9.
45. `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/spec.md:304-316` —
    re-derived: each acceptance criterion line has the exact form `- [ ] AC<N> — `, with a space,
    an em dash, and a space after the criterion number. `- [x] AC1` alone is a prefix of
    `- [x] AC10` through `- [x] AC13`, so P7-T1 through P7-T13 now assert the em-dash form. AC1 is
    at `:304` and AC13 at `:316`.
46. `QuickFiler/Interfaces/IQfcCollectionController.cs` — re-derived in this pass: `StackMovedItems`
    occurs exactly twice, at `:54` in an XML `param name` attribute and at `:63` in the
    `MoveEmailsAsync` parameter declaration, both PascalCase. A search for the camelCase substring
    returns only those two PascalCase hits, confirming the camelCase form is absent and that a
    case-sensitive gate on it would be unsatisfiable. The P5-T2 casing note now describes the
    `-CaseSensitive` command it actually runs.
47. Feature-folder contents — re-derived with `rg --files`: the folder holds exactly four files, `issue.md`,
    `spec.md`, this plan, and `research/2026-08-29T12-31-qfc-collection-move-diagnostics-defects-469.md`.
    There is no `evidence/` subtree yet and no `user-story.md`. `issue.md` and the research document
    were added by earlier commits on this branch, so both appear in every `origin/main`-anchored
    diff; P7-T16 excludes them from its exact enumeration for that reason and enumerates five source
    and document paths rather than seven.
48. Pathspec scoping of the Phase 7 git gates — re-derived from the branch history and issue scope.
    P7-T16 and P7-T17 carry the same `-- QuickFiler QuickFiler.Test docs` pathspec that P5-T1 uses,
    so the issue-deliverable gates do not attribute preparation-history paths to this plan. The later
    feature review remains responsible for the complete branch diff.
49. Reachability of the P7-T17 end state — re-derived from the progress-commit contract. P7-T14
    prepares but does not commit the final candidate, and P7-T17 writes its own artifact and task
    check-off before returning the final progress-commit signal. The orchestrator can therefore
    commit the complete task-36-through-task-69 interval and record one interval SHA.
50. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:171-174` and
    `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:397-403` — sibling-region re-check
    in this pass: the R1 target is exactly the three comment lines `:171-173`, with the filter
    statement at `:174` untouched; the R2 target is exactly the three doc-comment lines `:398-400`,
    with `/// <summary>` at `:397`, `/// </summary>` at `:401`, `[TestMethod]` at `:402` and the
    method declaration at `:403` untouched. The 3/3 and 3/3 numstat figures that P2-T6 and the new
    P6-T1 post-format re-check both assert follow from those two regions being equal in length to
    their replacements.
51. Sibling re-check of P6-T9 against the revised P6-T2 — re-derived from the plan text: P6-T9
    declares the clean pass over P6-T1 through P6-T7, and the revised P6-T2 can pass while recording
    a non-zero exit code. P6-T9 now states that "no failure" means each task's own acceptance
    condition held rather than that every exit code was 0, which is the same baseline-relative
    reading P6-T3 and P6-T4 already carried. Without that clarification the two tasks would have
    contradicted each other on the pre-existing-drift branch.
52. Branch base — re-derived at Codex conversion. `HEAD` is
    `30d2aeb298c9b2689dc69b38a5c733512c6e22f5`; the cached `origin/main` is
    `6191c74f3be6e37ecd82816902df9c3832bfc9af`; and their merge base is
    `69aa28dd1154684b622904b9958ecaa2c6aa17d0`. The branch is three commits behind and thirteen
    commits ahead of that cached base. The three upstream commits do not modify the five gated source
    files and retain the two test-project compile entries at lines 137 and 157. P0-T6 fetches and
    merges the execution-time `origin/main`, then re-establishes ancestry and line-count invariants;
    the conversion snapshot is provenance rather than an execution-time assumption.
53. P0-T15's eight-token bullet, table source correction — re-derived in this pass. The bullet
    previously sourced its eight tokens from the R3 table, whose third column is headed `Becomes`
    and states post-edit text. Every R3 token was measured at count 0 in its named file at branch
    head — the two `QfcCollectionController.cs` tokens
    (`Issue #469 defect 2: exactly one diagnostics line`,
    `Issue #469 defect 1: the null test must dominate`) and the six
    `QfcCollectionControllerDefects468MoveTests.cs` tokens
    (`Issue #469 defect 2. Regression test proving that the diagnostics array`,
    `issue #469 defect 2 requires one diagnostics line per cached move`,
    `Issue #469 defect 2. Regression test proving the off-by-one`,
    `issue #469 defect 2 requires exactly one diagnostics line per cached`,
    `Issue #469 defect 1. Regression test proving that the item-controller null guard`,
    `issue #469 defect 1 requires the null guard to run before the first`) — so a P0-T15 acceptance
    demanding count 1 for them was unsatisfiable and would have halted the executor in Phase 0. The
    bullet now names the eight tokens from the `Currently reads` column of the "Defect-numbering
    inversion — exactly 8 sites" table. Each of those eight was measured at count 1 in its named
    file in this pass: `Issue #469 defect 1: exactly one diagnostics line` and
    `Issue #469 defect 2: the null test must dominate` in
    `QuickFiler/Controllers/QfcCollectionController.cs`;
    `Issue #469 defect 1. Regression test proving that the diagnostics array`,
    `issue #469 defect 1 requires one diagnostics line per cached move`,
    `Issue #469 defect 1. Regression test proving the off-by-one`,
    `issue #469 defect 1 requires exactly one diagnostics line per cached`,
    `Issue #469 defect 2. Regression test proving that the item-controller null guard` and
    `issue #469 defect 2 requires the null guard to run before the first` in
    `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`. No one of the eight
    is a substring of another, so the eight counts are independent. The P0-T15 command block gained
    eight `Select-String` lines producing exactly these counts, so every count the acceptance
    asserts is now produced by a command in the block; the block's other assertions
    (rev-parse/merge-base, `one element longer`, `IQfcCollectionController`, `.Where(`, the five
    line counts, `CFN-2 RESOLVED`) already had producing commands and are unchanged. The `$m`
    variable introduced by those eight lines is local to the P0-T15 block and does not collide with
    `$f` in P3-T10 or `$s` in P4-T3.
54. Absolute-path exposure in committed evidence — re-derived from the plan text in this pass.
    The final progress commit stages everything under `FEATURE/evidence/`, so every artifact this plan
    writes is published. Six acceptance conditions require verbatim tool output that can carry an absolute
    path: P0-T10 and P6-T2 enumerate unformatted files, and the non-zero branches of P0-T11,
    P0-T12, P6-T3 and P6-T4 enumerate MSBuild diagnostics, which carry absolute paths. The evidence
    location rule constrained only where artifacts are written, and the repository hook checks the
    directory rather than the recorded text, so nothing kept an account name, machine name or drive
    letter out of a committed artifact. The rule now requires repository-relative recorded paths and
    names those six tasks explicitly. Counts, exit codes and quoted summary lines remain verbatim,
    so no acceptance condition in those six tasks loses its observable value.
