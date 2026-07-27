# P9-T15 nonnumeric adapter branch-scope ledger

Timestamp: 2026-07-27T08-20
Command: derive the C# ledger from the live merge base plus untracked QuickFiler, QuickFiler.Test, UtilitiesCS, and UtilitiesCS.Test sources; sort ordinal-ignore-case; LF-join; SHA-256.
Command: Select-String QuickFiler.csproj and QuickFiler.Test.csproj for the three P9-T12/P9-T13 Compile entries.
Command: Get-FileHash coverage.config .csharpierignore -Algorithm SHA256.
EXIT_CODE: 0

## Output Summary

Live merge base: e63ddc7c18ca71e2c968b3329e42d965d45af1eb.
Ordered branch-scope ledger count: 68.
LF-joined SHA-256: 2B63B4B315A68A72F23F8D5CDA3A055CEEB314BB9ADB7929B291477E7C7504A9.

The ordered 68-path ledger is byte-for-byte the P9-T10 reauthorization ledger in ac18-nonnumeric-adapter-reconciliation.2026-07-27T07-10.md, now with all three planned paths materialized:

- QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs
- QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs
- QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs

Project includes are exactly one each:

- QuickFiler/QuickFiler.csproj:392 Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs.
- QuickFiler.Test/QuickFiler.Test.csproj:64 Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs.
- QuickFiler.Test/QuickFiler.Test.csproj:65 Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs.

Protected hashes remain unchanged:

- coverage.config: B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943.
- .csharpierignore: 362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25.

No package, runsettings, solution, props, targets, coverage configuration, filter, threshold, or exclusion policy file differs from the merge base.

Result: PASS. This is the explicit post-correction authorization for exactly the P9-T12/P9-T13 C# sources and three project includes.
