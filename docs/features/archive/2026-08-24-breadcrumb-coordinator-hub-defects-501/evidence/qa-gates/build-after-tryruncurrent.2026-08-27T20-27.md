# QA Gate — Build After the TryRunCurrent Restructure (P3-T5)

Timestamp: 2026-08-27T20-27

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.40
```

- Error count: **0**
- Warning count: 5, all the same pre-existing `System.Reactive` `packages.config` advisory recorded at
  baseline. No new warning was introduced.
- Count of lines matching `Skipping target "CoreCompile"`: **0** — the gate is non-vacuous.
- `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` line count after the change: 331
  (was 309), within the 500-line cap.

## Structural verification — no `action` invocation inside a `lock`

The restructured method body, read back from the file verbatim:

```csharp
internal bool TryRunCurrent(BreadcrumbUpgradeLease lease, Action action)
{
    if (action == null)
    {
        throw new ArgumentNullException(nameof(action));
    }
    bool current;
    lock (_sync)
    {
        current = IsGenerationCurrentCore(lease) && !lease.Token.IsCancellationRequested;
    }
    if (!current)
    {
        return false;
    }
    action();
    return true;
}
```

The `lock (_sync)` block contains exactly one statement — the assignment that captures the currency
verdict. The `action()` invocation sits three statements later, lexically OUTSIDE the lock block, so
the body contains no invocation of `action` inside a `lock`. The `ArgumentNullException` guard is kept
ahead of the lock, unchanged.

This is research section 6.2 option A. Option B (folding a post-action currency re-check into the
return value) was NOT implemented; the XML `<returns>` comment added to the method states explicitly
that the `bool` is the entry-time verdict and must never be recomputed after the action returns, and
records the documented concurrency consequence (two threads could now both pass the check; not
reachable on current wiring because every guarded action runs on the captured
`BreadcrumbUiDispatcher` boundary).

Nothing else in the file was changed by this task.

Acceptance: the body of `TryRunCurrent` contains no invocation of `action` lexically inside a `lock`
block, and the analyzer build records `EXIT_CODE: 0`. PASS.
