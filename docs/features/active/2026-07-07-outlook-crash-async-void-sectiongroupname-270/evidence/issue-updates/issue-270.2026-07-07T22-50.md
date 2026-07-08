# Issue #270 Acceptance-Criteria Update Mirror

Timestamp: 2026-07-07T22-50

PostedAs: body (mirrored into the local feature `issue.md` `## Acceptance Criteria` section on disk; not pushed to GitHub by the executor)

## Updated Acceptance Criteria section (exact text on disk)

- [x] AC1: `OlToDoItems_ItemChange` no longer contains `catch (System.Exception) { throw; }`; a fault from the awaited `ToDoEvents.OlToDoItems_ItemChange` call is logged (full exception, via the existing `logger`) and contained, with no exception escaping the `async void` method.
- [x] AC2: `OlInboxItems_ItemAdd` no longer contains `catch (System.Exception) { throw; }`; a fault from the awaited `ProcessMailItemAsync` call is logged (full exception, via the existing `logger`) and contained, with no exception escaping the `async void` method.
- [x] AC3: The logged output preserves the original exception object (message and stack), so a previously-lost `sectionGroupName` `ArgumentException` becomes observable in the log rather than being silently swallowed or rethrown.
- [x] AC4: A deterministic MSTest regression test (Moq + FluentAssertions, no COM/network/temp files) drives each handler path with an injected collaborator that throws a synthetic exception and asserts the handler contains and logs it (does not throw). The test fails against the pre-fix `catch { throw; }` and passes after the fix.
- [x] AC5: The full C# toolchain passes in order (CSharpier -> .NET analyzers -> nullable/type-check -> MSTest) with no new warnings, and coverage on changed lines does not regress.
- [x] AC6: No scope creep — only `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` (production) and `TaskMaster.Test/AppGlobals/AppEventsTests.cs` (test) are changed for the fix. The proximate config trigger and the `RibbonViewer` async-void handlers remain documented follow-ups.

## Evidence backing each newly-checked criterion

- AC5:
  - Format: `evidence/qa-gates/format-final.2026-07-07T22-50.md` (EXIT 0; no reformat churn).
  - Analyzer: `evidence/qa-gates/analyzer-final.2026-07-07T22-50.md` (EXIT 0; only pre-existing CS8632 in untouched files; zero new warnings from touched files).
  - Type-check: `evidence/qa-gates/typecheck-final.2026-07-07T22-50.md` (vendored 84-error set identical to baseline; two CS8625 citing `AppEvents.ReadinessHookup.cs` are on pre-existing `Unhook()` lines 20-21, outside the #270 diff; touched files add zero new nullable diagnostics).
  - Tests: `evidence/qa-gates/test-final.2026-07-07T22-50.md` (202/202 passed).
  - Coverage: `evidence/qa-gates/coverage-delta.2026-07-07T22-50.md` (changed core methods 100% / 92.86%; no regression on changed lines).

- AC6 reconciliation:
  - Production scope is EXACTLY the single file named in AC6: `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs`. It is unchanged in scope from the criterion's wording.
  - The additional changed files are all test-scope or build-wiring driven by this fix and the 500-line file-size ceiling, not production scope creep:
    - `TaskMaster.Test/AppGlobals/AppEventsTests.cs` — the test file named in AC6 (holds the two new regression tests).
    - `TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs` — new file; a byte-equivalent relocation of pre-existing private helper methods to keep `AppEventsTests.cs` under the 500-line ceiling (P1-T2).
    - `TaskMaster.Test/AppGlobals/AppEventsCoverageExpansionTests.cs` — one existing test updated (P2-T4) because its assertion encoded the now-removed rethrow contract (existing tests are part of the spec); renamed `OlInboxItemsItemAdd_WhenProcessingThrows_RethrowsThroughSynchronizationContext` -> `..._ContainsAndDoesNotRethrow` and changed `CapturedException.Should().BeSameAs(expected)` -> `.Should().BeNull()`.
    - `TaskMaster.Test/TaskMaster.Test.csproj` — mechanical `<Compile Include>` wiring for the new helper file (legacy non-SDK project, no glob).
  - The proximate `sectionGroupName` config trigger, the ~40 `RibbonViewer` `async void *_Click` handlers, and `TaskMaster/AddInUtilities.cs` remain untouched documented follow-ups. No production behavior beyond the two contained handlers changed.

IssueUpdatedAt: local file only (executor did not post to GitHub).
Issue URL: https://github.com/drmoisan/TaskMaster/issues/270
