# P2-T2 — Red-Run Build (new host-seam tests against unmodified production code)

Timestamp: 2026-08-28T15-30

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
(run with `/v:m`)

EXIT_CODE: 0

Output Summary:

- Build succeeded with 0 error lines.
- 5 warning lines, all the pre-existing `System.Reactive.PackagesConfigCheck.targets`
  `packages.config` advisory recorded in the P0-T8 and P0-T9 baseline artifacts. Warning count is
  unchanged from baseline, so the six new tests introduce no diagnostic.
- The six #680 host-seam tests appended to
  `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` compile against the unmodified
  `BreadcrumbDropDownHost` production code. No production file has been edited at this point.

Acceptance: satisfied — `EXIT_CODE: 0`.
