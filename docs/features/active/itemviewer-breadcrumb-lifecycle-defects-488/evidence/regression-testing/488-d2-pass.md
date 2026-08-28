# D2 — Pass-After Evidence ([P2-T5])

Timestamp: 2026-08-28T05-35

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:02.94. The `/p:Platform=AnyCPU` substitution is
the documented deviation recorded in full in `488-d1-fail.md`.

### A nullable warning was raised and fixed before this gate was recorded

The first build of `[P2-T4]`'s edit reported **4** warnings rather than 3. The additional one was:

```
QuickFiler\Viewers\BreadcrumbItemViewerLifecycleCoordinator.cs(154,39): warning CS8604: Possible null
reference argument for parameter 'theme' in 'void IBreadcrumbDropDownHost.SetTheme(string theme)'.
```

`BreadcrumbItemViewerLifecycleCoordinator.cs` carries `#nullable enable` on its first line, so it
participates in nullable analysis, and `/p:TreatWarningsAsErrors=true` — the `[P8-T4]` gate command —
would have promoted that `CS86xx` warning to a **build error**. The cause is that the .NET Framework
4.8.1 reference assemblies carry no `NotNullWhen` post-condition attribute on
`string.IsNullOrWhiteSpace`, so a bare `if (!string.IsNullOrWhiteSpace(_retainedTheme))` does not
establish a non-null flow state for the argument.

The guard was rewritten to capture the field into a local and test it explicitly:

```csharp
string? retained = _retainedTheme;
if (retained != null && !string.IsNullOrWhiteSpace(retained))
{
    host.SetTheme(retained);
}
```

The `retained != null` conjunct supplies the flow state the analyzer needs and the
`IsNullOrWhiteSpace` conjunct retains the whitespace half of the guard, so the delivered code is still
guarded against **both** null and whitespace as the criterion requires. No null-forgiving `!` operator
and no warning suppression was used. The rebuild after that change reported **0 occurrences of
CS8604** and returned to 3 warnings, all three pre-existing.

## Step 2 — the test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ConfigureHostQueued_SetThemeBeforeDrain_ReplaysThemeOntoAdoptedHost|FullyQualifiedName~HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder|FullyQualifiedName~ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks" "/Logger:trx;LogFileName=488-d2-pass.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p2-t5-d2-pass
```

EXIT_CODE: 0

| Test | Outcome |
| --- | --- |
| `ConfigureHostQueued_SetThemeBeforeDrain_ReplaysThemeOntoAdoptedHost` | **Passed** |
| `HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder` | **Passed** |
| `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks` | **Passed** |

Total tests 3, Passed 3, **Failed 0**. `Test Run Successful.`

## Why the two constraining tests stayed green

- `HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder` pins the subscription order
  `add`/`remove` across a host swap. It never calls `SetTheme`, so `_retainedTheme` stays null
  throughout and the replay guard short-circuits on its `retained != null` conjunct. The replay adds
  no `EventOperations` entry in any case, because it calls `SetTheme` rather than subscribing.
- `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks` reconfigures with the **same** host, so
  `ConfigureHost` takes the `UpdateRequestProviders` `else` branch. That branch performs no theme call
  at all, by design, which is what keeps the replay from being observable as a duplicated callback.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p2-t5-d2-pass/488-d2-pass.trx`

Output Summary: EXIT_CODE 0, failed count **0**, all three named tests `Passed`. The D2 regression test
now records `ThemesApplied` as exactly `{"dark"}` where `[P2-T3]` observed an empty collection. A
CS8604 nullable warning introduced by the first form of the replay guard was found and fixed before
this gate was recorded, so the delivered guard is analyzer-clean and still checks both null and
whitespace.
