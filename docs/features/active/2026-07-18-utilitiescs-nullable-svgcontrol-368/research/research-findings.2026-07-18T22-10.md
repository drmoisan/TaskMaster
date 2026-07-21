# Research: utilitiescs-nullable-svgcontrol (Issue #368) — Wave-0

- Date: 2026-07-18T22-10
- Feature: `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/`
- Epic: `utilitiescs-nullable-remediation` (Wave 0, complexity C2, independent net481 WinForms control project)
- Scope: per-file `#nullable enable` remediation of `SVGControl/` (recursive; 20 `.cs` files total: 12
  hand-authored remediation targets, 3 already-`#nullable enable` verify-only vendored files, 5
  Designer/generated files)
- Method: static reading only (no build permitted in this environment). All null-risk assessments
  are static, derived from reading every file in full.

---

## 1. Enumeration and classification (Research Question 1)

Evidence: `Glob SVGControl/**/*.cs` (20 files) + `SVGControl/SVGControl.csproj` `<Compile Include>`
list (lines 96-134; confirms all 20 files are compiled, no orphans) + per-file line counts via
ripgrep line-count (`Grep pattern="^" output_mode=count`).

| # | File | Lines | Class | Role |
|---|------|------:|---|---|
| 1 | `ISvgResource.cs` | 30 | `ISvgResource` (interface) + `SvgResource` | Resource contract: `Name`/`Data` (byte[]) pair for embedded SVG resources; decorated `[TypeConverter(typeof(SvgResourceConverter))]`. |
| 2 | `SvgRenderer.cs` | 344 | `SvgRenderer : INotifyPropertyChanged` | Core rendering engine: owns the `SvgDocument`, computes proportional/stretch sizing, exposes `Render()` → `Bitmap`. Also hosts a static `AssemblyResolve` shim (lines 20-124) working around an ExCSS version-binding mismatch between production and `vstest`'s testhost. |
| 3 | `ToggleSwitch.cs` | 52 | `ToggleSwitch : CheckBox` | Pure GDI+-painted toggle control; no SVG/Svg-lib dependency. |
| 4 | `SVGParser.cs` | 111 | `SVGParser` (internal) | Standalone SVG-to-`Bitmap` file/byte-array loader with aspect-ratio resize. **Zero internal fan-in** — not referenced by any other file in `SVGControl/` (confirmed by grep; only self-reference and the `.csproj` compile item). Appears to be superseded/dead code relative to `SvgRenderer`. |
| 5 | `SvgResourceConverter.cs` | 41 | `SvgResourceConverter : TypeConverter` | Converts `ISvgResource` → display string for the PropertyGrid. |
| 6 | `DropDownEditor.cs` | 123 | `DropDownEditor : UITypeEditor` | PropertyGrid drop-down editor that lists embedded `.svg` resources from the design-time assembly's `Properties.Resources`. |
| 7 | `SvgImageSelector.cs` | 335 | `AutoSize` (enum) + `SvgImageSelector : INotifyPropertyChanged` | Hub type. Owns `SvgRenderer`, exposes `ImagePath`/`ResourceName`/`AutoSize`/`Size`/`Margin`/`Render()`/`SaveRendering`. Decorated `[TypeConverter(typeof(SvgOptionsConverter))]`, `ResourceName` decorated `[Editor(typeof(DropDownEditor),...)]`, `ImagePath` decorated `[Editor(typeof(SvgFileNameEditor),...)]`. Defines the `AutoSize` enum consumed by `SvgRenderer`, `SVGParser` is independent of it. |
| 8 | `SvgOptionsConverter.cs` | 59 | `SvgOptionsConverter1 : ExpandableObjectConverter` | **Dead/unreferenced class** (see §3). Converts `SvgImageSelector` → display string using `AboluteImagePath`. |
| 9 | `SvgOptionsConverter2.cs` | 73 | `SvgOptionsConverter : ExpandableObjectConverter` | The **live** class actually bound via `[TypeConverter(typeof(SvgOptionsConverter))]` on `SvgImageSelector`. Converts using `ResourceName` instead of `AboluteImagePath`. |
| 10 | `SVGFileNameEditor.cs` | 80 | `SvgFileNameEditor : FileNameEditor` (internal) | File-open dialog editor for `ImagePath`; uses `RelativePath.AbsoluteFromURI` (verify-only file, §2). |
| 11 | `ButtonSVG.cs` | 83 | `ButtonSVG : Button` | WinForms `Button` hosting an `SvgImageSelector` (`ImageSVG` property); repaints via `PropertyChanged`. |
| 12 | `PictureBoxSVG.cs` | 63 | `PictureBoxSVG : PictureBox` | WinForms `PictureBox` hosting an `SvgImageSelector` (`ImageSvg` property, note casing difference from `ButtonSVG.ImageSVG`); same repaint pattern. |

