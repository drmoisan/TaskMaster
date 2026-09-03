# Feature Audit — issue #733 (coverage-cobertura-mstest-powershell-tooling-defects)

- Timestamp: 2026-09-02T23-49
- Work Mode: `full-bug` (from issue.md line 12). AC source is `spec.md` **only**.
- AC section: `spec.md` `## Acceptance Criteria`, heading at line 172, 8 checkbox items at lines 173-180.
- Base: origin/main @ 8be5a6aacb (merge base recomputed). Head: 6c9329a3.
- `user-story.md` does not exist and is not required for `full-bug`.

The checkbox list at `spec.md` lines 155-158 is the Test Strategy scope list, not acceptance
criteria. It was correctly left untouched by the executor and is left untouched by this review.

## Method

Each AC was evaluated against evidence the reviewer re-derived, not against the executor's account.
Independent measurements performed for this audit:

- `git merge-base origin/main HEAD` and `git rev-parse HEAD`, both matching the caller's values.
- `git diff origin/main...HEAD --name-only` recounted to 63 paths and bucketed by prefix.
- Full `Invoke-Pester` run over `tests/scripts/vscode` with code coverage on the six changed
  production files.
- Each of the 10 test files run individually, in reverse-alphabetical order.
- `Invoke-ScriptAnalyzer` over both scan folders.
- Line counts of every `.ps1` in both folders via `[System.IO.File]::ReadAllLines().Length`.
- PowerShell return-enumeration and `Set-StrictMode -Version Latest` scalar-`.Count` semantics
  reproduced in a clean `pwsh -NoProfile` session.
- Filesystem mtimes of the three uncommitted `.claude/agent-memory/orchestrator/` paths.

## Baseline Comparison

| Metric | Baseline (origin/main, P0-T7) | Head (reviewer-measured) | Direction |
|---|---|---|---|
| Tests passing, whole folder | 70 | 92 | +22 |
| Tests failing | 0 | 0 | unchanged |
| Tests skipped | 0 | 0 | unchanged |
| PSScriptAnalyzer diagnostics, both folders | 16 (13 Warning, 3 Information, 0 Error) | 16, set-identical modulo line shift | unchanged |
| Aggregate command coverage | 90.42% over 522 commands in 4 files | 93.10% over 565 commands in 6 files | up |
| Production files below the 85% floor | 1 (Invoke-MSTest.ps1 at 68.89%) | 0 | improved |
| Files over the 500-line ceiling | 0 | 0 | unchanged |

## Acceptance Criteria Evaluation

### AC1 (spec.md line 173) — "Repro steps now produce the expected behavior in all documented environments."

**Verdict: PASS.** Already checked; the check-off is supported.

Findings with a behavioral repro are 1, 2, 3 and 7. Each is now pinned by a test that the reviewer
confirmed failing pre-fix in a recorded expect-fail run and passing at head:

| Finding | Expected behavior at head | Reviewer confirmation |
|---|---|---|
| 1 | `<package>` `line-rate` and `branch-rate` recomputed after a class merge | Helpers.ps1 lines 397-401 read; assertion `//package` `line-rate` = `'0.6'` passes; `case-03` records the stale `'0'` pre-fix |
| 2 | `<methods>` unioned across the merge group, not cloned from the primary alone | Helpers.ps1 lines 299-307 read; `M,N` and `M,N,O` assertions pass; `case-04` and `case-05` record the pre-fix single-method result |
| 3 | `.claude` paths excluded from assembly discovery | Invoke-MSTestWithCoverage.ps1 line 301 read; captured `-TestAssembly` contains only the ordinary path; `case-07` records both paths pre-fix |
| 7 | Discovery returns an array at zero, one and many matches, safe under StrictMode | Reproduced independently: `return @('x')` yields a `String` and `'x'.Count` throws `PropertyNotFoundException` under StrictMode Latest; `return , @('x')` yields a 1-element array. Five passing cases at head |

Findings 4, 5 and 6 have no behavioral repro by the spec's own corrected scope: finding 4 was
already correct and gained a test only; findings 5 and 6 are documentation-only per the Root Cause
Analysis correction. AC1 is therefore satisfiable without a behavior change for them.

Qualification recorded: "all documented environments" resolves to a single environment. `spec.md`
Environment names Windows 11 Pro with PowerShell 7+. Verification was performed on Windows 11 Pro
with PowerShell 7 and Pester 5.6.1. No second documented environment exists to verify against.

