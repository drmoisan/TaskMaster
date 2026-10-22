# P0-T10 — Pre-Change Descendant-Axis Baseline And Its Controls

Timestamp: 2026-09-09T10-49
Task: [P0-T10]
ExpectedExitCode: 1

The `ExpectedExitCode: 1` declaration above applies to commands 1 and 2, whose expected outcome is
that `git grep` finds nothing and therefore exits 1. Command 3 is the positive control and is
expected to exit 0; it is recorded with its own `EXIT_CODE:` line below. The evidence schema permits
one expectation per artifact file, so the file-level expectation is set to the value the two
zero-match gates require.

## Command 1 — case-sensitive descendant-axis search

Command: `git grep -c -F -e './/line' -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 1

```
(no output)
```

## Command 2 — case-insensitive variant of the same search

Command: `git grep -c -i -F -e './/line' -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 1

```
(no output)
```

## Command 3 — the positive control

Command: `git grep -c -F -e 'VBFunctions' -- scripts/vscode docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md`
EXIT_CODE: 0

```
docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md:3
```

## Recorded baseline

BASELINE_VBFUNCTIONS_COUNT = 3

Output Summary: Both descendant-axis searches printed nothing and exited 1, establishing a
pre-change baseline of zero occurrences of the literal under `scripts/vscode` and
`tests/scripts/vscode` in both the case-sensitive and the case-insensitive form. The positive
control printed exactly one line, naming the 2026-09-07 plan document with a count of 3 and naming
no path under `scripts/vscode`, which proves the search mechanism reports a hit when the literal is
present and so that the two zero results above are real observations rather than artifacts of a
broken query. The observed count of 3 equals the expected value the plan records as measured on this
branch head, so there is no discrepancy to note. P4-T3 compares its observed count against the 3
recorded here.
