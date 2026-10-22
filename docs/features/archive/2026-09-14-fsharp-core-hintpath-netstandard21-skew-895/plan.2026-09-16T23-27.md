# fsharp-core-hintpath-netstandard21-skew (Plan)

- **Issue:** #895
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-17 (date-only: the planner ran with no shell, so no minute-precision clock was observable; executor-written evidence keeps the `yyyy-MM-ddTHH-mm` convention)
- **Status:** Preflight cleared (revision R1; five mechanical corrections applied during executor preflight, recorded below)
- **Version:** 1.0
- **Work Mode:** full-bug (`spec.md` is the sole acceptance-criteria source; `user-story.md` is correctly absent)
- **Task Count:** 48 total, distributed 10 / 8 / 4 / 2 / 15 / 9 across Phases 0 to 5 (counted mechanically over `- [ ] [P#-T#]` lines after authoring; unique-ID count equals line count, every phase runs T1..Tn with no gap)

**Preflight Revision Record (R1).** `atomic-executor` reviewed this plan under `DIRECTIVE: PREFLIGHT VALIDATION ONLY` and applied five mechanical corrections. No task was added, removed or reordered; the count remains 48. (1) Every `spec.md` acceptance-criterion line citation was stale by exactly four lines because `spec.md` gained four lines in its `### Dependencies or blocked work:` bullet after this plan was authored; AC1 to AC5 are now cited at lines 422, 430, 444, 450 and 457, the `[P5-T6]` counting range at 422-465, and the AC5 prose citation at line 463, each re-derived against the current file. (2) `## Observed Corrections` item 1 asserted that `spec.md` denies the existence of issue #879's feature folder; `spec.md` now states the opposite, so the item is rewritten as a confirmed, already-applied correction rather than an outstanding one. (3) `[P3-T1]` said "the nine comment lines between `/// <remarks>` (line 363) and `/// </remarks>` (line 371)", but only seven lines lie strictly between those tags; the replacement block now carries both tag lines explicitly (thirteen lines replacing nine), so the `<remarks>` element AC5 is worded about survives the edit. (4) `[P3-T2]`'s arithmetic was updated to match: `NETSTANDARDBIND_LINES=470`, `CHANGED_LINES=22`, numstat `13	9`. The deletions figure of 9 that `[P4-T15]` gates on is unchanged. (5) `[P4-T11]` asserted `CHANGED-PRODUCTION-LINES: 0` from an anchored `git diff --name-only` alone, which enumerates tracked changes only and so could not report an untracked production `.cs` file; a `git status --porcelain --untracked-files=all` companion span now runs in the same task and is gated empty alongside it. `[P4-T15]`'s two re-anchored diff spans are additionally written out in full; both already carried `origin/main...HEAD`, and the expansion removes a bare `git diff` code span from the prose that a static scan reads as unanchored.

**Fail-closed evidence rule:** every baseline, regression, QA-gate and coverage task below names the exact artifact it must produce. If a named artifact is absent, or is present but missing a required field, the task is not complete and the verdict is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** every evidence-producing task records `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:` in its artifact. Where a gate is expected to exit non-zero, the artifact also carries `ExpectedExitCode:` with that integer. The one scoping carve-out is stated at `[P5-T9]`: a task that commits after writing its own artifact records the `EXIT_CODE:` of the pre-commit observation it can still observe, and reports the commit's own result in the executor's completion report.

**Evidence path shorthand (binding).** The prefix `.../evidence/` abbreviates, and resolves to, exactly `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/`. Every artifact lives under one of the canonical kinds `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`. No path under `artifacts/` is an evidence location and no task names one. Commands write the path out in full.

