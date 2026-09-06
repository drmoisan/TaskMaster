# Preflight round 1 — atomic-executor revisions required (issue #781)

- Timestamp: 2026-09-05T19-59 (UTC)
- Plan under review: `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/plan.2026-09-05T10-49.md` (version 1.2)
- Directive: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`
- Result: `PREFLIGHT: REVISIONS REQUIRED`
- Convergence: `CONVERGENCE: NO FURTHER ROUNDS EXPECTED`
- Recorded by: orchestrator session taskmaster-1d, verbatim from the atomic-executor preflight return (HTML entities in the transport unescaped; no other edits).

## What verified clean

Structure and minimal-audit gates all pass: exactly three phases (plan lines 108, 196, 241); task IDs sequential (P0-T1..T10, P1-T1..T17, P2-T1..T15); `issue.md` is the sole AC source with `- Work Mode: minor-audit` at `issue.md:12` and `## Acceptance Criteria` at `issue.md:118` carrying AC1..AC8 at lines 120-127, each present once and unchecked; no `spec.md` or `user-story.md` exists; Phase 2 final-QC tasks are unconditional; all evidence paths resolve under `FEATURE/evidence/<kind>/`; Phase 0 carries `phase0-instructions-read.md`; all 8 ACs map to a check-off task.

Repository facts 17/17 confirmed against the working tree. All P1-T5/T6/T7/T8 prohibited literals sit on a single line today (`ItemViewer.Breadcrumb.cs:388`, `:383`, `:414`; regression tests `:409`).

Test design satisfiable: the seven-test discrimination matrix in P1-T2 holds pre-fix and post-fix, including fact 16's constraint (`BreadcrumbUiDispatcher.CaptureCurrent()` at `ItemViewer.Breadcrumb.cs:80`). The 3-arg `ConfigureBreadcrumbDropDown` guard is the first statement (`:229`). `ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` still passes post-fix. Both classes named in P1-T9's filter exist.

## Defects — 8 requiring a delta, 3 advisory

### D1 (blocking) — P1-T4 and P1-T9 use a platform value the project file rejects

Both tasks run `msbuild QuickFiler.Test\QuickFiler.Test.csproj ... "/p:Platform=Any CPU"`. `QuickFiler.Test.csproj` declares `<Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>` at line 12 and defines `OutputPath` only under `Debug|AnyCPU` (line 32), `Release|AnyCPU` (41), `Debug|x86` (49), `Release|x86` (53). `Any CPU` is a solution-level alias that matches no property group, so the build fails with `The BaseOutputPath/OutputPath property is not set for project 'QuickFiler.Test.csproj'`. P1-T4's acceptance "the msbuild invocation exits 0" is unsatisfiable as written. The solution-level invocations in P0-T6, P0-T7, P2-T3 and P2-T4 are correct and must not be changed.

### D2 — P1-T10's diff span observes nothing

`git diff --name-status main...HEAD -- QuickFiler QuickFiler.Test` compares `merge-base(main,HEAD)..HEAD`. Verified: `main` = `a007f72e`, `HEAD` = `ef0b5253`, merge-base = `a007f72e`, and the command returns an empty list now and for the whole plan, because the plan never commits. The rationale (anchored diff sees tracked changes, `--intent-to-add` makes the new file visible) is true of a worktree diff and false of a commit-to-commit diff. The acceptance remains falsifiable through the porcelain span alone, but the diff contributes no evidence and the artifact would misstate what was checked.

### D3 — P0-T3 writes an absolute host path into a tracked evidence artifact

P0-T3 records `git rev-parse --show-toplevel` in `FEATURE/evidence/baseline/worktree-context.2026-09-05T10-49.md`, which P2-T13 stages. That path contains the operating-system account name. Version 1.2 correction 7 applied this rule elsewhere; it is unapplied here.

### D4 — P2-T13 stages before P2-T14 creates an evidence artifact

P2-T14 writes `FEATURE/evidence/other/reduced-audit-handoff.2026-09-05T10-49.md` after the only `git add` in the plan, so that artifact is left untracked.

### D5 — BASELINE_FAILURE_SET cannot absorb the flaky test it was written for

