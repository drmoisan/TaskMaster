# Research Findings — UtilitiesCS/HelperClasses Nullable Remediation (Issue #364 / epic child 9002)

- Timestamp: 2026-07-18T21-45
- Scope: `UtilitiesCS/HelperClasses/` recursive (43 `.cs` files), per-file `#nullable enable` opt-in, annotation/null-safety only.
- Base branch premise: epic integration branch off PR #361 head `20d163ac`.
- Method: static reading of every listed file (a subset read in full; the remainder classified from full or representative reads plus size/pattern). CS86xx counts are static estimates unless a build is stated as required; no build was run in this read-only research.

---

## 0. Verified environment facts

- `UtilitiesCS.csproj`: `LangVersion=12.0`, `TargetFrameworkVersion=v4.8.1` (net481). No `<Nullable>` element (verified by grep — only `LangVersion` and `TargetFrameworkVersion` matched). Nullable is OFF at project level.
- `Directory.Build.targets` (repo root): contains only VSTO signing/manifest logic. No `<Nullable>` property. Verified — there is no directory-level nullable context.
- `.editorconfig`: sets a large set of third-party analyzer IDs (MA/S/RCS/AsyncFixer/RS) to `suggestion`, but contains NO `dotnet_diagnostic.CS86xx.severity` override. CS86xx therefore keep their default `warning` severity and are promoted to errors under `/p:TreatWarningsAsErrors=true`. The nullable-style prefs present (`dotnet_style_coalesce_expression`, `dotnet_style_null_propagation`, `dotnet_style_prefer_is_null_check_over_reference_equality_method`) are all `suggestion` and are IDE style rules, not CS86xx.
- No file under `HelperClasses/` currently carries a `#nullable` pragma (verified by grep — "No matches found"). All 43 files start non-opted-in.
- LangVersion 12 is confirmed in use: primary constructors (`PhysicalDirectoryInfoAdapter(DirectoryInfo directoryInfo) : IDirectoryInfo`, `ShellUtilities()`) and collection expressions (`List<...> result = [];` in `TraceUtility`). Annotations must remain compatible with C# 12 on net481 (no `record struct`/`init`-only pitfalls apply here since no such constructs are introduced).

### Interfaces implemented by the FileSystem cluster live OUTSIDE this child's scope

`IFileInfo`, `IDirectoryInfo`, `IFileSystemInfo` are defined in `UtilitiesCS/Interfaces/IHelperClasses/` (verified), NOT under `HelperClasses/`. They are not in the 43-file scope and receive no pragma in this child. Consequence: when an adapter/wrapper file is opted-in, its members implement interface members that are in an *oblivious* nullable context. Oblivious targets accept either nullable or non-nullable implementing signatures without warning, so the opted-in file can annotate freely, but changing an implementing member's declared return to nullable does NOT ripple a contract onto the (still-oblivious) interface. This is the mechanism that lets `!` (null-forgiving) preserve exact current behavior at the BCL-nullable-to-throwing-ctor boundaries described in section 4.

---

## 1. File inventory: cluster, size, difficulty

Line counts verified via ripgrep line count. Difficulty is a static estimate of nullable-remediation effort (Low / Med / High). "Tests" = a dedicated test file exists in `UtilitiesCS.Test/HelperClasses/` (see section 6).

### Root (11 files)
| File | Lines | Difficulty | Notes |
| --- | --- | --- | --- |
| DvgForm.cs | 26 | Low | Form-derived hand-written partial; only nullable surface is event handler `object sender`. See Designer rule (section 3). |
| DvgForm.Designer.cs | 76 | Special | Designer-generated. `components = null`. See Designer rule (section 3). |
| Initializer.cs | 321 | **High** | Unconstrained generics with `ref T`, `default(T)`, `params object[] dependencies`. Foundational cross-module contract. |
| MergeSortImplementations.cs | 113 | Low-Med | Generic sort (read pending detail; size/pattern → low-med). |
| ObjectSize.cs | 59 | Low-Med | Reflection field walk; `field.GetValue` returns `object?`; recursion already null-guarded. |
| ParamArray.cs | 40 | Low-Med | `_args` field left null by default ctor (CS8618 + NRE risk in `AnyNull()`). |
| PrettyPrint.cs | 677 | **Med + FLAG** | Exceeds the 500-line file limit (pre-existing). DataFrame/Svg/Outlook-interop formatter, already uses `?.`/`?? ""`. Cross-module. |
| ReflectionHelper.cs | 159 | Med | `type = type.BaseType` (Type?), `ex.Types` (Type?[]), returns `List<Type>`; references `TraceUtility.ProjectNames`. |
| SegmentStopWatch.cs | 152 | Low-Med | Timing + dictionary (size/pattern estimate). Tested. |
| SimpleRegex.cs | 75 | Low-Med | Regex helper (estimate). Tested. |
| Tokenizer.cs | 117 | Low-Med | String tokenizing (estimate). Tested. |

