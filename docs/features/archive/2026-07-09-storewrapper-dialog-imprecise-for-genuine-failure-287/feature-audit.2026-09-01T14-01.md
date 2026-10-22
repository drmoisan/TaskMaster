# Feature Audit — storewrapper-dialog-imprecise-for-genuine-failure (#287)

- Reviewed: 2026-09-01
- Work mode: `full-bug` (from `issue.md:12`)
- AC source (authoritative, per `acceptance-criteria-tracking` skill for `full-bug`): `spec.md` v2.0 only. `issue.md`'s "Acceptance Criteria" section mirrors `spec.md` verbatim and was cross-checked for drift — no divergence found.
- Base anchor used: `09eae2e85cd586c092fb1977a76cd9e895ec0a3b` (independently recomputed via `git merge-base HEAD origin/main`, matches caller-supplied SHA).
- HEAD verified: `564792e57aa2a6f0088d0b4f727bdf86a115c92a`.

## Overall verdict: PASS

## Acceptance Criteria Status

- Source: `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/spec.md`
- Total AC items: 16
- Checked off (delivered, independently verified by this review): 16
- Remaining (unchecked): 0
- Items remaining: none

All 16 items were already checked `[x]` in both `spec.md` and `issue.md` by the executor. Every item was independently re-verified by this review (not merely trusted from the executor's self-report); no check-off action was required since none were left unchecked.

## AC-by-AC verification table

| AC | Text (abbreviated) | Verdict | Independent evidence |
|---|---|---|---|
| AC1 | Two pure `internal static` methods, no `System.Windows.Forms`/`MyBox`, no I/O, no `[ExcludeFromCodeCoverage]` | PASS | Direct read of `StoreLaunchReadinessEvaluator.cs:56-93`: pure switch expressions, no WinForms/MyBox using or reference, no attribute. |
| AC2 | No new source/test file; no `.csproj`/`.props`/`.targets`/`packages.config` modified | PASS | `git diff --name-only <BASE>..HEAD` outside the feature folder = exactly the 5 D1 files; no project-file extension present. |
| AC3 | Message differs between `ModelUnavailable` and `StoresUnavailable`, asserted by a named test | PASS | `BuildUnavailableMessage_ForTheTwoNonReadyStates_ReturnsDifferentStrings` (diff, `StoreWrapperController_Tests.Launch.cs`); also confirmed by direct string comparison of the two switch-arm literals. |
| AC4 | Both methods throw `ArgumentOutOfRangeException` for `Ready` | PASS | `BuildUnavailableMessage_WhenReady_ThrowsArgumentOutOfRangeException`, `BuildUnavailableTitle_WhenReady_ThrowsArgumentOutOfRangeException` present in diff; source confirms `Ready => throw new ArgumentOutOfRangeException(...)` in both switch expressions. |
| AC5 | Both methods return `ModelUnavailable` copy for an undefined enum cast value | PASS | `BuildUnavailableMessage_WhenStateIsUndefinedCast_ReturnsModelUnavailableCopy` / `BuildUnavailableTitle_WhenStateIsUndefinedCast_ReturnsModelUnavailableTitle` (cast `(StoreLaunchReadinessState)99`) present in diff; source's discard arm `_ =>` returns the `ModelUnavailable` copy for both `ModelUnavailable` and any undefined value. |
| AC6 | Neither `Launch()` contains a dialog literal; case-sensitive search for the old literal over `*.cs` = 0 matches | PASS | `Grep` (case-sensitive, `*.cs`) for `Store settings are not available yet` returned "No matches found" in this review's own independent search. |
| AC7 | Both `Launch()` keep `[ExcludeFromCodeCoverage]`, same gate/buttons/icon, still return without constructing a viewer | PASS | Diff shows both `Launch()` bodies unchanged except the two literal arguments replaced by two method calls; `[ExcludeFromCodeCoverage]` attribute present and unmoved at both sites (grep confirms `StoreWrapperController.cs:116`). |
| AC8 | Tests assert exact message/title through `MyBox.DialogInvoker` for both states at `StoreWrapperController.Launch()`, and that the two differ | PASS | Both existing `Launch_When...` tests extended with `capturedTitle`/`capturedMessage` assertions against the exact D2 strings; new `Launch_ForModelUnavailableAndStoresUnavailable_ShowsDifferentMessages` asserts the two differ end-to-end. |
| AC9 | Equivalent tests exist for `DisabledStoresController.Launch()`; `Viewer` stays null; no viewer constructed | PASS | Two new `[TestMethod]`s in `DisabledStoresControllerTests.cs` assert exact copy and `GetInternalProperty<IDisabledStoresViewer>(controller, "Viewer").Should().BeNull()`; diff shows no `DisabledStoresViewer`/`StoreWrapperViewer` construction added. |
| AC10 | Stale `DisabledStoresController.Launch` XML summary corrected | PASS | `grep -n "shows the same warning" DisabledStoresController.cs` = no match (exit 1); diff shows the summary rewritten to describe state-specific behavior. |
| AC11 | Evaluator XML doc records `ModelUnavailable` as the terminal state after the caught `LoadStoresAsync` failure, citing the location | PASS | Diff adds the citation `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72`; direct read of that file confirms a `catch (Exception e)` block at those exact lines that logs an `Error` line and leaves `StoresWrapper` unset with no retry — citation is accurate, not fabricated. |
| AC12 | All 5 existing readiness tests in `StoreWrapperController_Tests.Launch.cs` still pass; `Evaluate`'s return values unchanged | PASS | Diff hunks in that file stop before the `#region EvaluateLaunchReadiness` region (`Evaluate` untouched by construction); raw test log `coverage/p3-testrun.log` tail confirms `Total tests: 6912 / Passed: 6912`. |
| AC13 | Full C# toolchain passes in order, zero errors, single final pass | PASS | Independently re-verified from raw logs: CSharpier check clean (re-run by this review on the 5 files: "Checked 5 files... " exit 0); analyzer build 0 errors / 5 pre-existing unrelated warnings (`coverage/p3-analyzer-build.log`); nullable build 0 errors / same 5 warnings (`coverage/p3-nullable-build.log`); MSTest 6912/6912 passed (`coverage/p3-testrun.log`). |
| AC14 | No production file added to a coverage exclusion; two new methods measured >= 90% | PASS | Independently re-parsed `coverage/post-change.cobertura.xml`: `StoreLaunchReadinessEvaluator` class `line-rate=1`, `BuildUnavailableMessage` and `BuildUnavailableTitle` each `line-rate=1`/`branch-rate=1`, 0 uncovered lines. `git diff --name-only <BASE>..HEAD -- coverage.config TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings` empty. |
| AC15 | `StoreWrapperController.cs` under 500 lines; no touched file exceeds 500; post-change counts recorded | PASS | Independently re-measured with `awk 'END{print NR}'`: 96 / 478 / 181 / 480 / 373 — all under 500. Recorded in `evidence/qa-gates/ac15-file-sizes.md`, matches independent count exactly. |
| AC16 | No file outside the five listed in the design-summary table is modified | PASS | `git diff --name-only <BASE>..HEAD -- . ":(exclude)docs/features/active/.../" ":(exclude).claude"` = exactly the 5 D1 files, independently re-run by this review with the same result. |

## Coverage floor note (cross-reference to policy-audit)

The repository carries two unreconciled coverage-floor documents (CLAUDE.md: 80%/90%; `.claude/rules/quality-tiers.md` + `general-unit-test.md`: uniform 85%/75%). This PR's measured figures (repo-wide 85.297% line / 79.293% branch; new-code 100%/100%) clear both candidate floors, so the reconciliation gap does not affect this PR's verdict. See `policy-audit.2026-09-01T14-01.md` "Coverage floor reconciliation" section. Not a blocking finding for #287; flagged for maintainer attention as an open documentation-consistency item, out of this branch's own scope (AC16 footprint would forbid touching either policy document from this branch even if it were in scope).

## Follow-up items (not promoted, out of branch scope)

Per the task instruction, these are listed for the orchestrator to report and potentially file as a single consolidated post-merge issue. None were promoted from this branch, and no new issue or potential entry was created, to preserve AC16's exact five-file-plus-feature-folder footprint.

1. **`StoreWrapperController` is entirely absent from the Cobertura coverage report, in both baseline and post-change XML.** Independently confirmed by parsing both `coverage/baseline.cobertura.xml` and `coverage/post-change.cobertura.xml`: no `class` element named `UtilitiesCS.OutlookObjects.Store.StoreWrapperController` appears in either file, even though the class is `public` and carries `[ExcludeFromCodeCoverage]` on only 2 of its members (`Launch()` at line 116 and one other method at line 438), meaning the rest of the class should be instrumented and measurable. This is pre-existing (identical absence in both baseline and post-change, so this branch neither introduced nor fixed it) and does not affect this PR's AC14 or repo-wide coverage verdicts, but it means any currently-untested surface of `StoreWrapperController` outside `Launch()` is invisible to the repo-wide coverage metric rather than being counted as a shortfall. Worth a dedicated investigation into why the class is dropped from the Cobertura output (assembly-load timing, Koverage post-processing filter, or a per-class instrumentation gap) independent of this bugfix.
2. **The `ModelUnavailable`/`StoresUnavailable` copy strings are duplicated as inline literals** across two switch-expression arms in production code and again across roughly six MSTest assertion sites in two test files, with no single source-of-truth constant. `spec.md` explicitly considered and rejected a tuple/struct-returning alternative for `net48`/`LangVersion 12.0` reasons; the duplication is consistent with the pre-existing pattern in this codebase (the literal it replaced was itself duplicated at both former call sites) and is not a regression. A future edit to either string requires remembering all occurrences by hand.
3. **The genuine spec-documented follow-up**: `spec.md`'s own "Rollout & Follow-up" section already identifies that `ModelUnavailable` conflates "startup has not reached the store-load phase" with "the store-load phase completed through its catch block," and that resolving this ambiguity would require a new readiness state crossing the `UtilitiesCS`/`TaskMaster` boundary — explicitly scoped out of #287 and explicitly flagged by the spec author as warranting its own future issue. This item is restated here only for completeness of the follow-up list; it was already correctly identified as out-of-scope by the spec itself, not a gap found by this review.

## Verdict summary

feature-audit: PASS. 16/16 AC independently verified. 0 blocking findings. 3 non-blocking follow-up items listed above, not promoted per task instruction (AC16 footprint preservation for this parallel-batch item).