**Verify-only (already `#nullable enable`, vendored BCL-internal helpers):**

| File | Lines | Confirms clean under pragma? |
|---|------:|---|
| `PathInternal.cs` | 251 | YES — entire file body (lines 12-251, inside `namespace SVGControl`) is commented out (a dead `PathInternal7` class). Zero executable lines; trivially zero CS86xx. |
| `RelativePath.cs` | 1678 | YES on nullable grounds — already uses `string?`, `path!`, `[ThreadStatic]`-free `Span<char>`/`unsafe`/P-Invoke correctly annotated. **Flag (pre-existing, not epic scope): 1678 lines vastly exceeds the repo's 500-line file-size limit.** This is the file that actually contains the decompiled .NET `Path.GetFullPath`/`PathInternal` internals plus an embedded `SR` resource-string class (lines 1373-1678) — i.e., the "PathInternal" logic lives inside `RelativePath.cs`, not `PathInternal.cs` (which is dead code). Do not split; annotation-only scope forbids the refactor. |
| `ValueStringBuilder.cs` | 341 | YES — `internal ref partial struct ValueStringBuilder` (confirmed no other file declares a second `partial` part — grep for `partial struct ValueStringBuilder` returns only this file). Nullable-correct: `char[]? _arrayToReturnToPool`, `string? s` params throughout. |

**Designer/generated (do not opt in per AC6 unless required to keep the pragma build clean — none required, see §4):**

| File | Lines | Note |
|---|------:|---|
| `ButtonSVG.Designer.cs` | 43 | `partial class ButtonSVG`; wires `this.ParentChanged += ... ButtonSVG_ParentChanged` (confirms that private handler in `ButtonSVG.cs` is live, not dead). No nullable-sensitive code. |
| `PictureBoxSVG.Designer.cs` | 45 | `partial class PictureBoxSVG`; no nullable-sensitive code. |
| `ToggleSwitch.Designer.cs` | 36 | `partial class ToggleSwitch`; no nullable-sensitive code. |
| `Properties/Resources.Designer.cs` | 83 | Auto-generated `ResXFileCodeGenerator` output (`AutoGen=True`); byte[]/string resource accessors; no nullable-sensitive code. |
| `Properties/AssemblyInfo.cs` | 36 | Assembly-level attributes only (string literals); zero null-flow surface. |

12 hand-authored + 3 verify-only + 5 Designer/generated = 20, matching the `Glob` count exactly and
the `<Compile Include>` list in `SVGControl.csproj` (lines 96-134).

---

## 2. Per-file CS86xx surface characterization (Research Question 2)

Ordered from trivial to highest-effort. All net481 WinForms/`System.ComponentModel`/`System.Drawing.Design`
base-class signatures (`EventHandler`, `PropertyChangedEventHandler`, `TypeConverter.ConvertTo`,
`UITypeEditor.EditValue`/`GetEditStyle`, `IServiceProvider.GetService`) are **null-oblivious** (the
net481 reference assemblies carry no nullable annotations), so overriding them with unannotated
`object`/`string` parameters does not itself produce a CS86xx mismatch — the diagnostic surface is
entirely in each file's own field/local/return-type null-flow, not in matching base signatures.

