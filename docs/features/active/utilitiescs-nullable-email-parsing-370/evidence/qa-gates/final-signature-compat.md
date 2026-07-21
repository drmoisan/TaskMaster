# Final AC5 Signature Compatibility Verification

Timestamp: 2026-07-19T07-40

## Method

Reviewed `git diff df2235bc..HEAD -- UtilitiesCS/EmailIntelligence` (298 insertions, 260
deletions across the 24 cluster files) per-file, cross-referenced against each batch's
qa-gates evidence file, which documents every signature/annotation change made and its
justification. For each file, confirmed the diff contains only: (a) the `#nullable enable`
pragma line, (b) additive `?`/`!` nullability annotations, (c) any explicitly-plan-directed
tuple-element or out-parameter nullability change, and (d) CSharpier reflow of lines whose
length changed due to (a)-(c) — no logic, control-flow, or public-behavior change.

## Per-file confirmation

| File | Signature-level changes (all additive nullability) | Evidence reference |
|---|---|---|
| Ctf/CtfIncidence.cs | none (fields/props annotated `?`) | `batch-a-nullable-gate.md` |
| Ctf/CtfIncidenceList.cs | `TryDequeueIncidence` return → `CtfIncidence?` | `batch-b-nullable-gate.md` |
| Ctf/CtfMap.cs | `TryDequeueEntry` return → `CtfMapEntry?` | `batch-b-nullable-gate.md` |
| Ctf/CtfMapEntry.cs | none (fields/props annotated `?`) | `batch-a-nullable-gate.md` |
| EmailParsingSorting/AutoFile.cs | none (`dynamic` param left unannotated per plan) | `batch-g-nullable-gate.md` |
| EmailParsingSorting/EmailDataMiner.FolderExtraction.cs | `GetOlFolderTree`/`GetOlFolderSnapshotAsync` `ProgressTracker?` param; `TryResolveMapiHandles` `FolderWrapper? handle` local | `batch-f-nullable-gate.md` |
| EmailParsingSorting/EmailDataMiner.Serialization.cs | `Deserialize`/`DeserializeFromFolder`/`DeserializeAsync` (both overloads)/`DeserializeForValidation` → unconstrained `T?`; `TryLoadObjectAndGetMemorySize` tuple `Object` element → `T?` | `batch-f-nullable-gate.md` |
| EmailParsingSorting/EmailDataMiner.Transform.cs | none beyond pragma + local/call-site `!`/`?` (no public signature change) | `batch-f-nullable-gate.md` |
| EmailParsingSorting/EmailDataMiner.cs | `MineEmails` return → `Task<ScBag<MinedMailInfo>?>` | `batch-f-nullable-gate.md` |
| EmailParsingSorting/EmailFiler.cs | `TryMoveMailItemHelperAsync` tuple `Moved` element → `MailItem?` (plan-directed, tuple shape unchanged); nested `MoveMailResult.Moved`/ctor param → `MailItem?` to match | `batch-d-nullable-gate.md`, `batch-g-nullable-gate.md` (1 additional `!` fix) |
| EmailParsingSorting/EmailFilerConfig.cs | `TryResolveDestinationFolder` return → `Folder?` (plan-directed); several properties (`DestinationOlPath`, `Globals`, `FsAncestorEquivalent`, `SaveFsPath`, `DeleteFsPath`, `OriginFolder`, `OriginOlStem`, `DestinationOlFolder`) → `?` | `batch-d-nullable-gate.md` |
| EmailParsingSorting/EmailTokenizer.cs | `tokenize_word`'s `Func<string,int>? _len` default param | `batch-e-nullable-gate.md` |
| EmailParsingSorting/IEmailTokenizer.cs | none (pragma only) | `batch-a-nullable-gate.md` |
| EmailParsingSorting/ImageStripper.cs | constructor `cachefile`/`ocrTextExtractor` params → nullable (already called with literal `null` from other overloads); `GetFrameWithText` return → `Bitmap?` (plan-directed) | `batch-e-nullable-gate.md` |
| EmailParsingSorting/MinedMailInfo.cs | all reference-type properties → `?` (parameterless ctor never sets them) | `batch-a-nullable-gate.md` |
| EmailParsingSorting/MovedMailInfo.cs | `FolderOld`/`MailItem` properties → `Folder?`/`MailItem?` (plan-directed); `UndoMove` return → `MailItem?`; `UndoMoveMessage` return → `string?`; other properties → `?` | `batch-a-nullable-gate.md` |
| EmailParsingSorting/SortEmail.cs | both `ResolvePaths` overloads' `out string deleteFsPath`/`out Folder destinationFolder` → nullable (conditionally-assigned, plan-directed pattern); `GetAttachmentsInfo`/`GetAttachmentsInfoAsync`'s `deleteFsPath` param → nullable to match; `SanitizeArray`'s `string[,]?`/`ref string[]?` params → nullable to match; `InitializeSortToExisting`'s `object? objItem` default param | `batch-g-nullable-gate.md` |
| EmailParsingSorting/TesseractOcrTextExtractor.cs | none (pragma only) | `batch-a-nullable-gate.md` |
| SubjectMap/CommonWords.cs | none (pragma only) | `batch-b-nullable-gate.md` |
| SubjectMap/SubjectMapEncoder.cs | none beyond field annotations (no public method signature change) | `batch-c-nullable-gate.md` |
| SubjectMap/SubjectMapEntry.cs | `CommonWords`/`Folderpath`/`Foldername`/`EmailSubject`/`Encoder`/`FolderWordLengths`/`FolderEncoded`/`SubjectEncoded`/`SubjectWordLengths` properties → `?`; `IsNull(object?, ...)`; `TokensToEncode` return → `string[]?`; 2 `Encode(ISubjectMapEncoder, ...)` overloads' return → `int[]?` | `batch-c-nullable-gate.md` |
| SubjectMap/SubjectMapMetrics.cs | none (pragma only) | `batch-c-nullable-gate.md` |
| SubjectMap/SubjectMapSco.Orchestration.cs | `ResolveFolder` return → `MAPIFolder?` (plan-directed); `Consume<T>`'s `List<T>?` local; `SummaryMetric.FolderName`/`FolderPath` fields → `?`; `summaryMetrics` field → `?` | `batch-c-nullable-gate.md` |
| SubjectMap/SubjectMapSco.cs | `Find(string, string)` return → `SubjectMapEntry?` | `batch-c-nullable-gate.md` |

## Conclusion

Every signature-level change across the 24 files is limited to additive nullability
annotations (`?` on a type, `!` at a call site, or an unconstrained `T?` on a generic return)
that reflect the method's/property's actual, pre-existing null-return or null-input behavior.
No method was renamed, no parameter was added/removed/reordered, no return type's underlying
(non-nullable-annotation) shape changed, and no public API became binary- or source-breaking
for an existing caller that compiles today. This is consistent with the upstream
`utilitiescs-nullable-extensions` contracts (`NullExtensions.ThrowIfNull<T>`,
`StringExtensions.IsNullOrEmpty`, `IEnumerableExtensions.Transpose<T>`) this cluster consumes
(AC5 SATISFIED).
