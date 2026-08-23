# R-2 Gate Decision — `[P1-T7]`

- Task: `[P1-T7]` (appended by `[P1-T8]` and `[P1-T9]`)
- Issue: #418
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-44 (UTC)

Input: `evidence/other/langversion-probe.2026-08-05T01-50.md`

## Step 1 — vacuity check on the `SVGControl` project-reference set

The `SVGControl` project-reference set measured by `[P1-T6]` is **EMPTY** (0 diagnostics whose emitting
project is `SVGControl\SVGControl.csproj`; the reference built successfully and printed
`SVGControl -> ...\SVGControl\bin\Debug\SVGControl.dll`).

**The measurement is therefore NOT vacuous.** `SVGControl.Test` reached its own `CoreCompile`, proven by
the 24 diagnostics emitted from its own three source files. No `SVGControl`-alone build and no re-run of
`[P1-T6]` was required, and none was performed.

Explicit statement as `[P1-T7]` requires: **the `SVGControl` project-reference set was empty; no vacuous
measurement forced a re-run.**

## Step 2 — the branch

Out-of-scope set from `[P1-T6]`: **EMPTY (0 diagnostics).**

Full out-of-scope diagnostic list that drove the decision:

```
(none — zero diagnostics in Form1.cs, Form1.Designer.cs, Form2.cs, Form2.Designer.cs,
 Resources.Designer.cs, Properties\AssemblyInfo.cs, GetRelativePath_Test.cs,
 RelativePathCoverageTests.cs)
```

Branch A's condition — "the out-of-scope set is empty on a non-vacuous measurement" — is satisfied.
Branch B's condition — "the out-of-scope set is non-empty" — is not satisfied and Branch B is therefore
unavailable.

## Outcome token

```
R2_KEEP
```

The `<LangVersion>latest</LangVersion>` property added by `[P1-T5]` **stays in place**. Execution proceeds
to `[P1-T8]` (clear the in-scope diagnostics). `[P1-T9]` creates no potential-feature entry.

Note on the plan's expectation: § Risks item 1 predicted
`R2_REVERTED_OUT_OF_SCOPE_NULLABLE` on the basis of source inspection of the Designer and Resources
files. The measurement contradicts the prediction because Roslyn suppresses nullable diagnostics in
generated code, which the inspection did not account for. `[P1-T7]` is a deterministic gate on the
**measured** partition, so the measured empty out-of-scope set selects Branch A. See
`langversion-probe.2026-08-05T01-50.md` § "Set 2 — out-of-scope" for the mechanism.

## Resulting csproj diff

Command: `git diff --stat -- SVGControl.Test/SVGControl.Test.csproj`

EXIT_CODE: 0

```
 SVGControl.Test/SVGControl.Test.csproj | 1 +
 1 file changed, 1 insertion(+)
```

The single inserted line is `    <LangVersion>latest</LangVersion>`, placed immediately after
`<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` in the project's first `<PropertyGroup>`,
matching the placement in `SVGControl/SVGControl.csproj`. No other change.

## Output Summary

Gate token **`R2_KEEP`**. The measurement was non-vacuous (`SVGControl` project-reference set empty) and
the out-of-scope set was empty, so Branch A applies. `<LangVersion>latest</LangVersion>` is retained;
`git diff --stat` shows exactly one inserted line. `[P1-T8]` must now clear the 24 in-scope diagnostics
and drive the `[P1-T6]` command to `EXIT_CODE: 0`.

---

# `[P1-T8]` — Branch A execution record

Timestamp: 2026-08-05T01-48 (UTC)

Branch A was taken at `[P1-T7]`, so `[P1-T8]` performed the in-scope clearing edits.

## Constraint compliance

- Only the three authorized files were edited: `SVGControl.Test/SvgRendererParseContractTests.cs`,
  `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs`, `SVGControl.Test/SvgRendererNullToleranceTests.cs`.
  All three are in the plan's Scope Lock (`SvgRendererNullToleranceTests.cs` is listed there explicitly
  as an addition made for exactly this branch of the gate).
- **No assertion was changed.** Every `Should()` chain, every asserted value, and every `because` reason
  string is byte-identical to its pre-edit form.
- **No test name was changed.** All 28 existing `[TestMethod]` names are unchanged.
- No test was deleted, no `#pragma warning disable` was added, no `<NoWarn>` was added, and no
  `.editorconfig` severity was changed.

## Note on Design Decision 9 (`?` / `!` tokens)

