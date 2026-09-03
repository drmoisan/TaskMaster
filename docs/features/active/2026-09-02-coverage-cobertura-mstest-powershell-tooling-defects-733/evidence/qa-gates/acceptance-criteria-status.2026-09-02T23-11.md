# P5-T6 through P5-T13 — Acceptance criteria check-off

Timestamp: 2026-09-02T23-11

Work Mode: full-bug. AC source: `docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/spec.md`, sole source, `## Acceptance Criteria` section at lines 172-180 (8 checkbox items, lines 173-180).

The checkbox list at spec.md lines 155-158 is the Test Strategy / Proposed Fix scope list, not the
Acceptance Criteria section, and was not modified.

---

## [P5-T6] AC1 — "Repro steps now produce the expected behavior in all documented environments."

Verdict: **PASS — checked off** (spec.md line 173).

Cited evidence, all four pass-after artifacts:

| Artifact | Finding | Recorded outcome |
|---|---|---|
| evidence/regression-testing/pass-after-phase1.2026-09-02T22-37.md | findings 1, 2, 4 | "All six It cases added or updated across P1-T1 through P1-T6 pass... Passed 29, Failed 0, Skipped 0, with direct-run EXIT_CODE 0" |
| evidence/regression-testing/pass-after-phase2.2026-09-02T22-40.md | finding 3 | "The P2-T1 regression test now passes against the P2-T3 production change, and the captured `-TestAssembly` array contains only the ordinary path" |
| evidence/regression-testing/pass-after-phase3.2026-09-02T22-42.md | findings 5, 6 | "The P3-T3 pinning test passes, and no test in the file regressed relative to the P0-T7 baseline: 11 baseline tests passing before, 12 passing now" |
| evidence/regression-testing/pass-after-phase4.2026-09-02T22-52.md | finding 7 | "All three P4-T2 cases pass... The exactly-one-match case's returned array Count is 1 and the returned value is a real array at every cardinality" |

Expected behavior now holding, per finding:

- Finding 1 — `Merge-CoberturaClassesByFilename` recomputes the enclosing `<package>` node's
  `line-rate` and `branch-rate` after a class merge instead of leaving the input document's stale
  value. Pinned by the extended assertions in "computes the merged per-file line-rate from the
  merged rollup alone".
- Finding 2 — the merge unions `<methods>` entries across every group member instead of cloning
  only the primary class's. Pinned by the reversed assertion in "preserves the primary class
  methods subtree and every hits value when merging" and by the new three-way fixture.
- Finding 3 — assembly discovery in `Invoke-MSTestWithCoverageMain` excludes paths under a
  `.claude` segment. Pinned by "excludes assemblies discovered under a .claude worktree segment".
- Finding 7 — `Get-MSTestAssemblyPathList` returns an array at zero, one, and many matches, so
  downstream member access is safe under `Set-StrictMode -Version Latest`. Pinned by the three
  P4-T2 cases and, discriminatingly, by the two task-H1 cases recorded in
  evidence/regression-testing/case-10-assembly-discovery-array-shape-discriminating.2026-09-02T22-57.md.

Findings 5 and 6 are documentation-only by spec.md's corrected scope and carry no behavior repro;
their current behavior is pinned by the P3-T3 test. Finding 4 was already correct and is now
covered by a focused test.

All 84 tests across the 8 write-set test files pass in the final QC run
(evidence/qa-gates/poshqc-test.iter3.2026-09-02T23-27.md), so no repro regressed. Finding 7's
`Get-MSTestAssemblyPathList` is unchanged in text and behavior by the iteration-3 remediation, and
its zero, one, and many cardinality cases plus the two task-H1 shape assertions are all recorded
passing on that run.

---

## [P5-T7] AC2 — "Regression test(s) added and passing (list file path and test name)."

Verdict: **PASS — checked off** (spec.md line 174).

Every new or updated test across P1-T1 through P1-T6, P2-T1, P3-T3, and P4-T2, individually:

