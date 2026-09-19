# dependabot-fanout-and-ci-failing-nuget-upgrades (Plan)

- **Issue:** #911
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-19T09-44
- **Status:** Awaiting atomic-executor preflight
- **Version:** 1.0
- **Work Mode:** full-bug
- **Acceptance-criteria source:** `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`, section `## Acceptance Criteria`, AC1 through AC26. No other document carries acceptance criteria for this issue.
- **Task Count:** 123 total — P0 24, P1 14, P2 9, P3 10, P4 8, P5 19, P6 7, P7 11, P8 6, P9 15. Counted mechanically from lines matching `^- \[ \] \[P\d+-T\d+\]`; the line count and the unique-ID count are both 123, so no task ID is duplicated and every phase runs `T1..Tn` with no gap.

---

## Execution Environment (binding for every task in this plan)

**Execution worktree.** Every repository-relative path in this plan resolves against
`C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911`. That is the working directory for every
command. It is **not** the session worktree the plan was authored from; a relative path resolved
from the session worktree silently edits a different checkout of the same tracked file. Before
running any other task, P0-T1 records the resolved worktree root and every later task inherits it.

**Branch.** `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`, cut from `origin/main` at
`734112ed2`.

**Authoritative plan file.** The plan of record is
`C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\docs\features\active\2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911\plan.2026-09-19T09-44.md`.
The execution worktree carries a file at the same repository-relative path, but at the merge-base
that file is still the 45-line promotion template committed before planning began. The executor must
be handed the absolute session-worktree path above and must read the plan from it. P0-T23 replaces
the execution-worktree copy with byte-identical content so the branch ships the real plan, and until
that task runs the repository-relative path inside `TaskMaster-wt\dependabot-911` resolves to the
template.

**Diff anchor.** Every diff, merge-base, footprint and scope check in this plan anchors to
`origin/main`, never to bare `main`. Local `main` in these worktrees is hundreds of commits stale, so
a gate anchored to it is unsatisfiable by construction. The three-dot form `PINNED...HEAD` is
**prohibited** as a substitute: when the pinned ref is an ancestor of HEAD it degenerates to the
two-dot diff and inherits the same defect. P0-T3 resolves and records
`MERGE_BASE = git merge-base origin/main HEAD` once; later tasks cite that recorded value.

**Evidence location (non-overridable).** All evidence resolves under
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/<kind>/`
with `<kind>` drawn from `baseline`, `qa-gates`, `regression-testing`, `issue-updates`, `other`.

EVIDENCE_LOCATION_OVERRIDE_REJECTED: evidence/qa replaced with evidence/qa-gates
EVIDENCE_LOCATION_OVERRIDE_REJECTED: evidence/regression replaced with evidence/regression-testing

`spec.md` names `evidence/qa` and `evidence/regression`. Neither is a canonical sub-path under
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`; the canonical substitutions above are
used throughout and are not negotiable by any downstream instruction.

**Artifact schema.** Every command-bearing task writes one artifact carrying, at minimum:
`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. A task expected to return a non-zero exit
additionally carries `ExpectedExitCode:` with the integer it expects. Baseline and final-QC test
artifacts additionally carry numeric coverage headline values.

**Fail-closed evidence rule.** If a required baseline artifact, QA artifact or coverage-comparison
artifact is missing or incomplete, the outcome is BLOCKED or INCOMPLETE, never PASS. A planned
command task must execute its stated command; `EXIT_CODE: SKIPPED` is not a passing outcome. The one
exception in this plan is Phase 8, whose task text explicitly carries an approved deferral branch for
the three criteria that depend on a credential this change may not yet have.

---

## Scope Decisions Recorded by This Plan

1. **`.github/workflows/_pester.yml` is added to the change footprint.** `spec.md` does not list it.
   It must change, because `_pester.yml` hard-codes `Run.Path = 'tests/scripts/vscode'` and
   `CodeCoverage.Path = 'scripts/vscode'` (verified at `.github/workflows/_pester.yml` lines 41 and
   45). Every test file this change creates lives under `tests/scripts/dependencies/`, so without
   this edit the new suite never executes in CI and the `pester` check reports green while measuring
   nothing. P1-T1 amends the spec `## Write Set` to carry the path; P1-T13 makes the edit.
2. **The canonical form of `packages.config` and `app.config` is inline.** Per the orchestrator
   decision and `spec.md` section "Resolved tension". `.csharpierignore` gains the two patterns
   (P1-T2) **before** the one-time normalisation runs (P1-T7), because a normalisation performed
   while the formatter still owns those paths is undone by the next format step and makes AC3
   unsatisfiable. No standalone normaliser component is planned: the renderer lives in
   `scripts/dependencies/PackageGraph.psm1` and the repair pass's own writer emits canonical form by
   construction.
3. **The repair workflow triggers on `workflow_run` in base context.** `pull_request` is unusable (a
   Dependabot-triggered run receives a read-only token and no Actions secrets) and
   `pull_request_target` is rejected on security grounds. This is an assumption of record. AC19 is
   written as an outcome assertion so a wrong mechanism fails visibly on the fixture pull request.
4. **PowerShell batch splitting.** The change introduces 6 production and 7 test PowerShell files
   against a per-batch cap of 3 and 3 (`.claude/rules/powershell.md` section "Change Budget",
   enforced by `.claude/hooks/enforce-powershell-batch-budget.ps1`, `$ProdCap = 3` / `$TestCap = 3`).
   The plan therefore runs four batches, each closed by its own full toolchain pass and its own
   commit. The batch-boundary task at the end of each close-out phase resets the hook's per-session
   state file. That reset is the hook's own documented mechanism for beginning a new batch and is
   authorised here **only** at a declared boundary whose prior batch has already passed its gates and
   landed as a commit. It is not authorised anywhere else in this plan, and raising
   `CLAUDE_POWERSHELL_BUDGET_PROD` or `CLAUDE_POWERSHELL_BUDGET_TEST` is not authorised at all.

   | Batch | Phase | Production PowerShell | Test PowerShell |
   |---|---|---|---|
   | A | 1 | `scripts/dependencies/PackageGraph.psm1` | `tests/scripts/dependencies/PackageGraph.Tests.ps1` |
   | B | 3 | `scripts/dependencies/PackageCompatibility.psm1`, `scripts/vscode/Sync-PackageReferences.ps1` | `tests/scripts/dependencies/PackageCompatibility.Tests.ps1`, `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1` |
   | C | 5 | `scripts/dependencies/AnalyzerItemRepair.psm1`, `scripts/dependencies/ProjectConsistency.psm1` | `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1`, `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` |
   | D | 7 | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1` (extended) |

   Every batch leaves the solution buildable. Batches B and C change no C# compilation input at all,
   which each close-out phase asserts positively with an anchored diff plus a porcelain companion.

5. **Module placement rule, to hold every new file under the 500-line ceiling.** Text parsing,
   structure construction and canonical rendering live in `scripts/dependencies/PackageGraph.psm1`.
   Reconciliation and verification consume those structures and do not re-implement parsing.
   `scripts/dependencies/ProjectConsistency.psm1` is the file most at risk of the ceiling; when a
   size gate reports it over 500 lines the remedy is to move pure parsing or rendering helpers into
   `scripts/dependencies/PackageGraph.psm1`, which is an already-registered production path and
   therefore consumes no additional batch-budget slot and needs no Write Set amendment.
6. **No temporary files in tests, repository-wide, with no approved exceptions.** Every fixture is an
   in-memory string or hashtable; the directory listing the analyzer derivation consumes is supplied
   through an injected delegate per `.claude/rules/powershell.md` section "Design Seams".
7. **Test layout.** `scripts/dependencies/Foo.psm1` maps to `tests/scripts/dependencies/Foo.Tests.ps1`.
   Colocation in the production tree is prohibited.

---

## Measured Tree Facts This Plan Depends On

All measured in `C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911` during plan authoring.

| Fact | Value | Where measured |
|---|---|---|
| `packages.config` manifests | 18 | glob `**/packages.config` |
| `app.config` files | 17 (VBFunctions has none) | glob `*/app.config` |
| `<Analyzer Include>` items | 162 across 17 `.csproj` (SVGControl carries none) | grep `Analyzer Include=` over `*.csproj` |
| Stale Meziantou analyzer sites | 15 files, exactly 1 line each | grep `Meziantou.Analyzer.3.0.203` over `*.csproj` |
| `TaskMaster/TaskMaster.csproj` analyzer item | already `3.0.235`; not part of the 15 | `TaskMaster/TaskMaster.csproj:575` |
| Analyzer families and versions in the items | Meziantou 3.0.203 (stale), Roslynator.Analyzers 5.0.0, AsyncFixer 2.1.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0, SonarAnalyzer.CSharp 10.34.0.3385, MSTest.Analyzers 4.4.0 | grep over `*.csproj` |
| Analyzer item groups per project | not always one — `VBFunctions.Test/VBFunctions.Test.csproj` carries two (lines 263-265 and 287-294) | grep `Analyzer Include=` |
| Sibling element to preserve | `<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />` | `UtilitiesCS/UtilitiesCS.csproj:1317`, inside the item group at 1307-1319 |
| Explanatory comment to preserve | `<!-- Issue #181: analyzer-only references ... -->` | `UtilitiesCS/UtilitiesCS.csproj:1308` |
| `<Error>` guard version | `3.0.235` (correct) | `UtilitiesCS/UtilitiesCS.csproj:1301` |
| `.csharpierignore` | 14 lines; excludes `**/evidence/**`, coverage and trx artifacts, `*.csproj`, `*.props`, `*.targets`; excludes **neither** `packages.config` **nor** `app.config` | `.csharpierignore` |
| Floating NuGet selector | `nuget-version: latest` at `_mstest-coverage.yml:49`, `_build-nullable.yml:33`, `_build-analyzers.yml:33`; all three use `nuget/setup-nuget@v2` | grep over `.github/workflows` |
| `.github/dependabot.yml` | 4 groups, 4 inert `group-by:` keys, `open-pull-requests-limit: 10`, 8 `version-update:semver-major` ignore entries, no Deedle ignore | `.github/dependabot.yml` |
| The 8 major-version ignore names | `Microsoft.Extensions.*`, `Microsoft.Bcl.*`, `System.Text.Json`, `System.Drawing.Common`, `Microsoft.Graph*`, `Apache.Arrow*`, `Microsoft.Data.Analysis`, `Microsoft.ML*` | `.github/dependabot.yml:47-62` |
| `_pester.yml` exists and runs on every pull request | yes; `ci.yml` has six jobs including `pester` | `.github/workflows/_pester.yml`, `.github/workflows/ci.yml:33-35` |
| `_pester.yml` scope | `Run.Path = 'tests/scripts/vscode'`, `CodeCoverage.Path = 'scripts/vscode'`, line gate `< 80` exits 1 | `.github/workflows/_pester.yml:41,45,71` |
| `#903` orphan pair | `ToDoModel.Test/ToDoModel.Test.csproj:93` and `:96` carry `<HintPath>` for `Deedle.3.0.0` and `FSharp.Core.11.0.100`; `ToDoModel.Test/packages.config` declares neither | both files |
| `scripts/vscode/Sync-PackageReferences.ps1` | 159 lines; `$tfmPreference` at 14-19 with `netstandard2.1` at line 18 ranked above `netstandard2.0`; the only script in `scripts/vscode/` with no test file | the file, and glob over `tests/scripts/vscode` |
| Existing cold-restore red control | `evidence/regression-testing/898-cold-restore-red-run.2026-09-19T11-40.md` records `error CS0006` naming `Meziantou.Analyzer.3.0.203` | the artifact |
| `packages/` in the execution worktree | absent; the worktree is cold today, so the AC6 failing state is the current state | glob `packages/*/` |
| `coverage/*` is gitignored | `.gitignore:144` | `.gitignore` |

---

## Gate-Quality Rules Binding on Every Verification Task

This repository has a recorded history of gates passing for reasons unrelated to the property
asserted. The following rules are binding.

1. **Every verification task states its failing condition and that condition is reachable from where
   the check runs.** A task whose failing condition is unreachable is a defect, not a pass.
2. **No "nothing is wrong" check.** An empty result set must never satisfy an acceptance condition.
   Every absence assertion is paired with a positive assertion naming an explicit expected count or
   member set, so that a detector which never fires is distinguishable from a clean tree.
