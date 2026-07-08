# AC Source Confirmation (Issue #240)

Timestamp: 2026-07-06T07-01

Confirmation: `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/issue.md` contains an explicit `## Acceptance Criteria` heading. AC1-AC6 count = 6.

AC1-AC6 verbatim (as found under `## Acceptance Criteria`):

- AC1: `StoreWrapperController.Launch()` does not throw an unhandled `NullReferenceException` when `Globals.Ol.StoresWrapper` (`Model`) is null. It fails gracefully with a clear user-facing message and returns without opening a broken dialog.
- AC2: `Launch()` also handles a non-null `Model` whose `Stores` list is null (transient post-deserialize state) without throwing.
- AC3: A deterministic MSTest regression test reproduces the pre-fix crash path (fails before the fix, passes after) using Moq for `IApplicationGlobals`/`IOlObjects`; no live Outlook, no temporary files.
- AC4: The underlying readiness/initialization gap identified by root-cause research is addressed so that invoking the store-settings command when store state is unavailable produces deterministic, non-crashing behavior rather than an unhandled exception.
- AC5: The full C# toolchain passes in order (csharpier -> .NET analyzers -> nullable/TreatWarningsAsErrors -> MSTest with coverage); coverage on changed lines meets the >= 90% new-code target and repository line coverage remains >= 80% for the testable denominator.
- AC6: All required PR CI checks are green against the PR head SHA.

This section (only) is treated as the AC source for this minor-audit plan.
