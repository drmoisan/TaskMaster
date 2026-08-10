# Review-Finding Promotions — CR-1 / CR-2 / CR-3

Timestamp: 2026-08-09T00-45

Records the orchestrator's disposition of the non-blocking findings raised by the feature review in
`code-review.2026-08-08T21-59.md`. None was fixed inside this delivery; the user's scope directive
for #505/#506/#518 is that further defects are promoted, not folded in.

## Gate status at the time of this disposition

All three audit artifacts returned **PASS** with **0 blocking findings**:

| Artifact | Verdict | Blocking |
|---|---|---|
| `policy-audit.2026-08-08T21-59.md` | PASS | 0 |
| `code-review.2026-08-08T21-59.md` | PASS | 0 |
| `feature-audit.2026-08-08T21-59.md` | PASS | 0 |

Total blocking findings: **0**. The remediation-loop exit gate is therefore satisfied
(`blocking_count == 0`), and no `remediation-inputs` artifact was produced or required.

## CR-1 (Major, non-blocking) — promoted as #525

`EngineToggleStateCoordinator.ApplyPrimeAsync` can let an in-flight prime overwrite a fresher
toggle-written cache value; the retained prime marker then prevents any re-prime, so the stale
toggle display persists until the next click.

```
mcp__drm-copilot__new_potential_bug_entry(short_name=engine-toggle-prime-last-writer-race)
  -> docs/features/potential/2026-08-08-engine-toggle-prime-last-writer-race.md

mcp__drm-copilot__potential_to_issue(promotion_type=bug, work_mode=minor-audit)
  -> https://github.com/drmoisan/TaskMaster/issues/525
  -> docs/features/potential/promoted/2026-08-08-engine-toggle-prime-last-writer-race.md
```

CR-2 (canceled prime silently blocks re-priming) and CR-3 (the two uncovered defensive-guard lines)
are folded into the same issue, because all three sit in the same type and are testable at the same
seam. The reviewer's recommended one-line fix (`_pressedState.TryAdd` instead of the indexer,
invalidating only on a successful add) is carried into the issue body, together with the explicit
note that it closes prime-vs-toggle but not the residual toggle-vs-toggle double-click interleaving,
which needs write versioning.

`work_mode=minor-audit` was selected because the fix is confined to one production file
(`EngineToggleStateCoordinator.cs`) plus its existing test file.

### Why this was promoted rather than fixed here

Recorded explicitly so a later reader does not read it as an evasion:

- The reviewer dispositioned it **non-blocking** on its merits: the defect is display-only (the
  underlying configuration is always correct), the window is narrow, it self-corrects on the next
  click, and the behavior is strictly better than the merge base, where the toggles never reflected
  engine state at all because the `getPressed` callback never bound.
- It violates no acceptance criterion in `spec.md`.
- The delivery's scope directive is three issues; a production-code change at this point would also
  invalidate the completed audit and require a full re-review cycle.

The tradeoff is stated plainly: this ships a known, tracked, display-only race in newly added code.
If the maintainer prefers it closed before merge, #525 is a one-line production change plus three
tests at an already-established seam.

## CR-4 (Minor) — fixed in place, documentation only

`issue.md` Delivery Note point 3 still described the research section 10 item-2 promotion as
deferred to the orchestrator, although #524 had already been created. Corrected in place: the bullet
now cites #524 with its receipt path, and a new point 4 records the CR-1 promotion as #525. No
production file was touched, so no re-audit is required.

## Full promotion ledger for this delivery

| Source | Item | Issue |
|---|---|---|
| Research section 10, item 1 | Orphan `onAction` callbacks in `RibbonExplorer.xml` | #504 (pre-existing) |
| Research section 10, item 2 | Unguarded `Globals` derefs in `RibbonController.Intelligence.cs` | **#524** (created this session) |
| Research section 10, item 3 | `spec.md` unfilled template | resolved during authoring |
| Phase 5 observation | `WinFormsPumpHost` load flakiness | #511 (pre-existing) |
| Code review CR-1 / CR-2 / CR-3 | Coordinator prime/toggle race, canceled prime, uncovered guard | **#525** (created this session) |

Nothing found during this delivery remains unpromoted.
