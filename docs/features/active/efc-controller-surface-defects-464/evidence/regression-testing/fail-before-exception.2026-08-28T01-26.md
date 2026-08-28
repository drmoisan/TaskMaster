Subject: RC8 — #465 sub-defect B, cross-thread control read

# Fail-before exception dossier — RC8 (#465 sub-defect B)

Timestamp: 2026-08-28T01-26
Task: [P7-T14]

## WhyFailingRunImpossible

The RC8 remedy **relocates a control read rather than changing a value**. Before the change,
`RefreshSuggestionsAsync` read `_formViewer.SearchText.Text` from inside the second `Task.Run` lambda,
on a thread-pool thread. After the change it reads the same property into a local as the method's first
statement, on the UI thread, and the lambda closes over that local. `_dataModel.FindMatches` therefore
receives **exactly the same string** it received before, so no assertion over observable output can
distinguish the two shapes.

The only distinguishing observable is **which thread performs the read**. Under constraint C3 that
cannot be exercised:

- no test may start a thread whose completion is awaited by polling;
- no test may run a message loop;
- no test may construct the `EfcViewer` control whose `SearchText.Text` the cross-thread read touches
  (constructing it creates window handles and requires a pump, which C3 and the repository test policy
  both prohibit).

A cross-thread WinForms control access is also not deterministically observable in the first place: it
raises `InvalidOperationException` only when the control's handle has been created and only when
`Control.CheckForIllegalCrossThreadCalls` is enabled, both of which require exactly the live-control
setup C3 forbids. A test written to observe it would be non-deterministic, which the determinism
requirement independently prohibits.

Therefore no test can observe this defect red, and the fail-before evidence for RC8 is structural.

## StructuralFailBeforeEvidence

Recorded in `docs/features/active/efc-controller-surface-defects-464/evidence/qa-gates/465-source-structure.md`
(`[P7-T13]`).

The measured quantity is the line offset of the sole `_formViewer` occurrence inside
`RefreshSuggestionsAsync` relative to the first `Task.Run(` occurrence inside it:

| | Line of `_formViewer` | Line of first `Task.Run(` | Offset | Meaning |
|---|---|---|---|---|
| Pre-change, read from `BASELINE_SHA` `38f097898639b054428188c9c5e266e54972c259` | `:799` | `:797` | **+2 (positive)** | the control read sits **after** the first `Task.Run(`, inside a thread-pool lambda |
| Delivered | `:879` | `:881` | **−2 (negative)** | the control read sits **before** any `Task.Run(`, on the UI thread |

**The sign flip from positive to negative is the fail-before / pass-after pair for this defect**,
measured structurally because it cannot be measured behaviourally. The token occurs exactly once inside
the method in both readings, so the offset is unambiguous. The pre-change figures were read with
`git show <BASELINE_SHA>:QuickFiler/Controllers/EfcFormController.cs`, not from the working tree.

## Why `MatchesForSearchText` is absent from decision D8's defect-preserving list

Decision D8 requires each new member that a regression test names to be introduced first in a
defect-preserving form, so the test can be observed failing before the correction.
`MatchesForSearchText` is deliberately excluded, and its exclusion is the reason this dossier exists.

It is a **new pure helper with no pre-change counterpart to preserve**. There is no prior member whose
defective behaviour it could reproduce: before the change the expression
`_dataModel.FindMatches(_formViewer.SearchText.Text)` was written inline inside the lambda. A
defect-preserving introduction would produce a helper that returns the same matches for the same input
as the corrected one, so its test would pass both before and after and would gate nothing.

`[P7-T1]` therefore introduces the helper and performs the RC8 relocation in one task, and RC8 is
discharged by the structural pair above together with this dossier. Every other extracted helper in this
plan that has a pre-change counterpart — `WithTrashRow`, `IsBannerRow`, `IsSelectableFolder`,
`ThrowInitializationFailure`, `ClaimsAltChord`, `IncognitoArgument`, `ApplyDeleteGesture` and the five
RC3-B boundary members — is on D8's defect-preserving list and was observed red before correction.
`BindSourceFolderRows` is a new routing method introduced by the corrective task `[P7-T7]` itself, whose
fail-before evidence is `[P7-T5]`.

## Negative-evidence search record

SearchScope: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/`
SearchPatterns: `fail-before-exception.*.md`
SearchResult:
- `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/fail-before-exception.2026-08-28T00-15.md` (the `[P1-T15]` dossier, a different subject)
- `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/fail-before-exception.2026-08-28T01-26.md` (this dossier)

The feature is not versioned, so there is no `<FEATURE>/<VERSION>/evidence/` scope to search; the feature
root above is the only canonical location.

## Filename distinctness

This dossier's filename is `fail-before-exception.2026-08-28T01-26.md`. The `[P1-T15]` dossier's
filename is `fail-before-exception.2026-08-28T00-15.md`. The two differ, so no minute-resolution
collision occurred and no alternative minute had to be minted. Both filenames are recorded here as the
task requires.

Output Summary: RC8 has no behavioural fail-before run and cannot have one, because the remedy relocates
a control read without changing any value and the only distinguishing observable — the thread performing
the read — cannot be exercised under the headless determinism constraints. The fail-before / pass-after
pair is the structural sign flip of the `_formViewer`-to-`Task.Run(` offset, from +2 at `BASELINE_SHA`
to −2 delivered, recorded in `465-source-structure.md`.
