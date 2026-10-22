# P0-T2 — Branch and Merge Base

Timestamp: 2026-09-01T08-03

Command:

```text
git rev-parse --abbrev-ref HEAD
git rev-parse HEAD
git merge-base origin/main HEAD
```

EXIT_CODE: 0 (all three invocations)

## Recorded Values

| Value | Result |
| --- | --- |
| Branch (`git rev-parse --abbrev-ref HEAD`) | `bug/timeouttask-runwithtimeout-exception-type-mismatch-285` |
| HEAD (`git rev-parse HEAD`) | `21a47aac7f61668bf84c21c26949ed4bf0c50afb` |
| MERGE_BASE (`git merge-base origin/main HEAD`) | `2b85134b42872e405602e6064e02dc9cda6c319b` |

**MERGE_BASE = `2b85134b42872e405602e6064e02dc9cda6c319b`**

This is the single authoritative merge-base value. Tasks P2-T6, P3-T11, and P4-T14 read it from this
artifact rather than recomputing it.

Output Summary: The branch name printed is exactly
`bug/timeouttask-runwithtimeout-exception-type-mismatch-285`, matching the plan's declared branch.
The merge-base command exited 0. The recorded merge-base commit id
`2b85134b42872e405602e6064e02dc9cda6c319b` is 40 hexadecimal characters. It also matches the
`2b85134b` citation basis recorded at line 12 of `spec.md`, so the line numbers cited throughout the
spec and the plan were derived against this same tree state.

Acceptance: met. Branch string exact; merge-base exit 0; merge-base id is 40 hex characters.
