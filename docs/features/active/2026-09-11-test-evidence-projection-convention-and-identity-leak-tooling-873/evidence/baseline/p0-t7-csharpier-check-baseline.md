# P0-T7 — C# Format Baseline (CSharpier verify mode)

Timestamp: 2026-09-13T04-55
Task: [P0-T7]

Command: dotnet tool run csharpier check .
Invocation form: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; dotnet tool run csharpier check .'
EXIT_CODE: 0

Verify mode was used rather than write mode: this delivery changes no C# source file, and verify
mode exits non-zero on drift, so its exit code discriminates a clean tree from a drifted one. The
manifest-pinned version is 1.2.6, resolved through `dotnet tool run` per P0-T5.

Build lock: acquired for item 873 before the command and released immediately after it returned.

## Output

```
Checked 1626 files in 5826ms.
```

## Output Summary

EXIT_CODE: 0
CSHARPIER_FINAL_SUMMARY_LINE: `Checked 1626 files in 5826ms.`
CSHARPIER_FILES_CHECKED: 1626

The tree is clean under the pinned formatter: no file was reported as requiring formatting and the
command exited 0. The later C# format gate compares against this exit code and this summary shape.

EXIT_CODE: 0
