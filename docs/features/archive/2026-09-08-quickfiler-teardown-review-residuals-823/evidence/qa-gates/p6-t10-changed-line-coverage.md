# Phase 6 — Changed-line coverage and the R4 token reconciliation

Timestamp: 2026-09-09T14-52

Task: [P6-T10]

Command: `git diff --unified=0 d636b0f28f548181685260d929de6d7d2940d1da -- UtilitiesCS QuickFiler`
Command: cross-reference of each added line number against the `line` elements of
`coverage/823-post.cobertura.xml` for the same file

The 40-character SHA is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md` per D2. The two-dot form is required here in place of
the three-dot form used elsewhere in this plan, because [P6-T1] may rewrite files that [P2-T15],
[P3-T10] and [P4-T4] had already committed and those rewrites would still be uncommitted at this
point, so a three-dot diff would report pre-format line numbers against a coverage document
generated from the post-format tree. The two-dot form still carries an explicit ref operand, so
D2's prohibition on an unanchored diff is satisfied, and it is not a name-listing diff, so no
porcelain companion is required.

The Cobertura filenames are absolute Windows paths; the cross-reference matched on the trailing
file-name fragment rather than on the whole path, so no absolute host path is reproduced here (D14).

## CHANGED-LINES per changed production file

### CHANGED-LINES: QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs

Executable added lines and their hit counts:

- 48 — `if (itemViewer == null)` — hits 1
- 50 — `throw new ArgumentNullException(nameof(itemViewer));` — hits 1
- 53 — `if (popupIsOpen == null)` — hits 1
- 54 — `{` — hits 1
- 55 — `throw new ArgumentNullException(nameof(popupIsOpen));` — hits 1

Lines 51 and 52 are an added closing brace and an added blank line; the compiler emits no sequence
point for either, so the Cobertura document carries no `line` element for them and they are not
measurable. The added lines at `:31-36` and `:39` of the same file are XML-doc lines and are
likewise not measurable.

### CHANGED-LINES: UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs

- 56 — `&& !_userEmailRetryAttemptedStores.Contains(Current)` — hits 1
- 59 — `_userEmailRetryAttemptedStores.Add(Current);` — hits 1

The added lines at `:40-50` are comment lines and are not measurable.

### CHANGED-LINES: UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs

- 113 — `private readonly HashSet<StoreWrapper> _userEmailRetryAttemptedStores =` — hits 1
- 114 — `new HashSet<StoreWrapper>();` — hits 1

The field initialiser is emitted into the instance constructor, which is why both lines carry
sequence points and both are hit. The added lines at `:97-112` are XML-doc lines and are not
measurable.

### CHANGED-LINES: QuickFiler/Viewers/QfcFormViewer.cs

All added lines are XML-doc lines at `:220-226`. NOT MEASURABLE.

### CHANGED-LINES: QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs

One added line, the corrected comment at `:11`. NOT MEASURABLE.

### CHANGED-LINES: UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs

Three added lines, all comment lines at `:198-200`. NOT MEASURABLE.

## Dispositions

CHANGED-LINE-COVERAGE: 100

Nine measurable added lines, all with a `hits` attribute greater than zero: 9 of 9 is 100 percent.

UNCOVERED-CHANGED-LINES: NONE

UNMEASURABLE-FILES:
- `QuickFiler/Viewers/QfcFormViewer.cs` — carries a class-level `[ExcludeFromCodeCoverage]` and
  therefore emits no Cobertura class element at all; the cross-reference found zero class elements
  matching that filename. Its changed lines are additionally all XML-doc. Recorded as
  `NOT MEASURABLE` with that reason rather than as zero.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` — comment-only change, which emits no `line`
  element.
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` — comment-only change, which emits no `line`
  element.

NO-REGRESSION-ON-CHANGED-LINES: PASS

Decided by the mechanical rule and by no other judgement: the measurable subset is non-empty, with
nine lines, and `CHANGED-LINE-COVERAGE` is 100, which is at least 90. The 90 figure is the new-code
floor `CLAUDE.md` section UT2 states, applied here to the changed-line set.

This plan adds no new module, so no new-module figure is computed.

## R4 token reconciliation on the post-format tree

POSTFORMAT-DROPDOWNHOST-LINES: 459

`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` was re-measured with the Read tool after the final
[P6-T1] formatter pass and still measures 459 lines, with line 459 the file's closing brace. This
matches the [P4-T1] delivery measurement and the token now present in
`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`, which reads `(459 lines)`. The count did not
change under the formatter pass, so no token correction and no loop restart was required.

R4-TOKEN-RECONCILED: YES

Output Summary: Nine measurable changed production lines, all covered, giving 100 percent
changed-line coverage and `NO-REGRESSION-ON-CHANGED-LINES: PASS`. Three files are recorded
`NOT MEASURABLE` with their reasons. The R4 token reconciles against a post-format re-measurement
of 459 lines.
