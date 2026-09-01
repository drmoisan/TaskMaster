# 2026-08-28-qfc-initializewebviewasync-fault-is-unobserved (Plan)

- **Issue:** #670
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T21-10
- **Status:** Ready for execution
- **Version:** 1.0
- **Work Mode:** full-bug (acceptance criteria come from `spec.md` only; there is no `user-story.md` and none is created)

**Fail-closed evidence rule:** Every baseline, regression, and QA task below names its artifact path. If a required artifact is missing or incomplete, the verdict is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Do not mark an evidence-backed task complete without its artifact on disk carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`.

## 0. Base ref, evidence root, and non-overridable path correction

`BASE` for every diff gate in this plan is the literal commit `2b85134b42872e405602e6064e02dc9cda6c319b` (`origin/main` at plan authoring time). Every `git diff` below carries that ref explicitly; an unanchored `git diff` compares the worktree against the index and passes vacuously once the executor commits, so no unanchored form appears in any acceptance condition.

All evidence for this issue resolves under `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/<kind>/` using only the canonical kinds `baseline`, `regression-testing`, `qa-gates`, and `other`.

`EVIDENCE_LOCATION_OVERRIDE_REJECTED: docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/coverage/ replaced with docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/baseline/ (Phase 0 coverage) and docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/ (Phase 4 coverage).` AC14 of `spec.md` names `evidence/coverage/`, which is not a canonical kind. That instruction is rejected and the canonical kinds above are substituted. AC14 is satisfied by the substituted paths; no `evidence/coverage/` directory is created.

Artifact file names in this plan are fixed and carry no timestamp in the name, so every acceptance condition can name a concrete path with no placeholder character in it. The ISO-8601 timestamp is recorded inside each artifact on its `Timestamp:` line, per `evidence-and-timestamp-conventions`.

## 1. Settled design (do not re-open)

The remediation shape is settled by `research/initializewebviewasync-fault-observation.2026-08-31T20-30.md` §2.3, §4.5, §5.2 and §9, and carried into `spec.md`. This plan implements it and adds no design latitude.

**New production file.** `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, `namespace QuickFiler.Controllers`, `internal partial class QfcItemController`, no `#nullable enable` directive. A new file is mandatory rather than stylistic: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` measures 499 lines against the repository's 500-line ceiling (re-measured while authoring this plan: last content line 499), so the two members cannot land there.

**Two members.** The plan quotes both literals here, outside every command span, because later acceptance conditions search for them and they do not yet exist in the tree:

- `WebViewInitializationErrorSink`
- `InitializeWebViewGuardedAsync`

**csproj.** Exactly one added `<Compile Include>` line in `QuickFiler/QuickFiler.csproj`. That project enumerates the `QfcItemController` partials explicitly at lines 331-340 with no wildcard (re-derived: line 331 `QfcItemController.cs`, 332 `QfcItemController.Initialization.cs`, 333 `QfcItemController.ViewerSetup.cs`, through 340 `QfcItemController.MailActions.cs`). `.csharpierignore:12` is `*.csproj`, so that edit is never reformatted and does not participate in the format check.

**Three call-site substitutions**, all in `QuickFiler/Controllers/QfcItemController.Initialization.cs`, all net-zero-line replacements. No `#670` comment is added at any of the three sites: adding one would shift every line citation in this plan and would break the exact three-added/three-deleted diff gate in Phase 2. Re-derived current text: line 192 `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);`, line 288 `_ = InitializeWebViewAsync();`, line 324 `_ = InitializeWebViewAsync();`.

**Line 256 is deliberately unchanged**, and `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` receives zero changed lines. Line 256 currently reads `await InitializeWebViewAsync();` inside `public async Task InitializeAsync()` (declared at line 202), so its fault is already observed. Routing it through the guard would swallow the fault that `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` (`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:245`) asserts.

**Logging form.** log4net, message first and exception second: `logger.Error(message, exception)`. The static field is `private static readonly log4net.ILog logger` at `QuickFiler/Controllers/QfcItemController.cs:30`. The exception-first spelling does not exist on `log4net.ILog` and would not compile.

**Ratified precedent.** `QuickFiler/Controllers/EfcFormController.cs:127-129` declares `BoundaryErrorSink` in exactly this shape, and `EfcFormController.cs:940-950` is the fault-containing `async Task` member it protects, with the `catch (OperationCanceledException)` arm at `EfcFormController.cs:989-991`.

**Broad catch is deliberate.** `.claude/rules/csharp.md:27` permits `catch (Exception)` at a defined boundary with added context. `InitializeWebViewGuardedAsync` is that boundary and the sink call supplies the context.

## 2. Test placement and the line budget

`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` measures 398 lines (re-measured while authoring this plan), so it has 102 lines of headroom before the 500-line ceiling. `spec.md` AC4, AC5 and AC6 each name that file, so the three spec-named tests go there and nowhere else. The three tests must fit inside a budget of **100 added lines** for that file; if the drafted bodies exceed it, compact the XML documentation comments rather than relocating a test, because relocating one would falsify its acceptance criterion.

Two further additions do **not** go in `Part3.cs`, because they would push it past the ceiling:

- The shared arrange helper `BuildGuardedWebViewTarget` and
- a fourth test, `InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink`,

both go in `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`, which measures 209 lines and is the primary partial of the same `[TestClass] public partial class QfcItemController_InitializationTests` (attribute at line 29, `PumpTimeoutMs` at line 38, the shared `BuildHomeController` helper at line 40). That file is already carried by a `<Compile Include>` entry, so `QuickFiler.Test/QuickFiler.Test.csproj` remains unchanged, as AC1 requires.

**The fourth test is required by AC13, not optional.** `spec.md` "Edge cases and negative scenarios" leaves the cancellation arm to the planner's election. It is elected here because without it the `catch (OperationCanceledException)` arm of the guard is the only uncovered region of a very small new file, and AC13 demands `>= 90%` line coverage on that file. Adding it is the only way AC13 and AC3 can both hold.

## 3. Bugfix-workflow sequencing — why the literal RED step does not apply

`CLAUDE.md` "Bugfix Workflow" requires a failing regression test first. That cannot be applied literally here, and research §9 records why: the primary test asserts against `InitializeWebViewGuardedAsync` and `WebViewInitializationErrorSink`, neither of which exists before the fix. A test written first would fail to **compile**, and a non-compiling test assembly is not a red signal — it reports nothing about the defect, only about the missing member.

