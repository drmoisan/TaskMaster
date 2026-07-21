# Feature Audit — legacy-scodictionary-removal (#315)

- Timestamp: 2026-07-11T12-09
- Reviewer: feature-review
- Work Mode: full-feature -> AC sources: `spec.md` (§Acceptance Criteria) AND `user-story.md` (§Acceptance Criteria)
- Diff range: d2d5e73bfbce7fb73b9d5be1601612cc01e54f09..HEAD (7184d0d1)

## Summary

All five acceptance criteria (identical text in `spec.md` and `user-story.md`) are verified PASS against the
committed diff and evidence. Scope discipline holds: every declared Non-Goal file is untouched in the diff.
No PARTIAL/FAIL/UNVERIFIED criteria. The AC checkboxes in both source files were already `[x]`; they remain
checked, consistent with the PASS verdicts below.

## Scope and Baseline

- Baseline: epic integration base commit d2d5e73b (pre-change).
- Head: 7184d0d1 (single refactor commit).
- Change class: net removal of dead code + test retargets; zero new production executable lines.
- Non-Goal verification (spec.md lines 65-76): `git diff --name-only` contains none of `ISCODictionary.cs`,
  `ScoDictionaryConverter*`, `WrapperScoDictionary*` (or their tests), `IntelligenceConfig_Tests.cs`,
  `ObservableDictionary_Tests.cs`, `IScoCollection.cs`, or any `UtilitiesSwordfish` project/reference. The
  residual `using Swordfish.NET.Collections;` in `IScoCollection.cs` binds a different type (IScoCollection)
  and is F5/#308 scope, not a finding here.

## Acceptance Criteria Inventory

Source (both files, identical text):
1. `SCODictionary.cs` no longer exists and its `<Compile Include>` entry is removed from `UtilitiesCS.csproj`.
2. No production or test code references the legacy `ScoDictionary<>` class or its `Swordfish.NET.Collections` binding.
3. Generic serialization/wrapper test coverage that used `ScoDictionary` as a stand-in is preserved by retargeting to a first-party type.
4. On-disk JSON compatibility is preserved for any persisted payload touched by retargeted tests.
5. Full C# toolchain passes (CSharpier, analyzers, nullable+TreatWarningsAsErrors, MSTest) with zero test regressions and no coverage regression on changed lines.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | `SCODictionary.cs` deleted (diff lines 871-1336; `ls` confirms absent). `<Compile Include>` removed from `UtilitiesCS.csproj` (diff lines 1341-1348). Confirmed by `residual-binding-check.md`. |
| AC2 | PASS | Word-boundary grep for bare `ScoDictionary<` across `*.cs` returns none (all hits are `WrapperScoDictionary<`, `IScoDictionary<`, `ScoDictionaryNew`, or comments). No `Swordfish.NET.Collections` binding via `ScoDictionary` remains; `residual-binding-check.md` corroborates. |
| AC3 | PASS | Positives retargeted to `ScoDictionaryNew<>` (`SmartSerializableBase_Tests.cs`, `SmartSerializableNonTyped_Tests.cs` positives); negatives retargeted to first-party `ConcurrentObservableCollection<int>`; redundant Static negative removed with an equivalent negative retained. Infrastructure coverage (`DeserializeObject<T>`, `IsSmartSerializable`) preserved; suite green in `final-tests-coverage.md`. |
| AC4 | PASS | `ScoDictionaryNew_OnDiskCompatibility_Tests` 5/5 passed post-change (`regression-testing/ondisk-compat-green.md`); the authoritative persisted-dictionary shape coverage stays green and was left unedited (out of scope to edit). |
| AC5 | PASS | CSharpier clean (`final-csharpier.md`), analyzers 0 errors (`final-analyzer-build.md`), nullable+TreatWarningsAsErrors 0/0 (`final-nullable-build.md`), MSTest 4223/4223 (`final-tests-coverage.md`; baseline 4255, delta -32 = intentional deletions only). No coverage regression on changed lines: zero new/modified production executable lines, so no changed line's coverage can regress (`coverage-delta.md`, PASS). |

Seeded test conditions (spec.md lines 160-163) are covered by AC3 (retargeted tests pass), AC4 (JSON
compat), and AC5 (no suite regression); all PASS.

## Acceptance Criteria Check-off

Both `spec.md` (lines 154-158) and `user-story.md` (lines 52-56) already carry `- [x]` for all five criteria,
consistent with the PASS verdicts above. No checkbox required transition from `[ ]` to `[x]`; no PARTIAL/FAIL
item remains unchecked.

### Acceptance Criteria Status
- Source: docs/.../2026-07-11-legacy-scodictionary-removal-315/spec.md and user-story.md
- Total AC items: 5 (each file)
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

## Verdict

PASS. All acceptance criteria satisfied; scope discipline verified; zero blocking findings.
