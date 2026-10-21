# [P0-T11] — BLOCKED: acceptance condition unsatisfiable against the observed tree

STATUS: INCOMPLETE — BLOCKED. This artifact is **not** the `Baseline per-line hits` record [P0-T11] requires. It records the observed state that prevents that record from being produced. [P0-T11] remains unchecked in the plan.

Timestamp: 2026-09-03T13-32

Task: [P0-T11]
Issue: #731

Command: the [P0-T10] extraction rule (separator-anchored `class/@filename` match, de-duplicated per-line map, `GetAttribute` reads) applied to `coverage/baseline.cobertura.processed.xml` for each of the five production filenames this plan changes.

EXIT_CODE: 0 (the extraction itself ran without error; the blocking condition is in the data, not the command)

## Blocking observation

Two of the five production files this plan changes have **no `class` element at all** in the baseline Cobertura document, in either the raw or the processed form:

- `QuickFiler/Controllers/QfcCollectionController.cs`
- `QuickFiler/Controllers/QfcDatamodel.cs`

Cause, verified at source: both types carry a class-level `[ExcludeFromCodeCoverage]` attribute — `QuickFiler/Controllers/QfcCollectionController.cs:21` and `QuickFiler/Controllers/QfcDatamodel.cs:25`. `dotnet-coverage` honours that attribute, so neither file is instrumented and neither appears in the coverage document. This is a pre-existing property of the tree under the ratified COM/VSTO/WinForms coverage exemption; it is not a consequence of any edit this plan makes, and it is observable in the Phase 0 baseline document before any Phase 1 edit.

Confirmation that this is not a post-processing artefact: the set of `Qfc*` `class/@filename` values is identical in `coverage/baseline.cobertura.raw.xml` and in `coverage/baseline.cobertura.processed.xml`, and neither filename appears in either set.

## Why the acceptance cannot be met

[P0-T11]'s acceptance requires that the `Baseline per-line hits` heading "carries at least one `hits=` row for each of the five filenames". A `hits=` row is derived from a `line` element inside a selected `class` element. For the two filenames above there is no selected `class` element and therefore no `line` element, so zero rows exist for them. No sequence of actions available to an executor can produce a row for a file that the coverage tool did not instrument. Relaxing the condition to three of five, or synthesising rows, would be a change to an acceptance condition and is outside execution authority.

## Consequential defect in [P5-T7]

[P5-T7] carries a positive factual claim that the same observation falsifies. Its acceptance prose states that `QuickFiler/Controllers/QfcCollectionController.cs` "is expected in that sub-heading — its only changed executable line is the reentrancy guard, at baseline line 991 and post-change line 992 ... so the guard is uncovered before and after this change." That framing presumes the guard has a Cobertura entry carrying `hits=0`. It has none: the entire type is excluded from measurement, so the changed line has no `post_hits` value and no `baseline_hits` value of any kind. The same applies to the `QuickFiler/Controllers/QfcDatamodel.cs` construction-site change made by [P3-T4].

## Filename match audit (fully derivable, recorded here as observed)

Integers are the count of `class` elements selected by the separator-anchored match and the count the unanchored bare-filename match would have selected, taken from `coverage/baseline.cobertura.processed.xml`.

| Filename | Anchored | Unanchored | Additional elements the unanchored match would have added (by `name` attribute) |
|---|---|---|---|
| `QfcFormController.SetupDisposal.cs` | 1 | 1 | none |
| `QfcCollectionController.cs` | 0 | 0 | none |
| `QfcDatamodel.cs` | 0 | 1 | `QuickFiler.Interfaces.QfcDequeueBatch` |
| `QfcQueue.cs` | 1 | 1 | none |
| `QfcRemainingQueueAdmission.cs` | 1 | 1 | none |

The `QfcDatamodel.cs` row is direct confirmation of the anchoring rationale the plan states: the unanchored bare-filename match selects the `QuickFiler.Interfaces.QfcDequeueBatch` type declared in `QuickFiler/Interfaces/IQfcDatamodel.cs`, whose executable lines would have been folded into the `QfcDatamodel.cs` map. The anchored match correctly excludes it.

## De-duplicated per-line map totals for the three measured files

| Filename | Total map entries | Entries with hits greater than 0 |
|---|---|---|
| `QfcFormController.SetupDisposal.cs` | 157 | 111 |
| `QfcQueue.cs` | 312 | 157 |
| `QfcRemainingQueueAdmission.cs` | 25 | 23 |

No selected `class` element for any of the five filenames carries a `filename` attribute beginning with a drive-letter prefix, so the union-scoped `Cobertura document state:` value for this task is `processed`.

## Requested plan correction

This requires a plan revision by `atomic-planner`; it is not an executor decision. Two acceptance conditions need to change:

1. [P0-T11] acceptance: replace "carries at least one `hits=` row for each of the five filenames" with a condition that admits a file the coverage tool does not instrument — for example, that the heading carries at least one `hits=` row for each of the five filenames **that has at least one selected `class` element**, and that any filename with zero selected elements is recorded under a named sub-heading together with the reason and the source citation for its `[ExcludeFromCodeCoverage]` attribute.
2. [P5-T7]: the changed-line comparison must state how a changed line in an uninstrumented file is handled. Those lines have neither a baseline nor a post-change `hits` value, so they cannot be regressions and cannot be listed under `Pre-existing uncovered, no regression` in the form the task specifies. The `QuickFiler/Controllers/QfcCollectionController.cs` expectation quoted in that task's prose must be corrected accordingly, and `QuickFiler/Controllers/QfcDatamodel.cs` needs the same treatment.

---

SUPERSEDED 2026-09-03T14-05: the plan was revised to admit an uninstrumented file, and [P0-T11] then completed against the revised acceptance. The authoritative [P0-T11] record is the 'Baseline per-line hits' section of EVIDENCE/baseline/mstest-coverage.md. This file is retained as the audit record of the block that produced the revision; it is not the [P0-T11] artifact.
