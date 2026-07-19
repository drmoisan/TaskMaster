# Research: utilitiescs-nullable-email-parsing (Wave-1, issue #370)

- **Epic:** `utilitiescs-nullable-remediation`
- **Depends on:** `utilitiescs-nullable-extensions` (Wave-0, issue #363, spec at
  `docs/features/active/utilitiescs-nullable-extensions/spec.md`)
- **Scope:** `UtilitiesCS/EmailIntelligence/EmailParsingSorting/`,
  `UtilitiesCS/EmailIntelligence/SubjectMap/`, `UtilitiesCS/EmailIntelligence/Ctf/`
- **Researched:** 2026-07-18T22-05
- **Method:** Direct source read of all 25 `.cs` files in the three target directories, plus
  the upstream extensions spec, plus grep of `UtilitiesCS.Test` for existing coverage. No
  compilation was performed (per task instructions); CS86xx risk is inferred from source
  patterns.

## 1. Cluster Inventory

All line counts are from the files as currently checked in (no `#nullable enable` present in
any of the 25 files — confirmed by grep).

### EmailParsingSorting/ (14 files)

| File | ~Lines | Partial? | Generated? | `#nullable enable`? |
|---|---|---|---|---|
| `AutoFile.cs` | 157 | No (static class) | No | No |
| `EmailDataMiner.cs` | 143 | **Yes** — `partial class EmailDataMiner` (ns `UtilitiesCS.EmailIntelligence.Bayesian`) | No | No |
| `EmailDataMiner.FolderExtraction.cs` | 483 | **Yes** — same partial | No | No |
| `EmailDataMiner.Serialization.cs` | 404 | **Yes** — same partial | No | No |
| `EmailDataMiner.Transform.cs` | 410 | **Yes** — same partial | No | No |
| `EmailFiler.cs` | 453 | No | No | No |
| `EmailFilerConfig.cs` | 238 | No | No | No |
| `EmailTokenizer.cs` | 729 | No (contains 2 extra types: `SpamBayesOptions` struct, `CharsetCodebase` class, in the same file) | No | No |
| `IEmailTokenizer.cs` | 17 | No (interface) | No | No |
| `ImageStripper.cs` | 359 | No | No | No |
| `MinedMailInfo.cs` | 129 | No | No | No |
| `MovedMailInfo.cs` | 165 | No | No | No |
| `SortEmail.cs` | 1407 | No (static class) | No | No |
| `TesseractOcrTextExtractor.cs` | 53 | No (contains `IOcrTextExtractor` interface + `TesseractOcrTextExtractor` class) | No | No |

Note: despite living under the `EmailParsingSorting` folder, the four `EmailDataMiner.*`
files declare namespace `UtilitiesCS.EmailIntelligence.Bayesian`, not
`UtilitiesCS.EmailIntelligence.EmailParsingSorting`. This is a pre-existing folder/namespace
mismatch; it has no bearing on annotation work but is worth flagging so the atomic plan does
not assume namespace == folder.

### SubjectMap/ (7 files, 1 excluded)

| File | ~Lines | Partial? | Generated? | `#nullable enable`? |
|---|---|---|---|---|
| `CommonWords.cs` | 93 | No (static class) | No | No |
| `SubjectMapEncoder.cs` | 198 | No | No | No |
| `SubjectMapEntry.cs` | 657 | No | No | No |
| `SubjectMapMetrics.cs` | 31 | **Yes** — `partial class SubjectMapMetrics : Form` | No | No |
| `SubjectMapMetrics.Designer.cs` | 109 | **Yes** — same partial | **Yes — Designer-generated** | N/A — **excluded from remediation** |
| `SubjectMapSco.cs` | 198 | **Yes** — `partial class SubjectMapSco : ConcurrentObservableCollection<SubjectMapEntry>` | No | No |
| `SubjectMapSco.Orchestration.cs` | 273 | **Yes** — same partial | No | No |

### Ctf/ (4 files)

| File | ~Lines | Partial? | Generated? | `#nullable enable`? |
|---|---|---|---|---|
| `CtfIncidence.cs` | 76 | No | No | No — `[Obsolete("Use CtfMapEntry Instead")]` |
| `CtfIncidenceList.cs` | 316 | No | No | No — `[Obsolete("This class is deprecated, use CtfMap instead")]` |
| `CtfMap.cs` | 214 | No | No | No |
| `CtfMapEntry.cs` | 36 | No | No | No |

