# Acceptance-criteria check-off notes (Issue #824, tasks P6-T1 through P6-T12)

Timestamp: 2026-09-09T16-19

Work mode is `full-bug`, so
`docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/spec.md` is the
sole authoritative acceptance-criteria source. No `user-story.md` exists for #824 and none was
created; its absence is correct rather than a gap.

Each criterion was checked off individually after the plan task that satisfies it passed
verification, changing only `- [ ]` to `- [x]` on that criterion's bullet. No criterion text was
edited, reworded, renumbered, or reordered. `git diff --stat` for `spec.md` reports
`12 insertions(+), 12 deletions(-)` across 12 changed lines, which is exactly one line per
criterion and confirms no whole-file rewrite occurred.

Verification used the em-dash form, for example `- \[x\] \*\*AC1 —`, because the bare token `**AC1`
also prefixes `**AC10`, `**AC11` and `**AC12`. Each of the twelve returned exactly one checked match
and zero unchecked matches.

The three unchecked boxes in the Impact / Severity block are Blocker, High and Low severity
selectors, not acceptance criteria. They were left exactly as they were; `Medium` remains the
selected severity.

## Citations, one criterion per row

| AC | Satisfying task | Evidence cited |
|---|---|---|
| AC1 | P4-T1 | `evidence/qa-gates/ac1-assignment-sweep.2026-09-09T15-46.md`; `evidence/other/p2t2-line-range-record.2026-09-09T15-34.md` |
| AC2 | P1-T3, P2-T5 | `evidence/regression-testing/ac2-fail-before.2026-09-09T15-25.md`; `evidence/regression-testing/ac2-ac3-pass-after.2026-09-09T15-38.md` |
| AC3 | P1-T6, P2-T5 | `evidence/regression-testing/ac3-fail-before.2026-09-09T15-30.md`; `evidence/regression-testing/ac2-ac3-pass-after.2026-09-09T15-38.md` |
| AC4 | P3-T5 | `evidence/regression-testing/ilglobals-tests-after-rework.2026-09-09T15-44.md` |
| AC5 | P4-T2 | `evidence/qa-gates/ac5-loadopcodes-retained.2026-09-09T15-48.md` |
| AC6 | P4-T3, P5-T7 | `evidence/qa-gates/ac6-nullable-and-comment.2026-09-09T15-49.md`; `evidence/qa-gates/msbuild-nullable-nonvacuity.2026-09-09T16-03.md` |
| AC7 | P4-T4 | `evidence/qa-gates/ac7-test-rework.2026-09-09T15-50.md` |
| AC8 | P4-T5 | `evidence/qa-gates/ac8-parallelism-preserved.2026-09-09T15-51.md` |
| AC9 | P4-T6 | `evidence/qa-gates/ac9-no-synchronisation-primitive.2026-09-09T15-52.md` |
| AC10 | P4-T7 | `evidence/qa-gates/ac10-build-file-discipline.2026-09-09T15-53.md` |
| AC11 | P5-T1 through P5-T9 | the five command artifacts, the two non-vacuity artifacts, and `evidence/qa-gates/ac11-named-tests.2026-09-09T16-09.md` |
| AC12 | P0-T13, P5-T10, P5-T11 | `evidence/baseline/coverage-classes-baseline.2026-09-09T15-16.md`; `evidence/qa-gates/coverage-classes-post-change.2026-09-09T16-10.md`; `evidence/qa-gates/coverage-delta.2026-09-09T16-12.md` |

Every gate listed for AC1 through AC10 was additionally re-validated against the formatted tree by
P5-T13 and recorded in `evidence/qa-gates/static-gates-revalidated.2026-09-09T16-15.md`, with no
result changed.

## AC4 — restated qualification, per plan D2

AC4's own text states that `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` passes on the current
unfixed tree as well as on the fixed tree, that it is therefore a supporting test rather than a gate
for this defect, and that it must not be reported or relied upon as evidence that the race is fixed.
That qualification is restated here at check-off and is carried in the test's own XML documentation
comment in the source file.

AC4 is checked off because the test exists, is correctly implemented, and passes — which is what the
criterion asks — not because its passing demonstrates anything about the race. The deterministic
evidence for the fix is the fail-before / pass-after pairing for AC2 and AC3.

## AC11 — evidence-location reconciliation, per plan D21

AC11's final sentence requires that logs for the final pass be written under
`docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/`.

This run wrote each raw console log under `coverage/` and wrote one compact markdown artifact per
command step under `evidence/qa-gates/`. Two observed facts drive that, and both are reported rather
than the clause being silently dropped:

1. **`.gitignore:84` is `*.log`.** A raw log placed under the evidence directory is untracked and
   therefore cannot be committed as evidence. Writing it there would produce the appearance of
   compliance while leaving nothing in the repository.
2. **An msbuild console log carries absolute host paths.** Keeping raw tool output out of the
   committed feature folder is the same reason plan D7 gives for the coverage documents.

The evidence of record for each AC11 step is therefore the markdown artifact named by the task that
produced it, each carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:` with the
extracted counts. Those artifacts are listed in
`evidence/qa-gates/qc-loop-final-pass.2026-09-09T16-16.md`. No criterion text in `spec.md` was edited
to record this.

## AC12 — evidence-location reconciliation, per plan D22

AC12 requires a pre-change coverage document under `evidence/baseline/`, a post-change coverage
document under `evidence/qa-gates/`, and adds that no coverage document is written to any other
location.

This run wrote the raw Cobertura documents produced by the Coverage Command Of Record to
`coverage/baseline.cobertura.xml` and `coverage/post-change.cobertura.xml`, and committed under the
two canonical evidence directories only the compact markdown extracts. Two observed facts drive
that, both already recorded in plan D7:

1. **`.gitignore:144` is `coverage/*`**, with `coverage/.gitkeep` re-included at `:145`, so the raw
   documents are untracked and are not committed.
2. **A full-repository Cobertura document for this solution is on the order of 10 MB** and carries
   absolute host paths, so committing one as feature evidence is prohibited.

The coverage document of record for AC12 is therefore the markdown extract in each of the two
canonical directories. AC12's comparison clause is still satisfied literally: the per-`class`
`line-rate` and `branch-rate` values in those extracts **were read directly from the Cobertura
documents** by the P0-T13 command and by the P5-T10 re-run of that same command, not transcribed
from any intermediate source. No criterion text in `spec.md` was edited to record this.
