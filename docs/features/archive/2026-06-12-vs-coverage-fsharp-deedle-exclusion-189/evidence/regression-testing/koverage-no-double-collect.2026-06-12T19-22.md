# Phase 2 — Koverage No-Double-Collection Inspection (AC5)

Timestamp: 2026-06-12T19-22

Command: N/A — inspection

EXIT_CODE: N/A — inspection

Output Summary:
Inspected `Get-DotnetCoverageArgumentList` in `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (post-edit).
The argument list it returns (lines 70-76) is:

```powershell
return @(
    'collect',
    '--output', $OutputPath,
    '--output-format', 'cobertura',
    '--settings', $CoverageConfig,
    '--', $VsTestPath
) + @($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation')
```

Findings:
- The OUTER instrumentation path is `dotnet-coverage collect --settings <coverage.config>` (the `--settings`
  immediately before `--` is `coverage.config`, the instrumentation-exclude file). This is the sole
  instrumentation path.
- The INNER vstest invocation (everything after `-- $VsTestPath`) consists of the test assemblies plus only
  `"/Settings:$RunSettingsPath"` and `/InIsolation`. There is NO `/collect:"Code Coverage"` and no `/collect`
  of any kind in the inner vstest argument list.
- `$RunSettingsPath` is now resolved by `Resolve-RunSettingsPath` to `TaskMaster.cli.runsettings`, which itself
  contains NO `<DataCollector>` block. Therefore the inner vstest neither receives a `/collect` flag nor a
  runsettings-embedded coverage collector — it cannot activate a second collection.

Conclusion: the inner Koverage vstest invocation omits `/collect`, and the CLI runsettings carries no data
collector, so there is no double collection with the outer `dotnet-coverage`. AC5 (no-double-collection portion)
is confirmed by inspection.
