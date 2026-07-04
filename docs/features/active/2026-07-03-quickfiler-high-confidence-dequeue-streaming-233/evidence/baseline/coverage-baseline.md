# Numeric Coverage Baseline

- Timestamp: 2026-07-03T19:02:06-04:00
- Issue: 233
- Baseline source: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-results/final.cobertura.xml`
- Coverage source path: repository-approved VSTest `/EnableCodeCoverage` attachment converted with `dotnet-coverage` to Cobertura.
- Original Phase 0 baseline status: unavailable because the literal `vstest.console.exe` command was not on `PATH`.

## Extraction Command

`[xml]$xml = Get-Content -LiteralPath 'docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-results/final.cobertura.xml'; <extract Cobertura coverage, repo-path classes, and changed-file class lines>`

## Baseline Results

- Raw Cobertura coverage: 14997/79844 lines = 18.78%.
- Repository-path class coverage: 12850/57107 lines = 22.50%.
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: 54/56 lines = 96.43%.
- `QuickFiler/Controllers/QfcHomeController.cs`: 156/239 lines = 65.27%.
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: 41/52 lines = 78.85%.
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`: not emitted as a distinct Cobertura class entry in this baseline artifact.

## Baseline Status

Numeric remediation baseline is available for no-regression comparison. The repository-wide coverage floor remains below 80% in this baseline artifact, so final comparison evidence must distinguish no-regression status from the repository floor status.
