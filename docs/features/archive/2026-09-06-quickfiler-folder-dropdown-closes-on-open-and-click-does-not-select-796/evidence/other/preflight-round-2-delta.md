# Preflight round 2 delta — issue #796

- Timestamp: 2026-09-07T05:10
- Plan under revision: docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
- Round 2 verdict: PREFLIGHT: REVISIONS REQUIRED / CONVERGENCE: NO FURTHER ROUNDS EXPECTED
- Reviewer: atomic-executor, confirming round, text-and-citation pass with targeted read-only checks

Round 2 confirmed every sweep that round 1's fixes were meant to close. The line-count idiom
propagation, the B3 reversal, the conditional fifth expect-fail row, the porcelain changes at P1-T14
and P1-T15, the P7-T3 combined total, the AC6 phase ordering, whole-plan ordering integrity, evidence
paths, scope lock, path polarity, and cross-document consistency with spec.md all PASS.

Three blocking defects remain. All three are consequences of the adopted B3 mechanism that no earlier
pass could have observed, which is the sibling-invalidation class the confirming round exists to
catch. All three are orchestrator-accepted without amendment.

## How to apply this delta

Apply every item, blocking and non-blocking alike, in ONE revision round. Apply the supplied text
VERBATIM; do not substitute your own wording. Where text was wrapped for this document's width,
reassemble it onto one line. Report a per-item disposition for all seven items using exactly one of
`applied-verbatim`, `applied-with-mechanical-reassembly`, or `not-applied-with-reason`. If you judge
an item wrong, apply it as given or leave it unapplied and report the disagreement; do not silently
rewrite it.

## Orchestrator adjudication

All three blocking defects are ACCEPTED as written. No amendment. The reasoning behind each was
independently checked against the tree:

- B1 is correct on both legs. A non-nullable `bool` parameter cannot render an `unavailable` literal,
  and `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` injects its item controllers
  as Moq mocks of the interface, whose proxy is not the concrete type, so the successful-cast arm is
  unreachable from every test this plan creates. A formatter no test executes, counted in a 90 percent
  changed-line denominator with no admissible exclusion, is an unsatisfiable gate.
- B2 is correct. P9-T9's own acceptance requires it to record a SHA that does not exist until after
  its commit, so its artifact is necessarily untracked afterwards, and P9-T10 writes another artifact
  after that commit. The round-1 B6 fix closed this fixpoint only for P9-T11's own two writes; the
  same class reappears one commit further along.
- B3 is the most consequential of the three and is correct. Line 56 of
  QuickFiler/Controllers/QfcFormController.Deactivate.cs reads
  `group.ItemController?.CancelBreadcrumbSelector()`. That null-conditional is the existing code's own
  evidence that a group with a null item controller is reachable. An unguarded diagnostic would raise
  a NullReferenceException, the per-item boundary catch at lines 58-69 would convert it into a
  `logger.Error` entry, and a silent no-op would become a logged error. That is a behavioural change
  in Phase 1, which the AC6 ordering constraint forbids, and no existing test injects a null item
  controller so P1-T12 could not detect it.

Note the relationship between B1 and B3: B1 moves the unavailable case INTO the formatter via a
nullable parameter, and B3 makes the argument expressions null-safe. They are jointly satisfiable
only together, and D3c reconciles the Decisions record with both. Apply all three as one unit.

## Blocking defects

### B1 — [P1-T4] and [P9-T7]: the second formatter is unreachable from every test the plan creates

D1a. In [P1-T4], replace:

> and the pure method `internal static string FormatItemCancelDiagnostics(int itemNumber, bool selectorWasOpen)` returning a single line containing the labels `ItemNumber=` and `SelectorWasOpen=`.

with:

> and the pure method `internal static string FormatItemCancelDiagnostics(int itemNumber, bool? selectorWasOpen)` returning a single line containing the labels `ItemNumber=` and `SelectorWasOpen=`, rendering the second label as `SelectorWasOpen=unavailable` when the argument is null and as the boolean otherwise. The parameter is nullable so that the unavailable case is produced inside the formatter. That keeps the per-item log statement a single unconditional call, and it keeps the formatter reachable from the existing deactivate suite, whose tests inject their item controllers as Moq mocks of the interface and therefore never produce a successful cast to the concrete type.