### AC2 (spec.md line 174) — "Regression test(s) added and passing (list file path and test name)."

**Verdict: PASS.** Already checked; the check-off is supported.

The list is enumerated in
`evidence/qa-gates/acceptance-criteria-status.2026-09-02T23-11.md` with 24 rows (22 new, 2
updated). The reviewer reconciled the arithmetic independently: baseline 70 minus the 8 belonging
to the two out-of-scope files gives an in-scope baseline of 62; head in-scope is 84; net +22.
Per-file at head, reviewer-measured: PackageRate 2, Merge 2, Threshold 5, Helpers 20,
ClosureFilter 12, RunSettings 27, AssemblyDiscovery 5, Main 11 = 84. The Helpers-to-Threshold split
is count-neutral (25 becomes 20 plus 5), which the reviewer verified against the baseline figure of
25.

Every listed test is present in the tree and passes both in the full run and in isolation.

### AC3 (spec.md line 175) — "Edge cases and invalid inputs are handled with correct errors or fallbacks."

**Verdict: PASS.** Already checked; the check-off is supported.

All three cited items were read and confirmed:

1. Zero-denominator fallback — `PackageRate.Tests.ps1` "falls back to a zero rate when no class in
   the package carries any lines". The fixture's own `line-rate="0.5"` and `branch-rate="0.25"` are
   deliberately non-zero, so a returned `'0'` cannot be an echo of the input. That is a well-built
   fixture, not a token one.
2. Zero-match and multiple-match discovery — `AssemblyDiscovery.Tests.ps1`, plus the two
   shape-reading cases that make the zero case non-vacuous.
3. Fail-safe under-exclusion direction — `ClosureFilter.Tests.ps1` "retains a closure whose bare
   member name collides with a non-exempt overload", asserting `LinesValid` = `'2'` and
   `LinesCovered` = `'1'`, which pins both the retention and the denominator effect.

Additional negative paths verified beyond those cited: four `Should -Throw -ExpectedMessage` cases
in `Main.Tests.ps1` covering missing search root, missing `vswhere.exe`, unresolved
`vstest.console.exe`, and empty discovery.

### AC4 (spec.md line 176) — "No unintended behavior changes outside the defined scope."

**Verdict: PASS. Newly checked off by this review** in `spec.md` line 176 and, correspondingly,
plan task P5-T9.

The executor left this unchecked because the plan's literal P5-T9 mechanism — a repository-root
`git status --porcelain` requiring every reported path to fall under one of three prefixes —
reports three `.claude/agent-memory/orchestrator/` paths. The reviewer ruled on the substantive
criterion, which is what the AC text states, using evidence re-derived rather than accepted.

**Command 1, anchored footprint, reviewer-run.** `git diff origin/main...HEAD --name-only` returns
63 paths. Recounted by prefix:

| Prefix | Paths |
|---|---|
| docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/ | 49 |
| scripts/vscode/ | 6 |
| tests/scripts/vscode/ | 8 |
| **outside all three** | **0** |

The three-dot degeneration was checked: `origin/main` is an ancestor of HEAD because the branch
merged origin/main at 357b5770, so `A...B` and `A..B` select the same range. That is benign,
because origin/main at 8be5a6aa already contains every merged sibling commit, so merged sibling
content appears on both sides of the comparison and is excluded. The footprint is this item's own
work only.

This check is capable of failing. Had any commit on this branch touched a path outside the three
prefixes, it would appear in the 63 and be counted in the "outside all three" row.

**Command 2, porcelain status, reviewer-run.** Four paths are reported at review time:

```
 M .claude/agent-memory/orchestrator/MEMORY.md
?? .claude/agent-memory/orchestrator/powershell-change-budget-override-for-consolidated-issue.md
?? .claude/agent-memory/orchestrator/pwsh-blanket-blocked-in-isolated-worktree-for-orchestrator.md
?? docs/features/active/2026-09-02-.../evidence/qa-gates/ac4-scope-boundary-anchored-diff.2026-09-03T01-40.md
```

The fourth is under an allowed prefix. The first three are the ones at issue.

**The reviewer verified the "predates this item's work" claim rather than accepting it.**
Filesystem mtimes:

