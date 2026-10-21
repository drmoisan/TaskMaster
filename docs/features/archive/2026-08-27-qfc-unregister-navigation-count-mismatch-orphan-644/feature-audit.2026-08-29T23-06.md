# Feature Audit — issue #644, navigation key ledger

- Issue: #644
- Work mode: `full-bug`
- AC source: `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md` only
  (no `user-story.md` exists and none is to be created, per the spec's work-mode banner and
  `.claude/skills/acceptance-criteria-tracking/SKILL.md`)
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head: `a2c69aead286ad0ec6c7087f1bd8c46d39d0d472`
- Baseline: `main` at `fa2ddefacf2c08abe18f3e3250d77da804534637`
- Review timestamp: 2026-08-29T23-06

## AC-16 — Independent Adjudication

AC-16 was deliberately referred to this review unchecked. It is adjudicated here on its merits. The
recorded orchestrator override `p4_t6_comparison_clause_undecidable_at_measured_noise_floor` is
treated as what it says it is — an authorisation to proceed past a gate, not an assertion that the
criterion is verified — and is not treated as settling anything.

**Criterion text, verbatim:**

> **AC-16 (no coverage regression on changed lines).** The repository coverage figure from the AC-15
> step-4 run is greater than or equal to the AC-0 baseline. Changed production lines live in a
> `[ExcludeFromCodeCoverage]` class and are therefore outside the denominator; that fact is stated
> explicitly in the coverage evidence artifact so the gate is not read as vacuously satisfied.

### Verdict: **PARTIAL.** Left unchecked in `spec.md`.

### Clause 2 — PASS

The second clause requires two things: that the changed production lines sit in an
`[ExcludeFromCodeCoverage]` class, and that this fact be stated explicitly in the coverage evidence
artifact. Both verified.

- `QuickFiler/Controllers/QfcCollectionController.cs` line 21 carries `[ExcludeFromCodeCoverage]` on
  the `public class QfcCollectionController` declaration at line 22.
- The fact is stated at `evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md` lines 127-138
  under its own heading, and again in the "Standing note required by the acceptance regardless of
  outcome" section at lines 181-188.
- The attribute is **pre-existing**, not added or widened by this change. This was checked
  specifically, because a change that excludes its own modified code and then argues coverage is
  unaffected would be circular:

```
$ git show e968a1a8804b7641380d4489c496662824d45767:QuickFiler/Controllers/QfcCollectionController.cs | grep -n "ExcludeFromCodeCoverage"
21:    [ExcludeFromCodeCoverage]
$ git diff e968a1a8804b7641380d4489c496662824d45767 -- QuickFiler/Controllers/QfcCollectionController.cs | grep -n "ExcludeFromCodeCoverage"
(no match)
```

No `[ExcludeFromCodeCoverage]` attribute is added anywhere in the branch diff, and no coverage
configuration exclude entry is added or changed. The circularity concern is disproved.

### Clause 1 — not verified, and not verifiable by any instrument in this repository

The first clause requires that "the repository coverage figure from the AC-15 step-4 run" be greater
than or equal to the AC-0 baseline. It fails on two independent grounds, both of which this review
re-derived rather than accepted.

**Ground 1 — the named instrument produces no figure.** The AC-15 step-4 command is
`vstest.console.exe ... /EnableCodeCoverage`. It emits a binary `.coverage` attachment and prints no
percentage. `evidence/qa-gates/p4-t5-vstest-final.2026-08-29T08-15.md` records this at lines 79-83.
No figure attributable to the instrument the criterion names exists, and none can be produced without
a conversion step the criterion does not authorise. The figure actually used comes from a different
command — `Invoke-MSTestWithCoverage.ps1` with Cobertura post-processing — which is a substitute
instrument. The criterion is therefore not evaluable as written even before the numbers are examined.

**Ground 2 — the substitute cannot decide the comparison at the resolution demanded.** Re-derived
arithmetic, computed by this review from the recorded integer counts rather than copied:

```
A 0.853303 85.3303%     (baseline,      54800 / 64221)
E 0.853194 85.3194%     (final state,   54793 / 64221)
F 0.853475 85.3475%     (final state,   54811 / 64221, byte-identical tree to E)
E-A pts -0.0109
F-A pts +0.0171
F-E pts  0.0280
noise as lines 18.0
```

Every recorded figure reproduces exactly. `lines-valid` is invariant at 64221 across all three runs.

