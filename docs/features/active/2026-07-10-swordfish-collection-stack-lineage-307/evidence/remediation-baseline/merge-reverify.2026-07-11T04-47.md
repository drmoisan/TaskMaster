# Remediation Reverify — Merge Integration Tip 618954b8 (#307)

- Entry-ts: 2026-07-11T04-47
- Generated: 2026-07-11T04-58Z
- Feature branch: feature/swordfish-collection-stack-lineage
- Feature tip (pre-merge): 78684e65bcda53292f3e3dc5958d784f98322fd9
- Integration base merged: origin/epic/swordfish-removal-integration @ 618954b855a09235ed8d698eda3ac1720d2f3ddb
- Merge-base: 0b72b11bb1145dd00f70fe9de8d7a6ed3bef79bb
- Scope: reverify the C# toolchain after resolving the two integration-time merge
  conflicts (IToDoObjects.cs union retype; UtilitiesCS.csproj dual compile-entry deletion).
  No feature-behavior change.

## Conflict resolution summary

- `UtilitiesCS/Interfaces/IGlobals/IToDoObjects.cs` — union: `PrefixList` / `LoadPrefixList`
  typed `ConcurrentObservableCollection<IPrefix>` (F2 #307), `FilteredFolderScraping` /
  `FolderRemap` typed `ScoDictionaryNew<...>` (F1 #306). All conflict markers removed.
- `UtilitiesCS/UtilitiesCS.csproj` — both `ScoSortedDictionary.cs` (#309) and `ScoStack.cs`
  (#307) `<Compile Include>` entries removed; `SCODictionary.cs` now directly precedes
  `SerializableList.cs`. All conflict markers removed.
- `git diff --name-only --diff-filter=U` after staging both resolutions: empty (0 unmerged).
- No conflict markers remain in any tracked source file (`*.cs`, `*.csproj`, `*.props`,
  `*.targets`, `*.sln`, `*.json`, `*.config`).

## Step a — CSharpier format + check (P2-T1)

- Timestamp: 2026-07-11T04-54Z
- Command: `csharpier format .` then `csharpier check .` (CSharpier 1.3.0 v1 subcommand
  syntax; the plan's v0 `csharpier .` / `csharpier --check .` forms are not valid on the
  installed v1 tool — the v1 `format`/`check` subcommands are the mechanical equivalent and
  match the Phase 0 baseline command `csharpier check .`).
- EXIT_CODE: 0 (format) / 0 (check)
- Output Summary: Formatted 1378 files in 1176ms; Checked 1378 files in 2940ms; 0 files
  require formatting. `git diff` (unstaged) empty afterward — csharpier introduced no changes
  post-merge, so no toolchain-loop restart was required.

## Step b — MSBuild analyzer build (P2-T2)

- Timestamp: 2026-07-11T04-55Z
- Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` (VS18 MSBuild 18.7.x; dash-switch form + MSYS_NO_PATHCONV=1 under git-bash)
- EXIT_CODE: 0
- Output Summary: Build succeeded. 0 Error(s), 74 Warning(s). Warnings are pre-existing
  (CS8632 nullable-annotation-context in test files, CS0067 unused events, MSTEST0032).
  Zero analyzer errors — first-party analyzer gate green.

## Step c — MSBuild nullable TreatWarningsAsErrors build (P2-T3)

- Timestamp: 2026-07-11T04-56Z
- Command: `MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true` (`-t:Rebuild` forces recompilation; an incremental `-t:Build` reports up-to-date and emits no nullable diagnostics, so Rebuild is required to reproduce the diagnostic set)
- EXIT_CODE: 1 (expected — reproduces the vendored-only baseline error set)
- Output Summary: 168 raw error lines (each distinct diagnostic reported twice by the
  solution build). Grouped by project: SVGControl.csproj 68, UtilitiesSwordfish.NET.General.csproj
  100. ZERO first-party error lines (filter for non-Swordfish/non-SVGControl errors returned
  empty). After normalizing to distinct `file(line,col): error CS####` signatures, the merged
  error set is byte-identical to `evidence/baseline/nullable-baseline-errors.txt`
  (`diff` EXIT 0). Nullable diff vs vendored-only baseline: 0 new first-party diagnostics.

## Step d — vstest + coverage (P2-T4)

- Timestamp: 2026-07-11T04-57Z
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /InIsolation /Settings:TaskMaster.runsettings /Logger:trx` (VS18 vstest.console 18.7.x; canonical Phase 0 baseline invocation — `/InIsolation` required for the Moq test assemblies to avoid STTE Setup FileNotFound; `/Settings:TaskMaster.runsettings` enables the Code Coverage DataCollector and applies the repo module excludes that remove the spurious Deedle/FSharp coverage-instrumentation failures. The plan's bare `/EnableCodeCoverage` form is not the reliable repo-standard invocation for these assemblies.)
- EXIT_CODE: 0
- Output Summary: Test Run Successful. Total tests 4667 — Passed 4667 — Failed 0 — Skipped 0.
  Baseline was 4680/4680; the reduction to 4667 reflects sibling-feature test-file deletions
  brought in by the merge (ScoStack / ScoSortedDictionary removal). No pre-existing failing
  set existed at baseline, so the no-regression bar is zero failures — met.
  Coverage headline (Cobertura, converted from `.coverage` via
  `dotnet-coverage merge --output-format cobertura`, includes vendored code):
  line-rate 76.46% (lines-covered 106,331 / lines-valid 139,064). Baseline line-rate was
  76.59% (106,550 / 139,120); the 0.13-point delta is attributable to the merged sibling
  source/test deletions, not a first-party regression.

## Result

All four toolchain steps reproduce the no-regression baseline:
- Format: EXIT 0, clean.
- Analyzers: EXIT 0, 0 errors.
- Nullable: EXIT 1, error set identical to vendored-only baseline (SVGControl + UtilitiesSwordfish), 0 new first-party.
- Tests: EXIT 0, 4667/4667 passed, line 76.46%.
