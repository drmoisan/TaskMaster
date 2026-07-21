# Batch E — DO-NOT-ALTER Constraint Verification (Derived Engines / Predictors)

Timestamp: 2026-07-19T04-00

Scope: derived engines and predictors, including the co-remediated SpamBayes 4-file partial set and Triage 2-file partial set. Per-file confirmation that only nullability annotations and justified `!` changed; no derived-engine scoring/classification logic change; no SpamBayes/Triage behavioral filtering change; no new `if (x is null) throw` guard on any scoring or COM path. Base/override and interface/implementer nullability kept consistent with the Batch C/D bases.

## SpamBayes partial set (co-remediated together)
- **SpamBayes.cs**: `_globals`/`_classifierGroup` fields and `TypedItem` given `= null!` (set by ctor/InitAsync/builder); `CreateAsync`/`InitAsync`/`CreateEngineAsync` return nullable (existing `return null` non-activated paths). `IsActivated => ClassifierGroup is not null` runtime behavior preserved.
- **SpamBayes.Actions.cs**: `GetDestinationFolder` -> `Folder?` (existing `return null` branches); both `MoveSpamOrHam` locals `Folder? destination` with the existing `is not null` checks unchanged. Junk/Inbox routing thresholds and folder-path comparisons unchanged.
- **SpamBayes.Classify.cs**: base `TokenizeAsync`/`CalculateProbabilityAsync` delegates (assigned in InitAsync alongside ClassifierGroup) invoked with justified `!` and `// why` comments; `(item as MailItem)!` in `TrainCallbackAsync` preserves the pre-existing MailItem assumption. Tokenization/probability/train logic unchanged.
- **SpamBayes.Conditions.cs**: `AsyncAction` lambda's `: null` -> `: null!` (preserves the pre-existing null-Task return under the non-null delegate type). `Condition`/`ConditionLog` filtering and `GetOlItemString` unchanged.

## Triage partial set (co-remediated together)
- **Triage.cs**: `_callbackAsync`/`_classifierGroup`/`_tokenizeAsync` fields, `Globals`, `TypedItem` given `= null!`; `CreateAsync`/`CreateEngineAsync` return nullable; `AsyncAction` `: null!`; `CallbackAsync(item, mostLikely!)` and `TestActionAsync(..., predictedClass!, ...)` absorb the `Prediction<T>.Class` (`T?`) cascade from Batch A. `TokenizeAsync.ThrowIfNull` sites unchanged; classify/train logic and thresholds unchanged.
- **Triage_OlLogic.cs**: `Explorer?`/`View?` locals in `FilterView` (existing `is null` guards); `StripFilter` -> `TreeNode<string>?` (existing `return null` branches); `Parent!` at the four post-`await` first-deref sites (Parent is constructor-assigned and non-null in these Outlook-driven flows). Tree-strip and train/untrain logic unchanged.

## Other Batch E files
- **ActionableClassifierGroup.cs**: `InitAsync`/`CreateEngineAsync` return nullable (base `InitAsync` now `Task<T?>`); `AsyncAction` lambda `?.TestAsync(item)` wrapped `(...)!`; `.Select(x => x.Class!)` and `return filtered!` absorb the `Prediction.Class` cascade. `ProbabilityThreshold`/`!= "None"` filtering unchanged.
- **CategoryClassifierGroup.cs** (>500 lines, NOT split): `Globals`/`CgUtilities`/`ClassifierGroup`/`CategorySetter`/`EngineName`/`TypedItem` given `= null!`; `InitAsync`/`CreateEngineAsync` return nullable; `CreateMissingStagingDataException(string? folderPath = null)`; `.Select(x => x.Class!)`; `AsyncAction` `: null!`. Build/classify logic unchanged.
- **LcppnFolderPredictor.cs**: `predictor.Train(relativePath!, mail!.Tokens ?? ...)` with a `// why` comment — reached only when `relativePath` (derived from `mail`) is non-empty, so `mail` is non-null (net481 `string.IsNullOrEmpty` does not narrow). Training loop logic unchanged.
- **LcppnFolderPredictorStore.cs**: measured null-clean; `#nullable enable` only.
- **OlFolderClassifierGroup.cs**: `_folderPredictorConfig` -> nullable (lazy `??=` resolve pattern), `_mailInfoCollection` -> nullable, `LoadStaging` -> `Task<...?>` (existing `return null`). COM-bound references use the seam's existing structure; folder-predictor flag-gating unchanged.
- **SpamInitTimingProbe.cs**: measured null-clean; `#nullable enable` only.

## Interface co-annotation
- `IFolderPredictor.cs` was NOT co-annotated: the Batch E gate reached zero CS86xx without any CS8766/CS8767 implementer-mismatch, so the interface was not forced. It remains EXCLUDE (per plan, co-annotate only if forced).

Confirmation:
- No `System.Diagnostics.CodeAnalysis` post-condition attribute added.
- Base/override and interface/implementer nullability consistent; no scoring/model math changed; no new `if (x is null) throw` guard on any scoring or COM path (AC3, AC4, AC5).
- The SpamBayes (4-file) and Triage (2-file) partial sets were co-remediated within this batch.
