# Code Quality Review — efcviewer-folder-tree-percentage (#327)

- Timestamp: 2026-07-16T01-44
- Reviewer: feature-reviewer
- Branch: `feature/efcviewer-folder-tree-percentage-327`
- Base: `origin/epic/folder-tree-percentage-ui-integration` (merge-base `34ed0422`)

## Executive Summary

The code is well-structured and matches repository conventions. Testable behavior is factored into
five focused, host-neutral, well-documented modules; WinForms/COM contact is confined to the exempt
controller and Designer files. Contracts are explicit (null guards, nullable annotations, XML docs),
and the implementation reuses the existing `BrightIdeasSoftware.TreeListView` pattern rather than
hand-rolling tree glyph/hit-test/keyboard logic, consistent with the "simplicity first" and "match
existing style" principles. No blocking code-quality issues were found. Three low-severity /
informational findings are recorded for future consideration; none require remediation for merge.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | QuickFiler/Controllers/EfcFormController.cs | whole file | File is 1122 lines (baseline 1014), exceeding the 500-line limit. Pre-existing over-limit; extended by +108 lines of exempt WinForms/COM wiring for this feature. | Consider extracting cohesive controller regions in a future change; not required for this merge. | General code-change policy caps files at 500 lines; the file was already over the limit before #327, and the increment is necessary host-bound wiring that cannot move to the host-neutral helpers. | `git show HEAD:.../EfcFormController.cs \| wc -l` = 1122; base = 1014. |
| Low | QuickFiler/Controllers/EfcFormController.cs | `DictionaryProbabilitySource` (private nested type) | A pure `FolderPath -> Probability` dictionary wrapper implementing `IFolderProbabilitySource` is marked `[ExcludeFromCodeCoverage]` inside the exempt controller. Its logic (dictionary build + lookup) is host-neutral and unit-testable in principle. | Optional: if the seam projection grows, move it to a host-neutral module with direct tests. As written (a ~15-line trivial adapter fed by the fully-tested `FolderProbabilityAdapter`), leaving it in the exempt controller is acceptable. | The join logic that consumes this source is fully covered by `FolderProbabilityAdapterTests`; the wrapper itself is thin glue over the merged `FolderScore` COM-adjacent surface. | `EfcFormController.cs` diff: `BuildProbabilitySource()` + `DictionaryProbabilitySource`. |
| Info | QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs | `_selectedNode` injection | Test injects the selected `FolderSuggestionNode` via reflection because `TreeListView` cannot select without a native handle in a headless run. The `SelectionChanged -> _selectedNode` wiring is therefore not exercised by this unit test. | Keep as-is; verify the selection wiring via build + manual QA (per the CLAUDE.md coverage exemption). | Reflection injection sets the exact node production caches, so the unit under test (`ExecuteMoves`) reads the same `SelectedFolder` contract; the untested glue is exempt controller code. | Diff of `EfcHomeControllerExecuteMovesTests.cs`. |

## Detailed Observations

### Host-neutral modules (UtilitiesCS/OutlookObjects/Folder/)

- `FolderSuggestionNode.cs`: cohesive data-plus-state model; immutable identity (`FullPath`,
  `DisplayName`, `Kind`), controlled `Depth`/`Children` mutation via `internal` members, clear XML
  docs. Good separation between build-time (`internal AddChild`, `internal set` on `Depth`) and
  render-time (`public` `IsExpanded`, `Probability`) surface.
- `FolderSuggestionTree.cs`: single responsibility (build + project + transition). Hierarchy edges
  are derived by longest-present-prefix within a section, with no ancestor synthesis, matching the
  spec. Banner handling is explicit and never descended into. Null/empty inputs handled. Private
  static helpers (`IsBanner`, `LeafSegment`, `FindLongestPrefixParent`, `AssignDepth`) are small and
  focused. `StringComparison.Ordinal` is used consistently for path comparisons, which is correct for
  folder-path identity.
- `PercentageFormatter.cs`: pure function; `MidpointRounding.AwayFromZero` and
  `CultureInfo.InvariantCulture` are correct choices for a stable whole-number percent; null yields
  an empty string as specified.
- `FolderProbabilityAdapter.cs`: fail-fast null guards; joins by `FullPath` equality; skips banners;
  leaves unmatched folder nodes null. It consumes the source and never recomputes scores, satisfying
  the "consumed, not recomputed" constraint.
- `IFolderProbabilitySource.cs`: narrow single-method seam, documented as the sole coupling point to
  the upstream contract. Interface-only.

### Controller wiring (EfcFormController.cs)

- `ConfigureFolderTreeView`, `BindFolderRows`, `FolderListBox_SelectionChanged`,
  `BuildProbabilitySource`, and `DictionaryProbabilitySource` are thin adapters between the
  `TreeListView` and the host-neutral model. Delegation is clean; no domain logic is duplicated here.
- `BindFolderRows` captures a local `_formViewer` reference before use with a comment explaining the
  concurrent-`Cleanup()` null-safety rationale; the banner guard in the OK path now derives from
  `SelectedFolder` and correctly handles a null selection (`selectedFolder is null || StartsWith("====")`).
- The `Left`/`Right` `KeyDown` branches delegate to the host-neutral `LeftArrow`/`RightArrow` and
  document that the native `TreeListView` performs the visible toggle, keeping the model in sync
  without duplicating the control's behavior.

### Tests

- MSTest + Moq + FluentAssertions, AAA structure, descriptive names, and per-class XML summaries.
- `FolderProbabilityAdapterTests` uses `MockBehavior.Strict` and verifies the banner path is never
  queried (`Times.Never`), which is a precise negative assertion.
- Deterministic; no temp files, network, COM, RNG, or wall-clock usage.

## Overall Code-Review Verdict: PASS

No blocking findings. Blocking count contribution from this artifact: 0.