D1b. In [P9-T7], replace:

> states the framework reason it cannot be reached from a headless test

with:

> states the reason it cannot be reached from the tests this plan creates, which is either a framework limitation or the mocked-seam limitation named at the end of this task

D1c. Append to [P9-T7]:

> One changed line is unreachable from the test population this plan creates, and it is admitted here by name rather than left for the executor to discover at the gate: the internal selector-open member task P1-T4 adds to `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`. Its only reader is the per-item log statement in `ParkFocusAndCancelSelectors`, which reaches it only when the loop's interface-typed item controller casts successfully to the concrete internal type, and every test in QfcFormControllerDeactivateTests that injects item controllers injects them as Moq mocks of the interface, so that cast yields null in every test. The artifact names that line with its file and line number, states that reason, and counts it against the same at-most-3 allowance, leaving at most two further individually named exclusions available; the 90 percent threshold is then evaluated over the remaining measurable behavioural changed lines.

### B2 — [P9-T11]: the porcelain permitted set omits two artifacts that cannot exist before the last commit

D2. Replace the acceptance sentence of [P9-T11], from `Acceptance: the artifact names which of the two branches was taken` through `asserts over the amended message body.`, with:

> Acceptance: the artifact names which of the two branches was taken and records the four commands of the final clean pass in order; the porcelain reading is taken immediately before the amend, after this task's own artifact and check-off have been written; and it lists no path other than members of the `PRE-EXISTING-DIRTY-SET:` recorded in evidence/baseline/p0-t14-scope-baseline.md and paths inside the feature folder docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796. A terminal gate demanding a porcelain output of zero lines is not used, and neither is one permitting only this plan file and this task's own artifact: evidence/qa-gates/p9-t9-final-commit.md records a SHA that does not exist until after the P9-T9 commit, and evidence/qa-gates/p9-t10-scope-boundary.md is written after that commit, so both are necessarily untracked at this reading, as are this task's own artifact and its own check-off. The amend is what folds all four into the final commit. The gate remains strict outside the feature folder: any path under QuickFiler, under QuickFiler.Test, or anywhere else in the tree that is not a member of the recorded pre-existing set fails it. The commit message is not changed by the amend, which is why no acceptance clause asserts over the amended message body.

### B3 — [P1-T4]: the per-item diagnostic dereferences a reference the existing code guards

D3a. In [P1-T4], replace:

> The deactivate handler reads it by casting the loop's interface-typed item controller to the concrete internal type and reports `SelectorWasOpen=` from the result.

with:

> The deactivate handler reads it by casting the loop's interface-typed item controller to the concrete internal type and passes the result to the formatter. Both per-item argument expressions are null-safe, because the existing code guards that same reference with a null-conditional at line 56: the item number is obtained with a null-propagating access and a null-coalescing default, and the selector-open value with a null-propagating access on the cast result, which yields null both for a null item controller and for a controller that is not the concrete type. No `if` is added and no exception can escape the added statement, so a group whose `ItemController` is null continues to reach the boundary catch not at all, and Phase 1 stays free of behavioural change.

D3b. In [P1-T4], replace the final acceptance sentence:

> When the cast yields null, which cannot occur for any production item group but is reachable in principle, the diagnostic records the literal `SelectorWasOpen=unavailable` rather than a fabricated boolean, so the AC6 evidence never carries a value that was not observed.

with:

> When the cast yields null, or when the loop's item controller is itself null, the diagnostic records the literal `SelectorWasOpen=unavailable` rather than a fabricated boolean, so the AC6 evidence never carries a value that was not observed. That case is produced inside the formatter from its nullable parameter, and every member access on the loop's item controller inside the added per-item log statement is written with a null-propagating or null-coalescing operator, matching the existing guard at line 56.

D3c. Replace the final sentence of Decisions record item 9:

> The null-cast branch P1-T4's acceptance requires is written as a conditional EXPRESSION supplying the argument of the single per-item log statement, not as an added `if` statement, a `return`, a `throw`, or an assignment to a new local; that is what lets the same task satisfy both the `SelectorWasOpen=unavailable` clause and the no-added-control-flow clause.

