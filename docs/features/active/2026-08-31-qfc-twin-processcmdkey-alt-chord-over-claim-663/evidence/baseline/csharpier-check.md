# Phase 0 — Baseline formatting state ([P0-T8])

Timestamp: 2026-09-01T21-57

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

This is a read-only invocation, so its exit code is a sufficient observation and no tree comparison is
needed.

## BASELINE_CSHARPIER_EXIT

**BASELINE_CSHARPIER_EXIT = 0**

Final summary line, verbatim:

```
Checked 1566 files in 4494ms.
```

The exit code was read by wrapping the invocation as
`pwsh -NoProfile -Command 'dotnet tool run csharpier check . 2>&1 | Select-Object -Last 40; "CSHARPIER_EXIT=" + $LASTEXITCODE'`,
which printed `CSHARPIER_EXIT=0`.

## Files reported as unformatted

None. CSharpier reported no unformatted path. The verbatim list of unformatted files is therefore empty.

## Carry-forward disposition

BASELINE_CSHARPIER_EXIT is 0, so the `[P0-T8]` blocking branch does not apply and no carry-forward
disposition is created. `[P4-T2]` compares against exit code 0 repository-wide, unchanged, and the three
additional scoped read-only invocations that the carry-forward branch would have required are not needed.

The question the carry-forward branch would have asked — whether any unformatted path is one of the three
files this plan changes — is answered vacuously: the unformatted set is empty, so it contains none of
`QuickFiler/Controllers/QfcFormKeyHandler.cs`, `QuickFiler/Viewers/QfcFormViewer.cs` or
`QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`.

Output Summary: The read-only repository-wide CSharpier check exited 0 over 1566 files and reported no
unformatted path. BASELINE_CSHARPIER_EXIT is 0. `[P4-T2]` therefore retains its unmodified exit-code-0
acceptance and no pre-existing formatting drift is carried forward.
