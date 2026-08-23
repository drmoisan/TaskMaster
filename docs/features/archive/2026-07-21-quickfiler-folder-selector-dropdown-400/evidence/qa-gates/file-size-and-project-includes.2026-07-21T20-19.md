# File Size and Project Includes

Timestamp: 2026-07-21T20-19Z
Command: `Get-Content` numeric line counts for every modified production/test C# file; exact `Select-String -SimpleMatch` counts and line numbers for host, helper, readiness-test, and lifecycle-concurrency-test Compile includes; P0-T9 assertion-line SHA-256 recomputation for all four protected tests; and `git diff --check`
EXIT_CODE: 0
Output Summary: Every modified production and test C# file is at or below 500 lines. The host is within its required 475–485-line range, the focused helper remains approximately 105 lines, the helper include occurs exactly once immediately after the host include, both new test files have one include, all protected assertion hashes match P0-T9, and the diff has no whitespace error.

## Numeric Line Counts

| Modified C# file | Lines | Limit result |
|---|---:|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 484 | PASS: within 475–485 and at most 500 |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | 118 | PASS: focused approximate-105 helper and at most 500 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs` | 305 | PASS |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 379 | PASS |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 478 | PASS |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 369 | PASS |

No modified production or test C# file exceeds 500 lines.

## Legacy Project Inclusion

| Include | Count | Line | Result |
|---|---:|---:|---|
| `QuickFiler.csproj`: `BreadcrumbDropDownHost.cs` | 1 | 394 | PASS |
| `QuickFiler.csproj`: `BreadcrumbWebViewSurfaceFactory.cs` | 1 | 395 | PASS, immediately adjacent after host |
| `QuickFiler.Test.csproj`: `BreadcrumbDropDownReadinessTests.cs` | 1 | 66 | PASS |
| `QuickFiler.Test.csproj`: `BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 1 | 67 | PASS |

There is no duplicate, omission, or non-adjacent helper include.

## Protected Assertion Hashes

| Protected file | Assertions | Current SHA-256 | P0-T9 result |
|---|---:|---|---|
| `BreadcrumbDropDownReadinessTests.cs` | 51 | `58cff79fb67b5a6d95f60e961adedba7492691fdd9ffe16036ea467417bfda6d` | MATCH |
| `BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 81 | `a38135b5a39844c4a4f1a420773d54dac6cff6c87c0dcc979a8edd4ebce3e84a` | MATCH |
| `BreadcrumbDropDownHostTests.cs` | 52 | `8d9b16ed5d5e2ca21217e4e4c6653415f7fb7c13c105119f7a2182cac418f3dc` | MATCH |
| `BreadcrumbDropDownLifecycleTests.cs` | 34 | `fc9370c70b339dd99251e43385d82e7c04c2ac779a17546c3ae64e0a7c4fd5ce` | MATCH |

`git diff --check` exited 0. P4-T4 result: PASS.