Total remediation targets: 24 files (25 minus `SubjectMapMetrics.Designer.cs`).

## 2. CS86xx Risk Surface

Ordered roughly from lowest to highest annotation risk.

- **Low risk — plain data holders with backing fields.** `CtfMapEntry.cs`, `CtfIncidence.cs`,
  `MinedMailInfo.cs`, `MovedMailInfo.cs`, `EmailFilerConfig.cs`, `SubjectMapEncoder.cs` follow
  a uniform `private T _field; public T Prop { get => _field; set => _field = value; }`
  pattern. Each has several `string`/reference-type fields with no initializer (implicit
  `null` under oblivious code, `CS8618` "non-nullable field must contain a non-null value"
  under the per-file pragma). These need per-field `?` or constructor-assigned non-null
  values; `MinedMailInfo.Clone()`/`DeepCopy()` and `MovedMailInfo` COM-backed lazy getters
  (`FolderOld`, `MailItem`) already null-check before use and return `null` explicitly, so the
  properties are naturally `Folder?`/`MailItem?`.
- **Low risk — obsolete/[Obsolete] legacy classes.** `CtfIncidence.cs`, `CtfIncidenceList.cs`
  are marked `[Obsolete]`. They still compile today and are exercised by
  `CtfIncidence_Tests.cs` / `CtfIncidenceList_Tests.cs`, so they remain in scope for
  annotation (zero-behavior-change requirement applies equally to obsolete code), but the risk
  of a subtle behavior change is low because the code paths are simple string/list handling.
- **Medium risk — COM interop null-return patterns.** `EmailFilerConfig.TryResolveDestinationFolder()`
  returns `Folder` and explicitly returns `null` in both the not-found and catch branches,
  making it a natural `Folder?` return. `MovedMailInfo.MailItem` getter uses
  `Session.GetItemFromID` inside a try/catch that returns `null` on failure — same pattern.
  `EmailFiler.TryMoveMailItemHelperAsync` returns `(MailItem Original, MailItem Moved)` where
  `Moved` is set to `null` in the catch branch — the tuple element needs to become
  `MailItem?` without breaking the `(MailItem, MailItem)` deconstruction call sites in
  `EmailFiler.ProcessMailHelperAsync` and `TryMoveMailItemForProcessingAsync`.
- **Medium risk — nullable-returning helper methods with `default`/`null` sentinels.**
  `EmailDataMiner.Serialization.cs`: `Deserialize<T>`, `DeserializeFromFolder<T>`,
  `DeserializeAsync<T>` (two overloads) all return `default(T)` when the lookup or file is
  missing — under a `where T : notnull` constraint these need to become `T?` return types
  (unconstrained `T?`), matching the Wave-0 contract's guidance ("unconstrained `TValue` `out`
  parameter or return becomes `out TValue?` / `TValue?`"). `EmailDataMiner.ToMinedMail(IItemInfo[])`
  explicitly returns `?? null` from a LINQ projection.
  `EmailDataMiner.Transform.TryLoadObjectAndGetMemorySize<T>` returns `(default, 0)` in a catch
  branch — the tuple's `T Object` element needs `T?`.
- **Medium risk — LINQ chains over nullable/optional COM collections.**
  `EmailDataMiner.FolderExtraction.cs`: `QueryOlFolders(FolderTreeSnapshot)` uses
  `resolver.TryResolve(node, out var folder) ? folder : null` then `.OfType<MAPIFolder>()` to
  filter nulls — this is already null-safe by construction (`OfType` drops nulls) but the
  lambda's ternary branch typing needs the `out var folder` parameter/return path annotated
  consistently with `IFolderHandleResolver.TryResolve`. `CreateFolderWrapper` similarly
  branches on `resolver.TryResolve(...) && folder is MAPIFolder mapiFolder`.
  `TryResolveMapiHandles(FolderTree, FolderWrapper[])` has a local `FolderWrapper handle = null;`
  that is guaranteed assigned before use via control flow the compiler cannot always prove
  without an initial dummy assignment — likely needs `FolderWrapper? handle = null;` plus a
  guard, or restructuring is out of scope (annotation only per Wave-0/Wave-1 convention: prefer
  `?` and justified `!` over new guard statements).