| Path | Last written |
|---|---|
| .claude/agent-memory/orchestrator/MEMORY.md | 2026-09-02 12:54:40 |
| .../powershell-change-budget-override-for-consolidated-issue.md | 2026-09-02 12:54:28 |
| .../pwsh-blanket-blocked-in-isolated-worktree-for-orchestrator.md | 2026-09-02 12:54:06 |

against the item's own work:

| Marker | Time |
|---|---|
| Phase 0 baseline capture | 2026-09-02 21:50 |
| First implementation edit (PackageRate.ps1 created) | 2026-09-02 22:20:52 |
| Last production edit (Invoke-MSTest.ps1) | 2026-09-02 23:21:23 |
| Implementation commit 6c9329a3 | 2026-09-02 23:34:24 |

All three memory files were last written **nearly nine hours before** the first implementation edit
and more than eight hours before the Phase 0 baseline. They were not touched by this item's work.
They are also orchestrator-owned agent memory, which every executor on this run was prohibited from
writing to, and removing or reverting them would itself be the out-of-scope action AC4 exists to
prevent.

**Ruling.** The criterion is "No unintended behavior changes outside the defined scope." The
committed footprint contains zero paths outside the defined scope, proven by a check that could
have failed. The three porcelain-reported paths are uncommitted, third-party, and demonstrably
untouched, so they are neither behavior changes nor changes by this item. AC4 is satisfied.

The plan's P5-T9 mechanism was a reasonable proxy when authored, under a state where nothing had
been committed; it became the weaker of the two available checks once the commit existed. Ruling on
the criterion rather than the proxy is the correct disposition, and the substituted check is
strictly stronger, not weaker.

Two evidence defects attach to this AC and are recorded in the policy audit as PA-3, PA-4 and PA-5:
the supporting artifact is uncommitted, its filename timestamp is future-dated by 124 minutes, and
its prefix distribution (51/6/6) is wrong against the reviewer's recount (49/6/8). None affects the
ruling; the load-bearing claim was independently reproduced.

### AC5 (spec.md line 177) — "Required logs/telemetry updated and validated (if applicable)."

**Verdict: PASS, Not Applicable.** Already checked; the check-off is supported.

`spec.md` line 149 states "Logging/telemetry updates (if any): None." The reviewer verified the
stronger claim behind the check-off: the two `Write-Host` calls in `Invoke-MSTest.ps1` are the only
log statements anywhere in the changed production surface, and their message text is byte-identical
to the base branch (`"Using vstest.console: $vstestPath"` and
`"Discovered $($testAssemblies.Count) test assemblies."`), moved from lines 119-120 to 185-186 only
because the enclosing body was relocated. No log statement was added, removed, or reworded.

### AC6 (spec.md line 178) — "Performance constraints met or explicitly waived with rationale."

**Verdict: PASS, explicitly waived.** Already checked; the check-off is supported.

Citation `spec.md` lines 132-133 ("N/A — no latency/throughput/memory constraint applies"). The
reviewer verified the supporting rationale by reading: `Get-CoberturaPackageLineSummary` traverses
`.//class` nodes the caller already walked, and `Get-CoberturaCoverageSummary` was refactored to
delegate to it rather than add a second traversal, so the document-level pass does not double its
work. The `.claude` clause adds one regex test to an existing predicate. `Get-MSTestAssemblyPathList`
and `Invoke-MSTestMain` relocate existing statements without adding operations. Measured suite
duration moved from 15.78s over 70 tests to roughly 19s over 92 tests in the reviewer's run,
consistent with 22 additional cases and two additional files in the coverage denominator.

### AC7 (spec.md line 179) — "Full toolchain pass completed (format → lint → type-check → test)."

**Verdict: PASS.** Already checked; the check-off is supported, with one disclosure carried
forward.

| Step | Final-iteration artifact | Reviewer confirmation |
|---|---|---|
| Format | evidence/qa-gates/poshqc-format.iter3.2026-09-02T23-23.md | 21 of 21 SHA-256 hashes identical before and after; the artifact's line-count table matches the reviewer's independent measurement of all 14 footprint files exactly |
| Lint | evidence/qa-gates/poshqc-analyze.iter3.2026-09-02T23-25.md | Reviewer re-ran `Invoke-ScriptAnalyzer`: 16 diagnostics, 13 Warning + 3 Information, **0 Error**, set-identical to the P0-T6 baseline modulo line-number shift. Zero new |
| Type-check | not applicable | Correct per `.claude/rules/powershell.md` line 17 |
| Test | evidence/qa-gates/poshqc-test.iter3.2026-09-02T23-27.md | Reviewer re-ran: 92 passed, 0 failed, 0 skipped |