**Trivial (near-zero or zero CS86xx):**
- `ISvgResource.cs` — auto-properties only (`string Name { get; set; }`, `byte[] Data { get; set; }`), no null literals, no unions with null. Constructor takes non-null args as-is.
- `ToggleSwitch.cs` — pure value-type GDI+ drawing (`Padding`, `Rectangle`, `Brushes`, `SmoothingMode`); the two constructors and `OnPaint` override touch no reference-typed nullable state.
- `SVGParser.cs` — `TargetSize` is a `Size` (struct); the line `if (TargetSize != null) AdjustSize(document);` is a pre-existing dead/always-true value-type null comparison (CS0472-class issue) **unrelated to nullable reference types** — flag as a pre-existing defect, not an epic-scope fix. All methods return `Bitmap`/`SvgDocument` from `Svg.SvgDocument.Open(...)`, which the `Svg` package (oblivious, unannotated) types as non-null; no CS86xx expected.

**Moderate:**
- `SvgResourceConverter.cs` — `value is null` guard already present; cast `(ISvgResource)value` after the null check is safe. Low CS86xx risk (parameter is oblivious `object`).
- `SvgOptionsConverter.cs` (class `SvgOptionsConverter1`, dead — see §3) and `SvgOptionsConverter2.cs` (class `SvgOptionsConverter`, live) — both cast `value as SvgImageSelector` then null-check (`if (image != null)`); once `SvgImageSelector`'s own members (`AboluteImagePath`, `ResourceName`) are annotated in Batch order (§4), these two files consume already-nullable-correct members and need only local `string?`/local-var adjustments — no new guard logic.
- `SVGFileNameEditor.cs` (class `SvgFileNameEditor`) — three of four string fields already use the repo-local idiom `= string.Empty;` inline initializers (`_currentValue`, `_absoluteFilepath`, `_fileName`); the fourth, `private string _appPath;`, has **no** inline initializer even though it is unconditionally set by `SetDevLevelPath()` called from both constructors. Net481 has no `[MemberNotNull]` (§4), so CS8618 will fire on this field unless it either gets the same `= string.Empty;` inline-initializer idiom already used three lines above it, or is typed `string?`. The former is the lower-risk, already-established in-file pattern.
- `DropDownEditor.cs` — `IDesignerHost host = provider.GetService(typeof(IDesignerHost)) as IDesignerHost;` (CS8600, `as` may yield null); `Assembly asm = null;` (CS8600, null-literal into non-nullable) which is provably reassigned to non-null (`typ.Assembly`) on every path that reaches its later use (the `if (typ == null)` branch returns early) — a case where declaring the local `Assembly? asm = null;` lets flow analysis clear it by the point of use with **no new guard code**. The `host` case is different: `host.RootComponentClassName` is dereferenced with no existing guard; because a null `IDesignerHost` would already NRE at that exact line today, casting with `(provider.GetService(typeof(IDesignerHost)) as IDesignerHost)!` (null-forgiving on the `as`-cast result) preserves the exact current runtime behavior (same NRE if actually null) while resolving CS8602, without introducing a new `if (x is null) throw` guard clause. Field `private IWindowsFormsEditorService _editorService;` is never constructor-initialized (assigned mid-method in `EditValue`) — annotate `IWindowsFormsEditorService?`.

