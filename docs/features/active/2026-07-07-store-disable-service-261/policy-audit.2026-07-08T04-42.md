# Policy Compliance Audit — Store Disable Service (F1, Issue #261) — Remediation Cycle 1 Reaudit

- Timestamp: 2026-07-08T04-42
- Reviewer: feature-reviewer
- Feature branch: `feature/store-disable-service-261` @ HEAD `8e11614e` (original feature commit
  `88366ad4` + remediation cycle-1 commit `8e11614e`)
- Base (merge-base): `8bd91d1d` on `origin/epic/store-lockup-resilience-integration`
- Diff scope: `git diff 8bd91d1d..HEAD` (full branch-vs-base diff, both commits)
- Work mode: `full-feature` (AC sources: `spec.md` §9 + `user-story.md`)
- Prior-cycle audit reference: `policy-audit.2026-07-07T23-46.md`,
  `remediation-inputs.2026-07-07T23-46.md`

## Executive Summary

This is the cycle-1 exit reaudit following remediation of the single Blocking finding (R1) and the
one Non-blocking finding (N1) from the entry-cycle audit. Both are independently verified resolved:

- **R1 (file-size, was Blocking):** `StoresWrapperTests.cs` (688 lines) was split into
  `StoresWrapperTests.cs` (415 lines, independently confirmed via `wc -l`) and a new
  `StoresWrapperDisableTests.cs` (368 lines, independently confirmed via `wc -l`), wired into
  `UtilitiesCS.Test.csproj` via `<Compile Include="OutlookObjects\Store\StoresWrapperDisableTests.cs" />`
  (independently confirmed via `grep`). Both files are now well under the 500-line cap.
- **N1 (unawaited async assertions, was Non-blocking):** the two `ReenableAsync` guard tests in
  `StoreDisableServiceTests.cs` were converted from `public void` to `public async Task` with
  `await` added immediately before each `.ThrowAsync<...>()` call (independently confirmed via
  `git diff 88366ad4..8e11614e`). vstest reports both as individually timed `Passed` results.

No new Blocking or Non-blocking findings were introduced by the remediation commit. Toolchain gates
(csharpier, analyzers, nullable/TreatWarningsAsErrors, MSTest with coverage) are green per the
cycle-1 evidence tree. Repository line coverage after remediation is 81.61% (independently
consistent with the cycle-1 evidence's Cobertura root figure), still above the CLAUDE.md >= 80%
testable-denominator floor; the remediation touches only test files and adds no new production
code, so the AC15 new-code coverage obligation carries forward unchanged from the entry cycle
(StoreIdentity.cs 100%, StoreDisableService.cs 97.92%, DisabledStoreEntry 100%, all >= 90%).

**Total Blocking findings in this reaudit: 0.**

Overall verdict: **PASS**.

## Independent Verification Performed This Cycle

This reaudit did not rely solely on the remediation evidence tree; the following were independently
reproduced by this reviewer:

1. `wc -l UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs
   UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs` -> 415 and 368 lines.
2. `grep -n "StoresWrapperDisableTests" UtilitiesCS.Test/UtilitiesCS.Test.csproj` -> confirms the
   `<Compile Include>` wiring.
3. `git diff 88366ad4..8e11614e -- UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs`
   -> confirms exactly the two signature changes (`void` -> `async Task`) and the two added `await`
   keywords described by N1, with no other change to that file.
4. **Verbatim-move check (not previously performed):** concatenated the two post-split files,
   stripped `using`/`namespace`/brace-only lines, sorted, and diffed against the same normalization
   of the pre-split 688-line file (`git show 88366ad4:...StoresWrapperTests.cs`). Result: zero
   deleted lines (every line from the original file is present in the union of the two post-split
   files); the only additions are duplicated shared test-helper lines (`CreateStore`,
   `CreateGlobals`, `AssertInclusionDecision`) that necessarily appear once in each of the two
   files. `[TestMethod]` count: 22 in the pre-split file, 11 + 11 = 22 in the two post-split files.
   This confirms no test assertion or logic was altered, consistent with the "moved verbatim" claim.
5. Confirmed `LiveOutlookHookupIntegrationTests.cs` (the file containing the one environment-failing
   test) is not present in `git diff 8bd91d1d..HEAD --name-only` — this feature's diff does not
   touch that file at all.
6. Manual scan of `git diff 8bd91d1d..HEAD --name-only` for `artifacts/baselines/`,
   `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/` — no matches (see Evidence Location
   Compliance below).

## Assessment: Pre-Existing Live-Outlook Test Failure

The full-suite run in this cycle's evidence (`evidence/remediation-baseline/test-coverage-baseline-cycle1.md`,
`evidence/qa-gates/qa-05-coverage-post-change-cycle1.md`) shows 5031 passed / 1 failed out of 5032
total, both before and after the remediation edits. The single failure is
`TaskMaster.Test.AppGlobals.LiveOutlookHookupIntegrationTests.LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`,
failing with `COMException (0x80010100) ... RPC_E_SYS_CALL_FAILED` while retrieving the
`Outlook.Application` COM class factory — a live-Outlook-COM integration test that requires a
running/registered Outlook COM server.

