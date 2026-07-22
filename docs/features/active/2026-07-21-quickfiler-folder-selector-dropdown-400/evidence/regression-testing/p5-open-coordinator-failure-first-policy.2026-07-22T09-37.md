# P5 Open Coordinator Failure-First Policy Audit

Timestamp: 2026-07-22T09:37:58.7676253Z

Command: Read-only SHA-256 reconciliation of the eight P5 production sources, both project files, and `coverage.config` against `p5-numeric-coverage-scope-ledger.2026-07-22T09-33.md`; exclusion/test inventory checks with `Select-String`; and `git diff --check` on the J0 boundary.

EXIT_CODE: `0`

Output Summary: PASS. The failure-first fixture is deterministic MSTest with FluentAssertions. It contains exactly five `[TestMethod]` cases and one `AssertionScope`. The new case uses only assembly, type, member, and attribute reflection. The prohibited-resource scan returned zero matches for source-file reads, timing, process, UI/WebView construction, and temporary-file behavior.

## Five-case inventory

1. `ExistingAnchor_RemainsTheDesignerWebViewClosedSurface`
2. `ProductionConfiguration_AcceptsExistingEnvironmentAndInitializer`
3. `InjectedConfiguration_AcceptsHostAndScreenGeometryProviders`
4. `ExistingFolderEventsAndDropDownIntentSignatures_AreUnchanged`
5. `HostNeutralPopupOpenOrchestration_IsOwnedByInstrumentedCoordinator`

The exact filtered VSTest run discovered all five: the four existing contracts passed and only case 5 produced the intended aggregated omission failure.

## Protected-baseline reconciliation

All eight production SHA-256 values matched the P5-T104 ledger: `BreadcrumbUiDispatcher.cs`, `BreadcrumbWebViewSurfaceFactory.cs`, `BreadcrumbPopupUiOperations.cs`, `BreadcrumbDropDownOpenLifetime.cs`, `BreadcrumbDropDownHost.cs`, `BreadcrumbMessengerHub.cs`, `BreadcrumbCollapsedSurfaceController.cs`, and `ItemViewer.Breadcrumb.cs`.

- `QuickFiler.csproj`: `AE9E7B33BD3A15E4D84F300FCA4F42ADDF49906FE456F69C0DE2FEDD9E990829` (unchanged)
- `QuickFiler.Test.csproj`: `7DD0D954DE93C53CEEC0EE1F51D59DCA00DD9E0C59FA7393BC759AE85C445FDB` (unchanged)
- `coverage.config`: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` (unchanged)
- `ItemViewer.Breadcrumb.cs` method exclusions: exactly 2 (unchanged)
- `BreadcrumbPopupUiOperations.cs` method exclusions: exactly 7 (unchanged)
- Proposed coordinator production source: absent, as required before J1
- Authorized J0 test SHA-256: `56391C38A5A4EF599AA82D8BB981A29C61A968753D1604D3B7081CB49090C817`

`git diff --check` returned no whitespace errors for the protected J0 boundary.
