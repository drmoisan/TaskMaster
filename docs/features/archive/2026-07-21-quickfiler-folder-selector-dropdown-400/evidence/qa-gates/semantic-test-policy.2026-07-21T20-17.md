# Semantic Test Policy Audit

Timestamp: 2026-07-21T20-17Z
Command: `Get-Content` and `Select-String` audit of test line counts, MSTest methods, Arrange/Act/Assert markers, FluentAssertions calls, and Moq/fake usage in all modified or protected host tests; exact `QuickFiler.Test.csproj` Compile-include counts for the two new files; prohibited-pattern scan over both new files and added lines in the tracked integration/coordinator diffs; `git status --short -- QuickFiler.Test`; and `git diff --check -- QuickFiler.Test`
EXIT_CODE: 0
Output Summary: All modified tests are independent, deterministic, structured with Arrange/Act/Assert, use MSTest with FluentAssertions and focused fakes, contain no prohibited external or blocking behavior, remain at or below 500 lines, and have correct legacy project inclusion. No split is required.

## Files and Structure

| Test file | Lines | Test methods | Arrange markers | Act markers | Assert markers | Fluent assertion lines | Moq/fake references |
|---|---:|---:|---:|---:|---:|---:|---:|
| `BreadcrumbDropDownReadinessTests.cs` | 305 | 2 | 2 | 3 | 3 | 51 | 1 |
| `BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 379 | 6 | 6 | 7 | 7 | 81 | Focused fakes |
| `BreadcrumbDropDownIntegrationTests.cs` | 478 | 9 | 9 | 12 | 11 | 45 | 15 |
| `BreadcrumbSelectorCoordinatorTests.cs` | 369 | 11 | 11 | 15 | 11 | 55 | 10 |
| `BreadcrumbDropDownHostTests.cs` | 499 | 13 | 13 | 14 | 12 | 52 | 1 |
| `BreadcrumbDropDownLifecycleTests.cs` | 277 | 5 | 5 | 6 | 5 | 34 | Focused fakes |

Every file remains at or below 500 lines. The two Phase 3 test homes are 478 and 369 lines after formatting, so no boundary split is required.

## Independence and Determinism

- Each added test creates its own harness, coordinator, messenger, `TaskCompletionSource`, control, or Moq instance.
- Disposable harnesses are scoped with `using`; synchronization-context state is restored by the harness cleanup.
- Pending asynchronous paths are driven with deterministic `TaskCompletionSource` completion using `RunContinuationsAsynchronously` where applicable.
- No added test depends on execution order or mutable shared state.
- All assertions use FluentAssertions; all tests use MSTest attributes.
- External WebView, Outlook, native display, and message-loop boundaries are represented by injected delegates, Moq objects, or focused in-memory fakes.

The scan found zero occurrences in new files or tracked added test lines for:

- `Thread.Sleep`
- `Task.Delay`
- `Task.Yield`
- blocking `.Wait()` or `.Result`
- temporary-path or file creation/writes
- `HttpClient` or `WebRequest`
- `Process.Start`
- direct `new WebView2`

No temporary file, network, Outlook, externally launched process, live WebView, display, or user-input dependency is present.

## Legacy Project Includes

- `BreadcrumbDropDownReadinessTests.cs`: exactly one Compile include, line 66.
- `BreadcrumbDropDownLifecycleConcurrencyTests.cs`: exactly one Compile include, line 67.

Both new files are in the existing focused `Viewers` test boundary. No other new test file requires a project include. `git diff --check -- QuickFiler.Test` exited 0.

P3-T5 result: PASS.
