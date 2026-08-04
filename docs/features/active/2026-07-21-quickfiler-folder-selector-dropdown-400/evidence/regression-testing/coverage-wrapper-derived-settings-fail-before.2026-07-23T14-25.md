# Coverage wrapper derived-settings failure-first result

- Timestamp: `2026-07-23T14:25:57Z`
- Context: `Derived coverage settings lifecycle`
- Command: `$result = Invoke-Pester -Path 'tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1' -FullNameFilter '*Derived coverage settings lifecycle*' -PassThru -Output Detailed; $selected = @($result.Tests | Where-Object { $_.Result -ne 'NotRun' }); 'FILE_DISCOVERED={0}' -f $result.TotalCount; 'CONTEXT_SELECTED={0}' -f $selected.Count; 'PASSED={0}' -f $result.PassedCount; 'FAILED={0}' -f $result.FailedCount; 'SKIPPED={0}' -f $result.SkippedCount; 'NOT_RUN_OUTSIDE_CONTEXT={0}' -f $result.NotRunCount; 'EXIT_CODE={0}' -f $result.FailedCount; exit $result.FailedCount`
- File discovery: `15`
- Exact context cases selected and executed: `4`
- Passed: `0`
- Failed: `4`
- Skipped: `0`
- Cases outside the exact context not run: `11`
- EXIT_CODE: `4`

Pester 5.6.1 discovers the complete source file before applying `FullNameFilter`.
The filter selected and executed only the four required cases; the existing eleven
cases were reported as not run and produced no result.

## Intended failures

1. `retains canonical module exclusions and adds the test assembly exclusion exactly once`
   failed because `ConvertTo-DerivedCoverageSettingsXml` does not exist.
2. `uses the derived settings path and preserves all eight test assemblies after the vstest boundary`
   failed because `Invoke-DotnetCoverageCollection` does not exist.
3. `removes the derived settings after successful collection without writing the canonical file`
   failed because `Invoke-DotnetCoverageCollection` does not exist.
4. `removes the derived settings after failed collection without writing the canonical file`
   reached the same missing `Invoke-DotnetCoverageCollection` contract before it
   could produce the simulated collector exception.

The four failures are limited to the missing derived-settings lifecycle. There
were no unrelated failures, crashes, timeouts, skips, filesystem-backed test
fixtures, or external-process invocations.
