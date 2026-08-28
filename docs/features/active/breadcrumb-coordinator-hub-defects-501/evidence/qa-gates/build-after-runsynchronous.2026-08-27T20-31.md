# QA Gate — Build After the RunSynchronous Signature Change (P4-T3)

Timestamp: 2026-08-27T20-31

## Signature verification

Command: `git grep -F -n 'internal bool RunSynchronous' -- QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs`
Result: **exactly 1** matching line.

The method is now
`internal bool RunSynchronous(BreadcrumbUpgradeLease lease, Action operation)`, returning
`TryRunCurrent(lease, operation)`'s verdict. Per ruling PD-1 the `false` path additionally calls
`Abandon(lease)` before returning `false`, which is what allows the P4-T1 lifetime-level test — which
has no coordinator in the picture — to turn green.

The existing `catch { Abandon(lease); throw; }` is preserved **verbatim**; only the `try` body changed.
An XML `<returns>` comment was added stating that `true` means the guarded action was invoked at
entry-time currency and `false` means it was skipped and the lease has been settled.

## Analyzer build

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:20.05
```

- Error count: **0**
- Warning count: 5, all the same pre-existing `System.Reactive` `packages.config` advisory recorded at
  baseline.
- Count of lines matching `Skipping target "CoreCompile"`: **0** — the gate is non-vacuous.

## No caller broke

The `void` to `bool` change on this `internal` method broke no existing caller, as the research
document's section 3.4 predicted:

- The two production call sites (`SetSuggestions` and `AddItems`, now in
  `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs`) currently discard the value and
  still compile; P4-T5 and P4-T6 make them consume it.
- The two existing test call sites in
  `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` compile unchanged, because
  `Action a = () => Foo();` is legal when `Foo()` returns `bool` — an invocation is a valid
  statement-expression body.
- The new P4-T1 test writes the call as a statement, so it too compiles against both signatures.

Acceptance: `git grep -F -n 'internal bool RunSynchronous' -- QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs`
returns exactly one matching line, and the analyzer build records `EXIT_CODE: 0`. PASS.
