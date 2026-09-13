# P0-T19 — Derived instrumentation settings file

Timestamp: 2026-09-13T02-32

Command: a single pwsh payload that reads the repository-root `coverage.config` as XML, selects the `Configuration/CodeCoverage/ModulePaths/Exclude` node, appends one `ModulePath` element whose text is the regular expression matching any module name ending in `.Test.dll` (the same expression `scripts/vscode/Invoke-MSTestWithCoverage.ps1` assigns at its line 99), writes the result to `Join-Path $env:TEMP "taskmaster-838\coverage.effective.config"`, then re-reads the written file and counts its `ModulePath` children.

EXIT_CODE: 0

DERIVED_EXISTS=True
MODULE_EXCLUSIONS=8

The eight exclusions in the derived file, in order:

```
.*Deedle.*
.*FSharp.*
.*Castle\.Core.*
.*FluentAssertions.*
.*Moq.*
.*Microsoft\.Testing.*
.*MSTest.*
.*\.Test\.dll$
```

Output Summary: the derived settings file exists at the scratch path and carries eight module exclusions: the seven the repository `coverage.config` declares at its lines 14 through 20, plus the appended test-assembly exclusion. Without the appended exclusion the test assemblies are instrumented and test code enters the coverage denominator, which would make every figure in this plan incomparable to the repository's own measurements. The repository settings file was not modified: `git -C . status --porcelain -- coverage.config` produced no output after this task. The append is idempotent, matching the script's own guard, so a re-run cannot produce a ninth exclusion. Both acceptance clauses hold.
