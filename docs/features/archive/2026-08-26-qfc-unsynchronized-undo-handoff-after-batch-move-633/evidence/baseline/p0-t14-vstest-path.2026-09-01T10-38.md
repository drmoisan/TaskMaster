# vstest.console.exe path resolution (P0-T14)

Timestamp: 2026-09-01T10-38
Task: [P0-T14]
Working directory: WORKTREE

Command:

```
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
```

EXIT_CODE: 0

## Resolved path

```
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
```

`vswhere.exe` itself was confirmed present before the call. The command returned exactly one path.
`Test-Path` reports that the resolved path exists, and it ends in `vstest.console.exe`.

This path is recorded verbatim rather than tokenised. It carries no user-account segment and no machine
name — it sits under `Program Files` — so it is not an absolute host path of the kind
`.claude/agent-memory/_shared_no_absolute_host_paths.md` prohibits, and the P2-T7 and P8-T1 sanitisation
sweeps, which replace only the absolute worktree path, do not target it.

## Substitution instruction for the eight scoped run tasks

Every scoped test-run task in this plan — P1-T5, P2-T5, P4-T6, P5-T10, P6-T8, P6-T9, P6-T10, and P7-T8 —
is written as "the absolute path recorded by P0-T14" in place of the leading executable name. The
concrete value to substitute is the path above. The resolved installation is Visual Studio 18 Community.

Each of those eight runs also passes an explicit, neutral TRX file name inside a double-quoted switch,
for example `"/Logger:trx;LogFileName=p1-t5.trx"`. The quoting is load-bearing rather than cosmetic: an
unquoted semicolon terminates the argument in `pwsh`, which would pass a bare `/Logger:trx` and let
vstest name the TRX file after the current account and host. Content sanitisation cannot reach a file
name, so the name must be neutral at the moment the run produces it.

Output Summary: `vstest.console.exe` is not on `PATH` in this worktree, as the plan recorded. It was
resolved through `vswhere` to a single existing path under the Visual Studio 18 Community installation,
and that path is now available for substitution into the eight scoped, single-assembly test runs.
