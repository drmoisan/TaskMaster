# Baseline — Authoritative CS86xx Remediation Set (Measured)

Timestamp: 2026-07-19T00-40

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168` (WITHOUT `/p:Nullable=enable`), run after temporarily adding `#nullable enable` to all 36 REMEDIATE candidate files from the P0-T2 inventory.

EXIT_CODE: 1 (probe build; expected to fail because the probe pragmas surface the CS86xx being measured)

## Output Summary

- 36 REMEDIATE candidates were probed (BOM preserved; pragma inserted after BOM on the 26 BOM files, plain-prepended on the 10 non-BOM files).
- 188 unique CS86xx diagnostics emitted across 30 distinct files.
- 6 candidate files probed clean (zero CS86xx) — they still receive the pragma per plan (zero/near-zero code change).
- Probe pragmas were reverted via `git checkout`; the working tree carries no EmailIntelligence source changes (verified: 0 files still carry the probe pragma).

## Per-code CS86xx totals (unique)
| Code | Count | Meaning |
|---|---|---|
| CS8618 | 68 | Non-nullable field/property/event uninitialized (constructor exit) |
| CS8603 | 43 | Possible null reference return |
| CS8625 | 28 | Cannot convert null literal to non-nullable reference |
| CS8602 | 24 | Dereference of a possibly null reference |
| CS8600 | 16 | Converting null literal or possible null to non-nullable type |
| CS8601 | 4 | Possible null reference assignment |
| CS8619 | 3 | Nullability of reference types in value doesn't match target |
| CS8604 | 2 | Possible null reference argument |
| **Total** | **188** | |

## Authoritative remediation set — 30 CS86xx-emitting files (by batch)

### Batch A (Bayesian pure/contract leaves)
| File | CS86xx |
|---|---|
| Bayesian/Prediction.cs | 1 |
| Bayesian/BayesianClassifierExtensions.cs | 0 (clean; pragma-only) |
| Bayesian/DoNotSerializeContractResolver.cs | 0 (clean; pragma-only) |
| Bayesian/FolderHierarchyNode.cs | 0 (clean; pragma-only) |
| Bayesian/LcppnFolderPredictorConfig.cs | 0 (clean; pragma-only) |

### Batch B (Corpus core)
| File | CS86xx |
|---|---|
| Bayesian/Corpus.cs | 13 |
| Bayesian/CorpusInherit.cs | 10 |

### Batch C (Scoring engine core)
| File | CS86xx |
|---|---|
| Bayesian/BayesianClassifierShared.cs | 7 |
| Bayesian/BayesianClassifierGroup.cs | 1 |
| Bayesian/PerParentClassifier.cs | 1 |
| Bayesian/FolderHierarchyTree.cs | 3 |

### Batch D (Engine bases / generic engines)
| File | CS86xx |
|---|---|
| ClassifierGroups/TristateEngine.cs | 8 |
| ClassifierGroups/ConditionalItemEngine.cs | 2 |
| ClassifierGroups/MulticlassEngine.cs | 4 |
| ClassifierGroups/ManagerAsyncLazy.cs | 6 |
| ClassifierGroups/ClassifierGroupUtilities.cs | 7 |

### Batch E (Derived engines / predictors; partial sets co-remediated)
| File | CS86xx |
|---|---|
| ClassifierGroups/SpamBayes/SpamBayes.cs | 5 |
| ClassifierGroups/SpamBayes/SpamBayes.Actions.cs | 3 |
| ClassifierGroups/SpamBayes/SpamBayes.Classify.cs | 3 |
| ClassifierGroups/SpamBayes/SpamBayes.Conditions.cs | 1 |
| ClassifierGroups/SpamBayes/SpamInitTimingProbe.cs | 0 (clean; pragma-only) |
| ClassifierGroups/Triage/Triage.cs | 4 |
| ClassifierGroups/Triage/Triage_OlLogic.cs | 8 |
| ClassifierGroups/Actionable/ActionableClassifierGroup.cs | 3 |
| ClassifierGroups/Categories/CategoryClassifierGroup.cs | 5 |
| Bayesian/LcppnFolderPredictor.cs | 2 |
| ClassifierGroups/OlFolder/LcppnFolderPredictorStore.cs | 0 (clean; pragma-only) |
| ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs | 4 |

### Batch F (Flags) — CONFIRMED IN SCOPE (5 emitting files)
| File | CS86xx |
|---|---|
| Flags/FlagParser.cs | 15 |
| Flags/FlagClassNoItem.cs | 5 |
| Flags/FlagDetails.cs | 2 |
| Flags/FlagTranslator.cs | 1 |
| Flags/FlagConsolidator.cs | 1 |

### Batch G (Performance tooling) — CONFIRMED IN SCOPE (3 emitting files)
| File | CS86xx |
|---|---|
| Bayesian/Performance/BayesianPerformanceMeasurement.cs | 28 |
| Bayesian/Performance/BayesianMetricTypes.cs | 25 |
| Bayesian/Performance/BayesianSerializationHelper.cs | 10 |

## Already-null-clean candidates (6) — still receive the pragma (pragma-only, zero/near-zero code change)
1. Bayesian/BayesianClassifierExtensions.cs
2. Bayesian/DoNotSerializeContractResolver.cs
3. Bayesian/FolderHierarchyNode.cs
4. Bayesian/LcppnFolderPredictorConfig.cs
5. ClassifierGroups/OlFolder/LcppnFolderPredictorStore.cs
6. ClassifierGroups/SpamBayes/SpamInitTimingProbe.cs

## Scope-boundary determinations
- **Flags/ (Batch F): IN SCOPE.** 5 of 5 REMEDIATE Flags candidates emit CS86xx. `IFlagTranslator.cs` remains EXCLUDE unless a remediated implementer forces CS8766/CS8767.
- **Performance/ (Batch G): IN SCOPE.** 3 of 3 REMEDIATE Performance candidates emit CS86xx. The four Designer/WinForms viewer files remain EXCLUDE.

## Reconciliation vs epic estimate
- Epic planning estimate: ~18 files requiring code edits.
- Research §1 static estimate: ~30–33 candidate files.
- **Measured authoritative set: 30 CS86xx-emitting files** (24 requiring code edits with >=1 CS86xx, plus 6 pragma-only clean files that still carry the pragma). The measured count aligns with the research ~30 static estimate; the epic ~18 figure undercounted the true CS86xx surface. All 36 REMEDIATE candidates (30 emitting + 6 clean) receive the pragma across Batches A–G. The two interface-only files (`IFolderPredictor.cs`, `IFlagTranslator.cs`) and the empty `Bayesian/SpamBayes.cs` stub remain EXCLUDE (co-annotate the interfaces only if forced by an implementer mismatch).
