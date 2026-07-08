# Feature Audit — store-wrapper-launch-npe (Issue #240)

- Timestamp: 2026-07-06T13-00
- Review type: Re-audit after remediation (cycle 2)
- Work mode: `minor-audit`
- AC source (per work-mode routing): `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/issue.md`, explicit `## Acceptance Criteria` section only

## Scope and Baseline

- Base branch (resolved): `main` @ `4022fe7c9b07119224ca5aaa880b0a4003ef08db`
- Head: `TaskMaster-wt-2026-07-06-06-35` @ `9e3615b9dd369e66338b4ad333fb7c5371ece0dd`
- Full branch diff audited (per Scope Invariant): 49 files changed, 1949 insertions(+), 406 deletions(-) across the full range — 5 `.cs`/`.csproj` files (1 production, 3 test, 1 project file) plus 44 documentation/memory files.
- This cycle's own commit (`9e3615b9`, remediation of the prior cycle's file-size finding): 8 files changed — 3 test `.cs` files, 1 `.csproj`, 2 agent-memory files, 2 prior-cycle review artifacts already present in the working tree.
- Production change remains confined to `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` (396 lines, zero diff in this cycle — independently confirmed via `git diff dfbebb13..9e3615b9 -- UtilitiesCS/`). Test code is now split across three files (181 / 396 / 234 lines) instead of one 781-line file. No changes to `TaskMaster/Ribbon/RibbonController.cs` or `TaskMaster/AppGlobals/AppOlObjects.cs` (verified via `git diff --name-only` over the full range), matching the plan's declared small-path scope lock.

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

### AC1 — PASS (unchanged from prior cycle, re-verified independently)

`Launch()` calls `EvaluateLaunchReadiness()` first; when `Globals?.Ol?.StoresWrapper` is null, the state is `ModelUnavailable` and `Launch()` shows a `MyBox.ShowDialog(...)` message and returns without constructing `Viewer`. This code is unchanged by this cycle's split commit (zero diff to `StoreWrapperController.cs`). The regression test `Launch_WhenStoresWrapperIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer` (now in `StoreWrapperController_Tests.Launch.cs`) was independently re-run this cycle and passed.

### AC2 — PASS (unchanged from prior cycle, re-verified independently)

The `StoresUnavailable` branch is unchanged. `Launch_WhenStoresListIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer` was independently re-run this cycle and passed.

### AC3 — PASS (unchanged from prior cycle, re-verified independently)

Both regression tests, now relocated verbatim to `StoreWrapperController_Tests.Launch.cs`, still use MSTest, Moq, and FluentAssertions with no live Outlook process and no temporary files (confirmed by direct reading of the post-split file). Fail-before/pass-after evidence from the prior cycle remains valid because the production code and test bodies are unchanged by this cycle's split.

### AC4 — PASS (unchanged from prior cycle, re-verified independently)

`EvaluateLaunchReadiness()` is unchanged (zero diff to `StoreWrapperController.cs`). All 5 `EvaluateLaunchReadiness_*` unit tests, now in `StoreWrapperController_Tests.Launch.cs`, were independently re-run this cycle (part of the 39/39 passing re-run) and passed.

### AC5 — PARTIAL (unchanged verdict from prior cycle; independently re-confirmed, not merely carried forward)

- Toolchain order: independently re-verified this cycle. Csharpier (`EXIT_CODE 0`, 4 files checked) and a scoped analyzer build (`EXIT_CODE 0`, 0 errors) both pass cleanly on the touched files. The nullable gate's solution-wide `EXIT_CODE 1` was independently reproduced and confirmed confined to the same two pre-existing, out-of-scope vendored projects (`SVGControl.csproj`, `UtilitiesSwordfish.NET.General.csproj`) with zero occurrences of `StoreWrapperController` anywhere in the build log. This portion remains satisfied.
- New-code coverage on changed lines: still PASS. `EvaluateLaunchReadiness()` and its factory methods are unchanged by this cycle and remain at 100% line/block coverage per the prior cycle's coverage run; independently corroborated this cycle by the targeted 39/39 test pass (a coverage regression would require a test failure, which did not occur).
- "Repository line coverage remains >= 80% for the testable denominator": still **not substantiated at the scope the phrase implies**. No canonical `artifacts/csharp/coverage.xml` exists (independently re-confirmed this cycle — see `policy-audit.2026-07-06T13-00.md` §1.2.2). The cited 85.88% figure remains scoped to the single `UtilitiesCS.dll` module. This clause remains unverifiable as PASS, so AC5 is evaluated PARTIAL, unchanged from the prior cycle.

### AC6 — UNVERIFIED (deferred, unchanged)

`evidence/other/ac6-deferral.md` explicitly defers AC6 to post-PR-creation CI. This review has no GitHub CLI access and cannot independently verify CI status against the current head SHA (`9e3615b9`). AC6 remains correctly unchecked in `issue.md`.

## Summary

The prior cycle's file-size remediation (commit `9e3615b9`) is verified, independently, to be a clean, behavior-preserving split: all 39 tests preserved and passing, zero production-code diff, all three resulting files within the 500-line limit, and the C# toolchain green for the touched files (csharpier and scoped analyzer build both `EXIT_CODE 0`; the solution-wide nullable gate's pre-existing `EXIT_CODE 1` is confined to two unrelated vendored projects). AC1-AC4 remain PASS, re-verified independently rather than merely re-asserted. AC5 remains PARTIAL for the same reason as the prior cycle: the "repository line coverage >= 80%" clause is not substantiated at genuine repo-wide scope because no canonical `artifacts/csharp/coverage.xml` exists. AC6 remains correctly deferred pending PR/CI evidence. No acceptance-criteria check-off changes were made by this review; the file-size finding that previously accompanied AC5's evaluation context is now resolved, but the coverage-artifact gap that separately drives AC5's PARTIAL verdict is unchanged and is documented again in this cycle's `remediation-inputs.2026-07-06T13-00.md` for maintainer follow-up.

## Acceptance Criteria Check-off

- AC1: `[x]` in `issue.md` — consistent with this review's PASS verdict. No change made.
- AC2: `[x]` in `issue.md` — consistent with this review's PASS verdict. No change made.
- AC3: `[x]` in `issue.md` — consistent with this review's PASS verdict. No change made.
- AC4: `[x]` in `issue.md` — consistent with this review's PASS verdict. No change made.
- AC5: `[x]` in `issue.md` — **discrepancy persists, unchanged from prior cycle**. This review's verdict remains PARTIAL for the reason stated above. Per the AC check-off protocol, reviewers do not retroactively remove an executor's existing check-off; this checkbox is left as `[x]` and the gap is again flagged here and in `remediation-inputs.2026-07-06T13-00.md`.
- AC6: `[ ]` in `issue.md` — consistent with this review's UNVERIFIED/deferred status. No change made.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/issue.md` (`## Acceptance Criteria`)
- Total AC items: 6
- Checked off (delivered): 5 (AC1-AC5, pre-existing from executor; AC5 carries a flagged discrepancy — see above)
- Remaining (unchecked): 1
- Items remaining: AC6 ("All required PR CI checks are green against the PR head SHA.") — correctly deferred pending PR/CI evidence.
