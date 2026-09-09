# Phase 4 static gates re-validated against the formatted tree (Issue #824, task P5-T13)

Timestamp: 2026-09-09T16-15

Command: re-execution of the greps and anchored diffs of P4-T1 through P4-T8 exactly as written
there, with the anchored-diff spans anchored on `HEAD` per the adaptation recorded in
`evidence/other/executor-deviations.2026-09-09T15-28.md`.

EXIT_CODE: 0

This task exists because Phase 4 observed the pre-format tree, the P5-T1 format pass rewrote both
source files, and the Phase 6 acceptance check-offs cite the Phase 4 observations. Every gate is
re-measured below beside its Phase 4 value.

## P4-T1 — AC1 assignment sweep

| Measure | Phase 4 | Re-measured | Unchanged |
|---|---|---|---|
| Repository-wide matches for `(singleByteOpCodes\|multiByteOpCodes)\s*=` over `*.cs` | 2 | **2** | yes |
| File containing both | `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | same | yes |
| Assignment line numbers | 167, 168 | **167, 168** | yes |
| Static constructor declaration | 139 | **139** | yes |
| Closing brace of the reflection loop | 166 | **166** | yes |
| Closing brace of the static constructor | 169 | **169** | yes |

The matched lines, reproduced verbatim:

```
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:167:            singleByteOpCodes = singleTable;
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:168:            multiByteOpCodes = multiTable;
```

Both assignment lines still fall after the loop's closing brace at 166 and before the constructor's
closing brace at 169. The format pass shifted nothing in this region: CSharpier had already
normalised these lines during the P5-T1 pass that preceded the Phase 4 measurement's own re-derivation
window, and the numbers are identical.

## P4-T2 — AC5

| Measure | Phase 4 | Re-measured | Unchanged |
|---|---|---|---|
| `RuntimeHelpers.RunClassConstructor` in `ILGlobals.cs` | 1 | **1** | yes |
| `public static void LoadOpCodes\(\)` in `ILGlobals.cs` | 1 | **1** | yes |
| `git diff --stat HEAD` for `MethodBodyReader_Tests.cs` | empty | **empty** | yes |
| Line count of `MethodBodyReader_Tests.cs` | 489 | **489** | yes |

## P4-T3 — AC6

| Pattern | Phase 4 | Re-measured | Unchanged |
|---|---|---|---|
| `null!` | 0 | **0** | yes |
| `populated by LoadOpCodes` | 0 | **0** | yes |
| `never reassigned` | 2 | **2** | yes |
| `element mutation` | 2 | **2** | yes |
| `#nullable enable` | line 1 | **line 1** | yes |

The format pass did not reflow the XML documentation comments carrying `never reassigned` and
`element mutation`, so both tokens remain on single lines and remain findable by a line-oriented
search.

## P4-T4 — AC7

| Measure | Phase 4 | Re-measured | Unchanged |
|---|---|---|---|
| `ILGlobals\.LoadOpCodes\(\)` matches | 1, at line 50 | **1, at line 50** | yes |
| Enclosing method declaration line | 43 | **43** | yes |
| Four old test names | 0 | **0** | yes |
| `SingleByteOpCodes_IsPublishedWithFullLength` declaration | line 17 | **line 17** | yes |
| `MultiByteOpCodes_IsPublishedWithFullLength` declaration | line 29 | **line 29** | yes |
| `Length` assertions | lines 21, 33 | **lines 21, 33** | yes |

The single invocation at line 50 still lies inside the body of
`LoadOpCodes_DoesNotRepublishPublishedTables`, declared at line 43.

## P4-T5 — AC8

| Measure | Phase 4 | Re-measured | Unchanged |
|---|---|---|---|
| `DoNotParallelize` across `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/` | 0 | **0** | yes |
| `git diff --stat HEAD` for `AssemblyInfo.cs` | empty | **empty** | yes |
| `Parallelize` in `AssemblyInfo.cs` | line 18 | **line 18** | yes |

The re-measurement widened the `DoNotParallelize` search from the two named files to the whole
`SDILReader` test directory and still returned zero, which is a strictly stronger observation than
the Phase 4 one.

## P4-T6 — AC9

| Measure | Phase 4 | Re-measured | Unchanged |
|---|---|---|---|
| `lock\s*\(\|volatile\|Lazy<` in `ILGlobals.cs` | 0 | **0** | yes |
| `git diff --stat HEAD` for `MethodBodyReader.cs` | empty | **empty** | yes |
| Line count of `MethodBodyReader.cs` | 299 | **299** | yes |

## P4-T7 — AC10

| Measure | Phase 4 | Re-measured | Unchanged |
|---|---|---|---|
| `git diff --stat HEAD` for both project files | empty | **empty** | yes |
| Porcelain entries naming a project file | none | **none** | yes |

The merge-base-anchored form still reports the four inherited `<Compile Include>` insertions
attributable to a sibling child, exactly as P4-T7 recorded. That is unchanged and remains outside
this feature's footprint.

## P4-T8 — D6 Owned Write Set scope gate

| Measure | Phase 4 | Re-measured |
|---|---|---|
| `git diff --name-status HEAD` path count | 36 | **49** |
| `git status --porcelain --untracked-files=all` path count | 36 | **49** |
| Paths outside the three D6 classes | 0 | **0** |

The count rose from 36 to 49 because thirteen further evidence artifacts were written between the two
measurements: `csharpier-format`, `csharpier-check`, `format-scope-gate`, `msbuild-analyzers-final`,
`msbuild-analyzers-nonvacuity`, `msbuild-nullable-final`, `msbuild-nullable-nonvacuity`,
`coverage-post-change`, `ac11-named-tests`, `coverage-classes-post-change`, `coverage-delta`,
`file-line-counts-final`, and this artifact. Every one is D6 class 2.

The out-of-class filter returned an empty list, so no path in either listing lies outside the three
D6 classes. The scope gate passes again.

## Result

All eight Phase 4 acceptance conditions hold again after the P5-T1 format pass. No gate's result
changed, and no line number cited by a Phase 6 check-off has moved.