| # | Task | File path | It description | Status |
|---|---|---|---|---|
| 1 | P1-T1 | tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | accumulates line and branch totals across every class in the package | new, passing |
| 2 | P1-T2 | tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | falls back to a zero rate when no class in the package carries any lines | new, passing |
| 3 | P1-T3 | tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | computes the merged per-file line-rate from the merged rollup alone | updated (package line-rate and branch-rate assertions added), passing |
| 4 | P1-T4 | tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | preserves the primary class methods subtree and every hits value when merging | updated (assertion reversed to union-merge), passing |
| 5 | P1-T5 | tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | unions the methods of every group member into the merged class | new, passing |
| 6 | P1-T6 | tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | takes the higher hits value when the second class seen for a filename is strictly higher | new, passing |
| 7 | P2-T1 | tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | excludes assemblies discovered under a .claude worktree segment | new, passing |
| 8 | P3-T3 | tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | retains a closure whose bare member name collides with a non-exempt overload | new, passing |
| 9 | P4-T2 | tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns an empty array when discovery matches nothing | new, passing |
| 10 | P4-T2 | tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns a single-element array when discovery matches exactly one assembly | new, passing |
| 11 | P4-T2 | tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns every match when discovery matches multiple assemblies | new, passing |

Two further tests were added by orchestrator-directed task H1, outside the numbered plan, and are
also passing:

| # | Task | File path | It description | Status |
|---|---|---|---|---|
| 12 | H1 | tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns a value that is itself an array when discovery matches exactly one assembly | new, passing |
| 13 | H1 | tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns a value that is itself an array when discovery matches nothing | new, passing |

Eleven further tests were added by orchestrator-directed remediation task R1, also outside the
numbered plan, when `scripts/vscode/Invoke-MSTest.ps1`'s entry-point body was extracted into
`Invoke-MSTestMain` to close the P5-T5 criterion (d) coverage gap. All eleven are passing:

| # | Task | File path | It description | Status |
|---|---|---|---|---|
| 14 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | returns the off-root CLI runsettings path alongside the script directory | new, passing |
| 15 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | fails fast with a specific error naming the missing runsettings path | new, passing |
| 16 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | forwards every argument array element as a separate positional argument | new, passing |
| 17 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | fails when the search root cannot be found | new, passing |
| 18 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | fails when vswhere.exe is not installed | new, passing |
| 19 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | fails when vswhere resolves no vstest.console.exe | new, passing |
| 20 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | fails when discovery finds no test assemblies, naming the search root and configuration | new, passing |
| 21 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | returns before launching vstest.console.exe when NoExecute is supplied | new, passing |
| 22 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | launches vstest.console.exe with the discovered assemblies and the resolved runsettings | new, passing |
| 23 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | defaults the search root to the repository root and the configuration to Debug | new, passing |
| 24 | R1 | tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | throws naming the exit code when vstest.console.exe returns a nonzero status | new, passing |

Each is confirmed passing by its corresponding pass-after task: entries 1-6 by P1-T13, entry 7 by
P2-T4, entry 8 by P3-T4, entries 9-11 by P4-T6, entries 12-13 by the H1 two-run record, and
entries 14-24 by the iteration-3 final QC run. All 24 are additionally confirmed passing in that
run (84 passed, 0 failed, 0 skipped). No test in the list is omitted.

---

## [P5-T8] AC3 — "Edge cases and invalid inputs are handled with correct errors or fallbacks."

Verdict: **PASS — checked off** (spec.md line 175).

All three cited items:

1. **Zero-denominator fallback (P1-T2)** —
   `tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`, It "falls back to a
   zero rate when no class in the package carries any lines". A `<package>` whose classes carry no
   `<lines>` elements yields `LineRate` and `BranchRate` of the string `'0'` rather than a
   divide-by-zero, matching `Get-CoberturaCoverageSummary`'s existing convention. Evidence:
   evidence/regression-testing/case-02-package-summary-zero-denominator.2026-09-02T22-17.md.
