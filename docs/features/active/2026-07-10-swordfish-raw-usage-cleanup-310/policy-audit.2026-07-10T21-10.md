# Policy Audit — swordfish-raw-usage-cleanup (Issue #310, epic swordfish-removal child F4)

- Reviewed branch: `feature/swordfish-raw-usage-cleanup-310`
- Reviewed commit (HEAD): `97b0da88`
- Resolved base / merge-base: `origin/epic/swordfish-removal-integration` @ `0b72b11bb1145dd00f70fe9de8d7a6ed3bef79bb` (confirmed via `git merge-base HEAD origin/epic/swordfish-removal-integration`; this branch is exactly base + 1 commit)
- Work mode: `full-feature` (AC sources: `spec.md` AND `user-story.md`)
- Timestamp: 2026-07-10T21-10
- Reviewer: feature-review agent

## Executive Summary

Verdict: **PASS**. The branch delivers exactly the three disjoint, behavior-neutral C# edits
described in `spec.md`/`user-story.md`: a raw-collection type swap in `KbdActions.cs`, three
unused `using Swordfish.NET.Collections;` removals, and two dead trace-filter literal deletions
in `TraceUtility.cs`. Independent verification (branch diff inspection, consumer grep, merge-base
recomputation, and manual re-derivation of the coverage evidence) confirms the executor's claims.
No Blocking findings. One Moderate, non-blocking finding on evidence-labeling accuracy (see
`## 5. Test Coverage Detail`).

## Merge-Conflict Check (required)

- Command: `git fetch origin epic/swordfish-removal-integration` then `git merge-base HEAD origin/epic/swordfish-removal-integration`.
- Result: merge-base = `0b72b11bb1145dd00f70fe9de8d7a6ed3bef79bb`, which is identical to the current tip of `origin/epic/swordfish-removal-integration`. The feature branch is a direct, single-commit descendant of the current integration-branch tip (trivial fast-forward). `git merge-tree <merge-base> HEAD origin/epic/swordfish-removal-integration` produced no conflict markers.
- Verdict: **No merge conflict.** Not a Blocking finding.

## Rejected Scope Narrowing

No attempt to narrow the audit scope to a plan/task/phase subset, mark any language as
out-of-scope/informational, or skip a toolchain/coverage check was present in the caller prompt
for this review. This section is included for auditability; no entries to record.

## Evidence Location Compliance

- Diff scan: `git diff origin/epic/swordfish-removal-integration...HEAD --name-only | grep -iE "^artifacts/(baselines|qa|coverage|evidence)/"` returned no matches.
- All evidence for this feature is under the canonical `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/{baseline,qa-gates,regression-testing,other}/` tree, matching the plan's Evidence Location Contract.
- `validate_evidence_locations.py` is not present in this repository checkout (confirmed via `find`); the manual `git diff --name-only`-based scan above is the working substitute for this repo, consistent with prior review cycles.
- Verdict: **PASS.** No evidence-location violations.

## 1. General Unit Test Policy Compliance

- No new or modified unit tests are introduced by this branch (confirmed: `git diff --name-only` shows no file under `QuickFiler.Test/` or `UtilitiesCS.Test/`). This is consistent with the spec's Testing & Verification section, which states the existing `KbdActions` regression tests are the verification net and must pass unchanged.
- Existing regression tests (`QuickFiler.Test/Controllers/KbdActionsTests.cs`, `KbdActionsRemainingBranchesTests.cs`) were re-run against the swapped `List<UClass>` implementation and pass unchanged: 13/13 (`evidence/regression-testing/kbdactions-regression.2026-07-10T20-20.md`). Both test files are unmodified.
- Independence/isolation/determinism/AAA structure: not applicable to this branch — no test files changed. No violation.
- Test-file colocation and `tests/` mirror-structure rules: not applicable — no test files changed.
- Verdict: **PASS.**

## 2. General Code Change Policy Compliance

