# P3-T5 — Assembly-Discovery Test File Repaired

Timestamp: 2026-09-13T06-03
Task: [P3-T5]

`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` declared an explicit
five-parameter mock body for `Invoke-DotnetCoverageCollection`, which stops binding once that function
gains two parameters. The mock body now declares seven parameters, and the mocked post-processor
return value now carries the root attributes the reconciliation assertion reads. Nothing else in the
file changed.

## Structural probe

Command: pwsh -NoProfile -Command '<parse the file, print the parameter count and names of every param block, the line count, the occurrence count of the literal lines-valid and the count of path-based fixture loads>'
EXIT_CODE: 0

```
PARSE_ERRORS=0
PARAM_BLOCK_COUNT=2 NAMES=VsWherePath,VsWhereArgs
PARAM_BLOCK_COUNT=7 NAMES=OutputPath,CoverageConfig,VsTestPath,TestAssembly,RunSettingsPath,ResultsDirectory,LogFileName
LINES=106
LINES_VALID_HITS=2
PATH_FIXTURE_LOADS=0
```

## Pester run limited to this file

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 with Run.PassThru and Detailed output, printing the passed, failed and skipped counts and ending with an explicit exit>'
EXIT_CODE: 0

```
Describing Invoke-MSTestWithCoverage assembly discovery
WARNING: Test-result summary was not written: Test-result XML has no <ResultSummary> node.
  [+] includes an assembly directly beneath a search root that is itself under a .claude worktree segment
WARNING: Test-result summary was not written: Test-result XML has no <ResultSummary> node.
  [+] excludes a nested sibling worktree beneath a non-dot-claude search root
WARNING: Test-result summary was not written: Test-result XML has no <ResultSummary> node.
  [+] retains the root-level assembly and excludes a further-nested worktree beneath a dot-claude search root
PESTER_COUNTS passed=3 failed=0 skipped=0
```

PASSED: 3
FAILED: 0
SKIPPED: 0

## Acceptance mapping

- The file measures 106 lines, which is at most 500.
- A Pester run limited to this file reports zero failed.
- The mock body's parameter block declares seven parameters:
  `OutputPath,CoverageConfig,VsTestPath,TestAssembly,RunSettingsPath,ResultsDirectory,LogFileName`.
  The other param block in the file belongs to the pre-existing vswhere mock and is unchanged.
- The mocked post-processor return value contains the single-line literal `lines-valid`. Two
  occurrences of that literal are reported: the fixture itself and the comment that states why the
  fixture carries it.
- The unfiltered content-reader mock was left as it is. The three observed warnings
  `Test-result summary was not written: Test-result XML has no <ResultSummary> node.` are the
  broadened non-fatal branch P3-T3 specifies, taken once per entry-point call, and all three tests
  still pass. That is the mechanism that keeps this file passing without any further edit here, and
  it is observed rather than assumed.
- No file is created, written or deleted by these tests: the probe reports zero path-based fixture
  loads and every filesystem command the entry point reaches is mocked.

## Output Summary

EXIT_CODE: 0. The mock body declares seven parameters, the post-processor fixture carries the
`lines-valid` root attribute, 3 passed and 0 failed, 106 lines against the 500 ceiling, and the
non-fatal warning branch was observed once per entry-point call.
