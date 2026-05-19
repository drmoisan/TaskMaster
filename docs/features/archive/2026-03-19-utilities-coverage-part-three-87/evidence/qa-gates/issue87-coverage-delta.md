# Coverage Delta: Issue #87 Clean Branch vs Baseline

- **Timestamp:** 2026-03-27T02:11:49Z
- **Touched Scope:** UtilitiesCS
- **Baseline Repository Coverage:** 70.53%
- **Final Repository Coverage:** 70.41%
- **Baseline UtilitiesCS Coverage:** 69.81%
- **Final UtilitiesCS Coverage:** 69.64%

## Changed Production Files

| File | Baseline Line | Final Line | Delta |
|---|---|---|---|
| `InputBoxViewer.cs` | 85.71% | 85.71% | 0.00% (preserved) |
| `BayesianSerializationHelper.cs` | 73.02% | 73.02% | 0.00% (preserved) |
| `EmailDataMiner.cs` | 5.18% | 5.16% | -0.02% (regressed) |
| `SmithWaterman.cs` | 95.52% | 95.52% | 0.00% (preserved) |
| `DfDeedle.cs` | 10.73% | 10.73% | 0.00% (preserved) |
| `DerivedCompositionConverter_ConcurrentDictionary.cs` | 100.00% | 100.00% | 0.00% (preserved) |
| `RecipientStatic.cs` | 83.18% | 83.18% | 0.00% (preserved) |
| `SmartSerializable.cs` | 91.55% | 91.55% | 0.00% (preserved) |
| `SmartSerializableBase.cs` | 89.85% | 89.85% | 0.00% (preserved) |
| `ScoCollection.cs` | 85.82% | 85.82% | 0.00% (preserved) |
| `ScoSortedDictionary.cs` | 80.84% | 80.84% | 0.00% (preserved) |
| `SerializableList.cs` | 96.32% | 96.32% | 0.00% (preserved) |
| `CSVDictUtilities.cs` | 0.00% | 0.00% | 0.00% (no instrumented lines) |
| `FlattenArray.cs` | 0.00% | 0.00% | 0.00% (no instrumented lines) |
| `StackObjectVB.cs` | 0.00% | 0.00% | 0.00% (no instrumented lines) |

## Changed-Code Coverage

- **Changed Production Files:** `UtilitiesCS/Dialogs/InputBoxViewer.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianSerializationHelper.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs`, `UtilitiesCS/EmailIntelligence/OlFolderTools/OlFolderHelper/SmithWaterman.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/NewtonsoftHelpers/DerivedCompositionConverter_ConcurrentDictionary.cs`, `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs`, `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoCollection.cs`, `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs`, `UtilitiesCS/ReusableTypeClasses/Serializable/SerializableList.cs`, `UtilitiesCS/To Depricate/CSVDictUtilities.cs`, `UtilitiesCS/To Depricate/FlattenArray.cs`, `UtilitiesCS/To Depricate/StackObjectVB.cs`
- **Changed-Code Coverage:** 61.53% baseline line rate (2231 / 3626 covered lines) vs 61.48% final line rate (2231 / 3629 covered lines), a -0.05 percentage-point regression.

## Output Summary

Repository-wide line coverage did **not** remain `>= 80%`; the baseline was already below the repository target at 70.53%, and the clean issue `#87` branch regressed slightly to 70.41% (-0.12 percentage points). UtilitiesCS package line coverage also regressed slightly from 69.81% to 69.64% (-0.17 percentage points). Within the touched issue `#87` production scope, aggregate changed-code line coverage regressed slightly from 61.53% to 61.48% (-0.05 percentage points); all touched files preserved baseline coverage except `EmailDataMiner.cs`, which regressed by 0.02 percentage points because the final clean-branch report includes three additional valid lines with the same 44 covered lines.
