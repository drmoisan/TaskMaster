# Baseline File Inventory — In-Scope Candidate Enumeration

Timestamp: 2026-07-19T00-05

Scope roots: `UtilitiesCS/EmailIntelligence/Bayesian`, `UtilitiesCS/EmailIntelligence/ClassifierGroups`, `UtilitiesCS/EmailIntelligence/Flags`.
Columns: path | line count | already has `#nullable enable` | research classification (REMEDIATE/EXCLUDE) | batch.

## Bayesian/
| Path | Lines | Pragma | Class | Batch |
|---|---|---|---|---|
| Bayesian/BayesianClassifierExtensions.cs | 96 | no | REMEDIATE | A |
| Bayesian/BayesianClassifierGroup.cs | 515 | no | REMEDIATE (>500, no split) | C |
| Bayesian/BayesianClassifierShared.cs | 1008 | no | REMEDIATE (>500, no split) | C |
| Bayesian/Corpus.cs | 313 | no | REMEDIATE | B |
| Bayesian/CorpusInherit.cs | 297 | no | REMEDIATE | B |
| Bayesian/DoNotSerializeContractResolver.cs | 34 | no | REMEDIATE | A |
| Bayesian/FolderHierarchyNode.cs | 43 | no | REMEDIATE (get-only sealed record, no init) | A |
| Bayesian/FolderHierarchyTree.cs | 235 | no | REMEDIATE | C |
| Bayesian/IFolderPredictor.cs | 45 | no | EXCLUDE (interface-only; co-annotate only if implementer forces CS8767/CS8766) | E |
| Bayesian/LcppnFolderPredictor.cs | 363 | no | REMEDIATE | E |
| Bayesian/LcppnFolderPredictorConfig.cs | 125 | no | REMEDIATE | A |
| Bayesian/PerParentClassifier.cs | 319 | no | REMEDIATE | C |
| Bayesian/Prediction.cs | 45 | no | REMEDIATE | A |
| Bayesian/SpamBayes.cs | 10 | no | EXCLUDE (empty stub `internal class SpamBayes {}`; no CS86xx possible) | — |

## Bayesian/Obsolete/ — all EXCLUDE (dead code)
| Path | Lines | Pragma | Class |
|---|---|---|---|
| Bayesian/Obsolete/BayesianClassifier.cs | 646 | no | EXCLUDE |
| Bayesian/Obsolete/BayesianFilter.cs | 346 | no | EXCLUDE |
| Bayesian/Obsolete/ClassifierGroup.cs | 396 | no | EXCLUDE |
| Bayesian/Obsolete/CorpusExample.cs | 104 | no | EXCLUDE |
| Bayesian/Obsolete/CorpusVectorized_badidea.cs | 222 | no | EXCLUDE |
| Bayesian/Obsolete/DedicatedToken.cs | 59 | YES (pre-existing) | EXCLUDE |

Count: 6 Obsolete files EXCLUDE (matches research §1).

## Bayesian/Performance/
| Path | Lines | Pragma | Class | Batch |
|---|---|---|---|---|
| Bayesian/Performance/BayesianMetricTypes.cs | 198 | no | REMEDIATE (tooling) | G |
| Bayesian/Performance/BayesianPerformanceMeasurement.cs | 1537 | no | REMEDIATE (>500, no split) | G |
| Bayesian/Performance/BayesianSerializationHelper.cs | 351 | no | REMEDIATE (tooling) | G |
| Bayesian/Performance/ConfusionViewer.cs | 20 | no | EXCLUDE (WinForms Form-derived) | — |
| Bayesian/Performance/ConfusionViewer.Designer.cs | 45 | no | EXCLUDE (Designer-generated) | — |
| Bayesian/Performance/MetricChartViewer.cs | 20 | no | EXCLUDE (WinForms Form-derived) | — |
| Bayesian/Performance/MetricChartViewer.Designer.cs | 109 | no | EXCLUDE (Designer-generated) | — |

Count: 4 Performance viewer/Designer files EXCLUDE (matches research §1).

## ClassifierGroups/
| Path | Lines | Pragma | Class | Batch |
|---|---|---|---|---|
| ClassifierGroups/Actionable/ActionableClassifierGroup.cs | 149 | no | REMEDIATE | E |
| ClassifierGroups/Categories/CategoryClassifierGroup.cs | 523 | no | REMEDIATE (>500, no split) | E |
| ClassifierGroups/ClassifierGroupUtilities.cs | 474 | no | REMEDIATE | D |
| ClassifierGroups/ConditionalItemEngine.cs | 46 | no | REMEDIATE | D |
| ClassifierGroups/ManagerAsyncLazy.cs | 343 | no | REMEDIATE | D |
| ClassifierGroups/MulticlassEngine.cs | 458 | no | REMEDIATE (abstract base) | D |
| ClassifierGroups/OlFolder/LcppnFolderPredictorStore.cs | 67 | no | REMEDIATE | E |
| ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs | 346 | no | REMEDIATE | E |
| ClassifierGroups/SpamBayes/SpamBayes.cs | 446 | no | REMEDIATE (partial core) | E |
| ClassifierGroups/SpamBayes/SpamBayes.Actions.cs | 117 | no | REMEDIATE (partial) | E |
| ClassifierGroups/SpamBayes/SpamBayes.Classify.cs | 121 | no | REMEDIATE (partial) | E |
| ClassifierGroups/SpamBayes/SpamBayes.Conditions.cs | 100 | no | REMEDIATE (partial) | E |
| ClassifierGroups/SpamBayes/SpamInitTimingProbe.cs | 81 | no | REMEDIATE | E |
| ClassifierGroups/Triage/Triage.cs | 453 | no | REMEDIATE (partial) | E |
| ClassifierGroups/Triage/Triage_OlLogic.cs | 269 | no | REMEDIATE (partial) | E |
| ClassifierGroups/TristateEngine.cs | 144 | no | REMEDIATE (abstract base) | D |

## Flags/
| Path | Lines | Pragma | Class | Batch |
|---|---|---|---|---|
| Flags/FlagClassNoItem.cs | 239 | no | REMEDIATE | F |
| Flags/FlagConsolidator.cs | 135 | no | REMEDIATE | F |
| Flags/FlagDetails.cs | 217 | no | REMEDIATE | F |
| Flags/FlagParser.cs | 633 | no | REMEDIATE (>500, no split) | F |
| Flags/FlagTranslator.cs | 90 | no | REMEDIATE | F |
| Flags/IFlagTranslator.cs | 21 | no | EXCLUDE (interface-only; co-annotate only if implementer forces mismatch) | F |

## EXCLUDE Confirmation (matches research §1)
- Obsolete/ files: 6 EXCLUDE (dead code). `DedicatedToken.cs` already carries a pre-existing pragma.
- Performance/ Designer + WinForms viewer files: 4 EXCLUDE.
- Interface-only files: `IFolderPredictor.cs`, `IFlagTranslator.cs` EXCLUDE for standalone remediation (co-annotate only if a remediated implementer forces CS8766/CS8767).
- Empty stub: `Bayesian/SpamBayes.cs` (10 lines) EXCLUDE (no CS86xx possible).

REMEDIATE candidate count (code-editing candidates, excluding the two interface-only files and the empty stub): 30 files across Batches A–G. The authoritative CS86xx-emitting subset is MEASURED at P0-T6.
