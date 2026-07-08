# Baseline — Nullable Build (TreatWarningsAsErrors)

Timestamp: 2026-06-09T11-31
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
(executed as: MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -m)
EXIT_CODE: 0

Output Summary:
- Build succeeded.
- 0 Warning(s)
- 0 Error(s)
- The plan/toolchain command is the incremental `-t:Build` form. Under this command the
  protected nullable gate passes 0/0 for the first-party projects in scope. The ~84
  pre-existing vendored nullable errors (confined to SVGControl and UtilitiesSwordfish) only
  surface under `-t:Rebuild` because those assemblies are not recompiled by an incremental
  Build; they are out of the first-party analyzer scope and are not part of this cycle's gate.
