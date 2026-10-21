# Phase 6 — AC15 XML-doc read

Timestamp: 2026-09-09T15-02

Task: [P6-T28]

AC15 requires a read of both XML docs confirming that neither states a null argument is ignored or
tolerated and neither repeats the form-lookup rationale, in addition to the scoped token search.
Both were read with the Read tool on the post-format tree.

Command: `git grep -c -F "Ignored when null" -- QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs QuickFiler/Viewers/QfcFormViewer.cs`

EXIT_CODE: 1

The search printed nothing and exited 1, so neither file carries the token.

## QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs, `Register` XML doc at `:26-45`

The `<param name="itemViewer">` element at `:30-37` states that a null value is rejected with an
`ArgumentNullException`, and gives as the reason that the sole production call site consumes the
form lookup with the null-conditional operator, so a failed lookup produces a null receiver and
skips the invocation entirely rather than producing a null argument, and the two arguments it
passes are `this` and a lambda literal. The `<param name="popupIsOpen">` element at `:38-40` states
that a null value is rejected on the same reasoning.

Neither element states that a null argument is ignored or tolerated. Neither repeats the
form-lookup rationale: the doc now states the opposite of it, namely that the form lookup cannot
produce a null argument. The `<remarks>` block at `:41-45` is unchanged.

## QuickFiler/Viewers/QfcFormViewer.cs, `SetBreadcrumbPopupOwner` XML doc at `:216-231`

The `<param name="itemViewer">` element at `:220-223` states that the argument is forwarded
unchanged to the registry, which rejects a null with `ArgumentNullException`. The
`<param name="popupIsOpen">` element at `:224-226` states that the predicate is forwarded unchanged
on the same terms.

Neither element states that a null argument is ignored or tolerated, and neither repeats the
form-lookup rationale. The summary at `:216-219`, the `<remarks>` block at `:227-231` and the
expression body at `:232-233` are unchanged.

Output Summary: Both rewritten XML docs state rejection rather than tolerance and neither carries
the false form-lookup rationale. The scoped `Ignored when null` search exits 1 with no output.
AC15 met.
