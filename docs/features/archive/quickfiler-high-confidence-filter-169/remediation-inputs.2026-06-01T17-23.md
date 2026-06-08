# Remediation Inputs — quickfiler-high-confidence-filter (Issue #169)

- Generated: 2026-06-01T17-23 (UTC)
- Base branch: `development` @ `3322bbee6a941eaa05e8388dd78ec3998e542d75`
- Head: `32de29d7748492eb0ec62219f2fe20b3d279142e`
- Source artifacts:
  - `docs/features/active/quickfiler-high-confidence-filter-169/policy-audit.2026-06-01T17-23.md`
  - `docs/features/active/quickfiler-high-confidence-filter-169/code-review.2026-06-01T17-23.md`
  - `docs/features/active/quickfiler-high-confidence-filter-169/feature-audit.2026-06-01T17-23.md`

## Remediation Triggers (met)

- Acceptance criterion FAIL: AC6.
- Acceptance criteria PARTIAL: AC1, AC7.
- Policy audit FAIL: C# coverage (canonical artifact absent; two assemblies below floor).
- Code review blockers: F1 (persisted-mode leak), F2 (coverage artifact + 0% entry-point coverage).

## Blocking Findings

### [BLOCKER] R1 — High-confidence mode persists and leaks into the standard entry point (AC6 FAIL)

- Severity: BLOCKER
- Files:
  - `TaskMaster/Ribbon/RibbonController.cs` — `LoadQuickFilerHighConfidenceAsync` (lines 127–140),
    `LoadQuickFilerAsync` (lines 107–119)
  - `QuickFiler/Controllers/QfcFormController.cs` — `ApplyHighConfidenceFilterAsync` /
    `LoadItemsAsync` (line 958 reads the persisted flag)
  - `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` — `HighConfidenceModeEnabled` setter persists
    via `Settings.Default.Save()`
- Problem: `LoadQuickFilerHighConfidenceAsync` sets `Globals.InternalQfSettings.HighConfidenceModeEnabled = true`,
  which is a user-scoped persisted setting. No code path ever resets it to `false`. After one
  high-confidence launch, the persisted flag stays `true` across sessions, so the standard
  "QuickFiler" entry point applies the high-confidence filter, contradicting AC6 and the spec
  alternate flow ("the standard 'QuickFiler' entry point is used; `RemoveBelowThresholdAsync` is not
  called").
- Required fix (one of):
  1. Make the mode launch-scoped instead of persisted: pass a `highConfidence` boolean parameter
     through `QfcHomeController.LaunchAsync` / `QfcFormController` instead of reading a persisted
     setting; leave `HighConfidenceThreshold` persisted but stop persisting `HighConfidenceModeEnabled`
     as the launch switch; or
  2. Reset `HighConfidenceModeEnabled = false` at the start of `LoadQuickFilerAsync` (standard launch)
     and after the high-confidence session has consumed the flag, so the standard entry point always
     observes disabled.
- Required regression test: add a test asserting that a standard launch performed after a
  high-confidence launch does NOT call `RemoveBelowThresholdAsync` (e.g. drive the
  entry-point/decision path with a verifiable `IQfcCollectionController` mock or a testable helper
  extracted from the launch method).
- Acceptance check: AC6 evaluates PASS; AC1 entry-point logic covered by a unit test.

### [BLOCKER] R2 — C# coverage verification gap (policy FAIL, AC7 PARTIAL)

- Severity: BLOCKER
- Artifact: `artifacts/csharp/coverage.xml` (canonical, machine-readable; absent)
- Problem: coverage verification is mandatory for every changed language. The canonical C# coverage
  artifact consumed by `validate-feature-review-coverage.ps1` does not exist; coverage could only be
  assessed from a narrative comparison. Separately, the narrative shows QuickFiler.dll (23.40%) and
  TaskMaster.dll (25.16%) below the 80% floor, and the entry-point method
  `LoadQuickFilerHighConfidenceAsync` (the method carrying R1) at 0% coverage.
- Required fix:
  1. Emit `artifacts/csharp/coverage.xml` (Cobertura/JaCoCo XML) from the instrumented
     `vstest.console.exe ... /EnableCodeCoverage` run (convert the `.coverage` via
     `dotnet-coverage merge -f cobertura`/`-f xml` to the canonical path).
  2. Add coverage for the entry-point decision logic via the R1 seam/refactor so the feature's only
     behaviorally distinct member is exercised.
  3. In the policy audit, record an explicit C# coverage PASS/FAIL backed by the emitted artifact.
- Note on assembly floors: QuickFiler.dll/TaskMaster.dll are VSTO/WinForms/COM-dominated. If the
  repository's standing interpretation is that the 80% floor applies to unit-testable application code
  (UtilitiesCS 85.45% PASS) rather than UI-shell assemblies, the remediation owner should document that
  interpretation authoritatively (e.g. coverage exclusion configuration) rather than relying on an
  ad-hoc narrative. Until then the literal per-assembly floor is unmet for two assemblies.

## Non-Blocking Findings (track, not gating)

- M1 — `SetHighConfidenceThresholdText`/`GetHighConfidenceThresholdText` round-trip is lossy for
  fractional percentages (round-on-read). Constrain input to integers or render without rounding.
- I1 — Pre-existing file-size policy breaches in `QfcItemController.cs`, `QfcCollectionController.cs`,
  `QfcFormController.cs`, `FolderScorer.cs`. Not introduced by this feature; track a separate split.
- I2 — `UtilitiesCS.Test` timing/concurrency flakiness under coverage instrumentation (11 failures,
  asserted pre-existing). Continue existing isolation work.

## Acceptance-Criteria Source Correction Required

`user-story.md` currently marks AC1–AC7 all `[x]`. The remediation owner must revert AC1, AC6, and
AC7 to `[ ]` until R1 and R2 are resolved. The reviewer did not modify the source (no silent fixes).

## Handoff

- Recommended target: atomic planner for a remediation plan covering R1 and R2 (and optionally M1).
- `remediation-handoff-atomic-planner` skill is referenced by the workflow but is not present on disk
  in this repository; the canonical plan-template path and MCP handoff tooling could not be resolved.
  This file is the remediation input of record. A remediation plan file should be created by the
  planner from the canonical plan template once available; that step could not be completed by the
  reviewer because the template/handoff skill is unavailable in this environment.

UNVERIFIED (with reason): creation of the downstream remediation plan file — the
`remediation-handoff-atomic-planner` skill and canonical plan template are not present in the
repository, so the reviewer cannot author the plan artifact deterministically. The blocking findings
above are complete and actionable for a planner.
