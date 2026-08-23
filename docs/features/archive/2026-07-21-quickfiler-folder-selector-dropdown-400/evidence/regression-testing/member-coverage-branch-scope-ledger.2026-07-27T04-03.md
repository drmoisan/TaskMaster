# Member coverage branch-scope ledger

Timestamp: 2026-07-27T04-03
Command: Derived the live merge-base C# set with `git diff --name-only origin/main...HEAD -- '*.cs'` plus untracked authorized C# paths, then ordered paths using `StringComparer.OrdinalIgnoreCase` and hashed the LF-joined list.
EXIT_CODE: 0
Output Summary: The C# ledger contains 65 paths with LF-joined SHA-256 `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7`. It includes all five P8-T56/P8-T58/P8-T59/P8-T60 test files. The adjacent authorized non-C# project-scope entry `QuickFiler.Test/QuickFiler.Test.csproj` has exactly one new `BreadcrumbCoordinatorUpgradeLifetimeTests.cs` Compile include and is deliberately excluded from the C# hash. P8-T55 through P8-T60 changed no production source, coverage config/scope, or Cobertura postprocessor.
