# P4-T5 — Qfc Ancestor-Chain Prerequisite GREEN (pass-after)

Timestamp: 2026-08-26T09-52

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~ResolveLeafKeyAsync_ArchiveRelativeStem_ResolvesToUniqueSuffixMatchNode" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p4-t5"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**GREEN.** The same test and the same filter that `P4-T3` recorded RED now pass against the
decision-D5 fix applied by `P4-T4`.

TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p4-t5/results.trx`
records:

```
<Counters total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0"
          inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
          disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

| Test | Outcome | Duration |
|---|---|---|
| `ResolveLeafKeyAsync_ArchiveRelativeStem_ResolvesToUniqueSuffixMatchNode` | Passed | 181 ms |

Test run summary reported by vstest: `Test Run Successful. Total tests: 1, Passed: 1`.

Change that produced the result: `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` now returns
early on a successful exact `OrdinalIgnoreCase` match, and otherwise delegates to the new private
`ResolveByUniqueSuffix`, which accepts a node whose `FolderPath` ends with a directory separator
followed by the requested path and only when exactly one node qualifies. Zero or multiple
candidates are logged at `Error` through a `log4net.ILog` field declared following the directory
idiom (`FolderScorer.cs:20`) and return null, preserving today's single-segment rendering.

Satisfies AC-8 and the AC-27 pass-after obligation. Fail-before evidence:
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p4-t3-qfc-prereq-red.md`.
