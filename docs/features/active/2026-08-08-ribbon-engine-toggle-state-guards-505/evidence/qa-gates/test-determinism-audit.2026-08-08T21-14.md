# P4-T5 — Test Determinism Audit (AC-20)

Timestamp: 2026-08-08T21-14

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; Select-String -Path 'TaskMaster.Test\Ribbon\RibbonViewerEngineCallbackShapeTests.cs','TaskMaster.Test\Ribbon\EngineToggleCatalogTests.cs','TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs','TaskMaster.Test\Ribbon\EngineCommandCatalogTests.cs','TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs' -Pattern 'Path\.GetTempPath|Path\.GetTempFileName|Thread\.Sleep|Task\.Delay|DateTime\.Now|DateTime\.UtcNow|DateTimeOffset\.Now|new Form|MessageBox|BackgroundWorker|Application\.Run|Application\.DoEvents|Microsoft\.Office\.Interop\.Outlook|NotifyEngineCommandNotReady'"
```

Executed through a scratchpad `.ps1` so the five-element path list and the alternation pattern
survive intact. `EngineToggleStateCoordinatorTests.Part2.cs` was **not** appended to the `-Path`
list because no Part2 split occurred — verified: `Test-Path` on that file returns `False`.

EXIT_CODE: 0

## Output Summary — per-token counts across all five test files

| Token | Occurrences |
|---|---|
| `Path\.GetTempPath` | **0** |
| `Path\.GetTempFileName` | **0** |
| `Thread\.Sleep` | **0** |
| `Task\.Delay` | **0** |
| `DateTime\.Now` | **0** |
| `DateTime\.UtcNow` | **0** |
| `DateTimeOffset\.Now` | **0** |
| `new Form` | **0** |
| `MessageBox` | **0** |
| `BackgroundWorker` | **0** |
| `Application\.Run` | **0** |
| `Application\.DoEvents` | **0** |
| `Microsoft\.Office\.Interop\.Outlook` | **0** |
| `NotifyEngineCommandNotReady` | 1 — **prose in an XML doc comment**, not a call |

The single match is, verbatim:

```
EngineToggleStateCoordinatorTests.cs:20: /// reaches <c>NotifyEngineCommandNotReady</c>: the notification sink is an injected delegate.
```

It is the closing sentence of the fixture's `<remarks>` block asserting the very property this
audit checks. It is prose inside `<c>...</c>` markup, not an invocation.

## Explicit statement: no test drives a path reaching `NotifyEngineCommandNotReady`

`NotifyEngineCommandNotReady` is a `private` method of `RibbonController` that calls
`MessageBox.Show` (`RibbonController.EngineCommands.cs`), which would hang vstest under a modal
dialog. No test in this change invokes it, directly or transitively:

- All behavioral assertions live at the `EngineToggleStateCoordinator` /
  `EngineToggleCatalog` / `EngineCommandCatalog` seam, where the notification sink is an injected
  `Action<string>` recording into a list.
- `RibbonViewerEngineCallbackShapeTests` constructs `new RibbonViewer(new RibbonController())` and
  invokes only the two `getPressed` callbacks by reflection. `getPressed` routes to
  `EngineToggleStateCoordinator.GetPressed`, a dictionary read that never notifies. No test
  invokes any `*_Click` handler or `RunEngineCommandAsync`, which are the only paths that could
  reach the notification sink.

## Determinism

Every asynchronous outcome in `EngineToggleStateCoordinatorTests` is driven by a
`TaskCompletionSource` and awaited through the coordinator's own `GetPrimeTask` handle. No test
sleeps, polls, retries, reads the wall clock, touches the filesystem or network, creates a
temporary file, constructs a WinForms control, or starts a message pump. Corroborated by the
zero-failure runs recorded in
`<FEATURE>\evidence\other\phase2-seam-tests.2026-08-08T20-59.md` and
`<FEATURE>\evidence\regression-testing\pass-after-505.2026-08-08T21-06.md`.

Binary outcome: PASS.
