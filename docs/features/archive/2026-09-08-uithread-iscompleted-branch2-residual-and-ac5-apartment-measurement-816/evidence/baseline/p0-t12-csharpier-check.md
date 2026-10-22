# P0-T12 — Formatter baseline (baseline)

Timestamp: 2026-09-13T23-03

Command:

```
New-Item -ItemType Directory -Force -Path coverage\logs | Out-Null
dotnet tool run csharpier check . > coverage\logs\p0-t12-csharpier-check.log 2>&1
```

EXIT_CODE: 0

Output Summary:

- Exit code: **0**
- `(Select-String -Path coverage\logs\p0-t12-csharpier-check.log -Pattern 'Was not formatted').Count`: **0**
- Final line of the log: `Checked 1633 files in 5814ms.`

The count is zero, so no path is listed here: the tree carries no pre-existing formatting drift at
baseline. A non-zero exit code would have been recorded rather than repaired, because repairing
pre-existing drift at this point would make the Phase 4 formatting gate either a blanket waiver or
unsatisfiable.

This is the first task in the plan that writes under the gitignored coverage directory. The parent
`coverage` directory is present from checkout because its `.gitkeep` is tracked; the `logs`
subdirectory beneath it is untracked and was created here by the `New-Item -Force` call. Every later
redirect into `coverage\logs\...` relies on that creation.
