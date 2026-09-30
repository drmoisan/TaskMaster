# Feature Audit — package-manifest-consistency-residuals (Issue #929)

- Artifact timestamp label: 2026-09-30T10-30 (caller-assigned)
- Work mode: `minor-audit` (issue.md line 12); AC source: the `## Acceptance Criteria` section of issue.md only (AC1 to AC7)
- Branch: `bug/package-manifest-consistency-residuals-929`; head `5ce3c8c3b`; source diff base `231e1c0b5`
- Companion artifacts: `policy-audit.2026-09-30T10-30.md`, `code-review.2026-09-30T10-30.md`
- Total blocking findings: **0**
- Verdict: **PASS** — 7 of 7 acceptance criteria evaluated PASS; every executor check-off stands; no criterion is to be unchecked

## Executive Summary

Each of the seven acceptance criteria was evaluated against the files on disk in the item worktree and against the executor's committed evidence, not against the executor's check-off notes alone. AC1 to AC3 and AC5 to AC6 are verifiable by direct reading and all hold. AC4 holds on the two added in-memory detector tests plus the tree census test's red-then-green pair and the read-only verifier run whose absent-from-manifest count moved from 2 to 0. AC7 holds on the cold restore, the aligned analyzer census (no back-fill was needed, so the permissive back-fill clause was not exercised), and the two toolchains passing on the final iteration with no regression against the Phase 0 baseline. The one CI test failure and the one local iteration-1 test failure are on pre-existing timing-dependent C# tests and are not attributable to a change that edits no C# source. No blocking finding exists; follow-ups are listed for the orchestrator and not filed from this branch.

## Scope and Baseline

- Baseline: `231e1c0b55105aeb626bf5a6e8d0266a567cacad` (origin/main at merge 3091b8af9; P0-T1 BASE-SHA). Baseline measurements: MSTest 7346 of 7346, C# 85.91% lines / 80.08% branches (P0-T12); CI Pester on main run 36666302259: 373 passed, 94.51% lines (P0-T16); analyzer census `ANALYZER-ITEM-STATE: aligned`, `ANALYZER_ITEMS=162 FILES=17 UNRESOLVED=0` (P0-T6); pre-fix altcover lines 2 and verifier `ABSENT=2` (P0-T17); pre-fix redirect repairs 1 and 1 (P0-T18).
- Post-change: MSTest 7346 of 7346, C# 85.92% / 80.08% (P2-T7 iter2); CI Pester on branch run 36722780748 head `b96926588`: 379 passed, 94.51% (P2-T3 iter2); verifier `ABSENT=0` (P1-T12); redirect repairs 0 and 0 (P1-T5).
- Diff scope (P2-T10, verified on disk): `.github/workflows/README.md`, `.github/workflows/dependabot-repair.yml`, `QuickFiler.Test/QuickFiler.Test.csproj`, `SVGControl/app.config`, the 911 runbook, `scripts/dependencies/ConsistencyVerifier.psm1`, `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`, `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`, the promoted record, and the feature folder. Zero `.cs` files; zero other `.csproj`, `packages.config` or `app.config` files.
- Method: Read, Grep and Glob only (caller constraint). Branch head read from the worktree's git ref file. PR context artifacts are absent in the worktree and were not regenerable; scope was triangulated from the caller's diff, P2-T10 and disk.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/issue.md`, section `## Acceptance Criteria`, lines 44-50. Seven checkbox items, all `[x]` at review start. The maintainer follow-up paragraph (line 52) and Summary item 4 are not acceptance criteria and are not evaluated as such.

| ID | Criterion (abridged) | State at review start |
|---|---|---|
| AC1 | No `altcover` `Import` in `QuickFiler.Test.csproj`; case-insensitive search across tracked project files and manifests returns zero | `[x]` |
| AC2 | `SVGControl/app.config` Fizzler redirect newVersion 1.3.1.0 and oldVersion range ending at 1.3.1.0, matching the project reference | `[x]` |
| AC3 | `SVGControl/app.config` System.Runtime.CompilerServices.Unsafe redirect newVersion 6.0.3.0 and range ending at 6.0.3.0, matching the project reference | `[x]` |
| AC4 | Verifier reports an Import whose package is absent from the manifest and no finding when declared; both covered by in-memory Pester tests, no temporary files; zero findings against the tree after AC1 | `[x]` |
| AC5 | Workflow passes `client-id`, no `app-id`; actionlint clean | `[x]` |
| AC6 | Runbook instructs storing the Client ID (not the numeric App ID) in the secret the workflow reads, naming it exactly | `[x]` |
| AC7 | Cold restore, solution rebuild succeeds (permissive back-fill clause), C# and PowerShell toolchains pass with no new failures relative to the Phase 0 baseline | `[x]` |

