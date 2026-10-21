# D2 — Fail-Before Evidence ([P2-T3]) `[expect-fail]`

Timestamp: 2026-08-28T05-33

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:02.14. The `/p:Platform=AnyCPU` substitution
for the task's stated `"/p:Platform=Any CPU"` is the documented deviation recorded in full in
`488-d1-fail.md`.

## Step 2 — the failing test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ConfigureHostQueued_SetThemeBeforeDrain_ReplaysThemeOntoAdoptedHost" "/Logger:trx;LogFileName=488-d2-fail.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p2-t3-d2-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

| Test | Outcome |
| --- | --- |
| `ConfigureHostQueued_SetThemeBeforeDrain_ReplaysThemeOntoAdoptedHost` | **Failed** |

Total tests 1, Failed 1, elapsed 154 ms. `Test Run Failed.`

## The observed applied-theme sequence was EMPTY — which is the defect

```
Expected host.ThemesApplied to be equal to {"dark"}, but found empty collection.
```

The `RecordingHost` recorded **no theme at all**. Its `ThemesApplied` list is empty, not merely
wrong-valued and not merely short.

That is the mechanism of D2, observed directly. The test queues `ConfigureHost` and deliberately does
not drain, then calls `SetTheme("dark")`. At that moment the coordinator's `_openCoordinator` is still
null, because the lambda that constructs it is sitting in the queue, so
`BreadcrumbItemViewerLifecycleCoordinator.SetTheme`'s second forward,
`DropDownHost?.SetTheme(theme)`, null-propagates and reaches nothing. Draining afterwards adopts the
host, but nothing replays the theme onto it, so the theme is lost outright rather than delayed.

The assertion is an exact-sequence equality, so it would equally have caught a duplicated replay or an
extra theme; what it observed is the empty collection.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p2-t3-d2-fail/488-d2-fail.trx`

Output Summary: EXIT_CODE 1 with `ExpectedExitCode: 1`. The D2 regression test
`ConfigureHostQueued_SetThemeBeforeDrain_ReplaysThemeOntoAdoptedHost` records outcome **Failed**
against the unfixed code, with the observed applied-theme sequence **empty** — the theme set while the
host-configuration post was still queued was lost entirely. The intermediate build that produced the
assembly exited 0 and is not a gate.
