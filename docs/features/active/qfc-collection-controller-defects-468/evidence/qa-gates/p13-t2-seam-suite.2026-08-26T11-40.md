# [P13-T2] Full `QuickFiler.Test` suite at the move-readiness seam

Timestamp: 2026-08-26T11-40

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p13-t2.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p13-t2
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 964  Passed: 964`. Total time 9.64 s, first attempt.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p13-t2/p13-t2.trx`:

```
total="964" executed="964" passed="964" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

## Seam neutrality

| Run | Total | Passed | Failed |
|---|---|---|---|
| P12-T4 (end of Phase 12) | 964 | 964 | 0 |
| P13-T2 (this run, at the seam) | 964 | 964 | 0 |

The passed count is **identical** to the P12-T4 run and the failed count is exactly `0`. No test was
added by P13-T1; the seam is purely a restructuring.

## What the seam did, and why it is behaviour-preserving

The `ReadyForMove` getter previously interleaved three things: the readiness evaluation, the
notification-text accumulation, and a modal `MessageBox.Show`. It is now:

```
public bool ReadyForMove
{
    get
    {
        if (TryGetMoveReadiness(out string notifications))
        {
            return true;
        }

        NotifyNotReady(notifications);
        return false;
    }
}
```

- `TryGetMoveReadiness(out string notifications)` carries the evaluation loop **verbatim** — the same
  three header sentinel strings, the same `SelectedFolder is null` test, the same notification-text
  concatenation, the same iteration over `_itemGroups` with no added null guard. It sets
  `notifications` to `string.Empty` on the true path and to the accumulated text on the false path.
- `_notifyNotReady` is a private `Action<string>` whose lazily-assigned default is the same
  `MessageBox.Show` call with the same message, the same `"Error Notification"` caption, the same
  `MessageBoxButtons.OK` and the same `MessageBoxIcon.Error`.

Production therefore still evaluates identically and still presents the same dialog on the same
path. In this test run, no test injects the delegate, so the default is what would run; the suite
passes with no dialog because no test reaches the false path.

The seam mirrors the in-file precedent at `_removeGroupByEntryId` / `RemoveGroupByEntryId` — a
private backing field carrying the explanatory XML comment, plus a private property that assigns the
default with `??=` — so the file has one seam idiom rather than two.

## Scope assertions

| Assertion | State |
|---|---|
| `MessageBox.Show` occurrences in `<CTRL>` | exactly **1**, at `:179`, inside the delegate's default |
| `TryGetMoveReadiness` added to `IQfcCollectionController` | **no** — zero occurrences; the interface file is unmodified in the working tree |
| Interface member set | unchanged |

## Toolchain state at this run

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,525 files checked, 0 needing formatting |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors, 5 pre-existing warnings |
| Test | this run | `EXIT_CODE 0`, 964 passed, 0 failed |

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit. Any `Deploy_*` scaffolding
directory was removed. A post-sanitisation sweep returns zero hits for every token class recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
