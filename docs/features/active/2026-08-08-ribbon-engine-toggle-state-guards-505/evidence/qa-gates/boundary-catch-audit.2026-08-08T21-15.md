# P4-T6 — Boundary-Catch Audit (AC-7 structural clause)

Timestamp: 2026-08-08T21-15

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; Select-String -Path 'TaskMaster\Ribbon\EngineToggleStateCoordinator.cs','TaskMaster\Ribbon\RibbonController.EngineCommands.cs','TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs' -Pattern '\bcatch\s*\('; git diff f910ff2f21c67a03cf8eebcb340727d5415d8e08..HEAD -- TaskMaster\Ribbon\RibbonController.EngineCommands.cs TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs | Select-String -Pattern '^\+.*\bcatch\s*\('"
```

Executed through a scratchpad `.ps1` so the three-element path list and the escaped regex survive
intact; enclosing-member resolution was added to the same script to attribute the single match.

EXIT_CODE: 0

## Output Summary

### Exactly one `\bcatch\s*\(` match in `EngineToggleStateCoordinator.cs`

| File | `\bcatch\s*\(` matches |
|---|---|
| `TaskMaster\Ribbon\EngineToggleStateCoordinator.cs` | **1** |
| `TaskMaster\Ribbon\RibbonController.EngineCommands.cs` | **0** |
| `TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs` | **0** |

The single match, verbatim:

```
EngineToggleStateCoordinator.cs:178: catch (Exception ex)
```

Enclosing member, resolved mechanically as the nearest preceding member declaration:

```
internal async Task HandleToggleClickAsync(string engineName)   (declared at line 166)
```

The one `catch` is therefore **inside `HandleToggleClickAsync`**, the designated `async void`
click boundary, exactly as AC-7 requires.

Prose occurrences of the word "catch" in XML doc comments (for example
"`this type ... never catches`") do not match the regex `\bcatch\s*\(` because they are not
followed by an open parenthesis, and are not counted.

### `ExecuteToggleAsync` contains no `catch`

`ExecuteToggleAsync` spans lines **205-237** (from its declaration to the line before
`internal Task GetPrimeTask`). `\bcatch\s*\(` matches in that body: **0**. The testable core
propagates an engine fault unchanged to the boundary, preserving the #503 fail-fast philosophy.

The prime path likewise contains no `catch`: `ApplyPrimeAsync` propagates, and its fault is
observed by the `CompletePrime` continuation, which reads `Task.Exception` and routes it to
`logError`. That is why the file's total is 1 rather than 2.

### The branch diff adds zero `catch (` to the two glue files

`git diff <MERGE_BASE>..HEAD -- RibbonController.EngineCommands.cs RibbonViewer.EngineCommands.cs`
produced **0** added (`+`) lines matching `\bcatch\s*\(`. The six command sites inherit the #503
no-catch philosophy unchanged, and the two toggle handlers are single awaited expressions whose
awaited task cannot fault.

Binary outcome: PASS.
