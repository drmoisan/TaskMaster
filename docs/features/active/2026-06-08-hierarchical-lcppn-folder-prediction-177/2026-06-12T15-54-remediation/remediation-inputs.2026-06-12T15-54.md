# Remediation Inputs (Cycle 1): hierarchical-lcppn-folder-prediction (#177)

**Cycle:** 1
**Entry timestamp:** 2026-06-12T15-54 (UTC)
**Authored by:** orchestrator
**Base:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head:** `TaskMaster-wt-2026-06-08-12-06` (`d06f5c00`)

## Trigger

Step 8 feature-review (artifacts dated 2026-06-12T15-43) returned 0 FAIL and 0
blocking-PARTIAL, but produced a **Major** code-review finding that renders the
feature's primary capability non-functional when its flag is enabled. The
orchestrator judges this material to issue #177's objective (deliver working
hierarchical folder prediction) and opens remediation cycle 1.

## Source audit artifacts

- `policy-audit.2026-06-12T15-43.md`
- `code-review.2026-06-12T15-43.md`
- `feature-audit.2026-06-12T15-43.md`
- `remediation-inputs.2026-06-12T15-43.md` (reviewer-authored finding list)

## In-scope findings for cycle 1

### F1 [Major, REQUIRED] Flag-on LCPPN path unreachable in production

- Files: `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`
  (lines ~38, 78-90, 274-282) and callers `EmailFiler.cs`, `SortEmail.cs`, `FolderScorer.cs`.
- Problem: `_lcppnPredictor` and `FolderPredictorConfig` are per-instance state.
  Production callers construct a fresh `new OlFolderClassifierGroup(globals)` per call,
  so the predictor built at the registration site on a different instance is never
  returned. With `UseLcppnPredictor=true` the callers silently keep using the flat
  predictor.
- Required outcome: when the flag is on, the Folder call sites in `EmailFiler`,
  `SortEmail`, and `FolderScorer` must receive the built `LcppnFolderPredictor`
  through `IFolderPredictor`; when the flag is off, behavior is byte-for-byte
  unchanged (AC13 must remain PASS).
- HARD constraint (carries over from Option B): the fix MUST NOT retype the shared
  `Globals.AF.Manager` (`ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>`)
  value type, MUST NOT modify `ManagerAsyncLazy.cs`, and MUST NOT modify any
  out-of-scope classifier subsystem (`Triage.cs`, `SpamBayes.cs`,
  `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`). The planner selects the
  localized mechanism (for example, resolve the built predictor and config from a
  shared location reachable by the callers, or have the callers obtain the
  registration-time instance) and justifies it against the design policy.
- Verification: a test must prove the flag-on path returns the `LcppnFolderPredictor`
  to a caller-equivalent code path, not just to the build-time instance.

### F2 [Minor, REQUIRED] Strict new-code coverage below 90% for two real-logic types

- `FolderHierarchyTree.cs` — 86.4% strict / 91.4% inclusive.
- `LcppnFolderPredictor.cs` — 89.1% strict / 91.4% inclusive.
- Required outcome: add deterministic MSTest tests (Moq + FluentAssertions) raising
  both types to >= 90% strict line coverage, exercising `FolderHierarchyTree`
  `GetChildren`/`NodeKeys` accessors and the uncovered `LcppnFolderPredictor` descent
  branches. No temp files; pure logic; no Outlook COM.

## Out-of-scope for cycle 1 (recorded, not remediated here)

- `FolderHierarchyNode.cs` strict 60.0% / inclusive 100.0%: the strict shortfall is
  auto-generated record members; every line is exercised (inclusive 100%). Accepted;
  no action.
- `BayesianClassifierGroup.cs` 515 lines (> 500 cap): the file was already 513 lines
  (over-cap) before this feature; the feature added only the +2 `: IFolderPredictor`
  declaration. Splitting the class is a separate refactor outside #177's scope.
  Recorded as a follow-up; not remediated in this cycle. (Pre-existing over-cap
  `SortEmail.cs` 1406 and `FolderScorer.cs` 608 are likewise out of scope.)

## Exit condition for cycle 1

End-of-cycle feature-review (three reaudit artifacts) must show `blocking_count == 0`
AND the F1 Major finding resolved (flag-on path reachable, AC13 preserved) AND F2
types at >= 90% strict coverage, with the full C# toolchain green in a single final pass.
