# [P3-T7] Phase 3 commit — issue #286

Timestamp: 2026-08-26T09-57

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs \
           docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md \
           docs/features/active/qfc-collection-controller-defects-468/spec.md \
           docs/features/active/qfc-collection-controller-defects-468/evidence/
git commit -m "fix(286): restore the reentrancy counter on the exceptional exit path"
git show --name-only HEAD
```

EXIT_CODE: 0

## Output Summary

Commit `fbe5b3a6864a5571ef29238f30691db28f9af2d2`
`fix(286): restore the reentrancy counter on the exceptional exit path`

### Acceptance verification

`git show --name-only HEAD | grep -E '\.(cs|csproj)$'` returns exactly two paths and no others:

```
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
QuickFiler/Controllers/QfcCollectionController.cs
```

That is precisely the pair P3-T7 requires: `<CTRL>` and the Phase 2 defect test file. No `.csproj`
changed, because both files were already registered as `Compile Include` entries in Phase 2.

Per D15 and the P0-T16 / P1-T9 / P2-T12 precedent, the commit also carries the plan checklist,
`spec.md` (for the AC check-off), this phase's evidence artifacts, and the P2-T12 commit artifact
that could only be written after the Phase 2 commit existed.

`.claude/agent-memory/**` and `.claude/state/**` remain unstaged; every `git add` used an explicit
pathspec.

### Acceptance criteria checked off in this commit

**AC-1 (#286)** — marked `[x]` in `spec.md`. Every clause is positively verified:

| Clause | Evidence |
|---|---|
| The decrement executes on the exceptional exit path as well as the normal one | P3-T4 moved it into a `finally`; verified structurally (exactly one `finally`, containing the decrement, with the increment outside the `try`) |
| The counter returns to its pre-call value after a throw | P3-T5 TRX, passed 2 / failed 0, against the P3-T2 and P3-T3 red states that each observed `1` |
| Two named MSTest tests, one throwing at the first statement after the increment | `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter`, driven by `UnregisterNavigation()` on a `null` `_itemGroups` |
| ... and one throwing later in the body | `RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter`, driven by a mocked `IsActiveUI` getter after `UnregisterNavigation` completes |
| Each reads the static field by reflection | both call `ReadReentrancyCounter()`, which uses `BindingFlags.NonPublic | BindingFlags.Static` and asserts the field was found |
| Each resets it in `[TestInitialize]`/`[TestCleanup]` | `ResetReentrancyCounterBeforeTest` and `ResetReentrancyCounterAfterTest` (P3-T1), both using the same binding flags and both asserting the field was found before writing `0` |

Result: PASS.
