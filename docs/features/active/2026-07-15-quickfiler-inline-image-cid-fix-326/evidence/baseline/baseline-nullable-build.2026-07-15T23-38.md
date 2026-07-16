# Baseline Nullable Build — P0-T9

- **Timestamp:** 2026-07-15T23-38
- **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  (invoked in git-bash as `MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -nologo -v:minimal`
  to avoid git-bash POSIX path-mangling of `/`-prefixed MSBuild switches; semantically identical command.)
- **EXIT_CODE:** 0
- **Output Summary:** Build succeeded, 0 warnings, 0 errors. All 18 projects
  (`SVGControl`, `UtilitiesCS`, `Tags`, `ToDoModel`, `ToDoModel.Test`, `TaskVisualization`, `QuickFiler`,
  `TaskTree`, `TaskMaster`, `UtilitiesCS.Test`, `QuickFiler.Test`, `TaskVisualization.Test`,
  `Tags.Test`, `TaskTree.Test`, `VBFunctions`, `VBFunctions.Test`, `TaskMaster.Test`) built cleanly
  under `TreatWarningsAsErrors=true`. Nullable-gate baseline is clean; only the analyzer build
  (P0-T8) surfaces pre-existing non-nullable warnings (CS0649/CS0618/CS8632/CS0067/MSTEST0032), which
  are not promoted to errors under this build's default project-level nullable settings.
