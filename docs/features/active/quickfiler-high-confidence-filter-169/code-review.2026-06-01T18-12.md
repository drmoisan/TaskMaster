# Code Review (RE-AUDIT) — quickfiler-high-confidence-filter (Issue #169)

- Generated: 2026-06-01T18-12 (UTC)
- Audit type: RE-AUDIT following remediation (supersedes `code-review.2026-06-01T17-23.md`)
- Base branch (resolved): `development` @ `3322bbee6a941eaa05e8388dd78ec3998e542d75`
- Head SHA: `0d4f6331622f81637a47a3eb98832a0af2632053`
- Scope: full branch diff vs. base

## Executive Summary

The remediation resolves both prior BLOCKER findings. F1 (high-confidence mode persisting and leaking
into the standard entry point) is fixed by making the persisted `HighConfidenceModeEnabled` flag
launch-scoped: the standard launch path and release path now set it `false`, and the high-confidence
launch path sets it `true`, all through a single testable `SetHighConfidenceModeForLaunch(bool)`
decision method. F2 (absent canonical coverage artifact and 0%-covered entry-point decision logic) is
fixed: `artifacts/csharp/coverage.xml` now exists, and the extracted decision method is covered at
100% by two new MSTest regression tests.

No new blocking findings were introduced by the remediation. The change is additive and confined to
`TaskMaster/Ribbon/RibbonController.cs` (production) and
`TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (tests). The feature's core filtering logic
(`FolderScorer.TopScore`, `QfcCollectionController.RemoveBelowThresholdAsync`,
`QfcFormController.ApplyHighConfidenceFilterAsync`) is unchanged from the prior review and remains
well-covered. The toolchain gates pass for the feature scope (format, analyzers, nullable on touched
paths, tests). The optional non-blocking M1 (lossy threshold round-trip) was deliberately deferred
with a recorded IEEE-754 rationale; it does not gate the feature.

One residual non-blocking observation (F1-RESIDUAL) concerns statement ordering in
`LoadQuickFilerHighConfidenceAsync`: `_quickFilerLoaded = true` is set before
`SetHighConfidenceModeForLaunch(true)` and before the awaited `LaunchAsync`. If `LaunchAsync` returns
null, `_quickFilerLoaded` is reset to false but the persisted mode flag remains `true` until the next
standard launch or release resets it. AC6 is still satisfied because the standard entry point
unconditionally resets the flag to `false` at the start of `LoadQuickFilerAsync`; the window is
self-correcting and not user-observable through the standard path. This is recorded as LOW severity.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| RESOLVED (was BLOCKER) | `TaskMaster/Ribbon/RibbonController.cs` | `SetHighConfidenceModeForLaunch` (268-269); `LoadQuickFilerAsync` (111); `LoadQuickFilerHighConfidenceAsync` (133); `ReleaseQuickFiler` (147) | F1: high-confidence mode previously persisted across launches and leaked into the standard entry point (AC6 FAIL). Now launch-scoped: standard launch and release set the flag `false`; high-confidence launch sets it `true`. | None required; fix verified. | Standard entry point can no longer inherit high-confidence mode, so it never filters; satisfies AC6 and the spec alternate flow. | Source lines read directly; regression tests `StandardLaunchAfterHighConfidenceLaunch_DoesNotEnableMode` and `SetHighConfidenceModeForLaunch_True_EnablesMode` pass; new member 100% covered in `artifacts/csharp/coverage.xml`. |
| RESOLVED (was BLOCKER) | `artifacts/csharp/coverage.xml`; `TaskMaster/Ribbon/RibbonController.cs` | `SetHighConfidenceModeForLaunch` | F2: canonical C# coverage artifact was absent and the entry-point decision logic was at 0%. Artifact now emitted; decision logic extracted into a testable seam covered at 100%. | None required; fix verified. | Coverage is independently verifiable from the canonical artifact and the behaviorally-distinct decision is exercised. | `artifacts/csharp/coverage.xml` present (Cobertura); `SetHighConfidenceModeForLaunch` line-rate 1.0. |
| LOW | `TaskMaster/Ribbon/RibbonController.cs` | `LoadQuickFilerHighConfidenceAsync` (130-140) | F1-RESIDUAL: `_quickFilerLoaded = true` is set before `SetHighConfidenceModeForLaunch(true)` and before the awaited `LaunchAsync`. If `LaunchAsync` returns null, the mode flag stays `true` until the next standard launch/release resets it. AC6 still holds because `LoadQuickFilerAsync` unconditionally resets to `false`. | Optionally set the launch flag inside a try/finally or only after a successful launch, mirroring the `_quickFilerLoaded` rollback, so the at-rest flag is consistent on the failure path. | Defensive consistency; not user-observable via the standard path because that path resets the flag. Non-blocking. | Source lines 130-140 read directly; `LoadQuickFilerAsync` line 111 resets unconditionally. |
| LOW (non-blocking, deferred) | `TaskMaster/Ribbon/RibbonController.cs` | `GetHighConfidenceThresholdText` (276-278); `SetHighConfidenceThresholdText` (285-300) | M1: threshold round-trip is lossy for fractional percentages (round-on-read). Deliberately deferred during remediation. | Track separately; if a lossless integer-percentage round-trip is desired, constrain input to integers and adjust the existing expectation test. | Documented IEEE-754 rationale (`0.9 * 100 = 90.00000000000001`) makes a no-round render non-trivial; out of low-risk scope. AC4/AC5 satisfied with the current rounding behavior. | `remediation-plan.2026-06-01T18-05.md` Open Questions deferral note (2026-06-01T17-35-23Z). |
| INFO (pre-existing) | `QfcItemController.cs`, `QfcCollectionController.cs`, `QfcFormController.cs`, `FolderScorer.cs`, `RibbonController.cs` | whole-file | I1: files exceed the 500-line limit. Pre-existing at the merge-base; this feature added only small members. | Track a separate split; do not let these files keep growing. | Not introduced by issue #169; not a remediation trigger. | Policy audit File Size Limit section; diff shows additive changes only. |
| INFO (pre-existing) | `UtilitiesCS.Test` | timing/concurrency tests | I2: flaky timing/concurrency/timeout tests under `/EnableCodeCoverage` instrumentation. Pre-existing; varies per run (13→8→6). | Continue existing test-isolation work (commits 384858b8, b160037a). | Not in TaskMaster.Test/QuickFiler.Test; unrelated to issue #169; non-regressive. | `evidence/qa/final-toolchain.2026-06-01T17-35-23Z.md`; baseline tests-coverage evidence. |
| INFO (pre-existing) | `UtilitiesSwordfish`, `SVGControl` | various `.cs` | Vendored projects emit 84 nullable errors under forced `/p:Nullable=enable` `/t:Rebuild`. Not touched by issue #169. | Out of feature scope; track vendored-library nullable annotation separately if desired. | Errors appear only because nullable is forced solution-wide on libraries that do not opt in; touched paths are clean. | Independent `/t:Rebuild` nullable build; all 84 errors confined to those two projects. |

## Design and Policy Observations (non-finding)

- The R1 fix follows the C# DI-seam preference order: it introduces the smallest seam
  (`SetHighConfidenceModeForLaunch`) needed to make the launch decision unit-testable through the
  existing `AppQuickFilerSettings`/`Settings.Default` round-trip, snapshotted and restored in the test
  fixture. No broad refactor, no parameter threading through `QfcHomeController.LaunchAsync` (the
  documented FALLBACK was correctly not undertaken).
- The new method carries an XML doc comment stating it is launch-scoped, satisfying the "comment why,
  not what" guidance.
- Tests use MSTest + FluentAssertions, follow Arrange-Act-Assert, are independent (snapshot/restore of
  `Settings.Default` in `[TestInitialize]`/`[TestCleanup]`), deterministic, and use no Outlook COM or
  temporary files.
- The core feature logic reviewed in the original pass (`TopScore`, `RemoveBelowThresholdAsync` with
  the EntryID-capture-before-removal pattern to avoid mid-iteration index drift,
  `ApplyHighConfidenceFilterAsync` null-guards and enabled-only call) is unchanged and remains sound.
