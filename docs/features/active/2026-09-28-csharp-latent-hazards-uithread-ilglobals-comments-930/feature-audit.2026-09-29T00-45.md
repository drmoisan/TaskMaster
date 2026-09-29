# Feature Audit: csharp-latent-hazards-uithread-ilglobals-comments (Issue #930)

- Timestamp (caller-supplied artifact stamp): 2026-09-29T00-45
- Branch: `bug/csharp-latent-hazards-uithread-ilglobals-comments-930`
- Work mode: `minor-audit` (issue.md marker `- Work Mode: minor-audit`)
- AC source: `docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930/issue.md`, section `## Acceptance Criteria` (AC1 to AC7). No spec.md or user-story.md exists in the folder, as the mode requires.

## Scope and Baseline

- Base: `origin/main`; execution self-anchor `ac819907f479ee18026993054e714dc2e056142f` (evidence/baseline/tree-anchor.md), which is the branch head before any source edit.
- Branch commits after the anchor (reduced-audit-handoff.md): `df86ec9e` (phase 0 evidence), `699ad109` (implementation), `3b82d604` (final QC evidence), plus the terminal check-off commit and a plan-only commit reported in the executor's return.
- Code diff: six files (UtilitiesCS/Threading/UiThread.cs +2/-0; UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs +0/-3; QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs 1/1; QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs 1/1; UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs; UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs). No project, solution, runsettings or config file changed (evidence/qa-gates/footprint.md).
- Baseline state (Phase 0): CSharpier check clean; analyzer Rebuild 0 errors / 0 warnings; nullable Rebuild 0 errors; full suite 7320/7320 passed; first-party coverage 85.31% lines / 79.71% branches.
- Post-change state (Phase 2): one clean toolchain iteration; full suite 7322/7322 passed; first-party coverage 85.32% lines / 79.73% branches.
- Verification method for this audit: Read of issue.md, the plan and all 44 evidence files; Read of the six changed files at head; Grep-based confirmation of the reference search, the absolute-path sweep, and the test helper locations; arithmetic re-summation of both JaCoCo projections. No commands were executed (caller prohibited Bash); no figure below is a reviewer re-measurement.

## Acceptance Criteria Inventory

