# Phase 8 — Final QC MSBuild Nullable Build (P8-T3)

Timestamp: 2026-07-11T00-38
Command: `MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true`
(Rebuild forces a genuine nullable recompile of the vendored projects; a plain incremental
`-t:Build` can skip already-built vendored assemblies and under-report. Dash-switch form +
MSYS_NO_PATHCONV=1 under git-bash.)
EXIT_CODE: 1

## Output Summary

The nullable/TreatWarningsAsErrors gate remains RED **only because of pre-existing vendored debt**,
identical to the Phase 0 baseline. After deduplicating the MSBuild `-m` multi-node output (each
error is emitted twice, once per node context — 168 raw → 84 unique), the error set is:

- `UtilitiesSwordfish.NET.General.csproj` — 50 errors
- `SVGControl.csproj` — 34 errors
- **Total unique: 84 (byte-for-byte the Phase 0 baseline set).**
- **First-party nullable errors (UtilitiesCS, TaskMaster, QuickFiler, TaskVisualization, and all
  .Test projects): 0.**

Distinct codes (unique set): CS8625 (26), CS8618 (26), CS8603 (9), CS8600 (8), CS8602 (6),
CS8601 (5), CS0649 (2), CS8619 (1), CS8604 (1) — matching the baseline distribution.

## No-Regression Conclusion

The F2 migration introduced **zero new first-party nullable diagnostics**. The gate produces the
SAME vendored-only 84-error set as the Phase 0 baseline (`evidence/baseline/msbuild-nullable.md` and
`nullable-baseline-errors.txt`). The operative first-party type-safety gate is the analyzer build
(P8-T2), which is green (0 errors, 0 warnings).
