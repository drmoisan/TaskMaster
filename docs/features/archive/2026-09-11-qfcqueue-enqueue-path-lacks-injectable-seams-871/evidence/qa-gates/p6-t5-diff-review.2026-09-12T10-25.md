# P6-T5 — No-behaviour-change diff review

Timestamp: 2026-09-13T17-02
Command: git diff --unified=0 8213826f695439e86e3ed34faa575de493a11ec7..HEAD -- QuickFiler/Controllers/QfcQueue.cs QuickFiler/Controllers/QfcQueue.Enqueue.cs QuickFiler/Controllers/QfcQueue.Tlp.cs QuickFiler/Controllers/QfcQueue.UiIdle.cs ; git status --porcelain --untracked-files=all
EXIT_CODE: 0

## Anchored diffstat for the four production files

```
 QuickFiler/Controllers/QfcQueue.Enqueue.cs |  10 +-
 QuickFiler/Controllers/QfcQueue.Tlp.cs     | 329 +++++++++++++++++++++++++++++
 QuickFiler/Controllers/QfcQueue.UiIdle.cs  | 108 ++++++++++
 QuickFiler/Controllers/QfcQueue.cs         | 280 ++----------------------
 4 files changed, 463 insertions(+), 264 deletions(-)
```

## Anchored diff of `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, verbatim

```
diff --git a/QuickFiler/Controllers/QfcQueue.Enqueue.cs b/QuickFiler/Controllers/QfcQueue.Enqueue.cs
index e3b30c522..bc678f5f3 100644
--- a/QuickFiler/Controllers/QfcQueue.Enqueue.cs
+++ b/QuickFiler/Controllers/QfcQueue.Enqueue.cs
@@ -91 +91 @@ namespace QuickFiler.Controllers
-                items.ForEach(item => _moveMonitor.HookItem(item, async (x) => await RemoveItem(x)))
+                items.ForEach(item => MoveMonitor.HookItem(item, async (x) => await RemoveItem(x)))
@@ -97,3 +97 @@ namespace QuickFiler.Controllers
-            var tlp = await UiIdleCallAsync(() =>
-                _tlpTemplate.Clone(name: "BackgroundTableLayout")
-            );
+            var tlp = await UiIdleCallAsync(() => BackgroundTlpFactory(_tlpTemplate));
@@ -177 +175,3 @@ namespace QuickFiler.Controllers
-                .SelectAwait(async i => (i: i, grp: await AddAsync(tlp, items[i - start], i)))
+                .SelectAwait(async i =>
+                    (i: i, grp: await ItemGroupFactory(tlp, items[i - start], i))
+                )
```

Three hunks, one per seam substitution: S1 at line 91, S6 at lines 97 to 99, and S5 at line 177. Nothing
else in this file changed.

## Anchored diff of `QuickFiler/Controllers/QfcQueue.cs`, hunk inventory

The base part's diff carries exactly four hunks, reproduced here as an inventory because the full text of
the two deletion hunks is 259 lines and is reproduced in full by the P1-T1 and P1-T2 split artifacts:

| Hunk | Old range | New range | Content |
|---|---|---|---|
| 1 | line 1 | line 1 | The UTF-8 byte-order mark was dropped from line 1. The text `using System;` is otherwise unchanged. |
| 2 | after line 43 | lines 44 to 61 | Seam S1 added: the `MoveMonitor` property with its XML doc comment. |
| 3 | lines 230 to 453 | line 248 | The whole Tlp Manipulation region deleted, replaced by a one-line breadcrumb comment. |
| 4 | lines 472 to 505 | line 267 | The whole Helper Methods region deleted, replaced by a one-line breadcrumb comment. |

The two new files carry a single whole-file insertion hunk each, 329 lines and 108 lines respectively.

## Porcelain capture, paired with the diff so created files are visible

```
 M docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p6-t1-coverage-file-rates.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p6-t2-new-code-coverage.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p6-t4-repo-wide-projection.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/residual-uncovered-regions.2026-09-12T10-25.md
```

The porcelain span is paired with the anchored diff because a commit-to-commit comparison cannot show a
file this change has created but not yet committed. Every path in this capture is a Phase 6 Markdown
artifact or the plan itself; the four production files and the two new test files are all committed and
therefore appear only in the anchored diff. The porcelain contains no source, test or project file, which
is the direct evidence that Phase 6 modified none, and therefore that the clean toolchain pass Phase 5
recorded is still valid.

