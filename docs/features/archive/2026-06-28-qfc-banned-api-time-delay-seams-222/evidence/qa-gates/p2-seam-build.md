# QA Gate — Phase 2 Seam Build (P2-T4)

Timestamp: 2026-06-28T19-40
Commands:
1. csharpier format .  (EXIT 0; Formatted 1184 files) followed by csharpier check . (EXIT 0; clean/idempotent)
2. MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary:
- Build succeeded. 0 Error(s), 38 Warning(s) (all pre-existing CS8632/CS0067/CS0618/MSTEST0032).
- Seam properties present and compiling: `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` on QfcDatamodel (QfcDatamodel.cs) and QfcHomeController (QfcHomeController.Metrics.cs partial). Optional `TimeProvider timeProvider = null` parameter added to QfcHomeController.LaunchAsync with `controller.TimeProvider = timeProvider ?? TimeProvider.System;`.

Seam-file line counts (all <= 500):
- QfcHomeController.cs: 456 (was 454; +2 for optional param + assignment)
- QfcHomeController.Metrics.cs: 233 (was 226; +7 for seam property + doc)
- QfcDatamodel.cs: 438 (was 432; +6 for seam property + doc)

SCOPE NOTE (escalated, mechanically required): Adding the optional `TimeProvider` parameter to the public static `LaunchAsync` caused CS0012 in the consuming project TaskMaster (TaskMaster\Ribbon\RibbonController.cs lines 118, 139 call LaunchAsync with 2 args). The C# compiler requires a caller's project to reference the assembly defining an optional parameter's type even when the argument is omitted. Mechanically-required fix applied: added Microsoft.Bcl.TimeProvider package entry + `<Reference>` (HintPath to the already-restored net462 DLL) to TaskMaster/packages.config and TaskMaster/TaskMaster.csproj. No TaskMaster source code changed; RibbonController call sites remain 2-arg. Also corrected the TimeProvider.Testing `<Reference>` PublicKeyToken to the actual value 31bf3856ad364e35 (resolved MSB3245).
