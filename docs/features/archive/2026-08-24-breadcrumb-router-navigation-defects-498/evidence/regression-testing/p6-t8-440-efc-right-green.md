# P6-T8 — #440 Efc Right transition, pass-after (GREEN)

Timestamp: 2026-08-26T08-40

Command:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall|FullyQualifiedName~HandleArrowKey_RightAfterExpansion_DescendsByChildActivation" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t8"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 0

TRX: `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t8/results.trx`

Output Summary:

- TRX `<Counters total="2" executed="2" passed="2" failed="0" ... />`. Test Run Successful.
- `HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall` — Passed (534 ms).
- `HandleArrowKey_RightAfterExpansion_DescendsByChildActivation` — Passed (44 ms).
- Fix under test: `TryRightTreeTransitionAsync` in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` runs before the pre-existing `Right` behavior. It clears a collapse through `row.ReExpand()` as part of the transition, expands the active non-leaf segment through the landed `ExpandLeafAsync` (single `GetImmediateSubfoldersAsync` call, no `ResolveLeafKeyAsync`), and descends by activating child index 0 through `row.GetActiveChild(0)` and `SelectHierarchyPath`.

Satisfies AC-16 apart from its descent-mechanism recording clause, which `P6-T20` satisfies.
