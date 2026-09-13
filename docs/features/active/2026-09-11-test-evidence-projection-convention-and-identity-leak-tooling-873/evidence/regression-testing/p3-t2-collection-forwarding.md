# P3-T2 — Collection Function Carries and Forwards the Two New Parameters

Timestamp: 2026-09-13T06-03
Task: [P3-T2]

`Invoke-DotnetCoverageCollection` in `scripts/vscode/Invoke-MSTestWithCoverage.ps1` gained the same
two parameters and forwards both by name at its single call site to `Get-DotnetCoverageArgumentList`
inside that function.

Command: pwsh -NoProfile -Command '<dot-source the coverage entry point and print the complete non-common parameter set of Invoke-DotnetCoverageCollection>'
EXIT_CODE: 0

```
COLLECTION_PARAMETERS=OutputPath,CoverageConfig,VsTestPath,TestAssembly,RunSettingsPath,ResultsDirectory,LogFileName
```

Command: pwsh -NoProfile -Command '<parse the entry point file, locate the Invoke-DotnetCoverageCollection definition, count the builder call sites within it, list the named parameters that call binds, and list the distinct set of commands the function invokes>'
EXIT_CODE: 0

```
PARSE_ERRORS=0
BUILDER_CALL_SITE_COUNT=1
BUILDER_CALL_NAMED_PARAMETERS=OutputPath,CoverageConfig,VsTestPath,TestAssembly,RunSettingsPath,ResultsDirectory,LogFileName
COLLECTION_FILESYSTEM_COMMANDS=ConvertTo-DerivedCoverageSettingsXml,Get-Content,Get-DerivedCoverageSettingsPath,Get-DotnetCoverageArgumentList,Invoke-DotnetCoverageExe,Remove-Item,Set-Content
```

## Acceptance mapping

- The parameter block declares both new parameters: `ResultsDirectory` and `LogFileName` appear in
  `COLLECTION_PARAMETERS`. Both are declared `Mandatory = $true`, matching every other parameter of
  this function.
- The builder call site passes both by name: `BUILDER_CALL_NAMED_PARAMETERS` lists
  `ResultsDirectory` and `LogFileName` as bound parameter names, and there is exactly one builder
  call site inside the function (`BUILDER_CALL_SITE_COUNT=1`).
- The function introduces no new filesystem call. Its complete invoked-command set is unchanged from
  before this task: the three filesystem commands it reaches are `Get-Content`, `Set-Content` and
  `Remove-Item`, all of which were already present, and the remaining four entries are first-party
  functions and the wrapper seam.

## Output Summary

EXIT_CODE: 0. `Invoke-DotnetCoverageCollection` declares seven parameters including the two new ones,
forwards both by name at its one builder call site, and invokes no filesystem command it did not
invoke before.
