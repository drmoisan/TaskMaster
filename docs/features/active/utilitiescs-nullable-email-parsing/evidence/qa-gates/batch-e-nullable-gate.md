# Batch E Nullable Gate (ImageStripper, EmailTokenizer)

Timestamp: 2026-07-19T04-50

## 1. CSharpier

Command: `dotnet tool run csharpier -- format .`

EXIT_CODE: 0

Output Summary: `Formatted 1406 files in 1823ms.` No residual diff after the fix pass.

## 2. Scoped per-file nullable pragma gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 for both Batch E files (AC1 SATISFIED). Build
FAILED with the same 14 pre-existing, non-nullable errors as baseline (`CS0618` x13, `CS0168`
x1 in `AutoFile.cs`) — zero new non-nullable errors introduced by this batch.

## Fixes applied during this batch

- `ImageStripper.cs`: the primary constructor's `cachefile`/`ocrTextExtractor` parameters (and
  the `_cachefile` field) are annotated nullable since two of the four constructors delegate
  with a literal `null`; `_ocrTextExtractor`'s field stays non-nullable (guaranteed by its
  `?? new TesseractOcrTextExtractor()` fallback). `PIL_decode_parts`'s conditionally-assigned
  `byte[]? bytes`, `Image? image`, `Bitmap? bitmap` locals are annotated per the plan's explicit
  direction, with justified `!` at 4 dereferences the compiler cannot narrow through the
  `if (image is not null)`-guarded nested try/catch and null-coalescing reassignment
  (`bitmap = bitmap?.ToRGB();`). `GetFrameWithText`'s return type and `imageWithText` local are
  `Bitmap?` (returns `null` when the multi-frame loop finds no frames), consistent with the
  plan's explicit direction.
- `EmailTokenizer.cs`: `crack_images` delegate field annotated nullable (`setup()`, which
  assigns it, is called from the constructor but the compiler cannot trace assignment through a
  separate method call without a banned post-condition attribute); its one invocation site uses
  justified `!` (always assigned in the single, unconditional code path).
  `tokenize_word`'s `Func<string, int>? _len = null` default parameter, `tokenize_headers`'s
  `IEnumerable<string>? all_addrs = null` and `MatchCollection? matches = default` locals are
  annotated per the plan's explicit direction; each compiles clean without `!` since the
  existing `if (x is null) x = ...;`-then-use pattern narrows locals/parameters reliably. The
  existing `msg.Subject is not null` guards and the `?.Charset ?? string.Empty` null-safe
  pattern in `crack_content_xyz` are unchanged. `CharsetCodebase.Name`/`Charset` fields are
  nullable (populated only via JSON deserialization, not by the explicit parameterless
  constructor); `SpamBayesOptions` remains a plain `struct` of `const` fields (no annotation
  action, no `record`/`record struct` conversion). One additional fix beyond the plan's explicit
  list: the static `charsetCodebases` field's initializer (`JsonExtensions.Deserialize<T>`
  returns `T?`) uses a justified `!` to keep the field itself non-nullable, since it has exactly
  one consumption site and is populated once at type-initialization time from an embedded
  resource that is always present.

Both Batch E files carry `#nullable enable`; `EmailTokenizer.cs` (729 lines) was not split
(pre-existing >500-line condition, per Scope Invariants). No `<Nullable>` element was added to
the csproj (AC2). No post-condition attribute was added.