- **Medium-high risk — `EmailTokenizer.cs` (729 lines, the largest working file).** Heavy use of
  nullable-prone constructs: `MatchCollection matches = default;` (a struct, not directly
  nullable, but assigned from `Matches()` inside try/catch so may remain `default` if an
  exception is thrown and is null-checked afterward with `matches is not null` — `MatchCollection`
  is a reference type wrapping a struct-like sequence, so this is fine as `MatchCollection?`);
  `IEnumerable<string> all_addrs = null;` reassigned conditionally; `Func<string, int> _len = null`
  optional-parameter-with-null-default in `tokenize_word`; `crack_images` is a nullable delegate
  field (`internal Func<string, List<object>, (string texts, HashSet<string> tokens)> crack_images;`)
  set in `setup()` — never assigned inline, so `CS8618` risk on the field itself unless annotated
  `?` or assigned a default in the field initializer; `Tokenize(object obj, IApplicationGlobals globals)`
  throws on `obj is null` but the exception message uses the bare string literal `"obj"` (already
  fine); `msg.Subject is not null` guards used before member access — good existing guards to
  preserve as-is. `crack_content_xyz` computes `charset` via `?.Charset ?? string.Empty` (already
  null-safe, comment explicitly documents the null-safety fix). This file also declares two
  extra types (`SpamBayesOptions`, `CharsetCodebase`) whose public fields (`CharsetCodebase.Name`,
  `.Charset`) are non-nullable `string` fields with no initializer — `CS8618` candidates.
- **Medium risk — `ImageStripper.cs`.** `PIL_decode_parts` has `byte[] bytes = null;`,
  `Image image = null; Bitmap bitmap = null;` all conditionally assigned inside try/catch, then
  null-checked (`image is not null`) before use — natural `?` candidates.
  `GetFrameWithText` initializes `Bitmap imageWithText = null;` and returns it — if no frame
  satisfies the loop condition (defensively unreachable since `frames` should be non-empty by
  construction, but the compiler cannot prove it) the return type may need `Bitmap?` or a
  justified `!`.
- **Medium risk — `SubjectMapEntry.cs` (657 lines, the largest data/logic file in this
  cluster).** Many properties re-derive cached encoded arrays (`_folderEncoded`,
  `_subjectEncoded`) lazily and null-check `_encoder is not null` before invoking `Encode`.
  Constructors chain through several `Init` overloads; some fields (`_folderTokens`,
  `_subjectTokens`, `_folderPath`) are assigned only inside `Init`, not inline, so the compiler
  will flag them as possibly-unassigned reference fields (`CS8618`) despite being effectively
  guaranteed non-null after any constructor path completes — this is the single largest
  candidate for justified `!` or nullable `?` with runtime `ArgumentNullException` guards
  already present (e.g., `Init(string emailFolder, ...)` throws
  `ArgumentNullException` if `_folderPath is null`).
- **Medium risk — `SubjectMapEncoder.cs`.** `_encoder`/`_decoder` are lazily populated
  `IScoDictionaryNew<,>` fields, null-checked before use (`if (_encoder is null) ...`) —
  straightforward `?` candidates. `RebuildEncoding()` throws
  `NullReferenceException` if `_subjectMap is null` — existing guard, keep as-is.
- **Medium risk — `SubjectMapSco.Orchestration.cs`.** `ResolveFolder` returns `null` from a
  ternary (`? mapiFolder : null`) — natural `MAPIFolder?` return. `QueryOlFolders` filters
  `.Where(tuple => tuple.Folder != null)` after the nullable-producing `ResolveFolder` call —
  already null-safe by construction.
