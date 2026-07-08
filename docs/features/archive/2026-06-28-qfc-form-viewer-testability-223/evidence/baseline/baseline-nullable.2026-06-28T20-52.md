# Baseline — Nullable / TreatWarningsAsErrors Build (Issue #223)

Timestamp: 2026-06-28T20-52
Command: msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true
EXIT_CODE: 0

Output Summary: Build succeeded. 0 Warning(s), 0 Error(s). The policy nullable gate (-t:Build, incremental) is clean for first-party projects. Warning headline: 0 warnings / 0 errors. Per repo env notes, a forced -t:Rebuild under these flags surfaces ~84 pre-existing errors confined to vendored/exempt projects (SVGControl, UtilitiesSwordfish) and is NOT the policy gate; the policy gate is -t:Build, which passes.
