# Feature Audit — Issue #278 (flaky-physicalfileinfoadapter-open-fileshare-none)

- Reviewed branch: `bug/flaky-physicalfileinfoadapter-open-278` @ `555d8be822b4fc583a31d4954cbd68160734c40c`
- Base: `main` @ `8e29dd403bd130b7902968bdbd142dffd9822e5a`
- Work mode: `minor-audit`
- Audit performed: 2026-07-08T10-57

## Scope and Baseline

Scope is the full branch diff against `main` at merge-base `8e29dd403bd130b7902968bdbd142dffd9822e5a` (re-derived via `git merge-base HEAD main`, matching the caller-supplied SHA — no drift). The diff touches 26 files total: the 2 in-scope C# files named in AC6, 5 `.claude/agent-memory/**` files, and 19 files under this feature's own `docs/features/active/.../` folder (issue.md, plan.md, plan.2026-07-08T06-18.md, 16 evidence artifacts). No file outside this set is changed. Baseline behavior before this change: `PhysicalFileInfoAdapter.Open(FileMode, FileAccess)` called `_fileInfo.Open(mode, access)` directly (real `FileShare.None` open), and the test at the-then line 207 exercised that overload against the real `TaskMaster.sln`, causing the intermittent CI `IOException` documented in `issue.md`.

## Acceptance Criteria Source

`minor-audit` work mode: the authoritative AC source is the `## Acceptance Criteria` section of `issue.md` (AC1–AC6). No `spec.md` or `user-story.md` exists in this feature folder, consistent with `minor-audit` mode.

## Acceptance Criteria Inventory

| ID | Criterion (summary) |
|---|---|
| AC1 | `Open(FileMode, FileAccess)` delegates through a new injectable seam field, bound by default to `_fileInfo.Open` in the public constructor; internal test-only constructor accepts the new delegate. |
| AC2 | The flaky test no longer opens the real `TaskMaster.sln` (or any real/shared file) with `FileShare.None`; the 2-arg `Open` delegation is verified via a test-owned sentinel stream through the seam. |
| AC3 | No temporary/scratch file is created or used by the test; sentinels are in-memory or read-only `FileShare.ReadWrite` opens. |
| AC4 | The test remains meaningful: it still asserts the 2-arg `Open` overload's delegation, preserving (>=) prior coverage of that production line. |
| AC5 | The full C# toolchain (CSharpier → analyzers → nullable/type-check → MSTest) passes in order with no new warnings on touched files, and coverage on changed lines does not regress. |
| AC6 | Scope is limited to the two named files; no unrelated files changed (with an explicit, disclosed note on `OpenRead()`/`OpenText()` being left unseamed). |

## Acceptance Criteria Evaluation

### AC1 — Injectable seam for `Open(FileMode, FileAccess)`

**Verdict: PASS**

`PhysicalFileInfoAdapter.cs` adds `private readonly Func<FileMode, FileAccess, FileStream> _openByModeAndAccess;` (line 21), binds it to `_fileInfo.Open` in the public constructor (line 29, an exact method-group reference — production behavior unchanged), and adds it as a null-guarded parameter to the internal test-only constructor (lines 36-37, 44-45). `Open(FileMode mode, FileAccess access)` now reads `=> _openByModeAndAccess(mode, access);` (line 141-142), replacing the prior direct `_fileInfo.Open(mode, access)` call. Independently confirmed by reading the file directly and by the analyzer/nullable/CSharpier re-runs in the policy audit, all clean on this file.

### AC2 — No real `FileShare.None` open remains in the test

**Verdict: PASS**

Grep of `PhysicalFileSystemAdapters_Tests.cs` for `FileShare.None` finds it only inside explanatory comments, never in executable code. The prior real-file call `adapter.Open(FileMode.Open, FileAccess.Read)` (the old line 207, confirmed via `evidence/baseline/line-anchors-baseline.2026-07-08T00-05.md`) has been removed entirely; the corresponding `openModeReadCanRead` local and its assertion are gone. The 2-arg `Open` overload is now exercised only via `seamAdapter.Open(FileMode.Open, FileAccess.Read).Should().BeSameAs(sentinelOpenModeAndAccessStream)` (lines 303-306), where `seamAdapter` is constructed through the internal seam constructor with a test-owned sentinel — no real file handle of any kind is acquired for this call. Independently reproduced: 3 consecutive local runs of the test all passed with no `IOException` (this review's own re-run, corroborating the executor's 5-run determinism evidence in `evidence/qa-gates/determinism-repeat-final.2026-07-08T01-10.md`).

