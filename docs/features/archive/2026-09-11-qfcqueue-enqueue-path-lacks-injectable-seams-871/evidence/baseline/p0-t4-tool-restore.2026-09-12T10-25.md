# P0-T4 — Tool manifest restore and pinned formatter version

Timestamp: 2026-09-13T04-52
Command: dotnet tool restore
EXIT_CODE: 0

## CMD-TOOLRESTORE output

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
RESTORE-EXIT: 0
```

## CMD-TOOLVERIFY output

Command: dotnet tool run csharpier --version
EXIT_CODE: 0

```
1.2.6
```

Verified: the manifest-pinned formatter version is 1.2.6, which is the version the acceptance
condition names and the version the repository format-check workflow runs.

Output Summary: CMD-TOOLRESTORE exited 0 and restored csharpier 1.2.6. CMD-TOOLVERIFY printed
`1.2.6`. Both clauses of the acceptance condition are met. Both commands were run while this item
held the shared build lock, which was released immediately after the second command returned.
