# EFC Stopwatch Construction-Site Reachability

Timestamp: 2026-08-26T11-31
Task: [P7-T2]
Command: not applicable; this artifact records which construction site the [P1-T7] test exercises
EXIT_CODE: 0

## Which site the test exercises

`StopWatch_AfterControllerConstruction_IsRunning` exercises the **constructor** site at
`QuickFiler/Controllers/EfcHomeController.cs:76`, which is the plan's preferred target. The
`InitAsync` fallback at `:225` was not needed.

**Both sites are test-reachable in the sense that matters for this criterion, and no blocker is
recorded.** The `:76` site is exercised directly by a passing test; the `:225` site is covered by
the search assertion described below.

## Evidence that `:76` is the site reached

Two observations establish it.

1. **The red-state failure message.** In the [P1-T10] run, before [P2-T2] changed the construction
   form, the test failed with:

   > Expected controller.StopWatch.IsRunning to be True because a stopwatch that is never started
   > measures nothing, but found False.

   The assertion reached `IsRunning` and read `False`. It did not fail with a null-reference or a
   null-stopwatch message. `_stopWatch` is therefore non-null after construction, which is only true
   if the `if (DataModel.Mail is not null)` branch containing line 76 was entered. That branch is
   the only assignment to `_stopWatch` on the constructor path.

2. **The green-state pass.** After [P2-T2] replaced `_stopWatch = new Stopwatch();` with
   `_stopWatch = Stopwatch.StartNew();` at both sites, the same test passes, recorded in
   `evidence/regression-testing/efc-metrics-green.2026-08-26T11-09.md`. Nothing else on the
   constructor path assigns `_stopWatch`.

## How the site was made reachable without a live Outlook process

The pre-existing `CreateController` helper builds its `EfcDataModel` through
`FormatterServices.GetUninitializedObject` with `Mail` set to `null`, so the mail-bearing branch is
skipped and `_stopWatch` is never assigned. Supplying a non-null `Mail` enters the branch, but the
branch then constructs four collaborators, and one of them is a WinForms `Form`.

The plan's own instruction is to "supply a data model whose `Mail` is non-null for this one test"
and, if that is not achievable without touching an unowned file, to fall back to the `:225` site. It
was achievable. `EfcHomeControllerDependencies` already exposes every collaborator the branch creates
as an injectable constructor parameter, and that type is `internal` with `InternalsVisibleTo`
granted to `QuickFiler.Test`, so the whole fixture lives inside the owned test file. No unowned file
was written.

The fixture, `CreateControllerWithMail` in `EfcHomeControllerMetricsTests.cs`, supplies:

| Collaborator | Injected as |
| --- | --- |
| `EfcDataModel` | uninitialized instance with `Mail` set to a loose `Mock<MailItem>` |
| `EfcViewer` | uninitialized instance, finalizer suppressed |
| `IQfcKeyboardHandler` | `null` from a stub factory |
| `IQfcExplorerController` | `null` from a stub factory |
| `EfcFormController` | uninitialized instance, finalizer suppressed |

`EfcViewer` cannot be constructed normally in a unit-test host: its constructor calls
`InitializeComponent()` and `TaskScheduler.FromCurrentSynchronizationContext()`, and the latter
throws when `SynchronizationContext.Current` is null, which is the default in the MSTest host.
Allocating it with `FormatterServices.GetUninitializedObject` avoids running the constructor
entirely, so no window handle, message pump, or synchronization context is created. The only member
the production path reads from it is `UiSyncContext`, whose backing field is null on an
uninitialized instance, and the constructor merely assigns that null to `_uiSyncContext`. The
finalizer is suppressed because `Form` inherits a finalizer from `Component` that would run
`Dispose(false)` against deliberately uninitialized fields.

No test in either owned file displays a window, pumps messages, or requires a live Outlook process.

## Covering evidence for the `:225` site

The `:225` site is not exercised by a test. It is covered by the search assertion AC-9 names, whose
post-fix result is recorded in `evidence/qa-gates/efc-search-census.2026-08-26T11-09.md`:

```
QuickFiler/Controllers/EfcHomeController.cs:76:                _stopWatch = Stopwatch.StartNew();
QuickFiler/Controllers/EfcHomeController.cs:176:            var selectionStopwatch = Stopwatch.StartNew();
QuickFiler/Controllers/EfcHomeController.cs:225:            _stopWatch = Stopwatch.StartNew();
```

Three hits, at lines 76, 176, and 225. Lines 76 and 225 are the two `_stopWatch` sites, and both now
use the started form. Line 176 is the pre-existing `selectionStopwatch` call in the
selection-change path; it is unrelated to `_stopWatch` and was not modified.

The pre-fix count for this same search was **one** hit, at line 176 only, recorded in
`evidence/baseline/defect-site-census.2026-08-26T10-42.md`. The transition from one hit to three is
falsifiable and is not satisfiable by any change other than converting both `_stopWatch` sites.
