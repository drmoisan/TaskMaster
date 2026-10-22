# Preflight Round 1 — Issue #440

Timestamp: 2026-08-29T01-40
Reviewer: atomic-executor under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`
Plan under review: `plan.2026-08-29T00-22.md` in this feature folder
Signal: `PREFLIGHT: REVISIONS REQUIRED`
Convergence: `CONVERGENCE: NO FURTHER ROUNDS EXPECTED`

The reviewer executed no plan task and modified no file. All 55 tasks remain unchecked.

## Verification the reviewer performed and confirmed correct

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` is 248 lines; `LeftArrow()` at 220-246; the four guard conjuncts at 232-237 in the stated order; `#440` comment at 227-230; `#nullable enable` at line 1.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` is 235 lines; `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` is 495 lines. The orchestrator's correction of the spec's 236/496 figures is confirmed correct.
- Literal occurrence counts, each exactly 1, so each zero-or-one-hit gate can genuinely fail: `activeIndex.Value == row.Chain.Count - 1`; `row.ActivateSegment(activeIndex.Value - 1)`; `leaf-anchored node` at line 228; `a second Left is unhandled` at line 73; `the one available #440 parent-select transition` at line 370.
- `ActivateSegment` at `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` lines 195-211 refuses a negative index and an index at or beyond `Chain.Count - 1`, and clears `LeafExpanded`. `ActiveSegmentIndex` defaults to `Chain.Count - 1`.
- Regression completeness: the plan's claim that exactly two existing tests go red is correct. The reviewer independently traced every `LeftArrow()` call site and every left-arrow bridge payload.
- Both `[expect-fail]` tests do fail before the fix, at press 2 in each case.
- Bootstrap: no `.dotnet-sdk` and no `packages` directory in this worktree. The host SDK is 10.0.400 only; `global.json` pins 8.0.205 with `rollForward: latestFeature`, which does not roll forward to 10.x, so the plan's claim that every `dotnet` call fails without the repo-local install is correct.
- `.csharpierignore` excludes `*.cobertura.xml`, `*.trx`, `*.coverage` and `**/evidence/**`, so the artifacts this plan creates do not break the file-count-equality gate.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` has exactly 453 `Compile Include` items. No `PackageReference` exists in any project; there are 18 `packages.config` files.

## Defects requiring revision

### 1. P5-T18 unscoped clean-tree gate is unsatisfiable (blocking)

`git status --porcelain` with no pathspec is asserted clean. The tree is already dirty at base with tracked and untracked files this change must not commit:

```
 M .claude/agent-memory/prd-feature/MEMORY.md
 M .claude/agent-memory/task-researcher/MEMORY.md
?? .claude/agent-memory/prd-feature/feedback_backticked_paths_are_the_change_footprint.md
?? .claude/agent-memory/task-researcher/project_issue_440_already_landed_via_498.md
```

`.claude/agent-memory/` is tracked, at 570 files. The task simultaneously forbids `git add -A` and demands a globally clean tree; both cannot hold. Independently, after P5-T18 commits, P5-T19 writes a new artifact into the feature folder and the executor must flip both task checkboxes in the plan file, which is also inside the feature folder, so the tree is dirty again immediately. That is a check-off fixpoint failure.

Replacement text:

> - [ ] [P5-T18] Write the completion report: the phases executed, the final toolchain result, the coverage delta headline, the AC status, and the two known divergences the spec records as non-goals (the Right-descent commit asymmetry between the surfaces, and the single-level Right descent limit present on both), stated as items for the maintainer rather than as defects introduced here.
>   Acceptance: `<FEATURE>/evidence/other/completion-report.<timestamp>.md` exists and names both known divergences together with the spec section that records each as a non-goal.
> - [ ] [P5-T19] Final commit. Before staging, mark every remaining unchecked task in this plan file, including this one, as `[x]`, so no plan-file write remains after the commit. Stage with explicit pathspecs only: `git add -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440`. Do not use an unscoped `git add -A`: `.claude/agent-memory/` is tracked and carries unrelated modifications from other agents that must not enter this branch. Then commit with a message naming issue #440 and summarizing the one-clause guard relaxation and the two test corrections.
>   Acceptance: `git status --porcelain -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440` produces empty output after the commit, `git diff --name-only b56400ab663a85b6039139d4548f408821e957ce -- UtilitiesCS UtilitiesCS.Test` still lists exactly the three source paths, and `git status --porcelain -- .claude` is recorded verbatim in `<FEATURE>/evidence/other/completion-report.<timestamp>.md` as pre-existing out-of-scope state that this commit deliberately did not touch.

Renumber the AC coverage map row for Phase 5 accordingly.

### 2. P4-T3 and P4-T4 assert over a build log that no command produces (blocking)

Both acceptance conditions require searching a captured build log for the literal `Skipping target "CoreCompile"`, but neither command contains `/fl`, `/flp`, `/v:normal`, or any redirection. No log exists to search, so the AC-14 non-vacuity evidence cannot be produced. The repository's established form is `/v:normal /fl "/flp:LogFile=<path>;Verbosity=normal"`.

Under `/t:Rebuild` the skip literal is absent by construction, so a zero-count assertion alone discriminates only against an executor who substituted `/t:Build`. Pair it with positive evidence.

Replacement text:

> - [ ] [P4-T3] Analyzer step: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl "/flp:LogFile=coverage\logs\p4-t3-analyzer.msbuild.txt;Verbosity=normal"`. The log path sits under the gitignored `coverage/` tree per Global rule 8, because an msbuild log embeds absolute host paths and must never be written under `<FEATURE>/evidence/`.
>   Acceptance: `<FEATURE>/evidence/qa-gates/p4-t3-analyzer-build.<timestamp>.md` records Timestamp, Command, `EXIT_CODE: 0` and an Output Summary carrying the warning count and the error count. The error count must be 0, and the warning count must be at or below the P0-T11 baseline warning count. The artifact records two counts read from `coverage\logs\p4-t3-analyzer.msbuild.txt`: the occurrences of the literal `Skipping target "CoreCompile"`, which must be 0, and the occurrences of the literal `(Rebuild target)`, which must be at least 1. The second count is the positive half of the non-vacuity proof and is what fails if the log is empty or was never written (AC-14).

