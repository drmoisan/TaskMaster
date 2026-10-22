# P6-T20 — decision-D9 descent mechanism recorded in spec.md

Timestamp: 2026-08-26T10-38

Command:

```
pwsh -NoProfile -Command '$m = Select-String -LiteralPath "docs\features\active\breadcrumb-router-navigation-defects-498\spec.md" -SimpleMatch -Pattern "D9 DESCENT MECHANISM SELECTED: descend by child activation"; $m | ForEach-Object { "$($_.LineNumber): $($_.Line)" }; if ($m) { $code = 0 } else { $code = 1 }; "EXIT_CODE: $code"'
```

EXIT_CODE: 0

Output Summary:

- The fixed-string search returns one match: `567: **D9 DESCENT MECHANISM SELECTED: descend by child activation**`.
- Text added to the D9 section of `spec.md`, quoted verbatim:

  > **D9 DESCENT MECHANISM SELECTED: descend by child activation**
  >
  > Rationale. `ActivateSegment` refuses the leaf index (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:156`)
  > and therefore cannot express a downward transition, so a descent must be routed elsewhere. The child-activation
  > route adds no member to `BreadcrumbRow`: it reuses the landed `GetActiveChild(int)` together with the router's
  > `SelectHierarchyPath`, which is exactly how the landed mouse gesture descends via `ActivateChild`. Because a
  > Right key press carries no child index where the mouse gesture supplies one, the choice is fixed at child
  > index `0` (`row.GetActiveChild(0)`); when that returns null the descent is not available and the decision-D1
  > fall-through runs. The rejected alternative — a new owned transition on `BreadcrumbRow` — is the larger change
  > for no additional capability, and it would put pressure on `ActivateSegment`'s guard.
  >
  > Implementing task: `P6-T7` (`TryRightTreeTransitionAsync` in
  > `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`). Pinning test: `P6-T5`
  > (`HandleArrowKey_RightAfterExpansion_DescendsByChildActivation` in
  > `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs`), which asserts the descent targets child index
  > `0` and that a null result falls through instead.

Satisfies the AC-16 descent-mechanism recording clause.
