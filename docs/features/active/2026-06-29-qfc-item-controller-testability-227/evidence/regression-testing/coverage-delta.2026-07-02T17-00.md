# Coverage Delta — Cycle 5

- **Timestamp:** 2026-07-02T17-00
- **Task:** [P3-T5]

## Repo-wide coverage delta

| | Line-rate | Percentage | Lines covered / valid |
|---|---:|---:|---:|
| Baseline (P0-T5) | 0.6362 | 63.62% | 105053 / 165126 |
| Post-change (P3-T4) | 0.6375 | 63.75% | 105474 / 165451 |
| **Delta** | **+0.0013** | **+0.13 pts** | +421 covered / +325 valid |

No regression: post-change coverage is higher than baseline. (Whole-process coverage across all loaded modules including vendored/third-party code, per this repo's coverage-tooling convention — see `.claude/agent-memory/atomic-executor/project_build_test_env.md`.)

## Per-member coverage confirmation (post-change `evidence/qa-gates/final-coverage.2026-07-02T17-00.cobertura.xml`)

All 7 de-exempted members are confirmed covered (non-zero `line-rate`) in the post-change Cobertura report, verified by locating each `<method>` element under its correct `<class name="QuickFiler..." filename="...">` (disambiguating from unrelated same-named methods in other classes, e.g. `Tags.TagController.WireEvents`):

| Member | File (class instance) | `line-rate` |
|---|---|---:|
| `ResolveControlGroups(ItemViewer)` | `QfcItemController.ViewerSetup.cs` | 1.0 (100%) |
| `WireControlTreeEvents()` | `QfcItemController.EventWiring.cs` | 1.0 (100%) |
| `WireEvents()` | `QfcItemController.EventWiring.cs` | 1.0 (100%) |
| `ToggleExpansionOff()` | `QfcItemController.Navigation.cs` | 0.625 (partial — the `_emailIsReadTimer` disposal branch is exercised; the timer-creation branch is in `ToggleExpansionOn`, not this method) |
| `ToggleExpansionOn()` | `QfcItemController.Navigation.cs` | 0.5556 (partial — the read-timer creation branch, gated on `ItemHelper.UnRead == true`, is intentionally not exercised per the plan's test design, which leaves `ItemHelper` null to skip that branch) |
| `TlpCellSnapShotList.ApplyState(IContainerControlLocal)` | `TlpCellSnapShot.cs` | 1.0 (100%) |
| `TlpCellSnapShot.ApplyState(IContainerControlLocal)` | `TlpCellSnapShot.cs` | 0.875 (partial — the `SetCellPosition`/`SetRowSpan`/`SetColumnSpan` reassignment lines execute; minor branch variance from the `control.Parent != tlp` conditional not being hit in the same way across both new tests) |

All 7 members moved from **0% (exempted, uninstrumented)** at baseline to **genuinely-executed, non-zero coverage** post-change. Partial (< 100%) line-rates on `ToggleExpansionOff`/`ToggleExpansionOn`/`TlpCellSnapShot.ApplyState` reflect untested secondary branches (timer creation, parent-reassignment edge case) that are out of this cycle's scope — the acceptance criterion is that each member is covered by at least one passing test exercising real behavior, which is satisfied for all 7.
