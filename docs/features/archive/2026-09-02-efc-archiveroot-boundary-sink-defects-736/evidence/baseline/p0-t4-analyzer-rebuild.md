# P0-T4 — Analyzer gate baseline

Timestamp: 2026-09-03T23-33

Command:

```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:LogFile=coverage\p0-t4-analyzer.detailed.log;Verbosity=detailed" /fl1 "/flp1:LogFile=docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\baseline\p0-t4-analyzer.min.log.txt;Verbosity=minimal"
@(Select-String -LiteralPath coverage\p0-t4-analyzer.detailed.log -SimpleMatch -CaseSensitive -Pattern 'Skipping target "CoreCompile"').Count
@(Select-String -LiteralPath coverage\p0-t4-analyzer.detailed.log -SimpleMatch -CaseSensitive -Pattern 'Task "Csc"').Count
git add -N docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t4-analyzer.min.log.txt
git ls-files -- docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t4-analyzer.min.log.txt
```

`$msbuild` was resolved per D1 through vswhere with
`-latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'`.

EXIT_CODE: 0

## Non-vacuity observations (D9)

- Case-sensitive count of `Skipping target "CoreCompile"` in the detailed log: **0**
- Case-sensitive count of `Task "Csc"` in the detailed log: **18**

The zero establishes that no project short-circuited its compilation; the 18 establishes that the
C# compiler task actually ran, once per compiled project.

## Detailed log (not committed)

- Repository-relative path: `coverage/p0-t4-analyzer.detailed.log`
- Byte size: 10535222
- SHA-256: `3AFDC81297148944D6FD2E718F59CCB08272431259211A84FC8564AA459347FD`

The detailed log is written under `coverage\`, which `.gitignore` line 144 matches, and is
deliberately not committed.

## Minimal log — the two required observations

1. **Existence.** `Test-Path` on
   `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t4-analyzer.min.log.txt`
   printed `True`. The file exists on disk at the `.log.txt` evidence path.
2. **Trackedness.** `git ls-files --` on that same path printed that same path, so git tracks it.
   The `git add -N` step that precedes the `git ls-files` span exited **0**, which is what supplies
   the index entry that makes the `git ls-files` observation possible for a newly written file.

Both observations are required and neither substitutes for the other: a file matched by a
`.gitignore` pattern would exist on disk and pass the first while failing the second, and would never
reach a commit. The `.log.txt` suffix D9 requires is what keeps `.gitignore` line 84 (`*.log`) from
matching it.

## Warning count

Warning count printed by msbuild on the console at default verbosity, read from the `N Warning(s)`
summary line: **0**.

Console summary block, verbatim:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

This is the figure P6-T4 compares against. A recorded value of 0 makes P6-T4's comparison a
zero-new-analyzer-warning budget for this item's own edits.

The retained minimal-verbosity log carries no warning summary and no warning lines, which is why the
count is read from the console summary rather than from that log; the detailed log that does carry
the summary is deliberately not committed.

## Failure branch

Not triggered. The msbuild step exited 0, so the analyzer gate is green at the merge base and no
blocking condition is reported.

Output Summary: analyzer rebuild exited 0 with `0 Warning(s)` and `0 Error(s)`. Non-vacuity proven:
`Skipping target "CoreCompile"` count 0, `Task "Csc"` count 18. The minimal log exists on disk and is
tracked by git (`git add -N` exit 0, `git ls-files` echoed the path). Detailed log 10535222 bytes,
SHA-256 `3AFDC81297148944D6FD2E718F59CCB08272431259211A84FC8564AA459347FD`, left uncommitted under
`coverage/`.