### BinaryFlags (1)
| GenericBitwise.cs | 119 | Low-Med | `Func<...> _x = null;` field initializers (CS8625) reassigned in ctor; simplest annotation-only fix is removing the redundant `= null`. Expression-tree compile logic is null-clean. |

### CloningFunctions (3)
| DeepCompare.cs | 32 | Low-Med | `property.GetValue` → `object?`; contract `List<(string, object, object)>` → element `object?`. Uses `DispatchUtility.GetType`. |
| DispatchUtility.cs | 294 | Med-High | COM IDispatch interop. `Type result = null` returned as `Type` (CS8603); `Invoke` returns `object?` from `InvokeMember`; COM interface `out Type typeInfo`, `ref string name`. Public `GetType`/`Invoke` can return null. |
| ObjectCopier.cs | 39 | Low-Med | BinaryFormatter clone; `return default` for null source; `(T)formatter.Deserialize(...)` returns `object?`. Genuine nullable-return decision (`T?`). |

### FileSystem (10)
| DirectoryInfoWrapper.cs | 245 | Med | Delegates to inner `IDirectoryInfo`; `Parent`/`Root` surface BCL-null-vs-oblivious issue (section 4). Public. Tested. |
| FileInfoWrapper.cs | 215 | Med | Mirror of file adapter; `Directory`/`DirectoryName` null-boundary. Public. Tested. |
| FilePathHelper.cs | 494 | **High** | Near 500-line limit. `string` fields initialized to `null` (CS8625 x4), `_filePath = null` writes, `Path.GetDirectoryName` (string?), `object sender` event handler, INotifyPropertyChanged/ICloneable. Cross-module; has a Newtonsoft converter. Highest-risk single file. |
| FileSystemInfoWrapper.cs | 82 | Low | Clean delegating wrapper; ctor already `?? throw`. Public. Tested. |
| MyFileSystemInfo.cs | 168 | Med | `AsDirectory`/`AsFile` use `as` (nullable) but typed non-null; `Length` dereferences `AsFile`; `Equals(object obj)` → `object?`; `==`/`!=` operand nullability. Public (namespace `ObjectListViewDemo`). Tested. |
| PhysicalDirectoryInfoAdapter.cs | 212 | Med | Primary ctor; `Parent`/`Root` pass BCL `DirectoryInfo?` into throwing `DirectoryInfoWrapper` ctor (CS8604). Internal. Tested (PhysicalFileSystemAdapters_Tests). |
| PhysicalFileInfoAdapter.cs | 176 | Med + RISK | `Directory` passes `_fileInfo.Directory` (DirectoryInfo?) into throwing ctor; `DirectoryName` returns string?. Injectable-delegate seam present (do not perturb). Known flaky-test area (section 6). Internal. |
| ShellUtilities.cs | 181 | Med | Instance twin of ShellUtilitiesStatic (primary ctor `ShellUtilities()`); `GetFileIcon` returns null → `Icon?`. P/Invoke. Public (`ObjectListViewDemo`). Tested. |
| ShellUtilitiesStatic.cs | 200 | Med | P/Invoke SHGetFileInfo/ShellExecute; `GetFileIcon` returns null (XML doc already says "or null"); marshaled struct string fields. Public. Tested. |
| SysImageListHelper.cs | 176 | Med-High | Two mutually-exclusive fields `listView`/`treeView` (CS8618); collection-getter properties return null (CS8603); `GetImageIndex` dereferences them. Shell-icon interop via ShellUtilitiesStatic. Public. Tested. |