3. **Cold-cache verification is local only.** The build workflows' cache `restore-keys:` prefix
   fallback structurally prevents CI from reaching a cold-cache failure. AC6 is never rooted in CI.
4. **`Invoke-Pester` sets no process exit code by default.** `New-PesterConfiguration` defaults
   `Run.Exit` to `$false`, so a bare `pwsh -Command` Pester run exits 0 whatever the tests do. Every
   Pester command in this plan ends with an explicit
   `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }` placed **after** the count-emitting
   statement, so the counts are printed before the exit.
5. **`pwsh -Command` payloads use outer single quotes and inner double quotes.** The reverse lets the
   calling shell expand `$c`, `$r` and `$(...)` before pwsh parses the script, which produces an
   empty measurement that reads as a tool failure.
6. **`dotnet tool run csharpier format .` prints `Formatted N files in Xms.` and
   `dotnet tool run csharpier check .` prints `Checked N files in Xms.` — in both cases `N` is the
   scanned count, not a rewrite count.** A restart-on-rewrite rule therefore defines "rewritten" as
   the number of target files whose `Get-FileHash -Algorithm SHA256` differs between a capture taken
   immediately before and immediately after the invocation. `Formatted N files` must never be used
   as that count.
7. **MSBuild non-vacuity is asserted on the echoed compiler command line, not on `Task "Csc"`.**
   MSBuild echoes the full `csc.exe` command line under each project's `CoreCompile` heading at
   normal verbosity, and that line carries `/out:obj\Debug\<Assembly>.dll`. `Task "Csc"` is a
   detailed-verbosity event and can never be attributed to a named project on one line.
8. **Every diff gate is anchored to a ref and is paired with a staging or porcelain companion.** An
   anchored `git diff --name-only` enumerates tracked changes only and can never report a file a task
   creates; `git status --porcelain --untracked-files=all` goes empty once the change is committed.
   The two are complementary and each alone is wrong in one state.
9. **Phase 0 porcelain is non-empty by construction.** Never assert an empty
   `git status --porcelain` in Phase 0. Assert a type condition instead: no `.cs`, `.csproj`,
   `.sln`, `packages.config` or `app.config` path appears among the untracked or modified entries.

---

## Command Reference

Referenced by task text. Each block is stated once here and cited by name rather than repeated.

**CMD-CSHARPIER-CHECK**

```
dotnet tool run csharpier check .
```

Success-case output line begins `Checked ` and ends `ms.`; exit 0.

**CMD-CSHARPIER-FORMAT**

```
dotnet tool run csharpier format .
```

Success-case output line begins `Formatted ` and ends `ms.`; exit 0 whether or not it rewrote
anything. The rewrite count is the SHA-256 hash-difference count, per gate rule 6.

**CMD-MSBUILD-ANALYZERS**

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\analyzers.msbuild.log;Verbosity=normal"
```

**CMD-MSBUILD-NULLABLE**

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\nullable.msbuild.log;Verbosity=normal"
```

Do not add `/p:Nullable=enable` and do not substitute `/t:Build`. Both are load-bearing omissions
recorded in `CLAUDE.md` section C#1.3. The log file lands under `coverage/`, which `.gitignore:144`
covers, so it never reaches a commit.

**CMD-MSTEST-COVERAGE**

```
pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .
```

`-SearchRoot .` is mandatory; the script's single-search-root defect otherwise discovers assemblies
from a sibling worktree. The script always appends
`/TestCaseFilter:TestCategory!=LiveOutlook`, so every figure it produces excludes that category.

**CMD-PESTER-ALL**

```
pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies","tests/scripts/vscode"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "<OUTPATH>"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

`<OUTPATH>` is replaced per task with the JaCoCo path that task names. Per-file line coverage is read
from the JaCoCo document by selecting the `sourcefile` element whose `name` attribute equals the
module file name and reading its `counter` child with `type="LINE"`; the percentage is
`covered / (covered + missed) * 100`. Pester emits no branch counter in any output format, so no
branch-coverage figure is available for PowerShell and none is demanded.

**CMD-POSHQC-FORMAT**, **CMD-POSHQC-ANALYZE**

MCP tools `mcp__drm-copilot__run_poshqc_format` and `mcp__drm-copilot__run_poshqc_analyze`, each
invoked with `scan_folders` supplied **explicitly** as
`["scripts/dependencies","scripts/vscode","tests/scripts/dependencies","tests/scripts/vscode"]`.
The tool resolves its scan set from `config/poshqc-scan.json`, which does not exist in this
repository, so an omitted `scan_folders` measures nothing. Acceptance additionally requires
`MCP Result: ok:true`; an `ok:false` run is a failure even when the paired direct run is green.

**CMD-ACTIONLINT**

```
pwsh -NoProfile -File .\scripts\dev-tools\run-actionlint.ps1
```

The script resolves `actionlint-bin\actionlint.exe` relative to the repository root and throws when
it is absent, so an absent binary is a task failure rather than a silent pass.

---

### Phase 0 — Baseline Capture, Worktree Anchoring and Policy Reads

- [ ] [P0-T1] Resolve and record the execution worktree: run `git rev-parse --show-toplevel`, `git rev-parse --abbrev-ref HEAD` and `git rev-parse HEAD` from `C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911`, and write `evidence/baseline/p0-t1-worktree-anchor.2026-09-19T09-44.md`. Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`; the recorded toplevel ends with `TaskMaster-wt\dependabot-911`; the recorded branch is exactly `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`. Fails when the toplevel names any other worktree, which is reachable because the executor's ambient working directory is a different checkout by default.

- [ ] [P0-T2] Read the policy documents in the order fixed by `.claude/skills/policy-compliance-order/SKILL.md` — `CLAUDE.md`, then `.claude/rules/general-code-change.md`, then `.claude/rules/general-unit-test.md`, then `.claude/rules/powershell.md`, then `.claude/rules/csharp.md`, then `.claude/rules/quality-tiers.md`, then `.claude/rules/tonality.md` — and write `evidence/baseline/phase0-instructions-read.2026-09-19T09-44.md` carrying `Timestamp:`, `Policy Order:` and the explicit list of files read with each file's line count. Acceptance: seven files are listed, in that order, each with a non-zero line count.

- [ ] [P0-T3] Pin the diff anchor: run `git fetch origin main`, then `git merge-base origin/main HEAD`, then `git merge-base --is-ancestor <MERGE_BASE> origin/main`, and record all three in `evidence/baseline/p0-t3-diff-anchor.2026-09-19T09-44.md`. Acceptance: the artifact records a 40-character `MERGE_BASE`, the ancestor check returns `EXIT_CODE: 0`, and the artifact states in prose that every later diff gate cites this value and that the three-dot `PINNED...HEAD` form is prohibited. Fails when `origin/main` is unfetchable or when the recorded merge-base is not an ancestor of `origin/main`.

- [ ] [P0-T4] Record the PowerShell batch-budget state: list `.claude/state/` and record whether a `powershell-batch-budget.*.json` file exists for the current session, together with its `prodFiles` and `testFiles` arrays when present, into `evidence/baseline/p0-t4-batch-budget-state.2026-09-19T09-44.md`. Acceptance: the artifact names the exact state-file path the hook will use and records the starting production and test slot counts as integers. Fails when the artifact records no integer counts.

- [ ] [P0-T5] Provision the repository-pinned .NET SDK by running `pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1`, then record `dotnet --version` and `dotnet --list-sdks` into `evidence/baseline/p0-t5-sdk-bootstrap.2026-09-19T09-44.md`. Acceptance: `dotnet --version` prints the version `global.json` pins and `dotnet --list-sdks` includes a path ending `.dotnet-sdk\sdk`. Fails when `dotnet --version` prints the `global.json` `errorMessage` instead of a version, which is the state of a fresh worktree.

- [ ] [P0-T6] Run `dotnet tool restore` at the worktree root and record the result in `evidence/baseline/p0-t6-tool-restore.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0` and the `Output Summary:` names CSharpier at the version `.config/dotnet-tools.json` pins. Fails when the manifest cannot be restored, which leaves every later CSharpier command unrunnable.

- [ ] [P0-T7] Restore NuGet packages by running `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1` and record the result in `evidence/baseline/p0-t7-package-restore.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`, and the artifact records the count of directories under `packages/` as an integer greater than 100. Fails when `packages/` remains absent, which is the current state of this worktree.

- [ ] [P0-T8] Provision the `dotnet-coverage` global tool with the guarded form `if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }` and record the resolved command path in `evidence/baseline/p0-t8-dotnet-coverage.2026-09-19T09-44.md`. Acceptance: `Get-Command dotnet-coverage` resolves to a path. Fails when it does not, because `scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws before running anything when the tool is absent, and no coverage figure would ever be recorded.

- [ ] [P0-T9] Provision Pester 5.6.1 with `Install-Module Pester -RequiredVersion 5.6.1 -Force -SkipPublisherCheck -Scope CurrentUser`, then record `Get-Module Pester -ListAvailable | Select-Object Name,Version` into `evidence/baseline/p0-t9-pester-provision.2026-09-19T09-44.md`. Acceptance: the recorded list contains the exact version `5.6.1`. Fails when only the legacy 3.4.0 module shipped with Windows PowerShell is present, which has no `New-PesterConfiguration` and no JaCoCo output format.

- [ ] [P0-T10] Record the cold-cache precondition census into `evidence/baseline/p0-t10-cold-state-census.2026-09-19T09-44.md`: the existence of `packages/Meziantou.Analyzer.3.0.235` and the non-existence of `packages/Meziantou.Analyzer.3.0.203`, each recorded as an explicit boolean, plus the full sorted list of directory names under `packages/` matching `Meziantou.Analyzer.*`. Acceptance: the `3.0.235` directory exists, the `3.0.203` directory does not, and the recorded match list has exactly one member. The positive member-count assertion is the non-vacuity guard: a census that enumerated nothing would also report the `3.0.203` directory absent.

- [ ] [P0-T11] [expect-fail] Capture the AC6 failing direction on the merge-base tree: run CMD-MSBUILD-ANALYZERS and write `evidence/baseline/p0-t11-ac6-cold-analyzer-build-red.2026-09-19T09-44.md` carrying `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1`, `Output Summary:`, and the verbatim diagnostic lines. Acceptance: `EXIT_CODE:` is non-zero **and** the captured log carries at least one line containing both `CS0006` and `Meziantou.Analyzer.3.0.203`, and the artifact records the count of such lines as an integer greater than zero. The failing condition is reachable and already measured once in `evidence/regression-testing/898-cold-restore-red-run.2026-09-19T11-40.md`. If the build instead exits 0, the task is **not** complete: record the observation, do not tick AC6, and report `AC6 BASELINE NOT REPRODUCED` for planner re-scope rather than waiving the criterion.

- [ ] [P0-T12] Capture the nullable-build baseline by running CMD-MSBUILD-NULLABLE and writing `evidence/baseline/p0-t12-nullable-build.2026-09-19T09-44.md` with `EXIT_CODE:` recorded as returned and `ExpectedExitCode: 1`. Acceptance: the artifact exists with all four schema fields and the `Output Summary:` names the first error text verbatim. This baseline is expected red for the same cause as P0-T11; no exit-0 demand is placed on it, because a red baseline would otherwise make a sibling exit-0 demand unsatisfiable.

- [ ] [P0-T13] Capture the formatter baseline by running CMD-CSHARPIER-CHECK and writing `evidence/baseline/p0-t13-csharpier-check.2026-09-19T09-44.md`. Acceptance: the artifact records `EXIT_CODE:` as returned plus the verbatim `Checked N files in Xms.` line with `N` recorded as an integer, and the full list of any files reported with findings. Fails when no `Checked ` line is present, which would mean the command did not run.

- [ ] [P0-T14] Capture the C# test baseline by running CMD-MSTEST-COVERAGE and writing `evidence/baseline/p0-t14-mstest-coverage.2026-09-19T09-44.md`. Acceptance: the artifact records `EXIT_CODE:` as returned and an `Output Summary:` that either carries the numeric line-coverage and branch-coverage percentages the runner printed, or, when the run could not produce them, names the blocking diagnostic verbatim and states `coverage unmeasurable at merge-base; cause: <diagnostic>`. Because the analyzer build is red at merge-base for defect #898, the numeric C# coverage baseline used for the no-regression comparison is captured instead at P2-T7, which is the first point in the plan at which the solution compiles; the artifact must name P2-T7 as its numeric successor.

- [ ] [P0-T15] Capture the PowerShell formatter baseline: record `Get-FileHash -Algorithm SHA256` for every `.ps1`, `.psm1` and `.psd1` file under `scripts/vscode` and `tests/scripts/vscode`, run CMD-POSHQC-FORMAT, re-record the hashes, and write both sets plus the hash-difference count to `evidence/baseline/p0-t15-poshqc-format.2026-09-19T09-44.md`. Acceptance: the artifact carries both hash sets, an integer rewrite count derived from the hash difference, `MCP Result: ok:true`, and the verbatim `git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode` output taken immediately after the run. That recorded diff is the authoritative list of pre-existing formatting drift and is the only set later tasks may exclude from a changed-line audit.

- [ ] [P0-T16] Capture the PowerShell analyzer baseline by running CMD-POSHQC-ANALYZE and writing `evidence/baseline/p0-t16-poshqc-analyze.2026-09-19T09-44.md`. Acceptance: the artifact records `MCP Result: ok:true`, the integer finding count, and the full finding list when the count is non-zero. Fails when `scan_folders` was not supplied explicitly, which the artifact must show by quoting the exact argument value passed.

- [ ] [P0-T17] Capture the Pester baseline by running CMD-PESTER-ALL with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t17-pester-coverage.2026-09-19T09-44.xml` and writing `evidence/baseline/p0-t17-pester.2026-09-19T09-44.md`. Acceptance: the artifact records the verbatim `PESTER Passed=... Failed=... Skipped=... Total=...` line with `Total` greater than zero, the aggregate JaCoCo LINE percentage as a number with two decimals, and an explicit note that `tests/scripts/dependencies` currently contains no test file so its contribution is zero. Fails when `Total=0`, which would mean the run discovered nothing.

