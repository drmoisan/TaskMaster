# Fail-before exception dossier — issue #468 defect family

Timestamp: 2026-08-26T16-24

Command: not applicable (this artifact is a written dossier, not a command step)

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Seven items in this plan have no conventional fail-before (red) run. Each is recorded below with its
own `WhyFailingRunImpossible:` line and its own alternative-proof section. Every other defect in the
family does carry a genuine red-then-green pair; those pairs are indexed in
`evidence/qa-gates/p14-t8-fail-before-index.2026-08-26T16-30.md` and are not restated here.

`SearchScope:` `docs/features/active/qfc-collection-controller-defects-468/evidence/regression-testing/`
and `docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/`.
`SearchPatterns:` `*fail-before*.md`, `*pass-after*.md`, `fail-before-exception.*.md`.
`SearchResult:` fourteen `*fail-before*.md` artifacts and eleven `*pass-after*.md` artifacts exist;
before this file, no `fail-before-exception.*.md` existed. This file is the first.

---

## Item 1 — issue #469 defect 3, the behavioural ordering test

Test: `TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation`
(`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`).

WhyFailingRunImpossible: the pre-fix field was a `ConcurrentDictionary`, whose enumeration order is
explicitly unspecified by its contract. A behavioural ordering assertion run against it would not
have failed deterministically — it would have passed or failed depending on hash-bucket layout and
insertion history for that particular run. A test that fails only sometimes is not a red state; it
is a flaky test, and committing one to establish a red state would violate the determinism
requirement of the General Unit Test Policy.

Alternative proof. The defect was proven red by a *structural* test asserting the declared contract
of the field rather than an emergent property of one run:

| | Artifact | Result |
|---|---|---|
| Red | `p4-t3-fail-before.2026-08-26T10-03.md` | `ItemGroupsToMoveFieldDeclaresAnOrderedContract` — `EXIT_CODE: 1`, `Failed: 1` |
| Green | `p4-t7-pass-after.2026-08-26T10-12.md` | both tests pass — `total="2" passed="2" failed="0"` |

The structural test is deterministic in both directions: before the fix the field's declared type was
not an ordered collection type and the assertion failed on every run; after the fix it is, and the
assertion passes on every run. The behavioural test
`TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation` then rides on top of it as the permanent
green guard that the ordering is actually observed after a mutation, which is the property the
structural test alone cannot express.

---

## Item 2 — issue #468, removal of twelve dead members

WhyFailingRunImpossible: this defect is a *removal*. There is no behaviour to assert before the
change, and no test can be written that fails while dead code is present and passes once it is
absent without itself being a tautology over the source text. A test asserting "member X does not
exist" would not compile before the removal if it named the member in source, and if it used
reflection it would be asserting the same fact the removal performs — a restatement, not an
independent observation.

Alternative proof. Three independent observations, all recorded:

1. **Compilation.** The solution builds with 0 errors after the removal
   (`p1-t6-analyzers.2026-08-26T08-45.md`, `p1-t7-nullable.2026-08-26T08-45.md`). A removed member
   that still had a compile-time caller would have produced CS0103 or CS1061.
2. **The existing suite stays green.** `p1-t8-suite.2026-08-26T08-45.md` records the full
   `QuickFiler.Test` run after the removal with a failed count of `0`. No existing test exercised any
   removed member.
3. **The reflective-caller search.** `evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md`
   closes the only gap compilation cannot close: a caller that reaches a member by name at runtime.
   Search (a) covers `*.csproj`, `*.resx`, `*.config`, `*.xaml`, `*.json`, `*.settings` across 398
   build-input files for all twelve identifiers and returns zero hits. Search (b) enumerates all 42
   `GetMethod(` call sites and all 0 `InvokeMember(` call sites in first-party C# and shows that none
   passes any of the twelve identifiers. That artifact also records the measured non-vacuity of the
   search scope, so the zero result is not an artefact of an empty scope.

`p1-t3-dead-identifier-sweep.2026-08-26T08-45.md` additionally records the post-removal hit count for
each of the twelve identifiers in the owned file, and
`p1-t4-live-member-nonregression.2026-08-26T08-45.md` records that the live overload
`AnyOpenDropDowns(bool, CancellationToken)` was retained per D3.

---

## Item 3 — issue #474 defect 1, the `(QfcFormController)_parent` downcast

