# Post-change full-suite MSTest run with coverage (P6-T5)

Timestamp: 2026-09-02T23-40

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\qa-gates\coverage-final.cobertura.xml'`

EXIT_CODE: 0

PassedCount: 6955

FailedCount: 0

CoberturaProcessingState: processed

## Runner summary lines

```
Discovered 9 test assemblies.
Test Run Successful.
Total tests: 6955
     Passed: 6955
Post-processing coverage XML for Koverage compatibility...
```

## Coverage root attributes

Read from the `<coverage>` root element of
`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/coverage-final.cobertura.xml`:

- `line-rate` = `0.853836`
- `branch-rate` = `0.794529`
- `lines-covered` = `55139`, `lines-valid` = `64578`
- `branches-covered` = `13186`, `branches-valid` = `16596`

## Authorized branches — neither taken

- Non-zero threshold branch: not taken. The script exited 0, and a fixed-string search of the
  captured run log for `is below the required 80% threshold` returns 0 matches.
- #743 mechanical re-run branch: not taken. `FailedCount` is 0, so there is no failing test node
  in QuickFiler.Test or in any other assembly, and the run was not repeated.

Because the script exited 0, `ConvertTo-KoverageCoberturaXml` ran to completion and the
post-processed XML was written. `CoberturaProcessingState:` is therefore `processed`, matching the
value P0-T11 recorded for the baseline, so P6-T6 compares two artifacts on the same denominator
and no conversion is required.

## Stranded derived-settings observations

- `Test-Path 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\qa-gates\coverage-final.cobertura.xml.effective-coverage.config'` returns `False`.
- `Get-ChildItem -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates' -Filter '*.effective-coverage.config'` returns 0 items.

The runner's `finally` block deleted its transient derived coverage-settings file, so no deletion
and no repeat confirmation was required.

## Discovered-assembly assertion (D9)

DiscoveredAssemblyCount: 9 — matches the `Discovered 9 test assemblies.` line.

ZeroDiscoveredPathsContainWorktreesSegment: True. Every discovered path resolves under the search
root with no `\worktrees\` segment below it. The repository-relative form of each, derived by
removing the resolved search-root prefix from the runner's absolute `FullName` values:

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
SVGControl.Test\bin\Debug\SVGControl.Test.dll
Tags.Test\bin\Debug\Tags.Test.dll
TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
TaskTree.Test\bin\Debug\TaskTree.Test.dll
TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

No absolute path is written into this artifact.

## Per-node guard outcomes — two scoped confirmation runs

The runner's inner vstest invocation configures no logger and the default console logger does not
enumerate passing test nodes, so the two guard outcomes were obtained from two immediately
following single-assembly runs. Two single-assembly runs are used rather than one combined run
because both assemblies declare a class named `NoLiveFormInTestAssemblyTests` with a method named
`ExecutingAssembly_ContainsNoFormDerivedType`, and a TRX `<UnitTestResult>` element carries only
the bare method name.

Confirmation run 1: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests" /Logger:"trx;LogFileName=utilitiescs-noliveform.trx" /ResultsDirectory:coverage\trx\p6t5` — exit code 0.

Confirmation run 2: `& $vstest SVGControl.Test\bin\Debug\SVGControl.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests" /Logger:"trx;LogFileName=svgcontrol-noliveform.trx" /ResultsDirectory:coverage\trx\p6t5` — exit code 0.

Each TRX contains exactly one `<UnitTestResult>` element (count = 1 in both). Pairing each
element's `outcome` attribute with the fully-qualified identifier of the assembly that produced
that TRX gives:

```
UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType outcome="Passed"
SVGControl.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType outcome="Passed"
```

Only the derived fully-qualified identifier and the `outcome` value are transcribed. No raw
`<UnitTestResult>` element is pasted, because it carries a `computerName` host identifier. The two
TRX files live under `coverage\trx\p6t5`, and `coverage/` is gitignored, so they leave the tracked
tree clean.

Output Summary: The full suite passes with 6955 passed and 0 failed across 9 discovered
assemblies. Both structural guards pass, confirmed per node. The Cobertura artifact is
post-processed. Repository line-rate `0.853836`, branch-rate `0.794529`. This task runs three
commands; the `Command:` and `EXIT_CODE:` fields above carry the `Invoke-MSTestWithCoverage.ps1`
run, which is the run the coverage gate applies to, and the two scoped confirmation commands are
recorded with their own exit codes in this section.