Design Decision 9 forbids `?` and `!` in test code **this plan authors**, and enumerates the tasks it
binds: `[P1-T12]`, `[P1-T14]`, and `[P1-T15]`. `[P1-T8]` is deliberately not in that enumeration: it
exists to clear nullable diagnostics in *already-authored* test code, which is not achievable without
nullable-aware syntax. The decision's stated reason — that a reverted R-2 would leave the project at
C# 7.3 where `?`/`!` do not compile — cannot apply here, because `[P1-T8]` runs **only** on the
`R2_KEEP` branch, where `<LangVersion>latest</LangVersion>` is permanent. The three tasks that author new
tests remain bound by the rule and contain zero `?` and zero `!` tokens.

## Edits made

The minimum edit at each diagnostic site, chosen to preserve the existing declared types and assertion
text:

`git diff --numstat` for the three files: `4/4`, `3/3`, `19/19` — every edit is a one-for-one line
replacement, so no line was added or removed in any of the three test files.

| File | Sites | Diagnostic(s) cleared | Edit |
|---|---|---|---|
| `SvgAssemblyProbeDirectoryTests.cs` | 4 x `string directory = SvgAssemblyProbe.TryGetDirectoryFromCodeBase(...)` (lines 25, 43, 54, 64) | 4 x `CS8600` | declared type changed to `string?`, matching the helper's declared `string?` return |
| `SvgRendererNullToleranceTests.cs` | 2 x `Bitmap rendered = ...Render()` (lines 60, 87) | 2 x `CS8600` | declared type changed to `Bitmap?`, matching `Render()`'s declared `Bitmap?` return |
| `SvgRendererNullToleranceTests.cs` | `SvgImageSelector selector = null;` (line 108) | 1 x `CS8600` | `= null!` — the local is assigned inside the `Action` under test and asserted non-null afterwards |
| `SvgRendererParseContractTests.cs` | 4 x `SvgRenderer renderer = null;` (lines 36, 60, 90, 110) | 4 x `CS8600` | `= null!`, same shape as above |
| `SvgRendererParseContractTests.cs` | 1 x `SvgDocument document = SvgRenderer.GetSvgDocument(valid);` (line 136) | 1 x `CS8600` | declared type changed to `SvgDocument?`, matching the tolerant member's declared `SvgDocument?` return. `GetSvgDocumentOrThrow` returns non-nullable and needed no change. |
| `SvgRendererParseContractTests.cs` | `GetSvgDocument(null)` (line 148) and `TryGetSvgDocument(null, out _, out _)` (line 162) | 2 x `CS8625` | `null!` — these tests exist precisely to prove the runtime guard raises `ArgumentNullException`, so the null must still be passed |
| `SvgRendererParseContractTests.cs` | 4 x `out SvgDocument document, out Exception error` (lines 180-181, 202-203, 230-231, 317-318) | 8 x `CS8600` | `out SvgDocument? document, out Exception? error`, matching the declared `out SvgDocument?` / `out Exception?` parameters |
| `SvgRendererParseContractTests.cs` | `.Returns((SvgDocument)null)` (line 224) | 1 x `CS8600` + 1 x `CS8625` | `.Returns((SvgDocument)null!)` — the Moq setup must still return null to drive the element-free branch |

Total: 21 `CS8600` + 3 `CS8625` = 24, matching `[P1-T6]`'s measurement exactly.

One in-code comment was corrected in the same pass because `[P1-T5]` made it false: the Arrange comment
in `TryGetSvgDocument_WithInjectedParseSeam_SurfacesTheSameExceptionInstance` stated "Declared without a
nullable annotation because SVGControl.Test compiles as C# 7.3". The project no longer compiles as C#
7.3. The corrected text keeps the substantive explanation (the mock's unannotated type argument still
binds to the `Func<byte[], SvgDocument?>` parameter because nullability is metadata-only) and drops the
now-false reason. This changed no assertion and no test name.

## Assertion and test-name integrity check

Command: `git diff -- SVGControl.Test/ | grep -E "^[-+].*(Should\(\)|TestMethod|public void)"`

EXIT_CODE: 0 — **no output.** Not one line containing `Should()`, `[TestMethod]`, or a `public void`
test signature appears as either an addition or a deletion in the diff, which mechanically confirms that
no assertion and no test name changed.

## Second clearing pass — `CS8632` under the analyzer build