WhyFailingRunImpossible: the downcast statement is
`await ((QfcFormController)_parent).SkipGroupAsync();`. Reaching it at runtime requires the
collection controller to have been initialised far enough to route a keyboard action, which requires
`UiThread` initialisation and a shown form. Showing a form in a unit test is prohibited outright by
this repository's test policy, and no seam exists that would let the statement be reached without
one. A behavioural red state is therefore unreachable, not merely inconvenient.

Alternative proof. Two observations, one of which is a genuine red-then-green pair:

| | Artifact | Result |
|---|---|---|
| Red | `p2-t6-fail-before.2026-08-26T09-14.md` | `ParentFieldAndConstructorParameterAreTypedIQfcFormController` — `EXIT_CODE: 1`, `Failed: 1` |
| Green | `p2-t10-pass-after.2026-08-26T09-21.md` | passes after the retype |

The reflection assertion is a real observation of the compiled metadata, not a restatement of source
text: it reads the declared type of the `_parent` field and of the constructor parameter from the
emitted assembly. Before the retype both were the concrete `QfcFormController`; after, both are
`IQfcFormController`.

The second observation is the compile-time constraint itself, recorded in
`evidence/qa-gates/p2-t9-downcast-sweep.2026-08-26T09-20.md`: the literal
`(QfcFormController)_parent` occurred exactly once in the owned file at the P0-T15 baseline (`:1232`)
and occurs zero times after the fix. The call now binds to `IQfcFormController.SkipGroupAsync()`
declared at `QuickFiler/Controllers/IQfcFormController.cs:38`. A runtime cast that could fail has
been replaced by a binding the compiler checks, so the failure mode the defect describes can no
longer be expressed in the source.

---

## Item 4 — issue #469 defect 4, the undo-stack contract documentation

WhyFailingRunImpossible: per D11 this change is deliberately behaviour-preserving. It adds an XML doc
block to `IQfcCollectionController.MoveEmailsAsync` and its implementation, plus a single
`_ = stackMovedItems;` discard. The parameter is **not** removed and no call site changes. A test that
failed before this change and passed after it would have to observe a behavioural difference that the
change is specifically designed not to create.

Alternative proof. The triage finding the documentation records is that the undo record is written by
the email filer's push-to-undo-stack path onto the *same stack instance the caller passes*, so the
parameter is not unused in effect even though the controller body did not name it. The permanent
green guard for that contract is
`MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack`
(`p12-t3-pass-after.2026-08-26T11-37.md`, `total="1" passed="1" failed="0"`), which asserts that the
two argument shapes the contract now documents as equivalent are in fact equivalent. That test would
have passed before the change as well — which is the point: it pins the documented contract so a
future removal of the parameter cannot silently change it.

The removal of the parameter is recorded as a follow-up candidate rather than performed here.

---

## Item 5 — issue #470 defect 2, the above-reservation behavioural case

WhyFailingRunImpossible: per D7, `ToggleUnGroupConv` cannot be driven COM-free. Its first two
statements are `SafeSetTlpLayout(false)` and `UnregisterNavigation()`, and `MakeSpaceForItems`
reaches `TableLayoutHelper.InsertSpecificRow` on the WinForms `_itemTlp`. The pre-fix red state for
the above-reservation case is an `ArgumentOutOfRangeException` raised inside that method, and
reaching it requires a live `TableLayoutPanel` with rows and a controller initialised through the
WinForms path. There is therefore no permanent post-fix green counterpart **at the
`ToggleUnGroupConv` level**, and consequently no red run at that level either.

Alternative proof. The reconciliation logic was extracted into the pure static helper
`ReconcileInsertionCount(...)` (D6), and the above-reservation case is asserted permanently against
that helper by
`ReconcileInsertionCount_AboveReservation_ReturnsInsertionsCountAndWarnsOnce`
(`p7-t12-pass-after.2026-08-26T10-39.md`, `total="6" passed="6" failed="0"`). The equal- and
below-reservation cases are asserted by the two sibling tests in the same run, so all three arms of
the reconciliation are covered.

A genuine red-then-green pair does exist for the extraction itself:

| | Artifact | Result |
|---|---|---|
| Red | `p7-t3-fail-before.2026-08-26T10-33.md` | `ConversationReconciliationHelpersExist` — `EXIT_CODE: 1`, `Failed: 1` |
| Green | `p7-t12-pass-after.2026-08-26T10-39.md` | six tests pass |

That red state is attributable rather than incidental: the test looks both members up by name through
reflection and does not name them in source, so their absence before the fix is a runtime failure and
not a compile error.

