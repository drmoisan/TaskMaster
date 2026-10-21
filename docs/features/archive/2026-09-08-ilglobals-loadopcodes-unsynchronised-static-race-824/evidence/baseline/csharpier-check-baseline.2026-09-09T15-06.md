# Baseline formatting state — csharpier check (Issue #824, task P0-T8)

Timestamp: 2026-09-09T15-06

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; dotnet tool run csharpier check . 2>&1 | Tee-Object -FilePath coverage/csharpier-baseline.log; Write-Output ("EXIT=" + $LASTEXITCODE)'`

EXIT_CODE: 0

Output Summary:

```
Checked 1622 files in 4227ms.
```

Pre-existing-drift list, consumed by P5-T3:

```
none
```

csharpier reported no file as needing formatting. The `check` subcommand exits non-zero when any
file needs formatting, so the exit code of 0 discriminates here and the empty drift list is a real
observation rather than an absence of output.

The read-only `check` subcommand is used at baseline deliberately. Capturing a baseline after a
write-mode formatter had already repaired pre-existing drift would turn the later P5-T3 gate into a
blanket waiver.

Invocation note: the plan states this command terminating in `exit $LASTEXITCODE`. It was issued
with a trailing `Write-Output ("EXIT=" + $LASTEXITCODE)` instead, so that the exit code is visible in
the captured console output rather than only in the process status. The observation asserted is the
same value; the raw log at `coverage/csharpier-baseline.log` is unaffected. `coverage/*` is
gitignored, so that log is not committed.
