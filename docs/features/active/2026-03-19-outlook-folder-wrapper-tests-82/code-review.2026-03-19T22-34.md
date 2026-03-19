# Code Review — outlook-folder-wrapper-tests-82 (2026-03-19T22-34)

- **Feature folder:** `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/`
- **Feature folder selection rule:** Used `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/` because the refreshed `pr_context` names it as the active scoping-doc folder and its suffix matches issue `#82`.
- **Base branch:** `development`

## Executive summary

This branch delivers the intended folder-wrapper coverage feature cleanly relative to `development`: it expands the existing folder MSTest corpus, adds `MAPIMethodsTests.cs`, introduces only non-public seam hooks in `FolderPredictor.cs` and `FolderConverter.cs`, and backs the change with canonical coverage evidence showing every in-scope folder production file at `>= 80%` line coverage. The refreshed `pr_context` shows a focused 54-file branch, not the stale oversized diff described in the archived audit folder.

Live review-time validation also looks good: formatter completed successfully, analyzer build passed with `55` warnings and `0` errors, nullable build passed with `1` warning and `0` errors, and MSTest passed with `1273 total`, `1271 passed`, `2 skipped`, `0 failed`.

**Top 3 risks**

1. The seam strategy relies on mutable static delegates in `FolderPredictor.cs` and `FolderConverter.cs`, which can become cross-test hazards if reuse expands without disciplined reset patterns.
2. The solution still carries background build warnings, including an MSBuild reference-conflict warning under `UtilitiesCS.Test`, which is not blocking here but does add noise.
3. The feature folder contains an archived audit subfolder with stale conclusions; future reviewers could read that before the refreshed canonical evidence and get the wrong impression.

**PR readiness:** **Go** — ready for PR / merge review.

## Findings

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor | `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | internal seam delegates and wrapper methods | The new predictor seams use mutable static delegates (`PromptForFolderNameDialog`, `PromptForFolderNameWithDefaultDialog`, `ShowPromptMessageAction`, `EnterUiContextAsyncAction`, `CreateDirectoryPathFactory`). They are acceptable for this feature, but they create process-wide state. | Keep the current design for this scoped feature, but if this seam pattern spreads, add a shared reset helper or move toward instance-scoped collaborators. | Global mutable hooks are easy to leak across tests if cleanup is skipped after a failure or future parallelization is enabled. | `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`; injected seam tests in `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` |
| Minor | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | internal static dialog delegates | `FolderConverter.cs` uses the same static-hook pattern for prompt selection and input fallback. The implementation is still non-public and behavior-preserving, but the same test-isolation caution applies. | Keep it as-is for now; consider centralizing seam reset/setup conventions in folder tests to reduce maintenance risk. | The seam is justified by hard-to-reach static UI paths, but static state needs consistent hygiene. | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`; injected seam tests in `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` |
| Minor | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | package/reference graph | Live nullable validation still emits `MSB3277` reference-conflict noise in `UtilitiesCS.Test`. It does not block this feature, but it weakens signal quality for future reviews. | Track a separate repo-maintenance follow-up to rationalize the test project’s mixed test platform/reference graph. | Warning-heavy builds make it harder to spot genuinely new regressions. | Live review-time nullable build; `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-build-nullable-2026-03-19T21-39-29Z.md` |
| Nit | `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/audit-2026-03019T17-56/` | archived audit artifacts | The archived audit folder contains stale conclusions that conflict with the refreshed canonical `pr_context` bundle. | Treat archived audit content as historical only; reviewers should prioritize the refreshed root-level artifacts and canonical `pr_context`. | Audit drift can confuse reviewers even when the implementation is solid. | Archived audit files vs refreshed `artifacts/pr_context.summary.txt` / `appendix.txt` |

## Typed Python audit

**N/A** — no Python files were changed in the scoped feature or the refreshed branch diff.

## Test quality audit

### Strengths

- The feature keeps the existing one-file-per-area folder test structure instead of collapsing everything into a giant omnibus test file.
- New tests cover positive, negative, boundary, and seam-driven behaviors for folder conversion, scoring, prediction, wrapper state, traversal, tree operations, and MAPI declarations.
- The feature adds `MAPIMethodsTests.cs` and registers it explicitly in `UtilitiesCS.Test.csproj`, matching the repo’s explicit compile-include convention.
- Canonical coverage evidence is strong and specific: every one of the 13 in-scope production files is listed individually with final line coverage, and both changed production files show `100%` changed-line coverage.

### Watch items

- Reflection-heavy tests are appropriate here for hard-to-reach legacy code, but they do increase coupling to implementation details.
- Static seam mutation must remain tightly cleaned up in tests.
- The live formatter run surfaced unrelated repo compile/XML warnings; those did not dirty the tree or fail validation, but they are background noise worth separating from feature results when possible.

## Security / correctness checks

- **Secrets:** No secrets or credentials were found in the reviewed feature files or active feature docs.
- **Unsafe subprocess usage:** No new subprocess or shell execution was introduced in the reviewed production files.
- **Input validation:** The updated tests improve confidence around argument guards, null handling, folder lookup failures, illegal-name flows, and edge-path behavior.
- **API containment:** Public runtime behavior remains stable; seam changes stay `internal` and localized.

## Research log

None required. The review relied on repository-local canonical evidence and direct inspection of changed files.

## Review conclusion

**Go for PR readiness.**

The folder-wrapper coverage feature is well-scoped, well-tested, and well-documented in its current branch state. The only findings are maintainability-level concerns around static seam hooks, background build-warning noise, and stale archived audit docs; none of them block opening or merging a focused PR for `#82`.