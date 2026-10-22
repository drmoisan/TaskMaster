# Baseline Formatting State — CSharpier check (issue #781)

Timestamp: 2026-09-05T16-19

Task: [P0-T5]

Command: `dotnet tool run csharpier check .`, issued from the repository root inside a
`pwsh -NoProfile -Command` process.

EXIT_CODE: 0

Output Summary: The tool emitted a single output line and no per-file drift report. Final
summary line, quoted verbatim:

`Checked 71771 files in 144744ms.`

Count of files reported as needing formatting: **0**. CSharpier reports a file that requires
formatting by printing a `Error ./<path> - Was not formatted` entry for it, and the run produced
no such entry, so the count is zero. That is consistent with `EXIT_CODE: 0`, which CSharpier
returns from `check` only when every scanned file is already formatted.

The repository is therefore formatting-clean at the baseline. Any file that the [P2-T1]
`csharpier format .` run rewrites is attributable to this plan's own edits rather than to
pre-existing drift, which is what makes the [P2-T1] before-and-after tree comparison meaningful.
