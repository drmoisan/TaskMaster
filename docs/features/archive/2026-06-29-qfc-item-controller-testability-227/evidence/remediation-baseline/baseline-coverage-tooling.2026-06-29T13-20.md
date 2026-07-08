# Baseline — Coverage Tooling Availability and Prior-Cycle Numeric Headline (P0-T4)

Timestamp: 2026-06-29T13-20

Command: command -v dotnet-coverage ; dotnet-coverage --version ; vswhere.exe -latest -find "**/vstest.console.exe"

EXIT_CODE: 0

## Tooling availability

- `dotnet-coverage`: PRESENT — `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage`, version
  `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`.
- `vstest.console.exe`: PRESENT — located via vswhere at
  `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe`.
- `vswhere.exe`: PRESENT — `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe`.

## Prior-cycle numeric coverage headline (carried baseline)

From `evidence/regression-testing/coverage-delta.2026-06-29T12-50.md` and
`evidence/qa-gates/p8-tests-coverage.2026-06-29T12-40.md`:

- Tests: 233 total; 233 passed; 0 failed.
- Affected testable non-exempt denominator (gate metric, AC5): 484/585 = 82.74% (>= 80% MET).

Per-cluster figures (affected testable non-exempt):

| Cluster file | non-exempt covered/total | % |
|---|---|---|
| QfcItemController.cs (Properties/INotify) | 124/130 | 95.38% |
| QfcItemController.Conversation.cs | 70/100 | 70.00% |
| QfcItemController.EventWiring.cs | 186/242 | 76.86% |
| QfcItemController.FolderHandling.cs | 52/59 | 88.14% |
| QfcItemController.MailActions.cs | 24/24 | 100.00% |
| QfcItemController.Navigation.cs | 28/28 | 100.00% |
| QfcItemController.ViewerSetup.cs | 0/2 | 0.00% |
| AGGREGATE | 484/585 | 82.74% |

## Output Summary

Both coverage tools (`dotnet-coverage` 18.5.2, `vstest.console.exe` via VS18) are present and
usable. The prior-cycle numeric headline is 233/233 tests passing with an affected testable
non-exempt denominator of 484/585 = 82.74%. These values are the consistency reference for Phase 2.