**Disposition: not a Blocking finding for this feature; out of scope.** Reasoning:

- This feature's diff (`git diff 8bd91d1d..HEAD --name-only`) does not touch
  `LiveOutlookHookupIntegrationTests.cs` or any file in its dependency path. The test's git history
  (`13296f31 fix(test): skip LiveOutlook harness when Outlook is unavailable (#207, PR #210 CI)`)
  shows it is a pre-existing, previously-known-flaky live-Outlook harness unrelated to issue #261.
- The entry-cycle audit (`policy-audit.2026-07-07T23-46.md`, evidence
  `evidence/qa-gates/qa-04-test-coverage.md`) ran the full suite at commit `88366ad4` (identical
  commit to this cycle's Phase-0 baseline) and reported 5032 passed / **0** failed — this exact
  test passed in that run. The cycle-1 Phase-0 baseline re-ran the full suite at the same commit
  `88366ad4` and reported 5031 passed / 1 failed on this same test. Two test runs at the identical
  commit producing different outcomes for one test, with zero code difference between the runs,
  is direct evidence that the failure is a function of local machine/COM-server state (whether a
  live Outlook COM class factory happens to be registered/available in the run environment at
  that moment), not of any code change in this feature or its remediation.
- The failure is present identically before and after the remediation commit `8e11614e`
  (5031/1 both at Phase-0 baseline and post-change), so the remediation introduces no regression.
- Conclusion: this is a pre-existing, environment-dependent integration test failure, unrelated to
  and unaffected by this feature's diff. It is documented here for transparency but is **not
  counted toward the Blocking total** for this feature review.

## Rejected Scope Narrowing

None. The caller instruction directed review of the full branch diff against the resolved base
(`8bd91d1d..HEAD`, covering both the original feature commit and the remediation commit) and did
not attempt to narrow scope to a plan/task/phase or a file subset. The caller's "decided points"
(coverage floor per CLAUDE.md; net48 `readonly struct` realization; prior interface-forced-implementer
disposition) are policy-authority and platform-constraint clarifications carried forward from the
entry-cycle audit, not scope narrowing introduced in this reaudit. The full feature-vs-base diff
was audited, including all evidence and documentation files added by the remediation.

## Evidence Location Compliance

`validate_evidence_locations.py` is not present in this repository (consistent with prior review
cycles in this repo). A manual scan of `git diff 8bd91d1d..HEAD --name-only` shows no files written
under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. All
feature evidence, including the new cycle-1 remediation evidence, is under the canonical
`docs/features/active/2026-07-07-store-disable-service-261/evidence/<kind>/` tree (`baseline`,
`qa-gates`, `remediation-baseline`, `issue-updates`, `other`). Coverage Cobertura files remain under
the repo-standard `coverage/` directory produced by `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
PASS.

## 1. Coverage Verification (mandatory per changed language)

Only C# has changed code files in the branch diff (both the original feature commit and the
remediation commit). TypeScript, Python, and PowerShell have zero changed files (verified: no
`.ts/.tsx/.py/.ps1` in `git diff 8bd91d1d..HEAD --name-only`), so no coverage verdict is required
for them.

### 1.1 C# coverage (changed language — verdict required)

- Coverage source: `coverage/remediation-cycle1-baseline.cobertura.xml` (Phase-0, pre-remediation-edit
  baseline) and the cycle-1 post-change Cobertura re-measure, both produced by
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` over all 7 `*.Test.dll` as CI does. Evidence docs:
  `evidence/remediation-baseline/test-coverage-baseline-cycle1.md`,
  `evidence/qa-gates/qa-05-coverage-post-change-cycle1.md`,
  `evidence/qa-gates/qa-06-coverage-delta-cycle1.md`.
- Repo-wide line coverage:
  - Baseline (this cycle, pre-remediation-edit, at commit `88366ad4`): 81.62%
    (119,363 / 146,244 lines).
  - Post-change (at commit `8e11614e`): 81.61% (119,396 / 146,294 lines).
  - Change: -0.01pp (within run-to-run noise; the remediation adds only duplicated test-helper
    lines, no production code).
  - Disposition: PASS against the CLAUDE.md >= 80% testable-denominator floor.
  - Evidence: `evidence/qa-gates/qa-06-coverage-delta-cycle1.md`.
  - New/changed-code coverage: 97.92% (carried forward unchanged from the entry cycle; the
    remediation touches zero production files — `StoreIdentity.cs` 100.00%, `StoreDisableService.cs`
    97.92%, `DisabledStoreEntry` 100.00%, `StoreFilterAttribution.cs` 100.00%,
    `StoresWrapper.cs` 98.60% — all >= 90%). PASS.
- No regression on previously-covered lines: PASS. Test count unchanged at 5032 total across both
  runs; the same single pre-existing environment-dependent failure (see assessment above) is present
  identically before and after; the 13 test methods directly touched by the remediation (6 moved
  `InclusionFilters_*`, 5 moved disabled-store tests, 2 N1-fixed `ReenableAsync` guard tests) all
  report `Passed`.

**C# coverage verdict: PASS.**

## 2. General Code Change Policy (`CLAUDE.md`, `.claude/rules/general-code-change.md`)

| Area | Verdict | Evidence |
|---|---|---|
| Simplicity / separation of concerns | PASS | Unchanged from entry cycle; remediation is a pure test-file split plus an `async`/`await` correction, no design change. |
| File-size limit (500 lines) | **PASS (was FAIL)** | `StoresWrapperTests.cs` = 415 lines; `StoresWrapperDisableTests.cs` = 368 lines (both independently confirmed via `wc -l`). R1 fully resolved. |
| Module cohesion | PASS | The new file is scoped to the same `OutlookObjects/Store` test area and follows the same naming/namespace convention as its sibling. |
| Dependencies | PASS | No new external dependencies introduced by the remediation. |
| Public API compatibility | PASS (carried forward, non-blocking note) | Unchanged from entry cycle: `StoreFilterAttribution.Decide` and static `StoresWrapper.StoreIsIncluded` signature additions remain contained (verified no non-test caller exists). |

## 3. General Unit Test Policy (`.claude/rules/general-unit-test.md`, CLAUDE.md UT/CUT)

| Area | Verdict | Evidence |
|---|---|---|
| Independence / isolation | PASS | Each moved test method retains its own `Arrange` setup; no shared mutable state introduced by the split. |
| Determinism | PASS | No `Thread.Sleep`/`Task.Delay`/real timers introduced. |
| No temp files | PASS | Unchanged. |
| No external deps / live Outlook | PASS | Unchanged; Moq-based throughout. |
| Test file location | PASS | `StoresWrapperDisableTests.cs` lives in `UtilitiesCS.Test/OutlookObjects/Store/`, mirroring production and matching its sibling file; no colocation. |
| Effective assertion of async throw | **PASS (was PARTIAL/Non-blocking)** | `Writes_ThrowArgumentException_ForSentinelIdentity` and `Writes_ThrowInvalidOperation_WhenModelIsNull` are now `async Task` with `await`ed `ThrowAsync<...>()` calls; both report individually timed `Passed` results in the cycle-1 vstest run (`evidence/qa-gates/qa-08-n1-verification-cycle1.md`). N1 fully resolved. |
| Test-method count preserved | PASS | 22 `[TestMethod]` attributes in the pre-split file; 11 + 11 = 22 across the two post-split files. Zero deletions confirmed via normalized-content diff (see Independent Verification above). |

## 4. C# Code Change / Unit Test Policy (CLAUDE.md C#*, CUT*)

| Area | Verdict | Evidence |
|---|---|---|
| Formatting (csharpier) | PASS | `evidence/qa-gates/qa-01-format-cycle1.md`: `check .` reports 1284 files checked, 0 needing formatting, EXIT 0. |
| Analyzers | PASS | `evidence/qa-gates/qa-02-analyzers-cycle1.md`: build succeeded, 0 errors, 20 pre-existing warnings unrelated to the three touched files (confirmed via grep against the build log). |
| Nullable / TreatWarningsAsErrors | PASS | `evidence/qa-gates/qa-03-nullable-cycle1.md`: 0 warnings, 0 errors on the plan-specified incremental build; a diagnostic forced rebuild independently confirms pre-existing, out-of-scope nullable debt elsewhere in the solution is unrelated to the three touched files. |
| MSTest with coverage | PASS | `evidence/qa-gates/qa-04-mstest-cycle1.md`: 4410 tests in the two touched assemblies, 4409 passed, 1 pre-existing environment-dependent failure (assessed above, not attributable). All 13 directly-affected test methods passed. |
| Framework/libraries | PASS | MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions unchanged. |

## 5. File-Size Finding — Resolution Confirmed

- Rule: CLAUDE.md §4.1 "Do not exceed 500 lines for any one file";
  `.claude/rules/general-code-change.md` file-size limit.
- Prior state (entry cycle): `StoresWrapperTests.cs` at 688 lines — Blocking.
- Current state: `StoresWrapperTests.cs` = 415 lines; `StoresWrapperDisableTests.cs` = 368 lines
  (new file, wired into `UtilitiesCS.Test.csproj`). Both independently confirmed via `wc -l` by this
  reviewer.
- Scope discipline: the pre-existing 563-line baseline of `StoresWrapperTests.cs` (independent of
  F1) is out of scope for this feature and was not further remediated; the fix only addressed the
  F1-attributable overage, per the narrow remediation instruction. This is correct and consistent
  with the entry-cycle audit's documented scope boundary.
- **Disposition: RESOLVED. No longer a finding.**

## 6. Documented Deviations — Policy Dispositions (carried forward, unchanged)

1. **Interface member forces 7 test-double implementers** — Acceptable / PASS (unchanged from entry
   cycle; mechanically necessary consequence of adding `StoreDisable` to `IApplicationGlobals`).
2. **`Resolve(displayName, filePath)` instead of `Resolve(store)`** — Acceptable / PASS (unchanged;
   avoids a second blocking COM read).
3. **CSharpier v1 `check`/`format` subcommands** — Advisory (unchanged; correct equivalent for the
   pinned 1.2.6 tool).
4. **MSBuild/vstest full-path invocation instead of bare command** — Advisory (unchanged pattern from
   entry cycle; PATH-resolution workaround in this git-bash session, not a semantic change).

## Verdict Summary

- Blocking findings in this artifact: **0** (R1 resolved).
- Non-blocking findings in this artifact: **0** (N1 resolved).
- Advisory (non-gating): CSharpier v1 command form; MSBuild/vstest full-path invocation; the three
  Advisory code-quality items carried forward unchanged from the entry cycle (see code-review).
- Pre-existing, out-of-scope, environment-dependent finding (not counted as Blocking): the single
  live-Outlook COM integration-test failure, assessed above.
- Overall policy verdict: **PASS**.
