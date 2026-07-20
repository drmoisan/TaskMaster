# Batch A Nullable Gate (IEmailTokenizer, TesseractOcrTextExtractor, CtfMapEntry, CtfIncidence, MinedMailInfo, MovedMailInfo)

Timestamp: 2026-07-19T01-40

## 1. CSharpier

Command: `dotnet tool run csharpier -- format .`

EXIT_CODE: 0

Output Summary: `Formatted 1406 files in 2562ms.` (second pass after annotation fixes; no residual
diff — only the 6 Batch A files plus reformatting from the pragma/annotation edits changed).

## 2. Scoped per-file nullable pragma gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT `/p:Nullable=enable`; scoped per the pre-existing vendored-`SVGControl` CS0649 condition documented at baseline, P0-T6)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 for all 6 Batch A files (AC1 SATISFIED). Build
FAILED with 14 pre-existing, non-nullable errors, identical in file/line/code to the P0-T6
baseline scoped-gate run (`CS0618` x13, `CS0168` x1 in `AutoFile.cs`) — zero new non-nullable
errors introduced by this batch.

## Fixes applied during this batch (nullable-annotation remediation, no behavior change)

- `CtfIncidence.cs`: constructor `CtfIncidence(string, int, List<string>, List<int>)` now
  assigns `_emailFolders`/`_emailCounts` directly (instead of via the `EmailFolders`/
  `EmailCounts` property setters) so the compiler's constructor-exit definite-assignment
  analysis can see all three constructors leave these two fields non-null. The property
  setters' only side effect (`_field = value`) is unchanged; this is a behavior-identical
  plumbing change, not a refactor of logic (AC3/AC5 preserved).
- `MinedMailInfo.cs` `DeepCopy()`: the three `(T[])x?.Clone()` casts were changed to
  `(T[]?)x?.Clone()` (nullable target of the cast) — casting a possibly-null expression to a
  non-nullable array type raises CS8600 regardless of the assignment target's own nullability;
  casting to the nullable array type removes the false warning without changing runtime
  behavior.
- `MovedMailInfo.cs`: `NotNull(params object[] parameters)` changed to
  `NotNull(params object?[] parameters)` — the method already null-checks each element
  (`!parameters.Any(x => x is null)`), so accepting a nullable-element array is the correct
  reflection of its actual contract; this is an additive nullability annotation on an
  `internal` helper, not a behavior or API-shape change (same underlying `object[]` CLR type).

All 6 Batch A files carry `#nullable enable`. No `<Nullable>` element was added to the csproj
(AC2). No post-condition attribute (`[NotNullWhen]` etc.) was added.
