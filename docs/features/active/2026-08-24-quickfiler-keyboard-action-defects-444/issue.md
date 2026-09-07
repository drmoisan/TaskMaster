# quickfiler-keyboard-action-defects (Issue #444)

- Issue: #444
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/444
- Additional issues closed by this feature: #472, #482
- Type: bug
- Work Mode: full-bug
- Parent: epic `quickfiler-bug-family` (integration branch `epic/quickfiler-bug-family-integration`)
- Wave: 1
- Upstream dependency: #468 (`qfc-collection-controller-defects`) — prepared and merged
- Last Updated: 2026-08-24

> **Acceptance-criteria authority.** Work mode is `full-bug`. Under
> `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `spec.md` is the **sole** acceptance-criteria
> source for this work mode, and `user-story.md` is intentionally absent (reported as `NONE`). The
> acceptance-criteria section in this file is a pointer only; the binding checklist lives in
> `spec.md`.

## Promotion Provenance

All three issues closed by this feature were promoted before this run. No new potential entry and no
new GitHub issue was created here. Only `new_active_feature_folder` was invoked.

| Issue | Authoritative requirement document (on `origin/main`) | State |
| --- | --- | --- |
| #444 | `docs/features/potential/promoted/2026-08-07-kbdactions-enumerable-ctor-bypasses-duplicate-guard.md` | pre-existing, already promoted |
| #472 | `docs/features/potential/promoted/2026-08-07-qfc-collection-navigation-digits-desync.md` | pre-existing, already promoted |
| #482 | `docs/features/potential/promoted/2026-08-07-qfc-item-controller-expansion-registry-divergence.md` | pre-existing, already promoted |

## Summary

Three keyboard-action defects in the QuickFiler surface share one root cause family: the
`KbdActions<TKey, UClass, VDelegate>` registry admits inconsistent `(SourceId, Key)` state through
entry points that do not agree with one another, and every call site discards `Remove`'s `bool`
result, so a divergence stays silent until a later `Add` or `Find` throws.

- **#444** — the `KbdActions(IEnumerable<UClass>)` constructor bypasses the duplicate guard that
  `Add` enforces, and production seeds a duplicate `("Collection", Keys.Down)` pair through it, so
  `Find(Keys.Down)` resolves against a two-element match set and throws `InvalidOperationException`.
- **#472** — `QfcCollectionController.RegisterNavigation` captures the side-effecting `Digits`
  property once while `UnregisterNavigation` re-evaluates it per loop iteration, so a collection that
  crosses the 10-item boundary between the two calls unregisters keys under a different digit width
  than it registered them, leaving orphaned registrations that later collide.
- **#482** — `QfcItemController`'s synchronous and asynchronous expansion paths maintain disjoint
  `'B'`/`'D'` registries behind a single shared `_expanded` flag, so interleaving the two paths drives
  flag and registries out of agreement and the next registration throws `ArgumentException`.

## File Ownership

**Owned by this feature:**

- `QuickFiler/Controllers/KbdActions.cs`
- `QuickFiler/Controllers/QfcItemController.Navigation.cs`
- the regions of `QuickFiler/Controllers/QfcCollectionController.cs` required by #444 and #472

**Must not be written (owned by siblings):**

- `QuickFiler/Controllers/KeyboardHandler.cs` — sibling #498
- every other `QfcItemController` partial — siblings #484 and #489
- `QuickFiler/Interfaces/IQfcCollectionController.cs` — sibling #468

A fix that appears to require one of the above is recorded as a cross-feature note in `spec.md` and
kept out of the plan.

## Downstream Consumers

Siblings #464 and #489 are authored against this feature's contract for
`QuickFiler/Controllers/KbdActions.cs` and `QuickFiler/Controllers/QfcItemController.Navigation.cs`.
Every signature or behaviour change to those two files is stated explicitly in `spec.md`.

## Acceptance Criteria

See the `## Acceptance Criteria` section of `spec.md`, which is the sole binding checklist for this
`full-bug` work mode.
