# Issue 995 issue-995-red-build

Timestamp: 2026-10-17T23-02
Command: pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath UtilitiesCS.Test/UtilitiesCS.Test.csproj -Target Build
EXIT_CODE: 1
Output Summary: MSBuild version 18.10.1-1.26427.6+3cd27c13e for .NET Framework
Build started 10/17/2026 7:02:52 PM.

     1>Project ".\UtilitiesCS.Test\UtilitiesCS.Test.csproj" on node 1 (Build target(s)).
     1>C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(843,5): error : The BaseOutputPath/OutputPath property is not set for project 'UtilitiesCS.Test.csproj'.  Please check to make sure that you have specified a valid combination of Configuration and Platform for this project.  Configuration='Debug'  Platform='Any CPU'. You may be seeing this message because you are trying to build a project without a solution file, and have specified a non-default Configuration or Platform that doesn't exist for this project. [.\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
     1>Done Building Project ".\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (Build target(s)) -- FAILED.

Build FAILED.

       ".\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (Build target) (1) ->
       (_CheckForInvalidConfigurationAndPlatform target) -> 
         C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(843,5): error : The BaseOutputPath/OutputPath property is not set for project 'UtilitiesCS.Test.csproj'.  Please check to make sure that you have specified a valid combination of Configuration and Platform for this project.  Configuration='Debug'  Platform='Any CPU'. You may be seeing this message because you are trying to build a project without a solution file, and have specified a non-default Configuration or Platform that doesn't exist for this project. [.\UtilitiesCS.Test\UtilitiesCS.Test.csproj]

    0 Warning(s)
    1 Error(s)

Time Elapsed 00:00:00.09
Exception: .\scripts\vscode\Invoke-VSBuild.ps1:264:9
Line |
 264 |          throw "MSBuild failed with exit code $LASTEXITCODE"
     |          ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
     | MSBuild failed with exit code 1
