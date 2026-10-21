# Feature Audit: csharp-latent-hazards-uithread-ilglobals-comments (Issue #930), Remediation Cycle 1 Re-audit

- Timestamp (caller-supplied artifact stamp): 2026-09-29T10-16
- Branch: `bug/csharp-latent-hazards-uithread-ilglobals-comments-930`
- Work mode: `minor-audit` (issue.md marker `- Work Mode: minor-audit`)
- AC source: `docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930/issue.md`, section `## Acceptance Criteria` (AC1 to AC7). The folder holds no spec.md or user-story.md as an AC source (a Grep hit for those names in the plan, the remediation plan and the prior feature audit is prose about the mode rule).

## Scope and Baseline

- Base: `origin/main`; execution self-anchor `ac819907f479ee18026993054e714dc2e056142f`; cycle self-anchor `39845d4a3f2f38d5c018f41e15e8372d432553c5`.
- Code diff (unchanged since the prior audit): six files (UiThread.cs +2/-0; ILGlobals.cs +0/-3; two QuickFiler `.Search.cs` files 1/1 each; two UtilitiesCS.Test files). No project, solution, runsettings or config file changed.
- Cycle 1 diff: documentation only, inside the feature folder. Six tracked feature-folder modifications at footprint time (five substituted files plus the remediation plan's check-offs), plus untracked r1 evidence files and, later in the cycle, the AC7 marker in issue.md. No tracked path outside the feature folder and `.claude/agent-memory/` (r1-footprint.md `OUTSIDE_TRACKED_CHANGES=0`).
- Baseline state (Phase 0): CSharpier check clean; analyzer Rebuild 0 errors and 0 warnings; nullable Rebuild 0 errors; full suite 7320/7320; first-party coverage 85.31% lines / 79.71% branches.
- Post-change state (Phase 2, unchanged): one clean toolchain iteration; 7322/7322; 85.32% lines / 79.73% branches.
- Verification method: Read of issue.md, the remediation inputs and plan, every r1 evidence file and both substituted evidence summaries; Grep scans of the feature folder and the worktree; no command was executed (caller prohibited Bash), so no figure below is a reviewer re-measurement except the identified Grep counts.

## Acceptance Criteria Inventory

| ID | Criterion (abridged; full text in issue.md) | State in issue.md before this re-audit | Sub-issue |
|---|---|---|---|
| AC1 | New MSTest regression test exercising the dispatcher exit with the five stated conditions; asserts `IsCompleted` false; recorded failing before the fix | `[x]` | #889 |
| AC2 | Dispatcher exit requires `_dispatcher is not null`; AC1 test passes after the fix; every pre-existing `IsCompleted` test still passes | `[x]` | #889 |
| AC3 | `ILGlobals` no longer declares `modules` and no longer exposes `Cache` as a public mutable static; repository-wide search finds no remaining reference | `[x]` | #863 |
| AC4 | `ILGlobals.Cache` assertion in ILGlobals_Tests.cs updated or removed consistently; class passes | `[x]` | #863 |
| AC5 | Both XML doc comments state no numeric line count while still explaining why the partial part exists | `[x]` | #862 |
| AC6 | Full C# toolchain clean in one pass in CLAUDE.md order; existing parallel regime; changed executable lines covered; no regression | `[x]` | all |
| AC7 | Committed evidence is projections and summaries only; no committed file contains an absolute host path, the account name or the host name | `[x]` (restored by cycle 1) | all |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence and reasoning |
|---|---|---|
| AC1 | PASS | Unchanged from the prior audit. The test (UiThreadApartmentMeasurement_Tests.cs lines 99 to 138) arranges `SetDispatcher(null)`, a captured UI thread id equal to the executing thread's managed id, an awaiter context that is a `DispatcherSynchronizationContext` distinct from the captured UI context, a non-null differing ambient context, and an MTA executing thread with no WPF dispatcher; asserts `observed.Should().BeFalse()`. evidence/regression-testing/889-fail-before.md: `Total 3, passed 2, failed 1`, the new test failed with "Expected observed to be False, but found True", `VSTEST_EXIT=1`, production source unmodified. The cycle changed no test file (r1-toolchain-exemption.md). |
| AC2 | PASS | Reviewer Grep of UiThread.cs at head finds `&& _dispatcher is not null` at line 184 (captured-context exit) and line 199 (dispatcher exit). 889-pass-after.md: Threading namespace 128/128 with all twelve pre-existing `IsCompleted` names Passed; final-06-mstest-coverage.md confirms the same names in the full run (7322/7322). Unchanged by the cycle. |
| AC3 | PASS | Reviewer Grep over `*.cs` in the worktree for `ILGlobals\.(Cache|modules)\b` and a public static `Cache` or `modules` declaration: zero matches. 863-fix-applied.md: numstat 0 added / 3 deleted; 863-reference-search.md: no `.cs` hit; solution Rebuild 0 errors (863-build-green.md). |
| AC4 | PASS | `Cache_IsInitialized` removed (absent from all 15 results in 863-pass-after.md); replaced by `PublicStaticFields_AreAllInitOnly` and `PublicStaticFields_AreExactlyTheTwoOpCodeTables`, failing before (863-fail-before.md: 13 passed, 2 failed) and passing after (15/15). Unchanged by the cycle. |
| AC5 | PASS | Reviewer Grep of `QuickFiler/Viewers` for `(487|481) lines`: zero matches. 862-comment-edit.md: `STALE_487=0`, `STALE_481=0`, `CEILING_BRIDGE=1`, `CEILING_LIFECYCLE=1`; the explanatory sentence about the 500-line ceiling remains. |
| AC6 | PASS | toolchain-final-pass.md: `LOOP: CLEAN PASS`, `LOOP-ITERATIONS: 1`, steps in CLAUDE.md order each exit 0 (CSharpier check 1623 files; analyzers 0 errors and 0 warnings with 18 project compiles; nullable 0 errors; 7322/7322 with coverage). Parallel regime: `ADDED_DoNotParallelize=0`, `ADDED_Workers=0`, `ADDED_Retry=0`, runsettings hash unchanged. Coverage: the changed executable line has HITS=1, return-expression condition coverage 4/4, per-file uncovered counts unchanged, first-party 85.31% to 85.32% lines and 79.71% to 79.73% branches. The cycle changed no C# input, so this evidence remains current: r1-toolchain-exemption.md shows `CODE_OR_CONFIG_TRACKED_CHANGES=0` and `CODE_OR_CONFIG_PORCELAIN=0` (control 24 markdown rows), and r1-footprint.md shows `OUTSIDE_TRACKED_CHANGES=0`. |
| AC7 | PASS | All three clauses now hold. (1) Projection-and-summary form: `RAW_TOOL_DOCS=0` over 64 files (r1-sanitize-final.md); the tool-derived committed evidence is two package-level JaCoCo projections and two trx-derived summaries. (2) Account name and host name: `ACCOUNT_HITS=0`, `HOST_HITS=0`, `ROOT_HITS=0`, `DRIVE_USERS_HITS=0` with positive controls 15, 15, 7325 and 15 against the raw log and trx. (3) Absolute host path: reviewer Grep of the whole feature folder for `\b[A-Za-z]:[\\/]\S` returns zero matches; the same pattern returns 26 matches against the gitignored raw coverage log, so the zero is not a blind pattern; r1-drive-scan-baseline.md recorded 7 hits in 5 files (53 files scanned) and r1-drive-scan-final.md records 0 hits (63 files scanned, controls met). Substitution exactness: r1-subst-1 to r1-subst-5 each show the post-edit hash equal to the expected hash computed from the pre-edit text with only the path prefix replaced, with unchanged line counts, so the substituted files differ from their pre-image only in the path text; the reviewer re-read both evidence summaries and found the surrounding figures intact. The prior PARTIAL (two evidence lines plus three audit quotations) is resolved. |

## Summary

- Verdict: 7 of 7 acceptance criteria PASS. AC1 to AC6 are unchanged PASS (the C# code diff and toolchain evidence did not change in the cycle); AC7 moved from PARTIAL to PASS.
- Blocking findings: 0. Non-blocking: 0 (NB-1 resolved). Informational: 9, carried forward from the prior audit (I-1 to I-9 in policy-audit.2026-09-29T10-16.md): Phase 0 timestamp-correction disclosure; analyzer HintPath version skew on origin/main; the breaking removal of the public members `ILGlobals.Cache` and `ILGlobals.modules`, which the PR body must name; the now-unused `using System.Collections.Generic;` in ILGlobals.cs; six out-of-footprint UtilitiesCS lines flipping to covered between runs; four shell-icon test classes excluded from both local coverage runs; absent PR-context artifacts; ILGlobals.cs file-level percentage arithmetic; the test file hosting two classes.
- No new finding was introduced by the cycle (checked: footprint, code and configuration exemption, whole-folder drive-rooted scan, four identity counts, the hash chain, the issue.md diff limited to the AC7 marker).
- Procedural note (not a finding): the cycle's edits and the r1 evidence files are uncommitted in the working tree at review time; the caller instructed this review not to commit. The PR requires them committed in the exempt docs-only form.
- Remediation-required findings: none. No remediation-inputs artifact is produced.
- The caller-supplied artifact stamp (10-16) precedes some r1 evidence `Timestamp:` values (10-18 to 10-26). The stamp is retained for hook cross-artifact matching, as in the prior audit; no verdict depends on it.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930/issue.md
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

- AC1 to AC7: evaluated PASS; all seven are `[x]` in issue.md (lines 44 to 50, read directly by this reviewer). AC7 was flipped from `[ ]` to `[x]` by the cycle-1 executor on verified evidence (r1-ac-status.md); this re-audit's evaluation agrees, so no checkbox was changed by this review and no criterion text was modified.
- Newly checked-off items by this review: none.
- Phantom criteria added: none.
