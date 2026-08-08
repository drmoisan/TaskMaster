# Remediation Plan — ribbon-controller-engines-null-unsafe (#507), cycle 1

DIRECTIVE: PREFLIGHT VALIDATION ONLY

Scope: remediate finding B1 only (`TaskMaster.Test/Ribbon/RibbonControllerTests.cs` exceeds the
500-line file-size limit). Finding B2 (11 unguarded `RibbonViewer.cs` call sites) is explicitly
out of scope for this cycle and is being promoted to a separate tracked issue by the orchestrator.
No task in this plan touches `TaskMaster/Ribbon/RibbonViewer.cs`.

- Workspace: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7e887d12b262219`
- Branch: `bug/ribbon-controller-engines-null-unsafe-507`
- HEAD at plan authoring: `e589fad7`
- Merge base: `003c5715055d7d1933db68a742531332756e30b2`
- Feature folder: `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507`
- Evidence root (canonical, non-overridable): `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/<kind>/`

## Verified toolchain paths

- csharpier: `C:/Users/DanMoisan/.dotnet/tools/csharpier`
- msbuild: `C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe`
- vstest.console.exe: `C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe`

### Phase 0 — Baseline Capture and Remediation Implementation

- [x] [P0-T1] Read `CLAUDE.md` in full and record the read in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-instructions-read.2026-08-08T17-45.md`
  with fields `Timestamp:`, `Policy Order:` (list: CLAUDE.md, .claude/rules/general-code-change.md,
  .claude/rules/general-unit-test.md, .claude/rules/csharp.md if present), and an explicit list of
  files read. Acceptance: the artifact file exists and contains all four required fields.
- [x] [P0-T2] Read `.claude/rules/general-code-change.md` in full and append its path to the file
  read list in the same
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-instructions-read.2026-08-08T17-45.md`
  artifact created in P0-T1. Acceptance: the artifact's file-read list contains this path.
- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` in full and append its path to the file
  read list in the same artifact from P0-T1. Acceptance: the artifact's file-read list contains
  this path.
