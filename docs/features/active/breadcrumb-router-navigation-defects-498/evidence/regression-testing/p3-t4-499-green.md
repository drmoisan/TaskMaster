# P3-T4 — #499 Regression Tests GREEN (pass-after)

Timestamp: 2026-08-26T09-29

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~BindRowsAsync_AfterSelection_ClearsSelectedFolderPath|FullyQualifiedName~BindRowsAsync_AfterSelection_RaisesSelectedFolderPathChangedWithNull" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p3-t4"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**GREEN.** Both #499 regression tests pass against the `P3-T3` fix, with the same filter, the same
assembly and the same recipe used for the `P3-T2` RED run. Only the results directory differs.

TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p3-t4/results.trx`
records `<Counters total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" ... />`.

| Test | Outcome |
|---|---|
| `BindRowsAsync_AfterSelection_ClearsSelectedFolderPath` | Passed |
| `BindRowsAsync_AfterSelection_RaisesSelectedFolderPathChangedWithNull` | Passed |

### Fail-before / pass-after pairing

| Run | Task | Results directory | total | passed | failed | EXIT_CODE |
|---|---|---|---:|---:|---:|---:|
| RED | `P3-T2` | `.../trx/p3-t2` | 2 | 0 | 2 | 1 |
| GREEN | `P3-T4` | `.../trx/p3-t4` | 2 | 2 | 0 | 0 |

### The change between the two runs

`P3-T3` added a guarded clear to the internal `BindRowsAsync` overload in
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, immediately after the existing
`_selectedRowId = null;` and before `DeliverDocument()`:

```
            if (SelectedFolderPath != null)
            {
                SelectedFolderPath = null;
                SelectedFolderPathChanged?.Invoke(this, null);
            }
```

The guard is what makes the notification conditional on an actual change, which AC-5 requires: a
re-bind with no prior selection raises nothing. Nothing else in the method was touched — the
`ToHierarchyPath` call, the `AttachSegmentKeys` call and the `DeliverDocument` call are unchanged,
and `SelectFirstRow` is still not called from `BindRowsAsync`. The two existing `SelectedFolderPath`
write sites, `SelectRow` and `SelectHierarchyPath` in
`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`, were not modified; `SelectRow`'s
derivation from `row.FilingTarget`, which landed with pull request #605, is preserved verbatim.

Satisfies AC-26 pass-after.
