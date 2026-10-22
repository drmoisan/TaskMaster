# Residual uncovered regions after the issue 871 seam work

Timestamp: 2026-09-13T16-59
Command: enumeration of every line element with a zero hits attribute in the P5-T5 Cobertura document, grouped into contiguous runs and resolved against the source files
EXIT_CODE: 0

## The document this record is derived from

Every region below cites the post-change Cobertura document produced by P5-T5, whose file name is
coverage-postchange.2026-09-12T10-25.cobertura.xml in the qa-gates evidence directory of this feature
folder, and is interpreted in p5-t5-coverage-postchange.2026-09-12T10-25.md in the same directory. For
each region the cited element is the `class` element whose `filename` attribute is given, together with
the `line` element `number` values of the run.

## Counting convention, stated because it changes what counts as a residual

A Cobertura document produced by this runner can carry more than one `line` element for the same source
line number, because a lambda or an async state machine compiles to its own method element that repeats
the line. `Get-CoberturaClassLineSummary`, and therefore every file-level rate in this change's evidence,
deduplicates by line number and keeps the highest hit count. This record applies the same rule, so a
region appears here only when **every** `line` element for that line reports zero hits.

One consequence is worth stating explicitly. `QuickFiler/Controllers/QfcQueue.Enqueue.cs` carries a
zero-hit `line` element for line 91 inside a compiler-generated `MoveNext` state machine — the async
lambda `async (x) => await RemoveItem(x)` that the hook loop passes to the move monitor. P4-T12
deliberately does not invoke that captured delegate, because it is async-void shaped, so its state
machine never runs. Under the deduplicating rule the line is covered, because the enclosing loop
statement's own `line` element reports hits. The uninvoked delegate body is recorded here in the section
for that file rather than being silently dropped.

## Summary of residual regions by file

| File | Deduped valid lines | Covered | Zero-hit | Zero-hit runs |
|---|---|---|---|---|
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 85 | 85 | 0 | 0 |
| `QuickFiler/Controllers/QfcQueue.cs` | 155 | 109 | 46 | 8 |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` | 151 | 67 | 84 | 16 |
| `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | 29 | 5 | 24 | 3 |
| UtilitiesCS Extensions WinFormsExtensions.cs, clone members only | 22 | 0 | 22 | 3 |

---

## 1. The reflection-driven control clone and the template setter that calls it

**Region 1a — the three clone overloads in the utilities extensions namespace.**
Cited element: `class` element with `filename` attribute `UtilitiesCS\Extensions\WinFormsExtensions.cs`,
class name UtilitiesCS.WinFormsExtensions, whose own line-rate attribute reads 0.178182.

| Member | Line elements | Zero-hit |
|---|---|---|
| `Clone<T>(T, string, bool)` | 274 to 278 | 5 of 5 |
| `Clone<T>(T, bool)` | 282 to 295 | 10 of 10 |
| `Clone<T>(T, bool, int)` | 299 to 308 | 7 of 7 |

Why it remains uncovered: these overloads walk a control's public properties by reflection and copy each
one onto a newly constructed instance of the same runtime type. Running them requires a real WinForms
control graph with a live handle, which a headless test host does not have. The background-template seam
`BackgroundTlpFactory` exists precisely so the enqueue path can be driven without reaching them, and
every test substitutes a factory returning a sentinel panel. Covering them is not in scope for this item:
they live in another assembly and outside this item's Write Set, and closing them would require either a
UI-thread test host or a refactor of the clone itself.

**Region 1b — the template setter that calls the clone.**
Cited element: `class` element with `filename` attribute `QuickFiler\Controllers\QfcQueue.Tlp.cs`,
`line` elements 35, 37, 38, 39 and 43 — the getter and setter of `TlpTemplate`.

```
35   get => _tlpTemplate;
37   {
38       _tlpTemplate = value.Clone();
39       _tlpTemplate.Name = "TemplateTableLayout";
43   }
```

