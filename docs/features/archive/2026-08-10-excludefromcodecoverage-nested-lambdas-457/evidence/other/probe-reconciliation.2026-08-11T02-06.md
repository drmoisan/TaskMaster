# [P4-T5] Probe reconciliation against `spec.md` residual 1

Timestamp: 2026-08-11T02-06

## Branch taken: `Probe Answer: YES`

The `[P0-T12]` probe recorded `Probe Answer: YES`
(`<FEATURE>/evidence/baseline/async-d-state-machine-probe.2026-08-11T00-38.md`). Under the plan's
first branch, **the residual text stands as written** and this artifact records the confirmation.

## What changed in `spec.md`

**Nothing.** No text in `<FEATURE>/spec.md` was modified by this task.

The `NO` branch — which would have required correcting the residual text to state that the collector
emits no `d__` class for an attributed async member, thereby narrowing the residual — was **not
taken**, because the probe did not return `NO`. The `NOT-DETERMINABLE-FROM-CORPUS` branch was likewise
not taken.

## The residual text being reconciled

`<FEATURE>/spec.md` § Risks & Mitigations, residual 1 (lines 559-570) states:

> **Lambda bodies inside `[ExcludeFromCodeCoverage]` async members remain counted.** If an attributed
> member is `async` or an iterator, its state machine class `Type.<Member>d__<N>` is the only trace of
> the member, and because a `d__` class is admitted into the presence set (mandatory, per the async
> correction), lambdas declared inside an attributed async member are retained. This is deliberate:
> the alternative would delete covered lambdas in non-exempt async members and fail required
> direction 2.
> *Unverified sub-question:* whether the collector emits a `d__` class **at all** for an attributed
> async member could not be determined from the committed artifacts. If it does not, those lambdas are
> in fact excluded and this residual is narrower than described. **Probe that settles it:** apply
> `[ExcludeFromCodeCoverage]` to an `async` member in a scratch build, run the coverage pipeline, and
> search the raw report for `name="…&lt;Member&gt;d__…"`. Presence confirms the residual as stated;
> absence narrows it. Record the observed result in evidence.

## Confirmation

The probe searched a **verified raw** corpus and found the `d__` class **present** for an attributed
async member. Under the spec's own stated decision rule — "Presence confirms the residual as stated;
absence narrows it" — the residual is **confirmed as stated**.

| Element | Observed |
|---|---|
| Corpus | `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml` |
| Corpus raw-ness | verified: 2047 absolute `filename` attributes, 868 closure `<class>` elements, no `<sources>` element |
| Corpus capture date | 2026-08-07T02:19:25Z (from its own `<coverage timestamp="1786069165">`) |
| Search pattern | `name="QuickFiler\.Controllers\.QfcItemController.&lt;ToggleExpansionAsync&gt;d__` |
| Matches | **1** |
| Match (verbatim) | `name="QuickFiler.Controllers.QfcItemController.&lt;ToggleExpansionAsync&gt;d__203"` |
| Member attributed? | yes — `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcItemController.Navigation.cs:191` |
| Soundness guard | **satisfied** — the file's most recent commit of any kind is `6b821480` (2026-07-03T09:16:18-04:00), 35 days before the corpus was captured, and the attribute was verified present in that commit's content via `git show` |

Because the soundness guard is satisfied, the answer is `YES` rather than
`NOT-DETERMINABLE-FROM-CORPUS`: the attribute demonstrably predates the corpus, so the `d__` class in
the corpus was emitted for a member that was already attributed.

## Consequence

- The `spec.md` residual 1 text is accurate and requires no correction.
- The *Unverified sub-question* clause in that residual is now answered. Answering it was the point of
  spec AC 16; the answer is recorded in `evidence/baseline/` as that criterion requires. The spec text
  is left intact, including the sub-question paragraph, because this task's `YES` branch authorizes no
  edit — only the `NO` branch does.
- Presence-set source (2) is confirmed load-bearing in both directions, which is why `[P2-T4]` treats
  it as mandatory rather than optional.
- Residual (a) is handed to a follow-up potential entry at
  `docs/features/potential/2026-08-11-exempt-async-member-lambdas-remain-counted.md`, not absorbed
  into #457.

## Output Summary

Branch taken: `Probe Answer: YES`. The residual text in `<FEATURE>/spec.md` § Risks & Mitigations,
residual 1 stands as written and **nothing in `spec.md` was changed by this task**. The probe found
`QuickFiler.Controllers.QfcItemController.<ToggleExpansionAsync>d__203` present in a verified raw
corpus for a member attributed since 35 days before that corpus was captured.