Apply the identical change to P4-T4, substituting `p4-t4-nullable.msbuild.txt` and keeping `/p:TreatWarningsAsErrors=true` with no `/p:Nullable=enable`.

### 3. P4-T6 gate (4) collects no data (blocking)

Gate (4) reads `hits` for every changed line number of the production file that appears as a `line` element in the post-change Cobertura document. The change consists of one deleted conjunct line, which by definition has no post-change line number, and a rewritten comment block, and comment lines are never emitted as `line` elements. The derived set is empty, so the gate returns the same result whatever the executor does. This is the plan's sole discharge of AC-15's every-changed-line-covered requirement.

Replacement text:

> (4) changed-region coverage: derive the post-change line span of `LeftArrow()` in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` by locating the method declaration and its closing brace, enumerate every `line` element under that file's `class` element in the post-change Cobertura document whose `number` falls inside that span, and record each line number with its `hits` and, where present, its `condition-coverage`. The gate requires that the enumerated set contains at least 4 line elements and that every enumerated element has `hits` greater than 0. The at-least-4 floor is what makes the gate fail rather than pass vacuously when the lookup returns nothing, and it is derivable from the method's four instrumented statements at BASE: the `var row =` assignment, the `return false` guard, the `int? activeIndex =` assignment, and the transition `if`. The artifact additionally records the `condition-coverage` of the transition `if` line before and after the change, which is the branch-level evidence AC-15's second sentence requires.

### 4. Three acceptance conditions are internally contradictory (blocking)

Each states an unconditional green requirement and then, in the same condition, provides for a non-green outcome. Whichever the executor observes, one half is violated.

- P0-T13 (a) requires quoting a summary that prints no `Failed:` line at all, and (b) requires `BaselineTotalTests` and `BaselinePassedTests` as equal numeric values, yet (b) also provides for `BaselineFailureSet` naming the fully qualified name of every failing test.
- P4-T5 (a) requires the `Test Run Successful.` shape with the two counts equal, yet (c) requires `FinalFailureSet` only to be a subset of `BaselineFailureSet`, which explicitly contemplates a non-empty set.
- P4-T2 requires `EXIT_CODE: 0`, then adds an alternative for pre-existing drift. `csharpier check .` exits non-zero when it reports any file.

Corrections: in P0-T13, change (a) to "quotes the run summary verbatim; on a fully green run it has the shape `Test Run Successful.` followed by a `Total tests:` line and a `Passed:` line and no `Failed:` line", and change (b) to "records `BaselineTotalTests`, `BaselinePassedTests` and `BaselineFailedTests` as numeric values, and records `BaselineFailureSet` as `none` when `BaselineFailedTests` is 0 and otherwise as the fully qualified name of every failing test". In P4-T5, change (a) to "quotes the run summary verbatim" and drop the equal-counts clause, since (c) is the gate on failures. In P4-T2, change the acceptance to "`EXIT_CODE: 0`, or the set of files the tool reports as needing reformatting is exactly the pre-existing-drift set enumerated by P0-T10 with no additional file".

### 5. P3-T5 porcelain companion is made vacuous by the task's own ordering (blocking)

The task runs `git add -A -- UtilitiesCS UtilitiesCS.Test` first and then asserts that the porcelain output contains no untracked entry, which it calls the confirmation that no new test file was added (AC-10). `git add -A` converts every untracked entry into a staged entry, so after that command no untracked entry can exist regardless of what the executor did.

Correction: assert on the status codes rather than on the absence of untracked markers.

> The porcelain output must list exactly three entries, and every entry's two-character status field must be `M `, ` M`, or `MM`. Any entry whose status field begins with `A`, `?`, or `R` means a file was created, added, or renamed under a pathspec this change does not create files in, which fails the gate. This, together with the diff file list, is what confirms no new test file was added and that the test project file is absent from the change (AC-10).

### 6. P3-T5 requires a hunk-level observation but runs no content diff (blocking)

The acceptance requires the artifact to record that the production-file diff shows exactly one deleted conjunct line plus the comment rewrite, and that the retained `_selectedSubfolderIndex` conjunct appears as an unchanged context line. This is the sole discharge of the diff half of AC-2 and AC-3. Neither `--name-only` nor `--porcelain` emits hunks.

Correction: add a third command span, `git diff b56400ab663a85b6039139d4548f408821e957ce -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`, and require the artifact to quote that diff in full, stating the count of removed lines whose content is not a comment, which must be exactly 1, and quoting the `_selectedSubfolderIndex < 0` line with its leading space to show it is context.

### 7. P3-T2 omits the two corrected tests from its named-outcome list (blocking)

P3-T2 names six results whose outcome must be `Passed`. Neither `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` nor `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges` is among them, although both are the tests this plan rewrites and both are what AC-6 and AC-7 turn on. P2-T3 and P2-T4 verify only that a comment literal disappeared, which does not establish that the rewritten test asserts the corrected sequence or that it passes. P5-T4 then cites a corrected result the P3-T2 artifact is not required to contain.

Correction: extend the named-result list from six to eight by adding both test names, and add to the acceptance: "the seventh and eighth are the two tests P2-T3 and P2-T4 corrected; their `Passed` outcomes are the only evidence that the corrections encode the walk contract rather than merely deleting the old comment."

Related, lower severity: AC-9 requires the full QuickFiler.Test Efc breadcrumb router suite to pass, but P3-T3's filter reaches only `BreadcrumbBridgeRouterTests`. `BreadcrumbBridgeRouterQueueTests`, `BreadcrumbBridgeRouterIssue439Tests` and `BreadcrumbBridgeRouterIssue614Tests` match no alternate. Either add those three names to the P3-T3 filter or amend P5-T9 to cite P4-T5 for the full-suite half.

### 8. P3-T4 production-file line ceiling is unbudgeted against P2-T2 (blocking)

P3-T4 requires the production file to stay at or under its 248-line baseline on the ground that it gains no net line. P2-T1 removes 1 line; P2-T2 rewrites the four-line comment and mandates that it state three separate things. The available budget is therefore 5 comment lines. That budget is nowhere stated, no acceptance criterion requires this ceiling, and a six-line comment makes an otherwise-correct change fail the gate.

Correction: add to P2-T2's task text: "The rewritten comment must occupy at most 5 lines, so that with the one line P2-T1 deletes the file remains at or under its 248-line baseline (P3-T4)." Add to P3-T4's acceptance the derivation: "the production file's ceiling is 248 because P2-T1 removes one line and P2-T2's comment budget is 5 lines against the 4 it replaces."

### 9. P4-T7 contradicts Global rule 7 on exit codes (moderate)

P4-T7 requires the five artifacts to record exit codes all 0. Global rule 7 contemplates the `Assert-CoberturaLineCoverageThreshold` branch, which throws and therefore makes P4-T5 exit non-zero, while directing that the throw is not treated as a test failure. Both cannot hold.

Correction: change P4-T7's acceptance to "records exit code 0 for P4-T1 through P4-T4; for P4-T5, records exit code 0, or a non-zero exit code together with the `COVERAGE-THRESHOLD-THROW:` record required by Global rule 7 and the confirmation that the vstest summary itself reported `Test Run Successful.`. Any other non-zero exit code fails the gate."

### 10. The threshold-throw branch invalidates the prescribed Cobertura lookups (moderate)

In `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `Assert-CoberturaLineCoverageThreshold` runs after the conversion computes the processed content but before the write-back. On the throwing branch the on-disk document is the raw output: absolute `class/@filename` values, unfiltered packages, and unmerged duplicate classes. The instruction to select the class element whose `filename` equals the relative production path then matches nothing, and the root coverage totals are not comparable with a post-processed baseline. Both P0-T13 and P4-T5 demand numeric values that cannot be read in that state, and the plan forbids placeholders.

