# `folder-probability-plumbing` — User Story

- Issue: #324
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-15T17-20
- Epic: `folder-tree-percentage-ui` (child 9001, wave 0)

## Story Statement

- As a developer of feature 9002 (EfcViewer folder tree + percentage), I want the folder scoring
  layer to hand me each suggestion's identity plus a normalized `[0,1]` value and a way to tell
  suggestion rows apart from separators, so that I can render a right-aligned whole-number
  percentage next to each suggested folder without re-implementing scoring or string-matching
  sentinel rows.
- As a developer of feature 9003 (QuickFiler dropdown tree + percentage), I want the same scored
  contract from the same normalization point, so that the QuickFiler `ComboBox` displays identical
  percentages to the EfcViewer `ListBox` for the same suggestion set.
- As an end user filing email, I want to see a confidence percentage beside each suggested folder,
  so that I can judge at a glance how strongly the system recommends a destination.

## Problem / Why

`FolderScorer` and `FolderPredictor` compute a per-folder score for internal ranking, but that
score is never surfaced beyond the scoring layer. The string arrays handed to the EfcViewer folder
`ListBox` (`EfcFormController.cs`, `FolderListBox.DataSource` assignments around lines 551, 799,
961) and to the QuickFiler folder `ComboBox` carry folder names/paths only. `FolderScorer.ToArray()`
/ `ToArray(int)` deliberately discard the score and project only the key.

Two downstream sibling features in the same epic (EfcViewer folder tree + percentage, and
QuickFiler dropdown tree + percentage) need the per-folder score to render a right-aligned
whole-number percentage. Neither can proceed until the score is exposed as a stable public
contract. This feature is the prerequisite plumbing.

## Personas & Scenarios

- **Persona: downstream feature developer (9002 / 9003).**
  - Who: a C# developer building the folder-tree-with-percentage UI in EfcViewer or QuickFiler.
  - What they care about: a stable, strongly-typed contract that carries folder identity plus a
    display value, and a reliable way to distinguish suggestion rows from separator/section rows.
  - Constraints: cannot change the scoring algorithm; must not diverge from the sibling feature's
    percentage; T1-tier code with strict toolchain and coverage.
  - Goals and frustrations: today the only cross-boundary output is a bare `string[]`; deriving a
    percentage would require re-scoring or brittle `.StartsWith("====")` matching, and two features
    would risk computing different percentages.

- **Scenario: rendering a suggestion percentage in EfcViewer (9002).**
  - Who is acting: the 9002 renderer bound to `FindMatches` -> `FindFolder`.
  - Trigger: the user opens the folder picker for a mail item.
  - Steps: 9002 calls `FindFolderRows(...)` instead of `FindFolder(...)`; for each `FolderRow` it
    checks `Kind`, skips `Separator` / non-suggestion rows, and for `Suggestion` rows reads
    `Score.Probability` and renders `Math.Round(Probability * 100)` right-aligned.
  - Obstacles/decisions: separators and section headers must be excluded from percentage display;
    `Kind` resolves this without string-matching.
  - Expected outcome: each suggested folder shows a whole-number percentage; ordering and folder
    text are identical to the current name-only list.

- **Scenario: matching percentages in QuickFiler (9003).**
  - Who is acting: the 9003 renderer bound to `FolderArray` via `SetFolderItems`.
  - Trigger: the user opens the QuickFiler destination dropdown.
  - Steps: 9003 consumes `FolderRowArray` (or a future scored overload of `SetFolderItems`), reads
    the same `Kind` + `Probability` produced by the single normalization point in
    `FolderScorer.ToScoredArray`.
  - Expected outcome: the QuickFiler dropdown shows the same percentage for a given suggestion set
    as the EfcViewer list, because both read the same normalized value.

## Non-Goals

- No change to the scoring/ranking algorithm, the model, or any model output; raw scores and
  ordering are preserved exactly.
- No change to existing name-only outputs (`ToArray`, `ToArray(int)`, `FolderArray`, `FindFolder`).
- No UI rendering, percentage formatting, or compact/expanded UI behavior change in this feature;
  those belong to 9002 and 9003.
- No change to `IItemViewer.SetFolderItems(string[])` or any control binding.
- No direct exercise of the COM/Outlook-bound `AddBayesianSuggestionsAsync` path.

## Acceptance Criteria / Done When

- [x] A new immutable `FolderScore` value type (`FolderPath`, `Score` long, `Probability` double) is
      available from the scoring layer (net48-safe `readonly struct`, no record/init).
- [x] `FolderScorer.ToScoredArray()` and `ToScoredArray(int)` return `FolderScore[]` whose folder
      ordering matches `ToArray()` / `ToArray(int)` exactly, including ties (ordinal tie-break).
- [x] Existing outputs `ToArray`, `ToArray(int)`, `FolderArray`, and `FindFolder(...)` remain
      byte-for-byte unchanged in ordering and content (regression tests).
- [x] `Probability` is max-normalized to `[0,1]` with a zero-guard (empty scorer and all-zero seeds
      yield `Probability = 0`, no divide-by-zero) and is documented as a relative display value, not
      a calibrated Bayesian posterior.
- [x] The scored projection is verified across Bayesian, conversation, and word-sequence sources
      (plus mixed-source accumulation) via the `AddSuggestion` seam, without exercising the COM-bound
      Bayesian path directly.
- [x] A `FolderRow` row model with `FolderRowKind { Separator, SearchResult, Suggestion, Recent }`
      is available via `FolderPredictor.FolderRowArray` and `FindFolderRows(...)`; `Text` matches the
      legacy string output, `Kind` is correctly tagged, and `Score` is non-null only on `Suggestion`
      rows, so downstream renderers tag separators by `Kind` rather than `.StartsWith("====")`.
- [x] The `"Error"` sentinel never appears in the scored contract (regression test).
- [x] The contract is documented as sufficient for 9002 and 9003 to render a whole-number
      percentage (`Math.Round(Probability * 100)`) and skip non-suggestion rows via `Kind`, from a
      single shared normalization point, with no second plumbing pass.
- [x] New/changed code meets the stricter repository coverage regime and the full C# toolchain is
      green.