**High effort:**
- `SvgRenderer.cs` — `private SvgDocument _doc;` is left unassigned by the `SvgRenderer(Size outer, Padding margin, AutoSize autoSize)` constructor (lines 162-169; the byte[]/`SvgDocument`-accepting overloads all assign it). `Render()` returns `null` when `_doc == null` and also falls through to `return null;` in its final `else` branch (AutoSize value outside the three handled enum members) — return type must become `Bitmap?`. `Document` property getter/setter wraps `_doc`, so its public type must become `SvgDocument?` too (annotation-only; not a breaking change for existing non-null callers — nullable annotations are compile-time metadata, erased at the IL level). `GetSvgDocument(byte[] file)` (static) already `return null;` in its `catch` block — return type `SvgDocument?`. The static `AssemblyResolve` shim (`ResolveByNameAndKey`, `PublicKeyTokensEqual`) uses `byte[] a, byte[] b` params that can be null from `AssemblyName.GetPublicKeyToken()`; both already have explicit `a == null || b == null` handling — annotate as `byte[]?`.
- `SvgImageSelector.cs` — the highest-review-priority file. `private string _relativeImagePath;` and `private string _absoluteImagePath;` are declared with no initializer (CS8618-class); `private ISvgResource _svgResource = null;` explicitly assigns null (CS8600-class) — all three need `?`. **Notable nuance requiring a deliberate judgment call, not a mechanical fix:** the `ImagePath` property's `set` accessor body (lines 84-102) is **entirely commented out** — it is a functional no-op today, meaning `_relativeImagePath` is never actually assigned anywhere in the class's live code path. The `get` accessor's `else return _relativeImagePath;` branch is consequently dead-in-practice but not statically dead, so under nullable analysis it is a real CS8603 (possible null return from a `string`-typed property) that must be resolved without changing observable behavior — options are a null-forgiving `_relativeImagePath!` (with an in-code comment noting the setter is a no-op today) or a `?? "(none)"` fallback (which *would* change the returned value from `null` to `"(none)"` when this dead-ish path is hit, a subtle behavior change AC3 would need to accept or reject). This is the single most consequential remediation decision in the cluster and should be resolved explicitly during planning, not left implicit. `ResourceName` (public property) and `AboluteImagePath` (internal property) both need their exposed types to become nullable (`ISvgResource?`, `string?` respectively) — both are annotation-only changes, non-breaking at the IL level.
- `ButtonSVG.cs` / `PictureBoxSVG.cs` — fields (`_imageSVG`/`_imageSvg`) are always assigned in the constructor, so no CS8618 there. Two **public static** methods carry real null-flow: `ButtonSVG.ObjectToByteArray(Object obj)` already guards `if (obj != null) bf.Serialize(...)`, implying the intended contract is nullable input — annotate `object? obj` (public signature change is annotation-only, non-breaking). Private `GetStringForValue(object value)` (both `ButtonSVG.cs` and, separately, `DropDownEditor.cs` — two independent copies of the same helper, not shared) similarly guards `if (value == null) return "null";` — annotate `object? value`. Event handlers (`ButtonSVG_Resize`, `ImageSVG_PropertyChanged`, `Control_SizeChanged`) stay unannotated `object sender` safely (oblivious framework delegate types, §4).

---

## 3. Ordering / dependency constraints and dead-code flags (Research Question 3)

**Intra-project type dependencies (confirmed by reading, not assumed):**

- `ISvgResource.cs` defines `ISvgResource`/`SvgResource` — consumed by `SvgImageSelector.cs` (`List<ISvgResource> ResourceNames`, `ISvgResource ResourceName`), `SvgResourceConverter.cs` (cast target), `DropDownEditor.cs` (constructs `SvgResource` instances from a resource set). **True leaf, zero in-project dependencies.**
- `SvgRenderer.cs` defines `SvgRenderer` — consumed only by `SvgImageSelector.cs` (`_renderer` field, all ctor overloads). No dependency on `ISvgResource`. **Leaf w.r.t. other SVGControl types; only external dependency is the `Svg` NuGet package.**
- `SvgImageSelector.cs` is the **hub**: defines the `AutoSize` enum (consumed by `SvgRenderer.cs`'s public `AutoSize` property/param and by both `SvgOptionsConverter*.cs` files' `switch` statements) and the `SvgImageSelector` class (consumed by `ButtonSVG.cs`, `PictureBoxSVG.cs`, both `SvgOptionsConverter*.cs` files). It also carries **compile-time-only** type references via attributes to `DropDownEditor` (`[Editor(typeof(DropDownEditor),...)]`) and `SvgFileNameEditor` (`[Editor(typeof(SvgFileNameEditor),...)]`) and to `SvgOptionsConverter` (`[TypeConverter(typeof(SvgOptionsConverter))]`, resolving to the class in `SvgOptionsConverter2.cs` — see dead-code note below). Attribute-argument type references do not create null-flow coupling (they only require the referenced type to compile), so they constrain build order only insofar as `SvgImageSelector.cs` must compile after its own annotation work references correctly-typed members of `ISvgResource`/`SvgRenderer`.
- `SvgOptionsConverter.cs` / `SvgOptionsConverter2.cs` both cast `value as SvgImageSelector` and read `image.AboluteImagePath` / `image.ResourceName` / `image.AutoSize` — **both depend on `SvgImageSelector.cs`'s post-remediation member types.**
- `SVGFileNameEditor.cs` calls `RelativePath.AbsoluteFromURI(...)` — depends on the already-`#nullable enable` verify-only `RelativePath.cs`, not on any in-batch hand-authored file.
- `SVGParser.cs`, `ToggleSwitch.cs`, `ISvgResource.cs`, `SvgResourceConverter.cs`, `DropDownEditor.cs` have **no dependency on `SvgImageSelector.cs`** and can be remediated independently of the hub.

