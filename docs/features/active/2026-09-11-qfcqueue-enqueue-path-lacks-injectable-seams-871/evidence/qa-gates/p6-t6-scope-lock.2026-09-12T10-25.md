# P6-T6 — Untouched-files record and scope-lock adjudication

Timestamp: 2026-09-13T17-03
Command: git diff --name-status 8213826f695439e86e3ed34faa575de493a11ec7..HEAD ; git status --porcelain --untracked-files=all
EXIT_CODE: 0

## The rule being applied

> A reported path passes the scope lock when it is one of the Write Set paths, or when it lies
> underneath one of the three Write Set evidence directories, or when it lies underneath the tracked
> agent-memory directory of this worktree, or when it appears verbatim in the
> `PreExistingWorktreePaths:` block that P0-T2 recorded before this plan wrote anything. Any other
> reported path is a scope-lock failure and must be reported, not silently accepted.

Four clauses, referred to below as **W** (Write Set path), **E** (under a Write Set evidence directory),
**M** (under the tracked agent-memory directory) and **A** (anchor carve-out).

Clause A contributes nothing in this run. P0-T2 recorded an empty `PreExistingWorktreePaths:` block:
the porcelain status taken as the first git action of the resumed run, before any file was created or
edited, produced no output. The rule is therefore applied strictly narrower than the plan's preamble
anticipated, and no path anywhere in this record is admitted by clause A. The promotion-lifecycle
residuals the preamble expected — a staged rename of a potential entry, a staged addition under the
potential features directory, and modified and untracked agent-memory files — were not present.

## Full output of the name-status span of CMD-DIFF

```
M	.claude/agent-memory/orchestrator/MEMORY.md
A	.claude/agent-memory/orchestrator/cobertura-class-helper-property-names-differ-from-package.md
A	.claude/agent-memory/orchestrator/delegation-brief-silently-overrides-recorded-deviation.md
A	QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs
A	QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs
M	QuickFiler.Test/QuickFiler.Test.csproj
M	QuickFiler/Controllers/QfcQueue.Enqueue.cs
A	QuickFiler/Controllers/QfcQueue.Tlp.cs
A	QuickFiler/Controllers/QfcQueue.UiIdle.cs
M	QuickFiler/Controllers/QfcQueue.cs
A	QuickFiler/Interfaces/IUiIdleDispatcher.cs
M	QuickFiler/QuickFiler.csproj
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/p0-reanchor.2026-09-12T10-25.md
M	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/p0-t10-nullable-baseline.2026-09-12T10-25.md
M	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/p0-t11-test-baseline.2026-09-12T10-25.md
M	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/p0-t12-coverage-baseline.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/p0-t13-line-counts-baseline.2026-09-12T10-25.md
M	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/p0-t2-diff-anchor.2026-09-12T10-25.md
M	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/p0-t8-csharpier-baseline.2026-09-12T10-25.md
M	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/p0-t9-analyzer-baseline.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t1-split-tlp.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t2-split-uiidle.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t4-format.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t5-analyze.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t6-nullable.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t7-tests.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t8-line-counts.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p1-t9-invariants.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p2-t1-s1.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p2-t10-line-counts.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p2-t5-s2.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p2-t6-format.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p2-t7-analyze.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p2-t8-nullable.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p2-t9-tests.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t10-nullable.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t11-tests.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t12-line-counts.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t5-itemgroup-callsite.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t7-out-of-scope-untouched.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t8-format.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t9-analyze.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t20-test-file-size.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t21-format.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t22-analyze.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t23-nullable.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t24-tests.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t3-analyze.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p5-t1-format.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p5-t2-check.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p5-t3-analyze.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p5-t4-nullable.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p5-t5-coverage-postchange.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p5-t6-line-counts-final.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p5-t7-tests-final.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p5-t8-loop-result.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/fail-before-exception.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t10-catch-paths.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t11-collection-changed.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t12-move-monitor.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t13-digits.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t14-carrier.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t15-controller-passthrough.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t16-index-mapping.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t17-addasync-body.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t18-dispatcher-shapes.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t19-background-template.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t4-seam-contracts.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t5-headless-construction.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t6-viewer-factory-identity.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t7-guards.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t8-success-path.2026-09-12T10-25.md
A	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t9-counter-bookkeeping.2026-09-12T10-25.md
M	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
M	docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md
```

ReportedPathCount: 77

## Full output of the porcelain span of CMD-DIFF

```
 M docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p6-t1-coverage-file-rates.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p6-t2-new-code-coverage.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p6-t4-repo-wide-projection.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p6-t5-diff-review.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/residual-uncovered-regions.2026-09-12T10-25.md
```

PorcelainPathCount: 6

The porcelain span is captured because a file this change has created but not yet committed is invisible
to any commit-to-commit comparison. The six paths above are the Phase 6 artifacts written so far and the
plan file carrying this phase's check-offs; P7-T25 commits them.

## Adjudication, one row per distinct reported path group

