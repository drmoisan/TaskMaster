# Research — CI coverage threshold and Pester gates (Issue #869)

- Timestamp: 2026-09-12T11-05
- Issue: #869 (closes #561 and #562 on merge)
- Feature folder: `docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/`
- Author: task-researcher
- Method: file reads and content searches inside the isolated worktree. No command was executed; every number below is either read from a file in the tree (cited) or is an estimate whose derivation is shown.

---

## 0. Executive summary

1. The C# branch gate is a small, safe change: `ConvertTo-KoverageCoberturaXml` already rewrites the document root `line-rate` and `branch-rate` from the allowlist-filtered packages, so the post-processed document the existing line assertion reads is already the first-party projection. A branch assertion mirroring the line assertion and called at the same site reads first-party branch coverage, not the raw 23-package root. The #826 hazard is real but applies only to the raw collector output.
2. The Pester gate is greenfield. There is no Pester configuration file, no pinned Pester version, and no PSScriptAnalyzer step anywhere in CI. The only precedent for a Pester invocation in this repository is the inline `New-PesterConfiguration` command recorded in issue #752's evidence.
3. `scripts/vscode/Invoke-VSBuild.ps1` is the only script in its directory with an entry-point body and no invocation guard. Fixing the test defect **must** change that production file, or the fix will destroy more coverage than the item can add back.
4. **80 percent on `scripts/vscode` is reachable**, but only on the recommended route. The naive seam fix removes 78 covered commands (57 from Sync-PackageReferences.ps1 plus 21 from the Invoke-VSBuild.ps1 top-level body) before a single new test is written. Section E quantifies this and shows the HALT branch does not fire under the recommended route, with roughly 10 commands of margin, growing to roughly 30 if the reserve target is included.
5. Adopting `scripts/vscode/Invoke-MSTestWithCoverage.ps1` in CI introduces MSTest **class-level parallelization** that the current CI vstest invocation does not have, and drops the `/Logger:trx` argument the `test-results` artifact depends on. Both are behaviour changes that must be decided deliberately (Section B.6).

---

## A. Current CI shape

### A.1 Files and naming convention

`.github/workflows/` holds eight files (verified by directory listing):

| File | Role |
| --- | --- |
| `ci.yml` | Orchestrator. |
| `_actionlint.yml`, `_format-check.yml`, `_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml` | Reusable callees. |
| `codex-web-setup-test.yml` | Standalone workflow, not called by `ci.yml`, no `workflow_call` trigger (`codex-web-setup-test.yml:3-12`). |
| `README.md` | Pipeline documentation. |

The convention is: a reusable callee is named `_<gate>.yml`, its workflow-level `name:` is the bare gate name with no underscore (`_mstest-coverage.yml:1` reads `name: mstest-coverage`), and its `on:` block carries both `workflow_call` and `workflow_dispatch` (`_mstest-coverage.yml:3-5`, `_format-check.yml:3-5`, `_actionlint.yml:3-5`, `_build-analyzers.yml:3-5`, `_build-nullable.yml:3-5`). A new `_pester.yml` follows that shape exactly.

`ci.yml:17-32` calls each callee with a job key equal to the callee's workflow name and a job `name:` identical to the key, and contains no inline `steps:`.

### A.2 `permissions:`, `concurrency:`, `timeout-minutes:`

- `permissions: contents: read` is declared once at workflow level in `ci.yml:10-11` and once at workflow level in **every** callee (`_mstest-coverage.yml:7-8`, `_format-check.yml:7-8`, `_actionlint.yml:7-8`, `_build-analyzers.yml:7-8`, `_build-nullable.yml:7-8`). A new callee must carry the same block.
- `concurrency:` is declared **only** in `ci.yml:13-15`. `.github/workflows/README.md:31-38` records this as deliberate: "The callees declare no `concurrency` block of their own." A new callee must not declare one.
- `timeout-minutes:` is per job, not per workflow: 10 for the two fast gates (`_actionlint.yml:14`, `_format-check.yml:14`), 30 for the three msbuild-consuming gates (`_build-analyzers.yml:14`, `_build-nullable.yml:14`, `_mstest-coverage.yml:14`). A Pester job over 12 test files is a fast gate; 10 minutes matches the established value for that class.
- Runner: `ubuntu-latest` for actionlint only (`_actionlint.yml:13`); `windows-latest` for everything else. The Pester job needs Windows only if any test depends on Windows path semantics; several do (`tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1:75` asserts `'Search root not found: C:\repo\.'`). Use `windows-latest`.
- `actions/checkout@v4` with `fetch-depth: 1` is the uniform first step.

### A.3 Check-run context names

`.github/workflows/README.md:86-96` states the form as `<caller job id> / <callee job name>` and lists the five current contexts verbatim:

```
actionlint / actionlint
format-check / Verify formatting
build-analyzers / Build with analyzers and code style enforcement
build-nullable / Build with nullable warnings treated as errors
mstest-coverage / Run MSTest suite with coverage
```

Consequence for this item — this is a finding the maintainer needs before editing ruleset `18572843`:

- Adding the C# branch assertion **inside the existing `_mstest-coverage.yml` callee changes no name**. The context `mstest-coverage / Run MSTest suite with coverage` continues to report and stays required. **No ruleset edit is needed for the C# gate on that route.**
- The Pester gate adds exactly **one** new context. With the conventional shape (job key `pester` in `ci.yml`, job `name: Run Pester suite with coverage` in `_pester.yml`) the string is:

```
pester / Run Pester suite with coverage
```

- The issue text (`issue.md:38`) anticipates **two** new contexts. Two arise only if the C# threshold assertion is placed in its own callee rather than inside the existing one, for example `_coverage-threshold.yml` with job `name: Assert coverage thresholds`, giving `coverage-threshold / Assert coverage thresholds`. That route costs a second full build and test run because the callees share no artifacts (`README.md:25-30`, "Zero `needs:` edges"), so it is rejected. **The spec should record that the delivery adds one new required context, not two, and that the issue's "two" figure is superseded.**
- `README.md:98-99` forbids hand-writing these strings when editing branch protection; they must be captured from a live run with `gh api repos/drmoisan/TaskMaster/commits/<head-sha>/check-runs --jq '.check_runs[].name'` (`README.md:125`). The strings above are the predicted values, to be confirmed against the PR's own run.
- The ruleset is `18572843` with `strict_required_status_checks_policy: true` (`README.md:104-107`), and the edit is a single atomic PUT (`README.md:128-139`). A two-step remove-then-add is prohibited (`README.md:144-147`). This is a maintainer action, not part of the delivery.

### A.4 `.github/workflows/README.md` must be updated by the delivery

The table at `README.md:15-22` describes `_mstest-coverage.yml` as "Plain `msbuild /t:Build`, then `vstest.console.exe` with `/EnableCodeCoverage`; uploads the `test-results` artifact". `README.md:45-50` further states that the vstest invocation "was moved, not edited" and that any change to it is "a change to the gate's pass criterion". This item changes it deliberately, so the table row, the byte-identical claim, and the five-context list at `README.md:88-96` all require updating. Treat this as an acceptance criterion, not an optional docs touch-up.

---

## B. The C# coverage route

### B.1 Entry point and parameters

`scripts/vscode/Invoke-MSTestWithCoverage.ps1:1-13` declares four optional parameters: `-SearchRoot` (defaults to `.` at line 264), `-Configuration` (defaults to `Debug` at line 268), `-CoverageOutput` (defaults to `coverage\coverage.cobertura.xml`, line 9), and `-NoExecute`. The entry-point guard is `if ($MyInvocation.InvocationName -ne '.') { Invoke-MSTestWithCoverageMain @PSBoundParameters }` at lines 349-351.

### B.2 Tool chain the script drives

| Dependency | How it is resolved | Line |
| --- | --- | --- |
| `TaskMaster.cli.runsettings` | `Join-Path $ScriptRoot`, hard failure if absent | 33-36, 278 |
| `vswhere.exe` | `${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe`, hard failure if absent | 279-282 |
| `vstest.console.exe` | `Invoke-VsWhereExe` with `-latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'` | 284-290 |
| `dotnet-coverage` | `Get-Command 'dotnet-coverage'`, hard failure with the message "Install it with: dotnet tool install --global dotnet-coverage" | 292-294 |
| `coverage.config` | `Join-Path $repoRoot 'coverage.config'` | 321 |

**`dotnet-coverage` is required and is NOT a manifest tool.** `dotnet-tools.json` contains exactly one entry, `csharpier` at `1.2.6` (`dotnet-tools.json:1-13`). A manifest (local) tool would not satisfy the `Get-Command` check at line 292 because a local tool is not on `PATH`, so the install must be `--global`. `.codex/codex-web-setup.sh:201-210` already performs exactly that install for the Codex environment (`dotnet tool install --global dotnet-coverage`, with an update branch when already present), which is the in-repo precedent for the CI step.

