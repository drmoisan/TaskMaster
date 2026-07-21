# Final QC — AC5 Signature Compatibility

Timestamp: 2026-07-19T06-35

Method: reviewed `git diff df2235bc -- 'UtilitiesCS/EmailIntelligence/**/*.cs'` (36 source files, +412/-312 lines) and filtered added lines for any change that is not a pragma, nullability annotation, justified `!`, comment, or behavior-preserving `(await …)!` wrap.

## Findings
- Every public/protected signature change is limited to **additive nullability annotations** that reflect actual null behavior:
  - Parameter/return `?` annotations (e.g., `CompareTo(Prediction<T>? other)`, `GetWordInfo(): WordInfo?`, `InitAsync(): Task<T?>`, `SplitToList(string? MainString)`, factory `CreateAsync/CreateEngineAsync` returning nullable, `DeserializeAsync<T>(): Task<T?>`).
  - Unconstrained-generic null-state expressed as `T?` (`Prediction<T>.Class`, `Deserialize<T>(): T?`, `(T? Object, long Size)`).
  - Null-by-default delegate/field members annotated `Func<…>?`/`Action<…>?`/`?` or given `= null!`/`= default!` initializers (which do not change the declared member type).
  - Nullable events (`CollectionChanged?`, `PeopleChanged?`, …).
- The only non-annotation added lines are: (a) `#nullable enable` pragmas; (b) `// why` comments on justified `!`; (c) behavior-preserving `(await DeserializeAsync<…>())!` re-parenthesization; (d) diff-shift artifacts where inserting the pragma after a BOM re-emits the following `using System;` line. No arithmetic, comparison, control-flow, or logic line was changed.

## Base/override and interface/implementer consistency
- Base engine delegate/return annotations (`TristateEngine`, `MulticlassEngine`) were set so the Batch E derived overrides (`SpamBayes`, `Triage`, `Actionable`, `Category`) remained consistent — the per-batch nullable gates reached zero CS86xx with no CS8765/CS8767 base/override or CS8766/CS8767 interface/implementer mismatch. `IFolderPredictor.cs` and `IFlagTranslator.cs` were NOT forced (remain EXCLUDE).
- The `SubBayesianClassifier`/`SubClassifierGroup`/`SubCorpus` test-double override contracts remain intact: the full MSTest suite (5702/5702) passes unchanged, and the `UpdateProbability*` virtual signatures were not modified.

**AC5 SATISFIED.** Public signatures of remediated members remain behavior-compatible; nullability annotations reflect actual null behavior and honor the upstream #363 extension contracts (bare `ThrowIfNull()` remediated by return-capture-equivalent justified `!`, never a `[NotNull]` polyfill or a new throwing guard).
