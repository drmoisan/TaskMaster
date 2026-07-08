# Phase 0 — Baseline File Size and Exempt-Attribute Status (issue #211)

Timestamp: 2026-06-24T16-30
Command: `wc -l UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs TaskMaster/AppGlobals/ApplicationGlobals.cs TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs` and `grep ExcludeFromCodeCoverage <files>`
EXIT_CODE: 0

Output Summary:
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`: 185 lines. `[ExcludeFromCodeCoverage]`: NO (not exempt).
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`: 398 lines. `[ExcludeFromCodeCoverage]`: present (lifecycle entry-point class is exempt per CLAUDE.md COM/VSTO exemption). Headroom to 500: 102 lines.
- `TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs`: 344 lines. `[ExcludeFromCodeCoverage]`: NO — the only two `ExcludeFromCodeCoverage` text matches are inside XML doc `<remarks>` explicitly stating the types are intentionally NOT exempt (the coverable formatting seam). Headroom to 500: 156 lines.

Conclusion: StoreWrapper and StartupDiagnosticsProbe are coverable (not exempt), consistent with the plan's verified context. ApplicationGlobals.cs and StartupDiagnosticsProbe.cs both have sufficient headroom for the additive instrumentation.
