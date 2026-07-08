# Phase 5 Gate — Tests + Coverage (P5-T15)

Timestamp: 2026-07-02T09-16
Command (tests, canonical): vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage
Command (numeric coverage): dotnet-coverage collect --output artifacts\csharp\coverage-r2-p5.cobertura.xml --output-format cobertura --settings coverage.config -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
EXIT_CODE: 0

## Test result (regression guard)

- Total tests: 289
- Passed: 289
- Failed: 0
- Regression baseline (P0-T5) of 233 pre-existing tests preserved; 56 new Phase 5 tests added on top
  (233 + 56 = 289). All pass.

## Coverage mechanism note

The plan's canonical `vstest ... /EnableCodeCoverage` emits a binary `.coverage` that is not
offline-convertible to Cobertura in this environment (dotnet-coverage v18.5 and
Microsoft.CodeCoverage.Console v18.7 both produce empty packages; the deprecated CodeCoverage.exe
`analyze` command is disabled in VS18). Numeric per-partial coverage is therefore obtained with the
repository's established `dotnet-coverage collect --output-format cobertura --settings coverage.config`
mechanism (the same path used by `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and cycle-1), which
instruments at runtime and passes all 289 tests. The `[ExcludeFromCodeCoverage]` non-exempt denominator
is computed by excluding annotated member source-line ranges from the per-line hit data.

## Numeric coverage (affected QfcItemController non-exempt denominator)

| Partial | non-exempt covered/total | % |
|---|---:|---:|
| QfcItemController.cs | 72/73 | 98.63% |
| QfcItemController.Conversation.cs | 51/65 | 78.46% |
| QfcItemController.EventHandlers.cs | 53/68 | 77.94% |
| QfcItemController.EventWiring.cs | 156/199 | 78.39% |
| QfcItemController.FocusAndTheme.cs | 81/124 | 65.32% |
| QfcItemController.FolderHandling.cs | 26/29 | 89.66% |
| QfcItemController.Initialization.cs | 85/85 | 100.00% |
| QfcItemController.MailActions.cs | 36/45 | 80.00% |
| QfcItemController.Navigation.cs | 47/47 | 100.00% |
| QfcItemController.ViewerSetup.cs | 56/57 | 98.25% |
| AGGREGATE | 663/792 | 83.71% |

- Affected testable non-exempt denominator: 663/792 = **83.71%** (>= 80% AC5 floor met).
- Repo-wide (root, single-assembly instrumentation): 10390/70247 = 14.79% (satisfied-with-exception
  under the #223 authority-scoped precedent; residual uplift tracked under #197).

## Exemption count

- Starting (P0-T7): 103.
- After Phase 5: **57** (46 members de-exempted this phase and each covered by >= 1 passing test).
- Per-partial residual: Conversation 4, EventHandlers 7, EventWiring 3, FocusAndTheme 6, FolderHandling 4,
  Initialization 7, MailActions 5, Navigation 16, ViewerSetup 5.

## Scope deviations recorded (carried to Phase 7 residual boundary)

- FocusAndTheme: the two synchronous `ToggleFocus()` / `ToggleFocus(ToggleState)` overloads were
  reclassified as bucket-(iii) residuals (not de-exempted). Their entire body executes inside a single
  `_itemViewer.Invoke(...)` delegate that terminates in `_themes[_activeTheme].SetQfcTheme(async:false)`,
  a non-virtual method on the out-of-scope `Theme` collaborator that dispatches to live WinForms controls;
  under Option A (no Theme seam this cycle) the delegate body is not unit-reachable. This matches the
  already-exempt `ToggleFocusAsync` overloads. Plan P5-T8 named these for de-exemption; the barrier is
  genuine, so 12 of the 14 named FocusAndTheme members are de-exempted and 2 are justified residuals.
- MailActions `EnumerateConversation` was de-exempted using a `Mock<MailItem>` stubbing `EntryID` (the
  plan assumed a `_convOriginID` branch that this member does not have); it is covered by a passing test.

Output Summary: 289/289 tests pass (233 regression baseline preserved + 56 new). Affected non-exempt
denominator 663/792 = 83.71% (>= 80%). Exemption count reduced 103 -> 57 (46 de-exempted, each covered).
