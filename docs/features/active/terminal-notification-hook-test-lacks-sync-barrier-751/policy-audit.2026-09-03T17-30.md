# Policy Audit — terminal-notification-hook-test-lacks-sync-barrier (Issue #751)

- Artifact timestamp: 2026-09-03T17-30
- Reviewer: feature-review agent
- Worktree audited: `C:/Users/DanMoisan/repos/TaskMaster-wt/prep-751`
- Branch: `bug/terminal-notification-hook-test-lacks-sync-barrier-751`
- Head commit: `d2fbc327` (2026-09-03T14:49:35-04:00)
- Base ref supplied by caller: `f8414ee9`
- Base ref independently re-derived: `git merge-base origin/main HEAD` and `git merge-base main HEAD` both return `f8414ee979e1884c4a93703523509d4f45e89151`. The supplied ref is correct and is the true merge base; two-dot and three-dot diffs are therefore equivalent for this branch.
- Work mode: `full-bug` (marker read at `issue.md:12`). AC source: `spec.md` only.
- Audit scope: the full branch diff `f8414ee9...HEAD` — 44 changed paths (2 C# test files, 42 Markdown files in the feature folder). No caller instruction narrowed this scope.

## Executive Summary

The branch delivers a test-only synchronization fix: three changed source lines across two files in `TaskMaster.Test`, and zero production files. All ten acceptance criteria in `spec.md` are satisfied. The delivered barrier establishes a real happens-before edge rather than narrowing a window; the reasoning and its verification are in `code-review.2026-09-03T17-30.md`.

- **Blocking findings: 0.**
- **Non-blocking findings: 7** (NB-1 through NB-7, enumerated in § 8).
- One coverage row is recorded **FAIL** on the mandatory canonical-artifact rule (§ 5). Its disposition is procedural and non-blocking: the reviewer recovered the numeric baseline/post-change pair independently from the same in-run collector output and found no regression and clearance of the 85% line floor on first-party production code.

Overall verdict: **PASS**. No remediation-inputs artifact is produced, because no finding is blocking.

## Rejected Scope Narrowing

None. The delegating prompt directed a full-feature review against the resolved base branch, did not limit the review to a plan, task, or phase, did not exclude any changed file, and did not assert that any language category was inapplicable. No narrowing was detected and none was rejected.

Two caller instructions were recorded as constraints rather than narrowings, and both were honored:

1. Write scope limited to `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/` inside this worktree, with mirroring into the shared session worktree prohibited. Honored; the operational consequence is recorded in § 8 as an operational note, not as a finding against the delivery.
2. AC positions in `spec.md` supplied by the caller. These were re-derived independently rather than accepted: the ten `- [x]` items were located at `spec.md` lines 326, 331, 333, 339, 341, 346, 352, 356, 358, 362 (the caller's list identified the same ten criteria; minor line offsets between the caller's list and the read positions do not change the criteria set).

## Evidence Location Compliance

The branch diff was scanned for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`.

| Check | Command | Result |
|---|---|---|
| Any `artifacts/**` path in the branch diff | `git diff --name-only f8414ee9...HEAD -- 'artifacts/**'` | 0 rows — PASS |
| Evidence kinds used | directory listing of the feature folder | `baseline`, `issue-updates`, `other`, `qa-gates`, `regression-testing` — all five are canonical kinds per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md:15-19` — PASS |

`validate_evidence_locations.py` does not exist anywhere in this repository (recursive search returned no file), so the scan above was performed directly. No non-canonical evidence path exists on this branch.

## 1. General Unit Test Policy Compliance

Source: `.claude/rules/general-unit-test.md`.

| Requirement | Evidence | Verdict |
|---|---|---|
| Independence | The changed test constructs its own `ControlledUiDispatcher` and `ControlledAppOlObjects`; no static or shared state is touched. Test counts are identical before and after (408 in `TaskMaster.Test`, 6984 across the solution), so no ordering dependency was introduced. | PASS |
| Isolation | The change adds one assertion inside one existing test method and hardens one fixture counter. No new unit under test. | PASS |
| Fast execution | Target-test duration measured from the TRX files: 0.00164–0.00191 s pre-change (P0-T15 runs 1–3), 0.00177–0.00235 s post-change (P3-T2 runs 1–5). The added await costs sub-millisecond. | PASS |
| Determinism | The added `await run.Terminal` is a completion-signal await, not a wait. No `Thread.Sleep`, `Task.Delay`, wall-clock read, `SpinWait`, or polling loop appears anywhere in the branch diff (verified by direct search of both changed files and of the diff text). | PASS |
| Determinism Infrastructure — banned APIs in test code | Search over `TaskMaster.Test/AppGlobals` for `Thread\.Sleep|Task\.Delay|SpinWait` returned no match introduced by this branch. The only pre-existing `[Timeout]` uses are in `NonBlockingDelayTests.cs` and are unrelated. | PASS |
| Readability / documented intent | The inserted assertion reuses the identical shape used at seven sibling call sites (`await GetExceptionAsync(await run.Terminal)`), so intent is legible without a comment. | PASS |
| No temporary files in tests | No file I/O added. | PASS |
| Test file location (`tests/` mirror rule) | Both files are pre-existing at `TaskMaster.Test/AppGlobals/`, which is this repository's established C# test-project layout. No test file was created or moved into a production source tree. | PASS |
| Coverage exclusion policy — no production path excluded | No coverage configuration file is changed on this branch (`TaskMaster.runsettings` is untouched). | PASS |

## 2. General Code Change Policy Compliance

Source: `.claude/rules/general-code-change.md` and `CLAUDE.md`.

| Requirement | Evidence | Verdict |
|---|---|---|
| Simplicity first | The fix reuses the fixture's existing terminal signal. No new type, field, `TaskCompletionSource`, or helper method is introduced (verified against the three-line diff). | PASS |
| Minimal, targeted fix (Bugfix Workflow step 2) | 3 lines changed, 2 files, 0 production files. No opportunistic refactor. | PASS |
| Failing regression test first (Bugfix Workflow step 1) | Discharged by the spec-authorized substitute route rather than literally; see § 8 NB-5 and the feature audit for AC6. | PASS with disclosed deviation |
| File size limit — 500 lines | `AppOlObjectsFolderTreeServiceTests.cs` = 493 lines (was 492); `AppOlObjectsFolderTreeServiceLifecycleTests.cs` = 490 lines (unchanged). Both counted directly in this review with `(Get-Content).Count`. | PASS |
| Error handling / fail fast | Not engaged; no error-handling code changed. | PASS |
| Naming | No identifier added. | PASS |
| Public API compatibility | No production type or member changed; `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` blob hash is `fe980d3fc6726cbc49e391798489e0af82a683a1` at both `f8414ee9` and `HEAD`. | PASS |
| Dependencies | No package reference changed. | PASS |
| Mandatory toolchain loop, in order | See § 7. | PASS |

## 3. Language-Specific Code Change Policy Compliance (C#)

Source: `CLAUDE.md` § "C# Code Change Policy" and `.claude/rules/csharp.md`.

| Requirement | Evidence | Verdict |
|---|---|---|
| Formatting via manifest-pinned CSharpier | Re-executed by this reviewer, read-only: `dotnet tool run csharpier check "TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs" "TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs"` → `Checked 2 files in 537ms.`, exit 0. The executor additionally recorded a repository-wide `csharpier check .` at exit 0 (`Checked 1574 files in 4837ms.`). | PASS |
| `dotnet format` not used | No evidence artifact or command record references it. | PASS |
| Analyzer gate command shape | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, exit 0, 0 warnings, 0 errors. Character-for-character the `CLAUDE.md` prescription, and `/t:Rebuild` rather than `/t:Build`. | PASS |
| Nullable gate command shape | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, exit 0, 0 warnings, 0 errors. `/p:Nullable=enable` correctly not added; `/t:Rebuild` used. | PASS |
| Both msbuild gates actually compiled | Corroborated independently: `TaskMaster.Test/obj/Debug/TaskMaster.Test.dll` last-write 14:40:53, `TaskMaster/bin/Debug/TaskMaster.dll` 14:40:50, `UtilitiesCS/bin/Debug/UtilitiesCS.dll` 14:40:46 — a single sweep terminating at 14:40:53, consistent with two consecutive ~15 s rebuilds (15.10 s and 14.92 s recorded) finishing immediately before the 14:41:34 test run. | PASS |
| Strong contracts / type safety | No API surface touched. `Volatile.Read(ref sut.InvokedTerminalHookCount)` compiles only against a directly accessible instance field; see NB-7. | PASS |

## 4. Language-Specific Unit Test Policy Compliance (C#)

Source: `CLAUDE.md` § "C# Unit Test Policy".

| Requirement | Evidence | Verdict |
|---|---|---|
| MSTest framework | Unchanged; `[TestMethod]` on the modified test. | PASS |
| Moq for mocking | Unchanged; `Mock<TreeService>(MockBehavior.Strict)` in `CreateSut`. | PASS |
| FluentAssertions for assertions | The inserted assertion uses `.Should().BeSameAs(fault)`; the replaced assertion keeps `.Should().Be(1)`. | PASS |
| Toolchain command selection | See § 7. | PASS |

## 5. Test Coverage Detail

Languages with changed files in the branch diff, derived directly from `git diff --name-only f8414ee9...HEAD`: **C# only** (2 `.cs` files). No `.ts`, `.tsx`, `.py`, `.ps1`, or `.psm1` file is changed on this branch.

### Coverage verdict rows

| Language | Changed files on branch | Canonical artifact | Repo-wide figure | Verdict |
|---|---|---|---|---|
| C# | 2 | `artifacts/csharp/coverage.xml` — absent in this worktree and in the session worktree | first-party production line coverage 85.059% (recovered independently, below) | **FAIL** |
| TypeScript | 0 | — | — | PASS |
| Python | 0 | — | — | PASS |
| PowerShell | 0 | — | — | PASS |

- C# coverage verdict: **FAIL** — the canonical coverage artifact `artifacts/csharp/coverage.xml` is absent, and coverage verification is mandatory for every language with changed files. Disposition: procedural and non-blocking; the substantive numeric obligation is discharged by the independent recovery below.
- TypeScript coverage verdict: **PASS** — zero TypeScript files changed on this branch, so no TypeScript coverage obligation arises.
- Python coverage verdict: **PASS** — zero Python files changed on this branch, so no Python coverage obligation arises.
- PowerShell (Pester) coverage verdict: **PASS** — zero `.ps1` or `.psm1` files changed on this branch, so no PowerShell coverage obligation arises.

Cross-worktree observation, recorded for completeness and not as a finding against this branch: a stale `artifacts/pester/powershell-coverage.xml` exists in the shared session worktree and its PowerShell line coverage computes to 3.63%, which is below the 85% floor — **FAIL** for that stale artifact. It measures a run of an unrelated item and contains no file changed by this branch, which changes no PowerShell file at all.

### Independent recovery of the numeric baseline/post-change pair

The plan's own capture tasks (P0-T17 baseline, P4-T12 final) both recorded `Rung: 3` / `COVERAGE_CAPTURE_BLOCKED` and produced no figures. This reviewer recovered the pair from the same in-run collector output, using the converter route that memoized precedent establishes for this repository:

```
dotnet-coverage merge <run>/<guid>/<attachment>.coverage -f cobertura -o <scratch>/cob-<base|head>.xml
```

Inputs: the published attachment of run `P0-T14` (full suite, 14:24, pre-fix) and of run `P4-T5` (full suite, 14:41, post-fix). Within each run the two `.coverage` files are byte-identical in length (21356385 for P0-T14; 21356197 for P4-T5), confirming the executor's statement that they are one collection published twice, not two collections.

| Figure | Baseline (P0-T14, pre-fix) | Post-change (P4-T5, post-fix) | Delta |
|---|---|---|---|
| All-module lines covered / valid | 173005 / 227966 = 75.890% | 173009 / 227967 = 75.891% | +0.001 pp |
| First-party production lines covered / valid | 56000 / 65820 = **85.081%** | 55986 / 65820 = **85.059%** | −0.022 pp |
| `TaskMaster.Test` package | 7457 / 7859 | 7458 / 7860 | +1 valid, +1 covered |
| Changed production lines | 0 | 0 | — |

First-party production packages summed: `QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`, `VBFunctions`. The all-module figure includes vendor assemblies (`Deedle`, `FSharp.Core`, `log4net`, `FluentAssertions`, `System.Linq.Async`, and others) that policy does not place in the first-party denominator; it is reported for completeness only.

Interpretation:

1. The 85% line floor is met on both sides of the change (85.081% → 85.059%).
2. Per-package line rates are identical between the two runs for every first-party production package except `UtilitiesCS` (0.896939 → 0.896619, −14 covered lines of 43770). `UtilitiesCS` has zero changed lines on this branch, and this magnitude of drift is within the known cross-session nondeterminism band for this suite. It is not attributable to the change.
3. The changed test lines are themselves covered: the `TaskMaster.Test` package gains exactly one valid line and one covered line, matching the net +1 line of the diff.
4. Branch coverage cannot be computed from this collector output: the Visual Studio `.coverage` format carries block coverage, and the Cobertura projection emits no `branches-valid`/`branches-covered` counters (`branch-rate` is written as the filler value 1). This is a capability limit of the collector, not an omission by the executor. No branch percentage is asserted in either direction.

### Coverage Evidence Contract assessment

`.claude/skills/atomic-plan-contract/SKILL.md:107-117` requires: (a) explicit baseline and final-QC capture tasks in the plan — **met** (P0-T17, P4-T12 exist and were executed); (b) artifacts recording numeric values, not placeholders — **not met**; (c) if values are unavailable the outcome must be reported as remediation-required and not PASS — **met** (the executor reported remediation-required).

Cause of (b): the plan's locate step requires the `*.coverage` search under each run's `/ResultsDirectory` to return exactly one file. Under `/InIsolation`, vstest emits both the published attachment and an in-run `In\<machine>\` copy, so the precondition is unsatisfiable for the prescribed command shape. No executor could have passed it. The executor did not fabricate a figure and did not convert an arbitrary member of the set, both of which the plan forbade.

Adjudication: **non-blocking**, for three stated reasons, and for no other reason.

1. The obligation whose numbers would matter — no regression on changed production lines — has an empty subject. This reviewer re-derived that independently: `git diff --name-only f8414ee9 HEAD` restricted to all nine production project directories returns 0 rows, and the named production file's blob hash is identical at base and head.
2. The numeric pair is not merely unobtainable in principle; it was recovered above from the same attachments, and it shows no regression and clearance of the line floor. The record is therefore closed on the substance, not deferred.
3. The contract's own remedy for unavailable values is a reporting rule, which the executor followed. Blocking the change would enforce the reporting rule twice while adding no information.

Minimal remediation (owed against the plan template, not against this change): amend the locate step to select the published attachment deterministically — for example, exclude any path segment `In\` before applying the exactly-one requirement, or accept a candidate set whose members are byte-identical and convert the first — then re-run the two capture tasks. This does not gate the present pull request.

## 6. Test Execution Metrics

All figures below were read by this reviewer directly from the TRX files under `coverage/trx/`, not transcribed from the executor's artifacts.

| Run | Phase | Total | Passed | Failed | Not executed | Target test |
|---|---|---|---|---|---|---|
| P0-T14 | pre-fix, full suite | 6984 | 6984 | 0 | 0 | Passed (0.0019 s) |
| P0-T15-run1..3 | pre-fix, `TaskMaster.Test` | 408 | 408 | 0 | 0 | Passed ×3 |
| P3-T1 | post-fix, targeted | 20 | 20 | 0 | 0 | Passed |
| P3-T2-run1..5 | post-fix, `TaskMaster.Test` | 408 | 408 | 0 | 0 | Passed ×5 |
| P4-T5 | post-fix, full suite | 6984 | 6984 | 0 | 0 | Passed (0.0024 s) |

Integrity checks performed by this reviewer on the green-after series:

- `notExecuted = 0` on every run: no test was skipped, so no `[Ignore]` was used to obtain the result.
- Test population identical pre- and post-change: 408 in `TaskMaster.Test` (P0-T15 vs P3-T2) and 6984 across the solution (P0-T14 vs P4-T5). No test was removed and no filter was narrowed.
- No `[Ignore]`, `[DoNotParallelize]`, `[Timeout]`, retry, or sleep appears in either changed file; the `[DoNotParallelize]` occurrences in `TaskMaster.Test/AppGlobals` are pre-existing in five unrelated files.
- Commit `be2a3c1f` (the fix) is timestamped 14:35:08; the P3 runs began at 14:35:45 and the P4-T5 run at 14:41:34, so every green-after run postdates the fix. The P0 runs (14:24–14:27) predate it.

## 7. Code Quality Checks

| Stage | Command | Exit | Basis |
|---|---|---|---|
| Format (write) | `dotnet tool run csharpier format <2 files>` | 0 | executor record; command-shape deviation noted as NB-6 |
| Format (verify) | `dotnet tool run csharpier check .` | 0 | executor record (`Checked 1574 files`), plus this reviewer's own scoped re-run at exit 0 |
| Lint / analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | executor record (0 warnings, 0 errors); corroborated by build-output timestamps |
| Type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | executor record (0 warnings, 0 errors); corroborated by build-output timestamps |
| Test | `vstest.console.exe <9 assemblies> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` | 0 | this reviewer's own read of `P4-T5.trx` |

The pass is recorded as one uninterrupted sequence with zero preceding voided attempts. The two msbuild gates were not re-executed by this reviewer (each is a full-solution rebuild); they are corroborated rather than reproduced, and that limitation is stated here rather than implied away.

## 8. Gaps and Exceptions

Blocking findings: **0**.

Non-blocking findings: **7**.

- **NB-1 (code review, diagnosability).** The barrier converts a hypothetical future "terminal hook never invoked" regression from a deterministic assertion failure into an unbounded wait. The test method carries no `[Timeout]`; `TaskMaster.Test` declares no assembly-level timeout attribute; and `.github/workflows/_mstest-coverage.yml` invokes vstest without `/Settings`, so `TaskMaster.runsettings` is not applied in CI and imposes no session timeout. `spec.md:281` anticipates that such a fault "would hang until the test timed out", but no test timeout is configured. Recommendation: add `[Timeout(5000)]` to the method, matching the in-project precedent at `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:32`.
- **NB-2 (process).** Coverage Evidence Contract numeric-artifact clause unmet on both capture tasks; cause is an unsatisfiable plan precondition rather than the delivered change. Fully adjudicated in § 5.
- **NB-3 (process).** The canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent, which mandates a FAIL coverage row (§ 5). Procedural.
- **NB-4 (process).** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` in this worktree are stale: they describe head `e5dcbffd` on branch `bug/invoke-mstestwithcoverage-threshold-before-setcontent-565` with base `87233f86`, generated 2026-09-03 11:38 UTC. They do not describe this branch. Scope and evidence for this audit were therefore derived directly from git against the independently re-derived merge base. The artifacts were deliberately **not** regenerated: they are tracked files, and regenerating them would add an unrelated modification to this branch during a parallel run.
- **NB-5 (evidence reasoning).** The fail-before route-2 selection invokes the `spec.md` condition "exceeds the change budget or the remaining line headroom", supported by the observation that the Lifecycle file stands at 490 of 500 lines. That is the weaker of the recorded justifications, because route-1 instrumentation is reverted before landing and therefore never consumes landed headroom. The load-bearing justification is the second recorded reason: a red produced by deferring the fixture's own increment restates the race rather than reproducing it, and is not reproducible from the landed tree. AC6 remains satisfied as written; the criticism is of the invoked condition, not of the outcome.
- **NB-6 (command shape).** The format step ran `dotnet tool run csharpier format <two files>` rather than `CLAUDE.md`'s `format .`. Immaterial: the verification step ran `csharpier check .` repository-wide at exit 0, and this reviewer re-ran the scoped check at exit 0.
- **NB-7 (design coupling).** `Volatile.Read(ref sut.InvokedTerminalHookCount)` takes a `ref` to another type's internal instance field. It compiles only while that member remains a field; converting it to a property would break the assertion at compile time. This is consistent with the fixture's existing `_loadCount` pattern and is a deliberate trade against introducing a new accessor, but it is a coupling a future maintainer should know about.

Operational note (not a finding against the delivery): `.claude/hooks/validate-feature-review-coverage.ps1` resolves the advertised artifact paths and `artifacts/pr_context.summary.txt` with **relative** paths, against its own process working directory. The three artifacts of this review exist only under `C:/Users/DanMoisan/repos/TaskMaster-wt/prep-751`. If the hook executes with the shared session worktree as its working directory, it will not find them, because that worktree's copy of this feature folder contains only `issue.md`, `plan.2026-09-03T11-48.md`, `spec.md`, and the research record. Per the delegating instruction, no mirror was created and no `.git/info/exclude` was edited; the need is reported instead. In that same session worktree the changed-language set derived from `artifacts/pr_context.summary.txt` is empty (all ten parsed rows are `.md`), so the hook's coverage checks would be skipped there in any case.

## 9. Summary of Changes

| File | Added | Removed | Class |
|---|---|---|---|
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | 2 | 1 | test source |
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 1 | 1 | test source |
| 42 Markdown files under `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/` | — | — | requirements, plan, research, evidence |

Substantive edits:

1. `AppOlObjectsFolderTreeServiceTests.cs:113` — inserted `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);`
2. `AppOlObjectsFolderTreeServiceTests.cs:115` — `sut.InvokedTerminalHookCount.Should().Be(1);` → `Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);`
3. `AppOlObjectsFolderTreeServiceLifecycleTests.cs:200` — `InvokedTerminalHookCount++;` → `Interlocked.Increment(ref InvokedTerminalHookCount);`

## 10. Compliance Verdict

**PASS.** Zero blocking findings. Seven non-blocking findings are recorded in § 8, none of which requires work before merge. One coverage row is recorded FAIL under the mandatory canonical-artifact rule; its disposition is procedural and its substance is discharged by the independent numeric recovery in § 5. No `remediation-inputs` artifact is produced.

Deviation from `policy-audit-template-usage`: the MCP tools `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` are not available in this session. This artifact was hand-authored preserving all twelve canonical major headings rather than being marked BLOCKED, following the established handling for that unavailability.

## Appendix A: Test Inventory

| Test | File | Status |
|---|---|---|
| `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs:102` | modified — barrier assertion inserted, counter read hardened; passes on all 9 post-fix runs that include it |
| `ControlledAppOlObjects.OnFolderTreeServiceInitializationTerminal` (fixture override) | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs:196` | modified — increment made atomic |

No test was added, removed, renamed, or skipped. Solution-wide test count is 6984 before and after.

## Appendix B: Toolchain Commands Reference

```
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"
```

Commands executed by this reviewer (read-only):

```
git -C <worktree> merge-base origin/main HEAD
git -C <worktree> diff --numstat f8414ee9...HEAD
git -C <worktree> rev-parse f8414ee9:TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs
dotnet tool run csharpier check TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs
dotnet-coverage merge <attachment>.coverage -f cobertura -o <scratch>.xml
```
