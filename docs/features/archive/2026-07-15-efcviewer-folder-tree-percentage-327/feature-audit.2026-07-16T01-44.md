# Feature Acceptance Audit — efcviewer-folder-tree-percentage (#327)

- Timestamp: 2026-07-16T01-44
- Reviewer: feature-reviewer
- Branch: `feature/efcviewer-folder-tree-percentage-327`
- Base: `origin/epic/folder-tree-percentage-ui-integration` (merge-base `34ed0422`)
- Work mode: `full-feature` -> AC sources: `spec.md` (## Acceptance Criteria) and `user-story.md` (## Acceptance Criteria)

## Scope and Baseline

Audit scope is the full branch diff against the resolved base branch. The two AC lists (`spec.md`
items 1-8 and `user-story.md` items 1-7) are equivalent; spec items 1-7 map one-to-one to user-story
items 1-7, and spec item 8 (toolchain green) has no separate user-story counterpart. Verification
follows the CLAUDE.md model: host-neutral behavior is proven by unit tests; coverage-exempt
WinForms/COM wiring is verified by a green build plus documented manual QA. Executor check-offs were
independently re-verified against the delivered code, tests, and committed evidence rather than
trusted on their face.

## Acceptance Criteria Inventory

Source `spec.md`:
1. Folders containing subfolders render with a plus/minus expand affordance.
2. Mouse click on the plus expands / minus collapses (reveals children / hides descendants).
3. With a node highlighted, right arrow expands and left arrow collapses.
4. Each suggestion shows its prediction probability right-aligned in whole-number percent.
5. Probability consumed from upstream `folder-probability-plumbing` (path -> double `[0,1]`), not
   recomputed; rows with no probability render blank.
6. Behavior delivered in BOTH viewers `EfcViewer.cs` and `EfcViewer3.cs`.
7. Shared host-neutral logic factored into a reusable testable helper meeting coverage thresholds.
8. Full C# toolchain (csharpier, analyzers, nullable, MSTest+Moq+FluentAssertions) is green.

Source `user-story.md`: items 1-7 mirror `spec.md` items 1-7.

## Acceptance Criteria Evaluation

| # | Criterion | Verdict | Evidence / Rationale |
|---|---|---|---|
| 1 | Plus/minus expand affordance for folders with subfolders | PASS | Hierarchy + `HasChildren` derived in `FolderSuggestionTree.BuildFromRows`, tested (`FolderSuggestionTreeHierarchyTests`: nested-prefix, deep-without-parent, per-section isolation). Glyph is supplied by `TreeListView.CanExpandGetter/ChildrenGetter` wired in `EfcFormController.ConfigureFolderTreeView` and the Designer `TreeListView` (exempt; build + manual QA). |
| 2 | Mouse expand/collapse | PASS | Native `TreeListView` expand/collapse via the wired getters (exempt; build + manual QA). Host-neutral toggle semantics tested in `FolderSuggestionTreeStateTests.Toggle_*` (both directions; leaf/banner no-ops). |
| 3 | Right arrow expands, left arrow collapses highlighted node | PASS | Host-neutral `RightArrow`/`LeftArrow` no-op rules tested (`RightArrow_ExpandsCollapsedExpandableRoot`, `LeftArrow_CollapsesExpandedRoot`, leaf / already-expanded / already-collapsed / banner / null no-ops). Wired in `EfcFormController.FolderListBox_KeyDown` Left/Right branches (exempt; build + manual QA). |
| 4 | Right-aligned whole-number percent per suggestion | PASS | `PercentageFormatter.FormatPercent` tested for 0, 1, typical, below-midpoint, at-midpoint away-from-zero, small-midpoint, and null (`PercentageFormatterTests`). Right alignment via `olvColumnPercent.TextAlign = Right` in both Designers (exempt; build + manual QA). |
| 5 | Probability consumed not recomputed; blank when absent | PASS | `FolderProbabilityAdapter.Apply` join tested (matched / unmatched-null / banner-never-queried / nested) in `FolderProbabilityAdapterTests`. Contract re-confirmed in `evidence/other/upstream-9001-contract-reconfirm.md` (merged `FolderScore.Probability` is `double [0,1]` keyed by `FolderPath`). Production `DictionaryProbabilitySource` in `EfcFormController` projects `ToScoredArray()` into the seam; separators/search/recents carry null score -> blank cell. No recomputation. |
| 6 | Delivered in BOTH viewers | PASS | Both `EfcViewer.Designer.cs` and `EfcViewer3.Designer.cs` replace the flat `ListBox` with the two-column `TreeListView` (folder + right-aligned percent); `EfcViewer3.cs` received `[ExcludeFromCodeCoverage]` (P4-T1) and `EfcViewer.cs` already carried it. The shared `EfcFormController` drives both (EfcViewer3 exposes `SetController(EfcFormController)`). `EfcViewer.cs` required no source edit because the control change is in its Designer and the behavior is wired through the shared controller. Exempt; verified by green build (solution compiles) + manual QA. |
| 7 | Shared host-neutral helper meets coverage thresholds | PASS | Five modules under `UtilitiesCS/OutlookObjects/Folder/`; per-module coverage 96.43%-100% line (>=90% new-code target met) per `evidence/qa-gates/phase5-final-tests-coverage.md` and `phase5-coverage-delta.md`. `IFolderProbabilitySource` is interface-only (no executable lines). |
| 8 | Full C# toolchain green | PASS | `phase5-final-csharpier.md` (exit 0, no diffs), `phase5-final-analyzers.md` (0/0), `phase5-final-nullable.md` (0/0), `phase5-final-tests-coverage.md` (4762/4762 passed). |

Specific scrutiny items requested by the caller:

- WinForms/Designer/controller exemption (item 1): `EfcViewer.cs` retains `[ExcludeFromCodeCoverage]`
  (line 20), `EfcViewer3.cs` now carries it (line 15), and `EfcFormController.cs` retains it
  (line 26). The Designer files are partials of the exempt Form-derived types and inherit the
  attribute. No testable host-neutral logic was hidden inside an exempt file: the shared logic lives
  in the UtilitiesCS helpers; the exempt controller contains only wiring plus the trivial
  `DictionaryProbabilitySource` seam (noted as Low in the code review). Verdict: PASS.
- Out-of-scope-lock test edit (item 2): assessed acceptable and mechanically necessary; the reflection
  workaround does not weaken the `ExecuteMoves` assertion. See policy-audit section 5. Verdict: PASS.
- Banned APIs (item 3): none present in touched production files. Verdict: PASS.
- Determinism / test policy (item 4): new tests use MSTest + Moq + FluentAssertions, deterministic,
  no temp files / COM / network / wall-clock. Verdict: PASS.
- Upstream 9001 contract reconciliation (item 5): the adapter/seam consume the merged
  `FolderScore`/`FolderRow` surface by full-path key and do not recompute scores;
  `evidence/other/upstream-9001-contract-reconfirm.md` confirms no seam/adapter/formatter change was
  required. Verdict: PASS.

## Definition of Done Cross-check (spec.md)

The spec DoD item "Behavior matches acceptance criteria in both viewers" (spec.md line 163) remains
unchecked. This is the manual-QA confirmation of the coverage-exempt WinForms/COM runtime wiring
(plus/minus glyphs, mouse expand/collapse, arrow-key behavior, right-aligned percentage in the
running add-in). It is consistent with the CLAUDE.md verification model for exempt UI wiring and is
not itself an acceptance criterion. It does not block the AC verdicts, which are satisfied by unit
tests plus a green build; it is carried forward as the manual-QA action for the running add-in.

## Acceptance Criteria Check-off

All acceptance criteria in `spec.md` and `user-story.md` are evaluated PASS and were already checked
`[x]` by the executor. The reviewer independently verified each item (code + tests + evidence) and
confirms the check-offs stand; no box required changing and none were left incorrectly checked.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/spec.md` and `.../user-story.md`
- Total AC items: 8 (spec) + 7 (user-story) = 15, mapping to 8 distinct criteria
- Checked off (delivered and verified): 15 / 15
- Remaining (unchecked): 0
- Items remaining: none

## Summary

All acceptance criteria PASS. Host-neutral behavior is proven by deterministic unit tests at
96.43%-100% coverage; the coverage-exempt WinForms Designer/Form and controller wiring is verified by
a green build and remains subject to the documented manual QA of the running add-in. No PARTIAL,
FAIL, or UNVERIFIED items. Blocking count contribution from this artifact: 0.