- Design principles (simplicity, reusability, separation of concerns): the `KbdActions._list` swap replaces a raw vendor type with a BCL type using the field's existing `using System.Collections.Generic;`; no new abstraction, shim, or indirection was introduced. Consistent with "simplicity first."
- File size limit (500 lines): all five changed files are well under the limit — `KbdActions.cs` 146, `KeyboardHandler.cs` 604 (pre-existing size, unaffected by a 1-line using removal — flagged below as a pre-existing observation, not introduced by this change), `FlagDetails.cs` 217, `FolderRemapController.cs` 283, `TraceUtility.cs` 399 lines. `KeyboardHandler.cs` at 604 lines exceeds the repo's 500-line guideline, but this branch only removes one `using` line from that file and does not add to its size; the file was already over the limit on the integration base. Not attributable to this change; noted as a pre-existing condition for future cleanup, not a Blocking finding here.
- Error handling / logging / contracts: unchanged in this branch; no new error paths introduced.
- Naming: unchanged; `_list` retains its existing name and visibility (`private`).
- Public API / compatibility: `KbdActions<TKey, UClass, VDelegate>` public constructor signatures, public method signatures, and `IEnumerable<UClass>` surface are unchanged (verified by reading the full post-change file). `_list` was already `private` before and after; no public member changed type.
- Dependencies: no new package or project dependency added; `using Swordfish.NET.Collections;` removed from four files total (`KbdActions.cs` + the three unused-using files), net dependency surface shrinks.
- I/O boundaries / temp files: not applicable — no I/O logic touched.
- Verdict: **PASS.**

## 3. C# Code Change Policy Compliance

- Toolchain order (format -> analyze -> nullable/type-check -> test) followed exactly, evidenced by baseline and final artifacts under `evidence/baseline/` and `evidence/qa-gates/` (see `## 6` below).
- CSharpier: baseline clean (`Checked 1378 files`, 0 reformatted); final pass also 0 files reformatted (`evidence/qa-gates/final-csharpier.2026-07-10T20-20.md`).
- .NET analyzers (`EnableNETAnalyzers=true`, `EnforceCodeStyleInBuild=true`): baseline and final builds both `EXIT_CODE: 0`, 0 errors; all warnings present in both baseline and final passes are pre-existing and not attributed to any of the five changed files (verified via targeted per-filename search of the build logs, per the phase-level evidence files).
- Nullable / `TreatWarningsAsErrors=true`: baseline and final builds both `EXIT_CODE: 0`, 0 warnings, 0 errors. The `List<UClass>` swap introduces no nullable-flow warning.
- Null-safety, composition, async/resource-safety, C# naming conventions: no new code introduced that would engage these rules; existing conventions in the five files are unchanged apart from the specified edits.
- Verdict: **PASS.**

## 4. C# Unit Test Policy Compliance

