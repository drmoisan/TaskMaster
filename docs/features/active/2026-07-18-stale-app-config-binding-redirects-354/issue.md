# stale-app-config-binding-redirects (Issue #354)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/stale-app-config-binding-redirects/ (Issue #354)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #354
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/354
- Last Updated: 2026-07-18
- Work Mode: minor-audit

## Summary

Several first-party projects' `app.config` assembly `<bindingRedirect>` entries are stale relative to the assembly version actually referenced by the project's `.csproj`, so the CLR redirects a dependent assembly reference to a version that does not match the DLL physically restored at the `HintPath`, throwing `FileLoadException` at runtime.

## Environment

- OS/version: Windows, .NET Framework 4.8.1 (net481), MSBuild 18.8.2
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
- Data source or fixture: QuickFiler.Test unit test suite

## Steps to Reproduce

1. Restore NuGet packages so `Microsoft.Bcl.TimeProvider.10.0.10` is present under `packages\`.
2. Confirm `QuickFiler.Test\QuickFiler.Test.csproj` references `Microsoft.Bcl.TimeProvider, Version=10.0.0.10` with a matching `HintPath`.
3. Confirm `QuickFiler.Test\app.config` still has `<bindingRedirect oldVersion="0.0.0.0-10.0.0.7" newVersion="10.0.0.7" />` for `Microsoft.Bcl.TimeProvider`.
4. Build and run `QfcHomeControllerMetricsTests` / `QfcStreamingDequeueConfidenceGateTests`.

## Expected Behavior

The binding redirect's `newVersion` should equal the actual assembly version present at the `HintPath` so the CLR loads the assembly it redirects to without a manifest mismatch.

## Actual Behavior

```
System.IO.FileLoadException: Could not load file or assembly 'Microsoft.Bcl.TimeProvider, Version=10.0.0.7, ...'
 ---> System.IO.FileLoadException: Could not load file or assembly 'Microsoft.Bcl.TimeProvider, Version=8.0.0.1, ...'
The located assembly's manifest definition does not match the assembly reference.
```
8 of 21 tests in the affected classes fail. Confirmed locally that correcting the redirect's `newVersion` from `10.0.0.7` to `10.0.0.10` takes the same 21 tests from 8 failing to 0 failing.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: see Actual Behavior above (captured from a local `vstest.console.exe` run).

## Impact / Severity

- [x] High

Not test-only: `app.config` binding redirects govern assembly loading for the actual VSTO add-in at runtime too, so any code path in an affected project that loads one of the mismatched assemblies is exposed to the same `FileLoadException`, not just the unit tests that happen to exercise it.

## Suspected Cause / Notes

Root cause: a prior remediation pass (`fix_reference_versions.ps1`, run while resolving Dependabot NuGet PRs #343-#348) corrected each project's `<Reference Include="...">` `Version=` attribute to match the real assembly version read from the restored DLL, but never updated the sibling `app.config` `<bindingRedirect>` entry for the same assembly. An audit script (`fix_binding_redirects.py`, cross-referencing each project's `.csproj` Reference version against its `app.config` bindingRedirect `newVersion`) found **57 stale redirects on `main`** as of 2026-07-18, spanning QuickFiler, QuickFiler.Test, Tags.Test, TaskMaster, TaskMaster.Test, TaskTree.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS, UtilitiesCS.Test, and VBFunctions.Test, across packages: `Microsoft.Bcl.TimeProvider`, the `Microsoft.Extensions.*` family, `Microsoft.Identity.Client`(`.Extensions.Msal`), the `Microsoft.IdentityModel.*` family, `System.ClientModel`, `System.IdentityModel.Tokens.Jwt`, `System.Collections.Immutable`, `System.Diagnostics.DiagnosticSource`, `System.Formats.Asn1`, `System.Memory.Data`, `System.Net.Http.WinHttpHandler`, `System.Reflection.Metadata`, `System.Security.Cryptography.ProtectedData`, `System.Text.Encoding.CodePages`, `System.Text.Encodings.Web`, `System.Text.Json`, `Microsoft.ApplicationInsights`, and `Microsoft.Web.WebView2.Core`.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: existing `QfcHomeControllerMetricsTests` and `QfcStreamingDequeueConfidenceGateTests` already regression-test this once the redirects are corrected; no new test code needed since this is a config-only fix with existing coverage.
- [ ] Integration scenario to retest: full MSTest suite via `vstest.console.exe` across the solution to confirm no other project is newly exposed.
- [ ] Manual verification notes: for each project's `app.config`, cross-check every `<bindingRedirect>` `newVersion` against the corresponding `.csproj` `<Reference Version=...>` and correct any mismatch; no production `.cs` source changes required.

## Acceptance Criteria

- [x] AC1: Every `<bindingRedirect>` entry in every first-party project's `app.config` has a `newVersion` (and an `oldVersion` upper bound) equal to the actual assembly version referenced by that project's `.csproj` `<Reference Include="...", Version=...>` for the same assembly (matched by package id + publicKeyToken).
- [x] AC2: No production `.cs` source file is modified; the fix is confined to `app.config` files.
- [x] AC3: `QfcHomeControllerMetricsTests` and `QfcStreamingDequeueConfidenceGateTests` (previously 8 failing tests reproduced locally) pass with 0 failures after the fix.
- [x] AC4: The full solution builds cleanly (CSharpier format, .NET analyzers, nullable) with zero errors after the fix.
- [x] AC5: The full MSTest suite runs via `vstest.console.exe` across the solution with no new failures introduced relative to the pre-fix baseline (excluding failures already attributable to the stale redirects being fixed).

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
