# Batch A — CSharpier + Nullable Pragma Gate

Timestamp: 2026-07-19T01-45

## Step 1 — Formatting

Command: `dotnet tool run csharpier .`

EXIT_CODE: 0

Output Summary: `Checked 1406 files in 4890ms.` No formatting changes required after the Batch A
edits (`ISvgResource.cs`, `ToggleSwitch.cs`, `SVGParser.cs`, `SvgRenderer.cs` were explicitly
formatted with `csharpier format` prior to this check, and the repository-wide `check` confirms
no residual diffs).

## Step 2 — Per-File Nullable Pragma Gate

Command: `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
(WITHOUT `/p:Nullable=enable`; see `evidence/baseline/baseline-nullable-pragma-gate.md` for the
`AnyCPU`-vs-`"Any CPU"` platform-syntax note)

EXIT_CODE: 1 (2 pre-existing, unrelated `CS0649` errors in `SvgImageSelector.cs` — see below)

Output Summary: **Zero nullable diagnostics** for all 4 Batch A files
(`ISvgResource.cs`, `ToggleSwitch.cs`, `SVGParser.cs`, `SvgRenderer.cs`), confirmed via
`grep -oE "CS8[0-9]{3}" <log>` returning no matches (a broader nullable-diagnostic pattern than
the literal `CS86xx` substring, since `SvgResource`'s interface-implementation mismatch surfaced
as `CS8766`, which is in the CS87xx numeric range but is a genuine nullable reference-type
diagnostic; this broader pattern is used for all subsequent batch/final gate checks in this
plan). The only 2 build errors present are the pre-existing, out-of-scope `CS0649` diagnostics in
`SvgImageSelector.cs` (documented in `evidence/baseline/baseline-nullable-pragma-gate.md`),
unrelated to any Batch A file or to nullable reference types.

### Annotations applied (summary)

- `ISvgResource.cs`: `#nullable enable` pragma added. Interface members `ISvgResource.Name`
  (`string?`) and `ISvgResource.Data` (`byte[]?`), and the implementing `SvgResource.Name`
  (`string?`)/`SvgResource.Data` (`byte[]?`) properties, annotated nullable. This was required
  beyond the pragma alone: `SvgResource`'s parameterless constructor never assigns `Name`/`Data`,
  so the properties are genuinely nullable in practice (confirmed no other in-repo call site uses
  the parameterless constructor; `DropDownEditor.cs` line 60 is the only construction site, and it
  always supplies both arguments) — the nullable annotation accurately reflects actual null
  behavior (AC5) without changing runtime behavior (AC3). No post-condition attribute was added;
  the class remains a plain class (no `record`/`init`).
- `ToggleSwitch.cs`: `#nullable enable` pragma added; zero additional annotation changes required
  (matches plan expectation).
- `SVGParser.cs`: `#nullable enable` pragma added; zero additional annotation changes required
  (matches plan expectation). The pre-existing `if (TargetSize != null)` value-type comparison
  (line 28) is unchanged; `SVGParser.cs` is not renamed or deleted despite having zero in-project
  consumers.
- `SvgRenderer.cs`: `#nullable enable` pragma added. `Render()` returns `Bitmap?`; `Document`
  property and backing `_doc` field are `SvgDocument?`; static `GetSvgDocument(byte[] file)`
  returns `SvgDocument?`; `PublicKeyTokensEqual(byte[] a, byte[] b)` parameters are `byte[]?`
  (existing `a == null || b == null` guard unchanged) — all as specified by the plan. Additional
  annotations/justified `!` needed to reach zero nullable diagnostics, beyond the plan's explicit
  list: `_resolving` field (`HashSet<string>?`, reflecting its genuine `[ThreadStatic]`
  lazily-initialized-per-thread null state, already guarded by the pre-existing
  `_resolving ??= new HashSet<string>(...)`); `ResolveByNameAndKey` return type (`Assembly?`,
  since it explicitly returns `null` on two paths); `PropertyChanged` event
  (`PropertyChangedEventHandler?`, matching the pre-existing `?.Invoke` null-conditional call
  already in the file); a justified `!` on the two `byte[]`-argument constructors'
  `_doc = GetSvgDocument(doc)!;` assignments (preserves the pre-existing implicit
  assume-non-null-after-load behavior); a justified `!` on `_doc!.Draw()` inside the `Document`
  setter (the existing `if (value != null)` guard already proves non-nullness, but on `value`
  rather than the field `_doc`, which the compiler's field-narrowing does not carry across); and
  a justified `!` on `_doc!.Children.Add(group);` in the private, currently-unreferenced
  `AddMargins` helper (preserves its pre-existing implicit non-null assumption; the method is not
  renamed or deleted as that would be an out-of-scope refactor). No post-condition attribute was
  added in any case.

All annotation choices above are additive nullability metadata only; no runtime/IL behavior
changed, no new guard/throw statements were introduced, and no public API was removed or renamed.