## Acceptance Criteria Evaluation

| ID | Verdict | Blocking | Evidence (reviewer-verified) |
|---|---|---|---|
| AC1 | PASS | No | Grep `altcover` (case-insensitive) over `*.csproj`, `*.config`, `*.props`, `*.targets` in the worktree: 0 matches. P1-T4: `ALTCOVER_LINES=0` over `*.csproj` and `*/packages.config` (P0-T17 measured 2), file 570 to 568 lines, the two deleted lines quoted verbatim. Residue: two tracked `.csproj.bak` copies still carry the token; they are not project files (MSBuild never reads `.bak`) and are outside the criterion's stated pathspec — recorded as a follow-up, not a gap. |
| AC2 | PASS | No | `SVGControl/app.config` line 15: `oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"`; `SVGControl/SVGControl.csproj` line 58: `Fizzler, Version=1.3.1.0`; `packages.config` line 4 pins Fizzler 1.3.1. P0-T18 `FIZZLER_ASM=1.3.1.0`; P1-T5 `FIZZLER_REPAIRS=0`. Tree test 2 green (P1-T11). |
| AC3 | PASS | No | `SVGControl/app.config` line 19: `oldVersion="0.0.0.0-6.0.3.0" newVersion="6.0.3.0"`; `SVGControl.csproj` line 82: `System.Runtime.CompilerServices.Unsafe, Version=6.0.3.0` (package 6.1.2 ships assembly version 6.0.3.0; P0-T18 `UNSAFE_ASM=6.0.3.0`); P1-T5 `UNSAFE_REPAIRS=0`. Tree test 2 green. |
| AC4 | PASS | No | Positive case: `ConsistencyVerifier.Tests.ps1` lines 240-251 (`GuardedUnmanifestedProject` fixture at 60-71, two guarded altcover Imports; FindingCount 2, every Kind `Import`, every PackageFolder `altcover.8.6.45`). Negative case: lines 253-263 (`AgreeingProject` declares its imported package; Import-kind subset empty; ExaminedCount greater than 0). Fixtures are here-strings; Grep over both test files for `Out-File`, `Set-Content`, `Add-Content`, `New-Item`, `New-TemporaryFile`, `[System.IO.File]::Write*`, `Remove-Item`: none. Suite: 14 of 14 (P1-T3 onward). Tree: `RepositoryTreeConsistency.Tests.ps1` test 1 red with 2 findings on the base tree (P1-T2 message names lines 8 and 537 of `QuickFiler.Test.csproj`) and green after AC1 (P1-T11); read-only verifier `ABSENT=0` (P1-T12) versus 2 (P0-T17). The detection rule pre-existed (plan decision D1); the criterion asks for the behaviour and its tests, both of which are present. |
| AC5 | PASS | No | `dependabot-repair.yml` line 51: `client-id: ${{ secrets.DEPENDABOT_REPAIR_APP_ID }}`; Grep `app-id` under `.github/workflows`: no match. actionlint 1.7.7 scoped and repository-wide exit 0 with no output (P1-T7); CI actionlint job on run 36722780748 succeeded. Tree test 3 green. |
| AC6 | PASS | No | Runbook line 99 heading "Record the Client ID and generate a private key"; lines 101-106 instruct recording the Client ID and state the numeric App ID is not needed; lines 129-131 create the secret named `DEPENDABOT_REPAIR_APP_ID` with "the Client ID value recorded in step 10"; line 154 YAML sample `client-id: ${{ secrets.DEPENDABOT_REPAIR_APP_ID }}`, identical to workflow line 51. Tree test 4 extracts the secret name from the workflow and asserts it in the runbook (green). README line 116 aligned. Cosmetic residue at line 301 ("App ID location") noted in the code review. |
| AC7 | PASS | No | Cold restore: P0-T5 `PACKAGE_DIRS_BEFORE=0`, `PACKAGE_DIRS_AFTER=172`, `Build succeeded.`, 0 errors. Analyzer census aligned, `UNRESOLVED=0` (P0-T6), so no back-fill occurred and the permissive clause was not exercised; P2-T10 records 0 other `.csproj`, `packages.config` or `app.config` paths changed. Rebuilds: P0-T10, P0-T11, P2-T5 iter2, P2-T6 iter2 each `Build succeeded.` 0 warnings 0 errors. C# toolchain final iteration: CSharpier check exit 0 (P2-T4), analyzer and nullable rebuilds exit 0, MSTest 7346 of 7346 with 85.92% / 80.08% (P2-T7 iter2) against baseline 7346 of 7346 with 85.91% / 80.08%. PowerShell toolchain: PoshQC format rewrote nothing (P2-T1), analyze pass (P2-T2), test 137 of 137 (P2-T3) against 131 of 131 baseline; CI Pester 379 of 379 at 94.51% against 373 of 373 at 94.51%. No new failure relative to the Phase 0 baseline. Iteration 1's single MSTest failure and CI run 36722780748's single `mstest-coverage` failure are assessed below as not attributable. |

