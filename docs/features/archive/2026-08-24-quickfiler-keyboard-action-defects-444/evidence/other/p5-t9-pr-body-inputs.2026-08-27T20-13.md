# [P5-T9] Pull-request-body inputs

Timestamp: 2026-08-27T20-13
Command: none — this artifact is authored content, assembled from `spec.md` and the Phase 1 through
Phase 4 evidence
EXIT_CODE: 0
Output Summary: all four required items are present, each under its own named heading. This is the
exact text the epic orchestrator must carry into the integration pull request body.

Three acceptance criteria in `spec.md` are satisfied only by that PR body and therefore cannot be
checked off by this feature's execution: AC-472-10 (the follow-up issue number recorded in the PR
body), AC-482-11 (the behaviour widening stated in the PR body), and AC-482-12 (the trigger and
severity correction repeated in the PR body). `[P5-T25]`, `[P5-T26]`, and `[P5-T27]` record those
three as deferrals against this artifact.

---

## Item 1 — Deliberate behaviour widening

The #482 fix routes all expansion keyboard registration through a single owner,
`SyncExpandedRegistrations(bool expanded)`, which maintains **both** the synchronous
`_kbdHandler.CharActions` registry and the asynchronous `_kbdHandler.CharActionsAsync` registry
together rather than one per toggle path. That is a deliberate widening of observable behaviour, not
an incidental side effect:

- **`'B'` and `'D'` now respond after a *synchronous* expansion.** Previously only an asynchronous
  expansion populated `CharActionsAsync`, and the Alt-key path that reads `CharActions` was left
  empty after a synchronous toggle.
- **Alt+`B` and Alt+`D` now respond after an *asynchronous* expansion.** Previously only a
  synchronous expansion populated `CharActions`.

The alternative — collapsing onto a single registry — was considered and rejected. Four focus-path
methods in the forbidden `QfcItemController.EventWiring.cs` conditionally call the expansion
register and unregister methods on `_expanded`. Under a single-registry unification one of those
cleanup paths would remove from the registry that no longer holds the entries, re-creating exactly
the silent-`false` divergence these three issues describe. Maintaining both registries makes every
one of those four call sites operate on a registry that genuinely holds the entries.

Idempotence is preserved because the two unregister calls are unconditional and
`KbdActions.Remove` returns `false` rather than throwing when the pair is absent. That is why
repeated and interleaved toggles no longer raise `ArgumentException` from `KbdActions.Add`, without
any change to `Add`'s contract.

---

## Item 2 — Correction to #482's filed trigger and severity

The filed issue's stated trigger is **unreachable**, and the filed severity is **overstated**. Both
corrections are recorded in `spec.md` under `### #482 — expansion registry divergence` and are
repeated here because the PR must not restate an unsupported claim.

**The filed trigger is dead code with respect to this interleaving.** The promoted document names the
synchronous `ToggleExpansion()` call inside `ActivateBySelectionAsync` as what makes the interleaving
reachable in production rather than theoretical. That call is guarded by `if (blExpanded)`, and both
asynchronous callers pass a value that is always `false`: one passes the literal `false`, and the
other passes a value returned from `ToggleOffActiveItemAsync`, whose expansion branch is commented
out so it returns its parameter unchanged. The guarded call therefore never executes with a `true`
argument, and the filed trigger cannot produce the interleaving.

**The live trigger is Right, then Down, then Right.** Fully grounded in `spec.md`:

1. **Right** on an item runs `ToggleExpansionAsync(On)`, setting `_expanded = true` and adding `'B'`
   and `'D'` to `CharActionsAsync`.
2. **Down** runs `SelectNextItemAsync`, which marshals to the **synchronous** `SelectNextItem`, and
   through `ChangeByIndex` and `ToggleOffActiveItem` reaches the **synchronous** `ToggleExpansion()`.
   That clears `_expanded` and removes from `CharActions`, **where nothing was ever added**, so
   `Remove` returns `false` silently. `CharActionsAsync` still holds `'B'` and `'D'`.
