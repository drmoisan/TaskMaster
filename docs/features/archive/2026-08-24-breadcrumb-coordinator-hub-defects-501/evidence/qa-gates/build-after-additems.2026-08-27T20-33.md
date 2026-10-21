# QA Gate — Build After the AddItems False-Branch (P4-T6)

Timestamp: 2026-08-27T20-33

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:20.24
```

- Error count: **0**
- Warning count: 5, all the same pre-existing `System.Reactive` `packages.config` advisory recorded at
  baseline.
- Count of lines matching `Skipping target "CoreCompile"`: **0** — the gate is non-vacuous.

## Line count

`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs`: **108** lines, measured with
`(Get-Content -LiteralPath <path>).Count`. Required bound: at or below 120. **SATISFIED** (12 lines of
headroom remain).

## What changed

`AddItems` now captures `RunSynchronous`'s `bool` into a local `ran` and calls
`_upgradeLifetime.Abandon(lease)` when it is `false`. An XML `<remarks>` block was added to the method
documenting that a superseded `AddItems` exposes **no handle to replace** — its dispatch task is
deliberately discarded and nothing on the public surface reflects it — so the skip is observable only
through the settled lease. That is why the `false` branch calls `Abandon` and nothing else, and why the
discard is intentional rather than accidental (I-502.4).

This is the second of the two call-site changes SR-5 and research section 6.4 option C require. Together
with P4-T5's `SetSuggestionsCore` branch, both production call sites of `RunSynchronous` now consume the
value, satisfying the second half of AC-12.

Acceptance: the analyzer build records `EXIT_CODE: 0`, and
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` is at or below 120 lines (108). PASS.
