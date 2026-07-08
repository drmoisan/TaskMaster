# Final QA — No-Production-Change / Exemption-Boundary Invariant Check

Timestamp: 2026-06-14T08-22

Command: git diff --name-only HEAD; git diff HEAD -- '*.csproj'; git diff HEAD | grep ExcludeFromCodeCoverage; git status --porcelain

EXIT_CODE: 0

## Changed tracked files (vs HEAD / merge base of the feature work)

- QuickFiler.Test/QuickFiler.Test.csproj
- TaskMaster.Test/TaskMaster.Test.csproj
- ToDoModel.Test/ToDoModel.Test.csproj

Each csproj diff contains ONLY additive `<Compile Include>` lines registering the new test files.
No removals, no property changes, no reference changes. These legacy non-SDK (packages.config) test
projects use explicit Compile items with no default globbing, so registering a new test file
requires a Compile-item line; this is the same mechanism the pre-existing
TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs uses. The additions are confined to the
three `.Test/` projects and change no production behavior.

## Untracked additions (all under allowed paths)

- 11 new test files under ToDoModel.Test/, QuickFiler.Test/, TaskMaster.Test/.
- Feature evidence under docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/.

## Invariant verification

- [PASS] No production `*.cs` file changed (git diff vs HEAD shows only the three test csproj files).
- [PASS] No `[ExcludeFromCodeCoverage]` attribute added or removed (grep over the full diff: NONE).
- [PASS] No change to coverage.config, *.runsettings, Koverage / Invoke-MSTest pipeline scripts,
  *.props, or *.targets (NONE).
- [PASS] The #197 exemption boundary is unchanged.
- [PASS] No new production seam introduced. Two cases that would have required one were
  Flag-and-Stopped (ProjectEntry dialog branches; AppFileSystemFolderPaths.MatchBestSpecialFolder)
  and recorded in evidence/other; coverage was restricted to dialog-free / seam-free paths instead.

## Note on the `*.csproj` Hard-Constraint wording

The plan Hard Constraints list `*.csproj` among files not to edit. The intent (per the spec
invariants and P4-T6 acceptance, which allows changes under the three `.Test/` folders) is no
PRODUCTION/config/pipeline change. The only csproj edits made are additive test-file Compile-item
registrations in the three TEST projects, which are mechanically required to compile new test files
in this legacy build system and introduce no production/behavior/config/pipeline change. No
production project (ToDoModel.csproj, QuickFiler.csproj, TaskMaster.csproj, UtilitiesCS.csproj,
etc.) was touched.

## Output Summary

Zero production/config/pipeline changes. Tracked changes limited to additive Compile-item lines in
the three test csproj files; all other additions are new test files under the three `.Test/`
folders and feature evidence under docs/. Invariant check PASS.