Why it remains uncovered: assigning `TlpTemplate` invokes the clone described in region 1a on the
assigned panel, so no headless test can exercise the setter without reaching an untestable region. The
getter at line 35 is uncovered for the same reason: nothing in the covered enqueue path reads the
property, and the only production reader is `ChangeIterationSize`, which is itself residual under section
5b below. This property predates the change and no seam was introduced for it; the item's scope is the
enqueue path.

---

## 2. The dead, entirely commented-out template-activation member

Cited element: `class` element with `filename` attribute `QuickFiler\Controllers\QfcQueue.Tlp.cs`,
`line` elements 47 and 53.

```
46   internal void ActivateTlpTemplate(TableLayoutPanel tlp)
47   {
48       //_templateViewer.Text = "TemplateViewer";
49       //_templateViewer.L1v0L2_PanelMain.Controls.Remove(...);
50       //_templateViewer.L1v0L2L3v_TableLayout = tlp;
51       //_templateViewer.L1v0L2L3v_TableLayout.Parent = ...;
52       //_templateViewer.L1v0L2L3v_TableLayout.Visible = true;
53   }
```

Why it remains uncovered: the member's entire body is commented out, so it has no executable statement
and no caller anywhere in the solution. The only two `line` elements it carries are its opening and
closing braces. It was moved verbatim into this file by the Phase 1 split, which was a relocation
mandated by the 500-line ceiling; deleting dead code is a separate change with its own review surface and
is out of scope for this item, which is why the split moved it rather than dropping it. A test cannot
cover it in any meaningful sense: calling it would execute nothing.

---

## 3. The three relocated marshalling bodies inside the adapter class

Cited element: `class` element with `filename` attribute `QuickFiler\Controllers\QfcQueue.UiIdle.cs`,
`line` elements 78 to 83, 86 to 91 and 94 to 106. All 24 of that file's zero-hit lines fall in these
three runs, and the file's own line-rate attribute reads 0.172414 over 5 of 29 covered.

| Member of `UiThreadIdleDispatcher` | Line elements | Zero-hit |
|---|---|---|
| `InvokeIdleAsync(System.Action)` | 78 to 83 | 6 of 6 |
| `InvokeIdleAsync<T>(Func<T>)` | 86 to 91 | 6 of 6 |
| `InvokeIdleAsync<T>(Func<Task<T>>)` | 94 to 106, including the inner async lambda at 97 to 101 | 13 of 13 |

Why they remain uncovered: each body calls `UiThread.Dispatcher.InvokeAsync` at `ContextIdle` priority.
That requires a live process-wide WPF dispatcher, which a headless MSTest host does not have; a test that
invoked them would either hang waiting for a dispatcher pump or install a process-global dispatcher and
make every other test in the assembly order-dependent. This is the same region that was uncovered before
the change: P6-T2's unambiguous anchor comparison shows these statements at zero hits at the anchor as
well, when they were the bodies of the three marshalling members in the pre-split base part. The change
relocated them verbatim into the adapter — which is what makes seam S2 a relocation rather than a
reimplementation — and did not make them reachable.

What the change did achieve here is that the three *call sites* are now covered. The five covered lines
of this file are the three one-line forwards, the seam property getter and its setter guard, and P4-T18
asserts that a substituted dispatcher records one invocation of each of the three shapes. The untestable
region is now isolated behind an interface instead of being inlined into the queue class.

The commented-out alternative implementation at line 105 travels with the third body and carries no
`line` element, so it appears in neither the numerator nor the denominator. It is retained deliberately:
deleting it would break the verbatim-move property that acceptance criterion AC18 and P6-T5 gate.

---

## 4. The default lambda of `BackgroundTlpFactory`

Cited element: `class` element with `filename` attribute `QuickFiler\Controllers\QfcQueue.Tlp.cs`,
`line` element 121, inside the compiler-generated method `<.ctor>b__0_1`.

```
120   private Func<TableLayoutPanel, TableLayoutPanel> _backgroundTlpFactory = tlp =>
121       tlp.Clone(name: "BackgroundTableLayout");
```