- [ ] [P0-T18] Record the analyzer-item census into `evidence/baseline/p0-t18-analyzer-census.2026-09-19T09-44.md`: the total count of lines matching `Analyzer Include=` across `*.csproj`, the per-file breakdown, the count of files matching `Meziantou.Analyzer.3.0.203` and the per-file match count for each. Acceptance: the total is exactly 162 across exactly 17 files; the stale count is exactly 15 files with exactly 1 match each; and the artifact records that `TaskMaster/TaskMaster.csproj:575` already names `3.0.235` and is therefore not one of the 15. Fails when any of the three counts differs, which would mean the tree moved since plan authoring and the #898 edit set must be re-derived before P1-T9 runs.

- [ ] [P0-T19] Record the manifest census into `evidence/baseline/p0-t19-manifest-census.2026-09-19T09-44.md`: the count of `**/packages.config` files, the count of `*/app.config` files, and, for `ToDoModel.Test/ToDoModel.Test.csproj`, the verbatim `<HintPath>` lines naming `Deedle` and `FSharp.Core` with their line numbers, paired with the count of matches for `Deedle` and `FSharp.Core` in `ToDoModel.Test/packages.config`. Acceptance: 18 manifests, 17 `app.config` files, exactly 2 orphan `<HintPath>` lines recorded with their line numbers, and exactly 0 manifest matches. The paired positive count on the project file is the non-vacuity guard for the zero on the manifest.

- [ ] [P0-T20] Record the formatting-scope and NuGet-selector census into `evidence/baseline/p0-t20-format-and-nuget-census.2026-09-19T09-44.md`: the full verbatim contents of `.csharpierignore` with its line count, the count of lines in `.csharpierignore` matching `packages.config` or `app.config`, and every `.github/workflows/*.yml` line matching `nuget-version` or `setup-nuget` with file and line number. Acceptance: the `.csharpierignore` match count is exactly 0; exactly 3 `nuget-version: latest` lines are recorded, at `_mstest-coverage.yml:49`, `_build-nullable.yml:33` and `_build-analyzers.yml:33`; and exactly 3 `nuget/setup-nuget@v2` step lines are recorded. The three positive counts guard the zero.

- [ ] [P0-T21] Record the Dependabot configuration census into `evidence/baseline/p0-t21-dependabot-census.2026-09-19T09-44.md`: the count of group keys under `groups:`, the count of `group-by:` lines, the value of `open-pull-requests-limit`, the ordered list of `dependency-name` values carrying `version-update:semver-major`, and the count of ignore entries naming `Deedle`. Acceptance: 4 groups, 4 `group-by:` lines, limit `10`, exactly the 8 names `Microsoft.Extensions.*`, `Microsoft.Bcl.*`, `System.Text.Json`, `System.Drawing.Common`, `Microsoft.Graph*`, `Apache.Arrow*`, `Microsoft.Data.Analysis`, `Microsoft.ML*` in file order, and 0 Deedle entries. This recorded 8-member list is the literal expected set that `tests/scripts/dependencies/DependabotConfig.Tests.ps1` declares for AC1.

- [ ] [P0-T22] Record the CI Pester-scope census into `evidence/baseline/p0-t22-pester-scope-census.2026-09-19T09-44.md`: the verbatim `Run.Path` and `CodeCoverage.Path` assignment lines from `.github/workflows/_pester.yml` with their line numbers, and the verbatim job list from `.github/workflows/ci.yml`. Acceptance: `Run.Path` is recorded as `'tests/scripts/vscode'` at line 41, `CodeCoverage.Path` as `'scripts/vscode'` at line 45, and exactly 6 jobs are recorded from `ci.yml` including `pester`. This artifact is the evidence for the Scope Decision 1 amendment made by P1-T1.

- [ ] [P0-T23] Replace the stale plan copy in the execution worktree: overwrite `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md` inside `C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911` with the byte-identical contents of the authoritative copy at `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\docs\features\active\2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911\plan.2026-09-19T09-44.md`, and record both files' SHA-256 hashes in `evidence/other/p0-t23-plan-sync.2026-09-19T09-44.md`. Acceptance: the two recorded hashes are equal; the execution-worktree copy contains exactly 123 lines matching the task pattern and exactly 10 lines beginning `### Phase `; and `git status --porcelain --untracked-files=all -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md` reports the file modified. The stale copy is the 45-line promotion template committed at the merge-base; leaving it in place would ship a template as the branch's plan of record and would mislead any later reader who opens the repo-relative path rather than the session path.

- [ ] [P0-T24] Commit the Phase 0 evidence with an explicit pathspec limited to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/` and record the resulting head SHA in `evidence/baseline/p0-t23-commit.2026-09-19T09-44.md`. Acceptance: `git status --porcelain --untracked-files=all` is captured verbatim and no entry in it matches `*.cs`, `*.csproj`, `*.sln`, `packages.config` or `app.config`; the recorded head SHA differs from the value P0-T1 recorded. An empty porcelain is not asserted here, because Phase 0 artifacts and the generated `coverage/` logs make it non-empty by construction.

### Phase 1 — Batch A: Formatting Scope, Analyzer Realignment, Manifest Completeness and the NuGet Pin

- [ ] [P1-T1] Amend `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md` section `## Write Set`, subsection "Configuration and workflows", to add a backticked entry for `.github/workflows/_pester.yml`, and add one sentence naming the reason recorded in Scope Decision 1 and citing P0-T22 as its evidence. Acceptance: the spec `## Write Set` contains exactly one backticked `.github/workflows/_pester.yml` entry; no acceptance-criterion line is added, removed or reworded; `git diff origin/main -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md` shows changes confined to the Write Set section and its adjacent note. Evidence: `evidence/other/p1-t1-spec-write-set-amendment.2026-09-19T09-44.md`.

- [ ] [P1-T2] Add the two patterns `**/packages.config` and `**/app.config` to `.csharpierignore`, each preceded by a one-line comment giving the reason (the repository adopts the inline form these files are written in by the NuGet CLI, so the formatter no longer owns them). Acceptance: `.csharpierignore` contains a line whose text is exactly `**/packages.config` and a line whose text is exactly `**/app.config`; the file's other 14 lines are unchanged, verified by `git diff origin/main -- .csharpierignore` showing only additions. Evidence: `evidence/qa-gates/p1-t2-csharpierignore.2026-09-19T09-44.md`.

- [ ] [P1-T3] Verify AC2 with a live control in `UtilitiesCS/`: transiently rewrite `UtilitiesCS/packages.config` so every `<package .../>` element sits on one line, transiently rewrite `UtilitiesCS/app.config` so every `<assemblyIdentity .../>` element sits on one line, and transiently perturb `UtilitiesCS/Extensions/EnumExtensions.cs` by inserting four consecutive blank lines inside the type body; then run CMD-CSHARPIER-CHECK and capture the full output; then revert all three files with `git checkout -- UtilitiesCS/packages.config UtilitiesCS/app.config UtilitiesCS/Extensions/EnumExtensions.cs`. Acceptance: the captured output names `UtilitiesCS/Extensions/EnumExtensions.cs` and names neither `UtilitiesCS/packages.config` nor `UtilitiesCS/app.config`; the post-revert `git status --porcelain --untracked-files=all -- UtilitiesCS` is empty. The C# perturbation is the control that proves the check was live; without it a silent no-op run would read as a pass. Evidence: `evidence/qa-gates/p1-t3-ac2-format-scope-control.2026-09-19T09-44.md`. This task checks off **AC2**.

- [ ] [P1-T4] Create `scripts/dependencies/PackageGraph.psm1` providing advanced functions with `CmdletBinding()` for: discovering manifest paths from an injected directory-listing delegate; parsing `packages.config` text into ordered package records; parsing project-file text into the dependent-element records `<Import>`, `<Error>`, `<Reference>`, `<HintPath>` and `<Analyzer Include>`; parsing `app.config` text into binding-redirect records; and rendering the canonical inline form of a manifest and of an `app.config`. Every function is pure over text except the discovery function, whose only I/O is the injected delegate. Acceptance: the module imports without error; `Get-Command -Module PackageGraph` lists every exported function named in the module's own comment-based help; the file is at most 500 lines. Evidence: `evidence/qa-gates/p1-t4-packagegraph-module.2026-09-19T09-44.md`.

- [ ] [P1-T5] Create `tests/scripts/dependencies/PackageGraph.Tests.ps1` covering, with one `It` per behaviour and Arrange-Act-Assert structure: manifest parsing of a reflowed multi-line entry and of an inline entry yielding identical records; rendering a parsed manifest to inline form; rendering being byte-identical when applied twice to its own output; project-file parsing of each of the five dependent element kinds; `app.config` parsing of a binding redirect; and rejection of malformed input with an explicit `throw`. All fixtures are in-memory strings; no temporary file is created. Acceptance: the file is at most 500 lines and contains no call to `New-TemporaryFile`, `[System.IO.Path]::GetTempPath`, `$env:TEMP` or `Out-File`. Evidence: `evidence/qa-gates/p1-t5-packagegraph-tests-authored.2026-09-19T09-44.md`.

- [ ] [P1-T6] Run the PackageGraph suite with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/PackageGraph.Tests.ps1")` and `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p1-t6-packagegraph-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, the recorded `Failed=0`, `Total` greater than or equal to 8, and the JaCoCo `sourcefile` entry named `PackageGraph.psm1` reports a LINE percentage of at least 90. Fails when `Total=0`, which would mean discovery found no test. Evidence: `evidence/qa-gates/p1-t6-packagegraph-run.2026-09-19T09-44.md`.

- [ ] [P1-T7] Run the one-time normalisation over the working tree using the renderer in `scripts/dependencies/PackageGraph.psm1`, rewriting all 18 `packages.config` files and all 17 `app.config` files to canonical inline form, and record the per-file before and after SHA-256 hashes plus the count of files whose hash changed. Acceptance: exactly 35 files are examined, the examined count is emitted by the normaliser itself as an integer, and `git diff --name-only origin/main -- "*/packages.config" "*/app.config"` lists only files drawn from that 35-member set, with `git status --porcelain --untracked-files=all -- "*/packages.config" "*/app.config"` captured in the same task as the companion that observes any path the name-listing diff cannot see. This task must run after P1-T2, because a normalisation performed while the formatter still owns those paths is reverted by the next format step. Evidence: `evidence/qa-gates/p1-t7-normalisation.2026-09-19T09-44.md`.

