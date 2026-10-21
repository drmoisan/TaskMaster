# P9-T8 — File-size audit of the write set after the final formatter pass

Timestamp: 2026-09-07T16-07
Task: [P9-T8]
Issue: #796
Channel used: A

This audit runs after P9-T1, because the formatter can change line counts and an audit
taken before it would measure a superseded state. P9-T1 and P9-T2 are both checked off,
and P9-T2 recorded a clean `csharpier check` over the same scope, so the tree measured
here is the post-format tree.

LINE-COUNT-IDIOM: (Get-Content -LiteralPath $_).Count

That is the idiom recorded on the `LINE-COUNT-IDIOM:` line of
evidence/baseline/p0-t12-file-size-baseline.md, and no other idiom was used, so this
audit and the baseline are commensurable. `(Get-Content $_ | Measure-Object -Line).Lines`
remains prohibited, because `Measure-Object -Line` omits blank lines and under-reports
every count by that file's blank-line total.

Command, the P0-T12 command form extended with the three files this plan creates and
with the seventeenth write-set path:

```
pwsh -NoProfile -Command '@("QuickFiler\Controllers\QfcFormController.Deactivate.cs","QuickFiler\Interfaces\IQfcFormViewer.cs","QuickFiler\Viewers\QfcFormViewer.cs","QuickFiler\Viewers\BreadcrumbDropDownHost.cs","QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs","QuickFiler\Viewers\ItemViewer.Breadcrumb.cs","QuickFiler\Controllers\QfcItemController.EventHandlers.cs","QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs","QuickFiler\Resources\FolderBreadcrumb.html","QuickFiler.Test\Controllers\QfcFormControllerDeactivateTests.cs","QuickFiler.Test\Viewers\BreadcrumbPendingOpenCloseTests.cs","QuickFiler\Viewers\BreadcrumbDropDownHost.Diagnostics.cs","QuickFiler.Test\Viewers\BreadcrumbDropDownCloseOrderingTests.cs","QuickFiler.Test\Controllers\QfcItemController.SearchLeaveLatchTests.cs","QuickFiler.Test\Controllers\QfcItemController.SearchDismissalTests.cs") | ForEach-Object { $_ + " " + (Get-Content -LiteralPath $_).Count }'
```

EXIT_CODE: 0

## Scope of the audit

Fifteen paths. The write set holds seventeen; the two omitted are
`QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj`, which this
task's scope — every .cs and .html path in the write set — does not cover.

## Measured physical line counts

| Path | P0-T12 baseline | Final | Delta | At most 500 |
|---|---|---|---|---|
| QuickFiler/Controllers/QfcFormController.Deactivate.cs | 73 | 150 | +77 | yes |
| QuickFiler/Interfaces/IQfcFormViewer.cs | 72 | 88 | +16 | yes |
| QuickFiler/Viewers/QfcFormViewer.cs | 293 | 332 | +39 | yes |
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 498 | 496 | -2 | yes |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | 107 | 131 | +24 | yes |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 456 | 460 | +4 | yes |
| QuickFiler/Controllers/QfcItemController.EventHandlers.cs | 263 | 317 | +54 | yes |
| QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs | 395 | 395 | 0 | yes |
| QuickFiler/Resources/FolderBreadcrumb.html | 490 | 490 | 0 | yes |
| QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs | 248 | 305 | +57 | yes |
| QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs | 380 | 458 | +78 | yes |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs | 0 — did not exist at P0-T12; created by this plan | 79 | +79 | yes |
| QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs | 0 — did not exist at P0-T12; created by this plan | 290 | +290 | yes |
| QuickFiler.Test/Controllers/QfcItemController.SearchLeaveLatchTests.cs | 0 — did not exist at P0-T12; created by this plan | 102 | +102 | yes |
| QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs | NO PHASE 0 BASELINE | 181 | not computable | yes |

Every one of the fifteen recorded physical counts is at most 500. The largest is 496,
at `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, four lines below the ceiling.

## The stated exception

`QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs` records the
literal `NO PHASE 0 BASELINE` in its baseline column rather than a figure. It entered the
write set after Phase 0 had executed, when executing AC4 surfaced a pre-existing test in
it asserting the behaviour AC4 deliberately changes, so it carries no P0-T12 figure and
recording one would be recording a number with no source. Its delta is correspondingly
not computable.

That exception is distinct from the three created files above it in the table. Those
three also carry no row in the P0-T12 artifact, but their baseline is well defined and
has a source: they did not exist in the tree at P0-T12, so their baseline length is 0 and
their delta is their whole length. `NO PHASE 0 BASELINE` is not used for them, because it
would understate what is known about them.

## Two counts worth noting

`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` FELL by two lines, from 498 to 496,
against a plan that adds to it. That is the expected consequence of executed task P1-T2
relocating `OnDropDownClosed` into the new diagnostics part: the relocation removed more
lines than the later AC3 edit added back. The headroom that relocation bought is what
kept the file under the ceiling.

`QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs` grew by 78 lines to 458,
which is the largest growth in the test files and leaves 42 lines of headroom.

Output Summary: 15 paths measured with the recorded idiom
`(Get-Content -LiteralPath $_).Count`. Every count is at most 500, the maximum being 496.
Eleven paths carry their P0-T12 baseline figure beside the final count, three carry a
baseline of 0 with the reason that they did not exist at P0-T12, and one carries the
literal `NO PHASE 0 BASELINE` under the exception this task states.
