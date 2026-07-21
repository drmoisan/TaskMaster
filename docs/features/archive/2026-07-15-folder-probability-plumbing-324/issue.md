# folder-probability-plumbing (Issue #324)

- Date captured: 2026-07-15
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/folder-probability-plumbing/ (Issue #324)
- Epic: folder-tree-percentage-ui (child feature, wave 0)

- Issue: #324
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/324
- Last Updated: 2026-07-15
- Work Mode: full-feature

## Problem / Why

`FolderScorer` and `FolderPredictor` compute a per-folder score for internal ranking, but that
score/probability is never surfaced beyond the scoring layer. The string arrays handed to the
EfcViewer folder `ListBox` (`EfcFormController.cs`, `FolderListBox.DataSource` assignments around
lines 551, 799, 961) and to the QuickFiler folder `ComboBox` carry folder names/paths only.
`FolderScorer.ToArray()` / `ToArray(int)` deliberately discard the score and project only the key.

Two downstream sibling features in the same epic (EfcViewer folder tree + percentage, and
QuickFiler dropdown tree + percentage) need the per-folder probability to render a right-aligned
whole-number percentage. Neither can proceed until the probability is exposed as a stable public
contract. This feature is the prerequisite plumbing.

## Proposed Behavior

Expose the per-folder probability/score that the scoring layer already computes as an explicit,
strongly-typed public contract (for example a folder-with-probability DTO or an added result
field), carrying folder identity plus its probability, without altering ranking order, scores, or
model output. Keep pure scoring logic separate from the presentation adapters. This feature does
NOT change the scoring/ranking algorithm or model output; it only exposes the score already
computed.

## Acceptance Criteria (early draft)

- [ ] A public contract carrying folder identity plus its probability/score is available from the
      scoring layer (`FolderScorer`/`FolderPredictor`).
- [ ] The existing folder-name-only outputs (`ToArray`, `FolderArray`, `FindFolder`) remain
      byte-for-byte unchanged in ordering and content, so current callers are unaffected.
- [ ] Ranking order and score values are provably unchanged (regression tests prove no
      ranking/score regression).
- [ ] New/changed code meets repository coverage thresholds; full C# toolchain is green.

## Constraints & Risks

- `FolderScorer` / `FolderPredictor` are T1-tier scoring code; strict C# toolchain and coverage
  policy apply.
- Not every accumulated score is a probability: Bayesian suggestions store `probability * 1000`,
  while conversation-based and word-sequence suggestions accumulate weighted scores on other
  scales. Research must define precisely what "probability" the contract carries and how it maps
  to a percentage for the downstream renderers.
- Header/separator sentinel strings (e.g. `"========= SUGGESTIONS ========="`) are interleaved
  into the presentation arrays; the contract must distinguish real folders from separators.
- Must not alter compact/expanded UI behavior; this feature stops at the contract boundary.

## Test Conditions to Consider

- [ ] Unit coverage of the new contract projection (folder identity + probability) for Bayesian,
      conversation, and word-sequence suggestion sources.
- [ ] Regression tests proving `ToArray`/`FolderArray` ordering and content are unchanged.
- [ ] Edge cases: empty scorer, ties, "Error" sentinel, separator rows.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/folder-probability-plumbing/` folder from the template