The substantive red step is therefore a mutation check, sequenced as follows and executed in Phase 3:

1. Author the boundary member and the sink so the assembly compiles (Phase 1).
2. Author the test that asserts the sink receives the seam fault (P3-T2).
3. Run that test and record it green (P3-T4).
4. Remove the sink invocation from the guard's `catch (Exception ex)` arm, re-run the identical command, and record it red (P3-T5, tagged `[expect-fail]`).
5. Restore the sink invocation and record it green again (P3-T6).

The discriminating evidence is the pair P3-T4 and P3-T5: the same command, the same filter, the same assembly, differing only by the presence of the sink invocation, exiting 0 and then non-zero. A test that passed in both states would prove it does not observe the fix at all. Both runs are recorded under `evidence/regression-testing/`.

## 4. Coverage authority applied

Two repository authorities state different repository-wide floors: `CLAUDE.md` and `.claude/rules/csharp.md:39` state `>= 80%`, while `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state `>= 85%` line and `>= 75%` branch. The divergence is unresolved and #670 does not resolve it. **No acceptance condition in this plan asserts a repository-wide percentage as pass or fail.** The two coverage gates used are:

- the unambiguous new-module rule, `>= 90%` line coverage on `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` (CLAUDE.md §UT2, `.claude/rules/csharp.md:40`), and
- a no-regression comparison against the Phase 0 baseline captured before any edit.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1:341` calls `Assert-CoberturaLineCoverageThreshold` **before** it writes the post-processed XML at line 343, and that helper throws when the repository-wide line rate is below 80%. A non-zero exit from the runner therefore does not by itself mean a test failed. Every coverage task below records the exit code verbatim and derives its numbers from the `line` nodes of the Cobertura document rather than from the aggregate attribute, so the same arithmetic applies whether or not post-processing completed.

The runner merges Cobertura `class` nodes by filename (`Merge-CoberturaClassesByFilename`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:262`), which is what makes a per-file figure available for AC13. The extraction expressions below reproduce that merge explicitly (distinct line numbers, maximum hits) so they are correct on the raw document too.

Coverage figures are extracted with an inline PowerShell expression stated in each task rather than with a committed script. A committed PowerShell file would fall under `.claude/rules/general-unit-test.md` "Test File Location" and the PowerShell coverage obligations, which is scope this issue does not carry.

## 5. Toolchain invocation note

`CLAUDE.md` states the toolchain commands as `msbuild TaskMaster.sln ...`. This worktree is an isolated agent worktree and `msbuild.exe` is not guaranteed to be on `PATH` there, so every msbuild task below resolves the executable through `vswhere` first and then invokes it. The target and the property set are character-for-character the CLAUDE.md set: `/t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"` plus the step-specific properties. `/t:Build` is never substituted and `/p:Nullable=enable` is never added.

---

### Phase 0 — Context and Baseline Capture

- [ ] [P0-T1] Read, in the `policy-compliance-order` order, `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/csharp.md`, `.claude/rules/tonality.md`, and `.claude/rules/plan-acceptance-gates.md`; write `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/baseline/phase0-instructions-read.md` containing a `Timestamp:` line, a `Policy Order:` line, and one bullet per file naming the repository-relative path. **Acceptance:** that artifact exists and lists all seven paths.

- [ ] [P0-T2] Provision the repository-pinned .NET SDK in this worktree by running `pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1` from the worktree root. `global.json` pins `sdk.version` `8.0.205` with `paths` `[".dotnet-sdk", "$host$"]`, and this worktree has no `.dotnet-sdk` directory, so `dotnet --version` prints the `global.json` `errorMessage` until this runs. **Acceptance:** `dotnet --version` prints `8.0.205` and `dotnet --list-sdks` includes a path ending `.dotnet-sdk\sdk`; record both outputs in `evidence/baseline/p0-t2-sdk-bootstrap.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [ ] [P0-T3] Restore NuGet packages with `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1` (its parameters are `-SolutionPath` default `TaskMaster.sln`, `-Configuration` default `Debug`, `-Platform` default `Any CPU`; the defaults are correct here). A fresh worktree has no `packages/` directory and every project's `EnsureNuGetPackageBuildImports` target errors before compilation without it. **Acceptance:** `EXIT_CODE: 0` and the `packages` directory exists; record in `evidence/baseline/p0-t3-nuget-restore.md`.

- [ ] [P0-T4] Verify every analyzer assembly referenced by a first-party project resolves on disk after P0-T3, because a missing `<Analyzer Include>` path is compiler error CS0006 rather than a warning. Each `Analyzer` `Include` path resolves against the directory of the project that declares it, not against the repository root, so the check must join it to that project's own directory. Run: `Get-ChildItem -Path . -Filter *.csproj -Recurse | Where-Object { $_.FullName -notmatch '\\packages\\' } | ForEach-Object { $dir = $_.DirectoryName; ([xml](Get-Content -LiteralPath $_.FullName)).Project.ItemGroup.Analyzer.Include | Where-Object { $_ } | ForEach-Object { [pscustomobject]@{ Project = $dir; Path = $_; Exists = (Test-Path (Join-Path $dir $_)) } } }` and, for any entry reporting `Exists` false, install the exact package version named in that offending path into the `packages` directory with `nuget install` before continuing. **Acceptance:** every enumerated analyzer path resolves; record the full enumeration and the count of unresolved paths (target zero) in `evidence/baseline/p0-t4-analyzer-paths.md`.

- [ ] [P0-T5] Run `dotnet tool restore` from the worktree root so the manifest-pinned CSharpier 1.2.6 is available to `dotnet tool run`. **Acceptance:** `EXIT_CODE: 0`; record in `evidence/baseline/p0-t5-tool-restore.md`.

- [ ] [P0-T6] Provision the `dotnet-coverage` global tool, which `dotnet tool restore` does not supply because it is not in `dotnet-tools.json`: `if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }`. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:292-293` throws before running anything when it is absent, so P0-T12 records no coverage number without it. **Acceptance:** `Get-Command dotnet-coverage` resolves; record the resolved path in `evidence/baseline/p0-t6-dotnet-coverage-tool.md`.