- [ ] [P1-T8] Verify AC3: re-run the normaliser over the already-normalised tree, then capture `git diff -- "*/packages.config" "*/app.config"` and `git status --porcelain --untracked-files=all -- "*/packages.config" "*/app.config"`. Acceptance: the second run reports having examined exactly 18 `packages.config` files as an integer emitted by the normaliser, the captured `git diff` output is empty, and the captured porcelain output is empty. The examined-count assertion is the non-vacuity guard: a discovery glob that matched nothing would also produce an empty diff. The porcelain span is the companion required because a name-listing diff cannot observe an untracked path. Evidence: `evidence/qa-gates/p1-t8-ac3-normaliser-idempotence.2026-09-19T09-44.md`. This task checks off **AC3**.

- [ ] [P1-T9] Correct issue #898 by rewriting the single stale `<Analyzer Include>` line in each of the 15 project files listed in the spec `## Write Set` subsection "Project files carrying a stranded analyzer item (#898)" so that the package folder segment reads `Meziantou.Analyzer.3.0.235`, leaving the rest of each path — including the `analyzers\dotnet\roslyn5.0\cs` segment — byte-identical. Acceptance: `git diff --numstat origin/main -- "*.csproj"` reports exactly 15 files with exactly 1 added and 1 removed line each; `git status --porcelain --untracked-files=all -- "*.csproj"` lists those same 15 paths. Fails when any file shows a different line count, which would mean an unintended edit. Evidence: `evidence/qa-gates/p1-t9-898-analyzer-realignment.2026-09-19T09-44.md`.

- [ ] [P1-T10] Record the post-#898 analyzer census into `evidence/qa-gates/p1-t10-analyzer-census-post-fix.2026-09-19T09-44.md`: the count of `Analyzer Include=` lines across `*.csproj`, the count of files matching `Meziantou.Analyzer.3.0.203`, and the count of files matching `Meziantou.Analyzer.3.0.235` in an `<Analyzer Include>` line. Acceptance: the total remains exactly 162 across exactly 17 files; the `3.0.203` count is exactly 0; the `3.0.235` analyzer-item count is exactly 16 files, being the 15 corrected plus `TaskMaster/TaskMaster.csproj`. The two positive counts guard the zero.

- [ ] [P1-T11] Correct issue #903 by adding to `ToDoModel.Test/packages.config` the two entries `Deedle` version `3.0.0` and `FSharp.Core` version `11.0.100`, each with `targetFramework="net481"`, placed in the file's existing alphabetical position and written in the canonical inline form P1-T7 established. Acceptance: `git diff --numstat origin/main -- ToDoModel.Test/packages.config` reports added lines and no removed lines beyond those the normalisation already accounted for; a grep of `ToDoModel.Test/packages.config` for `Deedle` returns exactly 1 match and for `FSharp.Core` returns exactly 1 match; the versions match the `<HintPath>` folder segments recorded at `ToDoModel.Test/ToDoModel.Test.csproj:93` and `:96`. Evidence: `evidence/qa-gates/p1-t11-903-manifest-entries.2026-09-19T09-44.md`.

- [ ] [P1-T12] Pin the NuGet CLI to the exact three-part version `7.9.0` at `.github/workflows/_build-analyzers.yml:33`, `.github/workflows/_build-nullable.yml:33` and `.github/workflows/_mstest-coverage.yml:49`, replacing `nuget-version: latest`, and add a one-comment rationale at each site naming the reason (the tool that rewrites `.csproj` and `app.config` during an upgrade must be a known quantity for a given commit, and `7.9.0` is what `latest` resolved to, so the pin freezes current behaviour rather than changing it). Acceptance: the count of lines matching `nuget-version: latest` across `.github/workflows/` is exactly 0 and the count of lines matching `nuget-version: '7.9.0'` is exactly 3. The positive count of 3 guards the zero. Evidence: `evidence/qa-gates/p1-t12-nuget-pin.2026-09-19T09-44.md`.

- [ ] [P1-T13] Extend `.github/workflows/_pester.yml` so `Run.Path` is the two-member array `tests/scripts/dependencies` and `tests/scripts/vscode`, and `CodeCoverage.Path` is the two-member array `scripts/dependencies` and `scripts/vscode`, leaving the 80 percent line gate at line 71 and the artifact upload unchanged. Acceptance: the file's `Run.Path` and `CodeCoverage.Path` assignments each name both members; CMD-ACTIONLINT returns `EXIT_CODE: 0`; and the file contains exactly one `Invoke-Pester` invocation. Fails when either array is left single-valued, which would leave the new suite unexecuted in CI. Evidence: `evidence/qa-gates/p1-t13-pester-workflow-scope.2026-09-19T09-44.md`.

- [ ] [P1-T14] Verify the AC6 passing direction from the same cold state: delete the `packages/` directory, re-run `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1`, confirm `packages/Meziantou.Analyzer.3.0.235` exists and `packages/Meziantou.Analyzer.3.0.203` does not, then run CMD-MSBUILD-ANALYZERS. Acceptance: `EXIT_CODE: 0`; the captured log carries exactly 0 lines containing `CS0006`; and the captured log carries at least 18 lines containing `/out:obj\Debug\`, with the exact count recorded. The `/out:` count is the non-vacuity guard required because a build that compiled nothing would also report zero `CS0006` lines. Evidence: the passing log under `evidence/qa-gates/p1-t14-ac6-cold-analyzer-build-green.2026-09-19T09-44.md`, paired with the failing log P0-T11 wrote under `evidence/baseline/`. This criterion is deliberately local; the CI cache `restore-keys:` prefix fallback prevents CI from reaching the failing state. This task checks off **AC6**.

### Phase 2 — Batch A Close-Out: Toolchain Gates, Commit and Budget Boundary

- [ ] [P2-T1] Run CMD-POSHQC-FORMAT over the four `scan_folders` and record the before and after SHA-256 hash sets for every `.ps1`, `.psm1` and `.psd1` under `scripts/dependencies` and `tests/scripts/dependencies` into `evidence/qa-gates/p2-t1-poshqc-format.2026-09-19T09-44.md`. Acceptance: `MCP Result: ok:true`, both hash sets recorded, and the integer rewrite count recorded as the hash-difference count. When the rewrite count is greater than zero the phase restarts from P2-T1 after the rewritten files are re-read. `Formatted N files` must not be used as the rewrite count.

- [ ] [P2-T2] Run CMD-POSHQC-ANALYZE and record the result in `evidence/qa-gates/p2-t2-poshqc-analyze.2026-09-19T09-44.md`. Acceptance: `MCP Result: ok:true` and the integer finding count is exactly 0, with the artifact quoting the exact `scan_folders` argument value passed so an unscoped run is distinguishable from a clean one.

- [ ] [P2-T3] Run CMD-PESTER-ALL with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p2-t3-pester-coverage.2026-09-19T09-44.xml` and record the result in `evidence/qa-gates/p2-t3-pester.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total` greater than the `Total` P0-T17 recorded, the aggregate JaCoCo LINE percentage recorded to two decimals and at least 85, and the `sourcefile` LINE percentage for `PackageGraph.psm1` recorded and at least 90.

- [ ] [P2-T4] Run CMD-CSHARPIER-CHECK and record the result in `evidence/qa-gates/p2-t4-csharpier-check.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`, the verbatim `Checked N files in Xms.` line recorded with `N` as an integer, and zero files reported with findings. Fails when any normalised `packages.config` or `app.config` is reported, which would mean the `.csharpierignore` patterns added by P1-T2 do not match.

- [ ] [P2-T5] Run CMD-MSBUILD-ANALYZERS and record the result in `evidence/qa-gates/p2-t5-msbuild-analyzers.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`, exactly 0 lines containing `CS0006` in `coverage/analyzers.msbuild.log`, and at least 18 lines containing `/out:obj\Debug\` with the exact count recorded. The `/out:` count is the non-vacuity observation; a warm `/t:Build` that skipped every compile would report zero errors and zero such lines.

- [ ] [P2-T6] Run CMD-MSBUILD-NULLABLE and record the result in `evidence/qa-gates/p2-t6-msbuild-nullable.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0` and at least 18 lines containing `/out:obj\Debug\` in `coverage/nullable.msbuild.log`, with the exact count recorded.

- [ ] [P2-T7] Run CMD-MSTEST-COVERAGE and record the result in `evidence/baseline/p2-t7-mstest-numeric-baseline.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`; the artifact records the numeric line-coverage percentage and the numeric branch-coverage percentage printed by the runner, together with the passed, failed and skipped counts; and the artifact states that it is the numeric C# coverage baseline for the no-regression comparison, superseding the unmeasurable attempt recorded at P0-T14, and names the cause (the merge-base tree did not compile because of defect #898). Fails when either percentage is absent, because the no-regression comparison at P9-T9 reads both.

- [ ] [P2-T8] Commit batch A with explicit pathspecs covering `.csharpierignore`, `scripts/dependencies/PackageGraph.psm1`, `tests/scripts/dependencies/PackageGraph.Tests.ps1`, the 15 `*.csproj` files, `ToDoModel.Test/packages.config`, the 18 `*/packages.config` and 17 `*/app.config` files, `.github/workflows/_build-analyzers.yml`, `.github/workflows/_build-nullable.yml`, `.github/workflows/_mstest-coverage.yml`, `.github/workflows/_pester.yml`, `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, and record the head SHA in `evidence/qa-gates/p2-t8-commit.2026-09-19T09-44.md`. Acceptance: `git status --porcelain --untracked-files=all` is captured verbatim and contains no entry outside `coverage/`; `git show --name-only --format= HEAD` lists only paths from that pathspec set; the head SHA differs from the value P0-T24 recorded.

- [ ] [P2-T9] Close batch A at the budget boundary: record the current contents of `.claude/state/powershell-batch-budget.<session-id>.json`, then delete that file, then confirm it is absent, writing all three observations to `evidence/other/p2-t9-batch-a-boundary.2026-09-19T09-44.md`. Acceptance: the recorded pre-reset `prodFiles` array contains exactly `scripts/dependencies/PackageGraph.psm1` and the `testFiles` array contains exactly `tests/scripts/dependencies/PackageGraph.Tests.ps1`; the post-reset check reports the file absent. Preconditions, both of which the artifact must record as satisfied: P2-T2 through P2-T7 all returned `EXIT_CODE: 0`, and P2-T8 produced a commit. The reset is authorised only at this declared boundary; raising `CLAUDE_POWERSHELL_BUDGET_PROD` or `CLAUDE_POWERSHELL_BUDGET_TEST` is not authorised anywhere in this plan.

### Phase 3 — Batch B: Framework Compatibility, Reference-Sync Rewrite and Dependabot Consolidation

- [ ] [P3-T1] Create `scripts/dependencies/PackageCompatibility.psm1` providing an asset-level compatibility gate for `net481`: it decides from the asset folder names a candidate package actually ships, never from a declared framework attribute; it excludes `netstandard2.1` outright rather than ranking it last, because `net481` cannot consume it at any position; it returns an acceptance record naming the selected asset folder, or a rejection record carrying a non-empty reason string. Acceptance: the module imports without error, exports the selector and the gate as advanced functions with `CmdletBinding()`, contains no literal `netstandard2.1` inside any ordered preference collection, and is at most 500 lines. Evidence: `evidence/qa-gates/p3-t1-packagecompatibility-module.2026-09-19T09-44.md`.

- [ ] [P3-T2] Create `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` with one `It` per case: the selector returns `net481` when `net481` is present; returns `net48` when `net481` is absent; returns `netstandard2.0` when offered `netstandard2.1` and `netstandard2.0` together; returns no selection when offered only `netstandard2.1`; returns no selection when offered only a .NET-Core-era framework; returns no selection for an empty set; the gate returns a rejection carrying a non-empty reason when the asset set contains only frameworks `net481` cannot consume; and the gate returns an acceptance naming the selected asset folder when a consumable asset is present. All fixtures are in-memory arrays. Acceptance: the file contains exactly 8 `It` blocks matching that list, is at most 500 lines, and creates no temporary file. Evidence: `evidence/qa-gates/p3-t2-packagecompatibility-tests-authored.2026-09-19T09-44.md`.

- [ ] [P3-T3] Run the compatibility suite with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/PackageCompatibility.Tests.ps1")` and `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t3-compat-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total=8`, and the two gate cases named in AC9 — the rejection carrying a reason string and the acceptance naming the selected asset folder — are both present in the `Detailed` output by name. Evidence: `evidence/qa-gates/p3-t3-ac9-asset-level-gate.2026-09-19T09-44.md`. This task checks off **AC9**.