Correction: add to Global rule 7: "On the throwing branch, the on-disk document is the raw pre-processing output: absolute `class/@filename` values, all instrumented packages, and unmerged duplicate classes. Before reading any numeric field from it, the task dot-sources the helpers and applies `ConvertTo-KoverageCoberturaXml` in memory to the raw content with the repository root supplied, then reads every figure from that in-memory document so the values are on the same footing as the non-throwing branch. The artifact records that it did so."

### 11. P4-T6 gate (1) is presented as a gate but cannot fail (moderate)

The task says the four gates can each fail, but gate (1) requires only that the repository-wide figures are recorded numerically and that any decrease is stated with its magnitude. Recording a decrease satisfies it. Decision D5's reasoning about measurement noise applies to the production file, not to the repository-wide figure, where deleting one covered line out of roughly 64,000 is not a measurement hazard.

Correction: either restate gate (1) as a threshold, requiring `FinalLineCoveragePercent` to be no more than 0.01 percentage points below `BaselineLineCoveragePercent` and `FinalBranchCoveragePercent` no more than 0.05 points below its baseline, or move it out of the numbered gate list and label it a recorded observation.

### 12. The regression enumeration in ground truth #6 is incomplete (minor)

The record says the two-red-tests conclusion was derived by an exhaustive search for `LeftArrow()` call sites and left-arrow bridge payloads. Two left bridge sites in `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` are omitted: line 454 and line 483. Both go through the file's `ArrowAsync` helper, which builds the payload by string concatenation, so a literal payload search does not find them. Both were traced by name and each sends exactly one Left, so the conclusion is right, but the search method as described has a blind spot.

