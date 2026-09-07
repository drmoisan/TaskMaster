# CSharpier Formatter Baseline (P0-T7)

Timestamp: 2026-08-27T10-03
Task: [P0-T7]
Command: `dotnet tool run csharpier check .` (run from `<repo-root>`)
EXIT_CODE: 0
Output Summary: The base tree is clean under the manifest-pinned CSharpier 1.2.6. No file was
reported as needing formatting. Verbatim final summary line: `Checked 1540 files in 5181ms.`

## Verbatim final summary line

```
Checked 1540 files in 5181ms.
```

The command produced no per-file "would be reformatted" lines; the summary line above is the whole
output.

## Interpretation

Exit code 0 is the expected outcome per the plan's Notes rule 5: `.github/workflows/_format-check.yml`
runs the same manifest-pinned CSharpier against the same tree. The non-zero branch of that rule
(`BLOCKED: pre-existing csharpier drift`) was therefore not taken, and no file outside the Scope Lock
was formatted.

Raw log: `TestResults/plan-logs/p0-t7/csharpier-check.log` (git-ignored; not committed).
