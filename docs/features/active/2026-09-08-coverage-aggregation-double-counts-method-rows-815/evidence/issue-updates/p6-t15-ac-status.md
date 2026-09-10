# P6-T15 — Acceptance Criteria Status Summary

Timestamp: 2026-09-09T11-46
Task: [P6-T15]
Command: no command; this artifact is the acceptance-criteria status summary
EXIT_CODE: 0

- **Work Mode:** `full-bug`, resolved from the persisted marker `- Work Mode: full-bug` in
  `issue.md`, verified at P0-T12.
- **AC source:** `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/spec.md`,
  section `## Acceptance Criteria`. This is the sole authoritative source for `full-bug` mode.
  `user-story.md` does not exist and its absence is correct by design, not a gap.

## AC1 through AC14

| AC | Verdict | Discharging artifact | Note |
| --- | --- | --- | --- |
| AC1 | **PASS** | `evidence/qa-gates/p4-t4-invariant-and-trace-gate.md`; `evidence/regression-testing/p3-t1-pass-after.md` | Function resolves after dot-sourcing Helpers.ps1 alone; delegates to `Get-CoberturaPackageLineSummary`, count 3. |
| AC2 | **PASS** | `evidence/qa-gates/p4-t4-invariant-and-trace-gate.md` | Invariant literal present exactly once in the new file; branch-parsing literal absent from it, positive control count 6 in Helpers.ps1. |
| AC3 | **PASS** | `evidence/qa-gates/p4-t2-descendant-axis-gate.md`; `evidence/baseline/p0-t10-descendant-axis-baseline.md` | Positive control, negative assertion under `scripts/vscode/`, and case-sensitivity control pair all satisfied. |
| AC4 | **PASS** | `evidence/qa-gates/p4-t3-allowlist-derivation-gate.md`; `evidence/regression-testing/p3-t1-pass-after.md` | No production assembly-name literal under `scripts/vscode/`; T-C asserts the AST default, T-B asserts exclusion from both totals. |
| AC5 | **PASS** | `evidence/regression-testing/p3-t1-pass-after.md`; `evidence/regression-testing/p3-t2-differential-counts.md` | T-A asserts four counts and no rate; the artifact records that `LineRate` is `0.75` under both computations. |
| AC6 | **PASS** | `evidence/regression-testing/p3-t2-differential-counts.md`; `evidence/regression-testing/p1-t2-fail-before.md` | Both computations' four counts recorded as executed values: 8/6/16/10 against 4/3/8/4. |
| AC7 | **PASS** | `evidence/qa-gates/p3-t3-real-document-corroboration.md` | Both sets of four counts, both percentages, document SHA-256 matching P0-T11, and the strict inequality on `LinesValid`. |
| AC8 | **PASS** | `evidence/qa-gates/p3-t4-entry-point-report.md` | Asserted string, its pure producer, and the retained `Done. Coverage artifact:` line. |
| AC9 | **PASS** | `evidence/qa-gates/p5-t1-format.md`; `evidence/qa-gates/p5-t2-format-tree-observation.md`; `evidence/qa-gates/p5-t4-analyze-scripts.md`; `evidence/qa-gates/p5-t5-analyze-tests.md` | Format ok with empty porcelain; analyze 16 equal to baseline on scripts, 0 on tests. |
| AC10 | **PASS** | `evidence/qa-gates/p5-t6-test.md`; `evidence/qa-gates/p5-t7-coverage-final.md`; `evidence/qa-gates/p5-t8-coverage-comparison.md`; `evidence/baseline/p0-t9-bundled-coverage-nonprobative.md` | TESTS 103 against baseline 96, 0 errors, 0 failures; new module 96.97% against the 90% floor. |
| AC11 | **PASS** | `evidence/baseline/p0-t3-file-line-counts.md`; `evidence/qa-gates/p4-t5-file-line-counts.md` | Before-and-after counts for every Write Set file; largest count in the folders is 496. |
| AC12 | **PASS** | `evidence/qa-gates/p4-t6-threshold-unchanged-gate.md`; `evidence/qa-gates/p5-t8-coverage-comparison.md` | Threshold script byte-identical by SHA-256; the 80-versus-85 line-floor divergence recorded as a finding, not actioned. |
| AC13 | **PASS** | `evidence/qa-gates/p4-t7-scope-boundary.md`; `evidence/qa-gates/p5-t10-final-tree.md` | All listed paths within the five permitted prefixes; all eight prohibitions verified absent. |
| AC14 | **PASS** | `evidence/other/p4-t8-claude-md-cut3-handoff.md` | Resolved by the orchestrator at 2026-09-09T15-42; issue 828 raised. See the resolution below. |