- [x] [P0-T4] Read `.claude/rules/csharp.md` (if it exists at
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/../../../.claude/rules/csharp.md`
  resolved as `.claude/rules/csharp.md` from repo root) and append its path (or record its absence)
  to the file read list in the same artifact from P0-T1. Acceptance: the artifact's file-read list
  records either the path read or an explicit "file not present" note.
- [x] [P0-T5] Run `wc -l TaskMaster.Test/Ribbon/RibbonControllerTests.cs` from the workspace root and
  record the result (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with the exact line
  count) in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/pre-remediation-line-count.2026-08-08T17-45.md`.
  Acceptance: the artifact records a line count of 513.
- [x] [P0-T6] In `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`, delete the two `[TestMethod]`
  blocks `Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing` (with its preceding XML doc
  comment) and `Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines` (with its preceding XML doc
  comment), plus the single blank line separating them from the adjacent methods, so the file's
  remaining content matches the merge-base (`003c5715`) revision for that region. Acceptance:
  `git diff 003c5715055d7d1933db68a742531332756e30b2 -- TaskMaster.Test/Ribbon/RibbonControllerTests.cs`
  shows no textual difference for lines outside the `partial` class-declaration change made in
  P0-T7.
- [x] [P0-T7] In `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`, change the class declaration
  from `public class RibbonControllerTests` to `public partial class RibbonControllerTests`,
  keeping `[TestClass]` and `[DoNotParallelize]` attributes unchanged and attached only to this
  primary part. Acceptance: `grep -n "public partial class RibbonControllerTests"
  TaskMaster.Test/Ribbon/RibbonControllerTests.cs` returns exactly one match, and
  `[TestClass]`/`[DoNotParallelize]` remain present immediately above the class declaration.
- [x] [P0-T8] Create `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` declaring
  `namespace TaskMaster.Test.Ribbon { public partial class RibbonControllerTests { ... } }`
  containing the two test methods removed in P0-T6, verbatim including their XML doc comments, and
  containing only the `using` directives the moved methods require: `System`,
  `System.Reflection`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`,
  `UtilitiesCS`, `TaskMaster`. Do not apply `[TestClass]` or `[DoNotParallelize]` to this partial
  declaration. Acceptance: the file exists, contains exactly the two `[TestMethod]`s named
  `Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing` and
  `Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines`, and does not contain `[TestClass]` or
  `[DoNotParallelize]`.
- [x] [P0-T9] Add `<Compile Include="Ribbon\RibbonControllerTests.Engines.cs" />` to the
  `<ItemGroup>` in `TaskMaster.Test/TaskMaster.Test.csproj`, immediately adjacent to the existing
  `<Compile Include="Ribbon\RibbonControllerTests.cs" />` entry. Acceptance: `grep -n
  "RibbonControllerTests.Engines.cs" TaskMaster.Test/TaskMaster.Test.csproj` returns exactly one
  match inside an `<ItemGroup>` element.
- [x] [P0-T10] Run `wc -l TaskMaster.Test/Ribbon/RibbonControllerTests.cs
  TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` and record the result (`Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:` with both exact line counts) in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/remediation-baseline/post-split-line-counts.2026-08-08T17-45.md`.
  Acceptance: both reported line counts are `<= 500`.

### Phase 1 — Full QA Loop and Scope Verification

- [x] [P1-T1] Run `C:/Users/DanMoisan/.dotnet/tools/csharpier .` from the workspace root and record
  the result (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` — files reformatted, if
  any, and final exit status) in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/csharpier-format.2026-08-08T17-45.md`.
  Acceptance: `EXIT_CODE: 0` on the final invocation; if any file was reformatted, this task must be
  re-run (and P1-T2/P1-T3/P1-T4 restarted) until a pass reformats zero files.
- [x] [P1-T2] Run `"C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe"
  TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
  /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` from the workspace root and record the
  result (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` — warning/error count) in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/analyzer-build.2026-08-08T17-45.md`.
  Acceptance: `EXIT_CODE: 0`. If this stage fails or changes any file, restart from P1-T1.
- [x] [P1-T3] Run `"C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe"
  TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
  /p:TreatWarningsAsErrors=true` from the workspace root and record the result (`Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:` — warning/error count) in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/rebuild-warnings-as-errors.2026-08-08T17-45.md`.
  Do not add `/p:Nullable=enable` to this command. Acceptance: `EXIT_CODE: 0`. If this stage fails
  or changes any file, restart from P1-T1.
- [x] [P1-T4] Discover test assemblies by searching the workspace root
  (`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7e887d12b262219`) for
  `**\bin\**\*.Test.dll`, filtering out any path whose portion relative to the workspace root
  contains a nested `.claude` segment, `\obj\`, or `\ref\`, and record the resulting assembly list
  (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` listing all discovered assembly paths
  and the total count) in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/test-assembly-discovery.2026-08-08T17-45.md`.
  Acceptance: exactly 9 assemblies are listed, one per `*.Test` project.
- [ ] [P1-T5] Run `"C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe"
  <the 9 assembly paths discovered in P1-T4> /EnableCodeCoverage /InIsolation
  /TestCaseFilter:"TestCategory!=LiveOutlook"` from the workspace root and record the result
  (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with total/passed/failed counts and
  the numeric coverage headline) in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/vstest-run.2026-08-08T17-45.md`.
  Acceptance: `EXIT_CODE: 0` and the summary reports 6295 total, 6295 passed, 0 failed. If any test
  fails or any file changes as a result of this stage, restart the loop from P1-T1.
- [ ] [P1-T6] Run `wc -l TaskMaster.Test/Ribbon/RibbonControllerTests.cs
  TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` after the toolchain loop completes and
  record the result (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with both exact line
  counts) in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/final-line-counts.2026-08-08T17-45.md`.
  Acceptance: both reported line counts are `<= 500`.
- [ ] [P1-T7] Run `git diff --name-only 003c5715055d7d1933db68a742531332756e30b2...HEAD` from the
  workspace root and record the result (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`
  with the full file list) in
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/scope-diff-check.2026-08-08T17-45.md`.
  Acceptance: the listed file set does not contain `TaskMaster/Ribbon/RibbonViewer.cs`.