- [ ] [P3-T4] Rewrite `scripts/vscode/Sync-PackageReferences.ps1` to import `scripts/dependencies/PackageCompatibility.psm1` and resolve its framework selection through that module, deleting the `$tfmPreference` array currently at lines 14-19, and restructuring the script into advanced functions with `CmdletBinding()` and an injectable filesystem seam so its logic is testable without touching disk. Acceptance: the file contains exactly 0 lines matching `tfmPreference` and exactly 0 lines matching `netstandard2.1`, contains at least one import of `PackageCompatibility.psm1`, is at most 500 lines, and `scripts/vscode/Invoke-VSBuild.ps1` is unchanged, verified by `git diff --name-only origin/main -- scripts/vscode/Invoke-VSBuild.ps1` producing no output alongside a `git status --porcelain --untracked-files=all -- scripts/vscode` capture. The two zero counts are guarded by the positive import assertion. Evidence: `evidence/qa-gates/p3-t4-sync-package-references.2026-09-19T09-44.md`.

- [ ] [P3-T5] Create `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` asserting that the script resolves the same framework selection as the shared module for each of the four selection cases named in AC7, and that the script declares no framework ordering of its own, the latter asserted by exercising the script against an asset set whose correct answer differs from any fixed ordering the deleted array would have produced. All external boundaries are mocked at the wrapper-function seam; no real executable is mocked; no temporary file is created. Acceptance: the file is at most 500 lines and contains at least 5 `It` blocks. Evidence: `evidence/qa-gates/p3-t5-sync-tests-authored.2026-09-19T09-44.md`.

- [ ] [P3-T6] Run both AC7 suites with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/PackageCompatibility.Tests.ps1","tests/scripts/vscode/Sync-PackageReferences.Tests.ps1")` and `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t6-ac7-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total` at least 13, and the `Detailed` output names the passing case "returns no selection when offered only netstandard2.1". That case is what makes a merely-demoted framework fail: a demotion still returns a selection when nothing else is offered. Evidence: `evidence/qa-gates/p3-t6-ac7-framework-exclusion.2026-09-19T09-44.md`. This task checks off **AC7**.

- [ ] [P3-T7] Rewrite `.github/dependabot.yml` to one catch-all group declaring `applies-to: version-updates` and the pattern `"*"`, with `open-pull-requests-limit: 1`, every `group-by:` key removed, an `ignore` entry naming `Deedle` with neither a `versions` nor an `update-types` qualifier, and the 8 pre-existing `version-update:semver-major` ignore entries retained unchanged in the same order P0-T21 recorded. Acceptance: the file declares exactly 1 group key; contains exactly 0 lines matching `group-by`; `open-pull-requests-limit` reads `1`; exactly 1 `Deedle` ignore entry exists with no qualifier keys beneath it; and the ordered list of `dependency-name` values carrying `version-update:semver-major` equals the 8-member list P0-T21 recorded, compared element by element. The three positive assertions guard the `group-by` zero. Evidence: `evidence/qa-gates/p3-t7-dependabot-consolidation.2026-09-19T09-44.md`.

- [ ] [P3-T8] Create `tests/scripts/dependencies/DependabotConfig.Tests.ps1` asserting AC1 against `.github/dependabot.yml` with a text-based deterministic parse that imports no YAML module. No external PowerShell module may be taken as a dependency, because `powershell-yaml` is not guaranteed present on the `windows-latest` runner and an absent module would turn the CI `pester` job red for an unrelated reason. The suite asserts, as separate `It` blocks: exactly one entry under `groups`; that entry declares `applies-to: version-updates` and the catch-all pattern; `open-pull-requests-limit` equals 1; a `Deedle` ignore entry exists with neither a `versions` nor an `update-types` qualifier; and the set of semver-major pairs equals a literal expected set declared in the test, compared element by element. Acceptance: the file is at most 500 lines, contains exactly 0 `Import-Module` statements naming a module outside `scripts/`, and the literal expected set is the 8-member list recorded at P0-T21. Evidence: `evidence/qa-gates/p3-t8-dependabotconfig-tests-authored.2026-09-19T09-44.md`.

- [ ] [P3-T9] Run the AC1 suite with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/DependabotConfig.Tests.ps1")` and `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p3-t9-ac1-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, and `Total` at least 5 with each of the five AC1 assertions named individually in the `Detailed` output. The criterion fails if any ignore entry is dropped, renamed or re-qualified, if a second group is added, or if a `group-by` key is reintroduced anywhere in the file. Evidence: `evidence/qa-gates/p3-t9-ac1-dependabot-consolidated.2026-09-19T09-44.md`. This task checks off **AC1**.

- [ ] [P3-T10] Extend `tests/scripts/dependencies/DependabotConfig.Tests.ps1` with the AC4 assertions — enumerate every step across `.github/workflows/` that uses the setup-nuget action, assert the enumerated count is greater than zero, and assert each such step declares a `nuget-version` whose value is an exact three-part version literal — then run that suite with CMD-PESTER-ALL restricted to that file, and run CMD-ACTIONLINT. Acceptance: the Pester run reports `EXIT_CODE: 0` and `Failed=0`; the enumerated setup-nuget step count is recorded as exactly 3; CMD-ACTIONLINT reports `EXIT_CODE: 0`. The greater-than-zero assertion is what prevents a broken enumerator from passing vacuously. Evidence: `evidence/qa-gates/p3-t10-ac4-nuget-pin.2026-09-19T09-44.md`. This task checks off **AC4**.

### Phase 4 — Batch B Close-Out: Toolchain Gates, Commit and Budget Boundary

- [ ] [P4-T1] Run CMD-POSHQC-FORMAT over the four `scan_folders` with the before and after SHA-256 hash sets recorded into `evidence/qa-gates/p4-t1-poshqc-format.2026-09-19T09-44.md`. Acceptance: `MCP Result: ok:true`, both hash sets recorded, the rewrite count recorded as the hash-difference count. A non-zero rewrite count restarts the phase from P4-T1.

- [ ] [P4-T2] Run CMD-POSHQC-ANALYZE and record the result in `evidence/qa-gates/p4-t2-poshqc-analyze.2026-09-19T09-44.md`. Acceptance: `MCP Result: ok:true`, the integer finding count is exactly 0, and the exact `scan_folders` argument value is quoted.

- [ ] [P4-T3] Run CMD-PESTER-ALL with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p4-t3-pester-coverage.2026-09-19T09-44.xml` and record the result in `evidence/qa-gates/p4-t3-pester.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, the aggregate JaCoCo LINE percentage at least 85, the `sourcefile` LINE percentage for `PackageCompatibility.psm1` at least 90, and the `sourcefile` LINE percentage for `Sync-PackageReferences.ps1` recorded and strictly greater than the value that file reported in the P0-T17 baseline. The strict-increase clause is the no-regression-on-changed-lines gate for that file; the file had no test before this change, so its baseline is measurable and non-trivially improvable.

- [ ] [P4-T4] Run CMD-CSHARPIER-CHECK and record the result in `evidence/qa-gates/p4-t4-csharpier-check.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`, the verbatim `Checked N files in Xms.` line recorded, and zero files reported with findings.

- [ ] [P4-T5] Run CMD-ACTIONLINT and record the result in `evidence/qa-gates/p4-t5-actionlint.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0` and the artifact records the count of files under `.github/workflows/` as an integer greater than or equal to 9, so a run that linted nothing is distinguishable from a clean run.

- [ ] [P4-T6] Assert that batch B changed no C# compilation input, so the green build from P2-T5 and P2-T6 still holds: capture `git diff --name-only <P2-T8-head-sha> -- .` and `git status --porcelain --untracked-files=all` and record both in `evidence/qa-gates/p4-t6-csharp-input-invariance.2026-09-19T09-44.md`. Acceptance: the union of the two captures contains at least 4 paths, and contains exactly 0 paths matching `*.cs`, `*.csproj`, `*.sln`, `packages.config` or `app.config`. The at-least-4 clause is the non-vacuity guard: an empty union would also satisfy the zero.

