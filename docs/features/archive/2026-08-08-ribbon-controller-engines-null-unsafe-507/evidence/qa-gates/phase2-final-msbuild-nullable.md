# Phase 2 — Final msbuild (nullable)

Timestamp: 2026-08-08T16-05

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
Invocation used (literal task command, exactly as specified in the plan):
`MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" TaskMaster.sln -t:Build -p:Configuration=Debug "-p:Platform=Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -nologo -v:minimal`

EXIT_CODE: 0

Output Summary: The literal command as specified in the plan (`/t:Build`, solution-wide) returned
`EXIT_CODE: 0`, 0 errors. **This pass is vacuous, not a genuine nullable check**, and this is
disclosed here rather than reported at face value:

- `UtilitiesCS.dll`'s on-disk timestamp predates this build invocation, showing MSBuild's `/t:Build`
  up-to-date check treated `UtilitiesCS.csproj` (and transitively its dependents) as already
  up-to-date and skipped `CoreCompile`, ignoring the `/p:Nullable=enable` property change. This is
  the same incremental-build-vacuous-baseline mechanism recorded from prior sessions in this
  repository: an unconditional `/t:Build` does not re-evaluate `CoreCompile` inputs purely because a
  command-line `/p:` property differs from the last cached build in the same output folder.
- To obtain a genuine reading, three isolated, single-project, forced (`/t:Rebuild
  /p:BuildProjectReferences=false`) nullable checks were run as verification micro-actions (not
  plan tasks; no files were modified by these checks beyond a temporary `git stash`/`stash pop` of
  the two in-scope files, restored immediately after):
  1. `UtilitiesCS.csproj` (unrelated to this feature): 195 errors, identical in count and content
     to the Phase 0 baseline (`evidence/baseline/phase0-baseline-msbuild-nullable.md`). No new
     UtilitiesCS diagnostics.
  2. `TaskMaster.csproj` **pre-fix** (via `git stash` of the two in-scope files): 219 errors, 5 of
     them in `RibbonController.Intelligence.cs` (lines 160, 183×2, 198, 271, 288) — none at line
     204.
  3. `TaskMaster.csproj` **post-fix** (working tree, `git stash pop` restored): 220 errors, 6 of
     them in `RibbonController.Intelligence.cs` — the same 5 plus one new error at line **204,
     column 45**: `CS8603: Possible null reference return.` This is directly attributable to this
     feature's change: `Globals?.Engines` (null-conditional) is a possibly-null expression, and
     under a genuinely nullable-enabled compile it cannot be implicitly returned from the
     non-nullable `internal IAppItemEngines Engines` property without a diagnostic.
  4. `TaskMaster.Test.csproj` post-fix: 76 errors, 5 of them in `RibbonControllerTests.cs` (lines
     218, 231, 236, 454, 456) — all 5 pre-existing (confirmed against file content: lines 454/456
     are the pre-existing `CreateComparisonSnapshot`/`CreateNode` helper, shifted down by this
     feature's insertion, not new code). Zero errors inside either of the two new test methods
     added by this feature (`Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing`,
     `Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines`).

**Finding**: this feature's one-line production fix (`Globals.Engines` → `Globals?.Engines`)
introduces exactly one new nullable diagnostic (CS8603) when the project is genuinely compiled
with `Nullable=enable`, but `TaskMaster.csproj` does not have `Nullable` enabled by its own project
settings and already carries 219 pre-existing nullable errors under a forced check, so this
diagnostic is currently masked by (a) the project's own non-nullable-enabled default and (b) the
solution-wide toolchain command's incremental-build vacuity. The plan's Hard Scope Boundary
restricts this feature to exactly one changed line in
`TaskMaster/Ribbon/RibbonController.Intelligence.cs` ("No other line in the file changes"), which
precludes adding a null-forgiving operator, an explicit cast, or a suppression pragma to resolve
this diagnostic within the current plan's authorized scope. This finding — a solution-wide
`/p:Nullable=enable` gate that is genuinely broken (219 pre-existing TaskMaster.csproj errors +
195 pre-existing UtilitiesCS.csproj errors, both entirely predating this feature) and currently
passes only via an incremental-build caching artifact — is escalated in the executor's completion
report rather than remediated in-scope; recommended follow-up is a separate, explicitly-scoped
issue to either (a) add a project-level or per-file nullable annotation adjustment to
`RibbonController.Intelligence.cs`'s `Engines`/sibling properties, or (b) track the pre-existing
`TaskMaster.csproj`/`UtilitiesCS.csproj` nullable debt for remediation, consistent with this
repository's ongoing incremental nullable-migration effort.

No file was modified as a result of these isolated verification checks; the working tree after
this task contains only the same two in-scope files reported throughout this evidence trail. The
literal Phase 2 command (`/t:Build`, solution-wide) is recorded above per its actual EXIT_CODE (0)
as the plan requires; the genuine-state findings are recorded as an explicit disclosure alongside
it so the artifact is not misleadingly read as a clean, isolated nullable pass.