**Evidence timestamp token (binding).** Every artifact filename carries the fixed token `2026-09-16T23-27` (the plan's own token). An executor that invents another token has written to a path no task reads. The artifact's `Timestamp:` field records the write time.

---

## Defect Statement

The six `FSharp.Core` `HintPath` entries in the solution split three/three between the `lib\netstandard2.0` and `lib\netstandard2.1` flavours of `packages\FSharp.Core.11.0.100`. The netstandard2.1 flavour's own assembly-reference table names `netstandard, Version=2.1.0.0`, an identity that does not exist on .NET Framework, so that copy is unloadable wherever it is deployed. All six `Reference` elements carry the identical identity `FSharp.Core, Version=11.0.0.0`, so ResolveAssemblyReferences sees no conflict and which flavour lands in a transitive output directory is last-writer-wins build-order nondeterminism (research artifact section 2.2: rows 1-3 deterministic 2.1, rows 4-9 deterministic 2.0, rows 10-15 unspecified).

Re-derived on the assigned worktree on 2026-09-17 (Grep over `*.csproj`):

| Project file | HintPath line | Flavour today | Edited by this plan |
|---|---|---|---|
| `QuickFiler/QuickFiler.csproj` | 52 | netstandard2.1 | yes, `[P2-T1]` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 259 | netstandard2.1 | yes, `[P2-T2]` |
| `ToDoModel/ToDoModel.csproj` | 42 | netstandard2.1 | yes, `[P2-T3]` |
| `UtilitiesCS/UtilitiesCS.csproj` | 70 | netstandard2.0 | no |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 599 | netstandard2.0 | no |
| `ToDoModel.Test/ToDoModel.Test.csproj` | 96 | netstandard2.0 | no |

Eighteen `.csproj` files exist in the tree; the other twelve carry no `FSharp.Core` text. Every one of the eighteen declares `<OutputPath>bin\Debug\</OutputPath>` for `Debug|AnyCPU`.

## Authorised Write Set (exhaustive, repository-relative; identical to `spec.md` `## Write Set`)

1. `QuickFiler/QuickFiler.csproj` — one HintPath value edit at line 52.
2. `QuickFiler.Test/QuickFiler.Test.csproj` — one HintPath value edit at line 259.
3. `ToDoModel/ToDoModel.csproj` — one HintPath value edit at line 42.
4. `TaskMaster.Test/TaskMaster.Test.csproj` — two `Compile Include` items inserted after line 326.
5. `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` — new, Shape A.
6. `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` — new, Shape B.
7. `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` — XML-doc `<remarks>` block on `ProbeApplicationBase` (lines 363-371 today) only.

Documents and evidence, outside the source write set by convention: `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md` (acceptance check-off marks only), this plan file, and any path under `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/`.

Inherited-path rule (a rule, not a list): any path already changed relative to `origin/main` before `[P0-T8]` ran (captured mechanically by `[P0-T8]` as `INHERITED-CLAUSE-A:`), and any path under the prefix `.claude/agent-memory/` (clause B, written by agent harnesses during execution), is outside every scope assertion in this plan. Scope gates subtract the captured set and the prefix; they never subtract a Write Set path.

Untracked scratch output permitted and never committed: `TestResults/` (`.gitignore` line 39, `[Tt]est[Rr]esult*/`), `coverage/` contents (`.gitignore` line 144, `coverage/*`; `coverage/.gitkeep` is tracked so the directory always exists, and `coverage/logs/` is created by `[P0-T5]`), `packages/` (`.gitignore` line 191) and `.dotnet-sdk/` (`.gitignore` line 350).

## Non-Goals (binding; from `spec.md` `## Scope & Non-Goals`)

- Do not change the three already-correct HintPaths (`UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel.Test`).
- Do not change any `app.config`, `packages.config`, `.props`, `.targets`, runsettings or `coverage.config` file. The #879 `netstandard` redirect in `TaskMaster/app.config` lines 72-75 stays.
- Do not reformat, reorder or renumber any other item in the three edited project files; issue #898 (Meziantou analyzer HintPath skew, the `Meziantou.Analyzer.3.0.203` `<Analyzer Include>` items in fifteen project files against `packages.config` entries pinning `3.0.235`) is a separate run and no task edits an `<Analyzer Include>` line.
- Do not change test logic, assertions, attributes or constants in `NetstandardBindChildDomainTests.cs`, `ChildDomainBindProbe.cs`, `AddInEagerInstallShapeTests.cs` or `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`; the `because` message at `NetstandardBindChildDomainTests.cs` lines 206-207 is not touched.
- Do not add `[DoNotParallelize]` anywhere; do not modify `scripts/vscode/TaskMaster.cli.runsettings` (`Workers=0`, `Scope=ClassLevel`); never fix a parallel-execution failure with serialisation, retries, timing tolerance or sleeps.
- Do not fix latent defect 1 (`ToDoModel.Test/packages.config` lacks `FSharp.Core` and `Deedle` entries) or latent defect 2 (`scripts/vscode/Sync-PackageReferences.ps1` lines 13-19 rank `netstandard2.1` ahead of `netstandard2.0`). `[P5-T6]` records both as follow-ups.
- Pull-request authoring and CI monitoring are out of scope for this plan's execution run.

## Observed Corrections to the Requirements Documents (recorded, not amended)

1. Already applied upstream, recorded here as confirmed rather than outstanding. An earlier revision of this plan recorded that `spec.md` denied the existence of any `879` folder under `docs/features`. `spec.md` has since been corrected: its `### Dependencies or blocked work:` bullet at lines 257-267 now states that issue #879's active feature folder `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/` still exists on disk with a fully checked plan and has not yet been archived, and that the comment-only edit targets `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`, ordinary shared repository test code outside #879's own feature folder. Re-derived on the assigned worktree: the folder exists, its `plan.2026-09-13T18-22.md` carries zero unchecked task lines, and its `evidence/other/preflight-clearance.2026-09-14T01-55.md` records the completed run. The ownership conclusion is unchanged: #879 is delivered and the comment-only edit belongs to this issue; nothing in this plan writes into #879's canonical feature-folder state. `[P0-T1]` records this item as a confirmed, already-applied correction and must not restate the superseded claim as current `spec.md` text.
2. AC5 (`spec.md` line 463) says a grep of the unfixed file for `unsatisfiable` returns the stale sentence at lines 366-367. Re-derived: the token occurs at line 281 (the `NegativeControl_WithoutInstall_Netstandard21Throws` summary, which stays true after the fix and is retained) and at line 366 (the stale sentence). The pre-fix count is therefore 2 and the post-fix count is 1; `[P0-T9]` records the baseline count and `[P3-T2]` gates the transition.

## Run Environment Constraints (binding on every command task)

**WORKTREE-ROOT.** The delegation prompt supplies the absolute path of the execution worktree. Every `pwsh -NoProfile -Command` payload in this plan begins with the two statements below, written here once and referred to as **WT-PREAMBLE**; the executor substitutes the supplied path for the token `WORKTREE-ROOT` and never writes that absolute path into any evidence artifact (artifacts record the leaf directory name only, under `WORKTREE-LEAF:`). The second statement is required because `Set-Location` does not update .NET's `CurrentDirectory`, so `System.IO` calls with relative paths would otherwise escape the worktree.

```
Set-Location -LiteralPath "WORKTREE-ROOT"
[System.IO.Directory]::SetCurrentDirectory((Get-Location).Path)
```

**Command channel (two-rung probe, `[P0-T2]`).** This repository's worktree-isolation filter on the Bash tool refuses opaque executables (`pwsh`, `msbuild`, `dotnet`) in some sessions and not others. The refusal text, recorded by the research artifact section 5, is: `this command runs pwsh in a plain command; what it reads or is handed as shell text cannot be shown not to run git. Refusing to run it.` `[P0-T2]` probes both `pwsh -NoProfile -Command` and `pwsh -NoProfile -File` once. If either rung is refused with that text, the executor records the refusal verbatim in the `[P0-T2]` artifact as `CHANNEL: REFUSED`, executes no later command task, marks no later command task complete, and stops and reports so the caller can relaunch in a non-isolated context. A refused gate is never recorded as passed and never skipped silently.

**BUILD-LOCK (conditional).** When the delegation prompt supplies a build-lock directory (on this machine the sibling directory `parallel-build-lock` beside the item worktrees, holding `acquire.txt` and `release.txt`; both take `-Item`), every `msbuild`, `dotnet`, `csharpier` and `vstest` task acquires before and releases after, with `-Item "895"`: acquire must print `ACQUIRED 895` and exit 0; release must print `RELEASED by 895` and exit 0; `TIMEOUT` means report blocked, never force. When no lock directory is supplied, `[P0-T2]` records `BUILD-LOCK: NOT SUPPLIED` and the tasks run unlocked. The lock scripts are invoked as `pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "BUILD-LOCK-DIR/acquire.txt"))) -Item "895"'` with the supplied directory substituted for `BUILD-LOCK-DIR`.

**Outlook gate.** A running Outlook locks the add-in build output, so every task that runs `msbuild` first confirms `@(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue).Count` is `0`. A non-zero count is a stop-and-report condition; the executor never terminates the process.

**Shell discipline.** The permission engine allows only `git *`, `pwsh *`, `poetry run *` and three library scripts, and splits on `&&`, `;` and `|`, so no `cd`, `grep`, `sed`, `cat` or pipeline between commands appears anywhere in this plan; `|` occurs only inside single-quoted `pwsh` payloads, where it is one Bash segment. Every payload uses outer single quotes and inner double quotes, newline-separated statements, and `@(...)` plus `.Where({ })` instead of pipelines where practical. Every `git commit` carries explicit pathspec operands. No task chains a state write with a gated command.

**Tool resolution.** `msbuild` and `vstest.console.exe` are not on `PATH` and no shell variable survives a task boundary, so each payload resolves its own tool, using these two blocks verbatim (**MSBUILD-RESOLVE** and **VSTEST-RESOLVE**):

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
```

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
```

`scripts/vscode/Invoke-VSBuild.ps1` is not used: it runs `scripts/vscode/Sync-PackageReferences.ps1` first, which rewrites project-file HintPaths.

**Toolchain invariants.** `/t:Rebuild`, never `/t:Build`; never `/p:Nullable=enable`; CSharpier 1.2.6 through `dotnet tool run csharpier format .` and `dotnet tool run csharpier check .` (the `check` success line begins `Checked ` and ends `ms.`; `format` prints `Formatted N files in`, where N is the processed count, not a rewrite count). The MSBuild success signal is the anchored line `^\s+0 Error\(s\)$` (never a bare `0 Error(s)` substring, which `10 Error(s)` also contains). Every zero-count assertion read from an msbuild log is paired in the same task with a positive control from the same log.

**Raw console logs.** Every `msbuild` task redirects all streams with `*>` to a file under `coverage/logs/` (git-ignored). The `.md` artifact records counts read from the log; no raw log, no `.trx` and no `.cobertura.xml` is committed, so no absolute host path reaches the tree. Scoped `vstest` runs use `/ResultsDirectory:TestResults/<task-id>` plus `"/Logger:trx;LogFileName=<task-id>.trx"` (the quotes are load-bearing; an unquoted `;` degrades to a bare `/Logger:trx` named after the account and host).

**vstest counter derivation.** An all-green `vstest.console.exe` run prints only `Test Run Successful.`, `Total tests:`, `Passed:` and `Total time:`; it prints no `Failed:` and no `Skipped:` line. Every count this plan gates on is therefore read from the TRX `ResultSummary/Counters` attributes `total`, `executed`, `passed`, `failed` (never a `skipped` attribute, which does not exist) using this block verbatim (**TRX-READ**, with `<id>` substituted):

```
$m = @(Get-ChildItem -LiteralPath "TestResults/<id>" -Filter "<id>.trx" -Recurse)
Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
$x = [xml](Get-Content -LiteralPath $m[0].FullName -Raw)
$c = $x.TestRun.ResultSummary.Counters
Write-Output ("COUNTERS_TOTAL=" + $c.total + " EXECUTED=" + $c.executed + " PASSED=" + $c.passed + " FAILED=" + $c.failed)
foreach ($r in @($x.TestRun.Results.UnitTestResult)) { Write-Output ($r.testName + " OUTCOME=" + $r.outcome) }
foreach ($r in @($x.TestRun.Results.UnitTestResult).Where({ $_.outcome -eq "Failed" })) { Write-Output ("FAILED_MESSAGE[" + $r.testName + "]=" + $r.Output.ErrorInfo.Message) }
```

Each scoped run removes any earlier TRX from its own results directory first and asserts `TRX_MATCH_COUNT=1`, so the file read is this run's.

**Scoped-run command shape (SCOPED-RUN, with `<id>` and `<filter>` substituted).** `vstest.console.exe` never compiles; every scoped run in this plan is preceded in the same phase by a whole-solution `/t:Rebuild`.

```
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"<filter>" "/Logger:trx;LogFileName=<id>.trx" /ResultsDirectory:TestResults/<id>
$LASTEXITCODE
```

**Full-suite coverage runner.** `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` (PowerShell 7 only) discovers every `*.Test.dll` under `bin\Debug\` with the nested-worktree exclusion applied to the path relative to the search root (line 335), wraps `vstest.console.exe` in `dotnet-coverage collect` with `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` (line 91), writes `coverage/coverage.cobertura.xml` and `coverage/test-results/mstest-coverage-run.trx`, and appends the `.*\.Test\.dll$` module exclusion at run time (line 117), so the test assemblies, including the two new test files, are outside the coverage denominator. Three exit paths matter: (a) any failing test makes it throw at line 262 BEFORE post-processing at lines 382-384, leaving the RAW collector document; (b) the 80 percent line threshold (`Invoke-MSTestWithCoverage.Threshold.ps1` line 52) and 75 percent branch threshold (line 122) throw AFTER the post-processed document is written; (c) exit 0. The document state is decided from the runner's console text, captured to `coverage/logs/<task-id>-runner.txt`: `MSTest with coverage failed with exit code` means `RAW-COLLECTOR-OUTPUT`; `is below the required` means `POSTPROCESSED` with a threshold breach; neither, with exit 0, means `POSTPROCESSED`. The repository-wide document-level `line-rate` is not reproducible across runs of an identical tree (denominator swings recorded up to a factor of nearly two), so `[P4-T11]` compares rates only on comparable denominators.

**Execution risk, recorded not gated.** Four `UtilitiesCS.Test` shell-icon classes have stalled `vstest.console.exe` inside `SHGetFileInfo` on this machine in the past (recorded 2026-09-04; the #879 run on 2026-09-13 completed in about 65 seconds without a stall). The runner has no filter extension point. If a full-suite run stalls past the Bash tool's ceiling, the task records `STALL-OBSERVED` and re-runs the same command once; a second stall is a stop-and-report condition. `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` (issue #780) and the two `ItemViewerBreadcrumbThreadAffinityTests` worker-thread tests are known intermittent failures; `[P0-T7]` records whichever of them fail at baseline so `[P4-T9]` can classify a recurrence.

**Nullable context decision.** Both existing files in `TaskMaster.Test/Bootstrap/` carry no `#nullable` directive; the two new files follow that sibling convention (no directive) so that the nullable gate's `CS86xx` promotion cannot reach code the sibling files do not subject to it. `TaskMaster.Test.csproj` line 18 sets `<LangVersion>latest</LangVersion>`, so tuples and pattern matching are available.

**Dependency decision.** `TaskMaster.Test.csproj` already references `System.Reflection.Metadata 10.0.0.12` (lines 241-242) and `System.Collections.Immutable 10.0.0.12` (lines 204-205, with the binding redirect at `TaskMaster.Test/app.config` line 47); `packages.config` lines 157 and 166 pin both. No dependency edit is made. `PEReader` and `MetadataReader` have no prior use in the repository; this is a first use.

---

### Phase 0 — Policy Reading, Channel Probe, and Baseline Capture

- [x] [P0-T1] Read, in this exact order, `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/csharp.md`, `.claude/rules/tonality.md` and `.claude/rules/plan-acceptance-gates.md`; then read in full `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/issue.md`, `.../spec.md` and `.../research/2026-09-16T23-30-fsharp-core-hintpath-research.md`. Write `.../evidence/baseline/phase0-instructions-read.2026-09-16T23-27.md` with `Timestamp:`, `Policy Order:`, the explicit list of the seven policy files in that order, a `Requirements Sources:` section listing the three feature documents, the line `AC source: spec.md section "## Acceptance Criteria", 5 criteria`, a `Threshold Authority Note:` stating that `CLAUDE.md` (first in the compliance order) fixes the C# floors at `>= 80%` repository-wide on the testable denominator, `>= 90%` for new modules and no regression on changed lines, and an `Observed Corrections:` section reproducing items 1 and 2 of this plan's `## Observed Corrections to the Requirements Documents`.
      Acceptance: the artifact exists, names all ten files, carries the exact `AC source:` line above, and carries both observed-correction items.
- [x] [P0-T2] Command-channel probe, worktree identity, build-lock presence and Outlook gate, recorded in `.../evidence/baseline/channel-probe.2026-09-16T23-27.md`. Rung 1, the trivial `-Command` form: `pwsh -NoProfile -Command 'Write-Output ok'`. Rung 2, the `-File` form, which is also the first bootstrap step and is idempotent: `pwsh -NoProfile -File "WORKTREE-ROOT/scripts/vscode/Install-RepoDotNetSdk.ps1"` with the absolute worktree root substituted (a relative script path would resolve against whatever directory the Bash tool starts `pwsh` in, which in a non-isolated launch is another checkout). The installer's success-case output is `Installed repo-local .NET SDK 8.0.205 to` on first install or `Repo-local .NET SDK 8.0.205 is already installed at` thereafter; the installer sets no `$LASTEXITCODE` of its own, so its result is read from the `.dotnet-sdk/sdk/8.0.205` marker directory it creates (`Install-RepoDotNetSdk.ps1` lines 56 and 102-104). Then, in one payload with WT-PREAMBLE:

      ```
      pwsh -NoProfile -Command '
      Set-Location -LiteralPath "WORKTREE-ROOT"
      [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path)
      Write-Output ("WORKTREE-LEAF=" + (Split-Path -Leaf (Get-Location).Path))
      Write-Output ("TOPLEVEL-LEAF=" + (Split-Path -Leaf ((git rev-parse --show-toplevel) -replace "/", "\")))
      Write-Output ("SOLUTION-PRESENT=" + (Test-Path -LiteralPath "TaskMaster.sln"))
      Write-Output ("SPEC-PRESENT=" + (Test-Path -LiteralPath "docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md"))
      Write-Output ("SDK-MARKER-PRESENT=" + (Test-Path -LiteralPath ".dotnet-sdk/sdk/8.0.205"))
      Write-Output ("OUTLOOK-PROCESS-COUNT=" + @(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue).Count)
      '
      ```

      Record `CHANNEL:` as `PWSH-COMMAND=OK PWSH-FILE=OK` when both rungs ran, or `REFUSED` followed by the refusal text verbatim when either was refused; record `BUILD-LOCK:` as `SUPPLIED` (with the leaf name of the supplied directory) or `NOT SUPPLIED`; record the six emitted lines under `Output Summary:`.
      Acceptance: `CHANNEL: PWSH-COMMAND=OK PWSH-FILE=OK`; `WORKTREE-LEAF` equals `TOPLEVEL-LEAF`; `SOLUTION-PRESENT=True`; `SPEC-PRESENT=True`; `SDK-MARKER-PRESENT=True`; `OUTLOOK-PROCESS-COUNT=0`; `BUILD-LOCK:` carries one of the two permitted values. Any `CHANNEL: REFUSED` outcome, any identity mismatch, or a non-zero Outlook count stops the plan here with a blocked report; no later command task runs or is marked.
- [x] [P0-T3] Complete the toolchain bootstrap under BUILD-LOCK, recorded in `.../evidence/baseline/toolchain-bootstrap.2026-09-16T23-27.md`. Step 1: `pwsh -NoProfile -Command 'WT-PREAMBLE
      dotnet tool restore
      $LASTEXITCODE'` (success prints `Tool 'csharpier' (version '1.2.6') was restored.`). Step 2, the packages.config restore, with MSBUILD-RESOLVE inside the payload:

      ```
      & $msb TaskMaster.sln /t:Restore /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:RestorePackagesConfig=true
      $LASTEXITCODE
      ```

      Step 3, the analyzer-package premise. Fifteen first-party project files (every one except `TaskMaster/TaskMaster.csproj`, whose line 575 already names `3.0.235`) carry `<Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />` (for example `TaskMaster.Test/TaskMaster.Test.csproj` line 386) while every `packages.config` pins `3.0.235` (for example `TaskMaster.Test/packages.config` line 12), so step 2 materialises no `3.0.203` folder and every later build would fail with `CS0006`. That skew is issue #898 and no project file is edited for it. Run:

      ```
      pwsh -NoProfile -Command '
      Set-Location -LiteralPath "WORKTREE-ROOT"
      [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path)
      $p = "packages/Meziantou.Analyzer.3.0.203/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll"
      Write-Output ("MEZIANTOU_203_PRESENT_BEFORE=" + (Test-Path -LiteralPath $p))
      if (-not (Test-Path -LiteralPath $p)) { nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -NonInteractive }
      Write-Output ("MEZIANTOU_203_PRESENT_AFTER=" + (Test-Path -LiteralPath $p))
      Write-Output ("PACKAGES_DIR_COUNT=" + @(Get-ChildItem -LiteralPath "packages" -Directory).Count)
      Write-Output ("DOTNET_COVERAGE_RESOLVED=" + ($null -ne (Get-Command "dotnet-coverage" -ErrorAction SilentlyContinue)))
      $m = Get-Content -LiteralPath "dotnet-tools.json" -Raw | ConvertFrom-Json
      Write-Output ("CSHARPIER_PINNED_VERSION=" + $m.tools.csharpier.version)
      '
      ```

      The `nuget install` writes only under `packages/` (git-ignored) and reproduces the package set CI builds against. If `nuget` is not resolvable, or `DOTNET_COVERAGE_RESOLVED=False` after one `dotnet tool install --global dotnet-coverage` followed by a separate re-probe payload, record the observed text verbatim and stop and report. Record every command, each `EXIT_CODE:`, and the emitted lines.
      Acceptance: step 1 and step 2 exit 0; `MEZIANTOU_203_PRESENT_AFTER=True`; `PACKAGES_DIR_COUNT` greater than 0; `DOTNET_COVERAGE_RESOLVED=True`; `CSHARPIER_PINNED_VERSION=1.2.6`.
- [x] [P0-T4] Format baseline, read-only, under BUILD-LOCK, written to `.../evidence/baseline/format-baseline.2026-09-16T23-27.md`: `pwsh -NoProfile -Command 'WT-PREAMBLE
      dotnet tool run csharpier check .
      $LASTEXITCODE'`. Write `.../evidence/baseline/format-baseline.2026-09-16T23-27.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 0`, and an `Output Summary:` carrying the `Checked N files in` line verbatim and `UNFORMATTED-FILE-COUNT:` as an integer (the number of file paths CSharpier named; 0 on a clean tree). This is the read-only `check` subcommand, so the exit code distinguishes a clean tree from a drifted one and no file is rewritten.
      Acceptance: the artifact exists and records the exit code and the integer count. A non-zero count is recorded, not repaired, here; it becomes the drift list `[P4-T5]` consults, and a drifted path inside the Write Set is a stop-and-report condition before Phase 1.
- [x] [P0-T5] Analyzer baseline under BUILD-LOCK with the Outlook gate re-checked in the same payload. Payload: WT-PREAMBLE, `New-Item -ItemType Directory -Force -Path "coverage/logs" | Out-Null`, MSBUILD-RESOLVE, then

      ```
      & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true *> "coverage/logs/p0-t5-analyzer.txt"
      $LASTEXITCODE
      ```

      Then measure the log in a second payload:

      ```
      pwsh -NoProfile -Command '
      Set-Location -LiteralPath "WORKTREE-ROOT"
      [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path)
      $log = "coverage/logs/p0-t5-analyzer.txt"
      Write-Output ("ZERO_ERRORS_LINES=" + @(Select-String -LiteralPath $log -Pattern "^\s+0 Error\(s\)$").Count)
      Write-Output ("ERROR_SUMMARY_LINES=" + @(Select-String -LiteralPath $log -Pattern "^\s+\d+ Error\(s\)$").Count)
      Write-Output ("SKIPPED_CORECOMPILE=" + @(Select-String -LiteralPath $log -Pattern "Skipping target .CoreCompile.").Count)
      Write-Output ("CSC_OUT_LINES=" + @(Select-String -LiteralPath $log -SimpleMatch -Pattern "/out:obj\Debug\").Count)
      '
      ```

      `CSC_OUT_LINES` counts the echoed compiler command lines, one per project that compiled, and is the positive control that makes `SKIPPED_CORECOMPILE=0` an observation rather than an artefact of an empty log. Write `.../evidence/baseline/analyzer-baseline.2026-09-16T23-27.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 0` and an `Output Summary:` carrying the `Warning(s)`/`Error(s)` summary values and the four counts.
      Acceptance: `EXIT_CODE: 0`, `ZERO_ERRORS_LINES` at least 1, `ERROR_SUMMARY_LINES` equal to `ZERO_ERRORS_LINES`, `SKIPPED_CORECOMPILE=0`, `CSC_OUT_LINES` at least 15.
- [x] [P0-T6] Nullable baseline: identical to `[P0-T5]` with the property list replaced by `/p:TreatWarningsAsErrors=true` and the log path replaced by `coverage/logs/p0-t6-nullable.txt`. Do not add `/p:Nullable=enable`. Write `.../evidence/baseline/nullable-baseline.2026-09-16T23-27.md` with the same fields and the same four counts.
      Acceptance: `EXIT_CODE: 0`, `ZERO_ERRORS_LINES` at least 1, `ERROR_SUMMARY_LINES` equal to `ZERO_ERRORS_LINES`, `SKIPPED_CORECOMPILE=0`, `CSC_OUT_LINES` at least 15. This build also produces the `bin\Debug` output tree `[P0-T7]` runs against.
- [x] [P0-T7] Coverage-bearing test baseline under BUILD-LOCK via `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, on the tree before any test file exists. Payload: WT-PREAMBLE then

      ```
      & "scripts/vscode/Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug *> "coverage/logs/p0-t7-runner.txt"
      $LASTEXITCODE
      ```

      Then read the figures:

      ```
      pwsh -NoProfile -Command '
      Set-Location -LiteralPath "WORKTREE-ROOT"
      [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path)
      $log = "coverage/logs/p0-t7-runner.txt"
      Write-Output ("RUNNER_THREW_ON_TESTS=" + @(Select-String -LiteralPath $log -SimpleMatch -Pattern "MSTest with coverage failed with exit code").Count)
      Write-Output ("RUNNER_THREW_ON_THRESHOLD=" + @(Select-String -LiteralPath $log -SimpleMatch -Pattern "is below the required").Count)
      Write-Output ("DISCOVERED_LINE=" + @(Select-String -LiteralPath $log -SimpleMatch -Pattern "Discovered ").Count)
      $x = [xml](Get-Content -LiteralPath "coverage/coverage.cobertura.xml" -Raw)
      $c = $x.DocumentElement
      Write-Output ("DOC_LINE_RATE=" + $c.GetAttribute("line-rate") + " DOC_LINES_VALID=" + $c.GetAttribute("lines-valid") + " DOC_LINES_COVERED=" + $c.GetAttribute("lines-covered") + " DOC_BRANCH_RATE=" + $c.GetAttribute("branch-rate"))
      $t = [xml](Get-Content -LiteralPath "coverage/test-results/mstest-coverage-run.trx" -Raw)
      $k = $t.TestRun.ResultSummary.Counters
      Write-Output ("COUNTERS_TOTAL=" + $k.total + " EXECUTED=" + $k.executed + " PASSED=" + $k.passed + " FAILED=" + $k.failed)
      foreach ($r in @($t.TestRun.Results.UnitTestResult).Where({ $_.outcome -eq "Failed" })) { Write-Output ("BASELINE_FAILED=" + $r.testName) }
      '
      ```

      Write `.../evidence/baseline/test-coverage-baseline.2026-09-16T23-27.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode:` (0 when `COUNTERS_FAILED` is 0 and no threshold literal matched, otherwise 1), `Cobertura Document State:` (`RAW-COLLECTOR-OUTPUT` when `RUNNER_THREW_ON_TESTS` is 1, else `POSTPROCESSED`), an `Output Summary:` carrying every emitted line, a `Baseline Failing Set:` heading listing every `BASELINE_FAILED=` name or the literal `NONE`, and a `Coverage Obligations:` section stating the `80 / 90 / no-regression` floors, `CHANGED-PRODUCTION-LINES-EXPECTED: 0` (this fix changes no production `.cs` file) and `NEW-MODULE-COVERAGE-EXPECTED: N/A` (the new files are test code, excluded from instrumentation by the runner's `.*\.Test\.dll$` exclusion).
      Acceptance: the artifact exists; `DISCOVERED_LINE=1`; the four `DOC_` values and the four `COUNTERS_` values are present as numbers, not placeholders; `Cobertura Document State:` carries exactly one of the two permitted values; the `Baseline Failing Set:` heading exists. A stall is handled per the execution-risk rule; a missing numeric value blocks.
- [x] [P0-T8] Tree baseline and diff anchors, recorded in `.../evidence/baseline/tree-baseline.2026-09-16T23-27.md`. Commands, in this order:

      ```
      git fetch origin
      git rev-parse --verify origin/main
      git merge-base HEAD origin/main
      git rev-parse HEAD
      git diff --numstat origin/main -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj UtilitiesCS/UtilitiesCS.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj ToDoModel.Test/ToDoModel.Test.csproj
      git diff --name-only origin/main...HEAD
      git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"
      git status --porcelain --untracked-files=all
      ```

      Record the two SHAs as observations (`ORIGIN-MAIN-SHA:`, `MERGE-BASE-SHA:`, `HEAD-SHA:`; none is a plan expectation), the six-file numstat under `Baseline Six-File Numstat:` (empty output recorded as the literal `NONE`), the name-only list under `INHERITED-CLAUSE-A:` (this is the captured clause-A set the scope gates subtract), the excluded-pathspec porcelain under `Baseline Porcelain Status:` and the full porcelain under `Baseline Porcelain Full:`, each empty output as `NONE`. The anchor is `origin/main` after a fetch, never local `main`, because in a worktree-per-item run only the pulling checkout advances local `main`.
      Acceptance: `git rev-parse --verify origin/main` exits 0; `Baseline Six-File Numstat:` is the literal `NONE` (any line here means the branch already differs from `origin/main` in a HintPath-bearing project file, which makes AC3 unsatisfiable as worded: stop and report rather than proceeding); `Baseline Porcelain Status:` names no path ending `.cs`, `.csproj`, `packages.config` or `app.config` (docs, evidence and agent-memory paths are expected and recorded, never asserted empty); the `INHERITED-CLAUSE-A:` heading exists with its output or `NONE`.
- [x] [P0-T9] Source census of the tokens later gates transition, appended to `.../evidence/baseline/tree-baseline.2026-09-16T23-27.md` under a `Source Census:` heading. Payload with WT-PREAMBLE:

      ```
      $six = @("QuickFiler/QuickFiler.csproj","QuickFiler.Test/QuickFiler.Test.csproj","ToDoModel/ToDoModel.csproj","UtilitiesCS/UtilitiesCS.csproj","UtilitiesCS.Test/UtilitiesCS.Test.csproj","ToDoModel.Test/ToDoModel.Test.csproj")
      foreach ($p in $six) { Write-Output ($p + " NS21=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "lib\netstandard2.1\FSharp.Core.dll").Count + " NS20=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "lib\netstandard2.0\FSharp.Core.dll").Count) }
      $f = "TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs"
      Write-Output ("UNSATISFIABLE_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "unsatisfiable").Count)
      Write-Output ("DISPLAY_NAME_TESTS_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "display-name tests").Count)
      Write-Output ("DONOTPARALLELIZE_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "DoNotParallelize").Count)
      Write-Output ("TESTMETHOD_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "[TestMethod]").Count)
      Write-Output ("BECAUSE_206_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "the flavour of FSharp.Core deployed beside Deedle is what determines ").Count)
      Write-Output ("NETSTANDARDBIND_LINES=" + @(Get-Content -LiteralPath $f).Count)
      Write-Output ("TASKMASTER_TEST_CSPROJ_LINES=" + @(Get-Content -LiteralPath "TaskMaster.Test/TaskMaster.Test.csproj").Count)
      Write-Output ("BOOTSTRAP_CS_FILES=" + @(Get-ChildItem -LiteralPath "TaskMaster.Test/Bootstrap" -Filter "*.cs").Count)
      Write-Output ("SHAPE_A_PRESENT=" + (Test-Path -LiteralPath "TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs"))
      Write-Output ("SHAPE_B_PRESENT=" + (Test-Path -LiteralPath "TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs"))
      Write-Output ("CSPROJ_FILES=" + @(Get-ChildItem -Path . -Recurse -Filter "*.csproj").Where({ $_.FullName.Substring((Get-Location).Path.Length) -notmatch "\\(\.[^\\]+|packages|bin|obj|node_modules)\\" }).Count)
      ```

      The `BECAUSE_206_COUNT` literal is the first line of the `because` message at lines 206-207 (`"the flavour of FSharp.Core deployed beside Deedle is what determines "`), which this plan never touches; it is asserted here and again at `[P3-T2]` and `[P4-T12]`.
      Acceptance: the three edited files read `NS21=1 NS20=0` and the three untouched files read `NS21=0 NS20=1`; `UNSATISFIABLE_COUNT=2`; `DISPLAY_NAME_TESTS_COUNT=0`; `DONOTPARALLELIZE_COUNT=2`; `TESTMETHOD_COUNT=9`; `BECAUSE_206_COUNT=1`; `NETSTANDARDBIND_LINES=466`; `BOOTSTRAP_CS_FILES=3`; `SHAPE_A_PRESENT=False`; `SHAPE_B_PRESENT=False`; `CSPROJ_FILES=18`. Any other value means the tree is not the one this plan was authored against: stop and report.
- [x] [P0-T10] Commit the Phase 0 evidence under `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/` so later anchored diffs have a base that includes it. Commands:

      ```
      git add -- docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895
      git commit -m "docs(895): record Phase 0 policy-read, channel-probe and baseline evidence" -- docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895
      ```

      Acceptance: the commit exits 0, and `git status --porcelain --untracked-files=all -- QuickFiler QuickFiler.Test ToDoModel TaskMaster.Test UtilitiesCS UtilitiesCS.Test ToDoModel.Test` returns no output (no source or project file has been touched yet; a line here means a bootstrap step wrote into the tracked source tree, which blocks).

### Phase 1 — Regression Tests First, Observed Failing on the Unfixed Tree

Both new test files and their project-file registration land in this phase, before the fail-first runs and before any HintPath edit; Phase 2 carries the fix only. The identifiers below are fixed by this plan so that every later count gate is mechanical; the executor selects no name.

- [x] [P1-T1] Create `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` (Shape A, static source assertion). Contents, fixed:
      `using` directives for `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Xml.Linq`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`; namespace `TaskMaster.Test.Bootstrap`; no `#nullable` directive (sibling convention); `[TestClass] public class FSharpCoreHintPathAlignmentTests` with an XML `<summary>` stating it guards the invariant that every `FSharp.Core` HintPath selects the `netstandard2.0` flavour. Members:
      - `private const string ExpectedHintPathSuffix = @"\lib\netstandard2.0\FSharp.Core.dll";`
      - `private const int ExpectedHintPathCount = 6;`
      - `private const string SolutionFileName = "TaskMaster.sln";`
      - `private static readonly string[] SkippedDirectoryNames = { "packages", "bin", "obj", "node_modules" };`
      - `internal static string FindRepositoryRoot()` — walks up from `AppDomain.CurrentDomain.BaseDirectory` to the first directory containing `TaskMaster.sln` (precedent `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` lines 422-437) and throws `InvalidOperationException` naming the start directory when none is found.
      - `internal static IReadOnlyList<(string ProjectPath, string HintPathValue)> DiscoverFSharpCoreHintPaths()` — enumerates every `*.csproj` under the repository root with a manual recursive walk that skips any directory whose name begins with `.` (which subsumes `.git` and `.claude`, and also covers `.dotnet-sdk` and `.vs`) or is in `SkippedDirectoryNames` (ordinal-ignore-case); loads each with `XDocument.Load`; collects every element whose `Name.LocalName` is `HintPath` and whose value ends with `FSharp.Core.dll` (`StringComparison.OrdinalIgnoreCase`); returns the pairs ordered by `ProjectPath` with `StringComparer.Ordinal`.
      - `private static string RelativeToRoot(string root, string path)` — `path.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)`.
      - `[TestMethod] public void SolutionHasExactlySixFSharpCoreHintPaths()` — Arrange/Act: `DiscoverFSharpCoreHintPaths()`; Assert: `hintPaths.Select(h => RelativeToRoot(root, h.ProjectPath)).Should().HaveCount(ExpectedHintPathCount, "...")` with a `because` stating that a deleted HintPath or a renamed package folder would otherwise pass the flavour test vacuously.
      - `[TestMethod] public void EveryFSharpCoreHintPath_SelectsNetstandard20()` — Act: `offenders` = every pair whose value does not end with `ExpectedHintPathSuffix` (ordinal-ignore-case), projected to the string `RelativeToRoot(root, ProjectPath) + ": " + HintPathValue`; Assert: `offenders.Should().BeEmpty("...")` with a `because` stating that the netstandard2.1 flavour references an identity that does not exist on .NET Framework. FluentAssertions prints the offending items in the failure message, which is what `[P1-T6]` reads.
      Every test follows Arrange-Act-Assert, carries an XML `<summary>`, uses no clock, RNG, process, AppDomain, temporary file or shared mutable state, and carries no `[DoNotParallelize]`. The literals `SolutionHasExactlySixFSharpCoreHintPaths`, `EveryFSharpCoreHintPath_SelectsNetstandard20`, `DiscoverFSharpCoreHintPaths`, `FindRepositoryRoot` and `ExpectedHintPathSuffix` are quoted here in prose because they are absent from the tree until this task runs.
      Acceptance: the file exists; contains the literal `[TestMethod]` exactly 2 times, `SolutionHasExactlySixFSharpCoreHintPaths` and `EveryFSharpCoreHintPath_SelectsNetstandard20` exactly once each as method names, `internal static` exactly 2 times, `#nullable` 0 times and `DoNotParallelize` 0 times; `[P1-T4]` records the counts.
- [x] [P1-T2] Create `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` (Shape B, post-build deployed-binary assertion via `System.Reflection.Metadata`). Contents, fixed:
      `using` directives for `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Reflection.Metadata`, `System.Reflection.PortableExecutable`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`; namespace `TaskMaster.Test.Bootstrap`; no `#nullable` directive; `[TestClass] public class FSharpCoreDeployedIdentityTests`. Members:
      - `private const string NetstandardAssemblyName = "netstandard";`
      - `private const string FSharpCoreFileName = "FSharp.Core.dll";`
      - `private const string ControlFlavourDirectory = "netstandard2.1";`
      - `private static readonly Version Netstandard20 = new Version(2, 0, 0, 0);` and `Netstandard21 = new Version(2, 1, 0, 0);`
      - `internal static IReadOnlyList<(string Name, Version Version)> ReadAssemblyReferences(string assemblyPath)` — throws `InvalidOperationException` carrying the full path when the file does not exist (fail loud, never skip; precedent `NetstandardBindChildDomainTests.cs` lines 412-434); opens `new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.Read)` inside `using`, wraps it in `using var reader = new PEReader(stream)`, calls `reader.GetMetadataReader()`, and for each handle in `metadata.AssemblyReferences` returns `(metadata.GetString(reference.Name), reference.Version)`. No assembly is loaded into any AppDomain, which is why fifteen same-identity files can be read in one process.
      - `private static string DeployedFSharpCorePath(string projectDirectoryName)` — `Path.Combine(FSharpCoreHintPathAlignmentTests.FindRepositoryRoot(), projectDirectoryName, "bin", "Debug", FSharpCoreFileName)`.
      - `[DataTestMethod]` followed by exactly fifteen `[DataRow]` attributes, one per directory in this fixed order, each with `DisplayName` set to the method name followed by a space and the directory name in square brackets, for example `[DataRow("QuickFiler", DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler]")]`: `QuickFiler`, `QuickFiler.Test`, `ToDoModel`, `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel.Test`, `Tags`, `Tags.Test`, `VBFunctions.Test`, `TaskTree`, `TaskTree.Test`, `TaskVisualization`, `TaskVisualization.Test`, `TaskMaster`, `TaskMaster.Test`; then `public void DeployedFSharpCore_ReferencesNetstandard20(string projectDirectoryName)` — Act: `references = ReadAssemblyReferences(DeployedFSharpCorePath(projectDirectoryName))`, `netstandard = references.Where(r => r.Name == NetstandardAssemblyName).Select(r => r.Version).ToList()`; Assert: `netstandard.Should().ContainSingle("...").Which.Should().Be(Netstandard20, "...")` and `references.Select(r => r.Version).Should().NotContain(Netstandard21, "...")`, every `because` naming `projectDirectoryName`. The bracketed `DisplayName` suffix is what makes `[QuickFiler]` distinguishable from `[QuickFiler.Test]` in a TRX.
      - `[TestMethod] public void Detector_OnPackageNetstandard21Binary_Reports21()` — Arrange: `hintPaths = FSharpCoreHintPathAlignmentTests.DiscoverFSharpCoreHintPaths()`, `hintPaths.Should().NotBeEmpty("...")`; resolve the first pair's value against its project directory with `Path.GetFullPath(Path.Combine(Path.GetDirectoryName(first.ProjectPath), first.HintPathValue))`, take `Path.GetDirectoryName` twice to reach the package `lib` folder, and combine `ControlFlavourDirectory` and `FSharpCoreFileName` (the package version is derived, never hard-coded; the derivation is correct whichever flavour the first HintPath selects). Act: `ReadAssemblyReferences(controlPath)`. Assert: the single `netstandard` reference version `.Should().Be(Netstandard21, "...")`. This is the positive control that proves the detector can see a `2.1.0.0` reference at all.
      Same determinism and parallelism rules as `[P1-T1]`. The literals `DeployedFSharpCore_ReferencesNetstandard20`, `Detector_OnPackageNetstandard21Binary_Reports21`, `ReadAssemblyReferences`, `[DataTestMethod]` and `DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [` are quoted here in prose because they are absent from the tree until this task runs.
      Acceptance: the file exists; contains `[DataTestMethod]` exactly once, `[DataRow(` exactly 15 times, `DisplayName = "DeployedFSharpCore_ReferencesNetstandard20 [` exactly 15 times, `[TestMethod]` exactly once, `Detector_OnPackageNetstandard21Binary_Reports21` exactly once as a method name, `PEReader` at least once, `#nullable` 0 times and `DoNotParallelize` 0 times; `[P1-T4]` records the counts.
- [x] [P1-T3] Register both files in `TaskMaster.Test/TaskMaster.Test.csproj` by inserting exactly these two lines immediately after line 326 (`    <Compile Include="Bootstrap\AddInEagerInstallShapeTests.cs" />`), preserving the file's four-space indentation and CRLF line endings and changing no other line:

      ```
          <Compile Include="Bootstrap\FSharpCoreHintPathAlignmentTests.cs" />
          <Compile Include="Bootstrap\FSharpCoreDeployedIdentityTests.cs" />
      ```

      The project lists sources explicitly (0 wildcard `Compile Include` items), so an unregistered file silently does not exist. The literals `Compile Include="Bootstrap\FSharpCoreHintPathAlignmentTests.cs"` and `Compile Include="Bootstrap\FSharpCoreDeployedIdentityTests.cs"` are quoted here in prose because they are absent from the tree until this task runs.
      Acceptance: `git diff --numstat origin/main -- TaskMaster.Test/TaskMaster.Test.csproj` prints exactly `2	0	TaskMaster.Test/TaskMaster.Test.csproj` (two insertions, zero deletions); `[P1-T4]` records it.
- [x] [P1-T4] Static wiring and shape check of `[P1-T1]` to `[P1-T3]`, recorded in `.../evidence/regression-testing/test-authoring.2026-09-16T23-27.md`. Payload with WT-PREAMBLE:

      ```
      $a = "TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs"
      $b = "TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs"
      $p = "TaskMaster.Test/TaskMaster.Test.csproj"
      foreach ($t in @("[TestMethod]","SolutionHasExactlySixFSharpCoreHintPaths","EveryFSharpCoreHintPath_SelectsNetstandard20","internal static","#nullable","DoNotParallelize","[DataRow(")) { Write-Output ("A " + $t + " COUNT=" + @(Select-String -LiteralPath $a -SimpleMatch -CaseSensitive -Pattern $t).Count) }
      foreach ($t in @("[DataTestMethod]","[DataRow(","DisplayName = ""DeployedFSharpCore_ReferencesNetstandard20 [","[TestMethod]","Detector_OnPackageNetstandard21Binary_Reports21","PEReader","#nullable","DoNotParallelize")) { Write-Output ("B " + $t + " COUNT=" + @(Select-String -LiteralPath $b -SimpleMatch -CaseSensitive -Pattern $t).Count) }
      foreach ($t in @("Compile Include=""Bootstrap\FSharpCoreHintPathAlignmentTests.cs""","Compile Include=""Bootstrap\FSharpCoreDeployedIdentityTests.cs""")) { Write-Output ("P " + $t + " COUNT=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern $t).Count) }
      Write-Output ("A LINES=" + @(Get-Content -LiteralPath $a).Count)
      Write-Output ("B LINES=" + @(Get-Content -LiteralPath $b).Count)
      Write-Output ("BOOTSTRAP_CS_FILES=" + @(Get-ChildItem -LiteralPath "TaskMaster.Test/Bootstrap" -Filter "*.cs").Count)
      git diff --numstat origin/main -- TaskMaster.Test/TaskMaster.Test.csproj
      git status --porcelain --untracked-files=all -- TaskMaster.Test/Bootstrap TaskMaster.Test/TaskMaster.Test.csproj
      ```

      The `git status` span is the companion the tracked-only numstat needs, because the two new files are untracked until `[P1-T8]`.
      Acceptance: the artifact records `A [TestMethod] COUNT=2`, `A SolutionHasExactlySixFSharpCoreHintPaths COUNT=1`, `A EveryFSharpCoreHintPath_SelectsNetstandard20 COUNT=1`, `A internal static COUNT=2`, `A #nullable COUNT=0`, `A DoNotParallelize COUNT=0`, `A [DataRow( COUNT=0`; `B [DataTestMethod] COUNT=1`, `B [DataRow( COUNT=15`, the `B DisplayName` count 15, `B [TestMethod] COUNT=1`, `B Detector_OnPackageNetstandard21Binary_Reports21 COUNT=1`, `B PEReader` count at least 1, `B #nullable COUNT=0`, `B DoNotParallelize COUNT=0`; both `P` counts 1; `A LINES` and `B LINES` each at most 500; `BOOTSTRAP_CS_FILES=5`; the numstat line reads `2	0	TaskMaster.Test/TaskMaster.Test.csproj`; the porcelain span lists exactly three paths: the two new files as `??` and the project file as ` M`.
- [x] [P1-T5] Fresh whole-solution rebuild of the UNFIXED tree with the new tests registered, under BUILD-LOCK, Outlook gate re-checked, recorded in `.../evidence/regression-testing/expect-fail-build.2026-09-16T23-27.md`. This is the build AC2's pre-fix observation reads; a warm or partial build would let Shape B pass vacuously on stale output. Payload: WT-PREAMBLE, `Write-Output ("OUTLOOK-PROCESS-COUNT=" + @(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue).Count)`, `Write-Output ("BUILD-START-UTC=" + [DateTime]::UtcNow.ToString("o"))`, MSBUILD-RESOLVE, then

      ```
      & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" *> "coverage/logs/p1-t5-build.txt"
      $LASTEXITCODE
      ```

      Then measure, in a second payload with WT-PREAMBLE, substituting the recorded `BUILD-START-UTC` value for `BUILD-START-UTC` below:

      ```
      $log = "coverage/logs/p1-t5-build.txt"
      Write-Output ("ZERO_ERRORS_LINES=" + @(Select-String -LiteralPath $log -Pattern "^\s+0 Error\(s\)$").Count)
      Write-Output ("SKIPPED_CORECOMPILE=" + @(Select-String -LiteralPath $log -Pattern "Skipping target .CoreCompile.").Count)
      $start = [DateTime]::Parse("BUILD-START-UTC").ToUniversalTime()
      foreach ($d in @("QuickFiler","QuickFiler.Test","ToDoModel","UtilitiesCS","UtilitiesCS.Test","ToDoModel.Test","Tags","Tags.Test","VBFunctions.Test","TaskTree","TaskTree.Test","TaskVisualization","TaskVisualization.Test","TaskMaster","TaskMaster.Test")) {
      $csc = @(Select-String -LiteralPath $log -SimpleMatch -Pattern ("/out:obj\Debug\" + $d + ".dll")).Count
      $own = Get-Item -LiteralPath ($d + "/bin/Debug/" + $d + ".dll")
      Write-Output ($d + " CSC_OUT=" + $csc + " OWN_DLL_FRESH=" + ($own.LastWriteTimeUtc -ge $start) + " FSHARPCORE_PRESENT=" + (Test-Path -LiteralPath ($d + "/bin/Debug/FSharp.Core.dll"))) }
      ```

      `CSC_OUT` is the echoed compiler command line for that project (MSBuild prints it at normal verbosity; the `Task "Csc"` literal does not appear at that verbosity and is not used). `OWN_DLL_FRESH` proves the project's own output was written by this build; copied references keep their source timestamps, so freshness is asserted on the compiled assembly rather than on `FSharp.Core.dll`.
      Acceptance: `OUTLOOK-PROCESS-COUNT=0`; `EXIT_CODE: 0`; `ZERO_ERRORS_LINES` at least 1; `SKIPPED_CORECOMPILE=0`; every one of the fifteen lines reads `CSC_OUT=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True`. Any other value means the build was not a real whole-solution rebuild or a directory received no copy, which blocks.
- [x] [P1-T6] [expect-fail] Run Shape A against the unfixed tree, recorded in `.../evidence/regression-testing/expect-fail-shape-a.2026-09-16T23-27.md`. Under BUILD-LOCK, payload with WT-PREAMBLE, a pre-run removal of `TestResults/p1-t6/*.trx` with `PRERUN_TRX_COUNT=` printed, VSTEST-RESOLVE, then SCOPED-RUN with `<id>` = `p1-t6` and `<filter>` = `FullyQualifiedName~FSharpCoreHintPathAlignmentTests`; then TRX-READ with `<id>` = `p1-t6`.
      Acceptance: `PRERUN_TRX_COUNT=0`; `TRX_MATCH_COUNT=1`; `COUNTERS_TOTAL=2 EXECUTED=2 PASSED=1 FAILED=1`; `SolutionHasExactlySixFSharpCoreHintPaths OUTCOME=Passed` (the count is six on the unfixed tree, so the enumeration is proven live); `EveryFSharpCoreHintPath_SelectsNetstandard20 OUTCOME=Failed`; the `FAILED_MESSAGE[EveryFSharpCoreHintPath_SelectsNetstandard20]=` line contains each of the three tokens `QuickFiler.csproj`, `QuickFiler.Test.csproj` and `ToDoModel.csproj` and contains neither `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj` nor `ToDoModel.Test.csproj`; `EXIT_CODE: 1` with `ExpectedExitCode: 1`. A `PASSED=2` outcome here means the test cannot see the defect and blocks; a `TOTAL` other than 2 means discovery or wiring failed and blocks.
- [x] [P1-T7] [expect-fail] Run Shape B against the unfixed, freshly rebuilt tree, recorded in `.../evidence/regression-testing/expect-fail-shape-b.2026-09-16T23-27.md`. Same shape as `[P1-T6]` with `<id>` = `p1-t7` and `<filter>` = `FullyQualifiedName~FSharpCoreDeployedIdentityTests`. Record additionally, under `Additional Failing Rows:`, every failed row other than the three named below (these are observed build-order outcomes for the six flip-capable directories, recorded rather than gated), or the literal `NONE`.
      Acceptance: `PRERUN_TRX_COUNT=0`; `TRX_MATCH_COUNT=1`; `COUNTERS_TOTAL=16 EXECUTED=16`; `FAILED` at least 3 and at most 15; the three `OUTCOME=Failed` lines whose `testName` contains `[QuickFiler]`, `[QuickFiler.Test]` and `[ToDoModel]` respectively are present (matched with ordinal `Contains`, so `[QuickFiler]` does not match the `[QuickFiler.Test]` row); `Detector_OnPackageNetstandard21Binary_Reports21 OUTCOME=Passed`; every `FAILED_MESSAGE[` line for a failed row contains the token `2.1.0.0`; `EXIT_CODE: 1` with `ExpectedExitCode: 1`. A failing positive control blocks (the detector is broken); a `TOTAL` other than 16 blocks.
- [x] [P1-T8] Commit the two test files under `TaskMaster.Test/Bootstrap/`, the project-file registration in `TaskMaster.Test/TaskMaster.Test.csproj` and the Phase 1 evidence. Commands:

      ```
      git add -- TaskMaster.Test/Bootstrap TaskMaster.Test/TaskMaster.Test.csproj docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895
      git commit -m "test(895): add Shape A and Shape B FSharp.Core flavour regression tests, observed failing" -- TaskMaster.Test/Bootstrap TaskMaster.Test/TaskMaster.Test.csproj docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895
      ```

      Acceptance: the commit exits 0; `git status --porcelain --untracked-files=all -- QuickFiler QuickFiler.Test ToDoModel TaskMaster.Test UtilitiesCS UtilitiesCS.Test ToDoModel.Test` returns no output; `git show --name-only --format= HEAD` lists `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs`, `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` and `TaskMaster.Test/TaskMaster.Test.csproj`, and no path under `QuickFiler/`, `QuickFiler.Test/` or `ToDoModel/`.

### Phase 2 — Minimal Fix: Three One-Line HintPath Edits

Each edit replaces the single path segment `netstandard2.1` with `netstandard2.0` inside the existing `<HintPath>` element and touches no other character in the file. Project files are excluded from CSharpier by `.csharpierignore` line 12, so no format pass reaches them; the Edit tool preserves the files' CRLF endings.

- [x] [P2-T1] In `QuickFiler/QuickFiler.csproj` line 52, replace the element value `..\packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll` with `..\packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll`. The `<Reference Include>` at line 51 and every other line are unchanged.
      Acceptance: `git diff --numstat origin/main -- QuickFiler/QuickFiler.csproj` prints exactly `1	1	QuickFiler/QuickFiler.csproj`; `[P2-T4]` records it.
- [x] [P2-T2] In `QuickFiler.Test/QuickFiler.Test.csproj` line 259, the same one-segment replacement as `[P2-T1]`.
      Acceptance: `git diff --numstat origin/main -- QuickFiler.Test/QuickFiler.Test.csproj` prints exactly `1	1	QuickFiler.Test/QuickFiler.Test.csproj`; `[P2-T4]` records it.
- [x] [P2-T3] In `ToDoModel/ToDoModel.csproj` line 42, the same one-segment replacement as `[P2-T1]`.
      Acceptance: `git diff --numstat origin/main -- ToDoModel/ToDoModel.csproj` prints exactly `1	1	ToDoModel/ToDoModel.csproj`; `[P2-T4]` records it.
- [x] [P2-T4] Verify the three edits and the three non-edits, recorded in `.../evidence/other/hintpath-edits.2026-09-16T23-27.md`. Payload with WT-PREAMBLE re-running the six-file `NS21=`/`NS20=` census from `[P0-T9]`, followed by:

      ```
      git diff --numstat origin/main -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj UtilitiesCS/UtilitiesCS.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj ToDoModel.Test/ToDoModel.Test.csproj
      git diff -U0 origin/main -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj
      ```

      Record the census lines, the numstat output verbatim and the `-U0` hunks verbatim (they carry only repository-relative paths).
      Acceptance: all six census lines read `NS21=0 NS20=1`; the numstat output is exactly three lines, `1	1	QuickFiler/QuickFiler.csproj`, `1	1	QuickFiler.Test/QuickFiler.Test.csproj` and `1	1	ToDoModel/ToDoModel.csproj`, with no line for the three untouched files; in the `-U0` output every `-` content line contains `lib\netstandard2.1\FSharp.Core.dll` and every `+` content line contains `lib\netstandard2.0\FSharp.Core.dll`, three of each. This is the pre-commit measurement of AC3's edited-file clause; `[P4-T14]` is the post-commit confirming run.

### Phase 3 — Comment-Only Remark Correction

- [x] [P3-T1] In `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`, replace lines 363 to 371 inclusive on `ProbeApplicationBase` — the whole `<remarks>` block, counting the `/// <remarks>` line (363), the seven interior comment lines (364-370) and the `/// </remarks>` line (371), nine lines in total — with exactly these thirteen lines, at the same eight-space indentation, changing nothing else in the file. The replacement opens and closes the `<remarks>` element itself: there are seven lines strictly between the two tags, not nine, so a replacement that dropped the tags would delete the element that AC5 is worded about.

      ```
              /// <remarks>
              /// It is deliberately not this test assembly's own output directory. This directory is
              /// retained as the historically failing root: before issue 895 aligned every
              /// <c>FSharp.Core</c> HintPath on the netstandard2.0 flavour, it deployed the flavour
              /// whose own reference is <c>netstandard 2.1.0.0</c>, while this test assembly's own
              /// output directory did not, so a probe rooted here could observe the bind failure and a
              /// probe rooted there could not. After that alignment every deployed copy references
              /// <c>netstandard 2.0.0.0</c>, so the Deedle invocation no longer requests the
              /// <c>2.1.0.0</c> identity from any directory, and the discriminating power of this class
              /// rests with the display-name tests, which bind that identity directly. The host base
              /// directory is this assembly's <c>bin\Debug</c>, so three parent steps reach the
              /// repository root.
              /// </remarks>
      ```

      The block no longer states as fact that the directory deploys the netstandard2.1 flavour, describes the post-fix state, retains the directory as the historically failing root, names the display-name tests as the carriers of discriminating power, and keeps the three-parent-steps sentence. The replaced text used the word `unsatisfiable`; the new text does not, and it introduces the single-line token `display-name tests` (absent from the file today; the nearest existing text is `display-name creation` at line 443). No test method, assertion, attribute, constant or the `because` message at lines 206-207 changes. The fenced block's indentation is content here: each line begins with eight spaces then `///`. CSharpier does not reflow comments, so the line breaks above survive `[P4-T5]`.
      Acceptance: `[P3-T2]` records `UNSATISFIABLE_COUNT=1` and `DISPLAY_NAME_TESTS_COUNT=1` for the file.
- [x] [P3-T2] Verify the correction is comment-only, recorded in `.../evidence/other/remarks-correction.2026-09-16T23-27.md`. Payload with WT-PREAMBLE:

      ```
      $f = "TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs"
      Write-Output ("UNSATISFIABLE_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "unsatisfiable").Count)
      Write-Output ("DISPLAY_NAME_TESTS_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "display-name tests").Count)
      Write-Output ("DONOTPARALLELIZE_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "DoNotParallelize").Count)
      Write-Output ("TESTMETHOD_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "[TestMethod]").Count)
      Write-Output ("BECAUSE_206_COUNT=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "the flavour of FSharp.Core deployed beside Deedle is what determines ").Count)
      Write-Output ("NETSTANDARDBIND_LINES=" + @(Get-Content -LiteralPath $f).Count)
      $changed = @(git diff -U0 origin/main -- $f).Where({ ($_.StartsWith("+") -or $_.StartsWith("-")) -and -not $_.StartsWith("+++") -and -not $_.StartsWith("---") })
      Write-Output ("CHANGED_LINES=" + $changed.Count)
      Write-Output ("NON_COMMENT_CHANGED_LINES=" + @($changed.Where({ -not $_.Substring(1).TrimStart().StartsWith("///") })).Count)
      git diff --numstat origin/main -- $f
      ```

      Acceptance: `UNSATISFIABLE_COUNT=1` (down from the `[P0-T9]` baseline of 2; the surviving occurrence is the line-281 summary, which stays true); `DISPLAY_NAME_TESTS_COUNT=1` (up from 0); `DONOTPARALLELIZE_COUNT=2`, `TESTMETHOD_COUNT=9` and `BECAUSE_206_COUNT=1`, all unchanged from `[P0-T9]`; `NETSTANDARDBIND_LINES=470` (466 plus 13 minus 9); `CHANGED_LINES=22`; `NON_COMMENT_CHANGED_LINES=0`; the numstat line reads `13	9	TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`. This is the pre-commit measurement of AC5's comment-only clause; `[P4-T15]` is the post-commit confirming run.

### Phase 4 — Verification Loop, Full Toolchain, and Containment Gates

Order: a fresh rebuild of the fixed tree, the two regression tests observed passing, the whole `TaskMaster.Test.Bootstrap` namespace observed passing (including the #879 negative control), then the four-step C# toolchain loop restarted from step 1 on any failure or file change, then the commit and the two anchored diff gates.

- [x] [P4-T1] Fresh whole-solution rebuild of the FIXED tree, recorded in `.../evidence/regression-testing/pass-after-build.2026-09-16T23-27.md`. Identical commands, measurements and acceptance to `[P1-T5]` with the log path `coverage/logs/p4-t1-build.txt`.
      Acceptance: as `[P1-T5]`: `OUTLOOK-PROCESS-COUNT=0`, `EXIT_CODE: 0`, `ZERO_ERRORS_LINES` at least 1, `SKIPPED_CORECOMPILE=0`, all fifteen lines `CSC_OUT=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True`.
- [x] [P4-T2] Run Shape A against the fixed tree, recorded in `.../evidence/regression-testing/pass-after-shape-a.2026-09-16T23-27.md`. Same shape as `[P1-T6]` with `<id>` = `p4-t2`.
      Acceptance: `PRERUN_TRX_COUNT=0`; `TRX_MATCH_COUNT=1`; `COUNTERS_TOTAL=2 EXECUTED=2 PASSED=2 FAILED=0`; both `OUTCOME=Passed`; `EXIT_CODE: 0`. This is the measured pass-after run for AC1.
- [x] [P4-T3] Run Shape B against the fixed, freshly rebuilt tree, recorded in `.../evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md`. Same shape as `[P1-T7]` with `<id>` = `p4-t3`.
      Acceptance: `PRERUN_TRX_COUNT=0`; `TRX_MATCH_COUNT=1`; `COUNTERS_TOTAL=16 EXECUTED=16 PASSED=16 FAILED=0`; all fifteen bracketed rows and `Detector_OnPackageNetstandard21Binary_Reports21` read `OUTCOME=Passed`; `EXIT_CODE: 0`. This is the measured pass-after run for AC2.
- [x] [P4-T4] Run the whole `TaskMaster.Test.Bootstrap` namespace, recorded in `.../evidence/regression-testing/pass-after-bootstrap-namespace.2026-09-16T23-27.md`. Same shape as `[P1-T6]` with `<id>` = `p4-t4` and `<filter>` = `FullyQualifiedName~TaskMaster.Test.Bootstrap`. The run discovers `NetstandardBindChildDomainTests` (9 tests, pinned `[DoNotParallelize]` by #879 and unchanged here), `AddInEagerInstallShapeTests` (2 tests) and the two new classes (18 results). Record additionally the sentence `AfterInstall_DeedleTypeInitializerSucceeds passes with or without the #879 installer now that QuickFiler.Test deploys the netstandard2.0 flavour; its discriminating power has moved to the display-name tests, whose negative control is unaffected. Recorded, not fixed.`
      Acceptance: `PRERUN_TRX_COUNT=0`; `TRX_MATCH_COUNT=1`; `COUNTERS_TOTAL=29 EXECUTED=29 PASSED=29 FAILED=0`; `NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed` (the #879 isolation invariant holds: the display-name bind still throws without the installer); `EXIT_CODE: 0`; the recorded sentence is present.
- [x] [P4-T5] Toolchain step 1, format, under BUILD-LOCK, written to `.../evidence/qa-gates/format-final.2026-09-16T23-27.md`. This is a write-mode command whose exit code is identical on a clean and a repairing run, so the observation is a content hash of the three `.cs` files this plan writes plus the anchored numstat of the tree, taken before and after. Payload with WT-PREAMBLE:

      ```
      $files = @("TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs","TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs","TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs")
      foreach ($f in $files) { Write-Output ("BEFORE " + $f + " " + (Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash) }
      $before = (git diff --numstat origin/main | Out-String)
      dotnet tool run csharpier format .
      Write-Output ("FORMAT_EXIT=" + $LASTEXITCODE)
      foreach ($f in $files) { Write-Output ("AFTER " + $f + " " + (Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash) }
      $after = (git diff --numstat origin/main | Out-String)
      Write-Output ("FORMAT_CHANGED_TREE=" + ($before -ne $after))
      git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"
      ```

      Write `.../evidence/qa-gates/format-final.2026-09-16T23-27.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, an `Output Summary:` carrying the `Formatted N files in` line (N is the processed count and is never read as a rewrite count), the six hash lines, `FORMAT_CHANGED_TREE=`, and the porcelain output under `Tree Observation:` (empty as the literal `NONE`). `REWRITTEN-COUNT:` is the number of the three files whose `AFTER` hash differs from its `BEFORE` hash.
      Acceptance: `FORMAT_EXIT=0`; every path in `Tree Observation:` is one of the seven Write Set paths; `FORMAT_CHANGED_TREE` and `REWRITTEN-COUNT` are recorded. When `REWRITTEN-COUNT` is greater than 0 or `FORMAT_CHANGED_TREE=True` the loop restarts from this step (the rewritten state is the new input; a second pass on it must record `REWRITTEN-COUNT: 0`). When a rewritten path is outside the Write Set: if it is in the `[P0-T4]` drift list the repository owns that repair and the plan halts blocked; otherwise restore it with `git checkout -- <that path>`, record the restoration, and restart from this step.
- [x] [P4-T6] Toolchain step 1 verification, read-only, under BUILD-LOCK, appended to `.../evidence/qa-gates/format-final.2026-09-16T23-27.md`: `pwsh -NoProfile -Command 'WT-PREAMBLE
      dotnet tool run csharpier check .
      $LASTEXITCODE'`. Append `Check EXIT_CODE:` and the `Checked N files in` line to `.../evidence/qa-gates/format-final.2026-09-16T23-27.md`. N is repository-wide and is gated on its lower bound only: it is at least the `[P0-T4]` figure plus 2 (the two new countable files); any larger delta is recorded as `CHECKED-DELTA-RESIDUAL:` and is an observation, not a failure.
      Acceptance: `Check EXIT_CODE: 0` and the lower bound holds.
- [x] [P4-T7] Toolchain step 2, analyzers, under BUILD-LOCK with the Outlook gate re-checked: identical to `[P0-T5]` with the log path `coverage/logs/p4-t7-analyzer.txt`, written to `.../evidence/qa-gates/analyzer-final.2026-09-16T23-27.md`.
      Acceptance: `EXIT_CODE: 0`, `ZERO_ERRORS_LINES` at least 1, `ERROR_SUMMARY_LINES` equal to `ZERO_ERRORS_LINES`, `SKIPPED_CORECOMPILE=0`, `CSC_OUT_LINES` at least 15. A failure restarts the loop from `[P4-T5]` after the fix.
- [x] [P4-T8] Toolchain step 3, nullable: identical to `[P0-T6]` with the log path `coverage/logs/p4-t8-nullable.txt`, written to `.../evidence/qa-gates/nullable-final.2026-09-16T23-27.md`. Do not add `/p:Nullable=enable`.
      Acceptance: as `[P4-T7]`. A failure restarts the loop from `[P4-T5]` after the fix.
- [x] [P4-T9] Toolchain step 4, tests with coverage, under BUILD-LOCK: identical commands and measurements to `[P0-T7]` with the log path `coverage/logs/p4-t9-runner.txt`, written to `.../evidence/qa-gates/test-final.2026-09-16T23-27.md`, plus a `Failing Test Names:` field (every `Failed` name, or `NONE`) and a `NEWLY-FAILING:` field (every failing name absent from the `[P0-T7]` `Baseline Failing Set:`, or `NONE`). The run must show `Workers=0`/`ClassLevel` in force: record `RUNSETTINGS-UNCHANGED:` as the output of `git diff --numstat origin/main -- scripts/vscode/TaskMaster.cli.runsettings` (`NONE` expected). Re-run rule, stated once: when `COUNTERS_FAILED` is non-zero and `NEWLY-FAILING: NONE`, the failures are the known intermittent set and no file has changed, so this step alone is re-run once with the same command (the executor records both runs; the second is the measured run); when `NEWLY-FAILING` names any test, or the second run still fails, this task records `AC4-TEST-CLAUSE: NOT MET` and the plan stops after `[P4-T10]` with a blocked report rather than serialising, retrying further or relaxing anything. `ExpectedExitCode:` is keyed to the measured run: 0 when it reports `COUNTERS_FAILED=0` and no threshold literal matched, otherwise 1.
      Acceptance: the artifact exists with every numeric figure present as a number; `RUNSETTINGS-UNCHANGED: NONE`; `Cobertura Document State:` carries one of its two permitted values; `NEWLY-FAILING:` is present; the measured run's `COUNTERS_FAILED=0` with every new test among its `Passed` results (the TRX carries 18 results whose names begin `DeployedFSharpCore_ReferencesNetstandard20`, `Detector_OnPackageNetstandard21Binary_Reports21`, `SolutionHasExactlySixFSharpCoreHintPaths` or `EveryFSharpCoreHintPath_SelectsNetstandard20`, all `Passed`). A `RUNNER_THREW_ON_THRESHOLD=1` outcome with zero failed tests is recorded as `THRESHOLD-BREACH:` with the printed percentage and is reported to the caller as a repository-wide finding; it is not attributed to this change, which alters no production line.
- [x] [P4-T10] Loop closure, recorded in `.../evidence/qa-gates/loop-closure.2026-09-16T23-27.md`: one `EXPECTATION-MET: YES` or `NO` line per step `[P4-T5]` to `[P4-T9]` (a step meets its expectation when every clause of its own acceptance holds, including a declared non-zero `ExpectedExitCode:`), the iteration count of the loop, and `LOOP: CLEAN PASS` only when all five read `YES`; otherwise `LOOP: BLOCKED` with the failing step named.
      Acceptance: the artifact exists with exactly five `EXPECTATION-MET:` lines and one `LOOP:` line; `[P5-T4]` reads it.
- [x] [P4-T11] Coverage delta and no-regression record, written to `.../evidence/qa-gates/coverage-delta.2026-09-16T23-27.md` from figures copied out of `.../evidence/baseline/test-coverage-baseline.2026-09-16T23-27.md` and `.../evidence/qa-gates/test-final.2026-09-16T23-27.md` (no new command): `BASELINE_LINE_RATE`, `BASELINE_LINES_VALID`, `BASELINE_DOCUMENT_STATE`, `POST_CHANGE_LINE_RATE`, `POST_CHANGE_LINES_VALID`, `POST_CHANGE_DOCUMENT_STATE`. Then exactly one comparison branch, named: `BRANCH: COMPARABLE` when both document states are equal and the two `LINES_VALID` figures differ by at most 1 percent of the baseline figure, in which case `POST_CHANGE_LINE_RATE` must be at least `BASELINE_LINE_RATE` minus 0.005; `BRANCH: NOT COMPARABLE` otherwise, in which case the comparison is recorded and not gated, with one sentence stating that the two rates were computed over different instrumented denominators or document states. Also record `CHANGED-PRODUCTION-LINES: 0`, derived from two complementary spans run in this task, both of which must print nothing. First the anchored name-listing diff `git diff --name-only origin/main -- "*.cs" ":(exclude)*.Test/*"`, which enumerates tracked changes only and is therefore blind to a file the run created but never staged. Second its companion `git status --porcelain --untracked-files=all -- "*.cs" ":(exclude)*.Test/*"`, which is the only one of the two that can report such an untracked production `.cs` file. Neither span alone can fail in every state the record claims to exclude, which is why both are run: this task executes before the `[P4-T13]` commit, so the porcelain span is still live, and the anchored diff covers the committed Phase 1 state the porcelain span no longer reports. (The runner excludes `*.Test.dll` from instrumentation, so the three `.cs` files this plan touches are outside the denominator.) Also record `NEW-MODULE-COVERAGE: N/A (test code, outside the instrumented denominator)`.
      Acceptance: all six figures are present as numbers or the two permitted state literals; exactly one `BRANCH:` line; when `BRANCH: COMPARABLE` the rate clause holds; `CHANGED-PRODUCTION-LINES: 0` is present, and both the name-only span and the `git status --porcelain --untracked-files=all` companion span printed nothing.
- [x] [P4-T12] Post-format sweep of the Phase 2 and Phase 3 gates, because `[P4-T5]` rewrites tracked source across the whole tree: re-run the `[P2-T4]` census and numstat and the `[P3-T2]` payload, and the file-size and parallelism check, and write `.../evidence/other/post-format-sweep.2026-09-16T23-27.md`. Payload additions with WT-PREAMBLE:

      ```
      foreach ($f in @("TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs","TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs","TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs")) { Write-Output ($f + " LINES=" + @(Get-Content -LiteralPath $f).Count + " DONOTPARALLELIZE=" + @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "DoNotParallelize").Count) }
      ```

      Acceptance: every `[P2-T4]` clause holds again; every `[P3-T2]` clause holds again except that `NETSTANDARDBIND_LINES`, `CHANGED_LINES` and the numstat insertions may differ from the pre-format figures only if `[P4-T5]` recorded a rewrite of that file, in which case the post-format values are recorded beside the pre-format ones and `NON_COMMENT_CHANGED_LINES=0` still holds; each of the three files is at most 500 lines; `DONOTPARALLELIZE` reads 0, 0 and 2 respectively.
- [x] [P4-T13] Commit the fix in `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj` and `ToDoModel/ToDoModel.csproj`, the remark correction in `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`, and the Phase 2 to 4 evidence. Commands:

      ```
      git add -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj TaskMaster.Test/Bootstrap TaskMaster.Test/TaskMaster.Test.csproj docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895
      git commit -m "fix(895): align every FSharp.Core HintPath on the netstandard2.0 flavour" -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj TaskMaster.Test/Bootstrap TaskMaster.Test/TaskMaster.Test.csproj docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895
      ```

      Acceptance: the commit exits 0; `git status --porcelain --untracked-files=all -- QuickFiler QuickFiler.Test ToDoModel TaskMaster.Test UtilitiesCS UtilitiesCS.Test ToDoModel.Test` returns no output; `git show --name-only --format= HEAD` lists no path outside the seven Write Set paths and the feature folder.
- [x] [P4-T14] AC3 diff gate and write-set containment, post-commit, recorded in `.../evidence/other/scope-boundary-diff.2026-09-16T23-27.md`. Commands, in this order:

      ```
      git fetch origin
      git rev-parse --verify origin/main
      git diff --numstat origin/main -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj UtilitiesCS/UtilitiesCS.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj ToDoModel.Test/ToDoModel.Test.csproj
      git diff --numstat origin/main...HEAD -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj UtilitiesCS/UtilitiesCS.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj ToDoModel.Test/ToDoModel.Test.csproj
      git diff --name-only origin/main...HEAD
      git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"
      ```

      The two-dot form compares the worktree against `origin/main`; the three-dot form compares committed history from the merge base. Both are run and recorded, and they must agree. `ANCHOR: origin/main after fetch, never local main` is recorded as a sentence. Under `CHANGED-PATHS:` record the name-only list; under `OUT-OF-WRITE-SET:` record every changed path that is not one of the seven Write Set paths, not under the feature folder prefix, not in the `[P0-T8]` `INHERITED-CLAUSE-A:` set and not under `.claude/agent-memory/`, or the literal `NONE`.
      Acceptance: `git rev-parse --verify origin/main` exits 0; the two numstat outputs are byte-identical and each is exactly the three lines `1	1	QuickFiler/QuickFiler.csproj`, `1	1	QuickFiler.Test/QuickFiler.Test.csproj`, `1	1	ToDoModel/ToDoModel.csproj` with no line for `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` or `ToDoModel.Test/ToDoModel.Test.csproj` (AC3, measured here; `[P2-T4]` is the confirming pre-commit run); `OUT-OF-WRITE-SET: NONE`; the porcelain span returns no output. This is the measured run for AC3.
- [x] [P4-T15] AC5 diff gate, post-commit, appended to `.../evidence/other/scope-boundary-diff.2026-09-16T23-27.md` under an `AC5 Comment-Only Diff:` heading: re-run the `[P3-T2]` payload with both of its diff spans re-anchored from `origin/main` to `origin/main...HEAD`, after the `[P4-T14]` fetch. Written out in full so that no unanchored form is readable as an instruction, the two re-anchored spans are exactly:

      ```
      $changed = @(git diff -U0 origin/main...HEAD -- $f).Where({ ($_.StartsWith("+") -or $_.StartsWith("-")) -and -not $_.StartsWith("+++") -and -not $_.StartsWith("---") })
      git diff --numstat origin/main...HEAD -- $f
      ```

      Each carries an explicit ref operand. Neither is the bare `git diff` worktree-against-index form, which would pass vacuously here because `[P4-T13]` has already committed the change. The three-dot form is the one wanted at this point: the claim under test is this branch's committed contribution measured from the merge base, and `[P4-T14]` has already recorded that the two-dot and three-dot forms agree, so the three-dot form is not silently degenerating to the two-dot diff. The Select-String counts in the same payload read the worktree, which equals `HEAD` post-commit. Record the output.
      Acceptance: `UNSATISFIABLE_COUNT=1`, `DISPLAY_NAME_TESTS_COUNT=1`, `DONOTPARALLELIZE_COUNT=2`, `TESTMETHOD_COUNT=9`, `BECAUSE_206_COUNT=1`, `NON_COMMENT_CHANGED_LINES=0`, and the numstat deletions figure is 9. This is the measured run for AC5; `[P3-T2]` is the confirming pre-commit run.

### Phase 5 — Documentation, Acceptance Check-Off, and Final QA Reconciliation

Each check-off task flips exactly one criterion in `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md` by changing the leading `- [ ] ` to `- [x] ` on the criterion's first line and changing no other character; `spec.md` is never re-wrapped, so the line numbers below are stable. Every check-off task completes in either state: `[x]` when its named evidence holds, or `[ ]` with a `NOT MET` note when it does not, so the plan cannot be completed by checking off a failed criterion.

- [x] [P5-T1] Check off AC1 at `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md` line 422 when `.../evidence/regression-testing/expect-fail-shape-a.2026-09-16T23-27.md` records `EveryFSharpCoreHintPath_SelectsNetstandard20 OUTCOME=Failed` with the three file tokens and `SolutionHasExactlySixFSharpCoreHintPaths OUTCOME=Passed`, and `.../evidence/regression-testing/pass-after-shape-a.2026-09-16T23-27.md` records `PASSED=2 FAILED=0`; otherwise leave line 422 unchecked and append `AC1: NOT MET` with the failing value to `.../evidence/other/ac-status.2026-09-16T23-27.md`.
      Acceptance: exactly one branch is taken and evidenced: line 422 begins with `- [x] ` and both artifacts carry the named values, or line 422 begins with `- [ ] ` and the `AC1: NOT MET` line exists.
- [x] [P5-T2] Check off AC2 at `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md` line 430 when `.../evidence/regression-testing/expect-fail-build.2026-09-16T23-27.md` and `.../evidence/regression-testing/pass-after-build.2026-09-16T23-27.md` each record `SKIPPED_CORECOMPILE=0` with fifteen `CSC_OUT=1` lines, `.../evidence/regression-testing/expect-fail-shape-b.2026-09-16T23-27.md` records the `[QuickFiler]`, `[QuickFiler.Test]` and `[ToDoModel]` rows `Failed` with the control `Passed`, and `.../evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md` records `PASSED=16 FAILED=0`; otherwise the `NOT MET` branch as in `[P5-T1]`.
      Acceptance: exactly one branch is taken and evidenced, as in `[P5-T1]`, for line 430.
- [x] [P5-T3] Check off AC3 at `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md` line 444 when `.../evidence/other/scope-boundary-diff.2026-09-16T23-27.md` records the two identical three-line numstat outputs and `OUT-OF-WRITE-SET: NONE`; otherwise the `NOT MET` branch.
      Acceptance: exactly one branch is taken and evidenced, for line 444.
- [x] [P5-T4] Check off AC4 at `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md` line 450 when `.../evidence/qa-gates/loop-closure.2026-09-16T23-27.md` records `LOOP: CLEAN PASS`, `.../evidence/qa-gates/test-final.2026-09-16T23-27.md` records `COUNTERS_FAILED=0` on the measured run and `RUNSETTINGS-UNCHANGED: NONE`, `.../evidence/regression-testing/pass-after-bootstrap-namespace.2026-09-16T23-27.md` records `NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed`, and `.../evidence/other/post-format-sweep.2026-09-16T23-27.md` records `DONOTPARALLELIZE` 0, 0 and 2; otherwise the `NOT MET` branch. A baseline-subset argument does not discharge this criterion's "every test passing" clause; only `COUNTERS_FAILED=0` does.
      Acceptance: exactly one branch is taken and evidenced, for line 450.
- [x] [P5-T5] Check off AC5 at `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md` line 457 when the `AC5 Comment-Only Diff:` heading in `.../evidence/other/scope-boundary-diff.2026-09-16T23-27.md` records `UNSATISFIABLE_COUNT=1`, `DISPLAY_NAME_TESTS_COUNT=1`, `BECAUSE_206_COUNT=1` and `NON_COMMENT_CHANGED_LINES=0`, and `.../evidence/baseline/tree-baseline.2026-09-16T23-27.md` records the pre-fix `UNSATISFIABLE_COUNT=2`; otherwise the `NOT MET` branch.
      Acceptance: exactly one branch is taken and evidenced, for line 457.
- [x] [P5-T6] Write the acceptance-criteria status summary and the follow-up record to `.../evidence/other/ac-status.2026-09-16T23-27.md` (creating it if no `NOT MET` line was appended earlier): `Timestamp:`; the `### Acceptance Criteria Status` block from the `acceptance-criteria-tracking` skill with `Source:` naming `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md`, `Total AC items: 5`, `Checked off (delivered):` counted from the `- [x] AC` lines in `spec.md` lines 422-465, `Remaining (unchecked):` as the difference, and `Items remaining:` listing each unchecked criterion's leading text or `none`; an `UNMET:` line (`NONE` or the IDs); a `Record, Not Fix:` section reproducing the `AfterInstall_DeedleTypeInitializerSucceeds` sentence from `[P4-T4]`; a `Follow-Ups (not opened by this plan):` section naming latent defect 1 (`ToDoModel.Test/packages.config` lacks `FSharp.Core` and `Deedle` entries although `ToDoModel.Test/ToDoModel.Test.csproj` lines 92-96 carry HintPaths for both), latent defect 2 (`scripts/vscode/Sync-PackageReferences.ps1` lines 13-19 rank `netstandard2.1` ahead of `netstandard2.0`) and the analyzer-package skew handled at `[P0-T3]` (issue #898); and a `Spec Observed Corrections:` section reproducing the two items from `[P0-T1]`.
      Acceptance: the artifact exists; `Checked off (delivered):` equals the count of `spec.md` lines in the range 422-465 beginning `- [x] `; `Total AC items: 5`; the `Record, Not Fix:`, `Follow-Ups`, and `Spec Observed Corrections:` sections exist.
- [x] [P5-T7] Mirror the issue update locally at `.../evidence/issue-updates/issue-895.2026-09-16T23-27.md` with `Timestamp:`, the exact text intended for issue #895 (the three one-line HintPath edits, the two regression tests with their observed-failing and observed-passing evidence paths, the comment-only remark correction, the AC status block from `[P5-T6]`, and the two follow-ups), and `PostedAs: unknown` (posting is outside this run).
      Acceptance: the artifact exists and carries `Timestamp:`, `PostedAs: unknown`, and the five evidence paths `expect-fail-shape-a`, `expect-fail-shape-b`, `pass-after-shape-a`, `pass-after-shape-b` and `scope-boundary-diff` by filename.
- [x] [P5-T8] Commit the acceptance check-offs in `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md`, the Phase 5 evidence and the plan's task check-offs so far. Commands:

      ```
      git add -- docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895
      git commit -m "docs(895): record acceptance-criteria status, issue mirror and QA evidence" -- docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895
      ```

      Acceptance: the commit exits 0, and `git status --porcelain --untracked-files=all -- QuickFiler QuickFiler.Test ToDoModel TaskMaster.Test UtilitiesCS UtilitiesCS.Test ToDoModel.Test docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895` returns at most one line, and if one line is returned its path is `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md`.
- [x] [P5-T9] Final QA reconciliation and plan closure. Re-read `.../evidence/qa-gates/loop-closure.2026-09-16T23-27.md` and `.../evidence/qa-gates/coverage-delta.2026-09-16T23-27.md` and confirm the four-step toolchain loop (format, analyzers, nullable, tests with coverage) recorded `LOOP: CLEAN PASS` and the coverage branch was named; record `git status --porcelain --untracked-files=all` verbatim as the pre-commit observation whose `EXIT_CODE:` this task carries (the carve-out named in the evidence accounting rule); mark `[P5-T8]` and this task complete in this plan file; then commit the plan file alone:

      ```
      git add -- docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md
      git commit -m "docs(895): close the atomic plan checklist" -- docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md
      ```

      Acceptance: `LOOP: CLEAN PASS` and a `BRANCH:` line were found; the commit exits 0; the porcelain span over `QuickFiler QuickFiler.Test ToDoModel TaskMaster.Test UtilitiesCS UtilitiesCS.Test ToDoModel.Test docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895` returns at most one line naming only this plan file, which is this task's own check-off mark and the expected terminal state. The executor's completion report carries the AC status block and the commit results; pull-request authoring and CI monitoring are not part of this run.

---

## Acceptance-Criteria Traceability

| AC (`spec.md` line) | Detects the defect (observed failing) | Implementation | Tests | Evidence (measured run) | Check-off |
|---|---|---|---|---|---|
| AC1 (422) | `[P1-T6]` | `[P2-T1]` to `[P2-T3]` | `SolutionHasExactlySixFSharpCoreHintPaths`, `EveryFSharpCoreHintPath_SelectsNetstandard20` in `[P1-T1]` | `expect-fail-shape-a`, `pass-after-shape-a` | `[P5-T1]` |
| AC2 (430) | `[P1-T7]` after `[P1-T5]` | `[P2-T1]` to `[P2-T3]` | `DeployedFSharpCore_ReferencesNetstandard20` (15 rows), `Detector_OnPackageNetstandard21Binary_Reports21` in `[P1-T2]` | `expect-fail-build`, `expect-fail-shape-b`, `pass-after-build`, `pass-after-shape-b` | `[P5-T2]` |
| AC3 (444) | `[P0-T8]` proves the six files are unchanged before any edit | `[P2-T1]` to `[P2-T3]` | anchored `git diff --numstat` in `[P2-T4]` (confirming) and `[P4-T14]` (measured) | `hintpath-edits`, `scope-boundary-diff` | `[P5-T3]` |
| AC4 (450) | not a detection criterion; containment | whole tree after `[P4-T13]` | full suite under `Workers=0`/`ClassLevel` in `[P4-T9]`; `[P4-T4]` for the #879 negative control | `format-final`, `analyzer-final`, `nullable-final`, `test-final`, `loop-closure`, `post-format-sweep` | `[P5-T4]` |
| AC5 (457) | `[P0-T9]` records `UNSATISFIABLE_COUNT=2` | `[P3-T1]` | comment-only diff in `[P3-T2]` (confirming) and `[P4-T15]` (measured) | `tree-baseline`, `remarks-correction`, `scope-boundary-diff` | `[P5-T5]` |

## Planner Notes

- **Bugfix Workflow order.** Failing regression tests first (Phase 1, observed failing at `[P1-T6]` and `[P1-T7]`), minimal targeted fix second (Phase 2, three one-line edits), full toolchain verification third (Phase 4). The comment-only correction (Phase 3) is not part of the fix and changes no behaviour.
- **Why registration lands in Phase 1.** `TaskMaster.Test/TaskMaster.Test.csproj` lists sources explicitly; without `[P1-T3]` the fail-first runs would discover zero tests and report no failure. The project file is therefore edited in Phase 1 and its numstat (`2 0`) is gated there, separately from the three HintPath files gated in Phase 2 and Phase 4.
- **Why the Phase 0 six-file numstat must be empty.** AC3 is worded against `origin/main`. If the branch already differed from `origin/main` in any HintPath-bearing project file (for example if issue #898 merged to `main` first and touched the same files), the `1 1` clause could not be satisfied and the plan stops at `[P0-T8]` for a spec amendment rather than silently widening the diff.
- **Coverage.** No production `.cs` file changes, so the changed-line obligation is discharged by `CHANGED-PRODUCTION-LINES: 0` at `[P4-T11]`; the new files are test code outside the instrumented denominator; the repository-wide rate is compared only on comparable denominators because the merged denominator is not reproducible run to run.
- **Absolute host paths.** No artifact records the absolute worktree root, an account name or a machine name; `WORKTREE-ROOT` and `BUILD-LOCK-DIR` are substitution tokens supplied by the delegation prompt, and raw logs, TRX and Cobertura documents stay under git-ignored directories.