- [ ] [P4-T7] Commit batch B with explicit pathspecs covering `scripts/dependencies/PackageCompatibility.psm1`, `scripts/vscode/Sync-PackageReferences.ps1`, `tests/scripts/dependencies/PackageCompatibility.Tests.ps1`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1`, `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, `.github/dependabot.yml`, `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, and record the head SHA in `evidence/qa-gates/p4-t7-commit.2026-09-19T09-44.md`. Acceptance: `git show --name-only --format= HEAD` lists only paths from that set; `git status --porcelain --untracked-files=all` contains no entry outside `coverage/`; the head SHA differs from the value P2-T8 recorded.

- [ ] [P4-T8] Close batch B at the budget boundary: record `.claude/state/powershell-batch-budget.<session-id>.json`, delete it, confirm absence, and write all three observations to `evidence/other/p4-t8-batch-b-boundary.2026-09-19T09-44.md`. Acceptance: the recorded pre-reset `prodFiles` array has exactly 2 members and `testFiles` exactly 3; the post-reset check reports the file absent; the artifact records that P4-T2 through P4-T6 all passed and that P4-T7 produced a commit.

### Phase 5 — Batch C: Analyzer-Item Repair and Project Consistency, with the AC22 Red-Before Control

- [ ] [P5-T1] Create `scripts/dependencies/ProjectConsistency.psm1` as a declared pass-through: every function the later tasks implement is exported with its final name and signature, each body returning its input unchanged and reporting an empty disagreement set. Acceptance: the module imports without error and `Get-Command -Module ProjectConsistency` lists every function name the plan's later tasks cite. The pass-through shape is deliberate: it makes the P5-T4 red run a **behavioural** failure on assertions rather than an import failure, so the red proves the absent behaviour rather than an absent file. Evidence: `evidence/qa-gates/p5-t1-projectconsistency-passthrough.2026-09-19T09-44.md`.

- [ ] [P5-T2] Create `scripts/dependencies/AnalyzerItemRepair.psm1` as a declared pass-through on the same terms: exported functions with final names and signatures, bodies returning input unchanged. Acceptance: the module imports without error and `Get-Command -Module AnalyzerItemRepair` lists every function name the plan's later tasks cite. Evidence: `evidence/qa-gates/p5-t2-analyzeritemrepair-passthrough.2026-09-19T09-44.md`.

- [ ] [P5-T3] Create `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` carrying, among its cases, the AC21 fixture reproducing the #908 three-way divergence: one project whose in-memory manifest declares `3.0.235`, whose `<Import>` and `<Error>` name `3.0.259`, and whose `<Analyzer Include>` names `3.0.203`. The AC21 case asserts that before repair the verifier reports a disagreement for the guard elements and a separate disagreement for the analyzer item, and that after repair all three locations name `3.0.235`. All fixtures are in-memory strings. Acceptance: the file is at most 500 lines and contains an `It` whose name contains the token `AC21`. Evidence: `evidence/qa-gates/p5-t3-projectconsistency-tests-authored.2026-09-19T09-44.md`.

- [ ] [P5-T4] [expect-fail] Run the AC21 case against the pass-through tree with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1")` and `$c.Filter.FullName = "*AC21*"`, with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p5-t4-ac21-red-coverage.2026-09-19T09-44.xml`, and capture the run into `evidence/baseline/p5-t4-ac22-fail-before.2026-09-19T09-44.md` with `ExpectedExitCode: 1`. Acceptance: `EXIT_CODE: 1`; the recorded `Failed` count is at least 1 and `Total` is at least 1; and the artifact enumerates the failing `It` names together with their verbatim failure messages. The `Total` at least 1 clause is the non-vacuity guard: a filter that matched no test would also produce zero passes. A failure message naming a missing module or a missing command is **not** an acceptable red — it proves the file is absent rather than the behaviour, and the task must be redone with the pass-through modules importing cleanly.

- [ ] [P5-T5] Implement version reconciliation in `scripts/dependencies/ProjectConsistency.psm1`: given a manifest version and project text, force `<Import>`, `<Error>`, `<Reference>` and `<HintPath>` to agree with the manifest, consuming the parsed structures `scripts/dependencies/PackageGraph.psm1` produces rather than re-implementing parsing. Acceptance: the module imports without error, exports the reconciliation function, and the file is at most 500 lines. Evidence: `evidence/qa-gates/p5-t5-version-reconciliation.2026-09-19T09-44.md`.

- [ ] [P5-T6] Implement binding-redirect reconciliation in `scripts/dependencies/ProjectConsistency.psm1`: reconcile an `app.config` redirect to the assembly version resolved from the manifest, writing the resolved version into both the upper bound of `oldVersion` and into `newVersion`, and returning an `app.config` that carries no redirect for the assembly unchanged. Acceptance: the module imports without error and the file remains at most 500 lines. Evidence: `evidence/qa-gates/p5-t6-binding-redirect-reconciliation.2026-09-19T09-44.md`.

- [ ] [P5-T7] Implement the verifier in `scripts/dependencies/ProjectConsistency.psm1`: detect analyzer-item version disagreements with an examined-item count, detect orphaned `<HintPath>` entries, assert reference completeness by requiring a `<Reference>` with a matching `<HintPath>` for each consumable library asset resolved for each manifest package, emit a per-project repairs report, and return a failure result naming the specific condition and project when the post-repair state is still inconsistent. A repair that cannot be derived throws rather than emitting a guessed path. Acceptance: the module imports without error, exports the verifier and its report function, and the file is at most 500 lines. When the file exceeds 500 lines the remedy is to move pure parsing or rendering helpers into `scripts/dependencies/PackageGraph.psm1` per Scope Decision 5, which consumes no additional batch slot. Evidence: `evidence/qa-gates/p5-t7-verifier.2026-09-19T09-44.md`.

- [ ] [P5-T8] Create `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` supplying an injected directory listing and asserting: the derived path set for a plain language-folder shape; for a Roslyn-qualified shape; for a multi-assembly shape whose assembly names do not match the package id; for a shape with no intermediate folders; that a listing offering two Roslyn-qualified folders selects the higher; that non-C-sharp language folders and satellite resource assemblies are excluded; that a package whose listing contains no analyzer directory contributes no items; that after regeneration the item group still contains the `<AdditionalFiles>` element naming the banned-symbols list and the explanatory comment that precedes the items; and that a project fixture with no analyzer item group at all is returned byte-identical with no item group synthesised. The fixture set must include a project carrying two separate analyzer item groups, because `VBFunctions.Test/VBFunctions.Test.csproj` has that shape at lines 263-265 and 287-294 and a single-group assumption would silently drop one. Acceptance: the file is at most 500 lines, contains at least 10 `It` blocks, and creates no temporary file. Evidence: `evidence/qa-gates/p5-t8-analyzerrepair-tests-authored.2026-09-19T09-44.md`.

- [ ] [P5-T9] Implement `scripts/dependencies/AnalyzerItemRepair.psm1`: derive the `<Analyzer Include>` set by enumerating the restored package directory through the injected listing delegate, never by computing the path from the package id; select the highest Roslyn-qualified folder available; exclude non-C-sharp language folders and satellite resource assemblies; rewrite the owning item group in place while preserving the sibling `<AdditionalFiles>` element and the preceding explanatory comment; and throw when the restored directory for the manifest version does not exist. Acceptance: the module imports without error, exports the derivation and rewrite functions, contains no literal `analyzers\dotnet\cs` used as a computed default path, and is at most 500 lines. Evidence: `evidence/qa-gates/p5-t9-analyzer-item-repair.2026-09-19T09-44.md`.

- [ ] [P5-T10] Run the AC12 cases with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1")` and `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t10-ac12-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, and the `Detailed` output names each of the four shape cases, the higher-Roslyn-folder case, the exclusion case and the no-analyzer-directory case individually. An implementation that computed the path from the package id fails the multi-assembly and the bare-directory cases. Evidence: `evidence/qa-gates/p5-t10-ac12-analyzer-derivation.2026-09-19T09-44.md`. This task checks off **AC12**.

- [ ] [P5-T11] Run the AC13 cases with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1")` and `$c.Filter.FullName = "*AC13*"`, with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t11-ac13-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total` at least 3, and the `Detailed` output names both the `<AdditionalFiles>` survival case and the byte-identity case for a project with no analyzer item group. The `Total` at least 3 clause guards against a filter that matched nothing. Evidence: `evidence/qa-gates/p5-t11-ac13-sibling-survival.2026-09-19T09-44.md`. This task checks off **AC13**.

- [ ] [P5-T12] Run the AC11 cases with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1")` and `$c.Filter.FullName = "*AC11*"`, with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t12-ac11-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total=4`, and the `Detailed` output names one passing case per element kind — `<Import>`, `<Error>`, `<Reference>`, `<HintPath>` — each asserting the reconciled text names the manifest version. Per-kind assertions make a reconciler that handles only two kinds fail rather than pass on an aggregate. Evidence: `evidence/qa-gates/p5-t12-ac11-version-reconciliation.2026-09-19T09-44.md`. This task checks off **AC11**.

- [ ] [P5-T13] Run the AC14 cases with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1")` and `$c.Filter.FullName = "*AC14*"`, with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t13-ac14-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total=2`, and the `Detailed` output names the redirect-reconciled case, which asserts the resolved version appears in both the upper bound of `oldVersion` and in `newVersion`, and the no-redirect case, which asserts the input is returned unchanged. Evidence: `evidence/qa-gates/p5-t13-ac14-binding-redirects.2026-09-19T09-44.md`. This task checks off **AC14**.

- [ ] [P5-T14] Run the AC8 cases with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1")` and `$c.Filter.FullName = "*AC8*"`, with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t14-ac8-coverage.2026-09-19T09-44.xml`, and additionally run the verifier over the real `ToDoModel.Test/ToDoModel.Test.csproj` and `ToDoModel.Test/packages.config` pair. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total` at least 2; the in-memory case asserts the detector reports a **non-empty** orphan set for a fixture reproducing the pre-fix pair; and the live verifier invocation reports exactly 0 orphaned `<HintPath>` entries for that project while reporting a non-zero count of `<HintPath>` entries examined. Both directions are asserted, and the examined count guards the zero. Evidence: `evidence/qa-gates/p5-t14-ac8-orphan-hintpaths.2026-09-19T09-44.md`. This task checks off **AC8**.

- [ ] [P5-T15] Run the AC16 cases with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1")` and `$c.Filter.FullName = "*AC16*"`, with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t15-ac16-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total=2`, and the `Detailed` output names both a case in which every divergence is repairable and the entry point returns a success result whose report enumerates the repairs performed, and a case carrying a divergence no repair can resolve for which the entry point returns a failure result naming that condition and the project. The failing direction is what proves the verifier is not a pass-through. Evidence: `evidence/qa-gates/p5-t15-ac16-verifier-both-directions.2026-09-19T09-44.md`. This task checks off **AC16**.

- [ ] [P5-T16] Run the AC23 cases with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1")` and `$c.Filter.FullName = "*AC23*"`, with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t16-ac23-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total` at least 2, and the `Detailed` output names the case asserting the detector reports a **missing reference** for a fixture from which one `<Reference>`/`<HintPath>` pair has been removed, alongside the case asserting a complete fixture reports none. The detector must be demonstrated firing; a check that cannot be made to fail tests nothing. Evidence: `evidence/qa-gates/p5-t16-ac23-reference-completeness.2026-09-19T09-44.md`. This task checks off **AC23**.

- [ ] [P5-T17] Run the AC21 case against the implemented tree with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1")` and `$c.Filter.FullName = "*AC21*"`, with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t17-ac21-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total` equal to the `Total` P5-T4 recorded, and the `Detailed` output shows the case asserting all three locations name `3.0.235` after repair. The `Total` equality clause proves the same population ran red and green. Evidence: `evidence/qa-gates/p5-t17-ac21-908-divergence-resolved.2026-09-19T09-44.md`. This task checks off **AC21**.

- [ ] [P5-T18] Assemble the AC22 red-before and green-after pair into `evidence/regression-testing/p5-t18-ac22-fail-before-pass-after.2026-09-19T09-44.md`, citing the failing run at `evidence/baseline/p5-t4-ac22-fail-before.2026-09-19T09-44.md` and the passing run at `evidence/qa-gates/p5-t17-ac21-coverage.2026-09-19T09-44.xml`, and recording the failing and passing `It` names, `Total` counts and `EXIT_CODE` values side by side. Acceptance: both cited artifacts exist; the failing artifact records `EXIT_CODE: 1` with `Failed` at least 1; the passing artifact records `EXIT_CODE: 0` with `Failed=0`; the two `Total` values are equal; and the failure messages recorded in the failing artifact reference the assertion rather than a missing module or command. A test that cannot be shown failing is not admitted. This task checks off **AC22**.

- [ ] [P5-T19] Audit the line count of every file this batch created or modified — `scripts/dependencies/AnalyzerItemRepair.psm1`, `scripts/dependencies/ProjectConsistency.psm1`, `scripts/dependencies/PackageGraph.psm1`, `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1`, `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` — and record each count in `evidence/qa-gates/p5-t19-file-size-audit.2026-09-19T09-44.md`. Acceptance: exactly 5 files are listed with an integer line count each and every count is at most 500. The exactly-5 clause guards against an enumerator that listed nothing.

### Phase 6 — Batch C Close-Out: Toolchain Gates, Commit and Budget Boundary

- [ ] [P6-T1] Run CMD-POSHQC-FORMAT over the four `scan_folders` with before and after SHA-256 hash sets recorded into `evidence/qa-gates/p6-t1-poshqc-format.2026-09-19T09-44.md`. Acceptance: `MCP Result: ok:true`, both hash sets recorded, the rewrite count recorded as the hash-difference count. A non-zero rewrite count restarts the phase from P6-T1.

- [ ] [P6-T2] Run CMD-POSHQC-ANALYZE and record the result in `evidence/qa-gates/p6-t2-poshqc-analyze.2026-09-19T09-44.md`. Acceptance: `MCP Result: ok:true`, finding count exactly 0, and the exact `scan_folders` argument value quoted.

- [ ] [P6-T3] Run CMD-PESTER-ALL with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p6-t3-pester-coverage.2026-09-19T09-44.xml` and record the result in `evidence/qa-gates/p6-t3-pester.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, aggregate JaCoCo LINE percentage at least 85, and the `sourcefile` LINE percentage for each of `AnalyzerItemRepair.psm1` and `ProjectConsistency.psm1` recorded and at least 90.

- [ ] [P6-T4] Assert that batch C changed no C# compilation input: capture `git diff --name-only <P4-T7-head-sha> -- .` and `git status --porcelain --untracked-files=all` into `evidence/qa-gates/p6-t4-csharp-input-invariance.2026-09-19T09-44.md`. Acceptance: the union of the two captures contains at least 4 paths and exactly 0 paths matching `*.cs`, `*.csproj`, `*.sln`, `packages.config` or `app.config`.

- [ ] [P6-T5] Run CMD-CSHARPIER-CHECK and record the result in `evidence/qa-gates/p6-t5-csharpier-check.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`, the verbatim `Checked N files in Xms.` line recorded, and zero files reported with findings.

