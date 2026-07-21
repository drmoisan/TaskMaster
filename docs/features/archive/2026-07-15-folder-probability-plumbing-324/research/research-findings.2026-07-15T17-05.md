# Research Findings — folder-probability-plumbing (Issue #324, epic wave 0)

- Date: 2026-07-15T17-05
- Mode: research only (no code changes)
- Feature: `docs/features/active/2026-07-15-folder-probability-plumbing-324`
- Epic: `folder-tree-percentage-ui` (child 9001; consumers 9002 EfcViewer, 9003 QuickFiler dropdown)
- Target framework verified: `UtilitiesCS` and `QuickFiler` are both `net48` (`v4.8.1`), `LangVersion 12.0`
  (`UtilitiesCS/UtilitiesCS.csproj:24`, `QuickFiler/QuickFiler.csproj:12`).

## 1. Data-flow map: where the score is computed and where it is discarded

### 1.1 Score accumulation (all funnels through `FolderScorer.AddSuggestion(string, long)`)

`FolderScorer` holds `_folderNameScores` as `ScoDictionaryNew<string, long>` (folder path -> score).
Every score source writes through the single seam
`AddSuggestion(string folderPath, long score)` (`FolderScorer.cs:195`), which sums on key collision
(`_folderNameScores[folderPath] += score`). Sources:

- **Bayesian** — `AddBayesianSuggestionsAsync` (`FolderScorer.cs:152-179`):
  `long score = (long)Math.Round(prediction.Probability * 1000, 0)`. `prediction.Probability` is a
  `double` in `[0,1]` (`UtilitiesCS/EmailIntelligence/Bayesian/Prediction.cs:27`). So a Bayesian-only
  score equals `probability * 1000` (0..1000).
- **Conversation** — `AddConversationBasedSuggestions` (`FolderScorer.cs:257-279`):
  `score = Round(Pow(emailCount, LngConvCtPwr) * Conversation_Weight)`. Weighted integer on an
  arbitrary scale, not a 0-1 probability.
- **Word-sequence (Smith-Waterman)** — `AddWordSequenceSuggestions` (`FolderScorer.cs:281-357`) via
  `QuerySubject`/`QueryFolder`/`QueryCombined`: scores are `Pow(...)`-weighted and folder scores are
  squared (`entry.Score = (int)(fldrScore * fldrScore)`), summed per folder. Arbitrary large integers.
- **FolderKey / array / string seeds** — `AddOlFolderKeys`/`AddArray`/`FromArray`/`FromArrayOrString`
  add folders with `score = 0` (`FolderScorer.cs:219`, `FolderScorer.cs:104`).

Different public entry points combine different sources: the async
`RefreshSuggestions(MailItemHelper, ...)` (`FolderScorer.cs:129-150`) uses **Bayesian + conversation**;
the sync `RefreshSuggestions(MailItem, ...)` (`FolderScorer.cs:108-127`) uses
**conversation + folder-keys + word-sequence** (no Bayesian). Consequence: the stored `long` value has
different meaning per path. This is the central semantics constraint (see Section 2).

### 1.2 The exact seams where the score is discarded

- **`FolderScorer.ToArray()` / `ToArray(int topN)`** (`FolderScorer.cs:242-255`): orders
  `OrderByDescending(x => x.Value).ThenBy(x => x.Key, StringComparer.Ordinal)` then
  `Select(x => x.Key)` — the value is dropped here. This is the primary discard seam and the
  smallest-blast-radius place to introduce a score-carrying projection (the ordered sequence already
  exists; only the final `Select` drops the value).
- **`FolderPredictor.AddSuggestions(ref List<string>)`** (`FolderPredictor.cs:698-702`): calls
  `Suggestions.ToArray(5)` and appends the strings under a `"========= SUGGESTIONS ========="` header.
- **`FolderPredictor.FolderArray`** (`FolderPredictor.cs:210-225`) and **`FindFolder(...)`**
  (`FolderPredictor.cs:256-306`): assemble a `List<string>` interleaving separator rows, search
  results, suggestions, and recents — all as bare strings. `TopScore()` (`FolderScorer.cs:235`) is the
  only existing public numeric leak, and only exposes the single max value.

