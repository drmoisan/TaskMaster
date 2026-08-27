# P3-T6 — #499 No-Side-Effect Tests

Timestamp: 2026-08-26T09-32

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~BindRowsAsync_WithNoPriorSelection_RaisesNoSelectedFolderPathChangedEvent|FullyQualifiedName~BindRowsAsync_DoesNotAutoSelectAnyRow" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p3-t6"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**Both pass.** TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p3-t6/results.trx`
records `<Counters total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" ... />`.

| Test | Criterion | Outcome |
|---|---|---|
| `BindRowsAsync_WithNoPriorSelection_RaisesNoSelectedFolderPathChangedEvent` | AC-5 | Passed |
| `BindRowsAsync_DoesNotAutoSelectAnyRow` | AC-6 | Passed |

### AC-5 — the notification is conditional on an actual change

The test binds once, makes no selection, subscribes, then re-binds. It counts notifications rather
than inspecting a payload, so a spurious raise carrying null would still fail it. The observed count
is zero and `SelectedFolderPath` remains null. This is the behavior produced by the `if
(SelectedFolderPath != null)` guard added in `P3-T3`; without the guard the count would be one.

### AC-6 — no auto-selection side effect

The test binds with an initialized core so the rendered document is delivered to
`NavigateToString` and can be inspected. It asserts that exactly one document was delivered and that
it does not contain the substring `rowwrap selected`.

That substring is the renderer's selected-row marker and not an incidental string:
`BreadcrumbHtmlRenderer.RenderRowFragment` composes
`string wrapClass = "rowwrap" + (isSelected ? " selected" : string.Empty);` and emits it as
`<div class="…" data-row-id="…">`, so a selected row and only a selected row produces
`class="rowwrap selected"`. The assertion is therefore falsifiable rather than vacuous.

A static confirmation accompanies the behavioral one: `grep -rn "SelectFirstRow" --include=*.cs
QuickFiler/` returns exactly two hits — the declaration at
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:202` and its single call site at
`QuickFiler/Controllers/EfcFormController.cs:409`. `BindRowsAsync` does not call it. The declaration
sits at `:202` rather than the plan's cited `:192` purely because the `P3-T3` fix inserted ten lines
above it; the method body is unchanged.

Satisfies AC-5 and AC-6.