## Acceptance Criteria Check-off

- Newly checked off by the reviewer: none (all seven were already `[x]`).
- Left unchecked or recommended to uncheck: none. Each criterion evaluated PASS, so every existing `[x]` stands.
- `issue.md` was not modified by this review.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/issue.md` (`## Acceptance Criteria`)
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

## CI Failure Attribution

- CI run 36722780748 (workflow_dispatch, head `b96926588`): `pester`, `build-analyzers`, `build-nullable`, `format-check`, `actionlint`, `hygiene` succeeded; `mstest-coverage` failed 1 of 7346 (`Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` line 206, a dispatcher-fixture concurrency test).
- Local final QC iteration 1 failed a different test (`RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`, `QfcDatamodelLivenessTests.cs` line 189, a five-second wall-clock wait); iteration 2 on the unchanged tree passed 7346 of 7346, as did the Phase 0 baseline.
- Attribution: this change edits no `.cs` file. The only QuickFiler.Test edit removes two `Exists()`-guarded imports whose target folder is never restored (no manifest declares altcover; P0-T5 `ALTCOVER-RESTORED: False`; CI has no cache fallback since issue 936), so the compiled test assembly is unchanged. Both tests exist on the base tree (they arrived via the origin/main merge) and both passed twice locally on this tree. Two different tests failing once each on two different hosts is the signature of pre-existing timing dependence, not of this diff. Verdict: not attributable; AC7's "no new failures relative to the Phase 0 baseline" holds on the local baseline-versus-final comparison it names.
- Residual gate owned by the orchestrator: the PR-time CI run on the branch head `5ce3c8c3b` (CI has so far built only `b96926588`; the later commits are feature-folder evidence and the second origin/main merge).

## Recommended Follow-ups (list only; not filed from this branch)

1. Promote the two timing-dependent QuickFiler.Test tests (`QfcDatamodelLivenessTests.cs` line 189 wall-clock `Task.Wait`; `QfcItemController.UiThreadDispatcherFixtureTests.cs` line 206) to an issue under the determinism rule (no real wall-clock waits in test code).
2. Delete the eight tracked `*.csproj.bak` copies listed in P0-T1 BAK-TRACKED; two still carry the `altcover` token.
3. Runbook line 301: reword "App ID location" to "Client ID location".
4. `dependabot-repair.yml` line 14: re-wrap the header comment.
5. Optional test hardening in `RepositoryTreeConsistency.Tests.ps1`: split test 2 per assembly; assert test 1 skips no manifest directory.
6. Maintainer follow-up already recorded at P2-T19 (not a merge gate): provision the GitHub App credential with `DEPENDABOT_REPAIR_APP_ID` holding the Client ID, confirm the secret store for a Dependabot-triggered `workflow_run`, then exercise AC18 to AC20 of issue 911 and obtain the first green run of the modified workflow.
7. Pre-existing: eleven other `app.config` files redirect Fizzler to 1.3.0.0 (tracked in `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`).
8. Executor evidence hygiene: `Timestamp:` labels should be clock readings; on this run they lead the embedded UTC stamps by 38 to 72 minutes.

## Summary

Verdict PASS. 7 of 7 acceptance criteria PASS; 0 blocking findings; no remediation inputs. The delivered change satisfies the issue's invariant (a project's files and its own `packages.config` agree, and the repair workflow is runnable once its credential exists) with a minimal diff, a red-then-green regression suite, and toolchain evidence that reconciles to the committed projections and the CI logs. The orchestrator's remaining gate is the PR-time CI run on the branch head.
