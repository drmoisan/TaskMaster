# Phase 0 — Instructions and Policy Read (Cycle 2, Issue #292)

Timestamp: 2026-07-09T17-45

## P0-T1 — Remediation Inputs Read

Read in full: `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/remediation-inputs.2026-07-09T17-45.md`

Finding (single Major, non-CI-blocking, closing in-PR): the PR's new regression test class
`TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` both opens a
`CurrentStoreContext` process-global-static scope (via `wrapper.Init()` / `RewireAfterDeserializeAsync()`)
and reads the null baseline (`CurrentStoreContext.Current.Should().BeNull()`), yet is not marked
`[DoNotParallelize]`. This is the same shared-static parallel-race class fixed in cycle 1 for `UtilitiesCS.Test`.

Two required actions:
1. Mark the target class `StoresWrapperEnumerationScopeTests` `[DoNotParallelize]`.
2. Census `TaskMaster.Test` for any other `CurrentStoreContext` scope-opening / null-baseline-reader class
   that is unmarked and mark any found.

Scope class: minor, test-attribute-only (no production behavior change).

## P0-T2 — Policy Read

Policy Order:
1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

Files read (in the above order): all four listed files were read. Key constraints confirmed:
CSharpier formatting (not `dotnet format`); MSTest + Moq + FluentAssertions; no weakened assertions;
no sleeps/retries/timing hacks; no temp files; repository-wide line coverage >= 80% testable denominator;
changed-line coverage not reduced; full toolchain loop (format -> lint -> type-check -> test) restarting
on any change/failure.