---

## Verdicts on the eight properties

### Property 1 — every relocated member moved verbatim apart from the named seam substitutions

Verdict1: PASS

The mechanical evidence is the classification P6-T2 produced. Of the 498 added lines across the five
production paths, 299 are relocated, meaning their whitespace-stripped text is identical to that of at
least one removed line. Of the 199 genuinely-new lines, only 24 carry an executable statement, and every
one of those 24 is a seam declaration, a seam accessor, a substituted seam call site or a one-line
forward:

| File and line | Line | Role |
|---|---|---|
| `QuickFiler/Controllers/QfcQueue.cs` 58, 59 | getter and setter guard | seam S1 accessors |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` 91 | hook loop | S1 substitution |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` 97 | background clone | S6 substitution |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` 175, 176 | per-row construction | S5 substitution, re-wrapped by the formatter |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` 64, 78, 79 | field initializer, getter, setter guard | seam S3 |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` 96, 97 | lazy getter, setter guard | seam S4 |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` 116, 117 | lazy getter, setter guard | seam S5 |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` 120, 121, 134, 135 | field initializer lambda, getter, setter guard | seam S6 |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` 147, 149 | inside `AddAsync` | S3 and S4 substitutions |
| `QuickFiler/Controllers/QfcQueue.UiIdle.cs` 49, 50 | lazy getter, setter guard | seam S2 |
| `QuickFiler/Controllers/QfcQueue.UiIdle.cs` 54, 57, 60 | three one-line forwards | S2 call sites |

No executable line outside that list was introduced. The remaining 175 genuinely-new lines carry no
`line` element: they are using directives, namespace and type declarations, XML doc comments, blank lines
and brace-only lines.

Two byte-level deviations from strict verbatim are recorded rather than glossed:

- **Line 175 of `QuickFiler/Controllers/QfcQueue.Enqueue.cs` was re-wrapped by the formatter.** The
  single-line projection at anchor line 177 became three lines after the S5 substitution took the
  statement past CSharpier's 100-column default. P3-T5 captured the statement before the format and
  P3-T8 captured it after, so the re-wrap is auditable and is the form acceptance criterion AC5 cites.
  The expression is semantically unchanged.
- **The UTF-8 byte-order mark was dropped from `QuickFiler/Controllers/QfcQueue.cs`.** At the anchor that
  file began with the three bytes 239, 187, 191; it now begins directly with `using`. The other six
  Write Set code paths carry no byte-order mark and none was carried at the anchor either, so the tree is
  mixed and this file is now consistent with its six siblings. CSharpier rewrote the file during the
  scoped format in P1-T4 and dropped the mark. This is not a behaviour change: the C# compiler reads a
  UTF-8 file identically with or without the mark, the file's content is ASCII, and the analyzer and
  nullable gates both exit 0 at zero diagnostics afterwards. It is recorded because "verbatim" is the
  property under review and a byte-level change to line 1 is within its scope. The standing instructions
  file states that where a diff disagrees with the formatter, the formatter output wins.

### Property 2 — no nullable pragma was added to any relocated code

Verdict2: PASS

A literal search for the token `#nullable` returns these counts:

| File | `#nullable` occurrences |
|---|---|
| `QuickFiler/Controllers/QfcQueue.cs` | 0 |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 0 |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` | 0 |
| `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | 0 |
| `QuickFiler/Interfaces/IUiIdleDispatcher.cs` | 1 |
| `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` | 0 |

The single occurrence is in `QuickFiler/Interfaces/IUiIdleDispatcher.cs`, which is brand-new code and
which P2-T3 explicitly permitted to carry the pragma, following the existing threading interface files in
the utilities assembly. No file containing relocated code carries one. This matters because adding the
pragma to a relocated body would conscript it into nullable analysis and would be a change to code that
is supposed to have moved unmodified.

### Property 3 — the obsolete-API pragma pair is intact, disable and restore both present exactly once

Verdict3: PASS

In `QuickFiler/Controllers/QfcQueue.Enqueue.cs`:

```
line 171:  #pragma warning disable CS0618
line 196:  #pragma warning restore CS0618
```

DisableCount: 1
RestoreCount: 1

The pair brackets the async-enumerable projection. Neither directive appears in the anchored diff of that
file, whose three hunks sit at lines 91, 97 and 177, so both are byte-identical to the anchor. No other
production file in the Write Set carries a pragma-warning directive of any kind.

### Property 4 — every region opens and closes on the same side of the split

Verdict4: PASS

| File | `#region` | `#endregion` | Equal |
|---|---|---|---|
| `QuickFiler/Controllers/QfcQueue.cs` | 3 | 3 | yes |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 0 | 0 | yes |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` | 1 | 1 | yes |
| `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | 1 | 1 | yes |

Both relocated regions moved as complete opening-and-closing pairs. The base part retains three complete
pairs, and the diff shows the Tlp Manipulation pair leaving together in hunk 3 and the Helper Methods pair
leaving together in hunk 4. No region is left half-open on either side of the split, which would be a
compile error rather than a silent defect but is verified explicitly because the split was performed by
moving line ranges.

### Property 5 — the two catch blocks, the single error-log call and its message string are unchanged

Verdict5: PASS

The enqueue path's exception handling lives in `QuickFiler/Controllers/QfcQueue.Enqueue.cs`. Its two
catch blocks, its single `logger.Error` call, that call's interpolated message string and the `finally`
that decrements the running-jobs counter were compared line by line against the anchor. The lines shifted
by two positions, because hunk 2 of that file removed three lines and inserted one; after applying that
offset every line compares byte-identical with a case-sensitive comparison:

| Anchor line | Current line | Identical | Text |
|---|---|---|---|
| 118 | 116 | yes | `catch (OperationCanceledException)` |
| 119 | 117 | yes | `{` |
| 120 | 118 | yes | `//logger.Debug($"{nameof(EnqueueAsync)} was canceled by the user");` |
| 121 | 119 | yes | `}` |
| 122 | 120 | yes | `catch (System.Exception e)` |
| 123 | 121 | yes | `{` |
| 124 | 122 | yes | `logger.Error(` |
| 125 | 123 | yes | the interpolated failure message naming the member, the exception message and the stack trace |
| 126 | 124 | yes | `);` |
| 127 | 125 | yes | `}` |
| 128 | 126 | yes | `finally` |
| 129 | 127 | yes | `{` |
| 130 | 128 | yes | `Interlocked.Decrement(ref _jobsRunning);` |

The base part's `TryDequeueAsync` carries four `catch` clauses and one `logger.Error` call; none appears
in any hunk of that file's anchored diff, so all are unchanged as well.

### Property 6 — no public member of the queue class was added, removed, retyped or resigned

Verdict6: PASS

The public and protected declarations of the queue class were enumerated from the anchor's two partial
parts and from the current four, normalised for whitespace and compared as sets.

AnchorDeclarationCount: 18
CurrentDeclarationCount: 23
RemovedFromAnchor: 0

Every one of the anchor's 16 member declarations plus its two partial-class headers is present unchanged:
`Dequeue`, `ChangeIterationSize`, `CompleteAddingAsync`, `EnqueueAsync`, `JobsToFinish`, `RemoveItem`,
`TryDequeueAsync`, the `CollectionChanged` and `PropertyChanged` events, `Count`, `JobsRunning`,
`TlpTemplate`, `TlpStates`, `GrowEntry`, `RenumberGroups` and the protected `NotifyPropertyChanged`.

The five additional declarations are not members of the queue class:

- Two are the `public partial class QfcQueue` headers of the two new partial parts. They declare the same
  type a third and fourth time, which is what a partial split requires, and add no member.
- Three are the `InvokeIdleAsync` members of `UiThreadIdleDispatcher`. That type is declared
  `internal sealed`, so its members are not part of any publicly visible surface, and it is a new type
  rather than a member of the queue class.

All six seams are declared `internal`, not public. S1 is internal because the move-monitor interface is
itself internal and a public member of an internal type is an inconsistent-accessibility error; the other
five follow the same convention.

### Property 7 — no occurrence of the dotted framework-default dispatcher priority token was introduced

Verdict7: PASS