### Logging (4)
| DebugTextLogger.cs | 53 | Low | (estimate). Tested. |
| DebugTextWriter.cs | 49 | Low | Nested Stream with throw-helpers; no nullable surface. |
| TraceUtility.cs | 399 | Med-High | Reflection/stack-walk heavy; many `MethodBase`/`DeclaringType`/`GetMethod()` nullable dereferences; `Pop<T>` returns `default`; lazy `_projectNames`. Extension methods are cross-module (consumed by ReflectionHelper, FilePathHelper). Tested. |
| VerboseLogger.cs | 73 | Low-Med | Generic `VerboseLogger<T>`; `.ToDictionary()`; ConcurrentDictionary. Tested. |

### ThemeHelpers (4)
| SystemThemeDetector.cs | 71 | Low-Med | Registry read; `OpenSubKey` (RegistryKey?) and `GetValue` (object?) assigned to non-null locals (CS8600), both guarded. Well-structured TryGet. Tested. |
| Theme.cs | 457 | **High** | Partial class; ~44-parameter ctor with `IUiDispatcher uiDispatcher = null` / `Action<string> ... = null` defaults (CS8625); many reference-type fields; Outlook-interop + WebView2 + custom interfaces. Known dark-mode theming hotspot. Tested. |
| Theme.Rendering.cs | 132 | Med | Same partial type as Theme.cs — MUST be opted-in together (see section 3 note). |
| ThemeControlGroup.cs | 335 | Med | Control grouping over WinForms controls (estimate). Tested via ThemeTests. |

### ToolTips (2)
| QfcTipsDetails.cs | 275 | Med | Tips detail model/controls (estimate). Tested. |
| TipsController.cs | 191 | Med | Namespace `TaskVisualization`; `_labelControl.Parent` cast to TLP/Panel (CS8600/CS8602); uninitialized `_labelControl`/`_tlp`/`_panel` (CS8618). Tested. |

### Windows Forms (7)
| ControlPosition.cs | 177 | Low-Med | WinForms positioning (estimate). |
| ControlResizer.cs | 251 | Med | `ControlInfo` struct with non-null `string` fields defaulted null (CS8618 in struct-with-ctor scenarios); `ctl.Parent` re-access; broad empty catches. Tested. |
| ImageHelper.cs | 25 | Low | Tiny. |
| MouseDownFilter.cs | 41 | Low | `Form form = null` init reassigned in ctor; `event EventHandler FormClicked` should be `EventHandler?`; invoke already null-safe. Tested. |
| OlvExtension.cs | 24 | Low | Clean extension on `ObjectListView`; no visible nullable surface. Tested. |
| ScreenHelper.cs | 341 | Med | Screen/DPI helpers (estimate). Tested. |
| TableLayoutHelper.cs | 161 | Low-Med | TLP helper (estimate). Tested. |

### WipUnfinished (1)
| ComStreamWrapper.cs | 78 | Low | Global namespace; wraps COM `IStream`; `out STATSTG stat`; fields non-null in ctor. WIP but null-clean. Tested. |

Total: 43 files. Aggregate size ≈ 7,581 code lines (excluding the `DvgForm.resx` resource, which is not in scope).

---

## 2. Cross-module contract sensitivity (research question 2)

The following expose PUBLIC APIs consumed OUTSIDE `HelperClasses/`; their nullable annotations become contracts that downstream epic children (OutlookObjects, EmailIntelligence, Dialogs) and existing callers consume. Annotate to reflect actual null behavior and keep signatures behavior-compatible.