2. **Zero-match and multiple-match discovery cases (P4-T2)** —
   `tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1`, Its "returns an empty array
   when discovery matches nothing" and "returns every match when discovery matches multiple
   assemblies". The zero and many cardinality boundaries return an array without throwing under
   `Set-StrictMode -Version Latest`. Evidence:
   evidence/regression-testing/case-09-assembly-discovery-array-safety.2026-09-02T22-45.md and
   evidence/regression-testing/pass-after-phase4.2026-09-02T22-52.md.
3. **Fail-safe under-exclusion direction (P3-T3)** —
   `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`, It "retains a closure
   whose bare member name collides with a non-exempt overload". The overload-collision case
   resolves in the safe under-exclusion direction (lines retained in the denominator) rather than
   the forbidden over-exclusion direction (lines deleted). Evidence:
   evidence/regression-testing/case-08-overload-collision-pin.2026-09-02T22-41.md.

---

## [P5-T9] AC4 — "No unintended behavior changes outside the defined scope."

Verdict: **NOT MET — left unchecked** (spec.md line 176).

### Verbatim `git status --porcelain` at the repository root

Run unscoped from the repository root, so it surfaces every staged, unstaged, and untracked path
anywhere in the tree. No task in this plan stages or commits, so an anchored `git diff` against a
ref would report nothing regardless of what was touched; porcelain status is what makes this gate
able to fail.

```
 M .claude/agent-memory/orchestrator/MEMORY.md
 M docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/plan.2026-09-02T12-01.md
 M scripts/vscode/Invoke-MSTest.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
?? .claude/agent-memory/orchestrator/powershell-change-budget-override-for-consolidated-issue.md
?? .claude/agent-memory/orchestrator/pwsh-blanket-blocked-in-isolated-worktree-for-orchestrator.md
?? docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/evidence/
?? scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1
?? scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
?? tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1
```

The `?? docs/.../evidence/` entry is a collapsed untracked directory, so this artifact and the
other Phase 5 artifacts written after the capture do not add new lines to it. The plan file was
already listed as modified before the Phase 5 checkbox updates, so those do not add a line either.

### Prefix evaluation

Allowed prefixes per the task: `scripts/vscode/`, `tests/scripts/vscode/`, and
`docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/`.

15 of the 18 reported paths fall under an allowed prefix: 6 under `scripts/vscode/`, 7 under
`tests/scripts/vscode/`, and 2 under the feature folder.

Three paths fall **outside** all three allowed prefixes:

| Path | State | Owner |
|---|---|---|
| .claude/agent-memory/orchestrator/MEMORY.md | modified | orchestrator |
| .claude/agent-memory/orchestrator/powershell-change-budget-override-for-consolidated-issue.md | untracked | orchestrator |
| .claude/agent-memory/orchestrator/pwsh-blanket-blocked-in-isolated-worktree-for-orchestrator.md | untracked | orchestrator |

The task's acceptance states that any path outside those three prefixes fails this task's
acceptance. It does, so this AC is left unchecked.

Facts about the three paths, recorded so the gap can be judged rather than merely reported:

- All three were already present in the worktree before this executor made any Phase 5 edit. The
  first `git status --porcelain` of this session, taken before task H1's first file change,
  already listed the same modified `MEMORY.md` and the same two untracked memory files.
- They are agent-memory records written by the orchestrator, not product code, tests, config, or
  documentation of the shipped behavior. No production or test behavior depends on them.
- This executor is prohibited from modifying anything under `.claude/agent-memory/`, so it cannot
  remove them, and did not attempt to.

The substantive part of the criterion holds: no file outside `scripts/vscode/`,
`tests/scripts/vscode/`, and the feature folder was changed by this executor. Two supporting
records confirm it:

- The P5-T1 drift-detection-and-revert safeguard, applied on both loop iterations
  (evidence/qa-gates/poshqc-format.iter1.2026-09-02T22-58.md and
  evidence/qa-gates/poshqc-format.iter2.2026-09-02T23-04.md): the format tool rewrote no file on
  either iteration, and hash comparison over all 21 files in the two scan folders proves it.
  Separately, the P5-T3 autofix run did rewrite the out-of-write-set
  `scripts/vscode/Invoke-VSBuild.ps1`; that rewrite was reverted with `git checkout --` and the
  file is correctly absent from the porcelain output above, which is the safeguard working.
