---
name: 484-qfc-revision-seams
description: "#484 QfcItemController revision rounds: unreachable WebView2 detach needs a dedicated named-exception method; ownership-list change sweeps plan->issue.md->spec.md (spec is the AC source and was missed until R4); additive-only clause needs a CSharpier exemption"
metadata:
  type: project
---

Revision-cycle seams from the #484 qfc-item-controller-defects plan (R1-R6, 2026-08-24).

1. An unreachable guarded body (WebView2 `_coreWebView2` detach, null without a live runtime) placed
   inline in `UnwireEvents()` silently breaks per-member >=90% coverage floors. Fix shape: extract to
   a dedicated `private void DetachWebResourceRequestedHandler()`, name it as a second explicit
   exception in the coverage-delta task AND the spec criterion, verified by the fail-before exception
   dossier — never `[ExcludeFromCodeCoverage]` (C5-style constraints forbid it).
2. Adding a fifth owned test file changes every "eight owned files" / "eight added members" count.
   The revision directive enumerated 11 tasks but missed D6, P7-T1, P7-T2, P6-T1, P8-T15, P6-T5;
   leaving them at "eight" makes scope/format/surface gates unsatisfiable (the plan itself writes the
   ninth file). I applied the forced eight-to-nine edits and reported them as consequential.
   **Why:** count references are consumers of the ownership list; see [[thread-granted-discharges-through-consumers]].
   **How to apply:** after any ownership-list change, grep the plan for the old cardinal number
   ("eight", "four owned") and sweep every hit; report edits outside the directive's named list.
3. A directive's task list can mislabel: P7-T11 was listed for the four-to-five test-file sweep but
   counts the four owned PRODUCTION files (exemption audit). Changing it would corrupt the C5
   exemption arithmetic. Leave and report.
4. Unnamed test-adding tasks still saying "all four owned test files are at most 500 lines" were
   reconciled by a C2 capacity-rule sentence extending the check to the fifth file by reference,
   instead of editing tasks the directive forbade touching.
5. Shared-helper-only file pattern: `QfcItemController.TestSupport.cs` (baseline 365, Compile Include
   already at QuickFiler.Test.csproj:146) receives arrange helpers only, never test methods; per-file
   projections capped at 480 (20-line CSharpier reflow margin under the 500 ceiling).

Round 2 (B1-B5, same day): the R1-R6 revision itself created five second-order defects worth
checking proactively in any table-redistribution revision.

6. Re-deriving an assignment table without rewriting the sentence that introduces it leaves
   present-tense prose ("sum to 458... margin is 10") contradicting the table (400+88=488, margin 68).
   Recast the superseded figures as explicitly superseded, and check any Phase 0 task that transcribes
   them (P0-T17 records "aggregate planned addition and margin" into a baseline artifact).
7. A "identical to research section N" citation goes false the moment a row is added post-research;
   replace with a first-N-rows-identical + measured-fresh parenthetical, never leave "identical".
8. A carve-out that cites a dossier for fact X is unreachable until the dossier-producing task's
   acceptance clause REQUIRES recording X (P7-T7 cited P5-T12's dossier for
   DetachWebResourceRequestedHandler, but P5-T12 only required naming InitializeWebViewAsync). Same
   producer-consumer threading as [[thread-granted-discharges-through-consumers]].
9. Rule-count references ("the four capacity rules") are consumers of a rule list exactly like
   cardinal file counts — sweep them when a rule is added.
10. Companion issue.md is a consumer too: its owned-file list and Downstream Consumers section must
    disclose a newly-owned shared file (TestSupport.cs is consumed by 16 sibling test files incl. one
    adjacent to feature 444) or cross-child changed-file-set gates cannot see the collision risk.

Round 4 (B6-B8, 2026-08-24): the document family propagation was still incomplete two rounds later.

11. spec.md is the sole AC source for full-bug mode, and it was NEVER swept in R1-R3: five stale
    passages (four-file scope sentence, owned-file list + Compile Include parenthetical, an explicit
    "not to be written" prohibition on TestSupport.cs, files-to-change strategy line, and the
    changed-file-set AC) made [P6-T9] unsatisfiable. When an ownership list changes, the sweep order
    is plan -> issue.md -> spec.md, and spec.md is the one that gates execution. Also: a falsified
    claim corrected in the plan (R3's "-=`-statement-only unreachable" narrowing) recurs verbatim in
    the spec criterion the check-off task keys on — sweep the spec for the same false premise.
12. Preserve check-off key sentences: [P6-T9]/[P8-T10] key verbatim on the AC's LEADING sentence, so
    edit only the tail clause and keep the checkbox count invariant (50) — count `- [ ]` before and
    after.
13. An additive-only clause plus a plan-wide CSharpier format task is a latent contradiction: a
    fail-closed executor can read a formatting-only rewrite of a pre-existing member as a forbidden
    "body change". Append an explicit exemption ("formatting-only rewrite produced by the [P7-T1]
    pass is not a violation; comparison is on whitespace-normalized declarations").
14. Residual not in the directive's list: spec.md:556 still says "matches all four owned test files
    today" (framework-choice rationale). Left untouched per do-not-touch-unnamed-sections and
    reported; arguably still true since TestSupport.cs carries no assertions.
