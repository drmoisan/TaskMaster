# [P5-T8] Plan line-number-citation gate

Timestamp: 2026-08-27T20-12
Command: `@(Select-String -Pattern 'QfcCollectionController\.cs:[0-9]' -Path docs\features\active\quickfiler-keyboard-action-defects-444\plan.2026-08-24T20-33.md).Count` and `@(Select-String -Pattern 'QuickFiler\.Test\.csproj:[0-9]' -Path docs\features\active\quickfiler-keyboard-action-defects-444\plan.2026-08-24T20-33.md).Count`
EXIT_CODE: 0
Output Summary: `controller_line_citations=0`, `csproj_line_citations=0`. Both recorded counts are
exactly `0`, so the plan transcribes no line number into either line-number-constrained file.

Both patterns carry regular-expression metacharacters — the escaped `\.` before `cs` and `csproj` —
and therefore do not match their own occurrence in the command text quoted above, which spells the
escaped forms. The gate is not self-satisfying.

## Counts

| Pattern | Required | Observed |
| --- | --- | --- |
| `QfcCollectionController\.cs:[0-9]` | exactly `0` | **0** |
| `QuickFiler\.Test\.csproj:[0-9]` | exactly `0` | **0** |

## Every anchor this plan uses into `QuickFiler/Controllers/QfcCollectionController.cs`

Each is a **member name** (or, for the last row, an attribute name), re-derived at `[P0-T13]` by
name against the actual branch head rather than transcribed as a line number.

| # | Anchor as the plan spells it | Anchor kind | Re-derived declaration observed at `[P0-T13]` |
| --- | --- | --- | --- |
| 1 | `RegisterNavigation` | member name | `public void RegisterNavigation()` |
| 2 | `UnregisterNavigation` | member name | `public void UnregisterNavigation()` |
| 3 | `RegisterNavigationAsyncAction` | member name | `internal void RegisterNavigationAsyncAction(int itemIndex, int digits)` |
| 4 | `GenerateStringKbdAction` | member name | `internal KaStringAsync GenerateStringKbdAction(int i, int digits)` |
| 5 | `RegisterAsyncKeyActions` | member name | `internal void RegisterAsyncKeyActions()` |
| 6 | `Digits` | member name (property) | `internal int Digits` |
| 7 | `_digits` | member name (field) | `private int _digits = 1;` |
| 8 | `_digitRefreshNeeded` | member name (field) | `private bool _digitRefreshNeeded = false;` |
| 9 | `SetVisualDigits` | member name | `private void SetVisualDigits(int digits)` |
| 10 | `ExcludeFromCodeCoverage` (class-level attribute) | attribute name | `[ExcludeFromCodeCoverage]` at the class declaration |

Ten anchors, ten member or attribute names, zero line numbers. `[P0-T13]` recorded `PRESENT` for
each with the line number it observed at that moment; those observed numbers live in the evidence
artifact, not in the plan, which is exactly the separation this gate enforces. An observed number in
an evidence artifact is a measurement; a number in a plan task is an instruction that goes stale the
moment an upstream commit shifts the file.

## Every anchor this plan uses into `QuickFiler.Test/QuickFiler.Test.csproj`

Each is **XML element text**, re-derived at `[P0-T15]` by element text.

| # | Anchor as the plan spells it | Anchor kind | Occurrences observed at `[P0-T15]` |
| --- | --- | --- | --- |
| 1 | `<Compile Include="Controllers\QfcCollectionControllerTests.cs" />` | XML element text | 1 |
| 2 | `<Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />` | XML element text | 1 |
| 3 | `<Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />` | XML element text | 1 |

Three anchors, three element texts, zero line numbers. The two insertion anchors (rows 1 and 2) were
observed to be consecutive, so the owned one-line slot between them is unambiguous without any line
number. The item group is confirmed not alphabetically ordered, which is precisely why an element-text
anchor pair rather than a sort position or a line number is the correct identification.

## Why this gate exists

Upstream #468 rewrote large parts of `QfcCollectionController.cs` and inserted a contiguous block of
`<Compile Include>` entries into `QuickFiler.Test.csproj`. Any line number this plan had carried
forward from `spec.md` or from research would have been stale before Phase 1 began. Phase 0 therefore
re-derived every anchor by name or element text, and this gate is the mechanical proof that no stale
number leaked back into the plan text.

## Acceptance

- Both recorded counts are exactly `0` — met.
- The artifact enumerates every anchor the plan uses into those two files and confirms each is a
  member name or an XML element text — met: ten member or attribute names for the controller and
  three element texts for the project file.
