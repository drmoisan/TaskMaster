# QA Gate: Test Policy Audit (#1002)

Timestamp: 2026-10-22T10-16
Command: per new test file: grep -c for `using Microsoft.VisualStudio.TestTools.UnitTesting;`, `using Moq;`, `using FluentAssertions;`, `new Mock<`, `new Mock<...>(MockBehavior.Strict)`, the `// Arrange` / `// Act` / `// Assert` markers, `-E "Path\.GetTemp|File\.Write|File\.Create|Thread\.Sleep|Task\.Delay|new WebView2BreadcrumbHost"`, and the file name in its project file
EXIT_CODE: 0
Output Summary: all eight rows pass. Every mock constructed in the new tests is `MockBehavior.Strict` (six constructions, all strict); no file matches a banned token; every file has its `<Compile Include>` entry. Files that construct no mock carry no `using Moq;`: the three UtilitiesCS.Test files and the nesting contract exercise pure types with no collaborator to mock, and the stale-completion and trash router classes obtain their strict mocks from the shared `EfcDescendantRouterFixture` (declared in `BreadcrumbBridgeRouterIssue1002Tests.cs`); the stale-completion file adds sequence setups on that fixture and therefore carries `using Moq;`.

| File | MSTest | Moq | FluentAssertions | Mocks (strict/total) | AAA | Banned tokens | Compile Include | Result |
|---|---|---|---|---|---|---|---|---|
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs | yes | n/a (no collaborator) | yes | 0/0 | yes | 0 | yes | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs | yes | n/a (no collaborator) | yes | 0/0 | yes | 0 | yes | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs | yes | n/a (no collaborator) | yes | 0/0 | yes | 0 | yes | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs | yes | yes | yes | 2/2 (fixture provider and host) | yes | 0 | yes | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs | yes | yes (fixture setups) | yes | 0/0 own; shared strict fixture | yes | 0 | yes | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs | yes | via shared strict fixture | yes | 0/0 own; shared strict fixture | yes | 0 | yes | PASS |
| QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs | yes | yes | yes | 4/4 | yes | 0 | yes | PASS |
| QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs | yes | n/a (text contract) | yes | 0/0 | yes | 0 | yes | PASS |

Additional checks:

- No test creates a file, sleeps, or delays; ordering in the stale-completion class uses `TaskCompletionSource` barriers.
- No test constructs a real `WebView2BreadcrumbHost`; all host access goes through strict `IBreadcrumbWebHost` mocks.
- Tests share no static mutable state: each test builds a fresh fixture in `[TestInitialize]` or a local factory.