Runs E and F measured the same source with the same command on the same machine with no intervening
edit of any kind, and they differ by 18 covered lines. They straddle the baseline: E is 0.0109 points
below it, F is 0.0171 points above it. The instrument's run-to-run spread is 2.6 times the shortfall
the clause was asked to adjudicate.

A comparison whose sign is determined by which of two equally valid measurements of an identical tree
happens to be taken is not adjudicating the property it was written to adjudicate. Selecting run F
because it is favourable is correctly rejected in the recorded evidence and is rejected here for the
same reason: an executor free to choose the run it is judged against cannot fail.

This is not a novel or convenient observation. The same instrument's non-determinism was measured
independently on this repository during issue #511, where the repository-wide C# figure moved within
a band of roughly 0.015 points across sessions with no source change. The finding recorded there —
do not gate on cross-session repository-wide constants; use a same-session baseline and a per-file
changed-lines comparison — is the same conclusion this evidence reaches by a cleaner experiment
(same session, byte-identical tree, one variable).

**On whether a two-decimal repository-wide `>=` can be verified by any instrument here: it cannot.**
The only repository-wide instrument available is the Cobertura post-processing path, and it has been
measured to move ~0.028 points on an unchanged tree. A gate cannot enforce a threshold finer than the
noise floor of the instrument that feeds it. To make this criterion decidable, one of three things
must change: the criterion must carry an explicit tolerance wider than the measured noise floor; or
it must be restated as a per-file changed-lines comparison, which is what the underlying policy in
`.claude/rules/general-unit-test.md` actually requires ("Code changes or refactors must not reduce
coverage for the lines that were changed"); or the harness must be made deterministic. None of the
three is in this issue's scope.

### On the argument "the changed file is excluded, therefore it cannot move the figure"

Assessed independently rather than accepted. **It is sound for direct movement and overstated for
total movement.**

Sound part: `[ExcludeFromCodeCoverage]` removes the annotated type's methods from both the numerator
and the denominator. The mechanical corroboration is good — the file appears in 0 of the 558
`<class>` entries of the post-processed document, and `lines-valid` is invariant at 64221 across a
change that inserted 18 and deleted 9 lines in that file. If those lines were instrumented, the
denominator would almost certainly have moved. So the file's own lines contribute nothing in either
direction, and that part of the argument holds.

Overstated part: exclusion removes the file's lines from the metric; it does not remove the file's
effect on the executed-line set of other, measured types. `UnregisterNavigation` now calls
`KbdActions<string, KaStringAsync, Func<string, Task>>.Remove` a different number of times and with
different arguments, and the six new tests exercise `KbdActions.Add`, `Remove`, and its enumeration
surface. Neither collaborator is excluded:

```
$ grep -n "ExcludeFromCodeCoverage" QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/KaStringAsync.cs
(no match; grep exit 1)
```

Both are in the denominator, and their measured coverage can move because of this change. The
likeliest direction is upward, since six new tests execute them where none did before. So the
sentence at `p4-t6` lines 136-138 — "There is no path by which this change adds or removes coverage
from any production file at all" — is false as stated. The correct claim is the narrower one: this
change cannot move the figure *through the excluded file*, which is what the invariant `lines-valid`
demonstrates.

The referral artifact states the correct, weaker form: "This is corroboration, not proof of the
clause: a real regression below the measured noise floor would remain indistinguishable from noise by
this gate." That is right, and the adjudication does not lean on the overstated sentence. The
overstatement is recorded as policy-audit finding PA-4 and does not change the verdict.

### Why PARTIAL rather than FAIL or UNVERIFIED

- Not PASS: clause 1 is not satisfied by evidence. Run E, the designated measurement, is below the
  baseline; no run is authorised as the basis for a pass; and the named instrument produced nothing.
- Not FAIL: a FAIL asserts a regression occurred. The evidence does not support that assertion any
  more than it supports its converse. Run F, on a byte-identical tree, is above the baseline. Both
  measurements are correct measurements of the same source; the honest statement is that the
  comparison has no information content, not that it came out negative.
- Not UNVERIFIED for the criterion as a whole: clause 2 is verified and passing, so a blanket
  "no evidence available" verdict would misstate the position.
- PARTIAL is the accurate verdict: one clause verified, one clause not decidable by the available
  instruments.

**AC-16 is left unchecked in `spec.md`.** It is not checked off, and specifically it is not checked
off on the strength of the override existing.

### Residual risk, and whether it is acceptable for merge

**Acceptable for merge.** Stated plainly, with the risk not minimised:

- **The risk is real.** A genuine coverage regression smaller than roughly 0.03 points — on the order
  of 20 covered lines out of 64221 — would be indistinguishable from instrument noise by this gate.
  This gate cannot see such a regression, and no argument above makes it visible.
- **It is bounded at that size.** The measured spread puts a ceiling on what can hide, and the
  invariant denominator confirms the movement is entirely in the numerator.
- **The gate the repository actually enforces is decided and passing.** The absolute floors in
  `.claude/rules/quality-tiers.md` are line coverage >= 85% and branch coverage >= 75%. Both
  final-state runs clear the line floor: 85.3194% and 85.3475%, a margin of 0.32 to 0.35 points, more
  than eleven times the instrument's noise. Branch coverage is 0.792927, a margin of 4.29 points.
  These comparisons are decidable at this resolution and they pass on either run. Only AC-16's chosen
  *proxy* — a repository-wide `>=` against a prior session's figure at two decimals — is undecidable.
- **The property the underlying policy cares about is satisfied on its own terms.** "No regression on
  changed lines" is met: the changed production lines are not measured and have no coverage to lose;
  no test was deleted; six were added; 1254 of 1254 pass in the touched assembly and 6876 of 6876
  repository-wide, against a 1248-test baseline with the delta accounted for exactly.
- **The defect is in the criterion, not in the change.** AC-16 restated a per-changed-line
  no-regression requirement as a repository-wide two-decimal comparison. That restatement is
  unsatisfiable here for structural reasons that have nothing to do with this fix: the sole changed
  production file is not in the metric at all, so the criterion as written can only ever measure
  unrelated repository noise.

**Recommended follow-up (not a merge blocker):** file an issue to reformulate the repository's
coverage no-regression gate — either with an explicit tolerance exceeding the measured noise floor,
or as a same-session per-file changed-lines comparison. The #511 finding and this run's run-E/run-F
experiment are the two pieces of evidence that issue should carry.

---

## Disposition of the Two Other Disclosed Deviations

### 1. `[P4-T8]` sixth clause — disposition **sound**, with one bookkeeping objection

The clause enumerates four admissible feature-folder paths. This run is a resume; an earlier segment
had already committed Phases 0 through `[P4-T7]` with their evidence, so 35 committed evidence
artifacts appeared in the repository-wide listing and the observed set exceeded the enumeration.

**Sound, for three reasons:**

- The clause's premise, not the property it guards, is what failed. The plan's own supporting prose
  states the premise explicitly: "The evidence artifacts this plan writes under the feature folder
  are untracked and unstaged at this point and are correctly absent from the listing." A resume
  falsifies that premise. Nothing about the hazard changed.
- The hazard is measurably absent, and this review verified it independently against the resolved
  base branch rather than only against the anchor. The plan states the hazard at its `[P4-T8]`
  supporting prose: a rewrite made anywhere else in the repository by `[P4-T1]`'s repository-wide
  `dotnet tool run csharpier format .`, invisible to the three pathspec-scoped spans.
  `git diff --name-only fa2ddefa HEAD` returns the six code paths plus this feature folder and
  nothing else. `.csharpierignore` additionally excludes `**/evidence/**`, so the formatter could not
  have authored any of the 35 extra paths even in principle.
- Not rewriting the clause was the right call. The plan directs recording and reporting "rather than
  widening this acceptance". Editing acceptance text after seeing the result is retroactive
  acceptance-widening and is the worse error. The deviation is recorded verbatim under its own
  "LITERAL-CLAUSE DEVIATION" heading with measured category counts, which is a better audit trail
  than a silently amended clause would have been.

**One objection, non-blocking.** The plan routes an out-of-enumeration path to
`REMEDIATION-REQUIRED`. The executor recorded it, reported it, and also checked the task `[x]`. A
`[x]` asserts the acceptance held; it did not. The internally consistent handling is the one this
same run applied to AC-16: leave it unchecked, escalate, let the reviewer adjudicate. `[P4-T8]` and
AC-16 were treated differently under materially similar circumstances. This is bookkeeping, not
substance — the deviation is fully disclosed and the guarded property is verified — but the
inconsistency is worth naming, because a checked box with a disclosed failure inside it is easier for
a later reader to miss than an unchecked box.

### 2. The stale sentence at `p4-t6` line 205 — correcting forward is the **right call**; signposting is adequate but one-directional

Line 205 of `evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md` still reads "AC-16 is
checked off under this adjudication and is flagged as such in the `[P5-T19]` AC status summary." It
was deliberately left byte-for-byte unmodified.

**Correcting forward is right.** A recorded run artifact is evidence of what a run concluded at the
time it ran. Rewriting it would erase the fact that the run concluded something the referral later
withdrew — which is exactly the information a later reader needs to judge whether the referral was a
genuine escalation or a retrofit constructed to look like one. The audit trail's value is precisely
that it preserves superseded conclusions. Editing the record to agree with the current position would
make the record unfalsifiable.

**Signposting: adequate, but the back-pointer is missing.** The referral names the file, the exact
line number, and declares the sentence superseded, in a dedicated section at lines 84-93. A reader
arriving at `p4-t6` alone gets no forward pointer. Three facts prevent that from misleading:

- The artifact's own title reads "PROCEEDING UNDER RECORDED ORCHESTRATOR ADJUDICATION".
- Its line 28 states "It is a documented deviation from the task's literal `>=` clause, not a
  satisfaction of it. Feature review adjudicates it independently." Line 190 states "not a clean
  numeric pass."
- `spec.md`, the authoritative AC source, shows AC-16 as `- [ ]`.

So the stale sentence contradicts its own file three sections earlier and contradicts the source of
truth. A careful reader cannot be misled by it, and the contradiction is discoverable from either
end within one document.

**Recommendation, non-blocking:** *append* — do not edit — a one-line "SUPERSEDED BY
`p5-t17-ac16-referral.2026-08-29T08-15.md` as to AC-16 check-off" footer at the end of `p4-t6`. An
append adds to the record without rewriting it, preserving the audit trail while closing the
back-pointer gap. This is a strictly better outcome than either leaving it as-is or editing line 205.

---

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC-0 Phase 0 baselines | PASS | `evidence/baseline/p0-t7-counts.2026-08-29T08-15.md` records all five counts (500, 13, 226, 3, 2437) with commands and exit codes; `evidence/baseline/p0-t12-coverage-baseline.2026-08-29T08-15.md` records 0.853303. Each figure re-derived by this review where it survives at head. |
| AC-1 red before green | PASS | `evidence/regression-testing/p1-t4-expect-fail.2026-08-29T08-15.md` records T1 failing against unmodified production code with the failure message captured. |
| AC-2 `RemoveBelowThresholdAsync` path | PASS | T1 `Passed` in the `[P4-T5]` TRX. Test body verified to drive the removal through a recording `_removeGroupByEntryId` delegate and to assert `CollectionKeys(registry).Should().BeEmpty(...)`. |
| AC-3 `RemoveSpecificControlGroup(int)` path | PASS | T2 `Passed`. Test asserts `NotThrow` on a second `RegisterNavigation()` and `BeEquivalentTo(new[] {"1".."5"})` with `OnlyHaveUniqueItems`. |
| AC-4 width-crossing path | PASS | T6 `Passed`. Ten groups at width 2, shrunk to nine, registry empty after unregister. |
| AC-5 state transitions | PASS | T3 `Passed`. Register / unregister / register / unregister, no throw, registry empty. |
| AC-6 empty-ledger negative case | PASS | T4 `Passed`. Unrelated `"Other"`-sourced entry verified untouched by SourceId and Key. |
| AC-7 `UnregisterNavigation` no longer reads `_itemGroups` | PASS | T5 `Passed`. Verified structurally against the method body, which references only `RegisteredNavigationKeys` and `_kbdHandler.StringActionsAsync`. |
| AC-8 new test file exists and is compiled | PASS | `QuickFiler.Test.csproj` line 133 adds the `Compile Include`; all six tests appear as executed `Passed` results in the TRX, which is the detection mechanism for a missing entry in a legacy non-SDK project. |
| AC-9 amended characterisation tests pass | PASS | All three amended tests plus the unchanged duplicate-registration test recorded `Passed`. `*Key 2 SourceId Collection*` assertion verified preserved verbatim at line 422. |
| AC-10 frozen-file constraints hold | PASS | `awk 'END{print NR}'` returns 499 (baseline 500, ceiling 500); `[TestMethod]` count 13, equal to baseline. Both re-measured by this review at head. |
| AC-11 digits-file assertion flipped and passing | PASS | Assertion is `.BeEmpty("issue #644 replaced the count-bounded removal loop...")`; XML doc records the residual as closed; file is 226 lines with 3 `[TestMethod]`s; all three tests `Passed`. See CR-1 for a second, unamended doc block in the same file. |
| AC-12 `_registeredDigits` fully removed, no CS0414 | PASS | `grep -rn "_registeredDigits" --include=*.cs .` returns zero occurrences; `evidence/qa-gates/p4-t4-nullable-build.2026-08-29T08-15.md` records exit 0 with no `CS0414`. |
| AC-13 comment synchronisation, no assertion drift | PASS | The `#468` defects diff adds and removes only `///`, `//`, and `because:` string content; no `Should()`, `ThrowAsync`, or `[TestMethod]` appears on any changed line; the added text names `RemoveSpecificControlGroupAsync` and `_itemGroups[selection - 1]`; the test `Passed`. The new attribution is also substantively correct. |
| AC-14 footprint containment | PASS, with recorded anchor substitution | The criterion names `ecdb1c84...`. That anchor predates the merged fix for issue #638 and lists `QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`, and 40 paths of the #638 feature folder, none of which this change authored. The substitution to `e968a1a8...` was verified necessary and correct. The substantive property is verified against the resolved base branch, which is stronger: `git diff --name-only fa2ddefa HEAD` is the six code paths plus this feature folder and nothing else. `QuickFiler/QuickFiler.csproj` unchanged, no interface file touched, controller net +9 lines against a bound of 10. |
| AC-15 full toolchain pass | PASS | Four gates recorded green in one uninterrupted pass with no file rewritten by any step: csharpier check exit 0 over 1562 files; analyzer build exit 0, 0 errors, 5 warnings; type-check build exit 0, 0 errors, 5 warnings, no CS0414; vstest exit 0, 1254 / 1254. The pass was legitimately restarted from `[P4-T1]` after `[P4-T8]` found a net-line overrun, which is the required loop behaviour. |
| AC-16 no coverage regression on changed lines | **PARTIAL — remains unchecked** | Adjudicated in full above. Clause 2 PASS; clause 1 not decidable by any instrument in this repository. Residual risk bounded at ~20 covered lines and acceptable for merge. |
| AC-17 evidence location | PASS | All 40 artifacts under `<feature>/evidence/<kind>/` with `2026-08-29T08-15` stamps. No path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` appears in the branch diff. |

## Regression Check Against the Reported Defect

The issue's Expected Behavior is: "Unregistration removes exactly the set of navigation keys that
registration added, regardless of any `_itemGroups` mutation that occurred in between. A subsequent
registration succeeds, and a navigation keypress resolves against exactly one handler."

Each of the three clauses is now pinned by a test:

- "removes exactly the set registration added, regardless of any intervening mutation" — T1 (seam
  removal), T2 (direct list removal), T6 (width-crossing shrink), and the retained
  `UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys` (growth
  across the width boundary). All four assert an empty `"Collection"` key set.
- "a subsequent registration succeeds" — T2 asserts `NotThrow` on the second `RegisterNavigation()`.
- "a keypress resolves against exactly one handler" — T2 asserts `OnlyHaveUniqueItems` on the
  resulting key set, which is the property that prevents the `InvalidOperationException` from a
  multi-element `Find` match.

All three unbracketed reaches named in the Root Cause Analysis are covered: `RemoveBelowThresholdAsync`
via the seam (T1), and `RemoveSpecificControlGroup(int)` as reached by both the synchronous `'R'`
char action and `PopOutControlGroup(int)` (T2, which models the shared list mutation). The spec's
correction of the issue text — that the async `'R'` is Reply and the async remove is `'Z'`, which is
already bracketed — was checked against the Root Cause Analysis and is internally consistent.

The residual the #472 width test previously pinned explicitly as out of its scope — the surviving
`"10"` entry — is now closed, and that test's assertion was flipped from `.Equal(new[] { "10" })` to
`.BeEmpty(...)`. The issue's own "Logs / Screenshots" section identified that pinned residual as the
observable manifestation of this defect, so flipping it is the direct closure evidence.

### Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md
- Total AC items: 18
- Checked off (delivered): 17
- Remaining (unchecked): 1
- Items remaining: AC-16 (no coverage regression on changed lines) — adjudicated PARTIAL by this
  review and deliberately left unchecked. Clause 2 (excluded-class statement) is verified PASS;
  clause 1 (repository-wide >= comparison at two-decimal resolution) is not decidable by any
  instrument present in this repository. No AC item was newly checked off by this review.
```

## Verdict

**PASS. Blocking findings: 0.**

The fix delivers the invariant the issue asks for, is minimally scoped, is proved by six tests that
were demonstrated red beforehand, and passes every gate the repository can decide. Seventeen of
eighteen acceptance criteria are met. The eighteenth is PARTIAL because the criterion, as written,
asks for a comparison the repository has no instrument capable of making; its residual risk is
bounded, quantified, and acceptable for merge.

Recommended before merge, neither blocking: correct the stale XML doc identified as CR-1, and redact
the absolute host path identified as PA-7.
