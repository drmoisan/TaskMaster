# [P3-T7] Post-change repository coverage — extract

Timestamp: 2026-08-11T01-56
Source: post-processed `coverage\coverage.cobertura.xml` produced by the `[P3-T7]` run
Reason for extract rather than copy: the post-processed artifact is 10,446,316 bytes (9.962 MB),
above the plan's 5 MB copy threshold. The full dump was omitted for size per the plan's artifact copy
rule.

## Document element attributes (verbatim)

```xml
<coverage line-rate="0.855355" branch-rate="0.790134" complexity="24765" version="1.9" timestamp="1786424753" lines-covered="53375" lines-valid="62401" branches-covered="12541" branches-valid="15872">
```

Read individually:

| Attribute | Value |
|---|---|
| `line-rate` | 0.855355 |
| `branch-rate` | 0.790134 |
| `complexity` | 24765 |
| `version` | 1.9 |
| `timestamp` | 1786424753 |
| `lines-covered` | 53375 |
| `lines-valid` | 62401 |
| `branches-covered` | 12541 |
| `branches-valid` | 15872 |

## Per-file extract 1 — `TaskVisualization\FlagTasks.cs`

```
(absent)
```

`SelectSingleNode('//class[@filename="TaskVisualization\FlagTasks.cs"]')` returns nothing and the
node count for that filename is **0**. The file has disappeared from the report entirely.

At baseline the file was present as a single class:

```xml
<class line-rate="0" branch-rate="0" complexity="3" name="TaskVisualization.FlagTasks.&lt;&gt;c" filename="TaskVisualization\FlagTasks.cs"><methods><method line-rate="0" branch-rate="1" complexity="1" name="&lt;InitializeToDoList&gt;b__13_0" signature="(object)"><lines><line number="114" hits="0" branch="False" /></lines></method></methods><lines><line number="114" hits="0" branch="False" /> … 10 lines total … </lines></class>
```

That class carries the `.<>c` marker, so `Test-CoberturaClosureClassName` classifies it as a closure
class. Its sole method `<InitializeToDoList>b__13_0` derives the declaring member `InitializeToDoList`,
which is admitted to the presence set by neither source: no non-closure `TaskVisualization.FlagTasks`
class exists for this filename (source 1), and no
`TaskVisualization.FlagTasks.<InitializeToDoList>d__N` state-machine class exists (source 2). The
method is dropped, zero methods are retained, and the `<class>` element is removed from its
`<classes>` parent — at which point the filename no longer appears anywhere in the document.

Ten uncovered lines (114, 136-143, 159) left the denominator and zero covered lines left the
numerator, because every one of them carried `hits="0"`.

## Per-file extract 2 — `QuickFiler\Viewers\BreadcrumbPopupUiOperations.cs`

`<class>` open tag (verbatim):

```xml
<class line-rate="0.991453" branch-rate="0.896552" complexity="124" name="QuickFiler.Viewers.BreadcrumbPopupUiOperations" filename="QuickFiler\Viewers\BreadcrumbPopupUiOperations.cs">
```

Derived measures:

| Measure | Value |
|---|---|
| `<class>` count for this filename | 1 |
| class `name` | `QuickFiler.Viewers.BreadcrumbPopupUiOperations` |
| `line-rate` | 0.991453 |
| `branch-rate` | 0.896552 |
| class-level `<line>` count | 234 |
| class-level `<line>` count with `hits` > 0 | 232 |
| `<method>` count | 28 |
| method-level `<line>` count | 82 |
| `complexity` | 124 |

Arithmetic check: 232 / 234 = 0.991453, matching the emitted `line-rate` exactly.

The 28 retained methods are unchanged from baseline — the filter mutates only closure classes, and
this class carries no `.<>c` marker. The change to this file comes entirely from sibling closure
classes being removed **before** the merge collapsed them into this node.

## Output Summary

Document-element attributes recorded verbatim. `TaskVisualization/FlagTasks.cs` is absent from the
post-change report. `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` measures 232/234 = 0.991453.
Full 9.962 MB dump omitted for size per the plan's artifact copy rule.
