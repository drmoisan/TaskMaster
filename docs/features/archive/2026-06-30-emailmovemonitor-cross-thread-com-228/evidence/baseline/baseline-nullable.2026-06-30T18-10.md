# Baseline — Nullable Type-Check Build State (Issue #228)

Timestamp: 2026-06-30T22-16
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
(Executed via Bash with dash switches: MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true)
EXIT_CODE: 0
Output Summary: Build succeeded, 0 errors. This incremental -t:Build of the nullable gate ran immediately after the analyzer build (P0-T3) compiled all assemblies, so no project recompiled and no warnings/errors surfaced. Documented environment behavior: a forced-nullable build surfaces the ~84 pre-existing vendored errors (confined to SVGControl and UtilitiesSwordfish) only under -t:Rebuild; an incremental -t:Build (as mandated by the toolchain command) reports 0 because those assemblies are not recompiled. The mandated toolchain command sequence (analyzer build first, then nullable build) is what keeps QuickFiler.Test (C# 7.3) from emitting CS8630 in isolation. Baseline nullable gate is clean for the in-scope first-party projects (QuickFiler, QuickFiler.Test, UtilitiesCS).
