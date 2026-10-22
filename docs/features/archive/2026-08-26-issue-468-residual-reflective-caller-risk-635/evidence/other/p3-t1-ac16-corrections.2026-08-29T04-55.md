# Corrections to the AC-16 Record (P3-T1) — discharges AC-10

- **Issue:** #635
- **Plan task:** [P3-T1]

Timestamp: 2026-08-29T06-36

## Output Summary

Two corrections to the issue #468 AC-16 record are stated below. The first is an omission: the AC-16
build-input file-type search covered twelve identifiers and did not include the thirteenth, the private
field `_templateTlp`. The second is a superseded fact: AC-16's recorded claim of zero occurrences of any
removed identifier anywhere in the QuickFiler test tree no longer holds, and the superseding occurrence
is identified by file, line and category. This task runs no command; both corrections are derived from
evidence recorded elsewhere in this item and are cited to it.

AC16_CORRECTIONS: 2

## Correction 1 — the omitted thirteenth identifier

The AC-16 build-input file-type search covered twelve identifiers and omitted the thirteenth, the
private field `_templateTlp`.

**Evidence for the removal being twelve methods plus one field.** [P0-T4] derives the search set at
commit level from `63eebd47`. Its thirteen-row table quotes, from the diff of
QuickFiler/Controllers/QfcCollectionController.cs in that commit, a removed declaration line for each
identifier. Twelve of those rows are method declarations. The thirteenth row quotes the removed line
`-        private TableLayoutPanel _templateTlp;`, which is a field declaration, not a method. The
commit subject, `fix(468): remove unreachable load paths and the dead _templateTlp field`, names the
field explicitly. The evidence artifact is
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t4-identifier-derivation.2026-08-29T04-55.md`.

**Why the omitted identifier is the one for which the search mattered most.** Field reflection is the
only name-based mechanism that demonstrably exists anywhere near the affected type. [P2-T3] enumerates
eight reflection call sites whose receiver is `typeof(QfcCollectionController)` and whose member-name
argument is a variable; seven of the eight are `GetField(` sites, and three further sites pass a named
constant to `GetField(`. [P2-T1] measures the `GetField(` pattern at 172 occurrences in the QuickFiler
test tree, the largest count of any pattern in the seventeen-pattern inventory, against zero in the
production tree. A removed *field* is exactly the kind of member such a call site resolves. The
identifier the AC-16 build-input search omitted is therefore the identifier whose mechanism of risk was
real, while the twelve it did include name methods that no measured call site resolves by name.

This correction does not reverse the AC-16 disposition. The widened search performed by this item
covers all thirteen identifiers, and [P1-T1] returns zero over a measured 683-file scope while [P1-T4]
places every one of the 31 tracked-`.cs` hits in a category other than "genuine name-based caller". The
correction records that the earlier search's coverage was narrower than its conclusion implied, not
that its conclusion was wrong.

## Correction 2 — the superseded zero-hits-in-the-test-tree claim

The AC-16 claim of zero occurrences of any removed identifier anywhere in the QuickFiler test tree no
longer holds.

**The superseding occurrence, taken from the category C row of the [P1-T4] enumeration:**

| Field | Value |
|---|---|
| File | QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs |
| Line | 60 |
| Matched identifier | `WireUpKeyboardHandler` |
| Category | C — a hit whose line's first non-whitespace token is `//` or `///` |

The printed line, reproduced verbatim from the [P1-T4] output:

```
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:60:        /// Issue #444 decision pin. Upstream #468 deleted the dead <c>WireUpKeyboardHandler</c>
```

The occurrence is a triple-slash documentation comment naming `WireUpKeyboardHandler`, which is not a
string literal, is not emitted as a member name into assembly metadata, and cannot be passed to any
reflection API.

The occurrence is also the only one of its kind. [P1-T4] records that this is the sole occurrence of
any of the thirteen identifiers anywhere in the QuickFiler test tree, and that no string literal
anywhere in that tree equals one of the thirteen. That statement is what the [P2-T3] closure argument
consumes, so the superseding occurrence does not weaken the closure: a documentation comment cannot
supply a value to a member-name variable.

The evidence artifact is
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t4-partition-c-enumeration.2026-08-29T04-55.md`.

## The AC-16 artifact is not edited

The AC-16 artifact in the issue #468 feature folder is a time-stamped historical record and is not
edited by this item; these corrections are recorded here instead.

Its path is
docs/features/active/qfc-collection-controller-defects-468/evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md.
That file is outside this item's write set, which is confined to
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/`. [P4-T2] proves that
confinement over the branch diff and the working-tree status together.

The specification's non-goals section states the same rule: changing the AC-16 artifact is out of
scope, because a time-stamped record of what was measured on a given date remains accurate as a record
of that measurement even after the tree moves. Rewriting it would destroy the audit trail that makes
correction 2 legible as a change over time rather than as an error at the time.
