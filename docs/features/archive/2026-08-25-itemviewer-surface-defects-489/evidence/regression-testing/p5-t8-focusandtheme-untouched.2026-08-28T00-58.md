# P5-T8 — No #489 D2 test was added to the sibling-owned `QfcItemController.FocusAndThemeTests.cs`

Timestamp: 2026-08-28T00-58
Command: (Get-Content -LiteralPath QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs).Count ; git diff --numstat <BASELINE_SHA> -- QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

ObservedLineCount: 497
BaselineLineCount: 497
DiffRows: 0

`(Get-Content -LiteralPath …).Count` returns **497**, exactly equal to the P0-T15 baseline value
recorded for this file. `git diff --numstat` against `<BASELINE_SHA>` produces **no output row at
all** for the path, which is the stronger of the two checks: an equal line count could in principle
coexist with a one-line-added, one-line-removed edit, whereas an absent numstat row means git sees
the file as byte-identical to its baseline state.

## Why this file was avoided

`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` is the file a #489 D2 test
would most naturally have been appended to, since `HtmlDarkConverter` lives in the production partial
`QfcItemController.FocusAndTheme.cs`. Two constraints made that the wrong destination:

1. The file is named sibling-owned by child 493. Appending to it would create an avoidable
   cross-child conflict in the epic's highest-contention test assembly.
2. At 497 lines it had exactly **3** lines of headroom to the repository's 500-line ceiling. The
   three-test class this feature needed is 126 lines, so the append was not merely inadvisable but
   arithmetically impossible without first breaching the ceiling or extracting from a file this
   feature does not own.

The #489 D2 tests were therefore placed in the new file
`QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` created by P5-T1, which is
within this feature's permitted `Controllers\QfcItemController*` prefix, and registered by the single
`<Compile Include>` entry P5-T2 appended.

Output Summary: `QfcItemController.FocusAndThemeTests.cs` is untouched. Its line count is 497,
identical to the P0-T15 baseline, and `git diff --numstat` against `<BASELINE_SHA>` emits no row for
the path, so the file is byte-identical to its baseline state. The #489 D2 tests live in the new
`QfcItemController.ThemeMarshallingTests.cs` instead, avoiding both the 493 ownership conflict and
the 3-line headroom this file had to the 500-line ceiling.
