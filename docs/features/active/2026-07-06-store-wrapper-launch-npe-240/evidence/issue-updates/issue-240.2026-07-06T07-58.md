Timestamp: 2026-07-06T07-58

PostedAs: unknown (local mirror only; not posted to GitHub by this executor run)

Exact text applied to `## Acceptance Criteria` in `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/issue.md`:

```
## Acceptance Criteria

- [x] AC1: `StoreWrapperController.Launch()` does not throw an unhandled `NullReferenceException` when `Globals.Ol.StoresWrapper` (`Model`) is null. It fails gracefully with a clear user-facing message and returns without opening a broken dialog. (Evidence: `evidence/regression-testing/pass-after-240.md`, fix in `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` P2-T3.)
- [x] AC2: `Launch()` also handles a non-null `Model` whose `Stores` list is null (transient post-deserialize state) without throwing. (Evidence: `evidence/regression-testing/pass-after-240.md`, fix in `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` P2-T3.)
- [x] AC3: A deterministic MSTest regression test reproduces the pre-fix crash path (fails before the fix, passes after) using Moq for `IApplicationGlobals`/`IOlObjects`; no live Outlook, no temporary files. (Evidence: `evidence/regression-testing/fail-before-240.md` and `evidence/regression-testing/pass-after-240.md`, P1-T3/P2-T5.)
- [x] AC4: The underlying readiness/initialization gap identified by root-cause research is addressed so that invoking the store-settings command when store state is unavailable produces deterministic, non-crashing behavior rather than an unhandled exception. (Evidence: `EvaluateLaunchReadiness()` in `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`, P2-T1/P2-T2/P2-T3.)
- [x] AC5: The full C# toolchain passes in order (csharpier -> .NET analyzers -> nullable/TreatWarningsAsErrors -> MSTest with coverage); coverage on changed lines meets the >= 90% new-code target and repository line coverage remains >= 80% for the testable denominator. (Evidence: `evidence/qa-gates/qa-01-format.md` through `evidence/qa-gates/qa-05-coverage-delta.md`, P3-T1 through P3-T5. Note: the solution-wide nullable gate's raw `EXIT_CODE` is 1 due to a pre-existing, unrelated condition documented in `evidence/qa-gates/qa-03-nullable.md`; the touched files themselves introduce zero new nullable diagnostics.)
- [ ] AC6: All required PR CI checks are green against the PR head SHA.
```