- Framework (MSTest), mocking (Moq), assertions (FluentAssertions): not engaged — no test files added or modified by this branch. The existing `KbdActions` regression tests (already MSTest-based, per the repo's established convention) were re-run unchanged and pass (13/13).
- No new test authored; per the spec's explicit rationale (all three work items are behavior-neutral: a like-for-like collection-type swap covered by existing tests, and two rebuild-provable dead-reference removals), this is consistent with UT policy — coverage for the swap is exercised by the pre-existing test suite rather than new tests.
- Verdict: **PASS.**

## 5. Test Coverage Detail

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/baseline/baseline-coverage-repository.xml` (Cobertura, numeric, genuine — not a placeholder; 4758/4758 tests passed).
- C# post-change coverage artifact: `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/qa-gates/final-coverage-repository.xml` (Cobertura, numeric, genuine; 4758/4758 tests passed).
- TypeScript baseline/post-change coverage artifact: no TypeScript files changed on this branch.
- PowerShell baseline/post-change coverage artifact: no PowerShell files changed on this branch.
- Python baseline/post-change coverage artifact: no Python files changed on this branch.

### Toolchain-command deviation (verified, not a violation)

The plan's literal task text specifies `vstest.console.exe <assemblies> /EnableCodeCoverage`. Two
attempts using that literal flag combination produced `No code coverage data available. Profiler
was not initialized.` and an empty Cobertura output (documented in
`evidence/baseline/baseline-mstest-coverage.2026-07-10T20-20.md`). The executor substituted the
repository's own proven recipe from `scripts/vscode/Invoke-MSTestWithCoverage.ps1`:
`dotnet-coverage collect --settings coverage.config --output-format cobertura -- vstest.console.exe
<assemblies> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation`, applied identically
in both the baseline (P0-T5) and final (P6-T4) passes, scoped to the same two plan-mandated
assemblies (`QuickFiler.Test.dll`, `UtilitiesCS.Test.dll`). This is a mechanically necessary
command correction (the plan-literal flag produces no usable data in this environment), not a
scope or methodology change: same assemblies, same settings file, applied consistently baseline vs
final so the delta comparison is apples-to-apples. Verdict: **not a policy violation.**

### C# — coverage verdict for changed/modified files (AC5 scope)

No new production files were added by this branch (all edits are in-place modifications to five
pre-existing files: a field/constructor re-typing and four `using`/literal deletions). Per-file,
independently re-derived from the raw Cobertura package/class data in
`evidence/qa-gates/final-coverage-repository.xml`:

| File / Class | Baseline line-rate | Post-change line-rate | Baseline branch-rate | Post-change branch-rate | Verdict |
|---|---|---|---|---|---|
| `KbdActions<TKey,UClass,VDelegate>` | 93.98% | 93.98% | 100% | 100% | PASS — identical; the retyped field/constructors remain fully exercised by the unmodified `KbdActions` regression tests. |
| `TraceUtility` | 90.07% | 90.00% | 80.77% | 80.77% | PASS — the -0.07pp line delta is fully explained by removing 2 previously-covered dead literal lines from both numerator and denominator; no previously-covered line is now uncovered. |
| `FlagDetails` | 100% | 100% | 95.83% | 95.83% | PASS — identical; only a non-executable `using` line was removed. |
| `FolderRemapController` | 87.5% | 87.5% | 73.81% | 73.81% | PASS on line coverage (>=85%); branch-rate is pre-existing at 73.81% (below the 75% uniform floor) but is unchanged before and after this branch's edit (a non-executable `using`-line removal touches no branch), so there is no regression attributable to this change. Flagged as a pre-existing gap for a future, separately scoped change — not a Blocking finding for F4. |
| `KeyboardHandler` | not a distinct Cobertura `<class>` entry (no measurable executable-line data) | same | n/a | n/a | PASS by rebuild-clean proof — the sole edit is a non-executable `using` removal; verified by a clean solution rebuild (`evidence/qa-gates/phase2-unused-using-build.2026-07-10T20-20.md`), which is the acceptance criterion the plan itself specifies for this file. |

No regression on changed lines: **PASS** (executor's `evidence/qa-gates/coverage-delta.2026-07-10T20-20.md` verdict independently reproduced above from the raw per-class Cobertura data).

### C#, coverage: PASS

C# has changed files on this branch (5 production `.cs` files). Coverage verdict for C#: **PASS**.
Basis: zero new production files were added, so the new-file >=90% gate does not apply; every
modified file's line coverage meets or exceeds the 85% floor except the two files whose sole edit
is a non-executable `using`-directive removal (`KeyboardHandler.cs`, verified by rebuild instead of
a line-coverage delta, per the plan's own acceptance criterion); no changed line regressed in
coverage relative to baseline in any of the five files, independently re-derived above from
`evidence/qa-gates/final-coverage-repository.xml` against
`evidence/baseline/baseline-coverage-repository.xml`.

Separately, the executor's evidence files label a 77.14% (baseline) / 77.12% (post-change) figure
as "Repo-wide." Independent inspection of the raw Cobertura packages in
`evidence/qa-gates/final-coverage-repository.xml` shows this figure is produced by a two-test-project
run (`QuickFiler.Test.dll`, `UtilitiesCS.Test.dll` only) whose transitive package set also includes
vendor NuGet code (`log4net` 6.08% line-rate, `Mono.Reflection` 39.30%, `System.Interactive` 2.73%,
`System.Linq.Async` 3.86%) and several sibling first-party assemblies whose own dedicated test
projects were not part of this run and therefore read 0% (`TaskMaster`, `TaskVisualization`,
`Tags`, `ToDoModel`). This is not a genuine solution-wide coverage measurement; it understates true
repository coverage by treating untested-in-this-run first-party assemblies as 0%-covered and by
including vendor-package lines in the denominator. This mislabeling is a Moderate, non-blocking
finding (see Findings below) — it does not change the AC5 verdict above, which rests on the
per-file no-regression comparison, not on the mislabeled aggregate figure. A true canonical
repo-wide C# figure was not independently reproducible in this review session (the full
multi-project local vstest/coverage run is a known local blocker in this repository per prior
review cycles — see project memory on the Moq binding-redirect issue for a full-assembly local
run); the authoritative repo-wide gate for this repository is the CI pipeline run, consistent with
prior review cycles in this repository.

## 6. Test Execution Metrics

| Gate | Baseline | Post-change | Evidence |
|---|---|---|---|
| CSharpier format check | Clean, 1378 files, 0 need reformatting | Clean, 1378 files, 0 reformatted | `evidence/baseline/baseline-csharpier.2026-07-10T20-20.md`, `evidence/qa-gates/final-csharpier.2026-07-10T20-20.md` |
| Analyzer build (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`) | Build succeeded, 76 warnings, 0 errors | Build succeeded, 0 new warnings/errors attributable to changed files | `evidence/baseline/baseline-analyzer-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-analyzer-build.2026-07-10T20-20.md` |
| Nullable / `TreatWarningsAsErrors` build | Build succeeded, 0 warnings, 0 errors | Build succeeded, 0 warnings, 0 errors | `evidence/baseline/baseline-nullable-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-nullable-build.2026-07-10T20-20.md` |
| MSTest (full 2-assembly set) | 4758/4758 passed | 4758/4758 passed | `evidence/baseline/baseline-mstest-coverage.2026-07-10T20-20.md`, `evidence/qa-gates/final-mstest-coverage.2026-07-10T20-20.md` |
| `KbdActions`-filtered regression tests | n/a (covered by full-suite run above) | 13/13 passed | `evidence/regression-testing/kbdactions-regression.2026-07-10T20-20.md` |

