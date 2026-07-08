# Feature Audit — store-wrapper-launch-npe (Issue #240)

- Timestamp: 2026-07-06T12-15
- Work mode: `minor-audit`
- AC source (per work-mode routing): `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/issue.md`, explicit `## Acceptance Criteria` section only

## Scope and Baseline

- Base branch (resolved): `main` @ `4022fe7c9b07119224ca5aaa880b0a4003ef08db`
- Head: `TaskMaster-wt-2026-07-06-06-35` @ `dfbebb13fdc9ce2e9240376be2214dddf56ee5d0`
- Full branch diff audited (per Scope Invariant): 23 files changed, 929 insertions(+), 4 deletions(-) — 2 `.cs` files (production + test), 21 `.md` files (issue, plan, research, evidence).
- Production change confined to `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` (396 lines after change, verified via direct line count). Test change confined to `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs` (781 lines after change). No changes to `TaskMaster/Ribbon/RibbonController.cs` or `TaskMaster/AppGlobals/AppOlObjects.cs` (verified via `git diff --name-only`), matching the plan's declared small-path scope lock.

## Acceptance Criteria Inventory

| AC | Text |
|---|---|
| AC1 | `StoreWrapperController.Launch()` does not throw an unhandled `NullReferenceException` when `Globals.Ol.StoresWrapper` (`Model`) is null. It fails gracefully with a clear user-facing message and returns without opening a broken dialog. |
| AC2 | `Launch()` also handles a non-null `Model` whose `Stores` list is null (transient post-deserialize state) without throwing. |
| AC3 | A deterministic MSTest regression test reproduces the pre-fix crash path (fails before the fix, passes after) using Moq for `IApplicationGlobals`/`IOlObjects`; no live Outlook, no temporary files. |
| AC4 | The underlying readiness/initialization gap identified by root-cause research is addressed so that invoking the store-settings command when store state is unavailable produces deterministic, non-crashing behavior rather than an unhandled exception. |
| AC5 | The full C# toolchain passes in order (csharpier -> .NET analyzers -> nullable/TreatWarningsAsErrors -> MSTest with coverage); coverage on changed lines meets the >= 90% new-code target and repository line coverage remains >= 80% for the testable denominator. |
| AC6 | All required PR CI checks are green against the PR head SHA. |

## Acceptance Criteria Evaluation

### AC1 — PASS

`Launch()` now calls `EvaluateLaunchReadiness()` first; when `Globals?.Ol?.StoresWrapper` is null, the state is `ModelUnavailable`, and `Launch()` shows a `MyBox.ShowDialog(...)` message and returns without constructing `Viewer`. Verified directly in the diff and confirmed by the regression test `Launch_WhenStoresWrapperIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer`: fails pre-fix with an unhandled `NullReferenceException` (`evidence/regression-testing/fail-before-240.md`), passes post-fix asserting no throw, one dialog invocation, and `Viewer == null` (`evidence/regression-testing/pass-after-240.md`).

### AC2 — PASS

