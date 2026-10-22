# P4-T2 — CSharpier read-only verification (Phase 4, loop iteration 1)

Timestamp: 2026-08-28T03-57
Task: [P4-T2]
LoopIteration: 1
Command: dotnet tool run csharpier check .
EXIT_CODE: 0

Read-only verification, run from the worktree root through `dotnet tool run` with the manifest-pinned
CSharpier 1.2.6. This is the CI-parity form: `.github/workflows/ci.yml` runs the same subcommand after
`dotnet tool restore`.

## Complete output

```
Checked 1547 files in 4408ms.
```

That is the whole of stdout and stderr. A case-insensitive search of the captured output for
`was not formatted` and for a leading `Error ` returns **0** lines. **Zero files are reported
unformatted.**

## Comparison against the P0-T4 baseline set

| | P0-T4 baseline | P4-T2 |
|---|---|---|
| Files processed | 1547 | 1547 |
| Files reported unformatted | 0 (empty set) | **0 (empty set)** |
| `EXIT_CODE` | 0 | 0 |

The baseline unformatted set was empty, so the acceptance condition reduces to its strict form: the
reported set must also be empty, with no additional file. It is. No file appears in the P4-T2 report
that was not in the baseline set, because neither report names any file.

The processed count is identical at 1547 on both sides, so the scan set has not changed — the two
files this remediation edited were already inside it, and no new file entered or left it.

## Acceptance

| P4-T2 condition | Result |
|---|---|
| `EXIT_CODE: 0`, or the reported unformatted set is exactly the P0-T4 baseline set with no additional file | **Yes** — `EXIT_CODE: 0`, and independently the reported set is empty, matching the empty baseline set |

Output Summary: `dotnet tool run csharpier check .` exited **0**, reporting
`Checked 1547 files in 4408ms.` with **no** file listed as unformatted. That matches the P0-T4 baseline
exactly — an empty unformatted set on both sides and an identical processed count of 1547, so the scan
set is unchanged and no file was added to the unformatted report. Both gate branches are satisfied
independently: the exit code is 0 *and* the reported set equals the baseline set.