- The P5-T5 per-file coverage listing
  (evidence/qa-gates/toolchain-delta.2026-09-02T23-09.md) is confined to the 6 production files in
  this plan's write set and reports no coverage change in any file outside it.

Resolution required from the orchestrator: the three `.claude/agent-memory/orchestrator/` paths
must be dispositioned by their owner before this AC can be checked off.

---

## [P5-T10] AC5 — "Required logs/telemetry updated and validated (if applicable)."

Verdict: **Not Applicable — checked off** (spec.md line 177).

Citation: spec.md, `## Data / API / Config Impact` section, line 149:
"Logging/telemetry updates (if any): None."

No logging or telemetry surface exists in any of the seven findings. None of the production edits
(`Get-CoberturaPackageLineSummary`, the union-append loop, the package rate recomputation, the
`.claude` discovery-filter clause, the two ClosureFilter docstring addenda, the
`Get-MSTestAssemblyPathList` extraction, and the iteration-3 `Invoke-MSTestMain` extraction) adds,
removes, or changes a log statement. The two `Write-Host` calls in
`scripts/vscode/Invoke-MSTest.ps1` are pre-existing and byte-identical in text; they appear in the
analyze baseline at lines 119-120 and in the iteration-3 final QC run at lines 185-186, moved only
because the body enclosing them was relocated into `Invoke-MSTestMain` below the new
`Get-VsTestConsolePath` seam. Preserving their exact message text is a requirement of that
refactor, not an incidental outcome.

---

## [P5-T11] AC6 — "Performance constraints met or explicitly waived with rationale."

Verdict: **Explicitly waived — checked off** (spec.md line 178).

Citation: spec.md, Proposed Fix section, lines 132-133:
"#### Performance constraints (latency/throughput/memory):
N/A — no latency/throughput/memory constraint applies to these developer-tooling scripts beyond
existing test-run time."

Rationale: no new I/O and no expensive operation is introduced by any of the seven findings. The
new `Get-CoberturaPackageLineSummary` is a pure in-memory XML accumulation over nodes the caller
already walks, and P1-T10's refactor replaced an inline per-class loop with a call to it rather
than adding a second traversal. The union-append loop and package rate recomputation operate on
the same already-loaded `XmlDocument`. The `.claude` filter clause adds one regex test to an
existing `Where-Object` predicate. `Get-MSTestAssemblyPathList` moves an existing pipeline into a
function without changing what it enumerates. The iteration-3 remediation adds no operation
either: `Invoke-MSTestMain` executes the same commands the top-level body executed, and
`Get-VsTestConsolePath` wraps the same single `vswhere.exe` invocation. The measured Pester run
time for the 8 write-set test files is 16.75s at iteration 3, against a 15.78s baseline over a
partly different file set; the difference is attributable to 22 additional It cases and 2
additional files in the coverage denominator, not to a new expensive operation.

---

## [P5-T12] AC7 — "Full toolchain pass completed (format → lint → type-check → test)."

Verdict: **PASS — checked off** (spec.md line 179).

Final-iteration artifact paths, one per toolchain step:

| Step | Artifact | Result |
|---|---|---|
| Format | evidence/qa-gates/poshqc-format.iter3.2026-09-02T23-23.md | `ok: true`; no file rewritten (21 of 21 hashes byte-identical before and after) |
| Lint | evidence/qa-gates/poshqc-analyze.iter3.2026-09-02T23-25.md | 3 in-scope diagnostics, identical to the P0-T6 baseline set; zero new |
| Type-check | not applicable | `.claude/rules/powershell.md` line 17: "Type checking: Not applicable for PowerShell; skip to testing." |
| Test | evidence/qa-gates/poshqc-test.iter3.2026-09-02T23-27.md | EXIT_CODE 0; 84 passed, 0 failed, 0 skipped |

The autofix step's final-iteration record is
evidence/qa-gates/poshqc-analyze-autofix.iter3.2026-09-02T23-25.md (not run, by the task's own
branch condition).