The same `EvaluateLaunchReadiness()` path returns `StoresUnavailable` when `model.Stores` is null, taking the identical graceful-return branch. Verified by `Launch_WhenStoresListIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer`, which fails pre-fix with an unhandled `ArgumentNullException` (documented and explained precisely in `evidence/regression-testing/fail-before-240.md` as a more accurate characterization than "NullReferenceException" for this specific branch, while still satisfying AC2's "does not throw" requirement) and passes post-fix.

### AC3 — PASS

Both regression tests use MSTest (`[TestMethod]`), Moq (`Mock<IApplicationGlobals>`, `Mock<IOlObjects>`), and FluentAssertions (`Should().NotThrow()`, etc.), confirmed by direct code inspection. No live Outlook process is started (all Outlook-facing interfaces are mocked) and no temporary files are created or referenced anywhere in the new test code. Fail-before (`evidence/regression-testing/fail-before-240.md`, `EXIT_CODE 1`, 2 failed/0 passed) and pass-after (`evidence/regression-testing/pass-after-240.md`, `EXIT_CODE 0`, 4170 passed/0 failed) evidence is present and internally consistent with the diff.

### AC4 — PASS

`EvaluateLaunchReadiness()` is a deterministic, non-`[ExcludeFromCodeCoverage]` decision method that enumerates every readiness state identified in the issue's root-cause analysis (null `Globals`, null `Ol`, null `StoresWrapper`, null `Stores` list, and the ready state), replacing the previous unguarded dereference chain. This directly addresses the "underlying readiness/initialization gap" called out in `issue.md`'s Suspected Cause section. Verified via direct diff review and the 5 `EvaluateLaunchReadiness_*` unit tests, all passing.

### AC5 — PARTIAL

- Toolchain order (csharpier -> analyzers -> nullable -> MSTest/coverage) was followed. Csharpier and analyzer gates pass cleanly (`EXIT_CODE 0`). The nullable gate's solution-wide `EXIT_CODE 1` is a pre-existing, documented, unrelated condition in vendored projects (`SVGControl.csproj`, `Swordfish.NET.General.csproj`); a scoped rebuild confirms the two touched files introduce zero new nullable diagnostics. This portion is accepted as satisfied, consistent with AC5's own parenthetical caveat.
- New-code coverage on changed lines: **verified PASS**. `EvaluateLaunchReadiness()` and the two `StoreLaunchReadiness` factory methods show 100.00% line/block coverage in the underlying coverage data (`TestResults/final-coverage.xml`, function id 295736 and related entries), exceeding the >= 90% target.
- "Repository line coverage remains >= 80% for the testable denominator": **not substantiated at the scope the phrase implies**. The cited 85.88% figure (`evidence/qa-gates/qa-04-test-coverage.md`) is scoped to the single `UtilitiesCS.dll` module, not the full C# solution. No canonical repo-wide coverage artifact (`artifacts/csharp/coverage.xml`) exists. Inspection of the same underlying coverage run shows other first-party modules loaded during the test run (`TaskMaster.dll` 8.58%, `Tags.dll`/`ToDoModel.dll`/`QuickFiler.dll` 0.00%) — though these low figures likely reflect that those modules' own dedicated test projects were not exercised in this run, not a certified measurement of their true coverage. Net effect: AC5's coverage clause, as literally worded ("repository line coverage"), is not verifiable as PASS from available evidence, so this criterion is evaluated PARTIAL rather than PASS. See `policy-audit.2026-07-06T12-15.md` §1.2.2 and §5 for full detail.

### AC6 — UNVERIFIED (deferred, as declared)

`evidence/other/ac6-deferral.md` explicitly and correctly defers AC6 to post-PR-creation CI, since no PR/CI run exists during local execution. This review has no GitHub CLI access and cannot independently verify CI status; AC6 remains correctly unchecked in `issue.md`.

## Acceptance Criteria Check-off

- AC1: `[x]` in `issue.md` — consistent with this review's PASS verdict. No change made.
- AC2: `[x]` in `issue.md` — consistent with this review's PASS verdict. No change made.
- AC3: `[x]` in `issue.md` — consistent with this review's PASS verdict. No change made.
- AC4: `[x]` in `issue.md` — consistent with this review's PASS verdict. No change made.
- AC5: `[x]` in `issue.md` — **discrepancy noted**. This review's verdict is PARTIAL (see above), specifically because the "repository line coverage >= 80%" clause is not substantiated at true repo-wide scope. Per the AC check-off protocol, reviewers add check-offs for verified PASS items; they do not retroactively remove an executor's existing check-off. This checkbox is therefore left as `[x]` but the gap is flagged here and in `remediation-inputs.2026-07-06T12-15.md` for maintainer reconciliation (either narrow AC5's wording to the assembly actually measured, or obtain a genuine repo-wide coverage artifact).
- AC6: `[ ]` in `issue.md` — consistent with this review's UNVERIFIED/deferred status. No change made.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/issue.md` (`## Acceptance Criteria`)
- Total AC items: 6
- Checked off (delivered): 5 (AC1-AC5, pre-existing from executor; AC5 carries a flagged discrepancy — see above)
- Remaining (unchecked): 1
- Items remaining: AC6 ("All required PR CI checks are green against the PR head SHA.") — correctly deferred pending PR/CI evidence.

## Summary

Four of six acceptance criteria (AC1-AC4) are fully verified as PASS against direct code and test evidence. AC5 is PARTIAL: the toolchain-order and new-code-coverage clauses are verified PASS, but the "repository line coverage >= 80%" clause is not substantiated at genuine repo-wide scope — the cited figure is single-assembly. AC6 is correctly deferred and unverifiable without CI access. No acceptance-criteria check-off changes were made by this review; the AC5 discrepancy is documented for maintainer follow-up rather than unilaterally resolved.
