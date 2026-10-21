# P4-T6 — Decision D5 Boundary Tests (Efc no-op, ambiguous decoy)

Timestamp: 2026-08-26T10-02

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~ResolveLeafKeyAsync_EfcFullHierarchyPath_ResolvesByExactFirstPassWithoutSuffixFallback|FullyQualifiedName~ResolveLeafKeyAsync_AmbiguousStemWithDecoyNode_ReturnsNullAndLogsError" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p4-t6"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**Both boundary tests pass.** TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p4-t6/results.trx`
records:

```
<Counters total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0"
          inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
          disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

| Test | Outcome | Duration |
|---|---|---|
| `ResolveLeafKeyAsync_EfcFullHierarchyPath_ResolvesByExactFirstPassWithoutSuffixFallback` | Passed | 234 ms |
| `ResolveLeafKeyAsync_AmbiguousStemWithDecoyNode_ReturnsNullAndLogsError` | Passed | 3 ms |

### Test 1 — the decision-D5 change is a strict no-op for the Efc surface (AC-7)

The value supplied is the shape the landed `BreadcrumbBridgeRouter.ToHierarchyPath`
(`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:150-173`) produces for an archive-relative Efc
row: `root + "\\" + presentedTarget` at `:172`, that is
`\\Mailbox - User\Archive` joined to `Projects\Alpha`, giving the store-qualified path
`\\Mailbox - User\Archive\Projects\Alpha`.

The proof that the suffix fallback is never reached is arranged into the snapshot rather than
asserted through a seam. The test first asserts, as an explicit precondition, that **no** node's
`FolderPath` ends with a directory separator followed by that full hierarchy path. Had control
reached `ResolveByUniqueSuffix`, the candidate set would therefore have been empty and the method
would have returned `null` and logged at `Error`. The test then asserts that the returned key is
`ArchiveAlphaKey`. A non-null key on a snapshot where the fallback provably has no candidate can
only have come from the exact `OrdinalIgnoreCase` first pass at
`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:70-72`, which is preserved
unchanged by `P4-T4`.

### Test 2 — the suffix fallback is accepted only when unique (AC-9)

The snapshot carries `\\Mailbox - User\Archive\Projects\Alpha` and the decoy
`\\Mailbox - User\Inbox\Projects\Alpha`. Both end with `\Projects\Alpha`, so the stem
`Projects\Alpha` has two candidates and the method returns `null`. The Qfc row therefore keeps
today's single-segment fallback rendering, because
`FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` treats a null key as an empty chain
(`UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:55-68`) and adds the fallback
row built by `CreateFallbackRow`.

**Error logging.** The multiple-candidate branch of `ResolveByUniqueSuffix` calls
`logger.Error(...)` before returning null; the `log4net.ILog` field is declared at
`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:17-19` following the
directory idiom (`FolderScorer.cs:20`). The log call is verified by inspection rather than by
assertion: capturing log4net output requires attaching an appender to the shared, process-wide
log4net repository, which is mutable global state and is prohibited by the General Unit Test
Policy (`UT4 — Environment Stability`). The test asserts the observable contract — the null return
that preserves fallback rendering — which is the acceptance condition stated for this task.

Test run summary reported by vstest: `Test Run Successful. Total tests: 2, Passed: 2`.

Satisfies AC-7 and AC-9, and completes the AC-27 obligation (realistic full paths plus a decoy
node, demonstrated failing before the fix in `p4-t3-qfc-prereq-red.md` and passing after in
`p4-t5-qfc-prereq-green.md`).
