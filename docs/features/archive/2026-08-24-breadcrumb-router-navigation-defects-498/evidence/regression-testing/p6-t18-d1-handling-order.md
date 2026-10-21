# P6-T18 — #440 decision-D1 handling order on both surfaces

Timestamp: 2026-08-26T10-15

Commands:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~ArrowKey_SingleSegmentRow_TakesPreExistingCollapsePath" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t18-quickfiler-test"; "EXIT_CODE: $LASTEXITCODE"'
```

```
pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~ArrowKey_QfcSingleSegmentRow_TakesPreExistingCollapsePath" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t18-utilities-test"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 0

TRX:

- `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t18-quickfiler-test/results.trx`
- `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t18-utilities-test/results.trx`

Output Summary:

- Efc TRX: `<Counters total="1" executed="1" passed="1" failed="0" ... />`. `ArrowKey_SingleSegmentRow_TakesPreExistingCollapsePath` — Passed (364 ms).
- Qfc TRX: `<Counters total="1" executed="1" passed="1" failed="0" ... />`. `ArrowKey_QfcSingleSegmentRow_TakesPreExistingCollapsePath` — Passed (33 ms).
- Combined: failed 0 across both TRX files, passed 2.
- Behavior pinned: on a row whose resolved chain has exactly one segment the active node is already the leaf, so no tree transition is available on either surface. Right and Left therefore take the pre-existing expand and collapse path, and where no view transition applies they take the pre-existing unhandled fall-through — on the Efc surface no provider expansion is issued and no row render is posted; on the Qfc surface the third arrow returns false so the router emits `unhandledArrow`.

Satisfies AC-18.