`ScoDictionaryNew<TKey,TValue>` enumerates as `KeyValuePair<TKey,TValue>`
(`UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryNew.cs:21+`), so a
new projection at the `ToArray` seam has direct access to both key and value with no new plumbing.

## 2. Probability semantics (recommendation)

**Finding: there is no single mathematically-true probability across all three sources.** Only the
Bayesian-only path yields a calibrated `[0,1]` value (`Score/1000`), and even that can exceed 1000 once
`AddSuggestion` sums a Bayesian score with a conversation/word-sequence score for the same folder.
Conversation and word-sequence scores are unbounded weighted integers. Any claim that the stored `long`
is a "probability" is only true in the Bayesian-dominant path.

**Recommendation — expose both, normalize in one place:**

1. **Carry the raw `Score` (long) verbatim.** This is the one unambiguous, unchanged fact and is
   exactly the value used for internal ranking. It satisfies the epic NFR ("the probability value
   surfaced to the UI is the same score the scoring layer already computes for internal ranking").
2. **Also expose a normalized `Probability` (double in `[0,1]`) computed by a single deterministic
   function in the scoring layer**, so the two downstream features never re-derive normalization
   (separation of concerns; avoids divergence between 9002 and 9003).
3. **Recommended normalization: max-normalization** — `Probability = Score / TopScore` where
   `TopScore` is the maximum score in the projected set, with a guard returning `0` when `TopScore == 0`
   (empty scorer or all-zero seed folders). Rationale:
   - Rank-preserving (monotonic in `Score`) — provably does not change ordering.
   - Bounded to `[0,1]` for every source and after accumulation (top folder = 100%).
   - Stable per folder regardless of `topN` (unlike sum-normalization).
   - Reuses the concept already surfaced by the existing `TopScore()` method.
   - Interpretable to the user as "relative confidence of this suggestion vs the best suggestion."
   Document explicitly in XML doc that `Probability` is a **relative display value, not a calibrated
   Bayesian posterior**, because the underlying scores are mixed-scale.

**Rejected alternative (kept brief):** sum-of-total normalization (`Score / Σ Score`, percentages sum
to 100%). Rejected because each folder's value shifts with set composition and `topN`, and it reads as
"share of suggestions" rather than "confidence"; it is less intuitive when one folder dominates.
`Score/1000` (literal Bayesian) was also rejected as the contract's normalized value because it is
undefined/invalid for the non-Bayesian paths and can exceed 1 after accumulation — but downstream may
still compute it from the raw `Score` if a future Bayesian-only surface needs it.

## 3. Contract shape (recommendation)

### 3.1 net48 constraint (verified)

No `record`, `record struct`, positional record, or `{ get; init; }` may be used: `net48` has no
`IsExternalInit` and the repo has no polyfill; `init` fails **CS0518** under
`TreatWarningsAsErrors` (agent memory `record-struct-isexternalinit-netfx`; precedents
`UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs` `ResourceTimingRow`,
`TaskMaster/AppGlobals/HookReadinessCoordinator.cs`). Use a plain
`public readonly struct` with a constructor and get-only auto-properties (`{ get; }`).

### 3.2 Recommended: new immutable value type + new methods alongside the existing ones

**Layer 1 — core scored contract on `FolderScorer` (new file, e.g.
`UtilitiesCS/OutlookObjects/Folder/FolderScore.cs`):**

```
public readonly struct FolderScore
{
    public FolderScore(string folderPath, long score, double probability) { ... }
    public string FolderPath { get; }   // folder identity (path), unchanged key
    public long Score { get; }          // raw accumulated ranking score, verbatim
    public double Probability { get; }   // max-normalized [0,1] display value (Section 2)
}
```

New `FolderScorer` members that mirror `ToArray` ordering exactly:

- `public FolderScore[] ToScoredArray()`
- `public FolderScore[] ToScoredArray(int topN)`

**Ordering-parity technique (low risk):** extract the shared ordered enumeration used by both the
existing and new methods, e.g. a private
`IEnumerable<KeyValuePair<string,long>> OrderedScores() => _folderNameScores.OrderByDescending(x => x.Value).ThenBy(x => x.Key, StringComparer.Ordinal);`
Have `ToArray*` project `.Select(x => x.Key)` from it and `ToScoredArray*` project the struct (stamping
`Probability = maxScore == 0 ? 0 : (double)x.Value / maxScore`, where `maxScore` is computed once over
the same set). This guarantees byte-for-byte identical ordering/content for the existing methods while
adding the scored projection, and makes the "no ranking regression" claim structurally provable rather
than merely tested.

**Layer 2 — row model on `FolderPredictor` (so renderers never string-match separators):**

The UI binds a single `DataSource` that interleaves separators + search results + suggestions +
recents. To let 9002/9003 align a percentage to the suggestion rows without fragile `.StartsWith("====")`
matching, add a parallel row-model (new file, e.g.
`UtilitiesCS/OutlookObjects/Folder/FolderRow.cs`):

```
public enum FolderRowKind { Separator, SearchResult, Suggestion, Recent }

public readonly struct FolderRow
{
    public FolderRow(string text, FolderRowKind kind, FolderScore? score) { ... }
    public string Text { get; }            // exact string currently placed in the array
    public FolderRowKind Kind { get; }
    public FolderScore? Score { get; }      // non-null only for Suggestion rows
}
```

New `FolderPredictor` members mirroring the existing string builders:

- `public FolderRow[] FolderRowArray { get; }` (mirrors `FolderArray`)
- `public FolderRow[] FindFolderRows(...)` (same signature as `FindFolder`, mirrors its output)

Only `Suggestion` rows carry a non-null `Score` (sourced from `Suggestions.ToScoredArray(5)`);
separators, search results, and recents carry `null`. The `Text` of each row equals the current string
exactly, so a renderer can consume either the legacy `string[]` or the new `FolderRow[]`.

### 3.3 Existing outputs remain unchanged

`ToArray()`, `ToArray(int)`, `FolderArray`, `FindFolder(...)`, and `IFolderSearchHandler` are **not
modified in shape or output**. All new surface is additive. `IFolderSearchHandler`
(`UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs`) may optionally gain the new members if a
consumer needs them behind the seam, but that is a consumer-driven decision for 9003 and is not required
by this feature.

**Rejected alternative (kept brief):** an "added result field / parallel `long[]`" returned next to the
existing `string[]`. Rejected because parallel arrays are positionally coupled and error-prone across
the separator/section boundaries, and because they do not model the "which rows have a score" question
that both downstream features must answer.

## 4. Caller inventory (proof-of-no-regression targets)

`FolderScorer.ToArray` / `ToArray(int)`:
- Production: `FolderPredictor.AddSuggestions` -> `Suggestions.ToArray(5)` (`FolderPredictor.cs:701`);
  `QfcHighConfidencePreFilter` -> `predictor.Suggestions.ToArray(1)` (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:187`).
- Tests: `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs` (many:
  lines 86, 112, 124, 272, 303, 322, 475, 490, 519, 586, 659, 670);
  `FolderScorerCoverageExpansionTests.cs`.

`FolderPredictor.FolderArray`:
- UI adapters: `QuickFiler/Controllers/EfcFormController.cs:961`
  (`FolderListBox.DataSource = _dataModel.FolderHelper.FolderArray`);
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:170,176`
  (`_folderHandler?.FolderArray?.Length`, `_itemViewer.SetFolderItems(_folderHandler.FolderArray)`);
  legacy `QuickFiler/Legacy/QfcController.cs:701` (`FolderCbo.Items.AddRange(_fldrHandler.FolderArray)`).
- Interface: `IFolderSearchHandler.FolderArray` (`IFolderSearchHandler.cs:16`).
- Tests: `FolderPredictorTests.cs:90,153,179`; `FolderPredictorCoverageExpansionTests.cs:83`.

`FolderPredictor.FindFolder`:
- UI adapters: `QuickFiler/Controllers/EfcDataModel.cs:381` (`FindMatches` wrapper) consumed by
  `EfcFormController.cs:551,795`; `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:166` then
  `SetFolderItems(folders)` at `:173`; legacy `QfcController.cs:1801`.
- Interface: `IFolderSearchHandler.FindFolder` (`IFolderSearchHandler.cs:22`).
- Tests: `FolderPredictorTests.cs:252`; `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs:320`.

`FolderScorer Suggestions` (property):
- Production: `QfcHighConfidencePreFilter.cs:186-187` (`TopScore()` + `ToArray(1)`).
- Interface: `IFolderSearchHandler.Suggestions` (`IFolderSearchHandler.cs:19`).

QuickFiler ComboBox binding seam (relevant to 9003): `IItemViewer.SetFolderItems(string[] items)`
(`QuickFiler/Viewers/IItemViewer.cs:80`), implemented by `ItemViewer.FolderSearch.cs:13`
(`CboFolders.Items.AddRange(items)`). 9003 will need a scored overload/new method here; this feature
only needs to make a scored value reachable, not to change `SetFolderItems`.

All non-test callers are UI adapters that bind a `string[]` to a control; none inspect the score today.
Because the new surface is additive and the existing methods keep identical output, no adapter behavior
changes.

## 5. Separators, sentinels, and edge cases

- **Separator rows** (`"======= SEARCH RESULTS ======="`, `"========= SUGGESTIONS ========="`,
  `"======= RECENT SELECTIONS ========"`): added only by `FolderPredictor` (`FolderPredictor.cs:683,693,700`),
  never held by `FolderScorer`. The Layer-1 `FolderScore[]` contract is inherently separator-free. The
  Layer-2 `FolderRow` model tags them `FolderRowKind.Separator` with `Score = null`, replacing the
  current `.StartsWith("====")` string-matching used in `EfcFormController` selection logic.
- **`"Error"` sentinel**: `AddSuggestion(object,long)` rejects `null` and `"Error"`
  (`FolderScorer.cs:181-193`); `AddArray` rejects arrays whose `[0] == "Error"` (`FolderScorer.cs:209`).
  So `"Error"` never enters `_folderNameScores` and cannot appear in the scored contract. Add an
  explicit regression test asserting this.
- **Ties**: `ThenBy(x => x.Key, StringComparer.Ordinal)` gives deterministic, culture-independent tie
  ordering; the shared `OrderedScores()` helper preserves this for both string and scored projections.
- **Empty scorer**: `ToScoredArray()` returns `Array.Empty<FolderScore>()`; `TopScore()` already returns
  0 (`FolderScorer.cs:235`); the max-normalization guard returns `Probability = 0` and must not divide by
  zero.
- **All-zero seeds** (folder-key/array seeds add `score = 0`): `maxScore == 0`, so every `Probability`
  is 0 — correct (no confidence signal). Renderers should treat 0% / no-score rows per 9002/9003 design.

## 6. T1 testability and coverage approach

`FolderScorer` is fully unit-testable with **no COM**: every source funnels through
`AddSuggestion(string, long)`, and the existing `FolderScorerTests` already drive it directly
(e.g. `scorer.AddSuggestion("Archive\\Finance", 850)`). Recommended strategy (no live Outlook):

- **Ranking/score no-regression (characterization):** add tests asserting
  `ToScoredArray().Select(x => x.FolderPath)` equals `ToArray()` and
  `ToScoredArray(n).Select(x => x.FolderPath)` equals `ToArray(n)` for the same populated scorer,
  including a tie case (two folders with equal score) to lock the ordinal tie-break. Keep the existing
  `ToArray` assertions unchanged as the golden baseline.
- **Scored projection per source scale:** because all three sources write the same `long`, drive
  `AddSuggestion` with representative values and assert `Score` and `Probability`:
  Bayesian scale (e.g. 800 and 1000 -> `Probability` 0.8 and 1.0), conversation weighted integer, and
  word-sequence integer. Add named tests documenting each source's scale (traceability to AC), plus a
  mixed-source accumulation test (same folder summed across sources) confirming `Score` sums and
  `Probability` stays `<= 1` under max-normalization.
- **Edge cases:** empty scorer (empty array, no divide-by-zero), all-zero seeds (all `Probability == 0`),
  `topN` larger than count, `"Error"` rejection.
- **`FolderPredictor` row model:** reuse the existing mocked-Outlook harness in `FolderPredictorTests`
  (`CreateFolder`/`CreateApplication`/`CreateGlobals` + `Suggestions.AddSuggestion`) — these already
  exercise `FolderArray`/`FindFolder` end-to-end with `Mock<Outlook.Application>` and mocked `Folders`,
  so no live COM is needed. Assert `FolderRowArray`/`FindFolderRows` produce the same `Text` sequence as
  the legacy methods, with `Kind` correctly tagged and `Score` non-null only on `Suggestion` rows.
- **Do not** attempt to test `AddBayesianSuggestionsAsync` directly (it constructs
  `OlFolderClassifierGroup(globals).GetFolderPredictorAsync()` — model/COM-bound). Cover the Bayesian
  scale via `AddSuggestion` with `probability*1000` values and document the mapping.

**Coverage:** `FolderScorer`/`FolderPredictor` are T1 scoring code and the new `FolderScore`/`FolderRow`
structs + new methods are new code. The repository has two stated coverage regimes: CLAUDE.md embedded
policy (80% floor, >=90% for new modules/classes/methods) and `.claude/rules/general-unit-test.md`
(>=85% line, >=75% branch uniform across tiers). Meet the stricter bar: aim >=90% line on all new
members with branch coverage of the empty/all-zero/tie/topN paths. All targets are reachable without COM
via the seams above. `FolderScore`/`FolderRow` are value types with only a constructor and get-only
properties (no interface-only exemption needed since they have executable construction paths exercised
by the projection tests).

## 7. Downstream contract sufficiency (9002 and 9003)

Both consumers need, per displayed suggestion: (a) folder identity/path (to build the tree hierarchy and
to resolve the target folder) and (b) a value convertible to a right-aligned whole-number percentage.
The recommended contract supplies both without a second plumbing pass:

- **9002 (EfcViewer `ListBox`)** binds the output of `FindMatches` -> `FindFolder`. Consuming
  `FindFolderRows(...)` gives each row's `Kind` (to skip separators / section headers when computing
  tree nodes and to suppress a percentage on non-suggestion rows) and `Score.Probability` for the
  suggestion rows. Whole-number percentage = `Math.Round(Probability * 100)` in the renderer.
- **9003 (QuickFiler `ComboBox` `CboFolders`)** binds `FolderArray` via `SetFolderItems`. Consuming
  `FolderRowArray` (or a scored overload of `SetFolderItems`) gives the same `Kind` + `Probability`.
- Both can alternatively read raw `Score` (long) if a future design wants a different mapping; exposing
  both future-proofs the contract.

The single normalization decision (Section 2) living in `FolderScorer.ToScoredArray` guarantees 9002 and
9003 render identical percentages for the same suggestion set, satisfying the epic's shared-contract
intent.

## Recommended files to add / touch (additive only)

- **Add** `UtilitiesCS/OutlookObjects/Folder/FolderScore.cs` — `readonly struct FolderScore`.
- **Add** `UtilitiesCS/OutlookObjects/Folder/FolderRow.cs` — `enum FolderRowKind` + `readonly struct FolderRow`.
- **Touch** `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` — extract private `OrderedScores()`;
  add `ToScoredArray()` / `ToScoredArray(int)`; leave `ToArray*` output identical.
- **Touch** `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — add `FolderRowArray` and
  `FindFolderRows(...)` mirroring the existing builders; leave `FolderArray`/`FindFolder` identical.
- **Add tests** in `UtilitiesCS.Test/OutlookObjects/Folder/` (extend `FolderScorerTests` /
  `FolderPredictorTests` or add `FolderScoreContractTests` / `FolderRowTests`).
- Optional: extend `IFolderSearchHandler` only if 9003 needs the scored surface behind the seam.

## Open decisions for spec/plan

1. Confirm max-normalization vs sum-normalization for `Probability` (Section 2 recommends max).
2. Confirm whether Layer-2 (`FolderRow`) is delivered in 9001 or deferred — recommendation: deliver in
   9001 so both consumers avoid separator string-matching and no second plumbing pass is needed.
3. Confirm which coverage regime the gate enforces (80/90 vs 85/75); plan should target the stricter.
