# P9-T7 — Coverage delta

Timestamp: 2026-09-07T16-05
Task: [P9-T7]
Issue: #796
Channel used: A

Documents read:

- baseline: coverage/p0-t11-baseline.cobertura.xml, the file this task names
- final: coverage/p9-t6-final.cobertura.xml

Changed-line span:

```
pwsh -NoProfile -Command 'git diff -U0 c7ae69f1..HEAD -- QuickFiler'
```

## The three required figures

| Figure | Value |
|---|---|
| Baseline coverage | 24.1387 percent (14867 / 61590) |
| Post-change coverage | 24.1857 percent (14925 / 61710) |
| Changed-code coverage | 97.5610 percent (40 / 41) |

All three are numbers, not placeholders.

## No-regression comparison

Baseline ratio, computed from the two document-level attributes the baseline recorded:
14867 / 61590 = 0.2413866, or 24.1387 percent.

Post-change ratio, computed from the same two attributes of the final document:
14925 / 61710 = 0.2418571, or 24.1857 percent.

The post-change ratio is HIGHER than the baseline ratio by 0.0470 percentage points, so
it is not lower and the no-regression clause is met. The denominator grew by 120 valid
lines and the numerator grew by 58 covered lines.

## The anchor, and why it is retained at c7ae69f1

The anchor is deliberately RETAINED at c7ae69f1 and was NOT moved to the second merge
commit d78ae7f7 or the third merge commit 5b8e0bf5. The plan states both reasons and
both were re-checked here rather than accepted: the diff is scoped to the QuickFiler
directory, which neither the second nor the third merge of origin/main touched, so
c7ae69f1 already yields exactly this item's own QuickFiler changed lines; and each of
those two merge commits already contains this item's committed Phase 1 instrumentation,
so anchoring at either would silently drop the instrumentation lines out of the
changed-line set and contradict this task's own requirement that they be counted in the
denominator.

The span enumerates nine paths:

```
77	0	QuickFiler/Controllers/QfcFormController.Deactivate.cs
54	0	QuickFiler/Controllers/QfcItemController.EventHandlers.cs
16	0	QuickFiler/Interfaces/IQfcFormViewer.cs
1	0	QuickFiler/QuickFiler.csproj
79	0	QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs
24	0	QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
12	14	QuickFiler/Viewers/BreadcrumbDropDownHost.cs
4	0	QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
39	0	QuickFiler/Viewers/QfcFormViewer.cs
```

`QuickFiler/Resources/FolderBreadcrumb.html` and
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` are write-set paths that the
span does not enumerate, because neither was changed. Both are recorded below anyway,
the first in the NOT MEASURABLE list as the plan requires and the second here, so that a
reader does not mistake their absence for an omission.

## Method

Added and modified line numbers were taken from the `@@ -a,b +c,d @@` hunk headers of the
`-U0` diff, expanding each header to the line numbers `c` through `c + d - 1` and
discarding headers with `d = 0`, which are pure deletions and have no line in the
post-change file.

Those line numbers were intersected, per file, with the `line` nodes of the final
Cobertura document, aggregating class nodes by their `filename` attribute so that a C#
async state machine emitted as a separate class node does not split one source file's
denominator. Cobertura filenames use backslash separators; the forward-slash spellings
in this artifact name the same files. Where two class nodes both carry a line, the
higher `hits` value is taken, so a line covered through one node is not recorded as
uncovered because a second node did not reach it.

The measurable denominator for a file is therefore the set of its changed lines that
carry a `line` node. A changed line that carries no `line` node — a blank line, or a
comment line the emitter did not map — is outside the denominator by construction, since
it can appear in neither the covered numerator nor the valid denominator.

## Comment-only changed lines, per measurable file

Recorded as this task requires, for every measurable file in the diff span. A line is
classified comment-only when its trimmed text begins with `//`, `/*`, or `*`.

| File | Changed lines | Comment-only | Removed from the denominator on this basis |
|---|---|---|---|
| QuickFiler/Controllers/QfcFormController.Deactivate.cs | 77 | 44 | 44 |
| QuickFiler/Controllers/QfcItemController.EventHandlers.cs | 54 | 39 | 39 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 12 | 9 | 4 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | 24 | 21 | 21 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs | 79 | 31 | 31 |

The two files this task's acceptance names explicitly both carry a figure: 44 for
`QuickFiler/Controllers/QfcFormController.Deactivate.cs` and 39 for
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs`. Both exceed the counts the
plan attributes to executed task P1-T4 alone — 33 and 13 `///` lines — because later
phases added further comment lines to both files and because these counts include
ordinary `//` comments as well as `///` documentation comments.

### One measured departure from the plan's stated mechanism, disclosed

