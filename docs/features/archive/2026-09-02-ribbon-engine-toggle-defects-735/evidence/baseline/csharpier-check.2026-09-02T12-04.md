# Phase 0 — Read-Only Formatter Baseline (P0-T5)

Timestamp: 2026-09-03T01-18
Task: [P0-T5]
Command: `dotnet tool run csharpier check .` (working directory set to the worktree root, dotnet resolved to the repo-local SDK)
EXIT_CODE: 0

## Verbatim output

```
Checked 1571 files in 5672ms.
```

## Unformatted-file set at baseline

The exit code is 0 and CSharpier reported no unformatted file. The baseline unformatted set is
therefore EMPTY:

```
(no paths reported)
```

This empty set is the comparison basis for P4-T4. P4-T4's acceptance is satisfied when the final
check either exits 0 or reports exactly this same set; since this set is empty, the practical
requirement on P4-T4 is exit code 0.

This task is not a pass/fail gate. It is recorded as the baseline against which the final formatter
verification is compared.

## Formatter-visible write-set paths

Eight of the ten write-set paths are formatter-visible; the two project files are excluded by
`.csharpierignore`. None of the eight appears in the baseline unformatted set, because that set is
empty:

| Path | In baseline unformatted set |
|---|---|
| `TaskMaster/Ribbon/RibbonExplorer.xml` | No |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | No |
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | No |
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | No (does not exist at baseline) |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | No |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | No |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | No (does not exist at baseline) |
| `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | No (does not exist at baseline) |

Output Summary: `csharpier check .` returned EXIT_CODE 0 having checked 1571 files. The baseline
unformatted-file set is empty, and none of the eight formatter-visible write-set paths appears in
it.
