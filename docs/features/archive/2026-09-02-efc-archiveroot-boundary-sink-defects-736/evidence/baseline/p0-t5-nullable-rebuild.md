# P0-T5 — Nullable gate baseline

Timestamp: 2026-09-03T23-34

Command:

```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:LogFile=coverage\p0-t5-nullable.detailed.log;Verbosity=detailed" /fl1 "/flp1:LogFile=docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\baseline\p0-t5-nullable.min.log.txt;Verbosity=minimal"
@(Select-String -LiteralPath coverage\p0-t5-nullable.detailed.log -SimpleMatch -CaseSensitive -Pattern 'Skipping target "CoreCompile"').Count
@(Select-String -LiteralPath coverage\p0-t5-nullable.detailed.log -SimpleMatch -CaseSensitive -Pattern 'Task "Csc"').Count
git add -N docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t5-nullable.min.log.txt
git ls-files -- docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t5-nullable.min.log.txt
```

`/p:Nullable=enable` is deliberately absent, because adding that property conscripts every file that
has never adopted the `#nullable enable` pragma. `$msbuild` was resolved per D1 through vswhere.

EXIT_CODE: 0

## Non-vacuity observations (D9)

- Case-sensitive count of `Skipping target "CoreCompile"` in the detailed log: **0**
- Case-sensitive count of `Task "Csc"` in the detailed log: **18**

## Detailed log (not committed)

- Repository-relative path: `coverage/p0-t5-nullable.detailed.log`
- Byte size: 10645944
- SHA-256: `8FC4A684B2B4F4187B3B65F09CAF25416B9FA6647C67994D7071AA6FD04CE94D`

## Minimal log — the two required observations

1. **Existence.** `Test-Path` on
   `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t5-nullable.min.log.txt`
   printed `True`.
2. **Trackedness.** This task's own `git ls-files --` span printed that same path. This task's own
   `git add -N` step exited **0**.

Both observations were made against this task's own log path and its own command spans, not against
P0-T4's.

## Warning count

Warning count printed by msbuild on the console at default verbosity, read from the `N Warning(s)`
summary line: **0**. This is the figure P6-T5 compares against.

Console summary block, verbatim:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Failure branch

Not triggered. The msbuild step exited 0, so the nullable gate is green at the merge base.

Output Summary: nullable rebuild exited 0 with `0 Warning(s)` and `0 Error(s)`. Non-vacuity proven:
`Skipping target "CoreCompile"` count 0, `Task "Csc"` count 18. The minimal log exists on disk and is
tracked by git. Detailed log 10645944 bytes, SHA-256
`8FC4A684B2B4F4187B3B65F09CAF25416B9FA6647C67994D7071AA6FD04CE94D`, left uncommitted under
`coverage/`.
