# P0-T11 — Formatter Baseline (Issue #751)

Timestamp: 2026-09-03T14-26

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Sanitization: applied. Placeholder tokens used: `<WORKTREE>` for the worktree root (matched
case-insensitively in both slash directions) and `<USER>` for the account name derived at run time from
`$env:USERPROFILE`. Neither token appears in the output below, because the command named no file path.

## Output Summary

Sanitized stdout, transcribed:

```
Checked 1574 files in 4790ms.
```

The command exited 0 and named **no** unformatted file.

## Pre-existing-drift set

The pre-existing-drift set that the P4-T2 gate compares against is **empty**. CSharpier reports the
repository as fully formatted at baseline, across 1574 checked files.

CSharpier is pinned to 1.2.6 by the repository-root `dotnet-tools.json` and was invoked through
`dotnet tool run`, so the manifest-pinned version was used rather than any globally installed CSharpier.

## Bearing on P4-T2

P4-T2's acceptance is a two-rung ladder selected by this baseline exit code. Because this task recorded
`EXIT_CODE: 0`, **rung 1** applies to P4-T2: its acceptance is `EXIT_CODE: 0` for the repository-wide
`dotnet tool run csharpier check .`.

Because rung 1 applies, the P5-T9 Outcome B condition (pre-existing repository-wide csharpier drift in
files this plan does not own) is not met by this baseline.

## Note on the acceptance of this task

No particular stdout phrase is asserted by this task. The acceptance is the recorded exit code plus the
sanitized transcription above, together with the list of files named as unformatted, which is empty.