Why it remains uncovered: the lambda body performs the same reflection-driven clone call, with the same
named argument, as the expression it replaced on the enqueue path, so it reaches region 1a. It is
deliberately never invoked by any test. Every test that drives the enqueue path assigns a substitute
returning a sentinel panel, which is what makes that path reachable headlessly at all; a test that left
the default in place would reach the untestable clone and would be testing region 1a rather than the
enqueue path.

This is the single uncovered line among the 24 genuinely-new executable lines P6-T2 measured, and it is
the reason the genuinely-new rate reads 0.958333 rather than 1. It is recorded here rather than excluded
from measurement: no `[ExcludeFromCodeCoverage]` attribute and no assembly-level exclusion is introduced
anywhere in this change, so the cost of the untestable default stays visible in the metric.

---

## 5. Every remaining statement at zero hits in the enqueue-path files

### 5a. `QuickFiler/Controllers/QfcQueue.Enqueue.cs`

ZeroHitRunCount: 0

Cited element: `class` element with `filename` attribute `QuickFiler\Controllers\QfcQueue.Enqueue.cs`,
whose own line-rate attribute reads 1 over 85 of 85 covered. Under the deduplicating convention stated
above, this file has no residual region: every executable line is covered.

One sub-line residual exists and is recorded for completeness. The `line` element for line 91 inside the
compiler-generated `MoveNext` of the async lambda passed to the move monitor reports zero hits:

```
91   items.ForEach(item => MoveMonitor.HookItem(item, async (x) => await RemoveItem(x)))
```

Why it remains uncovered: P4-T12 captures that delegate and asserts it is non-null but does not invoke
it, because it is async-void shaped and invoking it from a test would raise on a thread the test cannot
observe. The hook-loop statement itself is covered, which is what the file's rate of 1 reflects; the body
of the uninvoked lambda is not. Covering it would require a decision about async-void callback semantics
that is out of scope for this item.

### 5b. `QuickFiler/Controllers/QfcQueue.Tlp.cs`, excluding regions 1b, 2 and 4 already recorded

Cited element: `class` element with `filename` attribute `QuickFiler\Controllers\QfcQueue.Tlp.cs`,
line-rate attribute 0.443709 over 67 of 151 covered.

| `line` elements | Member | Why uncovered |
|---|---|---|
| 154, 157 to 164 | `AddViewerToTlp` | The production default of seam S4. Its body assigns `Parent`, cell position, column span, autosize, border style and dock on a real `ItemViewer` control, which requires a live WinForms control graph. Every test substitutes `ViewerRowPlacer` with a recording delegate, and P4-T17 asserts the recorded arguments. The seam exists so `AddAsync` is coverable without this body; covering the body itself needs a UI-thread host. |
| 181 to 187 | `AdjustTlp`, the row-removal and minimum-size branch | Reached only when the old row count differs from the new one and rows must be removed. It calls `RemoveSpecificRow` and sets `MinimumSize` on a real panel. Covered lines exist elsewhere in this member, so the member is partially covered at 7 of 22 zero; this branch is not on the enqueue path and is driven only by `ChangeIterationSize`. |
| 199 to 201, 204, 207, 210 to 211, 214 to 266, 269, 272 to 275 | `ChangeIterationSize`, the whole body including its async `MoveNext` at 58 of 58 zero | The iteration-resize workflow. It awaits `JobsToFinish`, calls `AdjustTlp` on `TlpTemplate` (region 1b), rebuilds the blocking collection, calls `GrowEntry`, `RenumberGroups` and `DequeueNextItemGroupAsync` on the home controller's data model, and repopulates the queue. It is not on the enqueue path, no seam was introduced for it by this item, and driving it headlessly needs both a live panel graph and a home controller. It was uncovered at the anchor and is unchanged by this item beyond its relocation. |
| 300 to 301 | `GrowEntry`, the early-return arm when the growth count is zero | Reached only from `ChangeIterationSize`, which is itself uncovered. The rest of `GrowEntry` is covered at 2 of 27 zero, so the member is largely exercised; this one arm is not. |

