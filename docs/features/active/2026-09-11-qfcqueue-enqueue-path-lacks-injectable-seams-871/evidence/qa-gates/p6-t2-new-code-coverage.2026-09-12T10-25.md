# P6-T2 — New-code coverage derived from the anchored diff

Timestamp: 2026-09-13T16-58
Command: git diff --no-renames --unified=0 8213826f695439e86e3ed34faa575de493a11ec7..HEAD -- QuickFiler/Controllers/QfcQueue.cs QuickFiler/Controllers/QfcQueue.Enqueue.cs QuickFiler/Controllers/QfcQueue.Tlp.cs QuickFiler/Controllers/QfcQueue.UiIdle.cs QuickFiler/Interfaces/IUiIdleDispatcher.cs
EXIT_CODE: 0

## Property-name mapping for every class-level figure in this artifact

The plan asks for class-level figures under the names `LineRate`, `LinesCovered` and `LinesValid` from
`Get-CoberturaClassLineSummary`. That helper emits none of those names: it returns `LineMap`,
`TotalLines`, `CoveredLines`, `TotalBranches` and `CoveredBranches`, and emits no rate property. Only
`Get-CoberturaPackageLineSummary` emits the three names the plan uses. The mapping established by P0-T12
and reproduced in full here, so a reviewer meets the same explanation at every site reporting a
class-level figure, is:

- `LinesCovered` is the helper's `CoveredLines`.
- `LinesValid` is the helper's `TotalLines`.
- `LineRate` is read from the class element's own line-rate attribute.

This task reads per-line hit counts directly from the `line` elements of the P5-T5 and P0-T12 Cobertura
documents rather than through either helper, so the mapping affects only the corroborating aggregates
quoted below; those were cross-checked against their class elements' line-rate attributes and agree
exactly. The plan is not edited; the mapping is recorded.

## Procedure, stated so a third party re-running it obtains the same table

1. Run an anchored, rename-disabled, zero-context diff of the five production Write Set code paths
   against the sha P0-T2 recorded, namely 8213826f695439e86e3ed34faa575de493a11ec7.
2. Take the added lines: every output line whose first character is a plus sign and which is not a file
   header line. The new-file line number of each is tracked from the hunk header and incremented per
   added line.
3. Take the removed lines the same way from the minus side, tracking the old-file line number.
4. Classify each added line mechanically. A line is **relocated** when its text, with leading and
   trailing whitespace stripped, is identical to the stripped text of at least one removed line. It is
   **genuinely new** otherwise. No other classification is permitted and no line is classified by
   judgment.
5. A line that carries no `line` element in the P5-T5 document carries no executable statement and is
   excluded from both the numerator and the denominator of both rates.
6. Each remaining line's hit count is read from the P5-T5 document by matching the file's `filename`
   attribute in the backslash-separated form the post-processor writes and the `line` element's `number`
   attribute.

## Counts produced by that procedure

AddedLineCount: 498
RemovedLineCount: 264
GenuinelyNewLineCount: 199
RelocatedLineCount: 299
ExcludedNoLineElementCount: 311
ExcludedNoLineElementGenuinelyNew: 175
ExcludedNoLineElementRelocated: 136

The 498 added lines agree with the diff's own numstat totals of 5, 329, 108, 21 and 35 across the five
paths. The 311 excluded lines are `using` directives, namespace and type declaration headers, brace-only
lines that the compiler emits no sequence point for, XML documentation comments, blank lines, attribute
lines, field and property declaration syntax, and the commented-out alternative implementation that
travels with the third relocated marshalling body. They carry no `line` element in the P5-T5 document and
are therefore excluded from both the numerator and the denominator of both rates, as step 5 directs.

## The two rates

GenuinelyNewLinesCovered: 23
GenuinelyNewLinesValid: 24
GenuinelyNewLineRate: 0.958333

RelocatedLinesCovered: 56
RelocatedLinesValid: 163
RelocatedLineRate: 0.343558

## Gate

The gate is that the genuinely-new line rate is at least 0.90, which is the new-code floor stated in the
standing instructions file.