Verdict: **PASS.** Full toolchain green in a single pass at each gate check; no restart-from-step-1 cycles were required per the evidence trail (each phase build succeeded on first attempt).

## 7. Code Quality Checks

| Check | Command / Method | Result |
|---|---|---|
| Scope-boundary diff scan | `git diff origin/epic/swordfish-removal-integration...HEAD --name-only` and grep for Sco* lineage / `UtilitiesSwordfish` / `ProjectReference` / `TaskMaster.sln` | No matches — confirmed independently by this reviewer, not just by executor evidence. |
| Post-change Swordfish reference scan | `rg -n "Swordfish\.NET" <5 files>` | No matches (`EXIT_CODE 1`), confirmed independently. |
| Consumer-impact scan (public API check) | `grep -rn "KbdActions" --include="*.cs"` across the repo | Four non-test consumer files (`EfcFormController.cs`, `KbdActions.cs`, `KeyboardHandler.cs`, `QfcCollectionController.cs`, `IQfcKeyboardHandler.cs`); none reference `_list` directly or any Swordfish-specific member of it — consistent with "`_list` is private, no `CollectionChanged` exposure" claim in the decision record. |
| `.sln` / `.csproj` diff scan | `git diff origin/epic/swordfish-removal-integration...HEAD -- '*.sln' '*.csproj'` | Empty — confirms AC4's "TaskMaster.sln and ProjectReference entries untouched" claim. |

Verdict: **PASS.**

## Findings Summary

| Severity | Finding | Disposition |
|---|---|---|
| Moderate (non-blocking) | The coverage evidence files (`baseline-mstest-coverage.2026-07-10T20-20.md`, `final-mstest-coverage.2026-07-10T20-20.md`, `coverage-delta.2026-07-10T20-20.md`) label a two-test-project-scoped Cobertura aggregate as "Repo-wide," which understates true repository coverage by including vendor-package lines and untested-in-this-run sibling first-party assemblies in the denominator. Recommend relabeling as "Two-project aggregate (QuickFiler.Test + UtilitiesCS.Test transitive closure)" in future evidence to avoid confusion with a true solution-wide figure. Does not change the AC5 verdict, which rests on the per-file no-regression comparison. | Non-blocking; documentation-accuracy recommendation for future cycles. |
| Informational | `QuickFiler/Controllers/KeyboardHandler.cs` is 604 lines, over the repo's 500-line guideline. Pre-existing on the integration base; this branch only removes one line from it. | Not attributable to this change; no action required in this review. |

No Blocking findings.
