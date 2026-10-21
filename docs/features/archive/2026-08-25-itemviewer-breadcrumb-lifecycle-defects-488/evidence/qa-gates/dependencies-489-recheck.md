# `## Dependencies on 489` — Re-Check Against the Current Tree ([P7-T6])

Timestamp: 2026-08-28T06-16

Command: targeted `grep`/`sed` reads of `QuickFiler/Viewers/ItemViewer.cs` and
`ItemViewer.Designer.cs`; fixed-string searches of every `ItemViewer` partial other than
`ItemViewer.Breadcrumb.cs`; a search of `QuickFiler.Test/QuickFiler.Test.csproj`; and
`git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/IItemViewer.cs`.
EXIT_CODE: 0

## Is 489's spec present on this branch?

**Yes.** `docs/features/active/itemviewer-surface-defects-489/spec.md` exists on this branch, alongside
that feature's `issue.md`, plan, research, two code reviews, two feature audits, two policy audits, and
its remediation inputs and plan. Feature 489 is already merged into this branch's base
`12465043e052fce66a1861bf1ddd037a1aa81afc`, which is the tip of
`epic/quickfiler-bug-family-integration`.

The checks below were nevertheless made against **current source**, not against 489's spec text. That is
the stronger check: 489 is merged, so the tree is the delivered state of its contract rather than a
prediction of it, and a claim verified against source cannot drift from what the compiler sees.

## Row per dependency

| ID | Claim | Status |
| --- | --- | --- |
| D489-1 | `UiSyncContext` still exists on `ItemViewer.cs` and returns the constructor-captured context | **CONFIRMED** |
| D489-2 | The type-level coverage exclusion is still present at `ItemViewer.cs:20` | **CONFIRMED** |
| D489-3 | The designer disposal shape is unchanged | **CONFIRMED** |
| D489-4 | No member-name collision across `ItemViewer` partials | **CONFIRMED — zero matches for both names** |
| D489-5 | No other `.csproj` entry names the same file name as the new test file | **CONFIRMED** |
| D489-6 | `IItemViewer.cs` is unchanged | **CONFIRMED** |

No dependency requires a named adjustment.

### D489-1 — `UiSyncContext`

`QuickFiler/Viewers/ItemViewer.cs:59` declares `public SynchronizationContext UiSyncContext`, backed by
the private `_context` field assigned in the constructor as `_context = SynchronizationContext.Current;`.
It is the survivor of 489's surface work — `UiScheduler` was removed by that feature — and it is exactly
what D4's affinity guard compares against, so the guard needs no re-pointing.

**One correction is recorded here rather than passed over.** Constraint C6 argues that every
successfully constructed `ItemViewer` has a non-null `UiSyncContext` because the constructor calls
`TaskScheduler.FromCurrentSynchronizationContext()`, which throws under a null ambient context. The
delivered constructor contains no such call; it calls `Dispatcher.CurrentDispatcher`, which does not
throw on a null ambient context. A viewer constructed with no ambient context is therefore constructible
and has a null `UiSyncContext`. This changes no delivered design — the guard's null escape already
handles exactly that case — but a reviewer must not rely on C6's stated reachability argument.
`[P4-T2]` records the correction in full.

### D489-2 — the coverage exclusion

`QuickFiler/Viewers/ItemViewer.cs:20` reads `[ExcludeFromCodeCoverage]`, immediately above the
`public partial class ItemViewer` declaration. The citation resolves at the exact line the spec gives,
with no drift. Because a type-level attribute on one part applies to the whole partial type, every
member of `ItemViewer.Breadcrumb.cs` remains excluded from coverage measurement, which `[P0-T15]`
confirmed empirically: that file matches zero `class` elements in the baseline Cobertura document.

### D489-3 — the designer disposal shape

```csharp
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
```

Unchanged: `components` is disposed only when `disposing` is true and `components` is non-null. D5's
premise holds — a `Container` created after teardown has begun would never be disposed by this method,
which is why D5 refuses the creation instead of trying to dispose it late. `[P5-T5]` separately confirms
the file is byte-identical to `BASE_SHA`.

### D489-4 — the member-name collision check, made explicitly

This is the check the spec singles out, because a member-name collision across `ItemViewer` partials is
a **compile error at integration** rather than a merge conflict, and will not surface at fan-in.

Every `ItemViewer` partial other than `ItemViewer.Breadcrumb.cs` was searched: `ItemViewer.cs`,
`ItemViewer.Designer.cs`, `ItemViewer.WebViewThread.cs`, `ItemViewer.FolderSearch.cs`,
`ItemViewer.DisplayState.cs`, and `ItemViewer.Commands.cs`.

| Fixed string | Files matching |
| --- | --- |
| `ThrowIfOffUiBoundary` | **0** |
| `_breadcrumbProvider` | **0** |

**Both searches return zero matches.** Neither name this feature introduces collides with a member
declared by any other partial.

### D489-5 — the new `.csproj` entry's file name

`QuickFiler.Test/QuickFiler.Test.csproj` contains exactly **1** line naming
`ItemViewerBreadcrumbLifecycleRegressionTests.cs` — the entry `[P1-T2]` added. No pre-existing entry
names that file name, so there is no duplicate `Compile Include` and no CS2002.

### D489-6 — `IItemViewer.cs`

`git diff --name-only <BASE_SHA> -- QuickFiler/Viewers/IItemViewer.cs` produces **no output lines**; the
file is byte-identical. The dependency is additionally no-impact by construction: this feature calls
`SetFolderItems` nowhere, and a search of the whole `QuickFiler`/`QuickFiler.Test` diff for that
identifier returns **0** matches.

Output Summary: All six dependencies **D489-1 through D489-6 are CONFIRMED** against current source;
none requires a named adjustment. **D489-4 was checked explicitly and both fixed-string searches —
`ThrowIfOffUiBoundary` and `_breadcrumbProvider` — return zero matches across every other `ItemViewer`
partial.** `docs/features/active/itemviewer-surface-defects-489/spec.md` **is present** on this branch,
and the checks were nevertheless made against current source because 489 is already merged into this
base. One correction to constraint C6's reachability argument is recorded under D489-1.
