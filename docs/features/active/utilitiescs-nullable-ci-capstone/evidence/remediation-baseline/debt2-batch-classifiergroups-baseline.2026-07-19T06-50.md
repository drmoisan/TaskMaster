# Debt 2 — Batch: ClassifierGroups — Baseline

Timestamp: 2026-07-19T06-50
Command: filtered from P2-T1's authoritative full re-grep
(`evidence/remediation-baseline/debt2-fanin-full-regrep.2026-07-19T06-00.md`), re-confirmed by
the P2-T3 post-Bayesian-batch rebuild log
(`MSBuild.exe UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU
/p:TreatWarningsAsErrors=true`), which showed these files' diagnostics still present and
unaffected by the Bayesian batch's edits.

Files under `UtilitiesCS/EmailIntelligence/ClassifierGroups/**`:

| File | Diagnostics |
|---|---|
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs` | CS8602:1, CS8604:1, CS8620:1 |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs` | CS8601:1, CS8602:5, CS8604:2, CS8619:1, CS8620:1 |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs` | CS8604:1, CS8625:4 |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs` | CS0618:1, CS8602:1, CS8604:3 |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\MulticlassEngine.cs` | CS8601:1, CS8602:3, CS8619:1 |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs` | CS8601:1, CS8602:4, CS8604:1 |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.Classify.cs` | CS8604:2 |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.Conditions.cs` | CS8602:1 |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage.cs` | CS0618:3 |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs` | CS8602:1 |

Per-code totals for this batch (10 files): CS8602:16, CS8604:10, CS0618:4, CS8620:2, CS8601:2,
CS8619:2, CS8625:4 (40 total diagnostics before this batch's remediation; subject to the same
cascading-diagnostic caveat documented in the Bayesian batch's remediated artifact — a fresh
rebuild after this batch's edits is authoritative).