```
0.958333 >= 0.90   TRUE
```

GateVerdict: PASS

Twenty-three of the twenty-four genuinely-new executable lines are covered. The single uncovered one is:

```
QuickFiler/Controllers/QfcQueue.Tlp.cs:121   tlp.Clone(name: "BackgroundTableLayout");
```

That is the body of the default lambda of `BackgroundTlpFactory`, which is deliberately never invoked by
any test: invoking it would run the reflection-driven control clone in the utilities extensions
namespace, which is precisely the untestable region the seam exists to bypass. It is carried into the
residual record written by P6-T3 rather than excluded from measurement. Had it been excluded, the
genuinely-new rate would read 1 over 23 of 23 and the measurement would be reporting the absence of the
line rather than its coverage state.

## Per-file breakdown

| File | Added | Genuinely new | Relocated | New executable | New covered | Relocated executable | Relocated covered |
|---|---|---|---|---|---|---|---|
| `QuickFiler/Controllers/QfcQueue.cs` | 21 | 18 | 3 | 2 | 2 | 0 | 0 |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 5 | 4 | 1 | 4 | 4 | 1 | 1 |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` | 329 | 86 | 243 | 13 | 12 | 138 | 55 |
| `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | 108 | 63 | 45 | 5 | 5 | 24 | 0 |
| `QuickFiler/Interfaces/IUiIdleDispatcher.cs` | 35 | 28 | 7 | 0 | 0 | 0 | 0 |

`QuickFiler/Interfaces/IUiIdleDispatcher.cs` contributes no executable line to either rate. It declares
an interface and nothing else, so the P5-T5 document carries no class element for it and every one of
its 35 added lines is excluded at step 5. That is the expected shape for an interface-only file and is
not a measurement failure.

## The rate the relocated statements carried at the anchor

The plan requires the relocated rate to be reported alongside the rate the same statements carried at
the anchor. Resolving "the same statement" at the anchor requires mapping each relocated added line back
to a removed line, and the classification rule in step 4 matches on stripped text alone. For 75 of the
163 relocated executable lines that text matches more than one removed line, so the mapping is not
one-to-one for them. The distinct ambiguous texts are dominated by syntax with no unique content:

| Occurrences | Stripped text |
|---|---|
| 26 | `{` |
| 24 | `}` |
| 9 | `);` |
| 3 | `System.Windows.Threading.DispatcherPriority.ContextIdle` |
| 2 each | five multi-line-call fragments |

This artifact therefore reports the anchor comparison twice, and states which figure is sound.

RelocatedAnchorRateAllMatches: 0.578616
RelocatedAnchorMeasurableAllMatches: 159
RelocatedAnchorCoveredAllMatches: 92

That first figure resolves an ambiguous match by taking the maximum hit count over every removed line
sharing the text. It is reported because it is the figure a naive application of the step-4 rule
produces, and it is **not** sound: a bare closing brace in the relocated body inherits the hit count of
whichever brace anywhere in the removed set was hottest, which has nothing to do with the statement in
question. Comparing it against the post-change relocated rate of 0.343558 would suggest a coverage drop
that the underlying lines do not show.

The sound comparison restricts to the 85 relocated executable lines whose stripped text matches exactly
one removed line and whose matched removed line carries a `line` element in the P0-T12 document, so the
anchor statement is uniquely identified:

RelocatedUnambiguousCount: 85
RelocatedUnambiguousPostCovered: 34
RelocatedUnambiguousPostRate: 0.4
RelocatedUnambiguousAnchorCovered: 29
RelocatedUnambiguousAnchorRate: 0.341176
RelocatedUnambiguousRegressedCount: 0
RelocatedUnambiguousNewlyCoveredCount: 5
RelocatedUnambiguousUncoveredAtBothCount: 51

On the unambiguously mapped subset the relocated statements rose from 29 covered to 34 covered, a rate
movement from 0.341176 to 0.4. **No relocated statement that was covered at the anchor is uncovered
after the change**: the regressed count is zero. Five statements that were uncovered at the anchor are
covered now. The remaining 4 of the 163 relocated executable lines are unambiguous in text but carry no
`line` element at the anchor, so no anchor figure exists for them and they are excluded from the anchor
comparison only, not from the post-change relocated rate.

