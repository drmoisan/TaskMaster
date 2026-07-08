# Feature Audit — Issue #251 (quickfiler-darkmode-stale-subscription)

- Timestamp: 2026-07-07T03-55
- Work Mode: minor-audit
- AC Source: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md`, `## Acceptance Criteria` (AC1-AC8, the sole AC source for `minor-audit`; `spec.md`/`user-story.md` confirmed absent from the feature folder, consistent with `evidence/baseline/minor-audit-scope.2026-07-06T23-08.md`)

## Scope and Baseline

- Merge-base (recomputed independently via `git merge-base HEAD origin/main`): `c7c8e7e7ea8ce53d552745e9a15ef02cbce599e0` — matches the caller-supplied value.
- Head SHA: `6304fa13af4c8dfa9e9c5273f40dac04579e9178`.
- Baseline defect state confirmed by direct read of the merge-base revision: `Cleanup()`/`CleanupAsync()` null `_globals` with no unsubscribe; `DarkMode_CheckedChanged` dereferences `_globals.Ol.DarkMode` unconditionally.

## Acceptance Criteria Inventory

| ID | Criterion (verbatim from `issue.md`) |
|---|---|
| AC1 | A regression test reproduces the defect on the pre-fix code — raising `PropertyChanged("DarkMode")` on the globals dark-mode source after `Cleanup()` throws (or invokes the stale handler) — and passes after the fix. |
| AC2 | `Cleanup()` unsubscribes `DarkMode_CheckedChanged` from `_globals.Ol.PropertyChanged` before nulling `_globals`. |
| AC3 | `CleanupAsync()` unsubscribes `DarkMode_CheckedChanged` from `_globals.Ol.PropertyChanged` before nulling `_globals`. |
| AC4 | `DarkMode_CheckedChanged` no longer throws when invoked on a cleaned-up controller (defensive early return and/or reads state from `sender`), and performs no theme-change side effect in that state. |
| AC5 | After the fix, raising `PropertyChanged("DarkMode")` following `Cleanup()`/`CleanupAsync()` produces no exception and no call into `SetDarkMode`/`SetLightMode`. |
| AC6 | No production files other than `QuickFiler/Controllers/QfcCollectionController.cs` are changed; the fix is minimal and targeted. |
| AC7 | Full C# toolchain passes in order (CSharpier → analyzers → nullable → MSTest) with no regressions; changed-line coverage meets policy. |
| AC8 | Required CI checks pass green on the PR head SHA. |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence |
|---|---|---|
| AC1 | **PASS** | Pre-fix failure independently corroborated by executor evidence `evidence/regression-testing/fail-before-quickfiler-darkmode-stale-subscription.2026-07-06T23-08.md` (both tests failed with the exact reported `NullReferenceException` at the pre-fix `DarkMode_CheckedChanged`). Post-fix pass independently re-verified by this review: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerDarkModeTests"` → `Cleanup_ThenDarkModePropertyChanged_DoesNotThrow` and `CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow` both Passed. |
| AC2 | **PASS** | Diff confirms `Cleanup()` (line ~2191-2199) adds `if (_globals?.Ol is not null) { _globals.Ol.PropertyChanged -= DarkMode_CheckedChanged; }` immediately before `_globals = null;`. |
| AC3 | **PASS** | Diff confirms the identical guarded unsubscribe statement was added to `CleanupAsync()` (line ~2177-2185), immediately before `_globals = null;`. |
| AC4 | **PASS** | Diff confirms `DarkMode_CheckedChanged` now begins with `if (_formViewer is null) { return; }` and, past that guard, prefers `sender is IOlObjects senderOl` over `_globals.Ol` when reading `DarkMode`, falling through to a bare `return` when neither is available. No theme-change call (`SetDarkMode`/`SetLightMode`) occurs when the early return fires, verified by test (see AC5). See code-review.2026-07-07T03-55.md for a non-blocking observation that the sender-preference branch itself is not directly exercised by any test. |
| AC5 | **PASS** | Both regression tests assert `act.Should().NotThrow(...)` and additionally verify, via a mocked `IQfcItemController` injected into `_itemGroups`, that `SetThemeDark`/`SetThemeLight` are `Times.Never` invoked after `Cleanup()`/`CleanupAsync()`. Since `SetDarkMode`/`SetLightMode` are the only production call sites of `SetThemeDark`/`SetThemeLight` (confirmed by reading both methods), this is a valid proxy for "no call into `SetDarkMode`/`SetLightMode`." Independently re-verified passing by this review. |
| AC6 | **PASS** | `git diff --name-status` for the full range shows exactly one changed production file: `QuickFiler/Controllers/QfcCollectionController.cs`. All other changes are a new test file, a test-project `.csproj` wiring line, one unrelated memory doc, and feature-folder docs/evidence. |
| AC7 | **PASS, with a documented caveat** | All four toolchain stages independently re-verified passing with no regressions (policy-audit §1). Repo-wide C# coverage freshly measured with the full multi-assembly suite: 81.27% line / 73.68% branch — PASS against this repo's own `CLAUDE.md` 80% line gate; below the newer, conflicting `quality-tiers.md`/`general-unit-test.md` 85%/75% uniform floor (pre-existing gap, not caused by this PR; see policy-audit §2.1 for the full policy-conflict discussion). The changed production lines themselves are not numerically measurable by the coverage tool because `QfcCollectionController` carries a pre-existing, unmodified `[ExcludeFromCodeCoverage]` class attribute — there is no coverage regression because nothing was measured for this class in either the baseline or post-change state, and the two new regression tests do directly exercise (execute and assert on) every changed line at the source level even though the coverage percentage does not reflect it. |
| AC8 | **PENDING (correctly deferred, not a defect)** | `evidence/qa-gates/ci-check-verification.2026-07-07T00-12.md` records an explicit, plan-authorized deferral: no PR exists yet from this branch (`gh pr list --head bug/quickfiler-darkmode-stale-subscription` returned `[]` at plan-execution time), so no CI-check verdict can be obtained. `issue.md` correctly leaves AC8 unchecked. This criterion cannot be verified until a PR is opened and required checks run against the head SHA; it is not evidence of a defect in this branch. |

## Acceptance Criteria Status

- Source: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md`
- Total AC items: 8
- Checked off (delivered): 7 (AC1-AC7, already checked `[x]` in `issue.md` by the executor; independently re-verified by this review)
- Remaining (unchecked): 1
- Items remaining: AC8 ("Required CI checks pass green on the PR head SHA") — correctly deferred pending PR creation; no check-off action taken by this review since the criterion remains genuinely unverifiable at this stage.

## Verdict

**PASS.** All acceptance criteria verifiable at this stage (AC1-AC7) are satisfied with independently re-verified evidence. AC8 is appropriately pending PR/CI and is not a blocking defect in the branch as reviewed.
