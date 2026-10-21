# [P10-T11] Coverage-exemption audit

Timestamp: 2026-08-28T02-09
Task: [P10-T11]
Command: `grep -c 'ExcludeFromCodeCoverage' <path>` over the four audited files, plus
`git diff <BASE> -- . ":(exclude).claude/agent-memory"` filtered to added lines carrying the attribute
EXIT_CODE: 0

## Per-file occurrence counts against `BASELINE_EXEMPTIONS`

| File | `BASELINE_EXEMPTIONS` (`[P0-T15]`) | Delivered | Equal? |
|---|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | 0 | **0** | **yes** |
| `QuickFiler/Controllers/EfcItemController.cs` | 1 | **1** | **yes** |
| `QuickFiler/Viewers/EfcViewer.cs` | 1 | **1** | **yes** |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 2 | **2** | **yes** |

All four counts equal their baseline figures exactly. The surviving occurrences are the same
pre-existing ones `[P0-T15]` located:

| File | Line | Form |
|---|---|---|
| `QuickFiler/Controllers/EfcItemController.cs` | `:25` | `[ExcludeFromCodeCoverage]`, class level |
| `QuickFiler/Viewers/EfcViewer.cs` | `:20` | `[ExcludeFromCodeCoverage]`, class level |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | `:47` | `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | `:137` | `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` |

## No added line in the diff carries the attribute

### The measurement that answers the criterion

`git diff 38f097898639b054428188c9c5e266e54972c259 -- '*.cs' ":(exclude).claude/agent-memory"`, filtered
to lines beginning `+` (excluding the `+++` header) and containing `ExcludeFromCodeCoverage`:

```
0
```

Restricted further to just the four owned production files, the same filter also returns **0**.

**No `.cs` file in this feature's diff adds an `[ExcludeFromCodeCoverage]` attribute.**

### A distinction a reviewer must not mis-read

The same filter run **without** the `*.cs` pathspec — that is, over the whole diff including markdown —
returns **12** hits. Every one of the 12 is prose inside a documentation file, not an attribute in
source:

| File | Hits | Nature |
|---|---|---|
| `docs/.../evidence/baseline/file-sizes-and-exemptions.md` | 7 | the `[P0-T15]` `BASELINE_EXEMPTIONS` record and its locator table |
| `docs/.../plan.2026-08-25T07-01.md` | 3 | the plan text of `[P10-T8]` and `[P10-T11]` |
| `docs/.../evidence/other/463-viewersetup-review.md` | 1 | prose describing the two pre-existing `ViewerSetup.cs` attributes |
| `docs/.../evidence/qa-gates/efcviewer3-deletion.md` | 1 | prose describing the deleted file's attribute |

A raw `git diff | grep` over the whole diff therefore appears to show attribute additions where there
are none. The `.cs`-scoped count is the one that answers the criterion, and it is **0**. This
distinction is recorded explicitly so a later reviewer does not read a documentation mention as a source
change.

### The as-written base, for completeness

`git diff 002335989830ba9f3ad802858ef0b794f6281750 -- '*.cs'` reports **17** added attribute lines. All
17 belong to merged siblings carried in by the integration merge `25924673`, none to this feature:

| File | Hits | Owner |
|---|---|---|
| `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` | 6 | #476 |
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` | 5 | #476 |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | 4 | #476 |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` | 2 | #476 |

`[P9-T2]` and `[P9-T3]` independently show that none of those four paths is in this feature's diff.

## Consequence for coverage measurement

Because both class-level exemptions predate this feature and both survive unchanged,
`QuickFiler/Controllers/EfcItemController.cs` and `QuickFiler/Viewers/EfcViewer.cs` remain excluded from
coverage measurement in their entirety — the `[P10-T7]` Cobertura contains zero `<class>` elements for
either file. `[P10-T8]` records that consequence and asserts no threshold over the members inside them.

Constraint C5 is satisfied: no new `[ExcludeFromCodeCoverage]` attribute was introduced anywhere in this
feature's diff, and no existing one was removed.

Output Summary: PASS. Per-file `ExcludeFromCodeCoverage` counts are 0 / 1 / 1 / 2, equal to
`BASELINE_EXEMPTIONS` exactly. A `.cs`-scoped search of this feature's diff for added lines carrying the
attribute returns **0**. The 12 hits a whole-diff `grep` returns are all markdown prose in evidence
artifacts and the plan, not attributes in source; that distinction is recorded so it is not mis-read.
The 17 hits visible under the as-written `BASELINE_SHA` scope all belong to merged sibling #476.