### 5c. `QuickFiler/Controllers/QfcQueue.cs`

Cited element: `class` element with `filename` attribute `QuickFiler\Controllers\QfcQueue.cs`,
line-rate attribute 0.703226 over 109 of 155 covered.

| `line` elements | Member | Why uncovered |
|---|---|---|
| 139 to 142 | `TryDequeueAsync`, the poll-retry arm taken when `TryTake` times out | Requires a dequeue attempt that times out while jobs are still running. Reaching it deterministically needs a controllable clock inside `BlockingCollection.TryTake`, which the framework type does not offer; a wall-clock wait is prohibited by the determinism rules. |
| 145 to 147 | `TryDequeueAsync`, the completed-queue drain arm | Requires the blocking collection to be marked complete while still holding an entry. No production path in this item's scope marks completion. |
| 163 to 167 | `TryDequeueAsync`, the timeout arm of the `OperationCanceledException` handler | Requires the function-timeout linked token to fire rather than the caller's token. Both arms of that handler are distinguished only by which token cancelled; the cancelled-by-caller arm is covered and the timed-out arm needs a real timeout to elapse. |
| 173 to 178 | `TryDequeueAsync`, the general `System.Exception` catch and its error log | Requires an unexpected exception from the queue primitive. The catch is defensive; no seam in this item lets a test inject a throw into `BlockingCollection`. |
| 196, 205 to 228 | `RemoveItem`, the normal-completion path after the cancellation guard | Existing tests drive `RemoveItem` with an already-cancelled instance token, so the guarded early return at 197 to 202 is covered and the body that exchanges the queue, increments the running-jobs counter, marshals the row removal through `UiIdleCallAsync` and decrements again is not. Covering it needs a non-cancelled token plus a real row to remove, which reaches region 1b territory. Not on the enqueue path and no seam was introduced for it. |
| 238 to 239 | `JobsToFinish`, the polling delay inside the wait loop | Reached only when jobs are still running at the moment `JobsToFinish` is called. Covering it deterministically needs a controllable clock for `Task.Delay`; a real wall-clock wait is prohibited. |
| 255 to 260 | `NotifyPropertyChanged` | No production member of the queue raises property-changed notification, so the helper has no caller. The collection-changed event, which does have callers, is covered by the two tests P4-T11 added. |

---

## Completeness statement

The enumeration above covers every contiguous run of zero-hit lines that the P5-T5 document reports for
each of the four production files of this item's Write Set — 0 runs in the enqueue part, 8 in the base
part, 16 in the Tlp part and 3 in the UiIdle part, totalling 27 runs and 154 lines — together with the
three clone members in the utilities extensions file that the enqueue path would otherwise reach. Each
run is named, attributed to a member and given a reason. No zero-hit region in those files is omitted.

Regions of the utilities extensions file other than the three clone overloads are not enumerated: they
are not on the enqueue path, this item does not call them, and that file is outside the Write Set. Its
226 zero-hit lines are a pre-existing property of another assembly.

## No exclusions were introduced

No `[ExcludeFromCodeCoverage]` attribute was added to any type or member by this change, and no
assembly-level or file-level exclusion was added to any coverage configuration. Every residual region
above remains in the coverage denominator, which is what keeps its cost visible and keeps pressure on a
future item to close it.

Output Summary: 27 contiguous zero-hit runs totalling 154 lines across the four production files are
enumerated, attributed to members and explained, plus the three reflection-driven clone overloads in the
utilities extensions file. `QuickFiler/Controllers/QfcQueue.Enqueue.cs` has no residual region at all: 85
of 85 covered. The residuals fall into five causes — the reflection-driven clone and its callers, dead
commented-out code, the three adapter bodies needing a live WPF dispatcher, the deliberately uninvoked
`BackgroundTlpFactory` default, and off-enqueue-path members with no seam. No coverage exclusion of any
kind was introduced. Acceptance met.
