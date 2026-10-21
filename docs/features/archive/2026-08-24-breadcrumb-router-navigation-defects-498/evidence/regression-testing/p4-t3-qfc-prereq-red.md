# P4-T3 — Qfc Ancestor-Chain Prerequisite RED (fail-before)

Timestamp: 2026-08-26T09-46

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~ResolveLeafKeyAsync_ArchiveRelativeStem_ResolvesToUniqueSuffixMatchNode" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p4-t3"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

**RED as required.** This is an `[expect-fail]` task; the failing result is the intended outcome and
is the fail-before evidence for AC-27. A passing result here would have been a failure of the task.

TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p4-t3/results.trx`
records:

```
<Counters total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0"
          inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
          disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

| Test | Outcome |
|---|---|
| `ResolveLeafKeyAsync_ArchiveRelativeStem_ResolvesToUniqueSuffixMatchNode` | Failed |

**Observed return value: null.** The recorded failure message is verbatim:

```
Expected resolved not to be <null> because exactly one snapshot node path ends with the presented stem.
```

The assertion that failed is `resolved.Should().NotBeNull(...)`, so `ResolveLeafKeyAsync` returned
`null` for the archive-relative stem `Projects\Alpha`. The second assertion
(`resolved.FolderPath.Should().Be(...)`) was never reached.

Cause, named: `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync`
(`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:52-71`) performs a single
exact `OrdinalIgnoreCase` equality pass over `snapshot.NodesByKey.Values` at `:66-68`. No node's
`FolderPath` equals the presented stem — the unique candidate carries the store-qualified path
`\\Mailbox - User\Archive\Projects\Alpha` — so `FirstOrDefault` yields no node and `match?.Key`
evaluates to `null`. The suffix-match second pass required by decision D5 does not exist yet; it
is added by `P4-T4`.

Test run summary reported by vstest: `Test Run Failed. Total tests: 1, Failed: 1`.