| ID | Criterion (abridged; full text in issue.md) | State in issue.md before review | Sub-issue |
|---|---|---|---|
| AC1 | New MSTest regression test exercising the dispatcher exit with the five stated conditions; asserts `IsCompleted` false; recorded failing before the fix | `[x]` | #889 |
| AC2 | Dispatcher exit requires `_dispatcher is not null`; AC1 test passes after the fix; every pre-existing `IsCompleted` test still passes | `[x]` | #889 |
| AC3 | `ILGlobals` no longer declares `modules` and no longer exposes `Cache` as a public mutable static; repository-wide search finds no remaining reference | `[x]` | #863 |
| AC4 | `ILGlobals.Cache` assertion in ILGlobals_Tests.cs updated or removed consistently; class passes | `[x]` | #863 |
| AC5 | Both XML doc comments state no numeric line count while still explaining why the partial part exists | `[x]` | #862 |
| AC6 | Full C# toolchain clean in one pass in CLAUDE.md order; existing parallel regime; changed executable lines covered; no regression | `[x]` | all |
| AC7 | Committed evidence is projections and summaries only; no committed file contains an absolute host path, the account name or the host name | `[x]` | all |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence and reasoning |
|---|---|---|
| AC1 | PASS | Test source (UiThreadApartmentMeasurement_Tests.cs lines 99 to 138) arranges all five conditions: `SetDispatcher(null)`; `SetUiThreadId(Thread.CurrentThread.ManagedThreadId)` on the executing thread; awaiter context `new DispatcherSynchronizationContext(foreignHost.Dispatcher)`, which is a `DispatcherSynchronizationContext` distinct from the captured `new SynchronizationContext()`; ambient context `new SynchronizationContext()` (non-null, not the awaiter context); executing thread created as `ApartmentState.MTA`, owning no WPF dispatcher. Assertion `observed.Should().BeFalse()`. Fail-before: evidence/regression-testing/889-fail-before.md records `Total 3, executed 3, passed 2, failed 1`, the failing name equal to the new test, the message "Expected observed to be False, but found True", `VSTEST_EXIT=1`, and `ProductionSourceState:` empty (source unmodified). The test lives in the UtilitiesCS.Test project. |
| AC2 | PASS | UiThread.cs line 199 at head reads `&& _dispatcher is not null` between the type test (198) and the reference comparison (200 to 203), the same shape as the captured-context exit at line 184. 889-pass-after.md: Threading namespace 128/128 with the new test and all twelve pre-existing `IsCompleted` names listed as `RESULT Passed`; final-06-mstest-coverage.md confirms `COUNT=1 OUTCOME=Passed` for the same thirteen names in the full run. |
| AC3 | PASS | ILGlobals.cs at head (lines 111 to 128) declares only `multiByteOpCodes` and `singleByteOpCodes`, both `public static readonly`; neither `Cache` nor `modules` appears (863-fix-applied.md whole-word counts 0 and 0; numstat 0 added / 3 deleted). Reference search: 863-reference-search.md (`git grep -F` over the identifier, quoted and single-quoted string forms, excluding docs/.claude/artifacts) found one hit, the `"modules"` JSON key in config/blast-radius.json (unrelated positive control); repository-wide, every other hit is under docs/ or .claude/. Reviewer Grep over `*.cs` for `ILGlobals\.(Cache|modules)\b`: zero matches. Solution-wide Rebuild after deletion: 0 Error(s) across 18 projects (863-build-green.md). |
| AC4 | PASS | `Cache_IsInitialized` removed (absent from all 15 RESULT lines in 863-pass-after.md; present at baseline). Replaced by `PublicStaticFields_AreAllInitOnly` and `PublicStaticFields_AreExactlyTheTwoOpCodeTables`, both failing before (863-fail-before.md: passed 13, failed 2, naming `Cache` and `modules` in the failure messages) and passing after (15/15). |
| AC5 | PASS | Head lines read verbatim: BreadcrumbBridgeCoordinator.Search.cs line 11 `/// Held on a second partial-class part so <c>BreadcrumbBridgeCoordinator.cs</c>` and line 12 `/// stays clear of the repository's 500-line ceiling.`; BreadcrumbItemViewerLifecycleCoordinator.Search.cs lines 9 to 11 `/// Held on a second partial-class part so` / `/// <c>BreadcrumbItemViewerLifecycleCoordinator.cs</c> stays clear of the` / `/// repository's 500-line ceiling.`. No numeral remains; the explanation is retained. 862-comment-edit.md: `STALE_487=0`, `STALE_481=0`, `CEILING_BRIDGE=1`, `CEILING_LIFECYCLE=1`; numstat over QuickFiler/Viewers lists exactly the two files. |
| AC6 | PASS | toolchain-final-pass.md: `LOOP: CLEAN PASS`, `LOOP-ITERATIONS: 1`, `SOURCE-REWRITE-COMMITS: NONE`; steps in CLAUDE.md order (format, check, analyzers Rebuild, nullable Rebuild, MSTest with coverage), each exit 0. Parallel regime: concurrency-regime.md shows `ADDED_DoNotParallelize=0`, `ADDED_Workers=0`, `ADDED_Retry=0`, runsettings hash equal to baseline, `DNP_HARDENING_FILE=2` and `DNP_ILGLOBALS_FILE=0` unchanged. Coverage: the one changed executable production line (UiThread.cs 199) has HITS=1; the return expression's condition coverage is 4/4; per-file uncovered counts unchanged (UiThread 3 and 3; ILGlobals 2 and 2); first-party line and branch percentages rose (85.31 to 85.32; 79.71 to 79.73). Method note: both coverage runs excluded four shell-icon test classes identically (plan Decision D3, workstation hang); CI runs them unfiltered, so the local absolute figure is an under-approximation, while the delta and the per-file gates are unaffected. |
| AC7 | PARTIAL | Projection-and-summary form: PASS (`RAW_TOOL_DOCS=0`; the only tool-derived committed files are two package-level JaCoCo projections, which parse with 9 packages each and re-sum exactly to the reported totals, and two trx-derived summaries). Account name and host name: PASS (`ACCOUNT_HITS=0`, `HOST_HITS=0`, `ROOT_HITS=0`, `DRIVE_USERS_HITS=0`, with positive controls 15 / 15 / 7325 / 15 against the raw log and trx). Absolute host path clause: FAIL. Reviewer Grep of the feature folder for `[A-Za-z]:[\\/][A-Za-z]` found two lines: evidence/baseline/baseline-04-mstest-coverage.md line 11 and evidence/qa-gates/final-06-mstest-coverage.md line 12, both carrying `VS-INSTALL-ROOT\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. The plan's sanitize gate patterns (account, host, worktree root, `<drive>:\Users\`) do not match a Program Files path, so the gate passed while the AC's literal clause is unmet; the plan's own executor note required a placeholder for any value outside the repository. |

## Summary

- Verdict: 6 of 7 acceptance criteria PASS; AC7 PARTIAL.
- Blocking findings: 0. Non-blocking: 1 (AC7 absolute path on two evidence lines). Informational: see policy-audit.2026-09-29T00-45.md section 8 (I-1 to I-9) and code-review.2026-09-29T00-45.md.
- Evaluation of the caller's "known facts":
  - Breaking public API change (`ILGlobals.Cache`, `ILGlobals.modules` removed): no in-repo consumer, verified by the solution-wide Rebuild, the executor's string and reflection-aware search, and the reviewer's own Grep. The change-description obligation is met by plan Decision D4 and the handoff, and is discharged for the PR only when the PR body names both members. `quality-tiers.yml` is absent from the branch root, so the tier "major bump" gate is not operable; the call-out is the applicable control.
  - Timestamp correction: disclosed; no measured figure or AC depends on a `Timestamp:` value; the corrected values are file write times rather than CMD-TS observations; sequence is monotone and consistent with later phases; the disclosure does not enumerate the corrected artifacts. Informational.
  - Analyzer package versions installed into the gitignored packages folder: pre-existing HintPath skew on origin/main (reviewer-verified in UtilitiesCS.csproj: props import at 3.0.290, Analyzer item at 3.0.235); no tracked file changed; outside this diff's footprint. Informational follow-up.
  - Four shell-icon test classes excluded identically from both coverage runs: sound for the delta and the per-file gates; CI covers the absolute figure. Informational.

### Remediation-required findings (no separate remediation-inputs artifact, per the caller's three-artifact instruction)

1. AC7 / NB-1: in `evidence/baseline/baseline-04-mstest-coverage.md` (line 11) and `evidence/qa-gates/final-06-mstest-coverage.md` (line 12), replace `VS-INSTALL-ROOT\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` with a placeholder form (for example `PROGRAM-FILES\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`), commit in the exempt docs-only form, then re-check AC7 in issue.md. Optionally add the enumerated list of corrected Phase 0 artifacts to reduced-audit-handoff.md in the same commit (I-1).

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930/issue.md
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining: AC7: Committed evidence follows the CLAUDE.md "Committed Test Evidence Format" section (projections and summaries only, no raw trx or raw coverage collector document), and no committed file contains an absolute host path, the developer account name, or the host name.

## Acceptance Criteria Check-off

- AC1 to AC6: evaluated PASS; already `[x]` in issue.md; left checked. No criterion text modified.
- AC7: evaluated PARTIAL; changed from `[x]` to `[ ]` in issue.md by this review (only the checkbox changed; criterion text preserved). Gap recorded above.
- Newly checked-off items by this review: none.
- Phantom criteria added: none.
