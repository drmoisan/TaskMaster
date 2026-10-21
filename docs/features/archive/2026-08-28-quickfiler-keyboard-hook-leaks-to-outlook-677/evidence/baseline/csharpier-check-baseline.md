# CSharpier Formatting Baseline (P0-T5)

Timestamp: 2026-08-28T15-44
Command: `./.dotnet-sdk/dotnet.exe tool run csharpier check .` (from repo root)
EXIT_CODE: 0

## Output Summary

**clean** — zero pre-existing formatting violations.

```
Checked 1554 files in 4127ms.
EXIT_CODE=0
```

No file list was emitted because there are no violations. Consequence for P5-T1: the baseline is
clean, so P5-T1 takes the **repo-wide** branch (`csharpier format .`), not the scoped branch.