The vswhere path is identical to the one the current CI job already uses successfully (`_mstest-coverage.yml:76-84`), so vswhere and vstest resolution on `windows-latest` is verified by the existing green gate.

### B.3 Can the script run unmodified on `windows-latest`?

Yes, given these added setup steps. The script itself needs no change.

1. `actions/checkout@v4`, `microsoft/setup-msbuild@v2`, `nuget/setup-nuget@v2`, the `packages` cache, `nuget restore`, and `msbuild /t:Build` — all already present in `_mstest-coverage.yml:22-68` and all still required, because the script discovers built `*.Test.dll` files rather than building them (line 296).
2. `actions/setup-dotnet@v4` — needed so `dotnet tool install` exists. `_format-check.yml:22-25` uses `dotnet-version: 10.0.x`; mirror it.
3. `dotnet tool install --global dotnet-coverage` (pin a version for determinism; no pin exists anywhere in the tree today).
4. Ensure the global tools directory is on `PATH` for subsequent steps. Appending `$env:USERPROFILE\.dotnet\tools` to `$env:GITHUB_PATH` in the install step makes this explicit rather than relying on the runner image default.
5. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`, followed by `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` per the `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` convention at `_build-analyzers.yml:69` and `_build-nullable.yml:76`. The exact argument form is already recorded in `.codex/codex-web-setup.sh:344`.

One environment interaction to verify on the PR run, not resolvable from the tree: `global.json` pins `sdk.version` `8.0.205` with `rollForward: latestFeature` and `paths: [".dotnet-sdk", "$host$"]` (`global.json:1-11`). `latestFeature` does not roll forward across major versions, so a `dotnet` command run from the repository root requires an 8.0.2xx-or-later SDK to be resolvable. `_format-check.yml` installs `10.0.x` and then runs `dotnet tool restore` from the repository root, and that gate is currently green, which is indirect evidence that an 8.0.x SDK is also present on the image. If the `dotnet tool install` step fails with the `errorMessage` from `global.json:10`, the remedy is to add `8.0.x` to the `setup-dotnet` version list, not to alter global.json.

### B.4 Assembly-discovery predicate and the `\.claude\` filter

`Invoke-MSTestWithCoverage.ps1:296-303`:

```powershell
$testAssemblies = @(Get-ChildItem -Path $resolvedSearchRoot -Recurse -Filter '*.Test.dll' |
        Where-Object {
            $_.FullName -match "\\bin\\$Configuration\\" -and
            $_.FullName -notmatch '\\obj\\' -and
            $_.FullName -notmatch '\\ref\\' -and
            ([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '(^|\\)\.claude\\'
        } |
            Select-Object -ExpandProperty FullName)
```

The fourth clause is issue #752's fix: the `.claude\` exclusion is evaluated on the path **relative to the search root**, so a run whose search root is itself inside a `.claude/worktrees/...` directory still discovers its own assemblies, while a sibling worktree nested *beneath* the search root is still dropped. The three behaviours are pinned by `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1:52-98`.

**Consequence for CI: none.** A GitHub Actions checkout has no `.claude/worktrees/` directory containing built output, so the clause never matches and the predicate reduces to the same three clauses the current CI step already applies (`_mstest-coverage.yml:86-92`). The predicate is not a blocker and needs no CI-specific adjustment.

### B.5 Where the threshold assertion is called, and on which document

`Invoke-MSTestWithCoverage.ps1:339-346`:

```powershell
$xmlContent = Get-Content $resolvedOutputPath -Raw -Encoding UTF8
$processedXmlContent = ConvertTo-KoverageCoberturaXml -XmlContent $xmlContent -RepoRoot $repoRoot
Set-Content -Path $resolvedOutputPath -Value $processedXmlContent -Encoding UTF8 -NoNewline

Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent
Write-Output (Get-CoberturaFirstPartyCoverageReport -CoberturaXml $processedXmlContent)
```

So the line assertion is applied to the **post-processed** document, not to raw collector output. `Assert-CoberturaLineCoverageThreshold` reads `/coverage@line-rate` (`Invoke-MSTestWithCoverage.Threshold.ps1:31-33`) and throws below 80 (lines 52-55).

The critical mechanism, verified in `Invoke-MSTestWithCoverage.Helpers.ps1:406-460`, is that `ConvertTo-KoverageCoberturaXml`:

1. removes every `<package>` whose `name` is outside `$ProjectNames` (lines 430-434), where `$ProjectNames` defaults to `Get-KoverageProjectAllowlist` (line 417), which enumerates tracked project files and drops any assembly name ending `.Test` (lines 15-48);
2. runs `Remove-CoberturaExemptClosureCoverage` and `Merge-CoberturaClassesByFilename` (lines 440-441);
3. **overwrites the document root counters** from `Get-CoberturaCoverageSummary` over the surviving packages (lines 454-460).

`Get-CoberturaCoverageSummary` (lines 102-136) sums `Get-CoberturaPackageLineSummary` per package, which reduces each class through `Get-CoberturaClassLineSummary` — the single de-duplicated counting rule issue #815 established. `Get-CoberturaFirstPartyCoverageSummary` (`Invoke-MSTestWithCoverage.FirstParty.ps1:69-92`) accumulates the *same* `Get-CoberturaPackageLineSummary` helper over the *same* allowlist.

**Therefore, on the post-processed document, `/coverage@branch-rate` and `Get-CoberturaFirstPartyCoverageSummary(...).BranchRate` are equal by construction.** They differ only on raw collector output, where the root is unfiltered.

### B.5.1 Does `Get-CoberturaFirstPartyCoverageSummary` already expose branch figures?

Yes. `Invoke-MSTestWithCoverage.FirstParty.ps1:83-92` returns `BranchRate`, `BranchesCovered`, `BranchesValid` and `BranchPercent` (a two-decimal invariant-culture string). A branch assertion could consume any of them directly.

### B.5.2 The #826 hazard — verified as applying to raw output only

The brief's hazard (raw root `branch-rate` 0.6649 over 23 packages versus post-processed first-party 0.7987 over 9 packages at the same commit) is consistent with the code: the raw root aggregates every instrumented module including test assemblies and third-party packages that `coverage.config` did not exclude (`coverage.config:12-22` excludes only Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing and MSTest), whereas the projection retains only allowlisted first-party packages. The consequence of each choice:

| Gate input | First-party? | Can it fail for the right reason? | Verdict |
| --- | --- | --- | --- |
| `/coverage@branch-rate` on **raw** collector output | No | It would fail at ~66 percent against a 75 floor regardless of first-party quality. Unusable. | Reject |
| `/coverage@branch-rate` on the **post-processed** document | Yes, by construction (B.5) | Fails only when first-party branch coverage drops below 75. | **Recommended** |
| `Get-CoberturaFirstPartyCoverageSummary(...).BranchPercent` on either document | Yes | Correct, but introduces a second arithmetic path at the gate, requires `Get-KoverageProjectAllowlist` (which performs recursive file I/O), and diverges in shape from the existing line gate. | Reject for the gate; keep for the printed report |

The residual risk of the recommended choice is that the assertion's correctness depends on a precondition — that the caller passes the post-processed string — which is not enforced by the signature. Mitigation: state the precondition in the function docstring, and add an acceptance criterion that both assertions are invoked from the same site immediately after `ConvertTo-KoverageCoberturaXml`.

**Headroom.** Against the brief's 2026-09-11 figures (C# first-party line 84.56, branch 79.24), the 80 and 75 floors pass with 4.56 and 4.24 points of headroom respectively. Neither floor is at risk of a vacuous or knife-edge pass.

### B.5.3 Zero-branch edge case — requires an explicit decision

`Get-CoberturaCoverageSummary` emits `BranchRate = '0'` when `$totalBranches -eq 0` (`Invoke-MSTestWithCoverage.Helpers.ps1:130`). A document whose first-party projection contains zero branches therefore presents as `branch-rate="0"`, which a naive mirror of the line assertion would report as "0% is below the required 75% threshold". That is the fail-closed direction and is the recommended behaviour, because a zero-branch first-party projection means the pipeline produced nothing to measure — the exact failure mode the memory record on issue #494 describes as "the only numeric coverage gate is evadable by withholding its input". The spec should state this explicitly rather than leave it implicit, and an acceptance criterion should also require that `branches-valid` on the emitted document is greater than zero.

### B.6 Two behaviour changes introduced by adopting the script in CI

These are not obvious from the issue text and must be decided in the spec.

1. **MSTest class-level parallelization arrives in CI.** The current CI step passes no `/Settings:` (`_mstest-coverage.yml:99`), and `vstest.console.exe` does not auto-detect a runsettings file. The script passes `/Settings:$RunSettingsPath` (`Invoke-MSTestWithCoverage.ps1:76`) resolving to `scripts/vscode/TaskMaster.cli.runsettings`, which declares `<Workers>0</Workers>` and `<Scope>ClassLevel</Scope>` (`scripts/vscode/TaskMaster.cli.runsettings:3-8`). `Workers 0` means one worker per core. This is a genuine new source of CI flakiness for any test class that shares process-wide state. Options: accept it and monitor; or add a CI-specific runsettings with `<Workers>1</Workers>`. Recommendation: accept it for the first run and treat any new failure as a finding rather than pre-emptively diverging CI from the local route, since "runs the same route as the local tooling" is the stated intent (`issue.md:34`).
2. **`/Logger:trx` is lost.** The current step passes `/Logger:trx` (`_mstest-coverage.yml:99`); the script's argument builder does not (`Invoke-MSTestWithCoverage.ps1:70-76`). The existing upload step globs `TestResults/**/*.trx` and `TestResults/**/*.coverage` with `if-no-files-found: warn` (`_mstest-coverage.yml:104-112`), so the artifact would silently become empty — a warn, not a failure. Options: (a) change the upload step to publish `coverage/coverage.cobertura.xml`; (b) add `/Logger:trx` to `Get-DotnetCoverageArgumentList`, which is a production change whose argument list is pinned by assertions in `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:99-165`. Recommendation: (a). It requires no production change and publishes the artifact the new gate actually reads.

Note that `coverage/` already exists as a tracked directory with a `.gitkeep` (`coverage/.gitkeep`) and is gitignored except for that file (`.gitignore:143-145`), so the Cobertura output lands in a location that is already understood by the repository.

### B.7 Line counts against the 500-line ceiling

Measured by a content search counting every line (`^`) per file in `scripts/vscode`:

| File | Lines | Headroom to 500 |
| --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 470 | 30 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 413 | 87 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 351 | 149 |
| `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 162 | 338 |
| `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 65 | 435 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 56 | 444 |

`Invoke-MSTestWithCoverage.Threshold.ps1` at 56 lines is the obvious home for the branch assertion, matching the issue's own instruction (`issue.md:62`). A branch assertion mirroring the line assertion is roughly 55 lines including its comment-based help, bringing the file to approximately 111 lines — comfortably inside the ceiling, no new dot-sourced part needed. The issue's "469 lines at #815" figure for Helpers is one line stale; the measured value is 470.

Dot-source topology, verified at `Invoke-MSTestWithCoverage.Helpers.ps1:2-5`: Helpers dot-sources ClosureFilter, PackageRate, Threshold and FirstParty. Any caller that dot-sources Helpers alone resolves a new function placed in Threshold.ps1, so no new wiring is required.

---

## C. The PowerShell test route

### C.1 There is no Pester configuration in this repository

- `config/` contains exactly two files, `blast-radius.json` and `orchestration-routing.json`. There is no `config/poshqc-scan.json`.
- A repository-wide glob for `**/*.psd1` returns no files. There is therefore **no** `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`, even though `.claude/rules/powershell.md:18` directs the reader to that path. `scripts/` contains only `scripts/bash/`, `scripts/dev-tools/run-actionlint.ps1`, `scripts/temp-extract-coverage.ps1` and `scripts/vscode/`. The rules file is push-down-owned and names a path that does not exist here; the delivery must not attempt to use it.
- A content search for `Invoke-Pester` across the repository returns matches only in feature documentation and in one hook regex (`.claude/hooks/enforce-orchestration-preimplementation-gate.ps1:137`). No workflow, task, or script invokes Pester.
- `.vscode/tasks.json` defines nine tasks; none of them runs Pester (verified by reading the whole file).

### C.2 Pester version and installation

No version is pinned anywhere in the tree. The only version statements are prose: `.claude/rules/powershell.md:18` and `:56` say "Pester (v5.x)", and `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md:96` says "all tests are Pester v5.x under `tests/scripts/vscode/`". Whether `windows-latest` ships a Pester 5.x module is an external fact I cannot verify from this tree.

Recommendation: install a pinned version explicitly rather than depending on the runner image, because the repository has no lockfile to fall back on and an image bump would otherwise change the gate silently:

```
Install-Module Pester -RequiredVersion <pinned 5.x> -Force -SkipPublisherCheck -Scope CurrentUser
Import-Module Pester -RequiredVersion <pinned 5.x>
```

The pinned version should be recorded in the workflow and in `.github/workflows/README.md` so a future bump is a reviewable change.

### C.3 The exact configuration a CI job needs

The in-repo precedent is the command recorded verbatim at `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.md:6`:

```powershell
$c = New-PesterConfiguration
$c.Run.Path = 'tests/scripts/vscode'
$c.Run.PassThru = $true
$c.Output.Verbosity = 'Detailed'
$c.CodeCoverage.Enabled = $true
$c.CodeCoverage.Path = 'scripts/vscode'
$c.CodeCoverage.OutputFormat = 'JaCoCo'
$c.CodeCoverage.OutputPath = '<explicit path>'
$r = Invoke-Pester -Configuration $c
"PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"
"COVERAGE LinePercent=$($r.CodeCoverage.CoveragePercent)"
if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }
```

Two mechanical facts recorded by issue #752's plan at `.../plan.2026-09-03T07-23.md:53` and applicable directly:

- `New-PesterConfiguration` defaults `Run.Exit` to `$false`, and `pwsh -Command` exits 0 unless the script calls `exit`. **A Pester job without an explicit `exit 1` on failure is a green-no-matter-what gate.** The explicit exit must come *after* the count-emitting statements; setting `Run.Exit = $true` instead is prohibited there because it exits before the counts print.
- The threshold assertion belongs in the same step, after the counts, comparing `$r.CodeCoverage.CoveragePercent` against 80 and exiting non-zero below it. Restricting `CodeCoverage.Path` to `scripts/vscode` is what makes that percentage the one the #563 decision is about. Using `scripts/vscode/*.ps1` as the path value is equivalent for this directory (it contains one non-`.ps1` file, `TaskMaster.cli.runsettings`, which Pester's coverage analyser would not parse in any case); the directory form is the form with in-repo precedent and is preferred.

The coverage figure Pester reports is **command (instruction) coverage**, which `.claude/rules/powershell.md:64` and `.claude/skills/powershell-qa-gate/SKILL.md:45` both describe as informational with no threshold, while the *line* threshold is the one that applies. The `CoveragePercent` property is the command-based figure; the JaCoCo document carries both `INSTRUCTION` and `LINE` counters per method and per file. The spec must state which of the two the gate reads. Recommendation: gate on `CoveragePercent` because it is the figure every existing baseline artifact in this repository records and the figure the 78.3 number refers to (Section E.1 proves that identity arithmetically), and additionally record the LINE totals in the evidence artifact so a future reader can distinguish them.

### C.4 Coverage output path

Pester 5's default `CodeCoverage.OutputPath` is `coverage.xml` relative to the working directory. Verified against this tree: a file named `coverage.xml` at the repository root would be matched by **neither** `.gitignore` (which ignores `coverage/*` as a directory at `.gitignore:143-145`, `*.coverage` and `*.coveragexml` at `:139-141`, but never `coverage.xml`) **nor** `.csharpierignore` (which lists `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets` — all eight entries read, none matching `coverage.xml`). CSharpier 1.2.6 processes `*.xml` per `CLAUDE.md` C#1 item 1, so a stray root `coverage.xml` is a format-check liability on a developer workstation.

Recommendation: set an explicit `CodeCoverage.OutputPath` inside the already-gitignored `coverage/` directory, for example `coverage/pester-coverage.xml`, and upload it with `actions/upload-artifact@v4` using `if-no-files-found: error` so a silently absent coverage document fails the job rather than warning. Do not rely on the default.

### C.5 PSScriptAnalyzer in CI

**Not run anywhere.** The five callees run actionlint, csharpier, two msbuild gates and vstest; `codex-web-setup-test.yml` runs `bash -n` and ShellCheck. No workflow references PSScriptAnalyzer, `Invoke-ScriptAnalyzer`, or PoshQC. Adding a PSScriptAnalyzer gate is out of scope for #869 and should be recorded as a follow-up candidate rather than bundled here.

---

## D. The `Invoke-VSBuild.Tests.ps1` defect and its seam

### D.1 The defect, line by line

`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1:3-7`:

```powershell
BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $scriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-VSBuild.ps1'
    . $scriptPath -NoExecute
}
```

`scripts/vscode/Invoke-VSBuild.ps1` has **no entry-point guard**. Its top-level body begins at line 127 and runs unconditionally on dot-source. What executes:

| Line | Statement | Effect during a test run |
| --- | --- | --- |
| 130-135 | Resolve repo root, `Test-Path` the solution | Read-only, harmless |
| 137-140 | Locate `vswhere.exe`, throw if absent | Read-only |
| **142** | `$msbuildPath = & $vswherePath -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'` | **Launches `vswhere.exe`.** Machine-dependent; throws at line 144 on a runner without Visual Studio MSBuild components |
| 147 | `Write-Host "Using MSBuild: $msbuildPath"` | Console noise |
| 152-155 | `$syncScript = Join-Path $PSScriptRoot 'Sync-PackageReferences.ps1'`; `if (Test-Path $syncScript) { & $syncScript -SolutionRoot $repoRoot }` | **Executes `Sync-PackageReferences.ps1` against the real repository root.** That script walks every `packages.config` and, when a HintPath does not resolve, calls `[System.IO.File]::WriteAllText($csprojPath, $csprojText)` at `Sync-PackageReferences.ps1:148` |
| 157-158 | Build the property and argument lists | Harmless |
| 160-162 | `if ($NoExecute) { return }` | The **only** thing `-NoExecute` suppresses |
| 164-167 | `& $msbuildPath @msbuildArguments` | Suppressed by `-NoExecute` |

So `-NoExecute` guards the msbuild launch and nothing else. The two defects named in the issue are at line 142 (vswhere) and line 154 (Sync-PackageReferences). The non-determinism the issue reports (53/84 versus 71/84 on the same lines, `issue.md:30`) follows directly: whether the `packages` directory is restored, and whether any HintPath is stale, determines how much of Sync-PackageReferences.ps1 and of the vswhere-dependent block executes.

### D.2 The seam pattern used by the files that do not have this problem

Three of the four non-helper scripts under `scripts/vscode/` carry an entry-point guard of exactly this shape:

- `scripts/vscode/Invoke-MSTest.ps1:200-202` — `if ($MyInvocation.InvocationName -ne '.') { Invoke-MSTestMain @PSBoundParameters }`
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1:349-351` — same shape, `Invoke-MSTestWithCoverageMain`
- `scripts/vscode/Install-RepoDotNetSdk.ps1:109-111` — same shape, `Install-RepoDotNetSdk`

`Invoke-MSTest.ps1:132-198` shows the full pattern: the whole entry-point body lives in `Invoke-MSTestMain`, every external dependency is reached through a named wrapper seam (`Resolve-RunSettingsPath`, `Get-VsTestConsolePath`, `Get-MSTestAssemblyPathList`, `Invoke-VsTestExe`), and the docstring at lines 136-143 states the rationale explicitly: "The top-level wiring at the bottom of this file forwards the script parameters here and does nothing else, per the Coverage Exclusion Policy in .claude/rules/general-unit-test.md, which requires logic to live in testable units rather than in an untestable host-bound script body."

`tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1:53-143` is the corresponding test: it mocks `Resolve-Path`, `Test-Path`, and every named seam, then drives `Invoke-MSTestMain` through eight scenarios with no external process and no filesystem access.

`scripts/vscode/Invoke-VSBuild.ps1` has neither the guard nor the seams. It and `scripts/vscode/Invoke-Restore.ps1` are the only two scripts in the directory whose logic sits in an unguarded top-level body.

### D.3 Does `scripts/vscode/Invoke-VSBuild.ps1` itself have to change?

**Yes, and the reason is coverage arithmetic, not just determinism.**

A seam confined to the test file — for example dot-sourcing only the function region, or parsing the file's AST and executing a filtered scriptblock — makes the test deterministic but removes the top-level body from the executed set entirely. Per the measured JaCoCo record (Section E.1), that body currently contributes 21 covered commands in `Invoke-VSBuild.ps1` and, via its call to Sync-PackageReferences.ps1 at line 154, a further 57 covered commands in that script. **A test-only seam therefore costs 78 covered commands and drops `scripts/vscode` from roughly 78.3 percent to roughly 68.6 percent.**

The smallest change that makes the test deterministic *and* preserves the arithmetic is the one the sibling scripts already use:

1. Move lines 130-167 of `scripts/vscode/Invoke-VSBuild.ps1` into a new `Invoke-VSBuildMain` function taking `-SolutionPath`, `-Configuration`, `-Platform`, `-Target`, `-MSBuildProperty`, the four switches, and `-ScriptRoot = $PSScriptRoot` (mirroring `Invoke-MSTest.ps1:145-150`).
2. Introduce two named wrapper seams in the same file: `Get-MSBuildPath -VsWherePath <string>` (mirroring `Get-VsTestConsolePath` at `Invoke-MSTest.ps1:77-95`) and `Invoke-SyncPackageReferences -SyncScriptPath <string> -SolutionRoot <string>` (mirroring `Invoke-VsTestExe` at `Invoke-MSTest.ps1:57-75`). A third seam, `Invoke-MSBuildExe -MSBuildPath <string> -MSBuildArgs <string[]>`, completes the pattern. Per `.claude/rules/powershell.md:47-50`, the array parameter must not be named `Args`.
3. Add `if ($MyInvocation.InvocationName -ne '.') { Invoke-VSBuildMain @PSBoundParameters }` at the foot of the file.
4. Rewrite `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` to dot-source the file (which now executes nothing) and drive `Invoke-VSBuildMain` with all three seams mocked, in the shape of `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1`.

Behaviour is preserved exactly: the same guards fire in the same order with the same messages, and the script invoked from `.vscode/tasks.json` (five of its nine tasks invoke this file, at lines 36, 95, 123 and 152) and from `.codex/codex-web-setup.sh:285` continues to behave identically, because those callers use `-File`, for which `$MyInvocation.InvocationName` is the script path and never `.`.

This is a production change to one PowerShell file, within the two-file direct-mode budget at `.claude/rules/powershell.md:39`.

Even with this, the 57 commands in Sync-PackageReferences.ps1 are lost, because no test file targets that script. Section E accounts for them.

---

## E. Where the missing PowerShell coverage is

### E.1 Measured basis

The only per-file PowerShell coverage measurements in the tree are issue #752's JaCoCo artifacts. I use the post-change clean pass, `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-cleanpass.2026-09-03T07-23.xml`, reading the per-`<class>` `INSTRUCTION` counters (JaCoCo `INSTRUCTION` is Pester's "command").

| Rank by uncovered | File | Covered | Missed | Total | Per-file % |
| --- | --- | --- | --- | --- | --- |
| 1 | `Install-RepoDotNetSdk.ps1` | 3 | **38** | 41 | 7.3 |
| 2 | `TestProcessCleanup.ps1` | 0 | **35** | 35 | 0.0 |
| 3 | `Sync-PackageReferences.ps1` | 57 | **34** | 91 | 62.6 |
| 4 | `Invoke-MSTestWithCoverage.Helpers.ps1` | 228 | **23** | 251 | 90.8 |
| 5 | `Invoke-Restore.ps1` | 0 | **21** | 21 | 0.0 |
| 6 | `Invoke-MSTestWithCoverage.ps1` | 101 | **11** | 112 | 90.2 |
| 7 | `Invoke-VSBuild.ps1` | 42 | **7** | 49 | 85.7 |
| 8 | `Invoke-MSTest.ps1` | 47 | **3** | 50 | 94.0 |
| 9 | `Invoke-MSTestWithCoverage.Threshold.ps1` | 15 | **2** | 17 | 88.2 |
| — | `Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 111 | 0 | 111 | 100.0 |
| — | `Invoke-MSTestWithCoverage.PackageRate.ps1` | 25 | 0 | 25 | 100.0 |
| | **Total** | **629** | **174** | **803** | **78.33** |

Line-number citations for the class-level counters: `:27-28` (Install-RepoDotNetSdk), `:68-69` (Invoke-MSTest), `:104-105` (ClosureFilter), `:150-151` (Helpers), `:166-167` (PackageRate), `:217-218` (Invoke-MSTestWithCoverage), `:233-234` (Threshold), `:244-245` (Invoke-Restore), `:270-271` (Invoke-VSBuild), `:281-282` (Sync-PackageReferences), `:297-298` (TestProcessCleanup).

**Identity check on the 78.3 figure.** The pre-change baseline records `COVERAGE LinePercent=78.3042394014963` and Pester's own console line `Covered 78.3% / 75%. 802 analyzed Commands in 11 Files` (`.../evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.md:15`, `:28`). 628/802 = 0.7830423940149626, which reproduces the recorded figure to every printed digit. This confirms two things: the reported percentage is command coverage, and the clean-pass artifact I tabulated (629/803 = 78.331 percent) differs from the baseline by exactly the one command issue #752's fix added and covered.

**Staleness warning, important for Phase 0.** The artifact enumerates **11** files. `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` (162 lines) and its test `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` (271 lines) are absent from it, because they landed under issue #815 after 2026-09-03. A repository-wide glob for Pester coverage artifacts returns nothing dated later than 2026-09-03T11-09. The brief's "measured 2026-09-11: scripts/vscode PowerShell line 78.3" is digit-identical to the 2026-09-03 baseline and I could find no 2026-09-11 artifact backing it. **Treat 78.3 as the 2026-09-03 figure, not a current one, and re-measure in Phase 0 before computing the required delta.**

### E.2 Function-level test mapping

Functions with **zero** covered commands (no test exercises them at all), from the per-`<method>` counters:

| Script | Function | Uncovered commands | XML line |
| --- | --- | --- | --- |
| `TestProcessCleanup.ps1` | `Stop-RepoOwnedVSTestProcess` | 34 | `:287-290` |
| `TestProcessCleanup.ps1` | script-level `Set-Alias` | 1 | `:292-295` |
| `Install-RepoDotNetSdk.ps1` | `Install-RepoDotNetSdk` | 33 | `:22-25` |
| `Install-RepoDotNetSdk.ps1` | `Get-RepoDotNetSdkInstallDir` | 4 | `:17-20` |
| `Invoke-Restore.ps1` | top-level body (no functions exist) | 21 | `:239-242` |
| `Invoke-MSTest.ps1` | `Get-VsTestConsolePath` | 2 | `:48-51` |
| `Invoke-MSTestWithCoverage.ps1` | `Invoke-DotnetCoverageExe` | 1 | `:192-195` |
| `Invoke-MSTestWithCoverage.ps1` | `Invoke-VsWhereExe` | 1 | `:197-200` |

Partially covered functions holding the remaining misses: `Merge-CoberturaClassesByFilename` 20 of 97 (`:141-142`), `Invoke-MSTestWithCoverageMain` 6 of 53 (`:213`), `Invoke-VSBuild.ps1` top-level body 6 of 27 (`:266`), `ConvertTo-KoverageCoberturaXml` 1 (`:146`), `ConvertTo-KoverageRelativePath` 1 (`:121`), `Get-CoberturaClassLineSummary` 1 (`:136`), `Get-DerivedCoverageSettingsPath` 1 (`:188`), `Invoke-DotnetCoverageCollection` 1 (`:203`), `Assert-CoberturaLineCoverageThreshold` 2 (`:229`), `ConvertTo-MSBuildPropertyArgument` 1 (`:251`), `Sync-PackageReferences.ps1` top-level body 34 of 91 (`:277`), and three one-command `<script>` misses which are the unreachable guard-body invocations described in E.3.

Every one of the twelve production scripts except `Invoke-Restore.ps1`, `Sync-PackageReferences.ps1` and `TestProcessCleanup.ps1` has a same-named test file under `tests/scripts/vscode/`. Those three have none. `Invoke-VSBuild.ps1` has one, but it tests only the three pure helper functions (`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1:9-85`) and reaches the entry-point body only as an uncontrolled side effect.

### E.3 Structurally unreachable lines

Three categories cannot be closed by a unit test without a production change:

1. **Guard-body invocations.** The `<Function> @PSBoundParameters` line inside `if ($MyInvocation.InvocationName -ne '.')` is by construction never executed when the file is dot-sourced. One command each in `Invoke-MSTest.ps1`, `Invoke-MSTestWithCoverage.ps1` and `Install-RepoDotNetSdk.ps1` — **3 commands, permanently uncovered.** `Get-CoberturaFirstPartyCoverageReport`'s docstring at `Invoke-MSTestWithCoverage.FirstParty.ps1:130-134` names this property explicitly and is the repository's own precedent for accepting it.
2. **Unguarded entry-point bodies.** `Invoke-Restore.ps1` lines 12-39 (21 commands) and `Sync-PackageReferences.ps1` lines 6-158 (91 commands) execute on dot-source. They are reachable only by executing them, which is what makes today's figure non-deterministic. They become reachable-under-mock only if refactored into functions behind a guard.
3. **Network and archive I/O.** `Install-RepoDotNetSdk` lines 76-100 build an `HttpClient`, download a zip, and call `ZipFile::ExtractToDirectory`. `.claude/hooks/check-powershell-test-purity.ps1:110-113` blocks `Invoke-WebRequest`, `Invoke-RestMethod`, `[System.Net.Http.` and `[System.Net.WebRequest]` in any Pester test file, and lines 105-109 block every temp-file and temp-path form. Roughly 21 of that function's 33 commands are therefore unreachable without a production seam. **Quantified bound: about 21 commands.**

Total structurally unreachable without production changes: 3 (guards) + 21 (Invoke-Restore body) + 91 (Sync-PackageReferences body) + 21 (SDK download) = **136 of 803 commands, or 16.9 percent.** That leaves a maximum achievable of 83.1 percent on the frozen 2026-09-03 tree — above 80, but with only 3.1 points of slack. Refactoring `Invoke-Restore.ps1` behind a guard recovers 20 of its 21 and raises the ceiling to roughly 85.6 percent.

### E.4 How many commands must be closed

Arithmetic on the 803-command basis (re-derive against the Phase 0 re-measurement):

- Current: 629/803 = 78.331 percent.
- 80 percent requires ceil(0.80 x 803) = 643 covered. From today that is **+14 commands**, matching the brief's "1.7 points" (803 x 0.017 = 13.65).
- **But the seam fix subtracts first.** Under the recommended Main-extraction (Section D.3), the Invoke-VSBuild.ps1 body's 21 commands are retained because the extracted function is driven under mocks, while Sync-PackageReferences.ps1's 57 commands are lost because nothing else executes it. Post-seam covered = 629 - 57 = 572.
- Required from new tests: 643 - 572 = **+71 commands**.
- Under a test-only seam (no production change), covered = 629 - 78 = 551, and the requirement rises to **+92 commands**.

### E.5 Ranked, concrete test targets

Estimates are derived by inspecting each uncovered region and classifying each command as mockable, pure, or blocked. They are estimates, labelled as such.

| # | Target | Mechanism | Est. commands gained | Confidence |
| --- | --- | --- | --- | --- |
| 1 | `TestProcessCleanup.ps1` — `Stop-RepoOwnedVSTestProcess` | Dot-source is already safe (the file defines one function plus a `Set-Alias -Scope Script` at line 70; no entry-point body). Mock `Get-CimInstance`, `Get-Process`, `Stop-Process`, `Write-Verbose`. Scenarios: no matching processes (early return, line 13-15); a vstest process whose CommandLine does not match the repo root (`continue`, line 26-28); a matching vstest process with a testhost child (the BFS at lines 36-51); `Get-Process` returning `$null` (line 61-63); the `ShouldProcess` branch (line 65-67) via `-WhatIf`. | **+33 to +35** | High |
| 2 | `Invoke-MSTestWithCoverage.Helpers.ps1` — `Merge-CoberturaClassesByFilename` edge branches | 20 uncovered of 97. Reachable with crafted here-string Cobertura fixtures: a group whose only member is a synthesized `<` name (line 287-290), a class with no `<lines>` element (lines 314-317), the `condition-coverage` removal branch (lines 352-354), the `<conditions>` clone branch (lines 356-362). Existing file `tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1` (71 lines) is the natural home. | **+13 to +18** | Medium-high |
| 3 | `Install-RepoDotNetSdk.ps1` — `Get-RepoDotNetSdkInstallDir` and the two early-exit branches of `Install-RepoDotNetSdk` | `Get-RepoDotNetSdkInstallDir` is pure path arithmetic, both branches trivially reachable (+4). `Install-RepoDotNetSdk` with `Mock Test-Path { $true }` takes the already-installed return at lines 58-61 (+4). `-WhatIf` reaches lines 63-67 and returns at the `ShouldProcess` guard (+4). Existing 28-line test file extends naturally. | **+10 to +12** | High |
| 4 | `Invoke-VSBuild.ps1` — `Invoke-VSBuildMain` under mocked seams | Retains the 21 currently covered and closes the 6 currently missed (the two `throw` guards at lines 133-135 and 138-140, the `-not $msbuildPath` throw at 143-145, the `Test-Path $syncScript` false branch, the non-`NoExecute` path, and the non-zero exit throw at 165-167), plus the empty-property `throw` in `ConvertTo-MSBuildPropertyArgument` at lines 41-43 (+1). | **+7** net gain, **+21** retained | High |
| 5 | `Invoke-MSTestWithCoverage.ps1` — seams and the post-`NoExecute` tail | `Invoke-VsWhereExe` (+1) via the named-command technique already used at `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1:41`; `Invoke-DotnetCoverageExe` (+1) by declaring a `function dotnet-coverage {}` in the test scope and mocking it; `Get-DerivedCoverageSettingsPath` empty-parent throw (+1); one `Invoke-DotnetCoverageCollection` guard throw (+1); the six-command tail of `Invoke-MSTestWithCoverageMain` after the `NoExecute` return (+6) by mocking `Invoke-DotnetCoverageCollection`, `Get-Content`, `Set-Content`, `ConvertTo-KoverageCoberturaXml`, `Assert-CoberturaLineCoverageThreshold` and `Get-CoberturaFirstPartyCoverageReport`, exactly as `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1:36-49` already does. | **+8 to +10** | Medium-high |
| 6 | `Invoke-MSTest.ps1` — `Get-VsTestConsolePath` | 2 commands, reachable by passing a test-scope function name as `-VsWherePath`, the technique at `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1:41`. | **+2** | High |
| 7 | `Invoke-MSTestWithCoverage.Threshold.ps1` — the two uncovered commands | The out-of-range rejection at lines 47-49 has no test (the existing 15-line test file covers missing, non-numeric, below, at, and above, but not out-of-range). | **+2** | High |
| **Reserve A** | `Invoke-Restore.ps1` — extract `Invoke-RestoreMain` behind a guard | Same Main-extraction as target 4. Makes 20 of 21 commands reachable. **Second production file changed**, reaching the two-file direct-mode budget ceiling at `.claude/rules/powershell.md:39`. | **+20** | Medium-high |
| **Reserve B** | `Sync-PackageReferences.ps1` — recover the lost 57 | Options: (i) a tracked read-only fixture directory under `tests/` with a `packages.config` plus a `.csproj` whose HintPaths are unresolvable and unmatchable, so the loop reaches `continue` at line 79 and `if ($fixCount -eq 0) { return }` at line 112 without ever reaching `WriteAllText` at line 148 — recovers roughly 30 to 45 commands, no temp files, but adds fixture files; (ii) extract the script into functions behind a guard — a third production file, exceeding the budget without an override. | **+30 to +45** (option i) | Medium |

**Sum of targets 1-7 under the recommended route: +75 to +85 commands against a requirement of +71.** The HALT branch does not fire. Margin at the low end is 4 commands (0.5 points); at the mid estimate roughly 10 commands (1.2 points). Reserve A raises the margin to roughly +24 to +34 commands (3 to 4 points) and is the recommended contingency if the Phase 0 re-measurement moves the basis unfavourably.

Under a test-only seam the requirement is +92 and targets 1-7 yield at most +78, so **that route halts**. This is the decisive argument for changing `scripts/vscode/Invoke-VSBuild.ps1`.

### E.6 Two arithmetic caveats for the plan

1. New production code added by this item (the branch assertion, `Invoke-VSBuildMain`, the new seams) enters **both** the numerator and the denominator. Because `CLAUDE.md:310` requires new code to reach 90 percent and all of it is designed to be fully mockable, it enters at a higher rate than the current 78.3, so it raises the percentage slightly. Do not model it as neutral, but do not rely on it either.
2. The denominator will differ from 803 once `Invoke-MSTestWithCoverage.FirstParty.ps1` is included. Its 162 lines are exercised by a 271-line test file, so it very likely enters above the current average and raises the starting percentage. **The plan must re-derive the required delta from a fresh measurement using N = ceil(0.80 x T) - C, where T and C come from the Phase 0 run, not from this document.**

---

## F. Threshold policy reconciliation (recorded, not resolved)

The repository states three different line floors and disagrees with itself about whether a branch floor exists.

| Source | Line | Statement |
| --- | --- | --- |
| `CLAUDE.md` | 303 | "Repository-wide line coverage must remain `>= 80%`." |
| `CLAUDE.md` | 304 | "The 80% floor applies to the **testable denominator** — production-only first-party code" |
| `CLAUDE.md` | 309 | "Testable seams within otherwise-COM-bound assemblies ... must meet the `>= 80%` floor." |
| `CLAUDE.md` | 310 | "Any new modules, classes, or methods added must target `>= 90%` coverage." |
| `CLAUDE.md` | — | **No branch-coverage threshold is stated anywhere in CLAUDE.md.** |
| `.claude/rules/general-unit-test.md` | 23 | "**Line coverage must remain >= 85% across all tiers (T1–T4).**" |
| `.claude/rules/general-unit-test.md` | 24 | "**Branch coverage must remain >= 75% across all tiers (T1–T4) for languages whose coverage tooling measures branch coverage.** PowerShell (Pester) and bash (kcov) are the exceptions ... there is no branch-coverage gate." |
| `.claude/rules/quality-tiers.md` | 33 | "Line coverage: >= 85%." |
| `.claude/rules/quality-tiers.md` | 34 | "Branch coverage: >= 75% for languages whose coverage tooling measures branch coverage. PowerShell (Pester) and bash (kcov) are exempt" |
| `.claude/rules/quality-tiers.md` | 51 | "line coverage >= 85% applies uniformly across T1–T4 ... tier-specific lower coverage floors are not used in this repository." |
| `.claude/rules/powershell.md` | 63 | "Line coverage must remain >= 85% across all tiers (T1–T4) per `.claude/rules/quality-tiers.md`." |
| `.claude/rules/powershell.md` | 64 | "Branch coverage is not measurable by Pester for PowerShell; there is no PowerShell branch-coverage gate." |
| `.claude/skills/powershell-qa-gate/SKILL.md` | 45 | "**New modules, classes, or methods**: line coverage >= 85% per the uniform tier rule" |
| `.claude/agents/feature-review.md` | 118 | "Do not record FAIL for an absent PowerShell branch figure." |

Precise divergences:

- **C# line floor:** `CLAUDE.md` says 80; the three rule files say 85. The #563 decision implements **80**.
- **New-code floor:** `CLAUDE.md` says 90; `.claude/skills/powershell-qa-gate/SKILL.md:45` says 85. The #563 decision implements **90**.
- **C# branch floor:** `CLAUDE.md` is silent; the rule files say 75. The #563 decision implements **75**. This is the one number on which `CLAUDE.md` and the rule files do not actually conflict, because `CLAUDE.md` makes no branch claim.
- **PowerShell branch floor:** four independent sources agree there is none, because Pester measures no branch coverage. The #563 decision agrees: PowerShell line only. **The delivery must not add a PowerShell branch assertion.** A reviewer who sees a branch figure in a Pester gate will treat it as a policy violation per `.claude/agents/feature-review.md:118`.
- **PowerShell line floor:** every rule file says 85; the #563 decision says **80**. This is the only place where the maintainer decision sits *below* a stated rule-file figure, so it is the one a reviewer is most likely to flag. Note also `.../evidence/baseline/coverage-floor-position.2026-09-03T07-23.md:11-15`, where issue #752 recorded `FLOOR: 85` and `BASELINE AT OR ABOVE FLOOR: false` for this exact directory. The spec should cite the #563 decision as the governing authority and record the 85-to-80 gap explicitly as a documented, maintainer-ratified exception rather than leaving the reviewer to discover it.

Authority context recorded in memory and consistent with the tree: issue #178 rejected importing the 85/75 tier figures, and they were reintroduced into `.claude/rules/` afterwards; `.claude/rules/` and the sibling `.agents/`, `.github/instructions/` and `AGENTS.md` trees are push-down-owned from an upstream repository and are overwritten with no templating, so the divergence cannot be fixed here. Do not attempt to edit the rule files in this delivery.

**No number in this item is negotiable and nothing is to be lowered.** The delivery implements C# line 80, C# branch 75, PowerShell line 80, new code 90.

---

## G. Test-policy constraints binding the new tests

Sources: `.claude/rules/general-unit-test.md`, `CLAUDE.md` UT1-UT5, `.claude/rules/powershell.md`, and the enforcing hook.

1. **Temporary files are prohibited.** `CLAUDE.md` UT4 states "Creation and use of temporary files on the local filesystem is expressly prohibited" with "Currently approved exceptions: none"; `.claude/rules/general-unit-test.md:71` repeats it. This is machine-enforced: `.claude/hooks/check-powershell-test-purity.ps1:99-117` is a `PreToolUse` hook that denies a write to any `*.Tests.ps1` or `tests/**/*.ps1` path whose content matches `New-TemporaryFile`, `[System.IO.Path]::GetTempFileName`, `[System.IO.Path]::GetTempPath`, `$env:TEMP`, `$env:TMP`, `Invoke-WebRequest`, `Invoke-RestMethod`, `[System.Net.Http.`, `[System.Net.WebRequest]`, `[System.Net.Sockets.`, `Start-Process`, `Start-Sleep`, or a direct `Mock git` / `Mock gh` / `Mock actionlint`.
2. **Tests mirror production structure.** `.claude/rules/general-unit-test.md:87-90` requires `tests/scripts/powershell/Foo.Tests.ps1` for `scripts/powershell/Foo.ps1`; colocation is not permitted. Every new file belongs at `tests/scripts/vscode/<Script>.Tests.ps1`.
3. **Determinism.** `.claude/rules/powershell.md:69-76` bars dependence on network, mutable PATH, implicit working directory, and live executables, and requires identical results in Terminal and Test Explorer.
4. **Seam ordering.** `.claude/rules/powershell.md:80-88`: mock the wrapper, never the executable; mock signatures must match production named parameters exactly; register mocks before the code under test resolves commands; when importing via AST or `ScriptBlock`, dot-source the returned `ScriptBlock` in the test scope and import wrapper seams before mocking them.

### G.1 The in-repo techniques that satisfy the no-temporary-file rule

The existing `tests/scripts/vscode/` suite uses four, and the new tests must reuse them rather than invent a fifth:

1. **Here-string fixtures for file *content*.** `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:11-27` embeds a complete Cobertura document as a `@'...'@` literal and passes it as a string. `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1:10-14` uses one-line literals such as `'<coverage line-rate="0.799999" />'`. No file is created.
2. **Mocking the filesystem cmdlets.** `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1:26-49` mocks `Resolve-Path`, `Test-Path`, `Get-ChildItem`, `Get-Content` and `Set-Content` together, so the entry-point body runs end to end without touching disk. `Get-ChildItem` is mocked to return `[pscustomobject]@{ FullName = '...' }` shapes rather than real `FileInfo` objects.
3. **Mocking the named wrapper seam, never the executable.** `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1:60-68` mocks `Resolve-RunSettingsPath`, `Get-VsTestConsolePath`, `Get-MSTestAssemblyPathList` and `Invoke-VsTestExe`, capturing arguments into `$script:` variables for assertion.
4. **Driving a splatting seam against an in-process cmdlet.** `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1:36-44` calls `Invoke-VsTestExe -VsTestPath 'Join-Path' -VsTestArgs @('C:\alpha','beta')` and asserts the result is `C:\alpha\beta`, proving the splat contract with no external process. This is the technique that makes `Get-VsTestConsolePath`, `Invoke-VsWhereExe` and `Invoke-DotnetCoverageExe` coverable.
5. **AST parse plus scriptblock dot-source** when a file's `param()` block would otherwise bind. `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1:11-19` uses `[System.Management.Automation.Language.Parser]::ParseFile` and `. $ast.GetScriptBlock()`. Note that this does **not** suppress an unguarded entry-point body, so it is not a substitute for the guard `scripts/vscode/Invoke-VSBuild.ps1` needs.

One Pester 5 scoping fact worth carrying into the plan, documented in-repo at `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1:7-8`: a helper function must be defined inside `BeforeAll`, not at file scope, because each `It` runs in a child scope of the containing block. A second, at `docs/features/active/.../code-review.2026-09-03T12-23.md:90`: a failed assertion inside `BeforeAll` does not report as an ordinary test failure, so `BeforeAll` should validate rather than assert where possible.

Reserve B's fixture option needs an explicit note: a **tracked** fixture directory under `tests/` is not a temporary file and does not violate UT4 or the purity hook, but it does add files that must never be written to by a test. If Reserve B is taken, add an acceptance criterion that the fixture's `.csproj` is byte-identical before and after the suite runs.

---

## Candidate approaches and recommendation

### C# gate

**Recommended — extend the existing `mstest-coverage` callee.** Replace the inline vstest block in `.github/workflows/_mstest-coverage.yml` with setup-dotnet, a pinned global `dotnet-coverage` install, and a `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1` step whose exit code propagates. Add `Assert-CoberturaBranchCoverageThreshold` to `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, mirroring the existing line function exactly, and call it at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:344` immediately after the line assertion. Advantages: one new required context instead of two; the CI and local routes are byte-identical, which is the issue's stated intent; the gate reads the first-party projection with no new arithmetic. Limitation: it changes the gate's pass criterion and therefore requires the `.github/workflows/README.md` table and byte-identical claim to be updated, and it imports class-level parallelization into CI (Section B.6).

**Rejected alternative — a separate `_coverage-threshold.yml` callee.** It would satisfy the issue's "two new contexts" phrasing, but the zero-`needs:` topology (`README.md:25-30`) shares no artifacts, so it would duplicate the full restore, build and instrumented test run, roughly doubling the pipeline's most expensive gate for no additional signal.

**Rejected alternative — a gate reading `Get-CoberturaFirstPartyCoverageSummary`.** Correct but adds a second arithmetic path and a file-I/O dependency (`Get-KoverageProjectAllowlist`) at the gate, diverging in shape from the existing line assertion for no measurable benefit once B.5 establishes that the two agree on the post-processed document.

### PowerShell gate

**Recommended — a new `.github/workflows/_pester.yml` callee on `windows-latest`, 10-minute timeout, `permissions: contents: read`, no `concurrency` block, `workflow_call` plus `workflow_dispatch`.** Steps: checkout at `fetch-depth: 1`; install a pinned Pester 5.x from PSGallery; one `pwsh` step that builds a `New-PesterConfiguration`, sets `Run.Path` to `tests/scripts/vscode`, `CodeCoverage.Path` to `scripts/vscode`, `CodeCoverage.OutputFormat` to `JaCoCo`, and `CodeCoverage.OutputPath` to an explicit path under `coverage/`, emits the counts, asserts `CoveragePercent -ge 80`, and exits non-zero on any test failure or on a sub-threshold figure; then `actions/upload-artifact@v4` with `if-no-files-found: error`. `ci.yml` gains a `pester:` job keyed and named `pester`.

**Rejected alternative — routing through the PoshQC MCP tooling named in `.claude/rules/powershell.md:15-18`.** Those are agent MCP commands, not CLI entry points available to a GitHub runner, and the settings file they reference does not exist in this repository (Section C.1).

**Rejected alternative — folding the Pester run into an existing Windows callee.** It would hide the PowerShell result behind another gate's conclusion and contradicts the one-gate-one-context structure `README.md:25-30` describes as deliberate.

### `Invoke-VSBuild.Tests.ps1` seam

**Recommended — Main-extraction plus three wrapper seams in `scripts/vscode/Invoke-VSBuild.ps1`,** as detailed in D.3. It is the pattern three sibling scripts already use, it is the only route that preserves the coverage arithmetic, and it makes the test deterministic with no behaviour change for any caller.

**Rejected alternative — a test-only seam.** Deterministic but costs 78 covered commands and pushes the item into the HALT branch (E.4, E.5).

---

## Behaviour semantics

| Condition | Required outcome |
| --- | --- |
| C# first-party line rate >= 0.80 on the post-processed document | Gate passes |
| C# first-party line rate < 0.80 | `Assert-CoberturaLineCoverageThreshold` throws; the pwsh step exits non-zero; the `mstest-coverage` job fails |
| C# first-party branch rate >= 0.75 | Gate passes |
| C# first-party branch rate < 0.75 | New branch assertion throws with a message naming the measured percentage and the 75 floor |
| `branch-rate` attribute missing | Throw, distinct message |
| `branch-rate` non-numeric | Throw, distinct message |
| `branch-rate` outside [0,1] | Throw, distinct message |
| `branches-valid` is 0 | Throw. Fail-closed; a zero-branch projection means the pipeline measured nothing (B.5.3) |
| Ordering | The line assertion runs first, then the branch assertion, then the first-party report. Both assertions receive the post-processed string, never raw collector output |
| Any MSTest failure | vstest exits non-zero, `dotnet-coverage` propagates it, `Invoke-DotnetCoverageCollection` throws at line 236, the gate fails before any threshold is evaluated |
| Any Pester test failure | The job exits 1 regardless of the coverage figure |
| Pester `CoveragePercent` < 80 | The job exits non-zero after emitting the measured figure |
| Coverage document absent | The upload step's `if-no-files-found: error` fails the job. Withholding the gate's input must not produce a green run |

---

## Requirements mapping — proposed file changes

Files the delivery changes or adds:

| Path | Change |
| --- | --- |
| `.github/workflows/_mstest-coverage.yml` | Add `actions/setup-dotnet@v4` and a pinned global `dotnet-coverage` install; replace the inline vstest block with the `Invoke-MSTestWithCoverage.ps1` invocation plus exit-code propagation; repoint the upload step at the Cobertura document |
| `.github/workflows/_pester.yml` | New reusable callee (Section C.3, C.4) |
| `.github/workflows/ci.yml` | Add the `pester:` job calling `./.github/workflows/_pester.yml` |
| `.github/workflows/README.md` | Update the gate table row for the MSTest callee, add the Pester row, update the byte-identical claim, add the new required context to the verbatim list |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | Add `Assert-CoberturaBranchCoverageThreshold` (file goes from 56 to roughly 111 lines) |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | One added call line at the existing assertion site |
| `scripts/vscode/Invoke-VSBuild.ps1` | Extract `Invoke-VSBuildMain`; add `Get-MSBuildPath`, `Invoke-SyncPackageReferences` and `Invoke-MSBuildExe` seams; add the invocation guard (file goes from 167 to roughly 215 lines) |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | Rewrite against the seams; currently 86 lines |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1` | Add branch-assertion scenarios and the missing out-of-range line scenario; currently 15 lines |
| `tests/scripts/vscode/TestProcessCleanup.Tests.ps1` | New |
| `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` | Extend; currently 28 lines |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1` | Extend with the Merge edge branches; currently 71 lines |
| `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` | Extend with `Get-VsTestConsolePath`; currently 144 lines, 500-line ceiling not at risk |
| `docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/**` | Spec, plan, evidence |

Two files are deliberately left alone. The coverage settings file coverage.config already excludes the third-party modules that would otherwise break instrumentation and needs no change. The helpers file scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 sits at 470 of 500 lines and must not receive the new assertion; its dot-source of the threshold part already resolves the new function for every existing caller.

Two files would additionally change only if the reserve targets are taken: scripts/vscode/Invoke-Restore.ps1 under Reserve A, and a tracked fixture pair under Reserve B. Taking Reserve A reaches the two-production-file direct-mode budget ceiling; taking both exceeds it and requires an explicit override.

---

## Testing implications (strategy only, no test code)

1. **Branch assertion:** pass at exactly 0.75, pass above, fail below with the measured percentage in the message, fail on missing attribute, fail on non-numeric, fail on out-of-range, fail on `branches-valid="0"`. All driven by one-line here-string documents, matching `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1:10-14`.
2. **Call-site wiring:** an assertion that `Invoke-MSTestWithCoverageMain` invokes both threshold functions, with the post-processed string, after `Set-Content`. Drive it with the mock set already established at `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1:26-49`.
3. **`Invoke-VSBuildMain`:** the eight scenarios enumerated in E.5 target 4, in the shape of `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1:71-143`, with a positive assertion that `Invoke-SyncPackageReferences` is invoked with the resolved repo root and `Should -Invoke Invoke-MSBuildExe -Times 0 -Exactly` under `-NoExecute`.
4. **Determinism regression proof:** a test asserting that dot-sourcing `scripts/vscode/Invoke-VSBuild.ps1` invokes neither the vswhere seam nor the sync seam. This is the regression test for the reported defect and must fail on the pre-fix tree.
5. **`Stop-RepoOwnedVSTestProcess`:** the five scenarios in E.5 target 1, all under `Mock Get-CimInstance` returning `[pscustomobject]` shapes.
6. **Workflow self-validation:** `actionlint` already lints every workflow file (`_actionlint.yml:29` runs bare `./actionlint`), so the new callee is covered by an existing gate with no new configuration.
7. **Negative-path proof for both new gates:** per `issue.md:68`, demonstrate that removing one test turns the Pester gate red and that a deliberately introduced C# regression turns the MSTest gate red. Capture both as evidence under the feature folder's `evidence/regression-testing/` directory. Note `.claude/rules/ci-workflows.md`, summarised at `.github/workflows/README.md:176-185`: a step that deliberately invokes a failing command must reset `$LASTEXITCODE` or terminate with an explicit `exit 0`. No step in the pipeline currently uses that pattern; if the negative-path proof is scripted rather than performed by hand, that rule becomes load-bearing.

---

## Risks and open items

| # | Risk | Severity | Mitigation |
| --- | --- | --- | --- |
| 1 | The 78.3 basis is from 2026-09-03 and predates `Invoke-MSTestWithCoverage.FirstParty.ps1`; no later artifact exists | High for planning | Re-measure in Phase 0 and re-derive N from the fresh totals (E.6) |
| 2 | Class-level MSTest parallelization arrives in CI with the runsettings (B.6) | Medium | Accept for the first run; treat a new failure as a finding, not a flake |
| 3 | The `test-results` artifact silently empties when `/Logger:trx` is dropped (B.6) | Low | Repoint the upload at the Cobertura document with `if-no-files-found: error` |
| 4 | `global.json` SDK pinning could block `dotnet tool install` (B.3) | Low | Verify on the PR run; add `8.0.x` to `setup-dotnet` if it fails |
| 5 | Pester version unpinned; a runner image bump changes the gate silently (C.2) | Medium | Pin `-RequiredVersion` and record it in the workflow README |
| 6 | Margin at 80 percent is thin at the low estimate (E.5) | Medium | Hold Reserve A ready; escalate to the maintainer before considering Reserve B's budget override |
| 7 | The PowerShell 80 floor sits below the 85 stated in three rule files (F) | Medium | Record the #563 decision in the spec as a ratified exception with citations; do not edit the push-down-owned rule files |
| 8 | The issue expects two new required contexts; the recommended route produces one (A.3) | Low | Record the supersession explicitly in the spec so the maintainer's ruleset PUT uses the correct set |

---

## Numeric Derivation Evidence

Complete Family: Install-RepoDotNetSdk.ps1, Invoke-MSTest.ps1, Invoke-MSTestWithCoverage.ClosureFilter.ps1, Invoke-MSTestWithCoverage.FirstParty.ps1, Invoke-MSTestWithCoverage.Helpers.ps1, Invoke-MSTestWithCoverage.PackageRate.ps1, Invoke-MSTestWithCoverage.ps1, Invoke-MSTestWithCoverage.Threshold.ps1, Invoke-Restore.ps1, Invoke-VSBuild.ps1, Sync-PackageReferences.ps1, TestProcessCleanup.ps1

Exhaustive Search Scope: entire repository source tree of the isolated worktree, covering every tracked path and not limited to any subdirectory, with the membership predicate then applied to the full result set.

Inclusion Rules: a file is a member when it resides directly in the scripts/vscode directory, carries the .ps1 extension, and is production PowerShell that ships as part of the repository's developer tooling. Every such file is in the coverage denominator because the Coverage Exclusion Policy forbids excluding any production file.

Exclusion Rules: a file is excluded when it lies outside the scripts/vscode directory (for example the bash library files, the dev-tools actionlint runner, or the repository-root extraction helper); when it carries an extension other than .ps1 (TaskMaster.cli.runsettings is excluded on this ground); when it is a Pester test file under the tests tree; or when it is build output, evidence, or documentation.

Primary Search Strategy: a filesystem path-pattern enumeration executed with the Glob tool over the pattern scripts/vscode/*.ps1, whose returned listing was read item by item and yielded, in the order returned, Install-RepoDotNetSdk.ps1, Invoke-MSTest.ps1, Invoke-MSTestWithCoverage.ClosureFilter.ps1, Invoke-MSTestWithCoverage.FirstParty.ps1, Invoke-MSTestWithCoverage.Helpers.ps1, Invoke-MSTestWithCoverage.PackageRate.ps1, Invoke-MSTestWithCoverage.ps1, Invoke-MSTestWithCoverage.Threshold.ps1, Invoke-Restore.ps1, Invoke-VSBuild.ps1, Sync-PackageReferences.ps1, TestProcessCleanup.ps1; this enumeration is extension-filtered and therefore returns no runsettings entry.

Primary Member Set: Install-RepoDotNetSdk.ps1, Invoke-MSTest.ps1, Invoke-MSTestWithCoverage.ClosureFilter.ps1, Invoke-MSTestWithCoverage.FirstParty.ps1, Invoke-MSTestWithCoverage.Helpers.ps1, Invoke-MSTestWithCoverage.PackageRate.ps1, Invoke-MSTestWithCoverage.ps1, Invoke-MSTestWithCoverage.Threshold.ps1, Invoke-Restore.ps1, Invoke-VSBuild.ps1, Sync-PackageReferences.ps1, TestProcessCleanup.ps1

Primary Count: 12

Cross-check Search Strategy: an independent content-scan tally executed with the Grep tool in count output mode, using the every-line regular expression anchored at start of line and directed at the scripts/vscode directory with no extension filter at all, so that every file in that directory produced a per-file line-count record; the emitted records named Install-RepoDotNetSdk.ps1 at 111 lines, Invoke-MSTestWithCoverage.ClosureFilter.ps1 at 413, Invoke-MSTestWithCoverage.PackageRate.ps1 at 65, Invoke-MSTestWithCoverage.Threshold.ps1 at 56, Invoke-MSTestWithCoverage.Helpers.ps1 at 470, Invoke-MSTestWithCoverage.ps1 at 351, TestProcessCleanup.ps1 at 70, Invoke-VSBuild.ps1 at 167, Sync-PackageReferences.ps1 at 159, Invoke-MSTest.ps1 at 202, Invoke-MSTestWithCoverage.FirstParty.ps1 at 162, Invoke-Restore.ps1 at 39, and one additional record for TaskMaster.cli.runsettings at 9 lines which the Exclusion Rules discard because it is not a PowerShell script; the tally therefore enumerated thirteen directory entries and retained twelve.

Cross-check Member Set: Invoke-MSTest.ps1, Invoke-MSTestWithCoverage.ClosureFilter.ps1, Invoke-MSTestWithCoverage.FirstParty.ps1, Invoke-MSTestWithCoverage.Helpers.ps1, Invoke-MSTestWithCoverage.PackageRate.ps1, Invoke-MSTestWithCoverage.ps1, Invoke-MSTestWithCoverage.Threshold.ps1, Install-RepoDotNetSdk.ps1, Invoke-Restore.ps1, Invoke-VSBuild.ps1, Sync-PackageReferences.ps1, TestProcessCleanup.ps1

Cross-check Count: 12

Member-set Comparison: the primary member set and the cross-check member set are equal when compared as unordered, case-insensitive sets of bare file names; all twelve names appear in both, neither set holds a name absent from the other, and both cardinalities are 12, so the two independent enumerations match and the count of 12 production PowerShell scripts under scripts/vscode is asserted.
