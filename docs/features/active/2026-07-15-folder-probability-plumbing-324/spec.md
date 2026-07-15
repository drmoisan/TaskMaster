# folder-probability-plumbing — Spec

- **Issue:** #324
- **Parent (optional):** epic `folder-tree-percentage-ui` (child feature 9001, wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-15T17-20
- **Status:** Draft
- **Version:** 0.2

## Overview

`FolderScorer` and `FolderPredictor` (`UtilitiesCS/OutlookObjects/Folder/`) compute a per-folder
score for internal ranking, but that score is never surfaced beyond the scoring layer. The string
arrays handed to the EfcViewer folder `ListBox` (`EfcFormController.cs`, `FolderListBox.DataSource`
assignments around lines 551, 799, 961) and to the QuickFiler folder `ComboBox` carry folder
names/paths only. `FolderScorer.ToArray()` / `ToArray(int)` deliberately discard the score and
project only the folder key.

Two downstream sibling features in the same epic — 9002 (EfcViewer folder tree + percentage) and
9003 (QuickFiler dropdown tree + percentage) — need the per-folder score to render a right-aligned
whole-number percentage. Neither can proceed until the score is exposed as a stable public
contract. This feature is the prerequisite plumbing (child 9001).

## Problem Statement

The value used for internal ranking exists inside `FolderScorer` (`_folderNameScores` as
`ScoDictionaryNew<string, long>`, folder path -> accumulated `long` score) but is never emitted
across a module boundary. Every consumer receives a bare `string[]` of folder names/paths, so no
downstream renderer can display a confidence percentage without re-implementing the scoring or
scraping sentinel strings. The scoring/ranking logic is correct and must not change; only the
projection that drops the score needs an additive, score-carrying sibling.

## Non-Goals

This feature is contract plumbing only. It explicitly does **not**:

- Change the scoring or ranking algorithm, the model, or any model output. Raw scores and ordering
  are preserved exactly.
- Modify the existing name-only outputs `FolderScorer.ToArray()` / `ToArray(int)`,
  `FolderPredictor.FolderArray`, or `FolderPredictor.FindFolder(...)` in shape, ordering, or
  content.
- Render any UI, compute any displayed percentage, or alter compact/expanded UI behavior. Rendering
  belongs to downstream features 9002 and 9003.
- Change `IItemViewer.SetFolderItems(string[])` or any UI adapter binding. Making a scored value
  reachable is in scope; changing how a control is bound is not.
- Exercise or alter the COM/Outlook-bound `AddBayesianSuggestionsAsync` path.

## Behavior

Expose the per-folder ranking score and a normalized display value as an explicit, strongly-typed,
additive public contract from the scoring layer, carrying folder identity plus its raw `Score` and a
normalized `Probability`. Keep pure scoring logic separate from presentation adapters. All new
surface is additive; every existing output is preserved byte-for-byte.

## API / Contract Surface

Target framework: `net48` (`v4.8.1`), `LangVersion 12.0` (verified in `UtilitiesCS.csproj` and
`QuickFiler.csproj`). net48 constraint: no `record`, `record struct`, positional record, or
`{ get; init; }` — `net48` has no `IsExternalInit` and the repo has no polyfill, so `init` fails
CS0518 under `TreatWarningsAsErrors`. Use plain `public readonly struct` with a constructor and
get-only auto-properties (`{ get; }`). Precedents: `ResourceTimingRow` in
`UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`.

### Layer 1 — core scored contract on `FolderScorer`

New file `UtilitiesCS/OutlookObjects/Folder/FolderScore.cs`, namespace matching the existing
`FolderScorer` namespace:

```
public readonly struct FolderScore
{
    public FolderScore(string folderPath, long score, double probability);
    public string FolderPath { get; }   // folder identity (path); unchanged key
    public long Score { get; }          // raw accumulated ranking score, verbatim
    public double Probability { get; }  // max-normalized [0,1] relative display value
}
```

New `FolderScorer` members mirroring `ToArray` ordering exactly:

- `public FolderScore[] ToScoredArray()`
- `public FolderScore[] ToScoredArray(int topN)`

Ordering-parity technique (structural guarantee, not test-only): extract the shared ordered
enumeration used by both existing and new methods, e.g. a private
`IEnumerable<KeyValuePair<string,long>> OrderedScores()` returning
`_folderNameScores.OrderByDescending(x => x.Value).ThenBy(x => x.Key, StringComparer.Ordinal)`.
`ToArray*` projects `.Select(x => x.Key)` from it; `ToScoredArray*` projects the struct, stamping
`Probability = maxScore == 0 ? 0 : (double)x.Value / maxScore`, where `maxScore` is computed once
over the same ordered set. This makes the existing methods' output structurally identical while
adding the scored projection.

### Layer 2 — row model on `FolderPredictor`

New file `UtilitiesCS/OutlookObjects/Folder/FolderRow.cs`, same namespace:

```
public enum FolderRowKind { Separator, SearchResult, Suggestion, Recent }

public readonly struct FolderRow
{
    public FolderRow(string text, FolderRowKind kind, FolderScore? score);
    public string Text { get; }        // exact string currently placed in the array
    public FolderRowKind Kind { get; }
    public FolderScore? Score { get; } // non-null only for Suggestion rows
}
```

New `FolderPredictor` members mirroring the existing string builders:

- `public FolderRow[] FolderRowArray { get; }` (mirrors `FolderArray`)
- `public FolderRow[] FindFolderRows(...)` (same signature as `FindFolder`, mirrors its output)

Only `Suggestion` rows carry a non-null `Score` (sourced from `Suggestions.ToScoredArray(5)`);
separators, search results, and recents carry `null`. Each row's `Text` equals the current string
exactly, so a renderer can consume either the legacy `string[]` or the new `FolderRow[]`.
Downstream renderers tag separators by `Kind` rather than `.StartsWith("====")`.

`IFolderSearchHandler` (`UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs`) may optionally
gain the new members behind the seam if a consumer needs them, but that is a consumer-driven
decision for 9003 and is not required by this feature.

## Backward Compatibility (Required)

The following must remain byte-for-byte identical in ordering and content after this change; they
are treated as part of the spec and protected by characterization/regression tests:

- `FolderScorer.ToArray()` and `FolderScorer.ToArray(int)`.
- `FolderPredictor.FolderArray`.
- `FolderPredictor.FindFolder(...)`.
- `IFolderSearchHandler` shape (unless additively extended without changing existing members).

All new surface is additive. No existing caller behavior changes because existing methods keep
identical output and all non-test callers are UI adapters that bind a `string[]` to a control and
do not inspect a score today.

## Probability Semantics and Normalization

There is no single mathematically-true probability across the three score sources. Only the
Bayesian-only path yields a calibrated `[0,1]` value (`Score/1000`), and even that can exceed 1000
once `AddSuggestion` sums a Bayesian score with a conversation or word-sequence score for the same
folder. Conversation and word-sequence scores are unbounded weighted integers. The contract
therefore exposes both facts:

1. **Raw `Score` (long) verbatim** — the exact value used for internal ranking; unambiguous and
   unchanged. Satisfies the epic NFR that the surfaced value is the same score used for ranking.
2. **Normalized `Probability` (double in `[0,1]`)** computed by a single deterministic function in
   the scoring layer, so 9002 and 9003 never re-derive normalization and cannot diverge.

**Normalization rule — max-normalization:** `Probability = Score / TopScore`, where `TopScore` is
the maximum score in the projected set.

- **Zero guard:** when `TopScore == 0` (empty scorer or all-zero seed folders), `Probability = 0`
  for every row; no division by zero occurs.
- **Rank-preserving:** monotonic in `Score`, so ordering is provably unchanged.
- **Bounded:** `[0,1]` for every source and after accumulation (top folder = 1.0 = 100%).
- **Stable per folder** regardless of `topN` (unlike sum-normalization).
- **Tie-break:** `ThenBy(x => x.Key, StringComparer.Ordinal)` gives deterministic,
  culture-independent ordering; the shared `OrderedScores()` helper preserves this identically for
  both the string and scored projections.

`Probability` MUST be documented in XML doc as a **relative display value (relative confidence of
this suggestion vs the best suggestion), not a calibrated Bayesian posterior**, because the
underlying scores are mixed-scale. Downstream may still compute `Score/1000` from the raw `Score`
if a future Bayesian-only surface needs it. Sum-of-total normalization (`Score / Σ Score`) is
rejected: each folder's value shifts with set composition and `topN`.

## Edge Cases

- **Empty scorer:** `ToScoredArray()` returns `Array.Empty<FolderScore>()`; `TopScore()` returns 0;
  no divide-by-zero.
- **All-zero seeds** (folder-key/array/string seeds add `score = 0`): `maxScore == 0`, so every
  `Probability` is 0 (no confidence signal).
- **Ties:** deterministic ordinal tie-break by key, identical across both projections.
- **`"Error"` sentinel:** `AddSuggestion(object,long)` rejects `null` and `"Error"`, and `AddArray`
  rejects arrays whose `[0] == "Error"`, so `"Error"` never enters `_folderNameScores` and cannot
  appear in the scored contract. Covered by an explicit regression test.
- **Separator rows** (`"======= SEARCH RESULTS ======="`, `"========= SUGGESTIONS ========="`,
  `"======= RECENT SELECTIONS ========"`): added only by `FolderPredictor`, never held by
  `FolderScorer`; the Layer-1 `FolderScore[]` contract is inherently separator-free. Layer-2
  `FolderRow` tags them `FolderRowKind.Separator` with `Score = null`.
- **`topN` larger than count:** returns all available rows without error.

## Data & State

No new storage, caching, persistence, or migration. The feature adds read-only projections over
data already held in `FolderScorer._folderNameScores`. No mutable global state is introduced.

## Test & Coverage Requirements

`FolderScorer` / `FolderPredictor` are T1-tier scoring code. All new code is unit-testable without
COM through the `AddSuggestion(string, long)` seam, which every source (Bayesian, conversation,
word-sequence) funnels through.

Required tests:

- **No-regression characterization:** `ToScoredArray().Select(x => x.FolderPath)` equals
  `ToArray()`; `ToScoredArray(n).Select(x => x.FolderPath)` equals `ToArray(n)`, including a tie
  case that locks the ordinal tie-break. Keep existing `ToArray` assertions as the golden baseline.
- **Scored projection per source scale** via `AddSuggestion`: Bayesian scale (e.g. 800 and 1000 ->
  `Probability` 0.8 and 1.0), conversation weighted integer, word-sequence integer, plus a
  mixed-source accumulation test (same folder summed across sources) confirming `Score` sums and
  `Probability <= 1` under max-normalization. Do NOT exercise `AddBayesianSuggestionsAsync`
  directly (model/COM-bound); cover the Bayesian scale via `AddSuggestion` with `probability*1000`
  values and document the mapping.
- **Edge cases:** empty scorer (empty array, no divide-by-zero), all-zero seeds (all
  `Probability == 0`), `topN` larger than count, `"Error"` rejection.
- **`FolderPredictor` row model:** reuse the existing mocked-Outlook harness in
  `FolderPredictorTests` (`CreateFolder`/`CreateApplication`/`CreateGlobals` +
  `Suggestions.AddSuggestion`). Assert `FolderRowArray` / `FindFolderRows` produce the same `Text`
  sequence as the legacy methods, with `Kind` correctly tagged and `Score` non-null only on
  `Suggestion` rows.

**Coverage regime (stricter of the two):** the repository states CLAUDE.md embedded policy
(80% floor, >= 90% for new modules/classes/methods) and `.claude/rules/general-unit-test.md`
(>= 85% line, >= 75% branch uniform across tiers). Meet the stricter bar: aim >= 90% line on all
new members with branch coverage of the empty / all-zero / tie / `topN` paths, and do not reduce
coverage on any changed line. No production file may be excluded from coverage measurement.

## Toolchain (Definition of Done)

Full C# toolchain green in this order (restart on any change): `csharpier .` -> analyzer build
(`EnableNETAnalyzers` + `EnforceCodeStyleInBuild`) -> nullable/type build
(`Nullable=enable` + `TreatWarningsAsErrors`) -> `vstest.console.exe ... /EnableCodeCoverage`.
MSTest + Moq + FluentAssertions.

## Acceptance Criteria

- [ ] A new immutable `public readonly struct FolderScore` exists in
      `UtilitiesCS/OutlookObjects/Folder/FolderScore.cs` with get-only `FolderPath` (string),
      `Score` (long), and `Probability` (double) and a constructor (net48-safe: no record/init).
- [ ] `FolderScorer.ToScoredArray()` and `ToScoredArray(int topN)` exist and return
      `FolderScore[]`.
- [ ] `ToScoredArray().Select(x => x.FolderPath)` ordering equals `ToArray()` ordering, and
      `ToScoredArray(n)` ordering equals `ToArray(n)` ordering, including a tie case (regression
      test).
- [ ] Existing `FolderScorer.ToArray()` / `ToArray(int)` output is unchanged byte-for-byte in
      ordering and content (regression test with a golden baseline).
- [ ] `FolderPredictor.FolderArray` and `FolderPredictor.FindFolder(...)` output is unchanged
      byte-for-byte in ordering and content (regression test).
- [ ] `Probability` is max-normalized (`Score / TopScore`) and always in `[0,1]`, with a zero-guard
      returning 0 when `TopScore == 0` (empty scorer and all-zero-seed tests prove no
      divide-by-zero).
- [ ] Scored projection is verified across all three score sources (Bayesian, conversation,
      word-sequence) via the `AddSuggestion` seam, plus a mixed-source accumulation case; the
      COM-bound `AddBayesianSuggestionsAsync` path is not exercised directly.
- [ ] A `FolderRow` row model (`readonly struct FolderRow` with `Text`, `Kind`, nullable `Score`)
      and `enum FolderRowKind { Separator, SearchResult, Suggestion, Recent }` exist, with
      `FolderPredictor.FolderRowArray` and `FindFolderRows(...)`; `Text` matches the legacy string
      output, `Kind` is correctly tagged, and `Score` is non-null only on `Suggestion` rows.
- [ ] The `"Error"` sentinel never appears in the scored contract (regression test).
- [ ] Downstream contract sufficiency is documented: 9002 and 9003 can render a whole-number
      percentage from `Probability` (`Math.Round(Probability * 100)`) and skip non-suggestion rows
      via `Kind`, without a second plumbing pass.
- [ ] `Probability` XML documentation states it is a relative display value, not a calibrated
      Bayesian posterior.
- [ ] New/changed code meets the stricter repository coverage regime (>= 90% line on new members;
      branch coverage of empty / all-zero / tie / `topN` paths; no reduction on changed lines).
- [ ] Full C# toolchain is green (csharpier -> analyzer build -> nullable/type build -> vstest with
      code coverage), reported with the exact commands run.

## Seeded Test Conditions (from potential)
- [ ] Unit coverage of the new contract projection (folder identity + probability) for Bayesian,
      conversation, and word-sequence suggestion sources.
- [ ] Regression tests proving `ToArray`/`FolderArray` ordering and content are unchanged.
- [ ] Edge cases: empty scorer, ties, "Error" sentinel, separator rows.
