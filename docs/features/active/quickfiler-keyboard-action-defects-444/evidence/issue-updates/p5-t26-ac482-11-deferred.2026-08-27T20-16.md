# [P5-T26] Deferral of AC-482-11

Timestamp: 2026-08-27T20-16
Command: none — this artifact records a deferral decision
EXIT_CODE: 0
Output Summary: AC-482-11 requires a statement in the integration pull-request body. This
preparation run does not create a pull request, so the criterion cannot be satisfied from inside this
branch. The checkbox is deliberately left unchecked. The required text is authored and ready at
`[P5-T9]`.

DEFERRED-TO-ORCHESTRATOR: AC-482-11 requires the integration pull-request body, which this preparation run does not create

PostedAs: unknown

No GitHub issue comment or body update was posted by this task. This artifact is a local deferral
record.

## The criterion, verbatim from `spec.md`

> The deliberate behaviour widening — `'B'`/`'D'` responding after a synchronous expansion and
> Alt+`B`/Alt+`D` after an asynchronous one — is stated in the PR body.

## Status

| Clause | Status |
| --- | --- |
| the widening is stated in the PR body | **outstanding** — no pull request exists |

The behaviour widening itself **is delivered** and is verified by tests: `[P3-T8]` recorded the
interleaving regression test passing, and AC-482-04 through AC-482-07 are checked off against it.
What is outstanding is purely the disclosure obligation — that the widening be *stated in the PR
body* so a reviewer is not surprised by a behaviour change that no filed issue asked for.

## Where the required text lives

`docs/features/active/quickfiler-keyboard-action-defects-444/evidence/other/p5-t9-pr-body-inputs.2026-08-27T20-13.md`
holds the text under its heading `## Item 1 — Deliberate behaviour widening`. It states both halves
of the widening explicitly:

- `'B'` and `'D'` now respond after a **synchronous** expansion, where previously only an
  asynchronous expansion populated the registry the ordinary keystroke path reads.
- Alt+`B` and Alt+`D` now respond after an **asynchronous** expansion, where previously only a
  synchronous expansion populated the registry the Alt-key path reads.

It also records why the alternative — collapsing onto a single registry — was rejected: four
focus-path methods in the forbidden `QfcItemController.EventWiring.cs` conditionally call the
expansion register and unregister methods on `_expanded`, and under a single-registry unification one
of those cleanup paths would remove from the registry that no longer holds the entries, re-creating
the same silent-`false` divergence the three issues describe.

## What the orchestrator must do

Carry `## Item 1` of the `[P5-T9]` artifact into the integration pull-request body. Once it appears
there, this criterion becomes satisfiable and may be checked off by whoever authors that body.

## Acceptance

- The artifact carries the required `DEFERRED-TO-ORCHESTRATOR:` line — met; it appears above,
  verbatim.
- That criterion in `spec.md` remains `- [ ]` with its text unmodified — met; the checkbox was not
  touched.
