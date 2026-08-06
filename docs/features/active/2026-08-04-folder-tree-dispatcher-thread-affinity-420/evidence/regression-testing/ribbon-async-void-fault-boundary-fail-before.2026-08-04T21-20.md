Timestamp: 2026-08-04T21-20
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:DefineConstants=REMEDIATION_P1_T11 /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 1
Output Summary: The deliberate ribbon callback regression failed to compile because RibbonViewer has neither an internal constructor accepting the controlled initialization and error-boundary delegates nor an awaitable LoadFolderFilterAsync seam. No ribbon host, message loop, timer, or global mutable hook was used.

## Required assertion after P3-T10 and P3-T11

`RibbonFolderFilterCallback_ObservesOriginalInitializationFaultOnce` supplies a controlled faulted initialization task to the instance-level ribbon boundary and awaits the boundary result. It requires the original exception instance to be observed exactly once by the selected log/user-error policy.

## Compiler result

- Target assembly: `TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll`
- Diagnostics: `CS1729: RibbonViewer does not contain a constructor that takes 2 arguments`; `CS1061: RibbonViewer does not contain LoadFolderFilterAsync`.
- Result: expected red compile failure before P3-T10 provides an instance-scoped, awaitable seam behind the public `async void` callback.