A search for the whole dotted token `DispatcherPriority.Normal` across all seven Write Set code paths
returns zero matches.

DispatcherPriorityNormalOccurrences: 0

The token is searched as the whole dotted form rather than as the bare word, because the bare word occurs
in ordinary prose and in unrelated identifiers and a bare-word search would report matches that are not
priority arguments. The reason this property is gated at all is that the existing dispatcher abstraction
in the utilities assembly forwards two of the three shapes at the framework default; adopting it instead
of the new narrow interface would have silently promoted two call sites from context-idle priority and
changed when background page construction runs. All four `ContextIdle` occurrences in
`QuickFiler/Controllers/QfcQueue.UiIdle.cs` are accounted for by P2-T5: the three executable priority
arguments at lines 81, 89 and 102, and the commented-out alternative at line 105.

### Property 8 — the marshalling wrapper relocated with its wrapper unchanged, and two call sites are byte-identical

Verdict8: PASS

**The wrapper at anchor line 275 of `QuickFiler/Controllers/QfcQueue.cs`.** It now sits at line 149 of
`QuickFiler/Controllers/QfcQueue.Tlp.cs`:

```
anchor  QfcQueue.cs:275       await UiIdleCallAsync(() => AddViewerToTlp(tlp, viewer, indexNumber));
now     QfcQueue.Tlp.cs:149   await UiIdleCallAsync(() => ViewerRowPlacer(tlp, viewer, indexNumber));
```

The `UiIdleCallAsync` wrapper, the lambda form and the three arguments are unchanged; the only difference
is the callee, substituted from the method group to seam S4. Its immediate neighbours confirm the
relocation is otherwise verbatim: anchor line 274 `grp.ItemViewer = viewer;` is now line 148 byte-identical,
anchor line 276 `return grp;` is now line 150 byte-identical, and anchor line 273
`var viewer = ItemViewerQueue.Dequeue(_token);` is now line 147 with only the S3 callee substituted.

**Call site at anchor line 197 of `QuickFiler/Controllers/QfcQueue.cs`.** Now at line 215 of the same
file, shifted by the hunk-2 insertion and the hunk-3 deletion. A case-sensitive comparison of the two
strings returns identical:

```
anchor  QfcQueue.cs:197                    await UiIdleCallAsync(() =>
now     QfcQueue.cs:215                    await UiIdleCallAsync(() =>
```

**Call site at anchor line 105 of `QuickFiler/Controllers/QfcQueue.Enqueue.cs`.** Now at line 103, shifted
by two positions by hunk 2 of that file. A case-sensitive comparison returns identical:

```
anchor  QfcQueue.Enqueue.cs:105            var itemGroups = await UiIdleAsyncCallAsync(async () =>
now     QfcQueue.Enqueue.cs:103            var itemGroups = await UiIdleAsyncCallAsync(async () =>
```

Both call sites still call the queue's own marshalling members by their original names. Seam S2 is
installed inside those members, which now forward to `UiIdleDispatcher`, so no call site needed to change
and none did. That is the property that keeps the seam invisible to callers.

---

## Overall verdict

P6T5Verdict: PASS on all eight properties.

Output Summary: All eight no-behaviour-change properties PASS. The four production files differ from the
anchor by exactly four hunks in the base part, three hunks in the enqueue part and two whole-file
insertions. Every one of the 24 genuinely-new executable lines is a seam declaration, accessor,
substitution or forward. No nullable pragma is in any relocated file; the CS0618 disable and restore pair
is intact at one each; region and endregion counts are equal in all four files; the enqueue part's two
catch blocks, its error-log call and message string and its finally block are byte-identical to the anchor
after the two-line offset; all 16 anchor public and protected member declarations of the queue class are
unchanged with nothing added, removed, retyped or resigned; `DispatcherPriority.Normal` occurs zero times
across the Write Set; and the relocated marshalling wrapper kept its wrapper with only its inner argument
substituted while both cited call sites compare byte-identical. Two byte-level deviations from strict
verbatim are recorded: a formatter re-wrap of one statement, captured before and after by P3-T5 and
P3-T8, and the loss of the UTF-8 byte-order mark from `QuickFiler/Controllers/QfcQueue.cs`, which
CSharpier dropped and which changes no behaviour. Acceptance met.