Loop discipline verified: three iterations ran; iteration 3 (format 23-23, analyze 23-25, test
23-27) changed no file and failed no step, so termination was correct rather than premature.

Disclosure carried forward from the executor's own record: the lint MCP tool exits 1 on any
non-empty diagnostic set at any severity, so it exits 1 at baseline and at head alike. The gate
signal relied on is the per-file diagnostic set comparison, not the exit code. The uniform gate in
`.claude/rules/quality-tiers.md` is "Lint errors: 0", and the Error count is 0 at head. This
check-off is not stronger than that evidence.

### AC8 (spec.md line 180) — "Docs/config references updated to match the new behavior."

**Verdict: PASS.** Already checked; the check-off is supported.

All three cited locations were read at head and are present:

1. `ClosureFilter.ps1` `.DESCRIPTION`, paragraph beginning "That non-admission is an asserted
   design choice rather than a measured one (issue #733 finding 5)". Names the revisit trigger.
2. Same block, paragraph beginning "Known limitation, bare-name overload collision (issue #733
   finding 6)". Names both failure directions explicitly and the reason a re-key is not proposed,
   which was P3-T2's stated acceptance.
3. `Helpers.ps1` lines 373-376, replacing the stale "the spec specifies exactly one new helper"
   comment with an accurate explanation of why the merged class rate stays inline.

Config: `coverage.config`, `TaskMaster.runsettings` and `scripts/vscode/TaskMaster.cli.runsettings`
are absent from the branch diff, so the "no config change" determination is verified against the
diff rather than against a status snapshot. The relocated
`Assert-CoberturaLineCoverageThreshold` retains the literal `80` threshold and every `throw`
message unchanged, so no documented threshold reference went stale.

## Spec Deviations Assessed

| Deviation | Assessment |
|---|---|
| Plan P4-T4 specified `return @(...)`; delivered `return , @(...)` | **Warranted and required.** Reproduced independently: the plan's literal form would not have fixed finding 7, because a function return enumerates its output and unwraps the array again. Documented in the function's `.DESCRIPTION` and measured in `case-10` |
| Spec named 4 production files; 6 delivered | **Justified.** PackageRate.ps1 and Threshold.ps1 are mechanical consequences of the 500-line ceiling on Helpers.ps1 (492 baseline, 502 after Phase 1, 469 after extraction; all three figures re-measured). Authorized in-band by plan task P1-T14's acceptance text |
| Spec named 3 test files; 8 delivered | **Justified.** Merge.Tests.ps1 and Threshold.Tests.ps1 are ceiling-driven splits of Helpers.Tests.ps1 (498 baseline, 566 after Phase 1, 494 after extraction). AssemblyDiscovery.Tests.ps1 was a conditional in the plan and its condition fired (RunSettings.Tests.ps1 at 488). Main.Tests.ps1 exists because `Invoke-MSTest.ps1` was already below the 85% floor at baseline (68.89%) and the Coverage Exclusion Policy forbids excluding it |
| `Invoke-MSTestMain` and `Get-VsTestConsolePath` extraction, not in the plan | **Justified but discretionary.** Required by the plan's own P5-T5 criterion (d) once the file is in the write set; the remedy applied is the one `.claude/rules/general-unit-test.md` prescribes. Confined to one file. Recorded as PA-1 because the change-budget override record was not amended |
| Tasks H1 and R1 executed with no plan checkbox | **Recorded, non-blocking.** Both are documented in evidence with their rationale, but the plan file was never amended to carry them, so the plan no longer describes the delivered work |

## Acceptance Criteria Status

- Source: `docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/spec.md` (lines 173-180)
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

Newly checked off by this review: AC4, "No unintended behavior changes outside the defined scope."
(`spec.md` line 176), with the corresponding plan task P5-T9. The seven previously checked items
were each re-verified against the evidence they cite; none was found to be an unsupported
check-off.

## Verdict

**PASS.** 8 of 8 acceptance criteria satisfied. Zero blocking findings. Five non-blocking
procedural findings are recorded in `policy-audit.2026-09-02T23-49.md` (PA-1 through PA-5) and
eight advisory code findings in `code-review.2026-09-02T23-49.md` (CR-1 through CR-8). No
remediation-inputs artifact is produced, because no finding requires a code change before merge.
