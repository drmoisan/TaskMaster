# D5 — `ItemViewer.Designer.cs` Is Untouched ([P5-T5])

Timestamp: 2026-08-28T05-52

Command:

```
git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/ItemViewer.Designer.cs
```

EXIT_CODE: 0

## Result

**Output: no lines.**

`QuickFiler/Viewers/ItemViewer.Designer.cs` is **byte-identical** to its pre-change state at `BASE_SHA`
`12465043e052fce66a1861bf1ddd037a1aa81afc`. This is what establishes the criterion `[P5-T10]` flips;
a line count alone would not, since a file can be edited without changing its line count.

The file is also a **forbidden** file under constraint C1, owned by sibling feature
`itemviewer-surface-defects-489`, so the empty diff additionally confirms no ownership violation.

## Line count — measured 6223, with a recorded discrepancy

| Measurement | Value |
| --- | --- |
| `wc -l` | **6223** |
| `grep -c ''` | **6223** |
| Final byte of the file | `0a` (the file ends with a newline) |
| Value cited by the plan and `spec.md` | 6224 |

The two independent counters agree at **6223**, and the file terminates with a newline, so there is no
trailing partial line for the two methods to disagree about. The measured value is recorded here rather
than the cited one, because recording 6224 would be an unverified claim.

**The one-line difference is a counting-convention or citation drift, not a file difference.** The
`git diff` above proves the file is byte-identical to `BASE_SHA`, so this feature did not remove a line
from it. The cited figure was taken at the pre-change citation anchor `0a6aaa31`, and the plan's
standing rule is that every citation is resolved by name rather than by number. The substantive
property the criterion depends on — that the file is unmodified — is established by the empty diff and
is unaffected by which of 6223 or 6224 is the right convention.

## Why D5's design avoids this file

The alternative to refusing creation during teardown was to **dispose a `Container` created during
teardown**. That would have required either editing this designer-generated file — 6223 lines, already
more than twelve times the repository's 500-line ceiling, and inside sibling feature 489's surface — or
adding a second disposal path with its own re-entrancy problem.

D5 instead places the guard in `EnsureBreadcrumbResourceOwnership`, a member of the owned
`ItemViewer.Breadcrumb.cs`, where it costs four lines and touches no designer-generated code.
`Control.IsDisposed` and `Control.Disposing` are both public WinForms properties, so no new state was
needed either; `Disposing` covers the window *during* `Dispose(bool)`, which `IsDisposed` alone does
not.

Output Summary: `git diff --name-only <BASE_SHA> -- QuickFiler/Viewers/ItemViewer.Designer.cs` produces
**no output lines**, establishing byte-identity. The file measures **6223** lines by both `wc -l` and
`grep -c ''` and ends with a newline; the plan's cited 6224 is recorded as a one-line citation
discrepancy that the empty diff shows is not a content difference. D5's design avoids this file
deliberately, using the public `IsDisposed`/`Disposing` properties in an owned file instead.