- **Low-medium risk — `SortEmail.cs` (1407 lines, static class, almost entirely
  `[ExcludeFromCodeCoverage]`).** Bulk of the file is dead/legacy Outlook-sorting code paths
  (many are duplicative with `EmailFiler`/`EmailFilerConfig`, per the class's own doc comment
  in `EmailFiler.cs`: "It is a rewrite of the original SortEmail static class"). Patterns:
  `Folder olDestination = null;`, `MailItem mailItemTemp = null;`, `string[] strOutput = null;`,
  `string[,] strAryOutput;` (uninitialized local, assigned in one branch only) — mechanical `?`
  annotation work, low logical risk because almost the entire file carries
  `[ExcludeFromCodeCoverage]`, so there is no coverage-regression pressure, only compiler
  diagnostics to clear.
- **Low risk — `AutoFile.cs`.** `Category_IsAlreadySelected(dynamic objItem, string strCat)`
  takes `dynamic`, which is exempt from nullable analysis; `AutoFindPeople` builds a
  `List<string>` and indexes `ppl_dict[strTmp]` — no obvious null-return risk beyond the
  existing `strMissing` string accumulation pattern.
- **Low risk — `TesseractOcrTextExtractor.cs`.** Already carries an XML-documented,
  test-seam-friendly interface (`IOcrTextExtractor`) introduced for issue #209
  (OCR-engine-initialization-failure bugfix, referenced directly in the file's doc comment).
  Small (53 lines), no obvious null-prone fields.
- **Low risk — `CommonWords.cs`, `SubjectMapSco.cs`, `CtfMap.cs`, `CtfMapEntry.cs`,
  `EmailFilerConfig.cs`, `IEmailTokenizer.cs`.** Straightforward extension methods / DTOs /
  interface with conventional guard clauses already in place (e.g.
  `currentFolder.ThrowIfNull()` in `EmailFilerConfig.IsDeleteRelevant`).
- **Note — `SubjectMapMetrics.cs` is a WinForms `Form`-derived partial class.** Per the C#
  Unit Test Policy's COM/VSTO/WinForms coverage exemption, WinForms form-derived classes are
  coverage-exempt, but that exemption is about test coverage, not nullable annotation — this
  file is still an annotation target (its sibling `.Designer.cs` is the only file excluded, as
  Designer-generated code). The constructor takes
  `IEnumerable<SubjectMapSco.SummaryMetric> metrics` with no null-guard; low risk given its
  small size (31 lines).

## 3. Partial-Class Batching (must remediate together)

Two partial-class groups exist in this cluster, both requiring a single combined batch so
shared private fields and internal members are annotated consistently:

1. **`EmailDataMiner` (4 files, namespace `UtilitiesCS.EmailIntelligence.Bayesian`):**
   `EmailDataMiner.cs`, `EmailDataMiner.FolderExtraction.cs`,
   `EmailDataMiner.Serialization.cs`, `EmailDataMiner.Transform.cs`. The shared private field
   `_globals` (declared in `EmailDataMiner.cs`) and `_sw` (`SegmentStopWatch _sw = default;`,
   also declared in `EmailDataMiner.cs`) are consumed across all four files' methods
   (`_globals.FS.SpecialFolders`, `_globals.Ol...`, `_sw.LogDuration(...)`). Annotating
   `_globals`/`_sw` in isolation in `EmailDataMiner.cs` without checking usage in the other
   three files would risk an inconsistent contract. Combined line count across the four files
   is ~1440 lines — large but each file individually is under (or just over, at 483) the
   500-line ceiling; no file needs splitting for this batch, but the batch itself is the
   biggest remediation unit in the cluster.
2. **`SubjectMapSco` (2 files, namespace `UtilitiesCS`):** `SubjectMapSco.cs` and
   `SubjectMapSco.Orchestration.cs`. Shared private fields `_commonWords`, `_tokenizerRegex`
   (declared in `SubjectMapSco.cs`) are not directly referenced from
   `SubjectMapSco.Orchestration.cs` in the code read, but the class's public/internal surface
   (`Add`, `Find`, `Serialize` via base class) is exercised together across both files'
   methods (e.g. `RepopulateSubjectMapEntries` in `.Orchestration.cs` calls `this.Add(...)`
   defined in the primary file), so both files must be remediated in the same PR/commit to
   keep the partial type's nullable contract coherent.

`SubjectMapMetrics` / `SubjectMapMetrics.Designer.cs` is also technically a partial-class pair,
but the Designer file is generated and excluded from remediation entirely; only
`SubjectMapMetrics.cs` needs the pragma. No batching constraint arises because the Designer
file carries no `#nullable` state to reconcile.

No other files in this cluster are partial.

## 4. Upstream Dependency Mapping (issue #363 contracts)

Grep of the 25 cluster files for `UtilitiesCS.Extensions` usages and their call sites
(`ThrowIfNull`, `ThrowIfNullOrEmpty`, `IsNullOrEmpty`, `Transpose`) shows the cluster consumes
exactly four extension surfaces from `UtilitiesCS/Extensions/`, matching three of the Wave-0
batches documented in `docs/features/active/utilitiescs-nullable-extensions/spec.md`:

- **`NullExtensions.cs`** (Wave-0 "Verify-only" — already `#nullable enable`, confirmed by
  direct read: file opens with `#nullable enable` at line 12 inside the namespace block).
  Consumed via `ThrowIfNull<T>(this T? argument, ...)` and the three `ThrowIfNullOrEmpty`
  overloads. Call sites: `EmailFiler.cs` (`Config.ThrowIfNull`, `Globals.ThrowIfNull`,
  `mailHelpers.ThrowIfNullOrEmpty`, `MailHelpers.ThrowIfNullOrEmpty`),
  `EmailFilerConfig.cs` (`currentFolder.ThrowIfNull()`),
  `EmailDataMiner.Serialization.cs` (`loader.ThrowIfNull()`). Because this file is already
  annotated and verify-only in Wave-0, no re-annotation ordering risk exists here — the
  contract is fixed and this cluster only needs to consume it correctly (e.g. treat
  `ThrowIfNull`'s return as the non-null-asserted `T`, not `T?`).
- **`StringExtensions.cs`** (Wave-0 **Batch B**: "string / serialization / image-stream
  utilities"). Consumed via `string.IsNullOrEmpty()` extension (`this string str`). Call
  sites: `EmailDataMiner.Transform.cs`, `EmailDataMiner.Serialization.cs` (3 call sites),
  `EmailFiler.cs`, `EmailTokenizer.cs`, `ImageStripper.cs` (2 call sites), `SortEmail.cs`
  (3 call sites). This is the most heavily consumed single extension method in the cluster.
- **`IEnumerableExtensions.cs`** (Wave-0 **Batch C**: "core generic collection contracts;
  must precede Batch E"). Consumed via `Transpose<T>(this IEnumerable<IEnumerable<T>> source)`
  — used twice in `EmailTokenizer.cs` (`commonprefix`, `commonsuffix` helper methods, both via
  `.Transpose()` after a `.Select(...)` projection). `Transpose`'s current signature has no
  nullable annotations at all (unconstrained `T`, non-nullable `IEnumerable<...>` parameter and
  return) — Wave-0 must decide whether the sequence-of-sequences parameter can itself contain
  null inner sequences; this cluster's two call sites always pass non-null `IEnumerable<string>`
  projections, so no null-forwarding risk from this cluster's usage, but the annotation choice
  is still a Wave-0 decision this cluster inherits as-is.
- **`IListExtensions.cs`** (Wave-0 **Batch C**). Declares `IsNullOrEmpty(this IList<string> list)`
  but no call site to this specific overload was found in the cluster (the `IsNullOrEmpty()`
  calls observed all resolve to `StringExtensions.IsNullOrEmpty(string)` or
  `NullExtensions.IsNullOrEmpty<T>(IEnumerable<T>)` based on the receiver's static type).
  Retained in this section because it is part of the same Wave-0 Batch C group as
  `IEnumerableExtensions.cs` and is plausible for future cluster edits to reach for; no
  current hard dependency.

**Ordering constraint inherited from Wave-0:** the spec states Batch C
(`IEnumerableExtensions.cs`, `ArrayExtensions.cs`, `IListExtensions.cs`,
`DictionaryExtensions.cs`) must precede Batch E and is itself described as needing "careful
review." Because this Wave-1 cluster's `EmailTokenizer.cs` directly consumes
`IEnumerableExtensions.Transpose`, the atomic plan for this feature should not begin until
Wave-0's Batch C (and, since `NullExtensions.cs` and `StringExtensions.cs` are also consumed
here, Wave-0's verify-only file and Batch B) have merged. The epic's stated Wave dependency
(`#370` depends on `#363`) already encodes this at the feature level; this research confirms
the file-level reason: `Transpose`'s and `IsNullOrEmpty(string)`'s post-remediation signatures
are the actual cross-module contracts `EmailTokenizer.cs` and most of this cluster's other
files will compile against.

## 5. Interop and Constraint Notes

- **Outlook interop dependencies.** Every file in `EmailParsingSorting/` except
  `IEmailTokenizer.cs`, `MinedMailInfo.cs` (uses `IItemInfo`/`IFolderWrapper`/`IRecipientInfo`
  abstractions, not raw interop types), and `TesseractOcrTextExtractor.cs` directly references
  `Microsoft.Office.Interop.Outlook` types: `MailItem`, `Folder`/`MAPIFolder`, `Explorer`,
  `Attachment`, `Application` (via `IApplicationGlobals.Ol.App`). `SubjectMapSco.Orchestration.cs`
  and `CtfMap.cs` also reference `Outlook`/`MAPIFolder`. These COM types cannot be constructed
  in isolation for a compile-only nullable check, but the per-file pragma architecture means no
  live Outlook process is required to remediate annotations — only `msbuild /t:Rebuild` is
  needed, consistent with the Wave-0 toolchain note.
- **`FolderStruct` in `EmailDataMiner.Transform.cs` (line 17-28) is a plain `internal struct`
  using C# 12 primary-constructor syntax** (`internal struct FolderStruct(FolderWrapper
  folderInfo, long cumulativeSize, long chunkNumber, int cumulativeCount)`) with
  property-per-parameter initializers (`public FolderWrapper FolderInfo { get; set; } =
  folderInfo;` etc.). This is **not** a `record struct` — it is a plain `struct` using a
  primary constructor, which is valid on net481/C# 12 (primary constructors for
  non-record types do not require `IsExternalInit`). No CS0518 risk here; do not "simplify" it
  to a `record struct` during remediation, since that would introduce the CS0518 failure the
  Wave-0 spec warns against for `DfDeedle.EmailRecord`. This is the only struct declaration
  found in the cluster; `SpamBayesOptions` in `EmailTokenizer.cs` is also a plain `struct`
  containing only `const` fields (no instance state, no nullable risk).
- **Files exceeding the 500-line limit (pre-existing, flag only, do not fix):**
  - `SortEmail.cs` — 1407 lines (largest in the cluster by a wide margin).
  - `EmailTokenizer.cs` — 729 lines.
  - `SubjectMapEntry.cs` — 657 lines.
  - `EmailDataMiner.FolderExtraction.cs` — 483 lines (under the limit, noted only because it
    is the largest of the four `EmailDataMiner` partial files and close to the ceiling).
  These three overages are pre-existing conditions per the General Code Change Policy's file
  size limit; annotation-only remediation must not split them (splitting would be a refactor,
  out of scope per the same constraint the Wave-0 spec applied to `ArrayExtensions.cs`).
- **Tesseract/OCR external dependency.** `TesseractOcrTextExtractor.cs` wraps the `Tesseract`
  NuGet package's `TesseractEngine`/`EngineMode`/`Page` types. `ImageStripper.cs` depends on it
  only through the `IOcrTextExtractor` seam (constructor-injected, defaulting to
  `new TesseractOcrTextExtractor()` when not supplied) — this seam was introduced for issue
  #209 (Tesseract engine initialization failure bugfix) specifically so `ImageStripper` could
  be unit-tested without a live `TesseractEngine`. `EmailTokenizer.cs` also directly
  `using Tesseract;` but only references it via the `crack_images` delegate's Tesseract-named
  engine string (`"Tesseract"`), not the `TesseractEngine` type itself. No new nullable
  contract is needed for the Tesseract types themselves since they are only touched inside
  `TesseractOcrTextExtractor.ExtractText`, which already has a narrow, already-tested surface.
- **No `record`/`record struct`/`init` usage found anywhere in the 25-file cluster** (confirmed
  by direct read of every file). No CS0518 remediation risk from this cluster beyond the
  general awareness documented above for `FolderStruct`.

## 6. Recommended Remediation Batching (leaf-first, annotation-scope only)

Seeded for the atomic plan; sequencing/task breakdown is the atomic plan's responsibility, not
this research's.

- **Batch A — trivial leaves (DTOs / obsolete / small interfaces), no partial-class
  entanglement:** `IEmailTokenizer.cs`, `TesseractOcrTextExtractor.cs`, `CtfMapEntry.cs`,
  `CtfIncidence.cs`, `MinedMailInfo.cs`, `MovedMailInfo.cs`.
- **Batch B — CTF map and subject-map leaf collections (depend only on Batch A's
  `CtfMapEntry`):** `CtfMap.cs`, `CtfIncidenceList.cs`, `CommonWords.cs`.
- **Batch C — SubjectMap encoding chain (depends on Batch A's none, but internally ordered:
  `SubjectMapEncoder` before `SubjectMapEntry` before `SubjectMapSco`* since `SubjectMapEntry`
  consumes `ISubjectMapEncoder` and `SubjectMapSco` consumes `SubjectMapEntry`):**
  `SubjectMapEncoder.cs`, `SubjectMapEntry.cs`, then the combined partial pair
  `SubjectMapSco.cs` + `SubjectMapSco.Orchestration.cs` (single batch per Section 3), then
  `SubjectMapMetrics.cs` (consumes `SubjectMapSco.SummaryMetric`).
- **Batch D — Email filing/config core (depends on Wave-0's `NullExtensions`/`StringExtensions`
  already being remediated, per Section 4):** `EmailFilerConfig.cs`, then `EmailFiler.cs`
  (constructs/consumes `EmailFilerConfig`).
- **Batch E — Image/OCR/tokenization chain (depends on Batch A's `TesseractOcrTextExtractor`
  and Wave-0's `IEnumerableExtensions.Transpose`/`StringExtensions.IsNullOrEmpty`):**
  `ImageStripper.cs`, then `EmailTokenizer.cs` (constructs `new ImageStripper()` in `setup()`).
- **Batch F — EmailDataMiner partial-class group (single combined batch per Section 3;
  depends on `FolderWrapper`/`MailItemHelper` types outside this cluster, and internally on
  nothing else in this cluster except general `IApplicationGlobals`):**
  `EmailDataMiner.cs` + `EmailDataMiner.FolderExtraction.cs` +
  `EmailDataMiner.Serialization.cs` + `EmailDataMiner.Transform.cs` remediated together.
- **Batch G — Static sorting orchestrators (depend on Batches D/F types, e.g.
  `EmailFilerConfig`/`MovedMailInfo`/`OlFolderClassifierGroup`):** `AutoFile.cs`,
  `SortEmail.cs`.
- **Verify-only note:** none of the 24 targets already carry `#nullable enable`, so there is no
  Wave-1-internal "verify-only" set analogous to Wave-0's
  `IAsyncEnumerableExtensions.cs`/`NullExtensions.cs`; every file in this cluster requires the
  pragma to be added.
- **Exclude:** `SubjectMapMetrics.Designer.cs` (generated, not a remediation target).

## 7. Test Surface (existing coverage, for no-behavior-change / no-coverage-regression checks)

Grep of `UtilitiesCS.Test` confirms an existing test file (or files) for every non-trivial
class in the cluster:

- `AutoFile.cs` → `UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs`
- `EmailDataMiner.*` (all 4 partial files) →
  `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs`,
  `EmailDataMiner_Additional_Tests.cs`, `EmailDataMiner_FolderExtractionCoverage_Tests.cs`,
  `EmailDataMiner_TestSupport.cs`
- `EmailFiler.cs` → `UtilitiesCS.Test/EmailIntelligence/EmailFiler_Tests.cs`,
  `EmailFiler_TestSupport.cs`, and a second copy at
  `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/EmailFiler_Tests.cs` (two test files
  with the same class name in different folders — worth flagging to the atomic plan so it
  does not assume a single canonical test file per production file)
- `EmailFilerConfig.cs` → `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs`
- `EmailTokenizer.cs` → `UtilitiesCS.Test/EmailIntelligence/EmailTokenizerTests.cs` and
  `EmailTokenizer_Tests.cs` (also two files, same note as above)
- `ImageStripper.cs` → `UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs`
- `MinedMailInfo.cs` → `UtilitiesCS.Test/EmailIntelligence/MinedMailInfo_Tests.cs` and
  `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs`
- `MovedMailInfo.cs` → `UtilitiesCS.Test/EmailIntelligence/MovedMailInfo_Tests.cs`
- `SortEmail.cs` → `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs`
- `TesseractOcrTextExtractor.cs` → two copies:
  `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor_Tests.cs`
  (the issue-#209 seam test, per file naming) — only one location found under this exact path;
  no duplicate for this one.
- `CommonWords.cs` → `CommonWords_Test.cs` and `CommonWords_Tests.cs` (two files, same
  duplicate-name note)
- `SubjectMapEncoder.cs` → `SubjectMapEncoder_Tests.cs`
- `SubjectMapEntry.cs` → `SubjectMapEntry_Tests.cs`
- `SubjectMapMetrics.cs` → `SubjectMapMetrics_Tests.cs`
- `SubjectMapSco.cs` / `SubjectMapSco.Orchestration.cs` → `SubjectMapSco_Tests.cs` and
  `SubjectMapSco_Orchestration_Tests.cs` respectively (test files already mirror the
  partial-class split 1:1)
- `CtfMap.cs` → `CtfMap_Tests.cs` and `CtfMapTests.cs` (duplicate-name note); `CtfMapEntry`
  exercised indirectly through these and through
  `UtilitiesCS.Test/EmailIntelligence/Compatibility/CollectionRoundTrip_Tests.cs`
- `CtfIncidence.cs` / `CtfIncidenceList.cs` → `CtfIncidence_Tests.cs`,
  `CtfIncidenceList_Tests.cs`, `CtfIncidenceListTests.cs` (duplicate-name note)

**Duplicate test-file-name observation:** several classes have two test files with the same
class name in different directories (`EmailFiler_Tests.cs`, `MinedMailInfo*Tests.cs`,
`CommonWords_Test(s).cs`, `CtfMap(Tests|_Tests).cs`, `CtfIncidenceList(Tests|_Tests).cs`,
`EmailTokenizer(Tests|_Tests).cs`). This research did not open these files to check for actual
namespace/class-name collisions (MSTest requires unique fully-qualified class names, not
unique file names, so this is not necessarily a build problem) — the atomic plan should run
the existing test assembly once before starting remediation to confirm a clean, deterministic
baseline (test count and pass/fail state) so any regression during remediation is
attributable to an annotation change and not a pre-existing duplicate-test ambiguity.

**Test strategy implication:** because every file in the cluster already has at least one
corresponding test file, and the epic's zero-behavior-change constraint applies, the atomic
plan should:
1. Capture a baseline `vstest.console.exe` run (pass/fail counts and coverage percentage) for
   `UtilitiesCS.Test` before any edit, per the evidence-and-timestamp-conventions skill.
2. After each batch, rerun the same test assembly and diff pass/fail counts and per-file
   changed-line coverage against the baseline — no new failures, no coverage regression on the
   lines touched by that batch.
3. Prefer annotation (`?`) and justified `!` over new `if (x is null) throw` guards, consistent
   with the Wave-0 spec's rationale (new guard statements are executable lines requiring new
   test coverage and risk crossing into behavior change).

## Rejected Alternatives

- **Compiling the project up front to enumerate exact CS86xx diagnostics** was considered
  instead of source-pattern inference, but the task instructions explicitly state "You do NOT
  need to compile; infer from source reading," and a full local `msbuild /t:Rebuild` run is
  reserved for the atomic-executor phase per the C# Unit Test Policy's toolchain ordering.
- **Batching strictly by file-system subdirectory (EmailParsingSorting, then SubjectMap, then
  Ctf)** was considered but rejected in favor of a dependency-ordered batching (Section 6)
  because several cross-directory dependencies exist (e.g. `EmailFiler.cs` in
  `EmailParsingSorting/` calls `Globals.AF.SubjectMap.Add(...)` and `CtfMap`-typed members via
  `IApplicationGlobals`, and `SortEmail.cs` directly references `CtfMap`/`SubjectMap`/
  `MovedMailInfo` types) — a strictly per-directory batching would not reflect the leaf-first
  ordering constraint the Wave-0 spec modeled for its own batches.