- [ ] [P6-T6] Commit batch C with explicit pathspecs covering `scripts/dependencies/AnalyzerItemRepair.psm1`, `scripts/dependencies/ProjectConsistency.psm1`, `scripts/dependencies/PackageGraph.psm1`, `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1`, `tests/scripts/dependencies/ProjectConsistency.Tests.ps1`, `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, and record the head SHA in `evidence/qa-gates/p6-t6-commit.2026-09-19T09-44.md`. Acceptance: `git show --name-only --format= HEAD` lists only paths from that set; `git status --porcelain --untracked-files=all` contains no entry outside `coverage/`; the head SHA differs from the value P4-T7 recorded.

- [ ] [P6-T7] Close batch C at the budget boundary: record `.claude/state/powershell-batch-budget.<session-id>.json`, delete it, confirm absence, and write all three observations to `evidence/other/p6-t7-batch-c-boundary.2026-09-19T09-44.md`. Acceptance: the recorded pre-reset `prodFiles` array has exactly 3 members and `testFiles` exactly 2; the post-reset check reports the file absent; the artifact records that P6-T2 through P6-T5 all passed and that P6-T6 produced a commit.

### Phase 7 — Batch D: Composition Root, Repair Workflow and Documentation

- [ ] [P7-T1] Create `scripts/dependencies/Repair-PackageManifestConsistency.ps1` as the composition root and command-line entry point: it wires the four modules in the order asset-level compatibility gate, version reconciliation, analyzer-item regeneration, binding-redirect reconciliation, config normalisation, verification; it skips an incompatible package with a recorded reason and proceeds with the remaining upgrades, never failing the run on a skip; and it emits the repairs report consumed by the pull-request body, with a "Packages skipped" section present only when a skip was recorded. Acceptance: the script declares `[CmdletBinding(SupportsShouldProcess = $true)]`, imports all four modules from `scripts/dependencies/`, is at most 500 lines, and runs to completion with `-WhatIf` against the working tree producing no file modification, verified by an empty `git status --porcelain --untracked-files=all` capture taken after the `-WhatIf` run. Evidence: `evidence/qa-gates/p7-t1-composition-root.2026-09-19T09-44.md`.

- [ ] [P7-T2] Create `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` driving the entry point over an in-memory fixture with two candidate upgrades, one incompatible, asserting all three of: the incompatible package's manifest version is unchanged; the compatible package's manifest version is the target version; and the returned report contains a skip record naming the incompatible package together with a non-empty reason. Acceptance: the file is at most 500 lines, contains exactly 3 `It` blocks for that scenario, and creates no temporary file. Evidence: `evidence/qa-gates/p7-t2-repair-tests-authored.2026-09-19T09-44.md`.

- [ ] [P7-T3] Run the AC10 cases with CMD-PESTER-ALL restricted to `$c.Run.Path = @("tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1")` and `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t3-ac10-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, `Total=3`, and all three named assertions pass. A fail-fast implementation fails the second assertion, which is the one that proves the remaining upgrades proceeded. Evidence: `evidence/qa-gates/p7-t3-ac10-skip-and-proceed.2026-09-19T09-44.md`. This task checks off **AC10**.

- [ ] [P7-T4] Run the verifier from `scripts/dependencies/ProjectConsistency.psm1` over the working tree and record its analyzer-item report in `evidence/qa-gates/p7-t4-ac5-analyzer-verifier.2026-09-19T09-44.md`. Acceptance: the report states exactly 0 analyzer-item version disagreements **and** states having examined exactly 162 `<Analyzer Include>` items across exactly 17 project files. The examined-count assertion is the non-vacuity guard: a detector that matched nothing would report zero disagreements and zero examined, and would fail this criterion. This task checks off **AC5**.

- [ ] [P7-T5] Run `scripts/dependencies/Repair-PackageManifestConsistency.ps1` over the working tree, then run it a second time over its own output, then capture `git diff origin/main -- "*/packages.config" "*/app.config" "*.csproj"`, `git status --porcelain --untracked-files=all`, and CMD-CSHARPIER-CHECK. Acceptance: the second run produces no change, evidenced by the `git status --porcelain --untracked-files=all` capture taken between the two runs being byte-identical to the one taken after the second run; CMD-CSHARPIER-CHECK reports `EXIT_CODE: 0` with zero files reported with findings; and the repairs report from the second run records exactly 0 repairs applied while recording a non-zero count of elements examined. The examined count guards the zero-repairs figure. Evidence: `evidence/qa-gates/p7-t5-ac15-repair-idempotence.2026-09-19T09-44.md`. This task checks off **AC15**.

- [ ] [P7-T6] Create `.github/workflows/dependabot-repair.yml`: triggered by `workflow_run` on completion of the CI workflow, restricted to head branches under the Dependabot branch prefix, declaring `permissions: contents: write` and `pull-requests: write`, minting an installation token with `actions/create-github-app-token@v3` from the secrets `DEPENDABOT_REPAIR_APP_ID` and `DEPENDABOT_REPAIR_APP_PRIVATE_KEY`, checking out with that token, setting up MSBuild and NuGet pinned to `7.9.0`, restoring, running `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, committing, pushing onto the Dependabot branch with the same token, updating the pull-request body with the "Repairs applied" block and, when a skip was recorded, the "Packages skipped" block, and applying the `deps:autofixed` label when a repair outside the analyzer-item and binding-redirect classes was applied. Acceptance: the file exists; it contains exactly 0 occurrences of `pull_request_target`; it contains exactly 1 `workflow_run` trigger and at least 1 branch-prefix restriction expression; and it declares both write permissions. The positive counts guard the zero. Evidence: `evidence/qa-gates/p7-t6-repair-workflow.2026-09-19T09-44.md`.

- [ ] [P7-T7] Extend `tests/scripts/dependencies/DependabotConfig.Tests.ps1` with the AC17 assertions against `.github/workflows/dependabot-repair.yml` — the branch restriction is present and `pull_request_target` is absent — then run CMD-ACTIONLINT and run that suite with CMD-PESTER-ALL restricted to that test file, with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t7-ac17-coverage.2026-09-19T09-44.xml`. Acceptance: CMD-ACTIONLINT returns `EXIT_CODE: 0` and records a linted-file count of at least 10; the Pester run returns `EXIT_CODE: 0` with `Failed=0`; and the suite's recorded assertion for the branch restriction is a positive match on a named expression, not merely an absence check. Evidence: `evidence/qa-gates/p7-t7-ac17-workflow-static-validity.2026-09-19T09-44.md`. This task checks off **AC17**.

- [ ] [P7-T8] Update `.github/workflows/README.md` to document the repair workflow, its `workflow_run` trigger, its credential requirement including the two secret names and a pointer to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`, the degraded mode that applies when the credential is absent (the repair push produces a `pull_request` `synchronize` run that parks awaiting a human approval click), and the pinned NuGet CLI version literal `7.9.0`. Acceptance: the README contains the literal `7.9.0` exactly once in the NuGet-pin section; it names both secret names; and it names the degraded mode explicitly. Evidence: `evidence/qa-gates/p7-t8-workflow-readme.2026-09-19T09-44.md`.

- [ ] [P7-T9] Extend `tests/scripts/dependencies/DependabotConfig.Tests.ps1` with the AC26 assertion that the pinned NuGet version literal recorded in `.github/workflows/README.md` equals the literal declared in the workflow files, then run that suite with CMD-PESTER-ALL restricted to that test file and `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t9-ac26-coverage.2026-09-19T09-44.xml`. Acceptance: `EXIT_CODE: 0`, `Failed=0`, and the test records both the README literal and the set of workflow literals it compared, with the workflow set having exactly 3 members. The criterion fails when the pin is bumped in one place only, which the equality comparison over a non-empty set makes reachable. Evidence: `evidence/qa-gates/p7-t9-ac26-documentation-pin.2026-09-19T09-44.md`. This task checks off **AC26**.

- [ ] [P7-T10] Audit the line count of every file this batch created or modified — `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1` — and record each count in `evidence/qa-gates/p7-t10-file-size-audit.2026-09-19T09-44.md`. Acceptance: exactly 3 files are listed with an integer line count each and every count is at most 500.

- [ ] [P7-T11] Commit batch D with explicit pathspecs covering `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1`, `.github/workflows/dependabot-repair.yml`, `.github/workflows/README.md`, `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, and record the head SHA in `evidence/qa-gates/p7-t11-commit.2026-09-19T09-44.md`. Acceptance: `git show --name-only --format= HEAD` lists only paths from that set; `git status --porcelain --untracked-files=all` contains no entry outside `coverage/`; the head SHA differs from the value P6-T6 recorded.

### Phase 8 — Live-Credential Acceptance and Deferred-Verification Dossier

The three criteria in this phase depend on a GitHub App installation token that a repository admin
must provision by hand, following
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`.
Per the orchestrator decision, no task in this plan blocks on that credential: the change must be
shippable and verifiable before it exists. Each task below therefore carries an explicitly authorised
two-branch outcome, and both branches are binary and observable. A criterion is checked off only on
the live branch; the deferred branch leaves it unchecked and records why.

- [ ] [P8-T1] Measure credential and fixture availability: run `gh api repos/drmoisan/TaskMaster/actions/secrets --jq '[.secrets[].name] | sort'` and `gh api "repos/drmoisan/TaskMaster/pulls?state=open" --jq '[.pull_requests?] | length'` together with `gh pr list --repo drmoisan/TaskMaster --author app/dependabot --json number,headRefName`, and write `evidence/other/p8-t1-credential-availability.2026-09-19T09-44.md`. Acceptance: the artifact records the full sorted secret-name list as returned, an explicit boolean for whether both `DEPENDABOT_REPAIR_APP_ID` and `DEPENDABOT_REPAIR_APP_PRIVATE_KEY` are present, and the list of open Dependabot pull requests with their numbers. When the secret list is empty the artifact must say so explicitly rather than omitting the field, because an omitted field is indistinguishable from a failed query.

- [ ] [P8-T2] Discharge AC18 against `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p8-t2-ac18-repair-identity.2026-09-19T09-44.md`. Live branch, taken when P8-T1 recorded both secrets present and at least one open Dependabot pull request: after the repair run, capture `gh api repos/drmoisan/TaskMaster/pulls/<PR>  --jq '.head.sha'` and `gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA> --jq '.author.login'`; acceptance is that the recorded head SHA differs from the pre-repair SHA and the recorded login ends with `[bot]` and is not `github-actions[bot]`, and AC18 is checked off. Deferred branch, taken otherwise and explicitly authorised here: the artifact records `DEFERRED: credential or fixture absent`, quotes the P8-T1 measurement that established it, names the runbook, and states that AC18 remains unchecked. In both branches the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`.

- [ ] [P8-T3] Discharge AC19 against `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p8-t3-ac19-required-checks.2026-09-19T09-44.md`. Live branch: capture `gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA>/check-runs --jq '[.check_runs[] | {name, status, conclusion}] | sort_by(.name)'` and, for each check run, the originating workflow run's `event` field; acceptance is that for every check named required by repository ruleset 18572843 a check run exists on the post-repair head SHA, its originating run event is `pull_request`, its conclusion is `success`, no run carries `action_required` or a `waiting` status, and the recorded required-check count is exactly 5 — and AC19 is checked off. The count and the per-run event field together are what falsify a wrong trigger or credential choice: a `workflow_run`-sourced or parked check fails the criterion, and an empty check-run array fails the count. Deferred branch, explicitly authorised here: the artifact records `DEFERRED: credential or fixture absent`, quotes the P8-T1 measurement, and states that AC19 remains unchecked.

- [ ] [P8-T4] Discharge AC20 against `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p8-t4-ac20-disclosure.2026-09-19T09-44.md`. Live branch: capture the pull-request body and label state for two runs — one that applied a repair outside the analyzer-item and binding-redirect classes, and one that applied only those two classes; acceptance is that both bodies carry a "Repairs applied" block enumerating repairs by project, that a "Packages skipped" block is present on exactly those runs that recorded a skip, that `deps:autofixed` is present on the first run and absent on the second, and that both label states are captured — and AC20 is checked off. The absent-label case is what prevents an implementation that always labels from passing. Deferred branch, explicitly authorised here: the artifact records `DEFERRED: credential or fixture absent`, quotes the P8-T1 measurement, and states that AC20 remains unchecked.

- [ ] [P8-T5] File a follow-up GitHub issue carrying any criterion left unchecked by P8-T2, P8-T3 or P8-T4, titled to name issue #911 and the three criteria, with a body that quotes the P8-T1 measurement, names the runbook at `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`, and lists the exact verification commands from those three tasks; then mirror it to `evidence/issue-updates/p8-t5-followup-issue.2026-09-19T09-44.md` with `PostedAs:` and the issue URL. Acceptance: when at least one criterion is deferred, the mirror records a created issue number and URL; when all three were discharged live, the mirror records `SearchScope:`, `SearchPatterns:` and `SearchResult: none` together with the statement that no follow-up was required, so the absence claim is auditable rather than asserted.

