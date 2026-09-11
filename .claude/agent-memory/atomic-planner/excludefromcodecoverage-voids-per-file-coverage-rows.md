---
name: excludefromcodecoverage-voids-per-file-coverage-rows
description: A file carrying a class-level [ExcludeFromCodeCoverage] has NO class element in the Cobertura document, so any acceptance condition demanding a per-line hits row, a baseline-vs-post comparison, or a "pre-existing uncovered" row for it is unsatisfiable
metadata:
  type: feedback
---

Before writing any acceptance condition that demands a per-file coverage row — a `hits=` row per changed line, a baseline-vs-post-change `hits` comparison, an "uncovered before and after" classification — grep the file for `ExcludeFromCodeCoverage`. `dotnet-coverage` honours a class-level `[ExcludeFromCodeCoverage]`, so the file has **no `class` element at all** in either the raw or the processed document. Not a zero-hits entry: no entry.

**Why:** #731 round 10. `[P0-T11]` required "at least one `hits=` row for each of the five filenames"; two of the five (`QuickFiler/Controllers/QfcCollectionController.cs:21`, `QuickFiler/Controllers/QfcDatamodel.cs:25`) carry the attribute, so zero rows can exist and no executor action can produce one. Worse, `[P5-T7]` asserted a *specific false expectation* — that `QfcCollectionController.cs` "is expected" under `Pre-existing uncovered, no regression` because its changed guard line is "uncovered before and after". That reasoning silently presumes a `hits=0` entry. Both defects survived **nine** preflight rounds because they are invisible by reading: the sources compile, the plan is internally consistent, and the fact only surfaces once coverage actually runs.

**How to apply:**
- Enumerate `class/@filename` in a real collected document before authoring per-file coverage acceptance. Reading the source is not enough; reading the plan is not enough.
- Split the requirement: demand rows only for files with >= 1 selected `class` element, and route the rest to an explicitly named sub-heading (`Uninstrumented files`, `Uninstrumented, not comparable`) whose rows cite the attribute's file and line as the reason. State that appearing there is a recorded fact, not a task failure.
- Exclude uninstrumented lines from **both** the regression count and the "pre-existing uncovered" count, and say plainly that *no* coverage judgment is available — neither direction. Do not let them quietly land in the "no regression" bucket, which reads as a passing judgment that was never made.
- Record the gate's honest residual scope in one sentence. After excluding uninstrumented and comment-only files, a "five file" changed-line gate can shrink to one file that receives substantive lines. Say so, or the plan misrepresents its own coverage.
- Keep the gate hard over the files that *do* have data. Narrowing what a gate observes is not licence to soften what it requires — see [[acceptance-edits-must-be-false-before-true-after]].
- The pre-existing attribute is usually out of scope: note it is on `origin/main`, not introduced by the change, and do not widen the plan to remove it.

Corollary that makes separator anchoring load-bearing in fact: an *interface* file can be instrumented while its implementation is not. `QuickFiler/Interfaces/IQfcDatamodel.cs` has a `class` element with executable lines (a `readonly struct` with expression-bodied members) while `QuickFiler/Controllers/QfcDatamodel.cs` has none, so an unanchored suffix match on `QfcDatamodel.cs` selects the interface file and attributes its numbers to a file with none of its own. Never relax the `(^|/|\)` filename anchor. Related: [[observation-scope-must-match-blast-radius]], [[wiring-gates-must-be-wiring-sensitive]].
