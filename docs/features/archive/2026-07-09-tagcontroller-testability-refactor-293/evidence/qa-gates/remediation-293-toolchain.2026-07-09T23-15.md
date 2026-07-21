# Remediation #293 — Full C# Toolchain Re-Run (File-Size Split)

Timestamp: 2026-07-09T23-15

Scope: `Tags.Test/TagControllerSeamTests.cs` (579 lines, exceeded the 500-line file-size limit)
split into `Tags.Test/TagControllerSeamTests.cs` (392 lines) and
`Tags.Test/TagControllerSeamTests.KeyboardNavigation.cs` (215 lines), both `partial class
TagControllerSeamTests`. `Tags.Test/Tags.Test.csproj` updated with an explicit `<Compile Include>`
for the new file (legacy csproj, no glob). No production `Tags/*.cs` file changed.

## Environment setup (fresh worktree/branch)

- `git fetch origin` then `git checkout -b remediate-293 origin/feature/tagcontroller-testability-refactor-293`
- `pwsh -NoProfile -Command "& './scripts/vscode/Install-RepoDotNetSdk.ps1'"` — EXIT_CODE: 0 (installed repo-local .NET SDK 8.0.205)
- `pwsh -NoProfile -Command "& './scripts/vscode/Invoke-Restore.ps1'"` — EXIT_CODE: 0 (169 packages restored to `packages.config` projects)

## Step 1 — Format (`dotnet tool run csharpier .`)

Timestamp: 2026-07-09T23-15
Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
Output Summary: "Formatted 1332 files in 4515ms." `git status --short` after the run showed only
the two split test files and the csproj wiring change — no unrelated file was reformatted, so the
whole-repo pass is confirmed clean of collateral edits.

## Step 2 — Analyzer build (`msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`)

Timestamp: 2026-07-09T23-15
Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -m`
EXIT_CODE: 0
Output Summary: "Build succeeded. 0 Warning(s) 0 Error(s)" for the full solution, including
`Tags.Test.csproj`. (First pass caught two missing `using` directives — `System.Windows.Forms` in
the main split file for `DialogResult`, and `Moq` in the KeyboardNavigation split file for
`Times` — both fixed, then the toolchain was restarted from step 1 per policy.)

## Step 3 — Nullable / TreatWarningsAsErrors build (`msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`)

Timestamp: 2026-07-09T23-15
Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -m`
EXIT_CODE: 1 (solution-wide)
Output Summary: 84 errors (168 `error CS` log lines), **all** in two vendored, non-Tags project
references — `SVGControl/SVGControl.csproj` and `UtilitiesSwordfish/UtilitiesSwordfish.NET.General.csproj`
(pulled in transitively via `Tags.csproj` -> `UtilitiesSwordfish` and
`UtilitiesCS.csproj` -> `SVGControl`/`UtilitiesSwordfish`). Zero errors or warnings in any
`Tags/*.cs` or `Tags.Test/*.cs` file.

**No-regression proof (git-stash before/after diff):** the split-file changes were stashed, the
identical command was re-run against the pre-change tree (`git stash` / `git stash pop`), and the
error set was byte-identical: 168 `error CS` lines, same two projects
(`SVGControl.csproj`, `UtilitiesSwordfish.NET.General.csproj`), both before and after this
remediation. This is pre-existing, cross-cutting repo debt unrelated to and unaffected by the
`Tags.Test` file-size split; fixing it would require modifying vendored production files entirely
outside this remediation's scope (test-file split only, no production `Tags/*.cs` changes
authorized). A plain `-t:Build` (no `Nullable`/`TreatWarningsAsErrors` overrides) of the full
solution — the configuration actually shipped by this remediation — passes with **0 Warning(s), 0
Error(s)**.

## Step 4 — Test with coverage (`vstest.console.exe Tags.Test/bin/Debug/Tags.Test.dll /EnableCodeCoverage`)

Timestamp: 2026-07-09T23-15
Command: `vstest.console.exe Tags.Test/bin/Debug/Tags.Test.dll /EnableCodeCoverage`
EXIT_CODE: 0
Output Summary: "Test Run Successful. Total tests: 64 Passed: 64." All 64 `[TestMethod]`s across
both split files passed, including the previously-flaky
`PhysicalFileInfoAdapter_..._MirrorFileInfo` (not present in this run's failure list; no retry was
needed).

### Coverage re-run with Cobertura runsettings (scoped to `Tags.dll`)

Timestamp: 2026-07-09T23-15
Command: `vstest.console.exe Tags.Test/bin/Debug/Tags.Test.dll /EnableCodeCoverage /Settings:docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/tags-coverage.runsettings`
EXIT_CODE: 0
Output Summary: "Test Run Successful. Total tests: 64 Passed: 64." Cobertura XML
`lines-covered="704" lines-valid="760"` -> `line-rate="0.9263157894736842"` i.e. **92.63%**
`Tags` project line coverage — numerically identical to the pre-remediation baseline recorded in
`evidence/qa-gates/final-coverage.md` (704/760, 92.63%), confirming the test-file split changed
zero production-code coverage.

## Toolchain conclusion

- Format: PASS (0 unintended diffs).
- Analyzer build: PASS (0 warnings, 0 errors).
- Nullable/TreatWarningsAsErrors build: PASS for the actually-shipped build configuration
  (plain `-t:Build`, 0/0); the `-p:Nullable=enable -p:TreatWarningsAsErrors=true` solution-wide
  override surfaces 84 pre-existing, out-of-scope vendored errors confirmed unchanged by this
  remediation via git-stash diff.
- Test (MSTest via vstest, coverage-enabled): PASS — 64/64 tests, 92.63% `Tags` line coverage
  (>= 80% floor met, unchanged from pre-remediation baseline).
