# Preflight Round 4 — Issue #440

Timestamp: 2026-08-29T03-55
Reviewer: atomic-executor under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`
Plan under review: `plan.2026-08-29T00-22.md` in this feature folder
Signal: `PREFLIGHT: REVISIONS REQUIRED`
Convergence: `CONVERGENCE: FURTHER ROUNDS LIKELY`

The reviewer executed no plan task and modified no file. All 55 tasks remain unchecked.

## Confirmations

Both round-3 defects are confirmed closed, and both sibling invalidations the planner found on its own initiative are confirmed correct.

The rewritten P0-T7 was the primary object of this round and is confirmed executable and complete. The reviewer verified the provisioning-source resolution, the restore script's citations, that all 18 solution projects are packages.config style, that `Analyzer Include` items appear in 17 project files and in no `.props` or `.targets`, that the derived set has 11 distinct paths of which exactly 5 are unrestorable, that both acceptance halves can fail independently, and that the `TOOLCHAIN-BLOCKER:` halt precedes P0-T10.

The reviewer additionally checked for a failure mode the Analyzer-only scope could have missed. It compared every `packages\<Id>.<Version>\` reference across all 18 project files against the union of all 18 `packages.config` pins. Exactly three referenced directories are absent from that union: the two skewed analyzer directories and `altcover.8.6.45`. The altcover references are two `Import` elements with an `Exists` condition and no companion `Error Condition`, so they no-op silently when absent. All 80 references to the two skewed analyzer directories are `Analyzer` elements. P0-T7's Analyzer-only derivation is therefore sufficient: no other reference class can produce CS0006 after the restore.

The P4-T2 count-parity conclusion was confirmed against a recorded run rather than by inference. In issue #677 the csharpier baseline ran before the restore and before the first build, recording 1554 checked files, and the post-build post-test final check recorded 1558. The delta of 4 is accounted for by source files, not by the restored packages tree or by build output.

Also re-derived: the three file line counts; all six gate literals at exactly 1; the `LeftArrow()` span and its four conjuncts; the nine sequence points and the four floor lines; the Moq seam; the helper and call sites; all 14 filter class names and all 12 named results; and the wrapper-script line citations. Scope containment holds at exactly three backticked source files, with no project file, `packages.config`, or analyzer package directory backticked as a concrete path.

## Defect requiring revision

### 1. P4-T6's acceptance misattributes the branch-coverage requirement to the wrong sentence of AC-15 (blocking)

The P4-T6 acceptance states that the `condition-coverage` observation is the branch-level evidence AC-15's **second** sentence requires.

**The orchestrator verified this independently.** AC-15 in `spec.md` is two sentences. The first reads: "Line and branch coverage for `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` is not reduced relative to the pre-change measurement, and every changed line is covered." The second reads: "Evidence recorded under this feature folder's `evidence/coverage/` directory." The branch-coverage requirement is in the first sentence. The second names an evidence location and requires no branch-level evidence; it is the sentence the plan discharges through the `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record.

The statement is false as written. It sits inside an acceptance clause, so it is transcribed into the P4-T6 evidence artifact and read by the AC-15 reviewer at P5-T15, which cites that artifact. This is the same defect class round 3 identified in P3-T5's justification. The gate itself is unaffected: recording `condition-coverage` before and after is the correct evidence, and gates (1) through (4) are unchanged.

Replacement text, substituting for the sentence beginning "The artifact additionally records the `condition-coverage`":

> The artifact additionally records the `condition-coverage` of the transition `if` line before and after the change, which is the branch-level evidence AC-15's first sentence requires, that sentence being the one requiring that line and branch coverage for the file is not reduced relative to the pre-change measurement. AC-15's second sentence names the evidence location only, and it is discharged by the `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record at the head of this plan.

## Non-blocking observations

1. **P0-T7's resolution rule is ambiguous under a literal reading.** The task says the derived set is the Include values "with each item's leading relative segment resolved against the repository root." Every Include value begins with a parent-directory segment, and every project file sits one directory below the repository root, so the intended result is a path under the repository-root packages directory. Resolving literally against the repository root instead yields its parent. The misreading fails closed, since the referenced-but-missing count would stay non-zero after provisioning and the task would halt, so it cannot produce a false pass, but it can cost an execution attempt. Optional clarification: replace the quoted phrase with "with each item's parent-directory prefix resolved against the directory of the project file that declares it; every project file in this solution sits one directory below the repository root, so each Include resolves to a path under the repository-root packages directory."
2. **P0-T11's CS0006 attribution is a heuristic stated categorically.** It is correct for the missing-package cause, which the reviewer confirmed is the only class that can go missing after the restore, but CS0006 also arises when a project-to-project dependency fails to build. The prescribed remedy is harmless in either case, so no change is needed.
3. **Global rule 9's first sentence** still asserts anchoring for every diff, status and grep gate, with the qualification following. Unchanged from round 3, where it was recorded as non-blocking. No gate is affected.
4. **Glob pathspec arguments appear backticked** in P0-T7 and Global rule 9, but only inside command spans. They cannot resolve to a repository file, so the derived change footprint is unaffected.
5. **P0-T7's halt phrasing** says "rather than advancing to P0-T10" while P0-T8 is the immediate successor. "Halts the phase" is the operative instruction and is unambiguous.

## Round-count note

This is the fourth preflight round against a two-round target. Each round has found genuine blocking defects rather than re-reporting earlier ones, and three of the four were introduced by the replacement prose written to close the previous round's findings. The remaining defect is a single-sentence substitution that changes no command, no threshold, and no cross-task citation.
