# Final remediation MSTest coverage

- Timestamp (UTC): 2026-07-27T04:49Z
- Task: P9-T4
- Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final-remediation.2026-07-27T04-49.cobertura.xml`
- Result: `EXIT_CODE=0`; the wrapper discovered eight test assemblies and VSTest reported 6,056 total, 6,056 passed, 0 failed, and 0 skipped.
- Numeric repository line coverage: 91,895 covered of 108,736 valid lines = 84.512%.
- Cobertura artifact: `coverage-final-remediation.2026-07-27T04-49.cobertura.xml`; SHA-256 `DE964313E49BF96ACA614B8EEFFAA1FED18F0BAB3A54679C95A27DA2291F144C`.

## Post-VSTest-boundary test assemblies

1. `QuickFiler.Test\\bin\\Debug\\QuickFiler.Test.dll`
2. `Tags.Test\\bin\\Debug\\Tags.Test.dll`
3. `TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll`
4. `TaskTree.Test\\bin\\Debug\\TaskTree.Test.dll`
5. `TaskVisualization.Test\\bin\\Debug\\TaskVisualization.Test.dll`
6. `ToDoModel.Test\\bin\\Debug\\ToDoModel.Test.dll`
7. `UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll`
8. `VBFunctions.Test\\bin\\Debug\\VBFunctions.Test.dll`

## Settings integrity

- Canonical `coverage.config` SHA-256 before and after: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- In-memory effective coverage-settings XML SHA-256: `69509401502CFFF110C4EA8A72663E2A6A562C9DBCBA78D2E6E5BC682AF422F1`.
- Inner VSTest settings `scripts/vscode/TaskMaster.cli.runsettings` SHA-256: `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`.
- Effective-settings cleanup: `coverage-final-remediation.2026-07-27T04-49.cobertura.xml.effective-coverage.config` does not exist after the wrapper completed. The output-adjacent derived settings file was removed.

No coverage scope, settings, filter, exclusion, threshold, or postprocessor change was made.
