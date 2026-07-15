# folder-probability-plumbing - Plan

- **Issue:** #324
- **Parent:** epic `folder-tree-percentage-ui` (child feature 9001, wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-15T16-52
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature

## Required References

All work must comply with the following policies; their content is not duplicated here.

- `CLAUDE.md` (C# toolchain order, MSTest/Moq/FluentAssertions, coverage regime).
- `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`.
- Skills: `policy-compliance-order`, `atomic-plan-contract`, `evidence-and-timestamp-conventions`, `acceptance-criteria-tracking`.

## AC sources (full-feature)

Acceptance criteria are drawn from `spec.md` (`## Acceptance Criteria`, 13 items — authoritative)
and `user-story.md` (`## Acceptance Criteria / Done When`, 8 items — aligned). Spec AC labels used
below are AC1..AC13 in the spec list order; user-story items are covered by the same tasks.

- AC1 `FolderScore` readonly struct (net48-safe) in `FolderScore.cs`.
- AC2 `FolderScorer.ToScoredArray()` / `ToScoredArray(int topN)` return `FolderScore[]`.
- AC3 `ToScoredArray` ordering equals `ToArray` ordering incl. tie case (regression).
- AC4 `ToArray()` / `ToArray(int)` unchanged byte-for-byte (golden baseline regression).
- AC5 `FolderArray` / `FindFolder(...)` unchanged byte-for-byte (regression).
- AC6 `Probability` max-normalized `[0,1]` with zero-guard (empty + all-zero prove no divide-by-zero).
- AC7 Scored projection verified across Bayesian/conversation/word-sequence via `AddSuggestion` + mixed-source accumulation; `AddBayesianSuggestionsAsync` not exercised.
- AC8 `FolderRow` + `FolderRowKind` + `FolderRowArray` + `FindFolderRows(...)`; `Text` matches legacy, `Kind` tagged, `Score` non-null only on `Suggestion`.
- AC9 `"Error"` sentinel never appears in the scored contract (regression).
- AC10 Downstream contract sufficiency documented (9002/9003 render `Math.Round(Probability * 100)` and skip non-suggestion rows by `Kind`).
- AC11 `Probability` XML doc states relative display value, not a calibrated Bayesian posterior.
- AC12 New/changed code meets the stricter repository coverage regime (>= 90% line on new members; branch coverage of empty/all-zero/tie/topN paths; no reduction on changed lines).
- AC13 Full C# toolchain green (csharpier -> analyzer build -> nullable/type build -> vstest with code coverage), reported with exact commands.

## Evidence Location Invariant

All evidence artifacts resolve under the canonical scheme
`docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/<kind>/`
(`baseline/`, `qa-gates/`, `regression-testing/`, `other/`). Writing evidence under `artifacts/` is a
policy violation. Each command-step artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, and
`Output Summary:`.

## C# toolchain commands (canonical order)

1. Format: `dotnet tool run csharpier .`
2. Analyze: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. Type-check (nullable): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. Test (coverage): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`

Legacy-project note: `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` are non-SDK projects with
explicit `<Compile Include>` entries and no glob; every new `.cs` file MUST be wired with an explicit
`<Compile Include>` entry or it will not compile.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Policy Reads and Baseline Capture

- [ ] [P0-T1] Read the policy files in the `policy-compliance-order` sequence (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/csharp.md` if present) and record the read
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact contains `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [ ] [P0-T2] Capture baseline formatting state by running `dotnet tool run csharpier .` on the clean worktree
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/baseline/baseline-csharpier.md`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (files changed / already-formatted count).
- [ ] [P0-T3] Capture baseline analyzer build by running the analyzer msbuild command
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/baseline/baseline-analyzer-build.md`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (warning/error counts).
- [ ] [P0-T4] Capture baseline nullable/type build by running the nullable msbuild command
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/baseline/baseline-nullable-build.md`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (warning/error counts).
- [ ] [P0-T5] Capture baseline test + coverage by running `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/baseline/baseline-vstest-coverage.md`
  - Coverage extraction mechanism: `vstest /EnableCodeCoverage` emits a binary `.coverage` file, which is not directly readable for per-class line %. Produce a readable coverage report using the mechanism already present in-repo — either the Cobertura settings at `UtilitiesCS.Test/test.runsettings` (pass `/Settings:UtilitiesCS.Test\test.runsettings`) or the installed `dotnet-coverage` global tool (`dotnet-coverage collect` / `dotnet-coverage merge --output-format cobertura`) — and read the numeric headline values recorded in the artifact `Output Summary:` from that readable report. Executor fallback: if the Moq-heavy `UtilitiesCS.Test` assembly requires it, add `/InIsolation` to the vstest invocation.
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric coverage headline values (repository/assembly line % and branch %, plus baseline line % for `FolderScorer` and `FolderPredictor` modules) and pass/fail counts, with the headline values read from the readable (Cobertura / `dotnet-coverage`) report.

### Phase 1 — Layer 1 FolderScore Contract and Scorer Projection

- [ ] [P1-T1] Create the `FolderScore` value type
  - File: `UtilitiesCS/OutlookObjects/Folder/FolderScore.cs` (namespace `UtilitiesCS`, matching `FolderScorer`)
  - Content: `public readonly struct FolderScore` with a `(string folderPath, long score, double probability)` constructor and get-only `FolderPath` (string), `Score` (long), `Probability` (double); no `record`, no `init` (net48/CS0518 constraint; precedent `ResourceTimingRow`).
  - Acceptance (AC1, AC11): struct compiles under `TreatWarningsAsErrors`; `Probability` XML doc explicitly states it is a relative display value (relative confidence vs the best suggestion), not a calibrated Bayesian posterior.
- [ ] [P1-T2] Wire `FolderScore.cs` into the production project
  - File: `UtilitiesCS/UtilitiesCS.csproj` (add `<Compile Include="OutlookObjects\Folder\FolderScore.cs" />` near the existing Folder entries around line 777-781)
  - Acceptance: entry present; `FolderScore` resolves in a build of `UtilitiesCS`.
- [ ] [P1-T3] Extract a shared ordered-scores helper in `FolderScorer` and route both existing name-only methods through it
  - File: `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` (lines 242-255 region)
  - Content: add `private IEnumerable<KeyValuePair<string,long>> OrderedScores()` returning `_folderNameScores.OrderByDescending(x => x.Value).ThenBy(x => x.Key, StringComparer.Ordinal)`; refactor `ToArray()` and `ToArray(int)` to `OrderedScores().Select(x => x.Key)` (with `.Take(topN)` for the overload).
  - Acceptance (AC4 structural parity): `ToArray()`/`ToArray(int)` produce identical ordering/content to the pre-change implementation; verified structurally by shared enumeration and by the golden test in P1-T5.
- [ ] [P1-T4] Add the scored projection methods on `FolderScorer`
  - File: `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`
  - Content: `public FolderScore[] ToScoredArray()` and `public FolderScore[] ToScoredArray(int topN)`, both projecting `OrderedScores()` into `FolderScore`, stamping `Probability = maxScore == 0 ? 0 : (double)x.Value / maxScore` where `maxScore` is computed once over the same ordered set; empty scorer returns `Array.Empty<FolderScore>()`.
  - Acceptance (AC2, AC6): methods return `FolderScore[]`; `Probability` is max-normalized with a `TopScore == 0` zero-guard; no divide-by-zero on empty/all-zero input.
- [ ] [P1-T5] Create the scorer regression/characterization tests (golden baseline + ordering parity)
  - File: `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerRegressionTests.cs` (MSTest + FluentAssertions)
  - Content: golden-baseline assertions locking `ToArray()` and `ToArray(int)` ordering/content for a populated scorer (including a two-folders-equal-score tie case locking the ordinal tie-break); assertions that `ToScoredArray().Select(x => x.FolderPath)` equals `ToArray()` and `ToScoredArray(n).Select(x => x.FolderPath)` equals `ToArray(n)`; an `"Error"` rejection assertion (`AddSuggestion(object,"Error")`/`AddArray` with `[0]=="Error"` leaves `_folderNameScores` empty and `"Error"` absent from `ToScoredArray()`).
  - Acceptance (AC3, AC4, AC9): tests pass against the refactored code and prove ordering/content parity and `"Error"` exclusion.
- [ ] [P1-T6] Create the scored-projection and edge-case tests
  - File: `UtilitiesCS.Test/OutlookObjects/Folder/FolderScoreTests.cs` (MSTest + FluentAssertions)
  - Content: `FolderScore` constructor round-trip; scored projection driven only through `AddSuggestion(string,long)` for Bayesian scale (e.g. 800 and 1000 -> `Probability` 0.8 and 1.0, documenting the `probability*1000` mapping), a conversation-scale weighted integer, a word-sequence-scale integer, and a mixed-source accumulation case (same folder summed across sources) asserting `Score` sums and `Probability <= 1`; edge cases: empty scorer returns empty array with no divide-by-zero, all-zero seeds yield every `Probability == 0`, `topN` larger than count returns all rows, and every `Probability` lies within `[0,1]`. `AddBayesianSuggestionsAsync` is not called.
  - Acceptance (AC6, AC7): all listed assertions pass; no COM/model path exercised.
- [ ] [P1-T7] Wire the Layer-1 test files into the test project
  - File: `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (add `<Compile Include="OutlookObjects\Folder\FolderScorerRegressionTests.cs" />` and `<Compile Include="OutlookObjects\Folder\FolderScoreTests.cs" />` near the existing Folder test entries around line 311-315)
  - Acceptance: both entries present; both test files compile and are discovered by vstest.

### Phase 2 — Layer 2 FolderRow Model and Predictor Row Builders

- [ ] [P2-T1] Create the `FolderRow` row model and kind enum
  - File: `UtilitiesCS/OutlookObjects/Folder/FolderRow.cs` (namespace `UtilitiesCS`)
  - Content: `public enum FolderRowKind { Separator, SearchResult, Suggestion, Recent }` and `public readonly struct FolderRow` with a `(string text, FolderRowKind kind, FolderScore? score)` constructor and get-only `Text` (string), `Kind` (`FolderRowKind`), `Score` (`FolderScore?`); no `record`/`init`.
  - Acceptance (AC8): compiles under `TreatWarningsAsErrors`; `Score` is a nullable `FolderScore?`.
- [ ] [P2-T2] Wire `FolderRow.cs` into the production project
  - File: `UtilitiesCS/UtilitiesCS.csproj` (add `<Compile Include="OutlookObjects\Folder\FolderRow.cs" />`)
  - Acceptance: entry present; `FolderRow`/`FolderRowKind` resolve in a build of `UtilitiesCS`.
- [ ] [P2-T3] Add the `FolderRowArray` property to `FolderPredictor` mirroring `FolderArray`
  - File: `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` (Public Properties region around lines 209-225; Helper Functions around lines 679-702)
  - Content: `public FolderRow[] FolderRowArray { get; }` that assembles rows in the same order as `FolderArray` — the `"========= SUGGESTIONS ========="` separator tagged `FolderRowKind.Separator` (`Score = null`), suggestion rows sourced from `Suggestions.ToScoredArray(5)` tagged `FolderRowKind.Suggestion` (non-null `Score`), the `"======= RECENT SELECTIONS ========"` separator tagged `Separator`, and recents tagged `FolderRowKind.Recent` (`Score = null`); each row's `Text` equals the exact legacy string. Do not alter `FolderArray` or the existing `AddSuggestions`/`AddRecents` string output.
  - Acceptance (AC8, AC5): `FolderRowArray.Select(r => r.Text)` equals `FolderArray`; `FolderArray` output is unchanged.
- [ ] [P2-T4] Add the `FindFolderRows(...)` method to `FolderPredictor` mirroring `FindFolder`
  - File: `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` (public Methods region around lines 256-306)
  - Content: `public FolderRow[] FindFolderRows(...)` with the same signature as `FindFolder`, producing rows tagged `SearchResult` for the `"======= SEARCH RESULTS ======="` block, `Suggestion` (with `Score`) for the suggestion block, and `Recent` for the recents block, separators tagged `Separator`; `Text` equals the exact legacy string in the same order as `FindFolder`. Do not alter `FindFolder`.
  - Acceptance (AC8, AC5): `FindFolderRows(...).Select(r => r.Text)` equals `FindFolder(...)` for the same inputs; `FindFolder` output is unchanged.
- [ ] [P2-T5] Create the row-model tests using the existing mocked-Outlook harness
  - File: `UtilitiesCS.Test/OutlookObjects/Folder/FolderRowTests.cs` (MSTest + Moq + FluentAssertions)
  - Content: reuse the `FolderPredictorTests` harness pattern (`CreateFolder`/`CreateApplication`/`CreateGlobals` + `Suggestions.AddSuggestion`, `Mock<Outlook.Application>`); assert `FolderRowArray.Select(r => r.Text)` equals `FolderArray` and `FindFolderRows(...).Select(r => r.Text)` equals `FindFolder(...)` (byte-for-byte Text parity = AC5 golden baseline for the predictor); assert `Kind` is correctly tagged for separator/search-result/suggestion/recent rows and that `Score` is non-null only on `Suggestion` rows and equals the corresponding `Suggestions.ToScoredArray(5)` entry. `AddBayesianSuggestionsAsync` is not called.
  - Acceptance (AC5, AC8): all listed assertions pass with no live COM.
- [ ] [P2-T6] Wire `FolderRowTests.cs` into the test project
  - File: `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (add `<Compile Include="OutlookObjects\Folder\FolderRowTests.cs" />` near the `FolderPredictorTests.cs` entry around line 352)
  - Acceptance: entry present; test file compiles and is discovered by vstest.

### Phase 3 — Fail-Before Dossier and Downstream Sufficiency Documentation

- [ ] [P3-T1] Record a fail-before exception dossier for the additive contract
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/regression-testing/fail-before-exception.2026-07-15T16-52.md`
  - Content: `Timestamp:`, `WhyFailingRunImpossible:` (this is a pure additive contract with no defect; new-member tests for `ToScoredArray`/`FolderRow` reference symbols absent from the baseline and cannot compile against it, so a meaningful runtime fail-before cannot be produced; the byte-for-byte regression tests in P1-T5/P2-T5 are characterization tests that must pass both before and after by design). Include an alternative proof section: the golden-baseline characterization tests demonstrate no behavior change on existing outputs.
  - Acceptance: dossier is schema-valid per `evidence-and-timestamp-conventions` (satisfies the fail-before requirement without an `[expect-fail]` task); the filename matches the canonical discovery pattern `fail-before-exception.*.md` per evidence-and-timestamp-conventions.
- [ ] [P3-T2] Document downstream contract sufficiency for 9002 and 9003
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/other/downstream-sufficiency.md`
  - Content: state that 9002 (EfcViewer `ListBox` via `FindFolderRows`) and 9003 (QuickFiler `ComboBox` via `FolderRowArray`) can render a whole-number percentage as `Math.Round(Probability * 100)` and skip non-suggestion rows by `Kind` (no `.StartsWith("====")`), from the single normalization point in `FolderScorer.ToScoredArray`, with no second plumbing pass.
  - Acceptance (AC10): artifact records the two consumer paths, the `Math.Round(Probability * 100)` mapping, and the `Kind`-based skip.

### Phase 4 — Final QC Loop and Coverage Verification

Run the toolchain in the canonical order below. If any step changes files or fails, fix and restart
from P4-T1. Every command task is unconditional; `SKIPPED` is not a valid outcome.

- [ ] [P4-T1] Run formatting: `dotnet tool run csharpier .`
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/qa-gates/qc-csharpier.md`
  - Acceptance (AC13): artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` 0, `Output Summary:`; if files changed, the loop restarts.
- [ ] [P4-T2] Run analyzer build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/qa-gates/qc-analyzer-build.md`
  - Acceptance (AC13): artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` 0, `Output Summary:` with 0 analyzer errors.
- [ ] [P4-T3] Run nullable/type build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/qa-gates/qc-nullable-build.md`
  - Acceptance (AC13): artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` 0, `Output Summary:` with 0 nullable/type warnings-as-errors.
- [ ] [P4-T4] Run tests with coverage: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/qa-gates/qc-vstest-coverage.md`
  - Coverage extraction mechanism: `vstest /EnableCodeCoverage` emits a binary `.coverage` file, which is not directly readable for per-class line %. Produce a readable coverage report using the mechanism already present in-repo — either the Cobertura settings at `UtilitiesCS.Test/test.runsettings` (pass `/Settings:UtilitiesCS.Test\test.runsettings`) or the installed `dotnet-coverage` global tool (`dotnet-coverage collect` / `dotnet-coverage merge --output-format cobertura`) — and read the per-class numeric line % for `FolderScorer`/`FolderPredictor`/`FolderScore`/`FolderRow` recorded in the artifact `Output Summary:` from that readable report. Executor fallback: if the Moq-heavy `UtilitiesCS.Test` assembly requires it, add `/InIsolation` to the vstest invocation.
  - Acceptance (AC13): artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` 0, `Output Summary:` with pass count (all new tests passing) and numeric post-change coverage headline values (assembly line % and branch %, and line % for `FolderScorer`/`FolderPredictor`/`FolderScore`/`FolderRow`), with the headline values read from the readable (Cobertura / `dotnet-coverage`) report.
- [ ] [P4-T5] Verify coverage delta and thresholds against the stricter regime
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/qa-gates/coverage-delta.md`
  - Content: report baseline coverage (from P0-T5), post-change coverage (from P4-T4), and new/changed-code coverage for `FolderScore.cs`, `FolderRow.cs`, and the new `FolderScorer`/`FolderPredictor` members; verify >= 90% line on new members, branch coverage of the empty/all-zero/tie/topN paths, no reduction on changed lines, and no production file excluded from measurement.
  - Acceptance (AC12): artifact reports all three coverage figures and confirms the stricter thresholds are met; if any figure is below threshold, the outcome is remediation-required (not PASS).
- [ ] [P4-T6] Verify acceptance-criteria checkoff against evidence
  - File: `docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/qa-gates/ac-verification.md`
  - Content: map each spec AC (AC1..AC13) and each aligned user-story AC to the task(s) and evidence artifact(s) that satisfy it, per `acceptance-criteria-tracking`.
  - Acceptance: every AC is mapped to satisfying evidence; no AC is unmapped or contradicted by evidence on disk.

## Test Plan

- Unit (MSTest + Moq + FluentAssertions):
  - `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerRegressionTests.cs` — golden baselines for `ToArray`/`ToArray(int)`, ordering parity vs `ToScoredArray`, ordinal tie-break, `"Error"` exclusion.
  - `UtilitiesCS.Test/OutlookObjects/Folder/FolderScoreTests.cs` — scored projection across three source scales + mixed accumulation, probability `[0,1]` bound, empty/all-zero/topN edge cases, zero-guard.
  - `UtilitiesCS.Test/OutlookObjects/Folder/FolderRowTests.cs` — `FolderRowArray`/`FindFolderRows` Text parity with `FolderArray`/`FindFolder`, `Kind` tagging, `Score` non-null only on `Suggestion` rows (mocked-Outlook harness).
- No COM/model path: `AddBayesianSuggestionsAsync` is never invoked; the Bayesian scale is covered via `AddSuggestion` with `probability*1000` values.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-vstest-coverage.md`.
  - Post-change: `evidence/qa-gates/qc-vstest-coverage.md`.
  - Delta/threshold: `evidence/qa-gates/coverage-delta.md`.

## Open Questions / Notes

- Spec open decisions are resolved in favor of the spec: max-normalization for `Probability`, Layer-2 `FolderRow` delivered in this feature (9001), and the stricter coverage regime enforced.
- `IFolderSearchHandler` is intentionally left unchanged; extending it is a consumer-driven decision deferred to 9003.