- [ ] [P8-T6] Commit the Phase 8 evidence with an explicit pathspec limited to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/` and record the head SHA in `evidence/qa-gates/p8-t6-commit.2026-09-19T09-44.md`. Acceptance: `git show --name-only --format= HEAD` lists only paths under that folder; `git status --porcelain --untracked-files=all` contains no entry outside `coverage/`; the head SHA differs from the value P7-T11 recorded.

### Phase 9 — Final QA Loop, Acceptance Check-Off and Close-Out

The four PowerShell steps and the four C# steps below run in order. If any step fails, or if any step
changes a tracked file, the loop restarts from P9-T1. `EXIT_CODE: SKIPPED` is not a passing outcome
for any task in this phase.

- [ ] [P9-T1] PowerShell QA step 1 — run CMD-POSHQC-FORMAT over the four `scan_folders` with before and after SHA-256 hash sets recorded into `evidence/qa-gates/p9-t1-poshqc-format.iter1.2026-09-19T09-44.md`. Acceptance: `MCP Result: ok:true`, both hash sets recorded, and the hash-difference rewrite count recorded as an integer equal to 0. A non-zero count restarts the loop and the next iteration's artifact carries the suffix `iter2`, so an artifact is never overwritten by a same-minute rerun.

- [ ] [P9-T2] PowerShell QA step 2 — run CMD-POSHQC-ANALYZE and record the result in `evidence/qa-gates/p9-t2-poshqc-analyze.iter1.2026-09-19T09-44.md`. Acceptance: `MCP Result: ok:true`, the integer finding count is exactly 0, and the artifact quotes the exact `scan_folders` argument value so an unscoped run is distinguishable from a clean one.

- [ ] [P9-T3] PowerShell QA step 3 — run CMD-PESTER-ALL with `<OUTPATH>` set to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t3-pester-coverage.iter1.2026-09-19T09-44.xml` and record the result in `evidence/qa-gates/p9-t3-pester.iter1.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`; `Failed=0`; the aggregate JaCoCo LINE percentage recorded to two decimals and at least 85; the `sourcefile` LINE percentage recorded and at least 90 for each of `PackageGraph.psm1`, `PackageCompatibility.psm1`, `AnalyzerItemRepair.psm1`, `ProjectConsistency.psm1` and `Repair-PackageManifestConsistency.ps1`; the `sourcefile` LINE percentage for `Sync-PackageReferences.ps1` recorded and strictly greater than its P0-T17 baseline value; and the artifact states explicitly that Pester emits no branch counter in any output format, so the branch threshold is unevaluable for PowerShell and no branch figure is claimed. This task checks off **AC24**, together with the format and analyze results from P9-T1 and P9-T2 which the artifact must cite by path.

- [ ] [P9-T4] C# QA step 1 — run CMD-CSHARPIER-CHECK and record the result in `evidence/qa-gates/p9-t4-csharpier-check.iter1.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`, the verbatim `Checked N files in Xms.` line recorded with `N` as an integer greater than 900, and zero files reported with findings.

- [ ] [P9-T5] C# QA step 2 — run CMD-MSBUILD-ANALYZERS and record the result in `evidence/qa-gates/p9-t5-msbuild-analyzers.iter1.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`; exactly 0 lines containing `CS0006` in `coverage/analyzers.msbuild.log`; and at least 18 lines containing `/out:obj\Debug\` with the exact count recorded. The `/out:` count is the non-vacuity observation required by AC25, because a build whose compile targets were skipped would also report zero errors.

- [ ] [P9-T6] C# QA step 3 — run CMD-MSBUILD-NULLABLE and record the result in `evidence/qa-gates/p9-t6-msbuild-nullable.iter1.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0` and at least 18 lines containing `/out:obj\Debug\` in `coverage/nullable.msbuild.log`, with the exact count recorded.

- [ ] [P9-T7] C# QA step 4 — run CMD-MSTEST-COVERAGE and record the result in `evidence/qa-gates/p9-t7-mstest-coverage.iter1.2026-09-19T09-44.md`. Acceptance: `EXIT_CODE: 0`; the artifact records the numeric line-coverage and branch-coverage percentages the runner printed, together with passed, failed and skipped counts; and the failed count is 0 with the passed count greater than zero.

- [ ] [P9-T8] Record the AC25 single-pass attestation in `evidence/qa-gates/p9-t8-ac25-csharp-toolchain.2026-09-19T09-44.md`, citing the four artifacts from P9-T4 through P9-T7 by path and recording their four `EXIT_CODE` values and their four timestamps. Acceptance: all four exit codes are 0; the four timestamps are strictly increasing, proving they ran in order within one pass; and the analyzer and nullable non-vacuity counts recorded at P9-T5 and P9-T6 are both at least 18. If any of the four artifacts belongs to an earlier loop iteration, the attestation fails and the loop restarts from P9-T1. This task checks off **AC25**.

- [ ] [P9-T9] Record the coverage reconciliation in `evidence/qa-gates/p9-t9-coverage-reconciliation.2026-09-19T09-44.md`: for C#, the numeric baseline from `evidence/baseline/p2-t7-mstest-numeric-baseline.2026-09-19T09-44.md`, the post-change values from P9-T7, and the delta for each of line and branch; for PowerShell, the aggregate baseline from `evidence/baseline/p0-t17-pester.2026-09-19T09-44.md`, the post-change aggregate from P9-T3, and the per-new-module figures. Acceptance: every figure is a number, not a placeholder; the C# line and branch deltas are each greater than or equal to 0, which is the no-regression condition and is meaningful because this change modifies no `.cs` file; the PowerShell aggregate is at least 85; every new module is at least 90; and the artifact states that no branch figure exists for PowerShell and names the tooling reason.

- [ ] [P9-T10] Audit file size across the change footprint: for every path listed in the spec `## Write Set` under "Production PowerShell" and "Tests", record the line count, and record it also for `.github/workflows/dependabot-repair.yml` and `.github/workflows/_pester.yml`, into `evidence/qa-gates/p9-t10-file-size-audit.2026-09-19T09-44.md`. Acceptance: exactly 15 files are listed with an integer line count each and every count is at most 500. Markdown documentation under the feature folder is exempt from the 500-line cap per `.claude/rules/general-code-change.md` and is deliberately not in this list.

- [ ] [P9-T11] Sweep the acceptance criteria in `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`: tick each of AC1 through AC26 whose discharging task recorded a passing outcome, leave unticked each criterion a Phase 8 deferred branch left open, and append a status summary listing every criterion with its discharging task ID and its evidence artifact path. Acceptance: exactly 26 criteria are listed in the summary; every ticked criterion cites an evidence artifact that exists on disk; every unticked criterion names the P8-T5 follow-up issue; and no criterion text is reworded. Evidence: `evidence/qa-gates/p9-t11-ac-status-summary.2026-09-19T09-44.md`.

- [ ] [P9-T12] Verify the change footprint against the spec `## Write Set`: capture `git diff --name-only <MERGE_BASE> -- .` using the value P0-T3 recorded, and `git status --porcelain --untracked-files=all`, into `evidence/qa-gates/p9-t12-change-footprint.2026-09-19T09-44.md`. Acceptance: every path in the union of the two captures is either a member of the spec `## Write Set` or lies under `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, `.claude/agent-memory/` or `coverage/`; the union contains at least 70 paths; and the union contains exactly 0 paths under `.claude/rules/` or `.github/instructions/`, which policy prohibits this change from touching. The at-least-70 clause is the non-vacuity guard, and the merge-base anchor is what makes the diff non-vacuous at all given the commits P2-T8 through P8-T6 produced.

- [ ] [P9-T13] Commit all remaining work with explicit pathspecs and record the head SHA in `evidence/qa-gates/p9-t13-commit.2026-09-19T09-44.md`. Acceptance: `git status --porcelain --untracked-files=all` is captured verbatim and contains no entry outside `coverage/`; `git show --name-only --format= HEAD` is captured; the head SHA differs from the value P8-T6 recorded.

- [ ] [P9-T14] Write the review-handoff index to `evidence/other/p9-t14-review-handoff-index.2026-09-19T09-44.md`, listing every evidence artifact this plan produced with its path, its discharging task ID, its `EXIT_CODE` and, for the criteria-bearing artifacts, the criterion it discharges; and recording the head SHA from P9-T13, the merge-base from P0-T3 and the four batch commit SHAs. Acceptance: the index lists at least 80 artifacts, every listed path exists on disk, and the artifact count for `evidence/baseline/` is at least 20.

- [ ] [P9-T15] Commit the review-handoff index and any artifact written after P9-T13 with an explicit pathspec limited to `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`, and record the final head SHA in `evidence/qa-gates/p9-t15-final-commit.2026-09-19T09-44.md`. Acceptance: `git status --porcelain --untracked-files=all` is captured verbatim and contains no entry outside `coverage/`; the recorded head SHA differs from the value P9-T13 recorded. This second commit task exists because P9-T14 writes an artifact after the P9-T13 commit, which would otherwise leave the plan's terminal state as a worktree carrying untracked evidence.

---

## Acceptance-Criteria Traceability

| Criterion | Discharging task | Implementation tasks | Test artifact |
|---|---|---|---|
| AC1 | P3-T9 | P3-T7, P3-T8 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` |
| AC2 | P1-T3 | P1-T2 | live CSharpier control with a perturbed C# file |
| AC3 | P1-T8 | P1-T4, P1-T7 | normaliser idempotence plus empty diff and porcelain |
| AC4 | P3-T10 | P1-T12 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` |
| AC5 | P7-T4 | P1-T9, P5-T7 | verifier report with 162 examined across 17 files |
| AC6 | P1-T14 | P1-T9 | P0-T11 red log paired with P1-T14 green log |
| AC7 | P3-T6 | P3-T1, P3-T4 | `PackageCompatibility.Tests.ps1`, `Sync-PackageReferences.Tests.ps1` |
| AC8 | P5-T14 | P1-T11, P5-T7 | `ProjectConsistency.Tests.ps1` plus live verifier run |
| AC9 | P3-T3 | P3-T1 | `PackageCompatibility.Tests.ps1` |
| AC10 | P7-T3 | P7-T1 | `Repair-PackageManifestConsistency.Tests.ps1` |
| AC11 | P5-T12 | P5-T5 | `ProjectConsistency.Tests.ps1` |
| AC12 | P5-T10 | P5-T9 | `AnalyzerItemRepair.Tests.ps1` |
| AC13 | P5-T11 | P5-T9 | `AnalyzerItemRepair.Tests.ps1` |
| AC14 | P5-T13 | P5-T6 | `ProjectConsistency.Tests.ps1` |
| AC15 | P7-T5 | P7-T1 | repair idempotence plus CSharpier check |
| AC16 | P5-T15 | P5-T7 | `ProjectConsistency.Tests.ps1` |
| AC17 | P7-T7 | P7-T6 | actionlint plus `DependabotConfig.Tests.ps1` |
| AC18 | P8-T2 | P7-T6 | commits API capture, or authorised deferral record |
| AC19 | P8-T3 | P7-T6 | check-runs API capture, or authorised deferral record |
| AC20 | P8-T4 | P7-T1, P7-T6 | pull-request body and label capture for both runs |
| AC21 | P5-T17 | P5-T5, P5-T9 | `ProjectConsistency.Tests.ps1` AC21 fixture |
| AC22 | P5-T18 | P5-T1, P5-T2 | P5-T4 red run paired with P5-T17 green run |
| AC23 | P5-T16 | P5-T7 | `ProjectConsistency.Tests.ps1` |
| AC24 | P9-T3 | all PowerShell tasks | PoshQC format, PoshQC analyze, Pester coverage |
| AC25 | P9-T8 | all C# tasks | four CLAUDE.md commands in one ordered pass |
| AC26 | P7-T9 | P7-T8, P1-T12 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` |

---

## Residual Risks Carried by This Plan

1. **The `workflow_run` trigger mechanism is an assumption of record.** AC19 asserts the outcome, not
   the mechanism, so a wrong choice fails visibly on the fixture pull request. It is not measurable
   before the credential exists, which is why Phase 8 carries an authorised deferral.
2. **AC6's failing direction is recorded and its passing direction is not yet measured.** The failing
   direction is measured in `evidence/regression-testing/898-cold-restore-red-run.2026-09-19T11-40.md`
   against a single project. P0-T11 re-measures it solution-wide and P1-T14 measures the passing
   direction from the same cold state. A solution-wide cold rebuild may surface a second stranded
   reference class that the single-project measurement did not reach; when it does, P1-T14 fails and
   the finding is reported rather than absorbed.
3. **`scripts/dependencies/ProjectConsistency.psm1` carries the most behaviour of any new file and is
   the one most likely to reach the 500-line ceiling.** Scope Decision 5 names the remedy and P5-T19
   measures it.
4. **The repair pass's verifier depends on the restored `packages/` tree.** A repair that cannot be
   derived throws rather than emitting a guessed path, which converts a silent wrong answer into a
   visible failure but does mean the repair workflow requires a successful restore before it runs.
