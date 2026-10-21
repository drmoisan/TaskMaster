# [P11-T2] Full `QuickFiler.Test` suite at the `DrainBackgroundLoadingTasksAsync` seam

Timestamp: 2026-08-26T11-24

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p11-t2.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p11-t2
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 962  Passed: 962`. Total time 10.16 s, first attempt.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p11-t2/p11-t2.trx`:

```
total="962" executed="962" passed="962" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

## Seam neutrality

| Run | Total | Passed | Failed |
|---|---|---|---|
| P10-T11 (end of Phase 10) | 962 | 962 | 0 |
| P11-T2 (this run, at the seam) | 962 | 962 | 0 |

The passed count is **identical** to the P10-T11 run and the failed count is exactly `0`. The
extraction changed no observable behaviour.

## What was extracted, and why it is byte-identical

Two sites in `<CTRL>` carried the same two statements after the same comment line:

```
// Wait until Background Loading Tasks finish and then clear the collection
await Task.WhenAll(BackgroundLoadingTasks);
BackgroundLoadingTasks = [];
```

The two occurrences differed only in a blank line between the comment and the first statement.
Both now read `await DrainBackgroundLoadingTasksAsync();`, and the new member's body reproduces the
two statements verbatim, in the same order, with the same absence of `ConfigureAwait`:

```
internal async Task DrainBackgroundLoadingTasksAsync()
{
    await Task.WhenAll(BackgroundLoadingTasks);
    BackgroundLoadingTasks = [];
}
```

The extraction introduces one additional continuation hop — the caller awaits the extracted task,
which itself awaits `Task.WhenAll` — but both awaits capture and resume on the same
synchronization context, so the observable ordering at each call site is unchanged. The suite result
is the evidence for that claim rather than the argument alone.

## Toolchain state at this run

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier format QuickFiler/Controllers/QfcCollectionController.cs` | `EXIT_CODE 0` |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors, 5 pre-existing warnings |
| Test | this run | `EXIT_CODE 0`, 962 passed, 0 failed |

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit: 2,893 substitutions. No
`Deploy_*` scaffolding directory was left behind. A post-sanitisation sweep returns zero hits for
every token class recorded in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