Iteration 3 is a clean pass: no step failed and no step changed a file, so the loop terminated
rather than restarting. Three iterations ran in total. Iteration 1 restarted the loop because
P5-T2 modified `scripts/vscode/Invoke-MSTest.ps1` to resolve a newly introduced
`PSUseOutputTypeCorrectly` diagnostic. Iteration 3 was opened by the remediation that extracted
that file's entry-point body into `Invoke-MSTestMain` to close the P5-T5 criterion (d) coverage
gap; the iteration-2 artifacts remain on disk as the record of the intermediate clean pass.

Two facts are recorded so this check-off is not read as stronger than the evidence:

- The lint step's MCP tool exits 1 on any non-empty diagnostic set at any severity. It exits 1 at
  the P0-T6 baseline with 16 issues and exits 1 at final QC with 16 issues. The gate signal used
  here is the per-file diagnostic set comparison, which is identical to baseline with zero new
  diagnostics, not the exit code.
- The test step is green. The per-file coverage shortfall on `scripts/vscode/Invoke-MSTest.ps1`
  recorded at iteration 2 (72.34 percent against an 85 percent floor) was closed at iteration 3;
  that file now measures 94.00 percent and all six production files sit at or above the floor. The
  current record is evidence/qa-gates/toolchain-delta.2026-09-02T23-29.md, which supersedes
  evidence/qa-gates/toolchain-delta.2026-09-02T23-09.md.

---

## [P5-T13] AC8 — "Docs/config references updated to match the new behavior."

Verdict: **PASS — checked off** (spec.md line 180).

Cited locations, all verified present in the current tree:

1. **P3-T1 docstring addendum** —
   `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`, inside
   `Get-CoberturaInstrumentedMemberName`'s `.DESCRIPTION` block, a 9-line paragraph beginning
   "That non-admission is an asserted design choice rather than a measured one (issue #733
   finding 5)". It records the local-function exclusion as a ratified design choice and names the
   observation that should trigger a revisit.
2. **P3-T2 docstring addendum** — same file, same `.DESCRIPTION` block, a 15-line paragraph
   beginning "Known limitation, bare-name overload collision (issue #733 finding 6)". It names
   both failure directions explicitly (safe under-exclusion versus forbidden over-exclusion) and
   records why a signature-based re-key is not proposed.
3. **P1-T12 comment correction** —
   `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, inside
   `Merge-CoberturaClassesByFilename`. The stale comment "the spec specifies exactly one new
   helper" was replaced with an accurate explanation: the merged-class rate expressions stay
   inline because `Get-CoberturaPackageLineSummary` is package-scoped and cannot render a single
   merged class's own rate.

Two further explanatory comments were added alongside the production fixes, cited here for
completeness: the finding-2 union-append comment and the finding-1 package-rate-recomputation
comment, both in `Merge-CoberturaClassesByFilename`.

**spec.md determination:** no further edit to spec.md is required. It already documents the
corrected, post-fix scope: the Root Cause Analysis records the finding-6 correction from a
signature re-key to a documentation-only fix; the Assumptions section (line 138) records that
correction as superseding the issue's literal wording; line 139 records finding 5's policy as
ratified; and the Risks & Mitigations section (line 184) records finding 2's assertion reversal as
a deliberate, spec-approved change. The only spec.md edits made by this task set are the
Acceptance Criteria checkbox state changes recorded in this artifact.

No configuration file was changed. `coverage.config`, `TaskMaster.runsettings`, and
`scripts/vscode/TaskMaster.cli.runsettings` are untouched, confirmed by their absence from the
repository-root `git status --porcelain` output recorded under P5-T9.

---

## Acceptance Criteria Status

- Source: `docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/spec.md` (lines 173-180)
- Total AC items: 8
- Checked off (delivered): 7
- Remaining (unchecked): 1
- Items remaining: "No unintended behavior changes outside the defined scope." (AC4, spec.md line 176) — three `.claude/agent-memory/orchestrator/` paths fall outside the three prefixes the P5-T9 gate allows. They pre-date this executor's Phase 5 work, are orchestrator-owned, and are outside this executor's permitted write scope.