Highest contract sensitivity (annotate deliberately, prefer preserving current runtime behavior):
1. **FilePathHelper** (`UtilitiesCS` root namespace) — widely consumed; has a Newtonsoft converter (`FilePathHelperConverterTests`). The string properties split into two contract classes: `FilePath`/`FolderPath`/`FileName` default to `""` (treat as non-null), while `FileStemSeed`/`FileStemSuffix`/`FileStem`/`FileExtension` are null-by-design sentinels (treat as nullable). Getting this split right is the crux of the file.
2. **Initializer** (`UtilitiesCS`) — `SetAndSave`/`GetOrLoad`/`Load` generic helpers used across modules (doc references `OutlookItem`). `ref T`/`default(T)` returns require `[return: MaybeNull]`/`T?`-style decisions that ripple to every caller.
3. **FileSystem interfaces' implementations** — `DirectoryInfoWrapper`, `FileInfoWrapper`, `PhysicalDirectoryInfoAdapter`, `PhysicalFileInfoAdapter`, `MyFileSystemInfo`, `FileSystemInfoWrapper`. Because the interfaces themselves are out of scope (oblivious), the safe path is to annotate implementations to match current behavior using `!` at the `Parent`/`Root`/`Directory`/`DirectoryName` boundaries (section 4).
4. **TraceUtility** extension methods (`GetMyMethodNames`, `GetMyTraceString`, `GetCallerMethod`, `GetAssembly`) — consumed by ReflectionHelper and FilePathHelper; several return-nullable decisions.
5. **PrettyPrint** (`PrettyPrinters`, `UtilitiesCS`) — `ToFormattedText`/`PrettyText`/`ToMarkdown` formatting extensions used broadly.
6. **ReflectionHelper**, **ParamArray**, **ShellUtilitiesStatic.GetFileIcon** — public but with narrower, self-documenting nullability (GetFileIcon's XML doc already declares nullable).

Internal-only / low contract sensitivity: PhysicalFileInfoAdapter and PhysicalDirectoryInfoAdapter are `internal sealed` (constructed via the public wrappers); ComStreamWrapper, DebugTextWriter, and the WinForms visual helpers are effectively host-internal.

---

## 3. Designer / Form handling rule (research question 3)

Files affected: `DvgForm.cs` (hand-written partial, `Form`-derived) and `DvgForm.Designer.cs` (Designer-generated: `InitializeComponent`, `private IContainer components = null;`, `internal DataGridView Dgv;`).

Recommended concrete rule for the plan:

> For a WinForms Form split into `X.cs` (hand-written) + `X.Designer.cs` (Designer-generated): add `#nullable enable` ONLY to `X.cs`; never add a pragma to, and never hand-edit, `X.Designer.cs`. Bring `X.cs` to zero CS86xx. Because `#nullable enable` is lexical/per-file, the Designer file's members remain in an oblivious context, produce no CS8618/CS8625, and do not cross-block the opted-in hand-written part.

Rationale:
- Designer files are regenerated by the WinForms designer; a manually-added pragma or annotation is stripped on the next design edit, so any fix there is non-durable.
- Hand-editing `InitializeComponent`/generated members risks behavior/layout changes, which the epic and General Code Change Policy prohibit.
- `DgvForm` is one partial type across the two files; a mixed nullable context (one part enabled, one oblivious) is legal and is exactly the desired outcome — opt in the hand-written part, leave the generated part oblivious.
- The only nullable surface in `DvgForm.cs` is the event handler `private void DgvForm_ResizeEnd(object sender, EventArgs e)` → annotate `object? sender` to match the framework delegate.

Scope conflict to FLAG to the maintainer (do not silently resolve): the epic states each of the 43 listed files (which explicitly includes `DvgForm.Designer.cs`) receives a `#nullable enable` pragma and reaches zero CS86xx. That conflicts with the "do not touch Designer files" convention. Two options:
- (a) RECOMMENDED: treat `DvgForm.Designer.cs` as a documented exception that stays non-opted-in (oblivious), and record it in the child's acceptance notes. This keeps the file byte-identical.
- (b) If the maintainer requires all 43 opted-in, the only permitted change is annotating the generated field as `private IContainer? components = null;` (this is exactly what current WinForms templates emit, is annotation-only, and changes no behavior). Even then, avoid touching `InitializeComponent`.

The plan should carry option (a) as default and surface (b) as the maintainer-decision fallback.

Partial-type note (separate from Designer): `Theme.cs` and `Theme.Rendering.cs` are two files of one partial `Theme` type. They should be opted-in within the SAME batch. A partial type with one part enabled and the other oblivious is legal, but for a hand-written type (unlike the generated Designer case) splitting the nullable context invites inconsistent field-null-state analysis across the two files; opting both in together avoids that.

---

## 4. CS86xx hotspot patterns and `!`-vs-guard guidance (research question 4)

- **Reflection / dynamic** (ReflectionHelper, ObjectSize, DeepCompare, TraceUtility, VerboseLogger): `PropertyInfo.GetValue`/`FieldInfo.GetValue` return `object?`; `Type.BaseType`, `MethodBase.DeclaringType`, `StackFrame.GetMethod()` are nullable. These are genuine null sources — prefer real guards / null-conditional (`?.`) and nullable locals (`Type?`, `MethodBase?`). Most of this code already null-checks; the work is largely annotating locals and return types, not adding logic.
- **COM / P-Invoke interop** (DispatchUtility, ComStreamWrapper, ShellUtilities, ShellUtilitiesStatic, SysImageListHelper): COM signatures with `out T`/`ref string` and shell APIs that legitimately return null. `GetFileIcon` genuinely returns null (documented) → annotate return `Icon?` (honest, matches XML doc). DispatchUtility.GetType can return null when `!throwIfNotFound` → annotate `Type?`. For COM interop `out Type typeInfo` the marshaled value is effectively non-null on success; `!` on the post-call read is acceptable and documented.
- **BCL-nullable-into-throwing-ctor (adapters/wrappers)** — the key `!`-justified boundary: `PhysicalFileInfoAdapter.Directory`, `PhysicalDirectoryInfoAdapter.Parent`/`Root`, `FileInfoWrapper`/`DirectoryInfoWrapper` equivalents pass a BCL `DirectoryInfo?`/`FileInfo?` (null at filesystem root) straight into a `*Wrapper` ctor that throws `ArgumentNullException` on null. Current runtime behavior: accessing `Parent` on a root directory throws. To keep zero CS86xx WITHOUT changing behavior, use `!` (e.g., `new DirectoryInfoWrapper(_directoryInfo.Parent!)`) and add a short `// why` comment. Making the member nullable instead would be a contract change (and is blocked from rippling to the oblivious out-of-scope interface). The latent root-parent-throws behavior should be FLAGGED, not fixed here (section 8).
- **Event handlers / WinForms nullable args** (DvgForm, FilePathHelper, MouseDownFilter): `object sender` → `object? sender`; `event EventHandler X;` → `event EventHandler? X;`. Invocations already use `?.Invoke`.
- **`out`/`ref` params** (Initializer `ref T`, FilePathHelper `TryParse... out string`, DispatchUtility `out int dispId`): `out string` params that are always assigned before return are fine; `ref T`/`default(T)` on unconstrained generics need `[return: MaybeNull]`/`T?` decisions.
- **Serialization** (ObjectCopier BinaryFormatter, `GetObjectData`): `Deserialize` returns `object?`; `Clone<T>` returning `default` for null source is a real nullable-return (`T?`).
- **Static caches / fields initialized to null** (GenericBitwise `_and/_not/_or/_xor = null`, TraceUtility `_projectNames`, MouseDownFilter `form = null`, ParamArray `_args`, SysImageListHelper `listView`/`treeView`, ControlResizer/TipsController fields, FilePathHelper string fields, Theme optional-param fields): CS8618/CS8625. Prefer: remove redundant `= null` where the ctor assigns unconditionally (GenericBitwise, MouseDownFilter); annotate genuinely-optional fields nullable (`_args`, `listView`/`treeView`, Theme optional deps); keep lazy-init fields nullable with a non-null accessor or `!` after the guard (`_projectNames`).

`!` justified (behavior-preserving at a proven-non-null or already-throwing boundary): adapter `Parent`/`Root`/`Directory`/`DirectoryName`, COM post-call reads, marshaled struct string fields. Real guard/nullable-annotation required (do NOT paper over with `!`): reflection `GetValue` results, `default(T)` returns, event-arg `sender`, registry/`OpenSubKey` results, uninitialized optional fields.

---

## 5. Recommended batching strategy (research question 5)

43 files in 8 batches, foundational/low-difficulty clusters first, cross-module/high-contract files last. Each batch is subdirectory-cohesive and independently reviewable. Batches are additive (each opts in its files under the pragma and reaches zero CS86xx for those files under the pragma-only verification of section 7).

- **Batch 1 — Root pure/simple helpers (7):** GenericBitwise (BinaryFlags), MergeSortImplementations, ObjectSize, ParamArray, SimpleRegex, Tokenizer, SegmentStopWatch. Low-risk, well-tested leaves; establishes the pragma workflow.
- **Batch 2 — Logging (4):** DebugTextLogger, DebugTextWriter, VerboseLogger, TraceUtility. Three trivial + TraceUtility (med-high reflection). Do TraceUtility here so its extension-method contracts are settled before ReflectionHelper/FilePathHelper consume them.
- **Batch 3 — CloningFunctions + reflection (4):** DeepCompare, ObjectCopier, DispatchUtility, ReflectionHelper. Reflection/COM/serialization hotspots, bounded.
- **Batch 4 — FileSystem wrappers/adapters (6):** FileSystemInfoWrapper, DirectoryInfoWrapper, FileInfoWrapper, PhysicalDirectoryInfoAdapter, PhysicalFileInfoAdapter, MyFileSystemInfo. Review together — they share the BCL-null/oblivious-interface `!` decision (section 4). Contains the known flaky-test file (PhysicalFileInfoAdapter).
- **Batch 5 — COM/P-Invoke + Form/Designer special cases (6):** ShellUtilities, ShellUtilitiesStatic, SysImageListHelper, ComStreamWrapper, DvgForm.cs, DvgForm.Designer.cs. This batch exercises the Designer rule (section 3) and the COM null-forgiving patterns.
- **Batch 6 — Windows Forms cluster (7):** ControlPosition, ControlResizer, ImageHelper, MouseDownFilter, OlvExtension, ScreenHelper, TableLayoutHelper.
- **Batch 7 — ThemeHelpers + ToolTips (6):** SystemThemeDetector, Theme.cs, Theme.Rendering.cs, ThemeControlGroup, QfcTipsDetails, TipsController. Keep Theme.cs + Theme.Rendering.cs together (same partial type).
- **Batch 8 — High-contract finish (3):** Initializer, FilePathHelper, PrettyPrint. The three most contract-sensitive/highest-risk files, done last with full attention.

---

## 6. Existing test coverage and regression risk (research question 6)

`UtilitiesCS.Test/HelperClasses/` contains dedicated tests for the large majority of in-scope files (verified by directory listing). Confirmed test files map to: ComStreamWrapper, DebugTextLogger, DeepCompare, DirectoryInfoWrapper, DispatchUtility, DvgForm, FileInfoWrapper, FilePathHelper, FileSystemInfoWrapper, GenericBitwise, Initializer (+ PropertyInitializerTest), MergeSortImplementations, MyFileSystemInfo, ObjectCopier, ObjectSize, OlvExtension, ParamArray, PhysicalFileSystemAdapters (both physical adapters), PrettyPrint (three files), QfcTipsDetails, ReflectionHelper, SegmentStopWatch, ShellUtilities/ShellUtilitiesStatic (three files), SimpleRegex, SysImageListHelper, TableLayoutHelper, Theme (ThemeHelpers/ subfolder: Theme.DispatcherTests, Theme.MailLabelThemingTests, ThemeTests), TipsController (two files), Tokenizer, TraceUtility, VerboseLogger, and WinForms interaction/layout tests (ScreenHelper, TableLayoutHelper, ControlResizer, ControlPosition, MouseDownFilter, ImageHelper).

Implications:
- The broad existing suite is a strong regression backstop for the "no behavior change / no coverage regression on changed lines" acceptance criteria. Every batch should run the corresponding tests after annotation and require them green and unchanged.
- **Flaky-test risk — PhysicalFileInfoAdapter:** `PhysicalFileInfoAdapter.cs` (lines 12–18) documents an injectable-delegate seam (`_appendText`/`_openByMode`/`_openByModeAndAccess`/`_openWrite`) added specifically so `PhysicalFileSystemAdapters_Tests` can cover write/append delegation deterministically without acquiring real handles on shared files. Nullable annotation of this file must NOT alter the seam fields, the two constructors, or the `?? throw` guards; annotate only. This file (and its sibling PhysicalDirectoryInfoAdapter) carry the highest regression risk in the FileSystem batch.
- Files with no obvious dedicated test (covered only indirectly, if at all): Theme.Rendering.cs (covered via Theme partial-type tests), ThemeControlGroup (via ThemeTests), SystemThemeDetector (via ThemeTests), ControlPosition/ScreenHelper (via WinForms* tests), DvgForm.Designer.cs (generated, not directly tested). Annotation changes to these should be conservative.

---

## 7. Mechanics: pragma vs the gate command (research question 7) — CRITICAL for the plan's QA loop

- A file-level `#nullable enable` pragma sets the nullable annotation+warning context for that file from the pragma to EOF, independent of the project setting. With the project default OFF (verified, section 0), only pragma'd files emit CS86xx.
- The repo's documented type-check command (CLAUDE.md and `.claude/rules/csharp.md`) is `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`. `/p:Nullable=enable` sets the PROJECT-level nullable context to enable, which turns nullable ON for EVERY file in `UtilitiesCS`, not just this child's 43. Running that stock command during this child's local verification would surface the entire epic's ~2131 CS86xx across ~234 files and fail the build for reasons unrelated to issue #364.
- The epic's confirmed architecture requires the gate to STOP passing `/p:Nullable=enable` globally and instead rely on each file's own pragma under `/t:Rebuild /p:TreatWarningsAsErrors=true` (the capstone child 9012 makes that gate change). Local verification for THIS child must therefore use the pragma-only form — build with `/t:Rebuild /p:TreatWarningsAsErrors=true` and WITHOUT `/p:Nullable=enable` — so that only the opted-in files are checked and non-opted files stay oblivious.
- Because `.editorconfig` does not override CS86xx severity, CS86xx are warnings and become errors under `TreatWarningsAsErrors=true`; that is the mechanism that makes an opted-in file's residual nullable diagnostics fail the build. This is the desired enforcement for the 43 files.
- Precise per-file CS86xx counts require a build (not run in this read-only research). The plan should capture a baseline count per batch by building with the pragma-only command after adding each batch's pragmas, then drive each to zero.

FLAG to surface in the plan: this child deliberately deviates from the stock CLAUDE.md `/p:Nullable=enable` verification command, per the epic's per-file architecture and the epic's already-recorded "rules-vs-convention conflict." The plan's QA loop must document this substitution explicitly (use `/t:Rebuild /p:TreatWarningsAsErrors=true` for the type-check stage of this child); it must NOT edit `.claude/rules/*` to resolve it.

---

## 8. Risks requiring maintainer attention (research question 8)

1. **PrettyPrint.cs exceeds the 500-line limit (677 lines) — pre-existing.** Annotation-only work adds a pragma and annotations; it cannot bring the file under 500 without a refactor, which is out of scope. Surface as a known, pre-existing policy exception for this file; do not split it in this child.
2. **FilePathHelper.cs is 494 lines (near limit).** Adding a `#nullable enable` line plus annotations may push it over 500. If it crosses the limit, that is an annotation-driven limit breach to flag rather than trigger a refactor.
3. **Adapter/wrapper root-boundary latent behavior.** `PhysicalDirectoryInfoAdapter.Parent`/`Root` and `PhysicalFileInfoAdapter.Directory` throw `ArgumentNullException` when the underlying BCL value is null (filesystem root). The behavior-preserving nullable choice is `!` (documented). The latent "root throws" behavior is a real, pre-existing design question that annotation must expose but not fix — flag for a possible future issue.
4. **DvgForm.Designer.cs scope conflict** (section 3): the epic lists the generated Designer file in scope, but the durable/behavior-safe choice is to leave it non-opted-in. Maintainer decision required (default: exception; fallback: annotate only `components` as `IContainer?`).
5. **Verification-command deviation** (section 7): must use pragma-only build, not the stock `/p:Nullable=enable`. Requires explicit acknowledgment so the atomic-executor does not run the stock command verbatim and interpret the epic-wide failures as this child's regressions.
6. **Contract decisions on unconstrained-generic returns** (Initializer `GetOrLoad`/`Load` returning `default(T)`, ObjectCopier `Clone<T>`): choosing `T?`/`[return: MaybeNull]` changes the annotated public contract consumed by downstream children. These should be reviewed as deliberate contract choices, not mechanical fixes.

None of items 1–6 can be silently resolved within annotation-only scope; each is a flag, not a fix.

---

## 9. Testing implications (no test code written here)

- After each batch: run that batch's corresponding `UtilitiesCS.Test/HelperClasses/` tests (MSTest + Moq + FluentAssertions) and require them green and behavior-identical. Annotations must not change assertions or add/remove tests.
- Use the pragma-only build (section 7) as the type-check gate; require zero CS86xx for the batch's opted-in files.
- Coverage: annotation-only changes should not move covered-line counts materially; the "no coverage regression on changed lines" AC is well-supported by the existing suite. Where a batch touches a file with weak/indirect coverage (Theme.Rendering, ThemeControlGroup, SystemThemeDetector, ControlPosition, ScreenHelper), keep edits minimal and rely on existing partial coverage.
- Preserve the PhysicalFileInfoAdapter injectable-delegate seam exactly (section 6) to avoid reintroducing the known shared-file flakiness.
