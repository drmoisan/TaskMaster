# Debt 2 — Batch: Bayesian — Baseline

Timestamp: 2026-07-19T06-05
Command: filtered from P2-T1's authoritative full re-grep
(`evidence/remediation-baseline/debt2-fanin-full-regrep.2026-07-19T06-00.md`), which itself
derives from `MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform="Any CPU"
/p:TreatWarningsAsErrors=true`.

Files under `UtilitiesCS/EmailIntelligence/Bayesian/**` (no `Performance` subfolder distinction
needed at scan time beyond what is already listed; both are included below):

| File | Diagnostics |
|---|---|
| `UtilitiesCS\EmailIntelligence\Bayesian\BayesianClassifierGroup.cs` | CS0618:1 |
| `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs` | CS8602:24, CS8604:6 |
| `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianSerializationHelper.cs` | CS0618:1, CS8625:1 |

Per-code totals for this batch: CS8602:24, CS8604:6, CS0618:2, CS8625:1 (3 files, 33 total
diagnostics).
