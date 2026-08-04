# Final remediation MSTest coverage

Timestamp: 2026-07-27T03-32
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final-remediation.2026-07-27T03-32.cobertura.xml`
EXIT_CODE: 0
Output Summary: The coverage wrapper discovered and executed eight test assemblies. VSTest discovered 6,049 tests; 6,049 passed, with zero failed and zero skipped. The generated Cobertura file exists and has SHA-256 `524CCCFDFF74EF3BEECA9CC221C9684FA2B94D13020AE97F48360F9875EF62AA`. Numeric repository line coverage is 84.47% (91,846 covered of 108,736 valid lines). `coverage.config` matched canonical hash `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` before and after the run.

## Test assemblies after the VSTest boundary

The wrapper reported eight Debug test assemblies: `QuickFiler.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, and `VBFunctions.Test`.

## Effective-settings cleanup

A recursive post-run search for `*effective*settings*` returned no retained effective settings file. The wrapper completed its output-adjacent cleanup.