---

## Item 6 — issue #470 defect 2, the base-email-index guard

WhyFailingRunImpossible: the `baseEmailIndex == -1` guard sits on the same `ToggleUnGroupConv` path
described in item 5 and inherits the same barrier. Driving the guard's pre-fix failure requires the
same WinForms-bound initialisation, so no red run at that level is possible.

Alternative proof. `ReconcileInsertionCount(...)` takes `baseEmailIndex` as one of its six named
values and carries it into the warning message, so a `-1` base index is now reported rather than
silently subscripted. The adjacent negative-index defect, issue #470 defect 1, *is* reachable
COM-free at the `PromoteFirstChild` / `ToggleGroupConv` level and does carry a genuine red-then-green
pair:

| | Artifact | Result |
|---|---|---|
| Red | `p8-t1-fail-before.2026-08-26T10-45.md`, `p8-t2-fail-before.2026-08-26T10-45.md` | `EXIT_CODE: 1` each |
| Green | `p8-t4-pass-after.2026-08-26T10-48.md` | passes |

`PromoteFirstChild_WithNoMatchingChild_ReturnsMinusOneWithoutSubscripting` and
`ToggleGroupConv_WithNoMatchingOriginal_DoesNotSubscriptWithMinusOne` establish the sentinel
behaviour that the base-email-index guard relies on, per D4.

---

## Item 7 — issue #474 defect 2, the two move-readiness tests

Tests: `TryGetMoveReadiness_WithUnassignedDestination_ReturnsFalseAndProducesNotificationText` and
`TryGetMoveReadiness_WithAllDestinationsAssigned_ReturnsTrueAndEmptyNotification`
(`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`).

WhyFailingRunImpossible: before the P13-T1 seam, the only way to evaluate move readiness was to read
the `ReadyForMove` property, whose false path called `MessageBox.Show` directly. A unit test reading
that property in the pre-seam code would have blocked on a modal dialog that no test host can
dismiss. The run would have **hung**, not failed. A hung run produces no TRX and is not a red state;
it is an unbounded wait that would have to be killed.

Alternative proof. The seam (`p13-t3-seam-commit.2026-08-26T11-41.md`, commit `4938779a`) introduced
`TryGetMoveReadiness(out string notifications)` containing exactly the prior evaluation logic, plus a
private injectable delegate `_notifyNotReady` whose default is the unchanged modal call with the same
message, caption, buttons, and icon. Seam neutrality was measured, not assumed:
`p13-t2-seam-suite.2026-08-26T11-40.md` records 964 passed / 0 failed at the seam against 964 passed
/ 0 failed at P12-T4 — an identical passed count, so the restructuring changed no observable
behaviour anywhere in the suite.

The two behavioural tests then substitute a recording delegate for the default and assert the
readiness value, the notification text, and — in the first test — that the `ReadyForMove` property
returns the same value and hands the predicate's text to the notification path unchanged. Both pass:
`p13-t6-pass-after.2026-08-26T16-18.md`, `total="2" passed="2" failed="0"`.

---

## Summary table

| # | Item | Why no red run | Alternative proof |
|---|---|---|---|
| 1 | `#469-3` behavioural ordering test | concurrent-dictionary enumeration order is unspecified; a pre-fix red would be flaky by construction | structural red-then-green pair P4-T3 / P4-T7 |
| 2 | `#468` dead-member removal | a removal has no prior behaviour to assert | compilation, green suite, reflective-caller search over 398 build inputs |
| 3 | `#474-1` downcast | the call site is unreachable without a shown form | reflection red-then-green pair P2-T6 / P2-T10, plus the compile-time constraint |
| 4 | `#469-4` contract documentation | deliberately no behavioural delta (D11) | triage finding plus the permanent equivalence test P12-T3 |
| 5 | `#470-2` above-reservation case | `ToggleUnGroupConv` is WinForms-bound (D7) | pure-helper assertions P7-T8/T9/T10 plus the P7-T3 / P7-T12 pair |
| 6 | `#470-2` base-email-index guard | same WinForms barrier as item 5 | the six-value warning message plus the `#470-1` sentinel pair P8-T1/T2 / P8-T4 |
| 7 | `#474-2` readiness tests | the pre-seam property showed a modal; the run would hang, not fail | measured seam neutrality (964 = 964) plus the P13-T6 pass-after |

All seven items are named, each carries its own `WhyFailingRunImpossible:` line, and each carries its
own alternative-proof section.
