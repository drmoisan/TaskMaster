# Coverage Delta and Threshold Verification (P7-T5)

Timestamp: 2026-07-09T22-42

## Project-level `Tags` line coverage

| Measure | Value | Source |
|---|---|---|
| Baseline `Tags` line coverage | 67.28% (516 / 767) | P0-T10 (`evidence/baseline/baseline-coverage.md`) |
| Post-change `Tags` line coverage | 92.63% (704 / 760) | P7-T4 (`evidence/qa-gates/final-coverage.md`) |
| Delta | +25.35 percentage points | — |

Denominator note: the denominator shifted (767 -> 760) because `TagViewer` intent bodies moved
under `[ExcludeFromCodeCoverage]` (register E3, ~90 formerly-counted lines removed) while
`LauncherAutoAssign` and most of `CheckBoxController` entered the denominator as their exemptions
were removed/narrowed. Net effect is a large real coverage increase on a testable denominator.

## New / extracted module coverage

| Module | Coverage | Threshold | Result |
|---|---|---|---|
| `Tags.TagSelectionModel` (new) | 97.50% | >= 90% | PASS |
| `Tags.LauncherAutoAssign` (extracted, exemption removed) | 93.33% | >= 90% | PASS |
| `Tags.TagController` (+ `.Rendering` partial) | 95.10% / 89.71% | >= 80% | PASS |
| `Tags.CheckBoxController` (exemption narrowed) | 92.11% | (contributes to project floor) | PASS |

## Threshold gate

- `Tags` project >= 80%: **PASS** (92.63%).
- `TagSelectionModel` >= 90%: **PASS** (97.50%).
- `LauncherAutoAssign` >= 90%: **PASS** (93.33%).
- `TagController` (and extracted logic) >= 80%: **PASS** (95.10% / 89.71%).

## No-regression on changed lines

Every line changed or added by this refactor lives in `TagController.cs`,
`TagController.Rendering.cs`, `TagSelectionModel.cs`, `LauncherAutoAssign.cs`,
`CheckBoxController.cs`, `ITagViewer.cs`, `IUserPrompt.cs`, `WinFormsUserPrompt.cs`, and
`TagViewer.cs`. The testable seams among these are covered at 89.71%-100%; the only non-exempt
changed lines that are not fully covered are minor defensive `catch`-block error popups in
`LoadControls` (which route through `IUserPrompt.ShowMessage` and require a synthetic panel
exception to reach). No previously-covered production line regressed to uncovered: the migration
preserved or strengthened every prior behavioral assertion (13 original tests -> 64 total tests).

Outcome: **PASS** — all coverage thresholds met with no changed-line regression.