**Partial-class check:** no hand-authored file splits a class across multiple `.cs` files except the
expected WinForms Designer pairs (`ButtonSVG`/`ButtonSVG.Designer`, `PictureBoxSVG`/`PictureBoxSVG.Designer`,
`ToggleSwitch`/`ToggleSwitch.Designer`) and the self-contained `ValueStringBuilder` (`partial struct`
with no second part in the tree, confirmed by grep). Because nullable context is a **per-file**
directive in C#, adding `#nullable enable` to `ButtonSVG.cs` does not force `ButtonSVG.Designer.cs`
(which stays un-opted-in) to be checked — this is why the Designer files require no pragma (§4, AC6).

**Dead-code flags (evidence-based, out of scope to fix — annotate as-is, do not rename/delete):**
- `SvgOptionsConverter.cs` defines class **`SvgOptionsConverter1`**, which is never referenced by any
  `typeof(...)`/attribute in the codebase (grep-confirmed). `SvgImageSelector`'s
  `[TypeConverter(typeof(SvgOptionsConverter))]` attribute resolves to the class in
  `SvgOptionsConverter2.cs` (named `SvgOptionsConverter`, not `SvgOptionsConverter1`). This is a
  pre-existing file-name/class-name mismatch predating this remediation; both files still emit
  CS86xx surface and must both be remediated per AC1, which does not exempt unreferenced classes.
  Do not rename or delete either file/class — that would be a refactor, out of scope.
- `SVGParser.cs` has zero in-project consumers (confirmed by grep across `SVGControl/`); functionally
  superseded by `SvgRenderer.cs` + `SvgImageSelector.cs` for the controls' actual SVG-rendering path.
  Still in scope for AC1 (it is a hand-authored, compiled file).
- `PathInternal.cs` is fully commented-out dead code that already carries `#nullable enable`.
  The real "PathInternal" logic (P/Invoke `GetFullPathName`, `RemoveRelativeSegments`,
  `IsPartiallyQualified`, etc.) lives inside `RelativePath.cs` instead.

**Proposed batch grouping (leaf-first, five cohesive groups covering all 12 hand-authored files):**

| Batch | Files | Rationale |
|---|---|---|
| **A — trivial independent leaves (4)** | `ISvgResource.cs`, `ToggleSwitch.cs`, `SVGParser.cs`, `SvgRenderer.cs` | Zero or near-zero cross-file coupling; `SvgRenderer.cs` is graded "high effort" for its own null-flow (§2) but has **zero dependency on any other SVGControl hand-authored type**, so it belongs with the independent leaves rather than waiting on the hub. Establishes the pragma + csharpier + build loop before touching the hub. |
| **B — ISvgResource consumers, pre-hub (2)** | `SvgResourceConverter.cs`, `DropDownEditor.cs` | Both consume `ISvgResource` (Batch A) only; neither depends on `SvgImageSelector`. Groups the two PropertyGrid-support types that share the `ISvgResource` contract before the hub is touched. |
| **C — the hub (1, CAREFUL REVIEW — isolated on its own)** | `SvgImageSelector.cs` | Defines `AutoSize` (consumed by `SvgRenderer`, already remediated in Batch A, and by Batch D) and is the file with the single highest-consequence judgment call in the cluster (the dead-setter `ImagePath` getter, §2). Isolated as a single-file batch so its review is not diluted by unrelated changes, mirroring the sibling `utilitiescs-nullable-extensions` precedent of isolating the highest-scrutiny contract file(s). |
| **D — SvgImageSelector consumers (3)** | `SvgOptionsConverter.cs`, `SvgOptionsConverter2.cs`, `SVGFileNameEditor.cs` | All three read post-remediation members of `SvgImageSelector` (`AboluteImagePath`, `ResourceName`, `AutoSize`) or (for `SVGFileNameEditor`) the verify-only `RelativePath.cs`; must follow Batch C. |
| **E — WinForms controls, top of the graph (2)** | `ButtonSVG.cs`, `PictureBoxSVG.cs` | Both host an `SvgImageSelector` field and depend on its post-remediation public property types; naturally the last consumers in the dependency chain. |

