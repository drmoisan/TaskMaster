# [P0-T11] Repository coverage baseline — extract

Timestamp: 2026-08-11T00-30
Source: post-processed `coverage\coverage.cobertura.xml` produced by the `[P0-T11]` run
Reason for extract rather than copy: the post-processed artifact is 10,482,935 bytes (9.997 MB),
above the plan's 5 MB copy threshold. The full dump was omitted for size per the plan's artifact
copy rule.

## Document element attributes (verbatim)

```xml
<coverage line-rate="0.853514" branch-rate="0.790236" complexity="24765" version="1.9" timestamp="1786421720" lines-covered="53663" lines-valid="62873" branches-covered="12609" branches-valid="15956">
```

Read individually:

| Attribute | Value |
|---|---|
| `line-rate` | 0.853514 |
| `branch-rate` | 0.790236 |
| `complexity` | 24765 |
| `version` | 1.9 |
| `timestamp` | 1786421720 |
| `lines-covered` | 53663 |
| `lines-valid` | 62873 |
| `branches-covered` | 12609 |
| `branches-valid` | 15956 |

## Per-file extract 1 — `TaskVisualization\FlagTasks.cs`

Full `<class>` `OuterXml` (verbatim):

```xml
<class line-rate="0" branch-rate="0" complexity="3" name="TaskVisualization.FlagTasks.&lt;&gt;c" filename="TaskVisualization\FlagTasks.cs"><methods><method line-rate="0" branch-rate="1" complexity="1" name="&lt;InitializeToDoList&gt;b__13_0" signature="(object)"><lines><line number="114" hits="0" branch="False" /></lines></method></methods><lines><line number="114" hits="0" branch="False" /><line number="136" hits="0" branch="False" /><line number="137" hits="0" branch="False" /><line number="138" hits="0" branch="False" /><line number="139" hits="0" branch="False" /><line number="140" hits="0" branch="False" /><line number="141" hits="0" branch="False" /><line number="142" hits="0" branch="False" /><line number="143" hits="0" branch="False" /><line number="159" hits="0" branch="False" /></lines></class>
```

Derived measures:

| Measure | Value |
|---|---|
| `<class>` count for this filename | 1 |
| class `name` | `TaskVisualization.FlagTasks.<>c` |
| `line-rate` | 0 |
| `branch-rate` | 0 |
| class-level `<line>` count | 10 |
| class-level `<line>` count with `hits` > 0 | 0 |
| `<method>` count | 1 |
| method names | `<InitializeToDoList>b__13_0` |

Analysis relevant to `[P3-T7]` and `[P3-T8]`: the single surviving `<class>` for this filename is a
**closure class** (`.<>c` marker present) whose only method is `<InitializeToDoList>b__13_0`. The
declaring member token derives to `InitializeToDoList`. No non-closure `TaskVisualization.FlagTasks`
class exists for this filename, so `InitializeToDoList` cannot enter the presence set from source (1),
and no `TaskVisualization.FlagTasks.<InitializeToDoList>d__N` class exists either, so it cannot enter
from source (2). The filter is therefore expected to drop the sole method, retain zero methods, and
remove the `<class>` element entirely — at which point the filename disappears from the report. This
is the measured basis for `[P3-T8]`'s substantive gate on this file, and `[P0-T11]` records the file
as PRESENT, so that gate is live rather than waived.

Note that the class-level `<lines>` carries 10 line numbers while the sole method carries only line
114. The other nine (136-143, 159) are class-level rollup lines contributed by other, already-merged
sibling closure classes for this file (`Merge-CoberturaClassesByFilename` unions the group's
class-level `<lines>` into the surviving primary). Because the filter runs BEFORE the merge, it sees
those sibling closure classes individually and evaluates each on its own methods; the merge that
produced this 10-line rollup happens afterwards.

## Per-file extract 2 — `QuickFiler\Viewers\BreadcrumbPopupUiOperations.cs`

`<class>` open tag (verbatim):

```xml
<class line-rate="0.906977" branch-rate="0.883333" complexity="131" name="QuickFiler.Viewers.BreadcrumbPopupUiOperations" filename="QuickFiler\Viewers\BreadcrumbPopupUiOperations.cs">
```

Derived measures:

| Measure | Value |
|---|---|
| `<class>` count for this filename | 1 |
| class `name` | `QuickFiler.Viewers.BreadcrumbPopupUiOperations` |
| `line-rate` | 0.906977 |
| `branch-rate` | 0.883333 |
| class-level `<line>` count | 258 |
| class-level `<line>` count with `hits` > 0 | 234 |
| `<method>` count | 28 |
| method-level `<line>` count | 82 |

258 total lines with 234 covered reproduces the `(258 - 22)` ceiling arithmetic recorded in
`issue.md` § Impact / Severity, and 234/258 = 0.906977 matches the emitted `line-rate` exactly,
confirming the post-#441 per-file arithmetic is consistent with the class-level rollup.

The 28 retained methods on the merged class are the plain (non-synthesized) members of the type. All
of them are named without a leading `<`, so every one of them enters the presence set from source (1)
when the filter runs pre-merge. The exempt members whose closures the filter is expected to drop —
`BeginProductionNavigation`, `BindProductionNavigation`, `DisposeProductionSurface` per research §5.3 —
are **absent** from this method list, which is the measured confirmation that their closures resolve
to an absent declaring member:

```
.ctor (x2), CaptureCurrent, CreateForCurrentThreadTests, CaptureCurrentOrTests, NormalizeFactory,
RunAsync (x2), PostAsync, Report, CreateControlAsync, BeginInitializationAsync (x2), ReadCoreAsync (x2),
ReadRequiredAsync, BeginNavigationAsync, ObserveInitializationAsync, ObserveReadinessAsync,
DisposeSurfaceAsync, DisposeSurfaceAfterFailureAsync, PlaceSurfaceAsync, DisposeHostedSurfaceAsync,
DisposeHostedSurfaceAfterFailureAsync, Invalid, CreateDispatchedReadiness, NavigateToDocument,
NavigateToDocumentCore
```

## Output Summary

Document-element attributes and both named per-file `<class>` extracts recorded verbatim. Full
10 MB dump omitted for size per the plan's artifact copy rule. `TaskVisualization/FlagTasks.cs` is
PRESENT at baseline as a lone closure class, making `[P3-T8]`'s "absent post-change" gate live.
`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` measures 234/258 = 0.906977.
