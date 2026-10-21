# Fail-before exception dossier — the `Keys.Down` decision-pin test

Timestamp: 2026-08-27T09-45

Test: `QuickFiler.Controllers.Tests.QfcCollectionControllerNavigationDigitsTests.RegisterAsyncKeyActions_RegistersExactlyOneDownBoundToSelectNextItemAsync`

Pass-after run: `[P1-T15]`, artifact
`docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/p1-t15-keysdown-pin.2026-08-27T09-45.md`
(`Passed: 1`, `Failed: 0`, EXIT_CODE 0).

WhyFailingRunImpossible: `RegisterAsyncKeyActions` already registers exactly one
`("Collection", Keys.Down)` entry and exactly one `("Collection", Keys.Up)` entry at branch head,
because upstream #468 task `[P1-T2]` deleted `WireUpKeyboardHandler` — the dead method whose seed
registered `Keys.Down` twice, once to `SelectNextItem()` and once to `_parent.ActionOkAsync()`. That
deletion is verified at zero hits by `[P0-T12]`. This feature therefore has no red state to observe
for this test: the ambiguous registration site does not exist on the branch this feature is cut from,
and the plan's decision D-444-A forbids recreating the deleted block in order to remove it. The test
is a pass-after-only decision pin whose purpose is forward-looking: it fails if a future edit
re-introduces a second `Keys.Down` binding on the collection surface.

SearchScope:
- `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/regression-testing/`
- `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/`
- `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/`
- `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/other/`
- `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/issue-updates/`

SearchPatterns:
- `*RegisterAsyncKeyActions_RegistersExactlyOneDownBoundToSelectNextItemAsync*`
- `*-red.*.md`
- `fail-before-exception.*.md`

SearchResult: no failing run for this test exists anywhere in the feature's evidence tree, and none
can exist by construction for the reason recorded above. The only artifacts naming this test are the
passing `[P1-T15]` run and this dossier. The two red artifacts present in the tree,
`p1-t3-444-red.2026-08-27T09-45.md` and (later) the Phase 2 and Phase 3 reds, belong to different
tests.

Output Summary: pass-after-only test with no reachable red state; the absence of a failing run is
explained and is not a gap in the evidence trail.