### AC3 — No temporary/scratch file

**Verdict: PASS**

Grep of the test file for `GetTempPath|GetTempFileName|GetRandomFileName` returns no matches. The new sentinel (`sentinelOpenModeAndAccessStream`, lines 264-269) is a read-only `FileShare.ReadWrite` open of the test assembly's own DLL (`typeof(PhysicalFileSystemAdapters_Tests).Assembly.Location`), the same already-approved pattern used for the three pre-existing sentinels in this file.

### AC4 — Test remains meaningful; coverage of `Open(FileMode, FileAccess)` preserved

**Verdict: PASS**

The new assertion `seamAdapter.Open(FileMode.Open, FileAccess.Read).Should().BeSameAs(sentinelOpenModeAndAccessStream)` (lines 303-306) directly proves the seam delegation. Independently verified via a Cobertura conversion of the executor's own post-change `.coverage` output (full-suite run, `TestResults/1f67cdaa-.../...06_38_34.coverage`): the `Open(FileMode, FileAccess)` delegation line (line 142) shows `hits="1"`, and the pre-fix baseline (`evidence/baseline/mstest-targeted-baseline.2026-07-08T00-30.md`) reported the same line at 100% coverage before the change. Coverage of this line is preserved, not reduced.

### AC5 — Full toolchain passes in order; no new warnings; no changed-line coverage regression

**Verdict: PASS**

Independently re-run by this review (not solely accepted from the executor's evidence):
1. CSharpier check on both files: exit 0, "Checked 2 files in 509ms."
2. Analyzer build (`EnableNETAnalyzers=true`/`EnforceCodeStyleInBuild=true`): exit 0, no diagnostics referencing either file.
3. Nullable/type-check build (`Nullable=enable`/`TreatWarningsAsErrors=true`): exit 0, no diagnostics referencing either file.
4. MSTest targeted run (3 consecutive executions): all passed, no `IOException`.

Changed-line coverage independently verified at 100% (4/4 new/changed executable lines hit >= 1 — see policy-audit Section 6). Repo-wide first-party `UtilitiesCS` line coverage independently verified at 88.15% (corroborating the executor's own 86.02% figure from a different coverage-export tool), both clearing CLAUDE.md's 80% floor.

### AC6 — Scope limited to the two named files

**Verdict: PASS**

`git diff main...HEAD --name-only -- '*.cs'` returns exactly the two named files. No other production or test source file is changed. The 5 `.claude/agent-memory/**` and 19 `docs/features/active/.../**` files in the branch diff are standard delivery-workflow artifacts (memory notes and this feature's own plan/evidence), not unrelated production or test code, consistent with the intent of AC6. The disclosed exception (leaving `OpenRead()`/`OpenText()` unseamed) is honored: those two methods are structurally unchanged in this diff; only their surrounding test comment was updated to explain why they remain unseamed.

## Acceptance Criteria Check-off

All six items were already checked off (`- [x]`) in `issue.md` prior to this review (recorded by the executor session per `evidence/issue-updates/issue-278.2026-07-08T01-40.md`). This review independently re-verified each item against the current branch diff and toolchain state (see evaluations above) and confirms all six check-offs are warranted; no change to `issue.md` was required.

## Summary

The fix precisely targets the documented defect: `PhysicalFileInfoAdapter.Open(FileMode, FileAccess)` now delegates through an injectable seam (mirroring the class's existing seam pattern for three sibling members), and the previously-flaky test no longer acquires a real `FileShare.None` handle on the shared `TaskMaster.sln`, verified instead through a test-owned sentinel stream. All six acceptance criteria are met, independently re-verified by this review through direct toolchain re-execution and an independent coverage-artifact conversion rather than by trusting the executor's evidence narrative alone. No regressions, no scope creep, and no unresolved acceptance criteria.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-08-flaky-physicalfileinfoadapter-open-fileshare-none-278/issue.md` (`## Acceptance Criteria` section)
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none

## Verdict: PASS — all acceptance criteria met
