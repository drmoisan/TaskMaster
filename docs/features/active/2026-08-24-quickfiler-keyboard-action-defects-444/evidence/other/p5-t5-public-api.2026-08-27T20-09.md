# [P5-T5] Public-API gate

Timestamp: 2026-08-27T20-09
Command: `$mb = git merge-base HEAD origin/epic/quickfiler-bug-family-integration` then `git diff "$mb..HEAD" -- QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/QfcItemController.Navigation.cs`, reviewed for member declarations in added and deleted lines
EXIT_CODE: 0
Output Summary: zero added, zero removed, and zero re-signed `public` members across all three
production files. Exactly one member is added anywhere in the diff and it is `private`. One `private`
field is also added.

Merge base `4f238289090e4c97ca505511a5a73e8092dce0f9`, re-derived per `[P5-T1]`.

## Added, removed, and re-signed members, classified by accessibility

| File | Member | Change | Accessibility | Is it `public`? |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | `void SyncExpandedRegistrations(bool expanded)` | **added** | `private` | no |
| `QuickFiler/Controllers/QfcCollectionController.cs` | `int _registeredDigits` (field) | **added** | `private` | no |
| — | — | removed | — | **nothing removed in any of the three files** |
| — | — | re-signed | — | **nothing re-signed in any of the three files** |

### Per-file totals

| File | added `public` | removed `public` | re-signed `public` | added non-`public` |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/KbdActions.cs` | 0 | 0 | 0 | 0 |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 0 | 0 | 0 | 1 (`private` field `_registeredDigits`) |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | 0 | 0 | 0 | 1 (`private` method `SyncExpandedRegistrations`) |
| **total** | **0** | **0** | **0** | 2 |

## Evidence per file

**`QuickFiler/Controllers/KbdActions.cs`.** The diff contains additions only, with zero deletion
lines. Every added line is an XML documentation comment, an explanatory `//` comment, or a statement
inside the pre-existing enumerable constructor's body. No added line declares a member. The
constructor's own declaration line is not in the diff, so its accessibility and signature are
untouched — it is a behavioural change to an existing `public` member, which is not an addition, a
removal, or a re-signing.

**`QuickFiler/Controllers/QfcCollectionController.cs`.** One member is added:
`private int _registeredDigits;`, a `private` instance field. The remaining additions are one
assignment inside `RegisterNavigation`, one local variable declaration
(`var format = _registeredDigits == 2 ? "00" : "";`) inside `UnregisterNavigation`, and comments. The
eight deleted lines are an `if`/`else` block and its two `Remove` call statements inside
`UnregisterNavigation`; none is a member declaration. Neither `RegisterNavigation` nor
`UnregisterNavigation` has its declaration line in the diff, so both keep their `public` accessibility
and their signatures. A local variable is not a member.

**`QuickFiler/Controllers/QfcItemController.Navigation.cs`.** One member is added:
`private void SyncExpandedRegistrations(bool expanded)`, a `private` instance method. The four
deleted lines are call statements inside two existing method bodies, so nothing is removed. The
declaration lines and attribute lines of `ToggleExpansion(Enums.ToggleState)` and
`ToggleExpansionAsync(Enums.ToggleState)` are not in the diff, so neither `public virtual` member is
re-signed; `[P5-T3]` records both at the branch head verbatim.

## Consequence

This feature contributes **no public-API change**. Nothing a caller outside these three types can
bind to was added, removed, or re-signed, so no in-repo caller and no sibling feature needs
updating on account of this diff. `QuickFiler/Interfaces/IQfcCollectionController.cs` is absent from
the branch diff (`[P5-T2]`), which is the corresponding interface-level statement.

The two added members are both `private`. `SyncExpandedRegistrations` is named in the spec's upstream
contract table despite being `private`, so that a sibling authoring another partial of
`QfcItemController` does not introduce a colliding member; its `private` accessibility means it is
not part of the sibling-visible surface.

## Acceptance

- The artifact records zero added, zero removed, and zero re-signed `public` members across all three
  files — met; the totals row reads 0, 0, 0.
- It records the one added member as `private` — met. Two members are added and both are `private`:
  the method `SyncExpandedRegistrations` named by the acceptance condition, and the `private` field
  `_registeredDigits` in `QfcCollectionController.cs`, which the spec's informational contract table
  for that file lists as ADDED and which is recorded here for completeness.