All fourteen rows name an artifact that exists on disk.

## AC14 — resolved at 2026-09-09T15-42 (supersedes the PARTIAL recorded at 2026-09-09T11-46)

The middle clause is now satisfied. The orchestrator holds the promotion route that the executor
session lacked, and exercised it after the executor returned:

- **Issue 828** — https://github.com/drmoisan/TaskMaster/issues/828, state OPEN, title
  `Bug: claude-md-cut3-names-uninvoked-coverage-command`, promoted as type `bug` in `minor-audit`
  mode from `docs/features/potential/2026-09-09-claude-md-cut3-names-uninvoked-coverage-command.md`.
- The issue body is 3383 bytes and carries zero `not provided in potential file` placeholders, so
  every canonical bug-template section survived promotion with its content intact. The ownership
  caveat about `CLAUDE.md` versus `drm-copilot` is inside the issue body, not only in this folder.
- `evidence/other/p4-t8-claude-md-cut3-handoff.md` now carries the issue number and URL under its
  `## RESOLUTION` heading, which is the pointer AC14 requires.

All three AC14 clauses therefore hold: the mismatch is recorded in this feature's evidence, a pointer
to a separately raised issue exists, and `CLAUDE.md` does not appear in this branch's diff. AC14
reads `- [x]` in `spec.md` and plan task P6-T14 is checked off.

The section below is retained unaltered as the audit trail of the state the executor recorded and
correctly declined to overstate.

## The unmet clause as recorded at 2026-09-09T11-46, since resolved

**AC14.** The criterion reads: "The mismatch between CUT3 step 4 and the dotnet-coverage route is
recorded in this feature's evidence **with a pointer to a separate promotion or issue raised for
it**, and CLAUDE.md does not appear in this branch's diff."

Two of its three clauses are satisfied:

1. The mismatch is recorded in this feature's evidence, precisely, with the corroborating in-code
   comment at `scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 19-26 quoted, and with the full
   intended promotion text.
3. `CLAUDE.md` does not appear in this branch's diff. P4-T7 asserted this individually against both
   the anchored name-only diff and the porcelain status, finding zero occurrences.

The middle clause is **not** satisfied: **no promotion and no issue was raised, so no pointer to one
exists.** The MCP promotion route is not present in this executor's tool surface; the only MCP tools
available to this session are the four PoshQC tools. The artifact is marked `POSTING BLOCKED` with
that reason, as plan task P4-T8 directs.

AC14 is therefore left as `- [ ]` in `spec.md`. Plan task P6-T14 accepts a `POSTING BLOCKED` artifact
as sufficient for its own check-off, but checking the criterion off would assert that a promotion
exists when none does, so the criterion is left unchecked and P6-T14 is left unchecked with it.

**Residual action for the orchestrator**, which has the promotion route available: raise the issue
using the text recorded in `evidence/other/p4-t8-claude-md-cut3-handoff.md`, then check AC14 off
citing the resulting issue number and URL.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/spec.md
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none
```

Output Summary: 14 acceptance criteria evaluated, 14 PASS, 0 remaining. AC1 through AC13 were checked
off by the executor, each citing an artifact that exists on disk. AC14 was recorded PARTIAL by the
executor because the promotion MCP route is absent from its tool surface, and was resolved by the
orchestrator, which raised issue 828 and recorded the pointer in
`evidence/other/p4-t8-claude-md-cut3-handoff.md`. No assertion was weakened, no threshold was
lowered, and no criterion was checked off ahead of the evidence that discharges it.
