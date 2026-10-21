# P2-T5 — #498 Regression Tests GREEN (pass-after)

Timestamp: 2026-08-26T09-21

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~SegmentDoubleClick_IndexAboveRange_RejectedWithoutTransition|FullyQualifiedName~SegmentDoubleClick_NegativeIndex_RejectedWithoutTransition" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p2-t5"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**GREEN.** Both #498 regression tests now pass against the `P2-T4` range guard, with the same
filter, the same assembly and the same recipe used for the `P2-T3` RED run. Only the results
directory differs, so the two runs cannot be confused.

TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p2-t5/results.trx`
records `<Counters total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" ... />`.

| Test | Outcome |
|---|---|
| `SegmentDoubleClick_IndexAboveRange_RejectedWithoutTransition` | Passed |
| `SegmentDoubleClick_NegativeIndex_RejectedWithoutTransition` | Passed |

### Fail-before / pass-after pairing

| Run | Task | Results directory | total | passed | failed | EXIT_CODE |
|---|---|---|---:|---:|---:|---:|
| RED | `P2-T3` | `.../trx/p2-t3` | 2 | 0 | 2 | 1 |
| GREEN | `P2-T5` | `.../trx/p2-t5` | 2 | 2 | 0 | 0 |

The only change between the two runs is the `P2-T4` fix in
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`: the `SegmentDoubleClick` arm of
`ProcessInboundAsync` now reads `message.SegmentIndex` into a local, rejects it when it has no value
or falls outside `[0, row.Segments.Count - 1]`, logs the rejection at `Error` through the existing
`log4net` field, and breaks without a transition and without a render post. The null-forgiving
dereference `message.SegmentIndex!.Value` is gone from that arm.
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` was not modified, the
`catch (BreadcrumbMessageException)` in `OnHostMessageReceived` was not widened, and the
`SegmentActivate` and `RenderedChildActivate` arms were left untouched.

Satisfies AC-1 and the AC-25 pass-after obligation.
