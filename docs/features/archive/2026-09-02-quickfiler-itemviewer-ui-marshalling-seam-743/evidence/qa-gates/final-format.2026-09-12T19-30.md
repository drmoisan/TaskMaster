# Phase 6 — Final QA loop, step 1: repository-wide formatter (P6-T1)

Task: [P6-T1]
Toolchain pass: 1 (P6-T1 through P6-T5 form one pass; this is the first and only pass unless a later artifact in this directory records a restart).

## Command 1 — write-mode formatter

Timestamp: 2026-09-13T03-45
Command: `pwsh -Command 'dotnet tool run csharpier format .'` Run from the item worktree root via Set-Location inside one pwsh invocation, with console output tee'd to the ignored path `coverage\p6-t1-format.log`. Run while holding the shared machine build lock for item 743 (acquired 03:45:07, released 03:45:22 immediately after the command returned). Outlook was closed.
EXIT_CODE: 0
Output Summary:
- `Formatted 1625 files in 5392ms.` (a processed count, not a changed count; not used as a success signal per the task text)
- The observation beyond the exit code is the porcelain rewrite set recorded under Command 2 below, which is empty.

## Command 2 — rewrite-set capture (observation beyond the exit code)

Timestamp: 2026-09-13T03-45
Command: `pwsh -Command 'git status --porcelain --untracked-files=all'` (run as `git -C <worktree> status --porcelain --untracked-files=all`; same subcommand, same arguments)
EXIT_CODE: 0
Output Summary: the command printed nothing. Verbatim porcelain output:

```
```

(empty)

The formatter rewrote no tracked file and created no untracked file. Every path in the recorded porcelain output (there are none) trivially satisfies the acceptance condition. No restore was needed.

DRIFT RESTORED:
(empty — P0-T5 exited 0 with `PRE-EXISTING DRIFT FILES: none`, and this run rewrote nothing, so no `git checkout --` was issued)

## Restart-rule note

The formatter rewrote no file, so the toolchain-loop restart rule ("restart from P6-T1 if the formatter rewrites any file") is not triggered by this step.