with:

> The null-cast branch P1-T4's acceptance requires is carried by the formatter's nullable parameter rather than by any statement in the handler: the per-item log statement is a single unconditional call whose arguments are null-propagating and null-coalescing expressions, so no `if` statement, `return`, `throw`, or assignment to a new local is added. That is what lets the same task satisfy the `SelectorWasOpen=unavailable` clause, the no-added-control-flow clause, and the requirement that the formatter be reachable from the existing deactivate suite, which injects its item controllers as Moq mocks of the interface and therefore never produces a successful cast to the concrete type.

## Non-blocking observations — apply O1, O2 and O3 in the same round

### O1 — [P1-T2] slack description does not describe the stated band

The task says the band admits three lines of slack either side of the expected 485, while the stated
window is at least 480 and at most 486, which is five below and one above. The explicit bounds are
operative so the gate is unaffected. Replace the slack phrase with:

> and the band admits five lines below the expected value and one above

### O2 — expect-fail inventory row 4 overstates its own gate

The table records NativeCloseWithNoCommitPending_StillCancelsSelection as "Recorded Failed at P5-T3",
while P5-T3 admits either Passed or Failed for that test and says both satisfy the carve-out. Nothing
is unsatisfiable; the column is simply stronger than the gate it names. Change that cell to:

> Recorded Passed or Failed at P5-T3; both satisfy that gate's carve-out

### O3 — [P7-T4] does not carve out the recorded pre-existing dirty set

One of its two commands is a porcelain status, and its acceptance requires that neither output name a
path beginning with UtilitiesCS/ or UtilitiesCS.Test/. If the recorded pre-existing set ever held
such a path the gate would fail for a state the executor did not create. The set is expected empty,
so this is latent. Append to the P7-T4 acceptance:

> A path that is a member of the `PRE-EXISTING-DIRTY-SET:` recorded in evidence/baseline/p0-t14-scope-baseline.md does not fail this gate, because the executor did not create it and in some cases may not remediate it; the artifact names any such path it excluded on that basis.

### O4 — [P0-T13] will record a validator result rather than the absence line

No change needed. For the record, the orchestrator ran the validator against the revised plan and it
returned ok=true with no warnings, so the probe's second admitted outcome is the one that will be
observed. The task is correctly written as record-and-continue.

## Confirmed correct in round 2 — do not change

Changing any of these reopens a closed question and costs a round.

- All eight downstream ceiling gates read the recorded `LINE-COUNT-IDIOM:` line; the blank-line-omitting
  idiom survives only inside P0-T12's prohibition.
- The P1-T2 band arithmetic: 498 physical minus twelve handler lines minus one separating blank equals
  485, inside the stated window, and the window can fail because an unperformed move leaves 498.
- The write set is sixteen, both excluded files appear only unbackticked, and the enumeration returns
  101 backticked path occurrences across exactly 16 distinct values.
- The adopted cast mechanism is well typed: internal partial class at EventHandlers.cs line 25,
  `internal IQfcItemController ItemController` at QfcItemGroup.cs line 39, same assembly.
- Every expect-fail carve-out is complete at its own gate position in both branches of P4-T7, with the
  baseline counts of 7 and 5 test methods confirmed by measurement.
- P7-T3's combined total of 11 and the derived per-class subtotals.
- AC6 phase ordering, whole-plan ordering integrity, evidence paths, and cross-document consistency
  with spec.md including the six acceptance criteria matching issue.md verbatim.
- The porcelain gates are safe against the bootstrap products, because .gitignore line 191 ignores the
  packages tree and line 350 ignores the SDK directory.

## Required output for this revision round

Return the plan path, a per-item disposition line for all seven items (B1, B2, B3, O1, O2, O3), any
disagreement you are referring to the orchestrator, and the full `PLANNER-INTERNAL-REVIEW` and
`SELF-REVIEW` record blocks re-derived against the tree as it stands after this revision. A citation
verified in an earlier round is evidence about a superseded state and may not be carried forward.
