# Final QA Gate 1 — CSharpier (issue #742, [P5-T1])

Timestamp: 2026-09-14T02-21

Command:

1. `pwsh -NoProfile -Command 'dotnet tool run csharpier format .'`
2. `pwsh -NoProfile -Command 'dotnet tool run csharpier check .'`

EXIT_CODE: 0 (of the read-only `check` command; the `format` command also exited 0)

Output Summary:

- `format` printed `Formatted 1635 files in 5451ms.`
- `check` printed `Checked 1635 files in 5291ms.` and exited 0, so no formatting diff remains
  anywhere in the tree.

The file count rose from the 1634 recorded in the [P0-T4] baseline to 1635, which is the one new
test file this change adds.

Acceptance: the `check` command's `EXIT_CODE` is 0 — satisfied.

## Observation beyond the exit code

`format` is a write-mode command and exits 0 whether or not it rewrote a file, so its exit code
alone is not evidence. Two independent observations are recorded instead:

1. The read-only `check` run above reports no remaining diff over all 1635 files.
2. `git status --porcelain --untracked-files=all`, taken between the `format` and `check` runs,
   listed only this change's own Write Set paths, this feature folder's evidence artifacts, the plan
   file and `spec.md`. No unrelated tracked file was rewritten by the formatter, so the [P0-T4]
   baseline of a clean tree held and this gate is measuring only this change.

CSharpier was invoked through `dotnet tool run` so the manifest-pinned version 1.2.6 was used, per
`CLAUDE.md` and `.claude/rules/csharp.md`. No global installation was used.
