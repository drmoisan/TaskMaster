# Feature Audit — ribbon-controller-engines-null-unsafe (#507) — Remediation Cycle 1 Exit

Timestamp: 2026-08-08T19-10
Work Mode: `minor-audit`
AC Source: `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/issue.md`,
`## Acceptance Criteria` section only (AC1-AC6), per `minor-audit` work-mode routing.
`spec.md`/`user-story.md` are intentionally absent for `minor-audit` and are not treated as a
finding.

## Scope and Baseline

- Base: `main`, merge base `003c5715055d7d1933db68a742531332756e30b2`.
- Branch: `bug/ribbon-controller-engines-null-unsafe-507`, head `4fea8d6d` (advances cycle 1's head
  `e589fad7` by one remediation commit).
- Diff evaluated: `git diff 003c5715055d7d1933db68a742531332756e30b2...HEAD`.
- Production surface: unchanged since cycle 1 — one line in
  `TaskMaster/Ribbon/RibbonController.Intelligence.cs`. Test surface: the same two `[TestMethod]`s
  from cycle 1, now split across `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (unchanged tests)
  and the new `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` (moved tests), plus one
  `TaskMaster.Test.csproj` registration line. Remainder is feature-folder evidence/docs, agent-memory
  housekeeping, and one new promoted-issue doc (#518).

## Acceptance Criteria Inventory

| ID | Criterion (verbatim from `issue.md`) |
|---|---|
| AC1 | `RibbonController.Engines` returns `null` instead of throwing `NullReferenceException` when `Globals` has not been assigned (i.e. before `SetGlobals` has run). |
| AC2 | The change is confined to `TaskMaster/Ribbon/RibbonController.Intelligence.cs`; no other production file is modified. |
| AC3 | A deterministic MSTest regression test in `TaskMaster.Test` covers the unassigned-`Globals` case, fails against the pre-fix source, and passes after the fix. |
| AC4 | When `Globals` is assigned, `Engines` continues to return the value of `Globals.Engines` (no behavior regression for the assigned path). |
| AC5 | The full C# toolchain passes in a single clean pass, in order: `csharpier .`, msbuild with `EnableNETAnalyzers`/`EnforceCodeStyleInBuild`, the nullable gate as enforced by `.github/workflows/ci.yml`, and `vstest.console.exe` with `/EnableCodeCoverage`. |
| AC6 | No pre-existing test regresses; the MSTest pass/fail counts are no worse than the recorded Phase 0 baseline. |

## Acceptance Criteria Evaluation

### AC1 — PASS

Unchanged since cycle 1. `TaskMaster/Ribbon/RibbonController.Intelligence.cs` is byte-identical
between `e589fad7` and `4fea8d6d` (`git diff e589fad7 4fea8d6d -- TaskMaster/Ribbon/RibbonController.Intelligence.cs`
produces no output); line 204 still reads `internal IAppItemEngines Engines => Globals?.Engines;`.
The regression test `Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing`, now located in
`TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs`, is textually unchanged from cycle 1 and
was independently confirmed passing by name in the orchestrator's post-remediation toolchain run.

Caveat carried from cycle 1 (does not change the verdict): AC1 is scoped strictly to the property
boundary and is satisfied there. Whether the fix resolves the end-to-end reachable-crash scenario
for real callers is a separate question, tracked at #518 and dispositioned non-blocking for this PR
(see `policy-audit.2026-08-08T19-10.md` § 5).

### AC2 — PASS

`git diff --name-only 003c5715055d7d1933db68a742531332756e30b2...HEAD` (independently re-executed
this cycle) shows exactly one production `.cs`/`.csproj`/`.props`/`.targets` file touched:
`TaskMaster/Ribbon/RibbonController.Intelligence.cs`. The remediation commit added a new **test**
file (`RibbonControllerTests.Engines.cs`) and one **test-project** csproj entry, neither of which is
production code, and both of which serve AC3, not a violation of AC2.
`TaskMaster/Ribbon/RibbonViewer.cs` is confirmed absent from the diff, re-verified independently.

### AC3 — PASS

The regression test covering the unassigned-`Globals` case
(`Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing`) is unchanged in content from cycle 1
(only its file location changed, from `RibbonControllerTests.cs` to
`RibbonControllerTests.Engines.cs`, via a verbatim move confirmed in
`code-review.2026-08-08T19-10.md`). The pre-fix-fails / post-fix-passes evidence from cycle 1
(`evidence/regression-testing/phase1-expect-fail-engines-unassigned.md`,
`evidence/regression-testing/phase1-post-fix-engines-tests.md`) remains valid because the test's
content, not merely its file location, is what that evidence characterizes. The orchestrator's
post-remediation run confirms the test still passes by name after the file move.

### AC4 — PASS

Unchanged since cycle 1. `Globals?.Engines` still evaluates to `Globals.Engines` whenever `Globals`
is non-null. The second regression test, `Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines`
(unchanged content, moved to `RibbonControllerTests.Engines.cs`), still asserts reference equality
via `BeSameAs` against a distinguishable `Moq` instance and was independently confirmed passing by
name in the orchestrator's post-remediation run.

### AC5 — PASS

Re-verified via the orchestrator's post-remediation toolchain table (all four stages, single clean
pass, `EXIT_CODE=0` for each): `csharpier check .` (1489 files, 0 reformatted); `msbuild ...
/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (0 errors); `msbuild ... /t:Rebuild
/p:TreatWarningsAsErrors=true` matching `.github/workflows/ci.yml` (0 errors); `vstest.console.exe
<9 assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
(6295/6295 passed, 0 failed). The `CLAUDE.md`/`ci.yml` nullable-command divergence noted in cycle 1
remains a separate, pre-existing, informational finding and does not affect this verdict (see
`policy-audit.2026-08-08T19-10.md` § 2).

### AC6 — PASS

The orchestrator's post-remediation run (6295 total, 6295 passed, 0 failed) shows `total == passed`
and `failed == 0`, satisfying AC6's literal text. This audit reconciled the test-count figures across
both cycles (`policy-audit.2026-08-08T19-10.md` § 6): the filtered baseline-vs-final delta is exactly
+2, matching the two #507 regression tests, and is internally consistent with cycle 1's unfiltered
counts once the `TestCategory!=LiveOutlook` filter is accounted for. No test was lost from discovery
as a result of the `4fea8d6d` file split; the +2 delta and 0-failed count hold across every recorded
run in both cycles.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/issue.md`
- Total AC items: 6
- Checked off (delivered): 6 (AC1-AC6 remain checked `[x]` in `issue.md`, unchanged since cycle 1;
  this audit independently re-verified all 6 as PASS against the current head `4fea8d6d` and
  confirms the existing check-off state remains correct. No new check-offs were required.)
- Remaining (unchecked): 0
- Items remaining: none

## Findings Carried from Code Review / Policy Audit

Both Blocking findings from cycle 1 are resolved as of this cycle:

1. **File-size cap (B1)** — remediated. `RibbonControllerTests.cs` is 452 lines;
   `RibbonControllerTests.Engines.cs` is 73 lines. Both under the 500-line cap. Verified
   independently in `code-review.2026-08-08T19-10.md` and `policy-audit.2026-08-08T19-10.md` § 3.
2. **Unguarded call sites (B2)** — promoted to tracked issue #518 and dispositioned non-blocking for
   this PR, with independent concurrence recorded in `policy-audit.2026-08-08T19-10.md` § 5. This
   finding does not gate merge of #507.

**Total Blocking findings this cycle: 0.**

## Verdict

All 6 acceptance criteria PASS on their literal text, backed by re-verified evidence against the
current head. Both cycle-1 Blocking findings are resolved: one by direct remediation (file split),
one by legitimate scope-bounded promotion to a tracked follow-up issue that this audit independently
concurs should not block this PR. The feature is clear to merge from this review's perspective.