The plan's comment clause reasons that "a comment line carries no Cobertura `line` node
at all". That holds for 139 of the 144 comment-only changed lines across the five files
in the table above, including every one of the 83 in the two files this task names
explicitly. It does NOT hold for five lines in
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs`: lines 442 through 446
are comment lines that each carry a `line` node with `hits=1`. Lines 251 through 254 in
the same file are comment lines that carry no node, so the behaviour is not uniform even
within one file. The likely mechanism is that the .coverage-to-Cobertura conversion maps
the full source span preceding a mapped statement, but the mechanism was not established
and is not relied on here.

This is disclosed rather than smoothed over, because those five lines sit in the
denominator and are all covered, so including them raises the changed-code figure. The
figure is therefore reported a second way below with all comment lines removed from both
numerator and denominator, and the threshold is met on both computations. Neither
computation was selected after seeing which one passed; both are reported.

## Instrumentation changed lines, per file

Recorded as this task requires. Instrumentation means the AC6 log statements and the
pure formatter methods executed task P1-T4 added, and the internal selector-open member
the same task added.

| File | Changed lines | Instrumentation | Measurable instrumentation |
|---|---|---|---|
| QuickFiler/Controllers/QfcFormController.Deactivate.cs | 77 | 65 (lines 29-80, 93-99, 125-130) | 20 |
| QuickFiler/Controllers/QfcItemController.EventHandlers.cs | 54 | 15 (lines 269-283) | 1 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 12 | 0 | 0 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | 24 | 0 | 0 |

The instrumentation lines in these two files ARE counted in the changed-code
denominator, as this task requires, unlike the diagnostics part. Separating them per line
would make the figure unreproducible, and both files carry behavioural changes that are
measured in the same span.

Neither file carries a class-level `[ExcludeFromCodeCoverage]`. The exclusions in
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs` are method-level and sit at
lines 60, 83, 97, 111 and 125 only. That was re-derived here rather than carried
forward, and the search idiom matters: the attribute is written fully qualified as
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`, so a search for the literal
`[ExcludeFromCodeCoverage]` returns zero hits in this file and would wrongly suggest the
exclusions had been removed.

## The diagnostics part, reported on its own line

`QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs`: 79 changed lines, 28
measurable, 28 covered, 100.0000 percent.

It is EXCLUDED from the behavioural changed-code figure. The file contains logging and
the relocated close handler only, and its coverage contribution must not be used to
inflate the figure for the behavioural changes. It is not counted in the 41-line
denominator or the 40-line numerator above.

## Per-file changed-code detail, behavioural files

| File | Changed | Measurable | Covered | Uncovered |
|---|---|---|---|---|
| QuickFiler/Controllers/QfcFormController.Deactivate.cs | 77 | 23 | 23 | 0 |
| QuickFiler/Controllers/QfcItemController.EventHandlers.cs | 54 | 10 | 8 | 2 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 12 | 8 | 8 | 0 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | 24 | 1 | 1 | 0 |
| **Total** | **167** | **42** | **40** | **2** |

Measurable line numbers, with hit counts:

```
QfcFormController.Deactivate.cs   56(1) 57(1) 58(1) 76(1) 77(1) 78(1) 79(1) 93(1) 94(1)
                                  95(1) 96(1) 97(1) 98(1) 99(1) 118(1) 119(1) 120(1)
                                  125(1) 126(1) 127(1) 128(1) 129(1) 130(1)
QfcItemController.EventHandlers.cs 183(1) 209(0) 220(1) 234(1) 254(1) 257(1) 263(1)
                                   264(1) 265(1) 282(0)
