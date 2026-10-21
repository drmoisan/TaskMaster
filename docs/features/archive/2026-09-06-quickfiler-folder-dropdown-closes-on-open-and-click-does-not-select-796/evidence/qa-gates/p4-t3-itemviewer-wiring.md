# P4-T3 — AC2 item-viewer wiring

Timestamp: 2026-09-07T14-10
Task: [P4-T3]
Issue: #796
Channel used: A

## Branch taken

AC2-ITEMVIEWER-WIRING: REQUIRED

Quoted from evidence/other/close-ordering-decision.md:

> AC2-ITEMVIEWER-WIRING: REQUIRED

and from the derivation recorded beneath that line:

> The value must instead be supplied by the code that knows the popup is being opened, which is
> the wiring in `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` that already assigns
> `host.MayTakeFocus = MayRestoreBreadcrumbFocus;` at line 212. A popup-owns-activation
> assignment beside that existing assignment is therefore required rather than optional.

The REQUIRED branch is taken. The assignment was added immediately beneath the existing
`host.MayTakeFocus = MayRestoreBreadcrumbFocus;` assignment, as one statement plus a three-line
reason comment. No constructor arity changed; the new state is reported to the owning form through
a settable-style registration method, matching the `MayTakeFocus` precedent of assigning after
construction.

## Line-count gate

Command:

```
pwsh -NoProfile -Command '(Get-Content -LiteralPath QuickFiler\Viewers\ItemViewer.Breadcrumb.cs).Count'
```

EXIT_CODE: 0

LINE-COUNT-IDIOM: (Get-Content -LiteralPath $_).Count

| Path | Baseline | Measured after P4-T3 | Ceiling | Verdict |
|---|---|---|---|---|
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 456 | 460 | 460 | at ceiling, within gate |

The count was re-measured after `dotnet tool run csharpier format QuickFiler QuickFiler.Test`
rewrote nothing in this file, so the recorded value is the post-format value and not a value the
formatter can still move.

Output Summary: REQUIRED branch taken; the popup-owns-activation registration is in place beside
the may-take-focus assignment; the file measures 460 physical lines against the 460 ceiling.