The 51 statements uncovered at both points are carried into the residual record written by P6-T3. No
coverage-exclusion attribute and no assembly-level exclusion is introduced anywhere in this change, so
they remain in the denominator and remain visible.

## Corroboration against the class-level aggregates

The per-line figures above are consistent with the whole-file aggregates P5-T5 recorded. The two files
that gained the most covered lines are `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, which moved from 13
of 85 to 85 of 85, and `QuickFiler/Controllers/QfcQueue.cs`, whose post-split part reads 109 of 155. The
file with the lowest post-change rate, `QuickFiler/Controllers/QfcQueue.UiIdle.cs` at 5 of 29, is the one
holding the 24 relocated adapter-body lines that report zero covered here; those bodies cannot run
without a live process-wide dispatcher, which is the reason P6-T3 records them.

## Line-by-line table

The table below carries one row for each of the 187 added lines that carries a `line` element in the
P5-T5 document and therefore contributes to a rate. The 311 excluded lines are not listed individually;
their count is recorded above as step 5 requires. `AnchorHits` is populated only for relocated lines and
only where the anchor statement is uniquely identified; `ambiguous` marks a relocated line whose stripped
text matches more than one removed line, and `none` marks one whose matched removed line carries no
`line` element at the anchor.

| File | Line | Classification | Hits | Covered | AnchorHits | Source text |
|---|---|---|---|---|---|---|
| QfcQueue.Enqueue.cs | 91 | new | 1 | yes |  | `items.ForEach(item => MoveMonitor.HookItem(item, async (x) => await RemoveItem(x)))` |
| QfcQueue.Enqueue.cs | 97 | new | 1 | yes |  | `var tlp = await UiIdleCallAsync(() => BackgroundTlpFactory(_tlpTemplate));` |
| QfcQueue.Enqueue.cs | 175 | new | 1 | yes |  | `.SelectAwait(async i =>` |
| QfcQueue.Enqueue.cs | 176 | new | 1 | yes |  | `(i: i, grp: await ItemGroupFactory(tlp, items[i - start], i))` |
| QfcQueue.Enqueue.cs | 177 | relocated | 1 | yes | ambiguous | `)` |
| QfcQueue.Tlp.cs | 35 | relocated | 0 | no | 0 | `get => _tlpTemplate;` |
| QfcQueue.Tlp.cs | 37 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 38 | relocated | 0 | no | 0 | `_tlpTemplate = value.Clone();` |
| QfcQueue.Tlp.cs | 39 | relocated | 0 | no | 0 | `_tlpTemplate.Name = "TemplateTableLayout";` |
| QfcQueue.Tlp.cs | 43 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 47 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 53 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 60 | relocated | 1 | yes | 0 | `get => _tlpStates;` |
| QfcQueue.Tlp.cs | 61 | relocated | 1 | yes | 0 | `set => _tlpStates = value;` |
| QfcQueue.Tlp.cs | 64 | new | 1 | yes |  | `private Func<CancellationToken, ItemViewer> _itemViewerFactory = ItemViewerQueue.Dequeue;` |
| QfcQueue.Tlp.cs | 78 | new | 1 | yes |  | `get => _itemViewerFactory;` |
| QfcQueue.Tlp.cs | 79 | new | 1 | yes |  | `set => _itemViewerFactory = value ?? throw new ArgumentNullException(nameof(value));` |
| QfcQueue.Tlp.cs | 96 | new | 1 | yes |  | `get => _viewerRowPlacer ??= AddViewerToTlp;` |
| QfcQueue.Tlp.cs | 97 | new | 1 | yes |  | `set => _viewerRowPlacer = value ?? throw new ArgumentNullException(nameof(value));` |
| QfcQueue.Tlp.cs | 116 | new | 1 | yes |  | `get => _itemGroupFactory ??= AddAsync;` |
| QfcQueue.Tlp.cs | 117 | new | 1 | yes |  | `set => _itemGroupFactory = value ?? throw new ArgumentNullException(nameof(value));` |
| QfcQueue.Tlp.cs | 120 | new | 1 | yes |  | `private Func<TableLayoutPanel, TableLayoutPanel> _backgroundTlpFactory = tlp =>` |
| QfcQueue.Tlp.cs | 121 | new | 0 | no |  | `tlp.Clone(name: "BackgroundTableLayout");` |
| QfcQueue.Tlp.cs | 134 | new | 1 | yes |  | `get => _backgroundTlpFactory;` |
| QfcQueue.Tlp.cs | 135 | new | 1 | yes |  | `set => _backgroundTlpFactory = value ?? throw new ArgumentNullException(nameof(value));` |
| QfcQueue.Tlp.cs | 143 | relocated | 1 | yes | ambiguous | `{` |
| QfcQueue.Tlp.cs | 146 | relocated | 1 | yes | 0 | `var grp = new QfcItemGroup(mailItem);` |
| QfcQueue.Tlp.cs | 147 | new | 1 | yes |  | `var viewer = ItemViewerFactory(_token);` |
| QfcQueue.Tlp.cs | 148 | relocated | 1 | yes | 0 | `grp.ItemViewer = viewer;` |
| QfcQueue.Tlp.cs | 149 | new | 1 | yes |  | `await UiIdleCallAsync(() => ViewerRowPlacer(tlp, viewer, indexNumber));` |
| QfcQueue.Tlp.cs | 150 | relocated | 1 | yes | 0 | `return grp;` |
| QfcQueue.Tlp.cs | 151 | relocated | 1 | yes | ambiguous | `}` |
| QfcQueue.Tlp.cs | 154 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 157 | relocated | 0 | no | 0 | `viewer.Parent = tlp;` |
| QfcQueue.Tlp.cs | 158 | relocated | 0 | no | 0 | `tlp.SetCellPosition(viewer, new TableLayoutPanelCellPosition(0, indexNumber));` |
| QfcQueue.Tlp.cs | 159 | relocated | 0 | no | 0 | `tlp.SetColumnSpan(viewer, 2);` |
| QfcQueue.Tlp.cs | 160 | relocated | 0 | no | 0 | `viewer.AutoSize = true;` |
| QfcQueue.Tlp.cs | 161 | relocated | 0 | no | 0 | `viewer.AutoSizeMode = AutoSizeMode.GrowAndShrink;` |
| QfcQueue.Tlp.cs | 162 | relocated | 0 | no | 0 | `viewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;` |
| QfcQueue.Tlp.cs | 163 | relocated | 0 | no | 0 | `viewer.Dock = DockStyle.Fill;` |
| QfcQueue.Tlp.cs | 164 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 167 | relocated | 1 | yes | ambiguous | `{` |
| QfcQueue.Tlp.cs | 168 | relocated | 1 | yes | 1 | `var oldRowCount = tlp.RowCount - 1;` |
| QfcQueue.Tlp.cs | 169 | relocated | 1 | yes | 1 | `if (oldRowCount != newRowCount)` |
| QfcQueue.Tlp.cs | 170 | relocated | 1 | yes | ambiguous | `{` |
| QfcQueue.Tlp.cs | 171 | relocated | 1 | yes | 1 | `var diff = newRowCount - Math.Max(0, oldRowCount);` |
| QfcQueue.Tlp.cs | 172 | relocated | 1 | yes | 1 | `if (diff > 0)` |
| QfcQueue.Tlp.cs | 173 | relocated | 1 | yes | ambiguous | `{` |
| QfcQueue.Tlp.cs | 174 | relocated | 1 | yes | 1 | `tlp.InsertSpecificRow(oldRowCount, rowStyleTemplate, diff);` |
| QfcQueue.Tlp.cs | 175 | relocated | 1 | yes | ambiguous | `tlp.MinimumSize = new System.Drawing.Size(` |
| QfcQueue.Tlp.cs | 176 | relocated | 1 | yes | ambiguous | `tlp.MinimumSize.Width,` |
| QfcQueue.Tlp.cs | 177 | relocated | 1 | yes | 1 | `tlp.MinimumSize.Height + (int)Math.Round(rowStyleTemplate.Height * diff, 0)` |
| QfcQueue.Tlp.cs | 178 | relocated | 1 | yes | ambiguous | `);` |
| QfcQueue.Tlp.cs | 179 | relocated | 1 | yes | ambiguous | `}` |
| QfcQueue.Tlp.cs | 181 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 182 | relocated | 0 | no | 0 | `tlp.RemoveSpecificRow(newRowCount, diff);` |
| QfcQueue.Tlp.cs | 183 | relocated | 0 | no | ambiguous | `tlp.MinimumSize = new System.Drawing.Size(` |
| QfcQueue.Tlp.cs | 184 | relocated | 0 | no | ambiguous | `tlp.MinimumSize.Width,` |
| QfcQueue.Tlp.cs | 185 | relocated | 0 | no | 0 | `tlp.MinimumSize.Height - (int)Math.Round(rowStyleTemplate.Height * diff, 0)` |
| QfcQueue.Tlp.cs | 186 | relocated | 0 | no | ambiguous | `);` |
| QfcQueue.Tlp.cs | 187 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 188 | relocated | 1 | yes | ambiguous | `}` |
| QfcQueue.Tlp.cs | 189 | relocated | 1 | yes | ambiguous | `}` |
| QfcQueue.Tlp.cs | 199 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 201 | relocated | 0 | no | 0 | `await JobsToFinish(100, _token);` |
| QfcQueue.Tlp.cs | 204 | relocated | 0 | no | 0 | `AdjustTlp(TlpTemplate, newRowCount, rowStyleTemplate);` |
| QfcQueue.Tlp.cs | 207 | relocated | 0 | no | 0 | `var oldQueue = _queue;` |
| QfcQueue.Tlp.cs | 210 | relocated | 0 | no | 0 | `_queue =` |
| QfcQueue.Tlp.cs | 211 | relocated | 0 | no | ambiguous | `new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>();` |
| QfcQueue.Tlp.cs | 214 | relocated | 0 | no | 0 | `Interlocked.Increment(ref _jobsRunning);` |
| QfcQueue.Tlp.cs | 216 | relocated | 0 | no | 0 | `var queue =` |
| QfcQueue.Tlp.cs | 217 | relocated | 0 | no | ambiguous | `new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>();` |
| QfcQueue.Tlp.cs | 219 | relocated | 0 | no | 0 | `while (oldQueue.Count > 0)` |
| QfcQueue.Tlp.cs | 220 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 221 | relocated | 0 | no | 0 | `var nextEntry = oldQueue.Take();` |
| QfcQueue.Tlp.cs | 222 | relocated | 0 | no | 0 | `GrowEntry(ref entry, ref nextEntry, newRowCount, rowStyleTemplate);` |
| QfcQueue.Tlp.cs | 223 | relocated | 0 | no | 0 | `if (entry.ItemGroups.Count == newRowCount)` |
| QfcQueue.Tlp.cs | 224 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 225 | relocated | 0 | no | ambiguous | `RenumberGroups(entry.ItemGroups);` |
| QfcQueue.Tlp.cs | 226 | relocated | 0 | no | ambiguous | `queue.Add(entry);` |
| QfcQueue.Tlp.cs | 227 | relocated | 0 | no | 0 | `if (nextEntry.ItemGroups.Count > 0)` |
| QfcQueue.Tlp.cs | 228 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 229 | relocated | 0 | no | 0 | `entry = nextEntry;` |
| QfcQueue.Tlp.cs | 230 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 232 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 233 | relocated | 0 | no | 0 | `if (oldQueue.Count > 0)` |
| QfcQueue.Tlp.cs | 234 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 235 | relocated | 0 | no | 0 | `entry = oldQueue.Take();` |
| QfcQueue.Tlp.cs | 236 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 238 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 239 | relocated | 0 | no | 0 | `entry = default;` |
| QfcQueue.Tlp.cs | 240 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 241 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 242 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 243 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 245 | relocated | 0 | no | 0 | `if (entry != default)` |
| QfcQueue.Tlp.cs | 246 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 247 | relocated | 0 | no | 0 | `var items = await _homeController.DataModel.DequeueNextItemGroupAsync(` |
| QfcQueue.Tlp.cs | 248 | relocated | 0 | no | 0 | `newRowCount - entry.ItemGroups.Count,` |
| QfcQueue.Tlp.cs | 249 | relocated | 0 | no | 0 | `1000` |
| QfcQueue.Tlp.cs | 250 | relocated | 0 | no | ambiguous | `);` |
| QfcQueue.Tlp.cs | 251 | relocated | 0 | no | 0 | `if (items.Count > 0)` |
| QfcQueue.Tlp.cs | 252 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 253 | relocated | 0 | no | 0 | `AdjustTlp(entry.Tlp, newRowCount, rowStyleTemplate);` |
| QfcQueue.Tlp.cs | 254 | relocated | 0 | no | 0 | `var extraGroups = await LoadControllersViewersAsync(` |
| QfcQueue.Tlp.cs | 255 | relocated | 0 | no | 0 | `items,` |
| QfcQueue.Tlp.cs | 256 | relocated | 0 | no | 0 | `_globals,` |
| QfcQueue.Tlp.cs | 257 | relocated | 0 | no | 0 | `_homeController,` |
| QfcQueue.Tlp.cs | 258 | relocated | 0 | no | 0 | `_qfcCollectionController,` |
| QfcQueue.Tlp.cs | 259 | relocated | 0 | no | 0 | `entry.Tlp,` |
| QfcQueue.Tlp.cs | 260 | relocated | 0 | no | 0 | `entry.ItemGroups.Count` |
| QfcQueue.Tlp.cs | 261 | relocated | 0 | no | ambiguous | `);` |
| QfcQueue.Tlp.cs | 262 | relocated | 0 | no | 0 | `extraGroups.ForEach(group => entry.ItemGroups.Add(group));` |
| QfcQueue.Tlp.cs | 263 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 264 | relocated | 0 | no | ambiguous | `RenumberGroups(entry.ItemGroups);` |
| QfcQueue.Tlp.cs | 265 | relocated | 0 | no | ambiguous | `queue.Add(entry);` |
| QfcQueue.Tlp.cs | 266 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 269 | relocated | 0 | no | 0 | `_ = queue.Take();` |
| QfcQueue.Tlp.cs | 272 | relocated | 0 | no | 0 | `_queue = queue;` |
| QfcQueue.Tlp.cs | 273 | relocated | 0 | no | 0 | `Interlocked.Decrement(ref _jobsRunning);` |
| QfcQueue.Tlp.cs | 275 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.Tlp.cs | 278 | relocated | 1 | yes | ambiguous | `{` |
| QfcQueue.Tlp.cs | 279 | relocated | 1 | yes | 1 | `var digits = itemGroups.Count >= 10 ? 2 : 1;` |
| QfcQueue.Tlp.cs | 280 | relocated | 1 | yes | 1 | `for (int i = 0; i < itemGroups.Count; i++)` |
| QfcQueue.Tlp.cs | 281 | relocated | 1 | yes | ambiguous | `{` |
| QfcQueue.Tlp.cs | 282 | relocated | 1 | yes | 1 | `itemGroups[i].ItemController.ItemNumberDigits = digits;` |
| QfcQueue.Tlp.cs | 283 | relocated | 1 | yes | 1 | `itemGroups[i].ItemController.ItemNumber = i + 1;` |
| QfcQueue.Tlp.cs | 284 | relocated | 1 | yes | ambiguous | `}` |
| QfcQueue.Tlp.cs | 285 | relocated | 1 | yes | ambiguous | `}` |
| QfcQueue.Tlp.cs | 293 | relocated | 1 | yes | ambiguous | `{` |
| QfcQueue.Tlp.cs | 294 | relocated | 1 | yes | 1 | `var currentCount = target.ItemGroups.Count;` |
| QfcQueue.Tlp.cs | 295 | relocated | 1 | yes | 1 | `var grow = Math.Min(newRowCount - currentCount, source.ItemGroups.Count);` |
| QfcQueue.Tlp.cs | 297 | relocated | 1 | yes | 1 | `AdjustTlp(target.Tlp, newRowCount, rowStyleTemplate);` |
| QfcQueue.Tlp.cs | 299 | relocated | 1 | yes | 1 | `if (grow == 0)` |
| QfcQueue.Tlp.cs | 300 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.Tlp.cs | 301 | relocated | 0 | no | 0 | `return;` |
| QfcQueue.Tlp.cs | 304 | relocated | 1 | yes | 1 | `for (int i = 0; i < grow; i++)` |
| QfcQueue.Tlp.cs | 305 | relocated | 1 | yes | ambiguous | `{` |
| QfcQueue.Tlp.cs | 306 | relocated | 1 | yes | 1 | `var itemViewer = source.Tlp.Controls[i];` |
| QfcQueue.Tlp.cs | 307 | relocated | 1 | yes | 1 | `var position = source.Tlp.GetCellPosition(itemViewer);` |
| QfcQueue.Tlp.cs | 308 | relocated | 1 | yes | 1 | `itemViewer.Parent = target.Tlp;` |
| QfcQueue.Tlp.cs | 309 | relocated | 1 | yes | 1 | `target.Tlp.SetCellPosition(` |
| QfcQueue.Tlp.cs | 310 | relocated | 1 | yes | 1 | `itemViewer,` |
| QfcQueue.Tlp.cs | 311 | relocated | 1 | yes | 1 | `new TableLayoutPanelCellPosition(position.Column, currentCount + i)` |
| QfcQueue.Tlp.cs | 312 | relocated | 1 | yes | ambiguous | `);` |
| QfcQueue.Tlp.cs | 313 | relocated | 1 | yes | 1 | `var group = source.ItemGroups[0];` |
| QfcQueue.Tlp.cs | 314 | relocated | 1 | yes | 1 | `target.ItemGroups.Add(group);` |
| QfcQueue.Tlp.cs | 315 | relocated | 1 | yes | 1 | `source.ItemGroups.RemoveAt(0);` |
| QfcQueue.Tlp.cs | 316 | relocated | 1 | yes | 1 | `group.ItemController.ItemNumber = currentCount + i + 1;` |
| QfcQueue.Tlp.cs | 317 | relocated | 1 | yes | ambiguous | `}` |
| QfcQueue.Tlp.cs | 319 | relocated | 1 | yes | 1 | `source.Tlp.RemoveSpecificRow(0, grow);` |
| QfcQueue.Tlp.cs | 321 | relocated | 1 | yes | 1 | `source.Tlp.MinimumSize = new System.Drawing.Size(` |
| QfcQueue.Tlp.cs | 322 | relocated | 1 | yes | 1 | `source.Tlp.MinimumSize.Width,` |
| QfcQueue.Tlp.cs | 323 | relocated | 1 | yes | 1 | `source.Tlp.MinimumSize.Height - (int)Math.Round(rowStyleTemplate.Height * grow, 0)` |
| QfcQueue.Tlp.cs | 324 | relocated | 1 | yes | ambiguous | `);` |
| QfcQueue.Tlp.cs | 325 | relocated | 1 | yes | ambiguous | `}` |
| QfcQueue.UiIdle.cs | 49 | new | 1 | yes |  | `get => _uiIdleDispatcher ??= new UiThreadIdleDispatcher();` |
| QfcQueue.UiIdle.cs | 50 | new | 1 | yes |  | `set => _uiIdleDispatcher = value ?? throw new ArgumentNullException(nameof(value));` |
| QfcQueue.UiIdle.cs | 54 | new | 1 | yes |  | `UiIdleDispatcher.InvokeIdleAsync(action);` |
| QfcQueue.UiIdle.cs | 57 | new | 1 | yes |  | `UiIdleDispatcher.InvokeIdleAsync<T>(func);` |
| QfcQueue.UiIdle.cs | 60 | new | 1 | yes |  | `UiIdleDispatcher.InvokeIdleAsync<T>(func);` |
| QfcQueue.UiIdle.cs | 78 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.UiIdle.cs | 79 | relocated | 0 | no | 0 | `await UiThread.Dispatcher.InvokeAsync(` |
| QfcQueue.UiIdle.cs | 80 | relocated | 0 | no | 0 | `action,` |
| QfcQueue.UiIdle.cs | 81 | relocated | 0 | no | ambiguous | `System.Windows.Threading.DispatcherPriority.ContextIdle` |
| QfcQueue.UiIdle.cs | 82 | relocated | 0 | no | ambiguous | `);` |
| QfcQueue.UiIdle.cs | 83 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.UiIdle.cs | 86 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.UiIdle.cs | 87 | relocated | 0 | no | 0 | `return await UiThread.Dispatcher.InvokeAsync(` |
| QfcQueue.UiIdle.cs | 88 | relocated | 0 | no | 0 | `func,` |
| QfcQueue.UiIdle.cs | 89 | relocated | 0 | no | ambiguous | `System.Windows.Threading.DispatcherPriority.ContextIdle` |
| QfcQueue.UiIdle.cs | 90 | relocated | 0 | no | ambiguous | `);` |
| QfcQueue.UiIdle.cs | 91 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.UiIdle.cs | 94 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.UiIdle.cs | 95 | relocated | 0 | no | 0 | `T result = await await UiThread.Dispatcher.InvokeAsync(` |
| QfcQueue.UiIdle.cs | 96 | relocated | 0 | no | 0 | `async () =>` |
| QfcQueue.UiIdle.cs | 97 | relocated | 0 | no | ambiguous | `{` |
| QfcQueue.UiIdle.cs | 98 | relocated | 0 | no | none | `T result = await func();` |
| QfcQueue.UiIdle.cs | 99 | relocated | 0 | no | none | `await Task.Yield();` |
| QfcQueue.UiIdle.cs | 100 | relocated | 0 | no | ambiguous | `return result;` |
| QfcQueue.UiIdle.cs | 101 | relocated | 0 | no | none | `},` |
| QfcQueue.UiIdle.cs | 102 | relocated | 0 | no | ambiguous | `System.Windows.Threading.DispatcherPriority.ContextIdle` |
| QfcQueue.UiIdle.cs | 103 | relocated | 0 | no | ambiguous | `);` |
| QfcQueue.UiIdle.cs | 104 | relocated | 0 | no | ambiguous | `return result;` |
| QfcQueue.UiIdle.cs | 106 | relocated | 0 | no | ambiguous | `}` |
| QfcQueue.cs | 58 | new | 1 | yes |  | `get => _moveMonitor;` |
| QfcQueue.cs | 59 | new | 1 | yes |  | `set => _moveMonitor = value ?? throw new ArgumentNullException(nameof(value));` |

TableRowCount: 187

Output Summary: The anchored zero-context diff of the five production Write Set paths produced 498 added
and 264 removed lines. Mechanical text-equality classification gives 199 genuinely new and 299 relocated;
311 carry no line element in the P5-T5 document and are excluded from both rates. The genuinely-new line
rate is 0.958333 over 23 of 24 covered, which clears the 0.90 new-code floor the standing instructions
file states, so the gate PASSES. The single uncovered genuinely-new line is the default lambda body of
BackgroundTlpFactory at QfcQueue.Tlp.cs line 121, deliberately never invoked and carried into the P6-T3
residual record. The relocated line rate is 0.343558 over 56 of 163. On the 85 relocated lines whose
anchor statement is uniquely identifiable, coverage rose from 0.341176 to 0.4 with zero regressions and
five newly covered statements; the naive all-matches anchor figure of 0.578616 is reported and marked
unsound because bare brace text matches many removed lines. No coverage-exclusion attribute and no
assembly-level exclusion was introduced. Acceptance met.
