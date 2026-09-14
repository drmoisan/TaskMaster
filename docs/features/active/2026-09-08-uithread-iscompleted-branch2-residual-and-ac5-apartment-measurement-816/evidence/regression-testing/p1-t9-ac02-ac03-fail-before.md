# P1-T9 — AC2 and AC3 fail-before projection

Timestamp: 2026-09-13T23-21

Command, verbatim:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p1-t8-fail-before.trx" /ResultsDirectory:coverage\trx\p1-t8 "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

EXIT_CODE: 1

ExpectedExitCode: 1

## The two negative regression tests, recorded Failed

| # | Fully qualified test | Recorded outcome | Failure message |
|---|---|---|---|
| AC2 | `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse` | **Failed** | `Expected observed to be False, but found True.` |
| AC3 | `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse` | **Failed** | `Expected observed to be False, but found True.` |

## This run executed against the unmodified predicate

No production source had changed when this run was taken. Phase 1 of the plan of record changes no
production source at all: it adds the two negative regression tests, the positive twin, and the
project-file registration, and nothing else. The captured-UI-context exit of
`UtilitiesCS/Threading/UiThread.cs` still carried its pre-change condition

```
if (ReferenceEquals(_context, _uiSyncContext))
```

with no second, independent proof of thread identity.

The plan task that later applies the hardening is **P2-T1**. Until it runs, the exit returns `true`
whenever the captured context matches by reference and the captured managed thread id matches, so
both negative cases are red by construction rather than by accident.

## What each failure demonstrates

- **AC2, the recycled-managed-thread-id leg.** The test installs a captured dispatcher owned by a
  live STA host thread, installs a captured UI context, then on a dedicated MTA thread installs
  that thread's own managed id as the captured UI thread id and sets a second, different, non-null
  ambient context. The unmodified exit matched the context by reference and returned `true`, so the
  await would have completed inline on a thread that is not the UI thread. The assertion
  `observed.Should().BeFalse()` therefore failed with `but found True`.
- **AC3, the null-dispatcher fail-closed leg.** The test installs a null captured dispatcher and
  runs on a dedicated MTA thread that owns no dispatcher of its own. The unmodified exit again
  returned `true`. This case is what forbids the hardening from being satisfied by a bare reference
  comparison, which would match null against null and return `true` on exactly this thread shape.

The pass-after half of both criteria is recorded by P2-T5 and projected by P2-T6.
