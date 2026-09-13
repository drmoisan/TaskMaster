# Phase 0 — CSharpier Baseline (Read-Only Check)

Timestamp: 2026-09-13T05-02
Task: [P0-T5]

Command: dotnet tool run csharpier check .
EXIT_CODE: 0
CheckedFiles: 1626
UnformattedFileList: none

Output Summary: the tool printed exactly one summary line and no per-file diagnostic. That line reads
`Checked 1626 files in 5108ms.` No file was reported as not formatted, so the base tree carries no
pre-existing formatter drift and the `UnformattedFileList:` value above is the word none rather than a
path list.

The exit code is recorded as observed and was not asserted to be zero by this task. It happens to be 0,
which is consistent with the empty unformatted list: pre-existing formatter drift is a fact about the
base tree that P0-T14 evaluates, and on this tree there is none.

## Why The Check Subcommand And Not The Format Subcommand

The check subcommand is read-only. It reports drift without repairing it, which is what makes it usable
as a baseline. The format subcommand rewrites tracked source and exits 0 after rewriting, so a baseline
captured after it had already repaired pre-existing drift would record a clean tree whatever the tree
looked like beforehand. Phase 0 runs no formatter for that reason, and the first formatter invocation in
this plan belongs to Phase 2.

The manifest-pinned CSharpier 1.2.6 was used, resolved through `dotnet tool run` after the P0-T3
restore. No global CSharpier install was invoked.

## Build Lock

The cross-item build lock was held across the check invocation only. Acquired 2026-09-13T05:02:32,
released 2026-09-13T05:02:47.
