# P3-T4 — Mirror parity of the two measurement copies

Timestamp: 2026-09-13T23-30

Command:

```
git -C . diff --no-index -- docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/evidence/other/ac05-mta-initialize-measurement.md docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/ac05-mta-initialize-measurement.md
```

EXIT_CODE: 0

Output Summary:

The command printed **zero output lines** on standard output. That is the success case for
`git diff --no-index`: it means the two files are byte-identical. The command exits non-zero and
prints a unified diff when the files differ, so this condition can fail.

Two lines appeared on standard error, and they are not diff content:

```
warning: in the working copy of '<816 feature folder>/evidence/other/ac05-mta-initialize-measurement.md', LF will be replaced by CRLF the next time Git touches it
warning: in the working copy of '<809 feature folder>/evidence/other/ac05-mta-initialize-measurement.md', LF will be replaced by CRLF the next time Git touches it
```

These are the repository's normal line-ending normalisation warnings for newly written Markdown and
carry no information about parity. The run with standard error discarded printed
`STDOUT_LINE_COUNT=0`.

Corroborating observation, recorded because it is independent of git's diff machinery: the SHA-256
content hashes of the two files are equal.

| File | Hash equal |
|---|---|
| `docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/evidence/other/ac05-mta-initialize-measurement.md` | yes |
| `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/ac05-mta-initialize-measurement.md` | yes |

Both copies therefore carry identical measured values: guard `MTA_GUARD_APARTMENT: MTA` and
settling `MTA_INITIALIZE_OUTCOME: COMPLETED`, which is what AC11 requires of the pair.
