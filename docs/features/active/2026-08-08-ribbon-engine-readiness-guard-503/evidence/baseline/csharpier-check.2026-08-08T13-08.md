# CSharpier Merge-Base Formatting Baseline — Issue #503 (P0-T6)

Timestamp: 2026-08-08T13-08

Command:
```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' check .; Write-Host \"EXIT_CODE=$LASTEXITCODE\""
```

EXIT_CODE: 0

Output Summary:

```
Checked 1488 files in 3810ms.
EXIT_CODE=0
```

Measured merge-base unformatted set: **empty**. `csharpier check .` returned exit code 0 over 1488 files, so no file in the repository is unformatted at the merge-base.

This measurement was performed by executing the command, not assumed. It matches the expected value recorded in the plan (`EXIT 0` over 1488 files), so there is no finding to record.

This is the comparison basis for P6-T2: the post-change repo-wide check must also return `EXIT_CODE: 0`, because the merge-base unformatted set against which a non-zero result would be compared is empty.

Note: this task is read-only. `csharpier format` was NOT run here.
