# P3-T5 — #499 Write-Site Preservation Tests

Timestamp: 2026-08-26T09-30

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~SelectRow_StillAssignsFilingTargetToSelectedFolderPath|FullyQualifiedName~SelectHierarchyPath_StillAssignsArchiveRelativePathToSelectedFolderPath" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p3-t5"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**Both pass.** TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p3-t5/results.trx`
records `<Counters total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" ... />`.

| Test | Write site guarded | Outcome |
|---|---|---|
| `SelectRow_StillAssignsFilingTargetToSelectedFolderPath` | `SelectRow` in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:84-100` | Passed |
| `SelectHierarchyPath_StillAssignsArchiveRelativePathToSelectedFolderPath` | `SelectHierarchyPath` in the same file, `:102-110` | Passed |

### What each test pins

- **`SelectRow`.** After a bind, a `rowSelected` payload for the suggestion row still assigns
  `row.FilingTarget` — the presented row text — to `SelectedFolderPath`, and still publishes that
  value through `SelectedFolderPathChanged`. The test subscribes before the selection and asserts
  the published payload as well as the property, so a fix that assigned the property without
  notifying would fail it.
- **`SelectHierarchyPath`.** The test arranges an ancestor chain rooted under a non-empty archive
  root and binds through the internal four-argument `BindRowsAsync` overload, so that
  `ToArchiveRelativePath` is genuinely exercised rather than short-circuited by an empty root. A
  `segmentActivate` on the non-leaf ancestor then assigns the archive-relative form `Clients`
  rather than the full hierarchy path. The assertion pins both the expected value and the negative
  case, so a regression that stopped stripping the root would fail.

Both tests were placed in the shared `BreadcrumbBridgeRouterQueueTests` fixture, using its
`[TestInitialize]` mocks, and neither duplicates an assertion made by the read-only
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`. That file's
`Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget` asserts the archive-relative
projection under its own strict-mock fixture as a #609 behavioral claim; the test here is a
preservation guard attached to the #499 change, arranged from a different fixture, and exists so
that the `BindRowsAsync` clear cannot silently disturb either write site.

Neither write site was modified by `P3-T3`.

Satisfies the AC-4 write-site clause.
