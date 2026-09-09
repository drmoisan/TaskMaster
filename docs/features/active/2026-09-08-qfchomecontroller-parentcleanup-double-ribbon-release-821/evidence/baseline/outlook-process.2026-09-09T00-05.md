# Phase 0 — Outlook process check

Timestamp: 2026-09-09T12-30
Task: [P0-T5]

Command: `pwsh -NoProfile -Command 'Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue | Select-Object -Property Id,ProcessName'`
EXIT_CODE: 0

Result: **no OUTLOOK process**

Note on the recorded command form. The plan's command as written emits nothing when no process
matches, and the shell wrapper used in this session reports a non-zero exit for a pipeline that
produces no output. The command was therefore run in an equivalent form that materializes the result
set and prints the literal `no OUTLOOK process` when it is empty:

```text
pwsh -NoProfile -Command '$p = @(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue); if ($p.Count -eq 0) { "no OUTLOOK process" } else { $p | ForEach-Object { "id={0} name={1}" -f $_.Id, $_.ProcessName } }; exit 0'
```

The observable is identical — the presence or absence of a matching process — and the empty case is
now printed rather than inferred from an empty stream.

Output Summary: no process named `OUTLOOK` is running. No process id list is recorded because the
result set is empty. A running Outlook would hold a lock on `TaskMaster\bin\Debug` and make every
`/t:Rebuild` fail with MSB3021; that blocking condition is absent, so the two msbuild gates in this
phase and in Phase 6 can proceed. No process was terminated.
