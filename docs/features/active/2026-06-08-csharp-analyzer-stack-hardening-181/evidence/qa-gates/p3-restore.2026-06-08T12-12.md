# P3-T17 / P3-T18 — Restore Verification After SecurityCodeScan packages.config Cleanup (Issue #181)

Timestamp: 2026-06-08T13-22

> Revision 2.0 record. Supersedes the v1.0 record which verified the 6-analyzer set (including SecurityCodeScan.VS2019). Phase 3 cleanup (P3-T2..P3-T16) removed the SecurityCodeScan.VS2019 `<package>` entry from all 15 first-party packages.config files, reducing each to the 5 in-scope analyzers.

## P3-T17 — Solution restore with 5-analyzer packages.config entries

Command: `nuget.exe restore TaskMaster.sln`
EXIT_CODE: 0
Output Summary:
- MSBuild auto-detection: msbuild 18.6.3.22110.
- "All packages listed in packages.config are already installed." Restore succeeded.
- The five in-scope analyzer packages are present under `packages/`:
  - Meziantou.Analyzer.3.0.101 — PRESENT
  - SonarAnalyzer.CSharp.10.27.0.140913 — PRESENT
  - Roslynator.Analyzers.4.15.0 — PRESENT
  - AsyncFixer.2.1.0 — PRESENT
  - Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4 — PRESENT
- No `packages.config` references SecurityCodeScan.VS2019. Repo-wide search of `**/packages.config` for `SecurityCodeScan` returned 0 matches.
- Each of the 15 first-party `packages.config` files contains exactly 5 in-scope analyzer `<package>` entries (75 total across 15 files), each with `developmentDependency="true"` and `targetFramework="net481"`.
- Note: the `SecurityCodeScan.VS2019.5.6.7` package directory may still exist under `packages/` from the prior restore, but it is no longer referenced by any packages.config and will not be wired into any project after Phase 4. It is removed from the analyzer wiring in Phase 4; the leftover package directory is not committed source and is cleaned by the orchestrator/CI restore from the committed packages.config set.

## P3-T18 — Vendored exclusion check

The 4 vendored projects' packages.config files were NOT modified:
- `SVGControl/packages.config` — present; analyzer-package entries = 0 (no Meziantou/Sonar/Roslynator/AsyncFixer/BannedApiAnalyzers/SecurityCodeScan).
- `SVGControl.Test/packages.config` — present; analyzer-package entries = 0.
- `UtilitiesSwordfish/packages.config` — NOT PRESENT (UtilitiesSwordfish.NET.General project has no packages.config; consistent with the plan's "if present" note).
- `UtilitiesSwordfish.Test/packages.config` — NOT PRESENT.

`git status --porcelain` on the four vendored packages.config paths returned no output, confirming none of the vendored packages.config files were changed by this work.

## Verdict
Restore is clean (EXIT_CODE 0) with the reduced 5-analyzer set. SecurityCodeScan.VS2019 is fully removed from all first-party packages.config files. No vendored packages.config was modified. P3-T17 and P3-T18 complete.
