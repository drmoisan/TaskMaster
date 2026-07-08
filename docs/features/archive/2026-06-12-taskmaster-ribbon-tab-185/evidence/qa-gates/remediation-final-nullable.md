# Phase 2 — Final QA: Nullable / Type-Check Build (Issue #185)

Timestamp: 2026-06-12T11-24

Command: msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
(Executed in git-bash with `MSYS_NO_PATHCONV=1` and `-`-style switches. `/t:Rebuild` used because the nullable gate requires a forced rebuild to re-instrument under `Nullable=enable`, per the documented build environment; MSBuild 18.7.1, VS18 Community.)

EXIT_CODE: 1

Output Summary: Build exits 1 with 84 nullable/type errors. ALL 84 errors are confined to the two vendored projects and ZERO are in-scope (first-party):
- SVGControl.csproj: 34 error lines
- UtilitiesSwordfish.NET.General.csproj: 50 error lines
- First-party / in-scope errors: 0 (no error references any file under `TaskMaster/`, `TaskMaster.Test/`, `Ribbon/`, or any other first-party project).

Error-code distribution (all vendored): CS8618 x26, CS8625 x26, CS8603 x9, CS8600 x8, CS8602 x6, CS8601 x5, CS0649 x2, CS8604 x1, CS8619 x1.

Classification: pre-existing-vendored. These vendored nullable errors are the documented R3 (INFO) baseline and are excluded from this repository's standards per `.claude/rules/csharp.md` (SVGControl and UtilitiesSwordfish.NET.General are vendored projects). They are NOT remediated by Issue #185 per the plan's "Do Not Do" constraint (do not touch vendored projects to silence pre-existing nullable errors).

Note on baseline figures: the cycle-entry inputs recorded the baseline as 84 errors (68 SVGControl / 16 UtilitiesSwordfish) from an incremental build; this forced `/t:Rebuild` reports the same total of 84 with an owning-csproj split of 34 / 50. The total (84) and the in-scope conclusion (zero first-party nullable errors) are unchanged; the per-project split differs because Rebuild re-instruments all vendored sources rather than only the incrementally-dirty subset. No in-scope source files were changed by this step.