3. **Right** on the same item again finds `_expanded == false`, so `ToggleExpansionAsync(On)` runs
   and `CharActionsAsync.Add` is called for an entry that is already present, raising
   `ArgumentException`.

**The severity is a dead key, not a crash.** The exception surfaces through the asynchronous keyboard
handler in `KeyboardHandler.cs`, whose `catch` block logs it. The user-visible symptom is therefore a
`'B'` or `'D'` key that stops responding for that item, not an unhandled exception or a crash. The PR
body must state the corrected trigger and the corrected severity rather than repeating the filed
ones.

---

## Item 3 — Coverage-policy conflict: pre-existing and unresolved

This repository carries **two mutually inconsistent coverage policies**, and this feature neither
resolves nor silently picks between them:

| Source | Line floor | Branch floor | New-code floor |
| --- | --- | --- | --- |
| `CLAUDE.md` §UT2 | `>= 80%` | not stated | `>= 90%` for new modules, classes, or methods |
| `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` | `>= 85%` | `>= 75%` | not stated separately |

The conflict is **pre-existing**: it is a property of the repository's policy documents, not of this
feature's diff. This feature reports its figures against both and clears both, so no interpretation
was needed to declare a pass:

| Figure | Value | `CLAUDE.md` §UT2 | `general-unit-test.md` / `quality-tiers.md` |
| --- | --- | --- | --- |
| repository-wide line coverage, final | 85.13 percent | clears `>= 80%` | clears `>= 85%` |
| repository-wide branch coverage, final | 79.21 percent | no floor stated | clears `>= 75%` |
| `SyncExpandedRegistrations` line coverage (new member) | 100 percent | clears `>= 90%` | — |

Resolving the conflict requires a decision by the repository maintainer about which document is
authoritative. It is recorded here as outstanding, not closed.

---

## Item 4 — Outstanding promotion of the `UnregisterNavigation` count-mismatch defect

`spec.md` `### Downstream notes` item 3 describes a **second, distinct** defect in
`UnregisterNavigation`: the method bounds its unregister loop with the *current* `_itemGroups.Count`,
while `RemoveSpecificControlGroup(int)` mutates `_itemGroups` with no unregister/register bracket.
When a group is removed through that unbracketed path — reachable from `RemoveBelowThresholdAsync`
via the `RemoveGroupByEntryId` seam, and from the `'R'` char action — the count the unregister loop
later reads no longer matches the count in force at registration, so the loop stops short and leaves
orphaned navigation registrations behind. Every production call site discards `KbdActions.Remove`'s
`bool`, so the divergence is silent until a later `Add` or `Find` throws.

This feature **does not fix it**, under decision D-472-B and `CLAUDE.md`'s Bugfix Workflow step 2
("If you uncover deeper design problems, open a new issue instead of widening scope"): fixing it
requires the key-ledger design, which breaks characterisation tests in
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, a file that sits at exactly 500 lines
with a `[TestMethod]` count frozen by upstream #468.

It has been promoted through the feature-promotion lifecycle:

| Item | Value |
| --- | --- |
| GitHub issue | **#644** |
| Issue URL | `https://github.com/drmoisan/TaskMaster/issues/644` |
| Potential entry | `docs/features/potential/promoted/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan.md` |
| Promotion commit | `12256da4 docs(444): promote count-mismatch follow-up defect as issue #644` |

**The PR body must record issue number #644.** That is the outstanding clause of AC-472-10: the
potential entry and the GitHub issue both exist, but the criterion also requires the issue number to
appear in this feature's PR body, which only the integration PR can satisfy.

This feature's #472 regression test asserts the residual orphan explicitly — exactly one `"10"` entry
remains — and carries an XML documentation comment attributing that residual to issue #644 and
stating that it is out of this feature's scope, so the assertion does not silently absorb the second
defect.

---

## Acceptance

- The artifact carries all four items under four named headings — met: `## Item 1 — Deliberate
  behaviour widening`, `## Item 2 — Correction to #482's filed trigger and severity`, `## Item 3 —
  Coverage-policy conflict: pre-existing and unresolved`, and `## Item 4 — Outstanding promotion of
  the UnregisterNavigation count-mismatch defect`.