Correction: add to ground truth #6 a sentence naming both lines and stating that the helper builds the payload by concatenation and is therefore not reachable by a literal payload search, and that both were traced by name.

### 13. The sibling literal is misattributed in ground truth #4 and P2-T2 (minor)

P2-T2 states that the file's other leaf-anchored wording sits in the `TryRightTreeTransition` doc comment. It is not in the doc comment; it is an inline trailing comment inside the method body at line 198. The zero-hit gate is unaffected because the two literals do not overlap. Correct the citation to "an inline trailing comment at line 198 inside `TryRightTreeTransition`".

### 14. P0-T7 states an incorrect rationale (minor)

The claim that a `msbuild /t:Restore` must not be used is wrong. `scripts/vscode/Invoke-Restore.ps1` runs exactly that target with `/p:RestorePackagesConfig=true`, which is what makes it handle packages.config projects. The prescribed `nuget restore` also works, but repository policy directs preferring repo-defined commands.

Correction: replace P0-T7's command with `pwsh -NoProfile -File scripts\vscode\Invoke-Restore.ps1`, and replace the rationale with: "Every project in this solution is packages.config style, so a bare `msbuild /t:Restore` without `/p:RestorePackagesConfig=true` reports nothing to do. The repository's `Invoke-Restore.ps1` supplies that property and is the repo-standard restore path."

### 15. Tool-resolution forms diverge from the proven ones (minor)

Global rule 3 resolves the test runner with `-latest -property installationPath` joined to a sub-path. The wrapper uses `-latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`; omitting `-products *` restricts the lookup to the default product set and returns nothing on a Build-Tools-only install. The build tool is described only as an expected path shape; `Invoke-Restore.ps1` uses `-latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'`. Both plan forms happen to resolve on this machine, so this is not blocking. Adopt the two proven forms so the plan does not depend on the local install profile.

### 16. No branch for a red baseline build or a test-host crash (minor, robustness)

P0-T11 and P0-T12 state an expected exit code of 0 with no recorded outcome if the fresh worktree's baseline build is not green, and P4-T3's warning comparison presupposes a usable baseline. Separately, neither full-suite task provides for the test-host crash mode in which the runner prints an unknown total and no verdict, which makes the baseline and final totals unreadable.

Correction: add to P0-T11 and P0-T12, "if the exit code is non-zero, record the full error list as a pre-existing baseline condition and halt with `TOOLCHAIN-BLOCKER:` rather than proceeding". Add to P0-T13 and P4-T5 a single-re-execution allowance keyed to an unreadable or unknown total, with both runs recorded.

### 17. Tonality (minor)

Decision D3 contains "which is what buys the room for the extra Arrange press". Replace with "which is what creates the headroom for the extra Arrange press".

## Acceptance-criteria assessment

All fifteen criteria in `spec.md` are discharged by at least one Phase 5 task, and no plan task asserts anything `spec.md` does not require. Two coverage links are weak and are addressed by defects 3 and 7. The AC-15 evidence-location override is handled correctly: `spec.md` names a non-canonical sub-path and the plan records `EVIDENCE_LOCATION_OVERRIDE_REJECTED` rather than editing the acceptance-criteria source file.