Verify-only (not a batch): `PathInternal.cs`, `RelativePath.cs`, `ValueStringBuilder.cs` — confirm
still clean under the current pragma; no edits expected.

Not opted in (Designer/generated, per AC6): `ButtonSVG.Designer.cs`, `PictureBoxSVG.Designer.cs`,
`ToggleSwitch.Designer.cs`, `Properties/Resources.Designer.cs`, `Properties/AssemblyInfo.cs`.

---

## 4. Net481 nullable-attribute and syntax constraints (Research Question 4)

**Post-condition attributes are unavailable, exactly as in the sibling `Extensions` cluster.** A
repo-wide search for any *applied* attribute from `[NotNullWhen]`, `[MaybeNullWhen]`,
`[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
`[MemberNotNull]` returns exactly one hit inside `SVGControl/`, and it is commented out:
`SVGControl/PathInternal.cs:225: //        [return: NotNullIfNotNull("path")]` (inside the dead,
fully-commented `PathInternal7` class). No polyfill declaration for any of these attributes exists
in `SVGControl/`. `SVGControl.csproj` has no `ProjectReference` to `UtilitiesCS` (confirmed — the
`.csproj`'s only `<Reference>` items are DLL/NuGet references: ExCSS, Fizzler, log4net, Svg,
System.Buffers, System.Memory, System.Numerics.Vectors, framework assemblies), so `SVGControl/`
cannot even indirectly inherit a polyfill from `UtilitiesCS/Extensions/CompilerServicesExtensions.cs`
— it is genuinely isolated. This reconfirms the epic's cross-cutting finding rather than merely
restating it: the constraint independently holds here even though the two clusters share no code.