- [ ] [P0-T7] Record the delivery-run head and confirm the diff base is reachable: run `git rev-parse HEAD`, `git rev-parse --abbrev-ref HEAD`, and `git merge-base --is-ancestor 2b85134b42872e405602e6064e02dc9cda6c319b HEAD`. **Acceptance:** the `--is-ancestor` invocation exits 0, proving every later `git diff 2b85134b42872e405602e6064e02dc9cda6c319b` gate in this plan is well-formed; record all three outputs in `evidence/baseline/p0-t7-base-ref.md`. If it exits non-zero the admission condition has failed and the plan is BLOCKED pending a re-anchor by the caller.

- [ ] [P0-T8] Re-verify the file-size admission condition against the then-current tree, because `QfcItemController.ViewerSetup.cs` has one line of headroom and a concurrent merge into it invalidates this plan. Run `foreach ($p in @('QuickFiler/Controllers/QfcItemController.ViewerSetup.cs','QuickFiler/Controllers/QfcItemController.Initialization.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs')) { [pscustomobject]@{ Path = $p; Lines = (Get-Content -LiteralPath $p).Count } }`. **Acceptance:** the artifact `evidence/baseline/p0-t8-admission-line-counts.md` records the four measured numbers, and all four of these hold: `ViewerSetup.cs` is 499, `Initialization.cs` is 489, `Part3.cs` is at most 400, and `InitializationTests.cs` is at most 260. Any other measurement is an admission-condition failure and the plan is BLOCKED pending re-planning.

- [ ] [P0-T9] Capture the CSharpier baseline with the read-only command `dotnet tool run csharpier check .`, which is run **before** any write-mode formatter in this plan so a pre-existing drift cannot be silently repaired into the baseline. **Acceptance:** `evidence/baseline/p0-t9-csharpier-check.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, and the `Output Summary:` reproduces the final summary line the command printed verbatim plus the repository-relative path of every file the command named as needing formatting (an empty list when the tree is clean). A clean baseline here is what makes the repo-wide format in P4-T1 a no-op outside this plan's own files, which is in turn what makes the zero-changed-lines gate on `ViewerSetup.cs` in P2-T4 satisfiable.

- [ ] [P0-T10] Capture the analyzer baseline. Resolve MSBuild, then run the CLAUDE.md analyzer command:

      $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
      $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
      & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

  **Acceptance:** `evidence/baseline/p0-t10-msbuild-analyzers.md` records `Timestamp:`, `Command:` (including the resolved MSBuild path), `EXIT_CODE:`, and an `Output Summary:` carrying the MSBuild warning count and error count.

- [ ] [P0-T11] Capture the nullable/type-check baseline with the same MSBuild resolution and `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`. Do not add `/p:Nullable=enable` and do not substitute `/t:Build`. **Acceptance:** `evidence/baseline/p0-t11-msbuild-nullable.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` carrying the warning and error counts.

- [ ] [P0-T12] Capture the test-and-coverage baseline with `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\baseline.cobertura.xml`, then copy `coverage/baseline.cobertura.xml` to `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/baseline/baseline.cobertura.xml` (the `coverage/` directory is git-ignored by `.gitignore:144`, so the committed copy is the evidence). **Acceptance:** `evidence/baseline/p0-t12-vstest-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` that carries the script's `Discovered N test assemblies.` line verbatim, the total/passed/failed test counts, and a numeric repository-wide line-coverage headline percentage. The copied Cobertura file exists at the evidence path.

- [ ] [P0-T13] Derive the baseline coverage counters that the Phase 4 delta gate consumes, using the same arithmetic the runner's filename merge uses:

      [xml]$c = Get-Content -LiteralPath 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/baseline/baseline.cobertura.xml'
      $rows = $c.SelectNodes('//class/lines/line') | ForEach-Object { [pscustomobject]@{ File = $_.ParentNode.ParentNode.GetAttribute('filename'); Num = [int]$_.GetAttribute('number'); Hits = [int]$_.GetAttribute('hits') } }
      $g = $rows | Group-Object File, Num
      $valid = $g.Count
      $covered = ($g | Where-Object { ($_.Group | Measure-Object -Property Hits -Maximum).Maximum -gt 0 }).Count
      '{0} covered / {1} valid = {2:N4}%' -f $covered, $valid, (100 * $covered / $valid)

  **Acceptance:** `evidence/baseline/p0-t13-coverage-counters.md` records the expression verbatim and the three derived values `BASELINE_LINES_COVERED`, `BASELINE_LINES_VALID`, and `BASELINE_LINE_PERCENT` as concrete numbers, and additionally records that no `class` node in the baseline document has a `filename` containing `QfcItemController.WebViewFaultBoundary.cs` (the file does not exist yet).

- [ ] [P0-T14] Record the baseline failure set so Phase 4 can gate on a subset relation rather than on a repository-wide zero-failure claim that the pre-existing suite may not satisfy. From the P0-T12 run output, list every failing and every skipped test by fully qualified name. **Acceptance:** `evidence/baseline/p0-t14-baseline-failure-set.md` records `BASELINE_FAILURE_SET:` followed by one fully qualified test name per line, or the single token `NONE` when the suite is fully green, and `BASELINE_SKIPPED_COUNT:` as a number.

### Phase 1 — Fault Boundary

- [ ] [P1-T1] Create `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` carrying exactly these two `using` directives, the `QuickFiler.Controllers` namespace, and an empty `internal partial class QfcItemController` body, with **no** `#nullable enable` directive (neither sibling partial carries one, the repository is per-file opt-in, and adding it would conscript the file into the `TreatWarningsAsErrors` gate for no benefit):

      using System;
      using System.Threading.Tasks;

      namespace QuickFiler.Controllers
      {
          internal partial class QfcItemController
          {
          }
      }

  **Acceptance:** the file exists; `Select-String -Path 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs' -SimpleMatch 'internal partial class QfcItemController'` returns one match; `Select-String -Path 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs' -SimpleMatch 'namespace QuickFiler.Controllers'` returns one match; and `Select-String -Path 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs' -SimpleMatch '#nullable'` returns zero matches.

- [ ] [P1-T2] Add exactly one `<Compile Include>` line to `QuickFiler/QuickFiler.csproj`, immediately after the existing line 333 entry for `QfcItemController.ViewerSetup.cs`, reading `<Compile Include="Controllers\QfcItemController.WebViewFaultBoundary.cs" />` with the same four-space indentation as its neighbours. Then run `git add QuickFiler/QuickFiler.csproj QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` so the new untracked file is visible to a name-listing diff. **Acceptance:** `git diff --numstat 2b85134b42872e405602e6064e02dc9cda6c319b -- QuickFiler/QuickFiler.csproj` reports exactly one added line and zero deleted lines, and `git status --porcelain -- QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` reports the new file as added. `.csharpierignore:12` excludes `*.csproj`, so this edit is never reformatted.

