# Member coverage CSharpier gate

Timestamp: 2026-07-27T04-04
Command: `csharpier format` followed by `csharpier check` on exactly `BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, `BreadcrumbDropDownOpenCoordinatorTests.cs`, `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`, `BreadcrumbDropDownLifecycleCoverageTests.cs`, and `BreadcrumbCoordinatorLifecycleTests.cs`.
EXIT_CODE: 0
Output Summary: After the authorized initial formatter output, the repeated stable format pass and scoped check both exited 0 with zero file deltas. Physical line counts are 121, 447, 381, 469, and 493 respectively; all are at most 500 and the lifecycle coverage test remains below 480.
