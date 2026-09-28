# P5-T8 — Toolchain Loop Closure and Review-Input Copy

Timestamp: 2026-09-17T02-37

Command: reconciliation of P5-T1 through P5-T7; `Copy-Item` to `artifacts/csharp/coverage.xml`

EXIT_CODE: 0

CHANNEL: COMMAND

## Reconciliation of the most recent iteration

    EXPECTATION-MET: P5-T1 YES
    EXPECTATION-MET: P5-T2 YES
    EXPECTATION-MET: P5-T3 YES
    EXPECTATION-MET: P5-T4 YES
    EXPECTATION-MET: P5-T5 YES
    EXPECTATION-MET: P5-T6 YES
    EXPECTATION-MET: P5-T7 YES

Step by step, against each step's own declared expectation:

| Step | Declared expectation | Observed | Met |
| --- | --- | --- | --- |
| P5-T1 format | exit 0 and `FORMAT_CHANGED_TREE` recorded | exit 0, `False`, patch hash unchanged | YES |
| P5-T2 check | exit 0 and `CHECKED-DELTA` at least 0 | exit 0, delta 0 | YES |
| P5-T3 analyzers | exit equals `ANALYZE-BASELINE-EXIT` 0, errors equal baseline 0, `FILE-DIAGNOSTIC-LINES: 0`, `CSC_OUT_LINES` at least 1 | 0, 0, 0, 2 | YES |
| P5-T4 nullable | exit equals `NULLABLE-BASELINE-EXIT` 0, errors equal baseline 0, `FILE-DIAGNOSTIC-LINES: 0`, `CSC_OUT_LINES` at least 1 | 0, 0, 0, 2 | YES |
| P5-T5 coverage run | `ASSEMBLY_COUNT` equals P0-T10's 9, assembly shape, `NEWLY-FAILING: NONE`, seven in-scope results `Passed` | 9, all `\bin\Debug\`, `NONE`, seven `Passed` | YES |
| P5-T6 coverage delta | twelve numeric values, one `COMPARABILITY:` line with its gate, one `CHANGED-CODE COVERAGE:` line | all twelve, `A` with the rate gate holding, `NOT MEASURABLE` | YES |
| P5-T7 size and census | `LINES` at most 500, census equals the post-fix column, `SHA256` equals `FIX-HASH:` | 490, equal, equal | YES |

P5-T5 carried no declared `ExpectedExitCode: 1` in the iteration that closed the loop, because that
iteration reported no failed test; the expectation for it was exit 0 and it was met.

ITERATIONS: 2

The loop was started from P5-T1 twice. Iteration 1 completed P5-T1 through P5-T4 and failed at
P5-T5 on an environmental file-contention failure in an unrelated `UtilitiesCS.Test` test, recorded
in `evidence/other/p5-t5-iteration-1-environmental-failure.2026-09-17T02-32.md`. Per the plan's
restart rule the loop restarted from P5-T1, and iteration 2 completed all seven steps.

LOOP: CLEAN PASS

## Review-input copy

Because all seven steps met their expectations, the post-processed Cobertura document was copied:

    Copy-Item -LiteralPath coverage/final-900.cobertura.xml -Destination artifacts/csharp/coverage.xml -Force

COPY_EXISTS: True

COPY_BYTES: 12918518

ROOT_ELEMENT: coverage

COVERAGE-XML-ROOT-LINE-RATE: 0.852566

That value equals the `FINAL-COVERAGE:` `line-rate` recorded by P5-T6, read back from the copy
rather than assumed, so the copy is verified to be the post-processed document and not the raw
collector output.

The destination is git-ignored: `git check-ignore -v` reports `.gitignore:57:artifacts/` for
`artifacts/csharp/coverage.xml`. It is a review-tooling input, not evidence, and is never staged.
No formatter pass runs after this point in the plan, which is why the copy is deferred to this task:
`.csharpierignore` does not cover that path, so a `csharpier format .` run with the file present
would process a roughly 13 MB generated XML document.

## Acceptance

All three conditions hold: seven `EXPECTATION-MET:` lines, each `YES`, plus `ITERATIONS:`;
`LOOP: CLEAN PASS` is present; `artifacts/csharp/coverage.xml` exists, its root element is
`coverage`, and `COVERAGE-XML-ROOT-LINE-RATE:` equals P5-T6's `FINAL-COVERAGE:` `line-rate`.