The set is defined purely as the P0-T8 observation. If `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` (issue #780) passes in P0-T8 and fails in P2-T6, P2-T6 fails on a pre-existing intermittent condition in `UtilitiesCS.Test` that this change cannot affect.

### D6 — P0-T10 and P2-T8 assert an equality no available node axis produces

Root `lines-covered` / `lines-valid` written by `ConvertTo-KoverageCoberturaXml` come from `Get-CoberturaCoverageSummary` -> `Get-CoberturaPackageLineSummary` -> `Get-CoberturaClassLineSummary`. That helper (`Invoke-MSTestWithCoverage.Helpers.ps1` lines 193-232) merges `./lines/line` and `./methods/method/lines/line` keyed by line number, keeping the maximum `hits`. The plan's projection counts `./classes/class/lines/line` only, so a line present or covered only in the method view breaks the exact equality. Fact 14 is incomplete: neither axis reproduces the root attributes.

### D7 — P1-T11's evidence citation contradicts the condition it cites

P1-T11 cites "exactly one occurrence of `CheckAccess()`"; version 1.2 correction 6 relaxed P1-T5 to "at least one occurrence".

### D8 — the Write Set annotation contradicts P1-T8

The Write Set annotates `ItemViewerBreadcrumbLifecycleRegressionTests.cs` as "(deletions only)", but P1-T8 also rewrites the `SetViewerSyncContext` `<summary>` clause at line 409.

### Advisory A1 — P2-T5 carries no long-run guidance

`vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` over the whole assembly has been observed on this workstation to freeze the testhost on one attempt and complete in about twelve seconds on an identical retry (`TaskMaster.cli.runsettings` requests one worker per logical processor at class level; pump-host tests are load-sensitive).

### Advisory A2 — P2-T13's ignore rationale is inaccurate

`.gitignore` lines 144, 39, 57 apply to untracked paths only; `artifacts/orchestration/orchestrator-state.json` is a tracked, modified file. The acceptance holds because the `git add` span is pathspec-scoped, but the stated reason is wrong.

### Advisory A3 — no planner internal-review record reached this preflight

The `PLANNER-INTERNAL-REVIEW: PASS` ... `UNRESOLVED-GAPS: NONE` record and `SELF-REVIEW: RE-DERIVED THIS PASS` enumeration do not appear in the plan file and were not supplied in the preflight delegation. Orchestrator note: the planner did emit both in its return message to the orchestrator; the orchestrator will relay the record with the next preflight delegation.

## Plan delta (exact replacement text)

**1. `## Conventions Used By This Plan` — replace the `BASELINE_FAILURE_SET` bullet:**

> - `BASELINE_FAILURE_SET` denotes the union of two sets: the exact set of fully-qualified test names recorded as failed by task P0-T8, and the single known-flaky name `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` (issue #780), which fails only intermittently under a parallel coverage run and may therefore pass in P0-T8 and fail in P2-T6. The union is required rather than the P0-T8 observation alone: defining the set by one observation would fail P2-T6 on a pre-existing intermittent condition in `UtilitiesCS.Test` that this `QuickFiler` change cannot affect, which is the same unexecutability the version 1.2 correction removed from P0-T9. Repository-wide zero-failure assertions are not used for the same reason.

**2. `## Conventions Used By This Plan` — add one bullet after the command-step-artifact bullet:**

> - Every command this plan runs is issued either as a `git` invocation or from inside a `pwsh -NoProfile -Command` process (or `pwsh -NoProfile -File` for the P0-T8 script). `dotnet`, `msbuild`, `vstest.console.exe` and `dotnet-coverage` are invoked from within that `pwsh` process rather than directly, so the shell forms this workspace permits and the `Command:` values recorded in the evidence artifacts agree.

**3. `## Write Set` — replace the second Tests bullet:**

> - `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` (two test-method deletions plus one comment-clause correction in the `SetViewerSyncContext` `<summary>`; no other edit)

**4. Fact 14 — replace the final sentence:**

> Neither axis reproduces the document's root attributes, so a per-package count that must reconcile with them is computed by the repository's own `Get-CoberturaPackageLineSummary` rather than by hand: `./classes/class/lines/line` misses any line that appears only in the method view and reads a `hits` of 0 where the method view recorded a hit, and `.//lines/line` double-counts.

**5. `[P0-T3]` — replace in full:**

> - [ ] [P0-T3] Record the working context in `FEATURE/evidence/baseline/worktree-context.2026-09-05T10-49.md`: the output of `git rev-parse --abbrev-ref HEAD`, `git rev-parse HEAD`, and `git merge-base main HEAD`, all run inside the worktree that contains `FEATURE`. Do **not** record the output of `git rev-parse --show-toplevel` verbatim: that value is an absolute host path containing the operating-system account name, and this repository forbids an absolute host path in any tracked artifact — the same rule correction 7 of this plan applied to the execution-location decision. Record instead, under the literal heading `TOPLEVEL CONTAINS FEATURE:`, the single word `YES` or `NO`, obtained by testing whether the directory `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781` exists beneath the toplevel path, and record under the literal heading `TOPLEVEL LEAF:` only the final path segment of that toplevel path. If no local `main` ref exists in that worktree, use `origin/main` instead and record in the artifact, under the literal heading `BASE_REF:`, which of the two refs was used; every later task that names `main` as a diff base uses that same recorded ref. Acceptance, all five required: the artifact exists; `TOPLEVEL CONTAINS FEATURE:` is `YES`; `BASE_REF:` is present; the abbreviated branch name is recorded verbatim; and the artifact contains no absolute filesystem path. If the branch name is not `bug/breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781`, record the observed name and report `BRANCH MISMATCH` to the orchestrator before starting Phase 1; do not create or switch branches from inside plan execution.

**6. `[P0-T8]` — replace the sentence beginning "That failed-name list is `BASELINE_FAILURE_SET`":**

> Record the observed failed-name list in the artifact under the literal heading `BASELINE_FAILURE_SET:` even when it is empty, and on its own line under that same heading record `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue KNOWN-FLAKY #780` whether or not that test was observed to fail, per the `BASELINE_FAILURE_SET` convention above. Acceptance: the artifact exists, every `ASSEMBLY:` line printed by the script is free of the substring `\.claude\`, `ASSEMBLY_COUNT:` is recorded, `BASELINE_FAILURE_SET:` is present, and the `KNOWN-FLAKY #780` line is present under it. Do not copy the printed `ASSEMBLY:` paths into the artifact; they are absolute host paths and are inspected in the run output only. This run takes tens of minutes; do not abort it on a short timeout.

**7. `[P0-T10]` — replace in full:**

> - [ ] [P0-T10] Project the baseline Cobertura to a package-level JaCoCo summary at `FEATURE/evidence/baseline/coverage-baseline.jacoco.2026-09-05T10-49.xml`, using `pwsh -NoProfile -Command` over the block below, run from the repository root. This projection is what P2-T13 stages; the raw Cobertura is never staged. The per-package counters are produced by the repository's own `Get-CoberturaPackageLineSummary` and not by a hand-written node count, because that helper is exactly what `Get-CoberturaCoverageSummary` sums into the root attributes this task's acceptance compares against, so the identity holds by construction. A hand-written count over `./classes/class/lines/line` does not reproduce it: `Get-CoberturaClassLineSummary` (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` lines 193 through 232) merges `./lines/line` and `./methods/method/lines/line` into a map keyed by line number and keeps the maximum `hits` across the two views, so a line present only in the method view, or covered only there, is counted by the root attributes and missed by a class-direct count.
>
>   ```powershell
>       . .\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1
>       [xml]$cov = Get-Content -LiteralPath '.\coverage\baseline-781.cobertura.xml' -Raw -Encoding UTF8
>       $sb = [System.Text.StringBuilder]::new()
>       [void]$sb.AppendLine('<report name="TaskMaster C# (converted from Cobertura)">')
>       $tc = 0; $tm = 0; $tbc = 0; $tbm = 0
>       foreach ($pkg in @($cov.SelectNodes('/coverage/packages/package'))) {
>           $s = Get-CoberturaPackageLineSummary -PackageNode $pkg
>           $lc = [int]$s.LinesCovered; $lm = [int]$s.LinesValid - $lc
>           $bc = [int]$s.BranchesCovered; $bm = [int]$s.BranchesValid - $bc
>           $tc += $lc; $tm += $lm; $tbc += $bc; $tbm += $bm
>           [void]$sb.AppendLine("  <package name=""$($pkg.GetAttribute('name'))""><counter type=""LINE"" missed=""$lm"" covered=""$lc"" /><counter type=""BRANCH"" missed=""$bm"" covered=""$bc"" /></package>")
>       }
>       [void]$sb.AppendLine('</report>')
>       Set-Content -LiteralPath '.\docs\features\active\2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781\evidence\baseline\coverage-baseline.jacoco.2026-09-05T10-49.xml' -Value $sb.ToString() -Encoding UTF8
>       Write-Output "derived lines-covered=$tc lines-valid=$($tc + $tm) branches-covered=$tbc branches-valid=$($tbc + $tbm)"
>   ```
>
>   `Invoke-MSTestWithCoverage.Helpers.ps1` dot-sources `Invoke-MSTestWithCoverage.PackageRate.ps1`, which declares `Get-CoberturaPackageLineSummary`, so the single dot-source above resolves it. Both files call `Set-StrictMode -Version Latest`, which applies to this block after the dot-source; that is why the package name is read with `GetAttribute` rather than by bare property access. Acceptance: the JaCoCo file exists, its root element is `<report`, and the printed `derived lines-covered`, `lines-valid`, `branches-covered` and `branches-valid` values equal the Cobertura root `lines-covered`, `lines-valid`, `branches-covered` and `branches-valid` values recorded in P0-T9 exactly. A mismatch means the block was run against a document `ConvertTo-KoverageCoberturaXml` had not yet rewritten; rerun P0-T9 first rather than adjusting the recorded numbers.

**8. `[P1-T4]` — replace the first sentence pair:**

> - [ ] [P1-T4] [expect-fail] Build and run the new test class against the unfixed guard. Run `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`, then run the resolved `vstest.console.exe` against `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` with `/InIsolation /Logger:trx "/ResultsDirectory:TestResults\fail-before-781" "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests" "/Settings:scripts\vscode\TaskMaster.cli.runsettings"`. The platform value is `AnyCPU` with no space, and the solution-level alias `Any CPU` used by P0-T6, P0-T7, P2-T3 and P2-T4 must not be substituted here: `QuickFiler.Test/QuickFiler.Test.csproj` declares `<Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>` at line 12 and defines `OutputPath` only under the conditions `Debug|AnyCPU` (line 32), `Release|AnyCPU` (line 41), `Debug|x86` (line 49) and `Release|x86` (line 53), so a project-file build invoked with `Any CPU` matches no property group and fails with `The BaseOutputPath/OutputPath property is not set for project 'QuickFiler.Test.csproj'`, which would make this task's own "the msbuild invocation exits 0" condition unsatisfiable.

(The remainder of P1-T4, from "Write `FEATURE/evidence/regression-testing/regression-fail-before...`" onward, is unchanged.)

**9. `[P1-T9]` — replace the first sentence:**

> - [ ] [P1-T9] Rebuild and re-run both affected test classes. Run `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`, using the same `AnyCPU` project-file platform value and for the same reason stated in P1-T4, then run the resolved `vstest.console.exe` against `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` with `/InIsolation /Logger:trx "/ResultsDirectory:TestResults\pass-after-781" "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests|FullyQualifiedName~ItemViewerBreadcrumbLifecycleRegressionTests|FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests" "/Settings:scripts\vscode\TaskMaster.cli.runsettings"`.

(The remainder of P1-T9 is unchanged.)

**10. `[P1-T10]` — replace the first three sentences:**

> - [ ] [P1-T10] Verify the AC7 scope boundary. In one task run `git add --intent-to-add --all -- QuickFiler QuickFiler.Test`, then `git diff --name-status main -- QuickFiler QuickFiler.Test` (substituting the `BASE_REF:` value recorded in P0-T3 for `main` when that task recorded `origin/main`), then `git status --porcelain --untracked-files=all -- QuickFiler QuickFiler.Test`. The two-dot form is required and the three-dot form must not be substituted: `git diff --name-status main...HEAD` compares two commits and never reads the working tree, so because this plan never commits it reports an empty list for these pathspecs however the executor edits those files. The two-dot form compares the base commit against the working tree, which is the comparison the `--intent-to-add` companion completes: without that entry the new test file is untracked and invisible to any diff. The single commit between the base ref and `HEAD` touches only `docs/`, so restricting the diff to the `QuickFiler` and `QuickFiler.Test` pathspecs makes it report exactly this plan's uncommitted work and nothing inherited from the branch. The porcelain span remains required because it observes untracked paths directly and would still report them if the intent-to-add step were skipped.

(The remainder of P1-T10, from "Write `FEATURE/evidence/qa-gates/scope-boundary...`" onward, is unchanged.)

**11. `[P1-T11]` — replace in full:**

> - [ ] [P1-T11] Check off AC1 in `FEATURE/issue.md` by changing the single line beginning `- [ ] AC1: ` to `- [x] AC1: `, preserving the criterion text exactly. Evidence: the P1-T5 acceptance conditions, namely at least one occurrence of `CheckAccess()` and zero occurrences of `ReferenceEquals(SynchronizationContext.Current` in `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, together with P1-T9. The lower-bound wording matches P1-T5 exactly; version 1.2 relaxed that condition from an exact count because the P1-T6 documentation rewrite may legitimately name the member a second time, and citing an exact count here would reintroduce the condition that correction removed. Acceptance: `FEATURE/issue.md` contains exactly one line beginning `- [x] AC1: ` and no other AC line changed in this task.

**12. `[P2-T5]` — append after "Do not copy the `.trx` into the evidence folder.":**

> This run can stall: `/EnableCodeCoverage` over the whole `QuickFiler.Test.dll` has been observed on this workstation to freeze the testhost on one attempt and to complete in about twelve seconds on an identical retry, because `scripts/vscode/TaskMaster.cli.runsettings` requests one worker per logical processor at class level and the pump-host tests are load-sensitive. Do not abort it on a short timeout. If it stalls, sample the testhost process CPU time twice about twenty seconds apart to distinguish a hang from a slow run, terminate only this task's own `vstest.console` and `testhost` chain and no shared MSBuild or compiler worker, then rerun the identical command. A rerun with no intervening file change is not a toolchain-loop restart; record in the artifact that no file in the Write Set changed between the two attempts.

**13. `[P2-T8]` — replace the acceptance sentence:**

> Acceptance, all three required: the JaCoCo file exists and its derived `lines-covered`, `lines-valid`, `branches-covered` and `branches-valid` totals equal the final Cobertura root values exactly; `artifacts/csharp/coverage.xml` exists and its root element is `<coverage`; and its root `line-rate` attribute equals the value recorded in P2-T7.

**14. `[P2-T13]` — replace the third acceptance clause:**

> the porcelain output lists no staged path under `coverage/`, `TestResults/`, or `artifacts/`, which holds because the `git add` span above is scoped to three pathspecs that exclude all three trees, and not because `.gitignore` covers them: lines 144, 39 and 57 ignore `coverage/*`, `[Tt]est[Rr]esult*/` and `artifacts/` for untracked paths only, and `artifacts/orchestration/orchestrator-state.json` is a tracked file that no ignore rule affects;

**15. `[P2-T14]` — replace the acceptance sentence and append a staging span:**

> Then, because P2-T13 staged the Write Set before this artifact existed, re-stage the feature folder: run `git add -- docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781`, then `git status --porcelain --untracked-files=all -- docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781`. **Do not commit.** Acceptance, all four required: the artifact exists; every path it lists resolves on disk; the `CHANGED-CODE COVERAGE:` determination line is reproduced in it verbatim; and the porcelain output for the feature folder lists no path whose status code is `??`, so no evidence artifact is left untracked. The plan-checkbox writes for P2-T13, P2-T14 and P2-T15 are made after this staging span and are expected to leave `FEATURE/plan.2026-09-05T10-49.md` with the porcelain code `MM`, which is a staged and then further modified tracked file rather than an unstaged path; the orchestrator's commit step covers that residual.

## Executor delta self-check (verbatim)

Each replacement was checked against the rules it enforces. No delta text contains an absolute host path, hyperbole, humor, or metaphor. Every asserted literal introduced (`AnyCPU`, `CheckAccess()`, `TOPLEVEL CONTAINS FEATURE:`, `TOPLEVEL LEAF:`, `KNOWN-FLAKY #780`, `<report`) is a short single-line token quoted verbatim in the delta prose outside its command span. The `git diff` in delta 10 carries a non-flag ref operand and is accompanied in the same task by both a `git add` span and a `git status --porcelain` span, satisfying G8 and G8b. The `git add` spans in deltas 10 and 15 are excluded from the G7 write-mode register by that rule's own stated exclusion. The corrected P0-T10 acceptance compares four printed values against four recorded values, so it can fail. No delta introduces a coverage argument, a placeholder inside an asserted token, or a skip branch in a Phase 2 command task.
