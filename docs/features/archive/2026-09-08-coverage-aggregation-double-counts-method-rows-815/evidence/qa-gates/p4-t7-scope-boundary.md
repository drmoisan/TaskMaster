# P4-T7 — AC13 Scope-Boundary Gate

Timestamp: 2026-09-09T11-19
Task: [P4-T7]
EXIT_CODE: 0

Both listings were captured in the same task, and both are recorded in full. The two are
complementary and each alone is wrong in one state: the anchored diff enumerates committed tracked
change and is blind to an untracked path, while porcelain status sees untracked paths and goes empty
once a change is committed. This artifact's own file is not in either listing because both were
captured before it was written.

## Command 1 — anchored name-only diff

Command: `git diff --name-only epic/review-residuals-2026-09-08-integration...HEAD`
EXIT_CODE: 0

```
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t10-descendant-axis-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t11-threshold-and-fixture-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t12-mode-markers.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t2-branch-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t3-file-line-counts.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t4-format-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t5-analyze-scripts-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t6-analyze-tests-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t7-test-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.jacoco.xml
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t9-bundled-coverage-nonprobative.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p3-t3-real-document-corroboration.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p3-t4-entry-point-report.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/regression-testing/p1-t2-fail-before.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/regression-testing/p1-t3-test-file-size.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/regression-testing/p3-t1-pass-after.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/regression-testing/p3-t2-differential-counts.md
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/plan.2026-09-08T23-49.md
scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
scripts/vscode/Invoke-MSTestWithCoverage.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1
```

24 paths.

## Command 2 — full porcelain status

Command: `git status --porcelain --untracked-files=all`
EXIT_CODE: 0

```
 M docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/plan.2026-09-08T23-49.md
?? docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t2-descendant-axis-gate.md
?? docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t3-allowlist-derivation-gate.md
?? docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t4-invariant-and-trace-gate.md
?? docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t5-file-line-counts.md
?? docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p4-t6-threshold-unchanged-gate.md
```

6 paths.

## Acceptance, part one — permitted prefixes

Every path in the combined listing begins with one of the five permitted prefixes:

| Prefix | Paths in the combined listing |
| --- | --- |
| `scripts/vscode/` | 3 |
| `tests/scripts/vscode/` | 1 |
| `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/` | 26 |
| `.claude/agent-memory/` | 0 |
| `docs/features/potential/` | 0 |

No path falls outside the set. The `plan.2026-09-08T23-49.md` entry appears in both listings because
it was committed at P4-T1 with the Phase 0 through Phase 3 check-offs and has since accumulated the
Phase 4 check-offs, which are not yet committed.

## Acceptance, part two — the eight prohibitions, each asserted individually

Each of the following was expected to be absent and each was verified absent from the combined
24-path plus 6-path listing:

| Prohibition | Occurrences |
| --- | --- |
| A listed path equal to `CLAUDE.md` | **0** |
| A listed path beginning with `.claude/skills/` | **0** |
| A listed path beginning with `.claude/rules/` | **0** |
| A listed path equal to `.editorconfig` | **0** |
| A listed path equal to `BannedSymbols.txt` | **0** |
| A listed path ending with `.cs` | **0** |
| A listed path equal to `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md` | **0** |
| A listed path equal to `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t5-retained-cobertura-reaggregation.md` | **0** |

This half of the gate is prohibition-shaped and is therefore immune to the tracked-noise carve-out
below.

## The two permitted-prefix deviations of plan decision D6, and their rationale

AC13's literal wording names three prefixes. This gate admits five. Both additions are recorded here
rather than left implicit.

- **Deviation 1 — `.claude/agent-memory/`.** That directory is tracked and agents write to it during
  a run, including this feature's executor, so a strict three-prefix gate would not be satisfiable by
  any agent-driven delivery. The prefix is admitted as a rule rather than as a list of file names,
  and it is neither a code surface nor a governance surface. **On this run the prefix produced no
  entry at all**, so the deviation changed nothing about the observed result.
- **Deviation 2 — `docs/features/potential/`.** AC14 requires the `CLAUDE.md` CUT3 wording mismatch
  to be raised as a separate promotion, and this repository's promotion lifecycle is file-based, so
  the promotion route may create a record under that path. **On this run the prefix produced no entry
  either**, and P4-T8 records why.

The gate remains falsifiable in both directions: any path outside the five prefixes fails part one,
and any occurrence of the eight prohibited paths fails part two.

Output Summary: The anchored name-only diff lists 24 paths and the porcelain status lists 6; every
one of the 30 begins with one of the five permitted prefixes, and 4 of the 5 prefixes actually
occurred, the two D6 deviations contributing zero entries each. All eight prohibited paths are
absent. AC13 holds: only `scripts/vscode/`, `tests/scripts/vscode/` and this feature's folder are
touched, and `CLAUDE.md`, the governance trees, the configuration files, every C# file and both
historical descendant-axis documents are unmodified.