- [ ] [P1-T3] Add the sink property to `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, inside the class body, exactly as follows. The declaration is split across an explicit accessor block because the single-line form exceeds the formatter's print width and its post-format shape would then be unpredictable; the initializer is the ratified `EfcFormController.cs:128-129` expression:

      /// <summary>
      /// #670 fault-boundary sink: an injectable seam over the static log4net logger declared at
      /// QfcItemController.cs:30. Named distinctly from EfcFormController.BoundaryErrorSink so no
      /// shared contract between the two types is implied.
      /// </summary>
      internal System.Action<string, System.Exception> WebViewInitializationErrorSink
      {
          get;
          set;
      } = (message, exception) => logger.Error(message, exception);

  **Acceptance:** `Select-String -Path 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs' -SimpleMatch 'WebViewInitializationErrorSink'` returns at least one match, and `Select-String -Path 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs' -SimpleMatch 'logger.Error(message, exception)'` returns exactly one match.

- [ ] [P1-T4] Add the guard method to the same file, inside the class body, exactly as follows:

      /// <summary>
      /// #670 fault boundary for InitializeWebViewAsync. Three production call sites discard the
      /// returned task, so a fault there is never observed. This member contains the fault instead
      /// of returning it: the task it returns never transitions to Faulted.
      /// </summary>
      internal async Task InitializeWebViewGuardedAsync()
      {
          try
          {
              await InitializeWebViewAsync();
          }
          catch (OperationCanceledException)
          {
              // Cooperative cancellation during QuickFiler teardown is expected and is not a
              // fault: InitializeWebViewAsync opens with Token.ThrowIfCancellationRequested().
          }
          catch (Exception ex)
          {
              WebViewInitializationErrorSink("WebView2 initialization failed.", ex);
          }
      }

  **Acceptance:** all four of these single-line, non-interpolated literals are found by `Select-String -SimpleMatch` in `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, each exactly once — `internal async Task InitializeWebViewGuardedAsync()`, `await InitializeWebViewAsync();`, `catch (OperationCanceledException)`, and `WebViewInitializationErrorSink("WebView2 initialization failed.", ex);` — and `Select-String -SimpleMatch 'throw'` returns zero matches in that file.

- [ ] [P1-T5] Normalize the new file's formatting now, so every later assertion reads a formatter-stable shape and the repo-wide format in P4-T1 has nothing left to rewrite here: run `dotnet tool run csharpier format QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, then the read-only `dotnet tool run csharpier check QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, then `git status --porcelain -- QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`. The `format` invocation is write-mode and exits 0 whether or not it rewrote the file, so its exit code alone is not the observation. **Acceptance:** the `check` invocation exits 0, and the artifact `evidence/other/p1-t5-new-file-format.md` records the SHA-256 of the file before and after the `format` invocation together with the `git status --porcelain` output, so a rewrite is distinguishable from a clean pass.

