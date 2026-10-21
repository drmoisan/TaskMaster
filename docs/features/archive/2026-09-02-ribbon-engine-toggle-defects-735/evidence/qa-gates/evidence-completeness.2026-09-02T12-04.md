# Phase 5 — Evidence Completeness Audit (P5-T10)

Timestamp: 2026-09-03T03-40
Task: [P5-T10]
Command: existence check over the 55 artifact paths named by tasks P0-T1 through P5-T9; required-field scan over every command-bearing markdown artifact; `ExpectedExitCode: 1` check on the two intentionally failing runs; then a name scan and a content scan of the whole evidence tree for the two run-time-derived tokens.
EXIT_CODE: 0

## VERDICT: PASS

Every check in part 1 and part 2 passed. Had any failed, the verdict would be BLOCKED, never PASS.

## Acceptance, part 1 — artifact completeness

| Check | Result |
|---|---|
| Artifact paths named by tasks P0-T1 through P5-T9 | 55 |
| Of those, missing from disk | **0** |
| Command-bearing markdown artifacts missing any of `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` | **0** |
| Intentionally failing runs carrying `ExpectedExitCode: 1` | **2 of 2** |

The bound stops at P5-T9 deliberately. The reduced-audit handoff artifact is written by P5-T11,
which runs after this gate, so demanding it here would be unsatisfiable at the moment the gate runs.

### One incompleteness found and repaired

The first run of this audit reported
`qa-gates/file-line-counts.2026-09-02T12-04.md` as INCOMPLETE, missing `Output Summary:`. The cause
was that the branch B re-measurement had been appended to that artifact and its two summary lines
had been relabelled `Output Summary (pass 1):` and `Output Summary (final, ...):`, so the exact
required field name no longer appeared anywhere in the file. The artifact was corrected to carry a
single unqualified `Output Summary:` line covering both passes, and the audit was re-run. The
re-run reports zero incomplete artifacts.

This is recorded rather than quietly fixed because it is exactly the failure mode the gate exists to
catch: an artifact that reads as complete to a human but does not satisfy the machine-checkable
schema.

### The two `ExpectedExitCode` artifacts

| Artifact | Carries `ExpectedExitCode: 1` |
|---|---|
| `regression-testing/fail-before-finding1.2026-09-02T12-04.md` | Yes |
| `regression-testing/fail-before-finding3.2026-09-02T12-04.md` | Yes |

Both record a genuinely non-zero observed exit code (1) alongside the declared expectation, so each
normalises to a pass without concealing what actually happened.

## Acceptance, part 2 — sanitisation completeness

Both tokens are re-derived here by the same run-time expressions P5-T9 uses —
`Split-Path -Leaf $env:USERPROFILE` for the local account token and `$env:COMPUTERNAME` for the
machine-name token — and neither value is written into this artifact.

| Check | Count | Required |
|---|---|---|
| Files and directories anywhere under the evidence tree whose NAME contains either token, compared case-insensitively | **0** | 0 |
| Case-insensitive occurrences of either token in the CONTENT of every `.trx`, `.cobertura.xml` or `.md` file under the evidence tree | **0** | 0 |

Files scanned for content: 56.

The content check is the load-bearing half. A name-only check passes trivially on a TRX whose
`runUser=` and `computerName=` attributes still carry both tokens, which is precisely the state the
raw TRX documents were in before P5-T9 and the three capture-time sweeps rewrote them. Across all
four sweeps, 851 account-token occurrences and 426 machine-name-token occurrences were removed from
file content; a name-only gate would have seen none of them.

## Acceptance, part 3 — verdict

Part 1: PASS (0 missing artifacts, 0 incomplete field sets after the repair, 2 of 2 expectation
fields present).
Part 2: PASS (0 name occurrences, 0 content occurrences).

**Verdict: PASS.**

Output Summary: All 55 artifacts named by tasks P0-T1 through P5-T9 exist on disk; every
command-bearing markdown artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and
`Output Summary:`, after one artifact was repaired for a missing summary field and the audit re-run;
both intentionally failing runs carry `ExpectedExitCode: 1`. The evidence tree contains zero file or
directory names and zero content occurrences of either the local account token or the machine-name
token. Verdict PASS.
