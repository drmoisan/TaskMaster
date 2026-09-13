# Final QC Pass Attestation (P5-T15)

- Timestamp: 2026-09-13T02-30

## Per-task recorded results, P5-T1 through P5-T7

| Task | Gate | EXIT_CODE | Measured rewrite count (P5-T1 only) |
|---|---|---|---|
| P5-T1 | CSharpier format (scoped) | 0 | 0 |
| P5-T2 | CSharpier check (repo-wide) | 0 | n/a |
| P5-T3 | Analyzer Rebuild | 0 | n/a |
| P5-T4 | Nullable Rebuild | 0 | n/a |
| P5-T5 | Coverage-instrumented capture | 0 | n/a |
| P5-T6 | KbdActionsTests re-run | 0 | n/a |
| P5-T7 | Coverage delta computation | n/a (artifact-derived computation; no command exit code) | n/a |

All recorded exit codes are 0; P5-T1's measured SHA-256 rewrite count is 0. No restart of
Phase 5 from P5-T1 was triggered.

## Anchored name-status diff (origin/main)

Command: git diff --name-status origin/main

```
M	QuickFiler.Test/Controllers/KaStringAsyncTests.cs
M	QuickFiler/Controllers/KaStringAsync.cs
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/coverage-baseline.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/coverage-tool-probe.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/csharpier-check.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/dotnet-bootstrap.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/kbdactions-baseline.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/msbuild-analyzers.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/msbuild-nullable.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/nuget-restore.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/phase0-instructions-read.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/csharpier-check.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/csharpier-format.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/msbuild-analyzers.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/msbuild-nullable.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/regression-testing/green-after-fix.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/regression-testing/red-before-fix.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/issue.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/plan.2026-09-12T10-25.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/research/2026-09-12T10-35-kastringasync-keyequals-contains-offset-research.md
A	docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md
```

The feature folder's issue.md, spec.md, plan file and research record report as Added relative
to origin/main because the worktree HEAD carries a pre-execution documentation commit that
added all four on top of that ref; these four paths are in scope for this check and are not a
reason to restart the phase, per this task's own text.

## Companion git-status porcelain listing

Command: git status --porcelain

```
 M docs/features/active/kastringasync-keyequals-contains-offset-583/plan.2026-09-12T10-25.md
 M docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md
?? docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/ac-status-summary.md
?? docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/coverage-delta.md
?? docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/coverage-postchange.md
?? docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/kbdactions-postchange.md
```

The `M` entries are this feature's own plan file and spec.md (task check-offs and AC
check-offs, both entries in spec.md's Write Set). The four untracked paths are this phase's own
evidence artifacts, all inside this feature's evidence subtree.

## Scope confirmation

Every path reported by the anchored name-status diff falls within one of: (a) the two
production/test files QuickFiler/Controllers/KaStringAsync.cs and
QuickFiler.Test/Controllers/KaStringAsyncTests.cs; (b) this feature folder's issue.md, spec.md,
plan file, research record, and evidence subtree, all Write Set entries. No path falls outside
that scope. The companion porcelain listing reports no untracked path outside that same scope.
No agent-memory path appears in either listing at this point in execution.

## Toolchain loop attestation

The format-then-lint-then-type-check-then-test loop (P5-T1 CSharpier format, P5-T2 CSharpier
check, P5-T3 analyzer Rebuild, P5-T4 nullable Rebuild, P5-T5/P5-T6/P5-T7 test-and-coverage
gates) completed as one uninterrupted pass: every recorded exit code is 0, P5-T1's measured
rewrite count is 0, and no step required a restart from P5-T1.

## Output Summary

All of P5-T1 through P5-T7 recorded exit code 0 (P5-T1's measured rewrite count 0); the
anchored name-status diff and companion porcelain listing both fall entirely within the stated
scope; the toolchain loop completed as one uninterrupted pass with no restart. Final QC PASS.