- [ ] [P1-T6] Prove the new partial compiles, using the MSBuild resolution from P0-T10 and the analyzer command `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. **Acceptance:** `EXIT_CODE: 0`; record in `evidence/qa-gates/p1-t6-build-after-boundary.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` carrying the warning and error counts.

- [ ] [P1-T7] Confirm the new file is under the ceiling and is the only new production file: `(Get-Content -LiteralPath 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs').Count`, and `git status --porcelain -- QuickFiler` together with `git diff --name-status 2b85134b42872e405602e6064e02dc9cda6c319b -- QuickFiler` (the staging performed in P1-T2 is what lets the name-listing diff see the created file). **Acceptance:** the line count is at most 60, and the combined file set under `QuickFiler/` is exactly `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` and `QuickFiler/QuickFiler.csproj`; record both in `evidence/qa-gates/p1-t7-new-file-audit.md`.

### Phase 2 — Call-Site Observation

- [ ] [P2-T1] In `QuickFiler/Controllers/QfcItemController.Initialization.cs`, replace line 192 with `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewGuardedAsync);`, preserving the existing twelve-space indentation and adding no comment, so the file's line count and every line citation in this plan are unchanged. No `.Unwrap()` is required: the dispatched delegate is now an `async Task` method that catches `Exception`, so the `DispatcherOperation<Task>` the site discards carries no observable fault. **Acceptance:** `(Get-Content -LiteralPath 'QuickFiler/Controllers/QfcItemController.Initialization.cs')[191].Trim()` equals `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewGuardedAsync);`.

- [ ] [P2-T2] In the same file, replace line 288 with `_ = InitializeWebViewGuardedAsync();`, preserving indentation and adding no comment. **Acceptance:** `(Get-Content -LiteralPath 'QuickFiler/Controllers/QfcItemController.Initialization.cs')[287].Trim()` equals `_ = InitializeWebViewGuardedAsync();`.

- [ ] [P2-T3] In the same file, replace line 324 with `_ = InitializeWebViewGuardedAsync();`, preserving indentation and adding no comment. **Acceptance:** `(Get-Content -LiteralPath 'QuickFiler/Controllers/QfcItemController.Initialization.cs')[323].Trim()` equals `_ = InitializeWebViewGuardedAsync();`.

- [ ] [P2-T4] Verify the deliberately unchanged site and the untouched file. Run `(Get-Content -LiteralPath 'QuickFiler/Controllers/QfcItemController.Initialization.cs')[255].Trim()`, `git diff --numstat 2b85134b42872e405602e6064e02dc9cda6c319b -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, and `git status --porcelain -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`. **Acceptance:** line 256 still reads `await InitializeWebViewAsync();`, naming the unguarded member; the `--numstat` invocation prints nothing at all for `ViewerSetup.cs`; and the porcelain status prints nothing. Record all three in `evidence/qa-gates/p2-t4-line-256-and-viewersetup.md`.

- [ ] [P2-T5] Verify the call-site substitution is exactly three lines and introduced no new observation construct. Run `Select-String -Path 'QuickFiler/Controllers/QfcItemController.Initialization.cs' -SimpleMatch 'InitializeWebViewGuardedAsync'`, `Select-String -Path 'QuickFiler/Controllers/QfcItemController.Initialization.cs' -SimpleMatch 'InitializeWebViewAsync'`, `Select-String -Path 'QuickFiler/Controllers/QfcItemController.Initialization.cs' -SimpleMatch '.Unwrap()'`, `Select-String -Path 'QuickFiler/Controllers/QfcItemController.Initialization.cs' -SimpleMatch 'ContinueWith'`, and `git diff --numstat 2b85134b42872e405602e6064e02dc9cda6c319b -- QuickFiler/Controllers/QfcItemController.Initialization.cs`. `InitializeWebViewAsync` is not a substring of `InitializeWebViewGuardedAsync`, so the two counts are independent. **Acceptance:** exactly three matches for `InitializeWebViewGuardedAsync`, at lines 192, 288 and 324; exactly five matches for `InitializeWebViewAsync`, at lines 165, 193, 200, 256 and 345, of which only line 256 is executable code (165 and 200 are prose comments, 193 and 345 are commented-out code); zero matches for `.Unwrap()`; zero matches for `ContinueWith`; and the `--numstat` line reports exactly three added and three deleted lines. Record in `evidence/qa-gates/p2-t5-call-site-audit.md`.

- [ ] [P2-T6] Rebuild with the analyzer command from P0-T10 to prove the three substituted sites compile. **Acceptance:** `EXIT_CODE: 0`; record in `evidence/qa-gates/p2-t6-build-after-call-sites.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

### Phase 3 — Regression Tests

- [ ] [P3-T1] Add the shared arrange helper to `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`, inside the existing `public partial class QfcItemController_InitializationTests` body. It wires the controller to the faulting `IWebViewCoreInitializer` mock built by `BuildWebViewInitializerMock` (`QfcItemController.InitializationTests.Part2.cs:243`, both members stubbed `.ThrowsAsync(new WebViewSentinelException())` at lines 246-261) and to an `IItemViewer` mock whose `UiSyncContext` returns the supplied context:

      private static HarnessController BuildGuardedWebViewTarget(SynchronizationContext context)
      {
          HarnessController controller = new HarnessController();
          QfcItemControllerTestSupport.SetField(
              controller,
              "_webViewInitializer",
              BuildWebViewInitializerMock().Object
          );
          Mock<IItemViewer> viewer = new Mock<IItemViewer>();
          viewer.SetupGet(v => v.UiSyncContext).Returns(context);
          QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
          return controller;
      }

  No `using` directive is added: `System.Threading`, `Moq`, `QuickFiler.Viewers` and `FluentAssertions` are already imported by that file at lines 4, 12, 16 and 7, and `HarnessController` (`QfcItemController.TestSupport.cs:28`), `QfcItemControllerTestSupport.SetField` (`TestSupport.cs:40`) and `IWebViewCoreInitializer` (`QuickFiler.Viewers.IWebViewCoreInitializer`) are all already in scope there. **Acceptance:** `Select-String -Path 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs' -SimpleMatch 'BuildGuardedWebViewTarget'` returns exactly one match.

- [ ] [P3-T2] Add `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault` to `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`. It must carry `[TestMethod]` and must **not** carry `[Timeout]`, because it uses no pump. Arrange: save `SynchronizationContext.Current`, install a fresh `SynchronizationContext` as current, build the controller with `BuildGuardedWebViewTarget` passing that same instance, and capture the sink into a local `Exception`. Act: `Func<Task> act = () => controller.InitializeWebViewGuardedAsync();`. Assert: `await act.Should().NotThrowAsync(...)` and the captured exception `.Should().BeOfType<WebViewSentinelException>(...)`. Restore the previous `SynchronizationContext` in a `finally`. The installed-and-current arrangement is load-bearing rather than cosmetic: `UiThread.SynchronizationContextAwaiter.IsCompleted` is `_context == SynchronizationContext.Current` (`UtilitiesCS/Threading/UiThread.cs:100`), so awaiting `_itemViewer.UiSyncContext` at `QfcItemController.ViewerSetup.cs:64` continues inline only when the supplied context is already current; otherwise the continuation is posted to the thread pool where `SynchronizationContext.Current` is null and `TaskScheduler.FromCurrentSynchronizationContext()` at `ViewerSetup.cs:67` throws `InvalidOperationException` instead of the seam raising `WebViewSentinelException`. The `finally` restore is what keeps the ambient context from leaking onto a pooled thread and reaching another test. **Acceptance:** `Select-String -Path 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs' -SimpleMatch 'InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault'` returns exactly one match.

- [ ] [P3-T3] Rebuild with the analyzer command from P0-T10 so the new test compiles into `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`. **Acceptance:** `EXIT_CODE: 0`; record in `evidence/regression-testing/p3-t3-build.md`.

- [ ] [P3-T4] Run the new test alone and record it green. Resolve the runner and run:

      $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
      $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
      & $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/TestCaseFilter:FullyQualifiedName~InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault' /Logger:trx '/ResultsDirectory:coverage\testresults\p3-t4'

  `/Logger:trx` writes into `TestResults` relative to the working directory unless `/ResultsDirectory` is supplied, which is why the run gets its own directory. **Acceptance:** `EXIT_CODE: 0`, **and** the generated `.trx` under `coverage\testresults\p3-t4` contains at least one `Select-String -SimpleMatch` hit for `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault`, which rules out a vacuous pass from a filter that matched no test. Copy the `.trx` to `evidence/regression-testing/p3-t4-green.trx` and record the run in `evidence/regression-testing/p3-t4-green-run.md`.

- [ ] [P3-T5] [expect-fail] Demonstrate the red state. In `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, replace the single statement `WebViewInitializationErrorSink("WebView2 initialization failed.", ex);` with `_ = ex;` — a discard assignment, which keeps `ex` used so no unused-variable diagnostic appears and keeps the mutation a one-line, exactly reversible substitution. Rebuild with the analyzer command from P0-T10, then re-run the **identical** vstest command from P3-T4 with `'/ResultsDirectory:coverage\testresults\p3-t5'`. **Acceptance:** `EXIT_CODE:` is non-zero with `ExpectedExitCode: 1`, and the generated `.trx` under `coverage\testresults\p3-t5` contains at least one `Select-String -SimpleMatch` hit for `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault`. The discriminating evidence is the pair of exit codes from P3-T4 and P3-T5 for the same command against the same assembly, differing only by the presence of the sink invocation. Copy the `.trx` to `evidence/regression-testing/p3-t5-red.trx` and record the run, the exact mutated line, and the failure message in `evidence/regression-testing/p3-t5-red-run.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1`, `Output Summary:`.

- [ ] [P3-T6] Restore the mutation: put `WebViewInitializationErrorSink("WebView2 initialization failed.", ex);` back in place of `_ = ex;`, rebuild with the analyzer command from P0-T10, and re-run the identical vstest command with `'/ResultsDirectory:coverage\testresults\p3-t6'`. **Acceptance:** `Select-String -Path 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs' -SimpleMatch '_ = ex;'` returns zero matches; `Select-String -Path 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs' -SimpleMatch 'WebViewInitializationErrorSink("WebView2 initialization failed.", ex);'` returns exactly one match; the vstest `EXIT_CODE:` is 0. Record in `evidence/regression-testing/p3-t6-restored-green-run.md`.

- [ ] [P3-T7] Add `WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing` to `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`, mirroring `EfcFormControllerTests.BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing`. It must exercise the default lambda body rather than a test double: arrange `HarnessController controller = new HarnessController();` with no sink assignment, act `Action act = () => controller.WebViewInitializationErrorSink("smoke", new InvalidOperationException());`, assert `act.Should().NotThrow(...)`. Carry `[TestMethod]` and no `[Timeout]`. **Acceptance:** `Select-String -Path 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs' -SimpleMatch 'WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing'` returns exactly one match.

- [ ] [P3-T8] Add `InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink` to `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`, carrying `[TestMethod]` and `[Timeout(PumpTimeoutMs)]`. Arrange `WinFormsPumpHost host = new WinFormsPumpHost();` and `harness = await BuildPumpHarnessAsync(host, darkMode: false)`, then — **before** the Act — install a signalling sink on `harness.Controller`: `var observed = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);` and `harness.Controller.WebViewInitializationErrorSink = (m, e) => observed.TrySetResult(e);`. The sink must be installed during Arrange because the dispatched operation may complete before `host.InvokeAsync` returns, in which case a sink installed after the Act would miss the callback and the test would hang to its timeout. Act: `await host.InvokeAsync(() => harness.Controller.Initialize(async: false));` then `Exception fault = await observed.Task;`. Assert `fault.Should().BeOfType<WebViewSentinelException>(...)`. Teardown follows the existing `finally { if (harness != null) { harness.Restore(); } await host.StopAsync().ConfigureAwait(false); }` shape used at `Part3.cs:155-163`. The only wait is the `TaskCompletionSource` completion; there is no polling and no sleep. **Acceptance:** `Select-String -Path 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs' -SimpleMatch 'InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink'` returns exactly one match.

- [ ] [P3-T9] Add `InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink` to `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` (not to `Part3.cs`, which has no room for a fourth test). Arrange: `HarnessController controller = BuildGuardedWebViewTarget(new SynchronizationContext());`, a `bool sinkInvoked = false;` sink, a `CancellationTokenSource` that is cancelled before the Act, and `controller.Token = source.Token;`. Act: `Func<Task> act = () => controller.InitializeWebViewGuardedAsync();`. Assert `await act.Should().NotThrowAsync(...)` and `sinkInvoked.Should().BeFalse(...)`. `QfcItemController.Token` is the public auto-property at `QuickFiler/Controllers/QfcItemController.cs:267`, and `InitializeWebViewAsync` opens with `Token.ThrowIfCancellationRequested()` at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:52`, before any seam call, so the cancellation arm is reached deterministically with no timing dependency. This test is what covers the guard's `catch (OperationCanceledException)` arm and is therefore load-bearing for AC13. **Acceptance:** `Select-String -Path 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs' -SimpleMatch 'InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink'` returns exactly one match.

- [ ] [P3-T10] Rebuild with the analyzer command from P0-T10, then run all four new tests together with `'/TestCaseFilter:FullyQualifiedName~InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault|FullyQualifiedName~WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing|FullyQualifiedName~InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink|FullyQualifiedName~InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink'` and `'/ResultsDirectory:coverage\testresults\p3-t10'`. **Acceptance:** the build exits 0; the vstest `EXIT_CODE:` is 0; and the generated `.trx` contains at least one `Select-String -SimpleMatch` hit for each of the four test names, proving four tests ran rather than a filter matching none. Copy the `.trx` to `evidence/regression-testing/p3-t10-new-tests.trx` and record in `evidence/regression-testing/p3-t10-new-tests.md`.

- [ ] [P3-T11] Run the three pre-existing tests AC9 pins and prove their bodies are unchanged. Run vstest with `'/TestCaseFilter:FullyQualifiedName~InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState|FullyQualifiedName~InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme|FullyQualifiedName~InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults'` and `'/ResultsDirectory:coverage\testresults\p3-t11'`, then run `git diff 2b85134b42872e405602e6064e02dc9cda6c319b -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` and `git status --porcelain -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`. **Acceptance:** the vstest `EXIT_CODE:` is 0; the `.trx` contains at least one hit for each of the three names; and the diff hunks touch only added lines, with no hunk modifying or deleting any line inside the bodies of the three pinned tests (which occupy lines 40-72, 83-116 and 245-288 of the base revision). Record the diff and the run in `evidence/regression-testing/p3-t11-pinned-tests.md`.

- [ ] [P3-T12] Format the two touched test files and confirm the result is stable: run `dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`, `dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`, then the read-only `dotnet tool run csharpier check QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` and `dotnet tool run csharpier check QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`. The two `format` invocations are write-mode and exit 0 whether or not they rewrote a file, so the observation is the SHA-256 before and after each, plus the `check` exit code. **Acceptance:** both `check` invocations exit 0, and `evidence/qa-gates/p3-t12-test-file-format.md` records the four SHA-256 values (before and after, per file).

- [ ] [P3-T13] Verify the post-format line counts of the two touched test files before committing, because the 500-line ceiling is measured on the formatted file: `foreach ($p in @('QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs')) { [pscustomobject]@{ Path = $p; Lines = (Get-Content -LiteralPath $p).Count } }`. **Acceptance:** `Part3.cs` is at most 500 and `InitializationTests.cs` is at most 500; record both numbers, and the added-line count for each relative to the P0-T8 baseline measurement, in `evidence/qa-gates/p3-t13-test-file-sizes.md`.

- [ ] [P3-T14] Commit the implementation and the evidence produced so far, so every Phase 4 diff gate compares committed content rather than ambient worktree state: `git add QuickFiler QuickFiler.Test docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670` then `git commit`. **Acceptance:** `git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670` prints nothing; record the resulting commit SHA in `evidence/other/p3-t14-implementation-commit.md`.

### Phase 4 — Final QC Loop

- [ ] [P4-T1] Toolchain step 1 of 4, formatting. Run `git status --porcelain -- QuickFiler QuickFiler.Test` and record it, then `dotnet tool run csharpier format .`, then `git status --porcelain -- QuickFiler QuickFiler.Test` again. `format` is write-mode and exits 0 whether or not it rewrote tracked source, so the exit code is not the observation; the before-and-after tree comparison is. Because P3-T14 committed everything, the "before" listing is empty. **Acceptance:** both porcelain listings print nothing, proving the repo-wide format rewrote no file under `QuickFiler/` or `QuickFiler.Test/`. Record both listings and the command's final summary line verbatim in `evidence/qa-gates/p4-t1-csharpier-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [ ] [P4-T2] Verify formatting read-only with `dotnet tool run csharpier check .`. **Acceptance:** `EXIT_CODE: 0`; record in `evidence/qa-gates/p4-t2-csharpier-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [ ] [P4-T3] Toolchain step 2 of 4, linting. Resolve MSBuild as in P0-T10 and run `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. **Acceptance:** `EXIT_CODE: 0` and zero errors; record in `evidence/qa-gates/p4-t3-msbuild-analyzers.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` carrying the warning and error counts.

- [ ] [P4-T4] Toolchain step 3 of 4, type checking. Run `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, with no `/p:Nullable=enable` and no `/t:Build`. **Acceptance:** `EXIT_CODE: 0`; record in `evidence/qa-gates/p4-t4-msbuild-nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [ ] [P4-T5] Toolchain step 4 of 4, testing with coverage. Run `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\postchange.cobertura.xml`, then copy `coverage/postchange.cobertura.xml` to `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml`. **Acceptance:** `evidence/qa-gates/p4-t5-vstest-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` carrying the script's `Discovered N test assemblies.` line verbatim, the total/passed/failed counts, and a numeric repository-wide line-coverage headline percentage; and the copied Cobertura file exists at the evidence path.

- [ ] [P4-T6] Derive the post-change repository-wide counters with the identical expression P0-T13 used, reading `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml`. **Acceptance:** `evidence/qa-gates/p4-t6-coverage-counters.md` records the expression verbatim and the three derived values `POSTCHANGE_LINES_COVERED`, `POSTCHANGE_LINES_VALID`, and `POSTCHANGE_LINE_PERCENT` as concrete numbers.

- [ ] [P4-T7] Derive the per-file figure for the new module from the same post-change document, reproducing the runner's filename merge:

      [xml]$c = Get-Content -LiteralPath 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml'
      $rows = $c.SelectNodes('//class/lines/line') | Where-Object { $_.ParentNode.ParentNode.GetAttribute('filename') -like '*QfcItemController.WebViewFaultBoundary.cs' } | ForEach-Object { [pscustomobject]@{ Num = [int]$_.GetAttribute('number'); Hits = [int]$_.GetAttribute('hits') } }
      $g = $rows | Group-Object Num
      $valid = $g.Count
      $covered = ($g | Where-Object { ($_.Group | Measure-Object -Property Hits -Maximum).Maximum -gt 0 }).Count
      '{0} covered / {1} valid = {2:N4}%' -f $covered, $valid, (100 * $covered / $valid)

  **Acceptance:** `$valid` is greater than zero (a zero denominator means the file was never instrumented and the figure would be meaningless rather than passing), and the derived percentage is at least 90. Record the expression, `NEWFILE_LINES_COVERED`, `NEWFILE_LINES_VALID`, `NEWFILE_LINE_PERCENT`, and the uncovered line numbers if any, in `evidence/qa-gates/p4-t7-newfile-coverage.md`.

- [ ] [P4-T8] Compare baseline and post-change coverage in one artifact. **Acceptance:** `evidence/qa-gates/p4-t8-coverage-delta.md` records all seven numbers — `BASELINE_LINES_COVERED`, `BASELINE_LINES_VALID`, `BASELINE_LINE_PERCENT`, `POSTCHANGE_LINES_COVERED`, `POSTCHANGE_LINES_VALID`, `POSTCHANGE_LINE_PERCENT`, `NEWFILE_LINE_PERCENT` — and both of these hold: `POSTCHANGE_LINES_COVERED` is greater than or equal to `BASELINE_LINES_COVERED`, and `POSTCHANGE_LINE_PERCENT` is greater than or equal to `BASELINE_LINE_PERCENT` minus 0.10 percentage points. The covered-line count is the substantive no-regression content, because this change deletes no line and adds only lines the new tests execute; the 0.10-point band on the ratio absorbs run-to-run variation in the shared instrumented run and is not a licence for a real regression. Neither condition asserts a repository-wide percentage as a pass/fail number.

- [ ] [P4-T9] Confirm no test regressed. From the P4-T5 run, list every failing test by fully qualified name. **Acceptance:** `evidence/qa-gates/p4-t9-failure-set.md` records `POSTCHANGE_FAILURE_SET:` and demonstrates that it is a subset of the `BASELINE_FAILURE_SET` recorded in P0-T14 — that is, every name in the post-change set also appears in the baseline set — and additionally records that all seven of these tests are reported passed: `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault`, `WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing`, `InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink`, `InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink`, `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState`, `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme`, `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults`.

- [ ] [P4-T10] Audit file sizes **after** the final format, since the ceiling is measured on formatted files: `foreach ($p in @('QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs','QuickFiler/Controllers/QfcItemController.Initialization.cs','QuickFiler/Controllers/QfcItemController.ViewerSetup.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs')) { [pscustomobject]@{ Path = $p; Lines = (Get-Content -LiteralPath $p).Count } }`. **Acceptance:** every one of the five counts is at most 500, `Initialization.cs` is exactly 489 (the three substitutions are net-zero-line), and `ViewerSetup.cs` is exactly 499 (unmodified). Record all five in `evidence/qa-gates/p4-t10-file-size-audit.md`.

- [ ] [P4-T11] Gate the changed-file set. Run `git diff --name-status 2b85134b42872e405602e6064e02dc9cda6c319b HEAD -- QuickFiler QuickFiler.Test` together with `git status --porcelain -- QuickFiler QuickFiler.Test` (the porcelain span is required because a name-listing diff enumerates tracked changes only and this plan creates a file; the staging in P1-T2 and the commit in P3-T14 are what put the created file into the diff's view). **Acceptance:** the name-status listing is exactly these five paths and no others — `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` (added), `QuickFiler/QuickFiler.csproj` (modified), `QuickFiler/Controllers/QfcItemController.Initialization.cs` (modified), `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` (modified), `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` (modified) — the porcelain listing prints nothing, and `git diff --numstat 2b85134b42872e405602e6064e02dc9cda6c319b HEAD -- QuickFiler.Test/QuickFiler.Test.csproj` prints nothing. Record in `evidence/qa-gates/p4-t11-changed-file-set.md`.

- [ ] [P4-T12] Audit the added test code for determinism-banned APIs. Run `Select-String -Path 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs' -SimpleMatch 'Thread.Sleep'`, the same over `'Task.Delay'`, the same over `'SpinWait'`, and the same over `'DateTime.Now'`. **Acceptance:** all four searches return zero matches across both files, and `evidence/qa-gates/p4-t12-determinism-audit.md` additionally records that the only wait in the pump-hosted test is `await observed.Task` on a `TaskCompletionSource` completed from the sink callback, quoting that line from the test body.

- [ ] [P4-T13] Check off AC1 in `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md` by changing `- [ ] **AC1**` to `- [x] **AC1**`, citing `evidence/qa-gates/p1-t7-new-file-audit.md`, `evidence/qa-gates/p4-t11-changed-file-set.md`, and `evidence/qa-gates/p4-t3-msbuild-analyzers.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC1**'` returns exactly one match.

- [ ] [P4-T14] Check off AC2 the same way, citing `evidence/other/p1-t5-new-file-format.md` (which records the formatted declaration) and `evidence/qa-gates/p4-t3-msbuild-analyzers.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC2**'` returns exactly one match.

- [ ] [P4-T15] Check off AC3, citing the P1-T4 literal audit and `evidence/regression-testing/p3-t10-new-tests.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC3**'` returns exactly one match.

- [ ] [P4-T16] Check off AC4, citing `evidence/regression-testing/p3-t4-green-run.md`, `evidence/regression-testing/p3-t5-red-run.md`, and `evidence/qa-gates/p4-t9-failure-set.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC4**'` returns exactly one match.

- [ ] [P4-T17] Check off AC5, citing `evidence/regression-testing/p3-t10-new-tests.md` and `evidence/qa-gates/p4-t9-failure-set.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC5**'` returns exactly one match.

- [ ] [P4-T18] Check off AC6, citing `evidence/regression-testing/p3-t10-new-tests.md` and `evidence/qa-gates/p4-t9-failure-set.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC6**'` returns exactly one match.

- [ ] [P4-T19] Check off AC7, citing `evidence/qa-gates/p2-t5-call-site-audit.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC7**'` returns exactly one match.

- [ ] [P4-T20] Check off AC8, citing `evidence/qa-gates/p2-t4-line-256-and-viewersetup.md` and `evidence/qa-gates/p4-t11-changed-file-set.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC8**'` returns exactly one match.

- [ ] [P4-T21] Check off AC9, citing `evidence/regression-testing/p3-t11-pinned-tests.md` and `evidence/qa-gates/p4-t9-failure-set.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC9**'` returns exactly one match.

- [ ] [P4-T22] Check off AC10, citing `evidence/qa-gates/p4-t1-csharpier-format.md`, `evidence/qa-gates/p4-t2-csharpier-check.md`, `evidence/qa-gates/p4-t3-msbuild-analyzers.md`, `evidence/qa-gates/p4-t4-msbuild-nullable.md`, and `evidence/qa-gates/p4-t5-vstest-coverage.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC10**'` returns exactly one match.

- [ ] [P4-T23] Check off AC11, citing `evidence/qa-gates/p4-t10-file-size-audit.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC11**'` returns exactly one match.

- [ ] [P4-T24] Check off AC12, citing `evidence/qa-gates/p4-t12-determinism-audit.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC12**'` returns exactly one match.

- [ ] [P4-T25] Check off AC13, citing `evidence/qa-gates/p4-t7-newfile-coverage.md`. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC13**'` returns exactly one match.

- [ ] [P4-T26] Check off AC14, citing `evidence/qa-gates/p4-t8-coverage-delta.md`. In the same edit, correct the criterion's own evidence path: AC14 currently names one non-canonical location on `spec.md:503`, and that occurrence must be **replaced** by the two canonical paths `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/baseline/` and `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/`, then one sentence appended recording that the substitution was made under the non-overridable evidence-path clause. That appended sentence is the only place the superseded spelling may still appear. **Acceptance:** `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch '- [x] **AC14**'` returns exactly one match, and `Select-String -Path 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md' -SimpleMatch 'evidence/coverage/'` returns exactly one match, on the appended correction sentence.

- [ ] [P4-T27] Record the follow-up items the spec requires and this issue does not deliver: the `EfcItemController` fire-and-forget sites at `QuickFiler/Controllers/EfcItemController.cs:97` and `:153` (suggested slug `efc-item-controller-initializewebviewasync-fault-is-unobserved`), the optional `TaskScheduler.UnobservedTaskException` backstop, and the coverage-floor divergence between `CLAUDE.md` and `.claude/rules/general-unit-test.md`. **Acceptance:** `evidence/other/p4-t27-followups.md` names all three with their preconditions, so the orchestrator can route each through the promotion lifecycle rather than leaving it as prose that disappears at merge.

- [ ] [P4-T28] Commit the QA evidence and the spec check-offs: `git add QuickFiler QuickFiler.Test docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670` then `git commit`. **Acceptance:** `git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670` prints nothing, and `git diff --name-status 2b85134b42872e405602e6064e02dc9cda6c319b HEAD -- QuickFiler QuickFiler.Test` still lists exactly the five paths enumerated in P4-T11. Record the commit SHA in `evidence/other/p4-t28-final-commit.md`.
