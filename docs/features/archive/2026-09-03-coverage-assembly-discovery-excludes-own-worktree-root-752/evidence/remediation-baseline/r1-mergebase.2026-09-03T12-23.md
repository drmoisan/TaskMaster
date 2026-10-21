# Merge Base and Pre-Remediation Head — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-40
- Task: `[P0-T3]`

Command:

1. `git -C <repo-root> merge-base origin/main HEAD`
2. `git -C <repo-root> rev-parse HEAD`

EXIT_CODE:

1. `0`
2. `0`

Output Summary:

- `MERGE_BASE: 87233f867ad60c0a5c0d19b09cc121ae536d7ba1`
- `PRE_REMEDIATION_HEAD: 80d07a1c26122a5cede04edc5833c964d663d8b7`

Both values are 40 hexadecimal characters. No fetch was performed; the merge base was resolved
against the already-present `origin/main` ref. Commit SHAs are not host identifiers and are recorded
verbatim.

Observation, recorded for traceability and not used as an acceptance value: the resolved merge base
is the same SHA that `remediation-inputs.2026-09-03T12-23.md` cites at its "How it was found"
section as the base of the audit's diff.