**Consequence:** zero CS86xx is reachable using only plain `?`, guard-clause flow narrowing (as with
`DropDownEditor.Assembly asm`, §2), and justified null-forgiving `!` (as with `DropDownEditor.host`
and `SvgImageSelector.ImagePath`'s dead-setter getter, §2) — proven feasible by the already-clean
`RelativePath.cs`/`ValueStringBuilder.cs`, which use exactly this vocabulary (`string?`, `path!`,
`byte[]?`) at BCL-internal complexity and reach zero CS86xx today.

**`record`/`init`/`record struct` trap:** `SVGControl.csproj` sets `<LangVersion>latest</LangVersion>`
on `TargetFrameworkVersion v4.8.1` — same combination as `UtilitiesCS.csproj`. Net481 has no
`System.Runtime.CompilerServices.IsExternalInit` polyfill anywhere in the solution (confirmed absent
in `SVGControl/`), so `init` accessors, positional `record`, and `record struct` all fail CS0518.
This remediation is annotation-only and introduces none of these constructs, but a reviewer should
reject any drive-by "convert to record" temptation — e.g., `ISvgResource.cs`'s `SvgResource` class
(a plain mutable class with `{ get; set; }` auto-properties) must stay a class with settable
properties, not become a `record`.

**What IS available (used correctly already):** all nullable *syntax* under C# `latest`/net481 — `?`
on reference types, null-forgiving `!`, `is null`/`is not null` flow narrowing, unconstrained `T?`
(not exercised in this cluster — no generic methods with unconstrained `T` appear in `SVGControl/`,
unlike the `Extensions` cluster). `[ThreadStatic]` (used in `SvgRenderer.cs` line 33) and
`[CallerMemberName]` (used in `SvgRenderer.cs` line 337, `SvgImageSelector.cs` line 298) are ordinary
BCL attributes available on net481 with no polyfill required — distinct from the
`CallerArgumentExpression` polyfill needed in the sibling `Extensions` cluster (not used anywhere in
`SVGControl/`).

---

## 5. Existing test surface and AC4 posture (Research Question 5)

Evidence: `SVGControl.Test/SVGControl.Test.csproj` — a real MSTest project (`MSTest.TestAdapter
3.1.1`, `MSTest.TestFramework 3.1.1`) with `Moq` and `FluentAssertions` referenced, and a
`<ProjectReference Include="..\SVGControl\SVGControl.csproj">` (lines 84-88), targeting the same
`v4.8.1` framework. This confirms `SVGControl/` **is** covered by an automated MSTest suite in
principle.

**Actual coverage is narrow and does not touch any of the 12 remediation-target files.** The two
`[TestClass]` files in `SVGControl.Test/` — `GetRelativePath_Test.cs` (7 tests, `Assert.AreEqual`
style) and `RelativePathCoverageTests.cs` (16 tests/data-rows, `FluentAssertions` style) — exercise
**only `RelativePath`** (`MakeRelativePath`, `GetRelativeURI`, `AbsoluteFromURI`, `GetFullPath`,
`RemoveRelativeSegments`, `GetRootLength`, `GetExceptionForWin32Error`, `MakeHRFromErrorCode`,
`TryMakeWin32ErrorCodeFromHR`) — i.e., exactly the already-`#nullable enable` verify-only file, not
any of the 12 hand-authored control/parser/converter/editor files in scope for this feature.

`SVGControl.Test/Form1.cs`/`Form1.Designer.cs` and `Form2.cs`/`Form2.Designer.cs` instantiate
`SVGControl.ButtonSVG` (`Form1.Designer.cs:31`, `this.buttonSVG1 = new SVGControl.ButtonSVG();`) and
`SVGControl.PictureBoxSVG` (`Form2.Designer.cs:34`,
`this.pictureBoxSVG1 = new SVGControl.PictureBoxSVG();`) respectively, but neither `Form1` nor `Form2`
carries a `[TestClass]` attribute — they are manual visual-inspection WinForms harnesses, not
executed by `vstest.console.exe`. They provide **no automated coverage signal.**

**Implication for AC4:** for all 12 hand-authored remediation-target files (`ButtonSVG.cs`,
`PictureBoxSVG.cs`, `ToggleSwitch.cs`, `SvgImageSelector.cs`, `SvgRenderer.cs`, `SVGParser.cs`,
`ISvgResource.cs`, `SvgResourceConverter.cs`, `DropDownEditor.cs`, `SvgOptionsConverter.cs`,
`SvgOptionsConverter2.cs`, `SVGFileNameEditor.cs`), the current automated line-coverage baseline is
effectively **0%** — there is nothing to regress, so AC4 ("no coverage regression on changed lines")
is **vacuous as a numeric gate** for these files specifically. This does **not** reduce the
importance of behavior preservation (AC3): with no automated safety net, any judgment call that
risks an observable behavior change (e.g., the `SvgImageSelector.ImagePath` dead-setter nuance, §2)
must be resolved conservatively (prefer `!` over a new fallback value) precisely because a regression
would not be caught by CI and could only be caught by the manual `Form1`/`Form2` visual harnesses if
a developer happens to run them. For `RelativePath.cs` (the one file in `SVGControl/` with a real
test baseline), the existing `RelativePathCoverageTests.cs`/`GetRelativePath_Test.cs` suite provides
genuine AC4 protection; it is verify-only and no edits are expected there.

---

## 6. Toolchain note (Research Question 6)

1. **Format first:** `dotnet tool run csharpier .` (or `csharpier .`). Run before each build; adding
   a `#nullable enable` pragma line and `?`/`!` annotations reformats surrounding code.
2. **Nullable verification — pragma-driven gate, NOT the global flag:**
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
   Under `TreatWarningsAsErrors`, any CS86xx in a pragma-enabled `SVGControl/` file becomes a build
   error while the un-opted-in Designer/generated files and any not-yet-remediated hand-authored
   files elsewhere in the solution stay silent. This is the same gate used by PR #361 and by the
   sibling `utilitiescs-nullable-extensions` research, applied solution-wide because `SVGControl.csproj`
   is part of `TaskMaster.sln`.
3. **Do NOT pass `/p:Nullable=enable` for verification here**, for the identical reason documented
   in the sibling research: the CLAUDE.md / `.claude/rules/csharp.md` toolchain step 3 as literally
   written forces nullable project-wide, which — applied to `SVGControl.csproj` (no `<Nullable>`
   element, AC2) — would surface the full pre-existing CS86xx debt across every not-yet-remediated
   file in `SVGControl/` (and, if run solution-wide, across all ~234 not-yet-remediated `UtilitiesCS/`
   files too) instead of isolating this cluster's own per-file signal. This is the same
   rules-vs-convention conflict the epic flags for the maintainer and defers to the Wave-2 capstone
   (`utilitiescs-nullable-ci-capstone`); it is not resolved here. Verify with the pragma-driven
   `/t:Rebuild /p:TreatWarningsAsErrors=true` gate only.
4. **Analyzer/codestyle step**
   (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) runs as usual; per repo policy new
   analyzer severities remain at `suggestion` and will not break the nullable gate.
5. **Tests:** `vstest.console.exe <SVGControl.Test assembly path> /EnableCodeCoverage`. Per §5, this
   suite exercises only `RelativePath.cs` (verify-only); it will not detect a behavior regression in
   any of the 12 remediation-target files. Treat the absence of a safety net as a reason for
   conservatism (prefer `!` and existing in-file idioms such as `SVGFileNameEditor.cs`'s
   `= string.Empty;` pattern over new runtime guard clauses), not as license for larger edits.

---

## 7. Risks and constraints (summary)

- **Net481 attribute availability:** `[NotNullWhen]`/`[MaybeNullWhen]`/`[MemberNotNull]`/etc. are
  absent and unpolyfilled in `SVGControl/`, and the project has no path to inherit a polyfill (no
  `ProjectReference` to `UtilitiesCS`). Use plain `?`, flow-narrowed locals, and justified `!` only.
- **Designer-file handling:** none of the five Designer/generated files require opt-in; nullable
  context is per-file, so opting in a hand-authored partial-class file does not force its Designer
  counterpart to be checked. Verified no CS86xx-relevant code exists in any of the five.
- **Non-SDK legacy `.csproj`:** `SVGControl.csproj` uses explicit `<Compile Include>` items (`ToolsVersion="15.0"`,
  no SDK-style `<Nullable>` element anywhere) — do not convert project format; source-file edits only.
- **File-size limit pre-existing violation:** `RelativePath.cs` (1678 lines) already exceeds the
  500-line limit; it is verify-only and out of scope to split (would be a refactor).
- **Zero automated coverage baseline for 12 of 12 remediation-target files:** AC4 is numerically
  vacuous for this cluster, which raises rather than lowers the bar for AC3 (behavior-preservation)
  care — prefer annotation and justified `!` over new guard statements, since no CI test would catch
  an inadvertent behavior change; the only fallback detection is the non-automated `Form1`/`Form2`
  visual harnesses in `SVGControl.Test/`.
- **`SvgImageSelector.ImagePath` dead-setter nuance (§2, Batch C):** requires an explicit,
  documented judgment call (null-forgiving `!` vs. a `?? "(none)"` fallback) rather than a mechanical
  fix; flag for planning/review rather than resolving silently.
- **Pre-existing dead-code / naming quirks (do not fix, only annotate):** `SvgOptionsConverter.cs`
  defines the unreferenced class `SvgOptionsConverter1`; `SVGParser.cs` has zero in-project
  consumers; `SvgImageSelector.AboluteImagePath` keeps its pre-existing typo. None block AC1.

---

## 8. Rejected alternatives (brief)

- **Project-level `<Nullable>enable</Nullable>`** — rejected by confirmed architecture (AC2) and the
  epic's per-file opt-in premise; would make this child non-independently-mergeable.
- **Renaming/deleting the dead `SvgOptionsConverter1` class or the unreferenced `SVGParser.cs`** —
  rejected: AC1 requires remediating hand-authored files that emit CS86xx regardless of whether they
  are dead code; renaming/deleting is a refactor and out of scope for an annotation-only feature.
- **Resolving the `SvgImageSelector.ImagePath` dead-setter ambiguity unilaterally in research** —
  rejected: this is a genuine behavior-vs-annotation judgment call (§2, §7) that belongs in planning/
  implementation review, not a static-research artifact; flagging it explicitly is the correct scope
  for this document.
- **One large batch or 12 single-file batches** — rejected: the former is unreviewable for the one
  file with a real judgment call (the hub, `SvgImageSelector.cs`); the latter is excessive churn for
  a C2-complexity, independent-project cluster. The five-batch, leaf-first grouping in §3 isolates
  the highest-scrutiny file (Batch C) on its own while keeping the remaining 11 files in cohesive,
  dependency-ordered groups of 2-4.