| Paths | Clause | Justification |
|---|---|---|
| `.claude/agent-memory/orchestrator/MEMORY.md` and the two new orchestrator memory files beside it | **M** | All three lie underneath the tracked agent-memory directory of this worktree. That directory is tracked and the executing agents write to it during the run, which is why the rule carries this carve-out. |
| `QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, `QuickFiler/Controllers/QfcQueue.Tlp.cs`, `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, `QuickFiler/Interfaces/IUiIdleDispatcher.cs`, `QuickFiler/QuickFiler.csproj` | **W** | Six of the nine Write Set code and project paths. |
| `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs`, `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`, `QuickFiler.Test/QuickFiler.Test.csproj` | **W** | The remaining three Write Set code and project paths. |
| `docs/.../plan.2026-09-12T10-25.md` and `docs/.../spec.md` | **W** | Both are named Write Set paths. The plan carries checkbox state only; the spec carries the P6-T7 follow-up link and the Phase 7 acceptance-criteria check-offs. |
| 60 paths under `docs/.../evidence/baseline/`, `docs/.../evidence/qa-gates/` and `docs/.../evidence/regression-testing/` in the name-status span, plus 5 in the porcelain span | **E** | Every one lies underneath one of the three Write Set evidence directories. |

ScopeLockFailures: 0

Every one of the 77 paths in the name-status span and every one of the 6 paths in the porcelain span is
admitted by clause W, E or M. No path is admitted by clause A, because that clause is empty. No reported
path falls outside all four clauses, so there is no scope-lock failure to report.

## Paths admitted by the anchor clause

AnchorClauseAdmittedPaths: NONE

The `PreExistingWorktreePaths:` block of the P0-T2 artifact is empty, so there is no path to reproduce
here and no path in this run rests on the claim that it was present at the anchor rather than produced by
this item. Every path this record admits is admitted because this item wrote it deliberately, under
clause W, E or M.

## Files explicitly verified as untouched

The following appear in **neither** the name-status span nor the porcelain span. Each was checked by
re-running both spans restricted to its exact pathspec; both returned no output for all six.

| File | In anchored diff | In porcelain |
|---|---|---|
| QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs | no | no |
| QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | no | no |
| QuickFiler.Test/Controllers/QfcQueueTests.cs | no | no |
| UtilitiesCS/Threading/UiThread.cs | no | no |
| UtilitiesCS/Threading/WpfUiDispatcher.cs | no | no |
| UtilitiesCS/Extensions/WinFormsExtensions.cs | no | no |

The first three are the three existing QfcQueue test files in the QuickFiler test project's Controllers
folder. Six methods across the first two resolve the move-monitor backing field by reflection under its
current name, assert through FluentAssertions that the field descriptor is non-null, and then set the
field; that is precisely why seam S1 was added as a property over the retained field rather than by
converting the field to an auto-property, which would have renamed the backing field to a
compiler-generated one and failed all six. Their remaining unchanged state is the evidence that the
seam work did not disturb the existing suite.

The next two are the threading types in the utilities assembly: the static process-wide dispatcher
accessor and the WPF dispatcher wrapper. Seam S2 introduces a new narrow interface in the QuickFiler
assembly rather than reusing or modifying either of them, so neither needed to change and neither did.

The last is the utilities extension that performs the reflection-driven control clone. The
background-template seam bypasses it without modifying it; it remains a residual uncovered region and is
recorded as such by P6-T3.

## Proof that the full test assembly passes with those files unchanged

The P5-T7 result is the proof. That run executed the whole QuickFiler test assembly and reported
`total=1423 executed=1423 passed=1423 failed=0` with exit code 0, read from the trx `ResultSummary`
counters element. The three existing QfcQueue test files were compiled into that assembly in their
unchanged form — they appear in neither span above — and every case in them passed. The total of 1423 is
BASELINE_TEST_TOTAL of 1395 plus the 28 cases the new suite contributes, so no pre-existing case was lost
or silently filtered.

## Formatter drift reconciliation

Reproduced verbatim from the P5-T1 artifact:

```
FormatterRepairedPreExistingDrift: NONE
```

Reproduced verbatim from the P0-T8 artifact:

```
PreExistingDriftFiles: NONE

DriftInsideWriteSet: NONE
```

AgreementCheck: AGREE

P0-T8 recorded no path outside the Write Set, and the P5-T1 line names no path. The two agree. The
disagreement branch of this task's acceptance condition — a path recorded by P0-T8 outside the Write Set
that the P5-T1 line does not name — is not entered, and nothing is reconciled to `NONE`: the `NONE` in
the P5-T1 line is the value the derivation rule produces from a P0-T8 record that itself reads `NONE`.

Corroborating observations. P0-T8 inspected 1627 files at the anchor and named none. P5-T1 formatted 1632
files, the rise of five accounted for exactly by the five C# files this item created, and its porcelain
captures before and after are byte-identical, so it rewrote nothing. P5-T2 then inspected the same 1632
files read-only and named none. The consequence, stated in the P0-T8 record and carried here, is that a
file outside the Write Set appearing in a post-format porcelain status would have been a genuine
scope-lock failure rather than a pre-existing condition. None appeared.

This `NONE` reading is the condition under which P7-T22 may check off acceptance criterion AC22 rather
than taking its escalation branch.

Output Summary: All 77 paths in the anchored name-status span and all 6 in the porcelain span are
admitted by the Scope-lock rule — 3 by the agent-memory clause, 11 by the Write Set clause and 69 by the
evidence-directory clause. Zero scope-lock failures. The anchor clause admits nothing, because P0-T2
recorded an empty `PreExistingWorktreePaths:` block. The three existing QfcQueue test files, the two
threading types in the utilities assembly and the utilities control-clone extension appear in neither
span, verified by re-running both spans restricted to their pathspecs. P5-T7 proves the full assembly
passes with those files unchanged at 1423 of 1423 and `failed=0`. The P5-T1
`FormatterRepairedPreExistingDrift:` line reads `NONE` and agrees with the P0-T8
`PreExistingDriftFiles:` line, which also reads `NONE`. Acceptance met.
