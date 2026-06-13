# Phase 0 — global.json Baseline (Issue #194)

Timestamp: 2026-06-13T11-22

Command: Read repo-root global.json (c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-12-10-29\global.json)
EXIT_CODE: 0

Output Summary:
- sdk.version (baseline): 10.0.200
- sdk.rollForward: latestFeature (present)
- sdk.allowPrerelease: false (present)
- sdk.paths: [".dotnet-sdk", "$host$"] (present)
- sdk.errorMessage: present (references Install-RepoDotNetSdk.ps1)
- Baseline confirms the regressed value 10.0.200; revert target is 8.0.205.