BreadcrumbDropDownHost.cs          255(1) 256(1) 442(1) 443(1) 444(1) 445(1) 446(1) 447(1)
BreadcrumbDropDownHost.Open.cs     123(1)
```

## Individually named exclusions from the changed-code denominator

Exactly ONE line is excluded. The allowance is at most 3, so two remain unused. No
blanket exclusion and no unnamed exclusion is taken.

**Exclusion 1 of at most 3.** `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`
line 282:

```
internal bool IsBreadcrumbSelectorOpen => _itemViewer.IsFolderDropDownOpen;
```

Reason — the mocked-seam limitation this task names. This is the internal selector-open
member executed task P1-T4 added. Its only reader is the per-item log statement in
`ParkFocusAndCancelSelectors`, at
`QuickFiler/Controllers/QfcFormController.Deactivate.cs` line 128, which reaches it only
when the loop's interface-typed item controller casts successfully to the concrete
internal type `QfcItemController`. Every test in QfcFormControllerDeactivateTests that
injects item controllers injects them as Moq mocks of `IQfcItemController`, so that cast
yields null in every test and the member is never evaluated. The exclusion was admitted
by name in the plan rather than discovered at this gate, and the measurement confirms it:
the line is present in the final Cobertura document with `hits=0`.

### The second uncovered line is NOT excluded

`QuickFiler/Controllers/QfcItemController.EventHandlers.cs` line 209:

```
internal bool SearchOwnsDropDownDismissal => _searchOwnedDismissal;
```

This line is uncovered and it REMAINS IN THE DENOMINATOR. It is recorded here rather
than excluded, because the at-most-3 allowance is a permission and not an obligation,
and the threshold is met without spending it. A search of every `.cs` file in the tree
for the identifier `SearchOwnsDropDownDismissal` returns exactly one hit, the declaration
itself, so the member currently has no reader in production or in test code. That is the
reason no test covers it. It is reported to the reviewer as an observation; removing it
would be a code change, and this task computes and records a figure rather than editing
source.

## The changed-code figure

Behavioural measurable changed lines: 42.
Less the one individually named exclusion at EventHandlers.cs line 282: 41.
Covered among those 41: 40.

**Changed-code coverage = 40 / 41 = 97.5610 percent.**

That is at least 90 percent, so the threshold clause is met.

Two corroborating computations, neither of which is the reported figure:

- With no exclusion at all, 40 / 42 = 95.2381 percent. Still at or above 90 percent, so
  the result does not depend on the exclusion being taken.
- With every comment-only line removed from both numerator and denominator, which
  neutralises the five-line departure disclosed above, 35 / 36 = 97.2222 percent. Still
  at or above 90 percent, so the result does not depend on that departure either.

## NOT MEASURABLE write-set files, each with its citation

| File | Reason | Citation |
|---|---|---|
| `QuickFiler/Viewers/QfcFormViewer.cs` | class-level `[ExcludeFromCodeCoverage]` suppresses the whole type | attribute at QuickFiler/Viewers/QfcFormViewer.cs line 17, verified in this pass; the final Cobertura document carries 0 class nodes naming this file |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | partial part of `ItemViewer`, which carries a class-level `[ExcludeFromCodeCoverage]` | attribute at QuickFiler/Viewers/ItemViewer.cs line 20, verified in this pass; the final Cobertura document carries 0 class nodes naming this file |
| `QuickFiler/Interfaces/IQfcFormViewer.cs` | interface-only file with no executable lines | the final Cobertura document carries 0 class nodes naming this file |
| `QuickFiler/Resources/FolderBreadcrumb.html` | not C#; carries no coverage figure of any kind. It is additionally unchanged in this span | absent from the `--numstat` listing above |
| `QuickFiler/QuickFiler.csproj` | project file, not compiled source | 1 changed line, the `<Compile Include>` entry executed task P1-T3 added at line 417 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | project file, not compiled source; additionally outside the `-- QuickFiler` span, which matches the QuickFiler directory only | absent from the `--numstat` listing above |

Their 59 changed lines — 39 in QfcFormViewer.cs, 4 in ItemViewer.Breadcrumb.cs, 16 in
IQfcFormViewer.cs and 1 in QuickFiler.csproj — are in neither the numerator nor the
denominator of the changed-code figure. Reporting a figure for them would report a
number that has no source.

No file was recorded `ABSENT: no class node carries this filename` at both P0-T11 and
P9-T6, so no file enters this list on that basis.

## Acceptance clause by clause

| Clause | Observed | Met |
|---|---|---|
| all three figures present as numbers rather than placeholders | 24.1387, 24.1857, 97.5610 | yes |
| changed-code figure at least 90 percent over the measurable behavioural changed lines | 97.5610 percent, and 95.2381 or 97.2222 under the two corroborating computations | yes |
| post-change `lines-covered` / `lines-valid` not lower than the baseline ratio | 0.2418571 against 0.2413866 | yes |
| every NOT MEASURABLE entry carries its citation | six entries, each cited | yes |
| individually named exclusions at most 3, each naming file, line and reason | 1 exclusion, EventHandlers.cs line 282, mocked-seam limitation | yes |
| comment-only changed-line count recorded per measurable file, and at minimum for the two named files | recorded for all five, including 44 and 39 for the two named | yes |
| instrumentation changed-line count recorded per file | recorded for all four behavioural files | yes |
| diagnostics part reported on its own line and excluded from the behavioural figure | 28 / 28, excluded | yes |

Output Summary: baseline coverage 24.1387 percent, post-change coverage 24.1857 percent,
changed-code coverage 97.5610 percent over 41 measurable behavioural changed lines with
40 covered. The post-change document ratio is higher than the baseline ratio, so there is
no regression. One individually named exclusion is taken of an allowance of three, the
mocked-seam-unreachable member at EventHandlers.cs line 282; the second uncovered line,
EventHandlers.cs line 209, is left in the denominator rather than excluded and is
reported as an unreferenced member. All eight acceptance clauses are met.