Disclosed follow-up. After the edits above, the forced nullable rebuild was clean, but the **mandated
solution analyzer build** (which does **not** pass `/p:Nullable=enable`) reported **15 x `CS8632`**: "The
annotation for nullable reference types should only be used in code within a '#nullable' annotations
context." One per `?` annotation added above, all fifteen inside the three in-scope test files:

```
SvgAssemblyProbeDirectoryTests.cs(25,19) (43,19) (54,19) (64,19)
SvgRendererNullToleranceTests.cs(60,19) (87,19)
SvgRendererParseContractTests.cs(136,24) (180,32) (181,30) (202,32) (203,30) (230,32) (231,30) (317,32) (318,30)
```

Cause: `<LangVersion>latest</LangVersion>` makes the `?` token *legal*, but a nullable **annotations
context** is still required for it to be *meaningful*. `/p:Nullable=enable` supplies that context
project-wide, which is why the `[P1-T6]` command never saw `CS8632`; the analyzer build does not pass
that property, so the annotations sat outside any context.

Fix, still confined to the three authorized files: **`#nullable enable` added as the first line of each**,
matching the convention both production files already use (`SVGControl/SvgRenderer.cs:1`,
`SVGControl/SvgAssemblyProbe.cs:1`, `SVGControl/SvgAssemblyResolver.cs:1`). This is the correct scoping
mechanism, not a suppression: it *enables* nullable analysis in these files unconditionally rather than
silencing a diagnostic. No `<NoWarn>`, no `#pragma warning disable`, and no `.editorconfig` severity change
was used, per the binding `## Do Not Do` list. Because the 24 diagnostics above were already cleared, the
now-permanently-enabled context reports nothing.

Verification after the second pass:

| Gate | Command | Result |
|---|---|---|
| Forced nullable rebuild | the `[P1-T6]` command | `EXIT_CODE: 0`, `CS86xx` count **0** |
| Solution analyzer build | `Invoke-VSBuild.ps1 ... -EnableNETAnalyzers -EnforceCodeStyleInBuild` | `EXIT_CODE: 0`, 0 errors, **5 warnings**, `CS8632` count **0** |
| Formatting | `dotnet tool run csharpier check .` | `EXIT_CODE: 0`, `Checked 1467 files`, 0 need formatting |

Reruns of the `[P1-T6]` command required in total: **2** (one after the annotation edits, one after the
`#nullable enable` addition).

## Formatting

Command: `dotnet tool run csharpier check SVGControl.Test/`

EXIT_CODE: 0 — `Checked 10 files in 331ms`, 0 files needing formatting. Re-confirmed repository-wide after
the second pass: `dotnet tool run csharpier check .` returned `EXIT_CODE: 0`, `Checked 1467 files`, 0
needing formatting.

## Final rerun of the `[P1-T6]` command

Command:

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m
```

EXIT_CODE: 0

Verbatim output:

```

  SVGControl -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl\bin\Debug\SVGControl.dll
  SVGControl.Test -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl.Test\bin\Debug\SVGControl.Test.dll
```

- `CS86xx` count: **0** (`grep -c "CS86"` returns 0)
- `CS8630` count: **0**
- Total diagnostics: **0** (0 errors, 0 warnings)
- Reruns of the `[P1-T6]` command required to reach `EXIT_CODE: 0`: **1** (a second rerun followed the
  `CS8632` follow-up recorded below, which also returned `EXIT_CODE: 0`)

`SVGControl.Test` now compiles cleanly under the mandated nullable property set with warnings as errors.
This is the state `[P2-T5]` requires for the `R2_KEEP` token.

---

# `[P1-T9]` — Branch B record

```
Branch A taken at [P1-T7]; no potential entry required
```

`docs/features/potential/2026-08-05-test-project-langversion-alignment.md` was **not** created, per
`[P1-T9]`'s "If `[P1-T7]` recorded `R2_KEEP`, create no file."

Repository-wide context recorded here for the reaudit rather than in a potential-feature entry: five
other test projects (`QuickFiler.Test`, `Tags.Test`, `TaskTree.Test`, `TaskVisualization.Test`,
`ToDoModel.Test`) still declare no `<LangVersion>` and would emit the same `CS8630` at forced-recompile
scope, against three that already set it (`TaskMaster.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`).
They never reach their own `CoreCompile` in a cold solution-wide nullable build because they cascade-fail
from `UtilitiesCS` first. `SVGControl.Test` was the only one of the six that surfaced, because it
project-references only `SVGControl`. R-2 has now removed it from that set, reducing the outstanding
group from six to five. Closing the remaining five is repository-wide work outside issue #418's Scope
Lock and was not attempted.
